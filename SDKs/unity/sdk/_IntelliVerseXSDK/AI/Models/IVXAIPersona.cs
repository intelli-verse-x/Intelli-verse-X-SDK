using System;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Describes an AI persona available on the IVX backend.
    /// Personas are string-identified so studios can define custom ones server-side.
    /// </summary>
    [Serializable]
    public class IVXAIPersona
    {
        /// <summary>Unique identifier sent to the API (e.g. "FortuneTeller", "GameHost").</summary>
        [JsonProperty("type")] public string Id;

        /// <summary>Human-readable name (e.g. "Mystica the Fortune Teller").</summary>
        [JsonProperty("displayName")] public string DisplayName;

        /// <summary>Short description of what this persona does.</summary>
        [JsonProperty("description")] public string Description;

        /// <summary>Whether this persona requires a paid entitlement.</summary>
        [JsonProperty("isPremium")] public bool IsPremium;

        /// <summary>Revenue tier label (e.g. "tier_1", "tier_2", "premium").</summary>
        [JsonProperty("tier")] public string Tier;

        /// <summary>Default session duration in seconds for free users.</summary>
        [JsonProperty("defaultDurationSeconds")] public int DefaultDurationSeconds;

        /// <summary>Session duration in seconds for premium users.</summary>
        [JsonProperty("premiumDurationSeconds")] public int PremiumDurationSeconds;
    }

    /// <summary>
    /// Session status for tracking lifecycle.
    /// </summary>
    public enum IVXAISessionStatus
    {
        Active,
        Completed,
        Expired,
        Cancelled,
        Failed
    }

    /// <summary>
    /// Message types received from the IVX AI backend.
    /// </summary>
    public enum IVXAIMessageType
    {
        VoiceAudio,
        VoiceCaption,
        VoiceCaptionComplete,
        VoiceTurnComplete,
        SpeechDetected,
        SpeechStopped,
        SessionEnding,
        SessionComplete,
        SocialProof,
        ScarcityMessage,
        UpsellMessage,
        Error,
        ConnectionLost,
        Reconnecting,
        Reconnected,
        ConnectionFailed,
        Ping,
        Pong,
        ServerShutdown,
        Unknown
    }

    /// <summary>
    /// WebSocket / realtime connection state.
    /// </summary>
    public enum IVXAIConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Failed
    }

    /// <summary>
    /// Transport mode for a session.
    /// </summary>
    public enum IVXAIConnectionMode
    {
        WebSocket,
        HttpPolling
    }

    /// <summary>
    /// IAP product types exposed by the AI entitlement API.
    /// </summary>
    public enum IVXAIProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    /// <summary>
    /// Entitlement access levels.
    /// </summary>
    public enum IVXAIEntitlementLevel
    {
        None,
        FreeTrial,
        SessionPack,
        Subscriber,
        AllAccess
    }
}
