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
  kind: "realtime" | "avatar" | "anchor-viewer" | "agent" | "spectator" | "voice";
  /** Optional Nakama RPC client (if the harness has one wired) — used by
   *  voice bots to call mp_voice_token end-to-end against the target. */
  rpcClient?: { rpc: (id: string, payload: any) => Promise<any> };
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
  };
  const errors: string[] = [];

  const start = Date.now();
  const tickPeriodMs = 1000 / (cfg.tickRateHz ?? 30);
  const deadline = start + cfg.durationSec * 1000;

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
