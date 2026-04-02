using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Configuration ScriptableObject for the Discord Social SDK integration.
    /// Create via Assets → Create → IntelliVerseX → Discord Config.
    /// </summary>
    [CreateAssetMenu(fileName = "IVXDiscordConfig", menuName = "IntelliVerseX/Discord Config")]
    public sealed class IVXDiscordConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("Application")]
        [Tooltip("Your Discord Application ID from the Developer Portal.")]
        [SerializeField] private long _applicationId;

        [Header("OAuth2")]
        [Tooltip("OAuth2 Client ID (same as Application ID for most games).")]
        [SerializeField] private string _clientId;

        [Tooltip("OAuth2 redirect URI registered in the Developer Portal.")]
        [SerializeField] private string _redirectUri = "https://localhost";

        [Header("Rich Presence")]
        [Tooltip("Automatically update Rich Presence from IVX game state.")]
        [SerializeField] private bool _autoPresence = true;

        [Tooltip("Interval in seconds between Rich Presence updates.")]
        [SerializeField, Range(5f, 120f)] private float _presenceUpdateInterval = 15f;

        [Tooltip("Large image asset key uploaded to Discord Developer Portal.")]
        [SerializeField] private string _largeImageAssetKey = "game_logo";

        [Tooltip("Text shown when hovering the large image.")]
        [SerializeField] private string _largeImageText = "";

        [Header("Lobbies & Voice")]
        [Tooltip("Enable Discord voice chat in multiplayer lobbies.")]
        [SerializeField] private bool _enableVoiceChat = true;

        [Tooltip("Maximum members per voice lobby (recommended ≤ 25).")]
        [SerializeField, Range(2, 25)] private int _maxVoiceLobbySize = 8;

        [Tooltip("Bridge IVX lobbies to Discord lobbies automatically.")]
        [SerializeField] private bool _bridgeLobbiesToDiscord = true;

        [Header("Community")]
        [Tooltip("Discord server invite URL for the 'Join Community' Rich Presence button.")]
        [SerializeField] private string _communityInviteUrl = "";

        [Tooltip("Store page URL for the 'Play Now' Rich Presence button.")]
        [SerializeField] private string _storePageUrl = "";

        #endregion

        #region Properties

        /// <summary>Discord Application ID.</summary>
        public long ApplicationId => _applicationId;
        /// <summary>OAuth2 Client ID.</summary>
        public string ClientId => _clientId;
        /// <summary>OAuth2 redirect URI.</summary>
        public string RedirectUri => _redirectUri;
        /// <summary>Whether to auto-update Rich Presence from game state.</summary>
        public bool AutoPresence => _autoPresence;
        /// <summary>Seconds between Rich Presence refreshes.</summary>
        public float PresenceUpdateInterval => _presenceUpdateInterval;
        /// <summary>Large image asset key for Rich Presence.</summary>
        public string LargeImageAssetKey => _largeImageAssetKey;
        /// <summary>Hover text for the large Rich Presence image.</summary>
        public string LargeImageText => _largeImageText;
        /// <summary>Whether Discord voice chat is enabled.</summary>
        public bool EnableVoiceChat => _enableVoiceChat;
        /// <summary>Max members in a voice lobby.</summary>
        public int MaxVoiceLobbySize => _maxVoiceLobbySize;
        /// <summary>Whether IVX lobbies auto-bridge to Discord lobbies.</summary>
        public bool BridgeLobbiesToDiscord => _bridgeLobbiesToDiscord;
        /// <summary>Discord community server invite URL.</summary>
        public string CommunityInviteUrl => _communityInviteUrl;
        /// <summary>Game store page URL.</summary>
        public string StorePageUrl => _storePageUrl;

        #endregion
    }
}
