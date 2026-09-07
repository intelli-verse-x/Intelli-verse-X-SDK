using System;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// A single message received from the AI backend (voice or host).
    /// </summary>
    [Serializable]
    public class IVXAIMessage
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("audio")] public string Audio;
        [JsonProperty("text")] public string Text;
        [JsonProperty("timestamp")] public long Timestamp;
        [JsonProperty("error")] public string Error;
        [JsonProperty("message")] public string Message;
        [JsonProperty("action")] public string Action;
        [JsonProperty("data")] public string Data;
        [JsonProperty("reconnectAttempt")] public int ReconnectAttempt;
        [JsonProperty("maxReconnectAttempts")] public int MaxReconnectAttempts;
        [JsonProperty("latencyMs")] public int LatencyMs;
        [JsonProperty("socialProof")] public IVXAISocialProofData SocialProof;
        [JsonProperty("analytics")] public IVXAISessionAnalytics Analytics;
        [JsonProperty("scarcityMessage")] public string ScarcityMessage;
        [JsonProperty("upsellMessage")] public string UpsellMessage;

        /// <summary>
        /// Parse the raw <see cref="Type"/> string into the typed enum.
        /// </summary>
        public IVXAIMessageType GetMessageType()
        {
            return Type switch
            {
                "voice_audio"            => IVXAIMessageType.VoiceAudio,
                "voice_caption"          => IVXAIMessageType.VoiceCaption,
                "voice_caption_complete"  => IVXAIMessageType.VoiceCaptionComplete,
                "voice_turn_complete"     => IVXAIMessageType.VoiceTurnComplete,
                "speech_detected"         => IVXAIMessageType.SpeechDetected,
                "speech_stopped"          => IVXAIMessageType.SpeechStopped,
                "session_ending"          => IVXAIMessageType.SessionEnding,
                "session_complete"        => IVXAIMessageType.SessionComplete,
                "social_proof"            => IVXAIMessageType.SocialProof,
                "scarcity_message"        => IVXAIMessageType.ScarcityMessage,
                "upsell_message"          => IVXAIMessageType.UpsellMessage,
                "error"                   => IVXAIMessageType.Error,
                "connection_lost"         => IVXAIMessageType.ConnectionLost,
                "reconnecting"            => IVXAIMessageType.Reconnecting,
                "reconnected"             => IVXAIMessageType.Reconnected,
                "connection_failed"       => IVXAIMessageType.ConnectionFailed,
                "ping"                    => IVXAIMessageType.Ping,
                "pong"                    => IVXAIMessageType.Pong,
                "server_shutdown"         => IVXAIMessageType.ServerShutdown,
                _                         => IVXAIMessageType.Unknown
            };
        }
    }

    /// <summary>
    /// Wrapper for the poll-messages endpoint response.
    /// </summary>
    [Serializable]
    public class IVXAIPollMessagesResponse : IVXAIBaseResponse
    {
        [JsonProperty("messages")] public IVXAIMessage[] Messages;
        [JsonProperty("count")] public int Count;
    }
}
