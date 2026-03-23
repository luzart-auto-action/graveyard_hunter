using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Player;

namespace GraveyardHunter.Trap
{
    public class SpikeTrap : TrapBase
    {
        [SerializeField] private Transform _spikesTransform;
        [SerializeField] private float _spikeRiseHeight = 1f;
        [SerializeField] private float _spikeRiseDuration = 0.2f;
        [SerializeField] private float _spikeDownDuration = 0.5f;

        protected override void OnTriggerEffect(GameObject player)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            PlaySound("SpikeTrap");
            SpawnFX("SpikeTrapFX");

            AnimateSpikes();
        }

        private void AnimateSpikes()
        {
            if (_spikesTransform == null) return;

            Vector3 originalPos = _spikesTransform.localPosition;
            Vector3 raisedPos = originalPos + Vector3.up * _spikeRiseHeight;

            _spikesTransform
                .DOLocalMove(raisedPos, _spikeRiseDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _spikesTransform
                        .DOLocalMove(originalPos, _spikeDownDuration)
                        .SetEase(Ease.InQuad);
                });
        }
    }
}
