using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;
using GraveyardHunter.Player;

namespace GraveyardHunter.Level
{
    public class TreasurePickup : MonoBehaviour
    {
        [SerializeField] private TreasureType _treasureType = TreasureType.Gold;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Light _glowLight;
        [SerializeField] private float _glowIntensity = 1.5f;
        [SerializeField] private float _glowRange = 4f;
        [SerializeField] private float _pickupRadius = 1.5f;

        private bool _collected;
        private Transform _player;

        private void Start()
        {
            // Floating + rotating idle animation
            if (_visualRoot != null)
            {
                _visualRoot.DOLocalMoveY(_visualRoot.localPosition.y + 0.3f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                _visualRoot.DOLocalRotate(new Vector3(0, 360, 0), 3f, RotateMode.LocalAxisAdd)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear);
            }

            if (_glowLight != null)
            {
                _glowLight.intensity = _glowIntensity;
                _glowLight.range = _glowRange;
                _glowLight.DOIntensity(_glowIntensity * 0.5f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void Update()
        {
            if (_collected) return;

            // Find player if not cached
            if (_player == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) _player = playerGO.transform;
                else return;
            }

            // Distance check - works with CharacterController
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < _pickupRadius)
            {
                Collect();
            }
        }

        private void Collect()
        {
            _collected = true;

            var inventory = _player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.CollectTreasure(_treasureType);
            }

            EventBus.Publish(new PlaySFXEvent("TreasureCollect"));
            EventBus.Publish(new SpawnFXEvent("TreasureCollect", transform.position));

            // Collect animation: scale down + fly up
            DOTween.Kill(_visualRoot);
            if (_glowLight != null) DOTween.Kill(_glowLight);

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            seq.Join(transform.DOMoveY(transform.position.y + 2f, 0.3f).SetEase(Ease.OutQuad));
            seq.OnComplete(() => Destroy(gameObject));
        }

        public void SetTreasureType(TreasureType type)
        {
            _treasureType = type;
        }
    }
}
