using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Represents a single direct message in a Discord DM conversation.
    /// </summary>
    [Serializable]
    public sealed class IVXDirectMessage
    {
        /// <summary>Discord snowflake ID of the message.</summary>
        public ulong MessageId;
        /// <summary>Discord user ID of the author.</summary>
        public ulong AuthorId;
        /// <summary>Display name of the author at send time.</summary>
        public string AuthorName;
        /// <summary>Plain text body of the message.</summary>
        public string Content;
        /// <summary>Unix milliseconds (or SDK-defined epoch) when the message was sent.</summary>
        public long Timestamp;
        /// <summary>Whether this is a legal disclosure message from Discord.</summary>
        public bool IsDisclosure;
        /// <summary>Whether the message has unrenderable content (images, embeds, etc.).</summary>
        public bool HasAdditionalContent;
        /// <summary>Human-readable description of additional content when <see cref="HasAdditionalContent"/> is true.</summary>
        public string AdditionalContentDescription;
        /// <summary>Optional key/value metadata for moderation integration.</summary>
        public Dictionary<string, string> ModerationMetadata;
    }

    /// <summary>
    /// Summary row for a DM conversation (peer and last message pointers).
    /// </summary>
    [Serializable]
    public sealed class IVXDMSummary
    {
        /// <summary>Discord user ID of the conversation peer.</summary>
        public ulong UserId;
        /// <summary>Display name for the conversation.</summary>
        public string DisplayName;
        /// <summary>Snowflake ID of the last message in the thread.</summary>
        public ulong LastMessageId;
        /// <summary>Timestamp of the last message.</summary>
        public long LastMessageTimestamp;
    }

    /// <summary>
    /// Wraps the Discord Social SDK Direct Messages API: send, edit, history,
    /// conversation summaries, chat visibility (notification suppression), and deep links.
    /// </summary>
    public sealed class IVXDiscordMessages : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordMessages]";
        private const int DEFAULT_HISTORY_LIMIT = 50;
        private const int MAX_HISTORY_LIMIT = 200;

        #endregion

        #region Private Fields

        private static IVXDiscordMessages _instance;
        private bool _isShowingChat;
        private ulong _stubMessageIdCounter = 1000;
        private readonly List<IVXDirectMessage> _currentConversation = new();

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordMessages Instance => _instance;

        /// <summary>
        /// Whether in-game DM chat UI is visible. When true, Discord may suppress desktop notifications for DMs.
        /// </summary>
        public bool IsShowingChat => _isShowingChat;

        /// <summary>Cached messages for the active DM conversation (SDK or stub population).</summary>
        public IReadOnlyList<IVXDirectMessage> CurrentConversation => _currentConversation;

        #endregion

        #region Events

        /// <summary>Fired when a new DM is received.</summary>
        public event Action<IVXDirectMessage> OnDMReceived;

        /// <summary>Fired when a DM is edited or moderation metadata changes.</summary>
        public event Action<IVXDirectMessage> OnDMUpdated;

        /// <summary>Fired when a DM is deleted. Argument is the message snowflake ID.</summary>
        public event Action<ulong> OnDMDeleted;

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

#if INTELLIVERSEX_HAS_DISCORD
            RegisterDiscordMessageCallbacks();
#endif
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends a direct message to the given Discord user.
        /// </summary>
        /// <param name="recipientId">Target user snowflake ID.</param>
        /// <param name="message">Plain text body.</param>
        /// <param name="onSuccess">Called with the new message ID on success.</param>
        /// <param name="onError">Called with an error string on failure.</param>
        public void SendDM(ulong recipientId, string message, Action<ulong> onSuccess = null, Action<string> onError = null)
        {
            if (recipientId == 0)
            {
                onError?.Invoke("Invalid recipientId.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                onError?.Invoke("Message cannot be empty.");
                return;
            }

            Debug.Log($"{LOG_TAG} SendDM to {recipientId}");

#if INTELLIVERSEX_HAS_DISCORD
            SendDiscordDM(recipientId, message, onSuccess, onError);
#else
            ulong id = ++_stubMessageIdCounter;
            var dm = new IVXDirectMessage
            {
                MessageId = id,
                AuthorId = 1,
                AuthorName = "You",
                Content = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsDisclosure = false,
                HasAdditionalContent = false,
                AdditionalContentDescription = null,
                ModerationMetadata = null
            };
            _currentConversation.Add(dm);
            Debug.Log($"{LOG_TAG} [Stub] Sent DM id={id}");
            onSuccess?.Invoke(id);
#endif
        }

        /// <summary>
        /// Edits an existing DM sent to the given recipient.
        /// </summary>
        /// <param name="recipientId">Conversation peer user ID.</param>
        /// <param name="messageId">Message snowflake to edit.</param>
        /// <param name="newContent">Replacement plain text.</param>
        /// <param name="onSuccess">Called when the edit succeeds.</param>
        /// <param name="onError">Called with an error string on failure.</param>
        public void EditDM(ulong recipientId, ulong messageId, string newContent, Action onSuccess = null, Action<string> onError = null)
        {
            if (recipientId == 0 || messageId == 0)
            {
                onError?.Invoke("Invalid recipientId or messageId.");
                return;
            }

            if (string.IsNullOrEmpty(newContent))
            {
                onError?.Invoke("New content cannot be empty.");
                return;
            }

            Debug.Log($"{LOG_TAG} EditDM msg={messageId} to {recipientId}");

#if INTELLIVERSEX_HAS_DISCORD
            EditDiscordDM(recipientId, messageId, newContent, onSuccess, onError);
#else
            for (int i = 0; i < _currentConversation.Count; i++)
            {
                if (_currentConversation[i].MessageId != messageId)
                    continue;

                IVXDirectMessage updated = _currentConversation[i];
                updated.Content = newContent;
                OnDMUpdated?.Invoke(updated);
                onSuccess?.Invoke();
                Debug.Log($"{LOG_TAG} [Stub] Edited DM {messageId}");
                return;
            }

            onError?.Invoke("Message not found in stub conversation.");
#endif
        }

        /// <summary>
        /// Fetches recent DM history with a user. Discord may limit history (e.g. last 72 hours) and cap count.
        /// </summary>
        /// <param name="recipientId">Conversation peer user ID.</param>
        /// <param name="limit">Max messages to return (clamped to 1–200).</param>
        /// <param name="onComplete">Called with the message list (may be empty).</param>
        public void GetDMHistory(ulong recipientId, int limit = DEFAULT_HISTORY_LIMIT, Action<List<IVXDirectMessage>> onComplete = null)
        {
            if (recipientId == 0)
            {
                onComplete?.Invoke(new List<IVXDirectMessage>());
                return;
            }

            int clamped = limit;
            if (clamped < 1)
                clamped = 1;
            if (clamped > MAX_HISTORY_LIMIT)
                clamped = MAX_HISTORY_LIMIT;

            Debug.Log($"{LOG_TAG} GetDMHistory peer={recipientId} limit={clamped}");

#if INTELLIVERSEX_HAS_DISCORD
            FetchDiscordDMHistory(recipientId, clamped, onComplete);
#else
            var list = BuildStubDMHistory(recipientId, clamped);
            _currentConversation.Clear();
            for (int i = 0; i < list.Count; i++)
                _currentConversation.Add(list[i]);
            Debug.Log($"{LOG_TAG} [Stub] Loaded {list.Count} messages.");
            onComplete?.Invoke(list);
#endif
        }

        /// <summary>
        /// Lists all DM conversation summaries (peers and last message info).
        /// </summary>
        /// <param name="onComplete">Called with the summary list.</param>
        public void GetDMSummaries(Action<List<IVXDMSummary>> onComplete = null)
        {
            Debug.Log($"{LOG_TAG} GetDMSummaries");

#if INTELLIVERSEX_HAS_DISCORD
            FetchDiscordDMSummaries(onComplete);
#else
            var summaries = new List<IVXDMSummary>
            {
                new IVXDMSummary
                {
                    UserId = 222,
                    DisplayName = "StubFriend#2222",
                    LastMessageId = 5001,
                    LastMessageTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds()
                },
                new IVXDMSummary
                {
                    UserId = 333,
                    DisplayName = "GuildMate#3333",
                    LastMessageId = 5002,
                    LastMessageTimestamp = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds()
                }
            };
            Debug.Log($"{LOG_TAG} [Stub] {summaries.Count} DM summaries.");
            onComplete?.Invoke(summaries);
#endif
        }

        /// <summary>
        /// Sets whether in-game DM chat is visible, so Discord can suppress desktop notifications while the player is reading chat.
        /// </summary>
        /// <param name="showing">True when DM UI is foregrounded.</param>
        public void SetShowingChat(bool showing)
        {
            _isShowingChat = showing;
            Debug.Log($"{LOG_TAG} SetShowingChat={showing}");

#if INTELLIVERSEX_HAS_DISCORD
            SetDiscordShowingChat(showing);
#endif
        }

        /// <summary>
        /// Opens the Discord client to the given message (deep link).
        /// </summary>
        /// <param name="messageId">Message snowflake to focus.</param>
        public void OpenMessageInDiscord(ulong messageId)
        {
            if (messageId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} OpenMessageInDiscord: invalid messageId.");
                return;
            }

            Debug.Log($"{LOG_TAG} OpenMessageInDiscord({messageId})");

#if INTELLIVERSEX_HAS_DISCORD
            OpenDiscordMessageInDiscord(messageId);
#else
            Debug.Log($"{LOG_TAG} [Stub] Would open Discord for message {messageId}.");
#endif
        }

        /// <summary>
        /// Opens Discord Connected Games / DM-related settings in the Discord client.
        /// </summary>
        public void OpenDMSettingsInDiscord()
        {
            Debug.Log($"{LOG_TAG} OpenDMSettingsInDiscord");

#if INTELLIVERSEX_HAS_DISCORD
            OpenDiscordConnectedGamesSettings();
#else
            Debug.Log($"{LOG_TAG} [Stub] Would open Connected Games settings in Discord.");
#endif
        }

        #endregion

        #region Private Methods

#if !INTELLIVERSEX_HAS_DISCORD
        private static List<IVXDirectMessage> BuildStubDMHistory(ulong recipientId, int limit)
        {
            var list = new List<IVXDirectMessage>();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int count = limit < 3 ? limit : 3;
            for (int i = 0; i < count; i++)
            {
                list.Add(new IVXDirectMessage
                {
                    MessageId = 9000u + (ulong)i,
                    AuthorId = (ulong)(i % 2 == 0 ? recipientId : 1),
                    AuthorName = i % 2 == 0 ? "Peer" : "You",
                    Content = $"[Stub] DM line {i + 1}",
                    Timestamp = now - (long)(i * 60000),
                    IsDisclosure = i == count - 1,
                    HasAdditionalContent = i == 0,
                    AdditionalContentDescription = i == 0 ? "Image attachment" : null,
                    ModerationMetadata = i == 0
                        ? new Dictionary<string, string> { { "stub_flag", "review" } }
                        : null
                });
            }

            return list;
        }
#endif

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void SendDiscordDM(ulong recipientId, string message, Action<ulong> onSuccess, Action<string> onError)
        {
            var client = Client;
            if (client == null) { onError?.Invoke("Discord client unavailable."); return; }
            try
            {
                client.SendUserMessage(recipientId, message, (msgId) =>
                {
                    var dm = new IVXDirectMessage
                    {
                        MessageId = msgId,
                        AuthorId = 0,
                        AuthorName = "You",
                        Content = message,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    _currentConversation.Add(dm);
                    onSuccess?.Invoke(msgId);
                });
            }
            catch (Exception e) { onError?.Invoke(e.Message); }
        }

        private void EditDiscordDM(ulong recipientId, ulong messageId, string newContent, Action onSuccess, Action<string> onError)
        {
            var client = Client;
            if (client == null) { onError?.Invoke("Discord client unavailable."); return; }
            try
            {
                client.EditUserMessage(recipientId, messageId, newContent, (result) =>
                {
                    for (int i = 0; i < _currentConversation.Count; i++)
                    {
                        if (_currentConversation[i].MessageId == messageId)
                        {
                            _currentConversation[i].Content = newContent;
                            OnDMUpdated?.Invoke(_currentConversation[i]);
                            break;
                        }
                    }
                    onSuccess?.Invoke();
                });
            }
            catch (Exception e) { onError?.Invoke(e.Message); }
        }

        private void FetchDiscordDMHistory(ulong recipientId, int limit, Action<List<IVXDirectMessage>> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(new List<IVXDirectMessage>()); return; }
            try
            {
                client.GetUserMessages(recipientId, (messages) =>
                {
                    var list = new List<IVXDirectMessage>();
                    if (messages != null)
                    {
                        foreach (var m in messages)
                        {
                            list.Add(new IVXDirectMessage
                            {
                                MessageId = m.Id,
                                AuthorId = m.AuthorId,
                                AuthorName = m.AuthorName ?? m.AuthorId.ToString(),
                                Content = m.Content,
                                Timestamp = m.Timestamp,
                                IsDisclosure = m.IsDisclosure,
                                HasAdditionalContent = m.HasAdditionalContent,
                                AdditionalContentDescription = m.AdditionalContentDescription
                            });
                        }
                    }
                    _currentConversation.Clear();
                    _currentConversation.AddRange(list);
                    onComplete?.Invoke(list);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} FetchDMHistory error: {e.Message}"); onComplete?.Invoke(new List<IVXDirectMessage>()); }
        }

        private void FetchDiscordDMSummaries(Action<List<IVXDMSummary>> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(new List<IVXDMSummary>()); return; }
            try
            {
                client.GetUserMessageSummaries((summaries) =>
                {
                    var list = new List<IVXDMSummary>();
                    if (summaries != null)
                    {
                        foreach (var s in summaries)
                        {
                            list.Add(new IVXDMSummary
                            {
                                UserId = s.UserId,
                                DisplayName = s.DisplayName,
                                LastMessageId = s.LastMessageId,
                                LastMessageTimestamp = s.LastMessageTimestamp
                            });
                        }
                    }
                    onComplete?.Invoke(list);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} FetchDMSummaries error: {e.Message}"); onComplete?.Invoke(new List<IVXDMSummary>()); }
        }

        private void RegisterDiscordMessageCallbacks()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetUserMessageCreatedCallback((msg) =>
                {
                    var dm = new IVXDirectMessage
                    {
                        MessageId = msg.Id,
                        AuthorId = msg.AuthorId,
                        AuthorName = msg.AuthorName ?? msg.AuthorId.ToString(),
                        Content = msg.Content,
                        Timestamp = msg.Timestamp,
                        IsDisclosure = msg.IsDisclosure,
                        HasAdditionalContent = msg.HasAdditionalContent,
                        AdditionalContentDescription = msg.AdditionalContentDescription
                    };
                    _currentConversation.Add(dm);
                    OnDMReceived?.Invoke(dm);
                });

                client.SetUserMessageUpdatedCallback((msg) =>
                {
                    for (int i = 0; i < _currentConversation.Count; i++)
                    {
                        if (_currentConversation[i].MessageId == msg.Id)
                        {
                            _currentConversation[i].Content = msg.Content;
                            OnDMUpdated?.Invoke(_currentConversation[i]);
                            break;
                        }
                    }
                });

                client.SetUserMessageDeletedCallback((msgId) =>
                {
                    _currentConversation.RemoveAll(m => m.MessageId == msgId);
                    OnDMDeleted?.Invoke(msgId);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RegisterMessageCallbacks error: {e.Message}"); }
        }

        private void SetDiscordShowingChat(bool showing)
        {
            try { Client?.SetShowingChat(showing); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SetShowingChat error: {e.Message}"); }
        }

        private void OpenDiscordMessageInDiscord(ulong messageId)
        {
            try { Client?.OpenMessageInDiscord(messageId); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} OpenMessageInDiscord error: {e.Message}"); }
        }

        private void OpenDiscordConnectedGamesSettings()
        {
            try { Client?.OpenConnectedGamesSettingsInDiscord(); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} OpenConnectedGamesSettings error: {e.Message}"); }
        }
#endif

        #endregion
    }
}
