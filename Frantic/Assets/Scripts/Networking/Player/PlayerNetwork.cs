using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class PlayerNetwork : FranticNetworkObject
    {
        [SerializeField]
        private float _moveSpeed = 5f;

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
        }

        private void Update()
        {
            if (!IsOwner) return;

            var input = Vector2.zero;
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (Input.GetButtonDown("Fire1") && _combat != null)
            {
                _combat.FireWeaponServerRpc(input);
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
