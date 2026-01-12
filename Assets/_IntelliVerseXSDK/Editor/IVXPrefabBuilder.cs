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
                Description = "Nakama backend manager with geolocation",
                ComponentTypes = new List<string>
                {
                    "IntelliVerseX.Backend.IVXNManager",
                    "IntelliVerseX.Services.GeoLocationService"
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
            
            // ========== UI PREFABS ==========
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

        [MenuItem("IntelliVerseX/Prefabs/Create All Manager Prefabs", false, 200)]
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

        [MenuItem("IntelliVerseX/Prefabs/Create NakamaManager Prefab", false, 201)]
        public static void CreateNakamaManagerPrefab()
        {
            var def = AllPrefabs.FirstOrDefault(p => p.Name == "NakamaManager");
            if (def != null)
            {
                if (CreatePrefabFromDefinition(def))
                    Debug.Log("[IVXPrefabBuilder] NakamaManager prefab created successfully");
            }
        }

        [MenuItem("IntelliVerseX/Prefabs/Create UserData Prefab", false, 202)]
        public static void CreateUserDataPrefab()
        {
            var def = AllPrefabs.FirstOrDefault(p => p.Name == "UserData");
            if (def != null)
            {
                if (CreatePrefabFromDefinition(def))
                    Debug.Log("[IVXPrefabBuilder] UserData prefab created successfully");
            }
        }

        [MenuItem("IntelliVerseX/Scene Setup/Add All Managers to Scene", false, 300)]
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

        [MenuItem("IntelliVerseX/Scene Setup/Add NakamaManager to Scene", false, 301)]
        public static void AddNakamaManagerToScene()
        {
            var managersRoot = GetOrCreateRootObject("--- SDK Managers ---");
            InstantiatePrefabInScene("NakamaManager", managersRoot.transform);
        }

        [MenuItem("IntelliVerseX/Scene Setup/Add UserData to Scene", false, 302)]
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
