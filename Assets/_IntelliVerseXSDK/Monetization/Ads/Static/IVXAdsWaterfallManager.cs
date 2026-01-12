using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Waterfall mediation manager for IntelliVerse-X SDK.
    /// 
    /// Handles:
    /// - Multi-network waterfall (try networks in priority order)
    /// - Automatic fallback when ad fails to load
    /// - eCPM-based optimization
    /// - Network timeout handling
    /// - Fill rate tracking
    /// - Revenue analytics
    /// 
    /// Waterfall Flow:
    /// 1. Try highest priority network
    /// 2. If timeout or no fill, try next network
    /// 3. Continue until ad shown or all networks exhausted
    /// 4. Track performance and adjust priorities
    /// 
    /// Usage:
    ///   IVXAdsWaterfallManager.ShowRewardedAd((success, reward) => {
    ///       if (success) GiveReward(reward);
    ///   });
    /// </summary>
    public static class IVXAdsWaterfallManager
    {
        private static bool _isInitialized = false;
        private static Dictionary<IVXAdNetwork, IVXNetworkStats> _networkStats = new Dictionary<IVXAdNetwork, IVXNetworkStats>();
        private static List<IVXAdNetworkPriority> _currentWaterfall;
        private static MonoBehaviour _coroutineRunner;

        // Events
        public static event Action<IVXAdNetwork, IVXAdType, bool> OnAdAttempt; // network, adType, success
        public static event Action<IVXAdNetwork, IVXAdType> OnAdShown;
        public static event Action<IVXAdNetwork, float> OnAdRevenue; // network, revenue (USD)
        public static event Action<string> OnWaterfallFailed; // error message

        /// <summary>
        /// Initialize waterfall manager
        /// </summary>
        public static void Initialize(MonoBehaviour coroutineRunner)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXAdsWaterfallManager] Already initialized");
                return;
            }

            _coroutineRunner = coroutineRunner;
            _currentWaterfall = IVXAdNetworkConfig.WATERFALL_PRIORITY.OrderBy(x => x.Priority).ToList();

            // Initialize stats for each network
            foreach (var network in _currentWaterfall)
            {
                _networkStats[network.Network] = new IVXNetworkStats
                {
                    Network = network.Network,
                    Attempts = 0,
                    Successes = 0,
                    Revenue = 0f,
                    AverageEcpm = 0f
                };
            }

            _isInitialized = true;
            Debug.Log("[IVXAdsWaterfallManager] Initialized with waterfall mediation");
        }

        /// <summary>
        /// Show rewarded ad using waterfall
        /// </summary>
        public static void ShowRewardedAd(Action<bool, int> onComplete)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[IVXAdsWaterfallManager] Not initialized!");
                onComplete?.Invoke(false, 0);
                return;
            }

            _coroutineRunner.StartCoroutine(ShowAdWithWaterfall(IVXAdType.Rewarded, (success) =>
            {
                if (success)
                {
                    int reward = 100; // Get from ad metadata or config
                    onComplete?.Invoke(true, reward);
                }
                else
                {
                    onComplete?.Invoke(false, 0);
                }
            }));
        }

        /// <summary>
        /// Show interstitial ad using waterfall
        /// </summary>
        public static void ShowInterstitialAd(Action<bool> onComplete)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[IVXAdsWaterfallManager] Not initialized!");
                onComplete?.Invoke(false);
                return;
            }

            _coroutineRunner.StartCoroutine(ShowAdWithWaterfall(IVXAdType.Interstitial, onComplete));
        }

        /// <summary>
        /// Core waterfall logic
        /// </summary>
        private static IEnumerator ShowAdWithWaterfall(IVXAdType adType, Action<bool> onComplete)
        {
            bool adShown = false;
            string lastError = "";

            // Get current waterfall (may be optimized)
            var waterfall = IVXAdNetworkConfig.ENABLE_AUTO_OPTIMIZATION ? GetOptimizedWaterfall() : _currentWaterfall;

            foreach (var networkPriority in waterfall)
            {
                var network = networkPriority.Network;
                
                if (IVXAdNetworkConfig.ENABLE_AD_LOGGING)
                {
                    Debug.Log($"[Waterfall] Trying {network} for {adType}...");
                }

                // Track attempt
                _networkStats[network].Attempts++;
                OnAdAttempt?.Invoke(network, adType, false);

                // Try to load and show ad
                bool success = false;
                bool timedOut = false;
                float startTime = Time.time;

                // Simulate ad load (replace with actual SDK calls)
                yield return TryShowAdFromNetwork(network, adType, (result) =>
                {
                    success = result;
                });

                // Check timeout
                if (Time.time - startTime > networkPriority.Timeout)
                {
                    timedOut = true;
                    if (IVXAdNetworkConfig.ENABLE_AD_LOGGING)
                    {
                        Debug.LogWarning($"[Waterfall] {network} timed out after {networkPriority.Timeout}s");
                    }
                }

                if (success && !timedOut)
                {
                    // Ad shown successfully!
                    adShown = true;
                    _networkStats[network].Successes++;
                    
                    OnAdAttempt?.Invoke(network, adType, true);
                    OnAdShown?.Invoke(network, adType);

                    // Track revenue (would come from ad metadata in real implementation)
                    float revenue = EstimateRevenue(network, adType);
                    _networkStats[network].Revenue += revenue;
                    OnAdRevenue?.Invoke(network, revenue);

                    if (IVXAdNetworkConfig.ENABLE_AD_LOGGING)
                    {
                        Debug.Log($"[Waterfall] ✓ Ad shown from {network} (${revenue:F4})");
                    }

                    break;
                }
                else
                {
                    lastError = timedOut ? $"{network} timeout" : $"{network} no fill";
                    
                    if (IVXAdNetworkConfig.ENABLE_AD_LOGGING)
                    {
                        Debug.LogWarning($"[Waterfall] ✗ {network} failed: {lastError}");
                    }

                    // Continue to next network
                }
            }

            if (!adShown)
            {
                // All networks failed
                OnWaterfallFailed?.Invoke($"Waterfall exhausted. Last: {lastError}");
                Debug.LogError($"[Waterfall] All networks failed! Last error: {lastError}");
            }

            // Update eCPM calculations
            UpdateEcpmStats();

            onComplete?.Invoke(adShown);
        }

        /// <summary>
        /// Try to show ad from specific network
        /// Replace this with actual SDK integration
        /// </summary>
        private static IEnumerator TryShowAdFromNetwork(IVXAdNetwork network, IVXAdType adType, Action<bool> onComplete)
        {
            // This is where you'd integrate actual ad SDK calls
            // For now, simulate with delays and random success
            
            yield return new WaitForSeconds(0.5f); // Simulate load time

            // Simulate success rate (replace with actual SDK calls)
            bool success = false;

            switch (network)
            {
                case IVXAdNetwork.IronSource:
                    // IronSource.Agent.showRewardedVideo();
                    // Wait for callback...
                    success = UnityEngine.Random.value > 0.1f; // 90% success rate
                    break;

                case IVXAdNetwork.AdMob:
                    // Google Mobile Ads SDK
                    success = UnityEngine.Random.value > 0.2f; // 80% success rate
                    break;

                case IVXAdNetwork.MetaAudienceNetwork:
                    // Meta Audience Network SDK
                    success = UnityEngine.Random.value > 0.3f; // 70% success rate
                    break;

                case IVXAdNetwork.UnityAds:
                    // Unity Ads SDK
                    success = UnityEngine.Random.value > 0.2f; // 80% success rate
                    break;

                case IVXAdNetwork.Appodeal:
                    // Appodeal SDK
                    success = UnityEngine.Random.value > 0.15f; // 85% success rate
                    break;

                default:
                    success = false;
                    break;
            }

            onComplete?.Invoke(success);
        }

        /// <summary>
        /// Get optimized waterfall based on performance
        /// </summary>
        private static List<IVXAdNetworkPriority> GetOptimizedWaterfall()
        {
            // Sort by eCPM (highest first)
            return _currentWaterfall
                .OrderByDescending(x => _networkStats[x.Network].AverageEcpm)
                .ThenBy(x => x.Priority)
                .ToList();
        }

        /// <summary>
        /// Update eCPM statistics
        /// </summary>
        private static void UpdateEcpmStats()
        {
            foreach (var kvp in _networkStats)
            {
                var stats = kvp.Value;
                if (stats.Successes > 0)
                {
                    // eCPM = (Revenue / Impressions) * 1000
                    stats.AverageEcpm = (stats.Revenue / stats.Successes) * 1000f;
                }
            }
        }

        /// <summary>
        /// Estimate revenue for ad type and network
        /// In production, this comes from ad metadata
        /// </summary>
        private static float EstimateRevenue(IVXAdNetwork network, IVXAdType adType)
        {
            // Typical eCPM ranges (USD per 1000 impressions)
            // These are estimates - actual values vary by geo, time, etc.
            
            float baseEcpm = adType switch
            {
                IVXAdType.Rewarded => 10.0f,      // $10 eCPM (high value)
                IVXAdType.Interstitial => 5.0f,   // $5 eCPM
                IVXAdType.Banner => 0.5f,         // $0.50 eCPM
                _ => 1.0f
            };

            // Network multipliers (based on typical performance)
            float networkMultiplier = network switch
            {
                IVXAdNetwork.IronSource => 1.2f,          // Best mediation
                IVXAdNetwork.Appodeal => 1.15f,           // Good auto-optimization
                IVXAdNetwork.AdMob => 1.0f,               // Baseline
                IVXAdNetwork.MetaAudienceNetwork => 1.1f, // Good for premium users
                IVXAdNetwork.UnityAds => 0.9f,            // Lower but good fill
                _ => 0.8f
            };

            // Revenue per impression = eCPM / 1000
            return (baseEcpm * networkMultiplier) / 1000f;
        }

        /// <summary>
        /// Get network statistics
        /// </summary>
        public static IVXNetworkStats GetNetworkStats(IVXAdNetwork network)
        {
            return _networkStats.ContainsKey(network) ? _networkStats[network] : null;
        }

        /// <summary>
        /// Get all network statistics
        /// </summary>
        public static Dictionary<IVXAdNetwork, IVXNetworkStats> GetAllStats()
        {
            return new Dictionary<IVXAdNetwork, IVXNetworkStats>(_networkStats);
        }

        /// <summary>
        /// Reset statistics (for testing)
        /// </summary>
        public static void ResetStats()
        {
            foreach (var stats in _networkStats.Values)
            {
                stats.Attempts = 0;
                stats.Successes = 0;
                stats.Revenue = 0f;
                stats.AverageEcpm = 0f;
            }
        }

        /// <summary>
        /// Manually adjust waterfall priorities
        /// </summary>
        public static void SetCustomWaterfall(List<IVXAdNetworkPriority> customWaterfall)
        {
            _currentWaterfall = customWaterfall.OrderBy(x => x.Priority).ToList();
            Debug.Log("[IVXAdsWaterfallManager] Custom waterfall set");
        }
    }

    /// <summary>
    /// Ad type enum
    /// </summary>
    public enum IVXAdType
    {
        Rewarded,
        Interstitial,
        Banner
    }

    /// <summary>
    /// Network performance statistics
    /// </summary>
    public class IVXNetworkStats
    {
        public IVXAdNetwork Network;
        public int Attempts;
        public int Successes;
        public float Revenue;       // Total revenue (USD)
        public float AverageEcpm;   // Average eCPM

        public float FillRate => Attempts > 0 ? (float)Successes / Attempts : 0f;
        public float SuccessRate => FillRate * 100f;
    }
}
