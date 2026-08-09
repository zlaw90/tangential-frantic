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
                        Debug.Log($"[HUD] Found health bar: {image.gameObject.name}");
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
                        Debug.Log($"[HUD] Found ammo text: {text.gameObject.name}");
                        break;
                    }
                }
            }

            if (_healthBar == null)
            {
                Debug.LogWarning("[HUD] No health bar found! Trying fallback...");
                var allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
                foreach (var img in allImages)
                {
                    if (img.name.Contains("HealthBar") && !img.name.Contains("Background"))
                    {
                        _healthBar = img;
                        Debug.Log($"[HUD] Fallback health bar: {img.gameObject.name}");
                        break;
                    }
                }
            }

            if (_ammoText == null)
            {
                Debug.LogWarning("[HUD] No ammo text found! Trying fallback...");
                var allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
                foreach (var txt in allTexts)
                {
                    if (txt.name.Contains("Ammo"))
                    {
                        _ammoText = txt;
                        Debug.Log($"[HUD] Fallback ammo text: {txt.gameObject.name}");
                        break;
                    }
                }
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
                var player = FindObjectOfType<PlayerNetwork>();
                if (player != null)
                {
                    _health = player.GetComponent<PlayerHealthNetwork>();
                    _combat = player.GetComponent<PlayerCombatNetwork>();
                    _playerFound = true;

                    Debug.Log($"[HUD] Player found: {player.gameObject.name}");
                    Debug.Log($"[HUD] Health component: {_health != null}");
                    Debug.Log($"[Combat] Combat component: {_combat != null}");

                    if (_health != null)
                    {
                        _health.OnHealthChanged += UpdateHealthBar;
                        Debug.Log("[HUD] Subscribed to OnHealthChanged");
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
                if (Mathf.Abs(_healthBar.fillAmount - healthPercent) > 0.01f)
                {
                    Debug.Log($"[HUD] Updating health bar: {_health.CurrentHealth}/{_health.MaxHealth} = {healthPercent:F2}");
                }
                _healthBar.fillAmount = healthPercent;
            }
            else if (_health != null)
            {
                Debug.LogWarning($"[HUD] Health bar is null! _healthBar={_healthBar != null}");
            }

            if (_combat != null && _ammoText != null)
            {
                _ammoText.text = $"Ammo: {_combat.CurrentAmmo}";
            }
        }

        private void UpdateHealthBar(int health)
        {
            Debug.Log($"[HUD] OnHealthChanged called: {health}");
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
