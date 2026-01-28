using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CFramework.Systems.UISystem.Transitions
{
    [CreateAssetMenu(fileName = nameof(FadeTransition), menuName = "CFramework/UI/FadeTransition")]
    public sealed class FadeTransition : UITransition
    {
        public AnimationCurve outCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve inCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override async UniTask PlayOutAsync(GameObject target, float seconds = 0.2f)
        {
            CanvasGroup cg = EnsureCanvasGroup(target);
            await TweenAlphaAsync(cg, 0f, seconds, outCurve);
            target.SetActive(false);
        }

        private CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if(!cg) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        private async UniTask TweenAlphaAsync(CanvasGroup cg, float targetAlpha, float seconds, AnimationCurve curve)
        {
            seconds = Mathf.Max(0f, seconds);
            if(seconds <= 0f)
            {
                cg.alpha = targetAlpha;
                return;
            }

            float start = cg.alpha;
            var t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / seconds);
                cg.alpha = Mathf.Lerp(start, targetAlpha, curve.Evaluate(k));
                await UniTask.Yield();
            }

            cg.alpha = targetAlpha;
        }

        public override async UniTask PlayInAsync(GameObject target, float seconds = 0.2f)
        {
            CanvasGroup cg = EnsureCanvasGroup(target);
            cg.alpha = 0f;
            target.SetActive(true);
            await TweenAlphaAsync(cg, 1f, seconds, inCurve);
        }
    }
}