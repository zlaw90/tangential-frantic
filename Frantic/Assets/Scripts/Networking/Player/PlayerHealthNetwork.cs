using System;
using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class PlayerHealthNetwork : FranticNetworkObject
    {
        public event Action<int> OnHealthChanged;
        public event Action OnPlayerDied;

        [SerializeField]
        private int _maxHealth = 100;

        private NetworkVariable<int> _networkHealth = new NetworkVariable<int>();

        public int CurrentHealth => _networkHealth.Value;
        public int MaxHealth => _maxHealth;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _networkHealth.Value = _maxHealth;
            Debug.Log($"[Health] Initialized with {_maxHealth} HP");
        }

        [ServerRpc]
        public void TakeDamageServerRpc(int damage)
        {
            if (!IsServer) return;

            if (damage <= 0) return;

            var currentHealth = _networkHealth.Value;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            _networkHealth.Value = currentHealth;

            Debug.Log($"[Health] Took {damage} damage, health: {currentHealth}/{_maxHealth}");

            if (currentHealth <= 0)
            {
                OnPlayerDiedServerRpc();
            }
            else
            {
                OnHealthChangedClientRpc(currentHealth);
            }
        }

        [ServerRpc]
        private void OnPlayerDiedServerRpc()
        {
            Debug.Log($"[Health] Player died");
            var gameManager = GetComponent<GameManager>();
            gameManager?.DefeatServerRpc();
        }

        [ClientRpc]
        private void OnHealthChangedClientRpc(int health)
        {
            OnHealthChanged?.Invoke(health);
        }

        [ServerRpc]
        public void HealServerRpc(int amount)
        {
            if (!IsServer) return;

            var currentHealth = _networkHealth.Value;
            currentHealth = Mathf.Min(_maxHealth, currentHealth + amount);
            _networkHealth.Value = currentHealth;

            Debug.Log($"[Health] Healed {amount}, health: {currentHealth}/{_maxHealth}");
            OnHealthChangedClientRpc(currentHealth);
        }
    }
}
