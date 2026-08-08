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

            PlayerNetwork closestPlayer = null;
            float closestDistance = _pickupRange;

            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                var playerObject = client.PlayerObject;
                if (playerObject == null) continue;

                var distance = Vector3.Distance(transform.position, playerObject.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = playerObject.GetComponent<PlayerNetwork>();
                }
            }

            if (closestPlayer != null)
            {
                PickupForPlayer(closestPlayer);
            }
        }

        private void PickupForPlayer(PlayerNetwork player)
        {
            Debug.Log($"[Loot] Picked up by client {player.OwnerClientId}");

            var health = player.GetComponent<PlayerHealthNetwork>();
            if (health != null && _healthRestore > 0)
            {
                health.HealServerRpc(_healthRestore);
            }

            var combat = player.GetComponent<PlayerCombatNetwork>();
            if (combat != null && _ammoRestore > 0)
            {
                combat.AddAmmoServerRpc(_ammoRestore);
            }

            OnPickedUpClientRpc();

            if (NetworkObject != null)
            {
                NetworkObject.Despawn();
            }

            GameObject.Destroy(gameObject);
        }

        [ClientRpc]
        private void OnPickedUpClientRpc() { }
    }
}
