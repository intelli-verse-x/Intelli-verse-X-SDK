#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace IntelliVerseX.Games.Social.Editor
{
    /// <summary>
    /// Editor utility for creating social feature prefabs (Share, Rate) and adding them to scenes.
    /// </summary>
    public static class IVXGSocialPrefabBuilder
    {
        private const string PREFABS_PATH = "Assets/_IntelliVerseXSDK/Social/Prefabs";
        private const string LOG_PREFIX = "[IVXGSocialPrefabBuilder]";

        #region Menu Items

        [MenuItem("IntelliVerseX/Social/Create Share Manager Prefab")]
        public static GameObject CreateShareManagerPrefab()
        {
            EnsurePrefabsDirectory();

            var go = new GameObject("IVXGShareManager");
            go.AddComponent<IVXGShareManager>();

            string prefabPath = $"{PREFABS_PATH}/IVXGShareManager.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            Debug.Log($"{LOG_PREFIX} Created share manager prefab at: {prefabPath}");
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            return prefab;
        }

        [MenuItem("IntelliVerseX/Social/Create Rate App Manager Prefab")]
        public static GameObject CreateRateAppManagerPrefab()
        {
            EnsurePrefabsDirectory();

            var go = new GameObject("IVXGRateAppManager");
            go.AddComponent<IVXGRateAppManager>();

            string prefabPath = $"{PREFABS_PATH}/IVXGRateAppManager.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            Debug.Log($"{LOG_PREFIX} Created rate app manager prefab at: {prefabPath}");
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            return prefab;
        }

        [MenuItem("IntelliVerseX/Social/Create All Social Prefabs")]
        public static void CreateAllPrefabs()
        {
            CreateShareManagerPrefab();
            CreateRateAppManagerPrefab();

            EditorUtility.DisplayDialog("Social Prefabs Created",
                "Created social prefabs:\n\n" +
                "• IVXGShareManager.prefab\n" +
                "• IVXGRateAppManager.prefab\n\n" +
                $"Location: {PREFABS_PATH}",
                "OK");
        }

        [MenuItem("IntelliVerseX/Social/Add Share to Scene")]
        public static void AddShareToScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/IVXGShareManager.prefab");
            if (prefab == null)
            {
                prefab = CreateShareManagerPrefab();
            }

#if UNITY_2023_1_OR_NEWER
            var existing = Object.FindFirstObjectByType<IVXGShareManager>();
#else
            var existing = Object.FindObjectOfType<IVXGShareManager>();
#endif

            if (existing == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Share Manager");
                Debug.Log($"{LOG_PREFIX} Added IVXGShareManager to scene");
            }
            else
            {
                Debug.Log($"{LOG_PREFIX} IVXGShareManager already exists in scene");
            }

            ShowShareUsage();
        }

        [MenuItem("IntelliVerseX/Social/Add Rate App to Scene")]
        public static void AddRateAppToScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/IVXGRateAppManager.prefab");
            if (prefab == null)
            {
                prefab = CreateRateAppManagerPrefab();
            }

#if UNITY_2023_1_OR_NEWER
            var existing = Object.FindFirstObjectByType<IVXGRateAppManager>();
#else
            var existing = Object.FindObjectOfType<IVXGRateAppManager>();
#endif

            if (existing == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Add Rate App Manager");
                Debug.Log($"{LOG_PREFIX} Added IVXGRateAppManager to scene");
            }
            else
            {
                Debug.Log($"{LOG_PREFIX} IVXGRateAppManager already exists in scene");
            }

            ShowRateUsage();
        }

        [MenuItem("IntelliVerseX/Social/Add All Social Features to Scene")]
        public static void AddAllToScene()
        {
            AddShareToScene();
            AddRateAppToScene();

            EditorUtility.DisplayDialog("Social Features Added",
                "Added social features to scene:\n\n" +
                "• IVXGShareManager - Native sharing\n" +
                "• IVXGRateAppManager - In-app rating\n\n" +
                "See Console for usage instructions.",
                "OK");
        }

        #endregion

        #region Usage Instructions

        private static void ShowShareUsage()
        {
            Debug.Log($@"{LOG_PREFIX} === Share Manager Usage ===

1. Share Score:
   IVXGShareManager.Instance.ShareScore(""My Game"", 1000);

2. Share with Screenshot:
   IVXGShareManager.Instance.ShareWithScreenshot(""Check this out!"");

3. Share Achievement:
   IVXGShareManager.Instance.ShareAchievement(""First Win"", ""Won your first match!"");

4. Share Referral:
   IVXGShareManager.Instance.ShareReferral(""ABC123"");

5. Share Custom Text:
   IVXGShareManager.Instance.ShareText(""Custom message"", ""https://myapp.com"");

6. Listen for completion:
   IVXGShareManager.Instance.OnShareCompleted += (success) => {{ }};
");
        }

        private static void ShowRateUsage()
        {
            Debug.Log($@"{LOG_PREFIX} === Rate App Manager Usage ===

1. After positive event (win, achievement):
   IVXGRateAppManager.Instance.RegisterPositiveEvent();

2. Try to show prompt (respects conditions):
   IVXGRateAppManager.Instance.TryShowRatePrompt();

3. Show at natural break points:
   IVXGRateAppManager.Instance.ShowRatePromptIfAppropriate();

4. Force show (bypass conditions):
   IVXGRateAppManager.Instance.ForceShowRatePrompt();

5. Open store directly:
   IVXGRateAppManager.Instance.OpenStorePage();

6. Configure in Inspector:
   - iOS App ID
   - Android Package Name
   - Min sessions before prompt
   - Days before retry
   - Max prompt attempts

7. Events:
   IVXGRateAppManager.Instance.OnRatePromptShown += () => {{ }};
   IVXGRateAppManager.Instance.OnRateCompleted += () => {{ }};
");
        }

        #endregion

        #region Helpers

        private static void EnsurePrefabsDirectory()
        {
            if (!Directory.Exists(PREFABS_PATH))
            {
                Directory.CreateDirectory(PREFABS_PATH);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
#endif
