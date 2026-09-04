// IIVXVoice — adapter-side voice provider abstraction.
//
// The IVX kernel never moves audio bytes itself: it brokers session
// tokens, speaker permissions, server-side mute, spatial-audio positions,
// and ASR/moderation hooks against a pluggable provider. Engine bindings
// implement IIVXVoice once per (provider × engine) combination:
//
//   * IVXLiveKitVoiceProvider (Unity / WebGL / WebXR / visionOS / Unreal)
//   * IVXAgoraVoiceProvider   (Unity / Web)
//   * IVXTwilioVoiceProvider  (Unity / Web)
//   * IVXNullVoiceProvider    (text-only matches; no audio)
//
// A multiplayer session installs ONE active provider per match. Provider
// failover (e.g. LiveKit unavailable -> Twilio) is initiated by the kernel
// via OnProviderFailover and the adapter constructs a new provider on the
// fly without tearing the match down.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.API
{
    public enum IVXVoiceProvider
    {
        Unspecified = 0,
        LiveKit     = 1,
        Agora       = 2,
        Twilio      = 3,
        Dolby       = 4,
        None        = 5
    }

    public enum IVXVoiceCodec
    {
        Unspecified = 0,
        Opus        = 1,
        Aac         = 2
    }

    public enum IVXVoiceMode
    {
        Off       = 0,
        Broadcast = 1,
        Spatial   = 2,
        Ptt       = 3
    }

    [Serializable]
    public class IVXVoiceSessionToken
    {
        [JsonProperty("provider")]
        public IVXVoiceProvider Provider { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("room_id")]
        public string RoomId { get; set; } = string.Empty;

        [JsonProperty("identity")]
        public string Identity { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("expires_at_ms")]
        public long ExpiresAtMs { get; set; }

        [JsonProperty("can_publish")]
        public bool CanPublish { get; set; }

        [JsonProperty("can_subscribe")]
        public bool CanSubscribe { get; set; }

        [JsonProperty("spatial")]
        public bool Spatial { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [JsonProperty("provider_opts", NullValueHandling = NullValueHandling.Ignore)]
        public System.Collections.Generic.Dictionary<string, string> ProviderOpts { get; set; }
    }

    [Serializable]
    public class IVXVoiceCapability
    {
        [JsonProperty("can_publish")]
        public bool CanPublish { get; set; }

        [JsonProperty("can_subscribe")]
        public bool CanSubscribe { get; set; }

        [JsonProperty("can_spatial")]
        public bool CanSpatial { get; set; }

        [JsonProperty("codecs")]
        public IVXVoiceCodec[] Codecs { get; set; } = Array.Empty<IVXVoiceCodec>();

        [JsonProperty("max_publishers")]
        public uint MaxPublishers { get; set; }

        [JsonProperty("can_change_provider")]
        public bool CanChangeProvider { get; set; }

        [JsonProperty("can_passthrough_external")]
        public bool CanPassthroughExternal { get; set; }

        [JsonProperty("ptt_supported")]
        public bool PttSupported { get; set; }

        [JsonProperty("broadcast_supported")]
        public bool BroadcastSupported { get; set; }

        [JsonProperty("spatial_supported")]
        public bool SpatialSupported { get; set; }
    }

    [Serializable]
    public class IVXSpeakerStateChanged
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("granted")]
        public bool Granted { get; set; }

        [JsonProperty("muted_by_self")]
        public bool MutedBySelf { get; set; }

        [JsonProperty("muted_by_kernel")]
        public bool MutedByKernel { get; set; }

        [JsonProperty("floor_seconds_remaining")]
        public uint FloorSecondsRemaining { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;
    }

    [Serializable]
    public class IVXVoiceLevels
    {
        [Serializable]
        public class Sample
        {
            [JsonProperty("user_id")]
            public string UserId { get; set; } = string.Empty;

            [JsonProperty("talking_pct")]
            public uint TalkingPct { get; set; }

            [JsonProperty("silent")]
            public bool Silent { get; set; }
        }

        [JsonProperty("samples")]
        public Sample[] Samples { get; set; } = Array.Empty<Sample>();

        [JsonProperty("ts_ms")]
        public long TsMs { get; set; }
    }

    /// <summary>
    /// Voice provider abstraction. Implementations bridge the IVX wire to
    /// the underlying SFU SDK (LiveKit, Agora, etc.) and surface a uniform
    /// surface to game code.
    /// </summary>
    public interface IIVXVoice : IDisposable
    {
        IVXVoiceProvider Provider { get; }
        IVXVoiceCapability Capability { get; }
        IVXVoiceMode CurrentMode { get; }
        bool IsConnected { get; }
        bool IsLocallyMuted { get; }
        bool HasFloor { get; }

        /// <summary>The underlying provider session became (un)available.</summary>
        event Action<bool> OnConnectionChanged;

        /// <summary>Server told us our speaker state changed.</summary>
        event Action<IVXSpeakerStateChanged> OnSpeakerStateChanged;

        /// <summary>Periodic VAD broadcast for indicators / crowd meters.</summary>
        event Action<IVXVoiceLevels> OnVoiceLevels;

        /// <summary>Mode changed (PTT / broadcast / spatial / off).</summary>
        event Action<IVXVoiceMode> OnVoiceModeChanged;

        /// <summary>Failover: kernel switched provider mid-session. The current implementation MUST disconnect cleanly.</summary>
        event Action<IVXVoiceProvider> OnProviderFailover;

        /// <summary>Provider is unavailable; the session is degraded to text-only until <see cref="OnConnectionChanged"/> reports true.</summary>
        event Action<string /* reason */> OnVoiceUnavailable;

        Task ConnectAsync(IVXVoiceSessionToken token);
        Task DisconnectAsync();
        Task SetLocalMuteAsync(bool muted);
        Task RequestSpeakerAsync(string topicHint = null);
        Task ReleaseSpeakerAsync();
        Task PublishSpatialPositionAsync(IVXPoseFrameRef frameRef, float xMeters, float yMeters, float zMeters, float yawDeg);
        Task SetVoiceModeAsync(IVXVoiceMode mode);
    }
}
