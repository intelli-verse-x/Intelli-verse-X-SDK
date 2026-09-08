using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Creates UITK demo scenes under Assets and mirrors into the imported Samples folder when present.
    /// </summary>
    public static class IVXUITKSceneBootstrap
    {
        const string ScenesFolder = "Assets/IntelliVerseX UITK Demo Scenes";
        const string SampleScenesFolder = "Assets/Samples/IntelliVerseX SDK/UIToolkit/Scenes";
        const string PanelSettingsPath = "Assets/IntelliVerseX UITK Demo Scenes/IVXUITKPanelSettings.asset";
        const string MarkerPath = "Assets/IntelliVerseX UITK Demo Scenes/.uitk_scenes_ready";

        static readonly string[] SampleRootCandidates =
        {
            "Assets/Samples/IntelliVerseX SDK/UIToolkit",
            "Packages/com.intelliversex.sdk/Samples~/UIToolkit",
        };

        static readonly (string scene, string controllerType, string uxmlRel)[] Specs =
        {
            ("IVX_HomeScreen_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKHomeDemo", "UI/Home/IVXHome.uxml"),
            ("IVX_AuthTest_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKAuthDemo", "UI/Auth/IVXAuth.uxml"),
            ("IVX_Friends_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKFriendsDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_Clan_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKClanDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_Profile_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKProfileDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_LeaderboardTest_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKLeaderboardDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_WalletTest_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKWalletDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_WeeklyQuizTest_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKWeeklyQuizDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_DailyQuiz_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKDailyQuizDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_AdsTest_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKAdsDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_MoreOfUs_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKMoreOfUsDemo", "UI/Feature/IVXFeatureShell.uxml"),
            ("IVX_Share&RateUs_UITK", "IntelliVerseX.Samples.UIToolkit.IVXUITKShareRateDemo", "UI/Feature/IVXFeatureShell.uxml"),
        };

        [MenuItem("IntelliVerse-X/Samples/Create UITK Demo Scenes")]
        public static void MenuCreate()
        {
            if (File.Exists(MarkerPath))
                File.Delete(MarkerPath);
            Debug.Log("[IVXUITK] " + Run());
        }

        public static string Run()
        {
            string sampleRoot = ResolveSampleRoot();
            if (string.IsNullOrEmpty(sampleRoot))
                return "failed: UIToolkit sample root not found. Import Samples → UI Toolkit Demos first.";

            Directory.CreateDirectory(ScenesFolder);
            Directory.CreateDirectory(SampleScenesFolder);

            var theme = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{sampleRoot}/UI/IVXUITKTheme.uss");
            var panelSettings = EnsurePanelSettings();

            int ok = 0;
            var missing = new System.Text.StringBuilder();

            foreach (var spec in Specs)
            {
                var controllerType = FindType(spec.controllerType);
                if (controllerType == null)
                {
                    missing.AppendLine("missing type " + spec.controllerType);
                    continue;
                }

                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{sampleRoot}/{spec.uxmlRel}");
                if (uxml == null)
                {
                    missing.AppendLine("missing uxml " + spec.uxmlRel);
                    continue;
                }

                string scenePath = $"{ScenesFolder}/{spec.scene}.unity";
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                var cam = Camera.main;
                if (cam != null)
                    cam.backgroundColor = new Color(0.06f, 0.08f, 0.1f);

                var uiGo = new GameObject("UITK_Demo");
                var doc = uiGo.AddComponent<UIDocument>();
                doc.panelSettings = panelSettings;
                doc.visualTreeAsset = uxml;

                var controller = uiGo.AddComponent(controllerType);
                SetSerializedRefs(controller, doc, uxml, theme);

                EditorSceneManager.SaveScene(scene, scenePath);
                string mirror = $"{SampleScenesFolder}/{spec.scene}.unity";
                if (File.Exists(mirror))
                    AssetDatabase.DeleteAsset(mirror);
                AssetDatabase.CopyAsset(scenePath, mirror);
                ok++;
            }

            File.WriteAllText(MarkerPath, DateTime.UtcNow.ToString("o"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return $"created={ok}/{Specs.Length}; sampleRoot={sampleRoot}; missing={missing}";
        }

        static string ResolveSampleRoot()
        {
            for (int i = 0; i < SampleRootCandidates.Length; i++)
            {
                string root = SampleRootCandidates[i];
                if (AssetDatabase.LoadAssetAtPath<StyleSheet>($"{root}/UI/IVXUITKTheme.uss") != null)
                    return root;
                if (Directory.Exists(root) && File.Exists($"{root}/UI/IVXUITKTheme.uss"))
                    return root;
            }

            return null;
        }

        static PanelSettings EnsurePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null)
                return existing;

            Directory.CreateDirectory(ScenesFolder);
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        static void SetSerializedRefs(Component controller, UIDocument doc, VisualTreeAsset uxml, StyleSheet theme)
        {
            var so = new SerializedObject(controller);
            SetRef(so, "document", doc);
            SetRef(so, "visualTree", uxml);
            SetRef(so, "theme", theme);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetRef(SerializedObject so, string prop, UnityEngine.Object value)
        {
            var p = so.FindProperty(prop);
            if (p != null)
                p.objectReferenceValue = value;
        }

        static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null)
                    return t;
            }

            return null;
        }
    }
}
