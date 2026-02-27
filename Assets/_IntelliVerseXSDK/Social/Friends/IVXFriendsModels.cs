// ============================================================================
// IVXFriendsModels.cs - DTOs for friends (Nakama IApiFriend wrappers)
// ============================================================================
// Use IApiFriend directly from Nakama where possible.
// These models provide UI-friendly wrappers when needed.
// ============================================================================

using System;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// UI-friendly friend info. Convert from IApiFriend when needed.
    /// </summary>
    [Serializable]
    public class IVXFriendInfo
    {
        public string userId;
        public string displayName;
        public string avatarUrl;
        public bool isOnline;
        public long lastSeenEpoch;

        public static IVXFriendInfo FromApiFriend(Nakama.IApiFriend friend)
        {
            if (friend?.User == null) return null;
            var u = friend.User;
            return new IVXFriendInfo
            {
                userId = u.Id,
                displayName = u.DisplayName ?? u.Username ?? "Unknown",
                avatarUrl = u.AvatarUrl,
                isOnline = false,
                lastSeenEpoch = 0
            };
        }
    }

    /// <summary>
    /// UI-friendly pending request info.
    /// </summary>
    [Serializable]
    public class IVXFriendRequestInfo
    {
        public string userId;
        public string displayName;
        public string avatarUrl;
        public string fromUserId => userId;

        public static IVXFriendRequestInfo FromApiFriend(Nakama.IApiFriend friend)
        {
            if (friend?.User == null) return null;
            var u = friend.User;
            return new IVXFriendRequestInfo
            {
                userId = u.Id,
                displayName = u.DisplayName ?? u.Username ?? "Unknown",
                avatarUrl = u.AvatarUrl
            };
        }
    }
}
