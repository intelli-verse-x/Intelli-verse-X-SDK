using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Games.Leaderboard
{
    /// <summary>
    /// IVXGLeaderboard - Production-Ready Runtime Leaderboard Manager for IntelliVerseX Games SDK.
    /// Provides MonoBehaviour-based access to the static IVXGLeaderboardManager.
    /// Fixed for Android/iOS compatibility with main thread callbacks and proper error handling.
    /// </summary>
    [DisallowMultipleComponent]
    public class IVXGLeaderboard : MonoBehaviour
    {
        #region Singleton

        private static IVXGLeaderboard _instance;
        private static readonly object _instanceLock = new object();
        private static volatile bool _isQuitting;

        public static IVXGLeaderboard Instance
        {
            get
            {
                if (_isQuitting) return _instance;

                lock (_instanceLock)
                {
                    return _instance;
                }
            }
            private set
            {
                lock (_instanceLock)
                {
                    _instance = value;
                }
            }
        }

        #endregion

        #region Inspector Configuration

        [Header("Behaviour")]
        [Tooltip("If true, auto-initializes Nakama on Start.")]
        [SerializeField] private bool autoInitializeNakama = false;

        [Tooltip("If true, runs a smoke test after initialization.")]
        [SerializeField] private bool autoTestOnStart = false;

        [Tooltip("Enable debug logging.")]
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Test Settings")]
        [SerializeField] private int testMinScore = 100;
        [SerializeField] private int testMaxScore = 10000;
        [SerializeField] private int testLeaderboardLimit = 10;
        [SerializeField] private bool useGlobalForRankAndBest = false;

        [Header("Timeout Configuration")]
        [Tooltip("Timeout for leaderboard operations (seconds).")]
        [SerializeField] private float operationTimeoutSeconds = 30f;

        [Tooltip("Minimum interval between rank checks (seconds).")]
        [SerializeField] private float rankCheckCooldownSeconds = 5f;

        [Header("Notification Integration")]
        [Tooltip("Enable rank drop notifications.")]
        [SerializeField] private bool enableRankDropNotifications = true;

        [Header("Platform Settings")]
        [Tooltip("Use main thread dispatcher for callbacks.")]
        [SerializeField] private bool forceMainThreadCallbacks = true;

        [Header("Status (Read-only)")]
        [SerializeField] private bool nakamaInitialized;
        [SerializeField] private string nakamaUserId = "<none>";
        [SerializeField] private string nakamaUsername = "<none>";
        [SerializeField] private string lastErrorMessage = "";

        [Header("Leaderboard Snapshot (Read-only)")]
        [SerializeField] private int lastDailyCount;
        [SerializeField] private int lastWeeklyCount;
        [SerializeField] private int lastMonthlyCount;
        [SerializeField] private int lastAlltimeCount;
        [SerializeField] private int lastGlobalAlltimeCount;

        [Header("Player Rank Tracking (Read-only)")]
        [SerializeField] private int currentPlayerRank;
        [SerializeField] private long currentPlayerBestScore;

        [Header("Debug Info (Read-only)")]
        [SerializeField] private string currentPlatform = "";
        [SerializeField] private string lastRefreshTime = "";
        [SerializeField] private int totalRefreshAttempts;
        [SerializeField] private int successfulRefreshes;
        [SerializeField] private int failedRefreshes;

        #endregion

        #region Constants

        private const string LogTag = "[IVXGLeaderboard]";
        private const string ErrorNoInternet = "No internet connection. Please check your network.";
        private const string ErrorSessionExpired = "Session expired. Please log in again.";
        private const string ErrorTimeout = "Request timed out. Please try again.";
        private const string ErrorServerUnavailable = "Server unavailable. Please try again later.";
        private const string ErrorUnknown = "An error occurred. Please try again.";

        #endregion

        #region State

        private readonly object _rankLockObj = new object();
        private readonly object _operationLockObj = new object();
        private volatile bool _isRankLocked;
        private volatile bool _isOperationLocked;
        private float _lastRankCheckTime;
        private volatile bool _isDestroyed;
        private volatile bool _isRefreshing;
        private CancellationTokenSource _lifetimeCts;
        private SynchronizationContext _mainThreadContext;

        #endregion

        #region Public Properties

        public bool NakamaInitialized => nakamaInitialized;
        public string NakamaUserId => nakamaUserId;
        public string NakamaUsername => nakamaUsername;
        public string LastErrorMessage => lastErrorMessage;
        public int CurrentPlayerRank => currentPlayerRank;
        public long CurrentPlayerBestScore => currentPlayerBestScore;
        public bool IsRefreshing => _isRefreshing;
        public IVXGAllLeaderboardsResponse LastLeaderboardResponse { get; private set; }

        /// <summary>
        /// Returns true if leaderboard operations can be performed.
        /// </summary>
        public bool IsReadyForOperations
        {
            get
            {
                if (_isDestroyed || _isQuitting) return false;

                try
                {
                    // Check if guest user (leaderboard disabled for guests)
                    if (IsGuestUser) return false;

                    return nakamaInitialized;
                }
                catch (Exception ex)
                {
                    Log($"IsReadyForOperations exception: {ex.Message}", isError: true);
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if current user is a guest (leaderboard disabled).
        /// </summary>
        public bool IsGuestUser
        {
            get
            {
                try
                {
                    var guestType = FindType("GuestAuthBootstrapper");
                    if (guestType != null)
                    {
                        var isGuestProp = guestType.GetProperty("IsGuestUser",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (isGuestProp != null)
                        {
                            return isGuestProp.GetValue(null) as bool? ?? false;
                        }
                    }
                }
                catch { }
                return false;
            }
        }

        public bool IsLeaderboardAvailable => IsReadyForOperations;
        public bool CanViewLeaderboard => IsReadyForOperations;
        public bool CanSubmitScore => !IsGuestUser && IsReadyForOperations;

        #endregion

        #region Events

        public event Action<IVXGAllLeaderboardsResponse> OnLeaderboardsUpdated;
        public event Action<int, int> OnPlayerRankChanged;
        public event Action<string, string> OnLeaderboardError;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            lock (_instanceLock)
            {
                if (_instance != null && _instance != this)
                {
                    Log("Duplicate instance detected. Destroying.", isWarning: true);
                    Destroy(gameObject);
                    return;
                }

                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            _mainThreadContext = SynchronizationContext.Current;
            currentPlatform = $"{Application.platform} | {SystemInfo.operatingSystem}";
            Log($"Initializing on platform: {currentPlatform}");

            _lifetimeCts = new CancellationTokenSource();

            IVXGLeaderboardManager.EnableDebugLogs = enableDebugLogs;
            SubscribeToCoreEvents();
            RefreshStatusSnapshot();
        }

        private void Start()
        {
            StartCoroutine(SafeStartCoroutine());
        }

        private System.Collections.IEnumerator SafeStartCoroutine()
        {
            yield return null;

            if (IsGuestUser)
            {
                Log("Guest user detected. Leaderboard features disabled.");
                nakamaInitialized = false;
                yield break;
            }

            if (autoInitializeNakama)
            {
                yield return InitializeNakamaCoroutine();
            }

            if (autoTestOnStart && IsReadyForOperations)
            {
                yield return RunStartupSmokeTestCoroutine();
            }
        }

        private System.Collections.IEnumerator InitializeNakamaCoroutine()
        {
            Log("Starting Nakama initialization...");
            var task = InitializeNakamaAsync();
            while (!task.IsCompleted) yield return null;

            if (task.Exception != null)
            {
                Log($"Nakama initialization failed: {task.Exception.Message}", isError: true);
            }
        }

        private System.Collections.IEnumerator RunStartupSmokeTestCoroutine()
        {
            Log("[SMOKE] Running startup test...");

            int min = Mathf.Min(testMinScore, testMaxScore);
            int max = Mathf.Max(testMinScore, testMaxScore);
            if (max < int.MaxValue) max++;
            int score = UnityEngine.Random.Range(min, max);

            var submitTask = SubmitScoreAsync(score);
            while (!submitTask.IsCompleted) yield return null;

            var submitResult = submitTask.Result;
            Log(submitResult != null && submitResult.success
                ? $"[SMOKE] Submit OK. Reward={submitResult.reward_earned}"
                : "[SMOKE] Submit FAILED.");

            var fetchTask = RefreshLeaderboardsAsync();
            while (!fetchTask.IsCompleted) yield return null;

            bool fetchResult = fetchTask.Result;
            Log(fetchResult ? "[SMOKE] Fetch OK." : "[SMOKE] Fetch FAILED.");

            Log("[SMOKE] Startup test complete.");
        }

        private void OnDestroy()
        {
            _isDestroyed = true;

            lock (_instanceLock)
            {
                if (_instance == this) _instance = null;
            }

            UnsubscribeFromCoreEvents();
            CleanupResources();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            _isDestroyed = true;
            try { _lifetimeCts?.Cancel(); } catch { }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && nakamaInitialized)
            {
                Log("App resumed, refreshing status...");
                RefreshStatusSnapshot();
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToCoreEvents()
        {
            try
            {
                IVXGLeaderboardManager.OnScoreSubmitted -= HandleScoreSubmitted;
                IVXGLeaderboardManager.OnLeaderboardsFetched -= HandleLeaderboardsFetched;
                IVXGLeaderboardManager.OnError -= HandleLeaderboardError;

                IVXGLeaderboardManager.OnScoreSubmitted += HandleScoreSubmitted;
                IVXGLeaderboardManager.OnLeaderboardsFetched += HandleLeaderboardsFetched;
                IVXGLeaderboardManager.OnError += HandleLeaderboardError;

                Log("Subscribed to core events successfully.");
            }
            catch (Exception ex)
            {
                Log($"Failed to subscribe to core events: {ex.Message}", isError: true);
            }
        }

        private void UnsubscribeFromCoreEvents()
        {
            try
            {
                IVXGLeaderboardManager.OnScoreSubmitted -= HandleScoreSubmitted;
                IVXGLeaderboardManager.OnLeaderboardsFetched -= HandleLeaderboardsFetched;
                IVXGLeaderboardManager.OnError -= HandleLeaderboardError;
            }
            catch { }

            OnLeaderboardsUpdated = null;
            OnPlayerRankChanged = null;
            OnLeaderboardError = null;
        }

        private void CleanupResources()
        {
            try
            {
                _lifetimeCts?.Cancel();
                _lifetimeCts?.Dispose();
                _lifetimeCts = null;
            }
            catch { }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize Nakama connection.
        /// </summary>
        public async Task<bool> InitializeNakamaAsync()
        {
            if (_isDestroyed) return false;

            try
            {
                Log("Initializing Nakama...");

                // Use reflection to call IVXNManager.Instance.InitializeForCurrentUserAsync()
                var mgrType = FindType("IntelliVerseX.Backend.Nakama.IVXNManager");
                if (mgrType == null)
                {
                    SetError("Nakama manager not available", "IVXNManager type not found");
                    return false;
                }

                var instanceProp = mgrType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var mgr = instanceProp?.GetValue(null);

                if (mgr == null)
                {
                    SetError("Nakama manager not available", "IVXNManager.Instance is null");
                    return false;
                }

                var initMethod = mgrType.GetMethod("InitializeForCurrentUserAsync");
                if (initMethod != null)
                {
                    var task = initMethod.Invoke(mgr, null) as Task<bool>;
                    if (task != null)
                    {
                        bool success = await task;
                        RefreshStatusSnapshot();

                        if (success)
                        {
                            Log($"Nakama initialized successfully. UserId={nakamaUserId}, Username={nakamaUsername}");
                            ClearError();
                        }
                        else
                        {
                            SetError("Failed to initialize Nakama", "InitializeForCurrentUserAsync returned false");
                        }

                        return success;
                    }
                }

                SetError("Failed to initialize Nakama", "InitializeForCurrentUserAsync method not found");
                return false;
            }
            catch (Exception ex)
            {
                SetError(ErrorUnknown, ex.Message);
                Log($"InitializeNakamaAsync exception: {ex.Message}\n{ex.StackTrace}", isError: true);
                return false;
            }
        }

        /// <summary>
        /// Refresh all leaderboards from the server.
        /// </summary>
        public async Task<bool> RefreshLeaderboardsAsync(int limitOverride = -1, CancellationToken ct = default)
        {
            totalRefreshAttempts++;
            lastRefreshTime = DateTime.Now.ToString("HH:mm:ss");

            Log($"RefreshLeaderboardsAsync called. IsReadyForOperations={IsReadyForOperations}, IsRefreshing={_isRefreshing}");

            if (_isDestroyed)
            {
                Log("RefreshLeaderboardsAsync: Object is destroyed", isWarning: true);
                failedRefreshes++;
                return false;
            }

            if (!IsReadyForOperations)
            {
                string error = nakamaInitialized ? ErrorSessionExpired : "Not initialized";
                SetError(error, "IsReadyForOperations=false");
                Log($"RefreshLeaderboardsAsync: Not ready - {error}", isWarning: true);
                failedRefreshes++;
                return false;
            }

            lock (_operationLockObj)
            {
                if (_isOperationLocked)
                {
                    Log("Refresh already in progress, skipping.");
                    return false;
                }
                _isOperationLocked = true;
            }

            try
            {
                _isRefreshing = true;
                int limit = limitOverride > 0 ? limitOverride : testLeaderboardLimit;

                Log($"Fetching leaderboards (limit={limit})...");

                IVXGAllLeaderboardsResponse response = null;

                try
                {
                    response = await SafeExecuteAsync(
                        () => IVXGLeaderboardManager.GetAllLeaderboardsAsync(limit),
                        "Fetch leaderboards",
                        ct
                    );
                }
                catch (Exception ex)
                {
                    Log($"GetAllLeaderboardsAsync exception: {ex.Message}\n{ex.StackTrace}", isError: true);
                    SetError(MapToUserFriendlyError(ex.Message), ex.Message);
                    failedRefreshes++;
                    return false;
                }

                if (response == null)
                {
                    Log("RefreshLeaderboardsAsync: Response is null", isWarning: true);
                    SetError(ErrorUnknown, "Response is null");
                    failedRefreshes++;
                    return false;
                }

                if (!response.success)
                {
                    string error = response.error ?? "Unknown error";
                    Log($"RefreshLeaderboardsAsync: success=false, error={error}", isWarning: true);
                    SetError(MapToUserFriendlyError(error), error);
                    failedRefreshes++;
                    return false;
                }

                await ProcessLeaderboardResponseAsync(response);

                ClearError();
                successfulRefreshes++;
                Log($"Leaderboards refreshed successfully. Total records: daily={lastDailyCount}, weekly={lastWeeklyCount}, monthly={lastMonthlyCount}, alltime={lastAlltimeCount}");
                return true;
            }
            catch (OperationCanceledException)
            {
                Log("Refresh cancelled.");
                failedRefreshes++;
                return false;
            }
            catch (Exception ex)
            {
                SetError(MapToUserFriendlyError(ex.Message), ex.Message);
                Log($"RefreshLeaderboardsAsync exception: {ex.Message}\n{ex.StackTrace}", isError: true);
                failedRefreshes++;
                return false;
            }
            finally
            {
                _isRefreshing = false;
                lock (_operationLockObj)
                {
                    _isOperationLocked = false;
                }
            }
        }

        /// <summary>
        /// Submit score to leaderboard.
        /// </summary>
        public async Task<IVXGScoreSubmissionResponse> SubmitScoreAsync(int score)
        {
            if (IsGuestUser)
            {
                Log("SubmitScoreAsync: Guest user detected. Score submission skipped.");
                return new IVXGScoreSubmissionResponse
                {
                    success = true,
                    score = score,
                    reward_earned = 0,
                    reward_currency = ""
                };
            }

            if (_isDestroyed || !IsReadyForOperations)
            {
                SetError(ErrorSessionExpired, "Not ready for operations");
                return null;
            }

            try
            {
                Log($"Submitting score: {score}");

                var response = await SafeExecuteAsync(
                    () => IVXGLeaderboardManager.SubmitScoreAsync(score),
                    "Submit score"
                );

                if (response != null && response.success)
                {
                    Log($"Score submitted. Reward={response.reward_earned} {response.reward_currency}");
                    ClearError();
                    StartCoroutine(SafeCheckRankAfterSubmissionCoroutine());
                }
                else
                {
                    SetError("Score submission failed", response?.error ?? "Unknown error");
                }

                return response;
            }
            catch (Exception ex)
            {
                SetError(MapToUserFriendlyError(ex.Message), ex.Message);
                Log($"SubmitScoreAsync exception: {ex.Message}", isError: true);
                return null;
            }
        }

        #endregion

        #region Safe Async Execution

        private async Task<T> SafeExecuteAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken ct = default)
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeoutSeconds));

                CancellationTokenSource linkedCts = null;
                CancellationToken effectiveToken;

                if (ct.CanBeCanceled)
                {
                    linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
                    effectiveToken = linkedCts.Token;
                }
                else
                {
                    effectiveToken = timeoutCts.Token;
                }

                try
                {
                    var task = operation();
                    var delayTask = Task.Delay(Timeout.Infinite, effectiveToken);
                    var completed = await Task.WhenAny(task, delayTask);

                    if (completed != task)
                    {
                        throw new TimeoutException($"{operationName} timed out after {operationTimeoutSeconds}s");
                    }

                    return await task;
                }
                finally
                {
                    linkedCts?.Dispose();
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException($"{operationName} timed out after {operationTimeoutSeconds}s");
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Count == 1)
                {
                    throw ae.InnerExceptions[0];
                }
                throw;
            }
        }

        #endregion

        #region Event Handlers

        private void HandleScoreSubmitted(IVXGScoreSubmissionResponse response)
        {
            if (response == null)
            {
                Log("HandleScoreSubmitted: null response", isWarning: true);
                return;
            }

            Log($"Score submitted event: success={response.success}, score={response.score}, " +
                $"reward={response.reward_earned} {response.reward_currency}");
        }

        private void HandleLeaderboardsFetched(IVXGAllLeaderboardsResponse response)
        {
            if (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(HandleLeaderboardsFetchedCoroutine(response));
            }
        }

        private System.Collections.IEnumerator HandleLeaderboardsFetchedCoroutine(IVXGAllLeaderboardsResponse response)
        {
            if (_isDestroyed) yield break;

            if (response == null)
            {
                Log("HandleLeaderboardsFetched: null response", isWarning: true);
                yield break;
            }

            if (!response.success)
            {
                SetError(MapToUserFriendlyError(response.error ?? ""), response.error ?? "Unknown error");
                yield break;
            }

            var task = ProcessLeaderboardResponseAsync(response);
            while (!task.IsCompleted) yield return null;

            if (task.Exception != null)
            {
                Log($"HandleLeaderboardsFetchedCoroutine exception: {task.Exception.Message}", isWarning: true);
            }
        }

        private void HandleLeaderboardError(string message)
        {
            Log($"Leaderboard error event: {message}", isError: true);
            SetError(MapToUserFriendlyError(message), message);
        }

        #endregion

        #region Processing Pipeline

        private async Task ProcessLeaderboardResponseAsync(IVXGAllLeaderboardsResponse response)
        {
            LastLeaderboardResponse = response;
            UpdateLeaderboardCounts(response);
            await UpdatePlayerRankFromResponseAsync(response);
            InvokeOnMainThread(() => SafeInvokeLeaderboardsUpdated(response));
            LogLeaderboardsSafe(response);
        }

        private void UpdateLeaderboardCounts(IVXGAllLeaderboardsResponse response)
        {
            lastDailyCount = response.daily?.records?.Count ?? 0;
            lastWeeklyCount = response.weekly?.records?.Count ?? 0;
            lastMonthlyCount = response.monthly?.records?.Count ?? 0;
            lastAlltimeCount = response.alltime?.records?.Count ?? 0;
            lastGlobalAlltimeCount = response.global_alltime?.records?.Count ?? 0;

            Log($"Leaderboard counts updated: daily={lastDailyCount}, weekly={lastWeeklyCount}, " +
                $"monthly={lastMonthlyCount}, alltime={lastAlltimeCount}, global={lastGlobalAlltimeCount}");
        }

        private async Task UpdatePlayerRankFromResponseAsync(IVXGAllLeaderboardsResponse response)
        {
            if (!enableRankDropNotifications) return;

            lock (_rankLockObj)
            {
                if (_isRankLocked) return;
                _isRankLocked = true;
            }

            try
            {
                int playerRank = FindPlayerRankInResponse(response);

                if (playerRank <= 0 && Time.realtimeSinceStartup - _lastRankCheckTime >= rankCheckCooldownSeconds)
                {
                    _lastRankCheckTime = Time.realtimeSinceStartup;
                    try
                    {
                        playerRank = await SafeExecuteAsync(
                            () => IVXGLeaderboardManager.GetPlayerRankAsync(useGlobalForRankAndBest),
                            "Get player rank (fallback)"
                        );
                    }
                    catch (Exception ex)
                    {
                        Log($"Rank fallback failed: {ex.Message}", isWarning: true);
                    }
                }

                if (playerRank > 0)
                {
                    UpdateRank(playerRank);
                }
            }
            finally
            {
                lock (_rankLockObj)
                {
                    _isRankLocked = false;
                }
            }
        }

        private int FindPlayerRankInResponse(IVXGAllLeaderboardsResponse response)
        {
            if (response == null) return 0;

            string currentUserId = nakamaUserId;
            if (string.IsNullOrEmpty(currentUserId) || currentUserId == "<none>")
                return 0;

            if (response.alltime?.records != null)
            {
                foreach (var record in response.alltime.records)
                {
                    if (!string.IsNullOrEmpty(record.owner_id) && record.owner_id == currentUserId)
                    {
                        return record.rank;
                    }
                }
            }

            if (response.global_alltime?.records != null)
            {
                foreach (var record in response.global_alltime.records)
                {
                    if (!string.IsNullOrEmpty(record.owner_id) && record.owner_id == currentUserId)
                    {
                        return record.rank;
                    }
                }
            }

            return 0;
        }

        private void UpdateRank(int newRank)
        {
            if (newRank <= 0) return;

            int previousRank = currentPlayerRank;
            currentPlayerRank = newRank;

            if (previousRank > 0 && previousRank != newRank)
            {
                Log($"Rank changed: #{previousRank} → #{newRank}");
                InvokeOnMainThread(() => SafeInvokeRankChanged(previousRank, newRank));
            }
            else if (previousRank == 0)
            {
                Log($"Initial rank set: #{newRank}");
            }
        }

        private System.Collections.IEnumerator SafeCheckRankAfterSubmissionCoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            if (_isDestroyed) yield break;

            lock (_rankLockObj)
            {
                if (_isRankLocked) yield break;
                _isRankLocked = true;
            }

            Task<int> rankTask = null;
            Task<long> scoreTask = null;

            try
            {
                rankTask = SafeExecuteAsync(
                    () => IVXGLeaderboardManager.GetPlayerRankAsync(useGlobalForRankAndBest),
                    "Get rank after submission"
                );
            }
            catch (Exception ex)
            {
                Log($"SafeCheckRankAfterSubmission rank exception: {ex.Message}", isWarning: true);
            }

            if (rankTask != null)
            {
                while (!rankTask.IsCompleted) yield return null;
                if (rankTask.Exception == null && rankTask.Result > 0)
                {
                    UpdateRank(rankTask.Result);
                }
            }

            try
            {
                scoreTask = SafeExecuteAsync(
                    () => IVXGLeaderboardManager.GetPlayerBestScoreAsync(useGlobalForRankAndBest),
                    "Get best score"
                );
            }
            catch (Exception ex)
            {
                Log($"SafeCheckRankAfterSubmission score exception: {ex.Message}", isWarning: true);
            }

            if (scoreTask != null)
            {
                while (!scoreTask.IsCompleted) yield return null;
                if (scoreTask.Exception == null && scoreTask.Result > 0)
                {
                    currentPlayerBestScore = scoreTask.Result;
                }
            }

            lock (_rankLockObj)
            {
                _isRankLocked = false;
            }
        }

        #endregion

        #region Main Thread Dispatcher

        private void InvokeOnMainThread(Action action)
        {
            if (action == null) return;

            if (!forceMainThreadCallbacks)
            {
                action();
                return;
            }

            if (_mainThreadContext != null)
            {
                _mainThreadContext.Post(_ =>
                {
                    try
                    {
                        if (!_isDestroyed) action();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{LogTag} Main thread callback exception: {ex.Message}");
                    }
                }, null);
            }
            else
            {
                try { action(); }
                catch (Exception ex)
                {
                    Debug.LogError($"{LogTag} Direct callback exception: {ex.Message}");
                }
            }
        }

        #endregion

        #region Error Management

        private void SetError(string userFriendly, string technical)
        {
            lastErrorMessage = technical;
            InvokeOnMainThread(() => SafeInvokeError(userFriendly, technical));
        }

        private void ClearError()
        {
            lastErrorMessage = "";
        }

        private string MapToUserFriendlyError(string technicalError)
        {
            if (string.IsNullOrEmpty(technicalError)) return ErrorUnknown;

            string lower = technicalError.ToLowerInvariant();

            if (lower.Contains("timeout") || lower.Contains("timed out")) return ErrorTimeout;
            if (lower.Contains("network") || lower.Contains("internet") || lower.Contains("connection")) return ErrorNoInternet;
            if (lower.Contains("expired") || lower.Contains("unauthorized") || lower.Contains("session")) return ErrorSessionExpired;
            if (lower.Contains("server") || lower.Contains("503") || lower.Contains("unavailable")) return ErrorServerUnavailable;

            return ErrorUnknown;
        }

        #endregion

        #region Safe Event Invocation

        private void SafeInvokeLeaderboardsUpdated(IVXGAllLeaderboardsResponse response)
        {
            try { OnLeaderboardsUpdated?.Invoke(response); }
            catch (Exception ex) { Log($"OnLeaderboardsUpdated listener exception: {ex.Message}", isWarning: true); }
        }

        private void SafeInvokeRankChanged(int previousRank, int newRank)
        {
            try { OnPlayerRankChanged?.Invoke(previousRank, newRank); }
            catch (Exception ex) { Log($"OnPlayerRankChanged listener exception: {ex.Message}", isWarning: true); }
        }

        private void SafeInvokeError(string userFriendly, string technical)
        {
            try { OnLeaderboardError?.Invoke(userFriendly, technical); }
            catch (Exception ex) { Log($"OnLeaderboardError listener exception: {ex.Message}", isWarning: true); }
        }

        #endregion

        #region Status Snapshot

        private void RefreshStatusSnapshot()
        {
            try
            {
                var runtimeType = FindType("IntelliVerseX.Backend.Nakama.IVXNUserRuntime");
                if (runtimeType != null)
                {
                    var instanceProp = runtimeType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var runtime = instanceProp?.GetValue(null);

                    if (runtime != null)
                    {
                        var nakamaUserIdProp = runtimeType.GetProperty("NakamaUserId");
                        var nakamaUsernameProp = runtimeType.GetProperty("NakamaUsername");
                        var isExpiredProp = runtimeType.GetProperty("NakamaIsExpired");

                        string id = nakamaUserIdProp?.GetValue(runtime) as string;
                        bool hasId = !string.IsNullOrEmpty(id) && !id.Equals("<null>", StringComparison.OrdinalIgnoreCase);
                        bool isExpired = isExpiredProp?.GetValue(runtime) as bool? ?? true;

                        nakamaInitialized = hasId && !isExpired;
                        nakamaUserId = hasId ? id : "<none>";
                        nakamaUsername = nakamaUsernameProp?.GetValue(runtime) as string ?? "<none>";

                        Log($"Status snapshot (runtime): initialized={nakamaInitialized}, userId={nakamaUserId}, username={nakamaUsername}");
                        return;
                    }
                }

                var mgrType = FindType("IntelliVerseX.Backend.Nakama.IVXNManager");
                if (mgrType != null)
                {
                    var instanceProp = mgrType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var mgr = instanceProp?.GetValue(null);

                    if (mgr != null)
                    {
                        var isInitProp = mgrType.GetProperty("IsInitialized");
                        var sessionProp = mgrType.GetProperty("Session");
                        var nakamaUserIdProp = mgrType.GetProperty("NakamaUserId");
                        var nakamaUsernameProp = mgrType.GetProperty("NakamaUsername");

                        bool isInit = isInitProp?.GetValue(mgr) as bool? ?? false;
                        var session = sessionProp?.GetValue(mgr);
                        bool sessionValid = session != null;

                        if (sessionValid)
                        {
                            var isExpiredProp = session.GetType().GetProperty("IsExpired");
                            bool isExpired = isExpiredProp?.GetValue(session) as bool? ?? true;
                            sessionValid = !isExpired;
                        }

                        nakamaInitialized = isInit && sessionValid;
                        nakamaUserId = nakamaUserIdProp?.GetValue(mgr) as string ?? "<none>";
                        nakamaUsername = nakamaUsernameProp?.GetValue(mgr) as string ?? "<none>";

                        Log($"Status snapshot (manager): initialized={nakamaInitialized}, userId={nakamaUserId}, username={nakamaUsername}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"RefreshStatusSnapshot exception: {ex.Message}", isWarning: true);
            }
        }

        #endregion

        #region Logging

        private void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!enableDebugLogs && !isError && !isWarning) return;

            string msg = $"{LogTag} [{currentPlatform}] {message}";

            if (isError) Debug.LogError(msg);
            else if (isWarning) Debug.LogWarning(msg);
            else Debug.Log(msg);
        }

        private void LogLeaderboardsSafe(IVXGAllLeaderboardsResponse response)
        {
            if (!enableDebugLogs) return;

            try
            {
                Log($"Leaderboards: Daily={lastDailyCount}, Weekly={lastWeeklyCount}, " +
                    $"Monthly={lastMonthlyCount}, Alltime={lastAlltimeCount}, Global={lastGlobalAlltimeCount}");
            }
            catch { }
        }

        private static Type FindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            type = Type.GetType($"{typeName}, Assembly-CSharp");
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }

        #endregion

        #region Public Test Methods

        public async void TestSubmitLowScore()
        {
            const int lowScore = 10;
            try
            {
                Log($"[TEST] Submitting LOW score: {lowScore}");
                var response = await SubmitScoreAsync(lowScore);
                Log(response != null && response.success
                    ? $"[TEST] ✅ Low score submitted. Reward={response.reward_earned} {response.reward_currency}"
                    : "[TEST] ❌ Low score submission failed.", isWarning: response == null || !response.success);
            }
            catch (Exception ex) { Log($"TestSubmitLowScore exception: {ex.Message}", isError: true); }
        }

        public async void TestSubmitHighScore()
        {
            const int highScore = 9999;
            try
            {
                Log($"[TEST] Submitting HIGH score: {highScore}");
                var response = await SubmitScoreAsync(highScore);
                Log(response != null && response.success
                    ? $"[TEST] ✅ High score submitted. Reward={response.reward_earned} {response.reward_currency}"
                    : "[TEST] ❌ High score submission failed.", isWarning: response == null || !response.success);
            }
            catch (Exception ex) { Log($"TestSubmitHighScore exception: {ex.Message}", isError: true); }
        }

        public void TestResetWinStreak()
        {
            try { IVXGLeaderboardManager.ResetWinStreak(); Log("[TEST] ✅ Win streak reset."); }
            catch (Exception ex) { Log($"TestResetWinStreak exception: {ex.Message}", isError: true); }
        }

        public async void TestGetPlayerBestScore()
        {
            try
            {
                Log($"[TEST] Getting player best score (global={useGlobalForRankAndBest})...");
                long bestScore = await SafeExecuteAsync(
                    () => IVXGLeaderboardManager.GetPlayerBestScoreAsync(useGlobalForRankAndBest),
                    "Get player best score"
                );
                if (bestScore > 0)
                {
                    currentPlayerBestScore = bestScore;
                    Log($"[TEST] ✅ Player best score: {bestScore}");
                }
                else { Log("[TEST] ❌ No best score found.", isWarning: true); }
            }
            catch (Exception ex) { Log($"TestGetPlayerBestScore exception: {ex.Message}", isError: true); }
        }

        public async void TestFetchLeaderboardsTop50()
        {
            try
            {
                Log("[TEST] Fetching leaderboards (Top 50)...");
                bool success = await RefreshLeaderboardsAsync(50);
                Log(success ? "[TEST] ✅ Top 50 fetched." : "[TEST] ❌ Top 50 failed.", isWarning: !success);
            }
            catch (Exception ex) { Log($"TestFetchLeaderboardsTop50 exception: {ex.Message}", isError: true); }
        }

        public async void TestFetchLeaderboardsTop10()
        {
            try
            {
                Log("[TEST] Fetching leaderboards (Top 10)...");
                bool success = await RefreshLeaderboardsAsync(10);
                Log(success ? "[TEST] ✅ Top 10 fetched." : "[TEST] ❌ Top 10 failed.", isWarning: !success);
            }
            catch (Exception ex) { Log($"TestFetchLeaderboardsTop10 exception: {ex.Message}", isError: true); }
        }

        #endregion
    }
}
