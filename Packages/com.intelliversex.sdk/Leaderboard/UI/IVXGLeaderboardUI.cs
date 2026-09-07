using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Games.Leaderboard.UI
{
    /// <summary>
    /// Main UI controller for displaying leaderboards in IntelliVerseX Games SDK.
    /// Supports Daily, Weekly, Monthly, and All-time leaderboard periods.
    /// Production-ready with retry logic, proper error handling, and mobile compatibility.
    /// </summary>
    [DisallowMultipleComponent]
    public class IVXGLeaderboardUI : MonoBehaviour
    {
        private const string LOGTAG = "[IVXGLeaderboardUI]";

        #region Inspector Fields

        [Header("Dependencies")]
        [Tooltip("Reference to IVXGLeaderboard runtime manager. Auto-binds from Instance if not set.")]
        [SerializeField] private IVXGLeaderboard leaderboardBridge;

        [Tooltip("Parent transform for spawning entry prefabs.")]
        [SerializeField] private Transform entriesParent;

        [Tooltip("Prefab for leaderboard entry rows.")]
        [SerializeField] private IVXGLeaderboardEntryView entryPrefab;

        [Tooltip("Optional status text for displaying loading/error messages.")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Tooltip("Optional scroll view for auto-scroll functionality.")]
        [SerializeField] private ScrollRect scrollRect;

        [Header("Period Filter Buttons")]
        [SerializeField] private Button dailyButton;
        [SerializeField] private Button weeklyButton;
        [SerializeField] private Button monthlyButton;
        [SerializeField] private Button alltimeButton;
        [SerializeField] private Button refreshButton;

        [Header("Navigation Buttons (Always Interactable)")]
        [Tooltip("Close/back button - always stays interactable")]
        [SerializeField] private Button closeButton;

        [Header("Heading")]
        [Tooltip("Text displaying current leaderboard period")]
        [SerializeField] private TextMeshProUGUI headingText;

        [Header("Behaviour")]
        [Tooltip("Auto-bind IVXGLeaderboard.Instance if no bridge reference set")]
        [SerializeField] private bool autoBindBridge = true;

        [Tooltip("Auto-fetch leaderboard on enable")]
        [SerializeField] private bool autoFetchOnEnable = true;

        [Tooltip("Maximum entries to display")]
        [Range(1, 200)]
        [SerializeField] private int maxEntries = 50;

        [Tooltip("Highlight current player's entry")]
        [SerializeField] private bool highlightLocalPlayer = true;

        [Tooltip("Use alternating row colors")]
        [SerializeField] private bool useAlternateRowTint = true;

        [Tooltip("Default leaderboard period")]
        [SerializeField] private IVXGLeaderboardPeriod defaultPeriod = IVXGLeaderboardPeriod.Alltime;

        [Header("Retry Configuration")]
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelaySeconds = 2f;
        [SerializeField] private float initializationTimeoutSeconds = 10f;

        [Header("Loading Indicator")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private float loadingIndicatorDelay = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Debug Info (Read-only)")]
        [SerializeField] private string currentPlatform = "";
        [SerializeField] private string lastFetchTime = "";
        [SerializeField] private int totalFetchAttempts;
        [SerializeField] private int successfulFetches;
        [SerializeField] private int failedFetches;
        [SerializeField] private bool isBridgeReady;
        [SerializeField] private bool isInitialized;

        #endregion

        #region Private State

        private readonly List<IVXGLeaderboardEntryView> _spawnedEntries = new List<IVXGLeaderboardEntryView>();
        private bool _isFetching;
        private IVXGLeaderboardPeriod _currentPeriod;
        private IVXGAllLeaderboardsResponse _lastResponse;
        private Coroutine _fetchCoroutine;
        private Coroutine _initCoroutine;
        private Coroutine _loadingIndicatorCoroutine;
        private bool _isDestroyed;

        #endregion

        #region Public Properties

        public IVXGLeaderboardPeriod CurrentPeriod => _currentPeriod;
        public bool IsFetching => _isFetching;
        public bool IsInitialized => isInitialized;
        public IVXGAllLeaderboardsResponse LastResponse => _lastResponse;

        #endregion

        #region Events

        public event Action OnUIRefreshed;
        public event Action<string> OnFetchError;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            try
            {
                currentPlatform = $"{Application.platform} | {SystemInfo.operatingSystem}";
                _currentPeriod = defaultPeriod;
                Log($"Awake on platform: {currentPlatform}");

                InitializeUIComponents();
                IVXGLeaderboardManager.OnError -= HandleLeaderboardError;
                IVXGLeaderboardManager.OnError += HandleLeaderboardError;
            }
            catch (Exception ex)
            {
                Log($"Awake exception: {ex.Message}", isError: true);
            }
        }

        private void Start()
        {
            _initCoroutine = StartCoroutine(InitializeCoroutine());
        }

        private IEnumerator InitializeCoroutine()
        {
            Log("Starting initialization...");
            yield return null;

            try
            {
                BindBridge();
                SetupButtons();
                UpdateHeadingText();
                ValidateReferences();
                isInitialized = true;
                Log("Initialization complete.");
            }
            catch (Exception ex)
            {
                Log($"InitializeCoroutine exception: {ex.Message}", isError: true);
                isInitialized = true;
            }
        }

        private void OnEnable()
        {
            try
            {
                Log("OnEnable called");

                if (!isInitialized)
                {
                    StartCoroutine(WaitForInitAndFetch());
                    return;
                }

                BindBridge();
                UpdateHeadingText();

                if (autoFetchOnEnable)
                {
                    RefreshLeaderboardFromServer();
                }
                else
                {
                    TryDisplayCachedData();
                }
            }
            catch (Exception ex)
            {
                Log($"OnEnable exception: {ex.Message}", isError: true);
            }
        }

        private IEnumerator WaitForInitAndFetch()
        {
            float elapsed = 0f;

            while (!isInitialized && elapsed < initializationTimeoutSeconds)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            if (!isInitialized)
            {
                Log("Initialization timeout - forcing", isWarning: true);
                isInitialized = true;
            }

            BindBridge();

            if (autoFetchOnEnable && isActiveAndEnabled)
            {
                RefreshLeaderboardFromServer();
            }
        }

        private void OnDisable()
        {
            StopAllFetchOperations();
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            StopAllFetchOperations();
            UnbindBridge();
            IVXGLeaderboardManager.OnError -= HandleLeaderboardError;
            OnUIRefreshed = null;
            OnFetchError = null;
        }

        private void StopAllFetchOperations()
        {
            if (_fetchCoroutine != null)
            {
                StopCoroutine(_fetchCoroutine);
                _fetchCoroutine = null;
            }
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }
            if (_loadingIndicatorCoroutine != null)
            {
                StopCoroutine(_loadingIndicatorCoroutine);
                _loadingIndicatorCoroutine = null;
            }
            _isFetching = false;
            HideLoadingIndicator();
        }

        #endregion

        #region Initialization

        private void InitializeUIComponents()
        {
            HideLoadingIndicator();
        }

        private void ValidateReferences()
        {
            if (entriesParent == null)
                Log("entriesParent is not assigned!", isError: true);
            if (entryPrefab == null)
                Log("entryPrefab is not assigned!", isError: true);
            if (statusText == null)
                Log("statusText is not assigned (optional)", isWarning: true);
        }

        #endregion

        #region Bridge Binding

        private void BindBridge()
        {
            try
            {
                if (leaderboardBridge == null && autoBindBridge)
                {
                    leaderboardBridge = IVXGLeaderboard.Instance;
                }

                isBridgeReady = leaderboardBridge != null && leaderboardBridge.IsReadyForOperations;

                if (leaderboardBridge != null)
                {
                    Log($"BindBridge: Using IVXGLeaderboard bridge. Ready={isBridgeReady}");

                    leaderboardBridge.OnLeaderboardsUpdated -= HandleLeaderboardsFetched;
                    leaderboardBridge.OnLeaderboardError -= HandleBridgeError;

                    leaderboardBridge.OnLeaderboardsUpdated += HandleLeaderboardsFetched;
                    leaderboardBridge.OnLeaderboardError += HandleBridgeError;

                    IVXGLeaderboardManager.OnLeaderboardsFetched -= HandleLeaderboardsFetched;
                }
                else
                {
                    Log("BindBridge: No IVXGLeaderboard found. Using static manager directly.", isWarning: true);

                    IVXGLeaderboardManager.OnLeaderboardsFetched -= HandleLeaderboardsFetched;
                    IVXGLeaderboardManager.OnLeaderboardsFetched += HandleLeaderboardsFetched;
                }
            }
            catch (Exception ex)
            {
                Log($"BindBridge exception: {ex.Message}", isError: true);
            }
        }

        private void UnbindBridge()
        {
            try
            {
                if (leaderboardBridge != null)
                {
                    leaderboardBridge.OnLeaderboardsUpdated -= HandleLeaderboardsFetched;
                    leaderboardBridge.OnLeaderboardError -= HandleBridgeError;
                }

                IVXGLeaderboardManager.OnLeaderboardsFetched -= HandleLeaderboardsFetched;
            }
            catch (Exception ex)
            {
                Log($"UnbindBridge exception: {ex.Message}", isWarning: true);
            }
        }

        #endregion

        #region Button Setup

        private void SetupButtons()
        {
            try
            {
                SetupPeriodButton(dailyButton, IVXGLeaderboardPeriod.Daily);
                SetupPeriodButton(weeklyButton, IVXGLeaderboardPeriod.Weekly);
                SetupPeriodButton(monthlyButton, IVXGLeaderboardPeriod.Monthly);
                SetupPeriodButton(alltimeButton, IVXGLeaderboardPeriod.Alltime);

                if (refreshButton != null)
                {
                    refreshButton.onClick.RemoveAllListeners();
                    refreshButton.onClick.AddListener(OnRefreshButtonClicked);
                }

                if (closeButton != null)
                {
                    closeButton.onClick.RemoveAllListeners();
                    closeButton.onClick.AddListener(OnCloseButtonClicked);
                    closeButton.interactable = true;
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Log($"SetupButtons exception: {ex.Message}", isError: true);
            }
        }

        private void SetupPeriodButton(Button button, IVXGLeaderboardPeriod period)
        {
            if (button == null) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPeriodButtonClicked(period));
        }

        private void OnPeriodButtonClicked(IVXGLeaderboardPeriod period)
        {
            try
            {
                if (_isFetching)
                {
                    Log("Period button click ignored - fetch in progress");
                    return;
                }

                Log($"Period button clicked: {period}");
                SetPeriodAndRebuild(period);
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Log($"OnPeriodButtonClicked exception: {ex.Message}", isError: true);
            }
        }

        private void OnRefreshButtonClicked()
        {
            try
            {
                if (_isFetching)
                {
                    Log("Refresh button ignored - fetch in progress");
                    return;
                }

                Log("Refresh button clicked");
                ForceRefresh();
            }
            catch (Exception ex)
            {
                Log($"OnRefreshButtonClicked exception: {ex.Message}", isError: true);
            }
        }

        private void OnCloseButtonClicked()
        {
            try
            {
                Log("Close button clicked");
                StopAllFetchOperations();
                gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                Log($"OnCloseButtonClicked exception: {ex.Message}", isError: true);
            }
        }

        private void UpdateButtonStates()
        {
            // Update visual state of period buttons based on current selection
            UpdatePeriodButtonVisual(dailyButton, _currentPeriod == IVXGLeaderboardPeriod.Daily);
            UpdatePeriodButtonVisual(weeklyButton, _currentPeriod == IVXGLeaderboardPeriod.Weekly);
            UpdatePeriodButtonVisual(monthlyButton, _currentPeriod == IVXGLeaderboardPeriod.Monthly);
            UpdatePeriodButtonVisual(alltimeButton, _currentPeriod == IVXGLeaderboardPeriod.Alltime);
        }

        private void UpdatePeriodButtonVisual(Button button, bool isSelected)
        {
            if (button == null) return;

            // Optional: change button appearance when selected
            var colors = button.colors;
            if (isSelected)
            {
                colors.normalColor = colors.selectedColor;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            button.colors = colors;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Refresh leaderboard from server.
        /// </summary>
        [ContextMenu("Refresh Leaderboard")]
        public void RefreshLeaderboardFromServer()
        {
            try
            {
                if (!isActiveAndEnabled)
                {
                    Log("RefreshLeaderboardFromServer: Component not active", isWarning: true);
                    return;
                }

                if (_isFetching)
                {
                    Log("RefreshLeaderboardFromServer: Already fetching, ignoring");
                    return;
                }

                StopAllFetchOperations();
                _fetchCoroutine = StartCoroutine(RefreshCoroutineWithRetry());
            }
            catch (Exception ex)
            {
                Log($"RefreshLeaderboardFromServer exception: {ex.Message}", isError: true);
            }
        }

        /// <summary>
        /// Force refresh - clears cache.
        /// </summary>
        public void ForceRefresh()
        {
            _lastResponse = null;
            RefreshLeaderboardFromServer();
        }

        /// <summary>
        /// Set the leaderboard period.
        /// </summary>
        public void SetPeriod(IVXGLeaderboardPeriod period)
        {
            SetPeriodAndRebuild(period);
        }

        /// <summary>
        /// Get number of displayed entries.
        /// </summary>
        public int GetDisplayedEntryCount()
        {
            return _spawnedEntries.Count(e => e != null && e.gameObject.activeSelf);
        }

        #endregion

        #region Fetch Logic

        private IEnumerator RefreshCoroutineWithRetry()
        {
            _isFetching = true;
            totalFetchAttempts++;
            lastFetchTime = DateTime.Now.ToString("HH:mm:ss");

            ShowLoadingIndicator();
            UpdateStatus("Loading...", isError: false);
            SetButtonsInteractable(false);

            yield return null;

            bool success = false;
            int attempts = 0;
            string lastError = null;

            while (!success && attempts < maxRetryAttempts && !_isDestroyed)
            {
                attempts++;
                Log($"Fetch attempt {attempts}/{maxRetryAttempts}");

                if (leaderboardBridge == null && autoBindBridge)
                {
                    leaderboardBridge = IVXGLeaderboard.Instance;
                    if (leaderboardBridge != null)
                    {
                        BindBridge();
                    }
                }

                if (leaderboardBridge != null)
                {
                    bool bridgeSuccess = false;
                    yield return FetchViaBridgeCoroutine(result => bridgeSuccess = result);
                    success = bridgeSuccess;

                    if (!success)
                    {
                        lastError = leaderboardBridge.LastErrorMessage;
                    }
                }
                else
                {
                    var result = new FetchResult();
                    yield return FetchViaManager(result);
                    success = result.Success;
                    lastError = result.Error;
                }

                if (!success && attempts < maxRetryAttempts)
                {
                    Log($"Fetch attempt {attempts} failed. Retrying in {retryDelaySeconds}s...");
                    UpdateStatus($"Retrying... ({attempts}/{maxRetryAttempts})", isError: false);
                    yield return new WaitForSeconds(retryDelaySeconds);
                }
            }

            HideLoadingIndicator();

            if (success)
            {
                successfulFetches++;
                Log($"Leaderboard fetch successful after {attempts} attempt(s)");
            }
            else
            {
                failedFetches++;
                Log($"Leaderboard fetch failed after {attempts} attempt(s): {lastError}", isError: true);
                UpdateStatus(MapToUserFriendlyError(lastError), isError: true);
                SafeInvokeError(lastError);
            }

            SetButtonsInteractable(true);
            _isFetching = false;
            _fetchCoroutine = null;
        }

        private IEnumerator FetchViaBridgeCoroutine(Action<bool> onComplete)
        {
            float waitTime = 0f;
            while (!leaderboardBridge.IsReadyForOperations && waitTime < initializationTimeoutSeconds)
            {
                if (!leaderboardBridge.NakamaInitialized)
                {
                    Log("Bridge not initialized. Attempting initialization...");
                    var initTask = leaderboardBridge.InitializeNakamaAsync();

                    while (!initTask.IsCompleted)
                    {
                        yield return null;
                        waitTime += Time.deltaTime;

                        if (waitTime >= initializationTimeoutSeconds)
                        {
                            Log("Initialization timeout", isWarning: true);
                            onComplete(false);
                            yield break;
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }

            isBridgeReady = leaderboardBridge.IsReadyForOperations;

            if (!isBridgeReady)
            {
                Log("Bridge not ready after waiting", isWarning: true);
                onComplete(false);
                yield break;
            }

            var fetchTask = leaderboardBridge.RefreshLeaderboardsAsync(maxEntries);

            while (!fetchTask.IsCompleted)
            {
                yield return null;
            }

            if (fetchTask.Exception != null)
            {
                Log($"FetchViaBridge exception: {fetchTask.Exception.Message}", isError: true);
                onComplete(false);
                yield break;
            }

            bool success = fetchTask.Result;

            if (success && leaderboardBridge.LastLeaderboardResponse != null)
            {
                _lastResponse = leaderboardBridge.LastLeaderboardResponse;
                BuildUIFromResponse(_lastResponse);
            }

            onComplete(success);
        }

        private class FetchResult
        {
            public bool Success;
            public string Error;
        }

        private IEnumerator FetchViaManager(FetchResult result)
        {
            Log("Fetching directly via IVXGLeaderboardManager...");

            System.Threading.Tasks.Task<IVXGAllLeaderboardsResponse> task = null;

            try
            {
                task = IVXGLeaderboardManager.GetAllLeaderboardsAsync(maxEntries);
            }
            catch (Exception ex)
            {
                Log($"FetchViaManager: Failed to start task: {ex.Message}", isError: true);
                result.Success = false;
                result.Error = ex.Message;
                yield break;
            }

            while (task != null && !task.IsCompleted)
            {
                yield return null;
            }

            try
            {
                if (task.Exception != null)
                {
                    Log($"FetchViaManager task exception: {task.Exception.Message}", isError: true);
                    result.Success = false;
                    result.Error = task.Exception.Message;
                    yield break;
                }

                var response = task.Result;
                _lastResponse = response;

                if (response == null)
                {
                    Log("FetchViaManager: response is null.", isWarning: true);
                    result.Success = false;
                    result.Error = "No response received";
                }
                else if (!response.success)
                {
                    Log($"FetchViaManager: success=false. error={response.error}", isWarning: true);
                    result.Success = false;
                    result.Error = response.error;
                }
                else
                {
                    BuildUIFromResponse(response);
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                Log($"FetchViaManager exception: {ex.Message}", isError: true);
                result.Success = false;
                result.Error = ex.Message;
            }
        }

        #endregion

        #region Event Handlers

        private void HandleLeaderboardsFetched(IVXGAllLeaderboardsResponse response)
        {
            if (_isDestroyed) return;

            try
            {
                Log($"HandleLeaderboardsFetched: response={response != null}, success={response?.success}");

                if (response == null)
                {
                    Log("HandleLeaderboardsFetched: response is null.", isWarning: true);
                    return;
                }

                _lastResponse = response;

                if (!response.success)
                {
                    string error = response.error ?? "Unknown error";
                    Log($"HandleLeaderboardsFetched: success=false. error={error}", isWarning: true);
                    StartCoroutine(UpdateUIOnMainThread(() =>
                    {
                        UpdateStatus($"Error: {error}", isError: true);
                        SafeInvokeError(error);
                    }));
                    return;
                }

                StartCoroutine(UpdateUIOnMainThread(() =>
                {
                    BuildUIFromResponse(response);
                }));
            }
            catch (Exception ex)
            {
                Log($"HandleLeaderboardsFetched exception: {ex.Message}", isError: true);
            }
        }

        private void HandleBridgeError(string userFriendlyError, string technicalError)
        {
            if (_isDestroyed) return;

            try
            {
                Log($"HandleBridgeError: {userFriendlyError} | {technicalError}", isError: true);

                StartCoroutine(UpdateUIOnMainThread(() =>
                {
                    UpdateStatus(userFriendlyError, isError: true);
                    SafeInvokeError(technicalError);
                }));
            }
            catch (Exception ex)
            {
                Log($"HandleBridgeError exception: {ex.Message}", isError: true);
            }
        }

        private void HandleLeaderboardError(string message)
        {
            if (_isDestroyed) return;

            try
            {
                Log($"HandleLeaderboardError: {message}", isError: true);

                StartCoroutine(UpdateUIOnMainThread(() =>
                {
                    UpdateStatus($"Error: {message}", isError: true);
                    SafeInvokeError(message);
                }));
            }
            catch (Exception ex)
            {
                Log($"HandleLeaderboardError exception: {ex.Message}", isError: true);
            }
        }

        #endregion

        #region UI Building

        private void SetPeriodAndRebuild(IVXGLeaderboardPeriod period)
        {
            _currentPeriod = period;
            Log($"SetPeriodAndRebuild: period={_currentPeriod}");

            UpdateHeadingText();

            if (_lastResponse != null && _lastResponse.success)
            {
                BuildUIFromResponse(_lastResponse);
            }
            else
            {
                RefreshLeaderboardFromServer();
            }
        }

        private void TryDisplayCachedData()
        {
            if (leaderboardBridge != null &&
                leaderboardBridge.LastLeaderboardResponse != null &&
                leaderboardBridge.LastLeaderboardResponse.success)
            {
                _lastResponse = leaderboardBridge.LastLeaderboardResponse;
                BuildUIFromResponse(_lastResponse);
                return;
            }

            if (_lastResponse != null && _lastResponse.success)
            {
                BuildUIFromResponse(_lastResponse);
            }
        }

        private void BuildUIFromResponse(IVXGAllLeaderboardsResponse response)
        {
            if (_isDestroyed) return;

            try
            {
                string periodLabel;
                var data = GetLeaderboardDataForCurrentPeriod(response, out periodLabel);

                if (data == null)
                {
                    Log($"BuildUIFromResponse: no data for period={_currentPeriod} ({periodLabel}).", isWarning: true);
                    ClearEntries();
                    UpdateStatus($"No {periodLabel.ToLowerInvariant()} scores yet. Be the first to play!", isError: false);
                    return;
                }

                if (data.records == null || data.records.Count == 0)
                {
                    Log($"BuildUIFromResponse: {periodLabel} records empty.", isWarning: true);
                    ClearEntries();
                    UpdateStatus($"No {periodLabel.ToLowerInvariant()} scores yet. Be the first to play!", isError: false);
                    return;
                }

                if (entriesParent == null || entryPrefab == null)
                {
                    Log("BuildUIFromResponse: entriesParent or entryPrefab not assigned.", isError: true);
                    UpdateStatus("UI configuration error.", isError: true);
                    return;
                }

                UpdateStatus(string.Empty, isError: false);

                string localOwnerId = highlightLocalPlayer ? ResolveLocalNakamaUserId() : null;

                var orderedRecords = data.records
                    .Where(r => r != null)
                    .OrderBy(r => r.rank <= 0 ? int.MaxValue : r.rank)
                    .ThenByDescending(r => r.score)
                    .Take(maxEntries)
                    .ToList();

                Log($"BuildUIFromResponse: period={periodLabel}, leaderboard_id={data.leaderboard_id}, count={orderedRecords.Count}");

                EnsureEntryPoolSize(orderedRecords.Count);

                for (int i = 0; i < orderedRecords.Count; i++)
                {
                    var record = orderedRecords[i];
                    var view = _spawnedEntries[i];

                    if (view == null) continue;

                    view.gameObject.SetActive(true);
                    bool isAltRow = useAlternateRowTint && (i % 2 == 1);
                    view.Setup(record, localOwnerId, isAltRow);
                }

                for (int i = orderedRecords.Count; i < _spawnedEntries.Count; i++)
                {
                    if (_spawnedEntries[i] != null)
                    {
                        _spawnedEntries[i].gameObject.SetActive(false);
                    }
                }

                RebuildScrollLayout();

                if (scrollRect != null)
                {
                    StartCoroutine(ScrollToTopNextFrame());
                }

                SafeInvokeUIRefreshed();
                Log($"UI built successfully with {orderedRecords.Count} entries");
            }
            catch (Exception ex)
            {
                Log($"BuildUIFromResponse exception: {ex.Message}\n{ex.StackTrace}", isError: true);
                UpdateStatus("Failed to build leaderboard UI.", isError: true);
            }
        }

        private IVXGLeaderboardData GetLeaderboardDataForCurrentPeriod(IVXGAllLeaderboardsResponse response, out string label)
        {
            switch (_currentPeriod)
            {
                case IVXGLeaderboardPeriod.Daily:
                    label = "Daily";
                    return response.daily;
                case IVXGLeaderboardPeriod.Weekly:
                    label = "Weekly";
                    return response.weekly;
                case IVXGLeaderboardPeriod.Monthly:
                    label = "Monthly";
                    return response.monthly;
                case IVXGLeaderboardPeriod.Global:
                    label = "Global";
                    return response.global_alltime;
                case IVXGLeaderboardPeriod.Alltime:
                default:
                    label = "All-time";
                    return response.alltime;
            }
        }

        private IEnumerator ScrollToTopNextFrame()
        {
            yield return null;

            if (scrollRect != null && !_isDestroyed)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        private void RebuildScrollLayout()
        {
            if (scrollRect == null || scrollRect.content == null) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (entriesParent is RectTransform rt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            Canvas.ForceUpdateCanvases();
        }

        #endregion

        #region Helpers

        private void ClearEntries()
        {
            for (int i = 0; i < _spawnedEntries.Count; i++)
            {
                if (_spawnedEntries[i] != null)
                {
                    _spawnedEntries[i].gameObject.SetActive(false);
                }
            }
        }

        private void EnsureEntryPoolSize(int required)
        {
            if (required <= _spawnedEntries.Count) return;

            int toCreate = required - _spawnedEntries.Count;

            for (int i = 0; i < toCreate; i++)
            {
                try
                {
                    var instance = Instantiate(entryPrefab, entriesParent);
                    instance.gameObject.SetActive(false);
                    _spawnedEntries.Add(instance);
                }
                catch (Exception ex)
                {
                    Log($"Failed to instantiate entry prefab: {ex.Message}", isError: true);
                    break;
                }
            }
        }

        private void UpdateStatus(string message, bool isError)
        {
            try
            {
                if (statusText == null) return;

                statusText.text = message ?? string.Empty;

                if (!string.IsNullOrEmpty(message))
                {
                    Log($"Status: {message}", isError: isError, isWarning: !isError && message.Contains("Error"));
                }
            }
            catch (Exception ex)
            {
                Log($"UpdateStatus exception: {ex.Message}", isWarning: true);
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            try
            {
                if (dailyButton != null) dailyButton.interactable = interactable;
                if (weeklyButton != null) weeklyButton.interactable = interactable;
                if (monthlyButton != null) monthlyButton.interactable = interactable;
                if (alltimeButton != null) alltimeButton.interactable = interactable;
                if (refreshButton != null) refreshButton.interactable = interactable;

                // Close button ALWAYS stays interactable
                if (closeButton != null) closeButton.interactable = true;
            }
            catch (Exception ex)
            {
                Log($"SetButtonsInteractable exception: {ex.Message}", isWarning: true);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (_loadingIndicatorCoroutine != null)
                {
                    StopCoroutine(_loadingIndicatorCoroutine);
                }
                _loadingIndicatorCoroutine = StartCoroutine(ShowLoadingIndicatorDelayed());
            }
        }

        private IEnumerator ShowLoadingIndicatorDelayed()
        {
            yield return new WaitForSeconds(loadingIndicatorDelay);

            if (loadingIndicator != null && _isFetching)
            {
                loadingIndicator.SetActive(true);
            }
        }

        private void HideLoadingIndicator()
        {
            if (_loadingIndicatorCoroutine != null)
            {
                StopCoroutine(_loadingIndicatorCoroutine);
                _loadingIndicatorCoroutine = null;
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
        }

        private IEnumerator UpdateUIOnMainThread(Action action)
        {
            yield return null;

            if (_isDestroyed || action == null) yield break;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log($"UpdateUIOnMainThread exception: {ex.Message}", isError: true);
            }
        }

        private string ResolveLocalNakamaUserId()
        {
            try
            {
                if (leaderboardBridge != null && !string.IsNullOrEmpty(leaderboardBridge.NakamaUserId))
                {
                    string id = leaderboardBridge.NakamaUserId;
                    if (id != "<none>" && !string.Equals(id, "<null>", StringComparison.OrdinalIgnoreCase))
                    {
                        return id;
                    }
                }

                // Try via reflection for decoupling
                var mgrType = Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, Assembly-CSharp");
                if (mgrType != null)
                {
                    var instanceProp = mgrType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var mgr = instanceProp?.GetValue(null);

                    if (mgr != null)
                    {
                        var nakamaUserIdProp = mgrType.GetProperty("NakamaUserId");
                        return nakamaUserIdProp?.GetValue(mgr) as string;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Log($"ResolveLocalNakamaUserId exception: {ex.Message}", isWarning: true);
                return null;
            }
        }

        private string MapToUserFriendlyError(string technicalError)
        {
            if (string.IsNullOrEmpty(technicalError))
                return "An error occurred. Please try again.";

            string lower = technicalError.ToLowerInvariant();

            if (lower.Contains("timeout") || lower.Contains("timed out"))
                return "Request timed out. Please try again.";

            if (lower.Contains("network") || lower.Contains("internet") || lower.Contains("connection"))
                return "No internet connection. Please check your network.";

            if (lower.Contains("expired") || lower.Contains("unauthorized") || lower.Contains("session"))
                return "Session expired. Please log in again.";

            if (lower.Contains("server") || lower.Contains("503") || lower.Contains("unavailable"))
                return "Server unavailable. Please try again later.";

            return "An error occurred. Please try again.";
        }

        private void UpdateHeadingText()
        {
            if (headingText == null) return;

            headingText.text = _currentPeriod switch
            {
                IVXGLeaderboardPeriod.Daily => "Daily Leaderboard",
                IVXGLeaderboardPeriod.Weekly => "Weekly Leaderboard",
                IVXGLeaderboardPeriod.Monthly => "Monthly Leaderboard",
                IVXGLeaderboardPeriod.Global => "Global Leaderboard",
                IVXGLeaderboardPeriod.Alltime => "All-Time Leaderboard",
                _ => "Leaderboard"
            };
        }

        #endregion

        #region Safe Event Invocation

        private void SafeInvokeUIRefreshed()
        {
            try { OnUIRefreshed?.Invoke(); }
            catch (Exception ex) { Log($"OnUIRefreshed listener exception: {ex.Message}", isWarning: true); }
        }

        private void SafeInvokeError(string error)
        {
            try { OnFetchError?.Invoke(error); }
            catch (Exception ex) { Log($"OnFetchError listener exception: {ex.Message}", isWarning: true); }
        }

        #endregion

        #region Logging

        private void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!enableDebugLogs && !isError) return;

            string msg = $"{LOGTAG} [{currentPlatform}] {message}";

            if (isError) Debug.LogError(msg);
            else if (isWarning) Debug.LogWarning(msg);
            else Debug.Log(msg);
        }

        #endregion
    }
}
