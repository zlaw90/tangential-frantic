using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Frantic.Networking
{
    public class PlayerHealthNetwork : FranticNetworkObject
    {
        public event Action<int> OnHealthChanged;
        public event Action OnPlayerDied;

        [SerializeField]
        private int _maxHealth = 100;

        private int _currentHealth;
        private string _currentScene;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _currentScene = SceneManager.GetActiveScene().name;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            if (_currentScene == "Hub")
            {
                Debug.LogWarning("[PlayerHealth] Taking damage in Hub - ignoring");
                return;
            }

            var oldHealth = _currentHealth;
            var newHealth = Mathf.Max(0, _currentHealth - damage);
            _currentHealth = newHealth;

            Debug.Log($"[PlayerHealth] Took {damage} damage: {oldHealth} -> {_currentHealth}");

            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                Debug.LogError("[PlayerHealth] Player died!");
                GameManager.Instance?.Defeat();
                OnPlayerDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth);
        }
    }
}
