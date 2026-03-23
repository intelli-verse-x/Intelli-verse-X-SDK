// ============================================================================
// IVXFriendsSceneController.cs - Connects UI → IVXFriendsManager
// ============================================================================
// Scene root: IVX_FriendsBootstrap with IVXFriendsManager + IVXFriendsSceneController.
// EventSystem uses InputSystemUIInputModule (NOT StandaloneInputModule).
// 
// If IVXFriendsPanel exists in scene, this controller delegates UI to it.
// Otherwise, uses its own simple UI for backward compatibility.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Scene controller for IVX_Friends scene. Initializes IVXFriendsManager and coordinates UI.
    /// If IVXFriendsPanel exists in scene, delegates UI management to it.
    /// Otherwise, provides simple fallback UI functionality.
    /// </summary>
    public class IVXFriendsSceneController : MonoBehaviour
    {
        [Header("Simple UI (used if IVXFriendsPanel not present)")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;

        [Header("Friends List")]
        [SerializeField] private Transform friendsListRoot;
        [SerializeField] private GameObject friendItemPrefab;

        [Header("Pending List")]
        [SerializeField] private Transform pendingListRoot;
        [SerializeField] private GameObject pendingItemPrefab;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject loadingOverlay;

        private bool _useSimpleUI;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            
            // Check if IVXFriendsPanel exists - if so, it handles all UI
            var panelInstance = IVXFriendsPanel.Instance;
            _useSimpleUI = panelInstance == null;
            
            if (_useSimpleUI)
            {
                WireReferencesIfNeeded();
            }
        }

        private void WireReferencesIfNeeded()
        {
            // Wire Search components
            if (searchInput == null)
            {
                var go = GameObject.Find("SearchInput");
                if (go != null) searchInput = go.GetComponent<TMP_InputField>();
            }
            if (searchButton == null)
            {
                var go = GameObject.Find("SearchButton");
                if (go != null) searchButton = go.GetComponent<Button>();
            }

            // Wire Friends List - look for FriendsList first, then FriendsContent
            if (friendsListRoot == null)
            {
                var go = GameObject.Find("FriendsList");
                if (go == null) go = GameObject.Find("FriendsContent");
                if (go != null) friendsListRoot = go.transform;
            }

            // Wire Pending List - look for RequestsList first, then RequestsContent
            if (pendingListRoot == null)
            {
                var go = GameObject.Find("RequestsList");
                if (go == null) go = GameObject.Find("PendingContent");
                if (go == null) go = GameObject.Find("RequestsContent");
                if (go != null) pendingListRoot = go.transform;
            }

            // Wire Status Text - look for StatusText or FriendsEmptyText as fallback
            if (statusText == null)
            {
                var go = GameObject.Find("StatusText");
                if (go == null) go = GameObject.Find("FriendsEmptyText");
                if (go != null) statusText = go.GetComponent<TextMeshProUGUI>();
            }

            // Wire Loading Overlay
            if (loadingOverlay == null)
            {
                loadingOverlay = GameObject.Find("LoadingOverlay");
            }

            // Load prefabs from Resources
            if (friendItemPrefab == null)
                friendItemPrefab = Resources.Load<GameObject>("IntelliVerseX/Social/IVXFriendSlot");
            if (pendingItemPrefab == null)
                pendingItemPrefab = Resources.Load<GameObject>("IntelliVerseX/Social/IVXFriendRequestSlot");
        }

        private void Start()
        {
            // Only wire button events if using simple UI mode
            if (_useSimpleUI)
            {
                if (searchButton != null)
                    searchButton.onClick.AddListener(OnSearchClicked);
            }

            // Subscribe to IVXFriendsManager events (always)
            if (IVXFriendsManager.Instance != null)
            {
                IVXFriendsManager.Instance.OnFriendsUpdated += PopulateFriends;
                IVXFriendsManager.Instance.OnFriendRequestReceived += OnFriendRequestReceived;
            }

            // Subscribe to global events
            IVXFriendsEvents.OnFriendListChanged += OnFriendListChanged;
            IVXFriendsEvents.OnFriendRequestReceived += OnFriendRequestReceivedEvent;

            // Initialize manager (both modes need this)
            InitializeManagerAsync();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (IVXFriendsManager.Instance != null)
            {
                IVXFriendsManager.Instance.OnFriendsUpdated -= PopulateFriends;
                IVXFriendsManager.Instance.OnFriendRequestReceived -= OnFriendRequestReceived;
            }
            IVXFriendsEvents.OnFriendListChanged -= OnFriendListChanged;
            IVXFriendsEvents.OnFriendRequestReceived -= OnFriendRequestReceivedEvent;
        }

        private async void InitializeManagerAsync()
        {
            // Only show loading UI in simple mode
            if (_useSimpleUI)
            {
                SetLoading(true);
                SetStatus("Initializing...");
            }

            try
            {
                var mgr = IVXFriendsManager.Instance;
                if (mgr == null)
                {
                    var go = new GameObject("IVXFriendsManager");
                    mgr = go.AddComponent<IVXFriendsManager>();
                    DontDestroyOnLoad(go);
                }

                bool initialized = mgr.InitializeFromNakamaManager();
                
                if (!initialized)
                {
                    // Try using IVXFriendsService which has auto-initialization
                    Debug.Log("[IVXFriendsScene] Manager not initialized directly, using IVXFriendsService for auto-init...");
                    try
                    {
                        // This will trigger auto-initialization in IVXFriendsService
                        await IVXFriendsService.GetFriendsAsync(_cts?.Token ?? default);
                        initialized = true;
                        Debug.Log("[IVXFriendsScene] Auto-initialization via IVXFriendsService succeeded.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[IVXFriendsScene] Auto-init via service failed: {ex.Message}");
                    }
                }

                if (!initialized)
                {
                    if (_useSimpleUI) SetStatus("Nakama not ready. Please login first.");
                    return;
                }

                // Only refresh in simple UI mode (IVXFriendsPanel handles its own data loading)
                if (_useSimpleUI)
                {
                    await mgr.RefreshFriends();
                    RefreshPendingList();
                    SetStatus("Ready");
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (Exception ex)
            {
                if (_useSimpleUI) SetStatus($"Error: {ex.Message}");
                Debug.LogError($"[IVXFriendsScene] Initialize: {ex.Message}");
            }
            finally
            {
                if (_useSimpleUI) SetLoading(false);
            }
        }

        private async void OnSearchClicked()
        {
            if (searchInput == null || string.IsNullOrWhiteSpace(searchInput.text))
                return;

            SetLoading(true);
            try
            {
                var mgr = IVXFriendsManager.Instance;
                if (mgr == null) return;

                var users = await mgr.SearchUsersAsync(searchInput.text.Trim());
                SetStatus($"Found {users?.Count ?? 0} user(s)");
                if (users != null && users.Count > 0 && friendItemPrefab != null && friendsListRoot != null)
                {
                    foreach (var u in users)
                    {
                        var go = Instantiate(friendItemPrefab, friendsListRoot);
                        var itemUI = go.GetComponent<IVXFriendItemUI>();
                        if (itemUI != null)
                            itemUI.SetupFromUser(u);
                        else
                        {
                            var slot = go.GetComponent<IVXFriendSearchSlot>();
                            if (slot != null)
                            {
                                var sr = new FriendSearchResult { userId = u.Id, displayName = u.DisplayName ?? u.Username ?? "Unknown", avatarUrl = u.AvatarUrl, alreadyFriend = false, requestPending = false };
                                slot.Initialize(sr);
                                slot.OnAddClicked += r => { var m = IVXFriendsManager.Instance; if (m != null) m.AddFriendByIdAsync(r.userId).ContinueWith(_ => { if (slot != null) slot.SetPendingState(); }); };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Search failed: {ex.Message}");
                Debug.LogError($"[IVXFriendsScene] Search: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void PopulateFriends(IReadOnlyList<IApiFriend> friends)
        {
            if (friendsListRoot == null || friendItemPrefab == null) return;

            foreach (Transform child in friendsListRoot)
                Destroy(child.gameObject);

            if (friends == null || friends.Count == 0)
                return;

            foreach (var friend in friends)
            {
                var go = Instantiate(friendItemPrefab, friendsListRoot);
                var itemUI = go.GetComponent<IVXFriendItemUI>();
                if (itemUI != null)
                    itemUI.Setup(friend);
                else
                {
                    var slot = go.GetComponent<IVXFriendSlot>();
                    if (slot != null)
                    {
                        slot.Initialize(ToFriendInfo(friend));
                        slot.OnRemoveClicked += f => IVXFriendsManager.Instance?.RemoveFriend(f.userId);
                        slot.OnBlockClicked += f => IVXFriendsManager.Instance?.BlockFriend(f.userId);
                    }
                }
            }
        }

        private static FriendInfo ToFriendInfo(IApiFriend f)
        {
            if (f?.User == null) return null;
            var u = f.User;
            return new FriendInfo { userId = u.Id, displayName = u.DisplayName ?? u.Username ?? "Unknown", avatarUrl = u.AvatarUrl };
        }

        private static FriendRequest ToFriendRequest(IApiFriend f)
        {
            if (f?.User == null) return null;
            var u = f.User;
            return new FriendRequest { requestId = u.Id, fromUserId = u.Id, fromDisplayName = u.DisplayName ?? u.Username ?? "Unknown", fromAvatarUrl = u.AvatarUrl };
        }

        private async void RefreshPendingList()
        {
            var mgr = IVXFriendsManager.Instance;
            if (mgr == null || pendingListRoot == null || pendingItemPrefab == null) return;

            foreach (Transform child in pendingListRoot)
                Destroy(child.gameObject);

            try
            {
                var pending = await mgr.GetPendingRequestsAsync();
                if (pending == null || pending.Count == 0) return;

                foreach (var req in pending)
                {
                    var go = Instantiate(pendingItemPrefab, pendingListRoot);
                    var itemUI = go.GetComponent<IVXPendingRequestItemUI>();
                    if (itemUI != null)
                        itemUI.Setup(req);
                    else
                    {
                        var slot = go.GetComponent<IVXFriendRequestSlot>();
                        if (slot != null)
                        {
                            slot.Initialize(ToFriendRequest(req));
                            slot.OnAcceptClicked += r => IVXFriendsManager.Instance?.AddFriendByIdAsync(r.requestId);
                            slot.OnRejectClicked += r => IVXFriendsManager.Instance?.RemoveFriendAsync(r.requestId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXFriendsScene] RefreshPending: {ex.Message}");
            }
        }

        private void OnFriendListChanged()
        {
            var mgr = IVXFriendsManager.Instance;
            if (mgr != null)
                mgr.RefreshFriends().ContinueWith(_ => { });
        }

        private void OnFriendRequestReceived(string content)
        {
            SetStatus("New friend request!");
            RefreshPendingList();
        }

        private void OnFriendRequestReceivedEvent(string content)
        {
            OnFriendRequestReceived(content);
        }

        private void SetLoading(bool loading)
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(loading);
        }

        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }
    }
}
