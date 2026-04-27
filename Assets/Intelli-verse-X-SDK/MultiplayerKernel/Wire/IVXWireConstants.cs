// IVX Multiplayer Kernel — wire constants.
//
// Mirrors `schemas/multiplayer/opcodes.proto`, `kernel.proto`, and the
// per-template *.proto files. The generated `IntelliVerseX.Multiplayer.V1.*`
// C# types from `tools/codegen/` carry the same values; this file exists so
// the adapter compiles without requiring `protoc` on the build host (and so
// Unity-only projects without the codegen output still have stable opcode
// constants for InputField bindings, etc.).
//
// SINGLE SOURCE OF TRUTH: `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
// If you change a value here, ALSO update:
//   - `Intelli-verse-X-SDK/schemas/multiplayer/opcodes.proto`
//   - `nakama/data/modules/src/multiplayer-kernel/types.ts MpKernel.KernelOp`
//   - `Intelli-verse-X-SDK/SDKs/javascript/multiplayer/src/wire/constants.ts`

namespace IntelliVerseX.MultiplayerKernel.Wire
{
    /// <summary>Reserved opcode ranges (opcodes.proto).</summary>
    public static class IVXOpRange
    {
        public const int KERNEL_FROM           = 0x0000;
        public const int KERNEL_TO             = 0x0FFF;
        public const int SOCIAL_FROM           = 0x1000; // ConversationalParty
        public const int SOCIAL_TO             = 0x1FFF;
        public const int AGENTS_FROM           = 0x2000;
        public const int AGENTS_TO             = 0x2FFF;
        public const int MODERATION_FROM       = 0x3000;
        public const int MODERATION_TO         = 0x3FFF;
        public const int SYNC_TURN_FROM        = 0x4000;
        public const int SYNC_TURN_TO          = 0x4FFF;
        public const int ASYNC_TURN_FROM       = 0x5000;
        public const int ASYNC_TURN_TO         = 0x5FFF;
        public const int REALTIME_TICK_FROM    = 0x6000;
        public const int REALTIME_TICK_TO      = 0x6FFF;
        public const int LOBBY_FROM            = 0x7000;
        public const int LOBBY_TO              = 0x7FFF;
        public const int TOURNAMENT_FROM       = 0x8000;
        public const int TOURNAMENT_TO         = 0x8FFF;
        public const int LIVE_EVENT_FROM       = 0x9000;
        public const int LIVE_EVENT_TO         = 0x9FFF;
        public const int PERSISTENT_PARTY_FROM = 0xA000;
        public const int PERSISTENT_PARTY_TO   = 0xAFFF;
        public const int MR_ANCHOR_FROM        = 0xB000;
        public const int MR_ANCHOR_TO          = 0xBFFF;
        public const int GAME_DEFINED_FROM     = 0xC000;
        public const int GAME_DEFINED_TO       = 0xCFFF;
        public const int XR_POSE_FROM          = 0xF000;
        public const int XR_POSE_TO            = 0xFFFF;
    }

    /// <summary>Kernel control opcodes (opcodes.proto Opcode 0x0000-0x0FFF).</summary>
    public static class IVXKernelOp
    {
        public const int CLIENT_HELLO              = 0x0001;
        public const int SERVER_HELLO              = 0x0002;
        public const int HEARTBEAT                 = 0x0003;
        public const int PLAYER_JOINED             = 0x0004;
        public const int PLAYER_LEFT               = 0x0005;
        public const int PLAYER_KICKED             = 0x0006;
        public const int MATCH_ENDED               = 0x0007;
        public const int ERROR                     = 0x0008;
        public const int MATCH_RESUME              = 0x0009;
        public const int MATCH_RESUME_ACK          = 0x000A;
        public const int LATENCY_WARNING           = 0x000B;
        public const int TICK_RATE_CHANGED         = 0x000C;
        public const int VOICE_CAPABILITY_CHANGED  = 0x000D;
        public const int VOICE_UNAVAILABLE         = 0x000E;
        public const int VOICE_MODE_CHANGED        = 0x000F;
        public const int LOW_BANDWIDTH_REQUEST     = 0x0010;
        public const int NETWORK_CLOCK_PING        = 0x0011;
        public const int NETWORK_CLOCK_PONG        = 0x0012;
        public const int WARN_RATE_LIMITED         = 0x0013;
        public const int WARN_TICK_OVERRUN         = 0x0014;
        public const int WARN_MATCH_STATE_LARGE    = 0x0015;
        public const int WARN_AVATAR_FALLBACK      = 0x0016;
        public const int WARN_DEPRECATED_CLIENT    = 0x0017;
        public const int WARN_STATE_REBUILT        = 0x0018;
        public const int CLOCK_SYNC                = 0x0019; // server-initiated periodic clock broadcast

        // Legacy aliases retained for source-compat with prior P0 callers
        // until the codegen pipeline lands. Will be removed in P3.
        public const int HELLO                     = CLIENT_HELLO;
        public const int WELCOME                   = SERVER_HELLO;
        public const int LEAVE                     = PLAYER_LEFT;
        public const int STATE_RESYNC              = WARN_STATE_REBUILT;
        public const int WARN                      = WARN_RATE_LIMITED;
    }

    /// <summary>Realtime-tick opcodes (templates/realtime_tick.proto).</summary>
    public static class IVXRealtimeTickOp
    {
        public const int TICK_INPUT               = 0x6000;
        public const int TICK_SNAPSHOT            = 0x6001;
        public const int TICK_DELTA               = 0x6002;
        public const int TICK_RECONCILE           = 0x6003;
        public const int TICK_HEARTBEAT           = 0x6004;
        public const int TICK_QUALITY_REPORT      = 0x6005;
        public const int TICK_RATE_PROPOSAL       = 0x6006;
        // P2P WebRTC handoff signaling sub-range — relayed by server.
        public const int TICK_WEBRTC_OFFER        = 0x6080;
        public const int TICK_WEBRTC_ANSWER       = 0x6081;
        public const int TICK_WEBRTC_ICE          = 0x6082;
        public const int TICK_WEBRTC_BYE          = 0x6083;
        public const int TICK_WEBRTC_HANDOFF_INFO = 0x6084;
    }

    /// <summary>Async-turn opcodes (templates/async_turn.proto).</summary>
    public static class IVXAsyncTurnOp
    {
        public const int TURN_START      = 0x5000;
        public const int TURN_SUBMIT     = 0x5001;
        public const int TURN_END        = 0x5002;
        public const int NOTIFY_OPPONENT = 0x5003;
        public const int FORFEIT         = 0x5004;
        public const int RESIGN          = 0x5005;
    }

    /// <summary>Lobby-handoff opcodes (templates/lobby_handoff.proto).</summary>
    public static class IVXLobbyHandoffOp
    {
        public const int READY        = 0x7000;
        public const int FORM_UP_DONE = 0x7001;
        public const int HANDOFF_INFO = 0x7002;
        public const int DISBAND      = 0x7003;
    }

    /// <summary>Tournament opcodes (templates/tournament.proto).</summary>
    public static class IVXTournamentOp
    {
        public const int REGISTER             = 0x8000;
        public const int REGISTRATION_CLOSED  = 0x8001;
        public const int BRACKET_UPDATED      = 0x8002;
        public const int LEG_MATCH_INFO       = 0x8003;
        public const int LEG_MATCH_RESULT     = 0x8004;
        public const int TOURNAMENT_RESOLVED  = 0x8005;
        public const int PLAYER_FORFEIT       = 0x8006;
        public const int BYE_AWARDED          = 0x8007;
    }

    /// <summary>Live-event opcodes (templates/live_event.proto).</summary>
    public static class IVXLiveEventOp
    {
        public const int PHASE_CHANGED      = 0x9000;
        public const int REACTION           = 0x9001;
        public const int DROP_AWARDED       = 0x9002;
        public const int EVENT_PROGRESS     = 0x9003;
        public const int PARTICIPATION_LOG  = 0x9004;
        public const int EVENT_CHAT         = 0x9005;
        public const int EVENT_SIGNAL       = 0x9006;
        public const int QUEUED             = 0x9007;
        public const int TIME_TO_START      = 0x9008;
    }

    /// <summary>Persistent-party opcodes (templates/persistent_party.proto).</summary>
    public static class IVXPersistentPartyOp
    {
        public const int PARTY_STATE       = 0xA000;
        public const int INVITE            = 0xA001;
        public const int INVITE_ACCEPT     = 0xA002;
        public const int INVITE_DECLINE    = 0xA003;
        public const int KICK              = 0xA004;
        public const int PROMOTE           = 0xA005;
        public const int DEMOTE            = 0xA006;
        public const int TRANSFER_OWNER    = 0xA007;
        public const int LEAVE_PARTY       = 0xA008;
        public const int SETTING_UPDATED   = 0xA009;
        public const int PARTY_CHAT        = 0xA00A;
        public const int MEMBER_PRESENCE   = 0xA00B;
        public const int READY_FOR_MATCH   = 0xA00C;
        public const int MATCH_QUEUE_INFO  = 0xA00D;
    }

    /// <summary>
    /// Avatar-replication opcodes (templates/avatar_replication.proto, XR_POSE range 0xF000-0xFFFF).
    /// Mirrors `data/modules/avatar_replication/main.go` and the JS WebXR adapter
    /// (<c>SDKs/javascript/.../webxr/adapter.ts</c>). Used by <c>IVXAvatarReplicator</c>.
    /// </summary>
    public static class IVXAvatarOp
    {
        public const int HEAD_POSE         = 0xF000; // PoseQuantized (sender id stamped by server)
        public const int LEFT_HAND_POSE    = 0xF001; // HandPose (is_left implied by opcode)
        public const int RIGHT_HAND_POSE   = 0xF002; // HandPose (is_left implied by opcode)
        public const int BLENDSHAPES       = 0xF003; // FaceBlendshapes
        public const int FINGER_CURLS      = 0xF004; // FingerPose
        public const int AVATAR_DESCRIPTOR = 0xF005; // AvatarDescriptor (avatar_v1.proto)
        public const int LOD_HINT          = 0xF006; // AvatarLOD
        public const int PEER_LEFT         = 0xF007; // {user_id, reason}
        public const int AVATAR_FALLBACK   = 0xF008; // {user_id, reason}
    }

    /// <summary>Sync-turn opcodes (templates/sync_turn.proto).</summary>
    public static class IVXSyncTurnOp
    {
        public const int TURN_START         = 0x4001;
        public const int TURN_INPUT_OPENED  = 0x4002;
        public const int TURN_INPUT_CLOSED  = 0x4003;
        public const int TURN_RESOLVED      = 0x4004;
        public const int SCORE_UPDATE       = 0x4005;
        public const int PLAYER_ELIMINATED  = 0x4006;
        public const int ROUND_STARTED      = 0x4007;
        public const int ROUND_ENDED        = 0x4008;

        public const int TURN_INPUT_SUBMIT  = 0x4010;
        public const int PLAYER_READY       = 0x4011;
        public const int PLAYER_FORFEIT     = 0x4012;
    }

    /// <summary>
    /// Canonical error codes (envelope.proto ErrorCode). Adapters MUST surface
    /// the integer code even when a generated enum predates a value
    /// (forward-compat). See `docs/multiplayer/error-taxonomy.md` for retry
    /// guidance per range.
    /// </summary>
    public static class IVXErrorCode
    {
        public const int UNSPECIFIED              = 0;
        // 1-9 — schema / time
        public const int SCHEMA_TOO_OLD           = 1;
        public const int SERVER_TOO_OLD           = 2;
        public const int BAD_PAYLOAD              = 3;
        public const int SEQ_GAP                  = 4;
        public const int UNKNOWN_OPCODE           = 5;
        public const int DUPLICATE_OPCODE         = 6;
        public const int CLOCK_SKEW_EXTREME       = 7;
        public const int MATCH_STATE_LARGE        = 8;
        // 20-29 — capacity / membership
        public const int MATCH_FULL               = 20;
        public const int MATCH_NOT_FOUND          = 21;
        public const int NOT_A_MEMBER             = 22;
        public const int RATE_LIMITED             = 23;
        public const int FLAPPING                 = 24;
        public const int MATCH_ENDED              = 25;
        public const int SESSION_REPLACED         = 26;
        // 30-39 — auth / permission
        public const int PERMISSION_DENIED        = 30;
        public const int KICKED                   = 31;
        public const int BANNED                   = 32;
        public const int NOT_AUTHORIZED           = 33;
        // 40-49 — agent
        public const int BAD_PERSONA              = 40;
        public const int AGENT_BUDGET_EXCEEDED    = 41;
        public const int AGENT_PROVIDER_DOWN      = 42;
        // 50-59 — XR / spatial
        public const int ANCHOR_INCOMPAT          = 50;
        public const int ANCHOR_LOST              = 51;
        // 60-69 — voice
        public const int VOICE_UNAVAILABLE        = 60;
        public const int VOICE_PERMISSION_DENIED  = 61;
        // 70-79 — moderation
        public const int MODERATION_BLOCKED       = 70;
        // 80-89 — lifecycle (match-fatal)
        public const int TIMEOUT                  = 80;
        public const int QUORUM_LOST              = 81;
        public const int DURATION_EXCEEDED        = 82;
        public const int STATE_OVERFLOW           = 83;
        // 90-99 — capability
        public const int CAPABILITY_UNSUPPORTED   = 90;
        // 100-119 — infra
        public const int OVERLOAD                 = 100;
        public const int PERSISTENCE_DEGRADED     = 101;
        public const int TICK_OVERRUN_DEGRADED    = 102;
        public const int PROVIDER_UNAVAILABLE     = 103;
        // catch-all
        public const int INTERNAL                 = 999;
    }

    /// <summary>
    /// Canonical warning codes (envelope.proto WarningCode). Warnings never
    /// end a match; adapters surface them via OnWarning(WarningCode, detail).
    /// </summary>
    public static class IVXWarningCode
    {
        public const int UNSPECIFIED         = 0;
        public const int RATE_LIMITED        = 1;
        public const int TICK_OVERRUN        = 2;
        public const int MATCH_STATE_LARGE   = 3;
        public const int AVATAR_FALLBACK     = 4;
        public const int DEPRECATED_CLIENT   = 5;
        public const int STATE_REBUILT       = 6;
        public const int LOW_BANDWIDTH       = 7;
        public const int AGENT_DEGRADED      = 8;
        public const int CLOCK_REALIGN       = 9;
    }

    /// <summary>kernel.proto LeaveReason mirror.</summary>
    public static class IVXLeaveReason
    {
        public const int UNSPECIFIED = 0;
        public const int VOLUNTARY   = 1;
        public const int DISCONNECT  = 2;
        public const int KICK        = 3;
        public const int BAN         = 4;
        public const int TIMEOUT     = 5;
        public const int FLAPPING    = 6;
        public const int MATCH_ENDED = 7;
    }

    /// <summary>kernel.proto EndReason mirror.</summary>
    public static class IVXEndReason
    {
        public const int UNSPECIFIED       = 0;
        public const int COMPLETED         = 1;
        public const int TIMEOUT           = 2;
        public const int QUORUM_LOST       = 3;
        public const int HOST_DISBAND      = 4;
        public const int KICKED_ALL        = 5;
        public const int DURATION_EXCEEDED = 6;
        public const int KERNEL_INTERNAL   = 7;
        // P5: distinct from completion — lobby disbanded before handoff,
        // async-game cancelled, voluntary teardown by host, etc.
        public const int CANCELLED         = 8;
    }

    /// <summary>Wire-version constant; bump on breaking envelope changes.</summary>
    public static class IVXWireVersion
    {
        public const int V1 = 1;
    }
}
