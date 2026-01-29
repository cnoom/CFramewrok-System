using System;
using System.Collections.Generic;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using CFramework.Systems.AssetsSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UnityContainerSystem
{
    [AutoModule("cUnity容器系统", "管理Unity游戏对象的生命周期和依赖")]
    [ModuleDependsOn(typeof(IAssetsSystem))]
    public class UnityContainerSystemModule : IModule, IUnityContainerSystem, IRegisterAsync
    {

        // 存储与并发保护 - 使用读写锁优化读多写少场景
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, GoGroupBind> goGroupBinds = new Dictionary<string, GoGroupBind>();
        private readonly Dictionary<string, Dictionary<string, GoBind>> scopedGoBinds = new Dictionary<string, Dictionary<string, GoBind>>();
        private ICFLogger _logger;
        [Tooltip("全局配置文件")]
        public UnityContainerConfig config;

        public async UniTask RegisterAsync(CancellationToken cancellationToken)
        {
            // 直接通过 AssetsSystem 加载配置
            await CF.Execute(new AssetsCommands.RegisterAssetReceiver(typeof(UnityContainerConfig)));
            config = await CF.Query<AssetsQueries.Asset, UnityContainerConfig>(
                new AssetsQueries.Asset("CF_UnityContainerConfig"));
            _logger = CF.CreateLogger(config?.tag ?? nameof(IUnityContainerSystem));
        }

        /// <summary>
        ///     获取全局冲突策略（默认 Warn）
        /// </summary>
        private DuplicatePolicy GetGlobalPolicy()
        {
            return config?.globalDuplicatePolicy ?? DuplicatePolicy.Warn;
        }

        #region 命令处理

        [CommandHandler]
        private UniTask HandleRegisterGo(UnityGoCommands.RegisterGoBind cmd, CancellationToken token)
        {
            GoBind goBind = cmd.GoBind;
            if(goBind == null)
            {
                _logger.LogWarning("[UnityContainerSystem] Register 命令收到空 GoBind，已忽略。");
                return UniTask.CompletedTask;
            }


            _lock.EnterWriteLock();
            try
            {
                if(!scopedGoBinds.TryGetValue(goBind.scope, out Dictionary<string, GoBind> binds))
                {
                    binds = new Dictionary<string, GoBind>();
                    scopedGoBinds[goBind.scope] = binds;
                }

                if(!binds.TryAdd(goBind.GameObject.name, goBind))
                {
                    HandleDuplicateT(binds, goBind.GameObject.name, binds[goBind.GameObject.name], goBind);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask HandleUnregisterGo(UnityGoCommands.UnregisterGoBind cmd, CancellationToken token)
        {
            string goName = cmd.GoName;
            if(string.IsNullOrEmpty(goName)) return UniTask.CompletedTask;

            _lock.EnterWriteLock();
            try
            {
                if(!scopedGoBinds.TryGetValue(cmd.Scope, out Dictionary<string, GoBind> binds)) return UniTask.CompletedTask;
                if(!binds.TryGetValue(goName, out GoBind goBind)) return UniTask.CompletedTask;
                binds.Remove(goName);
                goBind.Dispose();

                // 清理空 scope
                if(binds.Count == 0)
                    scopedGoBinds.Remove(cmd.Scope);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask HandleRegisterGos(UnityGoCommands.RegisterGosBind cmd, CancellationToken token)
        {
            GoGroupBind goGroupBind = cmd.GoGroupBind;
            if(goGroupBind == null)
            {
                _logger.LogWarning("[UnityContainerSystem] Register 命令收到空 GoGroupBind，已忽略。");
                return UniTask.CompletedTask;
            }

            _lock.EnterWriteLock();
            try
            {
                if(!goGroupBinds.TryAdd(goGroupBind.GroupKey, goGroupBind))
                {
                    HandleDuplicateT(goGroupBinds, goGroupBind.GroupKey, goGroupBinds[goGroupBind.GroupKey],
                        goGroupBind);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return UniTask.CompletedTask;
        }

        [CommandHandler]
        private UniTask HandleUnRegisterGos(UnityGoCommands.UnregisterGosBind cmd, CancellationToken token)
        {
            string groupKey = cmd.GroupName;
            if(string.IsNullOrEmpty(groupKey))
            {
                _logger.LogWarning("[UnityContainerSystem] UnregisterGosBind 命令收到空 groupKey，已忽略。");
            }

            _lock.EnterWriteLock();
            try
            {
                if(!goGroupBinds.TryGetValue(groupKey, out GoGroupBind groupBind)) return UniTask.CompletedTask;
                goGroupBinds.Remove(groupKey);
                groupBind.Dispose();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return UniTask.CompletedTask;
        }

        private void HandleDuplicateT<T>(Dictionary<string, T> dict, string key, T existing, T newT)
        {
            DuplicatePolicy policy = GetGlobalPolicy();

            switch(policy)
            {
                case DuplicatePolicy.KeepFirst:
                    if(!existing.Equals(newT))
                        _logger.LogWarning($"[UnityContainerSystem] key='{key}' 已存在，策略为 KeepFirst，保持原值。");
                    break;
                case DuplicatePolicy.Replace:
                    if(!existing.Equals(newT))
                        _logger.LogWarning($"[UnityContainerSystem] key='{key}' 已存在，策略为 Replace，已替换为新对象。");
                    dict[key] = newT;
                    break;
                case DuplicatePolicy.Throw:
                    throw new InvalidOperationException($"[UnityContainerSystem] 容器中已存在相同 key: {key}");
                case DuplicatePolicy.Warn:
                    _logger.LogWarning($"[UnityContainerSystem] key='{key}' 已存在，策略为 Warn，保持原值。");
                    break;
                case DuplicatePolicy.WarnAndReplace:
                    _logger.LogWarning($"[UnityContainerSystem] key='{key}' 已存在，策略为 Warn，已替换为新对象。");
                    break;
            }
        }

        #endregion

        #region 查询处理

        /// <summary>
        ///     通用查询处理方法（减少重复代码）
        /// </summary>
        private UniTask<T> HandleQuery<T>(string key, string scope, Func<GoBind, T> selector, string queryTypeName)
        {
            _lock.EnterReadLock();
            try
            {
                // 参数验证
                if(string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning($"[UnityContainerSystem] {queryTypeName} 查询 key 为空，返回 null。");
                    return UniTask.FromResult<T>(default);
                }

                string actualScope = scope ?? string.Empty;
                if(!scopedGoBinds.TryGetValue(actualScope, out Dictionary<string, GoBind> binds))
                {
                    _logger.LogWarning($"[UnityContainerSystem] {queryTypeName} 未找到 Scope '{actualScope}'，返回 null。");
                    return UniTask.FromResult<T>(default);
                }

                if(!binds.TryGetValue(key, out GoBind goBind))
                {
                    _logger.LogWarning(
                        $"[UnityContainerSystem] {queryTypeName} 在 Scope '{actualScope}' 中未找到 Key '{key}'，返回 null。");
                    return UniTask.FromResult<T>(default);
                }

                try
                {
                    T result = selector(goBind);
                    return UniTask.FromResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"[UnityContainerSystem] {queryTypeName} 查询执行失败 (Key: {key}, Scope: {actualScope}), 错误: {ex.Message}");
                    return UniTask.FromResult<T>(default);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UnityContainerSystem] 查询处理异常 (Key: {key}), 错误: {ex.Message}");
                return UniTask.FromResult<T>(default);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        [QueryHandler]
        private UniTask<GameObject> HandleGetGameObject(UnityGoQueries.GetGameObject query, CancellationToken ct)
        {
            return HandleQuery(
                query.Key,
                query.Scope,
                goBind => goBind.GameObject,
                "GetGameObject"
            );
        }

        [QueryHandler]
        private UniTask<Transform> HandleGetTransform(UnityGoQueries.GetTransform query, CancellationToken ct)
        {
            return HandleQuery(
                query.Key,
                query.Scope,
                goBind => goBind.Transform,
                "GetTransform"
            );
        }

        [QueryHandler]
        private UniTask<RectTransform> HandleGetRectTransform(UnityGoQueries.GetRectTransform query,
            CancellationToken ct)
        {
            return HandleQuery(
                query.Key,
                query.Scope,
                goBind => goBind.RectTransform,
                "GetRectTransform"
            );
        }

        [QueryHandler]
        private UniTask<Component> HandleGetComponent(UnityGoQueries.GetComponent query, CancellationToken ct)
        {
            // 参数验证
            if(query.ComponentType == null)
            {
                CF.LogError("[UnityContainerSystem] GetComponent 查询 ComponentType 为 null，返回 null。");
                return UniTask.FromResult<Component>(null);
            }

            return HandleQuery(
                query.Key,
                query.Scope,
                goBind => goBind.Get(query.ComponentType),
                $"GetComponent<{query.ComponentType.Name}>"
            );
        }

        #endregion
    }
}