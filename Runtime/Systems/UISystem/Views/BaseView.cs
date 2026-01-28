using System.Threading;
using CFramework.Systems.UISystem.Internal;
using CFramework.Systems.UISystem.LifeScope;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem
{
    /// <summary>
    ///     UI视图控制器基类，提供默认生命周期实现。
    /// </summary>
    /// <remarks>
    ///     继承此类可选择性地重写生命周期方法。
    ///     所有生命周期方法都提供了同步和异步两个版本。
    ///     异步优先策略：如果实现了异步版本，则只调用异步版本；否则回退到同步版本。
    ///     对象池生命周期：OnPoolRetrieve（从池取出）和OnPoolReturn（归还到池）。
    /// </remarks>
    public abstract class BaseView : MonoBehaviour, IUIView,
        IViewCreate, IViewCreateAsync,
        IViewDestroy, IViewDestroyAsync,
        IViewShowBefore, IViewShowBeforeAsync,
        IViewShowAfter, IViewShowAfterAsync,
        IViewHide, IViewHideAsync,
        IViewClose, IViewCloseAsync,
        IViewPoolRetrieve, IViewPoolReturn
    {
        /// <summary>视图实例引用，可访问Layer、Visible等状态</summary>
        protected IViewInstance Instance { get; private set; }

        /// <summary>视图实例唯一ID（通过OnViewCreate注入）</summary>
        protected string ViewId { get; private set; }

        /// <summary>视图是否可见</summary>
        public bool IsVisible { get; private set; }

        /// <summary>设置视图实例引用（供UISystemModule内部使用）</summary>
        void IUIView.SetViewInstance(IViewInstance instance)
        {
            Instance = instance;
        }

        /// <summary>获取视图数据接口（供内部使用）</summary>
        internal void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        #region 生命周期 - 可重写

        /// <summary>视图创建时调用（同步版本）</summary>
        /// <param name="viewId">视图实例唯一ID，由系统分配</param>
        protected virtual void OnViewCreate(string viewId) {ViewId = viewId;}

        /// <summary>视图创建时调用（异步版本）</summary>
        /// <param name="viewId">视图实例唯一ID，由系统分配</param>
        /// <param name="token">取消令牌</param>
        protected virtual UniTask OnViewCreateAsync(string viewId, CancellationToken token)
        {
            ViewId = viewId;
            return UniTask.CompletedTask;
        }

        /// <summary>视图销毁时调用（同步版本）</summary>
        protected virtual void OnViewDestroy() { }

        /// <summary>视图销毁时调用（异步版本）</summary>
        protected virtual UniTask OnViewDestroyAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>显示前调用（同步版本）</summary>
        protected virtual void OnShowBefore(IViewData data) { }

        /// <summary>显示前调用（异步版本）</summary>
        protected virtual UniTask OnShowBeforeAsync(IViewData data, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>显示后调用（同步版本）</summary>
        protected virtual void OnShowAfter(IViewData data) { }

        /// <summary>显示后调用（异步版本）</summary>
        protected virtual UniTask OnShowAfterAsync(IViewData data, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>隐藏时调用（同步版本）</summary>
        protected virtual void OnHide() { }

        /// <summary>隐藏时调用（异步版本）</summary>
        protected virtual UniTask OnHideAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>关闭时调用（同步版本），用于重置视图状态</summary>
        protected virtual void OnViewClose() { }

        /// <summary>关闭时调用（异步版本），用于重置视图状态</summary>
        protected virtual UniTask OnViewCloseAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>视图从对象池中取出时调用，用于清理上次残留状态</summary>
        protected virtual void OnPoolRetrieve() { }

        /// <summary>视图归还到对象池时调用，用于保存状态或清理引用</summary>
        protected virtual void OnPoolReturn() { }

        #endregion

        #region 显式接口实现（供UISystemModule调用）

        void IViewCreate.OnViewCreate(string viewId)
        {
            OnViewCreate(viewId);
        }
        UniTask IViewCreateAsync.OnViewCreateAsync(string viewId, CancellationToken token)
        {
            return OnViewCreateAsync(viewId, token);
        }

        void IViewDestroy.OnViewDestroy()
        {
            OnViewDestroy();
        }
        UniTask IViewDestroyAsync.OnViewDestroyAsync(CancellationToken token)
        {
            return OnViewDestroyAsync(token);
        }

        void IViewShowBefore.OnShowBefore(IViewData data)
        {
            OnShowBefore(data);
        }
        UniTask IViewShowBeforeAsync.OnShowBeforeAsync(IViewData data, CancellationToken token)
        {
            return OnShowBeforeAsync(data, token);
        }

        void IViewShowAfter.OnShowAfter(IViewData data)
        {
            OnShowAfter(data);
        }
        UniTask IViewShowAfterAsync.OnShowAfterAsync(IViewData data, CancellationToken token)
        {
            return OnShowAfterAsync(data, token);
        }

        void IViewHide.OnHide()
        {
            OnHide();
        }
        UniTask IViewHideAsync.OnHideAsync(CancellationToken token)
        {
            return OnHideAsync(token);
        }

        void IViewClose.OnViewClose()
        {
            OnViewClose();
        }
        UniTask IViewCloseAsync.OnViewCloseAsync(CancellationToken token)
        {
            return OnViewCloseAsync(token);
        }

        void IViewPoolRetrieve.OnPoolRetrieve()
        {
            OnPoolRetrieve();
        }
        void IViewPoolReturn.OnPoolReturn()
        {
            OnPoolReturn();
        }

        #endregion
    }
}