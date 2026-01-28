using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    public interface IViewHide
    {
        void OnHide();
    }

    public interface IViewHideAsync
    {
        UniTask OnHideAsync(CancellationToken token);
    }
}