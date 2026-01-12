// ============================================================================
// IVXWebGLMonetizationSettings. cs
// ScriptableObject configuration for WebGL Monetization
// 
// Copyright (c) IntelliVerseX.  All rights reserved.
// Version: 2.1.0
// 
// Platform Safety:
// - This is a pure configuration ScriptableObject
// - Contains NO DllImport or native code
// - Safe to include in iOS, Android, WebGL, and all builds
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// ScriptableObject configuration for WebGL monetization. 
    /// 
    /// Platform Safety:
    /// - Pure data/configuration class
    /// - No native code or DllImport
    /// - Safe for all platforms (iOS, Android, WebGL, Standalone)
    /// 
    /// Create via: Assets > Create > IntelliVerse-X > WebGL Monetization Settings
    /// 
    /// Supported Features:
    /// - Multiple ad networks (GameMonetize, CrazyGames, LevelPlay, AdSense, Applixir)
    /// - Multiple game platforms (GamePix, Itch. io, Poki, Kongregate, Facebook Instant)
    /// - Payment providers (Stripe, Paddle, XUT Token)
    /// - Analytics (GA4, Unity Analytics)
    /// - Engagement features (fullscreen, idle detection, orientation)
    /// 
    /// Usage:
    /// 1. Create asset: Right-click > Create > IntelliVerse-X > WebGL Monetization Settings
    /// 2. Configure settings in Inspector
    /// 3. Pass to IVXWebGLMonetizationManager. Initialize(settings)
    /// </summary>
    [CreateAssetMenu(
        fileName = "WebGLMonetizationSettings",
        menuName = "IntelliVerse-X/WebGL Monetization Settings",
        order = 5
    )]
    public class IVXWebGLMonetizationSettings : ScriptableObject
    {
        #region WebGL Optimization

        [Header("=== WebGL Optimization ===")]
        [Tooltip("Enforce WebGL best practices during build")]
        public bool enforceWebGLOptimizations = true;

        [Tooltip("Enable decompression fallback for older browsers")]
        public bool enableDecompressionFallback = true;

        [Tooltip("Compression format for WebGL build")]
        public WebGLCompressionFormat compressionFormat = WebGLCompressionFormat.Brotli;

        [Tooltip("Enable WebAssembly streaming for faster load times")]
        public bool enableWasmStreaming = true;

        [Tooltip("Managed code stripping level (higher = smaller build)")]
        public ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.High;

        #endregion

        #region Game Orientation

        [Header("=== Game Orientation ===")]
        [Tooltip("Primary game orientation")]
        public GameOrientation gameOrientation = GameOrientation.Portrait;

        [Tooltip("Show rotate device overlay when in wrong orientation")]
        public bool showRotateOverlay = true;

        [Tooltip("Lock orientation (prevent auto-rotate)")]
        public bool lockOrientation = false;

        #endregion

        #region Ad Networks

        [Header("=== Ad Networks - GameMonetize ===")]
        [Tooltip("Enable GameMonetize Web SDK (eCPM $8-15)")]
        public bool enableGameMonetize = false;

        [Tooltip("GameMonetize Game ID from dashboard")]
        public string gameMonetizeGameId = "";

        [Header("=== Ad Networks - CrazyGames ===")]
        [Tooltip("Enable CrazyGames SDK (eCPM $12-20, highest revenue)")]
        public bool enableCrazyGames = false;

        [Tooltip("CrazyGames Game ID from dashboard")]
        public string crazyGamesGameId = "";

        [Header("=== Ad Networks - LevelPlay ===")]
        [Tooltip("Enable Unity LevelPlay Web (ironSource, eCPM $10-18)")]
        public bool enableLevelPlayWeb = false;

        [Tooltip("LevelPlay App Key from dashboard")]
        public string levelPlayAppKey = "";

        [Header("=== Ad Networks - AdSense & Applixir ===")]
        [Tooltip("Enable AdSense (display ads) and Applixir (rewarded video)")]
        public bool enableAdSenseApplixir = true;

        [Tooltip("AdSense Client ID (format: ca-pub-XXXXXXXXXXXXXXXX)")]
        public string adSenseClientId = "";

        [Tooltip("AdSense Ad Slot ID for display ads")]
        public string adSenseSlotId = "";

        [Tooltip("Applixir Zone ID for rewarded video")]
        public string applixirZoneId = "";

        #endregion

        #region Platform Integration

        [Header("=== Platform Integration - GamePix ===")]
        [Tooltip("Enable GamePix platform integration")]
        public bool enableGamePix = false;

        [Tooltip("GamePix Game ID")]
        public string gamePixGameId = "";

        [Header("=== Platform Integration - Itch. io ===")]
        [Tooltip("Enable Itch. io platform integration")]
        public bool enableItchIo = false;

        [Tooltip("Itch.io Game URL slug")]
        public string itchIoGameSlug = "";

        [Header("=== Platform Integration - Poki ===")]
        [Tooltip("Enable Poki platform integration (invite-only)")]
        public bool enablePoki = false;

        [Tooltip("Poki Game ID")]
        public string pokiGameId = "";

        [Header("=== Platform Integration - Kongregate ===")]
        [Tooltip("Enable Kongregate platform integration")]
        public bool enableKongregate = false;

        [Tooltip("Kongregate API Key")]
        public string kongregateApiKey = "";

        [Header("=== Platform Integration - Facebook Instant Games ===")]
        [Tooltip("Enable Facebook Instant Games platform")]
        public bool enableFacebookInstant = false;

        [Tooltip("Facebook App ID")]
        public string facebookAppId = "";

        #endregion

        #region Payment Providers

        [Header("=== Payment Providers - Stripe ===")]
        [Tooltip("Enable Stripe Web Checkout")]
        public bool enableStripe = false;

        [Tooltip("Stripe Publishable Key (pk_live_XXX or pk_test_XXX)")]
        public string stripePublishableKey = "";

        [Tooltip("Stripe webhook endpoint URL (optional)")]
        public string stripeWebhookUrl = "";

        [Header("=== Payment Providers - Paddle ===")]
        [Tooltip("Enable Paddle payments (good for subscriptions)")]
        public bool enablePaddle = false;

        [Tooltip("Paddle Vendor ID")]
        public string paddleVendorId = "";

        [Tooltip("Paddle Product ID")]
        public string paddleProductId = "";

        [Tooltip("Paddle Environment (sandbox/production)")]
        public bool paddleSandbox = true;

        [Header("=== Payment Providers - XUT Token ===")]
        [Tooltip("Enable XUT Token crypto payments")]
        public bool enableXUTToken = false;

        [Tooltip("XUT Token API Endpoint URL")]
        public string xutApiEndpoint = "";

        [Tooltip("XUT Token Contract Address")]
        public string xutTokenContractAddress = "";

        [Tooltip("XUT Token Chain ID (e.g., 1 for Ethereum mainnet)")]
        public int xutChainId = 1;

        #endregion

        #region Analytics

        [Header("=== Analytics - Google Analytics 4 ===")]
        [Tooltip("Enable Google Analytics 4")]
        public bool enableGA4 = true;

        [Tooltip("GA4 Measurement ID (format: G-XXXXXXXXXX)")]
        public string ga4MeasurementId = "";

        [Tooltip("Enable enhanced measurement (scroll, outbound clicks, etc.)")]
        public bool ga4EnhancedMeasurement = true;

        [Header("=== Analytics - Unity Analytics ===")]
        [Tooltip("Enable Unity Analytics")]
        public bool enableUnityAnalytics = true;

        [Header("=== Analytics - Custom Events ===")]
        [Tooltip("Track custom events to your backend")]
        public bool trackCustomEvents = true;

        [Tooltip("Custom event endpoint URL")]
        public string customEventEndpoint = "";

        [Tooltip("Custom event API key (optional)")]
        public string customEventApiKey = "";

        #endregion

        #region Engagement

        [Header("=== Engagement - Fullscreen ===")]
        [Tooltip("Auto-trigger fullscreen on first user interaction")]
        public bool autoFullscreen = true;

        [Tooltip("Show fullscreen prompt/button")]
        public bool showFullscreenButton = true;

        [Header("=== Engagement - Idle Detection ===")]
        [Tooltip("Seconds before showing idle prompt (0 = disabled)")]
        [Range(0, 600)]
        public int idlePromptSeconds = 45;

        [Tooltip("Custom idle prompt message")]
        public string idlePromptMessage = "Still there?  Tap to continue! ";

        [Header("=== Engagement - Tab Visibility ===")]
        [Tooltip("Pause game when browser tab is hidden")]
        public bool autoPauseOnHidden = true;

        [Tooltip("Mute audio when tab is hidden")]
        public bool autoMuteOnHidden = true;

        [Header("=== Engagement - Tracking ===")]
        [Tooltip("Track rage quits (closing tab during gameplay)")]
        public bool trackRageQuits = true;

        [Tooltip("Track session duration")]
        public bool trackSessionDuration = true;

        [Tooltip("Track return visits")]
        public bool trackReturnVisits = true;

        #endregion

        #region Revenue Optimization

        [Header("=== Revenue Optimization ===")]
        [Tooltip("Prioritize ad networks by eCPM (higher revenue first)")]
        public bool prioritizeByRevenue = true;

        [Tooltip("Enable waterfall mediation (fallback to next network if first fails)")]
        public bool enableWaterfall = true;

        [Tooltip("Enable A/B testing for ad placements")]
        public bool enableABTesting = false;

        [Tooltip("A/B test variant (A or B)")]
        [Range(0, 1)]
        public int abTestVariant = 0;

        [Tooltip("Minimum seconds between ads")]
        [Range(15, 300)]
        public int minAdCooldown = 30;

        [Tooltip("Maximum ads per session (0 = unlimited)")]
        [Range(0, 50)]
        public int maxAdsPerSession = 0;

        #endregion

        #region Debug Settings

        [Header("=== Debug Settings ===")]
        [Tooltip("Enable verbose logging")]
        public bool enableDebugLogging = false;

        [Tooltip("Use test/sandbox mode for all ad networks")]
        public bool useTestAds = false;

        [Tooltip("Simulate ad responses in Editor")]
        public bool simulateAdsInEditor = true;

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validate configuration
        /// </summary>
        /// <param name="error">Error message if validation fails</param>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid(out string error)
        {
            error = null;

            // Check if at least one feature is enabled
            bool hasAdNetwork = enableGameMonetize || enableCrazyGames ||
                               enableLevelPlayWeb || enableAdSenseApplixir;
            bool hasPayment = enableStripe || enablePaddle || enableXUTToken;
            bool hasAnalytics = enableGA4 || enableUnityAnalytics;
            bool hasPlatform = enableGamePix || enableItchIo || enablePoki ||
                              enableKongregate || enableFacebookInstant;

            if (!hasAdNetwork && !hasPayment && !hasAnalytics && !hasPlatform)
            {
                error = "No monetization features enabled.  Enable at least one ad network, payment provider, analytics, or platform. ";
                return false;
            }

            // Validate Ad Networks
            if (enableGameMonetize && string.IsNullOrWhiteSpace(gameMonetizeGameId))
            {
                error = "GameMonetize enabled but Game ID is empty";
                return false;
            }

            if (enableCrazyGames && string.IsNullOrWhiteSpace(crazyGamesGameId))
            {
                error = "CrazyGames enabled but Game ID is empty";
                return false;
            }

            if (enableLevelPlayWeb && string.IsNullOrWhiteSpace(levelPlayAppKey))
            {
                error = "LevelPlay Web enabled but App Key is empty";
                return false;
            }

            if (enableAdSenseApplixir)
            {
                if (string.IsNullOrWhiteSpace(adSenseClientId) && string.IsNullOrWhiteSpace(applixirZoneId))
                {
                    error = "AdSense/Applixir enabled but both Client ID and Zone ID are empty";
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(adSenseClientId) && !adSenseClientId.StartsWith("ca-pub-"))
                {
                    error = "Invalid AdSense Client ID format.  Should start with 'ca-pub-'";
                    return false;
                }
            }

            // Validate Payment Providers
            if (enableStripe && string.IsNullOrWhiteSpace(stripePublishableKey))
            {
                error = "Stripe enabled but Publishable Key is empty";
                return false;
            }

            if (enableStripe && !string.IsNullOrWhiteSpace(stripePublishableKey))
            {
                if (!stripePublishableKey.StartsWith("pk_live_") && !stripePublishableKey.StartsWith("pk_test_"))
                {
                    error = "Invalid Stripe Publishable Key format.  Should start with 'pk_live_' or 'pk_test_'";
                    return false;
                }
            }

            if (enablePaddle && string.IsNullOrWhiteSpace(paddleVendorId))
            {
                error = "Paddle enabled but Vendor ID is empty";
                return false;
            }

            if (enableXUTToken)
            {
                if (string.IsNullOrWhiteSpace(xutApiEndpoint))
                {
                    error = "XUT Token enabled but API Endpoint is empty";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(xutTokenContractAddress))
                {
                    error = "XUT Token enabled but Contract Address is empty";
                    return false;
                }
            }

            // Validate Analytics
            if (enableGA4 && string.IsNullOrWhiteSpace(ga4MeasurementId))
            {
                error = "GA4 enabled but Measurement ID is empty";
                return false;
            }

            if (enableGA4 && !string.IsNullOrWhiteSpace(ga4MeasurementId))
            {
                if (!ga4MeasurementId.StartsWith("G-"))
                {
                    error = "Invalid GA4 Measurement ID format. Should start with 'G-'";
                    return false;
                }
            }

            // Validate Platforms
            if (enablePoki && string.IsNullOrWhiteSpace(pokiGameId))
            {
                error = "Poki enabled but Game ID is empty";
                return false;
            }

            if (enableKongregate && string.IsNullOrWhiteSpace(kongregateApiKey))
            {
                error = "Kongregate enabled but API Key is empty";
                return false;
            }

            if (enableFacebookInstant && string.IsNullOrWhiteSpace(facebookAppId))
            {
                error = "Facebook Instant Games enabled but App ID is empty";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get all validation errors (not just the first one)
        /// </summary>
        public List<string> GetAllValidationErrors()
        {
            var errors = new List<string>();

            // Ad Networks
            if (enableGameMonetize && string.IsNullOrWhiteSpace(gameMonetizeGameId))
                errors.Add("GameMonetize: Game ID is empty");

            if (enableCrazyGames && string.IsNullOrWhiteSpace(crazyGamesGameId))
                errors.Add("CrazyGames: Game ID is empty");

            if (enableLevelPlayWeb && string.IsNullOrWhiteSpace(levelPlayAppKey))
                errors.Add("LevelPlay: App Key is empty");

            if (enableAdSenseApplixir)
            {
                if (string.IsNullOrWhiteSpace(adSenseClientId) && string.IsNullOrWhiteSpace(applixirZoneId))
                    errors.Add("AdSense/Applixir: Both IDs are empty");

                if (!string.IsNullOrWhiteSpace(adSenseClientId) && !adSenseClientId.StartsWith("ca-pub-"))
                    errors.Add("AdSense: Invalid Client ID format");
            }

            // Payment Providers
            if (enableStripe)
            {
                if (string.IsNullOrWhiteSpace(stripePublishableKey))
                    errors.Add("Stripe: Publishable Key is empty");
                else if (!stripePublishableKey.StartsWith("pk_"))
                    errors.Add("Stripe: Invalid Publishable Key format");
            }

            if (enablePaddle && string.IsNullOrWhiteSpace(paddleVendorId))
                errors.Add("Paddle: Vendor ID is empty");

            if (enableXUTToken)
            {
                if (string.IsNullOrWhiteSpace(xutApiEndpoint))
                    errors.Add("XUT Token: API Endpoint is empty");
                if (string.IsNullOrWhiteSpace(xutTokenContractAddress))
                    errors.Add("XUT Token: Contract Address is empty");
            }

            // Analytics
            if (enableGA4)
            {
                if (string.IsNullOrWhiteSpace(ga4MeasurementId))
                    errors.Add("GA4: Measurement ID is empty");
                else if (!ga4MeasurementId.StartsWith("G-"))
                    errors.Add("GA4: Invalid Measurement ID format");
            }

            // Platforms
            if (enablePoki && string.IsNullOrWhiteSpace(pokiGameId))
                errors.Add("Poki: Game ID is empty");

            if (enableKongregate && string.IsNullOrWhiteSpace(kongregateApiKey))
                errors.Add("Kongregate: API Key is empty");

            if (enableFacebookInstant && string.IsNullOrWhiteSpace(facebookAppId))
                errors.Add("Facebook Instant: App ID is empty");

            return errors;
        }

        #endregion

        #region Revenue Estimation

        /// <summary>
        /// Get estimated monthly revenue (based on 10k players)
        /// </summary>
        public float GetEstimatedRevenue()
        {
            float total = 0f;

            // Ad Networks (eCPM-based estimates)
            if (enableGameMonetize) total += 900f;      // $600-1200 range
            if (enableCrazyGames) total += 1150f;       // $800-1500 range (highest)
            if (enableLevelPlayWeb) total += 800f;      // $500-1100 range
            if (enableAdSenseApplixir) total += 2050f;  // $1400-2700 combined

            // Payment Providers (IAP-based estimates)
            if (enableStripe) total += 600f;            // $400-800 range
            if (enablePaddle) total += 600f;            // $400-800 range
            if (enableXUTToken) total += 400f;          // $200-600 range

            // Waterfall bonus
            if (enableWaterfall && GetActiveNetworkCount() > 1)
            {
                total *= 1.15f; // +15% for waterfall mediation
            }

            return total;
        }

        /// <summary>
        /// Get estimated revenue as formatted string
        /// </summary>
        public string GetEstimatedRevenueString()
        {
            float revenue = GetEstimatedRevenue();
            float minRevenue = revenue * 0.7f;
            float maxRevenue = revenue * 1.3f;

            return $"${minRevenue:N0} - ${maxRevenue:N0}/month (10k players)";
        }

        /// <summary>
        /// Get estimated revenue range
        /// </summary>
        public (float min, float max) GetEstimatedRevenueRange()
        {
            float revenue = GetEstimatedRevenue();
            return (revenue * 0.7f, revenue * 1.3f);
        }

        #endregion

        #region Count Methods

        /// <summary>
        /// Get count of enabled ad networks
        /// </summary>
        public int GetActiveNetworkCount()
        {
            int count = 0;
            if (enableGameMonetize) count++;
            if (enableCrazyGames) count++;
            if (enableLevelPlayWeb) count++;
            if (enableAdSenseApplixir) count++;
            return count;
        }

        /// <summary>
        /// Get count of enabled payment providers
        /// </summary>
        public int GetActivePaymentProviderCount()
        {
            int count = 0;
            if (enableStripe) count++;
            if (enablePaddle) count++;
            if (enableXUTToken) count++;
            return count;
        }

        /// <summary>
        /// Get count of enabled platforms
        /// </summary>
        public int GetActivePlatformCount()
        {
            int count = 0;
            if (enableGamePix) count++;
            if (enableItchIo) count++;
            if (enablePoki) count++;
            if (enableKongregate) count++;
            if (enableFacebookInstant) count++;
            return count;
        }

        /// <summary>
        /// Get count of enabled analytics
        /// </summary>
        public int GetActiveAnalyticsCount()
        {
            int count = 0;
            if (enableGA4) count++;
            if (enableUnityAnalytics) count++;
            if (trackCustomEvents && !string.IsNullOrEmpty(customEventEndpoint)) count++;
            return count;
        }

        /// <summary>
        /// Get total count of all enabled features
        /// </summary>
        public int GetTotalEnabledFeatureCount()
        {
            return GetActiveNetworkCount() +
                   GetActivePaymentProviderCount() +
                   GetActivePlatformCount() +
                   GetActiveAnalyticsCount();
        }

        #endregion

        #region Platform Detection

        /// <summary>
        /// Check if running on WebGL platform
        /// </summary>
        public bool IsWebGLPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if running in Editor
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
        /// Check if this configuration should be used on current platform
        /// </summary>
        public bool ShouldInitializeOnCurrentPlatform()
        {
#if UNITY_WEBGL
            return true;
#elif UNITY_EDITOR
            return simulateAdsInEditor;
#else
            return false;
#endif
        }

        #endregion

        #region Conversion Methods

        /// <summary>
        /// Convert to runtime config for IVXWebGLMonetizationManager
        /// </summary>
        public IVXWebGLMonetizationConfig ToRuntimeConfig()
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

                // Revenue Optimization
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

        #endregion

        #region Summary Methods

        /// <summary>
        /// Get configuration summary for debugging
        /// </summary>
        public string GetSummary()
        {
            return $"Ad Networks: {GetActiveNetworkCount()} | " +
                   $"Payment: {GetActivePaymentProviderCount()} | " +
                   $"Platforms: {GetActivePlatformCount()} | " +
                   $"Analytics: {GetActiveAnalyticsCount()} | " +
                   $"Est. Revenue: {GetEstimatedRevenueString()}";
        }

        /// <summary>
        /// Get detailed configuration info
        /// </summary>
        public string GetDetailedInfo()
        {
            var info = new System.Text.StringBuilder();

            info.AppendLine("=== WebGL Monetization Settings ===");
            info.AppendLine();

            // Ad Networks
            info.AppendLine("Ad Networks:");
            if (enableGameMonetize) info.AppendLine($"  • GameMonetize: {gameMonetizeGameId}");
            if (enableCrazyGames) info.AppendLine($"  • CrazyGames: {crazyGamesGameId}");
            if (enableLevelPlayWeb) info.AppendLine($"  • LevelPlay: {levelPlayAppKey}");
            if (enableAdSenseApplixir) info.AppendLine($"  • AdSense/Applixir: {adSenseClientId} / {applixirZoneId}");
            if (GetActiveNetworkCount() == 0) info.AppendLine("  (none enabled)");
            info.AppendLine();

            // Payment Providers
            info.AppendLine("Payment Providers:");
            if (enableStripe) info.AppendLine($"  • Stripe: {(stripePublishableKey?.StartsWith("pk_test_") == true ? "TEST" : "LIVE")}");
            if (enablePaddle) info.AppendLine($"  • Paddle: {paddleVendorId}");
            if (enableXUTToken) info.AppendLine($"  • XUT Token: Chain {xutChainId}");
            if (GetActivePaymentProviderCount() == 0) info.AppendLine("  (none enabled)");
            info.AppendLine();

            // Analytics
            info.AppendLine("Analytics:");
            if (enableGA4) info.AppendLine($"  • GA4: {ga4MeasurementId}");
            if (enableUnityAnalytics) info.AppendLine("  • Unity Analytics: Enabled");
            if (trackCustomEvents) info.AppendLine($"  • Custom Events: {customEventEndpoint}");
            if (GetActiveAnalyticsCount() == 0) info.AppendLine("  (none enabled)");
            info.AppendLine();

            // Revenue
            info.AppendLine($"Estimated Revenue: {GetEstimatedRevenueString()}");
            info.AppendLine($"Waterfall: {(enableWaterfall ? "Enabled" : "Disabled")}");

            return info.ToString();
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Validate and log results (Editor only)
        /// </summary>
        [ContextMenu("Validate Configuration")]
        private void EditorValidate()
        {
            if (IsValid(out string error))
            {
                Debug.Log($"[IVXWebGLMonetizationSettings] ✓ Configuration is valid\n{GetSummary()}");
            }
            else
            {
                Debug.LogError($"[IVXWebGLMonetizationSettings] ✗ Configuration invalid: {error}");
            }
        }

        /// <summary>
        /// Log detailed info (Editor only)
        /// </summary>
        [ContextMenu("Show Detailed Info")]
        private void EditorShowDetailedInfo()
        {
            Debug.Log(GetDetailedInfo());
        }

        /// <summary>
        /// Log all validation errors (Editor only)
        /// </summary>
        [ContextMenu("Show All Validation Errors")]
        private void EditorShowAllErrors()
        {
            var errors = GetAllValidationErrors();
            if (errors.Count == 0)
            {
                Debug.Log("[IVXWebGLMonetizationSettings] ✓ No validation errors");
            }
            else
            {
                Debug.LogWarning($"[IVXWebGLMonetizationSettings] Found {errors.Count} validation error(s):\n" +
                                string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Reset to default values (Editor only)
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        private void EditorResetToDefaults()
        {
            enableGameMonetize = false;
            gameMonetizeGameId = "";
            enableCrazyGames = false;
            crazyGamesGameId = "";
            enableLevelPlayWeb = false;
            levelPlayAppKey = "";
            enableAdSenseApplixir = true;
            adSenseClientId = "";
            adSenseSlotId = "";
            applixirZoneId = "";
            enableStripe = false;
            stripePublishableKey = "";
            enablePaddle = false;
            paddleVendorId = "";
            enableXUTToken = false;
            xutApiEndpoint = "";
            enableGA4 = true;
            ga4MeasurementId = "";
            enableUnityAnalytics = true;
            autoFullscreen = true;
            idlePromptSeconds = 45;
            autoPauseOnHidden = true;
            prioritizeByRevenue = true;
            minAdCooldown = 30;

            Debug.Log("[IVXWebGLMonetizationSettings] Reset to default values");
        }
#endif

        #endregion
    }
}