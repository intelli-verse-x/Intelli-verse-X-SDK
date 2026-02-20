// IVXAdsPrefabBuilder.cs
// Editor tool for creating and managing Ads prefabs in IntelliVerseX Games SDK

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using IntelliVerseX.Core;

namespace IntelliVerseX.Monetization.Ads.Editor
{
    /// <summary>
    /// Editor utility for creating Ads prefabs with default settings.
    /// Similar to wallet prefab builder - creates configured prefabs ready for use.
    /// </summary>
    public static class IVXAdsPrefabBuilder
    {
        private const string PREFABS_FOLDER = "Assets/_IntelliVerseXSDK/Monetization/Ads/Prefabs";
        private const string ADS_BOOTSTRAP_PREFAB = "IVXAdsBootstrap.prefab";

        #region Menu Items

        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Ads/Create Ads Bootstrap Prefab", false, 100)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab
        public static void CreateAdsBootstrapPrefab()
        {
            EnsureFolderExists();

            string prefabPath = Path.Combine(PREFABS_FOLDER, ADS_BOOTSTRAP_PREFAB);

            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Prefab Exists",
                    $"'{ADS_BOOTSTRAP_PREFAB}' already exists. Do you want to replace it?",
                    "Replace", "Cancel"))
                {
                    return;
                }
            }

            // Create GameObject
            GameObject go = new GameObject("Ads Bootstrap");

            // Find and add IVXAdsBootstrap component
            var bootstrapType = FindType("IntelliVerseX.Monetization.Ads.IVXAdsBootstrap");
            if (bootstrapType == null)
            {
                // Fallback to simple name search
                bootstrapType = FindType("IVXAdsBootstrap");
            }

            if (bootstrapType != null)
            {
                var component = go.AddComponent(bootstrapType);

                // Try to set default config
                var configAssetField = bootstrapType.GetField("configAsset", BindingFlags.NonPublic | BindingFlags.Instance);
                if (configAssetField != null)
                {
                    var config = FindDefaultConfig();
                    if (config != null)
                    {
                        configAssetField.SetValue(component, config);
                    }
                }

                // Set default values via reflection
                SetFieldValue(component, "keyAdsControlJson", "ads_control_json");
                SetFieldValue(component, "keyAbPrimary", "ads_primary_network");
                SetFieldValue(component, "remoteConfigTimeoutSeconds", 10f);
                SetFieldValue(component, "maxConsentWaitSeconds", 8f);
                SetFieldValue(component, "forceInitWithoutConsent", true);
                SetFieldValue(component, "preloadIntervalSeconds", 30f);
            }
            else
            {
                Debug.LogError("[IVXAdsPrefabBuilder] IVXAdsBootstrap type not found! Make sure the script compiles.");
                Object.DestroyImmediate(go);
                return;
            }

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            if (prefab != null)
            {
                Debug.Log($"[IVXAdsPrefabBuilder] ✓ Created prefab: {prefabPath}");
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
            }
        }

        // [MenuItem("IntelliVerse-X SDK/Ads/Add Ads Bootstrap to Scene", false, 101)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab
        public static void AddAdsBootstrapToScene()
        {
            // Check if already exists in scene
            var existingType = FindType("IntelliVerseX.Monetization.Ads.IVXAdsBootstrap") ?? FindType("IVXAdsBootstrap");
            if (existingType != null)
            {
                var existing = Object.FindObjectOfType(existingType);
                if (existing != null)
                {
                    Debug.LogWarning("[IVXAdsPrefabBuilder] Ads Bootstrap already exists in scene!");
                    Selection.activeObject = (existing as Component)?.gameObject;
                    return;
                }
            }

            string prefabPath = Path.Combine(PREFABS_FOLDER, ADS_BOOTSTRAP_PREFAB);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                // Create prefab first
                CreateAdsBootstrapPrefab();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "Ads Bootstrap";

                // Try to auto-wire config
                var bootstrapType = FindType("IntelliVerseX.Monetization.Ads.IVXAdsBootstrap") ?? FindType("IVXAdsBootstrap");
                if (bootstrapType != null)
                {
                    var component = instance.GetComponent(bootstrapType);
                    if (component != null)
                    {
                        var configField = bootstrapType.GetField("configAsset", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (configField != null)
                        {
                            var currentConfig = configField.GetValue(component);
                            if (currentConfig == null)
                            {
                                var config = FindDefaultConfig();
                                if (config != null)
                                {
                                    configField.SetValue(component, config);
                                    EditorUtility.SetDirty(instance);
                                }
                            }
                        }
                    }
                }

                Undo.RegisterCreatedObjectUndo(instance, "Add Ads Bootstrap");
                Selection.activeGameObject = instance;

                Debug.Log("[IVXAdsPrefabBuilder] ✓ Added Ads Bootstrap to scene!");
                Debug.Log("[IVXAdsPrefabBuilder] ℹ Make sure to assign your IntelliVerseXConfig asset if not auto-detected.");
            }
            else
            {
                Debug.LogError("[IVXAdsPrefabBuilder] Failed to create/load Ads Bootstrap prefab!");
            }
        }

        // [MenuItem("IntelliVerse-X SDK/Ads/Validate Ads Setup", false, 102)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab
        public static void ValidateAdsSetup()
        {
            Debug.Log("[IVXAdsPrefabBuilder] ========== Ads Setup Validation ==========");

            bool allGood = true;

            // Check for IVXAdsBootstrap script
            string bootstrapPath = "Assets/_IntelliVerseXSDK/Monetization/Ads/Runtime/IVXAdsBootstrap.cs";
            if (!File.Exists(bootstrapPath))
            {
                Debug.LogError($"[IVXAdsPrefabBuilder] ❌ IVXAdsBootstrap.cs not found at: {bootstrapPath}");
                allGood = false;
            }
            else
            {
                Debug.Log("[IVXAdsPrefabBuilder] ✓ IVXAdsBootstrap.cs found");
            }

            // Check for IVXAdsManager script
            string managerPath = "Assets/_IntelliVerseXSDK/Monetization/Ads/Static/IVXAdsManager.cs";
            if (!File.Exists(managerPath))
            {
                Debug.LogError($"[IVXAdsPrefabBuilder] ❌ IVXAdsManager.cs not found at: {managerPath}");
                allGood = false;
            }
            else
            {
                Debug.Log("[IVXAdsPrefabBuilder] ✓ IVXAdsManager.cs found");
            }

            // Check for IVXAdsConfig
            string configPath = "Assets/_IntelliVerseXSDK/Core/IVXAdsConfig.cs";
            if (!File.Exists(configPath))
            {
                Debug.LogError($"[IVXAdsPrefabBuilder] ❌ IVXAdsConfig.cs not found at: {configPath}");
                allGood = false;
            }
            else
            {
                Debug.Log("[IVXAdsPrefabBuilder] ✓ IVXAdsConfig.cs found");
            }

            // Check for prefab
            string prefabPath = Path.Combine(PREFABS_FOLDER, ADS_BOOTSTRAP_PREFAB);
            if (!File.Exists(prefabPath))
            {
                Debug.LogWarning($"[IVXAdsPrefabBuilder] ⚠ Ads Bootstrap prefab not found. Use 'Create Ads Bootstrap Prefab' to create it.");
            }
            else
            {
                Debug.Log("[IVXAdsPrefabBuilder] ✓ Ads Bootstrap prefab found");
            }

            // Check for IntelliVerseXConfig
            var config = FindDefaultConfig();
            if (config == null)
            {
                Debug.LogWarning("[IVXAdsPrefabBuilder] ⚠ No IntelliVerseXConfig found in Resources. Create one via Assets > Create > IntelliVerse-X > Game Configuration");
            }
            else
            {
                Debug.Log($"[IVXAdsPrefabBuilder] ✓ IntelliVerseXConfig found: {config.name}");

                if (!config.enableAds)
                {
                    Debug.LogWarning("[IVXAdsPrefabBuilder] ⚠ Ads are disabled in config!");
                }
            }

            // Check scene for bootstrap
            var bootstrapType = FindType("IntelliVerseX.Monetization.Ads.IVXAdsBootstrap") ?? FindType("IVXAdsBootstrap");
            if (bootstrapType != null)
            {
                var existing = Object.FindObjectOfType(bootstrapType);
                if (existing == null)
                {
                    Debug.LogWarning("[IVXAdsPrefabBuilder] ⚠ No Ads Bootstrap in current scene. Use 'Add Ads Bootstrap to Scene' to add one.");
                }
                else
                {
                    Debug.Log("[IVXAdsPrefabBuilder] ✓ Ads Bootstrap found in scene");
                }
            }

            if (allGood)
            {
                Debug.Log("[IVXAdsPrefabBuilder] ========== All core files present! ==========");
            }
            else
            {
                Debug.LogError("[IVXAdsPrefabBuilder] ========== Some files are missing! ==========");
            }
        }

        #endregion

        #region Helper Methods

        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(PREFABS_FOLDER))
            {
                // Create folder hierarchy
                string parentFolder = "Assets/_IntelliVerseXSDK/Monetization/Ads";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_IntelliVerseXSDK/Monetization", "Ads");
                }
                AssetDatabase.CreateFolder(parentFolder, "Prefabs");
                AssetDatabase.Refresh();
            }
        }

        private static IntelliVerseXConfig FindDefaultConfig()
        {
            // Search in Resources
            var configs = Resources.LoadAll<IntelliVerseXConfig>("");
            if (configs.Length > 0)
            {
                return configs[0];
            }

            // Search in project
            string[] guids = AssetDatabase.FindAssets("t:IntelliVerseXConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<IntelliVerseXConfig>(path);
                if (config != null)
                {
                    return config;
                }
            }

            return null;
        }

        private static void SetFieldValue(object component, string fieldName, object value)
        {
            var type = component.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(component, value);
            }
        }

        private static System.Type FindType(string typeName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName);
                    if (type != null) return type;

                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.Name == typeName || t.FullName == typeName)
                            return t;
                    }
                }
                catch
                {
                    // Ignore assembly loading errors
                }
            }
            return null;
        }

        #endregion

        #region Wizard Integration

        /// <summary>
        /// Called from IVXSDKSetupWizard to create ads prefabs
        /// </summary>
        public static void CreateAllPrefabs()
        {
            CreateAdsBootstrapPrefab();
        }

        /// <summary>
        /// Called from IVXSDKSetupWizard to add ads to scene
        /// </summary>
        public static void AddToScene()
        {
            AddAdsBootstrapToScene();
        }

        /// <summary>
        /// Check if ads module is properly set up
        /// </summary>
        public static bool IsSetupComplete()
        {
            // Check for bootstrap script
            if (!File.Exists("Assets/_IntelliVerseXSDK/Monetization/Ads/Runtime/IVXAdsBootstrap.cs"))
                return false;

            // Check for manager
            if (!File.Exists("Assets/_IntelliVerseXSDK/Monetization/Ads/Static/IVXAdsManager.cs"))
                return false;

            // Check for config
            if (!File.Exists("Assets/_IntelliVerseXSDK/Core/IVXAdsConfig.cs"))
                return false;

            return true;
        }

        #endregion
    }
}
#endif
