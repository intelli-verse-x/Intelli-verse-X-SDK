#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Canonical dependency + first-import setup helper (revamp P3).
    /// Replaces IVXAutoSetup, IVXDependencyChecker/Installer/Validator, and IVXFeatureSetup.
    /// </summary>
    [InitializeOnLoad]
    public static class IVXDependencies
    {
        private const string SetupCompleteKey = "IVX_SDK_SETUP_COMPLETE_V3";
        private const string DailyCheckPrefKey = "IVXDependencyCheck_LastCheck";
        private const int DailyCheckIntervalDays = 1;

        private static readonly string[] RequiredUpmPackages =
        {
            "com.unity.nuget.newtonsoft-json",
            // Unity 6+: TMP ships inside com.unity.ugui (com.unity.textmeshpro is not installable on 6000.3).
            "com.unity.ugui"
        };

        private static readonly string[] OptionalUpmPackages =
        {
            "com.unity.purchasing",
            "com.unity.services.levelplay",
            "com.unity.ads",
            "com.unity.localization",
            "com.unity.addressables"
        };

        private static readonly Dictionary<string, string> GitUrlPackages = new Dictionary<string, string>
        {
            { "com.yasirkula.nativeshare", "https://github.com/yasirkula/UnityNativeShare.git" }
        };

        private static readonly Dictionary<string, string> AssetStoreLinks = new Dictionary<string, string>
        {
            { "Nakama", "https://assetstore.unity.com/packages/tools/network/nakama-81338" },
            { "Photon PUN2", "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922" },
            { "Apple Sign-In", "https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088" },
            { "DOTween", "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676" }
        };

        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static readonly Queue<string> _packagesToInstall = new Queue<string>();
        private static readonly Queue<KeyValuePair<string, string>> _gitPackagesToInstall =
            new Queue<KeyValuePair<string, string>>();
        private static bool _isProcessing;
        private static int _totalPackages;
        private static int _installedCount;

        static IVXDependencies()
        {
            if (!EditorPrefs.GetBool(SetupCompleteKey, false))
            {
                EditorApplication.delayCall += CheckAndInstallRequired;
            }

            EditorApplication.delayCall += MaybeWarnMissingCoreDeps;
        }

        /// <summary>Snapshot used by Advanced Setup UI.</summary>
        public struct Status
        {
            public bool Newtonsoft;
            public bool TextMeshPro;
            public bool Nakama;
            public bool NativeShare;
            public bool Purchasing;
            public bool LevelPlay;
            public bool AllCoreReady;
        }

        public static Status GetStatus()
        {
            var s = new Status
            {
                Newtonsoft = TypeExists("Newtonsoft.Json.JsonConvert, Newtonsoft.Json")
                             || TypeExists("Newtonsoft.Json.JsonConvert"),
                TextMeshPro = TypeExists("TMPro.TextMeshProUGUI, Unity.TextMeshPro")
                              || TypeExists("TMPro.TextMeshProUGUI"),
                Nakama = TypeExists("Nakama.Client, NakamaRuntime")
                         || TypeExists("Nakama.IClient, Nakama")
                         || TypeExists("Nakama.Client"),
                NativeShare = TypeExists("NativeShare, NativeShare.Runtime")
                              || TypeExists("Sych.ShareAssets.Runtime.Share"),
                Purchasing = TypeExists("UnityEngine.Purchasing.UnityPurchasing, UnityEngine.Purchasing"),
                LevelPlay = TypeExists("Unity.Services.LevelPlay.LevelPlay, Unity.LevelPlay")
            };
            s.AllCoreReady = s.Newtonsoft && s.TextMeshPro && s.Nakama;
            return s;
        }

        public static bool TypeExists(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return false;
            try
            {
                if (Type.GetType(fullTypeName) != null)
                    return true;

                // Assembly-qualified may fail; scan loaded assemblies by simple name.
                string simple = fullTypeName;
                int comma = fullTypeName.IndexOf(',');
                if (comma > 0)
                    simple = fullTypeName.Substring(0, comma).Trim();

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (asm.GetType(simple, false) != null)
                            return true;
                    }
                    catch
                    {
                        // ignore unloadable assemblies
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static void CheckAndInstallRequired()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            _installedCount = 0;
            _packagesToInstall.Clear();
            _gitPackagesToInstall.Clear();

            Debug.Log("[IVX] Checking required packages...");
            _listRequest = Client.List(true);
            EditorApplication.update += OnListProgress;
        }

        public static void ForceRerunSetup()
        {
            EditorPrefs.DeleteKey(SetupCompleteKey);
            _isProcessing = false;
            _packagesToInstall.Clear();
            _gitPackagesToInstall.Clear();
            CheckAndInstallRequired();
        }

        public static void InstallOptionalPackages()
        {
            if (_isProcessing)
            {
                EditorUtility.DisplayDialog("Please wait", "Another installation is in progress.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Install optional packages?",
                    "Installs Unity IAP, Ads, LevelPlay, Localization, and Addressables.\n\nContinue?",
                    "Install",
                    "Cancel"))
            {
                return;
            }

            _isProcessing = true;
            _installedCount = 0;
            _packagesToInstall.Clear();
            foreach (var pkg in OptionalUpmPackages)
                _packagesToInstall.Enqueue(pkg);
            _totalPackages = _packagesToInstall.Count;
            InstallNextPackage();
        }

        public static void OpenAssetStoreLinks()
        {
            foreach (var kvp in AssetStoreLinks)
                Application.OpenURL(kvp.Value);
            Debug.Log("[IVX] Opened Asset Store pages for: " + string.Join(", ", AssetStoreLinks.Keys));
        }

        public static void LogStatusToConsole()
        {
            var s = GetStatus();
            Debug.Log("[IVX] Dependency status — Newtonsoft=" + s.Newtonsoft
                      + " TMP=" + s.TextMeshPro
                      + " Nakama=" + s.Nakama
                      + " Share=" + s.NativeShare
                      + " IAP=" + s.Purchasing
                      + " LevelPlay=" + s.LevelPlay);
            if (!s.AllCoreReady)
                Debug.LogWarning("[IVX] Core deps incomplete. Open IntelliVerseX → Advanced Setup.");
        }

        public static void ValidateAssembliesToConsole()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== IntelliVerseX dependency validation ===");
            var s = GetStatus();
            AppendCheck(report, "Newtonsoft.Json", s.Newtonsoft, "UPM: com.unity.nuget.newtonsoft-json");
            AppendCheck(report, "TextMeshPro", s.TextMeshPro, "UPM: com.unity.ugui (TMP is bundled in Unity 6+)");
            AppendCheck(report, "Nakama", s.Nakama, "Asset Store or GitHub heroiclabs/nakama-unity");
            AppendCheck(report, "Native Share (optional)", s.NativeShare, "Git: yasirkula/UnityNativeShare");
            AppendCheck(report, "Unity Purchasing (optional)", s.Purchasing, "UPM: com.unity.purchasing");
            AppendCheck(report, "LevelPlay (optional)", s.LevelPlay, "UPM: com.unity.services.levelplay");

            bool nakamaFolder = Directory.Exists("Assets/Packages/Nakama")
                                || Directory.Exists("Assets/Nakama")
                                || Directory.Exists("Packages/com.heroiclabs.nakama-unity");
            AppendCheck(report, "Nakama package folder", nakamaFolder || s.Nakama,
                "Expected under Assets/Nakama, Assets/Packages/Nakama, or UPM");

            Debug.Log(report.ToString());
        }

        private static void AppendCheck(System.Text.StringBuilder sb, string name, bool ok, string hint)
        {
            sb.AppendLine((ok ? "[OK] " : "[MISSING] ") + name + (ok ? "" : " — " + hint));
        }

        private static void MaybeWarnMissingCoreDeps()
        {
            string last = EditorPrefs.GetString(DailyCheckPrefKey, "");
            bool shouldCheck = true;
            if (!string.IsNullOrEmpty(last) && DateTime.TryParse(last, out DateTime lastCheck))
                shouldCheck = (DateTime.Now - lastCheck).TotalDays >= DailyCheckIntervalDays;

            if (!shouldCheck)
                return;

            EditorPrefs.SetString(DailyCheckPrefKey, DateTime.Now.ToString("o"));
            var s = GetStatus();
            if (!s.Newtonsoft || !s.Nakama)
            {
                Debug.LogWarning("[IVX] Missing core dependencies. Open IntelliVerseX → Advanced Setup (or Control Center).");
            }
        }

        private static void OnListProgress()
        {
            if (!_listRequest.IsCompleted)
                return;

            EditorApplication.update -= OnListProgress;

            if (_listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError("[IVX] Failed to list packages: " + _listRequest.Error.message);
                _isProcessing = false;
                return;
            }

            var installed = new HashSet<string>();
            foreach (var package in _listRequest.Result)
                installed.Add(package.name);

            foreach (var pkg in RequiredUpmPackages)
            {
                if (installed.Contains(pkg))
                    continue;

                // Unity 6 already has TMP via ugui — never try to Add com.unity.textmeshpro.
                if (pkg == "com.unity.ugui" && GetStatus().TextMeshPro)
                    continue;

                _packagesToInstall.Enqueue(pkg);
            }

            foreach (var kvp in GitUrlPackages)
            {
                if (!installed.Contains(kvp.Key))
                    _gitPackagesToInstall.Enqueue(kvp);
            }

            _totalPackages = _packagesToInstall.Count + _gitPackagesToInstall.Count;
            if (_totalPackages > 0)
            {
                Debug.Log("[IVX] Installing " + _totalPackages + " package(s)...");
                InstallNextPackage();
            }
            else
            {
                SetupComplete(showDialog: false);
            }
        }

        private static void InstallNextPackage()
        {
            if (_packagesToInstall.Count > 0)
            {
                string pkg = _packagesToInstall.Dequeue();
                _installedCount++;
                Debug.Log("[IVX] [" + _installedCount + "/" + _totalPackages + "] Installing " + pkg + "...");
                _addRequest = Client.Add(pkg);
                EditorApplication.update += OnInstallProgress;
                return;
            }

            if (_gitPackagesToInstall.Count > 0)
            {
                var kvp = _gitPackagesToInstall.Dequeue();
                _installedCount++;
                Debug.Log("[IVX] [" + _installedCount + "/" + _totalPackages + "] Installing " + kvp.Key + " from Git...");
                _addRequest = Client.Add(kvp.Value);
                EditorApplication.update += OnInstallProgress;
                return;
            }

            SetupComplete(showDialog: true);
        }

        private static void OnInstallProgress()
        {
            if (!_addRequest.IsCompleted)
                return;

            EditorApplication.update -= OnInstallProgress;

            if (_addRequest.Status == StatusCode.Failure)
            {
                string err = _addRequest.Error != null ? _addRequest.Error.message : "unknown";
                // Soft-skip obsolete TMP package id on Unity 6+
                if (err.IndexOf("com.unity.textmeshpro", StringComparison.OrdinalIgnoreCase) >= 0)
                    Debug.LogWarning("[IVX] Skipped obsolete TextMeshPro package id (use com.unity.ugui on Unity 6+): " + err);
                else
                    Debug.LogError("[IVX] Install failed: " + err);
            }
            else
                Debug.Log("[IVX] Installed: " + _addRequest.Result.name);

            InstallNextPackage();
        }

        private static void SetupComplete(bool showDialog)
        {
            EditorPrefs.SetBool(SetupCompleteKey, true);
            _isProcessing = false;
            Debug.Log("[IVX] Required package pass finished. Use Control Center for Game ID, Advanced Setup for extras.");

            if (!showDialog)
                return;

            int choice = EditorUtility.DisplayDialogComplex(
                "IntelliVerseX setup",
                "Core UPM packages are installed.\n\n"
                + "Still install manually if missing:\n"
                + "• Nakama Unity SDK (backend)\n"
                + "• DOTween (optional UI motion)\n\n"
                + "Open Control Center next?",
                "Control Center",
                "Later",
                "Asset Store");

            if (choice == 0)
                IVXControlCenter.ShowWindow();
            else if (choice == 2)
                OpenAssetStoreLinks();
        }
    }
}
#endif
