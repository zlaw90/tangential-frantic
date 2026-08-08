using UnityEngine;
using Unity.Netcode;

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

        private void Start()
        {
            OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnDestroy()
        {
            _isShuttingDown = true;
        }

        public void StartHost()
        {
            StartServer();
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

            Debug.Log($"[Network] Client {clientId} disconnected");

            if (IsHost)
            {
                var players = ConnectedClientsList;
                Debug.Log($"[Network] Host: {players.Count - 1} players remaining");

                if (players.Count <= 1)
                {
                    Debug.Log("[Network] Host is alone, returning to hub");
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
                Debug.Log("[Network] Already in hub scene");
                return;
            }

            Debug.Log("[Network] Host loading hub scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(HUB_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public void LoadDungeonScene()
        {
            if (!IsHost)
            {
                Debug.LogWarning("[Network] LoadDungeonScene called but not host");
                return;
            }
            if (_isShuttingDown) return;

            Debug.Log("[Network] Host loading dungeon scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(DUNGEON_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);
            Debug.Log("[Network] Dungeon scene load initiated");
        }

        public void LoadHubScene()
        {
            if (!IsHost) return;
            if (_isShuttingDown) return;

            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name == HUB_SCENE)
            {
                Debug.Log("[Network] Already in hub scene");
                return;
            }

            Debug.Log("[Network] Host loading hub scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(HUB_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
