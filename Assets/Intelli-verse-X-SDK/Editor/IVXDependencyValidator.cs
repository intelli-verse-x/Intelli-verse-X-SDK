// IVXDependencyValidator.cs
// Comprehensive dependency validator for IntelliVerseX SDK
// Validates all dependencies, assembly references, and provides detailed report

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Comprehensive dependency validator that checks all SDK dependencies
    /// and provides detailed reports on what's installed and what's missing.
    /// </summary>
    public class IVXDependencyValidator : EditorWindow
    {
        private ValidationReport report;
        private Vector2 scrollPosition;
        private bool showDetails = true;
        
        private class ValidationReport
        {
            public bool allPassed;
            public List<ValidationResult> results = new List<ValidationResult>();
            public int totalChecks;
            public int passedChecks;
            public int failedChecks;
            public int warningChecks;
        }
        
        private class ValidationResult
        {
            public string category;
            public string name;
            public bool passed;
            public bool isWarning;
            public string message;
            public string solution;
        }
        
        // REMOVED: Menu item consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Validate Dependencies", priority = 4)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Dependencies tab
        public static void ShowValidator()
        {
            var window = GetWindow<IVXDependencyValidator>("Dependency Validator");
            window.minSize = new Vector2(700, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            RunValidation();
        }
        
        private void RunValidation()
        {
            report = new ValidationReport();
            
            // Check Unity Package Manager dependencies
            CheckUnityPackages();
            
            // Check external dependencies
            CheckExternalDependencies();
            
            // Check native plugins
            CheckNativePlugins();
            
            // Check optional packages (monetization, IAP, etc.)
            CheckOptionalPackages();
            
            // Check assembly definitions
            CheckAssemblyDefinitions();
            
            // Check file structure
            CheckFileStructure();
            
            // Calculate totals
            report.totalChecks = report.results.Count;
            report.passedChecks = report.results.Count(r => r.passed);
            report.failedChecks = report.results.Count(r => !r.passed && !r.isWarning);
            report.warningChecks = report.results.Count(r => !r.passed && r.isWarning);
            report.allPassed = report.failedChecks == 0;
        }
        
        private void CheckUnityPackages()
        {
            var requiredPackages = new Dictionary<string, string>
            {
                { "com.unity.nuget.newtonsoft-json", "Newtonsoft.Json" },
                { "com.unity.textmeshpro", "TextMeshPro" }
            };
            
            var optionalPackages = new Dictionary<string, string>
            {
                { "com.unity.purchasing", "Unity IAP" },
                { "com.unity.services.levelplay", "LevelPlay SDK" },
                { "com.unity.ads", "Unity Ads" },
                { "com.unity.localization", "Unity Localization" },
                { "com.unity.addressables", "Addressables" }
            };
            
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestContent = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";
            
            foreach (var package in requiredPackages)
            {
                bool installed = manifestContent.Contains($"\"{package.Key}\"");
                report.results.Add(new ValidationResult
                {
                    category = "Unity Packages (Required)",
                    name = package.Value,
                    passed = installed,
                    isWarning = false,
                    message = installed ? $"✅ {package.Value} is installed" : $"❌ {package.Value} is NOT installed",
                    solution = installed ? "" : $"Install via Package Manager: {package.Key}"
                });
            }
            
            foreach (var package in optionalPackages)
            {
                bool installed = manifestContent.Contains($"\"{package.Key}\"");
                report.results.Add(new ValidationResult
                {
                    category = "Unity Packages (Optional)",
                    name = package.Value,
                    passed = installed,
                    isWarning = true,
                    message = installed ? $"✅ {package.Value} is installed" : $"⚠️ {package.Value} is not installed (optional)",
                    solution = installed ? "" : $"Install via Package Manager: {package.Key}"
                });
            }
        }
        
        private void CheckExternalDependencies()
        {
            // Check Nakama
            bool nakamaFolder = Directory.Exists(Path.Combine(Application.dataPath, "Packages/Nakama"));
            bool nakamaAssembly = System.Type.GetType("Nakama.IClient, Nakama") != null;
            report.results.Add(new ValidationResult
            {
                category = "External Dependencies (Required)",
                name = "Nakama Unity SDK",
                passed = nakamaFolder && nakamaAssembly,
                isWarning = false,
                message = (nakamaFolder && nakamaAssembly) ? "✅ Nakama SDK is installed" : "❌ Nakama SDK is NOT installed",
                solution = (nakamaFolder && nakamaAssembly) ? "" : "Download from: https://github.com/heroiclabs/nakama-unity/releases"
            });
            
            // Check Photon PUN2
            bool photonFolder = Directory.Exists(Path.Combine(Application.dataPath, "Photon"));
            bool photonAssembly = System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking") != null;
            report.results.Add(new ValidationResult
            {
                category = "External Dependencies (Required)",
                name = "Photon PUN2",
                passed = photonFolder && photonAssembly,
                isWarning = false,
                message = (photonFolder && photonAssembly) ? "✅ Photon PUN2 is installed" : "❌ Photon PUN2 is NOT installed",
                solution = (photonFolder && photonAssembly) ? "" : "Import from Unity Asset Store"
            });
            
            // Check DOTween
            bool dotweenFolder = Directory.Exists(Path.Combine(Application.dataPath, "Plugins/Demigiant/DOTween"));
            bool dotweenAssembly = System.Type.GetType("DG.Tweening.DOTween, DOTween") != null;
            report.results.Add(new ValidationResult
            {
                category = "External Dependencies (Required)",
                name = "DOTween",
                passed = dotweenFolder || dotweenAssembly,
                isWarning = false,
                message = (dotweenFolder || dotweenAssembly) ? "✅ DOTween is installed" : "❌ DOTween is NOT installed",
                solution = (dotweenFolder || dotweenAssembly) ? "" : "Import from Asset Store or download from http://dotween.demigiant.com"
            });
            
            // Check Google Mobile Ads (Optional)
            bool admobFolder = Directory.Exists(Path.Combine(Application.dataPath, "GoogleMobileAds"));
            report.results.Add(new ValidationResult
            {
                category = "External Dependencies (Optional)",
                name = "Google Mobile Ads",
                passed = admobFolder,
                isWarning = true,
                message = admobFolder ? "✅ Google Mobile Ads is installed" : "⚠️ Google Mobile Ads is not installed (optional)",
                solution = admobFolder ? "" : "Download from: https://github.com/googleads/googleads-mobile-unity/releases"
            });
        }
        
        private void CheckNativePlugins()
        {
            // Check Native Share (git package)
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestContent = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";
            bool nativeShareInstalled = manifestContent.Contains("com.yasirkula.nativeshare");
            
            report.results.Add(new ValidationResult
            {
                category = "Native Plugins (Required)",
                name = "Native Share",
                passed = nativeShareInstalled,
                isWarning = false,
                message = nativeShareInstalled ? "✅ Native Share is installed" : "❌ Native Share is NOT installed",
                solution = nativeShareInstalled ? "" : "Install via Package Manager git URL: https://github.com/yasirkula/UnityNativeShare.git"
            });
            
            // Check Native File Picker
            bool nativeFilePickerFolder = Directory.Exists(Path.Combine(Application.dataPath, "Plugins/NativeFilePicker"));
            bool nativeFilePickerAssembly = System.Type.GetType("NativeFilePicker, NativeFilePicker.Runtime") != null;
            report.results.Add(new ValidationResult
            {
                category = "Native Plugins (Required)",
                name = "Native File Picker",
                passed = nativeFilePickerFolder || nativeFilePickerAssembly,
                isWarning = false,
                message = (nativeFilePickerFolder || nativeFilePickerAssembly) ? "✅ Native File Picker is installed" : "❌ Native File Picker is NOT installed",
                solution = (nativeFilePickerFolder || nativeFilePickerAssembly) ? "" : "Download from: https://github.com/yasirkula/UnityNativeFilePicker/releases"
            });
            
            // Check Apple Sign In
            bool appleAuthFolder = Directory.Exists(Path.Combine(Application.dataPath, "AppleAuth"));
            bool appleAuthAssembly = System.Type.GetType("AppleAuth.IAppleAuthManager, AppleAuth") != null;
            report.results.Add(new ValidationResult
            {
                category = "Native Plugins (Required)",
                name = "Sign in with Apple",
                passed = appleAuthFolder || appleAuthAssembly,
                isWarning = false,
                message = (appleAuthFolder || appleAuthAssembly) ? "✅ Sign in with Apple is installed" : "❌ Sign in with Apple is NOT installed",
                solution = (appleAuthFolder || appleAuthAssembly) ? "" : "Download from: https://github.com/lupidan/apple-signin-unity/releases"
            });
            
            // Check VoxelBusters Essential Kit (Optional)
            bool voxelBustersFolder = Directory.Exists(Path.Combine(Application.dataPath, "Plugins/VoxelBusters/EssentialKit"));
            report.results.Add(new ValidationResult
            {
                category = "Native Plugins (Optional)",
                name = "VoxelBusters Essential Kit",
                passed = voxelBustersFolder,
                isWarning = true,
                message = voxelBustersFolder ? "✅ VoxelBusters Essential Kit is installed" : "⚠️ VoxelBusters Essential Kit is not installed (optional)",
                solution = voxelBustersFolder ? "" : "Import from Asset Store: https://assetstore.unity.com/packages/tools/integration/cross-platform-essential-kit-202287"
            });
        }
        
        private void CheckOptionalPackages()
        {
            // Check Unity Ads
            bool unityAdsInstalled = CheckPackageInManifest("com.unity.ads");
            report.results.Add(new ValidationResult
            {
                category = "Monetization (Optional)",
                name = "Unity Ads",
                passed = unityAdsInstalled,
                isWarning = true,
                message = unityAdsInstalled ? "✅ Unity Ads is installed" : "⚠️ Unity Ads is not installed (optional)",
                solution = unityAdsInstalled ? "" : "Install via: IntelliVerseX > Install All Dependencies or Package Manager"
            });
            
            // Check LevelPlay SDK
            bool levelPlayInstalled = CheckPackageInManifest("com.unity.services.levelplay");
            bool levelPlayFolder = Directory.Exists(Path.Combine(Application.dataPath, "../Library/PackageCache")) && 
                                   Directory.GetDirectories(Path.Combine(Application.dataPath, "../Library/PackageCache"), "com.unity.services.levelplay*").Length > 0;
            report.results.Add(new ValidationResult
            {
                category = "Monetization (Optional)",
                name = "LevelPlay SDK (IronSource)",
                passed = levelPlayInstalled || levelPlayFolder,
                isWarning = true,
                message = (levelPlayInstalled || levelPlayFolder) ? "✅ LevelPlay SDK is installed" : "⚠️ LevelPlay SDK is not installed (optional)",
                solution = (levelPlayInstalled || levelPlayFolder) ? "" : "Install via: IntelliVerseX > Install All Dependencies"
            });
            
            // Check Appodeal
            bool appodealInstalled = CheckPackageInManifest("com.appodeal.mediation");
            bool appodealFolder = Directory.Exists(Path.Combine(Application.dataPath, "Appodeal"));
            report.results.Add(new ValidationResult
            {
                category = "Monetization (Optional)",
                name = "Appodeal Mediation",
                passed = appodealInstalled || appodealFolder,
                isWarning = true,
                message = (appodealInstalled || appodealFolder) ? "✅ Appodeal is installed" : "⚠️ Appodeal is not installed (optional)",
                solution = (appodealInstalled || appodealFolder) ? "" : "Install via git URL: https://github.com/appodeal/appodeal-unity-plugin-upm.git#v3.12.0"
            });
            
            // Check Google Mobile Ads
            bool admobFolder = Directory.Exists(Path.Combine(Application.dataPath, "GoogleMobileAds"));
            report.results.Add(new ValidationResult
            {
                category = "Monetization (Optional)",
                name = "Google Mobile Ads (AdMob)",
                passed = admobFolder,
                isWarning = true,
                message = admobFolder ? "✅ Google Mobile Ads is installed" : "⚠️ Google Mobile Ads is not installed (optional)",
                solution = admobFolder ? "" : "Download from: https://github.com/googleads/googleads-mobile-unity/releases"
            });
            
            // Check Unity IAP
            bool iapInstalled = CheckPackageInManifest("com.unity.purchasing");
            report.results.Add(new ValidationResult
            {
                category = "In-App Purchasing (Optional)",
                name = "Unity IAP",
                passed = iapInstalled,
                isWarning = true,
                message = iapInstalled ? "✅ Unity IAP is installed" : "⚠️ Unity IAP is not installed (optional)",
                solution = iapInstalled ? "" : "Install via: IntelliVerseX > Install All Dependencies"
            });
            
            // Check Unity Localization
            bool localizationInstalled = CheckPackageInManifest("com.unity.localization");
            report.results.Add(new ValidationResult
            {
                category = "Optional Features",
                name = "Unity Localization",
                passed = localizationInstalled,
                isWarning = true,
                message = localizationInstalled ? "✅ Unity Localization is installed" : "⚠️ Unity Localization is not installed (optional)",
                solution = localizationInstalled ? "" : "Install via: IntelliVerseX > Install All Dependencies"
            });
            
            // Check Addressables
            bool addressablesInstalled = CheckPackageInManifest("com.unity.addressables");
            report.results.Add(new ValidationResult
            {
                category = "Optional Features",
                name = "Unity Addressables",
                passed = addressablesInstalled,
                isWarning = true,
                message = addressablesInstalled ? "✅ Unity Addressables is installed" : "⚠️ Unity Addressables is not installed (optional)",
                solution = addressablesInstalled ? "" : "Install via: IntelliVerseX > Install All Dependencies"
            });
        }
        
        private bool CheckPackageInManifest(string packageId)
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (File.Exists(manifestPath))
            {
                string content = File.ReadAllText(manifestPath);
                return content.Contains($"\"{packageId}\"");
            }
            return false;
        }
        
        private void CheckAssemblyDefinitions()
        {
            // Check SDK assembly definitions
            string[] sdkAsmdefs = new string[]
            {
                "Assets/_IntelliVerseXSDK/Core/IntelliVerseX.Core.asmdef",
                "Assets/_IntelliVerseXSDK/Backend/IntelliVerseX.Backend.asmdef",
                "Assets/_IntelliVerseXSDK/Analytics/IntelliVerseX.Analytics.asmdef",
                "Assets/_IntelliVerseXSDK/Quiz/IntelliVerseX.Quiz.asmdef",
                "Assets/_IntelliVerseXSDK/Networking/IntelliVerseX.Networking.asmdef"
            };
            
            foreach (var asmdef in sdkAsmdefs)
            {
                string fullPath = Path.Combine(Application.dataPath, "../", asmdef);
                bool exists = File.Exists(fullPath);
                string name = Path.GetFileNameWithoutExtension(asmdef);
                
                report.results.Add(new ValidationResult
                {
                    category = "Assembly Definitions",
                    name = name,
                    passed = exists,
                    isWarning = false,
                    message = exists ? $"✅ {name} exists" : $"❌ {name} is missing",
                    solution = exists ? "" : $"Assembly definition file is missing: {asmdef}"
                });
            }
            
            // Check for duplicate references
            string backendAsmdef = Path.Combine(Application.dataPath, "_IntelliVerseXSDK/Backend/IntelliVerseX.Backend.asmdef");
            if (File.Exists(backendAsmdef))
            {
                string content = File.ReadAllText(backendAsmdef);
                bool hasDuplicates = content.Contains("69810832b544b46da9804f1af9373521") && content.Contains("\"NakamaRuntime\"");
                
                report.results.Add(new ValidationResult
                {
                    category = "Assembly Definitions",
                    name = "Backend Assembly (Duplicate Check)",
                    passed = !hasDuplicates,
                    isWarning = false,
                    message = hasDuplicates ? "❌ Backend.asmdef has duplicate NakamaRuntime reference" : "✅ No duplicate references",
                    solution = hasDuplicates ? "Remove GUID reference for NakamaRuntime, keep only name reference" : ""
                });
            }
        }
        
        private void CheckFileStructure()
        {
            string[] requiredFolders = new string[]
            {
                "Assets/_IntelliVerseXSDK",
                "Assets/_IntelliVerseXSDK/Core",
                "Assets/_IntelliVerseXSDK/Backend",
                "Assets/_IntelliVerseXSDK/Analytics",
                "Assets/_IntelliVerseXSDK/Quiz",
                "Assets/_IntelliVerseXSDK/Editor"
            };
            
            foreach (var folder in requiredFolders)
            {
                string fullPath = Path.Combine(Application.dataPath, "../", folder);
                bool exists = Directory.Exists(fullPath);
                string name = Path.GetFileName(folder);
                
                report.results.Add(new ValidationResult
                {
                    category = "File Structure",
                    name = name,
                    passed = exists,
                    isWarning = false,
                    message = exists ? $"✅ {name} folder exists" : $"❌ {name} folder is missing",
                    solution = exists ? "" : $"SDK folder structure is incomplete"
                });
            }
            
            // Check for documentation
            string[] docs = new string[]
            {
                "Assets/_IntelliVerseXSDK/DEPENDENCIES.md",
                "Assets/_IntelliVerseXSDK/QUICK_START.md",
                "Assets/_IntelliVerseXSDK/SDK_OVERVIEW.md",
                "Assets/_IntelliVerseXSDK/SDK_DEPENDENCIES_MANIFEST.json"
            };
            
            foreach (var doc in docs)
            {
                string fullPath = Path.Combine(Application.dataPath, "../", doc);
                bool exists = File.Exists(fullPath);
                string name = Path.GetFileName(doc);
                
                report.results.Add(new ValidationResult
                {
                    category = "Documentation",
                    name = name,
                    passed = exists,
                    isWarning = true,
                    message = exists ? $"✅ {name} exists" : $"⚠️ {name} is missing",
                    solution = exists ? "" : "Documentation file is missing"
                });
            }
        }
        
        private void OnGUI()
        {
            // Header
            DrawHeader();
            
            GUILayout.Space(10);
            
            // Summary
            DrawSummary();
            
            GUILayout.Space(10);
            
            // Details toggle
            showDetails = EditorGUILayout.Toggle("Show Detailed Results", showDetails);
            
            GUILayout.Space(10);
            
            // Scroll area with results
            if (showDetails)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawDetailedResults();
                EditorGUILayout.EndScrollView();
            }
            
            GUILayout.FlexibleSpace();
            
            // Footer
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
            EditorGUILayout.LabelField("IntelliVerseX SDK - Dependency Validator", headerStyle);
            
            GUILayout.Space(5);
        }
        
        private void DrawSummary()
        {
            if (report == null) return;
            
            MessageType messageType = report.allPassed ? MessageType.Info : MessageType.Error;
            string summaryText = report.allPassed
                ? $"✅ ALL CHECKS PASSED!\n\nTotal: {report.totalChecks} | Passed: {report.passedChecks} | Warnings: {report.warningChecks}"
                : $"❌ VALIDATION FAILED!\n\nTotal: {report.totalChecks} | Passed: {report.passedChecks} | Failed: {report.failedChecks} | Warnings: {report.warningChecks}";
            
            EditorGUILayout.HelpBox(summaryText, messageType);
        }
        
        private void DrawDetailedResults()
        {
            if (report == null || report.results == null) return;
            
            var categories = report.results.GroupBy(r => r.category);
            
            foreach (var category in categories)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(category.Key, EditorStyles.boldLabel);
                GUILayout.Space(5);
                
                foreach (var result in category)
                {
                    DrawResultItem(result);
                }
            }
        }
        
        private void DrawResultItem(ValidationResult result)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            Color originalColor = GUI.color;
            GUI.color = result.passed ? Color.green : (result.isWarning ? Color.yellow : Color.red);
            GUILayout.Label(result.passed ? "✓" : (result.isWarning ? "⚠" : "✗"), EditorStyles.boldLabel, GUILayout.Width(20));
            GUI.color = originalColor;
            
            // Result info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(result.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(result.message, EditorStyles.miniLabel);
            
            if (!result.passed && !string.IsNullOrEmpty(result.solution))
            {
                EditorGUILayout.LabelField($"💡 Solution: {result.solution}", EditorStyles.wordWrappedMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFooter()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Re-run Validation", GUILayout.Height(35)))
            {
                RunValidation();
            }
            
            if (GUILayout.Button("Export Report", GUILayout.Height(35)))
            {
                ExportReport();
            }
            
            if (GUILayout.Button("Open Setup Wizard", GUILayout.Height(35)))
            {
                IVXSetupWizard.ShowWizard();
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }
        
        private void ExportReport()
        {
            if (report == null) return;
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("IntelliVerseX SDK - Dependency Validation Report");
            sb.AppendLine($"Generated: {System.DateTime.Now}");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine();
            
            sb.AppendLine("SUMMARY");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine($"Total Checks: {report.totalChecks}");
            sb.AppendLine($"Passed: {report.passedChecks}");
            sb.AppendLine($"Failed: {report.failedChecks}");
            sb.AppendLine($"Warnings: {report.warningChecks}");
            sb.AppendLine($"Overall Status: {(report.allPassed ? "PASSED" : "FAILED")}");
            sb.AppendLine();
            
            var categories = report.results.GroupBy(r => r.category);
            foreach (var category in categories)
            {
                sb.AppendLine($"{category.Key}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var result in category)
                {
                    string status = result.passed ? "[PASS]" : (result.isWarning ? "[WARN]" : "[FAIL]");
                    sb.AppendLine($"{status} {result.name}");
                    sb.AppendLine($"  {result.message}");
                    if (!result.passed && !string.IsNullOrEmpty(result.solution))
                    {
                        sb.AppendLine($"  Solution: {result.solution}");
                    }
                    sb.AppendLine();
                }
            }
            
            string reportPath = Path.Combine(Application.dataPath, "_IntelliVerseXSDK/dependency_validation_report.txt");
            File.WriteAllText(reportPath, sb.ToString());
            
            Debug.Log($"Report exported to: {reportPath}");
            EditorUtility.DisplayDialog("Success", $"Report exported to:\n{reportPath}", "OK");
        }
    }
}
#endif
