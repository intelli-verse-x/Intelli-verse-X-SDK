using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Offerwall configuration for IntelliVerse-X SDK.
    /// 
    /// Supported Platforms:
    /// 1. Pubscale - High-engagement offerwalls (surveys, videos, installs)
    /// 2. Xsolla - Premium offerwalls + IAP integration + Store builder
    /// 
    /// Revenue Potential (Monthly - 10k DAU):
    /// - Pubscale: $800-1,500/month (50-80% participation, $0.80-1.50 eCPO)
    /// - Xsolla: $1,200-2,500/month (with IAP integration, premium offers)
    /// 
    /// Config-Driven Architecture:
    /// - Create ScriptableObject asset (Right-click > IntelliVerse-X > Offerwall Configuration)
    /// - Configure app IDs, rewards, offer types via Inspector
    /// - Manager auto-initializes from config
    /// - No hardcoded values in game code
    /// 
    /// Features by Revenue Impact:
    /// HIGH REVENUE (Implement First):
    /// - Video offers ($0.50-2.00 per completion)
    /// - App install offers ($1.00-5.00 per install)
    /// - Survey offers ($0.30-3.00 per survey)
    /// 
    /// MEDIUM REVENUE:
    /// - Game achievement offers ($0.20-1.00)
    /// - Registration offers ($0.10-0.80)
    /// - Email subscription ($0.05-0.30)
    /// 
    /// LOW REVENUE (Optional):
    /// - Social shares ($0.01-0.10)
    /// - Newsletter signups ($0.02-0.15)
    /// </summary>
    [CreateAssetMenu(fileName = "IVXOfferwallConfig", menuName = "IntelliVerse-X/Offerwall Configuration", order = 3)]
    public class IVXOfferwallConfig : ScriptableObject
    {
        [Header("=== Platform Selection ===")]
        [Tooltip("Enable Pubscale offerwall (high engagement, good for casual games)")]
        public bool enablePubscale = true;
        
        [Tooltip("Enable Xsolla offerwall (premium offers, IAP integration)")]
        public bool enableXsolla = false;

        [Header("=== Pubscale Configuration ===")]
        [Tooltip("Get from: https://dashboard.pubscale.com/")]
        public string pubscaleAppId = "";
        
        [Tooltip("Pubscale Secret Key (for server validation)")]
        public string pubscaleSecretKey = "";
        
        [Tooltip("Enable Pubscale test mode (sandbox offers)")]
        public bool pubscaleTestMode = false;

        [Header("=== Xsolla Configuration ===")]
        [Tooltip("Get from: https://publisher.xsolla.com/")]
        public string xsollaProjectId = "";
        
        [Tooltip("Xsolla Merchant ID")]
        public string xsollaMerchantId = "";
        
        [Tooltip("Xsolla API Key (for backend calls)")]
        public string xsollaApiKey = "";
        
        [Tooltip("Enable Xsolla sandbox mode")]
        public bool xsollaTestMode = false;

        [Header("=== Offer Types (Revenue-Optimized) ===")]
        [Tooltip("Enable video offers (HIGH REVENUE: $0.50-2.00 per completion)")]
        public bool enableVideoOffers = true;
        
        [Tooltip("Enable app install offers (HIGH REVENUE: $1.00-5.00 per install)")]
        public bool enableAppInstallOffers = true;
        
        [Tooltip("Enable survey offers (HIGH REVENUE: $0.30-3.00 per survey)")]
        public bool enableSurveyOffers = true;
        
        [Tooltip("Enable game achievement offers (MEDIUM REVENUE: $0.20-1.00)")]
        public bool enableAchievementOffers = true;
        
        [Tooltip("Enable registration offers (MEDIUM REVENUE: $0.10-0.80)")]
        public bool enableRegistrationOffers = true;
        
        [Tooltip("Enable social/share offers (LOW REVENUE: $0.01-0.10)")]
        public bool enableSocialOffers = false;

        [Header("=== Reward Configuration ===")]
        [Tooltip("Currency type (coins, gems, points, etc.)")]
        public string rewardCurrencyName = "Coins";
        
        [Tooltip("Conversion rate: $1 USD = X coins (e.g., 100 means $1 = 100 coins)")]
        public int usdToCurrencyRate = 100;
        
        [Tooltip("Minimum offer payout in coins (filter low-value offers)")]
        public int minimumOfferReward = 10;
        
        [Tooltip("Enable bonus rewards for high-value offers")]
        public bool enableBonusRewards = true;
        
        [Tooltip("Bonus multiplier for offers >$2 (e.g., 1.5 = +50% bonus)")]
        public float bonusMultiplier = 1.5f;

        [Header("=== UI Configuration ===")]
        [Tooltip("Offerwall button placement in main menu")]
        public OfferwallButtonPlacement buttonPlacement = OfferwallButtonPlacement.TopRight;
        
        [Tooltip("Show offerwall notification badge when new offers available")]
        public bool showNotificationBadge = true;
        
        [Tooltip("Auto-refresh offers interval (seconds, 0 = manual only)")]
        public int autoRefreshInterval = 300; // 5 minutes
        
        [Tooltip("Show reward preview before opening offerwall")]
        public bool showRewardPreview = true;

        [Header("=== Revenue Optimization ===")]
        [Tooltip("Prioritize offers by eCPO (effective cost per offer)")]
        public bool prioritizeByRevenue = true;
        
        [Tooltip("Enable A/B testing for offerwall placements")]
        public bool enableABTesting = false;
        
        [Tooltip("Track user engagement metrics (completion rate, time spent)")]
        public bool trackEngagementMetrics = true;
        
        [Tooltip("Enable anti-fraud protection (validate conversions)")]
        public bool enableFraudProtection = true;

        [Header("=== Xsolla Exclusive Features ===")]
        [Tooltip("Enable Xsolla in-game store builder")]
        public bool enableXsollaStore = false;
        
        [Tooltip("Enable Xsolla subscription management")]
        public bool enableXsollaSubscriptions = false;
        
        [Tooltip("Enable Xsolla virtual currency management")]
        public bool enableXsollaVirtualCurrency = false;
        
        [Tooltip("Enable Xsolla Pay Station (payment processing)")]
        public bool enableXsollaPayStation = false;

        [Header("=== Analytics Integration ===")]
        [Tooltip("Log offerwall events to IVXAnalytics")]
        public bool enableAnalytics = true;
        
        [Tooltip("Track revenue attribution per offer type")]
        public bool trackRevenueAttribution = true;

        /// <summary>
        /// Get list of enabled offer types (sorted by revenue potential)
        /// </summary>
        public List<OfferwallOfferType> GetEnabledOfferTypes()
        {
            var enabledTypes = new List<OfferwallOfferType>();
            
            // HIGH REVENUE (priority 1)
            if (enableVideoOffers) enabledTypes.Add(OfferwallOfferType.Video);
            if (enableAppInstallOffers) enabledTypes.Add(OfferwallOfferType.AppInstall);
            if (enableSurveyOffers) enabledTypes.Add(OfferwallOfferType.Survey);
            
            // MEDIUM REVENUE (priority 2)
            if (enableAchievementOffers) enabledTypes.Add(OfferwallOfferType.GameAchievement);
            if (enableRegistrationOffers) enabledTypes.Add(OfferwallOfferType.Registration);
            
            // LOW REVENUE (priority 3)
            if (enableSocialOffers) enabledTypes.Add(OfferwallOfferType.Social);
            
            return enabledTypes;
        }

        /// <summary>
        /// Calculate coin reward from USD amount
        /// </summary>
        public int CalculateCoinReward(float usdAmount)
        {
            int baseReward = Mathf.RoundToInt(usdAmount * usdToCurrencyRate);
            
            // Apply bonus for high-value offers
            if (enableBonusRewards && usdAmount >= 2.0f)
            {
                baseReward = Mathf.RoundToInt(baseReward * bonusMultiplier);
            }
            
            return Mathf.Max(baseReward, minimumOfferReward);
        }

        /// <summary>
        /// Get primary offerwall platform
        /// </summary>
        public OfferwallPlatform GetPrimaryPlatform()
        {
            // Xsolla preferred if both enabled (higher revenue potential)
            if (enableXsolla) return OfferwallPlatform.Xsolla;
            if (enablePubscale) return OfferwallPlatform.Pubscale;
            return OfferwallPlatform.None;
        }

        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";
            
            if (!enablePubscale && !enableXsolla)
            {
                errorMessage = "At least one offerwall platform must be enabled";
                return false;
            }
            
            if (enablePubscale && string.IsNullOrEmpty(pubscaleAppId))
            {
                errorMessage = "Pubscale App ID is required";
                return false;
            }
            
            if (enableXsolla && string.IsNullOrEmpty(xsollaProjectId))
            {
                errorMessage = "Xsolla Project ID is required";
                return false;
            }
            
            if (usdToCurrencyRate <= 0)
            {
                errorMessage = "USD to currency rate must be positive";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get estimated monthly revenue (based on 10k DAU)
        /// </summary>
        public string GetEstimatedRevenue()
        {
            float minRevenue = 0f;
            float maxRevenue = 0f;
            
            if (enablePubscale)
            {
                minRevenue += 800f;
                maxRevenue += 1500f;
            }
            
            if (enableXsolla)
            {
                minRevenue += 1200f;
                maxRevenue += 2500f;
            }
            
            return $"${minRevenue:N0} - ${maxRevenue:N0}/month (10k DAU)";
        }
    }

    #region Enums

    public enum OfferwallPlatform
    {
        None,
        Pubscale,
        Xsolla
    }

    public enum OfferwallOfferType
    {
        Video,          // $0.50-2.00 per completion
        AppInstall,     // $1.00-5.00 per install
        Survey,         // $0.30-3.00 per survey
        GameAchievement,// $0.20-1.00 per achievement
        Registration,   // $0.10-0.80 per registration
        Social          // $0.01-0.10 per share
    }

    public enum OfferwallButtonPlacement
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        Custom
    }

    #endregion

    #region Data Classes

    /// <summary>
    /// Single offerwall offer
    /// </summary>
    [Serializable]
    public class OfferwallOffer
    {
        public string offerId;
        public string offerName;
        public string offerDescription;
        public OfferwallOfferType offerType;
        public int coinReward;
        public float usdValue;
        public string imageUrl;
        public int estimatedTimeMinutes;
        public OfferwallPlatform platform;
        public bool isAvailable;
        public DateTime expiryDate;
    }

    #endregion
}
