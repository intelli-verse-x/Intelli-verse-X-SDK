using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace IntelliVerseX.Bootstrap.Editor
{
    /// <summary>
    /// Bootstrap menu that always compiles regardless of which optional packages are installed.
    /// Provides core SDK functionality and dependency management.
    /// </summary>
    public static class IVXBootstrapMenu
    {
        private const string MENU_ROOT = "IntelliVerseX/";
        private const string DOCS_URL = "https://intelliversex.github.io/intelliversex-unity-sdk/";
        
        #region Documentation Menu
        
        [MenuItem(MENU_ROOT + "Documentation/Open Online Docs", false, 0)]
        private static void OpenOnlineDocs()
        {
            Application.OpenURL(DOCS_URL);
        }
        
        [MenuItem(MENU_ROOT + "Documentation/Getting Started", false, 1)]
        private static void OpenGettingStarted()
        {
            Application.OpenURL(DOCS_URL + "getting-started/installation/");
        }
        
        [MenuItem(MENU_ROOT + "Documentation/API Reference", false, 2)]
        private static void OpenAPIReference()
        {
            Application.OpenURL(DOCS_URL + "api-reference/");
        }
        
        [MenuItem(MENU_ROOT + "Documentation/Dependency Setup Guide", false, 3)]
        private static void OpenDependencyGuide()
        {
            Application.OpenURL(DOCS_URL + "getting-started/requirements/");
        }
        
        #endregion
        
        #region Dependency Status Menu
        
        [MenuItem(MENU_ROOT + "SDK Status/Check Dependencies", false, 100)]
        private static void CheckDependencies()
        {
            var status = new DependencyStatus();
            
            EditorUtility.DisplayDialog(
                "IntelliVerseX SDK - Dependency Status",
                status.GetStatusMessage(),
                "OK"
            );
        }
        
        [MenuItem(MENU_ROOT + "SDK Status/Open Package Manager", false, 101)]
        private static void OpenPackageManager()
        {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }
        
        [MenuItem(MENU_ROOT + "SDK Status/Refresh Symbols", false, 102)]
        private static void RefreshSymbols()
        {
            IVXDefineSymbolManager.ForceReapplyDefines();
        }
        
        #endregion
        
        #region Quick Actions Menu
        
        [MenuItem(MENU_ROOT + "Quick Actions/Create SDK Settings", false, 200)]
        private static void CreateSDKSettings()
        {
            // Find or create settings asset
            string settingsPath = "Assets/_IntelliVerseXSDK/Resources/IVXSettings.asset";
            
            if (File.Exists(settingsPath))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsPath);
                EditorGUIUtility.PingObject(Selection.activeObject);
                Debug.Log("[IntelliVerseX] SDK Settings found at: " + settingsPath);
            }
            else
            {
                Debug.LogWarning("[IntelliVerseX] SDK Settings not found. Install required dependencies first.");
                CheckDependencies();
            }
        }
        
        [MenuItem(MENU_ROOT + "Quick Actions/Locate SDK Folder", false, 201)]
        private static void LocateSDKFolder()
        {
            string sdkPath = "Assets/_IntelliVerseXSDK";
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(sdkPath);
            
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                Debug.LogError("[IntelliVerseX] SDK folder not found at: " + sdkPath);
            }
        }
        
        #endregion
        
        #region About Menu
        
        [MenuItem(MENU_ROOT + "About IntelliVerseX SDK", false, 1000)]
        private static void ShowAbout()
        {
            IVXAboutWindow.ShowWindow();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Checks for installed dependencies and reports status.
    /// </summary>
    internal class DependencyStatus
    {
        public bool HasNakama { get; private set; }
        public bool HasPhoton { get; private set; }
        public bool HasDOTween { get; private set; }
        public bool HasNewtonsoft { get; private set; }
        public bool HasTextMeshPro { get; private set; }
        public bool HasAppodeal { get; private set; }
        public bool HasLevelPlay { get; private set; }
        
        public bool HasAllRequired => HasNewtonsoft && HasTextMeshPro;
        public bool HasAllOptional => HasNakama && HasPhoton && HasDOTween;
        
        public DependencyStatus()
        {
            CheckDependencies();
        }
        
        private void CheckDependencies()
        {
            // Get scripting defines using version-compatible API
            string defines = GetCurrentDefines();
            
            // Check by type existence (more reliable)
            HasNakama = TypeExists("Nakama.Client");
            HasPhoton = TypeExists("Photon.Pun.PhotonNetwork");
            HasDOTween = TypeExists("DG.Tweening.DOTween");
            HasNewtonsoft = TypeExists("Newtonsoft.Json.JsonConvert");
            HasTextMeshPro = TypeExists("TMPro.TextMeshProUGUI");
            HasAppodeal = TypeExists("AppodealStack.Monetization.Common.Appodeal");
            HasLevelPlay = TypeExists("Unity.Services.LevelPlay.LevelPlay");
            
            // Also check for assembly definition symbols
            if (!HasNakama) HasNakama = defines.Contains("INTELLIVERSEX_HAS_NAKAMA");
            if (!HasPhoton) HasPhoton = defines.Contains("INTELLIVERSEX_HAS_PHOTON");
            if (!HasDOTween) HasDOTween = defines.Contains("INTELLIVERSEX_HAS_DOTWEEN");
        }
        
        private string GetCurrentDefines()
        {
#if UNITY_2021_2_OR_NEWER
            try
            {
                var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                return PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            }
            catch
            {
                return string.Empty;
            }
#else
            #pragma warning disable CS0618
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            #pragma warning restore CS0618
#endif
        }
        
        private bool TypeExists(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.GetType(fullTypeName) != null)
                        return true;
                }
                catch
                {
                    // Ignore assembly load errors
                }
            }
            return false;
        }
        
        public string GetStatusMessage()
        {
            var lines = new List<string>
            {
                "=== Required Dependencies ===",
                $"  Newtonsoft.Json: {(HasNewtonsoft ? "Installed" : "MISSING")}",
                $"  TextMeshPro: {(HasTextMeshPro ? "Installed" : "MISSING")}",
                "",
                "=== Backend Features (Nakama) ===",
                $"  Nakama SDK: {(HasNakama ? "Installed - Backend features enabled!" : "Not Installed")}",
                "",
                "=== Multiplayer (Photon PUN2) ===",
                $"  Photon: {(HasPhoton ? "Installed" : "Not Installed")}",
                "",
                "=== Animation (DOTween) ===",
                $"  DOTween: {(HasDOTween ? "Installed" : "Not Installed")}",
                "",
                "=== Ad Mediation (Optional) ===",
                $"  Appodeal: {(HasAppodeal ? "Installed" : "Not Installed")}",
                $"  LevelPlay: {(HasLevelPlay ? "Installed" : "Not Installed")}",
                "",
                "============================================"
            };
            
            if (!HasAllRequired)
            {
                lines.Add("");
                lines.Add("WARNING: Required dependencies missing!");
                lines.Add("Install via Package Manager:");
                lines.Add("  - com.unity.nuget.newtonsoft-json");
                lines.Add("  - com.unity.textmeshpro");
            }
            else if (!HasNakama)
            {
                lines.Add("");
                lines.Add("NOTE: Install Nakama SDK for full functionality:");
                lines.Add("  - Authentication, Leaderboards, Social");
                lines.Add("  - Backend services, User profiles");
                lines.Add("");
                lines.Add("Get Nakama: https://heroiclabs.com/nakama/");
            }
            else
            {
                lines.Add("");
                lines.Add("All core dependencies installed!");
                lines.Add("Full SDK functionality is available.");
            }
            
            return string.Join("\n", lines);
        }
    }
    
    /// <summary>
    /// About window for the SDK.
    /// </summary>
    internal class IVXAboutWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = GetWindow<IVXAboutWindow>(true, "About IntelliVerseX SDK", true);
            window.minSize = new Vector2(400, 350);
            window.maxSize = new Vector2(400, 350);
        }
        
        private void OnGUI()
        {
            GUILayout.Space(20);
            
            // Title
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label("IntelliVerseX SDK", titleStyle);
            
            GUILayout.Space(10);
            
            // Version
            var versionStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            GUILayout.Label("Version 5.0.0", versionStyle);
            
            GUILayout.Space(20);
            
            // Description
            EditorGUILayout.HelpBox(
                "A comprehensive Unity SDK for building games with backend services, " +
                "authentication, analytics, monetization, and multiplayer networking.\n\n" +
                "Install optional dependencies (Nakama, Photon, DOTween) to unlock " +
                "additional features. Use IntelliVerseX > SDK Status to check what's available.",
                MessageType.Info
            );
            
            GUILayout.Space(20);
            
            // Links
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Documentation", GUILayout.Width(120)))
            {
                Application.OpenURL("https://intelliversex.github.io/intelliversex-unity-sdk/");
            }
            
            if (GUILayout.Button("GitHub", GUILayout.Width(120)))
            {
                Application.OpenURL("https://github.com/IntelliVerseX/intelliversex-unity-sdk");
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Check Dependencies", GUILayout.Width(150)))
            {
                var status = new DependencyStatus();
                EditorUtility.DisplayDialog(
                    "IntelliVerseX SDK - Dependency Status",
                    status.GetStatusMessage(),
                    "OK"
                );
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            
            // Copyright
            var copyrightStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label("2024-2025 IntelliVerseX. All rights reserved.", copyrightStyle);
            
            GUILayout.Space(10);
        }
    }
}
