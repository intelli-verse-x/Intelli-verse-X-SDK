// IVXProjectSetup.cs
// Production-ready project setup and migration tool for IntelliVerseX SDK
// Validates and applies required project settings, Tags, Layers, Input, etc.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Comprehensive project setup and validation tool for IntelliVerseX SDK.
    /// Handles Tags, Layers, Input settings, Scripting Define Symbols, and more.
    /// </summary>
    public class IVXProjectSetup : EditorWindow
    {
        #region Constants
        
        private const string SDK_VERSION = "4.0.0";
        private const string PREFS_SETUP_COMPLETE = "IVX_ProjectSetupComplete";
        private const string PREFS_SETUP_VERSION = "IVX_ProjectSetupVersion";
        
        // Required Tags
        private static readonly string[] REQUIRED_TAGS = new string[]
        {
            "IVX_Manager",
            "IVX_UI",
            "IVX_Audio"
        };
        
        // Required Layers (layer index, name)
        private static readonly Dictionary<int, string> REQUIRED_LAYERS = new Dictionary<int, string>
        {
            { 8, "IVX_UI" },
            { 9, "IVX_Popup" },
            { 10, "IVX_Effects" }
        };
        
        // Required Scripting Define Symbols
        private static readonly string[] REQUIRED_DEFINES = new string[]
        {
            "INTELLIVERSEX_SDK"
        };
        
        // Optional defines based on detected packages
        private static readonly Dictionary<string, string> OPTIONAL_DEFINES = new Dictionary<string, string>
        {
            { "Nakama.IClient, Nakama", "IVX_NAKAMA" },
            { "Photon.Pun.PhotonNetwork, PhotonUnityNetworking", "IVX_PHOTON" },
            { "DG.Tweening.DOTween, DOTween", "IVX_DOTWEEN" },
            { "Unity.Services.LevelPlay.IronSource, com.unity.services.levelplay.runtime", "IVX_LEVELPLAY" },
            { "UnityEngine.Purchasing.IStoreController, UnityEngine.Purchasing", "IVX_IAP" }
        };
        
        #endregion
        
        #region Validation Results
        
        public class ValidationResult
        {
            public string Name;
            public bool Passed;
            public string Message;
            public Action FixAction;
            public bool IsWarning;
        }
        
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private Vector2 _scrollPosition;
        private bool _isValidating = false;
        private bool _setupComplete = false;
        
        #endregion
        
        #region Menu Items
        
        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Project Setup && Validation", priority = 10)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Platform Validation tab
        
        // [MenuItem("IntelliVerse-X SDK/Quick Validate", priority = 11)]
        public static void QuickValidate()
        {
            var results = RunValidation();
            int passed = results.Count(r => r.Passed);
            int failed = results.Count(r => !r.Passed && !r.IsWarning);
            int warnings = results.Count(r => !r.Passed && r.IsWarning);
            
            if (failed == 0 && warnings == 0)
            {
                Debug.Log($"[IVX] ✅ Project validation passed! All {passed} checks OK.");
            }
            else
            {
                Debug.LogWarning($"[IVX] Project validation: {passed} passed, {failed} failed, {warnings} warnings. Run IntelliVerseX > Project Setup for details.");
            }
        }
        
        #endregion
        
        #region Unity Callbacks
        
        private void OnEnable()
        {
            RunValidationAsync();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            GUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawValidationSection();
            DrawActionsSection();
            
            EditorGUILayout.EndScrollView();
            
            DrawFooter();
        }
        
        #endregion
        
        #region UI Drawing
        
        private void DrawHeader()
        {
            GUILayout.Space(10);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("IntelliVerseX SDK - Project Setup", headerStyle);
            EditorGUILayout.LabelField($"Version {SDK_VERSION}", EditorStyles.centeredGreyMiniLabel);
            
            GUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "This tool validates your project configuration and applies required settings for the IntelliVerseX SDK.\n" +
                "Run this after installing the SDK to ensure everything is configured correctly.",
                MessageType.Info
            );
        }
        
        private void DrawValidationSection()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            if (_isValidating)
            {
                EditorGUILayout.HelpBox("Validating...", MessageType.None);
                return;
            }
            
            if (_validationResults.Count == 0)
            {
                EditorGUILayout.HelpBox("Click 'Run Validation' to check your project.", MessageType.Info);
                return;
            }
            
            // Summary
            int passed = _validationResults.Count(r => r.Passed);
            int failed = _validationResults.Count(r => !r.Passed && !r.IsWarning);
            int warnings = _validationResults.Count(r => !r.Passed && r.IsWarning);
            
            var summaryStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            Color summaryColor = failed > 0 ? new Color(1f, 0.8f, 0.8f) : 
                                 warnings > 0 ? new Color(1f, 1f, 0.8f) : 
                                 new Color(0.8f, 1f, 0.8f);
            
            GUI.backgroundColor = summaryColor;
            EditorGUILayout.BeginVertical(summaryStyle);
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.LabelField($"✓ Passed: {passed}  |  ✗ Failed: {failed}  |  ⚠ Warnings: {warnings}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Individual results
            foreach (var result in _validationResults)
            {
                DrawValidationResult(result);
            }
        }
        
        private void DrawValidationResult(ValidationResult result)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // Status icon
            string icon = result.Passed ? "✓" : (result.IsWarning ? "⚠" : "✗");
            Color iconColor = result.Passed ? Color.green : (result.IsWarning ? Color.yellow : Color.red);
            
            var iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            
            GUI.color = iconColor;
            GUILayout.Label(icon, iconStyle, GUILayout.Width(25));
            GUI.color = Color.white;
            
            // Name and message
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(result.Name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            
            // Fix button
            if (!result.Passed && result.FixAction != null)
            {
                if (GUILayout.Button("Fix", GUILayout.Width(50)))
                {
                    result.FixAction();
                    RunValidationAsync();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawActionsSection()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Run Validation", GUILayout.Height(35)))
            {
                RunValidationAsync();
            }
            
            GUI.enabled = _validationResults.Any(r => !r.Passed && r.FixAction != null);
            if (GUILayout.Button("Fix All Issues", GUILayout.Height(35)))
            {
                FixAllIssues();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply All Required Settings", GUILayout.Height(30)))
            {
                ApplyAllRequiredSettings();
            }
            
            if (GUILayout.Button("Open Setup Wizard", GUILayout.Height(30)))
            {
                IVXSetupWizard.ShowWizard();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Documentation", GUILayout.Height(25)))
            {
                Application.OpenURL("https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK#readme");
            }
            
            if (GUILayout.Button("Report Issue", GUILayout.Height(25)))
            {
                Application.OpenURL("https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues");
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            EditorGUILayout.LabelField("IntelliVerseX SDK © 2024-2026", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
        }
        
        #endregion
        
        #region Validation Logic
        
        private void RunValidationAsync()
        {
            _isValidating = true;
            _validationResults.Clear();
            Repaint();
            
            EditorApplication.delayCall += () =>
            {
                _validationResults = RunValidation();
                _isValidating = false;
                Repaint();
            };
        }
        
        public static List<ValidationResult> RunValidation()
        {
            var results = new List<ValidationResult>();
            
            // 1. Check Unity version
            results.Add(ValidateUnityVersion());
            
            // 2. Check required dependencies
            results.Add(ValidateDependency("Newtonsoft.Json", "Newtonsoft.Json.JsonConvert, Newtonsoft.Json", 
                "Required for JSON serialization", "com.unity.nuget.newtonsoft-json"));
            results.Add(ValidateDependency("TextMeshPro", "TMPro.TextMeshProUGUI, Unity.TextMeshPro", 
                "Required for UI text", "com.unity.textmeshpro"));
            
            // 3. Check external dependencies (warnings only)
            results.Add(ValidateExternalDependency("Nakama SDK", "Nakama.IClient, Nakama", 
                "Required for backend services"));
            results.Add(ValidateExternalDependency("DOTween", "DG.Tweening.DOTween, DOTween", 
                "Required for UI animations"));
            
            // 4. Check Tags
            results.AddRange(ValidateTags());
            
            // 5. Check Layers
            results.AddRange(ValidateLayers());
            
            // 6. Check Scripting Define Symbols
            results.Add(ValidateScriptingDefines());
            
            // 7. Check Color Space
            results.Add(ValidateColorSpace());
            
            // 8. Check API Compatibility
            results.Add(ValidateApiCompatibility());
            
            return results;
        }
        
        private static ValidationResult ValidateUnityVersion()
        {
            var version = Application.unityVersion;
            var parts = version.Split('.');
            
            bool isValid = false;
            string message;
            
            if (int.TryParse(parts[0], out int major))
            {
                // Unity 2023.3+ or Unity 6000+
                if (major >= 6000 || (major == 2023 && parts.Length > 1 && int.TryParse(parts[1], out int minor) && minor >= 3))
                {
                    isValid = true;
                    message = $"Unity {version} is supported";
                }
                else if (major >= 2021)
                {
                    isValid = true;
                    message = $"Unity {version} - Minimum is 2023.3, but {version} may work";
                }
                else
                {
                    message = $"Unity {version} is not supported. Minimum: 2023.3";
                }
            }
            else
            {
                message = $"Could not parse Unity version: {version}";
            }
            
            return new ValidationResult
            {
                Name = "Unity Version",
                Passed = isValid,
                Message = message,
                IsWarning = !isValid && parts[0].StartsWith("202")
            };
        }
        
        private static ValidationResult ValidateDependency(string name, string typeName, string description, string packageId)
        {
            bool installed = CheckType(typeName);
            
            return new ValidationResult
            {
                Name = name,
                Passed = installed,
                Message = installed ? $"{name} is installed" : $"{name} is missing. {description}",
                FixAction = installed ? null : () => InstallPackage(packageId)
            };
        }
        
        private static ValidationResult ValidateExternalDependency(string name, string typeName, string description)
        {
            bool installed = CheckType(typeName);
            
            return new ValidationResult
            {
                Name = name,
                Passed = installed,
                Message = installed ? $"{name} is installed" : $"{name} not found. {description}",
                IsWarning = !installed // External deps are warnings, not errors
            };
        }
        
        private static List<ValidationResult> ValidateTags()
        {
            var results = new List<ValidationResult>();
            var existingTags = UnityEditorInternal.InternalEditorUtility.tags;
            
            foreach (var tag in REQUIRED_TAGS)
            {
                bool exists = existingTags.Contains(tag);
                results.Add(new ValidationResult
                {
                    Name = $"Tag: {tag}",
                    Passed = exists,
                    Message = exists ? "Tag exists" : "Tag is missing",
                    FixAction = exists ? null : () => AddTag(tag)
                });
            }
            
            return results;
        }
        
        private static List<ValidationResult> ValidateLayers()
        {
            var results = new List<ValidationResult>();
            
            foreach (var kvp in REQUIRED_LAYERS)
            {
                string layerName = LayerMask.LayerToName(kvp.Key);
                bool exists = layerName == kvp.Value;
                bool available = string.IsNullOrEmpty(layerName) || layerName == kvp.Value;
                
                results.Add(new ValidationResult
                {
                    Name = $"Layer {kvp.Key}: {kvp.Value}",
                    Passed = exists,
                    Message = exists ? "Layer configured correctly" : 
                              available ? "Layer slot available, needs configuration" :
                              $"Layer slot occupied by '{layerName}'",
                    FixAction = exists ? null : (available ? () => SetLayer(kvp.Key, kvp.Value) : null),
                    IsWarning = !exists && !available
                });
            }
            
            return results;
        }
        
        private static ValidationResult ValidateScriptingDefines()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            var defineList = defines.Split(';').ToList();
            
            var missing = REQUIRED_DEFINES.Where(d => !defineList.Contains(d)).ToList();
            
            return new ValidationResult
            {
                Name = "Scripting Define Symbols",
                Passed = missing.Count == 0,
                Message = missing.Count == 0 ? "All required defines present" : 
                          $"Missing: {string.Join(", ", missing)}",
                FixAction = missing.Count == 0 ? null : () => AddScriptingDefines(missing)
            };
        }
        
        private static ValidationResult ValidateColorSpace()
        {
            bool isLinear = PlayerSettings.colorSpace == ColorSpace.Linear;
            
            return new ValidationResult
            {
                Name = "Color Space",
                Passed = true, // Not a hard requirement
                Message = isLinear ? "Linear (recommended)" : "Gamma (Linear recommended for URP)",
                IsWarning = !isLinear
            };
        }
        
        private static ValidationResult ValidateApiCompatibility()
        {
            var api = PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            // Check for .NET Standard 2.1 or .NET Framework (acceptable for most use cases)
            // The specific enum values vary by Unity version, so we check by name
            string apiName = api.ToString();
            bool isNet = apiName.Contains("Standard") || apiName.Contains("Framework") || apiName.Contains("NET");
            
            return new ValidationResult
            {
                Name = "API Compatibility Level",
                Passed = isNet,
                Message = $"Current: {api}",
                IsWarning = !isNet
            };
        }
        
        #endregion
        
        #region Fix Actions
        
        private void FixAllIssues()
        {
            foreach (var result in _validationResults.Where(r => !r.Passed && r.FixAction != null))
            {
                try
                {
                    result.FixAction();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[IVX] Failed to fix '{result.Name}': {e.Message}");
                }
            }
            
            AssetDatabase.SaveAssets();
            RunValidationAsync();
        }
        
        private void ApplyAllRequiredSettings()
        {
            try
            {
                // Add all required tags
                foreach (var tag in REQUIRED_TAGS)
                {
                    AddTag(tag);
                }
                
                // Add all required layers
                foreach (var kvp in REQUIRED_LAYERS)
                {
                    SetLayer(kvp.Key, kvp.Value);
                }
                
                // Add scripting defines
                AddScriptingDefines(REQUIRED_DEFINES.ToList());
                
                // Add optional defines for detected packages
                foreach (var kvp in OPTIONAL_DEFINES)
                {
                    if (CheckType(kvp.Key))
                    {
                        AddScriptingDefines(new List<string> { kvp.Value });
                    }
                }
                
                AssetDatabase.SaveAssets();
                
                // Mark setup as complete
                EditorPrefs.SetBool(PREFS_SETUP_COMPLETE, true);
                EditorPrefs.SetString(PREFS_SETUP_VERSION, SDK_VERSION);
                
                EditorUtility.DisplayDialog("Success", 
                    "All required settings have been applied!\n\nPlease restart Unity if you experience any issues.", 
                    "OK");
                
                RunValidationAsync();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to apply settings:\n{e.Message}", 
                    "OK");
                Debug.LogException(e);
            }
        }
        
        private static void AddTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            // Check if tag already exists
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }
            
            // Add new tag
            tagsProp.arraySize++;
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            
            Debug.Log($"[IVX] Added tag: {tag}");
        }
        
        private static void SetLayer(int index, string name)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            
            if (index >= 0 && index < layersProp.arraySize)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(sp.stringValue) || sp.stringValue == name)
                {
                    sp.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[IVX] Set layer {index} to: {name}");
                }
            }
        }
        
        private static void AddScriptingDefines(List<string> defines)
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            var defineList = currentDefines.Split(';').Where(d => !string.IsNullOrEmpty(d)).ToList();
            
            bool changed = false;
            foreach (var define in defines)
            {
                if (!defineList.Contains(define))
                {
                    defineList.Add(define);
                    changed = true;
                    Debug.Log($"[IVX] Added scripting define: {define}");
                }
            }
            
            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", defineList));
            }
        }
        
        private static void InstallPackage(string packageId)
        {
            UnityEditor.PackageManager.Client.Add(packageId);
            Debug.Log($"[IVX] Installing package: {packageId}");
        }
        
        private static bool CheckType(string fullTypeName)
        {
            try
            {
                return Type.GetType(fullTypeName) != null;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Auto-Setup on Import
        
        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            // Check if this is a fresh install
            bool setupComplete = EditorPrefs.GetBool(PREFS_SETUP_COMPLETE, false);
            string setupVersion = EditorPrefs.GetString(PREFS_SETUP_VERSION, "");
            
            if (!setupComplete || setupVersion != SDK_VERSION)
            {
                EditorApplication.delayCall += () =>
                {
                    // Show setup window on first import
                    if (!setupComplete)
                    {
                        if (EditorUtility.DisplayDialog("IntelliVerseX SDK",
                            "Welcome to IntelliVerseX SDK!\n\n" +
                            "Would you like to run the SDK Setup Wizard now?\n\n" +
                            "This will configure your project with the required settings.",
                            "Run Setup", "Later"))
                        {
                            // Open the unified SDK Setup Wizard
                            IVXSDKSetupWizard.ShowWindow();
                        }
                    }
                    else if (setupVersion != SDK_VERSION)
                    {
                        Debug.Log($"[IVX] SDK updated to {SDK_VERSION}. Run IntelliVerse-X SDK > SDK Setup Wizard to apply new settings.");
                    }
                };
            }
        }
        
        #endregion
    }
}
#endif
