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

        private void Update()
        {
            if (!IsServer) return;

            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                var playerObject = client.PlayerObject;
                if (playerObject == null) continue;

                var distance = Vector3.Distance(transform.position, playerObject.transform.position);
                if (distance < _pickupRange)
                {
                    PickupForPlayer(client.ClientId);
                    return;
                }
            }
        }

        private void PickupForPlayer(ulong clientId)
        {
            var callerClient = NetworkManager.Singleton.ConnectedClients[clientId];
            var callerObject = callerClient?.PlayerObject;
            if (callerObject == null) return;

            Debug.Log($"[Loot] Player {clientId} picked up loot");

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
