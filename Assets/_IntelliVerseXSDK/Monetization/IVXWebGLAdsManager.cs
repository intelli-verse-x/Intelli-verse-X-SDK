// ============================================================================
// IVXWebGLAdsManager.cs
// WebGL-specific Ads Manager - Platform Safe Implementation
// 
// Copyright (c) IntelliVerseX. All rights reserved. 
// Version: 2.0.0
// 
// IMPORTANT: 
// - All DllImport declarations are wrapped with #if UNITY_WEBGL && ! UNITY_EDITOR
// - This ensures NO undefined symbol errors on iOS/Android builds
// - No NativeBridge. mm file needed! 
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

// Only include InteropServices on WebGL builds (not Editor, not iOS, not Android)
#if UNITY_WEBGL && ! UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// WebGL-specific ads manager for IntelliVerse-X SDK. 
    /// 
    /// Platform Safety:
    /// - All native JavaScript interop is wrapped in #if UNITY_WEBGL && ! UNITY_EDITOR
    /// - Code compiles cleanly on iOS, Android, and Standalone
    /// - No NativeBridge.mm file required
    /// 
    /// Supported Networks:
    /// - Google AdSense (display, native, in-feed ads)
    /// - Applixir (rewarded video, WebGL-optimized)
    /// 
    /// Waterfall Mediation:
    /// Priority 1: Applixir (eCPM $8-15, rewarded video)
    /// Priority 2: AdSense (eCPM $2-4, rewarded interstitial)
    /// 
    /// Revenue Strategy (Monthly - 10k WebGL Players):
    /// - AdSense: $600-1,200/month (CPM $2-4, display ads)
    /// - Applixir: $800-1,500/month (eCPM $8-15, rewarded video)
    /// - Combined: $1,400-2,700/month
    /// - Waterfall: +15-25% fill rate improvement
    /// 
    /// Usage:
    ///   IVXWebGLAdsManager.Initialize(webglAdsConfig);
    ///   IVXWebGLAdsManager.ShowBannerAd("Banner_Top");
    ///   IVXWebGLAdsManager.ShowRewardedAd("Rewarded_ExtraHints", (success, reward) => {
    ///       if (success) GiveCoins(reward);
    ///   });
    /// </summary>
    public static class IVXWebGLAdsManager
    {
        #region Constants

        private const string LOG_PREFIX = "[IVXWebGLAdsManager]";

        #endregion

        #region Private Fields

        private static IVXWebGLAdsConfig _config;
        private static bool _isInitialized = false;
        private static ConcurrentDictionary<string, bool> _activeAds = new ConcurrentDictionary<string, bool>();
        private static DateTime _lastApplixirAdTime = DateTime.MinValue;
        private static readonly object _lockObject = new object();

        // Callback storage - use ConcurrentDictionary for thread safety
        private static ConcurrentDictionary<string, Action<bool, int>> _applixirCallbacks = new ConcurrentDictionary<string, Action<bool, int>>();
        private static ConcurrentDictionary<string, Action<bool, int>> _adSenseCallbacks = new ConcurrentDictionary<string, Action<bool, int>>();

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
        /// Check if manager is initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        #endregion

        #region Events

        /// <summary>Fired when banner is shown.  Parameters: unitName, success</summary>
        public static event Action<string, bool> OnBannerShown;

        /// <summary>Fired when banner is hidden. Parameters: unitName, success</summary>
        public static event Action<string, bool> OnBannerHidden;

        /// <summary>Fired when rewarded ad completes. Parameters: unitName, coinReward</summary>
        public static event Action<string, int> OnRewardedAdCompleted;

        #pragma warning disable CS0067
        /// <summary>Fired on ad error. Parameters: network, errorMessage</summary>
        public static event Action<WebGLAdNetwork, string> OnAdError;

        /// <summary>Fired on ad impression. Parameters: unitName, estimatedRevenue</summary>
        public static event Action<string, float> OnAdImpression;
        #pragma warning restore CS0067

        #endregion

        #region JavaScript Interop - WEBGL RUNTIME ONLY

        // =====================================================================
        // CRITICAL: All DllImport MUST be inside #if UNITY_WEBGL && ! UNITY_EDITOR
        // This prevents "undefined symbol" linker errors on iOS/Android builds
        // =====================================================================

#if UNITY_WEBGL && !UNITY_EDITOR

        // === AdSense JavaScript Functions ===
        [DllImport("__Internal")]
        private static extern void InitializeAdSenseJS(string publisherId, bool autoAds);

        [DllImport("__Internal")]
        private static extern void ShowAdSenseBannerJS(string unitName, string slotId, string size);

        [DllImport("__Internal")]
        private static extern void HideAdSenseBannerJS(string unitName);

        [DllImport("__Internal")]
        private static extern void RefreshAdSenseBannerJS(string unitName);

        [DllImport("__Internal")]
        private static extern void ShowAdSenseInterstitialJS(string unitName, string slotId, bool rewarded);

        // === Applixir JavaScript Functions ===
        [DllImport("__Internal")]
        private static extern void InitializeApplixirJS(string zoneId, bool testMode);

        [DllImport("__Internal")]
        private static extern void ShowApplixirRewardedAdJS(string unitName, int skipDelay);

        [DllImport("__Internal")]
        private static extern bool IsApplixirAdReadyJS();

#endif // UNITY_WEBGL && !UNITY_EDITOR

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize WebGL ads system
        /// </summary>
        /// <param name="config">WebGL ads configuration</param>
        public static void Initialize(IVXWebGLAdsConfig config)
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"{LOG_PREFIX} Already initialized");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"{LOG_PREFIX} Config cannot be null");
                return;
            }

            _config = config;

            // Platform check - skip initialization on non-WebGL if configured
            if (config.webGLOnly && !IsWebGL)
            {
                Debug.Log($"{LOG_PREFIX} Skipping init (not WebGL platform).  Current: {Application.platform}");
                return;
            }

            // Validate config
            if (!config.IsValid(out string error))
            {
                Debug.LogError($"{LOG_PREFIX} Config invalid: {error}");
                return;
            }

            // Initialize networks
            if (config.enableAdSense)
            {
                InitializeAdSenseNetwork(config);
            }

            if (config.enableApplixir)
            {
                InitializeApplixirNetwork(config);
            }

            _isInitialized = true;
            Debug.Log($"{LOG_PREFIX} ✓ Initialized successfully");
            Debug.Log($"{LOG_PREFIX} Estimated Revenue: {config.GetEstimatedRevenue()}");
        }

        /// <summary>
        /// Shutdown and cleanup
        /// </summary>
        public static void Shutdown()
        {
            lock (_lockObject)
            {
                _isInitialized = false;
                _activeAds.Clear();
                _applixirCallbacks.Clear();
                _adSenseCallbacks.Clear();
                _config = null;
            }
            Debug.Log($"{LOG_PREFIX} Shutdown complete");
        }

        #endregion

        #region AdSense Implementation

        private static void InitializeAdSenseNetwork(IVXWebGLAdsConfig config)
        {
            Debug.Log($"{LOG_PREFIX} Initializing AdSense (Publisher: {config.adSensePublisherId})");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                InitializeAdSenseJS(config.adSensePublisherId, config.enableAutoAds);
                Debug.Log($"{LOG_PREFIX} ✓ AdSense initialized");
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} AdSense JS function not found: {e.Message}.  Ensure . jslib is included.");
                OnAdError?.Invoke(WebGLAdNetwork.AdSense, e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} AdSense init failed: {e.Message}");
                OnAdError?.Invoke(WebGLAdNetwork.AdSense, e.Message);
            }
#else
            Debug.Log($"{LOG_PREFIX} [NON-WEBGL] AdSense initialization simulated");
#endif
        }

        /// <summary>
        /// Show AdSense banner ad
        /// </summary>
        /// <param name="unitName">Name of the ad unit from config</param>
        public static void ShowAdSenseBanner(string unitName)
        {
            if (!ValidateInitialized() || !ValidateAdSenseEnabled()) return;

            var adUnit = _config.GetAdSenseUnit(unitName);
            if (adUnit == null || !adUnit.enabled)
            {
                Debug.LogError($"{LOG_PREFIX} AdSense unit '{unitName}' not found or disabled");
                OnBannerShown?.Invoke(unitName, false);
                return;
            }

            Debug.Log($"{LOG_PREFIX} Showing AdSense banner: {unitName}");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                ShowAdSenseBannerJS(unitName, adUnit.adSlotId, adUnit.GetAdSizeString());
                _activeAds[unitName] = true;
                OnBannerShown?. Invoke(unitName, true);
                LogAdEvent("banner_shown", WebGLAdNetwork.AdSense, unitName);
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} ShowAdSenseBanner JS not found: {e. Message}");
                OnBannerShown?.Invoke(unitName, false);
                OnAdError?.Invoke(WebGLAdNetwork.AdSense, e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Failed to show AdSense banner: {e. Message}");
                OnBannerShown?.Invoke(unitName, false);
                OnAdError?.Invoke(WebGLAdNetwork. AdSense, e.Message);
            }
#else
            // Editor/Non-WebGL simulation
            _activeAds[unitName] = true;
            OnBannerShown?.Invoke(unitName, true);
            Debug.Log($"{LOG_PREFIX} [SIMULATED] AdSense banner shown: {unitName}");
#endif
        }

        /// <summary>
        /// Hide AdSense banner ad
        /// </summary>
        /// <param name="unitName">Name of the ad unit</param>
        public static void HideAdSenseBanner(string unitName)
        {
            if (!_activeAds.TryGetValue(unitName, out bool isActive) || !isActive)
            {
                Debug.LogWarning($"{LOG_PREFIX} Banner '{unitName}' not active");
                return;
            }

            Debug.Log($"{LOG_PREFIX} Hiding AdSense banner: {unitName}");

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                HideAdSenseBannerJS(unitName);
                _activeAds[unitName] = false;
                OnBannerHidden?. Invoke(unitName, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Failed to hide AdSense banner: {e.Message}");
                OnBannerHidden?. Invoke(unitName, false);
                OnAdError?. Invoke(WebGLAdNetwork.AdSense, e.Message);
            }
#else
            _activeAds[unitName] = false;
            OnBannerHidden?.Invoke(unitName, true);
            Debug.Log($"{LOG_PREFIX} [SIMULATED] AdSense banner hidden: {unitName}");
#endif
        }

        /// <summary>
        /// Refresh AdSense banner (reload ad)
        /// </summary>
        /// <param name="unitName">Name of the ad unit</param>
        public static void RefreshAdSenseBanner(string unitName)
        {
            if (!_activeAds.TryGetValue(unitName, out bool isActive) || !isActive)
            {
                Debug.LogWarning($"{LOG_PREFIX} Cannot refresh: banner '{unitName}' not active");
                return;
            }

            Debug.Log($"{LOG_PREFIX} Refreshing AdSense banner: {unitName}");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                RefreshAdSenseBannerJS(unitName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Failed to refresh AdSense banner: {e.Message}");
            }
#else
            Debug.Log($"{LOG_PREFIX} [SIMULATED] AdSense banner refreshed: {unitName}");
#endif
        }

        /// <summary>
        /// Show AdSense rewarded interstitial (fallback for rewarded ads)
        /// </summary>
        private static void ShowAdSenseRewardedInterstitial(string unitName, Action<bool, int> callback = null)
        {
            var adUnit = _config.GetAdSenseUnit(unitName);
            if (adUnit == null)
            {
                Debug.LogError($"{LOG_PREFIX} AdSense unit '{unitName}' not found");
                callback?.Invoke(false, 0);
                return;
            }

            int reward = _config.adSenseFallbackReward;
            Debug.Log($"{LOG_PREFIX} Showing AdSense rewarded interstitial: {unitName} ({reward} coins)");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                ShowAdSenseInterstitialJS(unitName, adUnit. adSlotId, true);
                RegisterAdSenseCallback(unitName, reward, callback);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Failed to show AdSense interstitial: {e.Message}");
                OnAdError?.Invoke(WebGLAdNetwork.AdSense, e.Message);
                callback?.Invoke(false, 0);
            }
#else
            // Editor simulation
            Debug.Log($"{LOG_PREFIX} [SIMULATED] AdSense interstitial completed: {unitName} (+{reward} coins)");
            OnRewardedAdCompleted?.Invoke(unitName, reward);
            callback?.Invoke(true, reward);
            LogAdEvent("rewarded_ad_completed", WebGLAdNetwork.AdSense, unitName, reward);
#endif
        }

        private static void RegisterAdSenseCallback(string unitName, int reward, Action<bool, int> callback)
        {
            if (callback != null)
            {
                // Use AddOrUpdate for thread-safe registration
                _adSenseCallbacks.AddOrUpdate(unitName, callback, (key, oldValue) => callback);
            }
        }

        /// <summary>
        /// Called from JavaScript when AdSense interstitial completes
        /// </summary>
        public static void OnAdSenseInterstitialCompleted(string unitName, bool success)
        {
            Debug.Log($"{LOG_PREFIX} AdSense interstitial completed: {unitName} | Success: {success}");

            int reward = _config?.adSenseFallbackReward ?? 0;
            int coins = success ? reward : 0;

            OnRewardedAdCompleted?.Invoke(unitName, coins);

            if (_config?.enableAnalytics == true && success)
            {
                LogAdEvent("rewarded_ad_completed", WebGLAdNetwork.AdSense, unitName, coins);
            }

            // Invoke stored callback - use TryRemove for thread-safe removal
            if (_adSenseCallbacks.TryRemove(unitName, out var callback))
            {
                callback?.Invoke(success, coins);
            }
        }

        #endregion

        #region Applixir Implementation

        private static void InitializeApplixirNetwork(IVXWebGLAdsConfig config)
        {
            Debug.Log($"{LOG_PREFIX} Initializing Applixir (Zone: {config.applixirZoneId})");

#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                InitializeApplixirJS(config.applixirZoneId, config.applixirTestMode);
                Debug.Log($"{LOG_PREFIX} ✓ Applixir initialized");
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Applixir JS function not found: {e.Message}.  Ensure .jslib is included.");
                OnAdError?.Invoke(WebGLAdNetwork.Applixir, e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} Applixir init failed: {e.Message}");
                OnAdError?. Invoke(WebGLAdNetwork. Applixir, e.Message);
            }
#else
            Debug.Log($"{LOG_PREFIX} [NON-WEBGL] Applixir initialization simulated");
#endif
        }

        /// <summary>
        /// Show Applixir rewarded ad
        /// </summary>
        /// <param name="unitName">Name of the rewarded unit from config</param>
        /// <param name="callback">Callback with (success, coinReward)</param>
        public static void ShowApplixirRewardedAd(string unitName, Action<bool, int> callback = null)
        {
            if (!ValidateInitialized() || !ValidateApplixirEnabled())
            {
                callback?.Invoke(false, 0);
                return;
            }

            // Check cooldown
            var timeSinceLastAd = (DateTime.Now - _lastApplixirAdTime).TotalSeconds;
            if (timeSinceLastAd < _config.applixirAdCooldown)
            {
                int remaining = (int)(_config.applixirAdCooldown - timeSinceLastAd);
                Debug.LogWarning($"{LOG_PREFIX} Applixir on cooldown ({remaining}s remaining)");
                callback?.Invoke(false, 0);
                return;
            }

            var rewardedUnit = _config.GetApplixirUnit(unitName);
            if (rewardedUnit == null || !rewardedUnit.enabled)
            {
                Debug.LogError($"{LOG_PREFIX} Applixir unit '{unitName}' not found or disabled");
                callback?.Invoke(false, 0);
                return;
            }

            Debug.Log($"{LOG_PREFIX} Showing Applixir rewarded ad: {unitName} ({rewardedUnit.coinReward} coins)");

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // Check if ad is ready
                if (!IsApplixirAdReadyJS())
                {
                    Debug.LogWarning($"{LOG_PREFIX} Applixir ad not ready");
                    callback?. Invoke(false, 0);
                    return;
                }

                // Show ad
                ShowApplixirRewardedAdJS(unitName, _config.applixirSkipDelay);
                RegisterApplixirCallback(unitName, rewardedUnit.coinReward, callback);
                _lastApplixirAdTime = DateTime. Now;
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Applixir JS function not found: {e.Message}");
                callback?.Invoke(false, 0);
                OnAdError?. Invoke(WebGLAdNetwork. Applixir, e.Message);
            }
            catch (Exception e)
            {
                Debug. LogError($"{LOG_PREFIX} Failed to show Applixir ad: {e.Message}");
                callback?.Invoke(false, 0);
                OnAdError?. Invoke(WebGLAdNetwork.Applixir, e. Message);
            }
#else
            // Editor simulation
            Debug.Log($"{LOG_PREFIX} [SIMULATED] Applixir ad completed: {unitName} (+{rewardedUnit.coinReward} coins)");
            _lastApplixirAdTime = DateTime.Now;
            OnRewardedAdCompleted?.Invoke(unitName, rewardedUnit.coinReward);
            callback?.Invoke(true, rewardedUnit.coinReward);
            LogAdEvent("rewarded_ad_completed", WebGLAdNetwork.Applixir, unitName, rewardedUnit.coinReward);
#endif
        }

        private static void RegisterApplixirCallback(string unitName, int reward, Action<bool, int> callback)
        {
            if (callback != null)
            {
                // Use AddOrUpdate to ensure callback is registered even if key exists
                _applixirCallbacks.AddOrUpdate(unitName, callback, (key, oldValue) => callback);
            }
        }

        /// <summary>
        /// Called from JavaScript when Applixir ad completes
        /// </summary>
        public static void OnApplixirAdCompleted(string unitName, bool success)
        {
            var rewardedUnit = _config?.GetApplixirUnit(unitName);
            int coins = (success && rewardedUnit != null) ? rewardedUnit.coinReward : 0;

            Debug.Log($"{LOG_PREFIX} Applixir ad completed: {unitName} | Success: {success} | Coins: {coins}");

            OnRewardedAdCompleted?.Invoke(unitName, coins);

            if (_config?.enableAnalytics == true && success)
            {
                LogAdEvent("rewarded_ad_completed", WebGLAdNetwork.Applixir, unitName, coins);
            }

            // Invoke stored callback - use TryRemove for thread-safe removal
            if (_applixirCallbacks.TryRemove(unitName, out var callback))
            {
                callback?.Invoke(success, coins);
            }
        }

        /// <summary>
        /// Check if Applixir ad is ready
        /// </summary>
        public static bool IsApplixirAdReady()
        {
#if UNITY_WEBGL && ! UNITY_EDITOR
            try
            {
                return IsApplixirAdReadyJS();
            }
            catch
            {
                return false;
            }
#else
            // Always ready in simulation
            return true;
#endif
        }

        #endregion

        #region Waterfall System

        /// <summary>
        /// Show rewarded ad with waterfall logic
        /// Priority: Applixir → AdSense (rewarded interstitial)
        /// </summary>
        /// <param name="unitName">Name of the ad unit</param>
        /// <param name="callback">Callback with (success, coinReward)</param>
        public static void ShowRewardedAdWithWaterfall(string unitName, Action<bool, int> callback = null)
        {
            if (!ValidateInitialized())
            {
                callback?.Invoke(false, 0);
                return;
            }

            Debug.Log($"{LOG_PREFIX} [WATERFALL] Starting for unit: {unitName}");

            // Priority 1: Applixir (higher eCPM $8-15)
            if (_config.enableApplixir && _config.prioritizeApplixir)
            {
                var applixirUnit = _config.GetApplixirUnit(unitName);
                if (applixirUnit != null && applixirUnit.enabled)
                {
                    var timeSinceLastAd = (DateTime.Now - _lastApplixirAdTime).TotalSeconds;
                    if (timeSinceLastAd >= _config.applixirAdCooldown)
                    {
#if UNITY_WEBGL && ! UNITY_EDITOR
                        try
                        {
                            if (IsApplixirAdReadyJS())
                            {
                                Debug.Log($"{LOG_PREFIX} [WATERFALL] Using Applixir (Priority 1)");
                                ShowApplixirRewardedAd(unitName, callback);
                                return;
                            }
                            else
                            {
                                Debug. LogWarning($"{LOG_PREFIX} [WATERFALL] Applixir not ready, falling back");
                            }
                        }
                        catch
                        {
                            Debug.LogWarning($"{LOG_PREFIX} [WATERFALL] Applixir check failed, falling back");
                        }
#else
                        // Editor simulation - use Applixir
                        Debug.Log($"{LOG_PREFIX} [WATERFALL] Using Applixir (Priority 1) [SIMULATED]");
                        ShowApplixirRewardedAd(unitName, callback);
                        return;
#endif
                    }
                    else
                    {
                        int remaining = (int)(_config.applixirAdCooldown - timeSinceLastAd);
                        Debug.LogWarning($"{LOG_PREFIX} [WATERFALL] Applixir on cooldown ({remaining}s), falling back");
                    }
                }
            }

            // Priority 2: AdSense (rewarded interstitial - lower eCPM $2-4)
            if (_config.enableAdSense)
            {
                var adSenseUnit = _config.GetAdSenseUnit(unitName);
                if (adSenseUnit != null && adSenseUnit.enabled)
                {
                    Debug.Log($"{LOG_PREFIX} [WATERFALL] Using AdSense (Priority 2)");
                    ShowAdSenseRewardedInterstitial(unitName, callback);
                    return;
                }
            }

            // No network available
            Debug.LogError($"{LOG_PREFIX} [WATERFALL] No network available for unit: {unitName}");
            callback?.Invoke(false, 0);
        }

        #endregion

        #region Unified API

        /// <summary>
        /// Show banner ad (uses AdSense)
        /// </summary>
        public static void ShowBannerAd(string unitName)
        {
            ShowAdSenseBanner(unitName);
        }

        /// <summary>
        /// Hide banner ad (uses AdSense)
        /// </summary>
        public static void HideBannerAd(string unitName)
        {
            HideAdSenseBanner(unitName);
        }

        /// <summary>
        /// Refresh banner ad (uses AdSense)
        /// </summary>
        public static void RefreshBannerAd(string unitName)
        {
            RefreshAdSenseBanner(unitName);
        }

        /// <summary>
        /// Show rewarded ad with waterfall mediation (Applixir → AdSense)
        /// </summary>
        public static void ShowRewardedAd(string unitName, Action<bool, int> callback = null)
        {
            ShowRewardedAdWithWaterfall(unitName, callback);
        }

        #endregion

        #region Helper Methods

        private static bool ValidateInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"{LOG_PREFIX} Not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        private static bool ValidateAdSenseEnabled()
        {
            if (_config == null || !_config.enableAdSense)
            {
                Debug.LogWarning($"{LOG_PREFIX} AdSense not enabled");
                return false;
            }
            return true;
        }

        private static bool ValidateApplixirEnabled()
        {
            if (_config == null || !_config.enableApplixir)
            {
                Debug.LogWarning($"{LOG_PREFIX} Applixir not enabled");
                return false;
            }
            return true;
        }

        private static void LogAdEvent(string eventName, WebGLAdNetwork network, string unitName, int reward = 0)
        {
            if (_config == null || !_config.enableAnalytics) return;

            var eventData = new Dictionary<string, object>
            {
                { "network", network.ToString() },
                { "unit_name", unitName },
                { "platform", "WebGL" }
            };

            if (reward > 0)
            {
                eventData["reward"] = reward;
            }

            // Log to console in debug builds
            if (Debug.isDebugBuild || Application.isEditor)
            {
                Debug.Log($"{LOG_PREFIX} [ANALYTICS] {eventName} | {network} | {unitName} | Reward: {reward}");
            }

            // Integration with IVXAnalytics if available
#if IVX_ANALYTICS_AVAILABLE
            try
            {
                IVXAnalytics.LogEvent(eventName, eventData);
            }
            catch { }
#endif
        }

        /// <summary>
        /// Check if ad is currently active/visible
        /// </summary>
        public static bool IsAdActive(string unitName)
        {
            return _activeAds.TryGetValue(unitName, out bool active) && active;
        }

        /// <summary>
        /// Get time until next Applixir ad available (seconds)
        /// </summary>
        public static int GetApplixirCooldownRemaining()
        {
            if (_config == null) return 0;
            var elapsed = (DateTime.Now - _lastApplixirAdTime).TotalSeconds;
            var remaining = _config.applixirAdCooldown - elapsed;
            return Mathf.Max(0, (int)remaining);
        }

        /// <summary>
        /// Get current configuration (read-only)
        /// </summary>
        public static IVXWebGLAdsConfig GetConfig()
        {
            return _config;
        }

        #endregion
    }
}