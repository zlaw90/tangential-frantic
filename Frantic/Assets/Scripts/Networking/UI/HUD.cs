using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Frantic.Networking
{
    public class HUD : MonoBehaviour
    {
        [SerializeField]
        private Image _healthBar;

        [SerializeField]
        private TMP_Text _ammoText;

        [SerializeField]
        private TMP_Text _readyText;

        private PlayerHealthNetwork _health;
        private PlayerCombatNetwork _combat;
        private ReadyManager _readyManager;

        private void Awake()
        {
            _health = FindObjectOfType<PlayerHealthNetwork>();
            _combat = FindObjectOfType<PlayerCombatNetwork>();
            _readyManager = FindObjectOfType<ReadyManager>();
        }

        private void Update()
        {
            if (_health != null)
            {
                var healthPercent = (float)_health.CurrentHealth / _health.MaxHealth;
                _healthBar.fillAmount = healthPercent;
            }

            if (_combat != null)
            {
                _ammoText.text = $"Ammo: {_combat.CurrentAmmo}";
            }

            if (_readyManager != null)
            {
                var readyStates = _readyManager.GetReadyStates();
                var totalPlayers = FranticNetworkManager.Instance?.ConnectedClientsList.Count ?? 0;
                var readyCount = readyStates.Count(v => v);
                _readyText.text = $"Ready: {readyCount}/{totalPlayers}";
            }
        }
    }
}
