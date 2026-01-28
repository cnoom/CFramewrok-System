using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using CFramework.Systems.AssetsSystem.AssetsReceiver;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace CFramework.Systems.AssetsSystem
{
    [AutoModule("c资源系统", "管理游戏资源的加载、卸载和缓存")]
    public class AssetsSystemModule : IModule, IRegisterAsync, IUnRegister, IAssetsSystem
    {

        // 引用计数：标签批量级别（缓存句柄与结果）
        private readonly ConcurrentDictionary<(Type, string), GroupAssetEntry> _groupCache = new ConcurrentDictionary<(Type, string), GroupAssetEntry>();


        private readonly ConcurrentDictionary<string, CancellationTokenSource> _selectedProgressSessions = new ConcurrentDictionary<string, CancellationTokenSource>();

        // 引用计数：单资源 key 级别
        private readonly ConcurrentDictionary<(Type, string), SingleAssetEntry> _singleCache = new ConcurrentDictionary<(Type, string), SingleAssetEntry>();
        private AssetsReceiverManager _assetsReceiverManager;
        private CFLogger _logger;
        public bool IsRegistered { get; set; }

        #region 查询

        [QueryHandler]
        private UniTask<bool> OnQueryBool(AssetsQueries.HasAssetsReceiver query, CancellationToken ct)
        {
            return UniTask.FromResult(_assetsReceiverManager.HasReceiver(query.Type));
        }

        #endregion

        private readonly struct SingleAssetEntry
        {
            public readonly AsyncOperationHandle Handle;
            public readonly int RefCount;

            public SingleAssetEntry(AsyncOperationHandle handle, int refCount)
            {
                Handle = handle;
                RefCount = refCount;
            }

            public SingleAssetEntry Inc()
            {
                return new SingleAssetEntry(Handle, RefCount + 1);
            }
            public SingleAssetEntry Dec()
            {
                return new SingleAssetEntry(Handle, RefCount - 1);
            }
        }

        private readonly struct GroupAssetEntry
        {
            public readonly AsyncOperationHandle Handle;
            public readonly int RefCount;

            public GroupAssetEntry(AsyncOperationHandle handle, int refCount)
            {
                Handle = handle;
                RefCount = refCount;
            }

            public GroupAssetEntry Inc()
            {
                return new GroupAssetEntry(Handle, RefCount + 1);
            }
            public GroupAssetEntry Dec()
            {
                return new GroupAssetEntry(Handle, RefCount - 1);
            }
        }

        #region 命令

        [CommandHandler]
        public UniTask RegisterAssetsReceiver(AssetsCommands.RegisterAssetReceiver command, CancellationToken ct)
        {
            _assetsReceiverManager.RegisterReceiver(command.Type);
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        public UniTask StartTrackSelectedProgress(AssetsCommands.StartTrackSelectedProgress command,
            CancellationToken ct)
        {
            if(command.Ids == null || command.Ids.Length == 0 || string.IsNullOrEmpty(command.TrackId))
                return UniTask.CompletedTask;

            // 若已有同会话，先取消旧的
            if(_selectedProgressSessions.TryGetValue(command.TrackId, out CancellationTokenSource oldCts))
            {
                try
                {
                    oldCts.Cancel();
                    oldCts.Dispose();
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }

                _selectedProgressSessions.Remove(command.TrackId, out _);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            _selectedProgressSessions[command.TrackId] = cts;
            TrackSelectedProgressLoop(command.TrackId, command.Ids, cts.Token).Forget();
            return UniTask.CompletedTask;
        }

        [CommandHandler]
        public UniTask StopTrackSelectedProgress(AssetsCommands.StopTrackSelectedProgress command, CancellationToken ct)
        {
            StopTrackSelectedProgress(command.TrackId);
            return UniTask.CompletedTask;
        }

        #endregion

        #region API

        internal async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken)
        {
            // 若已在缓存，引用+1 并复用句柄结果
            if(_singleCache.TryGetValue((typeof(T), key), out SingleAssetEntry entryExisting))
            {
                _singleCache[(typeof(T), key)] = entryExisting.Inc();
                AsyncOperationHandle<T> existingHandle = entryExisting.Handle.Convert<T>();
                if(!existingHandle.IsValid())
                {
                    // 句柄无效则重新加载（极边缘场景）
                    _singleCache.Remove((typeof(T), key), out _);
                }
                else
                {
                    // 如果还在进行，确保有进度广播循环（无需重复添加到 _activeOperations 以免重复计算）
                    return await existingHandle.WithCancellation(cancellationToken);
                }
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            AsyncOperationHandle op = handle;
            _singleCache[(typeof(T), key)] = new SingleAssetEntry(op, 1);
            try
            {
                T result = await handle.WithCancellation(cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                throw;
            }
        }

        internal async UniTask<T[]> LoadAssetsAsync<T>(string label, CancellationToken cancellationToken)
        {
            IList<T> list;
            if(_groupCache.TryGetValue((typeof(T), label), out GroupAssetEntry groupExisting))
            {
                _groupCache[(typeof(T), label)] = groupExisting.Inc();
                AsyncOperationHandle<IList<T>> existingHandle = groupExisting.Handle.Convert<IList<T>>();
                if(!existingHandle.IsValid())
                {
                    _groupCache.Remove((typeof(T), label), out _);
                }
                else
                {
                    list = await existingHandle.WithCancellation(cancellationToken);
                    return list?.ToArray() ?? Array.Empty<T>();
                }
            }

            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
            AsyncOperationHandle op = handle;
            _groupCache[(typeof(T), label)] = new GroupAssetEntry(op, 1);
            list = await handle.WithCancellation(cancellationToken);
            return list?.ToArray() ?? Array.Empty<T>();
        }

        internal void ReleaseAsset<T>(string key)
        {
            if(!_singleCache.TryGetValue((typeof(T), key), out SingleAssetEntry entry)) return;
            SingleAssetEntry newEntry = entry.Dec();
            if(newEntry.RefCount <= 0)
            {
                // 真正释放 Addressables 资源与句柄
                try
                {
                    Addressables.Release(entry.Handle);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }

                _singleCache.Remove((typeof(T), key), out _);
            }
            else
            {
                _singleCache[(typeof(T), key)] = newEntry;
            }
        }

        internal void ReleaseAssets<T>(string label)
        {
            if(!_groupCache.TryGetValue((typeof(T), label), out GroupAssetEntry entry)) return;
            GroupAssetEntry newEntry = entry.Dec();
            if(newEntry.RefCount <= 0)
            {
                try
                {
                    Addressables.Release(entry.Handle);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }

                _groupCache.Remove((typeof(T), label), out _);
            }
            else
            {
                _groupCache[(typeof(T), label)] = newEntry;
            }
        }

        #endregion

        #region 内部方法

        private void StopTrackSelectedProgress(string sessionId)
        {
            if(string.IsNullOrEmpty(sessionId)) return;
            if(!_selectedProgressSessions.TryGetValue(sessionId, out CancellationTokenSource cts)) return;
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }

            _selectedProgressSessions.Remove(sessionId, out _);
        }


        // 选定地址集合进度跟踪循环
        private async UniTaskVoid TrackSelectedProgressLoop(string trackId, string[] ids, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    (float progress, int count) = ComputeSelectedProgress(ids);
                    bool isDone = count == 0 || progress >= 0.999f;
                    CF.Broadcast(new AssetsBroadcasts.SelectedAssetsProgress
                    {
                        TrackId = trackId,
                        Progress = count == 0 ? 1f : progress,
                        Count = count,
                        IsDone = isDone
                    }).Forget();

                    if(isDone)
                    {
                        StopTrackSelectedProgress(trackId);
                        break;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        private (float progress, int remainCount) ComputeSelectedProgress(string[] ids)
        {
            if(ids == null || ids.Length == 0) return (0f, 0);
            var sum = 0f;
            var count = 0;
            foreach (string id in ids)
            {
                if(string.IsNullOrEmpty(id)) continue;

                foreach (KeyValuePair<(Type, string), SingleAssetEntry> kv in _singleCache)
                {
                    if(kv.Key.Item2 == id)
                    {
                        sum += kv.Value.Handle.PercentComplete;
                        count++;
                        break;
                    }
                }

                foreach (KeyValuePair<(Type, string), GroupAssetEntry> kv in _groupCache)
                {
                    if(kv.Key.Item2 == id)
                    {
                        sum += kv.Value.Handle.PercentComplete;
                        count++;
                        break;
                    }
                }
            }

            return count == 0 ? (1, 0) : (sum / count, count);
        }

        #endregion

        #region 生命周期

        public async UniTask RegisterAsync(CancellationToken cancellationToken)
        {
            await Addressables.InitializeAsync();
            _logger = CF.CreateLogger(nameof(AssetsSystemModule));
            InitDefaultAssetsReceiver();
        }

        public void UnRegister()
        {
            _assetsReceiverManager.Dispose();
        }

        private void InitDefaultAssetsReceiver()
        {
            _assetsReceiverManager = new AssetsReceiverManager(this, _logger);
            _assetsReceiverManager.RegisterReceiver(typeof(Object));
            _assetsReceiverManager.RegisterReceiver(typeof(ScriptableObject));
            _assetsReceiverManager.RegisterReceiver(typeof(GameObject));
            _assetsReceiverManager.RegisterReceiver(typeof(TextAsset));
        }

        #endregion
    }
}