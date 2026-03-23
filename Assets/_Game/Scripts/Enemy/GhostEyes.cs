using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;

namespace GraveyardHunter.Enemy
{
    public class GhostEyes : MonoBehaviour
    {
        [SerializeField] private Light _leftEye;
        [SerializeField] private Light _rightEye;

        [SerializeField] private Color _scanColor = Color.yellow;
        [SerializeField] private Color _chaseColor = Color.red;

        [SerializeField] private float _scanIntensity = 1f;
        [SerializeField] private float _chaseIntensity = 3f;

        private Tweener _leftPulseTween;
        private Tweener _rightPulseTween;

        public void SetState(GhostState state)
        {
            KillPulseTweens();

            float duration = 0.3f;

            switch (state)
            {
                case GhostState.Scan:
                    _leftEye.DOColor(_scanColor, duration);
                    _rightEye.DOColor(_scanColor, duration);
                    _leftEye.DOIntensity(_scanIntensity, duration);
                    _rightEye.DOIntensity(_scanIntensity, duration).OnComplete(PulseEyes);
                    break;

                case GhostState.Chase:
                    _leftEye.DOColor(_chaseColor, duration);
                    _rightEye.DOColor(_chaseColor, duration);
                    _leftEye.DOIntensity(_chaseIntensity, duration);
                    _rightEye.DOIntensity(_chaseIntensity, duration);
                    break;
            }
        }

        public void PulseEyes()
        {
            KillPulseTweens();

            float minIntensity = _scanIntensity * 0.6f;
            float maxIntensity = _scanIntensity;
            float pulseDuration = 1.0f;

            _leftPulseTween = _leftEye.DOIntensity(minIntensity, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            _rightPulseTween = _rightEye.DOIntensity(minIntensity, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void KillPulseTweens()
        {
            _leftPulseTween?.Kill();
            _rightPulseTween?.Kill();
            _leftPulseTween = null;
            _rightPulseTween = null;
        }

        private void OnDestroy()
        {
            KillPulseTweens();
            _leftEye.DOKill();
            _rightEye.DOKill();
        }
    }
}
