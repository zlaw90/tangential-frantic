using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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

        public void SetUI(Image healthBar, TMP_Text ammoText, TMP_Text readyText)
        {
            _healthBar = healthBar;
            _ammoText = ammoText;
            _readyText = readyText;
        }

        private PlayerHealthNetwork _health;
        private PlayerCombatNetwork _combat;
        private ReadyManager _readyManager;

        private void Awake()
        {
            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    _health = player.GetComponent<PlayerHealthNetwork>();
                    _combat = player.GetComponent<PlayerCombatNetwork>();
                    break;
                }
            }

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
                var readyCount = readyStates.Values.Count(x => x);
                _readyText.text = $"Ready: {readyCount}/{totalPlayers}";
            }
        }
    }
}
