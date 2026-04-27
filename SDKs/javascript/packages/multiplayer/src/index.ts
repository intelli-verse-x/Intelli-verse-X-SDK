// @intelliversex/multiplayer — JavaScript/TypeScript adapter for the IVX
// Multiplayer Kernel running on Nakama.
//
// Implements the engine-agnostic IIVXMultiplayer contract over
// @heroiclabs/nakama-js so a browser game written against this package
// ports unchanged to Unity, Unreal, Godot, or any future engine adapter.
//
// Wire / opcode constants mirror `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`
// — that proto3 contract is the single source of truth.

export * from "./api";
export * from "./client";
export * from "./session";
export * from "./wire/constants";
export * from "./wire/envelope";
export {
  IVXSyncTurnClient,
  type SyncTurnStartPayload,
  type SyncTurnInputOpenedPayload,
  type SyncTurnInputClosedPayload,
  type SyncTurnResolvedPayload,
  type SyncTurnScoreUpdatePayload,
  type SyncTurnInputSubmitPayload,
} from "./templates/sync-turn";

export * from "./avatar";
export * from "./voice";
export * from "./webxr";
export * from "./discord";
