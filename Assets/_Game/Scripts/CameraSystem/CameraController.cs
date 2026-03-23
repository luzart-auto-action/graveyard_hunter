using DG.Tweening;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 15f, -8f);
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private float _lookAtOffset = 2f;

        [FoldoutGroup("Shake")]
        [SerializeField] private float _shakeIntensity = 0.3f;
        [FoldoutGroup("Shake")]
        [SerializeField] private float _shakeDuration = 0.3f;

        private Transform _target;
        private Vector3 _defaultOffset;

        private void Start()
        {
            _defaultOffset = _offset;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _followSpeed * Time.deltaTime);

            Vector3 lookTarget = _target.position + Vector3.up * _lookAtOffset;
            transform.LookAt(lookTarget);
        }

        public void Shake()
        {
            transform.DOShakePosition(_shakeDuration, _shakeIntensity);
        }

        public void ZoomTo(float newY, float duration)
        {
            _offset = new Vector3(_offset.x, newY, _offset.z);
            transform.DOMove(_target.position + _offset, duration);
        }

        public void ResetZoom()
        {
            _offset = _defaultOffset;
        }
    }
}
