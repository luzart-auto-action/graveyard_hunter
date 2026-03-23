using System.Collections;
using UnityEngine;
using DG.Tweening;
namespace GraveyardHunter.Trap
{
    public class LightBurstTrap : TrapBase
    {
        [SerializeField] private Light _burstLight;
        [SerializeField] private float _maxIntensity = 5f;
        [SerializeField] private float _fadeInDuration = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _lightBurstDuration = 2f;

        private void Start()
        {
            if (_burstLight != null)
            {
                _burstLight.intensity = 0f;
                _burstLight.enabled = false;
            }
        }

        protected override void OnTriggerEffect(GameObject player)
        {
            PlaySound("LightBurst");
            SpawnFX("LightBurstFX");

            StartCoroutine(LightBurstRoutine());
        }

        private IEnumerator LightBurstRoutine()
        {
            if (_burstLight == null) yield break;

            float duration = _lightBurstDuration;

            _burstLight.enabled = true;
            _burstLight.intensity = 0f;

            _burstLight.DOIntensity(_maxIntensity, _fadeInDuration).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(duration - _fadeOutDuration);

            _burstLight.DOIntensity(0f, _fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _burstLight.enabled = false;
                });
        }
    }
}
