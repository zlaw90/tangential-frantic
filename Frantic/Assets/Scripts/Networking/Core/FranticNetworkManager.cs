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
        private bool _inDungeon;
        private Camera _hubCamera;
        private Camera _dungeonCamera;

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
            _hubCamera = FindHubCamera();
        }

        private Camera FindHubCamera()
        {
            var hubScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(HUB_SCENE);
            if (!hubScene.IsValid()) return null;

            foreach (var root in hubScene.GetRootGameObjects())
            {
                var cam = root.GetComponent<Camera>();
                if (cam != null) return cam;
                foreach (var child in root.GetComponentsInChildren<Camera>())
                {
                    if (child.CompareTag("MainCamera")) return child;
                }
            }
            var mainCam = Camera.main;
            if (mainCam != null && mainCam.gameObject.scene.name == HUB_SCENE) return mainCam;
            return null;
        }

        private void DisableSceneCameras(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>())
                {
                    cam.enabled = false;
                }
            }
        }

        private void EnableSceneCameras(string sceneName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>())
                {
                    cam.enabled = true;
                }
            }
        }

        private void OnDestroy()
        {
            _isShuttingDown = true;
        }

        public void StartHost()
        {
            Debug.Log("[Network] Starting host via NGO");
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

            Debug.Log("[Network] Host returning to hub");
            UnityEngine.SceneManagement.SceneManager.LoadScene(HUB_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Single);
            _inDungeon = false;
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
            DisableSceneCameras(HUB_SCENE);
            _inDungeon = true;
            var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(DUNGEON_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = true;
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(HUB_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            var hubScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(HUB_SCENE);
            if (hubScene.IsValid())
            {
                EnableSceneCameras(HUB_SCENE);
                _inDungeon = false;
                var dungeonScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(DUNGEON_SCENE);
                if (dungeonScene.IsValid())
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(DUNGEON_SCENE);
                }
            }
        }
    }
}
