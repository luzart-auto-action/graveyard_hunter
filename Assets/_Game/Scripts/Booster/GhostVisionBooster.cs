using System.Collections;
using UnityEngine;
using GraveyardHunter.Core;
using GraveyardHunter.Enemy;

namespace GraveyardHunter.Booster
{
    public class GhostVisionBooster : BoosterBase
    {
        protected override void Activate(GameObject player)
        {
            PlaySound("GhostVision");
            SpawnFX("GhostVisionFX");

            EventBus.Publish(new BoosterActivatedEvent(BoosterType.GhostVision, _duration));

            player.GetComponent<MonoBehaviour>().StartCoroutine(GhostVisionRoutine());
        }

        private IEnumerator GhostVisionRoutine()
        {
            LightGhost[] ghosts = Object.FindObjectsOfType<LightGhost>();
            foreach (LightGhost ghost in ghosts)
            {
                ghost.SetVisionThroughWalls(true);
            }

            yield return new WaitForSeconds(_duration);

            LightGhost[] activeGhosts = Object.FindObjectsOfType<LightGhost>();
            foreach (LightGhost ghost in activeGhosts)
            {
                ghost.SetVisionThroughWalls(false);
            }

            EventBus.Publish(new BoosterExpiredEvent(BoosterType.GhostVision));
        }
    }
}
