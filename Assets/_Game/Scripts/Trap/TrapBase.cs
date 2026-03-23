using UnityEngine;
using GraveyardHunter.Core;

namespace GraveyardHunter.Trap
{
    public abstract class TrapBase : MonoBehaviour
    {
        [SerializeField] protected string _trapName;
        [SerializeField] protected bool _isOneTime = true;
        [SerializeField] protected Transform _fxPoint;
        [SerializeField] protected float _triggerRadius = 1f;

        protected bool _hasTriggered;
        private Transform _player;

        protected abstract void OnTriggerEffect(GameObject player);

        private void Update()
        {
            if (_hasTriggered && _isOneTime) return;

            if (_player == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) _player = playerGO.transform;
                else return;
            }

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < _triggerRadius)
            {
                if (!_hasTriggered || !_isOneTime)
                {
                    OnTriggerEffect(_player.gameObject);
                    if (_isOneTime) _hasTriggered = true;
                }
            }
        }

        protected void SpawnFX(string fxName)
        {
            Vector3 pos = _fxPoint != null ? _fxPoint.position : transform.position;
            EventBus.Publish(new SpawnFXEvent(fxName, pos));
        }

        protected void PlaySound(string sfxName)
        {
            EventBus.Publish(new PlaySFXEvent(sfxName));
        }

        public void ResetTrap()
        {
            _hasTriggered = false;
        }
    }
}
