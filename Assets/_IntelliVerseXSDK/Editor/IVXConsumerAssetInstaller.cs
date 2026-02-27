// IVXConsumerAssetInstaller.cs
// Comprehensive asset installer for IntelliVerseX SDK consumer projects
// Automatically copies demo scenes, prefabs, and resources when SDK is installed via UPM

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Comprehensive asset installer that copies all necessary SDK assets to consumer projects.
    /// Handles demo scenes, prefabs, resources, and configurations.
    /// Excludes MCP-related assets.
    /// </summary>
    [InitializeOnLoad]
    public static class IVXConsumerAssetInstaller
    {
        #region Constants
        
        private const string INSTALL_COMPLETE_KEY = "IVX_CONSUMER_ASSETS_INSTALLED_V5";
        private const string PACKAGE_NAME = "com.intelliversex.sdk";
        
        // Consumer project paths (where assets will be copied)
        private const string CONSUMER_DEMO_SCENES_FOLDER = "Assets/IntelliVerseX Demo Scenes";
        private const string CONSUMER_PREFABS_FOLDER = "Assets/IntelliVerseX/Prefabs";
        private const string CONSUMER_RESOURCES_FOLDER = "Assets/Resources/IntelliVerseX";
        private const string CONSUMER_GENERATED_FOLDER = "Assets/IntelliVerseX/Generated";
        
        // Files/folders to exclude (MCP and development-only)
        private static readonly string[] EXCLUDED_PATTERNS = new string[]
        {
            "MCP",
            "mcp",
            ".git",
            ".cursor",
            "Documentation~",
            "Tests~",
            ".meta.meta",
            "AGENTS.md",
            "AGENT.md"
        };
        
        // Demo scenes to copy
        private static readonly string[] DEMO_SCENES = new string[]
        {
            "IVX_HomeScreen.unity",
            "IVX_AuthTest.unity",
            "IVX_AdsTest.unity",
            "IVX_DailyQuiz.unity",
            "IVX_Friends.unity",
            "IVX_LeaderboardTest.unity",
            "IVX_MoreOfUs.unity",
            "IVX_Profile.unity",
            "IVX_Share&RateUs.unity",
            "IVX_WalletTest.unity",
            "IVX_WeeklyQuizTest.unity"
        };
        
        // Core prefabs to copy
        private static readonly Dictionary<string, string[]> PREFABS_TO_COPY = new Dictionary<string, string[]>
        {
            // Managers
            { "Managers", new string[] { 
                "NakamaManager.prefab"
            }},
            // Auth
            { "Auth", new string[] { 
                "IVX_AuthCanvas.prefab"
            }},
            // Social/Friends
            { "Social", new string[] { 
                "IVXFriendSlot.prefab",
                "IVXFriendRequestSlot.prefab",
                "IVXFriendSearchSlot.prefab",
                "IVXGShareManager.prefab",
                "IVXGRateAppManager.prefab"
            }},
            // Leaderboard
            { "Leaderboard", new string[] { 
                "IVXGLeaderboardCanvas.prefab",
                "IVXGLeaderboardEntry.prefab"
            }},
            // Quiz
            { "Quiz", new string[] { 
                "IVXWeeklyQuizBootstrap.prefab",
                "IVXWeeklyQuizTestUI.prefab"
            }},
            // Monetization/Ads
            { "Monetization", new string[] {
                "IVXAdsBootstrap.prefab",
                "IVXAdsTestUI.prefab"
            }},
            // More Of Us
            { "MoreOfUs", new string[] {
                "IVX_AppCard.prefab",
                "IVX_MoreOfUsCanvas.prefab"
            }}
        };
        
        #endregion
        
        #region Initialization
        
        static IVXConsumerAssetInstaller()
        {
            // Check if installation has already been completed
            if (EditorPrefs.GetBool(INSTALL_COMPLETE_KEY, false))
            {
                return;
            }
            
            // Delay to let Unity finish loading
            EditorApplication.delayCall += CheckAndInstallAssets;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force re-installation of all consumer assets.
        /// </summary>
        public static void ForceReinstall()
        {
            EditorPrefs.DeleteKey(INSTALL_COMPLETE_KEY);
            CheckAndInstallAssets();
        }
        
        /// <summary>
        /// Opens the first-time setup dialog.
        /// </summary>
        public static void ShowFirstTimeSetupDialog()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "IntelliVerseX SDK - First Time Setup",
                "Welcome to IntelliVerseX SDK!\n\n" +
                "Would you like to set up demo scenes and sample assets in your project?\n\n" +
                "This will copy:\n" +
                "• Demo scenes to 'Assets/IntelliVerseX Demo Scenes'\n" +
                "• Prefabs to 'Assets/IntelliVerseX/Prefabs'\n" +
                "• Resources to 'Assets/Resources/IntelliVerseX'\n\n" +
                "You can always do this later via the SDK Setup Wizard.",
                "Install Demo Assets",
                "Skip for Now",
                "Open Setup Wizard"
            );
            
            switch (choice)
            {
                case 0: // Install
                    InstallAllAssets(true);
                    break;
                case 1: // Skip
                    EditorPrefs.SetBool(INSTALL_COMPLETE_KEY, true);
                    Debug.Log("[IVX SDK] Skipped demo asset installation. You can install them later via Window > IntelliVerseX > SDK Setup Wizard.");
                    break;
                case 2: // Open Wizard
                    IVXSDKSetupWizard.ShowWindow();
                    break;
            }
        }
        
        /// <summary>
        /// Installs all SDK assets to the consumer project.
        /// </summary>
        /// <param name="showProgress">Whether to show progress dialogs.</param>
        /// <returns>True if installation succeeded.</returns>
        public static bool InstallAllAssets(bool showProgress = true)
        {
            if (showProgress)
            {
                EditorUtility.DisplayProgressBar("IntelliVerseX SDK", "Preparing asset installation...", 0f);
            }
            
            try
            {
                string sdkRoot = GetSDKRootPath();
                if (string.IsNullOrEmpty(sdkRoot))
                {
                    Debug.LogError("[IVX SDK] Could not find SDK root path. Please ensure the SDK is properly installed.");
                    return false;
                }
                
                var results = new InstallationResults();
                
                // Step 1: Install demo scenes
                if (showProgress)
                {
                    EditorUtility.DisplayProgressBar("IntelliVerseX SDK", "Copying demo scenes...", 0.2f);
                }
                InstallDemoScenes(sdkRoot, results);
                
                // Step 2: Install prefabs
                if (showProgress)
                {
                    EditorUtility.DisplayProgressBar("IntelliVerseX SDK", "Copying prefabs...", 0.5f);
                }
                InstallPrefabs(sdkRoot, results);
                
                // Step 3: Create resource configs
                if (showProgress)
                {
                    EditorUtility.DisplayProgressBar("IntelliVerseX SDK", "Creating resource configurations...", 0.8f);
                }
                CreateResourceConfigs(results);
                
                // Refresh asset database
                if (showProgress)
                {
                    EditorUtility.DisplayProgressBar("IntelliVerseX SDK", "Refreshing asset database...", 0.95f);
                }
                AssetDatabase.Refresh();
                
                // Mark as complete
                EditorPrefs.SetBool(INSTALL_COMPLETE_KEY, true);
                
                // Show results
                if (showProgress)
                {
                    EditorUtility.ClearProgressBar();
                    ShowInstallationResults(results);
                }
                
                Debug.Log($"[IVX SDK] Asset installation complete! Scenes: {results.ScenesInstalled}, Prefabs: {results.PrefabsInstalled}, Configs: {results.ConfigsCreated}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVX SDK] Asset installation failed: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                if (showProgress)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
        
        /// <summary>
        /// Installs only demo scenes.
        /// </summary>
        public static void InstallDemoScenesOnly()
        {
            string sdkRoot = GetSDKRootPath();
            if (string.IsNullOrEmpty(sdkRoot))
            {
                EditorUtility.DisplayDialog("Error", "Could not find SDK root path.", "OK");
                return;
            }
            
            var results = new InstallationResults();
            InstallDemoScenes(sdkRoot, results);
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Demo Scenes Installed",
                $"Installed {results.ScenesInstalled} demo scene(s) to:\n{CONSUMER_DEMO_SCENES_FOLDER}\n\n" +
                $"Skipped {results.ScenesSkipped} scene(s) that already exist.",
                "OK"
            );
        }
        
        /// <summary>
        /// Installs only prefabs.
        /// </summary>
        public static void InstallPrefabsOnly()
        {
            string sdkRoot = GetSDKRootPath();
            if (string.IsNullOrEmpty(sdkRoot))
            {
                EditorUtility.DisplayDialog("Error", "Could not find SDK root path.", "OK");
                return;
            }
            
            var results = new InstallationResults();
            InstallPrefabs(sdkRoot, results);
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Prefabs Installed",
                $"Installed {results.PrefabsInstalled} prefab(s) to:\n{CONSUMER_PREFABS_FOLDER}\n\n" +
                $"Skipped {results.PrefabsSkipped} prefab(s) that already exist.",
                "OK"
            );
        }
        
        #endregion
        
        #region Private Methods
        
        private static void CheckAndInstallAssets()
        {
            // Only run for UPM installs
            if (!IsUPMInstall())
            {
                Debug.Log("[IVX SDK] Development mode detected. Skipping auto-installation of consumer assets.");
                return;
            }
            
            // Show first-time setup dialog
            ShowFirstTimeSetupDialog();
        }
        
        private static bool IsUPMInstall()
        {
            // Check if SDK is in Assets folder (development mode)
            if (Directory.Exists(Path.Combine(Application.dataPath, "_IntelliVerseXSDK")))
            {
                return false;
            }
            
            // Check for UPM package
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                string manifest = File.ReadAllText(manifestPath);
                return manifest.Contains($"\"{PACKAGE_NAME}\"");
            }
            
            return false;
        }
        
        private static string GetSDKRootPath()
        {
            // Check Library/PackageCache first
            string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            if (Directory.Exists(packageCachePath))
            {
                var dirs = Directory.GetDirectories(packageCachePath, $"{PACKAGE_NAME}@*");
                if (dirs.Length > 0)
                {
                    return dirs[0];
                }
            }
            
            // Check local Packages folder
            string localPackagePath = Path.Combine(Application.dataPath, "..", "Packages", PACKAGE_NAME);
            if (Directory.Exists(localPackagePath))
            {
                return localPackagePath;
            }
            
            // Fallback to Assets folder (development mode)
            string assetsPath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK");
            if (Directory.Exists(assetsPath))
            {
                return assetsPath;
            }
            
            return null;
        }
        
        private static void InstallDemoScenes(string sdkRoot, InstallationResults results)
        {
            // Create destination folder
            string destFolder = Path.Combine(Application.dataPath, "..", CONSUMER_DEMO_SCENES_FOLDER);
            EnsureDirectoryExists(destFolder);
            
            Debug.Log("[IVX SDK] Installing demo scenes...");
            
            foreach (string sceneFileName in DEMO_SCENES)
            {
                // Skip if excluded
                if (IsExcluded(sceneFileName))
                {
                    results.ScenesSkipped++;
                    continue;
                }
                
                string sourcePath = FindSceneFile(sdkRoot, sceneFileName);
                string destPath = Path.Combine(destFolder, sceneFileName);
                
                if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                {
                    Debug.LogWarning($"[IVX SDK] Scene not found: {sceneFileName}");
                    results.ScenesFailed++;
                    continue;
                }
                
                if (File.Exists(destPath))
                {
                    Debug.Log($"[IVX SDK] Scene already exists: {sceneFileName}");
                    results.ScenesSkipped++;
                    continue;
                }
                
                try
                {
                    File.Copy(sourcePath, destPath, false);
                    
                    // Also copy .meta file
                    string sourceMetaPath = sourcePath + ".meta";
                    string destMetaPath = destPath + ".meta";
                    if (File.Exists(sourceMetaPath) && !File.Exists(destMetaPath))
                    {
                        File.Copy(sourceMetaPath, destMetaPath, false);
                    }
                    
                    Debug.Log($"[IVX SDK] Installed scene: {sceneFileName}");
                    results.ScenesInstalled++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[IVX SDK] Failed to copy scene {sceneFileName}: {ex.Message}");
                    results.ScenesFailed++;
                }
            }
        }
        
        private static void InstallPrefabs(string sdkRoot, InstallationResults results)
        {
            Debug.Log("[IVX SDK] Installing prefabs...");
            
            foreach (var category in PREFABS_TO_COPY)
            {
                string categoryName = category.Key;
                string[] prefabs = category.Value;
                
                // Create category folder
                string destCategoryFolder = Path.Combine(Application.dataPath, "..", CONSUMER_PREFABS_FOLDER, categoryName);
                EnsureDirectoryExists(destCategoryFolder);
                
                foreach (string prefabName in prefabs)
                {
                    if (IsExcluded(prefabName))
                    {
                        results.PrefabsSkipped++;
                        continue;
                    }
                    
                    string sourcePath = FindPrefabFile(sdkRoot, categoryName, prefabName);
                    string destPath = Path.Combine(destCategoryFolder, prefabName);
                    
                    if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                    {
                        // Try alternative paths
                        sourcePath = FindPrefabFileAlternative(sdkRoot, prefabName);
                    }
                    
                    if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                    {
                        Debug.LogWarning($"[IVX SDK] Prefab not found: {prefabName}");
                        results.PrefabsFailed++;
                        continue;
                    }
                    
                    if (File.Exists(destPath))
                    {
                        Debug.Log($"[IVX SDK] Prefab already exists: {prefabName}");
                        results.PrefabsSkipped++;
                        continue;
                    }
                    
                    try
                    {
                        File.Copy(sourcePath, destPath, false);
                        
                        // Also copy .meta file
                        string sourceMetaPath = sourcePath + ".meta";
                        string destMetaPath = destPath + ".meta";
                        if (File.Exists(sourceMetaPath) && !File.Exists(destMetaPath))
                        {
                            File.Copy(sourceMetaPath, destMetaPath, false);
                        }
                        
                        Debug.Log($"[IVX SDK] Installed prefab: {prefabName}");
                        results.PrefabsInstalled++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[IVX SDK] Failed to copy prefab {prefabName}: {ex.Message}");
                        results.PrefabsFailed++;
                    }
                }
            }
        }
        
        private static void CreateResourceConfigs(InstallationResults results)
        {
            Debug.Log("[IVX SDK] Creating resource configurations...");
            
            // Create Resources folder
            string resourcesFolder = Path.Combine(Application.dataPath, "Resources", "IntelliVerseX");
            EnsureDirectoryExists(resourcesFolder);
            
            // Note: ScriptableObject configs need to be created via AssetDatabase
            // This is handled by the SDK Setup Wizard when user runs full setup
            
            results.ConfigsCreated = 0;
        }
        
        private static string FindSceneFile(string sdkRoot, string sceneFileName)
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(sdkRoot, "Samples~", "TestScenes", sceneFileName),
                Path.Combine(sdkRoot, "Scenes", "Tests", sceneFileName),
                Path.Combine(sdkRoot, "Auth", "Scenes", sceneFileName),
                // Development mode paths
                Path.Combine(Application.dataPath, "Scenes", "Tests", sceneFileName)
            };
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        private static string FindPrefabFile(string sdkRoot, string category, string prefabName)
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(sdkRoot, "Prefabs", prefabName),
                Path.Combine(sdkRoot, category, "Prefabs", prefabName),
                Path.Combine(sdkRoot, $"{category}/Prefabs", prefabName)
            };
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        private static string FindPrefabFileAlternative(string sdkRoot, string prefabName)
        {
            // Search common prefab locations
            string[] searchFolders = new string[]
            {
                Path.Combine(sdkRoot, "Prefabs"),
                Path.Combine(sdkRoot, "Auth", "Prefabs"),
                Path.Combine(sdkRoot, "Social", "Prefabs"),
                Path.Combine(sdkRoot, "Social", "Friends", "Prefabs"),
                Path.Combine(sdkRoot, "Leaderboard", "Prefabs"),
                Path.Combine(sdkRoot, "Quiz", "Prefabs"),
                Path.Combine(sdkRoot, "Quiz", "WeeklyQuiz", "Prefabs"),
                Path.Combine(sdkRoot, "QuizUI", "Prefabs"),
                Path.Combine(sdkRoot, "Monetization", "Ads", "Prefabs"),
                Path.Combine(sdkRoot, "Monetization", "Prefabs"),
                Path.Combine(sdkRoot, "MoreOfUs", "Prefabs"),
                Path.Combine(sdkRoot, "UI", "Prefabs"),
                Path.Combine(sdkRoot, "Backend", "Prefabs")
            };
            
            foreach (string folder in searchFolders)
            {
                if (Directory.Exists(folder))
                {
                    string filePath = Path.Combine(folder, prefabName);
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
            }
            
            return null;
        }
        
        private static bool IsExcluded(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            
            foreach (string pattern in EXCLUDED_PATTERNS)
            {
                if (name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        private static void ShowInstallationResults(InstallationResults results)
        {
            string message = "IntelliVerseX SDK Asset Installation Complete!\n\n";
            
            message += "📂 Demo Scenes:\n";
            message += $"   • Installed: {results.ScenesInstalled}\n";
            message += $"   • Skipped (existing): {results.ScenesSkipped}\n";
            if (results.ScenesFailed > 0)
            {
                message += $"   • Failed: {results.ScenesFailed}\n";
            }
            
            message += "\n📦 Prefabs:\n";
            message += $"   • Installed: {results.PrefabsInstalled}\n";
            message += $"   • Skipped (existing): {results.PrefabsSkipped}\n";
            if (results.PrefabsFailed > 0)
            {
                message += $"   • Failed: {results.PrefabsFailed}\n";
            }
            
            message += "\n🔧 Next Steps:\n";
            message += "1. Open the SDK Setup Wizard to configure modules\n";
            message += "2. Test demo scenes in 'IntelliVerseX Demo Scenes' folder\n";
            message += "3. Install required dependencies (Nakama, DOTween, etc.)";
            
            bool hasFailures = results.ScenesFailed > 0 || results.PrefabsFailed > 0;
            
            int choice = EditorUtility.DisplayDialogComplex(
                hasFailures ? "Installation Complete (with warnings)" : "Installation Complete",
                message,
                "Open Setup Wizard",
                "Close",
                "Open Demo Scenes"
            );
            
            switch (choice)
            {
                case 0:
                    IVXSDKSetupWizard.ShowWindow();
                    break;
                case 2:
                    OpenDemoScenesFolder();
                    break;
            }
        }
        
        private static void OpenDemoScenesFolder()
        {
            string folderPath = CONSUMER_DEMO_SCENES_FOLDER;
            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }
        
        #endregion
        
        #region Installation Results
        
        private class InstallationResults
        {
            public int ScenesInstalled;
            public int ScenesSkipped;
            public int ScenesFailed;
            
            public int PrefabsInstalled;
            public int PrefabsSkipped;
            public int PrefabsFailed;
            
            public int ConfigsCreated;
        }
        
        #endregion
    }
}
#endif
