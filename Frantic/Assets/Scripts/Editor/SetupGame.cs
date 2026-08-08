using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;

namespace Frantic.Networking.Editor
{
    public class SetupGame
    {
        static Sprite _greenSprite;
        static Sprite _redSprite;
        static Sprite _graySprite;
        static Sprite _yellowSprite;
        static Sprite _blueSprite;
        static bool _spritesInitialized;

        static Sprite GetSprite(Color color, string name)
        {
            var texturePath = "Assets/Sprites/Temp_" + ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2") + ".png";
            var spritePath = "Assets/Sprites/" + name + ".sprite";
            System.IO.Directory.CreateDirectory("Assets/Sprites");

            var existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (existingSprite != null) return existingSprite;

            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var pixels = new Color[256 * 256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(texturePath, bytes);
            AssetDatabase.ImportAsset(texturePath);
            AssetDatabase.Refresh();

            var loadedTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            loadedTexture.LoadImage(bytes);
            var sprite = Sprite.Create(loadedTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 128f);
            AssetDatabase.CreateAsset(sprite, spritePath);
            AssetDatabase.SaveAssets();

            Debug.Log("[Setup] Created sprite " + name + " at " + spritePath);
            return sprite;
        }

        static Sprite GreenSprite => _greenSprite ??= GetSprite(Color.green, "Green");
        static Sprite RedSprite => _redSprite ??= GetSprite(Color.red, "Red");
        static Sprite GraySprite => _graySprite ??= GetSprite(Color.gray, "Gray");
        static Sprite YellowSprite => _yellowSprite ??= GetSprite(Color.yellow, "Yellow");
        static Sprite BlueSprite => _blueSprite ??= GetSprite(Color.blue, "Blue");

        [MenuItem("Frantic/Setup All")]
        public static void SetupEverything()
        {
            ClearOldAssets();
            CreateHubScene();
            CreateDungeonNetworkPrefab();
            CreateDungeonScene();
            CreatePlayerPrefab();
            CreateEnemyPrefab();
            CreateRoomPrefab();
            CreateProjectilePrefab();
            CreateLootPrefab();
            CreateDungeonExitPrefab();
            RegisterAllPrefabs();
            AddScenesToBuildSettings();

            Debug.Log("[Setup] All assets created and registered!");
        }

        static void ClearOldAssets()
        {
            if (AssetDatabase.IsValidFolder("Assets/Sprites"))
            {
                AssetDatabase.DeleteAsset("Assets/Sprites");
            }

            var spriteColors = new[] { Color.green, Color.red, Color.gray, Color.yellow, Color.blue };
            foreach (var c in spriteColors)
            {
                var path = "Assets/Sprites/Temp_" + ((int)(c.r * 255)).ToString("X2") + ((int)(c.g * 255)).ToString("X2") + ((int)(c.b * 255)).ToString("X2") + ".png";
                if (File.Exists(path)) File.Delete(path);
            }

            var spriteNames = new[] { "Green", "Red", "Gray", "Yellow", "Blue" };
            foreach (var name in spriteNames)
            {
                var path = "Assets/Sprites/" + name + ".sprite";
                if (File.Exists(path)) File.Delete(path);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Frantic/Setup Hub Scene")]
        public static void CreateHubScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            var networkManager = new GameObject("NetworkManager");
            var nmComponent = networkManager.AddComponent<Frantic.Networking.FranticNetworkManager>();

            var transport = networkManager.AddComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", 7777);
            nmComponent.NetworkConfig.NetworkTransport = transport;

            var gameManager = new GameObject("GameManager");
            gameManager.AddComponent<Frantic.Networking.GameManager>();

            var readyManager = new GameObject("ReadyManager");
            readyManager.AddComponent<Frantic.Networking.ReadyManager>();

            var entrance = new GameObject("DungeonEntrance");
            entrance.tag = "DungeonEntrance";
            var boxCollider = entrance.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(4f, 2f);
            entrance.transform.position = new Vector3(0f, -8f, 0f);

            var entranceSpriteRenderer = entrance.AddComponent<SpriteRenderer>();
            entranceSpriteRenderer.sprite = BlueSprite;
            entranceSpriteRenderer.sortingOrder = 5;

            var camera = new GameObject("MainCamera");
            camera.tag = "MainCamera";
            camera.AddComponent<Frantic.Networking.CameraController>();
            var mainCamera = camera.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var hubRoot = new GameObject("Hub");

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Hub.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Hub scene created");
        }

        [MenuItem("Frantic/Setup Dungeon Network Prefab")]
        public static void CreateDungeonNetworkPrefab()
        {
            var prefabPath = "Assets/Prefabs/DungeonNetwork.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var dungeonRoot = new GameObject("DungeonNetwork");
            dungeonRoot.AddComponent<NetworkObject>();
            var dungeonNetwork = dungeonRoot.AddComponent<Frantic.Networking.DungeonNetwork>();

            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Room.prefab");
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            var exitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonExit.prefab");

            if (roomPrefab != null) dungeonNetwork._roomPrefab = roomPrefab;
            if (enemyPrefab != null) dungeonNetwork._enemyPrefab = enemyPrefab;
            if (exitPrefab != null) dungeonNetwork._dungeonExitPrefab = exitPrefab;

            PrefabUtility.SaveAsPrefabAsset(dungeonRoot, prefabPath);
            GameObject.DestroyImmediate(dungeonRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] DungeonNetwork prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Setup Dungeon Scene")]
        public static void CreateDungeonScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            var dungeonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonNetwork.prefab");
            if (dungeonPrefab != null)
            {
                var dungeonInstance = EditorSceneManager.InstantiateSceneObject(dungeonPrefab, Vector3.zero, Quaternion.identity, scene);
                dungeonInstance.name = "DungeonNetwork";
            }
            else
            {
                Debug.LogWarning("[Setup] DungeonNetwork prefab not found, creating placeholder");
                var dungeonNetwork = new GameObject("DungeonNetwork");
                dungeonNetwork.AddComponent<NetworkObject>();
                dungeonNetwork.AddComponent<Frantic.Networking.DungeonNetwork>();
            }

            var camera = new GameObject("MainCamera");
            camera.tag = "MainCamera";
            camera.AddComponent<Frantic.Networking.CameraController>();
            var mainCamera = camera.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Dungeon.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Dungeon scene created");
        }

        [MenuItem("Frantic/Setup Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            var prefabPath = "Assets/Prefabs/Player.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var playerRoot = new GameObject("Player");

            if (playerRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(playerRoot.GetComponent<MeshRenderer>());
            if (playerRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(playerRoot.GetComponent<MeshFilter>());

            playerRoot.AddComponent<NetworkObject>();
            var networkTransform = playerRoot.AddComponent<NetworkTransform>();
            networkTransform.SyncPositionX = true;
            networkTransform.SyncPositionY = true;
            networkTransform.UseQuaternionSynchronization = false;

            var rb = playerRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            playerRoot.AddComponent<CircleCollider2D>().radius = 0.5f;

            var spriteRenderer = playerRoot.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GreenSprite;
            spriteRenderer.sortingOrder = 10;

            var playerInput = playerRoot.AddComponent<PlayerInput>();
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (inputActions != null) playerInput.actions = inputActions;

            playerRoot.AddComponent<Frantic.Networking.PlayerNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerHealthNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerCombatNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerMovement>();
            var combat = playerRoot.GetComponent<Frantic.Networking.PlayerCombatNetwork>();
            if (combat != null) combat._projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            playerRoot.AddComponent<Frantic.Networking.InteractionNetwork>();

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

            if (enemyRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(enemyRoot.GetComponent<MeshRenderer>());
            if (enemyRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(enemyRoot.GetComponent<MeshFilter>());

            enemyRoot.AddComponent<NetworkObject>();
            var enemyNetworkTransform = enemyRoot.AddComponent<NetworkTransform>();
            enemyNetworkTransform.SyncPositionX = true;
            enemyNetworkTransform.SyncPositionY = true;
            enemyNetworkTransform.UseQuaternionSynchronization = false;
            enemyRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            enemyRoot.AddComponent<CircleCollider2D>().radius = 0.5f;

            var enemySpriteRenderer = enemyRoot.AddComponent<SpriteRenderer>();
            enemySpriteRenderer.sprite = RedSprite;
            enemySpriteRenderer.sortingOrder = 10;

            var enemyNetwork = enemyRoot.AddComponent<Frantic.Networking.EnemyNetwork>();
            var lootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Loot.prefab");
            if (lootPrefab != null) enemyNetwork._lootPrefab = lootPrefab;

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

            if (roomRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(roomRoot.GetComponent<MeshRenderer>());
            if (roomRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(roomRoot.GetComponent<MeshFilter>());

            roomRoot.AddComponent<NetworkObject>();
            var roomNetworkTransform = roomRoot.AddComponent<NetworkTransform>();
            roomNetworkTransform.SyncPositionX = true;
            roomNetworkTransform.SyncPositionY = true;
            roomNetworkTransform.UseQuaternionSynchronization = false;
            roomRoot.AddComponent<BoxCollider2D>().size = new Vector2(10f, 10f);

            var roomSpriteRenderer = roomRoot.AddComponent<SpriteRenderer>();
            roomSpriteRenderer.sprite = GraySprite;
            roomSpriteRenderer.sortingOrder = 0;

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
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var projRoot = new GameObject("Projectile");

            if (projRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(projRoot.GetComponent<MeshRenderer>());
            if (projRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(projRoot.GetComponent<MeshFilter>());

            projRoot.AddComponent<NetworkObject>();
            var networkTransform = projRoot.AddComponent<NetworkTransform>();
            networkTransform.SyncPositionX = true;
            networkTransform.SyncPositionY = true;
            networkTransform.UseQuaternionSynchronization = false;
            projRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            projRoot.AddComponent<CircleCollider2D>().radius = 0.15f;

            var projSpriteRenderer = projRoot.AddComponent<SpriteRenderer>();
            projSpriteRenderer.sprite = YellowSprite;
            projSpriteRenderer.sortingOrder = 20;

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

            if (lootRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(lootRoot.GetComponent<MeshRenderer>());
            if (lootRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(lootRoot.GetComponent<MeshFilter>());

            lootRoot.AddComponent<NetworkObject>();
            lootRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            lootRoot.AddComponent<CircleCollider2D>().radius = 0.3f;

            var lootSpriteRenderer = lootRoot.AddComponent<SpriteRenderer>();
            lootSpriteRenderer.sprite = GreenSprite;
            lootSpriteRenderer.sortingOrder = 15;

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

            if (exitRoot.GetComponent<MeshRenderer>() != null) GameObject.DestroyImmediate(exitRoot.GetComponent<MeshRenderer>());
            if (exitRoot.GetComponent<MeshFilter>() != null) GameObject.DestroyImmediate(exitRoot.GetComponent<MeshFilter>());

            exitRoot.AddComponent<NetworkObject>();
            exitRoot.AddComponent<BoxCollider2D>().isTrigger = true;
            exitRoot.AddComponent<BoxCollider2D>().size = new Vector2(3f, 2f);

            var exitSpriteRenderer = exitRoot.AddComponent<SpriteRenderer>();
            exitSpriteRenderer.sprite = BlueSprite;
            exitSpriteRenderer.sortingOrder = 5;

            exitRoot.AddComponent<Frantic.Networking.DungeonExitNetwork>();

            PrefabUtility.SaveAsPrefabAsset(exitRoot, prefabPath);
            GameObject.DestroyImmediate(exitRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] DungeonExit prefab created at " + prefabPath);
        }

        [MenuItem("Frantic/Register Prefabs")]
        public static void RegisterAllPrefabs()
        {
            var networkManager = Object.FindObjectOfType<Frantic.Networking.FranticNetworkManager>();

            if (networkManager == null)
            {
                var go = new GameObject("NetworkManager");
                networkManager = go.AddComponent<Frantic.Networking.FranticNetworkManager>();
            }

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
                EditorUtility.SetDirty(networkManager);
                Debug.Log("[Setup] Player prefab registered");
            }

            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Room.prefab");
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            var lootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Loot.prefab");
            var dungeonNetworkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonNetwork.prefab");
            var dungeonExitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DungeonExit.prefab");

            var defaultPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            if (defaultPrefabs != null)
            {
                var serializedObject = new SerializedObject(defaultPrefabs);
                var listProp = serializedObject.FindProperty("List");
                listProp.ClearArray();

                var prefabsToAdd = new List<GameObject>();
                if (playerPrefab != null) prefabsToAdd.Add(playerPrefab);
                if (enemyPrefab != null) prefabsToAdd.Add(enemyPrefab);
                if (roomPrefab != null) prefabsToAdd.Add(roomPrefab);
                if (projectilePrefab != null) prefabsToAdd.Add(projectilePrefab);
                if (lootPrefab != null) prefabsToAdd.Add(lootPrefab);
                if (dungeonNetworkPrefab != null) prefabsToAdd.Add(dungeonNetworkPrefab);
                if (dungeonExitPrefab != null) prefabsToAdd.Add(dungeonExitPrefab);

                foreach (var prefab in prefabsToAdd)
                {
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    var elem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                    elem.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                    elem.FindPropertyRelative("SourcePrefabToOverride").objectReferenceValue = prefab;
                    elem.FindPropertyRelative("Override").intValue = (int)NetworkPrefabOverride.None;
                }

                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();

                Debug.Log("[Setup] Prefabs registered in DefaultNetworkPrefabs.asset");
            }
        }

        static void AddScenesToBuildSettings()
        {
            var scenesToAdd = new[] { "Assets/Scenes/Hub.unity", "Assets/Scenes/Dungeon.unity" };

            var existingScenes = BuildPipeline.GetBuildPlayerSceneCollection(scenesToAdd, null).scenes.ToList();
            var toAdd = new List<EditorBuildSettingsScene>();

            foreach (var scenePath in scenesToAdd)
            {
                var alreadyExists = false;
                foreach (var existing in existingScenes)
                {
                    if (existing.path == scenePath)
                    {
                        alreadyExists = true;
                        toAdd.Add(new EditorBuildSettingsScene(scenePath, true));
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
