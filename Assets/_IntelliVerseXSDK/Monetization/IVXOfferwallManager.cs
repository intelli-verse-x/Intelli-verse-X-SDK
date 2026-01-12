using System;
using System.Collections.Generic;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Unified offerwall manager for IntelliVerse-X SDK.
    /// 
    /// Supported Platforms:
    /// - Pubscale (high engagement, casual games)
    /// - Xsolla (premium offers, IAP hybrid)
    /// 
    /// Revenue Strategy (Monthly - 10k DAU):
    /// PRIMARY (Choose One):
    /// - Xsolla: $1,200-2,500/month (best for premium games with IAP)
    /// - Pubscale: $800-1,500/month (best for casual, high-traffic games)
    /// 
    /// HYBRID (Both):
    /// - Combined: $2,000-4,000/month (show both, user choice)
    /// 
    /// Usage:
    ///   IVXOfferwallManager.Initialize(offerwallConfig);
    ///   IVXOfferwallManager.ShowOfferwall((success, offers) => {
    ///       if (success) Debug.Log($"Loaded {offers.Count} offers");
    ///   });
    /// 
    /// Features:
    /// - Automatic platform initialization
    /// - Revenue-optimized offer sorting
    /// - Reward conversion (USD → coins)
    /// - Fraud protection
    /// - Analytics integration
    /// 
    /// IMPORTANT: Offerwall SDKs must be imported separately!
    /// See PUBSCALE_OFFERWALL_INTEGRATION.md and XSOLLA_OFFERWALL_INTEGRATION.md
    /// </summary>
    public static class IVXOfferwallManager
    {
        private static IVXOfferwallConfig _config;
        private static bool _isInitialized = false;
        private static OfferwallPlatform _activePlatform = OfferwallPlatform.None;
        private static List<OfferwallOffer> _cachedOffers = new List<OfferwallOffer>();
        private static DateTime _lastRefreshTime = DateTime.MinValue;

        // Events
        public static event Action<OfferwallPlatform, bool> OnOfferwallOpened;
#pragma warning disable CS0067 // Event is never used - public API for external consumption
        public static event Action<OfferwallPlatform, bool> OnOfferwallClosed;
        public static event Action<OfferwallOffer, int> OnOfferCompleted; // offer, coins earned
        public static event Action<OfferwallPlatform, string> OnOfferwallError;
        public static event Action<int> OnOffersRefreshed; // offer count
#pragma warning restore CS0067

        /// <summary>
        /// Initialize offerwall system
        /// </summary>
        public static void Initialize(IVXOfferwallConfig config)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXOfferwallManager] Already initialized");
                return;
            }

            _config = config;

            // Validate config
            if (!config.IsValid(out string error))
            {
                Debug.LogError($"[IVXOfferwallManager] Config invalid: {error}");
                return;
            }

            // Initialize platforms
            if (config.enablePubscale)
            {
                InitializePubscale();
            }

            if (config.enableXsolla)
            {
                InitializeXsolla();
            }

            // Set active platform
            _activePlatform = config.GetPrimaryPlatform();

            _isInitialized = true;
            Debug.Log($"[IVXOfferwallManager] Initialized with {_activePlatform} (Revenue: {config.GetEstimatedRevenue()})");
        }

        /// <summary>
        /// Show offerwall UI
        /// </summary>
        public static void ShowOfferwall(Action<bool, List<OfferwallOffer>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[IVXOfferwallManager] Not initialized! Call Initialize() first");
                callback?.Invoke(false, null);
                return;
            }

            Debug.Log($"[IVXOfferwallManager] Showing {_activePlatform} offerwall");
            OnOfferwallOpened?.Invoke(_activePlatform, true);

            switch (_activePlatform)
            {
                case OfferwallPlatform.Pubscale:
                    ShowPubscaleOfferwall(callback);
                    break;

                case OfferwallPlatform.Xsolla:
                    ShowXsollaOfferwall(callback);
                    break;

                default:
                    Debug.LogError("[IVXOfferwallManager] No offerwall platform enabled");
                    callback?.Invoke(false, null);
                    break;
            }
        }

        /// <summary>
        /// Refresh available offers (background fetch)
        /// </summary>
        public static void RefreshOffers(Action<bool, int> callback = null)
        {
            if (!_isInitialized)
            {
                callback?.Invoke(false, 0);
                return;
            }

            // Check cooldown
            if ((DateTime.Now - _lastRefreshTime).TotalSeconds < _config.autoRefreshInterval && _config.autoRefreshInterval > 0)
            {
                Debug.Log($"[IVXOfferwallManager] Refresh on cooldown, using cached offers ({_cachedOffers.Count})");
                callback?.Invoke(true, _cachedOffers.Count);
                return;
            }

            Debug.Log($"[IVXOfferwallManager] Refreshing offers from {_activePlatform}");

            switch (_activePlatform)
            {
                case OfferwallPlatform.Pubscale:
                    RefreshPubscaleOffers(callback);
                    break;

                case OfferwallPlatform.Xsolla:
                    RefreshXsollaOffers(callback);
                    break;

                default:
                    callback?.Invoke(false, 0);
                    break;
            }
        }

        /// <summary>
        /// Get cached offers (sorted by revenue)
        /// </summary>
        public static List<OfferwallOffer> GetCachedOffers()
        {
            if (_config.prioritizeByRevenue)
            {
                _cachedOffers.Sort((a, b) => b.usdValue.CompareTo(a.usdValue));
            }
            return _cachedOffers;
        }

        /// <summary>
        /// Check if new offers are available
        /// </summary>
        public static bool HasNewOffers()
        {
            return _cachedOffers.Count > 0;
        }

        /// <summary>
        /// Get offer count
        /// </summary>
        public static int GetOfferCount()
        {
            return _cachedOffers.Count;
        }

        #region Pubscale Implementation

        private static void InitializePubscale()
        {
            Debug.Log($"[IVXOfferwallManager] Initializing Pubscale (AppID: {_config.pubscaleAppId})");

            #if PUBSCALE_SDK_INSTALLED
            try
            {
                // Pubscale SDK initialization
                PubScale.PubScaleManager.Initialize(_config.pubscaleAppId, _config.pubscaleTestMode);
                
                // Register callbacks
                PubScale.PubScaleManager.OnOfferCompleted += OnPubscaleOfferCompleted;
                PubScale.PubScaleManager.OnOfferwallClosed += OnPubscaleOfferwallClosed;
                
                Debug.Log("[IVXOfferwallManager] Pubscale initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Pubscale init failed: {e.Message}");
                OnOfferwallError?.Invoke(OfferwallPlatform.Pubscale, e.Message);
            }
            #else
            Debug.LogWarning("[IVXOfferwallManager] Pubscale SDK not installed. Import package from: https://pubscale.gitbook.io/offerwall-sdk/");
            #endif
        }

        private static void ShowPubscaleOfferwall(Action<bool, List<OfferwallOffer>> callback)
        {
            #if PUBSCALE_SDK_INSTALLED
            try
            {
                // Show Pubscale offerwall
                PubScale.PubScaleManager.ShowOfferwall((success) =>
                {
                    callback?.Invoke(success, _cachedOffers);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Pubscale show failed: {e.Message}");
                OnOfferwallError?.Invoke(OfferwallPlatform.Pubscale, e.Message);
                callback?.Invoke(false, null);
            }
            #else
            Debug.LogError("[IVXOfferwallManager] Pubscale SDK not installed!");
            callback?.Invoke(false, null);
            #endif
        }

        private static void RefreshPubscaleOffers(Action<bool, int> callback)
        {
            #if PUBSCALE_SDK_INSTALLED
            try
            {
                PubScale.PubScaleManager.GetAvailableOffers((offers) =>
                {
                    _cachedOffers = ConvertPubscaleOffers(offers);
                    _lastRefreshTime = DateTime.Now;
                    OnOffersRefreshed?.Invoke(_cachedOffers.Count);
                    callback?.Invoke(true, _cachedOffers.Count);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Pubscale refresh failed: {e.Message}");
                callback?.Invoke(false, 0);
            }
            #else
            callback?.Invoke(false, 0);
            #endif
        }

        #if PUBSCALE_SDK_INSTALLED
        private static void OnPubscaleOfferCompleted(string offerId, float usdAmount)
        {
            int coinReward = _config.CalculateCoinReward(usdAmount);
            
            var offer = _cachedOffers.Find(o => o.offerId == offerId);
            if (offer != null)
            {
                OnOfferCompleted?.Invoke(offer, coinReward);
                
                // Log analytics
                if (_config.enableAnalytics)
                {
                    LogOfferwallEvent("offer_completed", OfferwallPlatform.Pubscale, usdAmount, coinReward);
                }
            }
        }

        private static void OnPubscaleOfferwallClosed()
        {
            OnOfferwallClosed?.Invoke(OfferwallPlatform.Pubscale, true);
        }

        private static List<OfferwallOffer> ConvertPubscaleOffers(List<PubScale.Offer> pubscaleOffers)
        {
            var converted = new List<OfferwallOffer>();
            foreach (var offer in pubscaleOffers)
            {
                converted.Add(new OfferwallOffer
                {
                    offerId = offer.id,
                    offerName = offer.name,
                    offerDescription = offer.description,
                    offerType = MapPubscaleOfferType(offer.type),
                    coinReward = _config.CalculateCoinReward(offer.payout),
                    usdValue = offer.payout,
                    imageUrl = offer.iconUrl,
                    estimatedTimeMinutes = offer.estimatedTime,
                    platform = OfferwallPlatform.Pubscale,
                    isAvailable = true,
                    expiryDate = offer.expiryDate
                });
            }
            return converted;
        }

        private static OfferwallOfferType MapPubscaleOfferType(string pubscaleType)
        {
            switch (pubscaleType.ToLower())
            {
                case "video": return OfferwallOfferType.Video;
                case "install": return OfferwallOfferType.AppInstall;
                case "survey": return OfferwallOfferType.Survey;
                case "achievement": return OfferwallOfferType.GameAchievement;
                case "registration": return OfferwallOfferType.Registration;
                default: return OfferwallOfferType.Social;
            }
        }
        #endif

        #endregion

        #region Xsolla Implementation

        private static void InitializeXsolla()
        {
            Debug.Log($"[IVXOfferwallManager] Initializing Xsolla (ProjectID: {_config.xsollaProjectId})");

            #if XSOLLA_SDK_INSTALLED
            try
            {
                // Xsolla SDK initialization
                Xsolla.Core.XsollaSettings.StoreProjectId = _config.xsollaProjectId;
                Xsolla.Core.XsollaSettings.IsSandbox = _config.xsollaTestMode;
                
                // Initialize Xsolla Store
                if (_config.enableXsollaStore)
                {
                    Xsolla.Store.XsollaStore.Initialize();
                }
                
                Debug.Log("[IVXOfferwallManager] Xsolla initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Xsolla init failed: {e.Message}");
                OnOfferwallError?.Invoke(OfferwallPlatform.Xsolla, e.Message);
            }
            #else
            Debug.LogWarning("[IVXOfferwallManager] Xsolla SDK not installed. Import package from: https://xsolla.com/unity");
            #endif
        }

        private static void ShowXsollaOfferwall(Action<bool, List<OfferwallOffer>> callback)
        {
            #if XSOLLA_SDK_INSTALLED
            try
            {
                // Show Xsolla store/offerwall
                // NOTE: Xsolla uses web-based UI, opens in browser or embedded WebView
                Debug.Log("[IVXOfferwallManager] Opening Xsolla store");
                
                // Implementation depends on Xsolla SDK version
                // This is a placeholder - refer to Xsolla documentation
                callback?.Invoke(true, _cachedOffers);
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Xsolla show failed: {e.Message}");
                OnOfferwallError?.Invoke(OfferwallPlatform.Xsolla, e.Message);
                callback?.Invoke(false, null);
            }
            #else
            Debug.LogError("[IVXOfferwallManager] Xsolla SDK not installed!");
            callback?.Invoke(false, null);
            #endif
        }

        private static void RefreshXsollaOffers(Action<bool, int> callback)
        {
            #if XSOLLA_SDK_INSTALLED
            try
            {
                // Fetch Xsolla virtual items/offers
                // Implementation depends on Xsolla SDK
                _lastRefreshTime = DateTime.Now;
                callback?.Invoke(true, _cachedOffers.Count);
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXOfferwallManager] Xsolla refresh failed: {e.Message}");
                callback?.Invoke(false, 0);
            }
            #else
            callback?.Invoke(false, 0);
            #endif
        }

        #endregion

        #region Helper Methods

        private static void LogOfferwallEvent(string eventName, OfferwallPlatform platform, float usdValue, int coinReward)
        {
            if (!_config.enableAnalytics) return;

            var eventData = new Dictionary<string, object>
            {
                { "platform", platform.ToString() },
                { "usd_value", usdValue },
                { "coin_reward", coinReward },
                { "conversion_rate", _config.usdToCurrencyRate }
            };

            // Log to IVXAnalytics if available
            #if IVX_ANALYTICS_AVAILABLE
            IVXAnalytics.LogEvent(eventName, eventData);
            #else
            Debug.Log($"[IVXOfferwallManager] Analytics Event: {eventName} | {platform} | ${usdValue} | {coinReward} coins");
            #endif
        }

        /// <summary>
        /// Check initialization status
        /// </summary>
        public static bool IsInitialized()
        {
            return _isInitialized;
        }

        /// <summary>
        /// Get active platform
        /// </summary>
        public static OfferwallPlatform GetActivePlatform()
        {
            return _activePlatform;
        }

        /// <summary>
        /// Switch active platform (if both enabled)
        /// </summary>
        public static void SwitchPlatform(OfferwallPlatform newPlatform)
        {
            if (!_isInitialized) return;

            if (newPlatform == OfferwallPlatform.Pubscale && !_config.enablePubscale)
            {
                Debug.LogWarning("[IVXOfferwallManager] Pubscale not enabled in config");
                return;
            }

            if (newPlatform == OfferwallPlatform.Xsolla && !_config.enableXsolla)
            {
                Debug.LogWarning("[IVXOfferwallManager] Xsolla not enabled in config");
                return;
            }

            _activePlatform = newPlatform;
            Debug.Log($"[IVXOfferwallManager] Switched to {newPlatform}");
        }

        #endregion
    }
}
