// ============================================================================
// IVXFriendsService.cs - IntelliVerse-X Friends API Service (100% Nakama)
// ============================================================================
// PRODUCTION-READY | NO HTTP | NO API MANAGER
//
// Facade over IVXFriendsManager. Uses Nakama native APIs only.
// Converts IApiFriend/IApiUser to FriendInfo, FriendRequest, FriendSearchResult
// for UI compatibility.
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
    /// Static API client for Friends. Delegates to IVXFriendsManager (Nakama native).
    /// </summary>
    public static class IVXFriendsService
    {
        #region Events

        public static event Action<List<FriendInfo>> OnFriendsListUpdated;
        public static event Action<List<FriendRequest>> OnRequestsListUpdated;
        public static event Action<FriendRequest> OnNewRequestReceived;
        public static event Action<FriendInfo> OnFriendAdded;
        public static event Action<string> OnFriendRemoved;
        public static event Action<string> OnError;

        #endregion

        private const string LOG_TAG = "[IVXFriends]";
        private static bool _nakamaInitialized = false;

        /// <summary>
        /// Ensures Nakama is initialized before using friend operations.
        /// Call this once at startup or before first friend operation.
        /// </summary>
        public static async Task<bool> EnsureNakamaInitializedAsync()
        {
            if (_nakamaInitialized)
                return true;

            try
            {
                // Find IVXNManager via reflection
                var mgrType = Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, IntelliVerseX.V2");
                if (mgrType == null)
                {
                    LogError("IVXNManager type not found. Ensure IntelliVerseX.Backend assembly is referenced.");
                    return false;
                }

                var instanceProp = mgrType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var mgr = instanceProp?.GetValue(null);

                if (mgr == null)
                {
                    // Bootstrap IVXNManager if it doesn't exist
                    if (typeof(MonoBehaviour).IsAssignableFrom(mgrType))
                    {
                        var bootstrapGo = new GameObject("IVXNManager");
                        UnityEngine.Object.DontDestroyOnLoad(bootstrapGo);
                        bootstrapGo.AddComponent(mgrType);
                        await Task.Yield();
                        mgr = instanceProp?.GetValue(null);
                    }

                    if (mgr == null)
                    {
                        LogError("Failed to create IVXNManager instance.");
                        return false;
                    }
                }

                // Check if session is valid
                var sessionProp = mgrType.GetProperty("Session");
                var session = sessionProp?.GetValue(mgr) as ISession;
                
                if (session == null || session.IsExpired)
                {
                    Log("Nakama session not ready. Initializing...");
                    
                    // Call InitializeForCurrentUserAsync
                    var initMethod = mgrType.GetMethod(
                        "InitializeForCurrentUserAsync",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        new[] { typeof(bool) },
                        null);
                    
                    if (initMethod != null)
                    {
                        var task = initMethod.Invoke(mgr, new object[] { false }) as Task<bool>;
                        if (task != null)
                        {
                            bool success = await task;
                            if (success)
                            {
                                Log("Nakama initialized successfully.");
                                _nakamaInitialized = true;
                                return true;
                            }
                            else
                            {
                                LogError("InitializeForCurrentUserAsync returned false.");
                                return false;
                            }
                        }
                    }
                    
                    // Fallback for older API without bool parameter
                    initMethod = mgrType.GetMethod(
                        "InitializeForCurrentUserAsync",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        Type.EmptyTypes,
                        null);
                    
                    if (initMethod != null)
                    {
                        var task = initMethod.Invoke(mgr, null) as Task<bool>;
                        if (task != null)
                        {
                            bool success = await task;
                            _nakamaInitialized = success;
                            if (!success) LogError("InitializeForCurrentUserAsync returned false.");
                            return success;
                        }
                    }
                    
                    LogError("InitializeForCurrentUserAsync method not found.");
                    return false;
                }

                _nakamaInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                LogError($"EnsureNakamaInitializedAsync: {ex.Message}");
                return false;
            }
        }

        private static IVXFriendsManager GetManager()
        {
            var mgr = IVXFriendsManager.Instance;
            if (mgr == null)
            {
                var go = new GameObject("IVXFriendsManager");
                mgr = go.AddComponent<IVXFriendsManager>();
                if (!mgr.InitializeFromNakamaManager())
                    return null;
            }
            return mgr;
        }

        private static async Task<IVXFriendsManager> GetManagerAsync()
        {
            // Ensure Nakama is initialized first
            if (!await EnsureNakamaInitializedAsync())
            {
                return null;
            }
            return GetManager();
        }

        public static async Task<List<FriendInfo>> GetFriendsAsync(CancellationToken ct = default)
        {
            var mgr = await GetManagerAsync();
            if (mgr == null)
            {
                LogWarning("Nakama not initialized. Initialize IVXNManager first.");
                return new List<FriendInfo>();
            }
            try
            {
                var list = await mgr.GetFriendsAsync(ct);
                var friends = ConvertToFriendInfoList(list);
                OnFriendsListUpdated?.Invoke(friends);
                return friends;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"GetFriends: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return new List<FriendInfo>();
            }
        }

        public static async Task<List<FriendRequest>> GetIncomingRequestsAsync(CancellationToken ct = default)
        {
            var mgr = await GetManagerAsync();
            if (mgr == null)
            {
                LogWarning("Nakama not initialized.");
                return new List<FriendRequest>();
            }
            try
            {
                var list = await mgr.GetPendingRequestsAsync(ct);
                var requests = ConvertToFriendRequestList(list);
                OnRequestsListUpdated?.Invoke(requests);
                return requests;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"GetIncomingRequests: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return new List<FriendRequest>();
            }
        }

        public static async Task<List<FriendSearchResult>> SearchUsersAsync(
            string query, int limit = 20, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<FriendSearchResult>();

            var mgr = await GetManagerAsync();
            if (mgr == null)
            {
                LogWarning("Nakama not initialized.");
                return new List<FriendSearchResult>();
            }
            try
            {
                var user = await mgr.SearchUserByUsernameAsync(query.Trim(), ct);
                if (user == null) return new List<FriendSearchResult>();
                var friends = await mgr.GetFriendsAsync(ct);
                var pending = await mgr.GetPendingRequestsAsync(ct);
                bool isFriend = friends.Any(f => f?.User?.Id == user.Id);
                bool isPending = pending.Any(f => f?.User?.Id == user.Id);
                return new List<FriendSearchResult>
                {
                    new FriendSearchResult
                    {
                        userId = user.Id,
                        displayName = user.DisplayName ?? user.Username ?? "Unknown",
                        avatarUrl = user.AvatarUrl,
                        alreadyFriend = isFriend,
                        requestPending = isPending
                    }
                };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"SearchUsers: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return new List<FriendSearchResult>();
            }
        }

        public static async Task<bool> SendFriendRequestAsync(string targetUserId, string message = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                LogError("targetUserId is required.");
                return false;
            }
            var mgr = await GetManagerAsync();
            if (mgr == null) return false;
            try
            {
                await mgr.AddFriendByIdAsync(targetUserId, ct);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"SendFriendRequest: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Accept friend request. requestId = fromUserId (initiator) in Nakama.
        /// </summary>
        public static async Task<bool> AcceptRequestAsync(string requestId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                LogError("requestId (fromUserId) is required.");
                return false;
            }
            var mgr = await GetManagerAsync();
            if (mgr == null) return false;
            try
            {
                await mgr.AddFriendByIdAsync(requestId, ct);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"AcceptRequest: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        public static async Task<bool> RejectRequestAsync(string requestId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                LogError("requestId (fromUserId) is required.");
                return false;
            }
            var mgr = await GetManagerAsync();
            if (mgr == null) return false;
            try
            {
                await mgr.RemoveFriendAsync(requestId, ct);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"RejectRequest: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        public static async Task<bool> RemoveFriendAsync(string friendUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(friendUserId))
            {
                LogError("friendUserId is required.");
                return false;
            }
            var mgr = await GetManagerAsync();
            if (mgr == null) return false;
            try
            {
                await mgr.RemoveFriendAsync(friendUserId, ct);
                OnFriendRemoved?.Invoke(friendUserId);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"RemoveFriend: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        public static async Task<bool> BlockUserAsync(string userId, string reason = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;
            var mgr = await GetManagerAsync();
            if (mgr == null) return false;
            try
            {
                await mgr.BlockFriendAsync(userId, ct);
                OnFriendRemoved?.Invoke(userId);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LogError($"BlockUser: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        private static List<FriendInfo> ConvertToFriendInfoList(IEnumerable<IApiFriend> list)
        {
            var result = new List<FriendInfo>();
            if (list == null) return result;
            foreach (var f in list)
            {
                if (f?.User == null) continue;
                var u = f.User;
                result.Add(new FriendInfo
                {
                    userId = u.Id,
                    displayName = u.DisplayName ?? u.Username ?? "Unknown",
                    avatarUrl = u.AvatarUrl,
                    isOnline = false,
                    lastSeenEpoch = 0
                });
            }
            return result;
        }

        private static List<FriendRequest> ConvertToFriendRequestList(IEnumerable<IApiFriend> list)
        {
            var result = new List<FriendRequest>();
            if (list == null) return result;
            foreach (var f in list)
            {
                if (f?.User == null) continue;
                var u = f.User;
                result.Add(new FriendRequest
                {
                    requestId = u.Id,
                    fromUserId = u.Id,
                    fromDisplayName = u.DisplayName ?? u.Username ?? "Unknown",
                    fromAvatarUrl = u.AvatarUrl,
                    sentAtEpoch = 0
                });
            }
            return result;
        }

        private static void Log(string msg) => Debug.Log($"{LOG_TAG} {msg}");
        private static void LogWarning(string msg) => Debug.LogWarning($"{LOG_TAG} {msg}");
        private static void LogError(string msg) => Debug.LogError($"{LOG_TAG} {msg}");
    }
}
