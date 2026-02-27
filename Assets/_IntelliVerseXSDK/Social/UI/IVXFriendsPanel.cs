using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Main controller for the Friends panel UI.
    /// Manages tabs (Friends, Requests, Search) and handles all friend operations.
    /// 
    /// Usage:
    ///   1. Add IVXFriendsCanvas prefab to your scene
    ///   2. Call IVXFriendsPanel.Instance.Open() to show the panel
    ///   3. Panel handles all API calls and UI updates automatically
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Social/Friends Panel")]
    public class IVXFriendsPanel : MonoBehaviour
    {
        #region Singleton

        private static IVXFriendsPanel _instance;
        
        /// <summary>Gets the singleton instance.</summary>
        public static IVXFriendsPanel Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<IVXFriendsPanel>();
#else
                    _instance = FindObjectOfType<IVXFriendsPanel>();
#endif
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Fields

        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform panelRectTransform;

        [Header("Tab Buttons")]
        [SerializeField] private Button friendsTabButton;
        [SerializeField] private Button requestsTabButton;
        [SerializeField] private Button searchTabButton;
        [SerializeField] private Button closeButton;

        [Header("Tab Indicators")]
        [SerializeField] private GameObject friendsTabIndicator;
        [SerializeField] private GameObject requestsTabIndicator;
        [SerializeField] private GameObject searchTabIndicator;
        [SerializeField] private TextMeshProUGUI requestsBadgeText;
        [SerializeField] private GameObject requestsBadge;

        [Header("Content Panels")]
        [SerializeField] private GameObject friendsContent;
        [SerializeField] private GameObject requestsContent;
        [SerializeField] private GameObject searchContent;
        [SerializeField] private CanvasGroup friendsContentCanvasGroup;
        [SerializeField] private CanvasGroup requestsContentCanvasGroup;
        [SerializeField] private CanvasGroup searchContentCanvasGroup;

        [Header("Friends List")]
        [SerializeField] private Transform friendsListContainer;
        [SerializeField] private GameObject friendSlotPrefab;
        [SerializeField] private TextMeshProUGUI friendsCountText;
        [SerializeField] private TextMeshProUGUI friendsEmptyText;

        [Header("Requests List")]
        [SerializeField] private Transform requestsListContainer;
        [SerializeField] private GameObject requestSlotPrefab;
        [SerializeField] private TextMeshProUGUI requestsCountText;
        [SerializeField] private TextMeshProUGUI requestsEmptyText;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;
        [SerializeField] private Transform searchResultsContainer;
        [SerializeField] private GameObject searchSlotPrefab;
        [SerializeField] private TextMeshProUGUI searchResultsText;
        [SerializeField] private TextMeshProUGUI searchEmptyText;
        [SerializeField] private GameObject searchInstructions;

        [Header("Loading")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private RectTransform loadingSpinner;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Error/Toast")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private float toastDuration = 3f;

        [Header("Behavior")]
        [Tooltip("If true, panel opens automatically when the scene loads")]
        [SerializeField] private bool autoOpenOnAwake = true;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmTitleText;
        [SerializeField] private TextMeshProUGUI confirmMessageText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        #endregion

        #region Events

        /// <summary>Fired when the panel is opened.</summary>
        public event Action OnPanelOpened;

        /// <summary>Fired when the panel is closed.</summary>
        public event Action OnPanelClosed;

        /// <summary>Fired when a friend is selected (for profile view).</summary>
        public event Action<FriendInfo> OnFriendSelected;

        #endregion

        #region Private Fields

        private enum Tab { Friends, Requests, Search }
        private Tab _currentTab = Tab.Friends;

        private List<FriendInfo> _friendsList = new List<FriendInfo>();
        private List<FriendRequest> _requestsList = new List<FriendRequest>();
        private List<FriendSearchResult> _searchResults = new List<FriendSearchResult>();

        private List<IVXFriendSlot> _friendSlots = new List<IVXFriendSlot>();
        private List<IVXFriendRequestSlot> _requestSlots = new List<IVXFriendRequestSlot>();
        private List<IVXFriendSearchSlot> _searchSlots = new List<IVXFriendSearchSlot>();

        private CancellationTokenSource _cts;
        private bool _isOpen;
        private float _lastRefreshTime;
        private Action _pendingConfirmAction;
        
        // Configuration constants
        private const int MAX_VISIBLE_FRIENDS = 50;
        private const int MAX_SEARCH_RESULTS = 20;
        private const float AUTO_REFRESH_INTERVAL = 0f; // 0 = disabled

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _instance = this;
            
            SetupButtons();
            
            // Initialize all content panels to a known state
            InitializeContentPanels();
        }

        private void Start()
        {
            // Auto-open if configured (useful for dedicated friends scenes)
            if (autoOpenOnAwake)
            {
                Open();
            }
        }

        /// <summary>
        /// Initializes all content panels to a clean starting state.
        /// </summary>
        private void InitializeContentPanels()
        {
            // Hide panel root initially if not auto-opening
            if (panelRoot != null && !autoOpenOnAwake)
            {
                panelRoot.SetActive(false);
            }
            
            // Ensure content panels start in correct state
            // Friends content is the default, others start hidden
            if (friendsContent != null)
            {
                friendsContent.SetActive(true);
                if (friendsContentCanvasGroup != null) friendsContentCanvasGroup.alpha = 1f;
            }
            if (requestsContent != null)
            {
                requestsContent.SetActive(false);
                if (requestsContentCanvasGroup != null) requestsContentCanvasGroup.alpha = 0f;
            }
            if (searchContent != null)
            {
                searchContent.SetActive(false);
                if (searchContentCanvasGroup != null) searchContentCanvasGroup.alpha = 0f;
            }
            
            // Hide overlays
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
            if (toastPanel != null) toastPanel.SetActive(false);
            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            // Kill any running animations to prevent errors
            IVXFriendsAnimations.KillAnimations(panelCanvasGroup);
            IVXFriendsAnimations.KillAnimations(friendsContentCanvasGroup);
            IVXFriendsAnimations.KillAnimations(requestsContentCanvasGroup);
            IVXFriendsAnimations.KillAnimations(searchContentCanvasGroup);
            if (loadingSpinner != null)
            {
                IVXFriendsAnimations.StopSpinnerRotation(loadingSpinner);
            }

            // Clear instance
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            // Auto-refresh if enabled and panel is open
            if (_isOpen && AUTO_REFRESH_INTERVAL > 0)
            {
                if (Time.time - _lastRefreshTime > AUTO_REFRESH_INTERVAL)
                {
                    RefreshCurrentTab();
                    _lastRefreshTime = Time.time;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the friends panel.
        /// </summary>
        public void Open()
        {
            if (_isOpen) return;

            _isOpen = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            // Ensure proper initial state for content panels
            EnsureContentPanelState(Tab.Friends);

            // Animate open
            IVXFriendsAnimations.AnimatePanelOpen(panelCanvasGroup, panelRectTransform);

            // Initialize to friends tab (force it even if already set)
            _currentTab = Tab.Friends;
            UpdateTabIndicators();

            // Load initial data
            _ = LoadInitialDataAsync();

            _lastRefreshTime = Time.time;
            OnPanelOpened?.Invoke();
        }

        /// <summary>
        /// Ensures proper state for content panels when opening.
        /// </summary>
        private void EnsureContentPanelState(Tab activeTab)
        {
            // Friends content
            if (friendsContent != null) friendsContent.SetActive(activeTab == Tab.Friends);
            if (friendsContentCanvasGroup != null) friendsContentCanvasGroup.alpha = activeTab == Tab.Friends ? 1f : 0f;
            
            // Requests content
            if (requestsContent != null) requestsContent.SetActive(activeTab == Tab.Requests);
            if (requestsContentCanvasGroup != null) requestsContentCanvasGroup.alpha = activeTab == Tab.Requests ? 1f : 0f;
            
            // Search content
            if (searchContent != null) searchContent.SetActive(activeTab == Tab.Search);
            if (searchContentCanvasGroup != null) searchContentCanvasGroup.alpha = activeTab == Tab.Search ? 1f : 0f;
        }

        /// <summary>
        /// Loads initial data when panel opens.
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            SetLoading(true, "Loading...");

            try
            {
                // Load friends and requests in parallel
                var friendsTask = IVXFriendsService.GetFriendsAsync(_cts?.Token ?? default);
                var requestsTask = IVXFriendsService.GetIncomingRequestsAsync(_cts?.Token ?? default);

                await Task.WhenAll(friendsTask, requestsTask);

                _friendsList = friendsTask.Result ?? new List<FriendInfo>();
                _requestsList = requestsTask.Result ?? new List<FriendRequest>();

                PopulateFriendsList();
                UpdateRequestsBadge();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
                Debug.Log("[IVXFriendsPanel] Initial load was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsPanel] Initial load error: {ex.Message}");
                ShowToast("Failed to load friends data", true);
                
                // Set empty lists on error
                _friendsList = new List<FriendInfo>();
                _requestsList = new List<FriendRequest>();
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Closes the friends panel.
        /// </summary>
        public void Close()
        {
            if (!_isOpen) return;

            _cts?.Cancel();
            
            // Hide toast and dialogs immediately
            HideToast();
            if (confirmDialog != null) confirmDialog.SetActive(false);
            
            // Kill loading animations
            SetLoading(false);

            IVXFriendsAnimations.AnimatePanelClose(panelCanvasGroup, panelRectTransform, () =>
            {
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }
                _isOpen = false;
                OnPanelClosed?.Invoke();
            });
        }

        /// <summary>
        /// Forces a complete refresh of all data (friends and requests).
        /// </summary>
        public async void ForceRefreshAll()
        {
            if (!_isOpen) return;
            
            await LoadInitialDataAsync();
            
            // Also refresh current tab's data
            RefreshCurrentTab();
        }

        /// <summary>
        /// Refreshes the current tab's data.
        /// </summary>
        public void RefreshCurrentTab()
        {
            switch (_currentTab)
            {
                case Tab.Friends:
                    _ = LoadFriendsAsync();
                    break;
                case Tab.Requests:
                    _ = LoadRequestsAsync();
                    break;
                case Tab.Search:
                    // Don't auto-refresh search
                    break;
            }
        }

        /// <summary>
        /// Shows a toast message.
        /// </summary>
        public void ShowToast(string message, bool isError = false)
        {
            if (toastPanel == null || toastText == null) return;

            toastText.text = message;
            toastText.color = isError ? Color.red : Color.white;
            toastPanel.SetActive(true);

            CancelInvoke(nameof(HideToast));
            Invoke(nameof(HideToast), toastDuration);
        }

        #endregion

        #region Tab Management

        private void SetupButtons()
        {
            // Tab buttons
            if (friendsTabButton != null)
                friendsTabButton.onClick.AddListener(() => SwitchToTab(Tab.Friends));
            
            if (requestsTabButton != null)
                requestsTabButton.onClick.AddListener(() => SwitchToTab(Tab.Requests));
            
            if (searchTabButton != null)
                searchTabButton.onClick.AddListener(() => SwitchToTab(Tab.Search));

            // Close button
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            // Search
            if (searchButton != null)
                searchButton.onClick.AddListener(OnSearchButtonClicked);

            if (searchInput != null)
                searchInput.onSubmit.AddListener(_ => OnSearchButtonClicked());

            // Confirmation dialog
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        private void SwitchToTab(Tab tab)
        {
            if (_currentTab == tab && _isOpen) return;

            var previousTab = _currentTab;
            var outgoingCanvasGroup = GetTabCanvasGroup(previousTab);
            var incomingCanvasGroup = GetTabCanvasGroup(tab);

            // Kill any existing animations to prevent stacking
            IVXFriendsAnimations.KillAnimations(outgoingCanvasGroup);
            IVXFriendsAnimations.KillAnimations(incomingCanvasGroup);

            // CRITICAL: Activate incoming content BEFORE animation so DOTween can find it
            _currentTab = tab;
            SetTabContentActive(tab, true);
            
            // Set initial alpha to 0 for fade-in effect (if DOTween is available)
            if (incomingCanvasGroup != null)
            {
                incomingCanvasGroup.alpha = 0f;
            }
            
            UpdateTabIndicators();

            IVXFriendsAnimations.AnimateTabSwitch(outgoingCanvasGroup, incomingCanvasGroup, () =>
            {
                // Hide old content after animation starts
                SetTabContentActive(previousTab, false);
            }, () =>
            {
                // OnComplete: Load data for new tab
                switch (tab)
                {
                    case Tab.Friends:
                        if (_friendsList.Count == 0) _ = LoadFriendsAsync();
                        break;
                    case Tab.Requests:
                        _ = LoadRequestsAsync();
                        break;
                    case Tab.Search:
                        if (searchInstructions != null) searchInstructions.SetActive(true);
                        if (searchEmptyText != null) searchEmptyText.gameObject.SetActive(false);
                        break;
                }
            });
        }

        private void SetTabContentActive(Tab tab, bool active)
        {
            switch (tab)
            {
                case Tab.Friends:
                    if (friendsContent != null) friendsContent.SetActive(active);
                    break;
                case Tab.Requests:
                    if (requestsContent != null) requestsContent.SetActive(active);
                    break;
                case Tab.Search:
                    if (searchContent != null) searchContent.SetActive(active);
                    break;
            }
        }

        private CanvasGroup GetTabCanvasGroup(Tab tab)
        {
            switch (tab)
            {
                case Tab.Friends: return friendsContentCanvasGroup;
                case Tab.Requests: return requestsContentCanvasGroup;
                case Tab.Search: return searchContentCanvasGroup;
                default: return null;
            }
        }

        private void UpdateTabIndicators()
        {
            if (friendsTabIndicator != null)
                friendsTabIndicator.SetActive(_currentTab == Tab.Friends);
            
            if (requestsTabIndicator != null)
                requestsTabIndicator.SetActive(_currentTab == Tab.Requests);
            
            if (searchTabIndicator != null)
                searchTabIndicator.SetActive(_currentTab == Tab.Search);
        }

        private void UpdateRequestsBadge()
        {
            int count = _requestsList?.Count ?? 0;
            
            if (requestsBadge != null)
            {
                requestsBadge.SetActive(count > 0);
            }
            
            if (requestsBadgeText != null)
            {
                requestsBadgeText.text = count > 99 ? "99+" : count.ToString();
            }
        }

        #endregion

        #region Data Loading

        private async Task LoadFriendsAndRequestsAsync()
        {
            SetLoading(true, "Loading...");

            try
            {
                // Load both in parallel
                var friendsTask = IVXFriendsService.GetFriendsAsync(_cts?.Token ?? default);
                var requestsTask = IVXFriendsService.GetIncomingRequestsAsync(_cts?.Token ?? default);

                await Task.WhenAll(friendsTask, requestsTask);

                _friendsList = friendsTask.Result ?? new List<FriendInfo>();
                _requestsList = requestsTask.Result ?? new List<FriendRequest>();

                PopulateFriendsList();
                UpdateRequestsBadge();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsPanel] Load error: {ex.Message}");
                ShowToast("Failed to load friends", true);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async Task LoadFriendsAsync()
        {
            Debug.Log("[IVXFriendsPanel] LoadFriendsAsync called");
            SetLoading(true, "Loading friends...");

            try
            {
                Debug.Log("[IVXFriendsPanel] Calling IVXFriendsService.GetFriendsAsync...");
                _friendsList = await IVXFriendsService.GetFriendsAsync(_cts?.Token ?? default) ?? new List<FriendInfo>();
                Debug.Log($"[IVXFriendsPanel] GetFriendsAsync returned {_friendsList.Count} friends.");
                PopulateFriendsList();
            }
            catch (OperationCanceledException) 
            { 
                Debug.Log("[IVXFriendsPanel] LoadFriendsAsync was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsPanel] Load friends error: {ex.GetType().Name} - {ex.Message}");
                Debug.LogError($"[IVXFriendsPanel] Load friends stack trace: {ex.StackTrace}");
                ShowToast("Failed to load friends", true);
                _friendsList = new List<FriendInfo>();
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async Task LoadRequestsAsync()
        {
            Debug.Log("[IVXFriendsPanel] LoadRequestsAsync called");
            SetLoading(true, "Loading requests...");

            try
            {
                Debug.Log("[IVXFriendsPanel] Calling IVXFriendsService.GetIncomingRequestsAsync...");
                _requestsList = await IVXFriendsService.GetIncomingRequestsAsync(_cts?.Token ?? default) ?? new List<FriendRequest>();
                Debug.Log($"[IVXFriendsPanel] GetIncomingRequestsAsync returned {_requestsList.Count} requests.");
                PopulateRequestsList();
                UpdateRequestsBadge();
            }
            catch (OperationCanceledException) 
            { 
                Debug.Log("[IVXFriendsPanel] LoadRequestsAsync was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsPanel] Load requests error: {ex.GetType().Name} - {ex.Message}");
                Debug.LogError($"[IVXFriendsPanel] Load requests stack trace: {ex.StackTrace}");
                ShowToast("Failed to load requests", true);
                _requestsList = new List<FriendRequest>();
            }
            finally
            {
                SetLoading(false);
            }
        }

        #endregion

        #region List Population

        private void PopulateFriendsList()
        {
            ClearSlots(_friendSlots, friendsListContainer);
            _friendSlots.Clear();

            if (_friendsList == null || _friendsList.Count == 0)
            {
                if (friendsEmptyText != null)
                {
                    friendsEmptyText.gameObject.SetActive(true);
                    friendsEmptyText.text = "No friends yet. Search for users to add!";
                }
                if (friendsCountText != null)
                {
                    friendsCountText.text = "0 Friends";
                }
                return;
            }

            if (friendsEmptyText != null) friendsEmptyText.gameObject.SetActive(false);
            if (friendsCountText != null) friendsCountText.text = $"{_friendsList.Count} Friends";

            int maxToShow = Mathf.Min(_friendsList.Count, MAX_VISIBLE_FRIENDS);
            for (int i = 0; i < maxToShow; i++)
            {
                var slot = CreateSlot<IVXFriendSlot>(friendSlotPrefab, friendsListContainer);
                if (slot != null)
                {
                    slot.Initialize(_friendsList[i], i);
                    slot.OnRemoveClicked += OnRemoveFriendClicked;
                    slot.OnBlockClicked += OnBlockFriendClicked;
                    slot.OnProfileClicked += OnFriendProfileClicked;
                    _friendSlots.Add(slot);
                }
            }
        }

        private void PopulateRequestsList()
        {
            ClearSlots(_requestSlots, requestsListContainer);
            _requestSlots.Clear();

            if (_requestsList == null || _requestsList.Count == 0)
            {
                if (requestsEmptyText != null)
                {
                    requestsEmptyText.gameObject.SetActive(true);
                    requestsEmptyText.text = "No pending requests";
                }
                if (requestsCountText != null)
                {
                    requestsCountText.text = "0 Requests";
                }
                return;
            }

            if (requestsEmptyText != null) requestsEmptyText.gameObject.SetActive(false);
            if (requestsCountText != null) requestsCountText.text = $"{_requestsList.Count} Requests";

            for (int i = 0; i < _requestsList.Count; i++)
            {
                var slot = CreateSlot<IVXFriendRequestSlot>(requestSlotPrefab, requestsListContainer);
                if (slot != null)
                {
                    slot.Initialize(_requestsList[i], i);
                    slot.OnAcceptClicked += OnAcceptRequestClicked;
                    slot.OnRejectClicked += OnRejectRequestClicked;
                    _requestSlots.Add(slot);
                }
            }
        }

        private void PopulateSearchResults()
        {
            ClearSlots(_searchSlots, searchResultsContainer);
            _searchSlots.Clear();

            if (searchInstructions != null) searchInstructions.SetActive(false);

            if (_searchResults == null || _searchResults.Count == 0)
            {
                if (searchEmptyText != null)
                {
                    searchEmptyText.gameObject.SetActive(true);
                    searchEmptyText.text = "No users found";
                }
                if (searchResultsText != null)
                {
                    searchResultsText.text = "0 Results";
                }
                return;
            }

            if (searchEmptyText != null) searchEmptyText.gameObject.SetActive(false);
            if (searchResultsText != null) searchResultsText.text = $"{_searchResults.Count} Results";

            for (int i = 0; i < _searchResults.Count; i++)
            {
                var slot = CreateSlot<IVXFriendSearchSlot>(searchSlotPrefab, searchResultsContainer);
                if (slot != null)
                {
                    slot.Initialize(_searchResults[i], i);
                    slot.OnAddClicked += OnAddFriendClicked;
                    _searchSlots.Add(slot);
                }
            }
        }

        private T CreateSlot<T>(GameObject prefab, Transform container) where T : MonoBehaviour
        {
            if (prefab == null || container == null) return null;

            var go = Instantiate(prefab, container);
            go.SetActive(true);
            return go.GetComponent<T>();
        }

        private void ClearSlots<T>(List<T> slots, Transform container) where T : MonoBehaviour
        {
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            // Also clear any orphaned children
            if (container != null)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    Destroy(container.GetChild(i).gameObject);
                }
            }
        }

        #endregion

        #region Actions

        private void OnSearchButtonClicked()
        {
            if (searchInput == null) return;

            string query = searchInput.text?.Trim();
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                ShowToast("Enter at least 2 characters to search");
                return;
            }

            _ = SearchUsersAsync(query);
        }

        private async Task SearchUsersAsync(string query)
        {
            Debug.Log($"[IVXFriendsPanel] SearchUsersAsync called with query='{query}'");
            
            SetLoading(true, "Searching...");

            try
            {
                Debug.Log($"[IVXFriendsPanel] Calling IVXFriendsService.SearchUsersAsync...");
                _searchResults = await IVXFriendsService.SearchUsersAsync(query, MAX_SEARCH_RESULTS, _cts?.Token ?? default) ?? new List<FriendSearchResult>();
                Debug.Log($"[IVXFriendsPanel] Search returned {_searchResults.Count} results.");
                PopulateSearchResults();
            }
            catch (OperationCanceledException) 
            { 
                Debug.Log("[IVXFriendsPanel] Search was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsPanel] Search error: {ex.GetType().Name} - {ex.Message}");
                Debug.LogError($"[IVXFriendsPanel] Search stack trace: {ex.StackTrace}");
                ShowToast("Search failed", true);
                _searchResults = new List<FriendSearchResult>();
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void OnAddFriendClicked(FriendSearchResult result)
        {
            var slot = _searchSlots.Find(s => s.GetSearchResult()?.userId == result.userId);
            if (slot != null) slot.SetLoadingState(true);

            bool success = await IVXFriendsService.SendFriendRequestAsync(result.userId, null, _cts?.Token ?? default);

            if (slot != null)
            {
                slot.SetLoadingState(false);
                if (success)
                {
                    slot.SetPendingState();
                    slot.PlaySuccessAnimation();
                    ShowToast("Friend request sent!");
                }
            }
        }

        private async void OnAcceptRequestClicked(FriendRequest request)
        {
            var slot = _requestSlots.Find(s => s.GetRequestData()?.requestId == request.requestId);
            if (slot != null) slot.SetLoadingState(true);

            bool success = await IVXFriendsService.AcceptRequestAsync(request.requestId, _cts?.Token ?? default);

            if (success)
            {
                ShowToast($"You are now friends with {request.fromDisplayName}!");
                
                if (slot != null)
                {
                    slot.PlaySuccessAnimation(() =>
                    {
                        slot.AnimateRemoval(() =>
                        {
                            _requestsList.RemoveAll(r => r.requestId == request.requestId);
                            _requestSlots.Remove(slot);
                            Destroy(slot.gameObject);
                            UpdateRequestsBadge();
                        });
                    });
                }

                // Refresh friends list
                _ = LoadFriendsAsync();
            }
            else
            {
                if (slot != null) slot.SetLoadingState(false);
            }
        }

        private async void OnRejectRequestClicked(FriendRequest request)
        {
            var slot = _requestSlots.Find(s => s.GetRequestData()?.requestId == request.requestId);
            if (slot != null) slot.SetLoadingState(true);

            bool success = await IVXFriendsService.RejectRequestAsync(request.requestId, _cts?.Token ?? default);

            if (success)
            {
                if (slot != null)
                {
                    slot.AnimateRemoval(() =>
                    {
                        _requestsList.RemoveAll(r => r.requestId == request.requestId);
                        _requestSlots.Remove(slot);
                        Destroy(slot.gameObject);
                        UpdateRequestsBadge();
                    });
                }
            }
            else
            {
                if (slot != null) slot.SetLoadingState(false);
            }
        }

        private void OnRemoveFriendClicked(FriendInfo friend)
        {
            ShowConfirmDialog(
                "Remove Friend",
                $"Are you sure you want to remove {friend.displayName} from your friends?",
                async () =>
                {
                    var slot = _friendSlots.Find(s => s.GetFriendData()?.userId == friend.userId);
                    
                    bool success = await IVXFriendsService.RemoveFriendAsync(friend.userId, _cts?.Token ?? default);

                    if (success)
                    {
                        ShowToast($"{friend.displayName} removed from friends");
                        
                        if (slot != null)
                        {
                            slot.AnimateRemoval(() =>
                            {
                                _friendsList.RemoveAll(f => f.userId == friend.userId);
                                _friendSlots.Remove(slot);
                                Destroy(slot.gameObject);
                                
                                if (friendsCountText != null)
                                    friendsCountText.text = $"{_friendsList.Count} Friends";
                            });
                        }
                    }
                }
            );
        }

        private void OnBlockFriendClicked(FriendInfo friend)
        {
            ShowConfirmDialog(
                "Block User",
                $"Are you sure you want to block {friend.displayName}? They will be removed from your friends and won't be able to contact you.",
                async () =>
                {
                    var slot = _friendSlots.Find(s => s.GetFriendData()?.userId == friend.userId);
                    
                    bool success = await IVXFriendsService.BlockUserAsync(friend.userId, null, _cts?.Token ?? default);

                    if (success)
                    {
                        ShowToast($"{friend.displayName} has been blocked");
                        
                        if (slot != null)
                        {
                            slot.AnimateRemoval(() =>
                            {
                                _friendsList.RemoveAll(f => f.userId == friend.userId);
                                _friendSlots.Remove(slot);
                                Destroy(slot.gameObject);
                                
                                if (friendsCountText != null)
                                    friendsCountText.text = $"{_friendsList.Count} Friends";
                            });
                        }
                    }
                }
            );
        }

        private void OnFriendProfileClicked(FriendInfo friend)
        {
            OnFriendSelected?.Invoke(friend);
        }

        #endregion

        #region UI Helpers

        private void SetLoading(bool isLoading, string message = null)
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(isLoading);
            }

            if (loadingText != null && message != null)
            {
                loadingText.text = message;
            }

            if (loadingSpinner != null)
            {
                if (isLoading)
                {
                    IVXFriendsAnimations.StartSpinnerRotation(loadingSpinner);
                }
                else
                {
                    IVXFriendsAnimations.StopSpinnerRotation(loadingSpinner);
                }
            }
        }

        private void HideToast()
        {
            if (toastPanel != null)
            {
                toastPanel.SetActive(false);
            }
        }

        private void ShowConfirmDialog(string title, string message, Action onConfirm)
        {
            if (confirmDialog == null) return;

            _pendingConfirmAction = onConfirm;

            if (confirmTitleText != null) confirmTitleText.text = title;
            if (confirmMessageText != null) confirmMessageText.text = message;

            confirmDialog.SetActive(true);
        }

        private void OnConfirmYes()
        {
            if (confirmDialog != null) confirmDialog.SetActive(false);
            _pendingConfirmAction?.Invoke();
            _pendingConfirmAction = null;
        }

        private void OnConfirmNo()
        {
            if (confirmDialog != null) confirmDialog.SetActive(false);
            _pendingConfirmAction = null;
        }

        #endregion
    }
}
