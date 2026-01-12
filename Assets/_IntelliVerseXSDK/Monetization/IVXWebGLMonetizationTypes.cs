// ============================================================================
// IVXWebGLMonetizationTypes.cs
// Shared types for WebGL Monetization System
// 
// Copyright (c) IntelliVerseX.  All rights reserved. 
// Version: 2.1.0
// 
// PLATFORM SAFETY:
// - This file contains ONLY enums, structs, and interfaces
// - NO DllImport or native code
// - 100% safe for all platforms (iOS, Android, WebGL, Standalone)
// - Can be included in any build without issues
// ============================================================================

using System;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    #region Orientation & Display

    /// <summary>
    /// Game orientation preference for WebGL builds
    /// </summary>
    public enum GameOrientation
    {
        /// <summary>Automatic orientation based on device/browser</summary>
        Auto = 0,

        /// <summary>Force portrait orientation (9:16)</summary>
        Portrait = 1,

        /// <summary>Force landscape orientation (16:9)</summary>
        Landscape = 2
    }

    /// <summary>
    /// Screen orientation lock state
    /// </summary>
    public enum OrientationLockState
    {
        /// <summary>Orientation is not locked</summary>
        Unlocked = 0,

        /// <summary>Orientation is locked to current</summary>
        Locked = 1,

        /// <summary>Orientation lock is not supported</summary>
        NotSupported = 2
    }

    #endregion

    #region Ad Types

    /// <summary>
    /// Ad types supported by the monetization system
    /// </summary>
    public enum AdType
    {
        /// <summary>Banner ad (persistent, usually at top/bottom)</summary>
        Banner = 0,

        /// <summary>Interstitial ad (fullscreen, skippable)</summary>
        Interstitial = 1,

        /// <summary>Rewarded ad (fullscreen, gives reward on completion)</summary>
        Rewarded = 2,

        /// <summary>Native ad (blends with content)</summary>
        Native = 3,

        /// <summary>In-feed ad (appears in scrollable content)</summary>
        InFeed = 4
    }

    /// <summary>
    /// Ad placement positions
    /// </summary>
    public enum AdPlacement
    {
        /// <summary>Top of screen</summary>
        Top = 0,

        /// <summary>Bottom of screen</summary>
        Bottom = 1,

        /// <summary>Left side of screen</summary>
        Left = 2,

        /// <summary>Right side of screen</summary>
        Right = 3,

        /// <summary>Center of screen (for interstitials)</summary>
        Center = 4,

        /// <summary>Custom position</summary>
        Custom = 5
    }

    /// <summary>
    /// Ad loading state
    /// </summary>
    public enum AdLoadState
    {
        /// <summary>Ad not loaded</summary>
        NotLoaded = 0,

        /// <summary>Ad is loading</summary>
        Loading = 1,

        /// <summary>Ad loaded and ready to show</summary>
        Ready = 2,

        /// <summary>Ad is currently showing</summary>
        Showing = 3,

        /// <summary>Ad load failed</summary>
        Failed = 4
    }

    /// <summary>
    /// WebGL Ad Network types
    /// </summary>
    public enum WebGLAdNetwork
    {
        /// <summary>No network</summary>
        None = 0,

        /// <summary>Google AdSense</summary>
        AdSense = 1,

        /// <summary>Applixir rewarded video</summary>
        Applixir = 2,

        /// <summary>GameMonetize</summary>
        GameMonetize = 3,

        /// <summary>CrazyGames</summary>
        CrazyGames = 4,

        /// <summary>Unity LevelPlay (ironSource)</summary>
        LevelPlay = 5,

        /// <summary>Poki</summary>
        Poki = 6,

        /// <summary>GamePix</summary>
        GamePix = 7,

        /// <summary>Facebook Instant Games</summary>
        FacebookInstant = 8
    }

    #endregion

    #region Payment Types

    /// <summary>
    /// Payment provider types
    /// </summary>
    public enum PaymentProvider
    {
        /// <summary>No provider</summary>
        None = 0,

        /// <summary>Stripe payment</summary>
        Stripe = 1,

        /// <summary>Paddle payment</summary>
        Paddle = 2,

        /// <summary>XUT Token crypto payment</summary>
        XUTToken = 3,

        /// <summary>PayPal payment</summary>
        PayPal = 4,

        /// <summary>Platform-specific payment (Facebook, etc.)</summary>
        Platform = 5
    }

    /// <summary>
    /// Payment status
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Payment not started</summary>
        None = 0,

        /// <summary>Payment pending/processing</summary>
        Pending = 1,

        /// <summary>Payment completed successfully</summary>
        Completed = 2,

        /// <summary>Payment failed</summary>
        Failed = 3,

        /// <summary>Payment cancelled by user</summary>
        Cancelled = 4,

        /// <summary>Payment refunded</summary>
        Refunded = 5
    }

    /// <summary>
    /// Currency types
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>US Dollars</summary>
        USD = 0,

        /// <summary>Euros</summary>
        EUR = 1,

        /// <summary>British Pounds</summary>
        GBP = 2,

        /// <summary>Cryptocurrency (XUT Token, etc.)</summary>
        Crypto = 3,

        /// <summary>In-game virtual currency</summary>
        Virtual = 4
    }

    #endregion

    #region WebGL Build Settings

    /// <summary>
    /// WebGL compression format options
    /// </summary>
    public enum WebGLCompressionFormat
    {
        /// <summary>Gzip compression (widely supported)</summary>
        Gzip = 0,

        /// <summary>Brotli compression (best compression, modern browsers)</summary>
        Brotli = 1,

        /// <summary>No compression</summary>
        Disabled = 2
    }

    /// <summary>
    /// Managed code stripping level
    /// </summary>
    public enum ManagedStrippingLevel
    {
        /// <summary>No stripping</summary>
        Disabled = 0,

        /// <summary>Low stripping (safe)</summary>
        Low = 1,

        /// <summary>Medium stripping</summary>
        Medium = 2,

        /// <summary>High stripping (smallest build, may break reflection)</summary>
        High = 3
    }

    /// <summary>
    /// WebGL exception handling mode
    /// </summary>
    public enum WebGLExceptionSupport
    {
        /// <summary>No exception support (smallest, fastest)</summary>
        None = 0,

        /// <summary>Explicit exceptions only</summary>
        ExplicitlyThrown = 1,

        /// <summary>Full exception support (largest, slowest)</summary>
        Full = 2
    }

    #endregion

    #region Analytics & Engagement

    /// <summary>
    /// Analytics event types
    /// </summary>
    public enum AnalyticsEventType
    {
        /// <summary>Custom event</summary>
        Custom = 0,

        /// <summary>Screen/page view</summary>
        ScreenView = 1,

        /// <summary>User action</summary>
        UserAction = 2,

        /// <summary>Ad event</summary>
        AdEvent = 3,

        /// <summary>Purchase event</summary>
        Purchase = 4,

        /// <summary>Error event</summary>
        Error = 5,

        /// <summary>Session event</summary>
        Session = 6
    }

    /// <summary>
    /// User engagement state
    /// </summary>
    public enum EngagementState
    {
        /// <summary>User is active</summary>
        Active = 0,

        /// <summary>User is idle</summary>
        Idle = 1,

        /// <summary>Tab is hidden</summary>
        Hidden = 2,

        /// <summary>User has left</summary>
        Left = 3
    }

    /// <summary>
    /// Session state
    /// </summary>
    public enum SessionState
    {
        /// <summary>New session</summary>
        New = 0,

        /// <summary>Returning session</summary>
        Returning = 1,

        /// <summary>Session active</summary>
        Active = 2,

        /// <summary>Session paused</summary>
        Paused = 3,

        /// <summary>Session ended</summary>
        Ended = 4
    }

    #endregion

    #region Result Structures

    /// <summary>
    /// Result of an ad operation
    /// </summary>
    [Serializable]
    public struct AdResult
    {
        /// <summary>Whether the ad operation was successful</summary>
        public bool Success;

        /// <summary>Reward amount (for rewarded ads)</summary>
        public int RewardAmount;

        /// <summary>Type of ad that was shown</summary>
        public string AdType;

        /// <summary>Network that served the ad</summary>
        public string Network;

        /// <summary>Error message if failed</summary>
        public string ErrorMessage;

        /// <summary>Duration the ad was shown (seconds)</summary>
        public float Duration;

        /// <summary>Timestamp when ad completed</summary>
        public DateTime Timestamp;

        /// <summary>
        /// Create a successful ad result
        /// </summary>
        public static AdResult Successful(string adType, int reward = 0, string network = null)
        {
            return new AdResult
            {
                Success = true,
                AdType = adType ?? "",
                RewardAmount = reward,
                Network = network ?? "",
                ErrorMessage = null,
                Duration = 0f,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a failed ad result
        /// </summary>
        public static AdResult Failed(string adType, string error, string network = null)
        {
            return new AdResult
            {
                Success = false,
                AdType = adType ?? "",
                RewardAmount = 0,
                Network = network ?? "",
                ErrorMessage = error ?? "Unknown error",
                Duration = 0f,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Get result as string for logging
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"AdResult: SUCCESS | Type={AdType} | Reward={RewardAmount} | Network={Network}";
            }
            else
            {
                return $"AdResult: FAILED | Type={AdType} | Error={ErrorMessage}";
            }
        }
    }

    /// <summary>
    /// Result of a payment operation
    /// </summary>
    [Serializable]
    public struct PaymentResult
    {
        /// <summary>Whether the payment was successful</summary>
        public bool Success;

        /// <summary>Product identifier</summary>
        public string ProductId;

        /// <summary>Transaction identifier</summary>
        public string TransactionId;

        /// <summary>Payment provider used</summary>
        public string Provider;

        /// <summary>Amount paid (in cents)</summary>
        public int AmountCents;

        /// <summary>Currency code</summary>
        public string Currency;

        /// <summary>Error message if failed</summary>
        public string ErrorMessage;

        /// <summary>Timestamp when payment completed</summary>
        public DateTime Timestamp;

        /// <summary>
        /// Create a successful payment result
        /// </summary>
        public static PaymentResult Successful(string productId, string transactionId, string provider = null, int amountCents = 0, string currency = "USD")
        {
            return new PaymentResult
            {
                Success = true,
                ProductId = productId ?? "",
                TransactionId = transactionId ?? "",
                Provider = provider ?? "",
                AmountCents = amountCents,
                Currency = currency ?? "USD",
                ErrorMessage = null,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a failed payment result
        /// </summary>
        public static PaymentResult Failed(string productId, string error, string provider = null)
        {
            return new PaymentResult
            {
                Success = false,
                ProductId = productId ?? "",
                TransactionId = null,
                Provider = provider ?? "",
                AmountCents = 0,
                Currency = null,
                ErrorMessage = error ?? "Unknown error",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Get formatted amount string
        /// </summary>
        public string GetFormattedAmount()
        {
            float dollars = AmountCents / 100f;
            return $"{Currency} {dollars:F2}";
        }

        /// <summary>
        /// Get result as string for logging
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"PaymentResult: SUCCESS | Product={ProductId} | TxID={TransactionId} | Amount={GetFormattedAmount()}";
            }
            else
            {
                return $"PaymentResult: FAILED | Product={ProductId} | Error={ErrorMessage}";
            }
        }
    }

    /// <summary>
    /// Result of an initialization operation
    /// </summary>
    [Serializable]
    public struct InitializationResult
    {
        /// <summary>Whether initialization was successful</summary>
        public bool Success;

        /// <summary>Error message if failed</summary>
        public string ErrorMessage;

        /// <summary>Features that were initialized</summary>
        public string[] InitializedFeatures;

        /// <summary>Features that failed to initialize</summary>
        public string[] FailedFeatures;

        /// <summary>Time taken to initialize (seconds)</summary>
        public float InitTime;

        /// <summary>Timestamp when initialization completed</summary>
        public DateTime Timestamp;

        /// <summary>
        /// Create a successful initialization result
        /// </summary>
        public static InitializationResult Successful(string[] features = null, float initTime = 0f)
        {
            return new InitializationResult
            {
                Success = true,
                ErrorMessage = null,
                InitializedFeatures = features ?? new string[0],
                FailedFeatures = new string[0],
                InitTime = initTime,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a failed initialization result
        /// </summary>
        public static InitializationResult Failed(string error, string[] failedFeatures = null)
        {
            return new InitializationResult
            {
                Success = false,
                ErrorMessage = error ?? "Unknown error",
                InitializedFeatures = new string[0],
                FailedFeatures = failedFeatures ?? new string[0],
                InitTime = 0f,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Get result as string for logging
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"InitResult: SUCCESS | Features={InitializedFeatures?.Length ?? 0} | Time={InitTime:F2}s";
            }
            else
            {
                return $"InitResult: FAILED | Error={ErrorMessage} | Failed={FailedFeatures?.Length ?? 0}";
            }
        }
    }

    #endregion

    #region Event Data Structures

    /// <summary>
    /// Data for ad events
    /// </summary>
    [Serializable]
    public struct AdEventData
    {
        /// <summary>Event type (shown, clicked, completed, failed)</summary>
        public string EventType;

        /// <summary>Ad type</summary>
        public AdType AdType;

        /// <summary>Ad network</summary>
        public WebGLAdNetwork Network;

        /// <summary>Ad unit name</summary>
        public string UnitName;

        /// <summary>Reward amount (for rewarded ads)</summary>
        public int Reward;

        /// <summary>Error message (for failed events)</summary>
        public string Error;

        /// <summary>Timestamp</summary>
        public DateTime Timestamp;

        /// <summary>
        /// Create ad event data
        /// </summary>
        public static AdEventData Create(string eventType, AdType adType, WebGLAdNetwork network, string unitName = null, int reward = 0, string error = null)
        {
            return new AdEventData
            {
                EventType = eventType ?? "unknown",
                AdType = adType,
                Network = network,
                UnitName = unitName ?? "",
                Reward = reward,
                Error = error,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Data for analytics events
    /// </summary>
    [Serializable]
    public struct AnalyticsEventData
    {
        /// <summary>Event name</summary>
        public string EventName;

        /// <summary>Event type</summary>
        public AnalyticsEventType EventType;

        /// <summary>Event parameters as JSON string</summary>
        public string Parameters;

        /// <summary>User ID (if available)</summary>
        public string UserId;

        /// <summary>Session ID</summary>
        public string SessionId;

        /// <summary>Timestamp</summary>
        public DateTime Timestamp;

        /// <summary>
        /// Create analytics event data
        /// </summary>
        public static AnalyticsEventData Create(string eventName, AnalyticsEventType eventType, string parameters = null, string userId = null, string sessionId = null)
        {
            return new AnalyticsEventData
            {
                EventName = eventName ?? "unknown",
                EventType = eventType,
                Parameters = parameters ?? "{}",
                UserId = userId ?? "",
                SessionId = sessionId ?? "",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Interfaces

    /// <summary>
    /// Interface for ad providers
    /// </summary>
    public interface IAdProvider
    {
        /// <summary>Provider name</summary>
        string ProviderName { get; }

        /// <summary>Is provider initialized</summary>
        bool IsInitialized { get; }

        /// <summary>Initialize the provider</summary>
        void Initialize(string config);

        /// <summary>Show an ad</summary>
        void ShowAd(AdType adType, Action<AdResult> callback);

        /// <summary>Check if ad is ready</summary>
        bool IsAdReady(AdType adType);
    }

    /// <summary>
    /// Interface for payment providers
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>Provider name</summary>
        string ProviderName { get; }

        /// <summary>Is provider initialized</summary>
        bool IsInitialized { get; }

        /// <summary>Initialize the provider</summary>
        void Initialize(string config);

        /// <summary>Process a payment</summary>
        void ProcessPayment(string productId, int amountCents, Action<PaymentResult> callback);
    }

    /// <summary>
    /// Interface for analytics providers
    /// </summary>
    public interface IAnalyticsProvider
    {
        /// <summary>Provider name</summary>
        string ProviderName { get; }

        /// <summary>Is provider initialized</summary>
        bool IsInitialized { get; }

        /// <summary>Initialize the provider</summary>
        void Initialize(string config);

        /// <summary>Track an event</summary>
        void TrackEvent(string eventName, string parameters);
    }

    #endregion

    #region Utility Classes

    /// <summary>
    /// Static utility methods for monetization types
    /// </summary>
    public static class MonetizationTypeUtils
    {
        /// <summary>
        /// Convert AdType enum to string
        /// </summary>
        public static string AdTypeToString(AdType adType)
        {
            return adType switch
            {
                AdType.Banner => "banner",
                AdType.Interstitial => "interstitial",
                AdType.Rewarded => "rewarded",
                AdType.Native => "native",
                AdType.InFeed => "infeed",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Parse string to AdType enum
        /// </summary>
        public static AdType StringToAdType(string adType)
        {
            if (string.IsNullOrEmpty(adType)) return AdType.Banner;

            return adType.ToLowerInvariant() switch
            {
                "banner" => AdType.Banner,
                "interstitial" => AdType.Interstitial,
                "rewarded" => AdType.Rewarded,
                "native" => AdType.Native,
                "infeed" => AdType.InFeed,
                _ => AdType.Banner
            };
        }

        /// <summary>
        /// Convert WebGLAdNetwork enum to string
        /// </summary>
        public static string NetworkToString(WebGLAdNetwork network)
        {
            return network switch
            {
                WebGLAdNetwork.None => "none",
                WebGLAdNetwork.AdSense => "adsense",
                WebGLAdNetwork.Applixir => "applixir",
                WebGLAdNetwork.GameMonetize => "gamemonetize",
                WebGLAdNetwork.CrazyGames => "crazygames",
                WebGLAdNetwork.LevelPlay => "levelplay",
                WebGLAdNetwork.Poki => "poki",
                WebGLAdNetwork.GamePix => "gamepix",
                WebGLAdNetwork.FacebookInstant => "facebook",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Parse string to WebGLAdNetwork enum
        /// </summary>
        public static WebGLAdNetwork StringToNetwork(string network)
        {
            if (string.IsNullOrEmpty(network)) return WebGLAdNetwork.None;

            return network.ToLowerInvariant() switch
            {
                "adsense" => WebGLAdNetwork.AdSense,
                "applixir" => WebGLAdNetwork.Applixir,
                "gamemonetize" => WebGLAdNetwork.GameMonetize,
                "crazygames" => WebGLAdNetwork.CrazyGames,
                "levelplay" => WebGLAdNetwork.LevelPlay,
                "poki" => WebGLAdNetwork.Poki,
                "gamepix" => WebGLAdNetwork.GamePix,
                "facebook" => WebGLAdNetwork.FacebookInstant,
                _ => WebGLAdNetwork.None
            };
        }

        /// <summary>
        /// Convert PaymentProvider enum to string
        /// </summary>
        public static string ProviderToString(PaymentProvider provider)
        {
            return provider switch
            {
                PaymentProvider.None => "none",
                PaymentProvider.Stripe => "stripe",
                PaymentProvider.Paddle => "paddle",
                PaymentProvider.XUTToken => "xut",
                PaymentProvider.PayPal => "paypal",
                PaymentProvider.Platform => "platform",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Parse string to PaymentProvider enum
        /// </summary>
        public static PaymentProvider StringToProvider(string provider)
        {
            if (string.IsNullOrEmpty(provider)) return PaymentProvider.None;

            return provider.ToLowerInvariant() switch
            {
                "stripe" => PaymentProvider.Stripe,
                "paddle" => PaymentProvider.Paddle,
                "xut" or "xuttoken" => PaymentProvider.XUTToken,
                "paypal" => PaymentProvider.PayPal,
                "platform" => PaymentProvider.Platform,
                _ => PaymentProvider.None
            };
        }

        /// <summary>
        /// Convert GameOrientation enum to string
        /// </summary>
        public static string OrientationToString(GameOrientation orientation)
        {
            return orientation switch
            {
                GameOrientation.Auto => "auto",
                GameOrientation.Portrait => "portrait",
                GameOrientation.Landscape => "landscape",
                _ => "auto"
            };
        }

        /// <summary>
        /// Parse string to GameOrientation enum
        /// </summary>
        public static GameOrientation StringToOrientation(string orientation)
        {
            if (string.IsNullOrEmpty(orientation)) return GameOrientation.Auto;

            return orientation.ToLowerInvariant() switch
            {
                "portrait" => GameOrientation.Portrait,
                "landscape" => GameOrientation.Landscape,
                _ => GameOrientation.Auto
            };
        }

        /// <summary>
        /// Check if current platform is WebGL
        /// </summary>
        public static bool IsWebGLPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if running in Unity Editor
        /// </summary>
        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get current platform as string
        /// </summary>
        public static string GetPlatformString()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return "webgl";
#elif UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#elif UNITY_EDITOR
            return "editor";
#elif UNITY_STANDALONE_WIN
            return "windows";
#elif UNITY_STANDALONE_OSX
            return "macos";
#elif UNITY_STANDALONE_LINUX
            return "linux";
#else
            return "unknown";
#endif
        }
    }

    #endregion
}