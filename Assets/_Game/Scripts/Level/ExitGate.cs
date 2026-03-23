using UnityEngine;
using DG.Tweening;
using GraveyardHunter.Core;

namespace GraveyardHunter.Level
{
    public class ExitGate : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Light _gateLight;
        [SerializeField] private Renderer _gateRenderer;
        [SerializeField] private Color _closedColor = new Color(0.29f, 0f, 0f);
        [SerializeField] private Color _openColor = new Color(0f, 1f, 0.27f);
        [SerializeField] private float _interactRadius = 1.5f;

        private bool _isOpen;
        private bool _escaped;
        private Material _material;
        private Transform _player;

        private void Start()
        {
            EventBus.Subscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Subscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);

            if (_gateRenderer != null)
                _material = _gateRenderer.material;

            if (_gateLight != null)
                _gateLight.enabled = false;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AllTreasuresCollectedEvent>(OnAllTreasuresCollected);
            EventBus.Unsubscribe<EscapePhaseStartedEvent>(OnEscapePhaseStarted);
        }

        private void Update()
        {
            if (!_isOpen || _escaped) return;

            // Find player if not cached
            if (_player == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null) _player = playerGO.transform;
                else return;
            }

            // Distance check - works with CharacterController
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < _interactRadius)
            {
                _escaped = true;
                EventBus.Publish(new PlayerEscapedEvent());
                EventBus.Publish(new PlaySFXEvent("Escape"));
            }
        }

        private void OnAllTreasuresCollected(AllTreasuresCollectedEvent evt)
        {
            OpenGate();
        }

        private void OnEscapePhaseStarted(EscapePhaseStartedEvent evt)
        {
            if (!_isOpen) OpenGate();
        }

        private void OpenGate()
        {
            _isOpen = true;

            if (_gateLight != null)
            {
                _gateLight.enabled = true;
                _gateLight.DOIntensity(3f, 0.5f);
            }

            if (_material != null)
            {
                _material.DOColor(_openColor, 0.5f);
                _material.EnableKeyword("_EMISSION");
                _material.DOColor(_openColor * 2f, "_EmissionColor", 0.5f);
            }

            if (_visualRoot != null)
            {
                _visualRoot.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f);
            }

            EventBus.Publish(new PlaySFXEvent("GateOpen"));
        }
    }
}
