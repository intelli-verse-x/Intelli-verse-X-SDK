using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Localization;
using IntelliVerseX.Monetization;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Complete example of game bootstrap using IntelliVerse-X SDK.
    /// 
    /// Instructions:
    /// 1. Create empty GameObject in your first scene (e.g., "Bootstrap")
    /// 2. Attach this script
    /// 3. Create IntelliVerseXConfig asset (Assets → Create → IntelliVerse-X → SDK Config)
    /// 4. Assign config to inspector
    /// 5. Run scene
    /// 
    /// The SDK will:
    /// - Initialize all modules
    /// - Auto-detect device language
    /// - Generate user identity
    /// - Initialize ads (if enabled)
    /// - Call OnSDKReady() when complete
    /// </summary>
    public class CompleteGameBootstrap : MonoBehaviour
    {
        [Header("SDK Configuration")]
        [SerializeField] 
        [Tooltip("Create via: Assets → Create → IntelliVerse-X → SDK Config")]
        private IntelliVerseXConfig sdkConfig;

        [Header("Scene Management")]
        [SerializeField] 
        [Tooltip("Scene to load after SDK initialization")]
        private string mainMenuSceneName = "MainMenu";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            // Validate configuration
            if (sdkConfig == null)
            {
                Debug.LogError("[Bootstrap] SDK Config is not assigned! Please assign it in the inspector.");
                return;
            }

            if (!sdkConfig.IsValid())
            {
                Debug.LogError("[Bootstrap] SDK Config is invalid! Check Game ID and Game Name.");
                return;
            }

            // Initialize SDK
            InitializeSDK();
        }

        private void InitializeSDK()
        {
            if (showDebugLogs)
                Debug.Log($"[Bootstrap] Initializing IntelliVerse-X SDK for '{sdkConfig.gameName}'...");

            // Initialize SDK with configuration
            IntelliVerseXManager.Initialize(sdkConfig);

            // Subscribe to events
            IntelliVerseXManager.Instance.OnReady += OnSDKReady;
            IntelliVerseXManager.Instance.OnError += OnSDKError;

            if (showDebugLogs)
                Debug.Log("[Bootstrap] SDK initialization started. Waiting for ready event...");
        }

        private void OnSDKReady()
        {
            if (showDebugLogs)
            {
                Debug.Log("[Bootstrap] ✅ SDK Ready!");
                LogSDKStatus();
            }

            // Optional: Subscribe to language changes
            IVXLanguageManager.OnLanguageChanged += OnLanguageChanged;

            // Optional: Show debug info
            if (showDebugLogs)
            {
                ShowWelcomeMessage();
            }

            // Load main menu
            LoadMainMenu();
        }

        private void OnSDKError(string errorMessage)
        {
            Debug.LogError($"[Bootstrap] ❌ SDK Initialization Error: {errorMessage}");
            
            // Optional: Show error UI to user
            // ShowErrorScreen(errorMessage);
        }

        private void OnLanguageChanged(string newLanguage)
        {
            if (showDebugLogs)
                Debug.Log($"[Bootstrap] Language changed to: {IVXLanguageManager.GetLanguageName(newLanguage)} ({newLanguage})");

            // Reload UI, update texts, etc.
        }

        private void LoadMainMenu()
        {
            if (showDebugLogs)
                Debug.Log($"[Bootstrap] Loading main menu scene: {mainMenuSceneName}");

            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.LogWarning("[Bootstrap] Main menu scene name is not set!");
            }
        }

        private void LogSDKStatus()
        {
            Debug.Log("=== IntelliVerse-X SDK Status ===");
            Debug.Log($"SDK Version: {IntelliVerseXManager.SDKVersion}");
            Debug.Log($"Game: {sdkConfig.gameName} ({sdkConfig.gameId})");
            Debug.Log($"SDK Version: {IntelliVerseXConfig.version}");
            Debug.Log("");
            
            Debug.Log("--- User Identity ---");
            // Debug.Log($"User ID: {IntelliVerseXIdentity.UserId}"); // Instance-based, not static
            Debug.Log($"Username: {IntelliVerseXIdentity.Username}");
            Debug.Log($"Device ID: {IntelliVerseXIdentity.DeviceId}");
            Debug.Log("");

            Debug.Log("--- Localization ---");
            Debug.Log($"Current Language: {IVXLanguageManager.GetLanguageName(IVXLanguageManager.CurrentLanguage)} ({IVXLanguageManager.CurrentLanguage})");
            Debug.Log($"Supported Languages: {string.Join(", ", IVXLanguageManager.SupportedLanguages)}");
            Debug.Log("");

            Debug.Log("--- Modules ---");
            Debug.Log($"Ads Enabled: {sdkConfig.enableAds}");
            Debug.Log($"IAP Enabled: {sdkConfig.enableIAP}");
            Debug.Log($"Multiplayer Enabled: {sdkConfig.enablePhotonMultiplayer}");
            Debug.Log($"Backend Configured: {!string.IsNullOrEmpty(sdkConfig.nakamaHost)}");
            Debug.Log("=================================");
        }

        private void ShowWelcomeMessage()
        {
            string language = IVXLanguageManager.GetLanguageName(IVXLanguageManager.CurrentLanguage);
            Debug.Log($"🎮 Welcome to {sdkConfig.gameName}!");
            Debug.Log($"👤 Playing as: {IntelliVerseXIdentity.Username}");
            Debug.Log($"🌍 Language: {language}");
        }

        private void OnDestroy()
        {
            // Cleanup event subscriptions
            if (IntelliVerseXManager.Instance != null)
            {
                IntelliVerseXManager.Instance.OnReady -= OnSDKReady;
                IntelliVerseXManager.Instance.OnError -= OnSDKError;
            }

            IVXLanguageManager.OnLanguageChanged -= OnLanguageChanged;
        }

        // === PUBLIC API FOR TESTING ===

        /// <summary>
        /// Test language switching (call from debug UI)
        /// </summary>
        public void TestLanguageSwitch(string languageCode)
        {
            IVXLanguageManager.SetLanguage(languageCode);
        }

        /// <summary>
        /// Test interstitial ad (call from debug UI)
        /// </summary>
        public void TestShowInterstitial()
        {
            if (!sdkConfig.enableAds)
            {
                Debug.LogWarning("[Bootstrap] Ads are disabled in SDK config!");
                return;
            }

            // TODO: IVXAdsManager.ShowInterstitial() not yet implemented in SDK
            Debug.LogWarning("[Bootstrap] ShowInterstitial() not yet implemented");
            // IVXAdsManager.ShowInterstitial((success) =>
            // {
            //     Debug.Log($"[Bootstrap] Interstitial ad result: {success}");
            // });
        }

        /// <summary>
        /// Test rewarded ad (call from debug UI)
        /// </summary>
        public void TestShowRewarded()
        {
            if (!sdkConfig.enableAds)
            {
                Debug.LogWarning("[Bootstrap] Ads are disabled in SDK config!");
                return;
            }

            // TODO: IVXAdsManager.ShowRewarded() not yet implemented in SDK
            Debug.LogWarning("[Bootstrap] ShowRewarded() not yet implemented");
            // IVXAdsManager.ShowRewarded((success, rewarded) =>
            // {
            //     Debug.Log($"[Bootstrap] Rewarded ad result - Success: {success}, Rewarded: {rewarded}");
            //     
            //     if (rewarded)
            //     {
            //         Debug.Log("[Bootstrap] User watched ad! Give reward here.");
            //         // Give coins, extra life, etc.
            //     }
            // });
        }

        /// <summary>
        /// Get SDK info (for debug UI)
        /// </summary>
        public string GetSDKInfo()
        {
            if (!IntelliVerseXManager.IsInitialized)
                return "SDK not initialized";

            return IntelliVerseXManager.GetSDKInfo();
        }
    }
}
