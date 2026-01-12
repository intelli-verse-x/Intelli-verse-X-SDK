// IVXAutoSetup.cs
// Automatically installs required dependencies when SDK is imported
// This runs ONCE when the SDK is first imported into a new project

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.Linq;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Auto-setup that runs when SDK is imported.
    /// Installs all required UPM packages automatically.
    /// </summary>
    [InitializeOnLoad]
    public static class IVXAutoSetup
    {
        private const string SETUP_COMPLETE_KEY = "IVX_SDK_SETUP_COMPLETE_V3";
        
        // =====================================================
        // ALL REQUIRED UPM PACKAGES - AUTO-INSTALLED
        // =====================================================
        private static readonly string[] REQUIRED_UPM_PACKAGES = new string[]
        {
            // Core Required (without these SDK won't compile)
            "com.unity.nuget.newtonsoft-json",  // JSON serialization
            "com.unity.textmeshpro",             // UI Text
        };
        
        // Optional but recommended UPM packages (installed on request)
        private static readonly string[] OPTIONAL_UPM_PACKAGES = new string[]
        {
            "com.unity.purchasing",              // IAP
            "com.unity.services.levelplay",      // LevelPlay Ads
            "com.unity.ads",                     // Unity Ads
            "com.unity.localization",            // Localization
            "com.unity.addressables",            // Addressables
        };
        
        // Git URL packages (auto-installed)
        private static readonly Dictionary<string, string> GIT_URL_PACKAGES = new Dictionary<string, string>
        {
            { "com.yasirkula.nativeshare", "https://github.com/yasirkula/UnityNativeShare.git" },
        };
        
        private static ListRequest listRequest;
        private static AddRequest addRequest;
        private static Queue<string> packagesToInstall = new Queue<string>();
        private static Queue<KeyValuePair<string, string>> gitPackagesToInstall = new Queue<KeyValuePair<string, string>>();
        private static bool isProcessing = false;
        private static int totalPackages = 0;
        private static int installedCount = 0;
        
        static IVXAutoSetup()
        {
            // Check if setup has already been completed
            if (EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            {
                return;
            }
            
            // Delay to let Unity finish loading
            EditorApplication.delayCall += CheckAndInstallDependencies;
        }
        
        private static void CheckAndInstallDependencies()
        {
            if (isProcessing) return;
            isProcessing = true;
            installedCount = 0;
            
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("[IVX SDK] 🚀 Starting Auto-Setup - Checking dependencies...");
            Debug.Log("═══════════════════════════════════════════════════════════════");
            
            // Start by listing installed packages
            listRequest = Client.List(true);
            EditorApplication.update += OnListProgress;
        }
        
        private static void OnListProgress()
        {
            if (!listRequest.IsCompleted) return;
            
            EditorApplication.update -= OnListProgress;
            
            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"[IVX SDK] ❌ Failed to list packages: {listRequest.Error.message}");
                isProcessing = false;
                return;
            }
            
            // Check which packages are missing
            var installedPackages = new HashSet<string>();
            foreach (var package in listRequest.Result)
            {
                installedPackages.Add(package.name);
            }
            
            // Check required UPM packages
            Debug.Log("\n[IVX SDK] 📦 Checking Required UPM Packages:");
            foreach (var pkg in REQUIRED_UPM_PACKAGES)
            {
                if (!installedPackages.Contains(pkg))
                {
                    packagesToInstall.Enqueue(pkg);
                    Debug.Log($"  ⚠️ Missing: {pkg} - Will install");
                }
                else
                {
                    Debug.Log($"  ✅ Installed: {pkg}");
                }
            }
            
            // Check Git URL packages
            Debug.Log("\n[IVX SDK] 🔗 Checking Git URL Packages:");
            foreach (var kvp in GIT_URL_PACKAGES)
            {
                if (!installedPackages.Contains(kvp.Key))
                {
                    gitPackagesToInstall.Enqueue(kvp);
                    Debug.Log($"  ⚠️ Missing: {kvp.Key} - Will install from Git");
                }
                else
                {
                    Debug.Log($"  ✅ Installed: {kvp.Key}");
                }
            }
            
            totalPackages = packagesToInstall.Count + gitPackagesToInstall.Count;
            
            if (totalPackages > 0)
            {
                Debug.Log($"\n[IVX SDK] 📥 Installing {totalPackages} package(s)...\n");
                InstallNextPackage();
            }
            else
            {
                SetupComplete();
            }
        }
        
        private static void InstallNextPackage()
        {
            // First install regular UPM packages
            if (packagesToInstall.Count > 0)
            {
                string packageToInstall = packagesToInstall.Dequeue();
                installedCount++;
                Debug.Log($"[IVX SDK] [{installedCount}/{totalPackages}] Installing {packageToInstall}...");
                
                addRequest = Client.Add(packageToInstall);
                EditorApplication.update += OnInstallProgress;
                return;
            }
            
            // Then install Git URL packages
            if (gitPackagesToInstall.Count > 0)
            {
                var kvp = gitPackagesToInstall.Dequeue();
                installedCount++;
                Debug.Log($"[IVX SDK] [{installedCount}/{totalPackages}] Installing {kvp.Key} from Git...");
                
                addRequest = Client.Add(kvp.Value);
                EditorApplication.update += OnInstallProgress;
                return;
            }
            
            SetupComplete();
        }
        
        private static void OnInstallProgress()
        {
            if (!addRequest.IsCompleted) return;
            
            EditorApplication.update -= OnInstallProgress;
            
            if (addRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"[IVX SDK] ❌ Failed to install: {addRequest.Error.message}");
            }
            else
            {
                Debug.Log($"[IVX SDK] ✅ Successfully installed: {addRequest.Result.name}");
            }
            
            // Install next package or complete
            InstallNextPackage();
        }
        
        private static void SetupComplete()
        {
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
            isProcessing = false;
            
            Debug.Log("\n═══════════════════════════════════════════════════════════════");
            Debug.Log("[IVX SDK] ✅ AUTO-SETUP COMPLETE!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("");
            Debug.Log("📦 INSTALLED AUTOMATICALLY:");
            Debug.Log("   • Newtonsoft.Json (JSON serialization)");
            Debug.Log("   • TextMeshPro (UI text rendering)");
            Debug.Log("   • Native Share (sharing functionality)");
            Debug.Log("");
            Debug.Log("⚠️ MANUAL INSTALLATION REQUIRED (External SDKs):");
            Debug.Log("   • Nakama Unity SDK - Download: https://github.com/heroiclabs/nakama-unity");
            Debug.Log("   • Photon PUN2 - Asset Store: https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
            Debug.Log("   • DOTween - Download: http://dotween.demigiant.com/download.php");
            Debug.Log("   • Apple Sign-In - Download: https://github.com/lupidan/apple-signin-unity");
            Debug.Log("");
            Debug.Log("📖 For full setup guide: Window → IntelliVerse-X SDK → Dependency Installer");
            Debug.Log("═══════════════════════════════════════════════════════════════\n");
            
            // Show dialog
            EditorUtility.DisplayDialog(
                "IVX SDK Setup Complete",
                "✅ Core dependencies installed!\n\n" +
                "Installed automatically:\n" +
                "• Newtonsoft.Json\n" +
                "• TextMeshPro\n" +
                "• Native Share\n\n" +
                "⚠️ Still need manual install:\n" +
                "• Nakama Unity SDK\n" +
                "• Photon PUN2\n" +
                "• DOTween\n" +
                "• Apple Sign-In (iOS)\n\n" +
                "Use: Window → IntelliVerse-X SDK → Dependency Installer",
                "OK"
            );
        }
        
        /// <summary>
        /// Force re-run of auto-setup
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Re-run Auto Setup", false, 100)]
        public static void ForceRerunSetup()
        {
            EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
            isProcessing = false;
            packagesToInstall.Clear();
            gitPackagesToInstall.Clear();
            CheckAndInstallDependencies();
        }
        
        /// <summary>
        /// Install optional packages
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Install Optional Packages", false, 101)]
        public static void InstallOptionalPackages()
        {
            if (isProcessing)
            {
                EditorUtility.DisplayDialog("Please Wait", "Another installation is in progress.", "OK");
                return;
            }
            
            bool install = EditorUtility.DisplayDialog(
                "Install Optional Packages?",
                "This will install:\n" +
                "• Unity IAP\n" +
                "• Unity Ads\n" +
                "• LevelPlay (IronSource)\n" +
                "• Unity Localization\n" +
                "• Addressables\n\n" +
                "Continue?",
                "Install All",
                "Cancel"
            );
            
            if (install)
            {
                InstallOptionalPackagesInternal();
            }
        }
        
        private static void InstallOptionalPackagesInternal()
        {
            isProcessing = true;
            installedCount = 0;
            
            Debug.Log("[IVX SDK] 📦 Installing optional packages...");
            
            foreach (var pkg in OPTIONAL_UPM_PACKAGES)
            {
                packagesToInstall.Enqueue(pkg);
            }
            
            totalPackages = packagesToInstall.Count;
            InstallNextPackage();
        }
        
        /// <summary>
        /// Check setup status
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Check Setup Status", false, 102)]
        public static void CheckSetupStatus()
        {
            Debug.Log("\n═══════════════════════════════════════════════════════════════");
            Debug.Log("[IVX SDK] 🔍 DEPENDENCY STATUS CHECK");
            Debug.Log("═══════════════════════════════════════════════════════════════\n");
            
            // Check required packages
            var missing = new List<string>();
            var installed = new List<string>();
            
            // Check Newtonsoft
            if (System.Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json") != null)
                installed.Add("Newtonsoft.Json");
            else
                missing.Add("Newtonsoft.Json");
            
            // Check TMP
            if (System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null)
                installed.Add("TextMeshPro");
            else
                missing.Add("TextMeshPro");
            
            // Check Native Share
            if (System.Type.GetType("NativeShare, NativeShare.Runtime") != null)
                installed.Add("Native Share");
            else
                missing.Add("Native Share");
            
            // Check Nakama
            if (System.Type.GetType("Nakama.Client, NakamaRuntime") != null)
                installed.Add("Nakama Unity SDK");
            else
                missing.Add("Nakama Unity SDK");
            
            // Check Photon
            if (System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking") != null)
                installed.Add("Photon PUN2");
            else
                missing.Add("Photon PUN2");
            
            // Check DOTween
            if (System.Type.GetType("DG.Tweening.DOTween, DOTween") != null)
                installed.Add("DOTween");
            else
                missing.Add("DOTween");
            
            Debug.Log("✅ INSTALLED:");
            foreach (var pkg in installed)
                Debug.Log($"   • {pkg}");
            
            if (missing.Count > 0)
            {
                Debug.Log("\n❌ MISSING:");
                foreach (var pkg in missing)
                    Debug.Log($"   • {pkg}");
                
                Debug.Log("\n⚠️ Use: Window → IntelliVerse-X SDK → Dependency Installer");
            }
            else
            {
                Debug.Log("\n🎉 All dependencies installed!");
            }
            
            Debug.Log("\n═══════════════════════════════════════════════════════════════\n");
        }
        
        /// <summary>
        /// Open dependency installer
        /// </summary>
        [MenuItem("IntelliVerse-X SDK/Open Dependency Installer", false, 200)]
        public static void OpenDependencyInstaller()
        {
            EditorApplication.ExecuteMenuItem("Window/IntelliVerse-X SDK/Dependency Installer");
        }
    }
}
#endif
