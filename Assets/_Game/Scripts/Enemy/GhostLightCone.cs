using UnityEngine;

namespace GraveyardHunter.Enemy
{
    public class GhostLightCone : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        [SerializeField] private float _coneAngle = 80f;
        [SerializeField] private float _coneRange = 10f;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private LayerMask _wallLayer;

        private bool _playerInCone;
        private Transform _player;

        public bool PlayerDetected => _playerInCone;
        public LayerMask WallLayer => _wallLayer;
        public bool IgnoreWalls { get; set; }

        public void Initialize(float angle, float range)
        {
            _coneAngle = angle;
            _coneRange = range;

            if (_spotLight != null)
            {
                _spotLight.spotAngle = _coneAngle;
                _spotLight.range = _coneRange;
            }
        }

        public void SetPlayer(Transform player)
        {
            _player = player;
        }

        private void Update()
        {
            UpdateDetection();
        }

        public void UpdateDetection()
        {
            bool wasInCone = _playerInCone;
            _playerInCone = IsPlayerInCone();
        }

        public bool IsPlayerInCone()
        {
            if (_player == null) return false;

            Vector3 directionToPlayer = _player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > _coneRange)
            {
                return false;
            }

            float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);

            if (angle > _coneAngle * 0.5f)
            {
                return false;
            }

            if (!IgnoreWalls && Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, _wallLayer))
            {
                if (((1 << hit.collider.gameObject.layer) & _playerLayer) == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
