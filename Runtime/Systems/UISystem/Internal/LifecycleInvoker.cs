using System;
using System.Threading;
using CFramework.Core.Log;
using CFramework.Systems.UISystem.LifeScope;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     生命周期调用器实现
    /// </summary>
    /// <remarks>
    ///     采用异步优先策略：如果实现了异步版本，则只调用异步版本；否则回退到同步版本。
    ///     这样避免了同步和异步方法同时执行的问题，提高性能和代码清晰度。
    /// </remarks>
    internal sealed class LifecycleInvoker : ILifecycleInvoker
    {
        private readonly CFLogger _logger;

        public LifecycleInvoker(CFLogger logger)
        {
            _logger = logger;
        }

        public async UniTask CallCreateAsync(IUIView controller, string viewId, CancellationToken token)
        {
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewCreateAsync createAsync)
            {
                try
                {
                    await createAsync.OnViewCreateAsync(viewId, token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewCreate create)
            {
                try
                {
                    create.OnViewCreate(viewId);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }

        public async UniTask CallDestroyAsync(IUIView controller, CancellationToken token)
        {
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewDestroyAsync destroyAsync)
            {
                try
                {
                    await destroyAsync.OnViewDestroyAsync(token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewDestroy destroy)
            {
                try
                {
                    destroy.OnViewDestroy();
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }

        public async UniTask CallShowBeforeAsync(IViewInstance inst, IViewData data, CancellationToken token)
        {
            IUIView controller = inst.Controller;
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewShowBeforeAsync beforeAsync)
            {
                try
                {
                    await beforeAsync.OnShowBeforeAsync(data, token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewShowBefore before)
            {
                try
                {
                    before.OnShowBefore(data);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }

        public async UniTask CallShowAfterAsync(IViewInstance inst, IViewData data, CancellationToken token)
        {
            IUIView controller = inst.Controller;
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewShowAfterAsync afterAsync)
            {
                try
                {
                    await afterAsync.OnShowAfterAsync(data, token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewShowAfter after)
            {
                try
                {
                    after.OnShowAfter(data);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }

        public async UniTask CallHideAsync(IViewInstance inst, CancellationToken token)
        {
            IUIView controller = inst.Controller;
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewHideAsync hideAsync)
            {
                try
                {
                    await hideAsync.OnHideAsync(token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewHide hide)
            {
                try
                {
                    hide.OnHide();
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }

        public async UniTask CallCloseAsync(IViewInstance inst, CancellationToken token)
        {
            IUIView controller = inst.Controller;
            // 异步优先：有异步实现就不调用同步
            if(controller is IViewCloseAsync closeAsync)
            {
                try
                {
                    await closeAsync.OnViewCloseAsync(token);
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
            else if(controller is IViewClose close)
            {
                try
                {
                    close.OnViewClose();
                }
                catch (Exception e)
                {
                    _logger?.LogException(e);
                }
            }
        }
    }
}