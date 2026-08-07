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
        [MenuItem("Frantic/Setup All")]
        public static void SetupEverything()
        {
            CreateHubScene();
            CreatePlayerPrefab();
            CreateEnemyPrefab();
            CreateRoomPrefab();
            CreateProjectilePrefab();
            RegisterAllPrefabs();

            Debug.Log("[Setup] All assets created and registered!");
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

            var exitMarker = entrance.AddComponent<SpriteRenderer>();
            exitMarker.color = Color.blue;

            var camera = new GameObject("MainCamera");
            camera.tag = "MainCamera";
            camera.AddComponent<Frantic.Networking.CameraController>();
            var mainCamera = camera.AddComponent<Camera>();
            mainCamera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var hubRoot = new GameObject("Hub");

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Hub.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Hub scene created");
        }

        [MenuItem("Frantic/Setup Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            var prefabPath = "Assets/Prefabs/Player.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

            var playerRoot = new GameObject("Player");

            playerRoot.AddComponent<NetworkObject>();
            var networkTransform = playerRoot.AddComponent<NetworkTransform>();
            networkTransform.SyncPositionX = true;
            networkTransform.SyncPositionY = true;
            networkTransform.UseQuaternionSynchronization = false;

            var rb = playerRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            playerRoot.AddComponent<CircleCollider2D>().radius = 0.5f;
            playerRoot.AddComponent<SpriteRenderer>().color = Color.green;

            var playerInput = playerRoot.AddComponent<PlayerInput>();
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (inputActions != null) playerInput.actions = inputActions;

            playerRoot.AddComponent<Frantic.Networking.PlayerNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerHealthNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerCombatNetwork>();
            playerRoot.AddComponent<Frantic.Networking.PlayerMovement>();

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
            enemyRoot.AddComponent<NetworkObject>();
            enemyRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            enemyRoot.AddComponent<CircleCollider2D>().radius = 0.5f;
            enemyRoot.AddComponent<SpriteRenderer>().color = Color.red;
            enemyRoot.AddComponent<Frantic.Networking.EnemyNetwork>();

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
            roomRoot.AddComponent<NetworkObject>();
            roomRoot.AddComponent<BoxCollider2D>().size = new Vector2(10f, 10f);
            var sr = roomRoot.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            sr.size = new Vector2(10f, 10f);

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
            projRoot.AddComponent<NetworkObject>();
            projRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            projRoot.AddComponent<CircleCollider2D>().radius = 0.15f;
            projRoot.AddComponent<SpriteRenderer>().color = Color.yellow;

            PrefabUtility.SaveAsPrefabAsset(projRoot, prefabPath);
            GameObject.DestroyImmediate(projRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Projectile prefab created at " + prefabPath);
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

            var defaultPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            if (defaultPrefabs != null)
            {
                var serializedObject = new SerializedObject(defaultPrefabs);
                var listProp = serializedObject.FindProperty("List");
                listProp.ClearArray();

                var prefabsToAdd = new List<GameObject>();
                if (enemyPrefab != null) prefabsToAdd.Add(enemyPrefab);
                if (roomPrefab != null) prefabsToAdd.Add(roomPrefab);
                if (projectilePrefab != null) prefabsToAdd.Add(projectilePrefab);

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
    }
}
