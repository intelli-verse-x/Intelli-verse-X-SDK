// ============================================================================
// IVXMoreOfUsModels.cs - "More Of Us" App Catalog Data Models
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Displays other apps by the same developer in a Netflix-style carousel
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.MoreOfUs
{
    // ========================================================================
    // ENUMS
    // ========================================================================

    /// <summary>
    /// Target platform for app catalog
    /// </summary>
    public enum IVXAppPlatform
    {
        Android,
        iOS
    }

    // ========================================================================
    // ANDROID APP CATALOG MODELS
    // ========================================================================

    #region Android Models

    /// <summary>
    /// Root response from Android app catalog JSON
    /// </summary>
    [Serializable]
    public class IVXAndroidAppCatalog
    {
        public string dataVersion;
        public string generatedAtUtc;
        public string developerName;
        public string developerId;
        public int totalApps;
        public List<IVXAndroidAppInfo> apps;

        public IVXAndroidAppCatalog()
        {
            apps = new List<IVXAndroidAppInfo>();
        }
    }

    /// <summary>
    /// Individual Android app info from Play Store catalog
    /// </summary>
    [Serializable]
    public class IVXAndroidAppInfo
    {
        public string appName;
        public string appIconUrl;
        public string playStoreUrl;
        public string appId;
        public string developerName;
        public string summary;
        public float score;
        public string ratings;
        public float price;
        public bool free;

        /// <summary>
        /// Convert to unified app info model
        /// </summary>
        public IVXUnifiedAppInfo ToUnified()
        {
            return new IVXUnifiedAppInfo
            {
                appName = appName,
                appIconUrl = appIconUrl,
                storeUrl = playStoreUrl,
                appId = appId,
                bundleId = appId,
                developerName = developerName,
                description = StripHtmlTags(summary),
                rating = score,
                ratingCount = ParseRatingCount(ratings),
                price = price,
                isFree = free,
                platform = IVXAppPlatform.Android
            };
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            
            // Simple HTML tag removal
            string result = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
            result = System.Text.RegularExpressions.Regex.Replace(result, "&amp;", "&");
            result = System.Text.RegularExpressions.Regex.Replace(result, "&#39;", "'");
            result = System.Text.RegularExpressions.Regex.Replace(result, "&quot;", "\"");
            result = System.Text.RegularExpressions.Regex.Replace(result, "\\s+", " ");
            return result.Trim();
        }

        private int ParseRatingCount(string ratings)
        {
            if (string.IsNullOrEmpty(ratings)) return 0;
            if (float.TryParse(ratings, out float val))
                return (int)val;
            return 0;
        }
    }

    #endregion

    // ========================================================================
    // iOS APP CATALOG MODELS
    // ========================================================================

    #region iOS Models

    /// <summary>
    /// Root response from iOS app catalog JSON
    /// </summary>
    [Serializable]
    public class IVXiOSAppCatalog
    {
        public string dataVersion;
        public string generatedAtUtc;
        public string developerName;
        public string developerId;
        public int totalApps;
        public List<IVXiOSAppInfo> apps;

        public IVXiOSAppCatalog()
        {
            apps = new List<IVXiOSAppInfo>();
        }
    }

    /// <summary>
    /// Individual iOS app info from App Store catalog
    /// </summary>
    [Serializable]
    public class IVXiOSAppInfo
    {
        public string appName;
        public string appIconUrl;
        public string appStoreUrl;
        public string bundleId;
        public long trackId;
        public string developerName;
        public string description;
        public string version;
        public float averageRating;
        public int ratingCount;
        public float price;
        public string formattedPrice;
        public string primaryGenre;
        public List<string> genres;
        public string releaseDate;
        public string currentVersionReleaseDate;
        public string minimumOsVersion;
        public List<string> screenshotUrls;
        public List<string> ipadScreenshotUrls;

        /// <summary>
        /// Convert to unified app info model
        /// </summary>
        public IVXUnifiedAppInfo ToUnified()
        {
            return new IVXUnifiedAppInfo
            {
                appName = appName,
                appIconUrl = appIconUrl,
                storeUrl = appStoreUrl,
                appId = trackId.ToString(),
                bundleId = bundleId,
                developerName = developerName,
                description = description,
                rating = averageRating,
                ratingCount = ratingCount,
                price = price,
                isFree = price <= 0,
                platform = IVXAppPlatform.iOS,
                version = version,
                primaryGenre = primaryGenre,
                genres = genres ?? new List<string>()
            };
        }
    }

    #endregion

    // ========================================================================
    // UNIFIED APP MODEL
    // ========================================================================

    #region Unified Model

    /// <summary>
    /// Unified app info that works across both platforms
    /// </summary>
    [Serializable]
    public class IVXUnifiedAppInfo
    {
        public string appName;
        public string appIconUrl;
        public string storeUrl;
        public string appId;
        public string bundleId;
        public string developerName;
        public string description;
        public float rating;
        public int ratingCount;
        public float price;
        public bool isFree;
        public IVXAppPlatform platform;
        public string version;
        public string primaryGenre;
        public List<string> genres;

        // Runtime cached icon
        [NonSerialized]
        public Texture2D cachedIcon;

        [NonSerialized]
        public bool iconLoadAttempted;

        public IVXUnifiedAppInfo()
        {
            genres = new List<string>();
        }

        /// <summary>
        /// Get a short description (first 150 chars)
        /// </summary>
        public string GetShortDescription(int maxLength = 150)
        {
            if (string.IsNullOrEmpty(description)) return string.Empty;
            if (description.Length <= maxLength) return description;
            return description.Substring(0, maxLength).TrimEnd() + "...";
        }

        /// <summary>
        /// Get formatted rating string
        /// </summary>
        public string GetRatingDisplay()
        {
            if (rating <= 0) return "No ratings";
            return $"{rating:F1} ({ratingCount} ratings)";
        }

        /// <summary>
        /// Get price display string
        /// </summary>
        public string GetPriceDisplay()
        {
            return isFree ? "FREE" : $"${price:F2}";
        }

        /// <summary>
        /// Check if this is the current running app
        /// </summary>
        public bool IsCurrentApp()
        {
            string currentBundleId = Application.identifier;
            return !string.IsNullOrEmpty(bundleId) && 
                   bundleId.Equals(currentBundleId, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Merged catalog containing apps from both platforms
    /// </summary>
    [Serializable]
    public class IVXMergedAppCatalog
    {
        public string dataVersion;
        public DateTime fetchedAtUtc;
        public List<IVXUnifiedAppInfo> apps;

        public IVXMergedAppCatalog()
        {
            apps = new List<IVXUnifiedAppInfo>();
            fetchedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Get apps excluding the current running app
        /// </summary>
        public List<IVXUnifiedAppInfo> GetOtherApps()
        {
            return apps.FindAll(a => !a.IsCurrentApp());
        }

        /// <summary>
        /// Get apps for the current platform
        /// </summary>
        public List<IVXUnifiedAppInfo> GetAppsForCurrentPlatform()
        {
#if UNITY_ANDROID
            return apps.FindAll(a => a.platform == IVXAppPlatform.Android && !a.IsCurrentApp());
#elif UNITY_IOS
            return apps.FindAll(a => a.platform == IVXAppPlatform.iOS && !a.IsCurrentApp());
#else
            // In editor or standalone, show all
            return GetOtherApps();
#endif
        }
    }

    #endregion

    // ========================================================================
    // CONFIGURATION
    // ========================================================================

    #region Configuration

    /// <summary>
    /// Configuration for "More Of Us" feature
    /// </summary>
    [Serializable]
    public class IVXMoreOfUsConfig
    {
        [Header("Data Sources")]
        [Tooltip("URL to Android app catalog JSON")]
        public string androidCatalogUrl = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/android.json";
        
        [Tooltip("URL to iOS app catalog JSON")]
        public string iosCatalogUrl = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/ios.json";

        [Header("Caching")]
        [Tooltip("Cache duration in hours")]
        public float cacheDurationHours = 24f;
        
        [Tooltip("Enable offline cache")]
        public bool enableOfflineCache = true;

        [Header("Display")]
        [Tooltip("Maximum apps to display")]
        public int maxAppsToDisplay = 10;
        
        [Tooltip("Show apps from both platforms in editor")]
        public bool showBothPlatformsInEditor = true;

        [Header("Animation")]
        [Tooltip("Card animation duration")]
        public float cardAnimationDuration = 0.3f;
        
        [Tooltip("Carousel auto-scroll interval (0 to disable)")]
        public float autoScrollInterval = 5f;

        /// <summary>
        /// Get the catalog URL for the current platform
        /// </summary>
        public string GetCatalogUrlForPlatform()
        {
#if UNITY_ANDROID
            return androidCatalogUrl;
#elif UNITY_IOS
            return iosCatalogUrl;
#else
            return androidCatalogUrl; // Default to Android in editor
#endif
        }
    }

    #endregion

    // ========================================================================
    // EVENTS
    // ========================================================================

    #region Events

    /// <summary>
    /// Event data for app card interactions
    /// </summary>
    public class IVXAppCardEventArgs : EventArgs
    {
        public IVXUnifiedAppInfo AppInfo { get; private set; }
        public string EventType { get; private set; }

        public IVXAppCardEventArgs(IVXUnifiedAppInfo appInfo, string eventType)
        {
            AppInfo = appInfo;
            EventType = eventType;
        }
    }

    #endregion
}
