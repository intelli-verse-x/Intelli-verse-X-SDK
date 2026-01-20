// ============================================================================
// IVXMoreOfUsHelper.cs - Helper class for easy More Of Us integration
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Simple static methods to show/hide the More Of Us panel
// ============================================================================

using UnityEngine;
using IntelliVerseX.MoreOfUs.UI;

namespace IntelliVerseX.MoreOfUs
{
    /// <summary>
    /// Static helper class for easy "More Of Us" panel integration.
    /// Use this class to show the cross-promotion panel from anywhere in your game.
    /// </summary>
    /// <example>
    /// <code>
    /// // Show the More Of Us panel
    /// IVXMoreOfUsHelper.Show();
    /// 
    /// // Hide the panel
    /// IVXMoreOfUsHelper.Hide();
    /// 
    /// // Toggle visibility
    /// IVXMoreOfUsHelper.Toggle();
    /// 
    /// // Pre-fetch data for faster display
    /// IVXMoreOfUsHelper.PreloadData();
    /// </code>
    /// </example>
    public static class IVXMoreOfUsHelper
    {
        private static IVXMoreOfUsCanvas _cachedCanvas;

        /// <summary>
        /// Show the "More Of Us" cross-promotion panel.
        /// Creates the canvas if it doesn't exist.
        /// Only shows on supported platforms (Android/iOS).
        /// </summary>
        public static void Show()
        {
            if (!IsSupportedPlatform)
            {
                Debug.Log("[IVXMoreOfUs] Not showing - unsupported platform. Only Android and iOS are supported.");
                return;
            }
            
            GetOrCreateCanvas()?.Show();
        }

        /// <summary>
        /// Hide the "More Of Us" panel.
        /// </summary>
        public static void Hide()
        {
            var canvas = FindCanvas();
            if (canvas != null)
                canvas.Hide();
        }

        /// <summary>
        /// Toggle the "More Of Us" panel visibility.
        /// Only toggles on supported platforms (Android/iOS).
        /// </summary>
        public static void Toggle()
        {
            if (!IsSupportedPlatform)
            {
                Debug.Log("[IVXMoreOfUs] Not toggling - unsupported platform. Only Android and iOS are supported.");
                return;
            }
            
            var canvas = GetOrCreateCanvas();
            if (canvas != null)
                canvas.Toggle();
        }

        /// <summary>
        /// Check if the panel is currently visible.
        /// </summary>
        public static bool IsVisible
        {
            get
            {
                var canvas = FindCanvas();
                return canvas != null && canvas.IsVisible;
            }
        }

        /// <summary>
        /// Pre-load app catalog data for faster display.
        /// Call this during loading screens or game initialization.
        /// Only works on supported platforms (Android/iOS).
        /// </summary>
        public static void PreloadData()
        {
            // Only preload on supported platforms
            if (!IsSupportedPlatform)
            {
                Debug.Log("[IVXMoreOfUs] Skipping preload - unsupported platform");
                return;
            }
            
            IVXMoreOfUsManager.Instance?.FetchCatalog();
        }

        /// <summary>
        /// Check if current platform is supported (Android or iOS).
        /// </summary>
        public static bool IsSupportedPlatform
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#elif UNITY_IOS && !UNITY_EDITOR
                return true;
#elif UNITY_EDITOR
                var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                return buildTarget == UnityEditor.BuildTarget.Android || 
                       buildTarget == UnityEditor.BuildTarget.iOS;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Get the total number of other apps available.
        /// Returns 0 if data hasn't been loaded yet or platform is unsupported.
        /// </summary>
        public static int OtherAppsCount
        {
            get
            {
                if (!IsSupportedPlatform)
                    return 0;
                if (!IVXMoreOfUsManager.HasInstance)
                    return 0;
                if (!IVXMoreOfUsManager.Instance.HasCachedData)
                    return 0;
                return IVXMoreOfUsManager.Instance.GetAppsForCurrentPlatform().Count;
            }
        }

        /// <summary>
        /// Clear cached data and force a refresh on next show.
        /// </summary>
        public static void ClearCache()
        {
            if (IVXMoreOfUsManager.HasInstance)
            {
                IVXMoreOfUsManager.Instance.ClearCache();
            }
        }

        /// <summary>
        /// Configure the catalog URLs if using custom endpoints.
        /// </summary>
        /// <param name="androidUrl">URL to Android app catalog JSON</param>
        /// <param name="iosUrl">URL to iOS app catalog JSON</param>
        public static void ConfigureUrls(string androidUrl, string iosUrl)
        {
            var config = IVXMoreOfUsManager.Instance?.Config;
            if (config == null) return;
            
            if (!string.IsNullOrEmpty(androidUrl))
                config.androidCatalogUrl = androidUrl;
            if (!string.IsNullOrEmpty(iosUrl))
                config.iosCatalogUrl = iosUrl;
        }

        /// <summary>
        /// Subscribe to app selection events.
        /// </summary>
        public static event System.Action<IVXUnifiedAppInfo> OnAppSelected
        {
            add
            {
                if (IVXMoreOfUsManager.HasInstance)
                    IVXMoreOfUsManager.Instance.OnAppSelected += value;
            }
            remove
            {
                if (IVXMoreOfUsManager.HasInstance)
                    IVXMoreOfUsManager.Instance.OnAppSelected -= value;
            }
        }

        #region Private Methods

        private static IVXMoreOfUsCanvas FindCanvas()
        {
            if (_cachedCanvas != null)
                return _cachedCanvas;

            _cachedCanvas = Object.FindObjectOfType<IVXMoreOfUsCanvas>();
            return _cachedCanvas;
        }

        private static IVXMoreOfUsCanvas GetOrCreateCanvas()
        {
            var canvas = FindCanvas();
            if (canvas != null)
                return canvas;

            // Try to create from prefab
            canvas = IVXMoreOfUsCanvas.Create();
            _cachedCanvas = canvas;
            return canvas;
        }

        #endregion
    }
}
