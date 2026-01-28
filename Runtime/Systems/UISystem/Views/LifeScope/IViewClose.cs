using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    /// <summary>
    ///     视图关闭时的生命周期接口，用于在视图关闭时重置状态。
    /// </summary>
    /// <remarks>
    ///     与OnViewDestroy的区别：
    ///     - OnViewDestroy在视图被真正销毁时调用（包括从对象池中移除时）
    ///     - OnViewClose在视图关闭时调用（包括放入对象池复用前）
    ///     适合用于重置UI状态、取消事件订阅、清理临时数据等
    /// </remarks>
    public interface IViewClose
    {
        /// <summary>
        ///     视图关闭时调用（同步版本）。
        /// </summary>
        void OnViewClose();
    }

    /// <summary>
    ///     视图关闭时的生命周期接口（异步版本）。
    /// </summary>
    public interface IViewCloseAsync
    {
        /// <summary>
        ///     视图关闭时调用（异步版本）。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        UniTask OnViewCloseAsync(CancellationToken token);
    }
}