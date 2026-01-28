using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    public interface IViewDestroy
    {
        void OnViewDestroy();
    }

    public interface IViewDestroyAsync
    {
        UniTask OnViewDestroyAsync(CancellationToken token);
    }
}