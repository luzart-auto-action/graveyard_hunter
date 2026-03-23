using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GraveyardHunter.Animation
{
    public static class DOTweenAnimator
    {
        public static Tween ScaleAppear(Transform t, float duration = 0.3f)
        {
            t.localScale = Vector3.zero;
            return t.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
        }

        public static Tween ScaleDisappear(Transform t, float duration = 0.2f, System.Action onComplete = null)
        {
            return t.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static Sequence CascadeAppear(List<Transform> items, float delayPerItem = 0.03f)
        {
            var seq = DOTween.Sequence();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.localScale = Vector3.zero;
                seq.Insert(i * delayPerItem, item.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            }
            return seq;
        }

        public static Tween PunchScale(Transform t, float punch = 0.1f)
        {
            return t.DOPunchScale(Vector3.one * punch, 0.3f);
        }

        public static Tween PunchRotation(Transform t, float punch = 15f)
        {
            return t.DOPunchRotation(new Vector3(0f, 0f, punch), 0.3f);
        }

        public static Tween ShakePosition(Transform t, float intensity = 0.3f, float duration = 0.3f)
        {
            return t.DOShakePosition(duration, intensity);
        }

        public static Tween FloatLoop(Transform t, float height = 0.3f, float duration = 1f)
        {
            Vector3 startPos = t.position;
            return t.DOMoveY(startPos.y + height, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public static Tween RotateLoop(Transform t, float speed = 90f)
        {
            return t.DORotate(new Vector3(0f, 360f, 0f), 360f / speed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        public static Tween FadeCanvasGroup(CanvasGroup cg, float target, float duration = 0.3f)
        {
            return cg.DOFade(target, duration).SetUpdate(true);
        }

        public static Tween ColorPulse(Renderer r, Color target, float duration = 0.5f)
        {
            return r.material.DOColor(target, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
