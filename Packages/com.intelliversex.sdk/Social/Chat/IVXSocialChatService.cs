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
            var channel = await socket.JoinChatAsync(otherUserId, ChannelType.DirectMessage, persistence, hidden);
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
            var channel = await socket.JoinChatAsync(groupId, ChannelType.Group, persistence, hidden);
            Debug.Log($"{LOG_TAG} Joined clan channel {channel?.Id} for group {groupId}");
            return channel;
        }

        /// <summary>Send a text chat message on an open channel.</summary>
        public static async Task<IApiChannelMessageAck> SendTextAsync(
            ISocket socket,
            string channelId,
            string text,
            CancellationToken ct = default)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("channelId required", nameof(channelId));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("text required", nameof(text));

            ct.ThrowIfCancellationRequested();
            string content = JsonConvert.SerializeObject(new { type = "text", text });
            return await socket.WriteChatMessageAsync(channelId, content);
        }

        /// <summary>List recent messages for a channel (history).</summary>
        public static async Task<IApiChannelMessageList> ListHistoryAsync(
            IClient client,
            ISession session,
            string channelId,
            int limit = 50,
            bool forward = false,
            string cursor = null,
            CancellationToken ct = default)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("channelId required", nameof(channelId));

            ct.ThrowIfCancellationRequested();
            return await client.ListChannelMessagesAsync(session, channelId, limit, forward, cursor);
        }

        /// <summary>
        /// Leave a previously joined chat channel.
        /// </summary>
        public static async Task LeaveAsync(ISocket socket, IChannel channel, CancellationToken ct = default)
        {
            if (socket == null || channel == null) return;
            ct.ThrowIfCancellationRequested();
            await socket.LeaveChatAsync(channel);
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
