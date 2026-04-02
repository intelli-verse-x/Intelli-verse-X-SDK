using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Manages linked channels that bridge in-game chat (e.g. clan chat,
    /// world chat) to Discord server text channels. Messages flow
    /// bidirectionally — players can chat in Discord even when not in-game.
    /// </summary>
    public sealed class IVXDiscordLinkedChannels : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordLinkedChannels]";

        #endregion

        #region Private Fields

        private static IVXDiscordLinkedChannels _instance;
        private ulong _linkedLobbyId;
        private ulong _linkedChannelId;
        private bool _isLinked;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordLinkedChannels Instance => _instance;
        /// <summary>Whether a channel is currently linked.</summary>
        public bool IsLinked => _isLinked;
        /// <summary>The Discord lobby ID that is linked to a channel.</summary>
        public ulong LinkedLobbyId => _linkedLobbyId;

        #endregion

        #region Events

        /// <summary>Fired when a channel is linked. Provides lobby ID and channel ID.</summary>
        public event Action<ulong, ulong> OnChannelLinked;
        /// <summary>Fired when a channel is unlinked.</summary>
        public event Action OnChannelUnlinked;
        /// <summary>Fired when a message arrives from the linked Discord channel.</summary>
        public event Action<string, string> OnLinkedMessageReceived;

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
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Link an in-game chat lobby to a Discord server text channel.
        /// Requires the player to have Manage Channel permission in the Discord server.
        /// </summary>
        /// <param name="lobbyId">The Discord lobby ID to link.</param>
        /// <param name="guildId">The Discord server (guild) ID.</param>
        /// <param name="channelId">The Discord text channel ID to link to.</param>
        public void LinkChannel(ulong lobbyId, ulong guildId, ulong channelId)
        {
            Debug.Log($"{LOG_TAG} Linking lobby {lobbyId} to channel {channelId} in guild {guildId}...");

#if INTELLIVERSEX_HAS_DISCORD
            LinkDiscordChannel(lobbyId, guildId, channelId);
#else
            _linkedLobbyId = lobbyId;
            _linkedChannelId = channelId;
            _isLinked = true;
            Debug.Log($"{LOG_TAG} [Stub] Channel linked.");
            OnChannelLinked?.Invoke(lobbyId, channelId);
#endif
        }

        /// <summary>
        /// Unlink the current channel connection.
        /// </summary>
        public void UnlinkChannel()
        {
            if (!_isLinked)
            {
                Debug.LogWarning($"{LOG_TAG} No channel linked.");
                return;
            }

#if INTELLIVERSEX_HAS_DISCORD
            UnlinkDiscordChannel(_linkedLobbyId);
#endif

            _linkedLobbyId = 0;
            _linkedChannelId = 0;
            _isLinked = false;
            OnChannelUnlinked?.Invoke();
        }

        /// <summary>
        /// Send a message to the linked Discord channel from in-game.
        /// </summary>
        /// <param name="message">The message text.</param>
        public void SendToLinkedChannel(string message)
        {
            if (!_isLinked)
            {
                Debug.LogWarning($"{LOG_TAG} No channel linked.");
                return;
            }

            var lobby = IVXDiscordLobby.Instance;
            if (lobby != null && lobby.IsInLobby)
            {
                lobby.SendMessage(message);
            }
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private void LinkDiscordChannel(ulong lobbyId, ulong guildId, ulong channelId)
        {
            // Wire to: Discord HTTP API POST /lobbies/{lobbyId}/channel-linking
            // or client-side equivalent when available
        }

        private void UnlinkDiscordChannel(ulong lobbyId)
        {
            // Wire to: Discord HTTP API DELETE /lobbies/{lobbyId}/channel-linking
        }
#endif

        #endregion
    }
}
