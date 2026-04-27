// Per-match session — implements IIVXMatchSession over a single
// @heroiclabs/nakama-js Match + Socket. Owns:
//  - inbound dispatcher map (per-opcode + range subscribers)
//  - outbound seq counter (per-(match, sender))
//  - outbound token-bucket rate limit
//  - server-clock sampling on Welcome / NETWORK_CLOCK_PONG / CLOCK_SYNC

import type { Socket, Match } from "@heroiclabs/nakama-js";

import {
  IVXKernelOp,
  IVXWireVersion,
} from "./wire/constants";
import {
  buildEnvelope,
  decodeEnvelope,
  encodeEnvelope,
  type IVXEnvelope,
  type IVXError,
  type IVXHeader,
} from "./wire/envelope";
import {
  IVXTransportState,
  type IIVXMatchSession,
  type IVXClockSyncPayload,
  type IVXJoinOptions,
  type IVXKernelEvent,
  type IVXMatchEndedPayload,
  type IVXPlayerJoinedPayload,
  type IVXPlayerLeftPayload,
  type IVXRawKernelEvent,
  type IVXSubscription,
  type IVXWelcomePayload,
} from "./api";

interface RangeSubscription {
  from: number;
  to: number;
  handler: (e: IVXRawKernelEvent) => void;
}

const MIN_RATE_LIMIT_DEFAULT = 30;

export class IVXMatchSession implements IIVXMatchSession {
  private readonly _socket: Socket;
  private readonly _match: Match;
  private readonly _options: IVXJoinOptions;
  private readonly _onClockSampled: (serverUnixMs: number) => void;
  private readonly _onSelfDispose: (matchId: string) => void;

  private _outboundSeq = 0;
  private _serverMatchTimeAtLastSyncMs = 0;
  private _lastSyncWallClockMs = Date.now();
  private _lastServerUnixMs = 0;
  private _disposed = false;
  private _state: IVXTransportState = IVXTransportState.Connected;

  // Token bucket for outbound rate limiting.
  private _bucketStartMs = Date.now();
  private _opsRemainingThisSecond: number;

  // Active player set.
  private readonly _activeUsers = new Set<string>();

  // Per-opcode handlers.
  private readonly _opHandlers = new Map<number, ((e: IVXRawKernelEvent) => void)[]>();
  private readonly _rangeHandlers: RangeSubscription[] = [];

  // Lifecycle event handlers.
  private readonly _welcomeHandlers: ((e: IVXKernelEvent<IVXWelcomePayload>) => void)[] = [];
  private readonly _playerJoinedHandlers: ((e: IVXKernelEvent<IVXPlayerJoinedPayload>) => void)[] = [];
  private readonly _playerLeftHandlers: ((e: IVXKernelEvent<IVXPlayerLeftPayload>) => void)[] = [];
  private readonly _matchEndedHandlers: ((e: IVXKernelEvent<IVXMatchEndedPayload>) => void)[] = [];
  private readonly _errorHandlers: ((e: IVXKernelEvent<IVXError>) => void)[] = [];
  private readonly _stateHandlers: ((s: IVXTransportState) => void)[] = [];

  public templateId: string;
  public localUserId: string;

  get matchId(): string { return this._match.match_id; }
  get currentMatchTimeMs(): number {
    const elapsed = Date.now() - this._lastSyncWallClockMs;
    return this._serverMatchTimeAtLastSyncMs + Math.max(0, elapsed);
  }
  get activePlayerCount(): number { return this._activeUsers.size; }
  get state(): IVXTransportState { return this._state; }

  constructor(opts: {
    socket: Socket;
    match: Match;
    options: IVXJoinOptions;
    onClockSampled: (serverUnixMs: number) => void;
    onSelfDispose: (matchId: string) => void;
  }) {
    this._socket = opts.socket;
    this._match = opts.match;
    this._options = opts.options;
    this._onClockSampled = opts.onClockSampled;
    this._onSelfDispose = opts.onSelfDispose;

    this.templateId = this._extractTemplateId(opts.match.label);
    this.localUserId = opts.match.self?.user_id ?? "";

    this._opsRemainingThisSecond =
      opts.options.outboundOpsPerSecondLimit && opts.options.outboundOpsPerSecondLimit > 0
        ? opts.options.outboundOpsPerSecondLimit
        : MIN_RATE_LIMIT_DEFAULT;

    if (opts.match.presences) {
      for (const p of opts.match.presences) {
        if (p?.user_id) this._activeUsers.add(p.user_id);
      }
    }
    if (opts.match.self?.user_id) this._activeUsers.add(opts.match.self.user_id);
  }

  // ----- subscribe -----

  subscribe<TPayload = unknown>(
    opcode: number,
    handler: (e: IVXKernelEvent<TPayload>) => void,
  ): IVXSubscription {
    if (typeof handler !== "function") throw new Error("[IVXMatchSession] handler required");
    const wrapped = (raw: IVXRawKernelEvent) => {
      let payload: TPayload | null = null;
      if (raw.payloadJson != null) {
        try { payload = JSON.parse(raw.payloadJson) as TPayload; }
        catch { /* ignore */ return; }
      }
      try {
        handler({ header: raw.header, payload: payload as TPayload, recvUnixMs: raw.recvUnixMs });
      } catch (e) {
        // Handlers must never crash the session.
        console.warn(`[IVXMatchSession] handler threw op=0x${opcode.toString(16)}: ${(e as Error).message}`);
      }
    };
    let list = this._opHandlers.get(opcode);
    if (!list) { list = []; this._opHandlers.set(opcode, list); }
    list.push(wrapped);
    return {
      dispose: () => {
        const l = this._opHandlers.get(opcode);
        if (!l) return;
        const i = l.indexOf(wrapped);
        if (i >= 0) l.splice(i, 1);
      },
    };
  }

  subscribeRange(
    opcodeFrom: number,
    opcodeTo: number,
    handler: (e: IVXRawKernelEvent) => void,
  ): IVXSubscription {
    const sub: RangeSubscription = { from: opcodeFrom, to: opcodeTo, handler };
    this._rangeHandlers.push(sub);
    return {
      dispose: () => {
        const i = this._rangeHandlers.indexOf(sub);
        if (i >= 0) this._rangeHandlers.splice(i, 1);
      },
    };
  }

  // ----- lifecycle -----

  onWelcome(h: (e: IVXKernelEvent<IVXWelcomePayload>) => void): IVXSubscription {
    this._welcomeHandlers.push(h);
    return { dispose: () => this._removeFrom(this._welcomeHandlers, h) };
  }
  onPlayerJoined(h: (e: IVXKernelEvent<IVXPlayerJoinedPayload>) => void): IVXSubscription {
    this._playerJoinedHandlers.push(h);
    return { dispose: () => this._removeFrom(this._playerJoinedHandlers, h) };
  }
  onPlayerLeft(h: (e: IVXKernelEvent<IVXPlayerLeftPayload>) => void): IVXSubscription {
    this._playerLeftHandlers.push(h);
    return { dispose: () => this._removeFrom(this._playerLeftHandlers, h) };
  }
  onMatchEnded(h: (e: IVXKernelEvent<IVXMatchEndedPayload>) => void): IVXSubscription {
    this._matchEndedHandlers.push(h);
    return { dispose: () => this._removeFrom(this._matchEndedHandlers, h) };
  }
  onError(h: (e: IVXKernelEvent<IVXError>) => void): IVXSubscription {
    this._errorHandlers.push(h);
    return { dispose: () => this._removeFrom(this._errorHandlers, h) };
  }
  onStateChanged(h: (s: IVXTransportState) => void): IVXSubscription {
    this._stateHandlers.push(h);
    return { dispose: () => this._removeFrom(this._stateHandlers, h) };
  }

  // ----- send -----

  async send<TPayload>(opcode: number, payload: TPayload): Promise<void> {
    this._ensureLive();
    if (!this._consumeBucket()) {
      console.warn(`[IVXMatchSession] outbound rate limit hit op=0x${opcode.toString(16)}`);
      return;
    }
    const env = buildEnvelope({
      op: opcode,
      payload,
      matchId: this._match.match_id,
      senderUserId: this.localUserId,
      seq: ++this._outboundSeq,
      matchTimeMs: this.currentMatchTimeMs,
    });
    return this.sendEnvelope(env);
  }

  async sendEnvelope<TPayload>(env: IVXEnvelope<TPayload>): Promise<void> {
    this._ensureLive();
    if (!env.h) throw new Error("[IVXMatchSession] envelope missing header");
    env.h.match_id = this._match.match_id;
    if (!env.h.sender_user_id) env.h.sender_user_id = this.localUserId;
    if (!env.h.client_opcode_uuid) {
      env.h.client_opcode_uuid = (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function")
        ? crypto.randomUUID().replace(/-/g, "")
        : Math.random().toString(36).slice(2);
    }
    if (env.h.wire_version == null) env.h.wire_version = IVXWireVersion.V1;
    const json = encodeEnvelope(env);
    try {
      await this._socket.sendMatchState(this._match.match_id, env.h.op, json);
    } catch (e) {
      console.error(`[IVXMatchSession] sendMatchState failed op=0x${env.h.op.toString(16)} match=${this._match.match_id}: ${(e as Error).message}`);
      throw e;
    }
  }

  async leave(): Promise<void> {
    if (this._disposed) return;
    // Voluntary leave is transport-level — Nakama emits matchLeave on the
    // server, which fans out PLAYER_LEFT(reason=VOLUNTARY) on its own.
    try {
      await this._socket.leaveMatch(this._match.match_id);
    } catch (e) {
      console.warn(`[IVXMatchSession] leaveMatch threw match=${this._match.match_id}: ${(e as Error).message}`);
    }
    this.dispose();
  }

  dispose(): void {
    if (this._disposed) return;
    this._disposed = true;
    try { this._onSelfDispose(this._match.match_id); } catch { /* ignore */ }
  }

  // ----- inbound (called by client) -----

  handleMatchState(state: { match_id: string; op_code: number; data: string | Uint8Array }): void {
    if (this._disposed) return;
    const env = decodeEnvelope(state.data);
    let header: IVXHeader;
    let payloadJson: string | null = null;
    if (env && env.h) {
      header = env.h;
      payloadJson = env.p == null ? null : JSON.stringify(env.p);
    } else {
      header = {
        wire_version: IVXWireVersion.V1,
        op: state.op_code,
        seq: 0,
        match_time_ms: 0,
        sender_user_id: "",
        match_id: this._match.match_id,
        client_opcode_uuid: "",
      };
    }
    const recv = Date.now();
    this._dispatchKernel(header, payloadJson, recv);

    const raw: IVXRawKernelEvent = { header, payloadJson, recvUnixMs: recv };
    const opCopy = this._opHandlers.get(header.op);
    if (opCopy) {
      const snap = opCopy.slice();
      for (const h of snap) {
        try { h(raw); } catch (e) { console.warn(`[IVXMatchSession] handler threw op=0x${header.op.toString(16)}: ${(e as Error).message}`); }
      }
    }
    if (this._rangeHandlers.length > 0) {
      const snap = this._rangeHandlers.slice();
      for (const r of snap) {
        if (header.op >= r.from && header.op <= r.to) {
          try { r.handler(raw); } catch (e) { console.warn(`[IVXMatchSession] range handler threw op=0x${header.op.toString(16)}: ${(e as Error).message}`); }
        }
      }
    }
  }

  handlePresence(ev: { joins?: Array<{ user_id: string }>; leaves?: Array<{ user_id: string }> }): void {
    if (this._disposed) return;
    if (ev.joins) {
      for (const j of ev.joins) if (j?.user_id) this._activeUsers.add(j.user_id);
    }
    if (ev.leaves) {
      for (const l of ev.leaves) if (l?.user_id) this._activeUsers.delete(l.user_id);
    }
  }

  setTransportState(s: IVXTransportState): void {
    if (this._state === s) return;
    this._state = s;
    for (const h of this._stateHandlers.slice()) {
      try { h(s); } catch { /* ignore */ }
    }
  }

  get lastServerUnixMs(): number { return this._lastServerUnixMs; }

  // ----- internals -----

  private _dispatchKernel(header: IVXHeader, payloadJson: string | null, recvMs: number): void {
    switch (header.op) {
      case IVXKernelOp.SERVER_HELLO:
      case IVXKernelOp.WELCOME: {
        const p = this._safeParse<IVXWelcomePayload>(payloadJson);
        if (p) {
          this._sampleClock(p.server_match_time_ms, p.server_unix_ms);
          if (p.assigned_user_id) this.localUserId = p.assigned_user_id;
          for (const h of this._welcomeHandlers.slice()) {
            try { h({ header, payload: p, recvUnixMs: recvMs }); } catch { /* ignore */ }
          }
        }
        return;
      }
      case IVXKernelOp.PLAYER_JOINED: {
        const p = this._safeParse<IVXPlayerJoinedPayload>(payloadJson);
        if (p) {
          this._activeUsers.add(p.user_id);
          for (const h of this._playerJoinedHandlers.slice()) {
            try { h({ header, payload: p, recvUnixMs: recvMs }); } catch { /* ignore */ }
          }
        }
        return;
      }
      case IVXKernelOp.PLAYER_LEFT: {
        const p = this._safeParse<IVXPlayerLeftPayload>(payloadJson);
        if (p) {
          this._activeUsers.delete(p.user_id);
          for (const h of this._playerLeftHandlers.slice()) {
            try { h({ header, payload: p, recvUnixMs: recvMs }); } catch { /* ignore */ }
          }
        }
        return;
      }
      case IVXKernelOp.CLOCK_SYNC: {
        const p = this._safeParse<IVXClockSyncPayload>(payloadJson);
        if (p) this._sampleClock(p.server_match_time_ms, p.server_unix_ms);
        return;
      }
      case IVXKernelOp.NETWORK_CLOCK_PONG: {
        const p = this._safeParse<{ client_ts_ms: number; server_ts_ms: number }>(payloadJson);
        if (p && typeof p.server_ts_ms === "number") {
          this._sampleClock(this._serverMatchTimeAtLastSyncMs, p.server_ts_ms);
        }
        return;
      }
      case IVXKernelOp.MATCH_ENDED: {
        const p = this._safeParse<IVXMatchEndedPayload>(payloadJson);
        for (const h of this._matchEndedHandlers.slice()) {
          try { h({ header, payload: p ?? { reason: 0 }, recvUnixMs: recvMs }); } catch { /* ignore */ }
        }
        this.dispose();
        return;
      }
      case IVXKernelOp.ERROR: {
        const p = this._safeParse<IVXError>(payloadJson);
        if (p) {
          for (const h of this._errorHandlers.slice()) {
            try { h({ header, payload: p, recvUnixMs: recvMs }); } catch { /* ignore */ }
          }
        }
        return;
      }
      default:
        return;
    }
  }

  private _sampleClock(serverMatchTimeMs: number, serverUnixMs: number): void {
    this._serverMatchTimeAtLastSyncMs = serverMatchTimeMs;
    this._lastSyncWallClockMs = Date.now();
    this._lastServerUnixMs = serverUnixMs;
    try { this._onClockSampled(serverUnixMs); } catch { /* ignore */ }
  }

  private _safeParse<T>(json: string | null): T | null {
    if (!json) return null;
    try { return JSON.parse(json) as T; }
    catch (e) {
      console.warn(`[IVXMatchSession] payload decode failed: ${(e as Error).message}`);
      return null;
    }
  }

  private _consumeBucket(): boolean {
    const cap = this._options.outboundOpsPerSecondLimit && this._options.outboundOpsPerSecondLimit > 0
      ? this._options.outboundOpsPerSecondLimit
      : MIN_RATE_LIMIT_DEFAULT;
    const now = Date.now();
    if (now - this._bucketStartMs >= 1000) {
      this._bucketStartMs = now;
      this._opsRemainingThisSecond = cap;
    }
    if (this._opsRemainingThisSecond <= 0) return false;
    this._opsRemainingThisSecond--;
    return true;
  }

  private _ensureLive(): void {
    if (this._disposed) throw new Error("[IVXMatchSession] disposed");
    if (!this._socket) throw new Error("[IVXMatchSession] socket missing");
  }

  private _extractTemplateId(label?: string): string {
    if (!label) return "";
    try {
      const obj = JSON.parse(label) as { template_id?: string };
      return obj.template_id ?? "";
    } catch {
      return "";
    }
  }

  private _removeFrom<T>(arr: T[], item: T): void {
    const i = arr.indexOf(item);
    if (i >= 0) arr.splice(i, 1);
  }
}
