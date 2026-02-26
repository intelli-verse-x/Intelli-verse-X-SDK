#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Configures the leaderboard test scene with production-ready runtime/UI wiring.
    /// </summary>
    public static class IVXLeaderboardSceneSetupTool
    {
        private const string LeaderboardScenePath = "Assets/Scenes/Tests/IVX_LeaderboardTest.unity";
        private const string LeaderboardCanvasPrefabPath = "Assets/_IntelliVerseXSDK/Leaderboard/Prefabs/IVXGLeaderboardCanvas.prefab";
        private const string LeaderboardManagerTypeName = "IntelliVerseX.Games.Leaderboard.IVXGLeaderboard";
        private const string LeaderboardUITypeName = "IntelliVerseX.Games.Leaderboard.UI.IVXGLeaderboardUI";

        [MenuItem("IntelliVerse-X SDK/Tools/Test Scenes/Setup Leaderboard Scene", false, 322)]
        public static void SetupLeaderboardScene()
        {
            var scene = OpenOrGetLeaderboardScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[IVXLeaderboardSceneSetup] Failed to open leaderboard scene.");
                return;
            }

            EnsureEventSystem();
            RemovePlaceholderInfoCanvas();
            EnsureLeaderboardManager();
            EnsureLeaderboardCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log("[IVXLeaderboardSceneSetup] Leaderboard scene setup complete.");
        }

        private static UnityEngine.SceneManagement.Scene OpenOrGetLeaderboardScene()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.IsValid() && string.Equals(active.path, LeaderboardScenePath, StringComparison.OrdinalIgnoreCase))
            {
                return active;
            }

            if (!System.IO.File.Exists(LeaderboardScenePath))
            {
                return default;
            }

            return EditorSceneManager.OpenScene(LeaderboardScenePath, OpenSceneMode.Single);
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                EnsureInputModule(eventSystem.gameObject);
                return;
            }

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            EnsureInputModule(go);
        }

        private static void EnsureInputModule(GameObject eventSystemGo)
        {
            var inputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                if (eventSystemGo.GetComponent(inputModuleType) == null)
                {
                    eventSystemGo.AddComponent(inputModuleType);
                }
                return;
            }

            if (eventSystemGo.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }
        }

        private static void RemovePlaceholderInfoCanvas()
        {
            var infoCanvas = GameObject.Find("InfoCanvas");
            if (infoCanvas != null)
            {
                UnityEngine.Object.DestroyImmediate(infoCanvas);
            }
        }

        private static void EnsureLeaderboardManager()
        {
            Type managerType = FindType(LeaderboardManagerTypeName);
            if (managerType == null)
            {
                Debug.LogError($"[IVXLeaderboardSceneSetup] Missing type: {LeaderboardManagerTypeName}");
                return;
            }

            var manager = FindFirstComponentByType(managerType);
            if (manager == null)
            {
                var go = new GameObject("IVXGLeaderboard");
                manager = go.AddComponent(managerType);
            }

            var so = new SerializedObject(manager);
            SetBool(so, "autoInitializeNakama", true);
            SetBool(so, "autoTestOnStart", false);
            SetBool(so, "enableDebugLogs", true);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureLeaderboardCanvas()
        {
            Type uiType = FindType(LeaderboardUITypeName);
            if (uiType == null)
            {
                Debug.LogError($"[IVXLeaderboardSceneSetup] Missing type: {LeaderboardUITypeName}");
                return;
            }

            var ui = FindFirstComponentByType(uiType);
            if (ui != null)
            {
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeaderboardCanvasPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[IVXLeaderboardSceneSetup] Missing prefab: {LeaderboardCanvasPrefabPath}");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[IVXLeaderboardSceneSetup] Failed to instantiate leaderboard canvas prefab.");
                return;
            }

            instance.name = "IVXGLeaderboardCanvas";
        }

        private static void SetBool(SerializedObject so, string propertyName, bool value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.boolValue = value;
            }
        }

        private static Type FindType(string fullName)
        {
            var type = Type.GetType(fullName);
            if (type != null) return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static Component FindFirstComponentByType(Type type)
        {
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                return null;
            }

            var components = UnityEngine.Object.FindObjectsByType(type, FindObjectsSortMode.None);
            if (components == null || components.Length == 0)
            {
                return null;
            }

            return components[0] as Component;
        }
    }
}
#endif
