// File: IVXSceneSetup.cs
// Purpose: Scene setup utilities for SDK integration
// Version: 1.0.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Scene setup utilities for SDK integration.
    /// Provides methods for configuring scenes with SDK managers and prefabs.
    /// </summary>
    public static class IVXSceneSetup
    {
        #region Constants

        private const string SDK_ROOT = "Assets/_IntelliVerseXSDK";
        private const string INTRO_SCENE_ROOT = "Assets/IntroSceneIntelliverseX";
        private const string QUIZVERSE_ROOT = "Assets/_QuizVerse";

        #endregion

        #region Menu Items

        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Setup Current Scene (Full)", false, 100)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        
        public static void SetupCurrentSceneFull()
        {
            var scene = EditorSceneManager.GetActiveScene();
            
            if (!EditorUtility.DisplayDialog("Setup Scene",
                $"This will add all SDK managers to '{scene.name}'.\n\nContinue?",
                "Yes", "Cancel"))
                return;

            SetupScene(scene, SceneSetupOptions.Full);
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Setup Current Scene (Minimal)", false, 101)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void SetupCurrentSceneMinimal()
        {
            var scene = EditorSceneManager.GetActiveScene();
            SetupScene(scene, SceneSetupOptions.Minimal);
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Create Intro Scene", false, 150)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Test Scenes tab
        public static void CreateIntroScene()
        {
            CreateIntroSceneFromTemplate();
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Verify Scene Setup", false, 200)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void VerifyCurrentSceneSetup()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var result = VerifySceneSetup(scene);

            EditorUtility.DisplayDialog("Scene Verification",
                result.ToString(), "OK");
        }

        #endregion

        #region Setup Options

        [Flags]
        public enum SceneSetupOptions
        {
            None = 0,
            
            // Core managers
            NakamaManager = 1 << 0,
            UserData = 1 << 1,
            BackendService = 1 << 2,
            
            // Feature managers
            WalletManager = 1 << 3,
            LeaderboardManager = 1 << 4,
            AnalyticsManager = 1 << 5,
            
            // UI Components
            LoadingOverlay = 1 << 6,
            
            // Presets
            Minimal = NakamaManager | UserData,
            Core = Minimal | BackendService,
            Full = Core | WalletManager | LeaderboardManager | AnalyticsManager | LoadingOverlay
        }

        #endregion

        #region Scene Setup

        /// <summary>
        /// Sets up a scene with SDK components based on options
        /// </summary>
        public static void SetupScene(Scene scene, SceneSetupOptions options)
        {
            if (!scene.IsValid())
            {
                Debug.LogError("[IVXSceneSetup] Invalid scene");
                return;
            }

            Debug.Log($"[IVXSceneSetup] Setting up scene: {scene.name} with options: {options}");

            // Create organization root
            var managersRoot = GetOrCreateGameObject("--- SDK Managers ---", scene);
            var uiRoot = GetOrCreateGameObject("--- SDK UI ---", scene);

            // Setup managers based on options
            if (options.HasFlag(SceneSetupOptions.NakamaManager))
            {
                SetupNakamaManager(managersRoot.transform);
            }

            if (options.HasFlag(SceneSetupOptions.UserData))
            {
                SetupUserData(managersRoot.transform);
            }

            if (options.HasFlag(SceneSetupOptions.BackendService))
            {
                SetupBackendService(managersRoot.transform);
            }

            if (options.HasFlag(SceneSetupOptions.LoadingOverlay))
            {
                SetupLoadingOverlay(uiRoot.transform);
            }

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[IVXSceneSetup] Scene setup complete: {scene.name}");
        }

        /// <summary>
        /// Sets up the NakamaManager in the scene
        /// </summary>
        public static void SetupNakamaManager(Transform parent)
        {
            const string name = "NakamaManager";
            
            // Check if already exists
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"[IVXSceneSetup] {name} already exists");
                return;
            }

            // Try to instantiate from prefab
            var prefabPath = $"{SDK_ROOT}/Prefabs/Managers/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, $"Add {name}");
                Debug.Log($"[IVXSceneSetup] Added {name} from prefab");
                return;
            }

            // Create from components
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            // Add IVXNManager
            var managerType = FindType("IntelliVerseX.Backend.IVXNManager");
            if (managerType != null)
                go.AddComponent(managerType);

            // Add GeoLocationService
            var geoType = FindType("IntelliVerseX.Services.GeoLocationService");
            if (geoType != null)
                go.AddComponent(geoType);

            Undo.RegisterCreatedObjectUndo(go, $"Add {name}");
            Debug.Log($"[IVXSceneSetup] Created {name} with components");
        }

        /// <summary>
        /// Sets up the UserData runtime monitor in the scene
        /// </summary>
        public static void SetupUserData(Transform parent)
        {
            const string name = "UserData";
            
            // Check if already exists
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"[IVXSceneSetup] {name} already exists");
                return;
            }

            // Try to instantiate from prefab
            var prefabPath = $"{SDK_ROOT}/Prefabs/Managers/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, $"Add {name}");
                Debug.Log($"[IVXSceneSetup] Added {name} from prefab");
                return;
            }

            // Create from components
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var runtimeType = FindType("IntelliVerseX.Backend.Nakama.IVXNUserRuntime");
            if (runtimeType != null)
                go.AddComponent(runtimeType);

            Undo.RegisterCreatedObjectUndo(go, $"Add {name}");
            Debug.Log($"[IVXSceneSetup] Created {name} with components");
        }

        /// <summary>
        /// Sets up the BackendService in the scene
        /// </summary>
        public static void SetupBackendService(Transform parent)
        {
            const string name = "BackendService";
            
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"[IVXSceneSetup] {name} already exists");
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var serviceType = FindType("IntelliVerseX.Backend.IVXBackendService");
            if (serviceType != null)
                go.AddComponent(serviceType);

            Undo.RegisterCreatedObjectUndo(go, $"Add {name}");
            Debug.Log($"[IVXSceneSetup] Created {name}");
        }

        /// <summary>
        /// Sets up a loading overlay UI
        /// </summary>
        public static void SetupLoadingOverlay(Transform parent)
        {
            const string name = "LoadingOverlay";
            
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"[IVXSceneSetup] {name} already exists");
                return;
            }

            // Create canvas if needed
            var canvas = parent.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("SDKCanvas");
                canvasGo.transform.SetParent(parent);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // High sort order for overlay
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Create loading overlay
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.8f);

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;

            go.SetActive(false); // Hidden by default

            Undo.RegisterCreatedObjectUndo(go, $"Add {name}");
            Debug.Log($"[IVXSceneSetup] Created {name}");
        }

        #endregion

        #region Intro Scene Creation

        /// <summary>
        /// Creates a new intro scene from the SDK template
        /// </summary>
        public static void CreateIntroSceneFromTemplate()
        {
            // Check if IntroSceneIntelliverseX exists
            if (!AssetDatabase.IsValidFolder(INTRO_SCENE_ROOT))
            {
                EditorUtility.DisplayDialog("Error",
                    "IntroSceneIntelliverseX folder not found. Please ensure the SDK is properly imported.", "OK");
                return;
            }

            // Ask for scene name
            var scenePath = EditorUtility.SaveFilePanelInProject(
                "Create Intro Scene",
                "IntroScene",
                "unity",
                "Choose location for the intro scene",
                "Assets/Scenes");

            if (string.IsNullOrEmpty(scenePath))
                return;

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add required objects
            CreateIntroSceneObjects(scene);

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[IVXSceneSetup] Created intro scene at: {scenePath}");

            EditorUtility.DisplayDialog("Success",
                $"Intro scene created at:\n{scenePath}\n\nRemember to add it to Build Settings.", "OK");
        }

        private static void CreateIntroSceneObjects(Scene scene)
        {
            // Camera
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.tag = "MainCamera";

            // Canvas
            var canvasGo = new GameObject("IntroCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Black overlay
            var overlayGo = new GameObject("BlackOverlay");
            overlayGo.transform.SetParent(canvasGo.transform);
            var overlayRt = overlayGo.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            var overlayImage = overlayGo.AddComponent<UnityEngine.UI.Image>();
            overlayImage.color = Color.black;
            var overlayGroup = overlayGo.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 1f;

            // X Logo placeholder
            var xLogoGo = new GameObject("XLogo");
            xLogoGo.transform.SetParent(canvasGo.transform);
            var xLogoRt = xLogoGo.AddComponent<RectTransform>();
            xLogoRt.anchoredPosition = new Vector2(-200, 0);
            xLogoRt.sizeDelta = new Vector2(200, 200);
            var xLogoImage = xLogoGo.AddComponent<UnityEngine.UI.Image>();
            xLogoImage.color = Color.white;
            var xLogoGroup = xLogoGo.AddComponent<CanvasGroup>();

            // IntelliVerse text placeholder
            var textGo = new GameObject("IntelliVerseText");
            textGo.transform.SetParent(canvasGo.transform);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchoredPosition = new Vector2(100, 0);
            textRt.sizeDelta = new Vector2(400, 100);
            var textImage = textGo.AddComponent<UnityEngine.UI.Image>();
            textImage.color = Color.white;
            var textGroup = textGo.AddComponent<CanvasGroup>();
            textGroup.alpha = 0f;

            // Tagline placeholder
            var taglineGo = new GameObject("Tagline");
            taglineGo.transform.SetParent(canvasGo.transform);
            var taglineRt = taglineGo.AddComponent<RectTransform>();
            taglineRt.anchoredPosition = new Vector2(0, -200);
            taglineRt.sizeDelta = new Vector2(600, 50);
            var taglineText = taglineGo.AddComponent<TMPro.TextMeshProUGUI>();
            taglineText.text = "Immersive, Interactive, Infinite";
            taglineText.alignment = TMPro.TextAlignmentOptions.Center;
            taglineText.fontSize = 36;
            var taglineGroup = taglineGo.AddComponent<CanvasGroup>();
            taglineGroup.alpha = 0f;

            // Audio source
            var audioGo = new GameObject("AudioSource");
            var audioSource = audioGo.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // Intro Controller (create component holder)
            var controllerGo = new GameObject("IntroController");
            var controllerType = FindType("LogoIntroController");
            if (controllerType != null)
            {
                var controller = controllerGo.AddComponent(controllerType);
                
                // Try to set references via reflection
                SetFieldValue(controller, "xLogo", xLogoRt);
                SetFieldValue(controller, "xLogoGroup", xLogoGroup);
                SetFieldValue(controller, "intelliVerseText", textRt);
                SetFieldValue(controller, "intelliVerseGroup", textGroup);
                SetFieldValue(controller, "taglineGroup", taglineGroup);
                SetFieldValue(controller, "blackOverlayGroup", overlayGroup);
                SetFieldValue(controller, "sfxSource", audioSource);
                SetFieldValue(controller, "nextSceneName", "Main");
            }
            else
            {
                Debug.LogWarning("[IVXSceneSetup] LogoIntroController type not found. Please add it manually.");
            }

            // SDK Managers
            var managersRoot = new GameObject("--- SDK Managers ---");
            SetupNakamaManager(managersRoot.transform);
            SetupUserData(managersRoot.transform);
        }

        #endregion

        #region Verification

        /// <summary>
        /// Verifies that a scene is properly set up
        /// </summary>
        public static SceneVerificationResult VerifySceneSetup(Scene scene)
        {
            var result = new SceneVerificationResult { SceneName = scene.name };

            // Check for SDK managers root
            var managersRoot = GameObject.Find("--- SDK Managers ---");
            if (managersRoot == null)
            {
                result.Warnings.Add("SDK Managers root not found");
            }
            else
            {
                result.HasManagersRoot = true;
            }

            // Check for IVXNManager
            var managerType = FindType("IntelliVerseX.Backend.IVXNManager");
            if (managerType != null)
            {
                var manager = UnityEngine.Object.FindObjectOfType(managerType);
                result.HasNakamaManager = manager != null;
                if (!result.HasNakamaManager)
                    result.MissingComponents.Add("IVXNManager");
            }

            // Check for IVXNUserRuntime
            var runtimeType = FindType("IntelliVerseX.Backend.Nakama.IVXNUserRuntime");
            if (runtimeType != null)
            {
                var runtime = UnityEngine.Object.FindObjectOfType(runtimeType);
                result.HasUserData = runtime != null;
                if (!result.HasUserData)
                    result.MissingComponents.Add("IVXNUserRuntime");
            }

            // Check for Camera
            result.HasCamera = Camera.main != null;
            if (!result.HasCamera)
                result.Warnings.Add("No main camera found");

            // Check for EventSystem
            var eventSystem = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            result.HasEventSystem = eventSystem != null;
            if (!result.HasEventSystem)
                result.Warnings.Add("No EventSystem found (required for UI)");

            return result;
        }

        public class SceneVerificationResult
        {
            public string SceneName { get; set; }
            public bool HasManagersRoot { get; set; }
            public bool HasNakamaManager { get; set; }
            public bool HasUserData { get; set; }
            public bool HasCamera { get; set; }
            public bool HasEventSystem { get; set; }
            public List<string> MissingComponents { get; } = new List<string>();
            public List<string> Warnings { get; } = new List<string>();

            public bool IsValid => HasNakamaManager && HasUserData && MissingComponents.Count == 0;

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Scene: {SceneName}");
                sb.AppendLine($"Valid: {(IsValid ? "✅" : "❌")}");
                sb.AppendLine();
                sb.AppendLine("Components:");
                sb.AppendLine($"  Managers Root: {(HasManagersRoot ? "✅" : "❌")}");
                sb.AppendLine($"  NakamaManager: {(HasNakamaManager ? "✅" : "❌")}");
                sb.AppendLine($"  UserData: {(HasUserData ? "✅" : "❌")}");
                sb.AppendLine($"  Camera: {(HasCamera ? "✅" : "❌")}");
                sb.AppendLine($"  EventSystem: {(HasEventSystem ? "✅" : "❌")}");

                if (MissingComponents.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Missing Components:");
                    foreach (var c in MissingComponents)
                        sb.AppendLine($"  - {c}");
                }

                if (Warnings.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Warnings:");
                    foreach (var w in Warnings)
                        sb.AppendLine($"  ⚠️ {w}");
                }

                return sb.ToString();
            }
        }

        #endregion

        #region Utility Methods

        private static GameObject GetOrCreateGameObject(string name, Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var existing = roots.FirstOrDefault(g => g.name == name);
            
            if (existing != null)
                return existing;

            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private static void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        #endregion
    }
}
