using System.Collections;
using UnityEngine;
using GraveyardHunter.Core;

namespace GraveyardHunter.Booster
{
    public class SmokeBombBooster : BoosterBase
    {
        [SerializeField] private float _smokeRadius = 5f;
        [SerializeField] private string _smokeFXName = "SmokeBombFX";

        protected override void Activate(GameObject player)
        {
            PlaySound("SmokeBomb");
            SpawnFX(_smokeFXName);

            // Start coroutine on player since this booster gets destroyed
            var coroutineHost = player.GetComponent<MonoBehaviour>();
            if (coroutineHost != null)
                coroutineHost.StartCoroutine(SmokeRoutine(player.transform.position));
        }

        private IEnumerator SmokeRoutine(Vector3 position)
        {
            GameObject smokeArea = new GameObject("SmokeArea");
            smokeArea.transform.position = position;
            smokeArea.layer = LayerMask.NameToLayer("SmokeZone");

            SphereCollider sphereCollider = smokeArea.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = _smokeRadius;

            smokeArea.AddComponent<SmokeBlocker>();

            EventBus.Publish(new BoosterActivatedEvent(BoosterType.SmokeBomb, _duration));

            yield return new WaitForSeconds(_duration);

            if (smokeArea != null)
            {
                Object.Destroy(smokeArea);
            }

            EventBus.Publish(new BoosterExpiredEvent(BoosterType.SmokeBomb));
        }
    }

    public class SmokeBlocker : MonoBehaviour
    {
        // Ghosts check for this component in their light detection logic
        // to determine if their vision is blocked by smoke.
    }
}
