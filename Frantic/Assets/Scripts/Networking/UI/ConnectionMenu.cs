using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Frantic.Networking
{
    public class ConnectionMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject _menuPanel;

        [SerializeField]
        private Button _hostButton;

        [SerializeField]
        private Button _clientButton;

        [SerializeField]
        private TMP_InputField _ipInputField;

        [SerializeField]
        private TMP_Text _statusText;

        private void Awake()
        {
            _hostButton.onClick.AddListener(StartHost);
            _clientButton.onClick.AddListener(StartClient);
        }

        private void Start()
        {
            _statusText.text = "Select mode to start";
        }

        private void StartHost()
        {
            Debug.Log("[Connection] Starting host");
            _statusText.text = "Starting host...";
            _menuPanel.SetActive(false);
            FranticNetworkManager.Instance.StartHost();
        }

        private void StartClient()
        {
            var ip = string.IsNullOrEmpty(_ipInputField.text) ? "127.0.0.1" : _ipInputField.text;
            Debug.Log($"[Connection] Connecting to {ip}");
            _statusText.text = $"Connecting to {ip}...";
            _menuPanel.SetActive(false);

            var transport = FranticNetworkManager.Instance.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(ip, 7778);
            }

            FranticNetworkManager.Instance.StartClient();
        }
    }
}
