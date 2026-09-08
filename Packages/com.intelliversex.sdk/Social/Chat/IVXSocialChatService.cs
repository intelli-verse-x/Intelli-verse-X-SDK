using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Social chat / messaging facade.
    /// Prod has no <c>chat_*</c> / <c>dm_*</c> custom RPCs — use Nakama native channels for realtime
    /// DMs &amp; clan chat, and Hiro mailbox for async gift/message inbox.
    /// </summary>
    public static class IVXSocialChatService
    {
        private const string LOG_TAG = "[IVXSocialChat]";
        private const int DefaultHistoryLimit = 50;
        private const int MaxHistoryLimit = 100;

        /// <summary>Join (or create) a 1:1 DM channel with another user.</summary>
        public static async Task<IChannel> JoinDirectAsync(
            ISocket socket,
            string otherUserId,
            bool persistence = true,
            bool hidden = false,
            CancellationToken ct = default)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (string.IsNullOrWhiteSpace(otherUserId))
                throw new ArgumentException("otherUserId required", nameof(otherUserId));

            ct.ThrowIfCancellationRequested();
            var channel = await socket.JoinChatAsync(otherUserId.Trim(), ChannelType.DirectMessage, persistence, hidden);
            Debug.Log($"{LOG_TAG} Joined DM channel {channel?.Id} with {otherUserId}");
            return channel;
        }

        /// <summary>Join a clan/group chat channel by Nakama group id.</summary>
        public static async Task<IChannel> JoinClanChatAsync(
            ISocket socket,
            string groupId,
            bool persistence = true,
            bool hidden = false,
            CancellationToken ct = default)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId required", nameof(groupId));

            ct.ThrowIfCancellationRequested();
            var channel = await socket.JoinChatAsync(groupId.Trim(), ChannelType.Group, persistence, hidden);
            Debug.Log($"{LOG_TAG} Joined clan channel {channel?.Id} for group {groupId}");
            return channel;
        }

        /// <summary>Send a text chat message on an open channel id.</summary>
        public static Task<IChannelMessageAck> SendTextAsync(
            ISocket socket,
            string channelId,
            string text,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("channelId required", nameof(channelId));
            return SendContentAsync(socket, channelId.Trim(), BuildTextContent(text), ct);
        }

        /// <summary>Send a text chat message on a joined channel.</summary>
        public static Task<IChannelMessageAck> SendTextAsync(
            ISocket socket,
            IChannel channel,
            string text,
            CancellationToken ct = default)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (string.IsNullOrWhiteSpace(channel.Id))
                throw new ArgumentException("channel.Id required", nameof(channel));
            return SendContentAsync(socket, channel.Id, BuildTextContent(text), ct);
        }

        /// <summary>Send arbitrary JSON content already serialized for the channel.</summary>
        public static Task<IChannelMessageAck> SendRawContentAsync(
            ISocket socket,
            string channelId,
            string contentJson,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("channelId required", nameof(channelId));
            if (string.IsNullOrWhiteSpace(contentJson))
                throw new ArgumentException("contentJson required", nameof(contentJson));
            return SendContentAsync(socket, channelId.Trim(), contentJson, ct);
        }

        /// <summary>List recent messages for a channel (history).</summary>
        public static async Task<IApiChannelMessageList> ListHistoryAsync(
            IClient client,
            ISession session,
            string channelId,
            int limit = DefaultHistoryLimit,
            bool forward = false,
            string cursor = null,
            CancellationToken ct = default)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("channelId required", nameof(channelId));

            ct.ThrowIfCancellationRequested();
            int clamped = Mathf.Clamp(limit <= 0 ? DefaultHistoryLimit : limit, 1, MaxHistoryLimit);
            return await client.ListChannelMessagesAsync(session, channelId.Trim(), clamped, forward, cursor);
        }

        /// <summary>Leave a previously joined chat channel.</summary>
        public static async Task LeaveAsync(ISocket socket, IChannel channel, CancellationToken ct = default)
        {
            if (socket == null || channel == null)
                return;
            ct.ThrowIfCancellationRequested();
            await socket.LeaveChatAsync(channel);
        }

        /// <summary>Try parse IVX text envelope from a channel message content string.</summary>
        public static bool TryParseContent(string contentJson, out IVXChatMessageContent content)
        {
            content = null;
            if (string.IsNullOrWhiteSpace(contentJson))
                return false;
            try
            {
                content = JsonConvert.DeserializeObject<IVXChatMessageContent>(contentJson);
                return content != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LOG_TAG} content parse failed: {ex.Message}");
                return false;
            }
        }

        private static string BuildTextContent(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("text required", nameof(text));
            return JsonConvert.SerializeObject(new IVXChatMessageContent { type = "text", text = text });
        }

        private static async Task<IChannelMessageAck> SendContentAsync(
            ISocket socket,
            string channelId,
            string contentJson,
            CancellationToken ct)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            ct.ThrowIfCancellationRequested();
            return await socket.WriteChatMessageAsync(channelId, contentJson);
        }
    }

    /// <summary>
    /// Thin envelope helpers for chat message JSON content.
    /// </summary>
    [Serializable]
    public class IVXChatMessageContent
    {
        [JsonProperty("type")] public string type = "text";
        [JsonProperty("text")] public string text;
        [JsonProperty("meta")] public Dictionary<string, string> meta;
    }
}
