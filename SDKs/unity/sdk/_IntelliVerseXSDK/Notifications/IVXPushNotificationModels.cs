using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Notifications
{
    /// <summary>
    /// Target push notification platform.
    /// </summary>
    public enum PushPlatform
    {
        /// <summary>Firebase Cloud Messaging (Android / cross-platform).</summary>
        FCM = 0,

        /// <summary>Apple Push Notification Service (iOS / macOS).</summary>
        APNS = 1,

        /// <summary>Huawei Mobile Services (Huawei devices).</summary>
        HMS = 2
    }

    /// <summary>
    /// Well-known push event types sent by the server.
    /// </summary>
    public static class PushEventType
    {
        public const string DailyRewardAvailable = "daily_reward_available";
        public const string StreakWarning = "streak_warning";
        public const string ChallengeInvite = "challenge_invite";
        public const string MatchReady = "match_ready";
        public const string NewSeason = "new_season";
    }

    /// <summary>
    /// Represents a registered push notification endpoint.
    /// </summary>
    [Serializable]
    public class PushEndpoint
    {
        /// <summary>Unique endpoint identifier.</summary>
        [JsonProperty("endpoint_id")] public string endpointId;

        /// <summary>The device token registered with the push service.</summary>
        [JsonProperty("device_token")] public string deviceToken;

        /// <summary>Platform this endpoint targets.</summary>
        [JsonProperty("platform")] public PushPlatform platform;

        /// <summary>When this endpoint was created.</summary>
        [JsonProperty("created_at")] public string createdAt;

        /// <summary>Whether this endpoint is currently active.</summary>
        [JsonProperty("is_active")] public bool isActive;
    }

    /// <summary>
    /// Payload of a push notification received from the server.
    /// </summary>
    [Serializable]
    public class PushEvent
    {
        /// <summary>Unique event identifier.</summary>
        [JsonProperty("event_id")] public string eventId;

        /// <summary>Event type (see <see cref="PushEventType"/>).</summary>
        [JsonProperty("type")] public string type;

        /// <summary>Notification title.</summary>
        [JsonProperty("title")] public string title;

        /// <summary>Notification body text.</summary>
        [JsonProperty("body")] public string body;

        /// <summary>Optional deep-link URL.</summary>
        [JsonProperty("deep_link")] public string deepLink;

        /// <summary>Arbitrary key-value metadata.</summary>
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;

        /// <summary>Server-side timestamp (ISO 8601).</summary>
        [JsonProperty("sent_at")] public string sentAt;
    }

    /// <summary>
    /// Response returned after successfully registering a push token.
    /// </summary>
    [Serializable]
    public class PushTokenRegistrationResult
    {
        /// <summary>Assigned endpoint identifier.</summary>
        [JsonProperty("endpoint_id")] public string endpointId;

        /// <summary>Whether the registration was successful.</summary>
        [JsonProperty("registered")] public bool registered;
    }

    /// <summary>
    /// Response containing all registered push endpoints for the user.
    /// </summary>
    [Serializable]
    public class PushEndpointsResponse
    {
        /// <summary>List of registered endpoints.</summary>
        [JsonProperty("endpoints")] public List<PushEndpoint> endpoints;
    }
}
