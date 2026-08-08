using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace Frantic.Networking
{
    public class PlayerNetwork : FranticNetworkObject
    {
        [SerializeField]
        internal Transform weaponTip;

        private PlayerHealthNetwork _health;
        private PlayerCombatNetwork _combat;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _health = GetComponent<PlayerHealthNetwork>();
            _combat = GetComponent<PlayerCombatNetwork>();

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                InputManager.Initialize(playerInput);
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (InputManager.Fire && _combat != null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    var mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    var direction = (mouseWorldPos - transform.position).normalized;
                    _combat.FireWeaponServerRpc(new Vector2(direction.x, direction.y));
                }
            }

            if (InputManager.Reload && _combat != null)
            {
                if (_combat.CurrentAmmo <= 0)
                {
                    _combat.ReloadServerRpc();
                }
            }
        }

        [ServerRpc]
        public void InteractServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (!IsServer) return;

            var callerId = serverRpcParams.Receive.SenderClientId;
            Debug.Log($"[Player] {callerId} interacting with dungeon entrance");
            GameManager.Instance?.RequestReady(callerId);
        }
    }
}
