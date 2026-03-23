using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraveyardHunter.Input
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _joystickBackground;
        [SerializeField] private RectTransform _joystickHandle;
        [SerializeField] private float _handleRange = 50f;

        private Vector2 _inputDirection;
        private Canvas _canvas;
        private Camera _canvasCamera;

        public Vector2 InputDirection => _inputDirection;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();

            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _canvasCamera = _canvas.worldCamera;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                _canvasCamera,
                out Vector2 localPoint
            );

            _joystickBackground.anchoredPosition = localPoint;
            _joystickHandle.anchoredPosition = Vector2.zero;

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _joystickBackground,
                eventData.position,
                _canvasCamera,
                out Vector2 localPoint
            );

            Vector2 direction = localPoint;
            float magnitude = direction.magnitude;

            if (magnitude > _handleRange)
            {
                direction = direction.normalized * _handleRange;
            }

            _joystickHandle.anchoredPosition = direction;

            _inputDirection = (magnitude > 0f)
                ? direction.normalized
                : Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _joystickHandle.anchoredPosition = Vector2.zero;
            _inputDirection = Vector2.zero;
        }
    }
}
