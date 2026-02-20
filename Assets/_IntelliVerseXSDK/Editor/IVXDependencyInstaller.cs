// IVXDependencyInstaller.cs
// Comprehensive automated dependency installer for IntelliVerseX SDK
// Installs ALL required dependencies from manifest

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Comprehensive dependency installer that reads SDK_DEPENDENCIES_MANIFEST.json
    /// and installs all required packages automatically.
    /// </summary>
    public class IVXDependencyInstaller : EditorWindow
    {
        private const string MANIFEST_PATH = "Assets/_IntelliVerseXSDK/SDK_DEPENDENCIES_MANIFEST.json";
        
        private DependencyManifest manifest;
        private Vector2 scrollPosition;
        private Dictionary<string, bool> installStatus = new Dictionary<string, bool>();
        private Dictionary<string, string> installMessages = new Dictionary<string, string>();
        private bool isInstalling = false;
        private Queue<PackageInstallRequest> installQueue = new Queue<PackageInstallRequest>();
        private AddRequest currentRequest;
        
        private class PackageInstallRequest
        {
            public string packageId;
            public string packageName;
            public string installCommand;
            public string description;
        }
        
        [System.Serializable]
        private class DependencyManifest
        {
            public string sdk_name;
            public string sdk_version;
            public Dependencies dependencies;
            public List<string> install_order;
            public Verification verification;
        }
        
        [System.Serializable]
        private class Dependencies
        {
            public Dictionary<string, DependencyInfo> required;
            public Dictionary<string, DependencyInfo> external_required;
            public Dictionary<string, DependencyInfo> native_plugins;
            public Dictionary<string, DependencyInfo> monetization;
            public Dictionary<string, DependencyInfo> iap;
            public Dictionary<string, DependencyInfo> optional;
        }
        
        [System.Serializable]
        private class DependencyInfo
        {
            public string version;
            public string source;
            public string install_method;
            public string description;
            public List<string> used_by;
            public string install_command;
            public string download_url;
            public string install_instructions;
            public string verify_path;
            public string verify_assembly;
            public bool optional;
        }
        
        [System.Serializable]
        private class Verification
        {
            public List<string> assemblies;
            public List<string> paths;
            public List<string> packages;
        }
        
        // REMOVED: Menu item consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Install All Dependencies", priority = 3)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Dependencies tab
        public static void ShowInstaller()
        {
            var window = GetWindow<IVXDependencyInstaller>("Dependency Installer");
            window.minSize = new Vector2(700, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadManifest();
            CheckAllDependencies();
        }
        
        private void LoadManifest()
        {
            try
            {
                string manifestPath = Path.Combine(Application.dataPath, "../", MANIFEST_PATH);
                if (File.Exists(manifestPath))
                {
                    string json = File.ReadAllText(manifestPath);
                    manifest = JsonUtility.FromJson<DependencyManifest>(json);
                }
                else
                {
                    Debug.LogError($"Manifest not found at: {manifestPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load manifest: {e.Message}");
            }
        }
        
        private void CheckAllDependencies()
        {
            installStatus.Clear();
            installMessages.Clear();
            
            if (manifest == null || manifest.dependencies == null) return;
            
            // Check required packages
            if (manifest.dependencies.required != null)
            {
                foreach (var dep in manifest.dependencies.required)
                {
                    bool installed = CheckPackageInstalled(dep.Key);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : "❌ Not Installed";
                }
            }
            
            // Check external required
            if (manifest.dependencies.external_required != null)
            {
                foreach (var dep in manifest.dependencies.external_required)
                {
                    bool installed = CheckExternalDependency(dep.Value);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : "⚠️ Manual Install Required";
                }
            }
            
            // Check monetization
            if (manifest.dependencies.monetization != null)
            {
                foreach (var dep in manifest.dependencies.monetization)
                {
                    bool installed = CheckPackageInstalled(dep.Key);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : "⚠️ Optional";
                }
            }
            
            // Check native plugins
            if (manifest.dependencies.native_plugins != null)
            {
                foreach (var dep in manifest.dependencies.native_plugins)
                {
                    bool installed = dep.Value.install_method == "git_url" 
                        ? CheckPackageInstalled(dep.Key) 
                        : CheckExternalDependency(dep.Value);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : (dep.Value.install_method == "git_url" ? "❌ Not Installed" : "⚠️ Manual Install Required");
                }
            }
            
            // Check monetization
            if (manifest.dependencies.monetization != null)
            {
                foreach (var dep in manifest.dependencies.monetization)
                {
                    bool installed = dep.Value.install_method == "unitypackage" 
                        ? CheckExternalDependency(dep.Value)
                        : CheckPackageInstalled(dep.Key);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : "⚠️ Optional - Can Auto-Install";
                }
            }
            
            // Check IAP
            if (manifest.dependencies.iap != null)
            {
                foreach (var dep in manifest.dependencies.iap)
                {
                    bool installed = CheckPackageInstalled(dep.Key);
                    installStatus[dep.Key] = installed;
                    installMessages[dep.Key] = installed ? "✅ Installed" : "⚠️ Optional - Can Auto-Install";
                }
            }
        }
        
        private bool CheckPackageInstalled(string packageId)
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (File.Exists(manifestPath))
            {
                string content = File.ReadAllText(manifestPath);
                return content.Contains($"\"{packageId}\"");
            }
            return false;
        }
        
        private bool CheckExternalDependency(DependencyInfo info)
        {
            if (!string.IsNullOrEmpty(info.verify_path))
            {
                string fullPath = Path.Combine(Application.dataPath, info.verify_path.Replace("Assets/", ""));
                return Directory.Exists(fullPath);
            }
            
            if (!string.IsNullOrEmpty(info.verify_assembly))
            {
                var type = System.Type.GetType(info.verify_assembly + ", " + info.verify_assembly);
                return type != null;
            }
            
            return false;
        }
        
        private void OnGUI()
        {
            if (manifest == null)
            {
                EditorGUILayout.HelpBox("Failed to load dependency manifest!", MessageType.Error);
                if (GUILayout.Button("Reload Manifest"))
                {
                    LoadManifest();
                    CheckAllDependencies();
                }
                return;
            }
            
            // Header
            DrawHeader();
            
            GUILayout.Space(10);
            
            // Scroll area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Required dependencies
            DrawDependencySection("Required Dependencies", manifest.dependencies.required, false);
            
            // External required
            DrawDependencySection("External Required (Manual Install)", manifest.dependencies.external_required, true);
            
            // Native plugins
            DrawDependencySection("Native Plugins", manifest.dependencies.native_plugins, false);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Optional Dependencies (Recommended for Production)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These packages are optional but highly recommended for production apps. They can be auto-installed!", MessageType.Info);
            
            // Monetization
            DrawDependencySection("Monetization SDKs", manifest.dependencies.monetization, false);
            
            // IAP
            DrawDependencySection("In-App Purchasing", manifest.dependencies.iap, false);
            
            // Optional features
            DrawDependencySection("Optional Features (Localization, Addressables)", manifest.dependencies.optional, false);
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            
            // Footer with install button
            DrawFooter();
        }
        
        private void DrawHeader()
        {
            GUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField($"{manifest.sdk_name} - Dependency Installer", headerStyle);
            
            GUILayout.Space(5);
            EditorGUILayout.LabelField($"Version: {manifest.sdk_version}", EditorStyles.centeredGreyMiniLabel);
            
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This tool will install all required dependencies for the SDK.\n\n" +
                "⚠️ Some dependencies require manual installation (Nakama, Photon, DOTween).\n" +
                "✅ Unity Package Manager dependencies will be installed automatically.",
                MessageType.Info
            );
        }
        
        private void DrawDependencySection(string title, Dictionary<string, DependencyInfo> dependencies, bool isManual)
        {
            if (dependencies == null || dependencies.Count == 0) return;
            
            GUILayout.Space(15);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            foreach (var dep in dependencies)
            {
                DrawDependencyItem(dep.Key, dep.Value, isManual);
            }
        }
        
        private void DrawDependencyItem(string packageId, DependencyInfo info, bool isManual)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            bool isInstalled = installStatus.ContainsKey(packageId) && installStatus[packageId];
            Color originalColor = GUI.color;
            GUI.color = isInstalled ? Color.green : (isManual ? Color.yellow : Color.red);
            GUILayout.Label(isInstalled ? "✓" : (isManual ? "⚠" : "✗"), EditorStyles.boldLabel, GUILayout.Width(20));
            GUI.color = originalColor;
            
            // Package info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(packageId, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Version: {info.version} | {info.description}", EditorStyles.miniLabel);
            
            if (info.used_by != null && info.used_by.Count > 0)
            {
                EditorGUILayout.LabelField($"Used by: {string.Join(", ", info.used_by)}", EditorStyles.miniLabel);
            }
            
            // Status message
            if (installMessages.ContainsKey(packageId))
            {
                EditorGUILayout.LabelField(installMessages[packageId], EditorStyles.wordWrappedMiniLabel);
            }
            
            // Install instructions for manual packages
            if (isManual && !isInstalled)
            {
                if (!string.IsNullOrEmpty(info.install_instructions))
                {
                    EditorGUILayout.LabelField($"📖 {info.install_instructions}", EditorStyles.wordWrappedMiniLabel);
                }
                
                if (!string.IsNullOrEmpty(info.download_url))
                {
                    if (GUILayout.Button($"Open Download Page", GUILayout.Height(25)))
                    {
                        Application.OpenURL(info.download_url);
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFooter()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh Status", GUILayout.Height(35)))
            {
                CheckAllDependencies();
            }
            
            GUI.enabled = !isInstalling;
            if (GUILayout.Button("Install Unity Packages", GUILayout.Height(35)))
            {
                InstallUnityPackages();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (isInstalling)
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox("Installing packages... Please wait.", MessageType.None);
            }
            
            GUILayout.Space(10);
        }
        
        private void InstallUnityPackages()
        {
            installQueue.Clear();
            
            // Queue required packages
            if (manifest.dependencies.required != null)
            {
                foreach (var dep in manifest.dependencies.required)
                {
                    if (!installStatus.ContainsKey(dep.Key) || !installStatus[dep.Key])
                    {
                        if (dep.Value.install_method == "package_manager" && !string.IsNullOrEmpty(dep.Value.install_command))
                        {
                            installQueue.Enqueue(new PackageInstallRequest
                            {
                                packageId = dep.Key,
                                packageName = dep.Key,
                                installCommand = dep.Value.install_command,
                                description = dep.Value.description
                            });
                        }
                    }
                }
            }
            
            // Queue native plugins (git URL packages)
            if (manifest.dependencies != null)
            {
                var nativePlugins = GetDependencyCategory("native_plugins");
                if (nativePlugins != null)
                {
                    foreach (var dep in nativePlugins)
                    {
                        if (!installStatus.ContainsKey(dep.Key) || !installStatus[dep.Key])
                        {
                            if (dep.Value.install_method == "git_url" && !string.IsNullOrEmpty(dep.Value.install_command))
                            {
                                installQueue.Enqueue(new PackageInstallRequest
                                {
                                    packageId = dep.Key,
                                    packageName = dep.Key,
                                    installCommand = dep.Value.install_command,
                                    description = dep.Value.description
                                });
                            }
                        }
                    }
                }
            }
            
            // Queue monetization packages (optional but can auto-install)
            var monetization = GetDependencyCategory("monetization");
            if (monetization != null)
            {
                foreach (var dep in monetization)
                {
                    if (!installStatus.ContainsKey(dep.Key) || !installStatus[dep.Key])
                    {
                        if ((dep.Value.install_method == "package_manager" || dep.Value.install_method == "git_url") 
                            && !string.IsNullOrEmpty(dep.Value.install_command))
                        {
                            installQueue.Enqueue(new PackageInstallRequest
                            {
                                packageId = dep.Key,
                                packageName = dep.Key,
                                installCommand = dep.Value.install_command,
                                description = dep.Value.description
                            });
                        }
                    }
                }
            }
            
            // Queue IAP packages
            var iap = GetDependencyCategory("iap");
            if (iap != null)
            {
                foreach (var dep in iap)
                {
                    if (!installStatus.ContainsKey(dep.Key) || !installStatus[dep.Key])
                    {
                        if (dep.Value.install_method == "package_manager" && !string.IsNullOrEmpty(dep.Value.install_command))
                        {
                            installQueue.Enqueue(new PackageInstallRequest
                            {
                                packageId = dep.Key,
                                packageName = dep.Key,
                                installCommand = dep.Value.install_command,
                                description = dep.Value.description
                            });
                        }
                    }
                }
            }
            
            // Queue optional packages
            var optional = GetDependencyCategory("optional");
            if (optional != null)
            {
                foreach (var dep in optional)
                {
                    if (!installStatus.ContainsKey(dep.Key) || !installStatus[dep.Key])
                    {
                        if (dep.Value.install_method == "package_manager" && !string.IsNullOrEmpty(dep.Value.install_command))
                        {
                            installQueue.Enqueue(new PackageInstallRequest
                            {
                                packageId = dep.Key,
                                packageName = dep.Key,
                                installCommand = dep.Value.install_command,
                                description = dep.Value.description
                            });
                        }
                    }
                }
            }
            
            if (installQueue.Count > 0)
            {
                isInstalling = true;
                ProcessNextPackage();
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "All Unity Package Manager dependencies are already installed!", "OK");
            }
        }
        
        private Dictionary<string, DependencyInfo> GetDependencyCategory(string category)
        {
            try
            {
                var field = manifest.dependencies.GetType().GetField(category);
                if (field != null)
                {
                    return field.GetValue(manifest.dependencies) as Dictionary<string, DependencyInfo>;
                }
            }
            catch
            {
                // Category doesn't exist
            }
            return null;
        }
        
        private void ProcessNextPackage()
        {
            if (installQueue.Count == 0)
            {
                isInstalling = false;
                CheckAllDependencies();
                EditorUtility.DisplayDialog("Success", "All packages installed successfully!", "OK");
                return;
            }
            
            var package = installQueue.Dequeue();
            Debug.Log($"[SDK Installer] Installing {package.packageName}...");
            
            currentRequest = Client.Add(package.installCommand);
            EditorApplication.update += CheckInstallProgress;
        }
        
        private void CheckInstallProgress()
        {
            if (currentRequest != null && currentRequest.IsCompleted)
            {
                EditorApplication.update -= CheckInstallProgress;
                
                if (currentRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[SDK Installer] ✅ Package installed successfully");
                }
                else
                {
                    Debug.LogError($"[SDK Installer] ❌ Failed to install package: {currentRequest.Error.message}");
                }
                
                currentRequest = null;
                ProcessNextPackage();
            }
        }
        
        private void OnDestroy()
        {
            EditorApplication.update -= CheckInstallProgress;
        }
    }
}
#endif
