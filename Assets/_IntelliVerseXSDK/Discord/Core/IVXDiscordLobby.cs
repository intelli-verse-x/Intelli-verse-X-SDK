using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Represents a Discord lobby with text and voice chat capabilities.
    /// </summary>
    [Serializable]
    public sealed class IVXDiscordLobbyInfo
    {
        /// <summary>Discord lobby ID.</summary>
        public ulong LobbyId;
        /// <summary>The secret used to create/join this lobby.</summary>
        public string Secret;
        /// <summary>Number of current members.</summary>
        public int MemberCount;
        /// <summary>Whether a voice call is active.</summary>
        public bool VoiceActive;
        /// <summary>Arbitrary metadata JSON blob.</summary>
        public string Metadata;
    }

    /// <summary>
    /// Bridges IntelliVerseX lobbies to Discord lobbies, providing
    /// text chat, voice chat, and game invite functionality.
    /// When a player creates or joins an IVX room, a corresponding
    /// Discord lobby is automatically managed.
    /// </summary>
    public sealed class IVXDiscordLobby : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordLobby]";

        #endregion

        #region Private Fields

        private static IVXDiscordLobby _instance;
        private ulong _currentLobbyId;
        private string _currentSecret;
        private bool _inLobby;
        private readonly List<string> _chatHistory = new();

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordLobby Instance => _instance;
        /// <summary>Current Discord lobby ID (0 if not in a lobby).</summary>
        public ulong CurrentLobbyId => _currentLobbyId;
        /// <summary>Whether the player is currently in a Discord lobby.</summary>
        public bool IsInLobby => _inLobby;
        /// <summary>Chat message history for the current lobby.</summary>
        public IReadOnlyList<string> ChatHistory => _chatHistory;

        #endregion

        #region Events

        /// <summary>Fired when joining a Discord lobby. Provides lobby ID.</summary>
        public event Action<ulong> OnLobbyJoined;
        /// <summary>Fired when leaving a Discord lobby.</summary>
        public event Action OnLobbyLeft;
        /// <summary>Fired when a text message is received. Provides sender name and message.</summary>
        public event Action<string, string> OnMessageReceived;
        /// <summary>Fired when a member joins the lobby. Provides user ID.</summary>
        public event Action<string> OnMemberJoined;
        /// <summary>Fired when a member leaves the lobby. Provides user ID.</summary>
        public event Action<string> OnMemberLeft;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                if (_inLobby) LeaveLobby();
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create or join a Discord lobby with the given secret.
        /// If a lobby with this secret exists, joins it; otherwise creates a new one.
        /// </summary>
        /// <param name="secret">Unique lobby secret (e.g. IVX room ID).</param>
        /// <param name="metadata">Optional JSON metadata for the lobby.</param>
        public void CreateOrJoinLobby(string secret, string metadata = null)
        {
            if (string.IsNullOrEmpty(secret))
            {
                Debug.LogError($"{LOG_TAG} Lobby secret cannot be null or empty.");
                return;
            }

            Debug.Log($"{LOG_TAG} Creating/joining lobby with secret: {secret}");

#if INTELLIVERSEX_HAS_DISCORD
            CreateOrJoinDiscordLobby(secret, metadata);
#else
            _currentSecret = secret;
            _currentLobbyId = (ulong)secret.GetHashCode();
            _inLobby = true;
            _chatHistory.Clear();
            Debug.Log($"{LOG_TAG} [Stub] Joined lobby {_currentLobbyId}");
            OnLobbyJoined?.Invoke(_currentLobbyId);
#endif
        }

        /// <summary>
        /// Leave the current Discord lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (!_inLobby)
            {
                Debug.LogWarning($"{LOG_TAG} Not in a lobby.");
                return;
            }

            Debug.Log($"{LOG_TAG} Leaving lobby {_currentLobbyId}...");

#if INTELLIVERSEX_HAS_DISCORD
            LeaveDiscordLobby(_currentLobbyId);
#endif

            _currentLobbyId = 0;
            _currentSecret = null;
            _inLobby = false;
            _chatHistory.Clear();
            OnLobbyLeft?.Invoke();
        }

        /// <summary>
        /// Send a text message to the current lobby.
        /// </summary>
        /// <param name="message">Message text to send.</param>
        public void SendMessage(string message)
        {
            if (!_inLobby)
            {
                Debug.LogError($"{LOG_TAG} Not in a lobby. Cannot send message.");
                return;
            }

            if (string.IsNullOrEmpty(message)) return;

#if INTELLIVERSEX_HAS_DISCORD
            SendDiscordLobbyMessage(_currentLobbyId, message);
#else
            _chatHistory.Add($"[You] {message}");
            Debug.Log($"{LOG_TAG} [Stub] Sent: {message}");
            OnMessageReceived?.Invoke("You", message);
#endif
        }

        /// <summary>
        /// Fetch recent chat history for the current lobby.
        /// </summary>
        /// <param name="limit">Maximum messages to retrieve (max 200).</param>
        /// <param name="onComplete">Callback with the list of messages.</param>
        public void FetchChatHistory(int limit = 50, Action<List<string>> onComplete = null)
        {
            if (!_inLobby)
            {
                onComplete?.Invoke(new List<string>());
                return;
            }

#if INTELLIVERSEX_HAS_DISCORD
            FetchDiscordLobbyHistory(_currentLobbyId, limit, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] Fetching {limit} messages from history.");
            onComplete?.Invoke(new List<string>(_chatHistory));
#endif
        }

        /// <summary>
        /// Bridge an IVX room to a Discord lobby automatically.
        /// Call this when IVXLobbyManager creates or joins a room.
        /// </summary>
        /// <param name="ivxRoomId">The IVX room ID to use as the lobby secret.</param>
        /// <param name="roomMetadata">Optional room metadata JSON.</param>
        public void BridgeIVXRoom(string ivxRoomId, string roomMetadata = null)
        {
            if (!IVXDiscordManager.Instance?.Config?.BridgeLobbiesToDiscord ?? true)
            {
                Debug.Log($"{LOG_TAG} Lobby bridging disabled in config.");
                return;
            }

            CreateOrJoinLobby($"ivx_{ivxRoomId}", roomMetadata);
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private void CreateOrJoinDiscordLobby(string secret, string metadata)
        {
            // Wire to: client->CreateOrJoinLobby(secret, callback)
            // or: client->CreateOrJoinLobbyWithMetadata(secret, metadata, callback)
            // On success: _currentLobbyId = lobbyId, _inLobby = true
            // Set up: client->SetMessageCreatedCallback for chat
        }

        private void LeaveDiscordLobby(ulong lobbyId)
        {
            // Wire to: client->LeaveLobby(lobbyId, callback)
        }

        private void SendDiscordLobbyMessage(ulong lobbyId, string message)
        {
            // Wire to: client->SendLobbyMessage(lobbyId, message, callback)
        }

        private void FetchDiscordLobbyHistory(ulong lobbyId, int limit, Action<List<string>> onComplete)
        {
            // Wire to: client->GetLobbyMessagesWithLimit(lobbyId, limit, callback)
            // Parse MessageHandle list into strings
        }
#endif

        #endregion
    }
}
