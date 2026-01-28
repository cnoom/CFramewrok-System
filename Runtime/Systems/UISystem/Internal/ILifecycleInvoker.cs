using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     生命周期调用器接口
    /// </summary>
    public interface ILifecycleInvoker
    {
        UniTask CallCreateAsync(IUIView controller, string viewId, CancellationToken token);
        UniTask CallDestroyAsync(IUIView controller, CancellationToken token);
        UniTask CallShowBeforeAsync(IViewInstance inst, IViewData data, CancellationToken token);
        UniTask CallShowAfterAsync(IViewInstance inst, IViewData data, CancellationToken token);
        UniTask CallHideAsync(IViewInstance inst, CancellationToken token);
        UniTask CallCloseAsync(IViewInstance inst, CancellationToken token);
    }
}