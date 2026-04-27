// Kernel control-plane payload DTOs (kernel.proto).
//
// All sender-side fields use System.Numerics-friendly types; we accept
// signed long for unsigned proto fields where Unity-Mono interop matters
// (Newtonsoft handles wide ranges fine via JSON numbers). Proto field
// names are preserved exactly via [JsonProperty] for cross-engine fidelity.

using System;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.Wire
{
    [Serializable]
    public class IVXHelloPayload
    {
        [JsonProperty("client_protocol_version")]
        public int ClientProtocolVersion { get; set; } = IVXWireVersion.V1;

        [JsonProperty("client_capabilities", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ClientCapabilities { get; set; }

        [JsonProperty("client_unix_ms")]
        public ulong ClientUnixMs { get; set; }

        [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLocale { get; set; }

        [JsonProperty("client_build_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientBuildId { get; set; }

        [JsonProperty("voice_provider_hint", NullValueHandling = NullValueHandling.Ignore)]
        public string VoiceProviderHint { get; set; }
    }

    [Serializable]
    public class IVXWelcomePayload
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; } = string.Empty;

        [JsonProperty("assigned_user_id")]
        public string AssignedUserId { get; set; } = string.Empty;

        [JsonProperty("server_match_time_ms")]
        public ulong ServerMatchTimeMs { get; set; }

        [JsonProperty("server_unix_ms")]
        public ulong ServerUnixMs { get; set; }

        [JsonProperty("feature_flags", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? FeatureFlags { get; set; }

        [JsonProperty("reconnect_grace_ms_remaining", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ReconnectGraceMsRemaining { get; set; }
    }

    [Serializable]
    public class IVXClockSyncPayload
    {
        [JsonProperty("server_unix_ms")]
        public ulong ServerUnixMs { get; set; }

        [JsonProperty("server_match_time_ms")]
        public ulong ServerMatchTimeMs { get; set; }

        [JsonProperty("client_unix_ms_echo")]
        public ulong ClientUnixMsEcho { get; set; }
    }

    [Serializable]
    public class IVXPlayerJoinedPayload
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("is_agent")]
        public bool IsAgent { get; set; }

        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
    }

    [Serializable]
    public class IVXPlayerLeftPayload
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;

        // 0=GRACEFUL_DISCONNECT 1=KICKED 2=TIMEOUT 3=SESSION_REPLACED.
        [JsonProperty("reason")]
        public int Reason { get; set; }
    }

    [Serializable]
    public class IVXMatchEndedPayload
    {
        // 0=COMPLETED 1=FORCE_END 2=QUORUM_LOST 3=DURATION_EXCEEDED 4=ERROR.
        [JsonProperty("reason")]
        public int Reason { get; set; }
    }

    /// <summary>Heartbeat payload — empty body. Sent every ~5s.</summary>
    [Serializable]
    public class IVXHeartbeatPayload { }
}
