// File: IVXSDKExporter.cs
// Purpose: SDK Export utility with dependency management
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
    /// SDK Export utility for packaging and exporting the SDK to other projects.
    /// Handles dependency resolution, validation, and package creation.
    /// </summary>
    public class IVXSDKExporter : EditorWindow
    {
        #region Constants

        private const string SDK_ROOT = "Assets/_IntelliVerseXSDK";
        private const string EXPORT_FOLDER = "IVX_SDK_Export";

        #endregion

        #region Export Configuration

        [Serializable]
        public class ExportConfig
        {
            public bool includeCore = true;
            public bool includeBackend = true;
            public bool includeIdentity = true;
            public bool includeNetworking = true;
            public bool includeStorage = true;
            public bool includeQuiz = true;
            public bool includeQuizUI = true;
            public bool includeLeaderboard = true;
            public bool includeSocial = true;
            public bool includeLocalization = true;
            public bool includeIAP = true;
            public bool includeMonetization = true;
            public bool includeAnalytics = true;
            public bool includeUI = true;
            public bool includeExamples = true;
            public bool includeDocumentation = true;
            public bool includeEditor = true;
            public bool includeIntroScene = true;
            
            public string exportPath = "";
            public string version = "1.0.0";
        }

        private ExportConfig config = new ExportConfig();
        private Vector2 scrollPosition;
        private List<DependencyInfo> detectedDependencies = new List<DependencyInfo>();
        private List<string> validationErrors = new List<string>();
        private bool dependenciesScanned = false;

        #endregion

        #region Dependency Info

        [Serializable]
        public class DependencyInfo
        {
            public string name;
            public string packageId;
            public string version;
            public string source; // "UPM", "AssetStore", "Manual"
            public bool isRequired;
            public bool isInstalled;
            public string installUrl;
            public string notes;
        }

        /// <summary>
        /// All SDK dependencies
        /// </summary>
        public static readonly List<DependencyInfo> AllDependencies = new List<DependencyInfo>
        {
            // Required dependencies
            new DependencyInfo
            {
                name = "TextMeshPro",
                packageId = "com.unity.textmeshpro",
                version = "3.0.6",
                source = "UPM",
                isRequired = true,
                installUrl = "com.unity.textmeshpro",
                notes = "Required for UI text rendering"
            },
            new DependencyInfo
            {
                name = "Unity Purchasing",
                packageId = "com.unity.purchasing",
                version = "4.9.3",
                source = "UPM",
                isRequired = true,
                installUrl = "com.unity.purchasing",
                notes = "Required for IAP functionality"
            },
            new DependencyInfo
            {
                name = "Newtonsoft Json",
                packageId = "com.unity.nuget.newtonsoft-json",
                version = "3.2.1",
                source = "UPM",
                isRequired = true,
                installUrl = "com.unity.nuget.newtonsoft-json",
                notes = "Required for JSON serialization"
            },
            new DependencyInfo
            {
                name = "Unity Localization",
                packageId = "com.unity.localization",
                version = "1.4.5",
                source = "UPM",
                isRequired = false,
                installUrl = "com.unity.localization",
                notes = "Optional - for advanced localization features"
            },
            
            // External dependencies
            // NOTE: Nakama SDK is NOT required - SDK uses custom forked backend
            new DependencyInfo
            {
                name = "Photon PUN2",
                packageId = "com.photonengine.pun",
                version = "2.0+",
                source = "AssetStore",
                isRequired = false,
                installUrl = "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922",
                notes = "Optional - for multiplayer features"
            },
            new DependencyInfo
            {
                name = "DOTween",
                packageId = "com.demigiant.dotween",
                version = "1.2+",
                source = "AssetStore",
                isRequired = false,
                installUrl = "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676",
                notes = "Optional - for animations (intro scene)"
            },
            new DependencyInfo
            {
                name = "LevelPlay (IronSource)",
                packageId = "com.unity.services.levelplay",
                version = "8.0+",
                source = "UPM",
                isRequired = false,
                installUrl = "com.unity.services.levelplay",
                notes = "Optional - for ad mediation"
            },
            new DependencyInfo
            {
                name = "Appodeal",
                packageId = "com.appodeal.mediation",
                version = "3.0+",
                source = "Manual",
                isRequired = false,
                installUrl = "https://docs.appodeal.com/unity/get-started",
                notes = "Optional - alternative ad mediation"
            }
        };

        #endregion

        #region Menu Items

        [MenuItem("IntelliVerse-X SDK/Export SDK/Export Wizard", false, 500)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXSDKExporter>("SDK Exporter");
            window.minSize = new Vector2(550, 700);
            window.Show();
        }

        [MenuItem("IntelliVerse-X SDK/Export SDK/Check Export Dependencies", false, 501)]
        public static void CheckDependenciesMenu()
        {
            var result = CheckAllDependencies();
            EditorUtility.DisplayDialog("SDK Dependencies",
                result.ToString(), "OK");
        }

        #endregion

        #region GUI

        private void OnEnable()
        {
            ScanDependencies();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawDependenciesSection();
            DrawModuleSelection();
            DrawExportOptions();
            DrawValidation();
            DrawExportButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            GUILayout.Label("📦 IntelliVerseX SDK Exporter", headerStyle);
            GUILayout.Label("Export SDK for use in other Unity projects", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawDependenciesSection()
        {
            EditorGUILayout.LabelField("📋 Dependencies", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "These dependencies are required for the SDK to work properly. " +
                "Make sure they are installed in your target project.",
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Required dependencies
            EditorGUILayout.LabelField("Required:", EditorStyles.miniBoldLabel);
            foreach (var dep in detectedDependencies.Where(d => d.isRequired))
            {
                DrawDependencyRow(dep);
            }

            EditorGUILayout.Space(5);

            // Optional dependencies
            EditorGUILayout.LabelField("Optional:", EditorStyles.miniBoldLabel);
            foreach (var dep in detectedDependencies.Where(d => !d.isRequired))
            {
                DrawDependencyRow(dep);
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("🔄 Refresh Dependencies", GUILayout.Height(25)))
            {
                ScanDependencies();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawDependencyRow(DependencyInfo dep)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            GUILayout.Label(dep.isInstalled ? "✅" : "❌", GUILayout.Width(25));
            
            // Name and version
            EditorGUILayout.LabelField($"{dep.name} ({dep.version})", GUILayout.Width(200));
            
            // Source
            EditorGUILayout.LabelField(dep.source, EditorStyles.miniLabel, GUILayout.Width(80));
            
            // Install button
            if (!dep.isInstalled)
            {
                if (GUILayout.Button("Install", GUILayout.Width(60)))
                {
                    InstallDependency(dep);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModuleSelection()
        {
            EditorGUILayout.LabelField("🧩 Modules to Export", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // Core modules (always required)
            EditorGUILayout.LabelField("Core (Required):", EditorStyles.miniBoldLabel);
            EditorGUI.BeginDisabledGroup(true);
            config.includeCore = EditorGUILayout.Toggle("Core", true);
            config.includeIdentity = EditorGUILayout.Toggle("Identity (UserSession, API)", true);
            config.includeNetworking = EditorGUILayout.Toggle("Networking", true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            // Backend modules
            EditorGUILayout.LabelField("Backend:", EditorStyles.miniBoldLabel);
            config.includeBackend = EditorGUILayout.Toggle("Backend (Nakama)", config.includeBackend);
            config.includeStorage = EditorGUILayout.Toggle("Storage (Secure)", config.includeStorage);

            EditorGUILayout.Space(5);

            // Feature modules
            EditorGUILayout.LabelField("Features:", EditorStyles.miniBoldLabel);
            config.includeQuiz = EditorGUILayout.Toggle("Quiz System", config.includeQuiz);
            config.includeQuizUI = EditorGUILayout.Toggle("Quiz UI", config.includeQuizUI);
            config.includeLeaderboard = EditorGUILayout.Toggle("Leaderboard", config.includeLeaderboard);
            config.includeSocial = EditorGUILayout.Toggle("Social (Friends)", config.includeSocial);
            config.includeLocalization = EditorGUILayout.Toggle("Localization", config.includeLocalization);

            EditorGUILayout.Space(5);

            // Monetization modules
            EditorGUILayout.LabelField("Monetization:", EditorStyles.miniBoldLabel);
            config.includeIAP = EditorGUILayout.Toggle("In-App Purchases", config.includeIAP);
            config.includeMonetization = EditorGUILayout.Toggle("Ads System", config.includeMonetization);
            config.includeAnalytics = EditorGUILayout.Toggle("Analytics", config.includeAnalytics);

            EditorGUILayout.Space(5);

            // UI and extras
            EditorGUILayout.LabelField("UI & Extras:", EditorStyles.miniBoldLabel);
            config.includeUI = EditorGUILayout.Toggle("UI Components", config.includeUI);
            config.includeExamples = EditorGUILayout.Toggle("Examples", config.includeExamples);
            config.includeDocumentation = EditorGUILayout.Toggle("Documentation", config.includeDocumentation);
            config.includeEditor = EditorGUILayout.Toggle("Editor Tools", config.includeEditor);
            config.includeIntroScene = EditorGUILayout.Toggle("Intro Scene Assets", config.includeIntroScene);
            
            config.includeIntroScene = EditorGUILayout.Toggle("Include Intro Scene", config.includeIntroScene);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawExportOptions()
        {
            EditorGUILayout.LabelField("⚙️ Export Options", EditorStyles.boldLabel);

            config.version = EditorGUILayout.TextField("Version:", config.version);

            EditorGUILayout.BeginHorizontal();
            config.exportPath = EditorGUILayout.TextField("Export Path:", config.exportPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                config.exportPath = EditorUtility.SaveFolderPanel(
                    "Select Export Folder",
                    string.IsNullOrEmpty(config.exportPath) ? Application.dataPath + "/.." : config.exportPath,
                    EXPORT_FOLDER);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawValidation()
        {
            if (validationErrors.Count > 0)
            {
                EditorGUILayout.LabelField("⚠️ Validation Issues", EditorStyles.boldLabel);
                
                foreach (var error in validationErrors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Warning);
                }

                EditorGUILayout.Space(5);
            }
        }

        private void DrawExportButtons()
        {
            EditorGUILayout.LabelField("📤 Export Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔍 Validate", GUILayout.Height(30)))
            {
                ValidateExport();
            }

            if (GUILayout.Button("📋 Generate Dependency List", GUILayout.Height(30)))
            {
                GenerateDependencyManifest();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📦 Export as Unity Package", GUILayout.Height(35)))
            {
                ExportAsUnityPackage();
            }

            if (GUILayout.Button("📁 Export as Folder", GUILayout.Height(35)))
            {
                ExportAsFolder();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("📖 View Export Guide", GUILayout.Height(30)))
            {
                ShowExportGuide();
            }
        }

        #endregion

        #region Dependency Scanning

        private void ScanDependencies()
        {
            detectedDependencies.Clear();

            foreach (var dep in AllDependencies)
            {
                var copy = new DependencyInfo
                {
                    name = dep.name,
                    packageId = dep.packageId,
                    version = dep.version,
                    source = dep.source,
                    isRequired = dep.isRequired,
                    installUrl = dep.installUrl,
                    notes = dep.notes,
                    isInstalled = IsDependencyInstalled(dep)
                };
                detectedDependencies.Add(copy);
            }

            dependenciesScanned = true;
        }

        private bool IsDependencyInstalled(DependencyInfo dep)
        {
            switch (dep.source)
            {
                case "UPM":
                    // Check manifest.json
                    var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                    if (File.Exists(manifestPath))
                    {
                        var manifest = File.ReadAllText(manifestPath);
                        return manifest.Contains(dep.packageId);
                    }
                    return false;

                case "AssetStore":
                case "Manual":
                    // Check for specific indicators
                    if (dep.name == "Nakama SDK")
                        return Directory.Exists("Assets/Nakama") || 
                               AssetDatabase.FindAssets("t:Script Nakama.Client").Length > 0;
                    
                    if (dep.name == "Photon PUN2")
                        return Directory.Exists("Assets/Photon");
                    
                    if (dep.name == "DOTween")
                        return Directory.Exists("Assets/Plugins/Demigiant") ||
                               AssetDatabase.FindAssets("t:Script DOTween").Length > 0;
                    
                    if (dep.name == "Appodeal")
                        return Directory.Exists("Assets/Appodeal");
                    
                    return false;

                default:
                    return false;
            }
        }

        private void InstallDependency(DependencyInfo dep)
        {
            if (dep.source == "UPM")
            {
                UnityEditor.PackageManager.Client.Add(dep.installUrl);
                EditorUtility.DisplayDialog("Installing...",
                    $"Installing {dep.name} via Package Manager. Check the console for progress.", "OK");
            }
            else
            {
                Application.OpenURL(dep.installUrl);
            }
        }

        public static DependencyCheckResult CheckAllDependencies()
        {
            var result = new DependencyCheckResult();

            foreach (var dep in AllDependencies)
            {
                bool installed = IsDependencyInstalledStatic(dep);
                
                if (installed)
                    result.InstalledDependencies.Add(dep);
                else if (dep.isRequired)
                    result.MissingRequired.Add(dep);
                else
                    result.MissingOptional.Add(dep);
            }

            return result;
        }

        private static bool IsDependencyInstalledStatic(DependencyInfo dep)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (dep.source == "UPM" && File.Exists(manifestPath))
            {
                var manifest = File.ReadAllText(manifestPath);
                return manifest.Contains(dep.packageId);
            }
            
            // Check folder existence for manual installs
            if (dep.name == "Nakama SDK")
                return Directory.Exists(Path.Combine(Application.dataPath, "Nakama"));
            if (dep.name == "Photon PUN2")
                return Directory.Exists(Path.Combine(Application.dataPath, "Photon"));
            if (dep.name == "DOTween")
                return Directory.Exists(Path.Combine(Application.dataPath, "Plugins", "Demigiant"));
                
            return false;
        }

        #endregion

        #region Validation

        private void ValidateExport()
        {
            validationErrors.Clear();

            // Check export path
            if (string.IsNullOrEmpty(config.exportPath))
            {
                validationErrors.Add("Export path is not set");
            }

            // Check required dependencies
            foreach (var dep in detectedDependencies.Where(d => d.isRequired && !d.isInstalled))
            {
                validationErrors.Add($"Required dependency not installed: {dep.name}");
            }

            // Check SDK folder exists
            if (!AssetDatabase.IsValidFolder(SDK_ROOT))
            {
                validationErrors.Add("SDK folder not found: " + SDK_ROOT);
            }

            // Check for APIManager
            if (!File.Exists(Path.Combine(Application.dataPath, "_IntelliVerseXSDK/Identity/APIManager.cs")))
            {
                validationErrors.Add("APIManager.cs not found - API calls will not work");
            }

            // Check for UserSessionManager
            if (!File.Exists(Path.Combine(Application.dataPath, "_IntelliVerseXSDK/Identity/UserSessionManager.cs")))
            {
                validationErrors.Add("UserSessionManager.cs not found - authentication will not work");
            }

            if (validationErrors.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Passed",
                    "All checks passed. Ready to export!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Issues",
                    $"Found {validationErrors.Count} issues. See the wizard for details.", "OK");
            }
        }

        #endregion

        #region Export Methods

        private void ExportAsUnityPackage()
        {
            ValidateExport();
            if (validationErrors.Count > 0 && !EditorUtility.DisplayDialog("Export Warning",
                "There are validation issues. Continue anyway?", "Yes", "No"))
            {
                return;
            }

            var paths = GetExportPaths();
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No paths to export", "OK");
                return;
            }

            string packagePath = string.IsNullOrEmpty(config.exportPath)
                ? EditorUtility.SaveFilePanel("Export SDK Package", "", $"IntelliVerseX_SDK_v{config.version}.unitypackage", "unitypackage")
                : Path.Combine(config.exportPath, $"IntelliVerseX_SDK_v{config.version}.unitypackage");

            if (string.IsNullOrEmpty(packagePath))
                return;

            try
            {
                AssetDatabase.ExportPackage(paths.ToArray(), packagePath, 
                    ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
                
                EditorUtility.DisplayDialog("Export Complete",
                    $"SDK exported to:\n{packagePath}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }

        private void ExportAsFolder()
        {
            ValidateExport();
            if (validationErrors.Count > 0 && !EditorUtility.DisplayDialog("Export Warning",
                "There are validation issues. Continue anyway?", "Yes", "No"))
            {
                return;
            }

            string targetPath = string.IsNullOrEmpty(config.exportPath)
                ? EditorUtility.SaveFolderPanel("Select Export Folder", "", "IntelliVerseX_SDK")
                : Path.Combine(config.exportPath, $"IntelliVerseX_SDK_v{config.version}");

            if (string.IsNullOrEmpty(targetPath))
                return;

            try
            {
                var sourcePath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK");
                
                if (Directory.Exists(targetPath))
                {
                    if (!EditorUtility.DisplayDialog("Overwrite?",
                        "Target folder exists. Overwrite?", "Yes", "No"))
                        return;
                    
                    Directory.Delete(targetPath, true);
                }

                CopyDirectory(sourcePath, targetPath);

                // Generate readme
                GenerateExportReadme(targetPath);

                EditorUtility.DisplayDialog("Export Complete",
                    $"SDK exported to:\n{targetPath}", "OK");

                // Open folder
                System.Diagnostics.Process.Start(targetPath);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Error", ex.Message, "OK");
            }
        }

        private List<string> GetExportPaths()
        {
            var paths = new List<string>();

            // Always include core
            paths.Add(SDK_ROOT + "/Core");
            paths.Add(SDK_ROOT + "/Identity");
            paths.Add(SDK_ROOT + "/Networking");

            if (config.includeBackend) paths.Add(SDK_ROOT + "/Backend");
            if (config.includeStorage) paths.Add(SDK_ROOT + "/Storage");
            if (config.includeQuiz) paths.Add(SDK_ROOT + "/Quiz");
            if (config.includeQuizUI) paths.Add(SDK_ROOT + "/QuizUI");
            if (config.includeLeaderboard) paths.Add(SDK_ROOT + "/Leaderboard");
            if (config.includeSocial) paths.Add(SDK_ROOT + "/Social");
            if (config.includeLocalization) paths.Add(SDK_ROOT + "/Localization");
            if (config.includeIAP) paths.Add(SDK_ROOT + "/IAP");
            if (config.includeMonetization) paths.Add(SDK_ROOT + "/Monetization");
            if (config.includeAnalytics) paths.Add(SDK_ROOT + "/Analytics");
            if (config.includeUI) paths.Add(SDK_ROOT + "/UI");
            if (config.includeExamples) paths.Add(SDK_ROOT + "/Examples");
            if (config.includeDocumentation) paths.Add(SDK_ROOT + "/Documentation");
            if (config.includeEditor) paths.Add(SDK_ROOT + "/Editor");
            if (config.includeIntroScene) paths.Add(SDK_ROOT + "/IntroScene");

            // Include package.json and readme
            paths.Add(SDK_ROOT + "/package.json");
            paths.Add(SDK_ROOT + "/README.md");
            paths.Add(SDK_ROOT + "/CHANGELOG.md");
            paths.Add(SDK_ROOT + "/INTEGRATION_GUIDE.md");

            // Filter to existing paths
            return paths.Where(p => AssetDatabase.IsValidFolder(p) || File.Exists(p.Replace("Assets/", Application.dataPath + "/"))).ToList();
        }

        private void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
            {
                var fileName = Path.GetFileName(file);
                if (fileName.EndsWith(".meta")) continue; // Skip meta files for folder export
                
                File.Copy(file, Path.Combine(target, fileName), true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var dirName = Path.GetFileName(dir);
                
                // Skip archived docs
                if (dirName == "_archived_docs") continue;
                
                CopyDirectory(dir, Path.Combine(target, dirName));
            }
        }

        private void GenerateDependencyManifest()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save Dependency Manifest",
                "",
                "IVX_SDK_Dependencies.md",
                "md");

            if (string.IsNullOrEmpty(path))
                return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# IntelliVerseX SDK Dependencies");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"SDK Version: {config.version}");
            sb.AppendLine();
            sb.AppendLine("## Required Dependencies");
            sb.AppendLine();
            sb.AppendLine("| Package | Version | Source | Install |");
            sb.AppendLine("|---------|---------|--------|---------|");

            foreach (var dep in AllDependencies.Where(d => d.isRequired))
            {
                sb.AppendLine($"| {dep.name} | {dep.version} | {dep.source} | `{dep.installUrl}` |");
            }

            sb.AppendLine();
            sb.AppendLine("## Optional Dependencies");
            sb.AppendLine();
            sb.AppendLine("| Package | Version | Source | Install | Notes |");
            sb.AppendLine("|---------|---------|--------|---------|-------|");

            foreach (var dep in AllDependencies.Where(d => !d.isRequired))
            {
                sb.AppendLine($"| {dep.name} | {dep.version} | {dep.source} | `{dep.installUrl}` | {dep.notes} |");
            }

            sb.AppendLine();
            sb.AppendLine("## Installation Instructions");
            sb.AppendLine();
            sb.AppendLine("### UPM Packages");
            sb.AppendLine("1. Open Window → Package Manager");
            sb.AppendLine("2. Click '+' → Add package by name");
            sb.AppendLine("3. Enter the package ID from the table above");
            sb.AppendLine();
            sb.AppendLine("### Git Packages (Nakama)");
            sb.AppendLine("1. Open Window → Package Manager");
            sb.AppendLine("2. Click '+' → Add package from git URL");
            sb.AppendLine("3. Enter: `https://github.com/heroiclabs/nakama-unity.git?path=/Packages/Nakama`");
            sb.AppendLine();
            sb.AppendLine("### Asset Store Packages");
            sb.AppendLine("Download from Unity Asset Store and import into your project.");

            File.WriteAllText(path, sb.ToString());
            EditorUtility.DisplayDialog("Manifest Generated",
                $"Dependency manifest saved to:\n{path}", "OK");
        }

        private void GenerateExportReadme(string targetPath)
        {
            var readmePath = Path.Combine(targetPath, "INSTALL.md");
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("# IntelliVerseX SDK Installation");
            sb.AppendLine();
            sb.AppendLine($"Version: {config.version}");
            sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("## Quick Install");
            sb.AppendLine();
            sb.AppendLine("1. Copy this folder to your Unity project's `Assets/` folder");
            sb.AppendLine("2. Install required dependencies (see below)");
            sb.AppendLine("3. Open `IntelliVerseX → SDK Setup Wizard`");
            sb.AppendLine("4. Click 'Setup All Modules'");
            sb.AppendLine();
            sb.AppendLine("## Required Dependencies");
            sb.AppendLine();
            
            foreach (var dep in AllDependencies.Where(d => d.isRequired))
            {
                sb.AppendLine($"- **{dep.name}** ({dep.version})");
                sb.AppendLine($"  - Source: {dep.source}");
                sb.AppendLine($"  - Install: `{dep.installUrl}`");
                sb.AppendLine();
            }

            sb.AppendLine("## Configuration");
            sb.AppendLine();
            sb.AppendLine("1. Create a configuration asset: `IntelliVerseX → SDK Setup Wizard → Settings → Create Configuration Asset`");
            sb.AppendLine("2. Set your Game ID and other settings");
            sb.AppendLine("3. Configure your Nakama server credentials if using a custom backend");

            File.WriteAllText(readmePath, sb.ToString());
        }

        private void ShowExportGuide()
        {
            EditorUtility.DisplayDialog("SDK Export Guide",
                "To export and use the SDK in another project:\n\n" +
                "1. Click 'Export as Unity Package' to create a .unitypackage file\n" +
                "2. In your new project, import the package via Assets → Import Package\n" +
                "3. Install all required dependencies (see dependency list)\n" +
                "4. Open IntelliVerseX → SDK Setup Wizard\n" +
                "5. Configure your game settings\n" +
                "6. Click 'Setup All Modules' to auto-wire everything\n\n" +
                "For detailed instructions, see INTEGRATION_GUIDE.md",
                "OK");
        }

        #endregion

        #region Data Classes

        public class DependencyCheckResult
        {
            public List<DependencyInfo> InstalledDependencies { get; } = new List<DependencyInfo>();
            public List<DependencyInfo> MissingRequired { get; } = new List<DependencyInfo>();
            public List<DependencyInfo> MissingOptional { get; } = new List<DependencyInfo>();

            public bool IsValid => MissingRequired.Count == 0;

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"SDK Dependencies Check");
                sb.AppendLine($"Status: {(IsValid ? "✅ Ready" : "❌ Missing Required")}");
                sb.AppendLine();
                sb.AppendLine($"Installed: {InstalledDependencies.Count}");
                sb.AppendLine($"Missing Required: {MissingRequired.Count}");
                sb.AppendLine($"Missing Optional: {MissingOptional.Count}");

                if (MissingRequired.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Missing Required:");
                    foreach (var dep in MissingRequired)
                        sb.AppendLine($"  ❌ {dep.name}");
                }

                return sb.ToString();
            }
        }

        #endregion
    }
}
