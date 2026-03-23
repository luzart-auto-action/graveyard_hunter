using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;

namespace GraveyardHunter.Booster
{
    public abstract class BoosterBase : MonoBehaviour
    {
        [SerializeField] protected BoosterType _boosterType;
        [SerializeField] protected float _duration = 5f;
        [SerializeField] protected Transform _visualRoot;
        [SerializeField] protected Transform _fxPoint;
        [SerializeField] private float _pickupRadius = 1.5f;

        [Header("Idle Animation")]
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private float _floatHeight = 0.3f;
        [SerializeField] private float _floatDuration = 1f;

        private bool _pickedUp;
        private Transform _player;

        private void Start()
        {
            StartIdleAnimation();
        }

        private void StartIdleAnimation()
        {
            if (_visualRoot == null) return;

            _visualRoot.DORotate(new Vector3(0f, 360f, 0f), 360f / _rotateSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            _visualRoot.DOLocalMoveY(
                _visualRoot.localPosition.y + _floatHeight,
                _floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void Update()
        {
            if (_pickedUp) return;

            if (_player == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) _player = playerGO.transform;
                else return;
            }

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < _pickupRadius)
            {
                PickUp();
            }
        }

        private void PickUp()
        {
            _pickedUp = true;

            EventBus.Publish(new BoosterPickedUpEvent(_boosterType));
            Activate(_player.gameObject);

            if (_visualRoot != null)
                _visualRoot.DOKill();

            Destroy(gameObject);
        }

        protected abstract void Activate(GameObject player);

        protected void SpawnFX(string fxName)
        {
            Vector3 pos = _fxPoint != null ? _fxPoint.position : transform.position;
            EventBus.Publish(new SpawnFXEvent(fxName, pos));
        }

        protected void PlaySound(string sfxName)
        {
            EventBus.Publish(new PlaySFXEvent(sfxName));
        }
    }
}
