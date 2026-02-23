// IVXAdsPrefabBuilder.cs
// Editor tool for creating and managing Ads prefabs in IntelliVerseX Games SDK

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        private const string SCENES_FOLDER = "Assets/Scenes/Tests";
        
        private static readonly Color PrimaryButtonColor = new Color(0.2f, 0.5f, 0.9f, 1f);
        private static readonly Color SecondaryButtonColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color TextColor = Color.white;

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

        #region Scene Builder

        [MenuItem("IntelliVerse-X SDK/Ads/Create Ads Test Scene", false, 200)]
        public static void CreateAdsTestScene()
        {
            // Ensure scenes folder exists
            if (!AssetDatabase.IsValidFolder(SCENES_FOLDER))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                AssetDatabase.CreateFolder("Assets/Scenes", "Tests");
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Clear default objects except camera
            var defaultLight = GameObject.Find("Directional Light");
            if (defaultLight != null) Object.DestroyImmediate(defaultLight);
            
            // Setup camera
            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }
            
            // Create EventSystem if not exists
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                
                // Try to add InputSystemUIInputModule for new Input System, fallback to StandaloneInputModule
                var inputModuleType = FindType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (inputModuleType != null)
                {
                    eventSystemGO.AddComponent(inputModuleType);
                }
                else
                {
                    eventSystemGO.AddComponent<StandaloneInputModule>();
                }
            }
            
            // Create Ads Bootstrap
            CreateAdsBootstrapInScene();
            
            // Create UI Canvas
            CreateAdsTestUI();
            
            // Save scene
            string scenePath = $"{SCENES_FOLDER}/IVX_AdsTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[IVXAdsPrefabBuilder] ✓ Created Ads Test Scene at: {scenePath}");
            
            // Add to build settings
            AddSceneToBuildSettings(scenePath);
        }

        [MenuItem("IntelliVerse-X SDK/Ads/Rebuild Ads Test UI", false, 201)]
        public static void RebuildAdsTestUI()
        {
            // Find and destroy existing UI
            var existingCanvas = GameObject.Find("IVX_AdsCanvas");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
            }
            
            CreateAdsTestUI();
            
            Debug.Log("[IVXAdsPrefabBuilder] ✓ Rebuilt Ads Test UI");
        }
        
        [MenuItem("IntelliVerse-X SDK/Ads/Save Ads UI as Prefab", false, 202)]
        public static void SaveAdsUIAsPrefab()
        {
            var existingCanvas = GameObject.Find("IVX_AdsCanvas");
            if (existingCanvas == null)
            {
                Debug.LogError("[IVXAdsPrefabBuilder] IVX_AdsCanvas not found in scene!");
                return;
            }
            
            EnsureFolderExists();
            string prefabPath = Path.Combine(PREFABS_FOLDER, "IVXAdsTestUI.prefab");
            
            // Save as prefab (overwrite if exists)
            var prefab = PrefabUtility.SaveAsPrefabAsset(existingCanvas, prefabPath);
            if (prefab != null)
            {
                Debug.Log($"[IVXAdsPrefabBuilder] ✓ Created UI prefab: {prefabPath}");
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
            }
        }
        
        [MenuItem("IntelliVerse-X SDK/Ads/Create All Ads Prefabs", false, 203)]
        public static void CreateAllAdsPrefabs()
        {
            EnsureFolderExists();
            
            // Create bootstrap prefab
            CreateAdsBootstrapPrefab();
            
            // Create UI if not exists
            var existingCanvas = GameObject.Find("IVX_AdsCanvas");
            if (existingCanvas == null)
            {
                CreateAdsTestUI();
                existingCanvas = GameObject.Find("IVX_AdsCanvas");
            }
            
            // Save UI as prefab
            if (existingCanvas != null)
            {
                string prefabPath = Path.Combine(PREFABS_FOLDER, "IVXAdsTestUI.prefab");
                PrefabUtility.SaveAsPrefabAsset(existingCanvas, prefabPath);
                Debug.Log($"[IVXAdsPrefabBuilder] ✓ Created UI prefab: {prefabPath}");
            }
            
            Debug.Log("[IVXAdsPrefabBuilder] ✓ All ads prefabs created!");
        }

        private static void CreateAdsBootstrapInScene()
        {
            // Check if already exists
            var bootstrapType = FindType("IntelliVerseX.Monetization.Ads.IVXAdsBootstrap") ?? FindType("IVXAdsBootstrap");
            if (bootstrapType != null)
            {
                var existing = Object.FindFirstObjectByType(bootstrapType) as Component;
                if (existing != null)
                {
                    Debug.Log("[IVXAdsPrefabBuilder] Ads Bootstrap already exists in scene");
                    return;
                }
            }
            
            // Create new GameObject
            var go = new GameObject("IVX_AdsBootstrap");
            
            // Add IVXAdsBootstrap
            if (bootstrapType != null)
            {
                var component = go.AddComponent(bootstrapType);
                
                // Set config
                var configField = bootstrapType.GetField("configAsset", BindingFlags.NonPublic | BindingFlags.Instance);
                if (configField != null)
                {
                    var config = FindDefaultConfig();
                    if (config != null)
                    {
                        configField.SetValue(component, config);
                    }
                }
                
                // Set default values
                SetFieldValue(component, "forceInitWithoutConsent", true);
                SetFieldValue(component, "preloadIntervalSeconds", 30f);
            }
            
            // Add test controller
            var testControllerType = FindType("IntelliVerseX.Examples.IVXAdsTestController") ?? FindType("IVXAdsTestController");
            if (testControllerType != null)
            {
                go.AddComponent(testControllerType);
            }
            
            // Save as prefab if not exists
            EnsureFolderExists();
            string prefabPath = Path.Combine(PREFABS_FOLDER, ADS_BOOTSTRAP_PREFAB);
            if (!File.Exists(prefabPath))
            {
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Debug.Log($"[IVXAdsPrefabBuilder] ✓ Created prefab: {prefabPath}");
            }
        }

        private static void CreateAdsTestUI()
        {
            // Create Canvas
            var canvasGO = new GameObject("IVX_AdsCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create main container
            var container = CreateUIElement("Container", canvasGO.transform);
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(500, 800);
            
            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 40, 40);
            
            // Add background
            var bg = container.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            bg.raycastTarget = false;
            
            // Title
            CreateText(container.transform, "Title", "IVX Ads Test", 36, true);
            
            // Network info
            CreateText(container.transform, "NetworkInfo", "Primary: IronSource\nFallback: AdMob", 18, false, new Color(0.7f, 0.7f, 0.7f));
            
            // Status text
            CreateText(container.transform, "Description", "Status: Waiting for initialization...", 16, false, new Color(0.8f, 0.8f, 0.8f));
            
            // Spacer
            var spacer = CreateUIElement("Spacer", container.transform);
            spacer.AddComponent<LayoutElement>().preferredHeight = 30;
            
            // Show Rewarded Button
            CreateButton(container.transform, "ShowRewardedAdButton", "Show Rewarded Ad", PrimaryButtonColor, 60);
            
            // Show Interstitial Button  
            CreateButton(container.transform, "ShowInterstitialButton", "Show Interstitial", PrimaryButtonColor, 60);
            
            // Spacer
            var spacer2 = CreateUIElement("Spacer2", container.transform);
            spacer2.AddComponent<LayoutElement>().preferredHeight = 20;
            
            // Banner section label
            CreateText(container.transform, "BannerLabel", "Banner Ads", 20, true);
            
            // Banner buttons row
            var bannerRow = CreateUIElement("BannerRow", container.transform);
            var hlg = bannerRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 15;
            bannerRow.AddComponent<LayoutElement>().preferredHeight = 50;
            
            // Show Banner Button
            CreateButton(bannerRow.transform, "ShowBannerButton", "Show Banner", new Color(0.2f, 0.6f, 0.3f, 1f), 50);
            
            // Hide Banner Button
            CreateButton(bannerRow.transform, "HideBannerButton", "Hide Banner", new Color(0.6f, 0.3f, 0.2f, 1f), 50);
            
            // Spacer
            var spacer3 = CreateUIElement("Spacer3", container.transform);
            spacer3.AddComponent<LayoutElement>().preferredHeight = 20;
            
            // Utility buttons
            CreateButton(container.transform, "PreloadButton", "Preload Ads", SecondaryButtonColor, 50);
            CreateButton(container.transform, "LogStatusButton", "Log Status", SecondaryButtonColor, 50);
            
            Debug.Log("[IVXAdsPrefabBuilder] ✓ Created Ads Test UI");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, bool bold, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, fontSize + 20);
            
            // Use reflection to add TextMeshProUGUI
            var tmpType = FindType("TMPro.TextMeshProUGUI");
            if (tmpType != null)
            {
                var tmp = go.AddComponent(tmpType);
                SetPropertyValue(tmp, "text", text);
                SetPropertyValue(tmp, "fontSize", (float)fontSize);
                SetPropertyValue(tmp, "fontStyle", bold ? 1 : 0); // 1 = Bold, 0 = Normal
                SetPropertyValue(tmp, "alignment", 514); // Center
                SetPropertyValue(tmp, "color", color ?? TextColor);
                SetPropertyValue(tmp, "raycastTarget", false);
            }
            else
            {
                // Fallback to standard Text
                var legacyText = go.AddComponent<Text>();
                legacyText.text = text;
                legacyText.fontSize = fontSize;
                legacyText.fontStyle = bold ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal;
                legacyText.alignment = TextAnchor.MiddleCenter;
                legacyText.color = color ?? TextColor;
                legacyText.raycastTarget = false;
            }
            
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 20;
            
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color bgColor, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);
            
            // Background image (critical for raycast!)
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;
            
            // Button component
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f),
                pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f),
                selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            
            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Use reflection to add TextMeshProUGUI
            var tmpType = FindType("TMPro.TextMeshProUGUI");
            if (tmpType != null)
            {
                var tmp = textGO.AddComponent(tmpType);
                SetPropertyValue(tmp, "text", text);
                SetPropertyValue(tmp, "fontSize", 22f);
                SetPropertyValue(tmp, "fontStyle", 1); // Bold
                SetPropertyValue(tmp, "alignment", 514); // Center
                SetPropertyValue(tmp, "color", TextColor);
                SetPropertyValue(tmp, "raycastTarget", false);
            }
            else
            {
                // Fallback to standard Text
                var legacyText = textGO.AddComponent<Text>();
                legacyText.text = text;
                legacyText.fontSize = 22;
                legacyText.fontStyle = UnityEngine.FontStyle.Bold;
                legacyText.alignment = TextAnchor.MiddleCenter;
                legacyText.color = TextColor;
                legacyText.raycastTarget = false;
            }
            
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            
            return go;
        }
        
        private static void SetPropertyValue(object component, string propertyName, object value)
        {
            var type = component.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(component, value);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            // Check if already in build settings
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    return;
                }
            }
            
            // Add to build settings
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            
            Debug.Log($"[IVXAdsPrefabBuilder] ✓ Added scene to build settings: {scenePath}");
        }

        #endregion
    }
}
#endif
