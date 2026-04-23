// ============================================================================
// IVXMoreOfUsModels.cs - "More Of Us" App Catalog Data Models
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Displays other apps by the same developer in a Netflix-style carousel
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IntelliVerseX.MoreOfUs
{
    // ========================================================================
    // STATIC REGEX CACHE (avoids GC allocations)
    // ========================================================================
    
    /// <summary>
    /// Cached compiled regex patterns to avoid repeated allocations
    /// </summary>
    internal static class IVXHtmlUtils
    {
        private static readonly Regex HtmlTagRegex = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new Regex("\\s+", RegexOptions.Compiled);
        
        /// <summary>
        /// Strip HTML tags from text efficiently using cached regex
        /// </summary>
        public static string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html)) 
                return string.Empty;
            
            // Use compiled regex for performance
            string result = HtmlTagRegex.Replace(html, " ");
            result = result.Replace("&amp;", "&");
            result = result.Replace("&#39;", "'");
            result = result.Replace("&quot;", "\"");
            result = result.Replace("&lt;", "<");
            result = result.Replace("&gt;", ">");
            result = result.Replace("&nbsp;", " ");
            result = WhitespaceRegex.Replace(result, " ");
            return result.Trim();
        }
    }

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
                description = IVXHtmlUtils.StripHtmlTags(summary),
                rating = score,
                ratingCount = ParseRatingCount(ratings),
                price = price,
                isFree = free,
                platform = IVXAppPlatform.Android
            };
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
        /// Get apps for the current platform.
        /// Returns only Android apps on Android, only iOS apps on iOS.
        /// In Editor, uses the active build target to simulate platform behavior.
        /// Returns empty list on unsupported platforms (Standalone, WebGL, etc.).
        /// </summary>
        public List<IVXUnifiedAppInfo> GetAppsForCurrentPlatform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return apps.FindAll(a => a.platform == IVXAppPlatform.Android && !a.IsCurrentApp());
#elif UNITY_IOS && !UNITY_EDITOR
            return apps.FindAll(a => a.platform == IVXAppPlatform.iOS && !a.IsCurrentApp());
#elif UNITY_EDITOR
            // In Editor, use the active build target to simulate platform behavior
            var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget == UnityEditor.BuildTarget.Android)
            {
                return apps.FindAll(a => a.platform == IVXAppPlatform.Android && !a.IsCurrentApp());
            }
            else if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                return apps.FindAll(a => a.platform == IVXAppPlatform.iOS && !a.IsCurrentApp());
            }
            else
            {
                // Unsupported platform in Editor - return empty list
                return new List<IVXUnifiedAppInfo>();
            }
#else
            // Unsupported platforms (Standalone, WebGL, etc.) - return empty list
            return new List<IVXUnifiedAppInfo>();
#endif
        }
    }

    #endregion

    // ========================================================================
    // ENRICHED INDEX CATALOG (single JSON for iOS + Android)
    // ========================================================================

    #region Enriched Index Catalog

    /// <summary>
    /// Root JSON from the enriched app catalog index (JsonUtility-compatible subset; extra JSON fields are ignored).
    /// </summary>
    [Serializable]
    public class IVXEnrichedAppCatalogIndex
    {
        public string dataVersion;
        public List<IVXEnrichedIndexApp> apps;

        public IVXEnrichedAppCatalogIndex()
        {
            apps = new List<IVXEnrichedIndexApp>();
        }
    }

    [Serializable]
    public class IVXEnrichedIndexApp
    {
        public string appName;
        public string bundleId;
        public IVXEnrichedIndexIos ios;
        public IVXEnrichedIndexAndroid android;
    }

    [Serializable]
    public class IVXEnrichedIndexIos
    {
        public bool available;
        public long trackId;
        public string appStoreUrl;
        public string appIconUrl;
        public float rating;
        public int ratingCount;
        public string version;
        public List<string> genres;
    }

    [Serializable]
    public class IVXEnrichedIndexAndroid
    {
        public bool available;
        public string packageName;
        public string playStoreUrl;
        public string appIconUrl;
        public float rating;
    }

    /// <summary>
    /// Maps enriched index JSON into the same <see cref="IVXUnifiedAppInfo"/> rows the dual-catalog flow produced.
    /// </summary>
    public static class IVXEnrichedCatalogMapper
    {
        public static void AppendEnrichedIndexToMerged(IVXEnrichedAppCatalogIndex index, IVXMergedAppCatalog merged)
        {
            if (index == null || merged == null)
                return;

            if (!string.IsNullOrEmpty(index.dataVersion))
                merged.dataVersion = index.dataVersion;

            if (index.apps == null)
                return;

            foreach (var app in index.apps)
            {
                if (app == null)
                    continue;

                TryAddAndroidRow(app, merged.apps);
                TryAddIosRow(app, merged.apps);
            }
        }

        private static void TryAddAndroidRow(IVXEnrichedIndexApp app, List<IVXUnifiedAppInfo> target)
        {
            var a = app.android;
            if (a == null || !a.available || string.IsNullOrWhiteSpace(a.playStoreUrl))
                return;

            var packageId = !string.IsNullOrWhiteSpace(a.packageName) ? a.packageName : app.bundleId;
            if (string.IsNullOrWhiteSpace(packageId))
                return;

            var bundle = !string.IsNullOrWhiteSpace(app.bundleId) ? app.bundleId : packageId;

            target.Add(new IVXUnifiedAppInfo
            {
                appName = app.appName ?? string.Empty,
                appIconUrl = a.appIconUrl ?? string.Empty,
                storeUrl = a.playStoreUrl,
                appId = packageId,
                bundleId = bundle,
                developerName = string.Empty,
                description = string.Empty,
                rating = a.rating,
                ratingCount = 0,
                price = 0f,
                isFree = true,
                platform = IVXAppPlatform.Android
            });
        }

        private static void TryAddIosRow(IVXEnrichedIndexApp app, List<IVXUnifiedAppInfo> target)
        {
            var i = app.ios;
            if (i == null || !i.available || string.IsNullOrWhiteSpace(i.appStoreUrl))
                return;

            var bundle = app.bundleId ?? string.Empty;
            var primaryGenre = (i.genres != null && i.genres.Count > 0) ? i.genres[0] : string.Empty;
            var genres = i.genres != null ? new List<string>(i.genres) : new List<string>();

            target.Add(new IVXUnifiedAppInfo
            {
                appName = app.appName ?? string.Empty,
                appIconUrl = i.appIconUrl ?? string.Empty,
                storeUrl = i.appStoreUrl,
                appId = i.trackId > 0 ? i.trackId.ToString() : bundle,
                bundleId = bundle,
                developerName = string.Empty,
                description = string.Empty,
                rating = i.rating,
                ratingCount = i.ratingCount,
                price = 0f,
                isFree = true,
                platform = IVXAppPlatform.iOS,
                version = i.version ?? string.Empty,
                primaryGenre = primaryGenre,
                genres = genres
            });
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
        [Tooltip("Enriched app catalog index (iOS + Android in one JSON file).")]
        public string catalogIndexUrl =
            "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/enriched/intelliversex/index.json";

        /*
        [Tooltip("URL to Android app catalog JSON")]
        public string androidCatalogUrl = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/android.json";

        [Tooltip("URL to iOS app catalog JSON")]
        public string iosCatalogUrl = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/ios.json";
        */

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
        /// Returns the enriched catalog index URL (single source for all platforms).
        /// </summary>
        public string GetCatalogUrlForPlatform()
        {
            return catalogIndexUrl;
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
