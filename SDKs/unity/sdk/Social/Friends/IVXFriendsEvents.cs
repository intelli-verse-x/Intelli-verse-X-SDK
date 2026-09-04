// ============================================================================
// IVXFriendsEvents.cs - Event system for friends
// ============================================================================

using System;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Static events for friends system. No polling.
    /// </summary>
    public static class IVXFriendsEvents
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnFriendListChanged = null;
            OnFriendAdded = null;
            OnFriendRemoved = null;
            OnFriendRequestReceived = null;
            OnFriendPresenceChanged = null;
            OnFriendsError = null;
        }

        public static event Action OnFriendListChanged;
        public static event Action<string> OnFriendAdded;
        public static event Action<string> OnFriendRemoved;
        public static event Action<string> OnFriendRequestReceived;
        public static event Action<string> OnFriendPresenceChanged;
        public static event Action<string> OnFriendsError;

        public static void RaiseFriendListChanged() => OnFriendListChanged?.Invoke();
        public static void RaiseFriendAdded(string userId) => OnFriendAdded?.Invoke(userId);
        public static void RaiseFriendRemoved(string userId) => OnFriendRemoved?.Invoke(userId);
        public static void RaiseFriendRequestReceived(string content) => OnFriendRequestReceived?.Invoke(content);
        public static void RaiseFriendPresenceChanged(string userId) => OnFriendPresenceChanged?.Invoke(userId);
        public static void RaiseFriendsError(string message) => OnFriendsError?.Invoke(message);
    }
}
