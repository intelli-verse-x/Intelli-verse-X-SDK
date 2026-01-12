// ============================================================================
// IVXFriendsService.cs - IntelliVerse-X Friends API Service
// ============================================================================
// PRODUCTION-READY | HIGH-PERFORMANCE | ENTERPRISE-GRADE
//
// Thin wrapper over APIManager for Friends functionality.
// Uses centralized APIManager for all HTTP calls - no duplicate HTTP code.
// Provides events and convenience methods for the Friends UI.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Static API client for the Friends system.
    /// Wraps APIManager's Friends methods with events and caching.
    /// 
    /// Architecture:
    /// - ALL HTTP calls go through APIManager (centralized)
    /// - This service adds: events, caching, convenience methods
    /// - Thread-safe, async-first design
    /// 
    /// Usage:
    ///   var friends = await IVXFriendsService.GetFriendsAsync();
    ///   await IVXFriendsService.SendFriendRequestAsync("user123");
    /// </summary>
    public static class IVXFriendsService
    {
        #region Events

        /// <summary>Fired when the friends list is updated.</summary>
        public static event Action<List<FriendInfo>> OnFriendsListUpdated;

        /// <summary>Fired when the requests list is updated.</summary>
        public static event Action<List<FriendRequest>> OnRequestsListUpdated;

        /// <summary>Fired when a new friend request is received.</summary>
        public static event Action<FriendRequest> OnNewRequestReceived;

        /// <summary>Fired when a friend is added (request accepted).</summary>
        public static event Action<FriendInfo> OnFriendAdded;

        /// <summary>Fired when a friend is removed.</summary>
        public static event Action<string> OnFriendRemoved;

        /// <summary>Fired on any API error.</summary>
        public static event Action<string> OnError;

        #endregion

        #region Configuration

        private static IVXFriendsConfig Config => IVXFriendsConfig.Instance;
        private const string LOG_TAG = "[IVXFriends]";

        #endregion

        #region Public API Methods

        /// <summary>
        /// Gets the current user's friends list.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of friends, or empty list on error.</returns>
        public static async Task<List<FriendInfo>> GetFriendsAsync(CancellationToken ct = default)
        {
            Log("GetFriendsAsync called");
            
            try
            {
                // Check session first with detailed logging
                var session = UserSessionManager.Current;
                if (session == null)
                {
                    LogWarning("GetFriends: Session is NULL. User must login first.");
                    return new List<FriendInfo>();
                }
                
                Log($"GetFriends: Session found. userId={session.userId ?? "NULL"}, " +
                    $"accessToken={(string.IsNullOrEmpty(session.accessToken) ? "EMPTY" : "present")}");
                
                if (string.IsNullOrEmpty(session.accessToken))
                {
                    LogWarning("GetFriends: Session exists but accessToken is EMPTY. User must re-login.");
                    return new List<FriendInfo>();
                }
                
                Log("GetFriends: Calling APIManager.GetAcceptedFriendsAsync...");
                var data = await APIManager.GetAcceptedFriendsAsync(ct);
                Log($"GetFriends: APIManager returned {data?.Count ?? 0} results.");
                
                var friends = ConvertToFriendInfoList(data);
                Log($"GetFriends: Converted to {friends.Count} FriendInfo items.");
                OnFriendsListUpdated?.Invoke(friends);
                return friends;
            }
            catch (OperationCanceledException)
            {
                Log("GetFriends: Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                LogError($"GetFriends exception: {ex.GetType().Name} - {ex.Message}");
                LogError($"GetFriends stack trace: {ex.StackTrace}");
                OnError?.Invoke(ex.Message);
                return new List<FriendInfo>();
            }
        }

        /// <summary>
        /// Gets incoming friend requests.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of pending friend requests.</returns>
        public static async Task<List<FriendRequest>> GetIncomingRequestsAsync(CancellationToken ct = default)
        {
            Log("GetIncomingRequestsAsync called");
            
            try
            {
                // Check session first with detailed logging
                var session = UserSessionManager.Current;
                if (session == null)
                {
                    LogWarning("GetIncomingRequests: Session is NULL. User must login first.");
                    return new List<FriendRequest>();
                }
                
                Log($"GetIncomingRequests: Session found. userId={session.userId ?? "NULL"}, " +
                    $"accessToken={(string.IsNullOrEmpty(session.accessToken) ? "EMPTY" : "present")}");
                
                if (string.IsNullOrEmpty(session.accessToken))
                {
                    LogWarning("GetIncomingRequests: Session exists but accessToken is EMPTY. User must re-login.");
                    return new List<FriendRequest>();
                }
                
                Log("GetIncomingRequests: Calling APIManager.GetIncomingRequestsAsync...");
                var data = await APIManager.GetIncomingRequestsAsync(ct);
                Log($"GetIncomingRequests: APIManager returned {data?.Count ?? 0} results.");
                
                var requests = ConvertToFriendRequestList(data);
                Log($"GetIncomingRequests: Converted to {requests.Count} FriendRequest items.");
                OnRequestsListUpdated?.Invoke(requests);
                return requests;
            }
            catch (OperationCanceledException)
            {
                Log("GetIncomingRequests: Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                LogError($"GetIncomingRequests exception: {ex.GetType().Name} - {ex.Message}");
                LogError($"GetIncomingRequests stack trace: {ex.StackTrace}");
                OnError?.Invoke(ex.Message);
                return new List<FriendRequest>();
            }
        }

        /// <summary>
        /// Searches for users by display name or username.
        /// </summary>
        /// <param name="query">Search query (min 2 characters).</param>
        /// <param name="limit">Maximum results to return.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of matching users.</returns>
        public static async Task<List<FriendSearchResult>> SearchUsersAsync(
            string query,
            int limit = 20,
            CancellationToken ct = default)
        {
            Log($"SearchUsersAsync called with query='{query}', limit={limit}");
            
            if (!Config.enableSearch)
            {
                LogWarning("Search is disabled in config.");
                return new List<FriendSearchResult>();
            }

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                Log("Query too short, returning empty list.");
                return new List<FriendSearchResult>();
            }

            try
            {
                // Check session first with detailed logging
                var session = UserSessionManager.Current;
                if (session == null)
                {
                    LogWarning("SearchUsers: Session is NULL. User must login first.");
                    return new List<FriendSearchResult>();
                }
                
                Log($"SearchUsers: Session found. userId={session.userId ?? "NULL"}, " +
                    $"accessToken={(string.IsNullOrEmpty(session.accessToken) ? "EMPTY" : "present")}, " +
                    $"userName={session.userName ?? "NULL"}");
                
                if (string.IsNullOrEmpty(session.accessToken))
                {
                    LogWarning("SearchUsers: Session exists but accessToken is EMPTY. User must re-login.");
                    return new List<FriendSearchResult>();
                }
                
                if (string.IsNullOrEmpty(session.userId))
                {
                    LogWarning("SearchUsers: Session exists but userId is EMPTY. Session is incomplete.");
                    return new List<FriendSearchResult>();
                }
                
                Log($"SearchUsers: Calling APIManager.SearchFriendsAsync with query='{query}'...");
                var data = await APIManager.SearchFriendsAsync(query, ct);
                Log($"SearchUsers: APIManager returned {data?.Count ?? 0} results.");
                
                var results = ConvertToSearchResultList(data, limit);
                Log($"SearchUsers: Converted to {results.Count} FriendSearchResult items.");
                return results;
            }
            catch (OperationCanceledException)
            {
                Log("SearchUsers: Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                LogError($"SearchUsers exception: {ex.GetType().Name} - {ex.Message}");
                LogError($"SearchUsers stack trace: {ex.StackTrace}");
                OnError?.Invoke(ex.Message);
                return new List<FriendSearchResult>();
            }
        }

        /// <summary>
        /// Sends a friend request to another user.
        /// </summary>
        /// <param name="targetUserId">The user ID to send the request to.</param>
        /// <param name="message">Optional message (not currently used by backend).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the request was sent successfully.</returns>
        public static async Task<bool> SendFriendRequestAsync(
            string targetUserId,
            string message = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                LogError("SendFriendRequest: targetUserId is required.");
                return false;
            }

            try
            {
                bool success = await APIManager.SendFriendInviteAsync(targetUserId, ct);
                if (success)
                {
                    Log($"Friend request sent to {targetUserId}");
                }
                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"SendFriendRequest exception: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Accepts a friend request.
        /// </summary>
        /// <param name="requestId">The request/relation ID to accept.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the request was accepted successfully.</returns>
        public static async Task<bool> AcceptRequestAsync(string requestId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                LogError("AcceptRequest: requestId is required.");
                return false;
            }

            try
            {
                bool success = await APIManager.AcceptFriendRequestAsync(requestId, ct);
                if (success)
                {
                    Log($"Friend request accepted: {requestId}");
                }
                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"AcceptRequest exception: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Rejects a friend request.
        /// </summary>
        /// <param name="requestId">The request/relation ID to reject.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the request was rejected successfully.</returns>
        public static async Task<bool> RejectRequestAsync(string requestId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                LogError("RejectRequest: requestId is required.");
                return false;
            }

            try
            {
                bool success = await APIManager.RejectFriendRequestAsync(requestId, ct);
                if (success)
                {
                    Log($"Friend request rejected: {requestId}");
                }
                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"RejectRequest exception: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Removes a friend from the friends list.
        /// </summary>
        /// <param name="friendUserId">The relation ID of the friend to remove.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the friend was removed successfully.</returns>
        public static async Task<bool> RemoveFriendAsync(string friendUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(friendUserId))
            {
                LogError("RemoveFriend: friendUserId is required.");
                return false;
            }

            try
            {
                bool success = await APIManager.RemoveFriendAsync(friendUserId, ct);
                if (success)
                {
                    Log($"Friend removed: {friendUserId}");
                    OnFriendRemoved?.Invoke(friendUserId);
                }
                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"RemoveFriend exception: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Blocks a user.
        /// </summary>
        /// <param name="userId">The relation ID to block.</param>
        /// <param name="reason">Optional reason (not currently used by backend).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the user was blocked successfully.</returns>
        public static async Task<bool> BlockUserAsync(
            string userId,
            string reason = null,
            CancellationToken ct = default)
        {
            if (!Config.enableBlocking)
            {
                LogWarning("Blocking is disabled in config.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                LogError("BlockUser: userId is required.");
                return false;
            }

            try
            {
                bool success = await APIManager.BlockUserAsync(userId, ct);
                if (success)
                {
                    Log($"User blocked: {userId}");
                    OnFriendRemoved?.Invoke(userId);
                }
                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError($"BlockUser exception: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        #endregion

        #region Data Conversion

        /// <summary>
        /// Converts API FriendData to UI FriendInfo.
        /// </summary>
        private static List<FriendInfo> ConvertToFriendInfoList(List<IVXModels.FriendData> data)
        {
            var result = new List<FriendInfo>();
            if (data == null) return result;

            foreach (var d in data)
            {
                result.Add(new FriendInfo
                {
                    userId = d.id ?? d.user?.id,
                    displayName = d.userName ?? d.user?.userName ?? "Unknown",
                    avatarUrl = d.profilePicture ?? d.user?.profilePicture,
                    isOnline = false, // Not provided by API currently
                    lastSeenEpoch = 0 // Not provided by API currently
                });
            }
            return result;
        }

        /// <summary>
        /// Converts API FriendData (pending) to UI FriendRequest.
        /// </summary>
        private static List<FriendRequest> ConvertToFriendRequestList(List<IVXModels.FriendData> data)
        {
            var result = new List<FriendRequest>();
            if (data == null) return result;

            foreach (var d in data)
            {
                result.Add(new FriendRequest
                {
                    requestId = d.relationId,
                    fromUserId = d.id ?? d.user?.id,
                    fromDisplayName = d.userName ?? d.user?.userName ?? "Unknown",
                    fromAvatarUrl = d.profilePicture ?? d.user?.profilePicture,
                    sentAtEpoch = 0 // Not provided by API currently
                });
            }
            return result;
        }

        /// <summary>
        /// Converts API SearchUser to UI FriendSearchResult.
        /// </summary>
        private static List<FriendSearchResult> ConvertToSearchResultList(List<IVXModels.SearchUser> data, int limit)
        {
            var result = new List<FriendSearchResult>();
            if (data == null) return result;

            int count = Math.Min(data.Count, limit);
            for (int i = 0; i < count; i++)
            {
                var d = data[i];
                // Determine relationship status from API data
                bool isPending = !string.IsNullOrEmpty(d.requestStatus) && 
                                 (d.requestStatus.Equals("pending", StringComparison.OrdinalIgnoreCase) ||
                                  d.requestStatus.Equals("sent", StringComparison.OrdinalIgnoreCase));
                bool isAccepted = !string.IsNullOrEmpty(d.requestStatus) && 
                                  d.requestStatus.Equals("accepted", StringComparison.OrdinalIgnoreCase);
                
                result.Add(new FriendSearchResult
                {
                    userId = d.id,
                    displayName = d.userName ?? "Unknown",
                    avatarUrl = d.profilePicture,
                    alreadyFriend = isAccepted,
                    requestPending = isPending
                });
            }
            return result;
        }

        #endregion

        #region Logging

        private static void Log(string message)
        {
            Debug.Log($"{LOG_TAG} {message}");
        }

        private static void LogWarning(string message)
        {
            Debug.LogWarning($"{LOG_TAG} {message}");
        }

        private static void LogError(string message)
        {
            Debug.LogError($"{LOG_TAG} {message}");
        }

        #endregion
    }
}
