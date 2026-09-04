using System;
using System.Collections.Generic;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Data transfer objects for the Friends system.
    /// All models are serializable for JSON parsing and Unity inspector display.
    /// </summary>

    #region Friend Models

    /// <summary>
    /// Represents a friend in the user's friend list.
    /// </summary>
    [Serializable]
    public class FriendInfo
    {
        /// <summary>Unique user identifier.</summary>
        public string userId;

        /// <summary>Display name shown in UI.</summary>
        public string displayName;

        /// <summary>URL to the user's avatar image.</summary>
        public string avatarUrl;

        /// <summary>Whether the friend is currently online.</summary>
        public bool isOnline;

        /// <summary>Unix timestamp of last activity (seconds since epoch).</summary>
        public long lastSeenEpoch;

        /// <summary>Optional status message set by the user.</summary>
        public string statusMessage;

        /// <summary>
        /// Returns a human-readable "last seen" string.
        /// </summary>
        public string GetLastSeenText()
        {
            if (isOnline) return "Online";
            if (lastSeenEpoch <= 0) return "Unknown";

            var lastSeen = DateTimeOffset.FromUnixTimeSeconds(lastSeenEpoch).LocalDateTime;
            var diff = DateTime.Now - lastSeen;

            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return lastSeen.ToString("MMM d");
        }
    }

    /// <summary>
    /// Represents an incoming friend request.
    /// </summary>
    [Serializable]
    public class FriendRequest
    {
        /// <summary>Unique request identifier for accept/reject operations.</summary>
        public string requestId;

        /// <summary>User ID of the person who sent the request.</summary>
        public string fromUserId;

        /// <summary>Display name of the sender.</summary>
        public string fromDisplayName;

        /// <summary>Avatar URL of the sender.</summary>
        public string fromAvatarUrl;

        /// <summary>Unix timestamp when the request was sent.</summary>
        public long sentAtEpoch;

        /// <summary>Optional message included with the request.</summary>
        public string message;

        /// <summary>
        /// Returns a human-readable "sent at" string.
        /// </summary>
        public string GetSentAtText()
        {
            if (sentAtEpoch <= 0) return "";
            var sentAt = DateTimeOffset.FromUnixTimeSeconds(sentAtEpoch).LocalDateTime;
            var diff = DateTime.Now - sentAt;

            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return sentAt.ToString("MMM d");
        }
    }

    /// <summary>
    /// Represents a user found via search.
    /// </summary>
    [Serializable]
    public class FriendSearchResult
    {
        /// <summary>Unique user identifier.</summary>
        public string userId;

        /// <summary>Display name shown in UI.</summary>
        public string displayName;

        /// <summary>URL to the user's avatar image.</summary>
        public string avatarUrl;

        /// <summary>Whether this user is already a friend.</summary>
        public bool alreadyFriend;

        /// <summary>Whether a friend request is already pending (sent or received).</summary>
        public bool requestPending;

        /// <summary>Direction of pending request: "sent" or "received".</summary>
        public string pendingDirection;
    }

    #endregion

    #region API Response Wrappers

    /// <summary>
    /// Generic API response wrapper.
    /// </summary>
    [Serializable]
    public class FriendsApiResponse<T>
    {
        public bool success;
        public string message;
        public T data;
        public string error;
    }

    /// <summary>
    /// Response for getting friends list.
    /// </summary>
    [Serializable]
    public class FriendsListResponse
    {
        public List<FriendInfo> friends;
        public int totalCount;
        public int onlineCount;
    }

    /// <summary>
    /// Response for getting friend requests.
    /// </summary>
    [Serializable]
    public class FriendRequestsResponse
    {
        public List<FriendRequest> requests;
        public int totalCount;
    }

    /// <summary>
    /// Response for searching users.
    /// </summary>
    [Serializable]
    public class FriendSearchResponse
    {
        public List<FriendSearchResult> results;
        public int totalCount;
        public bool hasMore;
    }

    /// <summary>
    /// Response for friend action (add, accept, reject, remove, block).
    /// </summary>
    [Serializable]
    public class FriendActionResponse
    {
        public bool success;
        public string message;
        public string actionType;
    }

    #endregion

    #region Request Bodies

    /// <summary>
    /// Request body for sending a friend request.
    /// </summary>
    [Serializable]
    public class SendFriendRequestBody
    {
        public string targetUserId;
        public string message;
    }

    /// <summary>
    /// Request body for accepting/rejecting a friend request.
    /// </summary>
    [Serializable]
    public class RespondToRequestBody
    {
        public string requestId;
        public string action; // "accept" or "reject"
    }

    /// <summary>
    /// Request body for removing a friend.
    /// </summary>
    [Serializable]
    public class RemoveFriendBody
    {
        public string friendUserId;
    }

    /// <summary>
    /// Request body for blocking a user.
    /// </summary>
    [Serializable]
    public class BlockUserBody
    {
        public string userId;
        public string reason;
    }

    /// <summary>
    /// Request body for searching users.
    /// </summary>
    [Serializable]
    public class SearchUsersBody
    {
        public string query;
        public int limit;
        public int offset;
    }

    #endregion

    #region Events

    /// <summary>
    /// Event arguments for friend-related events.
    /// </summary>
    public class FriendEventArgs : EventArgs
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string EventType { get; set; } // "added", "removed", "request_received", "request_accepted"
    }

    #endregion
}
