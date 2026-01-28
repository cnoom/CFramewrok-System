using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     视图池接口
    /// </summary>
    public interface IViewPool
    {
        IViewInstance Get(string key);
        UniTask ReturnAsync(IViewInstance instance, int? maxPoolSize = null);
        void Clear(string key = null);
    }
}