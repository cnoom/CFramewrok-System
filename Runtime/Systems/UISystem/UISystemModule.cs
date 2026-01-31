using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CFramework.Core;
using CFramework.Core.Attributes;
using CFramework.Core.Interfaces;
using CFramework.Core.Interfaces.LifeScope;
using CFramework.Core.Log;
using CFramework.Core.ModuleSystem;
using CFramework.Systems.AssetsSystem;
using CFramework.Systems.UISystem.Internal;
using CFramework.Systems.UISystem.Transitions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFramework.Systems.UISystem
{
    [AutoModule("cUI系统", "管理游戏界面的显示、隐藏、跳转和动画"), ModuleDependsOn(typeof(IAssetsSystem))]
    public sealed class UISystemModule : IModule, IRegisterAsync, IUnRegister, ICancellationHolder, IUISystem
    {
        // 并发控制：最大同时进行的UI操作数
        private const int MaxConcurrentOps = 3;
        private const int MaxRetryCount = 3;
        private readonly SemaphoreSlim _concurrencySemaphore = new SemaphoreSlim(MaxConcurrentOps, MaxConcurrentOps);

        // id -> instance
        private readonly Dictionary<string, IViewInstance> _instances = new Dictionary<string, IViewInstance>();

        // key -> opening guard
        private readonly HashSet<string> _openingKeys = new HashSet<string>();
        private UIConfig _config;

        // 错误处理
        private IUIErrorHandler _errorHandler;

        // 管理器
        private ILayerManager _layerManager;
        private ILifecycleInvoker _lifecycleInvoker;
        private CFLogger _logger;
        private IViewPool _pool;
        private UIRoot _root;
        private ITransitionManager _transitionManager;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public async UniTask RegisterAsync(CancellationToken cancellationToken)
        {
            await CF.Execute(new AssetsCommands.RegisterAssetReceiver(typeof(UITransition)));
            await CF.Execute(new AssetsCommands.RegisterAssetReceiver(typeof(UIConfig)));

            // 直接通过 AssetsSystem 加载 UIConfig
            _config = await CF.Query<AssetsQueries.Asset, UIConfig>(
                new AssetsQueries.Asset("UIConfig"));
            if(!_config)
            {
                CF.LogWarning("UIConfig 未找到，使用默认配置。");
                _config = ScriptableObject.CreateInstance<UIConfig>();
            }

            _logger = CF.CreateLogger(_config.logTag);
            // Apply logging config
            if(_config.enableLogs)
            {
                CF.EnableLogger(_config.logTag);
                CF.SetLogLevel(_config.logLevel, _config.logTag);
            }
            else
            {
                CF.DisableLogger(_config.logTag);
            }

            // Initialize managers
            _layerManager = new LayerManager(_logger, _config.layerOrder);
            _lifecycleInvoker = new LifecycleInvoker(_logger);
            _transitionManager = new TransitionManager(_logger);
            _pool = new UIViewPool(_logger, _config.enableViewPool, _config.viewPoolMaxSize);

            // Build root
            _root = new UIRoot();
            _root.Build(_config, _logger);

            // 自动发现并注册错误处理器
            FindAndRegisterErrorHandler();

            await UniTask.CompletedTask;
        }

        public void UnRegister()
        {
            // 关闭所有视图
            foreach (IViewInstance instance in _instances.Values)
            {
                InternalCloseAsync(instance, 0f).Forget();
            }

            // 清空视图池
            _pool?.Clear();

            // 释放并发信号量
            _concurrencySemaphore?.Dispose();

            // 释放 UIConfig 引用
            if(_config)
            {
                CF.Execute(new AssetsCommands.ReleaseAsset<ScriptableObject>("CF_UIConfig")).Forget();
            }
        }

        /// <summary>查找并注册实现了IUIErrorHandler的模块</summary>
        private void FindAndRegisterErrorHandler()
        {
            // 通过反射查找已注册的错误处理器
            try
            {
                Type handlerType = Type.GetType("CFramework.Systems.UISystem.IUIErrorHandler");
                if(handlerType != null)
                {
                    // 这里简化处理，实际可以通过ModuleManager或依赖注入获取
                    // 用户可以通过在RegisterAsync中调用CF.RegisterHandler注册
                }
            }
            catch (Exception e)
            {
                _logger?.LogWarning($"查找错误处理器失败: {e.Message}");
            }
        }

        /// <summary>注册错误处理器</summary>
        public void RegisterErrorHandler(IUIErrorHandler handler)
        {
            _errorHandler = handler;
            _logger?.LogInfo("UI错误处理器已注册");
        }

        /// <summary>取消注册错误处理器</summary>
        public void UnregisterErrorHandler()
        {
            _errorHandler = null;
        }

        /// <summary>根据异常类型分类错误严重程度</summary>
        private UIErrorSeverity ClassifyError(Exception e)
        {
            return e switch
            {
                FileNotFoundException => UIErrorSeverity.Fatal,
                MissingComponentException => UIErrorSeverity.Fatal,
                MissingReferenceException => UIErrorSeverity.Fatal,
                TimeoutException => UIErrorSeverity.Retryable,
                OperationCanceledException => UIErrorSeverity.Recoverable,
                InvalidOperationException => UIErrorSeverity.Recoverable,
                _ => UIErrorSeverity.Recoverable
            };
        }

        /// <summary>尝试处理错误并返回处理结果</summary>
        private async UniTask<ErrorHandlingResult> TryHandleError(UIErrorContext error, int retryCount = 0)
        {
            // 1. 使用自定义错误处理器
            if(_errorHandler != null)
            {
                try
                {
                    ErrorHandlingResult result = await _errorHandler.HandleError(error, CancellationTokenSource.Token);
                    if(result != ErrorHandlingResult.Abort)
                    {
                        _logger?.LogInfo($"自定义错误处理器返回: {result}");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"自定义错误处理器执行失败: {ex.Message}");
                }
            }

            // 2. 根据严重级自动处理
            switch(error.Severity)
            {
                case UIErrorSeverity.Recoverable:
                    _logger?.LogWarning($"可恢复错误，跳过: {error.Exception.Message}");
                    return ErrorHandlingResult.Continue;

                case UIErrorSeverity.Retryable:
                    if(retryCount < MaxRetryCount)
                    {
                        _logger?.LogWarning($"重试中 ({retryCount + 1}/{MaxRetryCount}): {error.Operation}");
                        // 指数退避延迟
                        int delayMs = 500 * (retryCount + 1);
                        await UniTask.Delay(delayMs, cancellationToken: CancellationTokenSource.Token);
                        return ErrorHandlingResult.Retry;
                    }

                    _logger?.LogError($"达到最大重试次数，放弃重试: {error.Operation}");
                    break;

                case UIErrorSeverity.Fatal:
                    _logger?.LogError($"致命错误: {error.Exception.Message}");
                    break;
            }

            // 广播失败事件
            if(!string.IsNullOrEmpty(error.ViewKey))
            {
                CF.Broadcast(new UIBroadcasts.ViewOpenFailed(
                    error.ViewKey,
                    error.Context?.ToString(),
                    error.Exception.Message
                )).Forget();
            }

            return ErrorHandlingResult.Abort;
        }

        #region Commands

        [CommandHandler]
        private async UniTask OnOpen(UICommands.OpenView cmd, CancellationToken token)
        {
            await DoOpenAsync(cmd.Key, cmd.Layer, cmd.Data, cmd.TransitionName, cmd.Seconds, cmd.BringToTop, token);
        }

        [CommandHandler]
        private async UniTask OnOpenByType(UICommands.OpenViewByType cmd, CancellationToken token)
        {
            if(cmd.ViewType == null) return;

            // 从Config获取address
            string key = _config.GetUiAddressByType(cmd.ViewType);
            if(string.IsNullOrEmpty(key))
            {
                _logger?.LogError($"未找到类型 {cmd.ViewType.Name} 对应的UI配置");
                CF.Broadcast(new UIBroadcasts.ViewOpenFailed(cmd.ViewType.Name, cmd.Layer, "Type not configured"))
                    .Forget();
                return;
            }

            // 委托给通用打开逻辑
            await DoOpenAsync(key, cmd.Layer, cmd.Data, cmd.TransitionName, cmd.Seconds, cmd.BringToTop, token);
        }

        /// <summary>通用的打开视图逻辑（避免嵌套CommandHandler导致的死锁）</summary>
        private async UniTask DoOpenAsync(string key, string layer, IViewData data,
            string transitionName, float? seconds, bool bringToTop, CancellationToken token)
        {
            if(string.IsNullOrEmpty(key)) return;

            var retryCount = 0;
            while (true)
            {
                await _concurrencySemaphore.WaitAsync(token);

                var added = false;
                try
                {
                    if(_config.respectGlobalPause && Mathf.Approximately(Time.timeScale, 0f))
                    {
                        _logger?.LogInfo("UI open blocked due to global pause");
                        CF.Broadcast(new UIBroadcasts.ViewOpenFailed(key, layer, "Global paused")).Forget();
                        return;
                    }

                    added = _openingKeys.Add(key);
                    if(!added)
                    {
                        _logger?.LogWarning($"视图正在打开中: {key}");
                        return;
                    }

                    string resolvedLayer = _layerManager.ResolveLayer(layer, null);

                    // 检查是否已存在
                    IViewInstance existing = _layerManager.FindTopByKey(resolvedLayer, key);
                    if(_config.preventReplaySameView && existing != null)
                    {
                        UICommands.OpenView cmd = new UICommands.OpenView(key, resolvedLayer, data, transitionName,
                            seconds, bringToTop);
                        await HandleExistingViewAsync(existing, cmd);
                        return;
                    }

                    // 尝试从池获取
                    IViewInstance instance = _pool.Get(key);
                    if(instance != null)
                    {
                        UICommands.OpenView cmd = new UICommands.OpenView(key, resolvedLayer, data, transitionName,
                            seconds, bringToTop);
                        await ShowPooledViewAsync(instance, resolvedLayer, cmd);
                        return;
                    }

                    // 加载新实例
                    UICommands.OpenView cmd2 =
                        new UICommands.OpenView(key, resolvedLayer, data, transitionName, seconds, bringToTop);
                    await LoadAndShowNewViewAsync(resolvedLayer, cmd2);

                    // 成功则跳出循环
                }
                catch (Exception e)
                {
                    UIErrorContext error = new UIErrorContext(
                        "OpenView", key, e, ClassifyError(e),
                        new
                        {
                            layer,
                            data,
                            transitionName,
                            seconds,
                            bringToTop
                        }
                    );

                    ErrorHandlingResult result = await TryHandleError(error, retryCount);

                    if(result == ErrorHandlingResult.Retry)
                    {
                        retryCount++;
                        _concurrencySemaphore.Release();
                        continue;
                    }

                    if(result == ErrorHandlingResult.Continue)
                    {
                        // 跳过错误继续
                    }

                    // Abort: 中止操作
                }
                finally
                {
                    if(added) _openingKeys.Remove(key);
                    _concurrencySemaphore.Release();
                }

                // Abort时退出循环
                break;
            }
        }

        [CommandHandler]
        private async UniTask OnCloseById(UICommands.CloseViewById cmd, CancellationToken token)
        {
            if(string.IsNullOrEmpty(cmd.Id)) return;
            if(!_instances.TryGetValue(cmd.Id, out IViewInstance inst)) return;
            await InternalCloseAsync(inst, cmd.Seconds);
        }

        [CommandHandler]
        private async UniTask OnCloseByKey(UICommands.CloseViewByKey cmd, CancellationToken token)
        {
            if(string.IsNullOrEmpty(cmd.Key)) return;
            IViewInstance inst = _layerManager.FindTopByKey(null, cmd.Key);
            if(inst == null)
            {
                // 如果在层顶没找到，查找任意位置
                foreach (KeyValuePair<string, IViewInstance> kv in _instances)
                {
                    if(kv.Value.Key == cmd.Key)
                    {
                        inst = kv.Value;
                        break;
                    }
                }
            }

            if(inst == null) return;
            await InternalCloseAsync(inst, cmd.Seconds);
        }

        [CommandHandler]
        private async UniTask OnCloseTop(UICommands.CloseTop cmd, CancellationToken token)
        {
            string layer = _layerManager.ResolveLayer(cmd.Layer, null);
            IViewInstance top = _layerManager.GetTop(layer);
            if(top == null) return;
            await InternalCloseAsync(top, cmd.Seconds);
        }

        [CommandHandler]
        private async UniTask OnHide(UICommands.HideView cmd, CancellationToken token)
        {
            if(string.IsNullOrEmpty(cmd.Id)) return;
            if(!_instances.TryGetValue(cmd.Id, out IViewInstance inst)) return;
            if(!inst.Visible) return;
            if(!inst.Root) return;

            try
            {
                await _transitionManager.PlayOutAsync(inst.Root, inst.TransitionName,
                    cmd.Seconds ?? inst.TransitionSeconds);
                await _lifecycleInvoker.CallHideAsync(inst, CancellationTokenSource.Token);
                inst.Visible = false;
                CF.Broadcast(new UIBroadcasts.ViewHidden(inst.Id, inst.Key, inst.Layer)).Forget();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        [CommandHandler]
        private async UniTask OnShow(UICommands.ShowView cmd, CancellationToken token)
        {
            if(string.IsNullOrEmpty(cmd.Id)) return;
            if(!_instances.TryGetValue(cmd.Id, out IViewInstance inst)) return;
            if(inst.Visible) return;

            try
            {
                await _transitionManager.PlayInAsync(inst.Root, inst.TransitionName,
                    cmd.Seconds ?? inst.TransitionSeconds);
                inst.Visible = true;
                CF.Broadcast(new UIBroadcasts.ViewShown(inst.Id, inst.Key, inst.Layer)).Forget();
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        #endregion

        #region Queries

        [QueryHandler]
        private UniTask<bool> OnIsOpen(UIQueries.IsOpen q, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(q.Id)) return UniTask.FromResult(_instances.ContainsKey(q.Id));
            if(!string.IsNullOrEmpty(q.Key))
            {
                foreach (IViewInstance inst in _instances.Values)
                {
                    if(inst.Key == q.Key)
                        return UniTask.FromResult(true);
                }

                return UniTask.FromResult(false);
            }

            return UniTask.FromResult(false);
        }

        [QueryHandler]
        private UniTask<ViewInfo> OnGetTop(UIQueries.GetTop q, CancellationToken token)
        {
            string layer = _layerManager.ResolveLayer(q.Layer, null);
            IViewInstance top = _layerManager.GetTop(layer);
            if(top == null) return default;
            return UniTask.FromResult(new ViewInfo(top.Id, top.Key, layer, top.Visible));
        }

        [QueryHandler]
        private UniTask<ViewInfo[]> OnGetOpenViews(UIQueries.GetOpenViews q, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(q.Layer))
            {
                string layer = _layerManager.ResolveLayer(q.Layer, null);
                List<IViewInstance> list = GetAllInLayer(layer);
                ViewInfo[] arr = new ViewInfo[list.Count];
                for(var i = 0; i < list.Count; i++)
                    arr[i] = new ViewInfo(list[i].Id, list[i].Key, layer, list[i].Visible);
                return UniTask.FromResult(arr);
            }

            List<ViewInfo> res = new List<ViewInfo>();
            foreach (IViewInstance instance in _instances.Values)
            {
                res.Add(new ViewInfo(instance.Id, instance.Key, instance.Layer, instance.Visible));
            }

            return UniTask.FromResult(res.ToArray());
        }

        #endregion

        #region Internals

        /// <summary>获取指定UI的池化最大数量</summary>
        private int GetPoolSize(string key)
        {
            if(!_config.enableViewPool) return 0;

            UiInfo uiInfo = _config.GetUiInfoByKey(key);
            if(uiInfo != null && uiInfo.disablePooling) return 0;

            return uiInfo?.maxPoolSize ?? _config.viewPoolMaxSize;
        }

        /// <summary>处理已存在的视图</summary>
        private async UniTask HandleExistingViewAsync(IViewInstance existing, UICommands.OpenView cmd)
        {
            if(!existing.Visible)
            {
                // 隐藏的视图，重新显示
                await _transitionManager.PlayInAsync(existing.Root, existing.TransitionName,
                    existing.TransitionSeconds);
                await _lifecycleInvoker.CallShowBeforeAsync(existing, cmd.Data, CancellationTokenSource.Token);
                existing.Visible = true;
                await _lifecycleInvoker.CallShowAfterAsync(existing, cmd.Data, CancellationTokenSource.Token);
                CF.Broadcast(new UIBroadcasts.ViewShown(existing.Id, existing.Key, existing.Layer)).Forget();
            }
            else if(cmd.BringToTop)
            {
                // 已显示且要求置顶
                _layerManager.MoveToTop(existing);
                CF.Broadcast(new UIBroadcasts.ViewShown(existing.Id, existing.Key, existing.Layer)).Forget();
            }
        }

        /// <summary>显示池中的视图</summary>
        private async UniTask ShowPooledViewAsync(IViewInstance instance, string layer, UICommands.OpenView cmd)
        {
            // 设置新属性
            instance.Layer = layer;
            instance.TransitionName = string.IsNullOrEmpty(cmd.TransitionName)
                ? _config.defaultTransition
                : cmd.TransitionName;
            instance.TransitionSeconds = cmd.Seconds.HasValue
                ? Mathf.Max(0f, cmd.Seconds.Value)
                : Mathf.Max(0f, _config.defaultTransitionSeconds);

            // 重新设置父节点
            RectTransform parent = _root.Layers[layer];
            instance.Root.transform.SetParent(parent, false);

            // 显示
            await _lifecycleInvoker.CallShowBeforeAsync(instance, cmd.Data, CancellationTokenSource.Token);
            await _transitionManager.PlayInAsync(instance.Root, instance.TransitionName, instance.TransitionSeconds);
            instance.Visible = true;
            await _lifecycleInvoker.CallShowAfterAsync(instance, cmd.Data, CancellationTokenSource.Token);

            // 注册到系统
            _instances[instance.Id] = instance;
            ((LayerManager)_layerManager).AddToLayer(instance);
            CF.Broadcast(new UIBroadcasts.ViewOpened(instance.Id, instance.Key, layer)).Forget();
        }

        /// <summary>加载并显示新视图</summary>
        private async UniTask LoadAndShowNewViewAsync(string layer, UICommands.OpenView cmd)
        {
            // Load prefab via AssetsSystem
            GameObject prefab = null;
            try
            {
                prefab = await CF.Query<AssetsQueries.Asset, GameObject>(new AssetsQueries.Asset(cmd.Key));
            }
            catch (Exception e)
            {
                UIErrorContext error = new UIErrorContext(
                    "LoadAsset", cmd.Key, e, ClassifyError(e), layer
                );

                ErrorHandlingResult result = await TryHandleError(error);
                if(result == ErrorHandlingResult.Abort)
                {
                    throw;
                }

                return;
            }

            if(!prefab)
            {
                UIErrorContext error = new UIErrorContext(
                    "LoadAsset",
                    cmd.Key,
                    new FileNotFoundException($"Prefab not found: {cmd.Key}"),
                    UIErrorSeverity.Fatal,
                    layer
                );

                await TryHandleError(error);
                return;
            }

            // Instantiate under layer
            RectTransform parent = _root.Layers[layer];
            GameObject go = Object.Instantiate(prefab, parent);
            go.name = $"{prefab.name}_Instance";
            go.gameObject.SetActive(false);

            // Call create lifecycle
            IUIView controller = go.GetComponent<IUIView>();
            var viewId = Guid.NewGuid().ToString("N");
            await _lifecycleInvoker.CallCreateAsync(controller, viewId, CancellationTokenSource.Token);

            UIViewInstance inst = new UIViewInstance
            {
                Id = viewId,
                Key = cmd.Key,
                Layer = layer,
                Root = go,
                Controller = controller,
                TransitionName = string.IsNullOrEmpty(cmd.TransitionName)
                    ? _config.defaultTransition
                    : cmd.TransitionName,
                TransitionSeconds = cmd.Seconds.HasValue
                    ? Mathf.Max(0f, cmd.Seconds.Value)
                    : Mathf.Max(0f, _config.defaultTransitionSeconds),
                Visible = false
            };

            // 注入实例引用
            controller.SetViewInstance(inst);

            // Show
            await _lifecycleInvoker.CallShowBeforeAsync(inst, cmd.Data, CancellationTokenSource.Token);
            await _transitionManager.PlayInAsync(go, inst.TransitionName, inst.TransitionSeconds);
            inst.Visible = true;
            await _lifecycleInvoker.CallShowAfterAsync(inst, cmd.Data, CancellationTokenSource.Token);

            // Push to stack
            _instances[inst.Id] = inst;
            ((LayerManager)_layerManager).AddToLayer(inst);
            CF.Broadcast(new UIBroadcasts.ViewOpened(inst.Id, inst.Key, layer)).Forget();
        }

        /// <summary>关闭视图内部实现</summary>
        private async UniTask InternalCloseAsync(IViewInstance inst, float? secondsOverride = null)
        {
            try
            {
                if(!inst.Root) return;

                // 播放退出动画
                if(inst.Visible)
                {
                    float seconds = secondsOverride ?? inst.TransitionSeconds;
                    await _transitionManager.PlayOutAsync(inst.Root, inst.TransitionName, seconds);
                }

                // 先调用Hide生命周期（此时Visible仍为true）
                await _lifecycleInvoker.CallHideAsync(inst, CancellationTokenSource.Token);

                // 改变可见状态
                inst.Visible = false;

                // 调用Close生命周期（重置状态）
                await _lifecycleInvoker.CallCloseAsync(inst, CancellationTokenSource.Token);

                // 移除引用
                ((LayerManager)_layerManager).RemoveFromLayer(inst);
                _instances.Remove(inst.Id);

                // 归还到池或销毁（传入特定UI的池大小）
                await _pool.ReturnAsync(inst, GetPoolSize(inst.Key));

                // 广播关闭事件
                if(CF.IsInitialized)
                {
                    CF.Broadcast(new UIBroadcasts.ViewClosed(inst.Id, inst.Key, inst.Layer)).Forget();
                }
            }
            catch (Exception e)
            {
                _logger?.LogException(e);
            }
        }

        /// <summary>获取指定层所有视图（用于查询）</summary>
        private List<IViewInstance> GetAllInLayer(string layer)
        {
            List<IViewInstance> result = new List<IViewInstance>();
            foreach (IViewInstance inst in _instances.Values)
            {
                if(inst.Layer == layer)
                    result.Add(inst);
            }

            return result;
        }

        #endregion
    }
}