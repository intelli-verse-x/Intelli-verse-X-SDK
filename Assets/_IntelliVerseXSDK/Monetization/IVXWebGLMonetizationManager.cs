// ============================================================================
// IVXWebGLMonetizationManager.cs
// WebGL Monetization Manager - Platform Safe Implementation
// 
// Copyright (c) IntelliVerseX. All rights reserved.  
// Version: 2.1.0
// 
// PLATFORM SAFETY:
// - All DllImport declarations are wrapped with #if UNITY_WEBGL && ! UNITY_EDITOR
// - No native code is compiled for iOS/Android
// - No NativeBridge. mm file required
// - Clean builds on all platforms guaranteed
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// CRITICAL: Only include InteropServices on WebGL runtime builds
// This prevents "undefined symbol" linker errors on iOS/Android
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace IntelliVerseX.Monetization
{
    #region Runtime Configuration

    /// <summary>
    /// Runtime configuration for WebGL monetization.  
    /// This is a pure data class - safe for all platforms.  
    /// </summary>
    [Serializable]
    public class IVXWebGLMonetizationConfig
    {
        #region Ad Networks

        [Header("Ad Networks")]
        public bool enableGameMonetize = false;
        public string gameMonetizeGameId = "";

        public bool enableCrazyGames = false;
        public string crazyGamesGameId = "";

        public bool enableLevelPlayWeb = false;
        public string levelPlayAppKey = "";

        public bool enableAdSenseApplixir = false;
        public string adSenseClientId = "";
        public string adSenseSlotId = "";
        public string applixirZoneId = "";

        public bool prioritizeByRevenue = true;
        public int minAdCooldown = 60;

        #endregion

        #region Payment Providers

        [Header("Payment Providers")]
        public bool enableStripe = false;
        public string stripePublishableKey = "";

        public bool enablePaddle = false;
        public string paddleVendorId = "";
        public string paddleProductId = "";

        public bool enableXUTToken = false;
        public string xutApiEndpoint = "";
        public string xutTokenContractAddress = "";

        #endregion

        #region Analytics

        [Header("Analytics")]
        public bool enableGA4 = false;
        public string ga4MeasurementId = "";

        public bool enableUnityAnalytics = false;

        public bool trackCustomEvents = false;
        public string customEventEndpoint = "";

        #endregion

        #region Engagement

        [Header("Engagement")]
        public bool autoFullscreen = true;
        public int idlePromptSeconds = 120;
        public GameOrientation gameOrientation = GameOrientation.Auto;
        public bool autoPauseOnHidden = true;

        #endregion

        #region Validation

        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid(out string error)
        {
            error = null;

            bool hasFeature = enableGameMonetize || enableCrazyGames ||
                              enableLevelPlayWeb || enableAdSenseApplixir ||
                              enableStripe || enablePaddle || enableXUTToken ||
                              enableGA4 || enableUnityAnalytics;

            if (!hasFeature)
            {
                error = "No monetization features enabled";
                return false;
            }

            if (enableGameMonetize && string.IsNullOrEmpty(gameMonetizeGameId))
            {
                error = "GameMonetize enabled but gameMonetizeGameId is empty";
                return false;
            }

            if (enableCrazyGames && string.IsNullOrEmpty(crazyGamesGameId))
            {
                error = "CrazyGames enabled but crazyGamesGameId is empty";
                return false;
            }

            if (enableLevelPlayWeb && string.IsNullOrEmpty(levelPlayAppKey))
            {
                error = "LevelPlay enabled but levelPlayAppKey is empty";
                return false;
            }

            if (enableStripe && string.IsNullOrEmpty(stripePublishableKey))
            {
                error = "Stripe enabled but stripePublishableKey is empty";
                return false;
            }

            if (enablePaddle && string.IsNullOrEmpty(paddleVendorId))
            {
                error = "Paddle enabled but paddleVendorId is empty";
                return false;
            }

            if (enableXUTToken && string.IsNullOrEmpty(xutTokenContractAddress))
            {
                error = "XUT Token enabled but contract address is empty";
                return false;
            }

            if (enableGA4 && string.IsNullOrEmpty(ga4MeasurementId))
            {
                error = "GA4 enabled but ga4MeasurementId is empty";
                return false;
            }

            return true;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get estimated monthly revenue
        /// </summary>
        public float GetEstimatedRevenue()
        {
            float estimate = 0f;
            if (enableCrazyGames) estimate += 150f;
            if (enableGameMonetize) estimate += 100f;
            if (enableLevelPlayWeb) estimate += 120f;
            if (enableAdSenseApplixir) estimate += 50f;
            if (enableStripe || enablePaddle) estimate += 200f;
            if (enableXUTToken) estimate += 100f;
            return estimate;
        }

        /// <summary>
        /// Clone this configuration
        /// </summary>
        public IVXWebGLMonetizationConfig Clone()
        {
            return new IVXWebGLMonetizationConfig
            {
                // Ad Networks
                enableGameMonetize = this.enableGameMonetize,
                gameMonetizeGameId = this.gameMonetizeGameId ?? "",
                enableCrazyGames = this.enableCrazyGames,
                crazyGamesGameId = this.crazyGamesGameId ?? "",
                enableLevelPlayWeb = this.enableLevelPlayWeb,
                levelPlayAppKey = this.levelPlayAppKey ?? "",
                enableAdSenseApplixir = this.enableAdSenseApplixir,
                adSenseClientId = this.adSenseClientId ?? "",
                adSenseSlotId = this.adSenseSlotId ?? "",
                applixirZoneId = this.applixirZoneId ?? "",
                prioritizeByRevenue = this.prioritizeByRevenue,
                minAdCooldown = this.minAdCooldown,

                // Payment Providers
                enableStripe = this.enableStripe,
                stripePublishableKey = this.stripePublishableKey ?? "",
                enablePaddle = this.enablePaddle,
                paddleVendorId = this.paddleVendorId ?? "",
                paddleProductId = this.paddleProductId ?? "",
                enableXUTToken = this.enableXUTToken,
                xutApiEndpoint = this.xutApiEndpoint ?? "",
                xutTokenContractAddress = this.xutTokenContractAddress ?? "",

                // Analytics
                enableGA4 = this.enableGA4,
                ga4MeasurementId = this.ga4MeasurementId ?? "",
                enableUnityAnalytics = this.enableUnityAnalytics,
                trackCustomEvents = this.trackCustomEvents,
                customEventEndpoint = this.customEventEndpoint ?? "",

                // Engagement
                autoFullscreen = this.autoFullscreen,
                idlePromptSeconds = this.idlePromptSeconds,
                gameOrientation = this.gameOrientation,
                autoPauseOnHidden = this.autoPauseOnHidden
            };
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        public static IVXWebGLMonetizationConfig CreateDefault()
        {
            return new IVXWebGLMonetizationConfig
            {
                enableGA4 = true,
                autoFullscreen = true,
                autoPauseOnHidden = true,
                minAdCooldown = 60,
                idlePromptSeconds = 120,
                gameOrientation = GameOrientation.Auto
            };
        }

        #endregion
    }

    #endregion

    #region Main Manager

    /// <summary>
    /// WebGL Monetization Manager - Platform Safe
    /// 
    /// Handles ads, payments, analytics, and engagement for WebGL builds. 
    /// 
    /// PLATFORM SAFETY:
    /// - All native JavaScript interop is wrapped in #if UNITY_WEBGL && ! UNITY_EDITOR
    /// - On iOS/Android, all methods are safe no-ops
    /// - No NativeBridge.mm file required
    /// - No undefined symbol errors
    /// 
    /// Usage:
    ///   IVXWebGLMonetizationManager. Initialize(config);
    ///   IVXWebGLMonetizationManager. ShowAd("rewarded", callback);
    ///   IVXWebGLMonetizationManager.ProcessPayment("product_id", 999, callback);
    /// </summary>
    public static class IVXWebGLMonetizationManager
    {
        #region Constants

        private const string LOG_PREFIX = "[IVXWebGLMonetization]";
        private const string VERSION = "3.0.0";
        private const int MAX_CALLBACK_AGE_SECONDS = 300;

        #endregion

        #region Private Fields

        private static IVXWebGLMonetizationConfig _config;
        private static bool _isInitialized = false;
        private static DateTime _lastAdTime = DateTime.MinValue;

        private static readonly Dictionary<string, AdCallbackEntry> _adCallbacks =
            new Dictionary<string, AdCallbackEntry>();
        private static readonly Dictionary<string, PaymentCallbackEntry> _paymentCallbacks =
            new Dictionary<string, PaymentCallbackEntry>();
        private static readonly object _lockObject = new object();

        private struct AdCallbackEntry
        {
            public Action<bool, int> Callback;
            public DateTime CreatedAt;
            public string AdType;
        }

        private struct PaymentCallbackEntry
        {
            public Action<bool, string> Callback;
            public DateTime CreatedAt;
            public string ProductId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true only on WebGL platform (not in Editor)
        /// </summary>
        public static bool IsWebGL
        {
            get
            {
#if UNITY_WEBGL && ! UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Returns true if running in Unity Editor
        /// </summary>
        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Returns true if initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Get current configuration (null if not initialized)
        /// </summary>
        public static IVXWebGLMonetizationConfig Config => _config;

        /// <summary>
        /// Get manager version
        /// </summary>
        public static string Version => VERSION;

        #endregion

        #region Events

        /// <summary>Fired when ad is shown.  Parameters: adType, success</summary>
        public static event Action<string, bool> OnAdShown;

        /// <summary>Fired when rewarded ad completes. Parameters: adType, rewardAmount</summary>
        public static event Action<string, int> OnAdCompleted;

        /// <summary>Fired when payment completes. Parameters: productId, success</summary>
        public static event Action<string, bool> OnPaymentComplete;

        /// <summary>Fired when orientation changes. Parameters: orientation</summary>
        public static event Action<string> OnOrientationChanged;

        /// <summary>Fired when idle is detected</summary>
        public static event Action OnIdleDetected;

        /// <summary>Fired when tab visibility changes.  Parameters: isVisible</summary>
        public static event Action<bool> OnVisibilityChanged;

        /// <summary>Fired when initialization completes.  Parameters: success, errorMessage</summary>
        public static event Action<bool, string> OnInitializationComplete;

        #endregion

        #region JavaScript Interop - WEBGL RUNTIME ONLY

        // =====================================================================
        // CRITICAL: All DllImport MUST be inside #if UNITY_WEBGL && ! UNITY_EDITOR
        // This prevents "undefined symbol" linker errors on iOS/Android builds
        // =====================================================================

#if UNITY_WEBGL && ! UNITY_EDITOR

        // === Ad Networks ===
        [DllImport("__Internal")]
        private static extern void GameMonetize_Init(string gameId);

        [DllImport("__Internal")]
        private static extern void GameMonetize_ShowAd(string adType);

        [DllImport("__Internal")]
        private static extern void CrazyGames_Init(string gameId);

        [DllImport("__Internal")]
        private static extern void CrazyGames_ShowAd(string adType);

        [DllImport("__Internal")]
        private static extern void CrazyGames_HappyTime();

        [DllImport("__Internal")]
        private static extern void LevelPlay_Init(string appKey);

        [DllImport("__Internal")]
        private static extern void LevelPlay_ShowRewardedAd();

        [DllImport("__Internal")]
        private static extern void InitializeAdSense(string clientId, string slotId);

        [DllImport("__Internal")]
        private static extern void ShowAdSenseBanner();

        [DllImport("__Internal")]
        private static extern void HideAdSenseBannerJS();

        [DllImport("__Internal")]
        private static extern void RefreshAdSenseBannerJS();

        [DllImport("__Internal")]
        private static extern void ShowAdSenseInterstitial();

        [DllImport("__Internal")]
        private static extern void InitializeApplixir(string zoneId);

        [DllImport("__Internal")]
        private static extern bool IsApplixirAdReady();

        [DllImport("__Internal")]
        private static extern void ShowApplixirRewardedAd();

        // === Payment Providers ===
        [DllImport("__Internal")]
        private static extern void Stripe_Checkout(string key, string productId, string priceId, int amount);

        [DllImport("__Internal")]
        private static extern void Paddle_Checkout(string vendorId, string productId, int amount);

        [DllImport("__Internal")]
        private static extern void XUT_ProcessPayment(string api, string contract, string productId, int amount);

        // === Analytics ===
        [DllImport("__Internal")]
        private static extern void GA4_Init(string measurementId);

        [DllImport("__Internal")]
        private static extern void GA4_TrackEvent(string eventName, string eventParams);

        // === Engagement ===
        [DllImport("__Internal")]
        private static extern void Engagement_EnableFullscreen();

        [DllImport("__Internal")]
        private static extern void Engagement_TrackIdle(int seconds);

        [DllImport("__Internal")]
        private static extern void Engagement_SetOrientation(string orientation);

#endif // UNITY_WEBGL && !UNITY_EDITOR

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize with ScriptableObject settings
        /// </summary>
        /// <param name="settings">WebGL monetization settings</param>
        public static void Initialize(IVXWebGLMonetizationSettings settings)
        {
            if (settings == null)
            {
                Debug.LogError($"{LOG_PREFIX} Settings cannot be null");
                OnInitializationComplete?.Invoke(false, "Settings is null");
                return;
            }

            Initialize(settings.ToRuntimeConfig());
        }

        /// <summary>
        /// Initialize with runtime config
        /// </summary>
        /// <param name="config">Runtime configuration</param>
        public static void Initialize(IVXWebGLMonetizationConfig config)
        {
            // Platform check - only run on WebGL
            if (!IsWebGL && !IsEditor)
            {
                Debug.Log($"{LOG_PREFIX} WebGL monetization is only available on WebGL platform.  Current: {Application.platform}");
                OnInitializationComplete?.Invoke(false, "Not a WebGL build");
                return;
            }

            if (_isInitialized)
            {
                Debug.LogWarning($"{LOG_PREFIX} Already initialized. Call Shutdown() first to reinitialize.");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"{LOG_PREFIX} Config cannot be null");
                OnInitializationComplete?.Invoke(false, "Config is null");
                return;
            }

            _config = config.Clone();

            if (!_config.IsValid(out string error))
            {
                Debug.LogError($"{LOG_PREFIX} Invalid config: {error}");
                OnInitializationComplete?.Invoke(false, error);
                return;
            }

            try
            {
                InitializeAdNetworksInternal();
                InitializePaymentProvidersInternal();
                InitializeAnalyticsInternal();
                InitializeEngagementInternal();

                _isInitialized = true;

                Debug.Log($"{LOG_PREFIX} ✓ Initialized successfully (v{VERSION})");
                Debug.Log($"{LOG_PREFIX} Platform: {(IsWebGL ? "WebGL" : "Editor Simulation")}");
                Debug.Log($"{LOG_PREFIX} Estimated Revenue: ${_config.GetEstimatedRevenue():F0}/month");

                OnInitializationComplete?.Invoke(true, null);

                TrackEvent("ivx_init", new Dictionary<string, object>
                {
                    { "platform", IsWebGL ? "webgl" : "editor" },
                    { "version", VERSION }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Init failed: {e.Message}");
                OnInitializationComplete?.Invoke(false, e.Message);
                _isInitialized = false;
                _config = null;
            }
        }

        /// <summary>
        /// Shutdown and cleanup the manager
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized) return;

            lock (_lockObject)
            {
                _adCallbacks.Clear();
                _paymentCallbacks.Clear();
            }

            _isInitialized = false;
            _config = null;
            _lastAdTime = DateTime.MinValue;

            Debug.Log($"{LOG_PREFIX} Shutdown complete");
        }

        #endregion

        #region Ad Networks

        private static void InitializeAdNetworksInternal()
        {
#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (_config.enableGameMonetize && ! string.IsNullOrEmpty(_config.gameMonetizeGameId))
                {
                    GameMonetize_Init(_config.gameMonetizeGameId);
                    Debug.Log($"{LOG_PREFIX} ✓ GameMonetize initialized");
                }

                if (_config.enableCrazyGames && !string.IsNullOrEmpty(_config.crazyGamesGameId))
                {
                    CrazyGames_Init(_config. crazyGamesGameId);
                    Debug.Log($"{LOG_PREFIX} ✓ CrazyGames initialized");
                }

                if (_config.enableLevelPlayWeb && !string.IsNullOrEmpty(_config.levelPlayAppKey))
                {
                    LevelPlay_Init(_config. levelPlayAppKey);
                    Debug. Log($"{LOG_PREFIX} ✓ LevelPlay initialized");
                }

                if (_config. enableAdSenseApplixir)
                {
                    if (!string.IsNullOrEmpty(_config.adSenseClientId))
                    {
                        InitializeAdSense(_config. adSenseClientId, _config.adSenseSlotId ??  "");
                        Debug.Log($"{LOG_PREFIX} ✓ AdSense initialized");
                    }

                    if (! string.IsNullOrEmpty(_config. applixirZoneId))
                    {
                        InitializeApplixir(_config.applixirZoneId);
                        Debug.Log($"{LOG_PREFIX} ✓ Applixir initialized");
                    }
                }
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} JS function not found: {e.Message}.  Ensure . jslib is included.");
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Ad network init error: {e.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"{LOG_PREFIX} [EDITOR] Ad networks simulated");
#else
            Debug.Log($"{LOG_PREFIX} [NON-WEBGL] Ad networks not available on this platform");
#endif
        }

        /// <summary>
        /// Show an ad with optional callback
        /// </summary>
        /// <param name="adType">Type: "banner", "interstitial", or "rewarded"</param>
        /// <param name="callback">Callback with (success, rewardAmount)</param>
        public static void ShowAd(string adType, Action<bool, int> callback = null)
        {
            // Platform check
            if (!IsWebGL && !IsEditor)
            {
                Debug.LogWarning($"{LOG_PREFIX} ShowAd is only available on WebGL platform");
                callback?.Invoke(false, 0);
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogWarning($"{LOG_PREFIX} Not initialized. Call Initialize() first.");
                callback?.Invoke(false, 0);
                return;
            }

            if (string.IsNullOrEmpty(adType))
            {
                Debug.LogWarning($"{LOG_PREFIX} Ad type cannot be null or empty");
                callback?.Invoke(false, 0);
                return;
            }

            adType = adType.ToLowerInvariant().Trim();

            // Cooldown check
            double elapsed = (DateTime.UtcNow - _lastAdTime).TotalSeconds;
            if (elapsed < _config.minAdCooldown)
            {
                int remaining = (int)(_config.minAdCooldown - elapsed);
                Debug.LogWarning($"{LOG_PREFIX} Ad on cooldown ({remaining}s remaining)");
                callback?.Invoke(false, 0);
                return;
            }

            CleanupExpiredCallbacks();

            // Register callback
            string key = $"{adType}_{DateTime.UtcNow.Ticks}";
            lock (_lockObject)
            {
                _adCallbacks[key] = new AdCallbackEntry
                {
                    Callback = callback,
                    CreatedAt = DateTime.UtcNow,
                    AdType = adType
                };
            }

            _lastAdTime = DateTime.UtcNow;

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (! TryShowAdInternal(adType))
                {
                    Debug.LogWarning($"{LOG_PREFIX} No ad network available for: {adType}");
                    InvokeAdCallback(adType, false, 0);
                }
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Ad JS function not found: {e.Message}");
                InvokeAdCallback(adType, false, 0);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} ShowAd error: {e.Message}");
                InvokeAdCallback(adType, false, 0);
            }
#elif UNITY_EDITOR
            SimulateAdInEditor(adType);
#else
            Debug.LogWarning($"{LOG_PREFIX} ShowAd not available on this platform");
            callback?.Invoke(false, 0);
#endif
        }

#if UNITY_WEBGL && ! UNITY_EDITOR
        private static bool TryShowAdInternal(string adType)
        {
            // Priority 1: CrazyGames (highest eCPM)
            if (_config.enableCrazyGames && _config.prioritizeByRevenue && ! string.IsNullOrEmpty(_config. crazyGamesGameId))
            {
                CrazyGames_ShowAd(adType);
                return true;
            }

            // Priority 2: GameMonetize
            if (_config. enableGameMonetize && ! string.IsNullOrEmpty(_config. gameMonetizeGameId))
            {
                GameMonetize_ShowAd(adType);
                return true;
            }

            // Priority 3: LevelPlay (rewarded only)
            if (_config.enableLevelPlayWeb && adType == "rewarded" && !string. IsNullOrEmpty(_config.levelPlayAppKey))
            {
                LevelPlay_ShowRewardedAd();
                return true;
            }

            // Priority 4: AdSense/Applixir
            if (_config. enableAdSenseApplixir)
            {
                switch (adType)
                {
                    case "rewarded":
                        if (!string.IsNullOrEmpty(_config.applixirZoneId) && IsApplixirAdReady())
                        {
                            ShowApplixirRewardedAd();
                            return true;
                        }
                        break;

                    case "interstitial":
                        if (! string.IsNullOrEmpty(_config. adSenseClientId))
                        {
                            ShowAdSenseInterstitial();
                            return true;
                        }
                        break;

                    case "banner":
                        if (!string. IsNullOrEmpty(_config.adSenseClientId))
                        {
                            ShowAdSenseBanner();
                            return true;
                        }
                        break;
                }
            }

            return false;
        }
#endif

#if UNITY_EDITOR
        private static void SimulateAdInEditor(string adType)
        {
            Debug.Log($"{LOG_PREFIX} [EDITOR] Simulating {adType} ad");
            int reward = adType == "rewarded" ? 100 : 0;

            OnAdShown?.Invoke(adType, true);

            if (reward > 0)
            {
                OnAdCompleted?.Invoke(adType, reward);
            }

            InvokeAdCallback(adType, true, reward);
        }
#endif

        /// <summary>
        /// Show or hide banner ad
        /// </summary>
        /// <param name="visible">True to show, false to hide</param>
        public static void SetBannerVisible(bool visible)
        {
            if (!IsWebGL || !_isInitialized) return;

#if UNITY_WEBGL && ! UNITY_EDITOR
            if (! _config.enableAdSenseApplixir || string.IsNullOrEmpty(_config.adSenseClientId)) return;

            try
            {
                if (visible)
                    ShowAdSenseBanner();
                else
                    HideAdSenseBannerJS();
            }
            catch (Exception e)
            {
                Debug. LogWarning($"{LOG_PREFIX} Banner error: {e.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"{LOG_PREFIX} [EDITOR] Banner visibility: {visible}");
#endif
        }

        /// <summary>
        /// Refresh banner ad
        /// </summary>
        public static void RefreshBanner()
        {
            if (!IsWebGL || !_isInitialized) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_config.enableAdSenseApplixir || string. IsNullOrEmpty(_config.adSenseClientId)) return;

            try
            {
                RefreshAdSenseBannerJS();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Banner refresh error: {e.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"{LOG_PREFIX} [EDITOR] Banner refreshed");
#endif
        }

        /// <summary>
        /// Trigger CrazyGames happy time effect
        /// </summary>
        public static void TriggerHappyTime()
        {
            if (!IsWebGL || !_isInitialized) return;

#if UNITY_WEBGL && ! UNITY_EDITOR
            if (!_config.enableCrazyGames || string.IsNullOrEmpty(_config.crazyGamesGameId)) return;

            try
            {
                CrazyGames_HappyTime();
                Debug.Log($"{LOG_PREFIX} Happy Time triggered!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} HappyTime error: {e.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"{LOG_PREFIX} [EDITOR] Happy Time simulated");
#endif
        }

        /// <summary>
        /// Called from JavaScript when ad completes
        /// Format: "adType|success|reward"
        /// </summary>
        public static void OnAdCompleteFromJS(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    Debug.LogWarning($"{LOG_PREFIX} Received empty ad result");
                    return;
                }

                string[] parts = data.Split('|');
                if (parts.Length < 3)
                {
                    Debug.LogWarning($"{LOG_PREFIX} Invalid ad result format: {data}");
                    return;
                }

                string adType = parts[0];
                bool success = parts[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                int.TryParse(parts[2], out int reward);

                Debug.Log($"{LOG_PREFIX} Ad complete: {adType} | success={success} | reward={reward}");

                OnAdShown?.Invoke(adType, success);

                if (success && reward > 0)
                {
                    OnAdCompleted?.Invoke(adType, reward);
                }

                InvokeAdCallback(adType, success, reward);

                TrackEvent(success ? "ad_complete" : "ad_fail", new Dictionary<string, object>
                {
                    { "type", adType },
                    { "reward", reward }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} OnAdComplete error: {e.Message}");
            }
        }

        private static void InvokeAdCallback(string adType, bool success, int reward)
        {
            Action<bool, int> callback = null;

            lock (_lockObject)
            {
                string foundKey = null;
                foreach (var kvp in _adCallbacks)
                {
                    if (kvp.Value.AdType == adType)
                    {
                        callback = kvp.Value.Callback;
                        foundKey = kvp.Key;
                        break;
                    }
                }

                if (foundKey != null)
                {
                    _adCallbacks.Remove(foundKey);
                }
            }

            try
            {
                callback?.Invoke(success, reward);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Ad callback error: {e.Message}");
            }
        }

        #endregion

        #region Payment Providers

        private static void InitializePaymentProvidersInternal()
        {
            int count = 0;

            if (_config.enableStripe && !string.IsNullOrEmpty(_config.stripePublishableKey))
            {
                count++;
                Debug.Log($"{LOG_PREFIX} ✓ Stripe ready");
            }

            if (_config.enablePaddle && !string.IsNullOrEmpty(_config.paddleVendorId))
            {
                count++;
                Debug.Log($"{LOG_PREFIX} ✓ Paddle ready");
            }

            if (_config.enableXUTToken && !string.IsNullOrEmpty(_config.xutTokenContractAddress))
            {
                count++;
                Debug.Log($"{LOG_PREFIX} ✓ XUT Token ready");
            }

            if (count > 0)
            {
                Debug.Log($"{LOG_PREFIX} {count} payment provider(s) configured");
            }
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="amountCents">Amount in cents</param>
        /// <param name="callback">Callback with (success, transactionId or error)</param>
        public static void ProcessPayment(string productId, int amountCents, Action<bool, string> callback = null)
        {
            // Platform check
            if (!IsWebGL && !IsEditor)
            {
                Debug.LogWarning($"{LOG_PREFIX} ProcessPayment is only available on WebGL platform");
                callback?.Invoke(false, "Not WebGL");
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogWarning($"{LOG_PREFIX} Not initialized");
                callback?.Invoke(false, "Not initialized");
                return;
            }

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning($"{LOG_PREFIX} Product ID cannot be null or empty");
                callback?.Invoke(false, "Invalid product ID");
                return;
            }

            if (amountCents <= 0)
            {
                Debug.LogWarning($"{LOG_PREFIX} Amount must be positive");
                callback?.Invoke(false, "Invalid amount");
                return;
            }

            CleanupExpiredCallbacks();

            // Register callback
            string key = $"{productId}_{DateTime.UtcNow.Ticks}";
            lock (_lockObject)
            {
                _paymentCallbacks[key] = new PaymentCallbackEntry
                {
                    Callback = callback,
                    CreatedAt = DateTime.UtcNow,
                    ProductId = productId
                };
            }

            Debug.Log($"{LOG_PREFIX} Processing payment: {productId} (${amountCents / 100f:F2})");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (! TryProcessPaymentInternal(productId, amountCents))
                {
                    Debug.LogWarning($"{LOG_PREFIX} No payment provider available");
                    InvokePaymentCallback(productId, false, "No payment provider");
                }
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Payment JS function not found: {e.Message}");
                InvokePaymentCallback(productId, false, "Payment provider not available");
            }
            catch (Exception e)
            {
                Debug. LogError($"{LOG_PREFIX} Payment error: {e.Message}");
                InvokePaymentCallback(productId, false, e.Message);
            }
#elif UNITY_EDITOR
            SimulatePaymentInEditor(productId);
#else
            Debug.LogWarning($"{LOG_PREFIX} ProcessPayment not available on this platform");
            callback?.Invoke(false, "Not supported");
#endif
        }

#if UNITY_WEBGL && ! UNITY_EDITOR
        private static bool TryProcessPaymentInternal(string productId, int amount)
        {
            // Priority 1: Stripe
            if (_config.enableStripe && !string.IsNullOrEmpty(_config.stripePublishableKey))
            {
                Stripe_Checkout(_config.stripePublishableKey, productId, productId, amount);
                return true;
            }

            // Priority 2: Paddle
            if (_config.enablePaddle && !string. IsNullOrEmpty(_config.paddleVendorId))
            {
                string paddleProductId = ! string.IsNullOrEmpty(_config. paddleProductId)
                    ? _config.paddleProductId
                    : productId;
                Paddle_Checkout(_config.paddleVendorId, paddleProductId, amount);
                return true;
            }

            // Priority 3: XUT Token
            if (_config. enableXUTToken && !string.IsNullOrEmpty(_config.xutTokenContractAddress))
            {
                XUT_ProcessPayment(
                    _config. xutApiEndpoint ??  "",
                    _config.xutTokenContractAddress,
                    productId,
                    amount
                );
                return true;
            }

            return false;
        }
#endif

#if UNITY_EDITOR
        private static void SimulatePaymentInEditor(string productId)
        {
            Debug.Log($"{LOG_PREFIX} [EDITOR] Simulating payment: {productId}");
            string txId = "sim_" + Guid.NewGuid().ToString("N").Substring(0, 12);

            OnPaymentComplete?.Invoke(productId, true);
            InvokePaymentCallback(productId, true, txId);
        }
#endif

        /// <summary>
        /// Called from JavaScript when payment completes
        /// Format: "productId|success|transactionId"
        /// </summary>
        public static void OnPaymentCompleteFromJS(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    Debug.LogWarning($"{LOG_PREFIX} Received empty payment result");
                    return;
                }

                string[] parts = data.Split('|');
                if (parts.Length < 2)
                {
                    Debug.LogWarning($"{LOG_PREFIX} Invalid payment result format: {data}");
                    return;
                }

                string productId = parts[0];
                bool success = parts[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                string txId = parts.Length > 2 ? parts[2] : "";

                Debug.Log($"{LOG_PREFIX} Payment complete: {productId} | success={success} | txId={txId}");

                OnPaymentComplete?.Invoke(productId, success);
                InvokePaymentCallback(productId, success, success ? txId : "Payment failed");

                TrackEvent(success ? "purchase_complete" : "purchase_fail", new Dictionary<string, object>
                {
                    { "product", productId },
                    { "transaction_id", txId }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} OnPaymentComplete error: {e.Message}");
            }
        }

        private static void InvokePaymentCallback(string productId, bool success, string result)
        {
            Action<bool, string> callback = null;

            lock (_lockObject)
            {
                string foundKey = null;
                foreach (var kvp in _paymentCallbacks)
                {
                    if (kvp.Value.ProductId == productId)
                    {
                        callback = kvp.Value.Callback;
                        foundKey = kvp.Key;
                        break;
                    }
                }

                if (foundKey != null)
                {
                    _paymentCallbacks.Remove(foundKey);
                }
            }

            try
            {
                callback?.Invoke(success, result);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Payment callback error: {e.Message}");
            }
        }

        #endregion

        #region Analytics

        private static void InitializeAnalyticsInternal()
        {
#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (_config.enableGA4 && !string. IsNullOrEmpty(_config.ga4MeasurementId))
                {
                    GA4_Init(_config.ga4MeasurementId);
                    Debug. Log($"{LOG_PREFIX} ✓ GA4 initialized");
                }
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} GA4 JS function not found: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} GA4 init error: {e. Message}");
            }
#elif UNITY_EDITOR
            if (_config.enableGA4)
            {
                Debug.Log($"{LOG_PREFIX} [EDITOR] GA4 simulated");
            }
#endif
        }

        /// <summary>
        /// Track an analytics event
        /// </summary>
        /// <param name="eventName">Event name (will be sanitized)</param>
        /// <param name="parameters">Optional event parameters</param>
        public static void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized || string.IsNullOrEmpty(eventName)) return;

            eventName = SanitizeEventName(eventName);

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (_config.enableGA4 && !string.IsNullOrEmpty(_config.ga4MeasurementId))
                {
                    GA4_TrackEvent(eventName, SerializeDict(parameters));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} TrackEvent error: {e.Message}");
            }
#endif

            // Debug logging
            if (Debug.isDebugBuild || IsEditor)
            {
                string paramsStr = parameters != null ? SerializeDict(parameters) : "{}";
                Debug.Log($"{LOG_PREFIX} [EVENT] {eventName}: {paramsStr}");
            }
        }

        private static string SanitizeEventName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unknown_event";

            name = name.ToLowerInvariant();
            name = System.Text.RegularExpressions.Regex.Replace(name, "[^a-z0-9_]", "_");

            if (name.Length > 40)
            {
                name = name.Substring(0, 40);
            }

            return name;
        }

        private static string SerializeDict(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0) return "{}";

            var parts = new List<string>();
            foreach (var kvp in dict)
            {
                string value;
                if (kvp.Value == null)
                {
                    value = "null";
                }
                else if (kvp.Value is string s)
                {
                    value = $"\"{EscapeJsonString(s)}\"";
                }
                else if (kvp.Value is bool b)
                {
                    value = b.ToString().ToLowerInvariant();
                }
                else
                {
                    value = kvp.Value.ToString();
                }

                parts.Add($"\"{EscapeJsonString(kvp.Key)}\":{value}");
            }

            return "{" + string.Join(",", parts) + "}";
        }

        private static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        #endregion

        #region Engagement

        private static void InitializeEngagementInternal()
        {
#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                if (_config.autoFullscreen)
                {
                    Engagement_EnableFullscreen();
                    Debug.Log($"{LOG_PREFIX} ✓ Auto-fullscreen enabled");
                }

                if (_config. idlePromptSeconds > 0)
                {
                    Engagement_TrackIdle(_config.idlePromptSeconds);
                    Debug.Log($"{LOG_PREFIX} ✓ Idle tracking: {_config.idlePromptSeconds}s");
                }

                string orientation = _config.gameOrientation switch
                {
                    GameOrientation.Portrait => "portrait",
                    GameOrientation.Landscape => "landscape",
                    _ => "auto"
                };
                Engagement_SetOrientation(orientation);
                Debug.Log($"{LOG_PREFIX} ✓ Orientation: {orientation}");
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Engagement JS function not found: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Engagement init error: {e.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"{LOG_PREFIX} [EDITOR] Engagement features simulated");
#endif
        }

        /// <summary>
        /// Called from JavaScript when orientation changes
        /// </summary>
        public static void OnOrientationChangedFromJS(string orientation)
        {
            Debug.Log($"{LOG_PREFIX} Orientation changed: {orientation}");
            OnOrientationChanged?.Invoke(orientation);
            TrackEvent("orientation_change", new Dictionary<string, object> { { "orientation", orientation } });
        }

        /// <summary>
        /// Called from JavaScript when idle is detected
        /// </summary>
        public static void OnIdleDetectedFromJS()
        {
            Debug.Log($"{LOG_PREFIX} Idle detected");
            OnIdleDetected?.Invoke();
            TrackEvent("user_idle");
        }

        /// <summary>
        /// Called from JavaScript when tab visibility changes
        /// </summary>
        public static void OnVisibilityChangedFromJS(string visible)
        {
            bool isVisible = visible.Equals("true", StringComparison.OrdinalIgnoreCase);
            Debug.Log($"{LOG_PREFIX} Visibility changed: {isVisible}");

            if (_config?.autoPauseOnHidden == true)
            {
                Time.timeScale = isVisible ? 1f : 0f;
                AudioListener.pause = !isVisible;
            }

            OnVisibilityChanged?.Invoke(isVisible);
            TrackEvent(isVisible ? "tab_visible" : "tab_hidden");
        }

        #endregion

        #region Helpers

        private static void CleanupExpiredCallbacks()
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                var adKeysToRemove = new List<string>();
                var paymentKeysToRemove = new List<string>();

                foreach (var kvp in _adCallbacks)
                {
                    if ((now - kvp.Value.CreatedAt).TotalSeconds > MAX_CALLBACK_AGE_SECONDS)
                    {
                        adKeysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in adKeysToRemove)
                {
                    _adCallbacks.Remove(key);
                }

                foreach (var kvp in _paymentCallbacks)
                {
                    if ((now - kvp.Value.CreatedAt).TotalSeconds > MAX_CALLBACK_AGE_SECONDS)
                    {
                        paymentKeysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in paymentKeysToRemove)
                {
                    _paymentCallbacks.Remove(key);
                }
            }
        }

        /// <summary>
        /// Get seconds until next ad is allowed
        /// </summary>
        public static int GetAdCooldownRemaining()
        {
            if (_config == null) return 0;

            double elapsed = (DateTime.UtcNow - _lastAdTime).TotalSeconds;
            int remaining = (int)(_config.minAdCooldown - elapsed);
            return Mathf.Max(0, remaining);
        }

        /// <summary>
        /// Check if ad can be shown now (not on cooldown)
        /// </summary>
        public static bool CanShowAd()
        {
            return GetAdCooldownRemaining() == 0;
        }

        /// <summary>
        /// Get configuration summary for debugging
        /// </summary>
        public static string GetSummary()
        {
            if (!_isInitialized || _config == null)
            {
                return "Not initialized";
            }

            int adNetworks = 0;
            if (_config.enableGameMonetize) adNetworks++;
            if (_config.enableCrazyGames) adNetworks++;
            if (_config.enableLevelPlayWeb) adNetworks++;
            if (_config.enableAdSenseApplixir) adNetworks++;

            int payments = 0;
            if (_config.enableStripe) payments++;
            if (_config.enablePaddle) payments++;
            if (_config.enableXUTToken) payments++;

            return $"v{VERSION} | Platform: {(IsWebGL ? "WebGL" : "Editor")} | " +
                   $"Ads: {adNetworks} | Payments: {payments} | " +
                   $"Est. Revenue: ${_config.GetEstimatedRevenue():F0}/mo";
        }

        #endregion
    }

    #endregion
}