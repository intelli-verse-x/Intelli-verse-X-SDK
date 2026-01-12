// IVXDependencyChecker.cs
// Automated dependency checker for IntelliVerseX SDK
// Validates that all required dependencies are installed before SDK can be used

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Automated dependency checker for IntelliVerseX SDK.
    /// Validates Nakama and Newtonsoft.Json installation.
    /// </summary>
    public class IVXDependencyChecker : EditorWindow
    {
        private const string NEWTONSOFT_TYPE = "Newtonsoft.Json.JsonConvert, Newtonsoft.Json";
        private const string NAKAMA_TYPE = "Nakama.IClient, Nakama";
        private const string NAKAMA_FOLDER_PATH = "Assets/Packages/Nakama";
        private const string NAKAMA_ALTERNATIVE_PATH = "Assets/Nakama";
        
        private static bool? _newtonsoftInstalled;
        private static bool? _nakamaInstalled;
        private static bool? _nakamaFolderExists;
        
        [MenuItem("IntelliVerseX/Check Dependencies", priority = 1)]
        public static void CheckDependencies()
        {
            ShowWindow();
        }
        
        [MenuItem("IntelliVerseX/Check Dependencies (Console)", priority = 2)]
        public static void CheckDependenciesConsole()
        {
            Debug.Log("========================================");
            Debug.Log("IntelliVerseX SDK - Dependency Check");
            Debug.Log("========================================");
            
            bool allGood = true;
            
            // Check Newtonsoft.Json
            bool hasNewtonsoft = CheckType(NEWTONSOFT_TYPE);
            LogResult("Newtonsoft.Json", hasNewtonsoft, 
                "✅ Newtonsoft.Json package is installed correctly",
                "❌ Newtonsoft.Json NOT FOUND - Install via Package Manager: com.unity.nuget.newtonsoft-json");
            allGood &= hasNewtonsoft;
            
            // Check Nakama
            bool hasNakama = CheckType(NAKAMA_TYPE);
            LogResult("Nakama SDK", hasNakama,
                "✅ Nakama SDK is installed correctly",
                "❌ Nakama SDK NOT FOUND - Download from: https://github.com/heroiclabs/nakama-unity/releases");
            allGood &= hasNakama;
            
            // Check Nakama folder
            bool hasNakamaFolder = Directory.Exists(NAKAMA_FOLDER_PATH) || Directory.Exists(NAKAMA_ALTERNATIVE_PATH);
            LogResult("Nakama Folder", hasNakamaFolder,
                $"✅ Nakama folder found at: {(Directory.Exists(NAKAMA_FOLDER_PATH) ? NAKAMA_FOLDER_PATH : NAKAMA_ALTERNATIVE_PATH)}",
                "❌ Nakama folder NOT FOUND - Expected at: Assets/Packages/Nakama or Assets/Nakama");
            allGood &= hasNakamaFolder;
            
            Debug.Log("========================================");
            if (allGood)
            {
                Debug.Log("✅ ALL DEPENDENCIES INSTALLED CORRECTLY!");
                Debug.Log("✅ IntelliVerseX SDK is ready to use.");
            }
            else
            {
                Debug.LogError("❌ MISSING DEPENDENCIES!");
                Debug.LogError("❌ Please install missing dependencies before using the SDK.");
                Debug.LogError("📖 See: Assets/_IntelliVerseXSDK/DEPENDENCIES.md for installation guide");
            }
            Debug.Log("========================================");
        }
        
        private static void ShowWindow()
        {
            var window = GetWindow<IVXDependencyChecker>("SDK Dependencies");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            // Reset cache when window opens
            _newtonsoftInstalled = null;
            _nakamaInstalled = null;
            _nakamaFolderExists = null;
        }
        
        private void OnGUI()
        {
            GUILayout.Space(10);
            
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("IntelliVerseX SDK - Dependency Checker", headerStyle);
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This tool checks if all required dependencies are installed for the IntelliVerseX SDK.",
                MessageType.Info
            );
            
            GUILayout.Space(20);
            
            // Check button
            if (GUILayout.Button("Check Dependencies", GUILayout.Height(40)))
            {
                CheckAllDependencies();
            }
            
            GUILayout.Space(20);
            
            // Results
            if (_newtonsoftInstalled.HasValue)
            {
                DrawDependencyStatus("Newtonsoft.Json", _newtonsoftInstalled.Value,
                    "JSON serialization library",
                    "Install via Package Manager: com.unity.nuget.newtonsoft-json");
                
                GUILayout.Space(10);
                
                DrawDependencyStatus("Nakama SDK", _nakamaInstalled.Value,
                    "Backend services (authentication, leaderboards, storage)",
                    "Download from: https://github.com/heroiclabs/nakama-unity/releases");
                
                GUILayout.Space(10);
                
                DrawDependencyStatus("Nakama Folder", _nakamaFolderExists.Value,
                    "Nakama SDK files in project",
                    "Expected at: Assets/Packages/Nakama or Assets/Nakama");
                
                GUILayout.Space(20);
                
                // Overall status
                bool allGood = _newtonsoftInstalled.Value && _nakamaInstalled.Value && _nakamaFolderExists.Value;
                
                if (allGood)
                {
                    EditorGUILayout.HelpBox(
                        "✅ ALL DEPENDENCIES INSTALLED CORRECTLY!\n\nIntelliVerseX SDK is ready to use.",
                        MessageType.Info
                    );
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "❌ MISSING DEPENDENCIES!\n\nPlease install missing dependencies before using the SDK.\n\nSee DEPENDENCIES.md for detailed installation guide.",
                        MessageType.Error
                    );
                    
                    GUILayout.Space(10);
                    
                    if (GUILayout.Button("Open DEPENDENCIES.md", GUILayout.Height(30)))
                    {
                        string docPath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK/DEPENDENCIES.md");
                        if (File.Exists(docPath))
                        {
                            System.Diagnostics.Process.Start(docPath);
                        }
                        else
                        {
                            Debug.LogError("DEPENDENCIES.md not found at: " + docPath);
                        }
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            
            // Footer
            GUILayout.Space(10);
            EditorGUILayout.LabelField("IntelliVerseX SDK v1.0.0", EditorStyles.centeredGreyMiniLabel);
        }
        
        private void CheckAllDependencies()
        {
            _newtonsoftInstalled = CheckType(NEWTONSOFT_TYPE);
            _nakamaInstalled = CheckType(NAKAMA_TYPE);
            _nakamaFolderExists = Directory.Exists(NAKAMA_FOLDER_PATH) || Directory.Exists(NAKAMA_ALTERNATIVE_PATH);
            
            Repaint();
        }
        
        private void DrawDependencyStatus(string name, bool installed, string description, string installHint)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            
            Color originalColor = GUI.color;
            GUI.color = installed ? Color.green : Color.red;
            GUILayout.Label(installed ? "✓" : "✗", iconStyle, GUILayout.Width(30));
            GUI.color = originalColor;
            
            // Name and description
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.miniLabel);
            
            if (!installed)
            {
                EditorGUILayout.LabelField(installHint, EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        public static bool CheckType(string fullTypeName)
        {
            try
            {
                var type = System.Type.GetType(fullTypeName);
                return type != null;
            }
            catch
            {
                return false;
            }
        }
        
        private static void LogResult(string name, bool exists, string successMsg, string failMsg)
        {
            if (exists)
            {
                Debug.Log(successMsg);
            }
            else
            {
                Debug.LogError(failMsg);
            }
        }
    }
    
    /// <summary>
    /// Automatic dependency check on SDK import.
    /// Shows warning if dependencies are missing.
    /// </summary>
    [InitializeOnLoad]
    public static class IVXDependencyAutoCheck
    {
        private const string PREF_KEY = "IVXDependencyCheck_LastCheck";
        private const int CHECK_INTERVAL_DAYS = 1;
        
        static IVXDependencyAutoCheck()
        {
            // Check if we should run auto-check
            string lastCheckStr = EditorPrefs.GetString(PREF_KEY, "");
            bool shouldCheck = true;
            
            if (!string.IsNullOrEmpty(lastCheckStr))
            {
                if (System.DateTime.TryParse(lastCheckStr, out System.DateTime lastCheck))
                {
                    shouldCheck = (System.DateTime.Now - lastCheck).TotalDays >= CHECK_INTERVAL_DAYS;
                }
            }
            
            if (shouldCheck)
            {
                EditorApplication.delayCall += () =>
                {
                    CheckDependenciesOnStartup();
                    EditorPrefs.SetString(PREF_KEY, System.DateTime.Now.ToString());
                };
            }
        }
        
        private static void CheckDependenciesOnStartup()
        {
            bool hasNewtonsoft = IVXDependencyChecker.CheckType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
            bool hasNakama = IVXDependencyChecker.CheckType("Nakama.IClient, Nakama");
            
            if (!hasNewtonsoft || !hasNakama)
            {
                Debug.LogWarning("========================================");
                Debug.LogWarning("[IntelliVerseX SDK] Missing Dependencies Detected!");
                Debug.LogWarning("========================================");
                
                if (!hasNewtonsoft)
                {
                    Debug.LogWarning("❌ Newtonsoft.Json is NOT installed");
                }
                
                if (!hasNakama)
                {
                    Debug.LogWarning("❌ Nakama SDK is NOT installed");
                }
                
                Debug.LogWarning("📖 Run: IntelliVerseX > Check Dependencies for details");
                Debug.LogWarning("📖 See: Assets/_IntelliVerseXSDK/DEPENDENCIES.md for installation guide");
                Debug.LogWarning("========================================");
            }
        }
    }
}
#endif
