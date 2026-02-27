using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace IntelliVerseX.Bootstrap.Editor
{
    /// <summary>
    /// Automatically detects installed optional dependencies and sets scripting define symbols.
    /// This ensures conditional compilation works correctly for optional packages.
    /// </summary>
    [InitializeOnLoad]
    public static class IVXDependencySymbolManager
    {
        private const string LOG_PREFIX = "[IntelliVerseX] ";
        
        // Symbol definitions
        private const string NAKAMA_SYMBOL = "INTELLIVERSEX_HAS_NAKAMA";
        private const string PHOTON_SYMBOL = "INTELLIVERSEX_HAS_PHOTON";
        private const string DOTWEEN_SYMBOL = "INTELLIVERSEX_HAS_DOTWEEN";
        private const string APPODEAL_SYMBOL = "INTELLIVERSEX_HAS_APPODEAL";
        private const string LEVELPLAY_SYMBOL = "INTELLIVERSEX_HAS_LEVELPLAY";
        
        // Assembly names to detect
        private const string NAKAMA_ASSEMBLY = "NakamaRuntime";
        private const string PHOTON_ASSEMBLY = "PhotonUnityNetworking";
        private const string PHOTON_REALTIME_ASSEMBLY = "PhotonRealtime";
        private const string DOTWEEN_ASSEMBLY = "DOTween.Modules";
        private const string APPODEAL_ASSEMBLY = "AppodealStack.Monetization.Api";
        private const string LEVELPLAY_ASSEMBLY = "Unity.Services.LevelPlay";
        
        static IVXDependencySymbolManager()
        {
            // Delay to ensure assemblies are loaded
            EditorApplication.delayCall += UpdateSymbols;
            
            // Also check when compilation finishes
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }
        
        private static void OnCompilationFinished(object obj)
        {
            UpdateSymbols();
        }
        
        /// <summary>
        /// Updates scripting define symbols based on installed assemblies.
        /// </summary>
        public static void UpdateSymbols()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup == BuildTargetGroup.Unknown)
                return;
            
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var defines = new HashSet<string>(currentDefines.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)));
            
            var assemblies = GetAssemblyNames();
            var changes = new List<string>();
            
            // Check Nakama
            bool hasNakama = assemblies.Contains(NAKAMA_ASSEMBLY);
            if (UpdateSymbol(defines, NAKAMA_SYMBOL, hasNakama))
                changes.Add($"Nakama: {(hasNakama ? "found" : "not found")}");
            
            // Check Photon
            bool hasPhoton = assemblies.Contains(PHOTON_ASSEMBLY) || assemblies.Contains(PHOTON_REALTIME_ASSEMBLY);
            if (UpdateSymbol(defines, PHOTON_SYMBOL, hasPhoton))
                changes.Add($"Photon: {(hasPhoton ? "found" : "not found")}");
            
            // Check DOTween
            bool hasDOTween = assemblies.Contains(DOTWEEN_ASSEMBLY);
            if (UpdateSymbol(defines, DOTWEEN_SYMBOL, hasDOTween))
                changes.Add($"DOTween: {(hasDOTween ? "found" : "not found")}");
            
            // Check Appodeal
            bool hasAppodeal = assemblies.Contains(APPODEAL_ASSEMBLY);
            if (UpdateSymbol(defines, APPODEAL_SYMBOL, hasAppodeal))
                changes.Add($"Appodeal: {(hasAppodeal ? "found" : "not found")}");
            
            // Check LevelPlay
            bool hasLevelPlay = assemblies.Contains(LEVELPLAY_ASSEMBLY);
            if (UpdateSymbol(defines, LEVELPLAY_SYMBOL, hasLevelPlay))
                changes.Add($"LevelPlay: {(hasLevelPlay ? "found" : "not found")}");
            
            // Apply if changed
            if (changes.Count > 0)
            {
                var newDefines = string.Join(";", defines.OrderBy(s => s));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                
                Debug.Log($"{LOG_PREFIX}Updated dependency symbols: {string.Join(", ", changes)}");
            }
        }
        
        private static HashSet<string> GetAssemblyNames()
        {
            var assemblies = new HashSet<string>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    assemblies.Add(assembly.GetName().Name);
                }
                catch
                {
                    // Ignore assembly load errors
                }
            }
            
            return assemblies;
        }
        
        private static bool UpdateSymbol(HashSet<string> defines, string symbol, bool shouldExist)
        {
            bool exists = defines.Contains(symbol);
            
            if (shouldExist && !exists)
            {
                defines.Add(symbol);
                return true;
            }
            else if (!shouldExist && exists)
            {
                defines.Remove(symbol);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Forces a refresh of dependency symbols.
        /// </summary>
        [MenuItem("IntelliVerseX/SDK Status/Refresh Dependency Symbols", false, 102)]
        public static void ForceRefreshSymbols()
        {
            UpdateSymbols();
            Debug.Log($"{LOG_PREFIX}Dependency symbols refreshed.");
        }
    }
}
