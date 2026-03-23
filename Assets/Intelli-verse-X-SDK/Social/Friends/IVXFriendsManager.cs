// ============================================================================
// IVXFriendsManager.cs - IntelliVerse-X Friends System (100% Nakama Native)
// ============================================================================
// PRODUCTION-READY | NO HTTP | NO API MANAGER
//
// Main service layer for friends using Nakama built-in APIs only:
// AddFriendsAsync, DeleteFriendsAsync, BlockFriendsAsync, ListFriendsAsync.
// Reusable across: IVX_Friends scene, Lobby, Multiplayer, Profile, any mode.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Main Friends service - 100% Nakama native. No HTTP, no APIManager.
    /// Initialize with IVXNManager.Client, Session, and optional Socket for realtime.
    /// </summary>
    public class IVXFriendsManager : MonoBehaviour
    {
        public static IVXFriendsManager Instance { get; private set; }

        private IClient _client;
        private ISession _session;
        private ISocket _socket;
        private IVXFriendsCache _cache;
        private const int DEFAULT_LIST_LIMIT = 100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _cache = new IVXFriendsCache();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Initialize with Nakama client, session, and optional socket.
        /// Call after IVXNManager is ready.
        /// </summary>
        public void Initialize(IClient client, ISession session, ISocket socket = null)
        {
            _client = client;
            _session = session;
            _socket = socket;
            if (_socket != null)
                SetupRealtimeListeners();
        }

        /// <summary>
        /// Initialize from IVXNManager.Instance.
        /// </summary>
        public bool InitializeFromNakamaManager()
        {
            var mgr = FindNakamaManager();
            if (mgr == null) return false;
            var t = mgr.GetType();
            var client = t.GetProperty("Client")?.GetValue(mgr) as IClient;
            var session = t.GetProperty("Session")?.GetValue(mgr) as ISession;
            if (client == null || session == null) return false;
            var socket = t.GetProperty("Socket")?.GetValue(mgr) as ISocket;
            Initialize(client, session, socket);
            return true;
        }

        private static object FindNakamaManager()
        {
            var t = Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, IntelliVerseX.V2");
            if (t == null) return null;
            var instanceProp = t.GetProperty("Instance", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return instanceProp?.GetValue(null);
        }

        private void SetupRealtimeListeners()
        {
            if (_socket == null) return;
            try
            {
                _socket.ReceivedStatusPresence += presence =>
                {
                    string userId = null;
                    var join = presence?.Joins?.FirstOrDefault();
                    var leave = presence?.Leaves?.FirstOrDefault();
                    userId = join?.UserId ?? leave?.UserId;
                    IVXFriendsEvents.RaiseFriendPresenceChanged(userId);
                };
                _socket.ReceivedNotification += notification =>
                {
                    if (notification?.Subject == "friend_request" && !string.IsNullOrEmpty(notification.Content))
                    {
                        IVXFriendsEvents.RaiseFriendRequestReceived(notification.Content);
                        OnFriendRequestReceived?.Invoke(notification.Content);
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXFriends] SetupRealtimeListeners: {ex.Message}");
            }
        }

        private void EnsureSession()
        {
            if (_session == null || _session.IsExpired)
                throw new InvalidOperationException("Nakama session is not ready or expired. Login first.");
        }

        #region Friend Operations

        /// <summary>
        /// Add friend by user ID (send request) or accept request (mutual add).
        /// </summary>
        public async Task AddFriendByIdAsync(string userId, CancellationToken ct = default)
        {
            if (!IVXFriendsValidator.CanAddFriend(userId, _session?.UserId, out var error))
                throw new ArgumentException(error ?? "Invalid user ID");

            EnsureSession();

            try
            {
                await _client.AddFriendsAsync(_session, new[] { userId }, null);
                IVXFriendsEvents.RaiseFriendAdded(userId);
                await RefreshFriendsAsync(ct);
            }
            catch (ApiResponseException ex)
            {
                IVXFriendsEvents.RaiseFriendsError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Remove friend or decline friend request.
        /// </summary>
        public async Task RemoveFriendAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required");

            EnsureSession();

            try
            {
                await _client.DeleteFriendsAsync(_session, new[] { userId }, null);
                IVXFriendsEvents.RaiseFriendRemoved(userId);
                await RefreshFriendsAsync(ct);
            }
            catch (ApiResponseException ex)
            {
                IVXFriendsEvents.RaiseFriendsError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Block a user.
        /// </summary>
        public async Task BlockFriendAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required");

            EnsureSession();

            try
            {
                await _client.BlockFriendsAsync(_session, new[] { userId }, null);
                IVXFriendsEvents.RaiseFriendRemoved(userId);
                await RefreshFriendsAsync(ct);
            }
            catch (ApiResponseException ex)
            {
                IVXFriendsEvents.RaiseFriendsError(ex.Message);
                throw;
            }
        }

        /// <summary>Convenience alias for BlockFriendAsync.</summary>
        public Task BlockFriend(string userId, CancellationToken ct = default) => BlockFriendAsync(userId, ct);

        /// <summary>Convenience alias for RemoveFriendAsync.</summary>
        public Task RemoveFriend(string userId, CancellationToken ct = default) => RemoveFriendAsync(userId, ct);

        #endregion

        #region List & Refresh

        /// <summary>
        /// Refresh friends list and cache. Use state: 0=FRIEND, 1=INVITE_SENT, 2=INVITE_RECEIVED, 3=BLOCKED.
        /// Invokes OnFriendsUpdated when complete.
        /// </summary>
        public async Task RefreshFriendsAsync(CancellationToken ct = default)
        {
            EnsureSession();

            try
            {
                var result = await _client.ListFriendsAsync(_session, 0, DEFAULT_LIST_LIMIT, null, null, ct);
                var list = (result?.Friends ?? Enumerable.Empty<IApiFriend>()).ToList();
                _cache.Update(list);
                IVXFriendsEvents.RaiseFriendListChanged();
                OnFriendsUpdated?.Invoke(list);
            }
            catch (ApiResponseException ex)
            {
                IVXFriendsEvents.RaiseFriendsError(ex.Message);
                throw;
            }
        }

        /// <summary>Convenience alias for RefreshFriendsAsync.</summary>
        public Task RefreshFriends(CancellationToken ct = default) => RefreshFriendsAsync(ct);

        /// <summary>
        /// Get confirmed friends (state 0).
        /// </summary>
        public async Task<IReadOnlyList<IApiFriend>> GetFriendsAsync(CancellationToken ct = default)
        {
            EnsureSession();
            var result = await _client.ListFriendsAsync(_session, 0, DEFAULT_LIST_LIMIT, null, null, ct);
            var list = (result?.Friends ?? Enumerable.Empty<IApiFriend>()).ToList();
            _cache.Update(list);
            return list;
        }

        /// <summary>
        /// Get incoming friend requests (state 2 = INVITE_RECEIVED).
        /// </summary>
        public async Task<IReadOnlyList<IApiFriend>> GetPendingRequestsAsync(CancellationToken ct = default)
        {
            EnsureSession();
            var result = await _client.ListFriendsAsync(_session, 2, DEFAULT_LIST_LIMIT, null, null, ct);
            return (result?.Friends ?? Enumerable.Empty<IApiFriend>()).ToList();
        }

        /// <summary>
        /// Get blocked users (state 3).
        /// </summary>
        public async Task<IReadOnlyList<IApiFriend>> GetBlockedUsersAsync(CancellationToken ct = default)
        {
            EnsureSession();
            var result = await _client.ListFriendsAsync(_session, 3, DEFAULT_LIST_LIMIT, null, null, ct);
            return (result?.Friends ?? Enumerable.Empty<IApiFriend>()).ToList();
        }

        #endregion

        #region Search

        /// <summary>
        /// Search users by exact username. Returns list for UI compatibility.
        /// Nakama native GetUsersAsync - exact match only; partial search requires RPC.
        /// </summary>
        public async Task<IReadOnlyList<IApiUser>> SearchUsersAsync(string username, CancellationToken ct = default)
        {
            var user = await SearchUserByUsernameAsync(username, ct);
            if (user == null) return new List<IApiUser>();
            return new List<IApiUser> { user };
        }

        /// <summary>
        /// Search user by exact username. Nakama native - no RPC.
        /// Partial search would require backend RPC.
        /// </summary>
        public async Task<IApiUser> SearchUserByUsernameAsync(string username, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 2)
                return null;

            EnsureSession();

            try
            {
                var result = await _client.GetUsersAsync(_session, null, new[] { username });
                if (result?.Users != null && result.Users.Any())
                    return result.Users.FirstOrDefault();
                return null;
            }
            catch (ApiResponseException)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if user is already friend or has pending request.
        /// </summary>
        public async Task<bool> IsFriendOrPendingAsync(string userId, CancellationToken ct = default)
        {
            EnsureSession();
            var all = await _client.ListFriendsAsync(_session, 0, DEFAULT_LIST_LIMIT, null, null, ct);
            if (all?.Friends == null) return false;
            foreach (var f in all.Friends)
            {
                if (f?.User?.Id == userId) return true;
            }
            var pending = await _client.ListFriendsAsync(_session, 2, DEFAULT_LIST_LIMIT, null, null, ct);
            if (pending?.Friends == null) return false;
            foreach (var f in pending.Friends)
            {
                if (f?.User?.Id == userId) return true;
            }
            return false;
        }

        #endregion

        #region Cache & Events

        public IVXFriendsCache Cache => _cache;

        public event Action<IReadOnlyList<IApiFriend>> OnFriendsUpdated;
        public event Action<string> OnFriendRequestReceived;

        #endregion
    }
}
