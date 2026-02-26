#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IntelliVerseX.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Sets up home navigation across IVX test scenes and ensures a home screen scene exists.
    /// </summary>
    public static class IVXTestSceneHomeNavigationTool
    {
        private const string TEST_SCENES_FOLDER = "Assets/Scenes/Tests";
        private const string HOME_SCENE_PATH = TEST_SCENES_FOLDER + "/IVX_Homescreen.unity";

        private static readonly string[] RequiredScenePaths =
        {
            HOME_SCENE_PATH,
            TEST_SCENES_FOLDER + "/IVX_AuthTest.unity",
            TEST_SCENES_FOLDER + "/IVX_AdsTest.unity",
            TEST_SCENES_FOLDER + "/IVX_LeaderboardTest.unity",
            TEST_SCENES_FOLDER + "/IVX_WalletTest.unity",
            TEST_SCENES_FOLDER + "/IVX_WeeklyQuizTest.unity"
        };

        [MenuItem("IntelliVerse-X SDK/Tools/Test Scenes/Setup Home Navigation", false, 320)]
        public static void SetupHomeNavigationForAllScenes()
        {
            EditorSceneManager.SaveOpenScenes();

            EnsureFolder(TEST_SCENES_FOLDER);
            EnsureHomeSceneExists();

            int updatedCount = 0;
            foreach (string scenePath in RequiredScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool changed = EnsureNavigatorInOpenScene();
                if (changed)
                {
                    EditorSceneManager.SaveScene(scene);
                }

                AddSceneToBuildSettings(scenePath);
                updatedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[IVXTestSceneHomeNavigationTool] Updated {updatedCount} test scene(s). Home={HOME_SCENE_PATH}");
        }

        private static void EnsureHomeSceneExists()
        {
            if (File.Exists(HOME_SCENE_PATH))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();
            EnsureNavigatorInOpenScene();
            EditorSceneManager.SaveScene(scene, HOME_SCENE_PATH);
            AddSceneToBuildSettings(HOME_SCENE_PATH);
        }

        private static bool EnsureNavigatorInOpenScene()
        {
            var existing = Object.FindFirstObjectByType<IVXTestSceneNavigator>();
            if (existing != null)
            {
                return false;
            }

            var go = new GameObject("IVX_TestSceneNavigator");
            go.AddComponent<IVXTestSceneNavigator>();
            return true;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Any(s => s.path == scenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            if (parts.Length < 2 || parts[0] != "Assets")
            {
                return;
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
