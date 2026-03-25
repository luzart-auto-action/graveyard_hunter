using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Sirenix.OdinInspector;
using GraveyardHunter.Core;
using GraveyardHunter.Data;
using GraveyardHunter.Player;

namespace GraveyardHunter.Enemy
{
    public class LightGhost : MonoBehaviour
    {
        [SerializeField] private int _ghostId;
        [ShowInInspector, ReadOnly] private GhostState _currentState = GhostState.Scan;

        private NavMeshAgent _agent;
        private Animator _animator;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIsChasing = Animator.StringToHash("IsChasing");

        [SerializeField] private Transform _visualRoot;
        [SerializeField] private GhostLightCone _lightCone;

        private GameConfig _config;
        private EnemyTypeData _enemyData;
        private Transform _playerTransform;

        private float _scanSpeed;
        private float _chaseSpeed;
        private float _patrolRadius = 15f;
        private float _chaseTimeout = 3f;
        private bool _isEscapePhase;
        private Vector3 _lastKnownPlayerPos;
        private float _wallCheckTimer;
        private bool _isActive;

        private bool _wasPlayerDetected;
        private bool _needsFirstDestination;

        // Search behavior: ghost investigates nearby points after losing sight
        private int _searchPointsChecked;
        private const int MaxSearchPoints = 3;
        private const float SearchRadius = 5f;
        private bool _isSearching;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            EventBus.Subscribe<NoiseTriggeredEvent>(OnNoiseTriggered);
            EventBus.Subscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<NoiseTriggeredEvent>(OnNoiseTriggered);
            EventBus.Unsubscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        /// <summary>Legacy initialize using global GameConfig ghost settings.</summary>
        public void Initialize(GameConfig config, Transform player, int id)
        {
            var defaultData = EnemyTypeData.GetDefault(Core.EnemyType.Ghost);
            Initialize(config, defaultData, player, id);
        }

        /// <summary>Initialize with specific enemy type data for varied behavior.</summary>
        public void Initialize(GameConfig config, EnemyTypeData enemyData, Transform player, int id)
        {
            _config = config;
            _enemyData = enemyData;
            _playerTransform = player;
            _ghostId = id;

            _scanSpeed = enemyData.ScanSpeed;
            _chaseSpeed = enemyData.ChaseSpeed;
            _patrolRadius = enemyData.PatrolRadius;
            _chaseTimeout = enemyData.ChaseTimeout;

            _agent.speed = _scanSpeed;

            _lightCone.SetPlayer(player);
            _lightCone.Initialize(enemyData.LightConeAngle, enemyData.LightRange);

            _isActive = true;

            // Force initial state setup (default is already Scan, so SetState would skip it)
            _currentState = GhostState.Scan;
            _agent.speed = _scanSpeed;

            // Try to set first destination; if agent not on NavMesh yet, retry in Update
            if (_agent.isOnNavMesh)
            {
                PickRandomDestination();
            }
            else
            {
                _needsFirstDestination = true;
            }

            EventBus.Publish(new GhostStateChangedEvent { GhostId = _ghostId, NewState = GhostState.Scan });
        }

        private void Update()
        {
            if (!_isActive) return;
            if (!_agent.isOnNavMesh) return;

            // Retry first destination if it wasn't set during Initialize
            if (_needsFirstDestination)
            {
                _needsFirstDestination = false;
                PickRandomDestination();
            }

            switch (_currentState)
            {
                case GhostState.Scan:
                    ScanBehavior();
                    break;
                case GhostState.Chase:
                    ChaseBehavior();
                    break;
            }

            UpdatePlayerDetection();

            // Update animation parameters
            if (_animator != null)
            {
                _animator.SetFloat(AnimSpeed, _agent.velocity.magnitude);
                _animator.SetBool(AnimIsChasing, _currentState == GhostState.Chase);
            }
        }

        private void UpdatePlayerDetection()
        {
            bool detected = _lightCone.PlayerDetected;

            if (detected && !_wasPlayerDetected)
            {
                EventBus.Publish(new PlayerInLightEvent { InLight = true });
                _lastKnownPlayerPos = _playerTransform.position;
                SetState(GhostState.Chase);
            }
            else if (!detected && _wasPlayerDetected)
            {
                EventBus.Publish(new PlayerInLightEvent { InLight = false });
            }

            _wasPlayerDetected = detected;
        }

        public void SetState(GhostState newState)
        {
            if (_currentState == newState) return;

            GhostState previousState = _currentState;
            _currentState = newState;

            switch (newState)
            {
                case GhostState.Scan:
                    _agent.speed = _isEscapePhase ? _scanSpeed * 1.2f : _scanSpeed;
                    PickRandomDestination();
                    break;

                case GhostState.Chase:
                    _agent.speed = _isEscapePhase ? _chaseSpeed * 1.2f : _chaseSpeed;
                    _isSearching = false;
                    _searchPointsChecked = 0;
                    break;
            }

            EventBus.Publish(new GhostStateChangedEvent { GhostId = _ghostId, NewState = newState });
        }

        private void ScanBehavior()
        {
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                PickRandomDestination();
            }

            if (_lightCone.IsPlayerInCone())
            {
                _lastKnownPlayerPos = _playerTransform.position;
                SetState(GhostState.Chase);
            }
        }

        private void ChaseBehavior()
        {
            // Check if player is hiding in shelter → give up immediately
            if (IsPlayerInShelter())
            {
                AbandonChase();
                return;
            }

            if (_lightCone.IsPlayerInCone())
            {
                // Player visible → pursue directly, reset search state
                _lastKnownPlayerPos = _playerTransform.position;
                _agent.SetDestination(_playerTransform.position);
                _isSearching = false;
                _searchPointsChecked = 0;
            }
            else if (!_isSearching)
            {
                // Lost sight → go to last known position first
                _agent.SetDestination(_lastKnownPlayerPos);

                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    // Reached last known position, start searching nearby
                    _isSearching = true;
                    _searchPointsChecked = 0;
                    PickSearchPoint();
                }
            }
            else
            {
                // Searching nearby points
                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    _searchPointsChecked++;

                    if (_searchPointsChecked >= MaxSearchPoints)
                    {
                        // Exhausted search → give up
                        AbandonChase();
                    }
                    else
                    {
                        PickSearchPoint();
                    }
                }
            }
        }

        private void PickSearchPoint()
        {
            Vector3 searchCenter = _lastKnownPlayerPos;
            Vector3 randomDir = Random.insideUnitSphere * SearchRadius;
            randomDir.y = 0f;
            Vector3 searchTarget = searchCenter + randomDir;

            if (NavMesh.SamplePosition(searchTarget, out NavMeshHit hit, SearchRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        private bool IsPlayerInShelter()
        {
            if (_playerTransform == null) return false;
            var playerCtrl = _playerTransform.GetComponent<PlayerController>();
            return playerCtrl != null && playerCtrl.IsInShelter;
        }

        private void AbandonChase()
        {
            _isSearching = false;
            _searchPointsChecked = 0;
            SetState(GhostState.Scan);
        }

        private bool CheckWallAhead()
        {
            if (_agent.velocity.sqrMagnitude < 0.01f) return false;

            Vector3 direction = _agent.velocity.normalized;
            float checkDistance = 1.0f;

            return Physics.Raycast(transform.position, direction, checkDistance, _lightCone.WallLayer);
        }

        private void PickRandomDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * _patrolRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _patrolRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        private void OnNoiseTriggered(NoiseTriggeredEvent evt)
        {
            if (!_isActive) return;

            Vector3 directionToNoise = (evt.Position - transform.position).normalized;
            directionToNoise.y = 0f;

            if (directionToNoise.sqrMagnitude > 0.01f)
            {
                transform.DORotateQuaternion(Quaternion.LookRotation(directionToNoise), 0.5f);
            }

            if (_currentState == GhostState.Scan)
            {
                _agent.SetDestination(evt.Position);
            }
        }

        private void OnPlayerShelter(PlayerShelterEvent evt)
        {
            // If player enters shelter while we're chasing, abandon chase
            if (evt.IsInShelter && _currentState == GhostState.Chase)
            {
                AbandonChase();
            }
        }

        private void OnEscapePhaseStarted(EscapePhaseStartedEvent evt)
        {
            _isEscapePhase = true;

            float speedMultiplier = 1.2f;
            _agent.speed *= speedMultiplier;
        }

        public void SetVisionThroughWalls(bool enabled)
        {
            if (_lightCone != null)
                _lightCone.IgnoreWalls = enabled;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case Core.GameState.Playing:
                case Core.GameState.EscapePhase:
                    _isActive = true;
                    if (_agent.isOnNavMesh) _agent.isStopped = false;
                    break;

                default:
                    _isActive = false;
                    if (_agent.isOnNavMesh) _agent.isStopped = true;
                    break;
            }
        }
    }
}
