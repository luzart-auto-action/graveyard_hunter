using UnityEngine;
using Sirenix.OdinInspector;
using GraveyardHunter.Core;

namespace GraveyardHunter.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick _joystick;

        [ShowInInspector, ReadOnly] private Vector2 _moveInput;

        private bool _useKeyboard;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InputManager>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (_joystick == null || _joystick.InputDirection == Vector2.zero)
            {
                float horizontal = 0f;
                float vertical = 0f;

                if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
                    vertical += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
                    vertical -= 1f;
                if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
                    horizontal += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                    horizontal -= 1f;

                Vector2 keyboardInput = new Vector2(horizontal, vertical);
                _useKeyboard = keyboardInput.sqrMagnitude > 0f;

                if (_useKeyboard)
                {
                    _moveInput = keyboardInput.normalized;
                    return;
                }
            }
#endif

            if (_joystick != null)
            {
                _moveInput = _joystick.InputDirection;
            }
            else
            {
                _moveInput = Vector2.zero;
            }
        }

        public Vector2 GetMoveInput()
        {
            return _moveInput.normalized;
        }

        public void SetJoystick(VirtualJoystick joystick)
        {
            _joystick = joystick;
        }
    }
}
