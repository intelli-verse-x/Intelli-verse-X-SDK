// IVX Multiplayer Kernel — JSON wire envelope.
//
// Server-side TS templates use JSON; the proto3 contract is the source of
// truth, but Goja does not run google-protobuf JS. Go-backed templates
// (RealtimeTickMatch, AvatarReplicationMatch) use binary protobuf — this
// envelope still describes the same `Header`, just serialised over a
// different codec. Use `IVXWireCodec` to pick the right encoder.

using System;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.Wire
{
    /// <summary>
    /// Wire header — every <see cref="IVXEnvelope{T}"/> has one. Mirrors
    /// `envelope.proto Header`. JSON property names match the proto field
    /// names exactly so any cross-language client interoperates.
    /// </summary>
    [Serializable]
    public class IVXHeader
    {
        [JsonProperty("wire_version")]
        public int WireVersion { get; set; } = IVXWireVersion.V1;

        [JsonProperty("op")]
        public int Op { get; set; }

        [JsonProperty("seq")]
        public ulong Seq { get; set; }

        [JsonProperty("match_time_ms")]
        public ulong MatchTimeMs { get; set; }

        [JsonProperty("sender_user_id")]
        public string SenderUserId { get; set; } = string.Empty;

        [JsonProperty("match_id")]
        public string MatchId { get; set; } = string.Empty;

        [JsonProperty("client_opcode_uuid")]
        public string ClientOpcodeUuid { get; set; } = string.Empty;

        [JsonProperty("quantization_profile", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuantizationProfile { get; set; }

        [JsonProperty("delta_base_seq", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? DeltaBaseSeq { get; set; }

        [JsonProperty("feature_flags", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? FeatureFlags { get; set; }

        [JsonProperty("trace_parent", NullValueHandling = NullValueHandling.Ignore)]
        public string TraceParent { get; set; }
    }

    /// <summary>
    /// Generic JSON envelope: <c>{ "h": Header, "p": Payload }</c>. The
    /// payload type is template-specific; SyncTurn-tier templates use
    /// concrete C# DTOs, while game-defined opcodes (0xE000-0xE7FF) can
    /// pass <c>JObject</c> through directly.
    /// </summary>
    [Serializable]
    public class IVXEnvelope<T>
    {
        [JsonProperty("h")]
        public IVXHeader Header { get; set; } = new IVXHeader();

        [JsonProperty("p")]
        public T Payload { get; set; }

        public IVXEnvelope() { }

        public IVXEnvelope(int op, T payload, string matchId, string senderUserId)
        {
            Header = new IVXHeader
            {
                WireVersion      = IVXWireVersion.V1,
                Op               = op,
                Seq              = 0,
                MatchTimeMs      = 0,
                SenderUserId     = senderUserId,
                MatchId          = matchId,
                ClientOpcodeUuid = Guid.NewGuid().ToString("N"),
            };
            Payload = payload;
        }
    }

    /// <summary>
    /// Error payload (envelope.proto Error). Used as <c>IVXEnvelope&lt;IVXError&gt;</c>
    /// when <see cref="IVXKernelOp.ERROR"/> is received.
    /// </summary>
    [Serializable]
    public class IVXError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty("retry_after_ms", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? RetryAfterMs { get; set; }

        [JsonProperty("min_required_version", NullValueHandling = NullValueHandling.Ignore)]
        public string MinRequiredVersion { get; set; }

        public override string ToString()
            => $"IVXError code={Code} detail={Detail ?? "<null>"} retry={(RetryAfterMs ?? 0)}";
    }
}
