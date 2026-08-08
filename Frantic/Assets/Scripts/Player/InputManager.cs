using UnityEngine;
using UnityEngine.InputSystem;

namespace Frantic.Networking
{
    public static class InputManager
    {
        private static PlayerInput _playerInput;

        public static Vector2 Move => _playerInput != null ? _playerInput.actions["Move"].ReadValue<Vector2>() : Vector2.zero;
        public static bool Fire => _playerInput != null && _playerInput.actions["Attack"].WasPressedThisFrame();
        public static bool Interact => _playerInput != null && _playerInput.actions["Interact"].WasPressedThisFrame();
        public static bool Reload => _playerInput != null && _playerInput.actions["Reload"].WasPressedThisFrame();

        public static void Initialize(PlayerInput playerInput)
        {
            _playerInput = playerInput;
        }

        public static Vector2 GetMouseWorldDirection(Camera mainCamera)
        {
            var mouseScreenPos = Mouse.current.position.ReadValue();
            var mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
            return (mouseWorldPos - Vector3.zero).normalized;
        }
    }
}
