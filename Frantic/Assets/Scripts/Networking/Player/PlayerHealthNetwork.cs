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
        }

        [ServerRpc]
        public void TakeDamageServerRpc(int damage)
        {
            if (!IsServer) return;
            if (damage <= 0) return;

            var currentHealth = Mathf.Max(0, _networkHealth.Value - damage);
            _networkHealth.Value = currentHealth;

            if (currentHealth <= 0)
            {
                GameManager.Instance?.Defeat();
            }
            else
            {
                OnHealthChangedClientRpc(currentHealth);
            }
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

            var currentHealth = Mathf.Min(_maxHealth, _networkHealth.Value + amount);
            _networkHealth.Value = currentHealth;
            OnHealthChangedClientRpc(currentHealth);
        }
    }
}
