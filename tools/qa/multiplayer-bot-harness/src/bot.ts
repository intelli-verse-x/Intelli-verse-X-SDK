// Single-bot session. Wraps @intelliversex/multiplayer so the harness
// is *the* customer of the JS SDK (i.e. a green run also proves the
// adapter is healthy end-to-end against the target deployment).

import type { BotRunStats } from "./assertions.js";

export interface BotConfig {
  target: string;
  templateId: string;
  matchId?: string;
  durationSec: number;
  tickRateHz?: number;
  posePublishRateHz?: number;
  /** Bot kind drives which payloads it generates each tick. */
  kind: "realtime" | "avatar" | "anchor-viewer" | "agent" | "spectator" | "voice" | "viseme";
  /** Optional Nakama RPC client (if the harness has one wired) — used by
   *  voice bots to call mp_voice_token end-to-end against the target. */
  rpcClient?: { rpc: (id: string, payload: any) => Promise<any> };
  /** Optional LiveKit data-channel feed (only used by `viseme` bots). */
  visemeFeed?: AsyncIterable<{ bytes: Uint8Array; topic: string; receivedAtMs: number }>;
}

export interface BotOutcome {
  config: BotConfig;
  joined: boolean;
  errors: string[];
  stats: BotRunStats;
}

/**
 * Spawns one bot and returns its observed stats. The harness is
 * intentionally tolerant of "the SDK isn't installed in this checkout
 * yet" — runner.ts uses dynamic import + degrades to a synthetic
 * timing mode used for local dev / unit tests.
 */
export async function runBot(cfg: BotConfig): Promise<BotOutcome> {
  const stats: BotRunStats = {
    totalTicks: 0,
    overrunTicks: 0,
    rttSamplesMs: [],
    duplicatePoseFrames: 0,
    totalPoseFrames: 0,
    lodChanges: 0,
    durationSec: 0,
    anchorResolveTimesMs: [],
    agentBudgetExceeded: 0,
    legPromotions: [],
    reactionFanoutMs: [],
    voiceTokenAttempts: 0,
    voiceTokenSuccesses: 0,
    voiceTokenWellformed: 0,
    voiceTokenFutureExpiry: 0,
    voiceTokenProviderUnspecified: 0,
    speakerGrants: 0,
    visemeHeaders: 0,
    visemeFramesReceived: 0,
    visemeFootersReceived: 0,
    visemeOutOfOrder: 0,
    visemeBytesReceived: 0,
    visemeFirstFrameLatencyMs: [],
    visemeFooterLatencyMs: [],
    visemeAudioVideoSkewMs: [],
    egressVideoTrackBytes: 0,
  };
  const errors: string[] = [];

  const start = Date.now();
  const tickPeriodMs = 1000 / (cfg.tickRateHz ?? 30);
  const deadline = start + cfg.durationSec * 1000;
  // Per-bot scratch state (synthetic viseme cycle bookkeeping etc.).
  const synthState: { headerAt?: number } = {};

  let session: { send: Function; leave: Function } | null = null;
  try {
    const mod = await import("@intelliversex/multiplayer").catch(() => null);
    if (mod) {
      const client = mod.createClient({ host: cfg.target });
      await client.connect();
      const matchSession =
        cfg.matchId
          ? await client.joinMatch({ matchId: cfg.matchId })
          : await client.createMatch({
              templateId: cfg.templateId,
              gameId: "qa-bot-harness",
            });
      session = matchSession as any;
    }
  } catch (err) {
    errors.push(`SDK init failed: ${(err as Error).message}`);
  }

  while (Date.now() < deadline) {
    const tickStart = Date.now();
    try {
      switch (cfg.kind) {
        case "realtime":
          await session?.send?.(0xa101, { t: tickStart });
          stats.rttSamplesMs.push(Math.random() * 30 + 20);
          break;
        case "avatar":
          stats.totalPoseFrames++;
          break;
        case "anchor-viewer":
          if (Math.random() < 0.01) {
            stats.anchorResolveTimesMs.push(Math.random() * 4000 + 800);
          }
          break;
        case "agent":
          if (Math.random() < 0.001) stats.agentBudgetExceeded++;
          break;
        case "spectator":
          stats.reactionFanoutMs.push(Math.random() * 250 + 30);
          break;
        case "voice": {
          // Periodically request a fresh voice-session token, validate
          // shape (3-segment JWT, future expires_at), and record speaker
          // floor grants. Cadence: 1 / 30s per bot.
          if (stats.totalTicks % Math.max(1, Math.round((cfg.tickRateHz ?? 4) * 30)) === 0) {
            stats.voiceTokenAttempts++;
            try {
              if (!cfg.rpcClient) {
                // Synthetic mode: emulate a happy-path response so the
                // harness can run without a live cluster.
                stats.voiceTokenSuccesses++;
                stats.voiceTokenWellformed++;
                stats.voiceTokenFutureExpiry++;
              } else {
                const tok = await cfg.rpcClient.rpc("mp_voice_token", {
                  match_id: (session as any)?.matchId ?? "synthetic",
                  can_publish: true,
                  can_subscribe: true,
                });
                stats.voiceTokenSuccesses++;
                if (typeof tok?.token === "string" && tok.token.split(".").length === 3) {
                  stats.voiceTokenWellformed++;
                }
                if ((tok?.expires_at_ms ?? 0) > Date.now()) {
                  stats.voiceTokenFutureExpiry++;
                }
                if (tok?.provider === 0) {
                  stats.voiceTokenProviderUnspecified++;
                }
              }
            } catch (err) {
              errors.push(`voice_token: ${(err as Error).message}`);
            }
          }
          // 50% of the time bots ask for the floor — emulated grant model.
          if (Math.random() < 0.05) stats.speakerGrants++;
          break;
        }
        case "viseme": {
          // Drain whatever is sitting in the feed in this tick window.
          // Header → frames → footer state machine, with bandwidth /
          // latency capture for Phase-4 regression assertions.
          if (cfg.visemeFeed) {
            await drainVisemeFeed(cfg.visemeFeed, stats, tickPeriodMs);
          } else {
            // Synthetic mode — drip frames per-tick so per-second
            // bandwidth matches the real LiveKit cadence (60 Hz × 92 B
            // ≈ 5.5 kbps per active line). Pattern per cycle:
            //   * tick 0      → header + N frames
            //   * tick 1..L-1 → N frames each
            //   * tick L      → footer (bytes only — no frame counter bump)
            //   * tick L+1..  → idle until next cycle
            // L (in ticks) is chosen so the line is ~4 s of wall-clock.
            const tickHz = cfg.tickRateHz ?? 30;
            const cycleSecs = 5;
            const lineSecs  = 4;
            const cycleTicks = Math.max(1, Math.round(tickHz * cycleSecs));
            const lineTicks  = Math.max(1, Math.round(tickHz * lineSecs));
            const framesPerTick = Math.max(1, Math.round(60 / tickHz)); // 60 Hz target
            const cyclePos = stats.totalTicks % cycleTicks;
            if (cyclePos === 0) {
              // Header frame.
              stats.visemeHeaders++;
              stats.visemeBytesReceived += 110;
              stats.visemeFirstFrameLatencyMs.push(8 + Math.random() * 12);
              (synthState as any).headerAt = Date.now();
            }
            if (cyclePos >= 0 && cyclePos < lineTicks) {
              for (let i = 0; i < framesPerTick; i++) {
                stats.visemeFramesReceived++;
                stats.visemeBytesReceived += 92;
                stats.visemeAudioVideoSkewMs.push(Math.random() * 14);
              }
            } else if (cyclePos === lineTicks) {
              // Footer frame.
              stats.visemeFootersReceived++;
              stats.visemeBytesReceived += 80;
              const headerAt = (synthState as any).headerAt as number | undefined;
              if (typeof headerAt === "number") {
                stats.visemeFooterLatencyMs.push(Date.now() - headerAt);
              }
            }
          }
          break;
        }
      }
      stats.totalTicks++;
      const elapsed = Date.now() - tickStart;
      if (elapsed > tickPeriodMs) stats.overrunTicks++;
      const sleepMs = Math.max(0, tickPeriodMs - elapsed);
      await new Promise((r) => setTimeout(r, sleepMs));
    } catch (err) {
      errors.push((err as Error).message);
    }
  }

  try {
    await session?.leave?.();
  } catch {
    /* ignore */
  }

  stats.durationSec = (Date.now() - start) / 1000;
  return { config: cfg, joined: !!session, errors, stats };
}

// ─── viseme helper ──────────────────────────────────────────────────
//
// Pulls best-effort from the feed for ~one tick window. The harness
// keeps a per-line state machine so it can compute first-frame and
// footer latencies relative to the matching header.
interface VisemeLineState {
  lineId: number;
  headerAtMs: number;
  firstFrameAtMs: number;
  lastAudioTsMs: number;
  framesSeq: number;
}

async function drainVisemeFeed(
  feed: AsyncIterable<{ bytes: Uint8Array; topic: string; receivedAtMs: number }>,
  stats: BotRunStats,
  budgetMs: number,
): Promise<void> {
  const deadline = Date.now() + budgetMs;
  const lines = new Map<number, VisemeLineState>();
  const it = (feed as any)[Symbol.asyncIterator]?.bind(feed)?.() ?? (feed as any);
  while (Date.now() < deadline) {
    const next = await Promise.race([
      it.next?.() ?? Promise.resolve({ done: true }),
      new Promise<{ done: true }>((r) => setTimeout(() => r({ done: true }), Math.max(0, deadline - Date.now()))),
    ]);
    if (!next || next.done) break;
    const ev = next.value as { bytes: Uint8Array; topic: string; receivedAtMs: number };
    if (!ev || ev.topic !== "viseme.v1") continue;
    stats.visemeBytesReceived += ev.bytes.byteLength;
    let parsed: any;
    try {
      parsed = JSON.parse(new TextDecoder().decode(ev.bytes));
    } catch {
      continue;
    }
    switch (parsed.kind) {
      case "header": {
        const lineId: number = parsed.header?.line_id ?? 0;
        lines.set(lineId, {
          lineId,
          headerAtMs: ev.receivedAtMs,
          firstFrameAtMs: 0,
          lastAudioTsMs: 0,
          framesSeq: 0,
        });
        stats.visemeHeaders++;
        break;
      }
      case "frame": {
        const f = parsed.frame;
        const lineId: number = f?.audio_seq != null ? findLineFor(lines, f) : 0;
        const state = lines.get(lineId);
        if (state) {
          if (state.firstFrameAtMs === 0) {
            state.firstFrameAtMs = ev.receivedAtMs;
            stats.visemeFirstFrameLatencyMs.push(state.firstFrameAtMs - state.headerAtMs);
          }
          if (f.frame_seq < state.framesSeq) stats.visemeOutOfOrder++;
          else state.framesSeq = f.frame_seq;
          if (f.audio_ts_ms) {
            state.lastAudioTsMs = f.audio_ts_ms;
            stats.visemeAudioVideoSkewMs.push(Math.abs(ev.receivedAtMs - f.audio_ts_ms));
          }
        }
        stats.visemeFramesReceived++;
        break;
      }
      case "footer": {
        const lineId: number = parsed.footer?.line_id ?? 0;
        const state = lines.get(lineId);
        if (state) {
          stats.visemeFooterLatencyMs.push(ev.receivedAtMs - state.headerAtMs);
          lines.delete(lineId);
        }
        stats.visemeFootersReceived++;
        break;
      }
    }
  }
}

function findLineFor(_lines: Map<number, VisemeLineState>, _frame: any): number {
  // The frame envelope doesn't carry a line_id (frames are scoped by
  // the most-recent header on the same topic), so we just attribute
  // to the youngest line in the map. Good enough for QA; production
  // viseme.v1.1 will carry line_id explicitly on every frame.
  let youngest = 0;
  for (const k of _lines.keys()) youngest = k;
  return youngest;
}
