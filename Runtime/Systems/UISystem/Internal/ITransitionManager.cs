using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Internal
{
    /// <summary>
    ///     过渡动画管理器接口
    /// </summary>
    public interface ITransitionManager
    {
        UniTask PlayInAsync(GameObject root, string transitionName, float seconds);
        UniTask PlayOutAsync(GameObject root, string transitionName, float seconds);
    }
}