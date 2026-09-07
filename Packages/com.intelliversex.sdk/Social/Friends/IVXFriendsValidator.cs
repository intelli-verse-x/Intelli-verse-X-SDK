// ============================================================================
// IVXFriendsValidator.cs - Edge case safety for friends operations
// ============================================================================

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Validates friends operations before Nakama calls. Edge case safety.
    /// </summary>
    public static class IVXFriendsValidator
    {
        public static bool CanAddFriend(string targetUserId, string selfUserId, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                error = "User ID is required";
                return false;
            }
            if (targetUserId == selfUserId)
            {
                error = "Cannot add yourself";
                return false;
            }
            return true;
        }

        public static bool CanRemoveOrBlock(string targetUserId, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                error = "User ID is required";
                return false;
            }
            return true;
        }

        public static bool IsValidSearchQuery(string query)
        {
            return !string.IsNullOrWhiteSpace(query) && query.Trim().Length >= 2;
        }
    }
}
