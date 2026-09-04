// Event + DTO types for the IIVXMultiplayer surface.

using System;
using System.Collections.Generic;

namespace IntelliVerseX.MultiplayerKernel
{
    /// <summary>
    /// Strongly-typed inbound match event. <typeparamref name="TPayload"/>
    /// matches the opcode's payload type (e.g. <c>QuestionTurnPayload</c>).
    /// </summary>
    public class IVXKernelEvent<TPayload>
    {
        public Wire.IVXHeader Header { get; }
        public TPayload Payload { get; }
        /// <summary>UTC unix-ms the client received the message (for latency samples).</summary>
        public ulong RecvUnixMs { get; }

        public IVXKernelEvent(Wire.IVXHeader header, TPayload payload, ulong recvUnixMs)
        {
            Header = header;
            Payload = payload;
            RecvUnixMs = recvUnixMs;
        }

        public int Op             => Header?.Op ?? 0;
        public ulong Seq          => Header?.Seq ?? 0;
        public ulong MatchTimeMs  => Header?.MatchTimeMs ?? 0;
        public string SenderId    => Header?.SenderUserId ?? string.Empty;
        public string MatchId     => Header?.MatchId ?? string.Empty;
    }

    /// <summary>Range-subscription event — payload kept as raw JSON.</summary>
    public class IVXRawKernelEvent
    {
        public Wire.IVXHeader Header { get; }
        public string PayloadJson { get; }
        public ulong RecvUnixMs { get; }

        public IVXRawKernelEvent(Wire.IVXHeader header, string payloadJson, ulong recvUnixMs)
        {
            Header = header;
            PayloadJson = payloadJson;
            RecvUnixMs = recvUnixMs;
        }
    }

    /// <summary>Argument bundle for <see cref="IIVXMultiplayer.CreateMatchAsync"/>.</summary>
    public class IVXCreateMatchRequest
    {
        public string TemplateId { get; set; }
        public string GameId { get; set; }
        public string Region { get; set; }
        /// <summary>Per-template init payload (e.g. SyncTurnInitParams).</summary>
        public IDictionary<string, object> TemplateInit { get; set; }

        public IVXCreateMatchRequest() { TemplateInit = new Dictionary<string, object>(); }

        public IVXCreateMatchRequest(string templateId, string gameId)
        {
            TemplateId = templateId;
            GameId = gameId;
            TemplateInit = new Dictionary<string, object>();
        }
    }

    public class IVXCreateMatchResponse
    {
        public string MatchId { get; set; }
        public string TemplateId { get; set; }
        public string GameId { get; set; }
        public string Region { get; set; }
        public ulong ServerUnixMs { get; set; }
    }

    public class IVXJoinOptions
    {
        /// <summary>Optional client capabilities ("voice:livekit", "xr:visionos").</summary>
        public string[] Capabilities { get; set; }
        /// <summary>Optional locale (BCP-47).</summary>
        public string PreferredLocale { get; set; }
        /// <summary>Optional client build identifier (e.g. <c>1.5.0+stage</c>).</summary>
        public string ClientBuildId { get; set; }

        /// <summary>Per-second cap on outbound non-XR opcodes. 0 = adapter default.</summary>
        public int OutboundOpsPerSecondLimit { get; set; }
    }
}
