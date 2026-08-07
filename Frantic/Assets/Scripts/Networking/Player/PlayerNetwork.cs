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
            Debug.Log($"[Player] Spawned for client {OwnerClientId}");

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
                _combat.FireWeaponServerRpc(InputManager.Move);
            }
        }

        [ServerRpc]
        public void InteractServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (!IsServer) return;

            var callerId = serverRpcParams.Receive.SenderClientId;
            Debug.Log($"[Player] {callerId} interacting");
        }
    }
}
