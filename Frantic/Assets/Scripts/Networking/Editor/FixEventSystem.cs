using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;
using System.Linq;
using Unity.VisualScripting;

namespace Frantic.Networking.Editor
{
    public class FixEventSystem
    {
        [MenuItem("Frantic/Fix EventSystem Input Module")]
        public static void FixEventSystemInputModule()
        {
            var hubScene = EditorSceneManager.GetSceneByPath("Assets/Scenes/Hub.unity");
            if (!hubScene.IsValid())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Hub.unity");
                hubScene = EditorSceneManager.GetActiveScene();
            }

            var eventSystems = hubScene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<EventSystem>())
                .ToList();

            if (eventSystems.Count == 0)
            {
                Debug.LogWarning("[Fix] No EventSystem found in Hub scene");
                return;
            }

            foreach (var es in eventSystems)
            {
                var oldModule = es.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    GameObject.DestroyImmediate(oldModule);
                }

                var newModule = es.GetComponent<InputSystemUIInputModule>();
                if (newModule == null)
                {
                    newModule = es.AddComponent<InputSystemUIInputModule>();
                }
            }

            EditorSceneManager.SaveScene(hubScene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Fix] Replaced StandaloneInputModule with InputSystemUIInputModule on EventSystem");
        }
    }
}
