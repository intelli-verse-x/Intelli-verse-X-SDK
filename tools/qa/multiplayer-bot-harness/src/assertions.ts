// Assertion helpers used by `runner.ts` against a finished bot run.
// Intentionally framework-free so this file can be unit-tested in
// isolation. All assertions throw on failure with a descriptive
// message — `runner.ts` catches and converts to JUnit / Prom signals.

export interface BotRunStats {
  totalTicks: number;
  overrunTicks: number;
  rttSamplesMs: number[];
  duplicatePoseFrames: number;
  totalPoseFrames: number;
  lodChanges: number;
  durationSec: number;
  anchorResolveTimesMs: number[];
  agentBudgetExceeded: number;
  legPromotions: number[];
  reactionFanoutMs: number[];
  // ── Voice / LiveKit token-issuance metrics (Phase 1) ──────────────────
  voiceTokenAttempts: number;
  voiceTokenSuccesses: number;
  voiceTokenWellformed: number;
  voiceTokenFutureExpiry: number;
  voiceTokenProviderUnspecified: number;
  // ── Conversational floor ─────────────────────────────────────────────
  speakerGrants: number;
}

export interface Expectation {
  /** e.g. "tick_overrun_ratio < 0.01" */
  expression: string;
  description?: string;
}

export function percentile(samples: number[], p: number): number {
  if (samples.length === 0) return 0;
  const sorted = [...samples].sort((a, b) => a - b);
  const idx = Math.min(
    sorted.length - 1,
    Math.floor((p / 100) * sorted.length),
  );
  return sorted[idx];
}

/**
 * Built-in metric extractors. The script YAML references these by
 * name in `expectations:`; runner.ts looks them up here.
 */
export const metrics = {
  tick_overrun_ratio: (s: BotRunStats): number =>
    s.totalTicks === 0 ? 0 : s.overrunTicks / s.totalTicks,
  rtt_p99_ms: (s: BotRunStats): number => percentile(s.rttSamplesMs, 99),
  rtt_p95_ms: (s: BotRunStats): number => percentile(s.rttSamplesMs, 95),
  dup_pose_ratio: (s: BotRunStats): number =>
    s.totalPoseFrames === 0 ? 0 : s.duplicatePoseFrames / s.totalPoseFrames,
  lod_changes_per_min: (s: BotRunStats): number =>
    s.durationSec === 0 ? 0 : (s.lodChanges / s.durationSec) * 60,
  anchor_resolve_p95_ms: (s: BotRunStats): number =>
    percentile(s.anchorResolveTimesMs, 95),
  agent_budget_exceeded: (s: BotRunStats): number => s.agentBudgetExceeded,
  reaction_fanout_p95_ms: (s: BotRunStats): number =>
    percentile(s.reactionFanoutMs, 95),
  legs_monotonic: (s: BotRunStats): number => {
    for (let i = 1; i < s.legPromotions.length; i++) {
      if (s.legPromotions[i] < s.legPromotions[i - 1]) return 0;
    }
    return 1;
  },
  // ── Phase-1 LiveKit voice-token metrics ─────────────────────────────
  voice_token_success_ratio: (s: BotRunStats): number =>
    s.voiceTokenAttempts === 0 ? 1 : s.voiceTokenSuccesses / s.voiceTokenAttempts,
  voice_token_wellformed_ratio: (s: BotRunStats): number =>
    s.voiceTokenSuccesses === 0 ? 1 : s.voiceTokenWellformed / s.voiceTokenSuccesses,
  voice_token_future_expiry_ratio: (s: BotRunStats): number =>
    s.voiceTokenSuccesses === 0 ? 1 : s.voiceTokenFutureExpiry / s.voiceTokenSuccesses,
  voice_token_provider_unspecified_count: (s: BotRunStats): number =>
    s.voiceTokenProviderUnspecified,
  speaker_grants: (s: BotRunStats): number => s.speakerGrants,
} as const;

export type MetricName = keyof typeof metrics;

const OPS: Record<string, (a: number, b: number) => boolean> = {
  "<":  (a, b) => a < b,
  "<=": (a, b) => a <= b,
  ">":  (a, b) => a > b,
  ">=": (a, b) => a >= b,
  "==": (a, b) => a === b,
  "!=": (a, b) => a !== b,
};

/**
 * Evaluate a single string expression like `"tick_overrun_ratio < 0.01"`.
 * Throws with a human-readable failure if it doesn't hold.
 */
export function evaluate(stats: BotRunStats, exp: Expectation): void {
  const m = exp.expression.match(/^\s*([a-z_]+)\s*(<=|>=|==|!=|<|>)\s*([0-9.eE+-]+)\s*$/);
  if (!m) {
    throw new Error(`bad expectation expression: ${exp.expression}`);
  }
  const [, name, op, rawNum] = m;
  if (!(name in metrics)) {
    throw new Error(`unknown metric '${name}' in expectation ${exp.expression}`);
  }
  const lhs = metrics[name as MetricName](stats);
  const rhs = Number(rawNum);
  if (!Number.isFinite(rhs)) {
    throw new Error(`bad numeric literal in expectation: ${exp.expression}`);
  }
  if (!OPS[op](lhs, rhs)) {
    throw new Error(
      `EXPECTATION FAILED ${exp.description ?? exp.expression}: ` +
        `${name}=${lhs} ${op} ${rhs} did not hold`,
    );
  }
}

export function evaluateAll(stats: BotRunStats, exps: Expectation[]): {
  passed: number;
  failed: { exp: Expectation; reason: string }[];
} {
  const failed: { exp: Expectation; reason: string }[] = [];
  let passed = 0;
  for (const e of exps) {
    try {
      evaluate(stats, e);
      passed++;
    } catch (err) {
      failed.push({ exp: e, reason: (err as Error).message });
    }
  }
  return { passed, failed };
}
