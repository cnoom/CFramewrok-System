using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    /// <summary>
    ///     视图创建生命周期接口（同步版本）。
    /// </summary>
    public interface IViewCreate
    {
        /// <summary>
        ///     视图创建时调用。
        /// </summary>
        /// <param name="viewId">视图实例唯一ID，由系统分配。</param>
        void OnViewCreate(string viewId);
    }

    /// <summary>
    ///     视图创建生命周期接口（异步版本）。
    /// </summary>
    public interface IViewCreateAsync
    {
        /// <summary>
        ///     视图创建时调用（异步版本）。
        /// </summary>
        /// <param name="viewId">视图实例唯一ID，由系统分配。</param>
        /// <param name="token">取消令牌。</param>
        UniTask OnViewCreateAsync(string viewId, CancellationToken token);
    }
}