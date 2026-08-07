using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class LootNetwork : FranticNetworkObject
    {
        [SerializeField]
        private float _pickupRange = 2f;

        [SerializeField]
        private int _healthRestore = 25;

        [SerializeField]
        private int _ammoRestore = 15;

        [ServerRpc]
        private void PickupServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (!IsServer) return;

            var callerId = serverRpcParams.Receive.SenderClientId;
            var callerClient = NetworkManager.Singleton.ConnectedClients[callerId];
            var callerObject = callerClient?.PlayerObject;
            if (callerObject == null) return;

            var distance = Vector3.Distance(transform.position, callerObject.transform.position);
            if (distance > _pickupRange)
            {
                Debug.LogWarning("[Loot] Player too far to pickup");
                return;
            }

            Debug.Log($"[Loot] Player {callerId} picked up loot");

            var callerHealth = callerObject.GetComponent<PlayerHealthNetwork>();
            if (callerHealth != null && _healthRestore > 0)
            {
                callerHealth.HealServerRpc(_healthRestore);
            }

            OnPickedUpClientRpc();

            if (NetworkObject != null)
            {
                NetworkObject.Despawn();
            }

            GameObject.Destroy(gameObject);
        }

        [ClientRpc]
        private void OnPickedUpClientRpc()
        {
            Debug.Log("[Loot] Pickup effect");
        }
    }
}
