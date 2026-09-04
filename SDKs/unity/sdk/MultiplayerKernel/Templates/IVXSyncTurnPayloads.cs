// Sync-turn payload DTOs (templates/sync_turn.proto).

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliVerseX.MultiplayerKernel.Templates.SyncTurn
{
    /// <summary>Initial parameters used by <c>mp_create_match</c>.</summary>
    [Serializable]
    public class SyncTurnInitParams
    {
        [JsonProperty("min_players")]
        public int MinPlayers { get; set; } = 2;

        [JsonProperty("max_players")]
        public int MaxPlayers { get; set; } = 5;

        [JsonProperty("default_input_window_ms")]
        public int DefaultInputWindowMs { get; set; } = 15000;

        [JsonProperty("max_match_duration_ms")]
        public int MaxMatchDurationMs { get; set; } = 30 * 60 * 1000;

        [JsonProperty("reconnect_grace_ms")]
        public int ReconnectGraceMs { get; set; } = 60000;

        [JsonProperty("game_id")]
        public string GameId { get; set; } = string.Empty;

        [JsonProperty("agent_seat_count")]
        public int AgentSeatCount { get; set; }

        [JsonProperty("generator_id")]
        public string GeneratorId { get; set; } = string.Empty;
    }

    /// <summary>TURN_START (opcode 0x4001).</summary>
    [Serializable]
    public class TurnStartPayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }

        [JsonProperty("round_index")]
        public int RoundIndex { get; set; }

        [JsonProperty("input_window_ms")]
        public int InputWindowMs { get; set; }

        [JsonProperty("input_opens_at_match_ms")]
        public ulong InputOpensAtMatchMs { get; set; }

        [JsonProperty("input_closes_at_match_ms")]
        public ulong InputClosesAtMatchMs { get; set; }

        [JsonProperty("turn_payload")]
        public JToken TurnPayload { get; set; }

        [JsonProperty("is_final_turn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFinalTurn { get; set; }
    }

    /// <summary>TURN_INPUT_OPENED (opcode 0x4002).</summary>
    [Serializable]
    public class TurnInputOpenedPayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }
    }

    /// <summary>TURN_INPUT_CLOSED (opcode 0x4003).</summary>
    [Serializable]
    public class TurnInputClosedPayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }

        [JsonProperty("all_submitted")]
        public bool AllSubmitted { get; set; }
    }

    /// <summary>TURN_RESOLVED (opcode 0x4004).</summary>
    [Serializable]
    public class TurnResolvedPayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }

        [JsonProperty("result_payload")]
        public JToken ResultPayload { get; set; }

        [JsonProperty("score_delta")]
        public Dictionary<string, int> ScoreDelta { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>SCORE_UPDATE (opcode 0x4005).</summary>
    [Serializable]
    public class ScoreUpdatePayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }

        [JsonProperty("totals")]
        public Dictionary<string, int> Totals { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>TURN_INPUT_SUBMIT (opcode 0x4010, client→server).</summary>
    [Serializable]
    public class TurnInputSubmitPayload
    {
        [JsonProperty("turn_index")]
        public int TurnIndex { get; set; }

        /// <summary>Client claim of response time; server clamps to [0, server-observed].</summary>
        [JsonProperty("client_response_ms")]
        public int ClientResponseMs { get; set; }

        /// <summary>Game-specific submission body.</summary>
        [JsonProperty("submission")]
        public JToken Submission { get; set; }
    }

    /// <summary>PLAYER_READY (opcode 0x4011).</summary>
    [Serializable]
    public class PlayerReadyPayload
    {
        [JsonProperty("ready")]
        public bool Ready { get; set; } = true;
    }

    /// <summary>PLAYER_FORFEIT (opcode 0x4012).</summary>
    [Serializable]
    public class PlayerForfeitPayload { }
}
