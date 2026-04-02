using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Runtime profile describing an NPC participant in the dialog system (persona, RAG, tools, UI).
    /// </summary>
    [Serializable]
    public class IVXAINPCProfile
    {
        #region Fields

        /// <summary>Stable identifier for this NPC (matches backend / routing).</summary>
        public string NpcId;

        /// <summary>Human-readable name shown in UI.</summary>
        public string DisplayName;

        /// <summary>System prompt defining personality and behaviour for the language model.</summary>
        public string PersonaPrompt;

        /// <summary>Optional narrative background for the character.</summary>
        public string Backstory;

        /// <summary>Document identifiers for retrieval-augmented generation (RAG).</summary>
        public string[] KnowledgeBaseIds;

        /// <summary>Voice identifier for text-to-speech integration.</summary>
        public string VoiceId;

        /// <summary>Maximum dialog turns; use 0 for unlimited.</summary>
        public int MaxTurns;

        /// <summary>Names of tools or game actions this NPC is allowed to invoke.</summary>
        public string[] AvailableActions;

        /// <summary>Optional portrait sprite for UI; may be null.</summary>
        public Sprite Avatar;

        #endregion
    }

    /// <summary>
    /// High-level state of an NPC dialog session.
    /// </summary>
    public enum IVXAINPCDialogState
    {
        /// <summary>Session is active and exchanges may continue.</summary>
        Active,

        /// <summary>Waiting for the player to send input.</summary>
        WaitingForPlayer,

        /// <summary>Waiting for the NPC / server reply.</summary>
        WaitingForNPC,

        /// <summary>Session has ended and should not accept further messages.</summary>
        Ended
    }

    /// <summary>
    /// A single turn in the dialog history.
    /// </summary>
    [Serializable]
    public class IVXAINPCDialogMessage
    {
        #region Fields

        /// <summary>Message role: typically <c>"player"</c> or <c>"npc"</c>.</summary>
        public string Role;

        /// <summary>Plain-text content of the message.</summary>
        public string Content;

        /// <summary>Unix-milliseconds (or server convention) timestamp.</summary>
        public long Timestamp;

        /// <summary>Optional structured action (tool call); null if none.</summary>
        public IVXAINPCAction Action;

        #endregion
    }

    /// <summary>
    /// A server- or client-side action associated with an NPC turn (e.g. give item, open shop).
    /// </summary>
    [Serializable]
    public class IVXAINPCAction
    {
        #region Fields

        /// <summary>Action key, e.g. <c>give_item</c>, <c>start_quest</c>, <c>open_shop</c>.</summary>
        [JsonProperty("action_name")]
        public string ActionName;

        /// <summary>JSON-encoded parameters for the action.</summary>
        [JsonProperty("action_payload")]
        public string ActionPayload;

        /// <summary>Whether the game client has executed this action.</summary>
        [JsonProperty("executed")]
        public bool Executed;

        #endregion
    }

    /// <summary>
    /// Server-backed dialog session: identity, state, and transcript.
    /// </summary>
    [Serializable]
    public class IVXAINPCDialogSession
    {
        #region Fields

        /// <summary>Backend session identifier.</summary>
        public string SessionId;

        /// <summary>NPC this session belongs to.</summary>
        public string NpcId;

        /// <summary>Player / user identifier.</summary>
        public string PlayerId;

        /// <summary>Current conversational state.</summary>
        public IVXAINPCDialogState State;

        /// <summary>Number of completed turns (server and client may both update).</summary>
        public int TurnCount;

        /// <summary>Session start time (Unix milliseconds or agreed convention).</summary>
        public long StartTimestamp;

        /// <summary>Ordered message history for this session.</summary>
        public List<IVXAINPCDialogMessage> History;

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="IVXAINPCDialogSession"/> with an empty history list.
        /// </summary>
        public IVXAINPCDialogSession()
        {
            History = new List<IVXAINPCDialogMessage>();
        }
    }

    /// <summary>
    /// JSON payload for creating or continuing NPC dialog via the REST API.
    /// </summary>
    [Serializable]
    public class IVXAINPCDialogRequest
    {
        #region API Fields

        /// <summary>NPC identifier.</summary>
        [JsonProperty("npc_id")]
        public string NpcId;

        /// <summary>Player identifier.</summary>
        [JsonProperty("player_id")]
        public string PlayerId;

        /// <summary>Free-form context about the player or scene (optional).</summary>
        [JsonProperty("player_context")]
        public string PlayerContext;

        /// <summary>Latest player message (used when sending a turn).</summary>
        [JsonProperty("message")]
        public string Message;

        /// <summary>Existing session identifier (omit when creating a session).</summary>
        [JsonProperty("session_id")]
        public string SessionId;

        #endregion
    }

    /// <summary>
    /// JSON response from NPC dialog endpoints (session create or message).
    /// </summary>
    [Serializable]
    public class IVXAINPCDialogResponse
    {
        #region API Fields

        /// <summary>Session identifier.</summary>
        [JsonProperty("session_id")]
        public string SessionId;

        /// <summary>Natural-language reply from the NPC.</summary>
        [JsonProperty("npc_response")]
        public string NpcResponse;

        /// <summary>Optional action for the client to execute.</summary>
        [JsonProperty("action")]
        public IVXAINPCAction Action;

        /// <summary>Current turn count reported by the server.</summary>
        [JsonProperty("turn_count")]
        public int TurnCount;

        /// <summary>Whether the server considers the conversation complete.</summary>
        [JsonProperty("is_complete")]
        public bool IsComplete;

        #endregion
    }
}
