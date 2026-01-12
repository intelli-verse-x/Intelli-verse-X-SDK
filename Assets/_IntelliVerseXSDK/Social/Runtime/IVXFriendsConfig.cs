using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Configuration asset for the Friends system.
    /// Create via: Assets → Create → IntelliVerse-X → Friends Config
    /// Place in: Resources/IntelliVerseX/FriendsConfig.asset
    /// </summary>
    [CreateAssetMenu(fileName = "FriendsConfig", menuName = "IntelliVerse-X/Friends Config", order = 100)]
    public class IVXFriendsConfig : ScriptableObject
    {
        #region Singleton Access

        private static IVXFriendsConfig _instance;

        /// <summary>
        /// Gets the singleton instance, loading from Resources if needed.
        /// </summary>
        public static IVXFriendsConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<IVXFriendsConfig>("IntelliVerseX/FriendsConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[IVXFriends] FriendsConfig not found in Resources/IntelliVerseX/. Using defaults.");
                        _instance = CreateInstance<IVXFriendsConfig>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region API Settings

        [Header("API Configuration")]
        [Tooltip("Base URL for the Friends API endpoints.")]
        public string baseUrl = "https://api.intelli-verse-x.ai/api/games/friends";

        [Tooltip("Request timeout in seconds.")]
        [Range(5, 60)]
        public int timeoutSeconds = 15;

        [Tooltip("Maximum retry attempts on transient failures.")]
        [Range(0, 5)]
        public int maxRetries = 1;

        [Tooltip("Delay between retries in seconds.")]
        [Range(0.5f, 5f)]
        public float retryDelaySeconds = 1f;

        #endregion

        #region Feature Flags

        [Header("Feature Flags")]
        [Tooltip("Enable the ability to block users.")]
        public bool enableBlocking = true;

        [Tooltip("Enable user search functionality.")]
        public bool enableSearch = true;

        [Tooltip("Enable sending messages with friend requests.")]
        public bool enableRequestMessages = true;

        [Tooltip("Show online/offline status indicators.")]
        public bool showOnlineStatus = true;

        [Tooltip("Enable pull-to-refresh gesture on mobile.")]
        public bool enablePullToRefresh = true;

        #endregion

        #region UI Settings

        [Header("UI Configuration")]
        [Tooltip("Maximum number of friends to display in the list.")]
        [Range(10, 200)]
        public int maxVisibleFriends = 50;

        [Tooltip("Maximum number of search results to display.")]
        [Range(5, 50)]
        public int maxSearchResults = 20;

        [Tooltip("Auto-refresh interval in seconds (0 = disabled).")]
        [Range(0, 300)]
        public float autoRefreshIntervalSeconds = 60f;

        [Tooltip("Default avatar to use when avatar URL is empty or fails to load.")]
        public Sprite defaultAvatar;

        [Tooltip("Online status indicator color.")]
        public Color onlineColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green

        [Tooltip("Offline status indicator color.")]
        public Color offlineColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

        #endregion

        #region Animation Settings

        [Header("Animation Settings")]
        [Tooltip("Duration for panel open/close animations.")]
        [Range(0.1f, 1f)]
        public float panelAnimationDuration = 0.3f;

        [Tooltip("Duration for slot appear animations.")]
        [Range(0.05f, 0.5f)]
        public float slotAnimationDuration = 0.15f;

        [Tooltip("Stagger delay between slot animations.")]
        [Range(0.01f, 0.2f)]
        public float slotStaggerDelay = 0.03f;

        [Tooltip("Enable slot slide-in animations.")]
        public bool enableSlotAnimations = true;

        #endregion

        #region Endpoint Helpers

        /// <summary>Gets the full URL for the friends list endpoint.</summary>
        public string GetFriendsListUrl() => $"{baseUrl}/list";

        /// <summary>Gets the full URL for the friend requests endpoint.</summary>
        public string GetRequestsUrl() => $"{baseUrl}/requests";

        /// <summary>Gets the full URL for sending a friend request.</summary>
        public string GetSendRequestUrl() => $"{baseUrl}/request/send";

        /// <summary>Gets the full URL for responding to a friend request.</summary>
        public string GetRespondRequestUrl() => $"{baseUrl}/request/respond";

        /// <summary>Gets the full URL for removing a friend.</summary>
        public string GetRemoveFriendUrl() => $"{baseUrl}/remove";

        /// <summary>Gets the full URL for blocking a user.</summary>
        public string GetBlockUserUrl() => $"{baseUrl}/block";

        /// <summary>Gets the full URL for searching users.</summary>
        public string GetSearchUrl() => $"{baseUrl}/search";

        #endregion

        #region Validation

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Debug.LogError("[IVXFriends] Base URL is not configured.");
                return false;
            }
            return true;
        }

        #endregion
    }
}
