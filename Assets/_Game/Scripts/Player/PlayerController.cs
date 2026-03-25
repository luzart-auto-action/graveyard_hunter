using DG.Tweening;
using GraveyardHunter.Core;
using GraveyardHunter.Data;
using GraveyardHunter.Input;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private Transform _visualRoot;

        private CharacterController _characterController;
        private Animator _animator;
        private GameConfig _config;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimWin = Animator.StringToHash("Win");
        private static readonly int AnimDie = Animator.StringToHash("Die");

        private bool _isSlowed;
        private bool _hasSpeedBoost;
        private float _speedBoostMultiplier = 1f;
        private float _slowRecoveryTimer;

        /// <summary>
        /// Reference counter: tracks how many ghosts currently see the player.
        /// Fixes multi-ghost bug where one ghost losing sight would cancel
        /// the slow effect from all other ghosts still shining on the player.
        /// </summary>
        [ShowInInspector, ReadOnly] private int _inLightCount;
        private bool IsInLight => _inLightCount > 0;

        private bool _movementEnabled;

        public bool IsInvisible { get; set; }
        public bool IsInShelter { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();

            EventBus.Subscribe<PlayerInLightEvent>(OnPlayerInLight);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerInLightEvent>(OnPlayerInLight);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<PlayerShelterEvent>(OnPlayerShelter);
        }

        public void Initialize(GameConfig config)
        {
            _config = config;
            _moveSpeed = config.PlayerMoveSpeed;
            _isSlowed = false;
            _hasSpeedBoost = false;
            _speedBoostMultiplier = 1f;
            _slowRecoveryTimer = 0f;
            _inLightCount = 0;
            _movementEnabled = true;
            IsInvisible = false;
            IsInShelter = false;
        }

        private void Update()
        {
            if (!_movementEnabled)
                return;

            HandleMovement();
            HandleSlowRecovery();
        }

        private void HandleMovement()
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            Vector2 moveInput = inputManager.GetMoveInput();

            // Update animation
            if (_animator != null)
                _animator.SetFloat(AnimSpeed, moveInput.sqrMagnitude > 0.01f ? 1f : 0f);

            if (moveInput.sqrMagnitude < 0.01f)
                return;

            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            float currentSpeed = _moveSpeed;

            if (_isSlowed)
            {
                currentSpeed *= (1f - _config.LightSlowPercent);
            }

            if (_hasSpeedBoost)
            {
                currentSpeed *= _speedBoostMultiplier;
            }

            Vector3 velocity = moveDirection * currentSpeed * Time.deltaTime;
            _characterController.Move(velocity);

            RotateTowardsDirection(moveDirection);
        }

        private void RotateTowardsDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            Transform rotateTarget = _visualRoot != null ? _visualRoot : transform;
            rotateTarget.rotation = Quaternion.Slerp(rotateTarget.rotation, targetRotation, 10f * Time.deltaTime);
        }

        private void HandleSlowRecovery()
        {
            // Only start recovery when NOT in any ghost's light
            if (!_isSlowed || IsInLight)
                return;

            _slowRecoveryTimer += Time.deltaTime;

            if (_slowRecoveryTimer >= _config.SlowRecoveryDelay)
            {
                _isSlowed = false;
                _slowRecoveryTimer = 0f;
            }
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            _hasSpeedBoost = true;
            _speedBoostMultiplier = multiplier;

            DOVirtual.DelayedCall(duration, RemoveSpeedBoost).SetTarget(this);
        }

        public void RemoveSpeedBoost()
        {
            _hasSpeedBoost = false;
            _speedBoostMultiplier = 1f;
        }

        private void OnPlayerInLight(PlayerInLightEvent evt)
        {
            if (evt.InLight)
            {
                _inLightCount++;
                _isSlowed = true;
                _slowRecoveryTimer = 0f;
            }
            else
            {
                _inLightCount = Mathf.Max(0, _inLightCount - 1);

                // Only start slow recovery when ALL ghosts lose sight
                if (!IsInLight)
                {
                    _slowRecoveryTimer = 0f;
                }
            }
        }

        private void OnPlayerShelter(PlayerShelterEvent evt)
        {
            IsInShelter = evt.IsInShelter;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _movementEnabled = evt.NewState == Core.GameState.Playing || evt.NewState == Core.GameState.EscapePhase;

            if (evt.NewState == Core.GameState.Win && _animator != null)
                _animator.SetTrigger(AnimWin);
            else if (evt.NewState == Core.GameState.Fail && _animator != null)
                _animator.SetTrigger(AnimDie);
        }
    }
}
