// IVXAdsTestController.cs
// Comprehensive test controller for ads functionality in IntelliVerse-X SDK
// Ensures ads work properly across all platforms and edge cases

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Core;
using IntelliVerseX.Monetization;
using IntelliVerseX.Monetization.Ads;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Comprehensive ads test controller for verifying ad functionality.
    /// 
    /// Features:
    /// - Tests all ad types (Rewarded, Interstitial, Banner)
    /// - Verifies initialization and preloading
    /// - Tests failover between primary and fallback networks
    /// - Displays detailed status information
    /// - Handles all edge cases
    /// 
    /// Usage:
    /// 1. Add this script to a GameObject in your test scene
    /// 2. Assign the UI references or let it auto-create
    /// 3. Run the scene and use buttons to test ads
    /// </summary>
    public class IVXAdsTestController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Config")]
        [SerializeField] private IntelliVerseXConfig configAsset;
        [SerializeField] private bool autoFindUI = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("UI References")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text networkInfoText;
        [SerializeField] private Button showRewardedBtn;
        [SerializeField] private Button showInterstitialBtn;
        [SerializeField] private Button showBannerBtn;
        [SerializeField] private Button hideBannerBtn;
        [SerializeField] private Button preloadBtn;
        [SerializeField] private Button logStatusBtn;

        #endregion

        #region Private Fields

        private const string LOG_TAG = "[IVXAdsTest]";
        private bool _isInitialized;
        private int _rewardedWatched;
        private int _interstitialWatched;
        private int _totalReward;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (autoFindUI)
            {
                AutoFindUIReferences();
            }
        }

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToEvents();
            
            // Auto-load config if not assigned
            if (configAsset == null)
            {
                configAsset = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/GameConfig");
            }

            // Start status update coroutine
            InvokeRepeating(nameof(UpdateStatusDisplay), 0.5f, 1f);
            
            Log("Ads Test Controller initialized");
            SetStatus("Ready - Waiting for ads initialization...");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            CancelInvoke(nameof(UpdateStatusDisplay));
        }

        #endregion

        #region Setup

        private void AutoFindUIReferences()
        {
            // Find buttons by name if not assigned
            if (showRewardedBtn == null)
            {
                var go = GameObject.Find("ShowRewardedAdButton");
                if (go != null) showRewardedBtn = go.GetComponent<Button>();
            }

            if (showInterstitialBtn == null)
            {
                var go = GameObject.Find("ShowInterstitialButton");
                if (go != null) showInterstitialBtn = go.GetComponent<Button>();
            }

            if (showBannerBtn == null)
            {
                var go = GameObject.Find("ShowBannerButton");
                if (go != null) showBannerBtn = go.GetComponent<Button>();
            }

            if (hideBannerBtn == null)
            {
                var go = GameObject.Find("HideBannerButton");
                if (go != null) hideBannerBtn = go.GetComponent<Button>();
            }

            // Find utility buttons
            if (preloadBtn == null)
            {
                var go = GameObject.Find("PreloadButton");
                if (go != null) preloadBtn = go.GetComponent<Button>();
            }
            
            if (logStatusBtn == null)
            {
                var go = GameObject.Find("LogStatusButton");
                if (go != null) logStatusBtn = go.GetComponent<Button>();
            }

            // Find text displays
            if (statusText == null)
            {
                var go = GameObject.Find("Description");
                if (go != null) statusText = go.GetComponent<TMP_Text>();
            }

            if (networkInfoText == null)
            {
                var go = GameObject.Find("NetworkInfo");
                if (go != null) networkInfoText = go.GetComponent<TMP_Text>();
            }

            Log($"Auto-found UI: Rewarded={showRewardedBtn != null}, Interstitial={showInterstitialBtn != null}, ShowBanner={showBannerBtn != null}, HideBanner={hideBannerBtn != null}, Preload={preloadBtn != null}, LogStatus={logStatusBtn != null}");
        }

        private void SetupButtonListeners()
        {
            if (showRewardedBtn != null)
            {
                showRewardedBtn.onClick.RemoveAllListeners();
                showRewardedBtn.onClick.AddListener(TestShowRewardedAd);
            }

            if (showInterstitialBtn != null)
            {
                showInterstitialBtn.onClick.RemoveAllListeners();
                showInterstitialBtn.onClick.AddListener(TestShowInterstitialAd);
            }

            if (showBannerBtn != null)
            {
                showBannerBtn.onClick.RemoveAllListeners();
                showBannerBtn.onClick.AddListener(TestShowBanner);
            }

            if (hideBannerBtn != null)
            {
                hideBannerBtn.onClick.RemoveAllListeners();
                hideBannerBtn.onClick.AddListener(TestHideBanner);
            }

            if (preloadBtn != null)
            {
                preloadBtn.onClick.RemoveAllListeners();
                preloadBtn.onClick.AddListener(TestPreloadAds);
            }

            if (logStatusBtn != null)
            {
                logStatusBtn.onClick.RemoveAllListeners();
                logStatusBtn.onClick.AddListener(LogAdStatus);
            }

            Log("Button listeners configured");
        }

        private void SubscribeToEvents()
        {
            IVXAdsManager.OnAdShown += HandleAdShown;
            IVXAdsManager.OnRewardEarned += HandleRewardEarned;
            IVXAdsManager.OnAdError += HandleAdError;
        }

        private void UnsubscribeFromEvents()
        {
            IVXAdsManager.OnAdShown -= HandleAdShown;
            IVXAdsManager.OnRewardEarned -= HandleRewardEarned;
            IVXAdsManager.OnAdError -= HandleAdError;
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Test showing a rewarded ad
        /// </summary>
        public void TestShowRewardedAd()
        {
            if (!IVXAdsManager.IsInitialized())
            {
                SetStatus("Ads not initialized yet!\nPlease wait...");
                Log("Rewarded ad requested but ads not initialized", true);
                return;
            }

            SetStatus("Showing rewarded ad...");
            Log("Requesting rewarded ad");

            IVXAdsManager.ShowRewardedAd((success, reward) =>
            {
                if (success)
                {
                    _rewardedWatched++;
                    _totalReward += reward;
                    SetStatus($"Rewarded ad SUCCESS!\nReward: {reward}\nTotal earned: {_totalReward}");
                    Log($"Rewarded ad completed: reward={reward}, total={_totalReward}");
                }
                else
                {
                    SetStatus("Rewarded ad FAILED or cancelled\nTrying to preload next ad...");
                    Log("Rewarded ad failed or cancelled");
                    
                    // Auto-preload for next attempt
                    IVXAdsManager.PreloadRewardedAd();
                }
            });
        }

        /// <summary>
        /// Test showing an interstitial ad
        /// </summary>
        public void TestShowInterstitialAd()
        {
            if (!IVXAdsManager.IsInitialized())
            {
                SetStatus("Ads not initialized yet!\nPlease wait...");
                Log("Interstitial ad requested but ads not initialized", true);
                return;
            }

            SetStatus("Showing interstitial ad...");
            Log("Requesting interstitial ad");

            IVXAdsManager.ShowInterstitialAd(success =>
            {
                if (success)
                {
                    _interstitialWatched++;
                    SetStatus($"Interstitial ad SUCCESS!\nTotal shown: {_interstitialWatched}");
                    Log($"Interstitial ad completed, total={_interstitialWatched}");
                }
                else
                {
                    SetStatus("Interstitial ad FAILED\nMay be on cooldown or not ready");
                    Log("Interstitial ad failed");
                    
                    // Auto-preload for next attempt
                    IVXAdsManager.PreloadInterstitialAd();
                }
            });
        }

        /// <summary>
        /// Test showing a banner ad
        /// </summary>
        public void TestShowBanner()
        {
            if (!IVXAdsManager.IsInitialized())
            {
                SetStatus("Ads not initialized yet!\nPlease wait...");
                return;
            }

            SetStatus("Showing banner ad at bottom...");
            Log("Requesting banner ad");
            
            IVXAdsManager.ShowBannerAd("DefaultBanner", IVXBannerPosition.Bottom);
        }

        /// <summary>
        /// Test hiding banner ad
        /// </summary>
        public void TestHideBanner()
        {
            SetStatus("Hiding banner ad...");
            Log("Hiding banner ad");
            
            IVXAdsManager.HideBannerAd();
        }

        /// <summary>
        /// Manually trigger ad preloading
        /// </summary>
        public void TestPreloadAds()
        {
            if (!IVXAdsManager.IsInitialized())
            {
                SetStatus("Ads not initialized yet!\nPlease wait...");
                return;
            }

            SetStatus("Preloading all ad types...");
            Log("Manual preload requested");
            
            IVXAdsManager.PreloadAllAds();
            
            SetStatus("Preload requested!\nCheck console for details.");
        }

        /// <summary>
        /// Log detailed ad status to console
        /// </summary>
        public void LogAdStatus()
        {
            IVXAdsManager.LogAdStatus();
            SetStatus("Status logged to console.\nCheck Unity Console for details.");
        }

        #endregion

        #region Event Handlers

        private void HandleAdShown(IntelliVerseX.Core.IVXAdType adType, bool success)
        {
            Log($"Ad shown event: type={adType}, success={success}");
        }

        private void HandleRewardEarned(int reward)
        {
            Log($"Reward earned event: {reward}");
        }

        private void HandleAdError(IVXAdNetwork network, string error)
        {
            Log($"Ad error event: network={network}, error={error}", true);
            SetStatus($"Ad Error!\nNetwork: {network}\n{error}");
        }

        #endregion

        #region Status Display

        private void UpdateStatusDisplay()
        {
            bool isInitialized = IVXAdsManager.IsInitialized();
            bool adsBootstrapReady = IVXAdsBootstrap.IsAdsInitialized;
            bool rewardedReady = isInitialized && IVXAdsManager.IsRewardedAdReady();
            bool interstitialReady = isInitialized && IVXAdsManager.IsInterstitialAdReady();

            // Update network info text
            if (networkInfoText != null)
            {
                string primaryNetwork = IVXAdNetworkConfig.PRIMARY_AD_NETWORK.ToString();
                string fallbackNetwork = IVXAdNetworkConfig.FALLBACK_AD_NETWORK.ToString();
                
                networkInfoText.text = $"IVX Ads Test\n" +
                                       $"Primary: {primaryNetwork}\n" +
                                       $"Fallback: {fallbackNetwork}";
            }

            // Update button interactability based on state
            if (showRewardedBtn != null)
            {
                showRewardedBtn.interactable = isInitialized;
                
                // Update button color based on ad readiness
                var img = showRewardedBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = rewardedReady 
                        ? new Color(0.2f, 0.7f, 0.3f, 1f)  // Green when ready
                        : new Color(0.25f, 0.52f, 0.96f, 1f);  // Blue otherwise
                }
            }

            if (showInterstitialBtn != null)
            {
                showInterstitialBtn.interactable = isInitialized;
                
                var img = showInterstitialBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = interstitialReady 
                        ? new Color(0.2f, 0.7f, 0.3f, 1f)
                        : new Color(0.25f, 0.52f, 0.96f, 1f);
                }
            }

            // Auto-update status if not showing results
            if (!_isInitialized && isInitialized)
            {
                _isInitialized = true;
                SetStatus($"Ads INITIALIZED!\n" +
                         $"Rewarded: {(rewardedReady ? "Ready" : "Loading...")}\n" +
                         $"Interstitial: {(interstitialReady ? "Ready" : "Loading...")}");
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            if (verboseLogging)
            {
                Log($"Status: {message.Replace("\n", " | ")}");
            }
        }

        #endregion

        #region Logging

        private void Log(string message, bool isError = false)
        {
            if (isError)
            {
                Debug.LogError($"{LOG_TAG} {message}");
            }
            else
            {
                Debug.Log($"{LOG_TAG} {message}");
            }
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Create Test UI")]
        private void CreateTestUIInEditor()
        {
            // Creates a basic test UI if none exists
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("TestCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGo.AddComponent<GraphicRaycaster>();
            }
            
            Debug.Log("[IVXAdsTestController] Test UI created. Wire up references in inspector.");
        }

        [ContextMenu("Log Current Status")]
        private void EditorLogStatus()
        {
            if (Application.isPlaying)
            {
                LogAdStatus();
            }
            else
            {
                Debug.Log("[IVXAdsTestController] Enter play mode to view ad status.");
            }
        }
#endif

        #endregion
    }
}
