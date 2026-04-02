using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace IntelliVerseX.Bootstrap.Editor
{
    /// <summary>
    /// IntelliVerseX SDK Define Symbol Manager.
    /// Automatically manages scripting define symbols for optional dependencies.
    /// 
    /// Features:
    /// - Auto-detects installed packages (Nakama, Photon, DOTween, etc.)
    /// - Sets INTELLIVERSEX_* prefixed symbols
    /// - Idempotent (safe to run multiple times)
    /// - Works with UPM and .unitypackage installs
    /// - Supports Unity 2020.3 LTS through Unity 6+
    /// </summary>
    [InitializeOnLoad]
    public static class IVXDefineSymbolManager
    {
        #region Constants
        
        private const string LOG_PREFIX = "[IntelliVerseX] ";
        private const string SDK_VERSION = "5.0.0";
        private const string EDITORPREFS_VERSION_KEY = "com.intelliversex.sdk.lastAppliedVersion";
        private const string EDITORPREFS_SYMBOLS_HASH_KEY = "com.intelliversex.sdk.lastSymbolsHash";
        
        // SDK Define Symbols (prefixed to avoid collisions)
        private const string SYMBOL_SDK = "INTELLIVERSEX_SDK";
        private const string SYMBOL_HAS_NAKAMA = "INTELLIVERSEX_HAS_NAKAMA";
        private const string SYMBOL_HAS_PHOTON = "INTELLIVERSEX_HAS_PHOTON";
        private const string SYMBOL_HAS_DOTWEEN = "INTELLIVERSEX_HAS_DOTWEEN";
        private const string SYMBOL_HAS_APPODEAL = "INTELLIVERSEX_HAS_APPODEAL";
        private const string SYMBOL_HAS_LEVELPLAY = "INTELLIVERSEX_HAS_LEVELPLAY";
        private const string SYMBOL_HAS_NATIVE_SHARE = "INTELLIVERSEX_HAS_NATIVE_SHARE";
        private const string SYMBOL_HAS_APPLE_SIGNIN = "INTELLIVERSEX_HAS_APPLE_SIGNIN";
        
        // All SDK-managed symbols (only these will be added/removed)
        private static readonly string[] ManagedSymbols = new[]
        {
            SYMBOL_SDK,
            SYMBOL_HAS_NAKAMA,
            SYMBOL_HAS_PHOTON,
            SYMBOL_HAS_DOTWEEN,
            SYMBOL_HAS_APPODEAL,
            SYMBOL_HAS_LEVELPLAY,
            SYMBOL_HAS_NATIVE_SHARE,
            SYMBOL_HAS_APPLE_SIGNIN
        };
        
        // Assembly names to detect for each dependency
        private static readonly Dictionary<string, string[]> DependencyAssemblies = new Dictionary<string, string[]>
        {
            { SYMBOL_HAS_NAKAMA, new[] { "NakamaRuntime", "Nakama" } },
            { SYMBOL_HAS_PHOTON, new[] { "PhotonUnityNetworking", "PhotonRealtime", "Photon.Pun" } },
            { SYMBOL_HAS_DOTWEEN, new[] { "DOTween.Modules", "DOTween", "DG.Tweening" } },
            { SYMBOL_HAS_APPODEAL, new[] { "AppodealStack.Monetization.Api", "AppodealStack.Monetization.Common" } },
            { SYMBOL_HAS_LEVELPLAY, new[] { "Unity.Services.LevelPlay" } },
            { SYMBOL_HAS_NATIVE_SHARE, new[] { "NativeShare" } },
            { SYMBOL_HAS_APPLE_SIGNIN, new[] { "AppleAuth" } }
        };
        
        // Types to check if assemblies aren't found
        private static readonly Dictionary<string, string[]> DependencyTypes = new Dictionary<string, string[]>
        {
            { SYMBOL_HAS_NAKAMA, new[] { "Nakama.Client", "Nakama.IClient" } },
            { SYMBOL_HAS_PHOTON, new[] { "Photon.Pun.PhotonNetwork", "Photon.Realtime.Player" } },
            { SYMBOL_HAS_DOTWEEN, new[] { "DG.Tweening.DOTween", "DG.Tweening.Tween" } },
            { SYMBOL_HAS_APPODEAL, new[] { "AppodealStack.Monetization.Common.Appodeal" } },
            { SYMBOL_HAS_LEVELPLAY, new[] { "Unity.Services.LevelPlay.LevelPlay" } },
            { SYMBOL_HAS_NATIVE_SHARE, new[] { "NativeShare" } },
            { SYMBOL_HAS_APPLE_SIGNIN, new[] { "AppleAuth.AppleAuthManager" } }
        };
        
        #endregion
        
        #region Static Constructor
        
        static IVXDefineSymbolManager()
        {
            // Delay to ensure Unity is fully loaded
            EditorApplication.delayCall += OnEditorLoaded;
            
            // Also update on compilation finished (catches package additions)
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }
        
        private static void OnEditorLoaded()
        {
            // Only run once per session or when version changes
            string lastVersion = EditorPrefs.GetString(EDITORPREFS_VERSION_KEY, "");
            
            if (lastVersion != SDK_VERSION)
            {
                Debug.Log($"{LOG_PREFIX}SDK version changed ({lastVersion} -> {SDK_VERSION}). Updating symbols...");
                ApplyDefinesForAllTargets(force: true);
                EditorPrefs.SetString(EDITORPREFS_VERSION_KEY, SDK_VERSION);
            }
            else
            {
                // Just check if symbols need updating (dependency added/removed)
                ApplyDefinesForAllTargets(force: false);
            }
        }
        
        private static void OnCompilationFinished(object obj)
        {
            // Re-evaluate after compilation (catches new packages)
            ApplyDefinesForAllTargets(force: false);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets all expected define symbols based on currently installed packages.
        /// </summary>
        public static HashSet<string> GetExpectedDefines()
        {
            var expected = new HashSet<string>();
            
            // Always add SDK marker
            expected.Add(SYMBOL_SDK);
            
            // Check each dependency
            var assemblyNames = GetLoadedAssemblyNames();
            
            foreach (var kvp in DependencyAssemblies)
            {
                string symbol = kvp.Key;
                string[] assemblies = kvp.Value;
                
                // Check if any of the dependency's assemblies are loaded
                bool found = assemblies.Any(a => assemblyNames.Contains(a));
                
                // Fallback: check by type if assembly name check failed
                if (!found && DependencyTypes.TryGetValue(symbol, out string[] types))
                {
                    found = types.Any(TypeExists);
                }
                
                if (found)
                {
                    expected.Add(symbol);
                }
            }
            
            return expected;
        }
        
        /// <summary>
        /// Applies defines for all build targets.
        /// </summary>
        /// <param name="force">If true, forces update even if symbols haven't changed.</param>
        public static void ApplyDefinesForAllTargets(bool force = false)
        {
            var expected = GetExpectedDefines();
            string expectedHash = string.Join(",", expected.OrderBy(s => s));
            string lastHash = EditorPrefs.GetString(EDITORPREFS_SYMBOLS_HASH_KEY, "");
            
            if (!force && expectedHash == lastHash)
            {
                // No changes needed
                return;
            }
            
            bool anyChanges = false;
            
            // Apply to all relevant build targets
#if UNITY_2021_2_OR_NEWER
            foreach (var namedTarget in GetAllNamedBuildTargets())
            {
                if (ApplyDefinesForNamedTarget(namedTarget, expected))
                {
                    anyChanges = true;
                }
            }
#else
            foreach (var group in GetAllBuildTargetGroups())
            {
                if (ApplyDefinesForBuildTargetGroup(group, expected))
                {
                    anyChanges = true;
                }
            }
#endif
            
            if (anyChanges)
            {
                EditorPrefs.SetString(EDITORPREFS_SYMBOLS_HASH_KEY, expectedHash);
                
                var addedSymbols = expected.Where(s => s != SYMBOL_SDK).ToList();
                if (addedSymbols.Count > 0)
                {
                    Debug.Log($"{LOG_PREFIX}Updated define symbols: {string.Join(", ", addedSymbols)}");
                }
            }
        }
        
        /// <summary>
        /// Forces a refresh of all define symbols.
        /// </summary>
        [MenuItem("IntelliVerseX/SDK Tools/Reapply Define Symbols", false, 200)]
        public static void ForceReapplyDefines()
        {
            EditorPrefs.DeleteKey(EDITORPREFS_SYMBOLS_HASH_KEY);
            ApplyDefinesForAllTargets(force: true);
            Debug.Log($"{LOG_PREFIX}Define symbols reapplied.");
        }
        
        /// <summary>
        /// Shows current define symbol status.
        /// </summary>
        [MenuItem("IntelliVerseX/SDK Tools/Show Define Symbol Status", false, 201)]
        public static void ShowDefineStatus()
        {
            var expected = GetExpectedDefines();
            var status = new System.Text.StringBuilder();
            
            status.AppendLine("=== IntelliVerseX SDK Define Symbols ===");
            status.AppendLine();
            status.AppendLine($"SDK Version: {SDK_VERSION}");
            status.AppendLine();
            status.AppendLine("Active Symbols:");
            
            foreach (var symbol in expected.OrderBy(s => s))
            {
                status.AppendLine($"  - {symbol}");
            }
            
            status.AppendLine();
            status.AppendLine("Inactive (dependency not installed):");
            
            foreach (var symbol in ManagedSymbols.Where(s => !expected.Contains(s)).OrderBy(s => s))
            {
                status.AppendLine($"  - {symbol}");
            }
            
            EditorUtility.DisplayDialog("IntelliVerseX SDK - Define Symbols", status.ToString(), "OK");
        }
        
        #endregion
        
        #region Private Methods
        
#if UNITY_2021_2_OR_NEWER
        private static IEnumerable<NamedBuildTarget> GetAllNamedBuildTargets()
        {
            yield return NamedBuildTarget.Standalone;
            yield return NamedBuildTarget.Android;
            yield return NamedBuildTarget.iOS;
            yield return NamedBuildTarget.WebGL;
            yield return NamedBuildTarget.Server;
            
            // Add other platforms as needed
#if UNITY_TVOS
            yield return NamedBuildTarget.tvOS;
#endif
        }
        
        private static bool ApplyDefinesForNamedTarget(NamedBuildTarget target, HashSet<string> expectedSymbols)
        {
            try
            {
                string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);
                var currentSet = ParseDefines(currentDefines);
                
                var merged = MergeDefines(currentSet, expectedSymbols);
                string newDefines = string.Join(";", merged.OrderBy(s => s));
                
                if (currentDefines != newDefines)
                {
                    PlayerSettings.SetScriptingDefineSymbols(target, newDefines);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Some targets may not be installed
                if (!ex.Message.Contains("not installed") && !ex.Message.Contains("Unknown"))
                {
                    Debug.LogWarning($"{LOG_PREFIX}Could not apply defines for {target}: {ex.Message}");
                }
            }
            
            return false;
        }
#endif
        
#pragma warning disable CS0618 // Type or member is obsolete - needed for Unity 2020 support
        private static IEnumerable<BuildTargetGroup> GetAllBuildTargetGroups()
        {
            yield return BuildTargetGroup.Standalone;
            yield return BuildTargetGroup.Android;
            yield return BuildTargetGroup.iOS;
            yield return BuildTargetGroup.WebGL;
            
            // Add other platforms as needed
#if UNITY_TVOS
            yield return BuildTargetGroup.tvOS;
#endif
        }
        
        private static bool ApplyDefinesForBuildTargetGroup(BuildTargetGroup group, HashSet<string> expectedSymbols)
        {
            try
            {
                if (group == BuildTargetGroup.Unknown)
                    return false;
                
                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                var currentSet = ParseDefines(currentDefines);
                
                var merged = MergeDefines(currentSet, expectedSymbols);
                string newDefines = string.Join(";", merged.OrderBy(s => s));
                
                if (currentDefines != newDefines)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Some groups may not be valid
                if (!ex.Message.Contains("not installed") && !ex.Message.Contains("Unknown"))
                {
                    Debug.LogWarning($"{LOG_PREFIX}Could not apply defines for {group}: {ex.Message}");
                }
            }
            
            return false;
        }
#pragma warning restore CS0618
        
        private static HashSet<string> ParseDefines(string defines)
        {
            if (string.IsNullOrWhiteSpace(defines))
                return new HashSet<string>();
            
            return new HashSet<string>(
                defines.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
            );
        }
        
        private static HashSet<string> MergeDefines(HashSet<string> current, HashSet<string> expected)
        {
            var result = new HashSet<string>(current);
            
            // Remove SDK-managed symbols that should not be present
            foreach (var symbol in ManagedSymbols)
            {
                if (!expected.Contains(symbol))
                {
                    result.Remove(symbol);
                }
            }
            
            // Add expected symbols
            foreach (var symbol in expected)
            {
                result.Add(symbol);
            }
            
            return result;
        }
        
        private static HashSet<string> GetLoadedAssemblyNames()
        {
            var names = new HashSet<string>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    names.Add(assembly.GetName().Name);
                }
                catch
                {
                    // Ignore assembly load errors
                }
            }
            
            return names;
        }
        
        private static bool TypeExists(string fullTypeName)
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
        
        #endregion
    }
}
