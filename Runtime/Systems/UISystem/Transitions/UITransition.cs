using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Transitions
{
    public abstract class UITransition : ScriptableObject, IUITransition
    {
        public abstract UniTask PlayInAsync(GameObject target, float seconds = 0.2f);
        public abstract UniTask PlayOutAsync(GameObject target, float seconds = 0.2f);
    }
}