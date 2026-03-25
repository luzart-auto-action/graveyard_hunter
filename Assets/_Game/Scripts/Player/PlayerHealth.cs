using GraveyardHunter.Core;
using GraveyardHunter.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] private int _currentHP;

        private int _maxHP;
        private bool _isInLight;
        private float _damageTimer;
        private float _damagePerSecond;
        private bool _isDead;
        private bool _isInvulnerable;

        public int CurrentHP => _currentHP;
        public bool IsDead => _isDead;

        private void Awake()
        {
            EventBus.Subscribe<PlayerInLightEvent>(OnPlayerInLight);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerInLightEvent>(OnPlayerInLight);
        }

        public void Initialize(GameConfig config)
        {
            _maxHP = config.PlayerMaxHP;
            _currentHP = _maxHP;
            _damagePerSecond = config.LightDamagePerSecond;
            _isDead = false;
            _isInvulnerable = false;
            _isInLight = false;
            _damageTimer = 0f;

            // Publish initial HP so UI updates immediately
            EventBus.Publish(new PlayerHPChangedEvent
            {
                CurrentHP = _currentHP,
                MaxHP = _maxHP
            });
        }

        private void Update()
        {
            if (_isDead)
                return;

            if (_isInLight && !_isInvulnerable)
            {
                _damageTimer += Time.deltaTime;

                if (_damageTimer >= 1f)
                {
                    _damageTimer -= 1f;
                    TakeDamage(1);
                }
            }
            else
            {
                _damageTimer = 0f;
            }
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || _isInvulnerable)
                return;

            _currentHP = Mathf.Max(0, _currentHP - amount);

            EventBus.Publish(new PlayerHPChangedEvent
            {
                CurrentHP = _currentHP,
                MaxHP = _maxHP
            });

            if (_currentHP <= 0)
            {
                _isDead = true;
                EventBus.Publish(new PlayerDiedEvent());
            }
        }

        public void Heal(int amount)
        {
            if (_isDead)
                return;

            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);

            EventBus.Publish(new PlayerHPChangedEvent
            {
                CurrentHP = _currentHP,
                MaxHP = _maxHP
            });
        }

        public void ResetHealth()
        {
            _currentHP = _maxHP;
            _isDead = false;
            _damageTimer = 0f;

            EventBus.Publish(new PlayerHPChangedEvent
            {
                CurrentHP = _currentHP,
                MaxHP = _maxHP
            });
        }

        public void SetInvulnerable(bool value)
        {
            _isInvulnerable = value;

            if (_isInvulnerable)
            {
                _damageTimer = 0f;
            }
        }

        private void OnPlayerInLight(PlayerInLightEvent evt)
        {
            _isInLight = evt.InLight;

            if (!_isInLight)
            {
                _damageTimer = 0f;
            }
        }
    }
}
