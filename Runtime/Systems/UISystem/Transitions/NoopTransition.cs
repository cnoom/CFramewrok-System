using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Transitions
{
    [CreateAssetMenu(fileName = nameof(NoopTransition), menuName = "CFramework/UI/NoopTransition")]
    public class NoopTransition : UITransition
    {
        public override UniTask PlayInAsync(GameObject target, float seconds = 0.2f)
        {
            target.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask PlayOutAsync(GameObject target, float seconds = 0.2f)
        {
            target.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}