// File: IVXPrefabBuilder.cs
// Purpose: Utility for creating and managing SDK prefabs
// Version: 1.0.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Utility class for creating and managing SDK prefabs.
    /// Provides methods to create manager prefabs with proper component configuration.
    /// </summary>
    public static class IVXPrefabBuilder
    {
        #region Constants

        private const string SDK_ROOT = "Assets/_IntelliVerseXSDK";
        private const string PREFABS_ROOT = SDK_ROOT + "/Prefabs";
        private const string MANAGERS_PATH = PREFABS_ROOT + "/Managers";
        private const string UI_PATH = PREFABS_ROOT + "/UI";

        #endregion

        #region Prefab Definitions

        /// <summary>
        /// Definition for a prefab to be created
        /// </summary>
        public class PrefabDefinition
        {
            public string Name { get; set; }
            public string Category { get; set; } // "Managers", "UI", "Scenes"
            public List<string> ComponentTypes { get; set; } = new List<string>();
            public Dictionary<string, object> DefaultValues { get; set; } = new Dictionary<string, object>();
            public string Description { get; set; }
        }

        /// <summary>
        /// All SDK prefab definitions
        /// </summary>
        public static readonly List<PrefabDefinition> AllPrefabs = new List<PrefabDefinition>
        {
            // ========== MANAGER PREFABS ==========
            new PrefabDefinition
            {
                Name = "NakamaManager",
                Category = "Managers",
                Description = "Complete Nakama backend manager with geolocation and user runtime",
                ComponentTypes = new List<string>
                {
                    "IntelliVerseX.Backend.Nakama.IVXNManager",
                    "IntelliVerseX.Services.GeoLocationService",
                    "IntelliVerseX.Backend.IVXGeolocationService",
                    "IntelliVerseX.Backend.Nakama.IVXNUserRuntime"
                }
            },
            new PrefabDefinition
            {
                Name = "UserData",
                Category = "Managers",
                Description = "User runtime data snapshot",
                ComponentTypes = new List<string>
                {
                    "IntelliVerseX.Backend.Nakama.IVXNUserRuntime"
                }
            },
            new PrefabDefinition
            {
                Name = "SDKManager",
                Category = "Managers",
                Description = "Main SDK coordinator",
                ComponentTypes = new List<string>
                {
                    "IntelliVerseX.Core.IntelliVerseXManager"
                }
            },
            new PrefabDefinition
            {
                Name = "BackendService",
                Category = "Managers",
                Description = "Nakama client and session manager",
                ComponentTypes = new List<string>
                {
                    "IntelliVerseX.Backend.IVXBackendService"
                }
            },
            
            // ========== AUTH UI PREFABS ==========
            new PrefabDefinition
            {
                Name = "IVX_AuthCanvas",
                Category = "UI",
                Description = "Complete authentication canvas with all auth panels",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.Canvas",
                    "UnityEngine.UI.CanvasScaler",
                    "UnityEngine.UI.GraphicRaycaster",
                    "IntelliVerseX.Auth.UI.IVXCanvasAuth"
                }
            },
            new PrefabDefinition
            {
                Name = "LoginPanel",
                Category = "UI",
                Description = "Login panel with email/password, social login, and remember me",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.CanvasGroup",
                    "IntelliVerseX.Auth.UI.IVXPanelLogin"
                }
            },
            new PrefabDefinition
            {
                Name = "RegisterPanel",
                Category = "UI",
                Description = "Registration panel with validation and referral support",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.CanvasGroup",
                    "IntelliVerseX.Auth.UI.IVXPanelRegister"
                }
            },
            new PrefabDefinition
            {
                Name = "OTPPanel",
                Category = "UI",
                Description = "OTP verification panel with resend timer",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.CanvasGroup",
                    "IntelliVerseX.Auth.UI.IVXPanelOTP"
                }
            },
            new PrefabDefinition
            {
                Name = "ForgotPasswordPanel",
                Category = "UI",
                Description = "Forgot password panel with OTP reset flow",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.CanvasGroup",
                    "IntelliVerseX.Auth.UI.IVXPanelForgotPassword"
                }
            },
            new PrefabDefinition
            {
                Name = "ReferralPanel",
                Category = "UI",
                Description = "Referral code popup panel",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.CanvasGroup",
                    "IntelliVerseX.Auth.UI.IVXPanelReferral"
                }
            },
            
            // ========== UTILITY UI PREFABS ==========
            new PrefabDefinition
            {
                Name = "LoadingOverlay",
                Category = "UI",
                Description = "Full-screen loading overlay",
                ComponentTypes = new List<string>
                {
                    "UnityEngine.UI.Image",
                    "UnityEngine.CanvasGroup"
                }
            }
        };

        #endregion

        #region Menu Items

        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Prefabs/Create All Manager Prefabs", false, 200)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        
        public static void CreateAllManagerPrefabs()
        {
            EnsureDirectories();

            int created = 0;
            int skipped = 0;

            foreach (var def in AllPrefabs.Where(p => p.Category == "Managers"))
            {
                if (CreatePrefabFromDefinition(def))
                    created++;
                else
                    skipped++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefab Creation Complete",
                $"Created: {created}\nSkipped (already exist): {skipped}", "OK");
        }

        // [MenuItem("IntelliVerse-X SDK/Prefabs/Create NakamaManager Prefab", false, 201)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void CreateNakamaManagerPrefab()
        {
            var def = AllPrefabs.FirstOrDefault(p => p.Name == "NakamaManager");
            if (def != null)
            {
                if (CreatePrefabFromDefinition(def))
                    Debug.Log("[IVXPrefabBuilder] NakamaManager prefab created successfully");
            }
        }

        // [MenuItem("IntelliVerse-X SDK/Prefabs/Create UserData Prefab", false, 202)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void CreateUserDataPrefab()
        {
            var def = AllPrefabs.FirstOrDefault(p => p.Name == "UserData");
            if (def != null)
            {
                if (CreatePrefabFromDefinition(def))
                    Debug.Log("[IVXPrefabBuilder] UserData prefab created successfully");
            }
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Add All Managers to Scene", false, 300)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void AddAllManagersToScene()
        {
            var managersRoot = GetOrCreateRootObject("--- SDK Managers ---");

            int added = 0;
            foreach (var def in AllPrefabs.Where(p => p.Category == "Managers"))
            {
                if (InstantiatePrefabInScene(def.Name, managersRoot.transform))
                    added++;
            }

            Debug.Log($"[IVXPrefabBuilder] Added {added} managers to scene");
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Add NakamaManager to Scene", false, 301)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void AddNakamaManagerToScene()
        {
            var managersRoot = GetOrCreateRootObject("--- SDK Managers ---");
            InstantiatePrefabInScene("NakamaManager", managersRoot.transform);
        }

        // [MenuItem("IntelliVerse-X SDK/Scene Setup/Add UserData to Scene", false, 302)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Core tab
        public static void AddUserDataToScene()
        {
            var managersRoot = GetOrCreateRootObject("--- SDK Managers ---");
            InstantiatePrefabInScene("UserData", managersRoot.transform);
        }

        #endregion

        #region Prefab Creation

        /// <summary>
        /// Creates a prefab from a definition
        /// </summary>
        public static bool CreatePrefabFromDefinition(PrefabDefinition def)
        {
            EnsureDirectories();

            string prefabPath = GetPrefabPath(def);
            
            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[IVXPrefabBuilder] Prefab already exists: {prefabPath}");
                return false;
            }

            // Create GameObject
            var go = new GameObject(def.Name);

            try
            {
                // Add components
                foreach (var typeName in def.ComponentTypes)
                {
                    var type = FindType(typeName);
                    if (type != null)
                    {
                        go.AddComponent(type);
                        Debug.Log($"[IVXPrefabBuilder] Added component: {typeName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[IVXPrefabBuilder] Type not found: {typeName}");
                    }
                }

                // Apply default values
                foreach (var kvp in def.DefaultValues)
                {
                    ApplyDefaultValue(go, kvp.Key, kvp.Value);
                }

                // Save as prefab
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Debug.Log($"[IVXPrefabBuilder] Created prefab: {prefabPath}");

                return true;
            }
            finally
            {
                // Cleanup
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Creates a prefab with specific components
        /// </summary>
        public static bool CreatePrefab(string name, string category, params Type[] componentTypes)
        {
            EnsureDirectories();

            string folder = category == "Managers" ? MANAGERS_PATH : UI_PATH;
            string prefabPath = $"{folder}/{name}.prefab";

            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[IVXPrefabBuilder] Prefab already exists: {prefabPath}");
                return false;
            }

            var go = new GameObject(name);

            try
            {
                foreach (var type in componentTypes)
                {
                    if (type != null)
                    {
                        go.AddComponent(type);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Debug.Log($"[IVXPrefabBuilder] Created prefab: {prefabPath}");

                return true;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region Scene Helpers

        /// <summary>
        /// Instantiates a prefab in the current scene
        /// </summary>
        public static bool InstantiatePrefabInScene(string prefabName, Transform parent = null)
        {
            // Try Managers folder first, then UI
            string prefabPath = $"{MANAGERS_PATH}/{prefabName}.prefab";
            if (!File.Exists(prefabPath.Replace("Assets/", Application.dataPath + "/")))
            {
                prefabPath = $"{UI_PATH}/{prefabName}.prefab";
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[IVXPrefabBuilder] Prefab not found: {prefabName}");
                return false;
            }

            // Check if already in scene
            var existing = GameObject.Find(prefabName);
            if (existing != null)
            {
                Debug.Log($"[IVXPrefabBuilder] {prefabName} already exists in scene");
                return false;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Add {prefabName}");
            Debug.Log($"[IVXPrefabBuilder] Added {prefabName} to scene");

            return true;
        }

        /// <summary>
        /// Gets or creates a root GameObject for organization
        /// </summary>
        public static GameObject GetOrCreateRootObject(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
                return existing;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        /// <summary>
        /// Instantiates manager components directly (without prefab)
        /// </summary>
        public static void InstantiateManagerComponents(Transform parent, params string[] componentTypeNames)
        {
            foreach (var typeName in componentTypeNames)
            {
                var type = FindType(typeName);
                if (type == null)
                {
                    Debug.LogWarning($"[IVXPrefabBuilder] Type not found: {typeName}");
                    continue;
                }

                // Check if already exists in scene
                var existing = UnityEngine.Object.FindObjectOfType(type);
                if (existing != null)
                {
                    Debug.Log($"[IVXPrefabBuilder] {typeName} already exists in scene");
                    continue;
                }

                var go = new GameObject(type.Name);
                go.AddComponent(type);
                
                if (parent != null)
                    go.transform.SetParent(parent);

                Undo.RegisterCreatedObjectUndo(go, $"Add {type.Name}");
                Debug.Log($"[IVXPrefabBuilder] Created {type.Name} in scene");
            }
        }

        #endregion

        #region Utility Methods

        private static void EnsureDirectories()
        {
            EnsureDirectory(PREFABS_ROOT);
            EnsureDirectory(MANAGERS_PATH);
            EnsureDirectory(UI_PATH);
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = nextPath;
            }
        }

        private static string GetPrefabPath(PrefabDefinition def)
        {
            string folder = def.Category switch
            {
                "Managers" => MANAGERS_PATH,
                "UI" => UI_PATH,
                _ => PREFABS_ROOT
            };

            return $"{folder}/{def.Name}.prefab";
        }

        private static Type FindType(string fullName)
        {
            // Try exact match first
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            // Try Unity types
            if (fullName.StartsWith("UnityEngine."))
            {
                return Type.GetType($"{fullName}, UnityEngine");
            }
            if (fullName.StartsWith("UnityEngine.UI."))
            {
                return Type.GetType($"{fullName}, UnityEngine.UI");
            }

            return null;
        }

        private static void ApplyDefaultValue(GameObject go, string propertyPath, object value)
        {
            // Property path format: "ComponentType.PropertyName"
            var parts = propertyPath.Split('.');
            if (parts.Length < 2) return;

            var componentTypeName = string.Join(".", parts.Take(parts.Length - 1));
            var propertyName = parts.Last();

            var componentType = FindType(componentTypeName);
            if (componentType == null) return;

            var component = go.GetComponent(componentType);
            if (component == null) return;

            var property = componentType.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(component, value);
            }

            var field = componentType.GetField(propertyName);
            if (field != null)
            {
                field.SetValue(component, value);
            }
        }

        #endregion

        #region Auth Scene Setup

        /// <summary>
        /// Creates a complete auth scene with all required components
        /// </summary>
        public static void CreateAuthScene()
        {
            // Create managers root
            var managersRoot = GetOrCreateRootObject("--- SDK Managers ---");
            
            // Add NakamaManager if not exists
            InstantiatePrefabInScene("NakamaManager", managersRoot.transform);
            
            // Create auth canvas
            CreateAuthCanvas();
            
            Debug.Log("[IVXPrefabBuilder] Auth scene setup complete");
        }

        /// <summary>
        /// Creates the auth canvas with all panels
        /// </summary>
        public static GameObject CreateAuthCanvas()
        {
            // Check if already exists
            var existing = GameObject.Find("IVX_AuthCanvas");
            if (existing != null)
            {
                Debug.Log("[IVXPrefabBuilder] IVX_AuthCanvas already exists in scene");
                return existing;
            }

            // Create canvas
            var canvasGO = new GameObject("IVX_AuthCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var canvasAuth = canvasGO.AddComponent<IntelliVerseX.Auth.UI.IVXCanvasAuth>();

            // Create panels
            var loginPanel = CreateAuthPanel("LoginPanel", canvasGO.transform, typeof(IntelliVerseX.Auth.UI.IVXPanelLogin));
            var registerPanel = CreateAuthPanel("RegisterPanel", canvasGO.transform, typeof(IntelliVerseX.Auth.UI.IVXPanelRegister));
            var otpPanel = CreateAuthPanel("OTPPanel", canvasGO.transform, typeof(IntelliVerseX.Auth.UI.IVXPanelOTP));
            var forgotPanel = CreateAuthPanel("ForgotPasswordPanel", canvasGO.transform, typeof(IntelliVerseX.Auth.UI.IVXPanelForgotPassword));
            var referralPanel = CreateAuthPanel("ReferralPanel", canvasGO.transform, typeof(IntelliVerseX.Auth.UI.IVXPanelReferral));
            var loadingPanel = CreateAuthPanel("LoadingPanel", canvasGO.transform, null);

            // Wire up references using SerializedObject
            var so = new SerializedObject(canvasAuth);
            SetSerializedField(so, "_loginPanel", loginPanel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelLogin>());
            SetSerializedField(so, "_registerPanel", registerPanel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelRegister>());
            SetSerializedField(so, "_otpPanel", otpPanel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelOTP>());
            SetSerializedField(so, "_forgotPasswordPanel", forgotPanel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelForgotPassword>());
            SetSerializedField(so, "_referralPanel", referralPanel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelReferral>());
            SetSerializedField(so, "_loadingPanel", loadingPanel);
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Auth Canvas");
            Debug.Log("[IVXPrefabBuilder] Created IVX_AuthCanvas with all panels");

            return canvasGO;
        }

        private static GameObject CreateAuthPanel(string name, Transform parent, Type componentType)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panel.AddComponent<CanvasGroup>();

            if (componentType != null)
            {
                panel.AddComponent(componentType);
            }

            // Start with panel inactive except login
            if (name != "LoginPanel")
            {
                panel.SetActive(false);
            }

            return panel;
        }

        private static void SetSerializedField(SerializedObject so, string fieldName, UnityEngine.Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        /// <summary>
        /// Auto-wires an existing IVXCanvasAuth by finding child panels
        /// </summary>
        public static void AutoWireAuthCanvas(GameObject canvasGO)
        {
            if (canvasGO == null) return;

            var canvasAuth = canvasGO.GetComponent<IntelliVerseX.Auth.UI.IVXCanvasAuth>();
            if (canvasAuth == null)
            {
                Debug.LogWarning("[IVXPrefabBuilder] No IVXCanvasAuth component found");
                return;
            }

            var so = new SerializedObject(canvasAuth);

            // Find and wire panels
            WireChildComponent<IntelliVerseX.Auth.UI.IVXPanelLogin>(canvasGO, so, "_loginPanel");
            WireChildComponent<IntelliVerseX.Auth.UI.IVXPanelRegister>(canvasGO, so, "_registerPanel");
            WireChildComponent<IntelliVerseX.Auth.UI.IVXPanelOTP>(canvasGO, so, "_otpPanel");
            WireChildComponent<IntelliVerseX.Auth.UI.IVXPanelForgotPassword>(canvasGO, so, "_forgotPasswordPanel");
            WireChildComponent<IntelliVerseX.Auth.UI.IVXPanelReferral>(canvasGO, so, "_referralPanel");

            // Find loading panel by name
            var loadingPanel = canvasGO.transform.Find("LoadingPanel");
            if (loadingPanel != null)
            {
                SetSerializedField(so, "_loadingPanel", loadingPanel.gameObject);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(canvasAuth);

            Debug.Log("[IVXPrefabBuilder] Auto-wired IVXCanvasAuth panels");
        }

        private static void WireChildComponent<T>(GameObject root, SerializedObject so, string fieldName) where T : Component
        {
            var component = root.GetComponentInChildren<T>(true);
            if (component != null)
            {
                SetSerializedField(so, fieldName, component);
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required prefabs exist
        /// </summary>
        public static ValidationResult ValidatePrefabs()
        {
            var result = new ValidationResult();

            foreach (var def in AllPrefabs)
            {
                string prefabPath = GetPrefabPath(def);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    result.MissingPrefabs.Add(def.Name);
                }
                else
                {
                    result.ExistingPrefabs.Add(def.Name);

                    // Validate components
                    foreach (var typeName in def.ComponentTypes)
                    {
                        var type = FindType(typeName);
                        if (type == null)
                        {
                            result.Warnings.Add($"{def.Name}: Component type not found - {typeName}");
                            continue;
                        }

                        if (prefab.GetComponent(type) == null)
                        {
                            result.Warnings.Add($"{def.Name}: Missing component - {typeName}");
                        }
                    }
                }
            }

            return result;
        }

        public class ValidationResult
        {
            public List<string> ExistingPrefabs { get; } = new List<string>();
            public List<string> MissingPrefabs { get; } = new List<string>();
            public List<string> Warnings { get; } = new List<string>();

            public bool IsValid => MissingPrefabs.Count == 0 && Warnings.Count == 0;

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Existing: {ExistingPrefabs.Count}");
                sb.AppendLine($"Missing: {MissingPrefabs.Count}");
                sb.AppendLine($"Warnings: {Warnings.Count}");

                if (MissingPrefabs.Count > 0)
                {
                    sb.AppendLine("\nMissing prefabs:");
                    foreach (var p in MissingPrefabs)
                        sb.AppendLine($"  - {p}");
                }

                if (Warnings.Count > 0)
                {
                    sb.AppendLine("\nWarnings:");
                    foreach (var w in Warnings)
                        sb.AppendLine($"  - {w}");
                }

                return sb.ToString();
            }
        }

        #endregion
    }
}
