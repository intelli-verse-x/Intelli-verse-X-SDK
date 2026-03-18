// IVXAdsConfig.cs
// Production-ready ads configuration for IntelliVerseX Games SDK
// Integrates with IntelliVerseXConfig for centralized game configuration

using System;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Supported ad networks in the IntelliVerseX SDK.
    /// </summary>
    public enum IVXAdNetwork
    {
        None = 0,
        IronSource = 1,         // Unity LevelPlay - Primary recommended
        AdMob = 2,              // Google AdMob
        UnityAds = 3,           // Unity Ads
        MetaAudience = 4,       // Meta Audience Network
        MetaAudienceNetwork = 4, // Alias for MetaAudience
        Appodeal = 5,           // Appodeal with auto-mediation
        AppLovin = 6            // AppLovin MAX
    }

    /// <summary>
    /// Ad types supported by the SDK.
    /// </summary>
    public enum IVXAdType
    {
        Banner = 0,
        Interstitial = 1,
        Rewarded = 2,
        Offerwall = 3,
        Native = 4
    }

    /// <summary>
    /// Embedded ads configuration for IntelliVerseXConfig.
    /// Contains all ad network IDs and settings for a game.
    /// </summary>
    [Serializable]
    public class IVXAdsConfig
    {
        [Header("Primary Network")]
        [Tooltip("Primary ad network to use")]
        public IVXAdNetwork primaryNetwork = IVXAdNetwork.IronSource;

        [Tooltip("Fallback network if primary fails")]
        public IVXAdNetwork fallbackNetwork = IVXAdNetwork.AdMob;

        [Header("IronSource / Unity LevelPlay")]
        [Tooltip("IronSource App Key for iOS")]
        public string ironSourceAppKeyIOS = "23edc7cf5";

        [Tooltip("IronSource App Key for Android")]
        public string ironSourceAppKeyAndroid = "23ed37fb5";

        [Tooltip("LevelPlay Rewarded Ad Unit ID (iOS)")]
        public string levelPlayRewardedIdIOS = "x7vb2kx3gid5pdbl";

        [Tooltip("LevelPlay Rewarded Ad Unit ID (Android)")]
        public string levelPlayRewardedIdAndroid = "1ah2yohmmy0qqwbe";

        [Tooltip("LevelPlay Interstitial Ad Unit ID (iOS)")]
        public string levelPlayInterstitialIdIOS = "kotf16z0o9lwut9z";

        [Tooltip("LevelPlay Interstitial Ad Unit ID (Android)")]
        public string levelPlayInterstitialIdAndroid = "52tmiwawdpyj30rl";

        [Tooltip("LevelPlay Banner Ad Unit ID (iOS)")]
        public string levelPlayBannerIdIOS = "ocfoon8gno7yneas";

        [Tooltip("LevelPlay Banner Ad Unit ID (Android)")]
        public string levelPlayBannerIdAndroid = "bampapqm2xqa97vh";

        [Header("AdMob")]
        [Tooltip("AdMob App ID for iOS")]
        public string admobAppIdIOS = "ca-app-pub-3940256099942544~1458002511";

        [Tooltip("AdMob App ID for Android")]
        public string admobAppIdAndroid = "ca-app-pub-3940256099942544~3347511713";

        [Tooltip("AdMob Rewarded Ad Unit ID (iOS)")]
        public string admobRewardedIdIOS = "ca-app-pub-3940256099942544/1712485313";

        [Tooltip("AdMob Rewarded Ad Unit ID (Android)")]
        public string admobRewardedIdAndroid = "ca-app-pub-3940256099942544/5224354917";

        [Tooltip("AdMob Interstitial Ad Unit ID (iOS)")]
        public string admobInterstitialIdIOS = "ca-app-pub-3940256099942544/4411468910";

        [Tooltip("AdMob Interstitial Ad Unit ID (Android)")]
        public string admobInterstitialIdAndroid = "ca-app-pub-3940256099942544/1033173712";

        [Tooltip("AdMob Banner Ad Unit ID (iOS)")]
        public string admobBannerIdIOS = "ca-app-pub-3940256099942544/2934735716";

        [Tooltip("AdMob Banner Ad Unit ID (Android)")]
        public string admobBannerIdAndroid = "ca-app-pub-3940256099942544/6300978111";

        [Header("Appodeal")]
        [Tooltip("Appodeal App Key for iOS")]
        public string appodealAppKeyIOS = "";

        [Tooltip("Appodeal App Key for Android")]
        public string appodealAppKeyAndroid = "";

        [Header("Unity Ads")]
        [Tooltip("Unity Ads Game ID for iOS")]
        public string unityAdsGameIdIOS = "";

        [Tooltip("Unity Ads Game ID for Android")]
        public string unityAdsGameIdAndroid = "";

        [Tooltip("Unity Ads Rewarded Placement ID")]
        public string unityAdsRewardedPlacement = "Rewarded_iOS";

        [Tooltip("Unity Ads Interstitial Placement ID")]
        public string unityAdsInterstitialPlacement = "Interstitial_iOS";

        [Tooltip("Unity Ads Banner Placement ID")]
        public string unityAdsBannerPlacement = "Banner_iOS";

        [Header("WebGL / AppLixir (Only for WebGL builds)")]
        [Tooltip("Enable AppLixir for WebGL rewarded ads")]
        public bool enableAppLixir = true;

        [Tooltip("AppLixir Zone ID (get from AppLixir dashboard)")]
        public string appLixirZoneId = "";

        [Tooltip("Enable AppLixir test mode")]
        public bool appLixirTestMode = false;

        [Tooltip("AppLixir ad cooldown in seconds")]
        public int appLixirCooldown = 60;

        [Tooltip("Skip button delay in seconds (0 = no skip)")]
        public int appLixirSkipDelay = 5;

        [Header("WebGL / AdSense (Only for WebGL builds)")]
        [Tooltip("Enable AdSense for WebGL display ads")]
        public bool enableAdSense = false;

        [Tooltip("AdSense Publisher ID (ca-pub-XXXXXXXX)")]
        public string adSensePublisherId = "";

        [Tooltip("AdSense Banner Slot ID")]
        public string adSenseBannerSlotId = "";

        [Header("Ad Settings")]
        [Tooltip("Enable ad mediation (waterfall)")]
        public bool enableMediation = true;

        [Tooltip("Interstitial cooldown (seconds) between shows")]
        public float interstitialCooldown = 60f;

        [Tooltip("Max interstitials per session")]
        public int maxInterstitialsPerSession = 10;

        [Tooltip("Banner auto-refresh rate (seconds, 0 = disabled)")]
        public float bannerRefreshRate = 30f;

        [Tooltip("Enable test mode (show test ads)")]
        public bool testMode = false;

        [Header("Consent & Privacy")]
        [Tooltip("Enable GDPR consent flow (UMP SDK)")]
        public bool enableGDPRConsent = true;

        [Tooltip("Enable CCPA compliance")]
        public bool enableCCPA = true;

        [Tooltip("Enable COPPA for child-directed apps")]
        public bool enableCOPPA = false;

        [Tooltip("Test device IDs for UMP consent debugging")]
        public string[] umpTestDeviceIds = new string[0];

        #region Platform-Specific Getters

        /// <summary>
        /// Get IronSource app key for current platform
        /// </summary>
        public string GetIronSourceAppKey()
        {
#if UNITY_IOS
            return ironSourceAppKeyIOS;
#else
            return ironSourceAppKeyAndroid;
#endif
        }

        /// <summary>
        /// Get LevelPlay rewarded ad unit ID for current platform
        /// </summary>
        public string GetLevelPlayRewardedId()
        {
#if UNITY_IOS
            return levelPlayRewardedIdIOS;
#else
            return levelPlayRewardedIdAndroid;
#endif
        }

        /// <summary>
        /// Get LevelPlay interstitial ad unit ID for current platform
        /// </summary>
        public string GetLevelPlayInterstitialId()
        {
#if UNITY_IOS
            return levelPlayInterstitialIdIOS;
#else
            return levelPlayInterstitialIdAndroid;
#endif
        }

        /// <summary>
        /// Get LevelPlay banner ad unit ID for current platform
        /// </summary>
        public string GetLevelPlayBannerId()
        {
#if UNITY_IOS
            return levelPlayBannerIdIOS;
#else
            return levelPlayBannerIdAndroid;
#endif
        }

        /// <summary>
        /// Get AdMob app ID for current platform
        /// </summary>
        public string GetAdMobAppId()
        {
#if UNITY_IOS
            return admobAppIdIOS;
#else
            return admobAppIdAndroid;
#endif
        }

        /// <summary>
        /// Get AdMob rewarded ad unit ID for current platform
        /// </summary>
        public string GetAdMobRewardedId()
        {
#if UNITY_IOS
            return admobRewardedIdIOS;
#else
            return admobRewardedIdAndroid;
#endif
        }

        /// <summary>
        /// Get AdMob interstitial ad unit ID for current platform
        /// </summary>
        public string GetAdMobInterstitialId()
        {
#if UNITY_IOS
            return admobInterstitialIdIOS;
#else
            return admobInterstitialIdAndroid;
#endif
        }

        /// <summary>
        /// Get AdMob banner ad unit ID for current platform
        /// </summary>
        public string GetAdMobBannerId()
        {
#if UNITY_IOS
            return admobBannerIdIOS;
#else
            return admobBannerIdAndroid;
#endif
        }

        /// <summary>
        /// Get Appodeal app key for current platform
        /// </summary>
        public string GetAppodealAppKey()
        {
#if UNITY_IOS
            return appodealAppKeyIOS;
#else
            return appodealAppKeyAndroid;
#endif
        }

        /// <summary>
        /// Get Unity Ads game ID for current platform
        /// </summary>
        public string GetUnityAdsGameId()
        {
#if UNITY_IOS
            return unityAdsGameIdIOS;
#else
            return unityAdsGameIdAndroid;
#endif
        }

        #endregion

        #region WebGL Getters

        /// <summary>
        /// Get AppLixir zone ID
        /// </summary>
        public string GetAppLixirZoneId() => appLixirZoneId;

        /// <summary>
        /// Get AdSense publisher ID
        /// </summary>
        public string GetAdSensePublisherId() => adSensePublisherId;

        /// <summary>
        /// Get AdSense banner slot ID
        /// </summary>
        public string GetAdSenseBannerSlotId() => adSenseBannerSlotId;

        /// <summary>
        /// Check if WebGL ads are configured
        /// </summary>
        public bool IsWebGLConfigured()
        {
            return enableAppLixir && !string.IsNullOrEmpty(appLixirZoneId);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate ads configuration
        /// </summary>
        public bool Validate(out string error)
        {
            error = null;

            // WebGL validation
#if UNITY_WEBGL
            if (enableAppLixir && string.IsNullOrEmpty(appLixirZoneId))
            {
                error = "AppLixir Zone ID is required for WebGL builds";
                return false;
            }
            return true;
#endif

            if (primaryNetwork == IVXAdNetwork.None)
            {
                error = "No primary ad network selected";
                return false;
            }

            // Validate primary network has required IDs
            switch (primaryNetwork)
            {
                case IVXAdNetwork.IronSource:
                    if (string.IsNullOrEmpty(GetIronSourceAppKey()))
                    {
                        error = "IronSource app key is required";
                        return false;
                    }
                    break;

                case IVXAdNetwork.AdMob:
                    if (string.IsNullOrEmpty(GetAdMobAppId()))
                    {
                        error = "AdMob app ID is required";
                        return false;
                    }
                    break;

                case IVXAdNetwork.Appodeal:
                    if (string.IsNullOrEmpty(GetAppodealAppKey()))
                    {
                        error = "Appodeal app key is required";
                        return false;
                    }
                    break;

                case IVXAdNetwork.UnityAds:
                    if (string.IsNullOrEmpty(GetUnityAdsGameId()))
                    {
                        error = "Unity Ads game ID is required";
                        return false;
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Get a summary of the ads configuration for logging
        /// </summary>
        public string GetConfigSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== IVX Ads Config Summary ===");
            sb.AppendLine($"Primary Network: {primaryNetwork}");
            sb.AppendLine($"Fallback Network: {fallbackNetwork}");
            sb.AppendLine($"Test Mode: {testMode}");
            sb.AppendLine($"GDPR Consent: {enableGDPRConsent}");
            sb.AppendLine($"COPPA: {enableCOPPA}");
#if UNITY_WEBGL
            sb.AppendLine($"AppLixir: {enableAppLixir} (Zone: {appLixirZoneId})");
            sb.AppendLine($"AdSense: {enableAdSense}");
#endif
            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Priority configuration for waterfall mediation
    /// </summary>
    [Serializable]
    public class IVXAdNetworkPriority
    {
        public IVXAdNetwork Network;
        public int Priority; // Lower = higher priority
        public bool Enabled;
        public float TimeoutSeconds;

        /// <summary>
        /// Alias for TimeoutSeconds (backward compatibility)
        /// </summary>
        public float Timeout => TimeoutSeconds;

        public IVXAdNetworkPriority(IVXAdNetwork network, int priority, bool enabled = true, float timeout = 10f)
        {
            Network = network;
            Priority = priority;
            Enabled = enabled;
            TimeoutSeconds = timeout;
        }
    }

    /// <summary>
    /// Network performance statistics
    /// </summary>
    [Serializable]
    public class IVXNetworkStats
    {
        public IVXAdNetwork Network;
        public int Attempts;
        public int Successes;
        public float Revenue;
        public float AverageEcpm;

        public float FillRate => Attempts > 0 ? (float)Successes / Attempts : 0f;
    }
}

