// Reserved opcode ranges + named kernel/template opcodes.
//
// SINGLE SOURCE OF TRUTH: `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
// This file is hand-maintained until the codegen pipeline lands; values
// MUST match the proto enum values exactly.
//
// If you change a value here, ALSO update:
//   - `Intelli-verse-X-SDK/schemas/multiplayer/opcodes.proto` (canonical)
//   - `Intelli-verse-X-SDK/Assets/Intelli-verse-X-SDK/MultiplayerKernel/Wire/IVXWireConstants.cs`
//   - `nakama/data/modules/src/multiplayer-kernel/types.ts MpKernel.KernelOp`

export const IVXOpRange = {
  /** Kernel control range (Hello / Heartbeat / Player events / ClockSync / Error). */
  KERNEL_FROM:                0x0000,
  KERNEL_TO:                  0x0FFF,

  /** Social — ConversationalParty template. */
  SOCIAL_FROM:                0x1000,
  SOCIAL_TO:                  0x1FFF,

  /** AI agent kernel service. */
  AGENTS_FROM:                0x2000,
  AGENTS_TO:                  0x2FFF,

  /** Moderation kernel service. */
  MODERATION_FROM:            0x3000,
  MODERATION_TO:              0x3FFF,

  /** SyncTurnMatch template. */
  SYNC_TURN_FROM:             0x4000,
  SYNC_TURN_TO:               0x4FFF,

  /** AsyncTurnMatch template. */
  ASYNC_TURN_FROM:            0x5000,
  ASYNC_TURN_TO:              0x5FFF,

  /** RealtimeTickMatch (Go fast-path). */
  REALTIME_TICK_FROM:         0x6000,
  REALTIME_TICK_TO:           0x6FFF,

  /** LobbyHandoffMatch template. */
  LOBBY_FROM:                 0x7000,
  LOBBY_TO:                   0x7FFF,

  /** TournamentOrchestrator template. */
  TOURNAMENT_FROM:            0x8000,
  TOURNAMENT_TO:              0x8FFF,

  /** LiveEventRoom template. */
  LIVE_EVENT_FROM:            0x9000,
  LIVE_EVENT_TO:              0x9FFF,

  /** PersistentPartyRoom template. */
  PERSISTENT_PARTY_FROM:      0xA000,
  PERSISTENT_PARTY_TO:        0xAFFF,

  /** MixedRealityAnchorMatch template (anchor + spatial frame). */
  MR_ANCHOR_FROM:             0xB000,
  MR_ANCHOR_TO:               0xBFFF,

  /** Game-defined opcodes (per-game range). */
  GAME_DEFINED_FROM:          0xC000,
  GAME_DEFINED_TO:            0xCFFF,

  /** XR pose / AvatarReplication fast-path. */
  XR_POSE_FROM:               0xF000,
  XR_POSE_TO:                 0xFFFF,
} as const;

/** Kernel-control opcodes (0x0000-0x0FFF). Mirror of `Opcode` enum. */
export const IVXKernelOp = {
  CLIENT_HELLO:               0x0001,
  SERVER_HELLO:               0x0002,
  HEARTBEAT:                  0x0003,
  PLAYER_JOINED:              0x0004,
  PLAYER_LEFT:                0x0005,
  PLAYER_KICKED:              0x0006,
  MATCH_ENDED:                0x0007,
  ERROR:                      0x0008,
  MATCH_RESUME:               0x0009,
  MATCH_RESUME_ACK:           0x000A,
  LATENCY_WARNING:            0x000B,
  TICK_RATE_CHANGED:          0x000C,
  VOICE_CAPABILITY_CHANGED:   0x000D,
  VOICE_UNAVAILABLE:          0x000E,
  VOICE_MODE_CHANGED:         0x000F,
  LOW_BANDWIDTH_REQUEST:      0x0010,
  NETWORK_CLOCK_PING:         0x0011,
  NETWORK_CLOCK_PONG:         0x0012,
  WARN_RATE_LIMITED:          0x0013,
  WARN_TICK_OVERRUN:          0x0014,
  WARN_MATCH_STATE_LARGE:     0x0015,
  WARN_AVATAR_FALLBACK:       0x0016,
  WARN_DEPRECATED_CLIENT:     0x0017,
  WARN_STATE_REBUILT:         0x0018,
  CLOCK_SYNC:                 0x0019,
  // Legacy aliases retained for source-compat. Will be removed in P3 cleanup.
  HELLO:                      0x0001,
  WELCOME:                    0x0002,
  LEAVE:                      0x0005,
} as const;

export const IVXSyncTurnOp = {
  TURN_START:                 0x4001,
  TURN_INPUT_OPENED:          0x4002,
  TURN_INPUT_CLOSED:          0x4003,
  TURN_RESOLVED:              0x4004,
  SCORE_UPDATE:               0x4005,
  PLAYER_ELIMINATED:          0x4006,
  ROUND_STARTED:              0x4007,
  ROUND_ENDED:                0x4008,
  TURN_INPUT_SUBMIT:          0x4010,
  PLAYER_READY:               0x4011,
  PLAYER_FORFEIT:             0x4012,
} as const;

export const IVXAsyncTurnOp = {
  TURN_START:                 0x5000,
  TURN_SUBMIT:                0x5001,
  TURN_END:                   0x5002,
  NOTIFY_OPPONENT:            0x5003,
  FORFEIT:                    0x5004,
  RESIGN:                     0x5005,
} as const;

/** RealtimeTickMatch opcodes (Go fast-path). */
export const IVXRealtimeTickOp = {
  TICK_INPUT:                 0x6000,
  TICK_SNAPSHOT:              0x6001,
  TICK_DELTA:                 0x6002,
  TICK_RECONCILE:             0x6003,
  TICK_HEARTBEAT:             0x6004,
  TICK_QUALITY_REPORT:        0x6005,
  TICK_RATE_PROPOSAL:         0x6006,
  // P2P WebRTC handoff signaling sub-range — relayed by server.
  TICK_WEBRTC_OFFER:          0x6080,
  TICK_WEBRTC_ANSWER:         0x6081,
  TICK_WEBRTC_ICE:            0x6082,
  TICK_WEBRTC_BYE:            0x6083,
  TICK_WEBRTC_HANDOFF_INFO:   0x6084,
} as const;

export const IVXLobbyHandoffOp = {
  READY:                      0x7000,
  FORM_UP_DONE:               0x7001,
  HANDOFF_INFO:               0x7002,
  DISBAND:                    0x7003,
} as const;

/** TournamentOrchestrator opcodes (templates/tournament.proto). */
export const IVXTournamentOp = {
  REGISTER:             0x8000,
  REGISTRATION_CLOSED:  0x8001,
  BRACKET_UPDATED:      0x8002,
  LEG_MATCH_INFO:       0x8003,
  LEG_MATCH_RESULT:     0x8004,
  TOURNAMENT_RESOLVED:  0x8005,
  PLAYER_FORFEIT:       0x8006,
  BYE_AWARDED:          0x8007,
} as const;

/** LiveEventRoom opcodes (templates/live_event.proto). */
export const IVXLiveEventOp = {
  PHASE_CHANGED:        0x9000,
  REACTION:             0x9001,
  DROP_AWARDED:         0x9002,
  EVENT_PROGRESS:       0x9003,
  PARTICIPATION_LOG:    0x9004,
  EVENT_CHAT:           0x9005,
  EVENT_SIGNAL:         0x9006,
  QUEUED:               0x9007,
  TIME_TO_START:        0x9008,
} as const;

/** PersistentPartyRoom opcodes (templates/persistent_party.proto). */
export const IVXPersistentPartyOp = {
  PARTY_STATE:        0xA000,
  INVITE:             0xA001,
  INVITE_ACCEPT:      0xA002,
  INVITE_DECLINE:     0xA003,
  KICK:               0xA004,
  PROMOTE:            0xA005,
  DEMOTE:             0xA006,
  TRANSFER_OWNER:     0xA007,
  LEAVE_PARTY:        0xA008,
  SETTING_UPDATED:    0xA009,
  PARTY_CHAT:         0xA00A,
  MEMBER_PRESENCE:    0xA00B,
  READY_FOR_MATCH:    0xA00C,
  MATCH_QUEUE_INFO:   0xA00D,
} as const;

/**
 * Canonical error codes (envelope.proto ErrorCode).
 * See `docs/multiplayer/error-taxonomy.md` for retry-policy guidance per range.
 * Adapters MUST surface the integer code even when their generated enum
 * predates a value (forward-compat).
 */
export const IVXErrorCode = {
  UNSPECIFIED:                0,
  // 1-9 — schema / time / frame
  SCHEMA_TOO_OLD:             1,
  SERVER_TOO_OLD:             2,
  BAD_PAYLOAD:                3,
  SEQ_GAP:                    4,
  UNKNOWN_OPCODE:             5,
  DUPLICATE_OPCODE:           6,
  CLOCK_SKEW_EXTREME:         7,
  MATCH_STATE_LARGE:          8,
  // 20-29 — capacity / membership
  MATCH_FULL:                 20,
  MATCH_NOT_FOUND:            21,
  NOT_A_MEMBER:               22,
  RATE_LIMITED:               23,
  FLAPPING:                   24,
  MATCH_ENDED:                25,
  SESSION_REPLACED:           26,
  // 30-39 — auth / permission
  PERMISSION_DENIED:          30,
  KICKED:                     31,
  BANNED:                     32,
  NOT_AUTHORIZED:             33,
  // 40-49 — agent
  BAD_PERSONA:                40,
  AGENT_BUDGET_EXCEEDED:      41,
  AGENT_PROVIDER_DOWN:        42,
  // 50-59 — XR / spatial
  ANCHOR_INCOMPAT:            50,
  ANCHOR_LOST:                51,
  // 60-69 — voice
  VOICE_UNAVAILABLE:          60,
  VOICE_PERMISSION_DENIED:    61,
  // 70-79 — moderation
  MODERATION_BLOCKED:         70,
  // 80-89 — lifecycle (match-fatal)
  TIMEOUT:                    80,
  QUORUM_LOST:                81,
  DURATION_EXCEEDED:          82,
  STATE_OVERFLOW:             83,
  // 90-99 — capability
  CAPABILITY_UNSUPPORTED:     90,
  // 100-119 — infra (transient)
  OVERLOAD:                   100,
  PERSISTENCE_DEGRADED:       101,
  TICK_OVERRUN_DEGRADED:      102,
  PROVIDER_UNAVAILABLE:       103,
  // catch-all
  INTERNAL:                   999,
} as const;

/**
 * Canonical warning codes (envelope.proto WarningCode). Warnings never end
 * a match; adapters surface them via OnWarning(WarningCode, detail).
 */
export const IVXWarningCode = {
  UNSPECIFIED:        0,
  RATE_LIMITED:       1,
  TICK_OVERRUN:       2,
  MATCH_STATE_LARGE:  3,
  AVATAR_FALLBACK:    4,
  DEPRECATED_CLIENT:  5,
  STATE_REBUILT:      6,
  LOW_BANDWIDTH:      7,
  AGENT_DEGRADED:     8,
  CLOCK_REALIGN:      9,
} as const;

/** kernel.proto LeaveReason mirror. */
export const IVXLeaveReason = {
  UNSPECIFIED:                0,
  VOLUNTARY:                  1,
  DISCONNECT:                 2,
  KICK:                       3,
  BAN:                        4,
  TIMEOUT:                    5,
  FLAPPING:                   6,
  MATCH_ENDED:                7,
} as const;

/** kernel.proto EndReason mirror. */
export const IVXEndReason = {
  UNSPECIFIED:                0,
  COMPLETED:                  1,
  TIMEOUT:                    2,
  QUORUM_LOST:                3,
  HOST_DISBAND:               4,
  KICKED_ALL:                 5,
  DURATION_EXCEEDED:          6,
  KERNEL_INTERNAL:            7,
  // P5: distinct from completion — lobby disbanded before handoff,
  // async-game cancelled, voluntary teardown by host, etc.
  CANCELLED:                  8,
} as const;

export const IVXWireVersion = {
  V1: 1,
} as const;

/** Reserved Nakama match metadata key carrying the locale (BCP-47). */
export const IVX_META_LOCALE = "locale";
/** Reserved Nakama match metadata key carrying the client build identifier. */
export const IVX_META_CLIENT_BUILD = "client_build_id";
/** Reserved Nakama match metadata key carrying capability tags ("voice:livekit"). */
export const IVX_META_CAPABILITIES = "capabilities";
