using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Frantic.Networking
{
    public class HUD : MonoBehaviour
    {
        [SerializeField]
        private Image _healthBar;

        [SerializeField]
        private TMP_Text _ammoText;

        private PlayerHealthNetwork _health;
        private PlayerCombatNetwork _combat;
        private bool _playerFound;

        public void SetUI(Image healthBar, TMP_Text ammoText, TMP_Text readyText)
        {
            _healthBar = healthBar;
            _ammoText = ammoText;
        }

        private void Awake()
        {
            if (_healthBar == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var image in images)
                {
                    if (image.transform.root == transform)
                    {
                        _healthBar = image;
                        break;
                    }
                }
            }

            if (_ammoText == null)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in texts)
                {
                    if (text.transform.root == transform)
                    {
                        _ammoText = text;
                        break;
                    }
                }
            }

            if (_healthBar != null)
            {
                Debug.Log($"[HUD] Found health bar: {_healthBar.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[HUD] No health bar found! Check prefab child structure.");
            }

            if (_ammoText != null)
            {
                Debug.Log($"[HUD] Found ammo text: {_ammoText.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[HUD] No ammo text found!");
            }

            var inDungeon = SceneManager.GetActiveScene().name == "Dungeon";

            if (_ammoText != null)
            {
                _ammoText.gameObject.SetActive(inDungeon);
            }

            if (_healthBar != null)
            {
                var healthBarContainer = _healthBar.transform.parent != null ? _healthBar.transform.parent.gameObject : _healthBar.gameObject;
                healthBarContainer.SetActive(inDungeon);
            }
        }

        private void Update()
        {
            if (!_playerFound)
            {
                var player = FindFirstObjectByType<PlayerNetwork>();
                if (player != null)
                {
                    _health = player.GetComponent<PlayerHealthNetwork>();
                    _combat = player.GetComponent<PlayerCombatNetwork>();
                    _playerFound = true;

                    if (_health != null)
                    {
                        _health.OnHealthChanged += UpdateHealthBar;
                    }
                    if (_combat != null)
                    {
                        _combat.OnWeaponFired += UpdateAmmoText;
                    }
                }
                return;
            }

            if (_health != null && _healthBar != null)
            {
                var healthPercent = (float)_health.CurrentHealth / _health.MaxHealth;
                _healthBar.fillAmount = healthPercent;
            }

            if (_combat != null && _ammoText != null)
            {
                _ammoText.text = $"Ammo: {_combat.CurrentAmmo}";
            }
        }

        private void UpdateHealthBar(int health)
        {
            if (_health != null && _healthBar != null)
            {
                var healthPercent = (float)_health.CurrentHealth / _health.MaxHealth;
                _healthBar.fillAmount = healthPercent;
            }
        }

        private void UpdateAmmoText()
        {
            if (_combat != null && _ammoText != null)
            {
                _ammoText.text = $"Ammo: {_combat.CurrentAmmo}";
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= UpdateHealthBar;
            }
            if (_combat != null)
            {
                _combat.OnWeaponFired -= UpdateAmmoText;
            }
        }
    }
}
