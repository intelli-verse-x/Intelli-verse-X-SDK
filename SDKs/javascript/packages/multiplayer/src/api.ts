// Public API surface for `@intelliversex/multiplayer`.
//
// Mirrors the IIVXMultiplayer / IIVXMatchSession contracts from the Unity
// adapter so a browser game written against this package ports to Unity
// (or any other engine adapter) without rewrites.
//
// SINGLE SOURCE OF TRUTH: `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.

import type {
  IVXEnvelope,
  IVXError,
  IVXHeader,
} from "./wire/envelope";

// ---------- transport state ----------

export enum IVXTransportState {
  Disconnected = 0,
  Connecting = 1,
  Connected = 2,
  Reconnecting = 3,
  FailedFatal = 4,
}

// ---------- adapter contract ----------

/**
 * Top-level multiplayer adapter contract. The Nakama-JS, Unity, Unreal,
 * Godot, etc. adapters all implement this same logical surface so a
 * game written against it ports without rewrites.
 */
export interface IIVXMultiplayer {
  /** Has the adapter been initialised? Idempotent. */
  readonly isInitialized: boolean;

  /** Is the underlying realtime socket open? */
  readonly isConnected: boolean;

  /** Last server unix-ms we observed (clock authority). */
  readonly lastServerUnixMs: number;

  /** Initialise the adapter. Must be called after Nakama auth. Idempotent. */
  initialize(): Promise<void>;

  /** Tear down sockets, joined matches, voice provider. Safe to call multiple times. */
  shutdown(): Promise<void>;

  /** Create a new match via the kernel `mp_create_match` RPC. */
  createMatch(req: IVXCreateMatchRequest): Promise<IVXCreateMatchResponse>;

  /** Join an existing match by id. Returns a live session handle. */
  joinMatch(matchId: string, options?: IVXJoinOptions): Promise<IIVXMatchSession>;

  /** Convenience: create + join in one go. */
  createAndJoin(
    req: IVXCreateMatchRequest,
    options?: IVXJoinOptions,
  ): Promise<IIVXMatchSession>;

  /** Subscribe to top-level kernel errors (every match-session error is also fanned-out here). */
  onKernelError(handler: (e: IVXKernelEvent<IVXError>) => void): IVXSubscription;

  /** Subscribe to transport-state transitions. */
  onTransportStateChanged(handler: (s: IVXTransportState) => void): IVXSubscription;
}

// ---------- session contract ----------

export interface IIVXMatchSession {
  readonly matchId: string;
  readonly templateId: string;
  readonly localUserId: string;
  readonly currentMatchTimeMs: number;
  readonly activePlayerCount: number;
  readonly state: IVXTransportState;

  /** Subscribe to a single opcode. Disposing the returned token unsubscribes. */
  subscribe<TPayload = unknown>(
    opcode: number,
    handler: (e: IVXKernelEvent<TPayload>) => void,
  ): IVXSubscription;

  /** Subscribe to ALL opcodes in [from..to]. Useful for game-defined ranges. */
  subscribeRange(
    opcodeFrom: number,
    opcodeTo: number,
    handler: (e: IVXRawKernelEvent) => void,
  ): IVXSubscription;

  /** Send `payload` to the server. Header (seq, match_time_ms, uuid) auto-stamped. */
  send<TPayload>(opcode: number, payload: TPayload): Promise<void>;

  /** Send a pre-built envelope (advanced). */
  sendEnvelope<TPayload>(env: IVXEnvelope<TPayload>): Promise<void>;

  /** Politely leave the match (transport-level Nakama leave; server fans out PLAYER_LEFT). */
  leave(): Promise<void>;

  /** Lifecycle events. */
  onWelcome(h: (e: IVXKernelEvent<IVXWelcomePayload>) => void): IVXSubscription;
  onPlayerJoined(h: (e: IVXKernelEvent<IVXPlayerJoinedPayload>) => void): IVXSubscription;
  onPlayerLeft(h: (e: IVXKernelEvent<IVXPlayerLeftPayload>) => void): IVXSubscription;
  onMatchEnded(h: (e: IVXKernelEvent<IVXMatchEndedPayload>) => void): IVXSubscription;
  onError(h: (e: IVXKernelEvent<IVXError>) => void): IVXSubscription;
  onStateChanged(h: (s: IVXTransportState) => void): IVXSubscription;

  /** Dispose the session. Safe to call multiple times. */
  dispose(): void;
}

// ---------- request/response shapes ----------

export interface IVXCreateMatchRequest {
  templateId: string;
  gameId?: string;
  region?: string;
  templateInit?: Record<string, unknown>;
}

export interface IVXCreateMatchResponse {
  match_id: string;
  template_id: string;
  region?: string;
  expires_unix_ms?: number;
}

export interface IVXJoinOptions {
  preferredLocale?: string;
  clientBuildId?: string;
  capabilities?: string[];
  /** Outbound rate limit. Default 30/s. */
  outboundOpsPerSecondLimit?: number;
}

// ---------- kernel payloads ----------

export interface IVXWelcomePayload {
  match_id: string;
  assigned_user_id: string;
  server_match_time_ms: number;
  server_unix_ms: number;
  feature_flags?: number;
  reconnect_grace_ms_remaining?: number;
}

export interface IVXPlayerJoinedPayload {
  user_id: string;
  is_agent: boolean;
  display_name?: string;
}

export interface IVXPlayerLeftPayload {
  user_id: string;
  /** kernel.proto LeaveReason. */
  reason: number;
}

export interface IVXMatchEndedPayload {
  /** kernel.proto EndReason. */
  reason: number;
  result_envelope?: unknown;
}

export interface IVXClockSyncPayload {
  server_unix_ms: number;
  server_match_time_ms: number;
  client_unix_ms_echo: number;
}

// ---------- event shapes ----------

export interface IVXKernelEvent<TPayload> {
  header: IVXHeader;
  payload: TPayload;
  recvUnixMs: number;
}

export interface IVXRawKernelEvent {
  header: IVXHeader;
  payloadJson: string | null;
  recvUnixMs: number;
}

export interface IVXSubscription {
  dispose(): void;
}
