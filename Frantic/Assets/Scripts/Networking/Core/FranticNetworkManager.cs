using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class FranticNetworkManager : NetworkManager
    {
        public static FranticNetworkManager Instance { get; private set; }

        public const string HUB_SCENE = "Hub";
        public const string DUNGEON_SCENE = "Dungeon";

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
            if (IsHost)
            {
                Debug.Log("[Network] Host loading hub scene");
                SceneManager.LoadScene(HUB_SCENE, LoadSceneMode.Single);
            }
        }

        public void LoadDungeonScene()
        {
            if (IsHost)
            {
                Debug.Log("[Network] Host loading dungeon scene");
                SceneManager.LoadScene(DUNGEON_SCENE, LoadSceneMode.Single);
            }
        }

        public void LoadHubScene()
        {
            if (IsHost)
            {
                Debug.Log("[Network] Host loading hub scene");
                SceneManager.LoadScene(HUB_SCENE, LoadSceneMode.Single);
            }
        }
    }
}
