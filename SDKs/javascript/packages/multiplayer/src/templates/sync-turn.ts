// Strongly-typed wrapper around an IIVXMatchSession running the
// `sync-turn-v1` template. Game code typically wants typed events
// (TurnStart, TurnResolved, etc.) instead of raw opcodes.

import { IVXSyncTurnOp } from "../wire/constants";
import type {
  IIVXMatchSession,
  IVXKernelEvent,
  IVXSubscription,
} from "../api";

// ---------- payloads (templates/sync_turn.proto) ----------

export interface SyncTurnStartPayload {
  turn_index: number;
  round_index: number;
  input_window_ms: number;
  input_opens_at_match_ms: number;
  input_closes_at_match_ms: number;
  turn_payload: unknown;
  is_final_turn: boolean;
}

export interface SyncTurnInputOpenedPayload {
  turn_index: number;
}

export interface SyncTurnInputClosedPayload {
  turn_index: number;
  all_submitted: boolean;
}

export interface SyncTurnResolvedPayload {
  turn_index: number;
  result_payload: unknown;
  score_delta: Record<string, number>;
}

export interface SyncTurnScoreUpdatePayload {
  turn_index: number;
  totals: Record<string, number>;
}

export interface SyncTurnInputSubmitPayload {
  turn_index: number;
  client_response_ms: number;
  submission: unknown;
}

// ---------- typed wrapper ----------

export class IVXSyncTurnClient {
  private readonly _subs: IVXSubscription[] = [];

  constructor(public readonly session: IIVXMatchSession) {
    if (!session) throw new Error("[IVXSyncTurnClient] session required");
  }

  get matchId(): string { return this.session.matchId; }

  onTurnStart(h: (e: IVXKernelEvent<SyncTurnStartPayload>) => void): IVXSubscription {
    const sub = this.session.subscribe<SyncTurnStartPayload>(IVXSyncTurnOp.TURN_START, h);
    this._subs.push(sub);
    return sub;
  }
  onTurnInputOpened(h: (e: IVXKernelEvent<SyncTurnInputOpenedPayload>) => void): IVXSubscription {
    const sub = this.session.subscribe<SyncTurnInputOpenedPayload>(IVXSyncTurnOp.TURN_INPUT_OPENED, h);
    this._subs.push(sub);
    return sub;
  }
  onTurnInputClosed(h: (e: IVXKernelEvent<SyncTurnInputClosedPayload>) => void): IVXSubscription {
    const sub = this.session.subscribe<SyncTurnInputClosedPayload>(IVXSyncTurnOp.TURN_INPUT_CLOSED, h);
    this._subs.push(sub);
    return sub;
  }
  onTurnResolved(h: (e: IVXKernelEvent<SyncTurnResolvedPayload>) => void): IVXSubscription {
    const sub = this.session.subscribe<SyncTurnResolvedPayload>(IVXSyncTurnOp.TURN_RESOLVED, h);
    this._subs.push(sub);
    return sub;
  }
  onScoreUpdate(h: (e: IVXKernelEvent<SyncTurnScoreUpdatePayload>) => void): IVXSubscription {
    const sub = this.session.subscribe<SyncTurnScoreUpdatePayload>(IVXSyncTurnOp.SCORE_UPDATE, h);
    this._subs.push(sub);
    return sub;
  }

  submitInput(turnIndex: number, submission: unknown, clientResponseMs: number): Promise<void> {
    return this.session.send<SyncTurnInputSubmitPayload>(IVXSyncTurnOp.TURN_INPUT_SUBMIT, {
      turn_index: turnIndex,
      client_response_ms: clientResponseMs,
      submission,
    });
  }

  ready(): Promise<void> {
    return this.session.send(IVXSyncTurnOp.PLAYER_READY, { ready: true });
  }

  forfeit(): Promise<void> {
    return this.session.send(IVXSyncTurnOp.PLAYER_FORFEIT, {});
  }

  dispose(): void {
    for (const s of this._subs) {
      try { s.dispose(); } catch { /* ignore */ }
    }
    this._subs.length = 0;
  }
}
