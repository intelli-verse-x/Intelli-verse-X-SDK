using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Monetization;

using System;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Unity.Services.Core;

namespace IntelliVerseX.Monetization.Ads
{
    /// <summary>
    /// Production-ready ads bootstrap with robust initialization.
    /// Uses reflection for optional dependencies (consent, authentication, remote config, analytics).
    /// </summary>
    public class IVXAdsBootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private IntelliVerseXConfig configAsset;

        [Header("Inspector emergency override (leave None for remote control)")]
        [SerializeField] private IVXAdNetwork overridePrimary = IVXAdNetwork.None;

        [Header("Remote Config Keys")]
        [SerializeField] private string keyAdsControlJson = "ads_control_json";
        [SerializeField] private string keyAbPrimary = "ads_primary_network";
        [SerializeField] private float remoteConfigTimeoutSeconds = 10f;

        [Header("Failsafe Settings")]
        [Tooltip("Maximum time to wait for consent before forcing ads init (seconds)")]
        [SerializeField] private float maxConsentWaitSeconds = 8f;

        [Tooltip("Force ads init even without consent (for non-GDPR regions)")]
        [SerializeField] private bool forceInitWithoutConsent = true;

        [Tooltip("Preload interval for rewarded ads (seconds)")]
        [SerializeField] private float preloadIntervalSeconds = 30f;

        private bool _initialized;
        private bool _rcReady;
        private bool _consentReady;
        private bool _canRequestAds;
        private bool _forceInitTriggered;
        private bool _ugsReady;

        private Delegate _consentCallback;
        private object _consentInstance;

        public struct userAttributes { }
        public struct appAttributes { }

        [Serializable]
        private class AdsControlConfig
        {
            public string mode;
            public string forcePrimary;
        }

        private static IVXAdsBootstrap _instance;

        public static IVXAdsBootstrap Instance => _instance;
        public static bool IsAdsInitialized => _instance != null && _instance._initialized;
        public IntelliVerseXConfig ConfigAsset => configAsset;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SubscribeToConsentEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromConsentEvents();
        }

        private void Start()
        {
            Debug.Log("[IVXAdsBootstrap] Starting ads initialization...");

            if (configAsset != null)
            {
                IVXAdNetworkConfig.LoadConfig(configAsset);
            }

            StartCoroutine(FailsafeInitCoroutine());
            BeginRemoteConfigFetch();
            TryInitializeWithConsent();
        }

        private void SubscribeToConsentEvents()
        {
            var consentType = FindType("GoogleUMPConsent");
            if (consentType == null) return;

            var instanceProp = consentType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProp == null) return;

            _consentInstance = instanceProp.GetValue(null);
            if (_consentInstance == null) return;

            var eventInfo = consentType.GetEvent("OnConsentResult");
            if (eventInfo != null)
            {
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, nameof(OnConsentResultReflection));
                eventInfo.AddEventHandler(null, handler);
                _consentCallback = handler;
            }
        }

        private void UnsubscribeFromConsentEvents()
        {
            if (_consentCallback == null) return;

            var consentType = FindType("GoogleUMPConsent");
            if (consentType == null) return;

            var eventInfo = consentType.GetEvent("OnConsentResult");
            if (eventInfo != null)
            {
                eventInfo.RemoveEventHandler(null, _consentCallback);
            }
            _consentCallback = null;
        }

        public void OnConsentResultReflection(bool canRequestAds)
        {
            OnConsentResult(canRequestAds);
        }

        private void TryInitializeWithConsent()
        {
            var consentType = FindType("GoogleUMPConsent");
            if (consentType == null)
            {
                _consentReady = true;
                _canRequestAds = true;
                TryInitialize();
                return;
            }

            var instanceProp = consentType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var ump = instanceProp?.GetValue(null);

            if (ump == null)
            {
                _consentReady = true;
                _canRequestAds = true;
                TryInitialize();
                return;
            }

            var isConsentObtainedProp = consentType.GetProperty("IsConsentObtained");
            bool isConsentObtained = isConsentObtainedProp != null && (bool)isConsentObtainedProp.GetValue(ump);

            if (isConsentObtained)
            {
                var canRequestAdsProp = consentType.GetProperty("CanRequestAds");
                bool canRequest = canRequestAdsProp != null && (bool)canRequestAdsProp.GetValue(ump);
                OnConsentResult(canRequest);
            }
            else
            {
                var isRequestInProgressProp = consentType.GetProperty("IsRequestInProgress");
                bool isRequestInProgress = isRequestInProgressProp != null && (bool)isRequestInProgressProp.GetValue(ump);

                if (!isRequestInProgress && !isConsentObtained)
                {
                    var checkMethod = consentType.GetMethod("CheckAndRequestConsent");
                    checkMethod?.Invoke(ump, null);
                }
            }
        }

        private IEnumerator FailsafeInitCoroutine()
        {
            yield return new WaitForSecondsRealtime(maxConsentWaitSeconds);

            if (!_initialized && !_forceInitTriggered)
            {
                Debug.LogWarning("[IVXAdsBootstrap] Failsafe triggered.");
                _forceInitTriggered = true;

                if (!_consentReady && forceInitWithoutConsent)
                {
                    _consentReady = true;
                    _canRequestAds = true;
                }

                if (!_rcReady)
                {
                    _rcReady = true;
                }

                TryInitialize();
            }
        }

        private void OnConsentResult(bool canRequestAds)
        {
            if (_initialized) return;
            UnsubscribeFromConsentEvents();
            _consentReady = true;
            _canRequestAds = canRequestAds;
            TryInitialize();
        }

        private void BeginRemoteConfigFetch()
        {
            if (overridePrimary != IVXAdNetwork.None)
            {
                _rcReady = true;
                TryInitialize();
                return;
            }

            Invoke(nameof(RemoteConfigTimeoutFallback), remoteConfigTimeoutSeconds);
            FetchRemoteConfigAsync();
        }

        private void RemoteConfigTimeoutFallback()
        {
            if (_rcReady) return;
            _rcReady = true;
            TryInitialize();
        }

        private async void FetchRemoteConfigAsync()
        {
            string chosen = "Base";

            try
            {
                await EnsureUGSAsync();

                // Use reflection for Remote Config
                chosen = await TryFetchRemoteConfigAsync();

                overridePrimary = MapPrimary(chosen);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IVXAdsBootstrap] RC fetch failed: " + e.Message);
            }
            finally
            {
                CancelInvoke(nameof(RemoteConfigTimeoutFallback));
                _rcReady = true;
                TryInitialize();
            }
        }

        private async System.Threading.Tasks.Task<string> TryFetchRemoteConfigAsync()
        {
            string chosen = "Base";

            try
            {
                // Find RemoteConfigService via reflection
                var rcServiceType = FindType("Unity.Services.RemoteConfig.RemoteConfigService");
                if (rcServiceType == null)
                {
                    Debug.Log("[IVXAdsBootstrap] RemoteConfig not available.");
                    return chosen;
                }

                var instanceProp = rcServiceType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null) return chosen;

                var rcInstance = instanceProp.GetValue(null);
                if (rcInstance == null) return chosen;

                // SetCustomUserID
                var setUserIdMethod = rcServiceType.GetMethod("SetCustomUserID");
                if (setUserIdMethod != null)
                {
                    setUserIdMethod.Invoke(rcInstance, new object[] { Sha256(SystemInfo.deviceUniqueIdentifier) });
                }

                // FetchConfigsAsync - needs struct instances
                var fetchMethod = rcServiceType.GetMethod("FetchConfigsAsync");
                if (fetchMethod != null)
                {
                    var task = fetchMethod.Invoke(rcInstance, new object[] { new userAttributes(), new appAttributes() }) as System.Threading.Tasks.Task;
                    if (task != null) await task;
                }

                // Get appConfig
                var appConfigProp = rcServiceType.GetProperty("appConfig");
                if (appConfigProp == null) return chosen;

                var appConfig = appConfigProp.GetValue(rcInstance);
                if (appConfig == null) return chosen;

                // GetString
                var getStringMethod = appConfig.GetType().GetMethod("GetString", new[] { typeof(string), typeof(string) });
                if (getStringMethod != null)
                {
                    string json = (string)getStringMethod.Invoke(appConfig, new object[] { keyAdsControlJson, "{}" });

                    AdsControlConfig ctrl = null;
                    try { ctrl = JsonUtility.FromJson<AdsControlConfig>(json); } catch { }

                    string mode = (ctrl != null && !string.IsNullOrEmpty(ctrl.mode)) ? ctrl.mode.Trim() : "AB";

                    if (string.Equals(mode, "FORCE", StringComparison.OrdinalIgnoreCase))
                    {
                        chosen = (ctrl != null && !string.IsNullOrEmpty(ctrl.forcePrimary)) ? ctrl.forcePrimary : "Iron Source";
                    }
                    else
                    {
                        chosen = (string)getStringMethod.Invoke(appConfig, new object[] { keyAbPrimary, "Base" });
                    }
                }

                Debug.Log("[IVXAdsBootstrap] RC chosen: " + chosen);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IVXAdsBootstrap] RC reflection failed: " + e.Message);
            }

            return chosen;
        }

        private IVXAdNetwork MapPrimary(string chosen)
        {
            if (chosen == "Appodeal") return IVXAdNetwork.Appodeal;
            if (chosen == "Iron Source" || chosen == "IronSource") return IVXAdNetwork.IronSource;
            return IVXAdNetwork.None;
        }

        private async System.Threading.Tasks.Task EnsureUGSAsync()
        {
            if (_ugsReady) return;

            try
            {
                var options = new InitializationOptions()
                    .SetOption("com.unity.services.core.environment-name", "production");
                await UnityServices.InitializeAsync(options);
                await TrySignInAnonymouslyAsync();
                _ugsReady = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IVXAdsBootstrap] UGS init failed: " + e.Message);
                _ugsReady = true;
            }
        }

        private async System.Threading.Tasks.Task TrySignInAnonymouslyAsync()
        {
            try
            {
                var authServiceType = FindType("Unity.Services.Authentication.AuthenticationService");
                if (authServiceType == null) return;

                var instanceProp = authServiceType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null) return;

                var instance = instanceProp.GetValue(null);
                if (instance == null) return;

                var isSignedInProp = authServiceType.GetProperty("IsSignedIn");
                if (isSignedInProp != null && (bool)isSignedInProp.GetValue(instance)) return;

                var signInMethod = authServiceType.GetMethod("SignInAnonymouslyAsync");
                if (signInMethod != null)
                {
                    var task = signInMethod.Invoke(instance, null) as System.Threading.Tasks.Task;
                    if (task != null) await task;
                }
            }
            catch { }
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private void TryInitialize()
        {
            if (_initialized || !_rcReady || !_consentReady) return;
            InitializeAds(CanTrack());
        }

        private bool CanTrack()
        {
#if UNITY_IOS
            var attType = FindType("ATTRequestManager");
            if (attType != null)
            {
                var instanceProp = attType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = instanceProp?.GetValue(null);
                if (instance == null) return false;

                var isAuthorizedProp = attType.GetProperty("IsTrackingAuthorized");
                bool isAuthorized = isAuthorizedProp != null && (bool)isAuthorizedProp.GetValue(instance);
                return _canRequestAds && isAuthorized;
            }
#endif
            return _canRequestAds;
        }

        private void InitializeAds(bool canTrack)
        {
            if (_initialized) return;
            _initialized = true;

            Debug.Log("[IVXAdsBootstrap] Initializing ads...");

            if (configAsset == null)
            {
                Debug.LogError("[IVXAdsBootstrap] Config asset is NULL!");
                return;
            }

            if (!configAsset.enableAds)
            {
                Debug.LogWarning("[IVXAdsBootstrap] Ads disabled in config.");
                return;
            }

            try
            {
                IVXAdsManager.SetUserConsent(canTrack);

                var primary = overridePrimary != IVXAdNetwork.None
                    ? overridePrimary
                    : IVXAdNetworkConfig.PRIMARY_AD_NETWORK;

                IVXAdsManager.Initialize(configAsset, primary);
                StartCoroutine(PreloadCoroutine());
                Debug.Log("[IVXAdsBootstrap] Ads initialized!");
            }
            catch (Exception e)
            {
                Debug.LogError("[IVXAdsBootstrap] Init failed: " + e.Message);
            }
        }

        private IEnumerator PreloadCoroutine()
        {
            yield return new WaitForSecondsRealtime(2f);

            for (int i = 0; i < 3; i++)
            {
                if (IVXAdsManager.IsInitialized())
                {
                    if (!IVXAdsManager.IsRewardedAdReady()) IVXAdsManager.PreloadRewardedAd();
                    if (!IVXAdsManager.IsInterstitialAdReady()) IVXAdsManager.PreloadInterstitialAd();
                }
                yield return new WaitForSecondsRealtime(3f);
            }

            StartCoroutine(PreloadLoop());
        }

        private IEnumerator PreloadLoop()
        {
            var wait = new WaitForSecondsRealtime(preloadIntervalSeconds);
            while (true)
            {
                if (IVXAdsManager.IsInitialized())
                {
                    if (!IVXAdsManager.IsRewardedAdReady()) IVXAdsManager.PreloadRewardedAd();
                    if (!IVXAdsManager.IsInterstitialAdReady()) IVXAdsManager.PreloadInterstitialAd();
                }
                yield return wait;
            }
        }

        public static void ForceReinitialize()
        {
            if (_instance == null) return;
            _instance._initialized = false;
            _instance._forceInitTriggered = true;
            _instance._consentReady = true;
            _instance._canRequestAds = true;
            _instance._rcReady = true;
            _instance.TryInitialize();
        }

        public void SetConfigAsset(IntelliVerseXConfig config)
        {
            configAsset = config;
            if (config != null) IVXAdNetworkConfig.LoadConfig(config);
        }

        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name == typeName || type.FullName == typeName)
                            return type;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
