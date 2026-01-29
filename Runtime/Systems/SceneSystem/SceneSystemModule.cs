using System;
using System.Collections.Generic;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CFramework.Systems.SceneSystem
{
    /// <summary>
    ///     场景系统第一阶段：仅支持 Single 模式加载，统一提供命令/查询/广播。
    ///     后续可扩展 Additive、预加载、配置驱动等功能。
    /// </summary>
    [AutoModule("c场景系统", "管理游戏场景的加载、卸载和切换")]
    public sealed class SceneSystemModule : IModule, IRegister, IUnRegister, ISceneSystem
    {

        private readonly Dictionary<string, SceneRuntimeInfo> _scenes = new Dictionary<string, SceneRuntimeInfo>();
        private AsyncOperationHandle<SceneInstance> _activeHandle;

        private string _currentSceneKey;
        private string _loadingSceneKey;
        private CFLogger _logger;

        public void Register()
        {
            _logger = CF.CreateLogger("Scene");
        }

        public void UnRegister()
        {
            _scenes.Clear();
            _currentSceneKey = null;
            _loadingSceneKey = null;
            if(_activeHandle.IsValid())
            {
                Addressables.Release(_activeHandle);
                _activeHandle = default;
            }
        }

        private struct SceneRuntimeInfo
        {
            public string SceneKey;
            public string UnitySceneName;
            public SceneLoadState State;
            public float LastProgress;
            public string LastError;
        }

        #region 命令处理

        [CommandHandler]
        private async UniTask OnLoadScene(SceneCommands.LoadScene cmd, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(cmd.SceneKey))
            {
                _logger?.LogWarning("[SceneSystem] 收到空 SceneKey，已忽略。");
                return;
            }

            // 当前实现只支持一次一个加载任务，后续可扩展并发策略
            if(_activeHandle.IsValid() && !_activeHandle.IsDone)
            {
                _logger?.LogWarning($"[SceneSystem] 已在加载场景 '{_loadingSceneKey}'，忽略新的加载请求 '{cmd.SceneKey}'。");
                return;
            }

            string sceneKey = cmd.SceneKey;
            string unitySceneName = sceneKey; // 第一阶段：SceneKey == Unity 场景名

            _loadingSceneKey = sceneKey;

            SceneRuntimeInfo info = GetOrCreate(sceneKey, unitySceneName);
            info.State = SceneLoadState.Loading;
            info.LastError = null;
            info.LastProgress = 0f;
            _scenes[sceneKey] = info;

            // 场景加载开始一律广播，由上层决定是否展示 Loading UI
            CF.Broadcast(new SceneBroadcasts.SceneLoadingStarted(sceneKey)).Forget();

            AsyncOperationHandle<SceneInstance> handle = default;
            try
            {
                handle = Addressables.LoadSceneAsync(unitySceneName);
                if(!handle.IsValid())
                {
                    throw new InvalidOperationException($"无法开始加载场景: {unitySceneName}");
                }

                _activeHandle = handle;

                while (!handle.IsDone)
                {
                    float progress = handle.PercentComplete;
                    info.LastProgress = progress;
                    _scenes[sceneKey] = info;
                    CF.Broadcast(new SceneBroadcasts.SceneLoadingProgress(sceneKey, progress)).Forget();
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                if(handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new InvalidOperationException($"加载场景失败: {unitySceneName}, status={handle.Status}");
                }

                info.State = SceneLoadState.Activated;
                info.LastProgress = 1f;
                _scenes[sceneKey] = info;

                _currentSceneKey = sceneKey;
                _loadingSceneKey = null;

                CF.Broadcast(new SceneBroadcasts.SceneLoaded(sceneKey)).Forget();
            }
            catch (Exception ex)
            {
                info.State = SceneLoadState.NotLoaded;
                info.LastError = ex.Message;
                _scenes[sceneKey] = info;

                _loadingSceneKey = null;

                _logger?.LogException(ex);
                CF.Broadcast(new SceneBroadcasts.SceneLoadFailed(sceneKey, ex.Message)).Forget();
            }
        }

        private SceneRuntimeInfo GetOrCreate(string sceneKey, string unitySceneName)
        {
            if(_scenes.TryGetValue(sceneKey, out SceneRuntimeInfo info)) return info;
            info = new SceneRuntimeInfo
            {
                SceneKey = sceneKey,
                UnitySceneName = unitySceneName,
                State = SceneLoadState.NotLoaded,
                LastProgress = 0f,
                LastError = null
            };
            _scenes[sceneKey] = info;
            return info;
        }

        #endregion

        #region 查询处理

        [QueryHandler]
        private UniTask<SceneQueries.SceneInfo> OnGetCurrentSceneInfo(SceneQueries.GetCurrentSceneInfo query, CancellationToken ct)
        {
            if(string.IsNullOrEmpty(_currentSceneKey))
            {
                return UniTask.FromResult<SceneQueries.SceneInfo>(default);
            }

            if(!_scenes.TryGetValue(_currentSceneKey, out SceneRuntimeInfo info))
            {
                return UniTask.FromResult<SceneQueries.SceneInfo>(default);
            }

            SceneQueries.SceneInfo result = new SceneQueries.SceneInfo(info.SceneKey, info.UnitySceneName, info.State, info.LastProgress,
                info.LastError);
            return UniTask.FromResult(result);
        }

        [QueryHandler]
        private UniTask<float> OnGetSceneProgress(SceneQueries.GetSceneProgress query, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(query.SceneKey)) return UniTask.FromResult(0f);
            if(_scenes.TryGetValue(query.SceneKey, out SceneRuntimeInfo info))
            {
                return UniTask.FromResult(info.LastProgress);
            }

            return UniTask.FromResult(0f);
        }

        #endregion
    }
}