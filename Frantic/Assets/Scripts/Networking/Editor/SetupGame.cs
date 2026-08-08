using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Frantic.Networking.Editor
{
    public class SetupGame
    {
        [MenuItem("Frantic/Setup All")]
        public static void SetupEverything()
        {
            CreateHubScene();
            SetupConnectionMenu();
            SetupHUD();
            CreatePlayerPrefab();
            CreateProjectilePrefab();
            CreateLootPrefab();
            CreateEnemyPrefab();
            CreateRoomPrefab();
            CreateDungeonExitPrefab();
            CreateDungeonNetworkPrefab();
            CreateDungeonScene();
            AddScenesToBuildSettings();

            Debug.Log("[Setup] All assets created and configured!");
        }

        [MenuItem("Frantic/Setup Hub Scene")]
        public static void CreateHubScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            var gameManager = new GameObject("GameManager");
            gameManager.AddComponent<Frantic.Networking.GameManager>();

            var entrance = new GameObject("DungeonEntrance");
            entrance.tag = "DungeonEntrance";
            var boxCollider = entrance.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(4f, 2f);
            entrance.transform.position = new Vector3(0f, -8f, 0f);

            var camera = new GameObject("MainCamera");
            camera.tag = "MainCamera";
            camera.AddComponent<Frantic.Networking.CameraController>();
            var mainCamera = camera.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Hub.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Hub scene created");
        }

        [MenuItem("Frantic/Setup Connection Menu")]
        public static void SetupConnectionMenu()
        {
            var hubScene = EditorSceneManager.GetSceneByPath("Assets/Scenes/Hub.unity");
            if (!hubScene.IsValid())
            {
                Debug.LogWarning("[Setup] Hub scene not found");
                return;
            }

            if (!hubScene.isLoaded)
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity");
                hubScene = EditorSceneManager.GetActiveScene();
            }

            if (hubScene.GetRootGameObjects().Any(g => g.name == "ConnectionMenu"))
            {
                Debug.Log("[Setup] ConnectionMenu already in scene");
                return;
            }

            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.position = Vector3.zero;
            EditorSceneManager.MoveGameObjectToScene(canvasGO, hubScene);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.sizeDelta = Vector2.zero;
            canvasRect.anchoredPosition = Vector2.zero;

            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(canvasGO.transform);
            menuPanel.transform.localPosition = Vector3.zero;

            var panelRect = menuPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400f, 200f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = menuPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);

            var startButton = CreateButton("Start Button", "START", new Vector2(0f, 0f), new Vector2(200f, 50f), () => { });
            startButton.transform.SetParent(menuPanel.transform);

            var statusText = CreateText("Status Text", "Press START to begin", new Vector2(0f, -80f), new Vector2(300f, 30f));
            statusText.transform.SetParent(menuPanel.transform);

            var connectionMenuGO = new GameObject("ConnectionMenu");
            connectionMenuGO.transform.SetParent(canvasGO.transform);

            var connectionMenu = connectionMenuGO.AddComponent<Frantic.Networking.ConnectionMenu>();

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            connectionMenu._playerPrefab = playerPrefab;

            connectionMenu.SetUI(menuPanel, startButton, statusText);

            EditorSceneManager.SaveScene(hubScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] ConnectionMenu created");
        }

        [MenuItem("Frantic/Setup HUD")]
        public static void SetupHUD()
        {
            var hubScene = EditorSceneManager.GetSceneByPath("Assets/Scenes/Hub.unity");
            if (!hubScene.IsValid())
            {
                Debug.LogWarning("[Setup] Hub scene not found");
                return;
            }

            if (!hubScene.isLoaded)
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity");
                hubScene = EditorSceneManager.GetActiveScene();
            }

            if (hubScene.GetRootGameObjects().Any(g => g.name == "HUD"))
            {
                Debug.Log("[Setup] HUD already in scene");
                return;
            }

            var hudGO = new GameObject("HUD");

            var hudRect = hudGO.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.zero;
            hudRect.pivot = new Vector2(0.5f, 0.5f);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = Vector2.zero;

            EditorSceneManager.MoveGameObjectToScene(hudGO, hubScene);

            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(hudGO.transform);

            var healthBarRect = healthBarGO.AddComponent<RectTransform>();
            healthBarRect.anchoredPosition = new Vector2(-100f, -30f);
            healthBarRect.sizeDelta = new Vector2(150f, 20f);
            healthBarRect.anchorMin = new Vector2(0.5f, 0.5f);
            healthBarRect.anchorMax = new Vector2(0.5f, 0.5f);
            healthBarRect.pivot = new Vector2(0.5f, 0.5f);

            var healthBarImage = healthBarGO.AddComponent<Image>();
            healthBarImage.color = Color.red;

            var healthBarBackground = new GameObject("HealthBarBackground");
            healthBarBackground.transform.SetParent(healthBarGO.transform);
            healthBarBackground.transform.localPosition = Vector3.zero;

            var bgRect = healthBarBackground.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(150f, 20f);

            var bgImage = healthBarBackground.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgImage.raycastTarget = false;

            healthBarRect.SetAsFirstSibling();

            var ammoTextGO = new GameObject("AmmoText");
            ammoTextGO.transform.SetParent(hudGO.transform);

            var ammoTextRect = ammoTextGO.AddComponent<RectTransform>();
            ammoTextRect.anchoredPosition = new Vector2(100f, -30f);
            ammoTextRect.sizeDelta = new Vector2(100f, 30f);
            ammoTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            ammoTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            ammoTextRect.pivot = new Vector2(0.5f, 0.5f);

            var ammoText = ammoTextGO.AddComponent<TextMeshProUGUI>();
            ammoText.fontSize = 18;
            ammoText.alignment = TextAlignmentOptions.Center;
            ammoText.text = "Ammo: 30";

            hudGO.AddComponent<Frantic.Networking.HUD>();

            EditorSceneManager.SaveScene(hubScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] HUD created");
        }

        static Button CreateButton(string name, string buttonText, Vector2 anchoredPosition, Vector2 size, System.Action onClick)
        {
            var buttonGO = new GameObject(name);

            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.8f);

            var button = buttonGO.AddComponent<Button>();
            button.onClick.AddListener(() => onClick());

            var buttonTextGO = new GameObject("ButtonText");
            buttonTextGO.transform.SetParent(buttonGO.transform);
            buttonTextGO.transform.localPosition = Vector3.zero;

            var textRect = buttonTextGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            var text = buttonTextGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = 20;
            text.text = buttonText;
            text.alignment = TextAlignmentOptions.Center;

            return button;
        }

        static TextMeshProUGUI CreateText(string name, string text, Vector2 anchoredPosition, Vector2 size)
        {
            var textGO = new GameObject(name);

            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 16;
            textComponent.text = text;
            textComponent.alignment = TextAlignmentOptions.Center;

            return textComponent;
        }

        [MenuItem("Frantic/Setup Dungeon Network Prefab")]
        public static void CreateDungeonNetworkPrefab()
        {
            var prefabPath = "Assets/Prefabs/DungeonNetwork.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var dungeonRoot = new GameObject("DungeonNetwork");
            var dungeonNetwork = dungeonRoot.AddComponent<Frantic.Networking.DungeonNetwork>();

            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Room.prefab");
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

            if (roomPrefab != null) dungeonNetwork._roomPrefab = roomPrefab;
            if (enemyPrefab != null) dungeonNetwork._enemyPrefab = enemyPrefab;
            if (playerPrefab != null) dungeonNetwork._playerPrefab = playerPrefab;

            PrefabUtility.SaveAsPrefabAsset(dungeonRoot, prefabPath);
            GameObject.DestroyImmediate(dungeonRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] DungeonNetwork prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Dungeon Scene")]
        public static void CreateDungeonScene()
        {
            var dungeonScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(dungeonScene);

            var dungeonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonNetwork.prefab");
            if (dungeonPrefab != null)
            {
                var dungeonInstance = GameObject.Instantiate(dungeonPrefab);
                dungeonInstance.name = "DungeonNetwork";
                EditorSceneManager.MoveGameObjectToScene(dungeonInstance, dungeonScene);
                Debug.Log("[Setup] Added DungeonNetwork to scene");
            }
            else
            {
                Debug.LogWarning("[Setup] DungeonNetwork prefab not found");
            }

            var exitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonExit.prefab");
            if (exitPrefab != null)
            {
                var exitInstance = GameObject.Instantiate(exitPrefab);
                exitInstance.name = "DungeonExit";
                exitInstance.transform.position = new Vector3(0f, -6f, 0f);
                EditorSceneManager.MoveGameObjectToScene(exitInstance, dungeonScene);
                Debug.Log("[Setup] Added DungeonExit to scene");
            }
            else
            {
                Debug.LogWarning("[Setup] DungeonExit prefab not found");
            }

            var camera = new GameObject("MainCamera");
            camera.tag = "MainCamera";
            camera.AddComponent<Frantic.Networking.CameraController>();
            var mainCamera = camera.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            EditorSceneManager.MoveGameObjectToScene(camera, dungeonScene);
            Debug.Log("[Setup] Added MainCamera to scene");

            EditorSceneManager.SaveScene(dungeonScene, "Assets/Scenes/Dungeon.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Dungeon scene ready");
        }

        [MenuItem("Frantic/Setup Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            var prefabPath = "Assets/Prefabs/Player.prefab";

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log("[Setup] Player prefab already exists, recreating for single-player");
                AssetDatabase.DeleteAsset(prefabPath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var playerRoot = new GameObject("Player");

            var rb = playerRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            playerRoot.AddComponent<CircleCollider2D>().radius = 0.5f;

            var playerInput = playerRoot.AddComponent<PlayerInput>();

            playerRoot.AddComponent<Frantic.Networking.PlayerNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerHealthNetwork>();
            var combat = playerRoot.AddComponent<Frantic.Networking.PlayerCombatNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerMovement>();
            playerRoot.AddComponent<Frantic.Networking.InteractionNetwork>();

            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            if (projectilePrefab != null)
            {
                combat._projectilePrefab = projectilePrefab;
            }

            var weaponTip = new GameObject("WeaponTip");
            weaponTip.transform.parent = playerRoot.transform;
            weaponTip.transform.position = new Vector3(0.5f, 0f, 0f);

            PrefabUtility.SaveAsPrefabAsset(playerRoot, prefabPath);
            GameObject.DestroyImmediate(playerRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Player prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Enemy Prefab")]
        public static void CreateEnemyPrefab()
        {
            var prefabPath = "Assets/Prefabs/Enemy.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var enemyRoot = new GameObject("Enemy");

            if (enemyRoot.GetComponent<SpriteRenderer>() != null) GameObject.DestroyImmediate(enemyRoot.GetComponent<SpriteRenderer>());

            enemyRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            enemyRoot.AddComponent<CircleCollider2D>().radius = 0.5f;

            enemyRoot.AddComponent<SpriteRenderer>();

            var enemyNetwork = enemyRoot.AddComponent<Frantic.Networking.EnemyNetwork>();

            PrefabUtility.SaveAsPrefabAsset(enemyRoot, prefabPath);
            GameObject.DestroyImmediate(enemyRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Enemy prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Room Prefab")]
        public static void CreateRoomPrefab()
        {
            var prefabPath = "Assets/Prefabs/Room.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var roomRoot = new GameObject("Room");

            if (roomRoot.GetComponent<SpriteRenderer>() != null) GameObject.DestroyImmediate(roomRoot.GetComponent<SpriteRenderer>());

            roomRoot.AddComponent<BoxCollider2D>().size = new Vector2(20f, 20f);

            roomRoot.AddComponent<SpriteRenderer>();

            PrefabUtility.SaveAsPrefabAsset(roomRoot, prefabPath);
            GameObject.DestroyImmediate(roomRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Room prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Projectile Prefab")]
        public static void CreateProjectilePrefab()
        {
            var prefabPath = "Assets/Prefabs/Projectile.prefab";

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log("[Setup] Projectile prefab already exists, skipping creation");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var projRoot = new GameObject("Projectile");

            projRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            projRoot.AddComponent<CircleCollider2D>().radius = 0.15f;

            PrefabUtility.SaveAsPrefabAsset(projRoot, prefabPath);
            GameObject.DestroyImmediate(projRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Projectile prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Loot Prefab")]
        public static void CreateLootPrefab()
        {
            var prefabPath = "Assets/Prefabs/Loot.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var lootRoot = new GameObject("Loot");

            if (lootRoot.GetComponent<SpriteRenderer>() != null) GameObject.DestroyImmediate(lootRoot.GetComponent<SpriteRenderer>());

            lootRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            lootRoot.AddComponent<CircleCollider2D>().radius = 0.3f;

            lootRoot.AddComponent<SpriteRenderer>();
            lootRoot.AddComponent<Frantic.Networking.LootNetwork>();

            PrefabUtility.SaveAsPrefabAsset(lootRoot, prefabPath);
            GameObject.DestroyImmediate(lootRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Loot prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Dungeon Exit Prefab")]
        public static void CreateDungeonExitPrefab()
        {
            var prefabPath = "Assets/Prefabs/DungeonExit.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var exitRoot = new GameObject("DungeonExit");
            exitRoot.tag = "DungeonExit";

            if (exitRoot.GetComponent<SpriteRenderer>() != null) GameObject.DestroyImmediate(exitRoot.GetComponent<SpriteRenderer>());

            var exitCollider = exitRoot.AddComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector2(3f, 2f);

            exitRoot.AddComponent<SpriteRenderer>();
            exitRoot.AddComponent<Frantic.Networking.DungeonExitNetwork>();

            PrefabUtility.SaveAsPrefabAsset(exitRoot, prefabPath);
            GameObject.DestroyImmediate(exitRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] DungeonExit prefab created at " + prefabPath);
        }

        static void AddScenesToBuildSettings()
        {
            var scenesToAdd = new[] { "Assets/Scenes/Hub.unity", "Assets/Scenes/Dungeon.unity" };

            var toAdd = new List<EditorBuildSettingsScene>();

            foreach (var existing in EditorBuildSettings.scenes)
            {
                toAdd.Add(existing);
            }

            foreach (var scenePath in scenesToAdd)
            {
                var alreadyExists = false;
                foreach (var existing in toAdd)
                {
                    if (existing.path == scenePath)
                    {
                        alreadyExists = true;
                        toAdd[toAdd.IndexOf(toAdd.First(e => e.path == scenePath))] = new EditorBuildSettingsScene(scenePath, true);
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    toAdd.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = toAdd.ToArray();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[Setup] Scenes added to Build Settings");
        }
    }
}
