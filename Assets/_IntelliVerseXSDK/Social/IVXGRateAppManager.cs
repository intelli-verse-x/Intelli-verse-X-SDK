// IVXGRateAppManager.cs
// Cross-platform app rating/review for IntelliVerseX Games SDK
// Supports iOS StoreKit and Android In-App Review API

using System;
using UnityEngine;

namespace IntelliVerseX.Games.Social
{
    /// <summary>
    /// IVXGRateAppManager - Production-Ready Rate My App Manager
    /// ----------------------------------------------------------
    /// 
    /// Features:
    /// - Native in-app review on iOS 14+ (StoreKit)
    /// - Native in-app review on Android (Play Core)
    /// - Smart prompting with configurable triggers
    /// - Persistent tracking to avoid over-prompting
    /// - Fallback to store page for older OS versions
    /// </summary>
    public class IVXGRateAppManager : MonoBehaviour
    {
        #region Singleton

        private static IVXGRateAppManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        public static IVXGRateAppManager Instance
        {
            get
            {
                if (_isQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
#if UNITY_2023_1_OR_NEWER
                        _instance = FindFirstObjectByType<IVXGRateAppManager>();
#else
                        _instance = FindObjectOfType<IVXGRateAppManager>();
#endif
                        if (_instance == null)
                        {
                            var go = new GameObject("[IVXGRateAppManager]");
                            _instance = go.AddComponent<IVXGRateAppManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        #endregion

        #region Config

        [Header("App Store Info")]
        [SerializeField] private string iosAppId = "";
        [SerializeField] private string androidPackageName = "";

        [Header("Prompting Rules")]
        [Tooltip("Minimum sessions before showing rate prompt")]
        [SerializeField] private int minSessionsBeforePrompt = 3;

        [Tooltip("Minimum days since install before showing prompt")]
        [SerializeField] private int minDaysBeforePrompt = 2;

        [Tooltip("Days to wait after user dismisses prompt before showing again")]
        [SerializeField] private int daysBeforeRetry = 7;

        [Tooltip("Maximum times to show the prompt ever")]
        [SerializeField] private int maxPromptAttempts = 3;

        [Tooltip("Require a positive event (win, achievement) before prompting")]
        [SerializeField] private bool requirePositiveEvent = true;

        [Header("Settings")]
        [SerializeField] private bool logDebug = true;

        #endregion

        #region Persistence Keys

        private const string KEY_FIRST_LAUNCH = "ivxg_rate_first_launch";
        private const string KEY_SESSION_COUNT = "ivxg_rate_sessions";
        private const string KEY_PROMPT_COUNT = "ivxg_rate_prompts";
        private const string KEY_LAST_PROMPT = "ivxg_rate_last_prompt";
        private const string KEY_RATED = "ivxg_rate_completed";
        private const string KEY_NEVER_ASK = "ivxg_rate_never_ask";

        #endregion

        #region State

        private bool _hasPositiveEvent;
        private int _sessionCount;
        private int _promptCount;
        private DateTime _firstLaunch;
        private DateTime _lastPrompt;
        private bool _hasRated;
        private bool _neverAsk;

        /// <summary>Event fired when rate prompt is shown</summary>
        public event Action OnRatePromptShown;

        /// <summary>Event fired when user completes rating (or we assume they did)</summary>
        public event Action OnRateCompleted;

        /// <summary>Has user already rated the app?</summary>
        public bool HasRated => _hasRated;

        /// <summary>Has user chosen "never ask again"?</summary>
        public bool NeverAskAgain => _neverAsk;

        /// <summary>Number of times prompt has been shown</summary>
        public int PromptCount => _promptCount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-detect package name
            if (string.IsNullOrEmpty(androidPackageName))
            {
                androidPackageName = Application.identifier;
            }

            LoadState();
            IncrementSessionCount();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Call this after a positive event (win, achievement, etc.)
        /// </summary>
        public void RegisterPositiveEvent()
        {
            _hasPositiveEvent = true;
            Log("Positive event registered");
        }

        /// <summary>
        /// Try to show rate prompt if conditions are met
        /// </summary>
        public bool TryShowRatePrompt()
        {
            if (!ShouldShowPrompt())
            {
                return false;
            }

            ShowRatePrompt();
            return true;
        }

        /// <summary>
        /// Force show rate prompt (bypasses conditions)
        /// </summary>
        public void ForceShowRatePrompt()
        {
            ShowRatePrompt();
        }

        /// <summary>
        /// Show rate prompt if appropriate, called automatically at good moments
        /// </summary>
        public void ShowRatePromptIfAppropriate()
        {
            if (ShouldShowPrompt())
            {
                ShowRatePrompt();
            }
        }

        /// <summary>
        /// Open app store page directly
        /// </summary>
        public void OpenStorePage()
        {
            string url = GetStoreUrl();
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                Log($"Opened store page: {url}");
            }
        }

        /// <summary>
        /// User selected "never ask again"
        /// </summary>
        public void SetNeverAskAgain()
        {
            _neverAsk = true;
            PlayerPrefs.SetInt(KEY_NEVER_ASK, 1);
            PlayerPrefs.Save();
            Log("User selected 'never ask again'");
        }

        /// <summary>
        /// Mark that user has rated (call after opening store)
        /// </summary>
        public void MarkAsRated()
        {
            _hasRated = true;
            PlayerPrefs.SetInt(KEY_RATED, 1);
            PlayerPrefs.Save();
            OnRateCompleted?.Invoke();
            Log("Marked as rated");
        }

        /// <summary>
        /// Reset all rate tracking (for testing)
        /// </summary>
        public void ResetTracking()
        {
            PlayerPrefs.DeleteKey(KEY_FIRST_LAUNCH);
            PlayerPrefs.DeleteKey(KEY_SESSION_COUNT);
            PlayerPrefs.DeleteKey(KEY_PROMPT_COUNT);
            PlayerPrefs.DeleteKey(KEY_LAST_PROMPT);
            PlayerPrefs.DeleteKey(KEY_RATED);
            PlayerPrefs.DeleteKey(KEY_NEVER_ASK);
            PlayerPrefs.Save();

            _sessionCount = 0;
            _promptCount = 0;
            _hasRated = false;
            _neverAsk = false;
            _firstLaunch = DateTime.UtcNow;
            _lastPrompt = DateTime.MinValue;

            Log("Rate tracking reset");
        }

        #endregion

        #region Internal Logic

        private bool ShouldShowPrompt()
        {
            // Never show if already rated or opted out
            if (_hasRated || _neverAsk)
            {
                Log($"Skip prompt: hasRated={_hasRated}, neverAsk={_neverAsk}");
                return false;
            }

            // Max attempts reached
            if (_promptCount >= maxPromptAttempts)
            {
                Log($"Skip prompt: max attempts reached ({_promptCount}/{maxPromptAttempts})");
                return false;
            }

            // Not enough sessions
            if (_sessionCount < minSessionsBeforePrompt)
            {
                Log($"Skip prompt: not enough sessions ({_sessionCount}/{minSessionsBeforePrompt})");
                return false;
            }

            // Not enough days since install
            int daysSinceInstall = (int)(DateTime.UtcNow - _firstLaunch).TotalDays;
            if (daysSinceInstall < minDaysBeforePrompt)
            {
                Log($"Skip prompt: not enough days ({daysSinceInstall}/{minDaysBeforePrompt})");
                return false;
            }

            // Too soon since last prompt
            if (_lastPrompt != DateTime.MinValue)
            {
                int daysSinceLastPrompt = (int)(DateTime.UtcNow - _lastPrompt).TotalDays;
                if (daysSinceLastPrompt < daysBeforeRetry)
                {
                    Log($"Skip prompt: too soon since last ({daysSinceLastPrompt}/{daysBeforeRetry} days)");
                    return false;
                }
            }

            // Require positive event
            if (requirePositiveEvent && !_hasPositiveEvent)
            {
                Log("Skip prompt: waiting for positive event");
                return false;
            }

            return true;
        }

        private void ShowRatePrompt()
        {
            _promptCount++;
            _lastPrompt = DateTime.UtcNow;
            _hasPositiveEvent = false; // Reset for next time

            PlayerPrefs.SetInt(KEY_PROMPT_COUNT, _promptCount);
            PlayerPrefs.SetString(KEY_LAST_PROMPT, _lastPrompt.ToString("O"));
            PlayerPrefs.Save();

            Log($"Showing rate prompt (attempt {_promptCount})");

            // Try native in-app review
            bool nativeShown = TryShowNativeReview();

            if (!nativeShown)
            {
                // Fallback: open store page
                Log("Native review not available, opening store page");
                OpenStorePage();
            }

            OnRatePromptShown?.Invoke();
        }

        private bool TryShowNativeReview()
        {
#if UNITY_EDITOR
            Log("In-app review simulated in Editor");
            return true;
#elif UNITY_IOS
            return TryShowIOSReview();
#elif UNITY_ANDROID
            return TryShowAndroidReview();
#else
            return false;
#endif
        }

#if UNITY_IOS
        private bool TryShowIOSReview()
        {
            try
            {
                // Check iOS version (14.0+)
                string[] versionParts = UnityEngine.iOS.Device.systemVersion.Split('.');
                if (versionParts.Length > 0 && int.TryParse(versionParts[0], out int majorVersion))
                {
                    if (majorVersion >= 14)
                    {
                        UnityEngine.iOS.Device.RequestStoreReview();
                        Log("iOS StoreKit review requested");
                        return true;
                    }
                }

                Log("iOS version < 14, StoreKit not available");
                return false;
            }
            catch (Exception ex)
            {
                Log($"iOS review error: {ex.Message}", true);
                return false;
            }
        }
#endif

#if UNITY_ANDROID
        private bool TryShowAndroidReview()
        {
            try
            {
                // Try VoxelBusters EssentialKit first
                var rateMyAppType = FindType("VoxelBusters.EssentialKit.RateMyApp");
                if (rateMyAppType != null)
                {
                    var askMethod = rateMyAppType.GetMethod("AskForReview", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (askMethod != null)
                    {
                        askMethod.Invoke(null, null);
                        Log("Android review via VoxelBusters EssentialKit");
                        return true;
                    }
                }

                // Try Google Play Core directly
                var reviewManagerType = FindType("Google.Play.Review.ReviewManager");
                if (reviewManagerType != null)
                {
                    var instance = Activator.CreateInstance(reviewManagerType);
                    var requestFlowMethod = reviewManagerType.GetMethod("RequestReviewFlow");
                    if (requestFlowMethod != null)
                    {
                        requestFlowMethod.Invoke(instance, null);
                        Log("Android review via Play Core");
                        return true;
                    }
                }

                Log("Android native review not available");
                return false;
            }
            catch (Exception ex)
            {
                Log($"Android review error: {ex.Message}", true);
                return false;
            }
        }
#endif

        #endregion

        #region Persistence

        private void LoadState()
        {
            // First launch
            if (PlayerPrefs.HasKey(KEY_FIRST_LAUNCH))
            {
                DateTime.TryParse(PlayerPrefs.GetString(KEY_FIRST_LAUNCH), out _firstLaunch);
            }
            else
            {
                _firstLaunch = DateTime.UtcNow;
                PlayerPrefs.SetString(KEY_FIRST_LAUNCH, _firstLaunch.ToString("O"));
            }

            // Session count
            _sessionCount = PlayerPrefs.GetInt(KEY_SESSION_COUNT, 0);

            // Prompt count
            _promptCount = PlayerPrefs.GetInt(KEY_PROMPT_COUNT, 0);

            // Last prompt
            if (PlayerPrefs.HasKey(KEY_LAST_PROMPT))
            {
                DateTime.TryParse(PlayerPrefs.GetString(KEY_LAST_PROMPT), out _lastPrompt);
            }
            else
            {
                _lastPrompt = DateTime.MinValue;
            }

            // Rated status
            _hasRated = PlayerPrefs.GetInt(KEY_RATED, 0) == 1;

            // Never ask
            _neverAsk = PlayerPrefs.GetInt(KEY_NEVER_ASK, 0) == 1;

            PlayerPrefs.Save();

            Log($"State loaded: sessions={_sessionCount}, prompts={_promptCount}, rated={_hasRated}");
        }

        private void IncrementSessionCount()
        {
            _sessionCount++;
            PlayerPrefs.SetInt(KEY_SESSION_COUNT, _sessionCount);
            PlayerPrefs.Save();
            Log($"Session count: {_sessionCount}");
        }

        #endregion

        #region Helpers

        private string GetStoreUrl()
        {
#if UNITY_IOS
            return string.IsNullOrEmpty(iosAppId) 
                ? null 
                : $"https://apps.apple.com/app/id{iosAppId}?action=write-review";
#else
            return string.IsNullOrEmpty(androidPackageName) 
                ? null 
                : $"https://play.google.com/store/apps/details?id={androidPackageName}";
#endif
        }

        private Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName, false);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        private void Log(string message, bool isError = false)
        {
            if (!logDebug && !isError) return;

            if (isError)
                Debug.LogError($"[IVXGRateAppManager] {message}");
            else
                Debug.Log($"[IVXGRateAppManager] {message}");
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Set app store IDs
        /// </summary>
        public void SetAppIds(string iosId, string androidPackage)
        {
            iosAppId = iosId;
            androidPackageName = androidPackage;
        }

        /// <summary>
        /// Configure prompting rules
        /// </summary>
        public void SetPromptingRules(int minSessions, int minDays, int retryDays, int maxAttempts)
        {
            minSessionsBeforePrompt = minSessions;
            minDaysBeforePrompt = minDays;
            daysBeforeRetry = retryDays;
            maxPromptAttempts = maxAttempts;
        }

        #endregion
    }
}
