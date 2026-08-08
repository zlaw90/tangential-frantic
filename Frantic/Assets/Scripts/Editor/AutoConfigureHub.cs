using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Frantic.Networking.Editor
{
    [InitializeOnLoad]
    public class AutoConfigureHub
    {
        static AutoConfigureHub()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ConfigureHubScene();
            }
        }

        [MenuItem("Frantic/Configure Hub Scene")]
        public static void ConfigureHubScene()
        {
            var hubScene = SceneManager.GetSceneByName("Hub");
            if (!hubScene.IsValid())
            {
                Debug.LogWarning("[AutoConfigure] Hub scene not found");
                return;
            }

            GameObject networkManagerGO = null;
            foreach (var root in hubScene.GetRootGameObjects())
            {
                var nm = root.GetComponent<Frantic.Networking.FranticNetworkManager>();
                if (nm != null)
                {
                    networkManagerGO = root;
                    break;
                }
            }

            if (networkManagerGO == null)
            {
                Debug.LogWarning("[AutoConfigure] No NetworkManager found in Hub scene");
                return;
            }

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            if (playerPrefab == null)
            {
                Debug.LogError("[AutoConfigure] Player prefab not found at Assets/Prefabs/Player.prefab");
                return;
            }

            var nmComponent = networkManagerGO.GetComponent<Frantic.Networking.FranticNetworkManager>();
            if (nmComponent != null)
            {
                nmComponent.NetworkConfig.PlayerPrefab = playerPrefab;
                nmComponent._playerPrefabOverride = playerPrefab;
                EditorUtility.SetDirty(networkManagerGO);
                Debug.Log("[AutoConfigure] Player prefab assigned to NetworkManager");
            }

            var defaultPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            if (defaultPrefabs != null)
            {
                if (!defaultPrefabs.Contains(playerPrefab))
                {
                    var networkObject = playerPrefab.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        var networkPrefab = new NetworkPrefab
                        {
                            Prefab = playerPrefab,
                            SourcePrefabToOverride = playerPrefab,
                            Override = NetworkPrefabOverride.None
                        };
                        defaultPrefabs.Add(networkPrefab);
                        EditorUtility.SetDirty(defaultPrefabs);
                        Debug.Log("[AutoConfigure] Player prefab added to DefaultNetworkPrefabs");
                    }
                }
            }

            EditorSceneManager.SaveScene(hubScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
