// IVXNakamaMultiplayer — `IIVXMultiplayer` over @heroiclabs/nakama-js.
//
// Owns the Nakama IClient + ISocket, the active match-session map, and
// fans inbound MatchState / MatchPresence events out to the right session.
// Sends a CLIENT_HELLO immediately after each join so the server can
// renegotiate schema / record capabilities.

import type {
  Client,
  Match,
  MatchData,
  MatchPresenceEvent,
  Session,
  Socket,
} from "@heroiclabs/nakama-js";

import {
  IVX_META_CAPABILITIES,
  IVX_META_CLIENT_BUILD,
  IVX_META_LOCALE,
  IVXKernelOp,
  IVXWireVersion,
} from "./wire/constants";
import {
  IVXTransportState,
  type IIVXMultiplayer,
  type IIVXMatchSession,
  type IVXCreateMatchRequest,
  type IVXCreateMatchResponse,
  type IVXJoinOptions,
  type IVXKernelEvent,
  type IVXSubscription,
} from "./api";
import type { IVXError } from "./wire/envelope";
import { IVXMatchSession } from "./session";

const RPC_CREATE_MATCH = "mp_create_match";

export interface IVXNakamaClientOptions {
  /** Default outbound ops/sec limit applied to sessions that don't override. */
  defaultOutboundOpsPerSecondLimit?: number;
  /** Optional console logger override. Defaults to `console`. */
  logger?: { log: (m: string) => void; warn: (m: string) => void; error: (m: string) => void };
}

export class IVXNakamaMultiplayer implements IIVXMultiplayer {
  private readonly _client: Client;
  private readonly _session: Session;
  private readonly _socket: Socket;
  private readonly _opts: IVXNakamaClientOptions;
  private readonly _sessions = new Map<string, IVXMatchSession>();

  private readonly _kernelErrorHandlers: ((e: IVXKernelEvent<IVXError>) => void)[] = [];
  private readonly _stateHandlers: ((s: IVXTransportState) => void)[] = [];

  private _initialized = false;
  private _disposed = false;
  private _state: IVXTransportState = IVXTransportState.Disconnected;
  private _lastServerUnixMs = 0;

  get isInitialized(): boolean { return this._initialized; }
  get isConnected(): boolean { return Boolean(this._socket && (this._socket as unknown as { isOpen?: () => boolean }).isOpen?.()); }
  get lastServerUnixMs(): number { return this._lastServerUnixMs; }

  constructor(opts: { client: Client; session: Session; socket: Socket; options?: IVXNakamaClientOptions }) {
    if (!opts.client) throw new Error("[IVXNakamaMultiplayer] client required");
    if (!opts.session) throw new Error("[IVXNakamaMultiplayer] session required");
    if (!opts.socket) throw new Error("[IVXNakamaMultiplayer] socket required");
    this._client = opts.client;
    this._session = opts.session;
    this._socket = opts.socket;
    this._opts = opts.options ?? {};
  }

  async initialize(): Promise<void> {
    if (this._initialized) return;
    // nakama-js Socket exposes onmatchdata / onmatchpresence as callbacks.
    this._socket.onmatchdata = (state: MatchData) => this._onMatchData(state);
    this._socket.onmatchpresence = (ev: MatchPresenceEvent) => this._onMatchPresence(ev);
    this._socket.ondisconnect = () => this._setTransportState(IVXTransportState.Disconnected);
    this._socket.onerror = (e: Event) => {
      const log = this._opts.logger ?? console;
      log.error(`[IVXNakamaMultiplayer] socket error: ${(e as ErrorEvent).message ?? "(unknown)"}`);
      this._setTransportState(IVXTransportState.Reconnecting);
    };
    this._setTransportState(this.isConnected ? IVXTransportState.Connected : IVXTransportState.Connecting);
    this._initialized = true;
  }

  async shutdown(): Promise<void> {
    if (!this._initialized) return;
    const snapshot = Array.from(this._sessions.values());
    for (const s of snapshot) {
      try { await s.leave(); } catch { /* ignore */ }
      s.dispose();
    }
    this._sessions.clear();
    this._initialized = false;
    this._disposed = true;
    this._setTransportState(IVXTransportState.Disconnected);
  }

  // ----- match factory -----

  async createMatch(req: IVXCreateMatchRequest): Promise<IVXCreateMatchResponse> {
    this._ensureReady();
    if (!req || !req.templateId) throw new Error("[IVXNakamaMultiplayer] templateId required");
    // nakama-js `rpc` takes an object input which it serialises internally;
    // do NOT pre-serialise here or the server sees a JSON-encoded string.
    const payload = {
      template_id:   req.templateId,
      game_id:       req.gameId ?? "",
      region:        req.region ?? "",
      template_init: req.templateInit ?? {},
    };
    const resp = await this._client.rpc(this._session, RPC_CREATE_MATCH, payload);
    let parsed: IVXCreateMatchResponse;
    try {
      parsed = (typeof resp.payload === "string"
        ? JSON.parse(resp.payload)
        : resp.payload) as IVXCreateMatchResponse;
    } catch (e) {
      throw new Error(`[IVXNakamaMultiplayer] mp_create_match decode failed: ${(e as Error).message}`);
    }
    if (!parsed?.match_id) {
      throw new Error("[IVXNakamaMultiplayer] mp_create_match returned empty match_id");
    }
    return parsed;
  }

  async joinMatch(matchId: string, options?: IVXJoinOptions): Promise<IIVXMatchSession> {
    this._ensureReady();
    if (!matchId) throw new Error("[IVXNakamaMultiplayer] matchId required");

    const opts: IVXJoinOptions = {
      ...options,
      outboundOpsPerSecondLimit:
        options?.outboundOpsPerSecondLimit ?? this._opts.defaultOutboundOpsPerSecondLimit,
    };

    const metadata: Record<string, string> = {};
    if (opts.preferredLocale) metadata[IVX_META_LOCALE] = opts.preferredLocale;
    if (opts.clientBuildId) metadata[IVX_META_CLIENT_BUILD] = opts.clientBuildId;
    if (opts.capabilities && opts.capabilities.length > 0) {
      metadata[IVX_META_CAPABILITIES] = opts.capabilities.join(",");
    }

    let match: Match;
    try {
      match = await this._socket.joinMatch(matchId, undefined, metadata);
    } catch (e) {
      throw new Error(`[IVXNakamaMultiplayer] joinMatch failed: ${(e as Error).message}`);
    }

    const session = new IVXMatchSession({
      socket: this._socket,
      match,
      options: opts,
      onClockSampled: (t) => { this._lastServerUnixMs = t; },
      onSelfDispose: (id) => { this._sessions.delete(id); },
    });
    this._sessions.set(match.match_id, session);

    // Send CLIENT_HELLO so the server can renegotiate schema/feature flags
    // after a transient disconnect (Pillar 8: schema-version handshake).
    try {
      await session.send(IVXKernelOp.CLIENT_HELLO, {
        client_protocol_version: IVXWireVersion.V1,
        client_capabilities:     opts.capabilities ?? [],
        client_unix_ms:          Date.now(),
        preferred_locale:        opts.preferredLocale ?? "",
        client_build_id:         opts.clientBuildId ?? "",
        voice_provider_hint:     "",
      });
    } catch (e) {
      const log = this._opts.logger ?? console;
      log.warn(`[IVXNakamaMultiplayer] CLIENT_HELLO send failed (continuing): ${(e as Error).message}`);
    }
    return session;
  }

  async createAndJoin(req: IVXCreateMatchRequest, options?: IVXJoinOptions): Promise<IIVXMatchSession> {
    const created = await this.createMatch(req);
    return this.joinMatch(created.match_id, options);
  }

  // ----- diagnostics -----

  onKernelError(handler: (e: IVXKernelEvent<IVXError>) => void): IVXSubscription {
    this._kernelErrorHandlers.push(handler);
    return { dispose: () => {
      const i = this._kernelErrorHandlers.indexOf(handler);
      if (i >= 0) this._kernelErrorHandlers.splice(i, 1);
    } };
  }

  onTransportStateChanged(handler: (s: IVXTransportState) => void): IVXSubscription {
    this._stateHandlers.push(handler);
    return { dispose: () => {
      const i = this._stateHandlers.indexOf(handler);
      if (i >= 0) this._stateHandlers.splice(i, 1);
    } };
  }

  // ----- inbound -----

  private _onMatchData(state: MatchData): void {
    const session = this._sessions.get(state.match_id);
    if (!session) return;
    session.handleMatchState({ match_id: state.match_id, op_code: Number(state.op_code), data: state.data });

    // Fan ERROR envelopes up to top-level subscribers too.
    if (Number(state.op_code) === IVXKernelOp.ERROR && this._kernelErrorHandlers.length > 0) {
      // Re-decode minimally; sessions already routed payloads to their subscribers.
      // Top-level is best-effort observability, never blocking.
      try {
        const json = typeof state.data === "string" ? state.data : new TextDecoder().decode(state.data);
        const env = JSON.parse(json) as { h: { op: number }; p: IVXError };
        if (env?.p) {
          for (const h of this._kernelErrorHandlers.slice()) {
            try { h({ header: env.h as never, payload: env.p, recvUnixMs: Date.now() }); } catch { /* ignore */ }
          }
        }
      } catch { /* ignore */ }
    }
  }

  private _onMatchPresence(ev: MatchPresenceEvent): void {
    const session = this._sessions.get(ev.match_id);
    if (!session) return;
    session.handlePresence({
      joins:  ev.joins?.map((p) => ({ user_id: p.user_id })),
      leaves: ev.leaves?.map((p) => ({ user_id: p.user_id })),
    });
  }

  private _setTransportState(s: IVXTransportState): void {
    if (this._state === s) return;
    this._state = s;
    for (const h of this._stateHandlers.slice()) {
      try { h(s); } catch { /* ignore */ }
    }
    for (const session of this._sessions.values()) session.setTransportState(s);
  }

  private _ensureReady(): void {
    if (this._disposed) throw new Error("[IVXNakamaMultiplayer] disposed");
    if (!this._initialized) throw new Error("[IVXNakamaMultiplayer] call initialize() first");
  }
}
