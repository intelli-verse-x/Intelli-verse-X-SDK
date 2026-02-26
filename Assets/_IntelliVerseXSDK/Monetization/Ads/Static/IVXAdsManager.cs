using System;
using UnityEngine;
using System.Collections;

using IntelliVerseX.Core;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif
#if IRONSOURCE
using Unity.Services.LevelPlay;
#endif
#if APPODEAL
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
#endif

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Banner ad position
    /// </summary>
    public enum IVXBannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Alias for IVXBannerPosition
    /// </summary>
    public enum IVXAdPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5
    }

    /// <summary>
    /// Unified ads manager for IntelliVerse-X SDK.
    /// 
    /// Supports:
    /// - IronSource / Unity LevelPlay (with mediation)
    /// - Google AdMob
    /// - Meta Audience Network
    /// - Unity Ads
    /// - Appodeal
    /// 
    /// Features:
    /// - Automatic network initialization
    /// - GDPR/CCPA consent flow
    /// - Ad capping and cooldowns
    /// - Analytics integration
    /// - Waterfall mediation (via IVXAdsWaterfallManager)
    /// 
    /// Usage (Simple):
    ///   IVXAdsManager.Initialize(config);
    ///   IVXAdsManager.ShowRewardedAd((success, reward) => {
    ///       if (success) GiveReward(reward);
    ///   });
    /// 
    /// Usage (Waterfall):
    ///   IVXAdsWaterfallManager.Initialize(this);
    ///   IVXAdsWaterfallManager.ShowRewardedAd((success, reward) => {
    ///       if (success) GiveReward(reward);
    ///   });
    /// 
    /// SDK Integration:
    /// 1. Import ad network packages (see AD_NETWORKS_SETUP_GUIDE.md)
    /// 2. Configure app keys in IVXAdNetworkConfig
    /// 3. Initialize SDK
    /// 4. Show ads!
    /// 
    /// NOTE: This is a wrapper. Actual SDK calls happen in #if blocks
    /// to avoid compile errors when SDKs aren't installed yet.
    /// </summary>
    public static class IVXAdsManager
    {
        private static IVXAdNetwork _primaryNetwork = IVXAdNetworkConfig.PRIMARY_AD_NETWORK;
        private static bool _isInitialized = false;
        private static int _interstitialCount = 0;
        private static float _lastInterstitialTime = 0f;
        private static string _bannerReqPlacement;
        private static IVXBannerPosition _bannerReqPosition;
        private static IVXAdNetwork _bannerNetworkInUse;
        private static bool _bannerFailoverAttempted;
        private static bool _bannerWantsVisible;

        // runner for banner timeout
        private class IVXAdsRunner : MonoBehaviour { }
        private static IVXAdsRunner _runner;
        private static Coroutine _bannerTimeoutCo;

        /// <summary>
        /// Get fallback ad network for failover scenarios
        /// </summary>
        private static IVXAdNetwork GetFallback(IVXAdNetwork current)
        {
            return current switch
            {
                IVXAdNetwork.Appodeal => IVXAdNetwork.IronSource,
                IVXAdNetwork.IronSource => IVXAdNetwork.Appodeal,
                IVXAdNetwork.AdMob => IVXAdNetwork.Appodeal,
                _ => IVXAdNetwork.None
            };
        }

        /// <summary>
        /// Ensure the coroutine runner GameObject exists
        /// </summary>
        private static void EnsureRunner()
        {
            if (_runner != null) return;
            var go = new GameObject("IVXAdsManager_Runner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<IVXAdsRunner>();
        }

        /// <summary>
        /// Start a banner timeout coroutine
        /// </summary>
        private static void StartBannerTimeout(float seconds)
        {
            EnsureRunner();
            if (_bannerTimeoutCo != null) _runner.StopCoroutine(_bannerTimeoutCo);
            _bannerTimeoutCo = _runner.StartCoroutine(BannerTimeout(seconds));
        }

        /// <summary>
        /// Banner timeout coroutine for failover
        /// </summary>
        private static System.Collections.IEnumerator BannerTimeout(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);

            if (!_bannerWantsVisible) yield break;

#if IRONSOURCE
            // If IronSource is active and still not loaded => failover
            if (_bannerNetworkInUse == IVXAdNetwork.IronSource && !_levelPlayBannerLoaded)
            {
                HandleBannerFailure(IVXAdNetwork.IronSource, "Banner load timeout");
            }
#endif
#if APPODEAL
            // For Appodeal banner, we can also timeout if needed
            if (_bannerNetworkInUse == IVXAdNetwork.Appodeal && !_bannerFailoverAttempted)
            {
                // Appodeal doesn't have a reliable "loaded" state for banners,
                // so we just log and potentially failover
                Debug.Log("[IVXAdsManager] Appodeal banner timeout check - assuming displayed");
            }
#endif
        }

#if GOOGLE_MOBILE_ADS
        private static bool _adMobInitialized = false;
        private static RewardedAd _adMobRewardedAd;
        private static InterstitialAd _adMobInterstitialAd;
        private static BannerView _adMobBannerView;
        private static bool _adMobRewardedLoading = false;
        private static bool _adMobInterstitialLoading = false;
#endif
#if APPODEAL
        private static bool _appodealInitialized = false;
        private static bool _appodealRewardedInFlight = false;
        private static bool _appodealInterstitialInFlight = false;
        private static Action<bool, int> _appodealRewardedCallback;
        private static Action<bool> _appodealInterstitialCallback;
#endif

#if IRONSOURCE
        private static bool _levelPlayInitialized = false;

        private static LevelPlayRewardedAd _levelPlayRewardedAd;
        private static LevelPlayInterstitialAd _levelPlayInterstitialAd;

        private static Action<bool, int> _levelPlayRewardedCallback;
        private static Action<bool> _levelPlayInterstitialCallback;

        private static bool _levelPlayRewardEarned;
        private static int _levelPlayRewardAmount;
        private static bool _levelPlayInterstitialDisplayed;
        private static LevelPlayBannerAd _levelPlayBannerAd;
        private static bool _levelPlayBannerLoaded;
        private static bool _levelPlayBannerWantsToBeVisible;
        private static string _levelPlayBannerLastPlacement;
        private static IVXBannerPosition _levelPlayBannerLastPosition;

        private static LevelPlayBannerPosition MapBannerPos(IVXBannerPosition pos)
        {
            // LevelPlay supports 9 positions (TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight). :contentReference[oaicite:3]{index=3}
            return pos switch
            {
                IVXBannerPosition.TopLeft => LevelPlayBannerPosition.TopLeft,
                IVXBannerPosition.TopRight => LevelPlayBannerPosition.TopRight,
                IVXBannerPosition.BottomLeft => LevelPlayBannerPosition.BottomLeft,
                IVXBannerPosition.BottomRight => LevelPlayBannerPosition.BottomRight,
                IVXBannerPosition.Top => LevelPlayBannerPosition.TopCenter,
                _ => LevelPlayBannerPosition.BottomCenter
            };
        }

        private static void EnsureLevelPlayBanner(string placementName, IVXBannerPosition position)
        {
            if (!_levelPlayInitialized)
                return;

            // Recreate banner if placement/position changed
            bool needsRecreate =
                _levelPlayBannerAd == null ||
                _levelPlayBannerLastPlacement != placementName ||
                _levelPlayBannerLastPosition != position;

            if (!needsRecreate)
                return;

            // Destroy old banner object (best practice when changing config)
            try { _levelPlayBannerAd?.DestroyAd(); } catch { }
            _levelPlayBannerAd = null;
            _levelPlayBannerLoaded = false;

            string adUnitId = IVXAdNetworkConfig.GetLevelPlayBannerAdUnitId();

            var builder = new LevelPlayBannerAd.Config.Builder();
            builder.SetSize(LevelPlayAdSize.BANNER);
            builder.SetPosition(MapBannerPos(position));
            builder.SetDisplayOnLoad(false);            // We control Show/Hide manually. :contentReference[oaicite:4]{index=4}
            builder.SetRespectSafeArea(true);           // Android cutouts safe-area option. :contentReference[oaicite:5]{index=5}
            builder.SetPlacementName(placementName);    // Reporting only. :contentReference[oaicite:6]{index=6}
            var config = builder.Build();

            _levelPlayBannerAd = new LevelPlayBannerAd(adUnitId, config);

            HookLevelPlayBannerCallbacks(_levelPlayBannerAd);

            _levelPlayBannerLastPlacement = placementName;
            _levelPlayBannerLastPosition = position;

            Debug.Log($"[LevelPlay] Banner created. unit={adUnitId}, placement={placementName}, pos={position}");
            _levelPlayBannerAd.LoadAd(); // Load banner :contentReference[oaicite:7]{index=7}
        }

        private static void HookLevelPlayBannerCallbacks(LevelPlayBannerAd bannerAd)
        {
            // FULL set from Unity docs :contentReference[oaicite:8]{index=8}
            bannerAd.OnAdLoaded += (LevelPlayAdInfo adInfo) =>
            {
                _levelPlayBannerLoaded = true;
                Debug.Log("[LevelPlay] Banner Loaded");

                if (_levelPlayBannerWantsToBeVisible)
                {
                    try { bannerAd.ShowAd(); } catch { }
                }
            };

            bannerAd.OnAdLoadFailed += (LevelPlayAdError error) =>
            {
                _levelPlayBannerLoaded = false;
                Debug.LogError($"[LevelPlay] Banner LoadFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());

                HandleBannerFailure(IVXAdNetwork.IronSource, error.ToString());
            };

            bannerAd.OnAdDisplayed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Banner Displayed");
            };

            bannerAd.OnAdDisplayFailed += (LevelPlayAdInfo adInfo, LevelPlayAdError error) =>
            {
                Debug.LogError($"[LevelPlay] Banner DisplayFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());

                HandleBannerFailure(IVXAdNetwork.IronSource, error.ToString());
            };

            bannerAd.OnAdClicked += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Banner Clicked");
            };

            bannerAd.OnAdExpanded += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Banner Expanded");
            };

            bannerAd.OnAdCollapsed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Banner Collapsed");
            };

            bannerAd.OnAdLeftApplication += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Banner LeftApplication");
            };
        }
#endif


        // Events
#pragma warning disable CS0067 // Event is never used - public API for external consumption
        public static event Action<IVXAdType, bool> OnAdShown; // adType, success
        public static event Action<int> OnRewardEarned; // reward amount
        public static event Action<IVXAdNetwork, string> OnAdError; // network, error
#pragma warning restore CS0067
        public static event Action OnConsentRequired;

        /// <summary>
        /// Initialize ads system
        /// </summary>
        public static void Initialize(IntelliVerseXConfig config, IVXAdNetwork primaryNetwork = IVXAdNetwork.None)
        {
            if (_isInitialized) return;
            if (!config.enableAds) return;

            _primaryNetwork = primaryNetwork != IVXAdNetwork.None ? primaryNetwork : IVXAdNetworkConfig.PRIMARY_AD_NETWORK;

            if (IVXAdNetworkConfig.ENABLE_CONSENT_FLOW)
                CheckConsent();

            // Init primary
            InitializeNetwork(_primaryNetwork);

            // Init "the other one" (dynamic fallback)
            var fallback = GetFallback(_primaryNetwork);
            if (fallback != IVXAdNetwork.None && fallback != _primaryNetwork)
            {
                InitializeNetwork(fallback);
            }
        

        _isInitialized = true;
            Debug.Log($"[IVXAdsManager] Initialized with primary={_primaryNetwork}, fallback={fallback}");
        }

        /// <summary>
        /// Initialize specific ad network
        /// </summary>
        private static void InitializeNetwork(IVXAdNetwork network)
        {
            switch (network)
            {
                case IVXAdNetwork.IronSource:
                    InitializeIronSource();
                    break;

                case IVXAdNetwork.AdMob:
                    InitializeAdMob();
                    break;

                case IVXAdNetwork.MetaAudienceNetwork:
                    InitializeMetaAudienceNetwork();
                    break;

                case IVXAdNetwork.UnityAds:
                    InitializeUnityAds();
                    break;

                case IVXAdNetwork.Appodeal:
                    InitializeAppodeal();
                    break;

                default:
                    Debug.LogWarning($"[IVXAdsManager] Unsupported network: {network}");
                    break;
            }
        }

        #region IronSource / Unity LevelPlay (LATEST 9.x)

        private static void InitializeIronSource()
        {
#if IRONSOURCE
            try
            {
                string appKey = IVXAdNetworkConfig.GetIronSourceAppKey();
                string userId = SystemInfo.deviceUniqueIdentifier;

                // (Optional) Pause game while ads show (global)
                // LevelPlay.SetPauseGame(true); // :contentReference[oaicite:4]{index=4}

                // COPPA (still metadata-driven)
                if (IVXAdNetworkConfig.ENABLE_COPPA)
                {
                    LevelPlay.SetMetaData("is_child_directed", "true"); // SetMetaData exists :contentReference[oaicite:5]{index=5}
                }

                // Test suite / validation (LevelPlay has ValidateIntegration)
                if (IVXAdNetworkConfig.TEST_MODE)
                {
                    // In 9.x you can validate integration from code
                    // (You can also enable test suite via metadata depending on your workflow)
                    LevelPlay.ValidateIntegration(); // :contentReference[oaicite:6]{index=6}
                }

                // Ensure we don't double-subscribe
                LevelPlay.OnInitSuccess -= OnLevelPlayInitSuccess;
                LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;

                LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
                LevelPlay.OnInitFailed += OnLevelPlayInitFailed; // :contentReference[oaicite:7]{index=7}




                LevelPlay.Init(appKey, userId);

                Debug.Log($"[LevelPlay] Init requested. appKey={appKey}, userId={userId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelPlay] Initialization request failed: {e.Message}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, e.Message);
            }
#else
            Debug.LogWarning("[LevelPlay] SDK not installed. Install 'Ads Mediation / com.unity.services.levelplay' or LevelPlay Unity Plugin 9.x.");
#endif
        }

#if IRONSOURCE
        private static ILevelPlayRewardedAd _lpRewardedAd;
        private static ILevelPlayInterstitialAd _lpInterstitialAd;

        private static void OnLevelPlayInitSuccess(LevelPlayConfiguration config)
        {
            _levelPlayInitialized = true;
            Debug.Log("[LevelPlay] Init success");

            string rewardedId = IVXAdNetworkConfig.GetLevelPlayRewardedAdUnitId();
            string interId = IVXAdNetworkConfig.GetLevelPlayInterstitialAdUnitId();

            _lpRewardedAd = new LevelPlayRewardedAd(rewardedId);
            _lpInterstitialAd = new LevelPlayInterstitialAd(interId);

            // hook callbacks here...
            _lpRewardedAd.LoadAd();
            _lpInterstitialAd.LoadAd();
        }

        private static void OnLevelPlayInitFailed(LevelPlayInitError error)
        {
            _levelPlayInitialized = false;
            string msg = error != null ? error.ToString() : "Unknown init error";
            Debug.LogError($"[LevelPlay] Init FAILED: {msg}");
            OnAdError?.Invoke(IVXAdNetwork.IronSource, msg);
        }

        private static void EnsureLevelPlayAdObjects()
        {
            // You MUST provide AdUnitIds (NOT placement names)
            // Add these getters/strings in IVXAdNetworkConfig.
            string rewardedAdUnitId = IVXAdNetworkConfig.GetLevelPlayRewardedAdUnitId();
            string interstitialAdUnitId = IVXAdNetworkConfig.GetLevelPlayInterstitialAdUnitId();

            if (_levelPlayRewardedAd == null)
            {
                _levelPlayRewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
                HookLevelPlayRewardedCallbacks(_levelPlayRewardedAd);
            }

            if (_levelPlayInterstitialAd == null)
            {
                _levelPlayInterstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
                HookLevelPlayInterstitialCallbacks(_levelPlayInterstitialAd);
            }
        }

        private static void LoadLevelPlayRewarded()
        {
            try
            {
                if (_levelPlayInitialized && _levelPlayRewardedAd != null)
                {
                    _levelPlayRewardedAd.LoadAd();
                    Debug.Log("[LevelPlay] Rewarded LoadAd()");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LevelPlay] Rewarded LoadAd exception: {e.Message}");
            }
        }

        private static void LoadLevelPlayInterstitial()
        {
            try
            {
                if (_levelPlayInitialized && _levelPlayInterstitialAd != null)
                {
                    _levelPlayInterstitialAd.LoadAd();
                    Debug.Log("[LevelPlay] Interstitial LoadAd()");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LevelPlay] Interstitial LoadAd exception: {e.Message}");
            }
        }

        // -----------------------------
        // Rewarded callbacks (FULL SET)
        // -----------------------------
        private static void HookLevelPlayRewardedCallbacks(LevelPlayRewardedAd ad)
        {
            ad.OnAdLoaded += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Rewarded Loaded");
            };

            ad.OnAdLoadFailed += (LevelPlayAdError error) =>
            {
                Debug.LogError($"[LevelPlay] Rewarded LoadFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());
                // optional retry strategy
            };

            ad.OnAdDisplayed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Rewarded Displayed");
                _levelPlayRewardEarned = false;
                _levelPlayRewardAmount = 0;
            };

            // 9.0.0+ signature: (LevelPlayAdInfo, LevelPlayAdError) :contentReference[oaicite:10]{index=10}
            ad.OnAdDisplayFailed += (LevelPlayAdInfo adInfo, LevelPlayAdError error) =>
            {
                Debug.LogError($"[LevelPlay] Rewarded DisplayFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());
                OnAdShown?.Invoke(IVXAdType.Rewarded, false);

                var cb = _levelPlayRewardedCallback;
                _levelPlayRewardedCallback = null;
                cb?.Invoke(false, 0);

                // reload for next time
                LoadLevelPlayRewarded();
            };

            ad.OnAdClicked += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Rewarded Clicked");
            };

            // Rewarded: (LevelPlayAdInfo, LevelPlayReward)
            ad.OnAdRewarded += (LevelPlayAdInfo adInfo, LevelPlayReward reward) =>
            {
                int amount = 1;
                try { amount = reward != null ? (int)reward.Amount : 1; } catch { /* ignore */ }

                _levelPlayRewardEarned = true;
                _levelPlayRewardAmount = amount;

                OnRewardEarned?.Invoke(amount);
                Debug.Log($"[LevelPlay] Rewarded Rewarded: {amount}");
            };

            ad.OnAdClosed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Rewarded Closed");

                bool success = _levelPlayRewardEarned;
                int amount = success ? _levelPlayRewardAmount : 0;

                OnAdShown?.Invoke(IVXAdType.Rewarded, success);

                var cb = _levelPlayRewardedCallback;
                _levelPlayRewardedCallback = null;
                cb?.Invoke(success, amount);

                // reload for next time
                LoadLevelPlayRewarded();
            };

            ad.OnAdInfoChanged += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Rewarded InfoChanged");
            };
        }

        // ---------------------------------
        // Interstitial callbacks (FULL SET)
        // ---------------------------------
        private static void HookLevelPlayInterstitialCallbacks(LevelPlayInterstitialAd ad)
        {
            ad.OnAdLoaded += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Interstitial Loaded");
            };

            ad.OnAdLoadFailed += (LevelPlayAdError error) =>
            {
                Debug.LogError($"[LevelPlay] Interstitial LoadFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());
            };

            ad.OnAdDisplayed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Interstitial Displayed");
                _levelPlayInterstitialDisplayed = true;
                OnAdShown?.Invoke(IVXAdType.Interstitial, true);
            };

            // 9.0.0+ signature: (LevelPlayAdInfo, LevelPlayAdError) :contentReference[oaicite:11]{index=11}
            ad.OnAdDisplayFailed += (LevelPlayAdInfo adInfo, LevelPlayAdError error) =>
            {
                Debug.LogError($"[LevelPlay] Interstitial DisplayFailed: {error}");
                OnAdError?.Invoke(IVXAdNetwork.IronSource, error.ToString());
                OnAdShown?.Invoke(IVXAdType.Interstitial, false);

                var cb = _levelPlayInterstitialCallback;
                _levelPlayInterstitialCallback = null;
                cb?.Invoke(false);

                _levelPlayInterstitialDisplayed = false;

                LoadLevelPlayInterstitial();
            };

            ad.OnAdClicked += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Interstitial Clicked");
            };

            ad.OnAdClosed += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Interstitial Closed");

                bool success = _levelPlayInterstitialDisplayed;
                _levelPlayInterstitialDisplayed = false;

                if (success)
                {
                    _interstitialCount++;
                    _lastInterstitialTime = Time.time;
                }

                var cb = _levelPlayInterstitialCallback;
                _levelPlayInterstitialCallback = null;
                cb?.Invoke(success);

                LoadLevelPlayInterstitial();
            };

            ad.OnAdInfoChanged += (LevelPlayAdInfo adInfo) =>
            {
                Debug.Log("[LevelPlay] Interstitial InfoChanged");
            };
        }
#endif

        #endregion


        #region AdMob

        private static void InitializeAdMob()
        {
#if GOOGLE_MOBILE_ADS
            try
            {
                // Initialize the Google Mobile Ads SDK
                GoogleMobileAds.Api.MobileAds.Initialize((initStatus) =>
                {
                    Debug.Log($"[AdMob] Initialized: {initStatus}");
                    _adMobInitialized = true;

                    // Preload ad formats once initialization completes
                    LoadAdMobRewardedAd();
                    LoadAdMobInterstitialAd();
                });

                // Set COPPA if needed
                if (IVXAdNetworkConfig.ENABLE_COPPA)
                {
                    var requestConfiguration = new GoogleMobileAds.Api.RequestConfiguration
                    {
                        TagForChildDirectedTreatment = GoogleMobileAds.Api.TagForChildDirectedTreatment.True
                    };
                    GoogleMobileAds.Api.MobileAds.SetRequestConfiguration(requestConfiguration);
                }

                Debug.Log("[AdMob] Initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdMob] Initialization failed: {e.Message}");
            }
#else
            Debug.LogWarning("[AdMob] SDK not installed. Import from: https://github.com/googleads/googleads-mobile-unity");
#endif
        }

        #endregion

        #region Meta Audience Network


#if GOOGLE_MOBILE_ADS
        private static void LoadAdMobRewardedAd()
        {
            if (!_adMobInitialized || _adMobRewardedLoading)
            {
                return;
            }

            _adMobRewardedLoading = true;
            string adUnitId = IVXAdNetworkConfig.GetAdMobRewardedAdUnitId();
            var request = new AdRequest();

            RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
            {
                _adMobRewardedLoading = false;

                if (error != null)
                {
                    Debug.LogError($"[AdMob] Failed to load rewarded ad: {error}");
                    OnAdError?.Invoke(IVXAdNetwork.AdMob, error.ToString());
                    return;
                }

                _adMobRewardedAd = ad;
                AttachAdMobRewardedCallbacks(ad);
                Debug.Log("[AdMob] Rewarded ad loaded");
            });
        }

        private static void AttachAdMobRewardedCallbacks(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _adMobRewardedAd = null;
                LoadAdMobRewardedAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMob] Rewarded ad failed to show: {error}");
                OnAdError?.Invoke(IVXAdNetwork.AdMob, error.ToString());
                OnAdShown?.Invoke(IVXAdType.Rewarded, false);
                _adMobRewardedAd = null;
                LoadAdMobRewardedAd();
            };
        }

        private static void LoadAdMobInterstitialAd()
        {
            if (!_adMobInitialized || _adMobInterstitialLoading)
            {
                return;
            }

            _adMobInterstitialLoading = true;
            string adUnitId = IVXAdNetworkConfig.GetAdMobInterstitialAdUnitId();
            var request = new AdRequest();

            InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                _adMobInterstitialLoading = false;

                if (error != null)
                {
                    Debug.LogError($"[AdMob] Failed to load interstitial: {error}");
                    OnAdError?.Invoke(IVXAdNetwork.AdMob, error.ToString());
                    return;
                }

                _adMobInterstitialAd = ad;
                AttachAdMobInterstitialCallbacks(ad);
                Debug.Log("[AdMob] Interstitial ad loaded");
            });
        }

        private static void AttachAdMobInterstitialCallbacks(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _adMobInterstitialAd = null;
                LoadAdMobInterstitialAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMob] Interstitial failed to show: {error}");
                OnAdError?.Invoke(IVXAdNetwork.AdMob, error.ToString());
                OnAdShown?.Invoke(IVXAdType.Interstitial, false);
                _adMobInterstitialAd = null;
                LoadAdMobInterstitialAd();
            };
        }

        private static void ShowAdMobBanner(IVXBannerPosition position)
        {
            if (!_adMobInitialized)
            {
                Debug.LogWarning("[AdMob] Banner requested before initialization finished");
                return;
            }

            if (_adMobBannerView != null)
            {
                _adMobBannerView.Destroy();
                _adMobBannerView = null;
            }

            var adPosition = position switch
            {
                IVXBannerPosition.Top => AdPosition.Top,
                IVXBannerPosition.TopLeft => AdPosition.TopLeft,
                IVXBannerPosition.TopRight => AdPosition.TopRight,
                IVXBannerPosition.BottomLeft => AdPosition.BottomLeft,
                IVXBannerPosition.BottomRight => AdPosition.BottomRight,
                _ => AdPosition.Bottom
            };

            string adUnitId = IVXAdNetworkConfig.GetAdMobBannerAdUnitId();
            _adMobBannerView = new BannerView(adUnitId, AdSize.Banner, adPosition);
            _adMobBannerView.LoadAd(new AdRequest());
            Debug.Log("[AdMob] Banner requested");
        }

        private static void HideAdMobBanner()
        {
            if (_adMobBannerView != null)
            {
                _adMobBannerView.Destroy();
                _adMobBannerView = null;
            }
        }
#endif



        private static void InitializeMetaAudienceNetwork()
        {
#if AUDIENCE_NETWORK
            try
            {
                // Initialize Meta Audience Network
                AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
                
                // Test mode
                if (IVXAdNetworkConfig.TEST_MODE)
                {
                    AudienceNetwork.AdSettings.AddTestDevice("YOUR_TEST_DEVICE_HASH");
                }

                Debug.Log("[Meta] Audience Network initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Meta] Initialization failed: {e.Message}");
            }
#else
            Debug.LogWarning("[Meta] Audience Network SDK not installed. Import from: https://developers.facebook.com/docs/audience-network/unity");
#endif
        }

        #endregion

        #region Unity Ads

        private static void InitializeUnityAds()
        {
#if UNITY_ADS
            try
            {
                string gameId = IVXAdNetworkConfig.GetUnityAdsGameId();
                bool testMode = IVXAdNetworkConfig.UNITY_ADS_TEST_MODE;

                UnityEngine.Advertisements.Advertisement.Initialize(gameId, testMode);
                
                Debug.Log($"[Unity Ads] Initialized (Game ID: {gameId}, Test: {testMode})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Unity Ads] Initialization failed: {e.Message}");
            }
#else
            Debug.LogWarning("[Unity Ads] Enable in Services window: Window > General > Services > Ads");
#endif
        }

        #endregion



        #region Appodeal

        private static void InitializeAppodeal()
        {
#if APPODEAL
            try
            {
                string appKey = IVXAdNetworkConfig.GetAppodealAppKey();

                // COPPA
                if (IVXAdNetworkConfig.ENABLE_COPPA)
                {
                    Appodeal.SetChildDirectedTreatment(true);
                }

                int adTypes = IVXAdNetworkConfig.AD_TYPES;

                // Init
                Appodeal.Initialize(appKey, adTypes);
                _appodealInitialized = true;

                // Auto-cache
                if (IVXAdNetworkConfig.APPODEAL_AUTO_CACHE)
                {
                    Appodeal.SetAutoCache(adTypes, true);
                }

                // Subscribe to rewarded callbacks
                AppodealCallbacks.RewardedVideo.OnClosed += OnAppodealRewardedClosed;
                AppodealCallbacks.RewardedVideo.OnShowFailed += OnAppodealRewardedShowFailed;
                AppodealCallbacks.RewardedVideo.OnExpired += OnAppodealRewardedExpired;

                // Subscribe to interstitial callbacks
                AppodealCallbacks.Interstitial.OnClosed += OnAppodealInterstitialClosed;
                AppodealCallbacks.Interstitial.OnShowFailed += OnAppodealInterstitialShowFailed;
                AppodealCallbacks.Interstitial.OnExpired += OnAppodealInterstitialExpired;

#if UNITY_ANDROID || UNITY_IOS
                // Prime the cache once
                Appodeal.Cache(AppodealAdType.RewardedVideo);
                Appodeal.Cache(AppodealAdType.Interstitial);
#endif

                Debug.Log($"[Appodeal] Initialized with key: {appKey}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Appodeal] Initialization failed: {e.Message}");
            }
#else
    Debug.LogWarning("[Appodeal] SDK not installed. Import from: https://www.appodeal.com/sdk/unity");
#endif
        }

        #endregion
#if APPODEAL
        // ================================
        // Appodeal Rewarded callbacks
        // ================================

        private static void OnAppodealRewardedClosed(object sender, RewardedVideoClosedEventArgs e)
        {
            bool finished = e.Finished;
            int rewardAmount = finished ? 1 : 0;      // you can map this to coins later

            OnAdShown?.Invoke(IVXAdType.Rewarded, finished);
            if (finished)
                OnRewardEarned?.Invoke(rewardAmount);

            _appodealRewardedInFlight = false;

            var cb = _appodealRewardedCallback;
            _appodealRewardedCallback = null;
            cb?.Invoke(finished, rewardAmount);

            // Preload next
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        try { Appodeal.Cache(AppodealAdType.RewardedVideo); } catch { }
#endif
        }

        private static void OnAppodealRewardedShowFailed(object sender, EventArgs e)
        {
            OnAdError?.Invoke(IVXAdNetwork.Appodeal, "Rewarded show failed");
            OnAdShown?.Invoke(IVXAdType.Rewarded, false);

            _appodealRewardedInFlight = false;

            var cb = _appodealRewardedCallback;
            _appodealRewardedCallback = null;
            cb?.Invoke(false, 0);
        }

        private static void OnAppodealRewardedExpired(object sender, EventArgs e)
        {
            if (!_appodealRewardedInFlight)
                return;

            OnAdError?.Invoke(IVXAdNetwork.Appodeal, "Rewarded expired");
            OnAdShown?.Invoke(IVXAdType.Rewarded, false);

            _appodealRewardedInFlight = false;

            var cb = _appodealRewardedCallback;
            _appodealRewardedCallback = null;
            cb?.Invoke(false, 0);
        }

        // ================================
        // Appodeal Interstitial callbacks
        // ================================

        private static void OnAppodealInterstitialClosed(object sender, EventArgs e)
        {
            OnAdShown?.Invoke(IVXAdType.Interstitial, true);
            _interstitialCount++;
            _lastInterstitialTime = Time.time;

            _appodealInterstitialInFlight = false;

            var cb = _appodealInterstitialCallback;
            _appodealInterstitialCallback = null;
            cb?.Invoke(true);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        try { Appodeal.Cache(AppodealAdType.Interstitial); } catch { }
#endif
        }

        private static void OnAppodealInterstitialShowFailed(object sender, EventArgs e)
        {
            OnAdError?.Invoke(IVXAdNetwork.Appodeal, "Interstitial show failed");
            OnAdShown?.Invoke(IVXAdType.Interstitial, false);

            _appodealInterstitialInFlight = false;

            var cb = _appodealInterstitialCallback;
            _appodealInterstitialCallback = null;
            cb?.Invoke(false);
        }

        private static void OnAppodealInterstitialExpired(object sender, EventArgs e)
        {
            if (!_appodealInterstitialInFlight)
                return;

            OnAdError?.Invoke(IVXAdNetwork.Appodeal, "Interstitial expired");
            OnAdShown?.Invoke(IVXAdType.Interstitial, false);

            _appodealInterstitialInFlight = false;

            var cb = _appodealInterstitialCallback;
            _appodealInterstitialCallback = null;
            cb?.Invoke(false);
        }
#endif




        #region Consent Flow

        private static void CheckConsent()
        {
            // Check if we need to show consent dialog
            // This is simplified - use proper consent SDK in production (e.g., Google UMP SDK)

            bool needsConsent = IsInGdprRegion() || IsInCcpaRegion();

            if (needsConsent && !HasUserConsent())
            {
                OnConsentRequired?.Invoke();
                Debug.Log("[IVXAdsManager] User consent required");
            }
        }

        private static bool IsInGdprRegion()
        {
            // Check if user is in EU
            // In production, use proper geo-detection
            return false; // Placeholder
        }

        private static bool IsInCcpaRegion()
        {
            // Check if user is in California
            return false; // Placeholder
        }

        private static bool HasUserConsent()
        {
            // Check if user has given consent
            return PlayerPrefs.GetInt("ad_consent_given", 0) == 1;
        }

        public static void SetUserConsent(bool granted)
        {
            PlayerPrefs.SetInt("ad_consent_given", granted ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show rewarded ad
        /// </summary>
        public static void ShowRewardedAd(Action<bool, int> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] Not initialized");
                onComplete?.Invoke(false, 0);
                return;
            }

            // Show ad from primary network
            ShowRewardedAdFromNetwork(_primaryNetwork, (success, reward) =>
            {
                if (!success)
                {
                    // Try fallback network
                    var fallback = GetFallback(_primaryNetwork);

                    if (fallback != IVXAdNetwork.None && fallback != _primaryNetwork)
                    {
                        Debug.Log($"[IVXAdsManager] Trying fallback network: {fallback}");
                        ShowRewardedAdFromNetwork(fallback, onComplete);
                    }
                    else
                    {
                        onComplete?.Invoke(false, 0);
                    }
                }
                else
                {
                    onComplete?.Invoke(true, reward);
                }
            });
        }

        private static void ShowRewardedAdFromNetwork(IVXAdNetwork network, Action<bool, int> onComplete)
        {
            switch (network)
            {

                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    if (_adMobRewardedAd != null && _adMobRewardedAd.CanShowAd())
                    {
                        _adMobRewardedAd.Show(reward =>
                        {
                            int amount = (int)reward.Amount;
                            OnRewardEarned?.Invoke(amount);
                            OnAdShown?.Invoke(IVXAdType.Rewarded, true);
                            _adMobRewardedAd = null;
                            LoadAdMobRewardedAd();
                            onComplete?.Invoke(true, amount);
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[AdMob] Rewarded ad not ready - reloading");
                        LoadAdMobRewardedAd();
                        onComplete?.Invoke(false, 0);
                    }
#else
                    onComplete?.Invoke(false, 0);
#endif
                    break;
                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS)
                    if (!_appodealInitialized)
                    {
                        Debug.LogWarning("[IVXAdsManager] Appodeal rewarded requested before initialization.");
                        onComplete?.Invoke(false, 0);
                        break;
                    }

                    if (!Appodeal.IsLoaded(AppodealAdType.RewardedVideo))
                    {
                        Debug.LogWarning("[IVXAdsManager] Appodeal rewarded not loaded, caching now.");
                        try { Appodeal.Cache(AppodealAdType.RewardedVideo); } catch { }
                        onComplete?.Invoke(false, 0);
                        break;
                    }

                    _appodealRewardedInFlight = true;
                    _appodealRewardedCallback = onComplete ?? ((_, __) => { });

                    try
                    {
                        bool shown = Appodeal.Show(AppodealShowStyle.RewardedVideo);
                        if (!shown)
                        {
                            Debug.LogWarning("[IVXAdsManager] Appodeal.Show(RewardedVideo) returned false.");
                            _appodealRewardedInFlight = false;
                            var cb = _appodealRewardedCallback;
                            _appodealRewardedCallback = null;
                            cb?.Invoke(false, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] Appodeal rewarded show error: {ex.Message}");
                        _appodealRewardedInFlight = false;
                        var cb = _appodealRewardedCallback;
                        _appodealRewardedCallback = null;
                        cb?.Invoke(false, 0);
                    }
#else
            onComplete?.Invoke(false, 0);
#endif
                    break;
                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    if (!_levelPlayInitialized)
                    {
                        Debug.LogWarning("[LevelPlay] Rewarded requested before init success.");
                        onComplete?.Invoke(false, 0);
                        break;
                    }

                    EnsureLevelPlayAdObjects();

                    if (_levelPlayRewardedAd != null && _levelPlayRewardedAd.IsAdReady())
                    {
                        _levelPlayRewardedCallback = onComplete;
                        _levelPlayRewardEarned = false;
                        _levelPlayRewardAmount = 0;

                        // Optional placement name: _levelPlayRewardedAd.ShowAd("DefaultRewarded"); 
                        _levelPlayRewardedAd.ShowAd();
                    }
                    else
                    {
                        Debug.LogWarning("[LevelPlay] Rewarded not ready - calling LoadAd()");
                        LoadLevelPlayRewarded();
                        onComplete?.Invoke(false, 0);
                    }
#else
                    onComplete?.Invoke(false, 0);
#endif
                    break;

                // Add other networks...
                default:
                    Debug.LogWarning($"[IVXAdsManager] Network {network} not implemented");
                    onComplete?.Invoke(false, 0);
                    break;
            }
        }

        /// <summary>
        /// Check if ads manager is initialized
        /// </summary>
        public static bool IsInitialized()
        {
            return _isInitialized;
        }

        /// <summary>
        /// Show banner ad (stub - implement based on ad network)
        /// </summary>
        public static void ShowBannerAd(string placementName, IVXBannerPosition position)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] Not initialized");
                return;
            }

            _bannerReqPlacement = placementName;
            _bannerReqPosition = position;
            _bannerWantsVisible = true;
            _bannerFailoverAttempted = false;

            TryShowBannerFromNetwork(_primaryNetwork);

            // timeout safety (mostly for LevelPlay)
            StartBannerTimeout(8f);
        }
        private static void TryShowBannerFromNetwork(IVXAdNetwork network)
        {
            _bannerNetworkInUse = network;

            if (network == IVXAdNetwork.IronSource)
            {
#if IRONSOURCE
                if (!_levelPlayInitialized)
                {
                    HandleBannerFailure(IVXAdNetwork.IronSource, "Banner requested before init success");
                    return;
                }

                _levelPlayBannerWantsToBeVisible = true;
                EnsureLevelPlayBanner(_bannerReqPlacement, _bannerReqPosition);

                if (_levelPlayBannerLoaded && _levelPlayBannerAd != null)
                {
                    try { _levelPlayBannerAd.ShowAd(); } catch (Exception ex) { HandleBannerFailure(network, ex.Message); }
                }
                return;
#else
        HandleBannerFailure(network, "IRONSOURCE not defined");
        return;
#endif
            }

            if (network == IVXAdNetwork.Appodeal)
            {
#if APPODEAL
#if UNITY_ANDROID || UNITY_IOS
                try
                {
                    int showStyle = _bannerReqPosition switch
                    {
                        IVXBannerPosition.Top => AppodealShowStyle.BannerTop,
                        IVXBannerPosition.TopLeft => AppodealShowStyle.BannerTop,
                        IVXBannerPosition.TopRight => AppodealShowStyle.BannerTop,
                        _ => AppodealShowStyle.BannerBottom
                    };

                    Appodeal.Show(showStyle);
                }
                catch (Exception ex)
                {
                    HandleBannerFailure(network, ex.Message);
                }
#else
        Debug.Log("[IVXAdsManager] Appodeal banner requested in editor/non-mobile � skipping SDK call.");
#endif
#else
        HandleBannerFailure(network, "APPODEAL not defined");
#endif
                return;
            }

            Debug.LogWarning($"[IVXAdsManager] Banner not implemented for {network}");
        }

        private static void HandleBannerFailure(IVXAdNetwork failedNetwork, string reason)
        {
            Debug.LogWarning($"[IVXAdsManager] Banner failure on {failedNetwork}: {reason}");

            if (!_bannerWantsVisible) return;
            if (_bannerFailoverAttempted) return;
            if (failedNetwork != _bannerNetworkInUse) return;

            var fallback = GetFallback(failedNetwork);
            if (fallback == IVXAdNetwork.None) return;



            _bannerFailoverAttempted = true;

            HideBannerFromNetwork(failedNetwork);
            TryShowBannerFromNetwork(fallback);

            // restart timeout for fallback if needed
            StartBannerTimeout(8f);
        }

        private static void HideBannerFromNetwork(IVXAdNetwork network)
        {
            if (network == IVXAdNetwork.IronSource)
            {
#if IRONSOURCE
                _levelPlayBannerWantsToBeVisible = false;
                _levelPlayBannerLoaded = false;
                try { _levelPlayBannerAd?.HideAd(); } catch { }
#endif
            }
            else if (network == IVXAdNetwork.Appodeal)
            {
#if APPODEAL
#if UNITY_ANDROID || UNITY_IOS
                try { Appodeal.Hide(AppodealAdType.Banner); } catch { }
#endif
#endif
            }
        }

        /// <summary>
        /// Show banner ad with IVXAdPosition
        /// </summary>
        public static void ShowBannerAd(string placementName, IVXAdPosition position)
        {
            ShowBannerAd(placementName, (IVXBannerPosition)position);
        }

        /// <summary>
        /// Hide banner ad (stub - implement based on ad network)
        /// </summary>
        public static void HideBannerAd()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] Not initialized");
                return;
            }
            Debug.Log("[IVXAdsManager] HideBannerAd");
            // TODO: Implement hide banner logic

            if (_primaryNetwork == IVXAdNetwork.AdMob)
            {
#if GOOGLE_MOBILE_ADS
                HideAdMobBanner();
#endif
            }
            else if (_primaryNetwork == IVXAdNetwork.Appodeal)
            {
#if APPODEAL
#if UNITY_ANDROID || UNITY_IOS
                try
                {
                    Appodeal.Hide(AppodealAdType.Banner);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[IVXAdsManager] Appodeal.Hide(Banner) error: {ex.Message}");
                }
#else
        Debug.Log("[IVXAdsManager] Appodeal hide banner requested in editor/non-mobile � skipping SDK call.");
#endif
#endif
            }
            else if (_primaryNetwork == IVXAdNetwork.IronSource)
            {
#if IRONSOURCE
                _levelPlayBannerWantsToBeVisible = false;
                if (_levelPlayBannerAd != null)
                {
                    try { _levelPlayBannerAd.HideAd(); } catch (Exception ex) { Debug.LogWarning(ex.Message); }
                }
#endif
            }

        }

        /// <summary>
        /// Show interstitial ad
        /// </summary>
        public static void ShowInterstitialAd(Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (Time.time - _lastInterstitialTime < IVXAdNetworkConfig.INTERSTITIAL_COOLDOWN_SECONDS)
            {
                Debug.Log("[IVXAdsManager] Interstitial on cooldown");
                onComplete?.Invoke(false);
                return;
            }

            if (_interstitialCount >= IVXAdNetworkConfig.MAX_INTERSTITIALS_PER_SESSION)
            {
                Debug.Log("[IVXAdsManager] Interstitial cap reached");
                onComplete?.Invoke(false);
                return;
            }

            // TRY PRIMARY
            ShowInterstitialAdFromNetwork(_primaryNetwork, success =>
            {
                if (success)
                {
                    onComplete?.Invoke(true);
                    return;
                }

                // TRY FALLBACK
                var fallback = GetFallback(_primaryNetwork);
                if (fallback != IVXAdNetwork.None && fallback != _primaryNetwork)
                {
                    Debug.Log($"[IVXAdsManager] Interstitial primary failed. Trying fallback: {fallback}");
                    ShowInterstitialAdFromNetwork(fallback, onComplete);
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
        }


        private static void ShowInterstitialAdFromNetwork(IVXAdNetwork network, Action<bool> onComplete)
        {
            switch (network)
            {
                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    if (!_levelPlayInitialized)
                    {
                        Debug.LogWarning("[LevelPlay] Interstitial requested before init success.");
                        onComplete?.Invoke(false);
                        break;
                    }

                    EnsureLevelPlayAdObjects();

                    if (_levelPlayInterstitialAd != null && _levelPlayInterstitialAd.IsAdReady())
                    {
                        _levelPlayInterstitialCallback = onComplete;
                        _levelPlayInterstitialDisplayed = false;

                        // Optional placement name: _levelPlayInterstitialAd.ShowAd("DefaultInterstitial");
                        _levelPlayInterstitialAd.ShowAd();
                    }
                    else
                    {
                        Debug.LogWarning("[LevelPlay] Interstitial not ready - calling LoadAd()");
                        LoadLevelPlayInterstitial();
                        onComplete?.Invoke(false);
                    }
#else
                    onComplete?.Invoke(false);
#endif
                    break;


                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    if (_adMobInterstitialAd != null && _adMobInterstitialAd.CanShowAd())
                    {
                        _adMobInterstitialAd.Show();
                        _adMobInterstitialAd = null;
                        _interstitialCount++;
                        _lastInterstitialTime = Time.time;
                        OnAdShown?.Invoke(IVXAdType.Interstitial, true);
                        LoadAdMobInterstitialAd();
                        onComplete?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogWarning("[AdMob] Interstitial not ready - reloading");
                        LoadAdMobInterstitialAd();
                        onComplete?.Invoke(false);
                    }
#else
                    onComplete?.Invoke(false);
#endif
                    break;
                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS)
                    if (!_appodealInitialized)
                    {
                        Debug.LogWarning("[IVXAdsManager] Appodeal interstitial requested before initialization.");
                        onComplete?.Invoke(false);
                        break;
                    }

                    if (!Appodeal.IsLoaded(AppodealAdType.Interstitial))
                    {
                        Debug.LogWarning("[IVXAdsManager] Appodeal interstitial not loaded, caching now.");
                        try { Appodeal.Cache(AppodealAdType.Interstitial); } catch { }
                        onComplete?.Invoke(false);
                        break;
                    }

                    _appodealInterstitialInFlight = true;
                    _appodealInterstitialCallback = onComplete ?? (_ => { });

                    try
                    {
                        bool shown = Appodeal.Show(AppodealShowStyle.Interstitial);
                        if (!shown)
                        {
                            Debug.LogWarning("[IVXAdsManager] Appodeal.Show(Interstitial) returned false.");
                            _appodealInterstitialInFlight = false;
                            var cb = _appodealInterstitialCallback;
                            _appodealInterstitialCallback = null;
                            cb?.Invoke(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] Appodeal interstitial show error: {ex.Message}");
                        _appodealInterstitialInFlight = false;
                        var cb = _appodealInterstitialCallback;
                        _appodealInterstitialCallback = null;
                        cb?.Invoke(false);
                    }
#else
            onComplete?.Invoke(false);
#endif
                    break;
                // Add other networks...
                default:
                    Debug.LogWarning($"[IVXAdsManager] Network {network} not implemented");
                    onComplete?.Invoke(false);
                    break;
            }
        }

        /// <summary>
        /// Check if rewarded ad is ready
        /// </summary>
        public static bool IsRewardedAdReady()
        {
            if (!_isInitialized) return false;



#if GOOGLE_MOBILE_ADS
            if (_primaryNetwork == IVXAdNetwork.AdMob)
                return _adMobRewardedAd != null && _adMobRewardedAd.CanShowAd();
#endif

#if APPODEAL && (UNITY_ANDROID || UNITY_IOS)
            if (_primaryNetwork == IVXAdNetwork.Appodeal)
                return Appodeal.IsLoaded(AppodealAdType.RewardedVideo);
#endif
#if IRONSOURCE
            if (_primaryNetwork == IVXAdNetwork.IronSource)
                return _levelPlayRewardedAd != null && _levelPlayRewardedAd.IsAdReady();
#endif

            return false;
        }


        /// <summary>
        /// Check if interstitial ad is ready
        /// </summary>
        public static bool IsInterstitialAdReady()
        {
            if (!_isInitialized) return false;



#if GOOGLE_MOBILE_ADS
            if (_primaryNetwork == IVXAdNetwork.AdMob)
                return _adMobInterstitialAd != null && _adMobInterstitialAd.CanShowAd();
#endif

#if APPODEAL && (UNITY_ANDROID || UNITY_IOS)
            if (_primaryNetwork == IVXAdNetwork.Appodeal)
                return Appodeal.IsLoaded(AppodealAdType.Interstitial);
#endif
#if IRONSOURCE
            if (_primaryNetwork == IVXAdNetwork.IronSource)
                return _levelPlayInterstitialAd != null && _levelPlayInterstitialAd.IsAdReady();
#endif

            return false;
        }

        #endregion
        
        #region Preloading Methods
        
        /// <summary>
        /// Preload rewarded ad for faster display
        /// </summary>
        public static void PreloadRewardedAd()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] PreloadRewardedAd called before initialization.");
                return;
            }

            Debug.Log($"[IVXAdsManager] PreloadRewardedAd - Primary: {_primaryNetwork}");

            // ---------- PRIMARY NETWORK ----------
            switch (_primaryNetwork)
            {
                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    LoadAdMobRewardedAd();
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadRewardedAd (AdMob) but GOOGLE_MOBILE_ADS not defined.");
#endif
                    break;

                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    try
                    {
                        Debug.Log("[IVXAdsManager] PreloadRewardedAd -> Appodeal.Cache(RewardedVideo)");
                        Appodeal.Cache(AppodealAdType.RewardedVideo);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] PreloadRewardedAd (Appodeal) error: {ex.Message}");
                    }
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadRewardedAd (Appodeal) but APPODEAL not defined or wrong platform.");
#endif
                    break;

                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    EnsureLevelPlayAdObjects();
                    LoadLevelPlayRewarded();
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadRewardedAd (LevelPlay) but IRONSOURCE not defined.");
#endif
                    break;


                default:
                    Debug.LogWarning($"[IVXAdsManager] PreloadRewardedAd not implemented for network {_primaryNetwork}.");
                    break;
            }

            // ---------- OPTIONAL FALLBACK NETWORK ----------
            var fallback = GetFallback(_primaryNetwork);
            if (fallback != IVXAdNetwork.None && fallback != _primaryNetwork)
            {
                PreloadRewardedAdFromNetwork(fallback);
            }
        }
        
        private static void PreloadRewardedAdFromNetwork(IVXAdNetwork network)
        {
            switch (network)
            {
                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    LoadAdMobRewardedAd();
#endif
                    break;

                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    try
                    {
                        Debug.Log("[IVXAdsManager] Fallback PreloadRewardedAd -> Appodeal.Cache(RewardedVideo)");
                        Appodeal.Cache(AppodealAdType.RewardedVideo);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] Fallback PreloadRewardedAd (Appodeal) error: {ex.Message}");
                    }
#endif
                    break;

                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    Debug.Log("[IVXAdsManager] Fallback PreloadRewardedAd (IronSource) - usually auto-handled.");
                    EnsureLevelPlayAdObjects();
                    LoadLevelPlayRewarded();
#endif
                    break;
            }
        }

        /// <summary>
        /// Preload interstitial ad for faster display
        /// </summary>
        public static void PreloadInterstitialAd()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] PreloadInterstitialAd called before initialization.");
                return;
            }

            Debug.Log($"[IVXAdsManager] PreloadInterstitialAd - Primary: {_primaryNetwork}");

            // ---------- PRIMARY NETWORK ----------
            switch (_primaryNetwork)
            {
                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    LoadAdMobInterstitialAd();
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadInterstitialAd (AdMob) but GOOGLE_MOBILE_ADS not defined.");
#endif
                    break;

                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    try
                    {
                        Debug.Log("[IVXAdsManager] PreloadInterstitialAd -> Appodeal.Cache(Interstitial)");
                        Appodeal.Cache(AppodealAdType.Interstitial);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] PreloadInterstitialAd (Appodeal) error: {ex.Message}");
                    }
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadInterstitialAd (Appodeal) but APPODEAL not defined or wrong platform.");
#endif
                    break;

                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    EnsureLevelPlayAdObjects();
                    LoadLevelPlayInterstitial();
#else
                    Debug.LogWarning("[IVXAdsManager] PreloadInterstitialAd (LevelPlay) but IRONSOURCE not defined.");
#endif
                    break;

                default:
                    Debug.LogWarning($"[IVXAdsManager] PreloadInterstitialAd not implemented for network {_primaryNetwork}.");
                    break;
            }

            // ---------- OPTIONAL FALLBACK NETWORK ----------
            var fallback = GetFallback(_primaryNetwork);
            if (fallback != IVXAdNetwork.None && fallback != _primaryNetwork)
            {
                PreloadInterstitialAdFromNetwork(fallback);
            }
        }
        
        private static void PreloadInterstitialAdFromNetwork(IVXAdNetwork network)
        {
            switch (network)
            {
                case IVXAdNetwork.AdMob:
#if GOOGLE_MOBILE_ADS
                    LoadAdMobInterstitialAd();
#endif
                    break;

                case IVXAdNetwork.Appodeal:
#if APPODEAL && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    try
                    {
                        Debug.Log("[IVXAdsManager] Fallback PreloadInterstitialAd -> Appodeal.Cache(Interstitial)");
                        Appodeal.Cache(AppodealAdType.Interstitial);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXAdsManager] Fallback PreloadInterstitialAd (Appodeal) error: {ex.Message}");
                    }
#endif
                    break;

                case IVXAdNetwork.IronSource:
#if IRONSOURCE
                    Debug.Log("[IVXAdsManager] Fallback PreloadInterstitialAd (IronSource)");
                    EnsureLevelPlayAdObjects();
                    LoadLevelPlayInterstitial();
#endif
                    break;
            }
        }
        
        /// <summary>
        /// Preload all ad types (rewarded + interstitial)
        /// Call this after initialization for best ad availability
        /// </summary>
        public static void PreloadAllAds()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] PreloadAllAds called before initialization.");
                return;
            }
            
            Debug.Log("[IVXAdsManager] PreloadAllAds - Preloading rewarded and interstitial ads...");
            PreloadRewardedAd();
            PreloadInterstitialAd();
        }
        
        #endregion
        
        #region Safe Show Methods (with availability check)
        
        /// <summary>
        /// Safely show rewarded ad with automatic preload if not ready.
        /// Returns false immediately if ad not ready (doesn't block).
        /// </summary>
        public static bool TryShowRewardedAd(Action<bool, int> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] TryShowRewardedAd - Not initialized");
                onComplete?.Invoke(false, 0);
                return false;
            }
            
            if (!IsRewardedAdReady())
            {
                Debug.LogWarning("[IVXAdsManager] TryShowRewardedAd - Ad not ready, preloading...");
                PreloadRewardedAd();
                onComplete?.Invoke(false, 0);
                return false;
            }
            
            ShowRewardedAd(onComplete);
            return true;
        }
        
        /// <summary>
        /// Safely show interstitial ad with automatic preload if not ready.
        /// Returns false immediately if ad not ready (doesn't block).
        /// </summary>
        public static bool TryShowInterstitialAd(Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAdsManager] TryShowInterstitialAd - Not initialized");
                onComplete?.Invoke(false);
                return false;
            }
            
            if (!IsInterstitialAdReady())
            {
                Debug.LogWarning("[IVXAdsManager] TryShowInterstitialAd - Ad not ready, preloading...");
                PreloadInterstitialAd();
                onComplete?.Invoke(false);
                return false;
            }
            
            ShowInterstitialAd(onComplete);
            return true;
        }
        
        #endregion
        
        #region Diagnostic Methods
        
        /// <summary>
        /// Get detailed ad status for debugging
        /// </summary>
        public static string GetAdStatusReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========== AD STATUS REPORT ==========");
            sb.AppendLine($"Initialized: {_isInitialized}");
            sb.AppendLine($"Primary Network: {_primaryNetwork}");
            sb.AppendLine($"Fallback Network: {GetFallback(_primaryNetwork)}");
            sb.AppendLine($"Rewarded Ready: {IsRewardedAdReady()}");
            sb.AppendLine($"Interstitial Ready: {IsInterstitialAdReady()}");
            sb.AppendLine($"Interstitial Count: {_interstitialCount}");
            sb.AppendLine($"Last Interstitial Time: {_lastInterstitialTime}");
            
#if APPODEAL
            sb.AppendLine("--- Appodeal Status ---");
            sb.AppendLine($"  Appodeal Initialized: {_appodealInitialized}");
#if UNITY_ANDROID || UNITY_IOS
            try
            {
                sb.AppendLine($"  Rewarded Loaded: {Appodeal.IsLoaded(AppodealAdType.RewardedVideo)}");
                sb.AppendLine($"  Interstitial Loaded: {Appodeal.IsLoaded(AppodealAdType.Interstitial)}");
            }
            catch { sb.AppendLine("  (Appodeal status check failed)"); }
#endif
#endif

#if IRONSOURCE
            sb.AppendLine("--- LevelPlay Status ---");
            sb.AppendLine($"  LevelPlay Initialized: {_levelPlayInitialized}");
            sb.AppendLine($"  Rewarded Ad Object: {(_levelPlayRewardedAd != null ? "Created" : "NULL")}");
            sb.AppendLine($"  Interstitial Ad Object: {(_levelPlayInterstitialAd != null ? "Created" : "NULL")}");
            if (_levelPlayRewardedAd != null)
                sb.AppendLine($"  Rewarded IsAdReady: {_levelPlayRewardedAd.IsAdReady()}");
            if (_levelPlayInterstitialAd != null)
                sb.AppendLine($"  Interstitial IsAdReady: {_levelPlayInterstitialAd.IsAdReady()}");
#endif

#if GOOGLE_MOBILE_ADS
            sb.AppendLine("--- AdMob Status ---");
            sb.AppendLine($"  AdMob Initialized: {_adMobInitialized}");
            sb.AppendLine($"  Rewarded Ad: {(_adMobRewardedAd != null ? "Loaded" : "NULL")}");
            sb.AppendLine($"  Interstitial Ad: {(_adMobInterstitialAd != null ? "Loaded" : "NULL")}");
#endif
            
            sb.AppendLine("=======================================");
            return sb.ToString();
        }
        
        /// <summary>
        /// Log detailed ad status to console
        /// </summary>
        public static void LogAdStatus()
        {
            Debug.Log(GetAdStatusReport());
        }
        
        #endregion
    }

    /// <summary>
    /// Comprehensive offerwall manager for IntelliVerse-X SDK.
    /// 
    /// Supports:
    /// - IronSource Offerwall
    /// - Tapjoy
    /// - Fyber
    /// - AdGate Media
    /// - OfferToro
    /// - Pollfish
    /// - TheoremReach
    /// - Xsolla integration (virtual currency + payments)
    /// - Pollwall integration (survey aggregator)
    /// 
    /// Features:
    /// - Multiple provider support with fallback
    /// - Server-side reward validation
    /// - Analytics integration
    /// - Currency conversion
    /// - Notification system
    /// 
    /// Usage:
    ///   IVXOfferwallManager.Initialize();
    ///   IVXOfferwallManager.ShowOfferwall((coins) => {
    ///       Debug.Log($"Earned {coins} coins!");
    ///   });
    /// Note: This legacy class is deprecated. Use the new IVXOfferwallManager in IVXOfferwallManager.cs instead.
    /// Keeping this as a stub for backwards compatibility.
    /// </summary>
    /*
    public static class IVXOfferwallManager
    {
        private static bool _isInitialized = false;
        private static IVXOfferwallNetwork _primaryProvider = IVXOfferwallNetwork.IronSource;

        // Events
        public static event Action<int> OnOfferwallCredited; // coins earned
        public static event Action<IVXOfferwallNetwork> OnOfferwallOpened;
        public static event Action<IVXOfferwallNetwork> OnOfferwallClosed;
        public static event Action<IVXOfferwallNetwork, string> OnOfferwallError;

        /// <summary>
        /// Initialize offerwall system
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXOfferwallManager] Already initialized");
                return;
            }

            if (!IVXAdNetworkConfig.ENABLE_OFFERWALL)
            {
                Debug.Log("[IVXOfferwallManager] Offerwall disabled in config");
                return;
            }

            // Initialize each enabled provider
            _primaryProvider = IVXOfferwallNetwork.IronSource;
            InitializeProvider(_primaryProvider);

            _isInitialized = true;
            Debug.Log($"[IVXOfferwallManager] Initialized with {_primaryProvider}");
        }

        private static void 
    
    
    
    
    
    
    Provider(IVXOfferwallNetwork provider)
        {
            switch (provider)
            {
                case IVXOfferwallNetwork.IronSource:
                    InitializeIronSourceOfferwall();
                    break;

                case IVXOfferwallNetwork.Tapjoy:
                    InitializeTapjoy();
                    break;

                case IVXOfferwallNetwork.Fyber:
                    InitializeFyber();
                    break;

                case IVXOfferwallNetwork.Pollfish:
                    InitializePollfish();
                    break;

                // Others initialized on-demand
                default:
                    Debug.Log($"[IVXOfferwallManager] {provider} will be initialized on first use");
                    break;
            }
        }

        #region IronSource Offerwall

        private static void InitializeIronSourceOfferwall()
        {
            #if IRONSOURCE
            try
            {
                // IronSource offerwall uses same app key as ads
                // Just subscribe to events
                IronSourceEvents.onOfferwallOpenedEvent += OnIronSourceOfferwallOpened;
                IronSourceEvents.onOfferwallClosedEvent += OnIronSourceOfferwallClosed;
                IronSourceEvents.onOfferwallShowFailedEvent += OnIronSourceOfferwallFailed;
                IronSourceEvents.onOfferwallAdCreditedEvent += OnIronSourceOfferwallCredited;

                Debug.Log("[IronSource Offerwall] Initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IronSource Offerwall] Init failed: {e.Message}");
            }
            #else
            Debug.LogWarning("[IronSource Offerwall] SDK not installed");
            #endif
        }

        #if IRONSOURCE
        private static void OnIronSourceOfferwallOpened()
        {
            OnOfferwallOpened?.Invoke(IVXOfferwallNetwork.IronSource);
        }

        private static void OnIronSourceOfferwallClosed()
        {
            OnOfferwallClosed?.Invoke(IVXOfferwallNetwork.IronSource);
        }

        private static void OnIronSourceOfferwallFailed(IronSourceError error)
        {
            OnOfferwallError?.Invoke(IVXOfferwallNetwork.IronSource, error.getDescription());
        }

        private static void OnIronSourceOfferwallCredited(Dictionary<string, object> credits)
        {
            if (credits != null && credits.ContainsKey("credits"))
            {
                int amount = Convert.ToInt32(credits["credits"]);
                OnOfferwallCredited?.Invoke(amount);
            }
        }
        #endif

        #endregion

        #region Tapjoy

        private static void InitializeTapjoy()
        {
            #if TAPJOY
            try
            {
                string sdkKey = IVXOfferwallConfig.GetTapjoySdkKey();
                bool debugMode = IVXOfferwallConfig.TAPJOY_DEBUG_MODE;

                Tapjoy.SetDebugEnabled(debugMode);
                Tapjoy.Connect(sdkKey);

                // Subscribe to events
                Tapjoy.OnConnectSuccess += OnTapjoyConnected;
                Tapjoy.OnConnectFailure += OnTapjoyConnectFailed;

                Debug.Log($"[Tapjoy] Connecting with SDK key: {sdkKey}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Tapjoy] Init failed: {e.Message}");
            }
            #else
            Debug.LogWarning("[Tapjoy] SDK not installed. Import from: https://dev.tapjoy.com/sdk-integration/unity/");
            #endif
        }

        #if TAPJOY
        private static void OnTapjoyConnected()
        {
            Debug.Log("[Tapjoy] Connected successfully");
        }

        private static void OnTapjoyConnectFailed()
        {
            OnOfferwallError?.Invoke(IVXOfferwallNetwork.Tapjoy, "Connection failed");
        }
        #endif

        #endregion

        #region Fyber

        private static void InitializeFyber()
        {
            #if FYBER
            try
            {
                string appId = IVXOfferwallConfig.GetFyberAppId();
                string securityToken = IVXOfferwallConfig.FYBER_SECURITY_TOKEN;

                // Initialize Fyber SDK
                // FyberSDK.Init(appId, securityToken);

                Debug.Log($"[Fyber] Initialized with app ID: {appId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Fyber] Init failed: {e.Message}");
            }
            #else
            Debug.LogWarning("[Fyber] SDK not installed. Import from: https://www.fyber.com/");
            #endif
        }

        #endregion

        #region Pollfish

        private static void InitializePollfish()
        {
            #if POLLFISH
            try
            {
                string apiKey = IVXOfferwallConfig.GetPollfishApiKey();
                bool releaseMode = IVXOfferwallConfig.POLLFISH_RELEASE_MODE;
                int position = (int)IVXOfferwallConfig.POLLFISH_POSITION;

                // Initialize Pollfish
                // PollfishSDK.Init(apiKey, position, releaseMode);

                Debug.Log($"[Pollfish] Initialized (Release: {releaseMode})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Pollfish] Init failed: {e.Message}");
            }
            #else
            Debug.LogWarning("[Pollfish] SDK not installed. Import from: https://www.pollfish.com/docs/unity");
            #endif
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show offerwall
        /// </summary>
        public static void ShowOfferwall(Action<int> onEarned = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXOfferwallManager] Not initialized");
                onEarned?.Invoke(0);
                return;
            }

            ShowOfferwallFromProvider(_primaryProvider, onEarned);
        }

        /// <summary>
        /// Show offerwall from specific provider
        /// </summary>
        public static void ShowOfferwallFromProvider(IVXOfferwallNetwork provider, Action<int> onEarned = null)
        {
            switch (provider)
            {
                case IVXOfferwallNetwork.IronSource:
                    #if IRONSOURCE
                    if (IronSource.Agent.isOfferwallAvailable())
                    {
                        IronSource.Agent.showOfferwall();
                    }
                    else
                    {
                        Debug.LogWarning("[IronSource Offerwall] Not available");
                        onEarned?.Invoke(0);
                    }
                    #else
                    Debug.LogWarning("[IronSource Offerwall] SDK not installed");
                    onEarned?.Invoke(0);
                    #endif
                    break;

                case IVXOfferwallNetwork.Tapjoy:
                    #if TAPJOY
                    string placement = IVXOfferwallConfig.TAPJOY_OFFERWALL_PLACEMENT;
                    if (Tapjoy.IsContentReady(placement))
                    {
                        Tapjoy.ShowContent(placement);
                    }
                    else
                    {
                        Debug.LogWarning("[Tapjoy] Content not ready");
                        onEarned?.Invoke(0);
                    }
                    #else
                    Debug.LogWarning("[Tapjoy] SDK not installed");
                    onEarned?.Invoke(0);
                    #endif
                    break;

                // Add other providers...

                default:
                    Debug.LogWarning($"[IVXOfferwallManager] Provider {provider} not implemented");
                    onEarned?.Invoke(0);
                    break;
            }
        }

        /// <summary>
        /// Check if offerwall is available
        /// </summary>
        public static bool IsOfferwallAvailable()
        {
            if (!_isInitialized) return false;

            #if IRONSOURCE
            if (_primaryProvider == IVXOfferwallNetwork.IronSource)
            {
                return IronSource.Agent.isOfferwallAvailable();
            }
            #endif

            #if TAPJOY
            if (_primaryProvider == IVXOfferwallNetwork.Tapjoy)
            {
                return Tapjoy.IsContentReady(IVXOfferwallConfig.TAPJOY_OFFERWALL_PLACEMENT);
            }
            #endif

            return false;
        }

        /// <summary>
        /// Get available providers
        /// </summary>
        public static List<IVXOfferwallNetwork> GetAvailableProviders()
        {
            var available = new List<IVXOfferwallNetwork>();

            #if IRONSOURCE
            if (IronSource.Agent.isOfferwallAvailable())
            {
                available.Add(IVXOfferwallNetwork.IronSource);
            }
            #endif

            #if TAPJOY
            if (Tapjoy.IsConnected())
            {wrong 
                available.Add(IVXOfferwallNetwork.Tapjoy);
            }
            #endif

            return available;
        }

        #endregion
    }
    */
}
