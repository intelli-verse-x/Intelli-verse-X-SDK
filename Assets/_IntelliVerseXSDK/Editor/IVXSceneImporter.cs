// IVXSceneImporter.cs
// Automatically copies test scenes to consumer project's Assets folder when SDK is imported
// Scenes are copied to: Assets/IntelliVerseX Scenes/

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Automatically imports test scenes to consumer project when SDK package is installed.
    /// Scenes are copied to Assets/IntelliVerseX Scenes/ folder (outside the package).
    /// </summary>
    [InitializeOnLoad]
    public static class IVXSceneImporter
    {
        private const string SCENES_IMPORTED_KEY = "IVX_SCENES_IMPORTED_V3";
        private const string CONSUMER_SCENES_FOLDER = "Assets/IntelliVerseX Scenes";
        private const string PACKAGE_NAME = "com.intelliversex.sdk";
        
        // Test scenes to copy (relative to SDK root)
        private static readonly string[] TEST_SCENES = new string[]
        {
            "Auth/Scenes/IVX_AuthDemo.unity",
            "Auth/Scenes/Game.unity",
            "IntroScene/IntroScene.unity"
        };
        
        static IVXSceneImporter()
        {
            // Delay to let Unity finish loading
            EditorApplication.delayCall += CheckAndImportScenes;
        }
        
        /// <summary>
        /// Checks if scenes need to be imported and copies them to consumer project.
        /// </summary>
        private static void CheckAndImportScenes()
        {
            // Check if scenes have already been imported
            if (EditorPrefs.GetBool(SCENES_IMPORTED_KEY, false))
            {
                return;
            }
            
            // Only run if SDK is installed as UPM package (not in development mode)
            if (!IsUPMInstall())
            {
                Debug.Log("[IVX Scene Importer] SDK is in development mode. Skipping scene import.");
                return;
            }
            
            // Find SDK root path
            string sdkRoot = GetSDKRootPath();
            if (string.IsNullOrEmpty(sdkRoot))
            {
                Debug.LogWarning("[IVX Scene Importer] Could not find SDK root path. Scenes will not be imported.");
                return;
            }
            
            // Import scenes
            ImportScenes(sdkRoot);
        }
        
        /// <summary>
        /// Checks if SDK is installed as UPM package.
        /// </summary>
        private static bool IsUPMInstall()
        {
            // Check if package exists in Packages folder or PackageCache
            string packagesPath = Path.Combine(Application.dataPath, "..", "Packages", PACKAGE_NAME);
            if (Directory.Exists(packagesPath))
            {
                return true;
            }
            
            // Check Library/PackageCache
            string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            if (Directory.Exists(packageCachePath))
            {
                var dirs = Directory.GetDirectories(packageCachePath, $"{PACKAGE_NAME}@*");
                if (dirs.Length > 0)
                {
                    return true;
                }
            }
            
            // Check manifest.json
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                string manifestContent = File.ReadAllText(manifestPath);
                if (manifestContent.Contains($"\"{PACKAGE_NAME}\""))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the physical file system path to the SDK root.
        /// </summary>
        private static string GetSDKRootPath()
        {
            // Check Library/PackageCache first (most common for Git URL installs)
            string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            if (Directory.Exists(packageCachePath))
            {
                var dirs = Directory.GetDirectories(packageCachePath, $"{PACKAGE_NAME}@*");
                if (dirs.Length > 0)
                {
                    return dirs[0];
                }
            }
            
            // Check local Packages folder (for local development packages)
            string localPackagePath = Path.Combine(Application.dataPath, "..", "Packages", PACKAGE_NAME);
            if (Directory.Exists(localPackagePath))
            {
                return localPackagePath;
            }
            
            return null;
        }
        
        /// <summary>
        /// Imports test scenes from SDK package to consumer project.
        /// </summary>
        private static void ImportScenes(string sdkRoot)
        {
            try
            {
                // Create destination folder
                string destFolder = Path.Combine(Application.dataPath, "..", CONSUMER_SCENES_FOLDER);
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                
                int copiedCount = 0;
                int skippedCount = 0;
                
                Debug.Log("═══════════════════════════════════════════════════════════════");
                Debug.Log("[IVX Scene Importer] 🎬 Importing test scenes...");
                Debug.Log("═══════════════════════════════════════════════════════════════");
                
                foreach (string sceneRelativePath in TEST_SCENES)
                {
                    string sourcePath = Path.Combine(sdkRoot, sceneRelativePath);
                    string sceneFileName = Path.GetFileName(sceneRelativePath);
                    string destPath = Path.Combine(destFolder, sceneFileName);
                    
                    // Check if source exists
                    if (!File.Exists(sourcePath))
                    {
                        Debug.LogWarning($"[IVX Scene Importer] ⚠️ Scene not found: {sceneRelativePath}");
                        skippedCount++;
                        continue;
                    }
                    
                    // Check if destination already exists
                    if (File.Exists(destPath))
                    {
                        Debug.Log($"[IVX Scene Importer] ⏭️ Scene already exists: {sceneFileName}");
                        skippedCount++;
                        continue;
                    }
                    
                    // Copy scene file
                    File.Copy(sourcePath, destPath, false);
                    Debug.Log($"[IVX Scene Importer] ✅ Copied: {sceneFileName}");
                    copiedCount++;
                    
                    // Copy .meta file if it exists
                    string sourceMetaPath = sourcePath + ".meta";
                    string destMetaPath = destPath + ".meta";
                    if (File.Exists(sourceMetaPath) && !File.Exists(destMetaPath))
                    {
                        File.Copy(sourceMetaPath, destMetaPath, false);
                    }
                }
                
                // Refresh AssetDatabase to make Unity recognize the new files
                AssetDatabase.Refresh();
                
                Debug.Log("═══════════════════════════════════════════════════════════════");
                Debug.Log($"[IVX Scene Importer] ✅ Import complete!");
                Debug.Log($"   • Copied: {copiedCount} scene(s)");
                Debug.Log($"   • Skipped: {skippedCount} scene(s)");
                Debug.Log($"   • Location: {CONSUMER_SCENES_FOLDER}");
                Debug.Log("═══════════════════════════════════════════════════════════════");
                
                // Mark as imported
                EditorPrefs.SetBool(SCENES_IMPORTED_KEY, true);
                
                // Show notification
                if (copiedCount > 0)
                {
                    EditorUtility.DisplayDialog(
                        "IntelliVerseX Scenes Imported",
                        $"{copiedCount} test scene(s) have been copied to:\n\n{CONSUMER_SCENES_FOLDER}\n\n" +
                        "You can now open these scenes from your Assets folder.",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVX Scene Importer] ❌ Error importing scenes: {ex.Message}");
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// Force re-import of scenes (for testing or manual re-import).
        /// </summary>
        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Re-import Test Scenes", false, 300)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Test Scenes tab
        
        public static void ForceReimportScenes()
        {
            EditorPrefs.DeleteKey(SCENES_IMPORTED_KEY);
            
            if (!IsUPMInstall())
            {
                EditorUtility.DisplayDialog(
                    "Not Available",
                    "Scene import is only available when SDK is installed as a UPM package.\n\n" +
                    "In development mode, scenes are already in Assets/_IntelliVerseXSDK/",
                    "OK"
                );
                return;
            }
            
            string sdkRoot = GetSDKRootPath();
            if (string.IsNullOrEmpty(sdkRoot))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Could not find SDK root path. Please ensure the SDK package is properly installed.",
                    "OK"
                );
                return;
            }
            
            ImportScenes(sdkRoot);
        }
        
        /// <summary>
        /// Opens the IntelliVerseX Scenes folder in the Project window.
        /// </summary>
        // [MenuItem("IntelliVerse-X SDK/Open Test Scenes Folder", false, 301)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Test Scenes tab
        public static void OpenScenesFolder()
        {
            string folderPath = CONSUMER_SCENES_FOLDER;
            
            // Check if folder exists
            if (!Directory.Exists(Path.Combine(Application.dataPath, "..", folderPath)))
            {
                EditorUtility.DisplayDialog(
                    "Folder Not Found",
                    $"The test scenes folder does not exist yet:\n\n{folderPath}\n\n" +
                    "Scenes will be automatically imported when the SDK package is installed.",
                    "OK"
                );
                return;
            }
            
            // Select and ping the folder
            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }
    }
}
#endif
