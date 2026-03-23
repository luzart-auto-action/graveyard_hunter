using UnityEngine;

namespace GraveyardHunter.FX
{
    public class FXSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _fxPointName;

        public string PointName => _fxPointName;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
