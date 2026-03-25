using UnityEngine;
using GraveyardHunter.Core;

namespace GraveyardHunter.Level
{
    /// <summary>
    /// Hiding spot in the maze. When player enters the trigger zone,
    /// publishes PlayerShelterEvent so ghosts stop chasing.
    /// </summary>
    public class ObstacleShelter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EventBus.Publish(new PlayerShelterEvent { IsInShelter = true });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EventBus.Publish(new PlayerShelterEvent { IsInShelter = false });
            }
        }
    }
}
