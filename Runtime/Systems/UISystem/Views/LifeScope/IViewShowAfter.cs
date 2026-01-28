using System.Threading;
using Cysharp.Threading.Tasks;

namespace CFramework.Systems.UISystem.LifeScope
{
    public interface IViewShowAfter
    {
        void OnShowAfter(IViewData data);
    }

    public interface IViewShowAfterAsync
    {
        UniTask OnShowAfterAsync(IViewData data, CancellationToken token);
    }
}