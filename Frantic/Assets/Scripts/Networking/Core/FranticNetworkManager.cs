using UnityEngine;
using Unity.Netcode;
using System.Linq;

namespace Frantic.Networking
{
    public class FranticNetworkManager : NetworkManager
    {
        public static FranticNetworkManager Instance { get; private set; }

        public const string HUB_SCENE = "Hub";
        public const string DUNGEON_SCENE = "Dungeon";

        private bool _isShuttingDown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [SerializeField]
        private GameObject _playerPrefabOverride;

        private void Start()
        {
            OnClientDisconnectCallback += OnClientDisconnect;

            if (NetworkConfig.PlayerPrefab == null && _playerPrefabOverride != null)
            {
                NetworkConfig.PlayerPrefab = _playerPrefabOverride;
            }
        }

        private void DisableAllCameras()
        {
            var cameras = FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                if (cam != null) cam.enabled = false;
            }
        }

        private void EnableAllCameras()
        {
            var cameras = FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                if (cam != null) cam.enabled = true;
            }
        }

        private void OnDestroy()
        {
            _isShuttingDown = true;
        }

        public void StartHost()
        {
            base.StartHost();
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void JoinGame()
        {
            NetworkManager.Singleton.StartClient();
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_isShuttingDown) return;

            if (IsHost)
            {
                var players = ConnectedClientsList;
                if (players.Count <= 1)
                {
                    ReturnToHub();
                }
            }
        }

        public void ReturnToHub()
        {
            if (!IsHost) return;
            if (_isShuttingDown) return;

            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name == HUB_SCENE)
            {
                return;
            }

            LoadHubScene();
        }

        public void LoadDungeonScene()
        {
            if (!IsHost)
            {
                Debug.LogWarning("[Network] LoadDungeonScene called but not host");
                return;
            }
            if (_isShuttingDown) return;

            HostLoadDungeonSceneClientRpc();
        }

        public void LoadHubScene()
        {
            if (!IsHost) return;
            HostLoadHubSceneClientRpc();
        }

        [ClientRpc]
        private void HostLoadDungeonSceneClientRpc()
        {
            DisableAllCameras();
            var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(DUNGEON_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = true;
        }

        [ClientRpc]
        private void HostLoadHubSceneClientRpc()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(HUB_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            EnableAllCameras();
            var dungeonScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(DUNGEON_SCENE);
            if (dungeonScene.IsValid())
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(DUNGEON_SCENE);
            }
        }
    }
}
