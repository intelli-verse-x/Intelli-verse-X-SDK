// ============================================================================
// IVXWebGLAdsConfig.cs
// WebGL-specific Ads Configuration for IntelliVerse-X SDK
// 
// Copyright (c) IntelliVerseX. All rights reserved.  
// Version: 2.0.0
// 
// This is a pure data/configuration file - NO native code. 
// Safe for all platforms (iOS, Android, WebGL, Standalone). 
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// WebGL-specific ads configuration for IntelliVerse-X SDK. 
    /// 
    /// Platform Safety:
    /// - This is a pure configuration ScriptableObject
    /// - Contains NO DllImport or native code
    /// - Safe to include in iOS, Android, and all builds
    /// 
    /// Supported Networks:
    /// 1. Google AdSense - Display, Native, In-Feed ads for WebGL
    /// 2. Applixir - Rewarded video ads optimized for WebGL
    /// 
    /// Waterfall Mediation (Automatic):
    /// Priority 1: Applixir (eCPM $8-15, rewarded video)
    /// Priority 2: AdSense (eCPM $2-4, rewarded interstitial)
    /// Result: +15-25% fill rate, maximizes revenue per impression
    /// 
    /// Revenue Potential (Monthly - 10k WebGL Players):
    /// - AdSense: $600-1,200/month (CPM: $2-4, 70-85% fill)
    /// - Applixir: $800-1,500/month (eCPM: $8-15, rewarded video)
    /// - Combined with Waterfall: $1,400-2,700/month
    /// 
    /// Usage:
    /// 1. Create asset: Right-click > Create > IntelliVerse-X > WebGL Ads Configuration
    /// 2. Configure publisher IDs and ad units in Inspector
    /// 3. Pass to IVXWebGLAdsManager. Initialize()
    /// </summary>
    [CreateAssetMenu(
        fileName = "WebGLAdsConfig",
        menuName = "IntelliVerse-X/WebGL Ads Configuration",
        order = 4
    )]
    public class IVXWebGLAdsConfig : ScriptableObject
    {
        #region Platform Selection

        [Header("=== Platform Selection ===")]
        [Tooltip("Enable Google AdSense (display, native, in-feed ads)")]
        public bool enableAdSense = true;

        [Tooltip("Enable Applixir (rewarded video, optimized for WebGL)")]
        public bool enableApplixir = true;

        [Tooltip("Only initialize on WebGL builds (recommended)")]
        public bool webGLOnly = true;

        #endregion

        #region AdSense Configuration

        [Header("=== Google AdSense Configuration ===")]
        [Tooltip("Publisher ID from AdSense (Format: ca-pub-XXXXXXXXXXXXXXXX)")]
        public string adSensePublisherId = "";

        [Tooltip("Enable AdSense Auto Ads (automatic ad placement)")]
        public bool enableAutoAds = false;

        [Tooltip("Enable AdSense ad blocking recovery")]
        public bool enableAdBlockRecovery = true;

        [Header("=== AdSense Ad Units ===")]
        [Tooltip("Display ad units (banner-style ads)")]
        public AdSenseAdUnit[] displayAdUnits = new AdSenseAdUnit[]
        {
            new AdSenseAdUnit
            {
                unitName = "Banner_Top",
                adSlotId = "",
                adFormat = AdSenseFormat. Display,
                adSize = AdSenseSize.Responsive,
                enabled = true
            }
        };

        [Tooltip("Native ad units (in-content ads)")]
        public AdSenseAdUnit[] nativeAdUnits = new AdSenseAdUnit[0];

        [Tooltip("In-Feed ad units (scrollable content ads)")]
        public AdSenseAdUnit[] inFeedAdUnits = new AdSenseAdUnit[0];

        #endregion

        #region Applixir Configuration

        [Header("=== Applixir Configuration ===")]
        [Tooltip("Zone ID from Applixir dashboard")]
        public string applixirZoneId = "";

        [Tooltip("Enable Applixir test mode (sandbox ads)")]
        public bool applixirTestMode = false;

        [Tooltip("Minimum seconds between rewarded ads")]
        [Range(15, 300)]
        public int applixirAdCooldown = 60;

        [Tooltip("Skip button delay in seconds (0 = no skip allowed)")]
        [Range(0, 30)]
        public int applixirSkipDelay = 5;

        [Header("=== Applixir Rewarded Video Units ===")]
        [Tooltip("Rewarded video placements")]
        public ApplixirRewardedUnit[] applixirRewardedUnits = new ApplixirRewardedUnit[]
        {
            new ApplixirRewardedUnit
            {
                unitName = "Rewarded_ExtraHints",
                coinReward = 50,
                videoLengthSeconds = 30,
                enabled = true
            },
            new ApplixirRewardedUnit
            {
                unitName = "Rewarded_ContinueGame",
                coinReward = 100,
                videoLengthSeconds = 30,
                enabled = true
            }
        };

        #endregion

        #region Ad Placement Strategy

        [Header("=== Ad Placement Strategy ===")]
        [Tooltip("Show banner ad on game load")]
        public bool showBannerOnLoad = true;

        [Tooltip("Auto-refresh banner ads (seconds, 0 = no refresh)")]
        [Range(0, 300)]
        public int bannerRefreshInterval = 60;

        [Tooltip("Show interstitial on level complete")]
        public bool showVideoOnLevelComplete = false;

        [Tooltip("Maximum ads per session (0 = unlimited)")]
        [Range(0, 50)]
        public int maxAdsPerSession = 10;

        #endregion

        #region Revenue Optimization

        [Header("=== Revenue Optimization ===")]
        [Tooltip("Prioritize Applixir over AdSense (higher eCPM)")]
        public bool prioritizeApplixir = true;

        [Tooltip("Enable waterfall mediation (Applixir → AdSense fallback)")]
        public bool enableWaterfall = true;

        [Tooltip("Reward amount for AdSense fallback ads")]
        [Range(1, 500)]
        public int adSenseFallbackReward = 50;

        [Tooltip("Enable A/B testing for ad placements")]
        public bool enableABTesting = false;

        [Tooltip("Track ad performance metrics")]
        public bool trackPerformanceMetrics = true;

        [Tooltip("Enable viewability tracking")]
        public bool enableViewabilityTracking = true;

        #endregion

        #region User Experience

        [Header("=== User Experience ===")]
        [Tooltip("Close button delay for video ads (seconds)")]
        [Range(0, 30)]
        public int closeButtonDelay = 5;

        [Tooltip("Show ad loading indicator")]
        public bool showLoadingIndicator = true;

        [Tooltip("Mute ads by default")]
        public bool muteAdsByDefault = false;

        [Tooltip("Remember user's mute preference")]
        public bool rememberMutePreference = true;

        #endregion

        #region Analytics

        [Header("=== Analytics Integration ===")]
        [Tooltip("Log ad events to analytics")]
        public bool enableAnalytics = true;

        [Tooltip("Track revenue attribution per network")]
        public bool trackRevenueAttribution = true;

        [Tooltip("Log ad errors and failures")]
        public bool logAdErrors = true;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get AdSense ad unit by name
        /// </summary>
        /// <param name="unitName">Name of the ad unit</param>
        /// <returns>AdSenseAdUnit or null if not found</returns>
        public AdSenseAdUnit GetAdSenseUnit(string unitName)
        {
            if (string.IsNullOrEmpty(unitName)) return null;

            // Search display units
            if (displayAdUnits != null)
            {
                foreach (var unit in displayAdUnits)
                {
                    if (unit != null && unit.unitName == unitName)
                        return unit;
                }
            }

            // Search native units
            if (nativeAdUnits != null)
            {
                foreach (var unit in nativeAdUnits)
                {
                    if (unit != null && unit.unitName == unitName)
                        return unit;
                }
            }

            // Search in-feed units
            if (inFeedAdUnits != null)
            {
                foreach (var unit in inFeedAdUnits)
                {
                    if (unit != null && unit.unitName == unitName)
                        return unit;
                }
            }

            return null;
        }

        /// <summary>
        /// Get Applixir rewarded unit by name
        /// </summary>
        /// <param name="unitName">Name of the rewarded unit</param>
        /// <returns>ApplixirRewardedUnit or null if not found</returns>
        public ApplixirRewardedUnit GetApplixirUnit(string unitName)
        {
            if (string.IsNullOrEmpty(unitName)) return null;

            if (applixirRewardedUnits != null)
            {
                foreach (var unit in applixirRewardedUnits)
                {
                    if (unit != null && unit.unitName == unitName)
                        return unit;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all enabled AdSense units
        /// </summary>
        public List<AdSenseAdUnit> GetAllEnabledAdSenseUnits()
        {
            var result = new List<AdSenseAdUnit>();

            if (displayAdUnits != null)
            {
                foreach (var unit in displayAdUnits)
                {
                    if (unit != null && unit.enabled)
                        result.Add(unit);
                }
            }

            if (nativeAdUnits != null)
            {
                foreach (var unit in nativeAdUnits)
                {
                    if (unit != null && unit.enabled)
                        result.Add(unit);
                }
            }

            if (inFeedAdUnits != null)
            {
                foreach (var unit in inFeedAdUnits)
                {
                    if (unit != null && unit.enabled)
                        result.Add(unit);
                }
            }

            return result;
        }

        /// <summary>
        /// Get all enabled Applixir units
        /// </summary>
        public List<ApplixirRewardedUnit> GetAllEnabledApplixirUnits()
        {
            var result = new List<ApplixirRewardedUnit>();

            if (applixirRewardedUnits != null)
            {
                foreach (var unit in applixirRewardedUnits)
                {
                    if (unit != null && unit.enabled)
                        result.Add(unit);
                }
            }

            return result;
        }

        /// <summary>
        /// Get primary ad network based on configuration
        /// </summary>
        public WebGLAdNetwork GetPrimaryNetwork()
        {
            if (enableApplixir && prioritizeApplixir)
                return WebGLAdNetwork.Applixir;

            if (enableAdSense)
                return WebGLAdNetwork.AdSense;

            if (enableApplixir)
                return WebGLAdNetwork.Applixir;

            return WebGLAdNetwork.None;
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";

            // Check at least one network enabled
            if (!enableAdSense && !enableApplixir)
            {
                errorMessage = "At least one ad network must be enabled (AdSense or Applixir)";
                return false;
            }

            // Validate AdSense config
            if (enableAdSense)
            {
                if (string.IsNullOrEmpty(adSensePublisherId))
                {
                    errorMessage = "AdSense Publisher ID is required when AdSense is enabled";
                    return false;
                }

                if (!adSensePublisherId.StartsWith("ca-pub-"))
                {
                    errorMessage = "Invalid AdSense Publisher ID format.  Should start with 'ca-pub-'";
                    return false;
                }

                // Check for at least one ad unit
                bool hasAdSenseUnit = (displayAdUnits != null && displayAdUnits.Length > 0) ||
                                      (nativeAdUnits != null && nativeAdUnits.Length > 0) ||
                                      (inFeedAdUnits != null && inFeedAdUnits.Length > 0);

                if (!hasAdSenseUnit)
                {
                    errorMessage = "At least one AdSense ad unit is required when AdSense is enabled";
                    return false;
                }
            }

            // Validate Applixir config
            if (enableApplixir)
            {
                if (string.IsNullOrEmpty(applixirZoneId))
                {
                    errorMessage = "Applixir Zone ID is required when Applixir is enabled";
                    return false;
                }

                // Check for at least one rewarded unit
                if (applixirRewardedUnits == null || applixirRewardedUnits.Length == 0)
                {
                    errorMessage = "At least one Applixir rewarded unit is required when Applixir is enabled";
                    return false;
                }
            }

            // Validate unique unit names
            var unitNames = new HashSet<string>();

            if (displayAdUnits != null)
            {
                foreach (var unit in displayAdUnits)
                {
                    if (unit != null && !string.IsNullOrEmpty(unit.unitName))
                    {
                        if (unitNames.Contains(unit.unitName))
                        {
                            errorMessage = $"Duplicate ad unit name: {unit.unitName}";
                            return false;
                        }
                        unitNames.Add(unit.unitName);
                    }
                }
            }

            if (nativeAdUnits != null)
            {
                foreach (var unit in nativeAdUnits)
                {
                    if (unit != null && !string.IsNullOrEmpty(unit.unitName))
                    {
                        if (unitNames.Contains(unit.unitName))
                        {
                            errorMessage = $"Duplicate ad unit name: {unit.unitName}";
                            return false;
                        }
                        unitNames.Add(unit.unitName);
                    }
                }
            }

            if (inFeedAdUnits != null)
            {
                foreach (var unit in inFeedAdUnits)
                {
                    if (unit != null && !string.IsNullOrEmpty(unit.unitName))
                    {
                        if (unitNames.Contains(unit.unitName))
                        {
                            errorMessage = $"Duplicate ad unit name: {unit.unitName}";
                            return false;
                        }
                        unitNames.Add(unit.unitName);
                    }
                }
            }

            if (applixirRewardedUnits != null)
            {
                foreach (var unit in applixirRewardedUnits)
                {
                    if (unit != null && !string.IsNullOrEmpty(unit.unitName))
                    {
                        if (unitNames.Contains(unit.unitName))
                        {
                            errorMessage = $"Duplicate ad unit name: {unit.unitName}";
                            return false;
                        }
                        unitNames.Add(unit.unitName);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Get estimated monthly revenue (based on 10k WebGL players)
        /// </summary>
        public string GetEstimatedRevenue()
        {
            float minRevenue = 0f;
            float maxRevenue = 0f;

            if (enableAdSense)
            {
                minRevenue += 600f;
                maxRevenue += 1200f;
            }

            if (enableApplixir)
            {
                minRevenue += 800f;
                maxRevenue += 1500f;
            }

            // Waterfall bonus
            if (enableWaterfall && enableAdSense && enableApplixir)
            {
                minRevenue *= 1.15f;
                maxRevenue *= 1.25f;
            }

            return $"${minRevenue:N0} - ${maxRevenue:N0}/month (10k players)";
        }

        /// <summary>
        /// Get estimated revenue as numeric values
        /// </summary>
        public (float min, float max) GetEstimatedRevenueValues()
        {
            float minRevenue = 0f;
            float maxRevenue = 0f;

            if (enableAdSense)
            {
                minRevenue += 600f;
                maxRevenue += 1200f;
            }

            if (enableApplixir)
            {
                minRevenue += 800f;
                maxRevenue += 1500f;
            }

            if (enableWaterfall && enableAdSense && enableApplixir)
            {
                minRevenue *= 1.15f;
                maxRevenue *= 1.25f;
            }

            return (minRevenue, maxRevenue);
        }

        /// <summary>
        /// Check if running on WebGL platform
        /// </summary>
        public bool IsWebGLPlatform()
        {
#if UNITY_WEBGL
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if running in Unity Editor
        /// </summary>
        public bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Should initialize on current platform? 
        /// </summary>
        public bool ShouldInitialize()
        {
            if (!webGLOnly)
                return true;

#if UNITY_WEBGL
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get total count of enabled ad networks
        /// </summary>
        public int GetEnabledNetworkCount()
        {
            int count = 0;
            if (enableAdSense) count++;
            if (enableApplixir) count++;
            return count;
        }

        /// <summary>
        /// Get summary string for debugging
        /// </summary>
        public string GetSummary()
        {
            var adSenseUnits = GetAllEnabledAdSenseUnits().Count;
            var applixirUnits = GetAllEnabledApplixirUnits().Count;

            return $"AdSense: {(enableAdSense ? $"ON ({adSenseUnits} units)" : "OFF")} | " +
                   $"Applixir: {(enableApplixir ? $"ON ({applixirUnits} units)" : "OFF")} | " +
                   $"Waterfall: {(enableWaterfall ? "ON" : "OFF")} | " +
                   $"Est. Revenue: {GetEstimatedRevenue()}";
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// WebGL Ad Network types
    /// NOTE: This enum is also defined in IVXWebGLMonetizationTypes.cs
    /// If you have both files, remove one definition to avoid conflicts. 
    /// </summary>
    // Uncomment below if IVXWebGLMonetizationTypes.cs doesn't exist
    /*
    public enum WebGLAdNetwork
    {
        None = 0,
        AdSense = 1,
        Applixir = 2
    }
    */

    /// <summary>
    /// AdSense ad format types
    /// </summary>
    public enum AdSenseFormat
    {
        /// <summary>Standard banner/display ads</summary>
        Display = 0,

        /// <summary>In-content native ads</summary>
        Native = 1,

        /// <summary>Scrollable feed ads</summary>
        InFeed = 2,

        /// <summary>Article-embedded ads</summary>
        InArticle = 3,

        /// <summary>Contextual matched ads</summary>
        Matched = 4
    }

    /// <summary>
    /// AdSense ad size options
    /// </summary>
    public enum AdSenseSize
    {
        /// <summary>Auto-size based on container</summary>
        Responsive = 0,

        /// <summary>728x90 pixels</summary>
        Leaderboard = 1,

        /// <summary>300x250 pixels</summary>
        MediumRectangle = 2,

        /// <summary>336x280 pixels</summary>
        LargeRectangle = 3,

        /// <summary>300x600 pixels</summary>
        HalfPage = 4,

        /// <summary>120x600 pixels</summary>
        Skyscraper = 5,

        /// <summary>160x600 pixels</summary>
        WideSkyscraper = 6,

        /// <summary>Custom size defined in ad unit</summary>
        Custom = 7
    }

    #endregion

    #region Data Classes

    /// <summary>
    /// Google AdSense ad unit configuration
    /// </summary>
    [Serializable]
    public class AdSenseAdUnit
    {
        [Tooltip("Unique identifier for this ad unit")]
        public string unitName = "Banner_Default";

        [Tooltip("AdSense ad slot ID (from AdSense dashboard)")]
        public string adSlotId = "";

        [Tooltip("Ad format type")]
        public AdSenseFormat adFormat = AdSenseFormat.Display;

        [Tooltip("Ad size (responsive recommended)")]
        public AdSenseSize adSize = AdSenseSize.Responsive;

        [Tooltip("Custom width in pixels (only used if adSize = Custom)")]
        [Range(50, 1200)]
        public int customWidth = 320;

        [Tooltip("Custom height in pixels (only used if adSize = Custom)")]
        [Range(50, 600)]
        public int customHeight = 50;

        [Tooltip("Enable this ad unit")]
        public bool enabled = true;

        /// <summary>
        /// Get ad size as string for JavaScript
        /// </summary>
        public string GetAdSizeString()
        {
            switch (adSize)
            {
                case AdSenseSize.Responsive:
                    return "auto";
                case AdSenseSize.Leaderboard:
                    return "728x90";
                case AdSenseSize.MediumRectangle:
                    return "300x250";
                case AdSenseSize.LargeRectangle:
                    return "336x280";
                case AdSenseSize.HalfPage:
                    return "300x600";
                case AdSenseSize.Skyscraper:
                    return "120x600";
                case AdSenseSize.WideSkyscraper:
                    return "160x600";
                case AdSenseSize.Custom:
                    return $"{customWidth}x{customHeight}";
                default:
                    return "auto";
            }
        }

        /// <summary>
        /// Get width in pixels (0 for responsive)
        /// </summary>
        public int GetWidth()
        {
            switch (adSize)
            {
                case AdSenseSize.Responsive:
                    return 0;
                case AdSenseSize.Leaderboard:
                    return 728;
                case AdSenseSize.MediumRectangle:
                    return 300;
                case AdSenseSize.LargeRectangle:
                    return 336;
                case AdSenseSize.HalfPage:
                    return 300;
                case AdSenseSize.Skyscraper:
                    return 120;
                case AdSenseSize.WideSkyscraper:
                    return 160;
                case AdSenseSize.Custom:
                    return customWidth;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Get height in pixels (0 for responsive)
        /// </summary>
        public int GetHeight()
        {
            switch (adSize)
            {
                case AdSenseSize.Responsive:
                    return 0;
                case AdSenseSize.Leaderboard:
                    return 90;
                case AdSenseSize.MediumRectangle:
                    return 250;
                case AdSenseSize.LargeRectangle:
                    return 280;
                case AdSenseSize.HalfPage:
                    return 600;
                case AdSenseSize.Skyscraper:
                    return 600;
                case AdSenseSize.WideSkyscraper:
                    return 600;
                case AdSenseSize.Custom:
                    return customHeight;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Clone this ad unit
        /// </summary>
        public AdSenseAdUnit Clone()
        {
            return new AdSenseAdUnit
            {
                unitName = this.unitName,
                adSlotId = this.adSlotId,
                adFormat = this.adFormat,
                adSize = this.adSize,
                customWidth = this.customWidth,
                customHeight = this.customHeight,
                enabled = this.enabled
            };
        }
    }

    /// <summary>
    /// Applixir rewarded video unit configuration
    /// </summary>
    [Serializable]
    public class ApplixirRewardedUnit
    {
        [Tooltip("Unique identifier for this rewarded unit")]
        public string unitName = "Rewarded_Default";

        [Tooltip("Coin/currency reward amount")]
        [Range(1, 1000)]
        public int coinReward = 50;

        [Tooltip("Typical video length in seconds (for UI display)")]
        [Range(5, 60)]
        public int videoLengthSeconds = 30;

        [Tooltip("Enable this rewarded unit")]
        public bool enabled = true;

        [Tooltip("Custom reward message (optional, leave empty for default)")]
        [TextArea(1, 3)]
        public string rewardMessage = "";

        /// <summary>
        /// Get reward message or generate default
        /// </summary>
        public string GetRewardMessage()
        {
            if (!string.IsNullOrEmpty(rewardMessage))
                return rewardMessage;

            return $"Watch a {videoLengthSeconds}s video to earn {coinReward} coins!";
        }

        /// <summary>
        /// Get short reward description
        /// </summary>
        public string GetShortDescription()
        {
            return $"+{coinReward} coins ({videoLengthSeconds}s)";
        }

        /// <summary>
        /// Clone this rewarded unit
        /// </summary>
        public ApplixirRewardedUnit Clone()
        {
            return new ApplixirRewardedUnit
            {
                unitName = this.unitName,
                coinReward = this.coinReward,
                videoLengthSeconds = this.videoLengthSeconds,
                enabled = this.enabled,
                rewardMessage = this.rewardMessage
            };
        }
    }

    #endregion
}