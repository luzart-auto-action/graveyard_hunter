using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;

namespace GraveyardHunter.Trap
{
    public class NoiseTrap : TrapBase
    {
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField] private float _punchDuration = 0.4f;

        protected override void OnTriggerEffect(GameObject player)
        {
            EventBus.Publish(new NoiseTriggeredEvent { Position = transform.position });

            PlaySound("NoiseTrap");
            SpawnFX("NoiseTrapFX");

            AnimateVisual();
        }

        private void AnimateVisual()
        {
            if (_visualTransform == null) return;

            _visualTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 6, 0.5f);
        }
    }
}
