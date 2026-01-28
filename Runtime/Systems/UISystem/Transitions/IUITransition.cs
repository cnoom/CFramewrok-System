using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Transitions
{
    public interface IUITransition
    {
        UniTask PlayInAsync(GameObject target, float seconds = 0.2f);
        UniTask PlayOutAsync(GameObject target, float seconds = 0.2f);
    }
}