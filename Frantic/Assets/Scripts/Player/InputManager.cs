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

        public static void Initialize(PlayerInput playerInput)
        {
            _playerInput = playerInput;
        }
    }
}
