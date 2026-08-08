using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Frantic.Networking
{
    public class ConnectionMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject _menuPanel;

        [SerializeField]
        private Button _startButton;

        [SerializeField]
        private TMP_Text _statusText;

        [SerializeField]
        public GameObject _playerPrefab;

        public void SetUI(GameObject menuPanel, Button startButton, TMP_Text statusText)
        {
            _menuPanel = menuPanel;
            _startButton = startButton;
            _statusText = statusText;
            SetupListeners();
        }

        private void Awake()
        {
            if (_startButton != null)
            {
                SetupListeners();
            }
        }

        private void Start()
        {
            if (_statusText != null) _statusText.text = "Press START to begin";
        }

        public void SetupListeners()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(StartGame);
            }
        }

        private void StartGame()
        {
            Debug.Log("[Connection] Spawning player in Hub");
            if (_statusText != null) _statusText.text = "Use E on DungeonEntrance to start";
            if (_menuPanel != null) _menuPanel.SetActive(false);

            var existingPlayer = FindObjectOfType<PlayerNetwork>();
            if (existingPlayer != null)
            {
                Debug.Log("[Connection] Player already exists");
                return;
            }

            if (_playerPrefab != null)
            {
                Instantiate(_playerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            }
            else
            {
                Debug.LogError("[Connection] Player prefab not assigned in inspector!");
            }
        }
    }
}
