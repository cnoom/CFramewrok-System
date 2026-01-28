using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    public interface IViewShowBefore
    {
        void OnShowBefore(IViewData data);
    }

    public interface IViewShowBeforeAsync
    {
        UniTask OnShowBeforeAsync(IViewData data, CancellationToken token);
    }
}