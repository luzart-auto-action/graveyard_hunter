using UnityEngine;
using GraveyardHunter.Player;
using Sirenix.OdinInspector;

namespace GraveyardHunter.Enemy
{
    public class GhostLightCone : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        [SerializeField] private float _coneAngle = 80f;
        [SerializeField] private float _coneRange = 10f;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private LayerMask _wallLayer;

        [Title("Detection Tuning")]
        [Tooltip("Always detect player within this distance regardless of cone angle")]
        [SerializeField] private float _proximityRange = 2f;

        [Tooltip("Extra angle added when player is ALREADY detected (prevents flicker at cone edge)")]
        [SerializeField] private float _hysteresisAngle = 10f;

        [Tooltip("Grace period: keep detection for this many seconds after losing sight")]
        [SerializeField] private float _lostSightGrace = 0.3f;

        private bool _playerInCone;
        private Transform _player;
        private PlayerController _playerCtrl;
        private float _lostSightTimer;

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
            _playerCtrl = player != null ? player.GetComponent<PlayerController>() : null;
        }

        private void Update()
        {
            UpdateDetection();
        }

        public void UpdateDetection()
        {
            bool rawDetected = RawConeCheck();

            if (rawDetected)
            {
                _playerInCone = true;
                _lostSightTimer = 0f;
            }
            else if (_playerInCone)
            {
                // Grace period: don't immediately lose detection
                _lostSightTimer += Time.deltaTime;
                if (_lostSightTimer >= _lostSightGrace)
                {
                    _playerInCone = false;
                }
            }
        }

        public bool IsPlayerInCone()
        {
            return _playerInCone;
        }

        /// <summary>
        /// Core detection logic with proximity and hysteresis.
        /// </summary>
        private bool RawConeCheck()
        {
            if (_player == null) return false;

            // Player hidden in shelter or invisible → not detectable
            if (_playerCtrl != null && (_playerCtrl.IsInShelter || _playerCtrl.IsInvisible))
                return false;

            Vector3 directionToPlayer = _player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            // --- Proximity detection: always detect if very close ---
            if (distanceToPlayer <= _proximityRange)
            {
                // Still check walls even at close range
                if (!IsBlockedByWall(directionToPlayer, distanceToPlayer))
                    return true;
            }

            // --- Cone range check ---
            if (distanceToPlayer > _coneRange)
                return false;

            // --- Cone angle check with hysteresis ---
            float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
            float effectiveHalfAngle = _coneAngle * 0.5f;

            // If already detected, use wider angle to prevent flicker at edges
            if (_playerInCone)
                effectiveHalfAngle += _hysteresisAngle;

            if (angle > effectiveHalfAngle)
                return false;

            // --- Wall check ---
            if (IsBlockedByWall(directionToPlayer, distanceToPlayer))
                return false;

            return true;
        }

        private bool IsBlockedByWall(Vector3 directionToPlayer, float distance)
        {
            if (IgnoreWalls) return false;

            if (Physics.Raycast(transform.position, directionToPlayer.normalized,
                    out RaycastHit hit, distance, _wallLayer))
            {
                // Hit something on wall layer → check it's not the player
                if (((1 << hit.collider.gameObject.layer) & _playerLayer) == 0)
                    return true;
            }

            return false;
        }
    }
}
