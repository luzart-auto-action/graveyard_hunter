using System.Collections;
using UnityEngine;
using GraveyardHunter.Core;
using GraveyardHunter.Player;

namespace GraveyardHunter.Booster
{
    public class SpeedBootsBooster : BoosterBase
    {
        [SerializeField] private float _speedMultiplier = 1.3f;

        protected override void Activate(GameObject player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController == null) return;

            playerController.ApplySpeedBoost(_speedMultiplier, _duration);

            PlaySound("SpeedBoots");
            SpawnFX("SpeedBootsFX");

            EventBus.Publish(new BoosterActivatedEvent(BoosterType.SpeedBoots, _duration));

            playerController.StartCoroutine(SpeedBoostRoutine());
        }

        private IEnumerator SpeedBoostRoutine()
        {
            yield return new WaitForSeconds(_duration);

            EventBus.Publish(new BoosterExpiredEvent(BoosterType.SpeedBoots));
        }
    }
}
