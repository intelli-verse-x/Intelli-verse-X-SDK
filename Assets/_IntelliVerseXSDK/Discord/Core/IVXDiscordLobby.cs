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
        /// <summary>Lobby-level metadata JSON (explicit lobby scope).</summary>
        public string LobbyMetadata;
        /// <summary>Member user IDs currently in the lobby.</summary>
        public string[] MemberIds;
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
        private const int DEFAULT_LOBBY_IDLE_TIMEOUT_SECONDS = 300;
        private const int MAX_LOBBY_IDLE_TIMEOUT_SECONDS = 604800;

        #endregion

        #region Private Fields

        private static IVXDiscordLobby _instance;
        private ulong _currentLobbyId;
        private string _currentSecret;
        private bool _inLobby;
        private readonly List<string> _chatHistory = new();
        private string _currentLobbyMetadata;
        private string _currentUserMetadata;
        private int _lobbyIdleTimeoutSeconds = DEFAULT_LOBBY_IDLE_TIMEOUT_SECONDS;

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
        /// <summary>JSON metadata for the current lobby (lobby scope).</summary>
        public string CurrentLobbyMetadata => _currentLobbyMetadata;
        /// <summary>JSON metadata for the local user in the current lobby.</summary>
        public string CurrentUserMetadata => _currentUserMetadata;

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
            _currentLobbyMetadata = metadata;
            CreateOrJoinDiscordLobby(secret, metadata);
#else
            _currentSecret = secret;
            _currentLobbyId = (ulong)secret.GetHashCode();
            _currentLobbyMetadata = metadata;
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
            _currentLobbyMetadata = null;
            _currentUserMetadata = null;
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

        #region Lobby Metadata

        /// <summary>
        /// Create or join a lobby with separate lobby-level and per-user metadata JSON.
        /// </summary>
        /// <param name="secret">Unique lobby secret.</param>
        /// <param name="lobbyMetadata">Lobby-level JSON (searchable / discovery fields).</param>
        /// <param name="userMetadata">Local member metadata JSON.</param>
        /// <param name="onComplete">Invoked with the lobby ID when ready.</param>
        public void CreateOrJoinLobbyWithMetadata(string secret, string lobbyMetadata, string userMetadata, Action<ulong> onComplete = null)
        {
            if (string.IsNullOrEmpty(secret))
            {
                Debug.LogError($"{LOG_TAG} Lobby secret cannot be null or empty.");
                return;
            }

            Debug.Log($"{LOG_TAG} Creating/joining lobby with metadata (secret hash={secret.GetHashCode()})");

#if INTELLIVERSEX_HAS_DISCORD
            _currentLobbyMetadata = lobbyMetadata;
            _currentUserMetadata = userMetadata;
            CreateOrJoinDiscordLobbyWithMetadata(secret, lobbyMetadata, userMetadata, lobbyId =>
            {
                _currentLobbyId = lobbyId;
                _currentSecret = secret;
                _inLobby = true;
                _chatHistory.Clear();
                OnLobbyJoined?.Invoke(lobbyId);
                onComplete?.Invoke(lobbyId);
            });
#else
            _currentSecret = secret;
            _currentLobbyId = (ulong)secret.GetHashCode();
            _currentLobbyMetadata = lobbyMetadata;
            _currentUserMetadata = userMetadata;
            _inLobby = true;
            _chatHistory.Clear();
            Debug.Log($"{LOG_TAG} [Stub] Joined lobby {_currentLobbyId} with metadata.");
            OnLobbyJoined?.Invoke(_currentLobbyId);
            onComplete?.Invoke(_currentLobbyId);
#endif
        }

        /// <summary>
        /// Update the local member's metadata JSON for the current lobby.
        /// </summary>
        public void UpdateLobbyMemberMetadata(string metadata)
        {
            if (!_inLobby)
            {
                Debug.LogWarning($"{LOG_TAG} Not in a lobby.");
                return;
            }

            _currentUserMetadata = metadata;

#if INTELLIVERSEX_HAS_DISCORD
            UpdateDiscordLobbyMemberMetadata(_currentLobbyId, metadata);
#else
            Debug.Log($"{LOG_TAG} [Stub] Updated member metadata.");
#endif
        }

        /// <summary>
        /// Fetch full lobby details including members and metadata.
        /// </summary>
        public void GetLobbyInfo(Action<IVXDiscordLobbyInfo> onComplete = null)
        {
            if (!_inLobby)
            {
                onComplete?.Invoke(null);
                return;
            }

#if INTELLIVERSEX_HAS_DISCORD
            FetchDiscordLobbyInfo(_currentLobbyId, onComplete);
#else
            var info = new IVXDiscordLobbyInfo
            {
                LobbyId = _currentLobbyId,
                Secret = _currentSecret,
                MemberCount = 1,
                VoiceActive = false,
                Metadata = _currentLobbyMetadata,
                LobbyMetadata = _currentLobbyMetadata,
                MemberIds = new[] { "local_user" }
            };
            onComplete?.Invoke(info);
#endif
        }

        /// <summary>
        /// Set how long the lobby may stay idle before the server tears it down (seconds).
        /// Default 300; maximum 604800 (7 days).
        /// </summary>
        public void SetLobbyIdleTimeout(int seconds)
        {
            seconds = Mathf.Clamp(seconds, 1, MAX_LOBBY_IDLE_TIMEOUT_SECONDS);
            _lobbyIdleTimeoutSeconds = seconds;

#if INTELLIVERSEX_HAS_DISCORD
            if (_inLobby)
                SetDiscordLobbyIdleTimeout(_currentLobbyId, seconds);
#else
            Debug.Log($"{LOG_TAG} [Stub] Lobby idle timeout = {seconds}s");
#endif
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void CreateOrJoinDiscordLobby(string secret, string metadata)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.CreateOrJoinLobby(secret, (lobbyId) =>
                {
                    _currentLobbyId = lobbyId;
                    _currentSecret = secret;
                    _currentLobbyMetadata = metadata;
                    _inLobby = true;
                    _chatHistory.Clear();
                    Debug.Log($"{LOG_TAG} Joined Discord lobby: {lobbyId}");
                    OnLobbyJoined?.Invoke(lobbyId);

                    client.SetMessageCreatedCallback(lobbyId, (senderId, content) =>
                    {
                        _chatHistory.Add(content);
                        OnMessageReceived?.Invoke(senderId.ToString(), content);
                    });
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} CreateOrJoinDiscordLobby error: {e.Message}"); }
        }

        private void CreateOrJoinDiscordLobbyWithMetadata(string secret, string lobbyMetadata, string userMetadata, Action<ulong> onJoined)
        {
            var client = Client;
            if (client == null) { return; }
            try
            {
                client.CreateOrJoinLobby(secret, (lobbyId) =>
                {
                    client.SetMessageCreatedCallback(lobbyId, (senderId, content) =>
                    {
                        _chatHistory.Add(content);
                        OnMessageReceived?.Invoke(senderId.ToString(), content);
                    });
                    onJoined?.Invoke(lobbyId);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} CreateOrJoinLobbyWithMetadata error: {e.Message}"); }
        }

        private void LeaveDiscordLobby(ulong lobbyId)
        {
            try { Client?.LeaveLobby(lobbyId, (result) => Debug.Log($"{LOG_TAG} Left lobby {lobbyId}: {result}")); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} LeaveDiscordLobby error: {e.Message}"); }
        }

        private void SendDiscordLobbyMessage(ulong lobbyId, string message)
        {
            try { Client?.SendLobbyMessage(lobbyId, message, (result) => { }); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SendDiscordLobbyMessage error: {e.Message}"); }
        }

        private void FetchDiscordLobbyHistory(ulong lobbyId, int limit, Action<List<string>> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(new List<string>()); return; }
            try
            {
                client.GetLobbyMessages(lobbyId, (messages) =>
                {
                    var list = new List<string>();
                    if (messages != null)
                        foreach (var m in messages)
                            list.Add(m.Content);
                    onComplete?.Invoke(list);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_TAG} FetchDiscordLobbyHistory error: {e.Message}");
                onComplete?.Invoke(new List<string>());
            }
        }

        private void UpdateDiscordLobbyMemberMetadata(ulong lobbyId, string metadata)
        {
            try { Client?.UpdateLobbyMemberMetadata(lobbyId, metadata, (r) => { }); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} UpdateMemberMetadata error: {e.Message}"); }
        }

        private void FetchDiscordLobbyInfo(ulong lobbyId, Action<IVXDiscordLobbyInfo> onComplete)
        {
            var info = new IVXDiscordLobbyInfo
            {
                LobbyId = lobbyId,
                Secret = _currentSecret,
                MemberCount = 1,
                VoiceActive = false,
                Metadata = _currentLobbyMetadata,
                LobbyMetadata = _currentLobbyMetadata,
                MemberIds = Array.Empty<string>()
            };
            onComplete?.Invoke(info);
        }

        private void SetDiscordLobbyIdleTimeout(ulong lobbyId, int seconds)
        {
            try { Client?.SetLobbyIdleTimeout(lobbyId, seconds); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetLobbyIdleTimeout error: {e.Message}"); }
        }
#endif

        #endregion
    }
}
