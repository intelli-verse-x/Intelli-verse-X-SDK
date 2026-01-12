// IVXAdNetworkConfig.cs
// Legacy configuration bridge - redirects to IVXAdsConfig in IntelliVerseXConfig
// Maintained for backward compatibility with existing code

using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// LEGACY: Ad network configuration for IntelliVerse-X platform.
    /// 
    /// NOTE: This class is maintained for backward compatibility.
    /// New code should use IntelliVerseXConfig.adsConfig instead.
    /// 
    /// The actual ad IDs are now stored in the IntelliVerseXConfig ScriptableObject,
    /// which can be configured per-game in the Unity Inspector.
    /// </summary>
    public static class IVXAdNetworkConfig
    {
        // ============================================
        // STATIC DEFAULTS (used if no config loaded)
        // ============================================

        #region IronSource / Unity LevelPlay Configuration

        /// <summary>
        /// IronSource App Key (iOS)
        /// </summary>
        public const string IRONSOURCE_APP_KEY_IOS = "23edc7cf5";

        /// <summary>
        /// IronSource App Key (Android)
        /// </summary>
        public const string IRONSOURCE_APP_KEY_ANDROID = "23ed37fb5";

        /// <summary>
        /// Enable IronSource mediation (Unity LevelPlay)
        /// </summary>
        public const bool IRONSOURCE_ENABLE_MEDIATION = true;

        public static string GetLevelPlayRewardedAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetLevelPlayRewardedId();

#if UNITY_ANDROID
            return "1ah2yohmmy0qqwbe";
#elif UNITY_IPHONE
            return "x7vb2kx3gid5pdbl";
#else
            return "unexpected_platform";
#endif
        }

        public static string GetLevelPlayInterstitialAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetLevelPlayInterstitialId();

#if UNITY_ANDROID
            return "52tmiwawdpyj30rl";
#elif UNITY_IPHONE
            return "kotf16z0o9lwut9z";
#else
            return "unexpected_platform";
#endif
        }

        public static string GetLevelPlayBannerAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetLevelPlayBannerId();

#if UNITY_ANDROID
            return "bampapqm2xqa97vh";
#elif UNITY_IPHONE
            return "ocfoon8gno7yneas";
#else
            return "unexpected_platform";
#endif
        }

        /// <summary>
        /// IronSource networks to mediate
        /// </summary>
        public static readonly string[] IRONSOURCE_MEDIATED_NETWORKS = new string[]
        {
            "AdMob",
            "Meta Audience Network",
            "Unity Ads",
            "AppLovin"
        };

        public static string GetIronSourceAppKey()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetIronSourceAppKey();

#if UNITY_IOS
            return IRONSOURCE_APP_KEY_IOS;
#elif UNITY_ANDROID
            return IRONSOURCE_APP_KEY_ANDROID;
#else
            return IRONSOURCE_APP_KEY_ANDROID;
#endif
        }

        #endregion

        #region AdMob Configuration

        /// <summary>
        /// AdMob App ID (iOS) - Test ID
        /// </summary>
        public const string ADMOB_APP_ID_IOS = "ca-app-pub-3940256099942544~1458002511";

        /// <summary>
        /// AdMob App ID (Android) - Test ID
        /// </summary>
        public const string ADMOB_APP_ID_ANDROID = "ca-app-pub-3940256099942544~3347511713";

        // Rewarded Ads
        public const string ADMOB_REWARDED_AD_UNIT_IOS = "ca-app-pub-3940256099942544/1712485313";
        public const string ADMOB_REWARDED_AD_UNIT_ANDROID = "ca-app-pub-3940256099942544/5224354917";

        // Interstitial Ads
        public const string ADMOB_INTERSTITIAL_AD_UNIT_IOS = "ca-app-pub-3940256099942544/4411468910";
        public const string ADMOB_INTERSTITIAL_AD_UNIT_ANDROID = "ca-app-pub-3940256099942544/1033173712";

        // Banner Ads
        public const string ADMOB_BANNER_AD_UNIT_IOS = "ca-app-pub-3940256099942544/2934735716";
        public const string ADMOB_BANNER_AD_UNIT_ANDROID = "ca-app-pub-3940256099942544/6300978111";

        public static string GetAdMobAppId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetAdMobAppId();

#if UNITY_IOS
            return ADMOB_APP_ID_IOS;
#elif UNITY_ANDROID
            return ADMOB_APP_ID_ANDROID;
#else
            return ADMOB_APP_ID_ANDROID;
#endif
        }

        public static string GetAdMobRewardedAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetAdMobRewardedId();

#if UNITY_IOS
            return ADMOB_REWARDED_AD_UNIT_IOS;
#elif UNITY_ANDROID
            return ADMOB_REWARDED_AD_UNIT_ANDROID;
#else
            return ADMOB_REWARDED_AD_UNIT_ANDROID;
#endif
        }

        public static string GetAdMobInterstitialAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetAdMobInterstitialId();

#if UNITY_IOS
            return ADMOB_INTERSTITIAL_AD_UNIT_IOS;
#elif UNITY_ANDROID
            return ADMOB_INTERSTITIAL_AD_UNIT_ANDROID;
#else
            return ADMOB_INTERSTITIAL_AD_UNIT_ANDROID;
#endif
        }

        public static string GetAdMobBannerAdUnitId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetAdMobBannerId();

#if UNITY_IOS
            return ADMOB_BANNER_AD_UNIT_IOS;
#elif UNITY_ANDROID
            return ADMOB_BANNER_AD_UNIT_ANDROID;
#else
            return ADMOB_BANNER_AD_UNIT_ANDROID;
#endif
        }

        #endregion

        #region Appodeal Configuration

        public const string APPODEAL_APP_KEY_ANDROID = "";
        public const string APPODEAL_APP_KEY_IOS = "";

        public static string GetAppodealAppKey()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetAppodealAppKey();

#if UNITY_IOS
            return APPODEAL_APP_KEY_IOS;
#elif UNITY_ANDROID
            return APPODEAL_APP_KEY_ANDROID;
#else
            return APPODEAL_APP_KEY_ANDROID;
#endif
        }

        #endregion

        #region Unity Ads Configuration

        public const string UNITY_ADS_GAME_ID_IOS = "";
        public const string UNITY_ADS_GAME_ID_ANDROID = "";
        public const string UNITY_ADS_REWARDED_PLACEMENT = "Rewarded_iOS";
        public const string UNITY_ADS_INTERSTITIAL_PLACEMENT = "Interstitial_iOS";
        public const string UNITY_ADS_BANNER_PLACEMENT = "Banner_iOS";

        public static string GetUnityAdsGameId()
        {
            if (_loadedConfig != null)
                return _loadedConfig.adsConfig.GetUnityAdsGameId();

#if UNITY_IOS
            return UNITY_ADS_GAME_ID_IOS;
#elif UNITY_ANDROID
            return UNITY_ADS_GAME_ID_ANDROID;
#else
            return UNITY_ADS_GAME_ID_ANDROID;
#endif
        }

        #endregion

        #region Network Priority & Waterfall

        /// <summary>
        /// Primary ad network - reads from loaded config or uses default
        /// </summary>
        public static IVXAdNetwork PRIMARY_AD_NETWORK
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.primaryNetwork;
                return IVXAdNetwork.IronSource;
            }
        }

        /// <summary>
        /// Fallback ad network - reads from loaded config or uses default
        /// </summary>
        public static IVXAdNetwork FALLBACK_AD_NETWORK
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.fallbackNetwork;
                return IVXAdNetwork.AdMob;
            }
        }

        /// <summary>
        /// Waterfall priority for mediation
        /// </summary>
        public static IVXAdNetworkPriority[] WATERFALL_PRIORITY => new IVXAdNetworkPriority[]
        {
            new IVXAdNetworkPriority(IVXAdNetwork.IronSource, 1, true, 15f),
            new IVXAdNetworkPriority(IVXAdNetwork.Appodeal, 2, true, 12f),
            new IVXAdNetworkPriority(IVXAdNetwork.AdMob, 3, true, 10f),
            new IVXAdNetworkPriority(IVXAdNetwork.UnityAds, 4, true, 10f)
        };

        #endregion

        #region Ad Settings

        /// <summary>
        /// Interstitial cooldown in seconds
        /// </summary>
        public static float INTERSTITIAL_COOLDOWN
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.interstitialCooldown;
                return 60f;
            }
        }

        /// <summary>
        /// Alias for backward compatibility
        /// </summary>
        public static float INTERSTITIAL_COOLDOWN_SECONDS => INTERSTITIAL_COOLDOWN;

        /// <summary>
        /// Max interstitials per session
        /// </summary>
        public static int MAX_INTERSTITIALS_PER_SESSION
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.maxInterstitialsPerSession;
                return 10;
            }
        }

        /// <summary>
        /// Banner refresh rate in seconds
        /// </summary>
        public static float BANNER_REFRESH_RATE
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.bannerRefreshRate;
                return 30f;
            }
        }

        /// <summary>
        /// Test mode enabled
        /// </summary>
        public static bool TEST_MODE
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.testMode;
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Enable consent flow
        /// </summary>
        public static bool ENABLE_CONSENT_FLOW
        {
            get
            {
                if (_loadedConfig != null)
                    return _loadedConfig.adsConfig.enableGDPRConsent;
                return true;
            }
        }

        /// <summary>
        /// Enable auto optimization for waterfall
        /// </summary>
        public static bool ENABLE_AUTO_OPTIMIZATION => true;

        /// <summary>
        /// Enable ad logging
        /// </summary>
        public static bool ENABLE_AD_LOGGING
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return _loadedConfig?.enableDebugLogs ?? false;
#endif
            }
        }

        #endregion

        #region Config Loading

        private static IntelliVerseXConfig _loadedConfig;

        /// <summary>
        /// Load configuration from IntelliVerseXConfig asset
        /// Call this during initialization
        /// </summary>
        public static void LoadConfig(IntelliVerseXConfig config)
        {
            _loadedConfig = config;

            if (config != null)
            {
                Debug.Log($"[IVXAdNetworkConfig] Loaded ads config from {config.name}");
                Debug.Log($"[IVXAdNetworkConfig] Primary: {config.adsConfig.primaryNetwork}, Fallback: {config.adsConfig.fallbackNetwork}");
            }
        }

        /// <summary>
        /// Get the loaded config (may be null)
        /// </summary>
        public static IntelliVerseXConfig LoadedConfig => _loadedConfig;

        /// <summary>
        /// Get the ads config (may be null)
        /// </summary>
        public static IVXAdsConfig AdsConfig => _loadedConfig?.adsConfig;

        /// <summary>
        /// Check if config is loaded
        /// </summary>
        public static bool IsConfigLoaded => _loadedConfig != null;

        #endregion
    }
}
