using UnityEngine;
using UnityEngine.InputSystem;

namespace Frantic.Networking
{
    public class PlayerNetwork : FranticNetworkObject
    {
        private PlayerHealthNetwork _health;
        private PlayerCombatNetwork _combat;


        private Camera _cachedCamera;

        private void Awake()
        {
            _health = GetComponent<PlayerHealthNetwork>();
            _combat = GetComponent<PlayerCombatNetwork>();

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                InputManager.Initialize(playerInput);
            }
            _cachedCamera = Camera.main;
        }

        private void Update()
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
            }

            if (InputManager.Fire && _combat != null && _cachedCamera != null)
            {
                var mouseWorldPos = _cachedCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var direction = (mouseWorldPos - transform.position).normalized;
                _combat.FireWeapon(new Vector2(direction.x, direction.y));
            }

            if (InputManager.Reload && _combat != null)
            {
                if (_combat.CurrentAmmo <= 0)
                {
                    _combat.Reload();
                }
            }
        }

        public void Interact()
        {
            Debug.Log($"[Player] Interact called! Position: {transform.position}, State: {GameManager.Instance?.CurrentState}");
            GameManager.Instance?.RequestReady();
        }
    }
}
