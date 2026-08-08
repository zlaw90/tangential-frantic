using UnityEngine;

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
            PlayerNetwork closestPlayer = null;
            float closestDistance = _pickupRange;

            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

            foreach (var player in players)
            {
                var distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                PickupForPlayer(closestPlayer);
            }
        }

        private void PickupForPlayer(PlayerNetwork player)
        {
            Debug.Log("[Loot] Picked up by player");

            var health = player.GetComponent<PlayerHealthNetwork>();
            if (health != null && _healthRestore > 0)
            {
                health.Heal(_healthRestore);
            }

            var combat = player.GetComponent<PlayerCombatNetwork>();
            if (combat != null && _ammoRestore > 0)
            {
                combat.AddAmmo(_ammoRestore);
            }

            Destroy(gameObject);
        }
    }
}
