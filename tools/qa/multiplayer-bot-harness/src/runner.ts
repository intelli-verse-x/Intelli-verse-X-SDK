// Loads a YAML script, spawns N bots in parallel, and emits results
// either as JUnit XML (default) or as Prom pushgateway counters.

import * as fs from "node:fs";
import * as path from "node:path";
import { parse as parseYaml } from "yaml";
import { runBot, BotConfig, BotOutcome } from "./bot.js";
import {
  BotRunStats,
  Expectation,
  evaluateAll,
  metrics,
  MetricName,
} from "./assertions.js";
import { writeJUnit } from "./reporters/junit.js";
import { pushProm } from "./reporters/prom-pushgateway.js";

interface ScriptFile {
  name: string;
  template_id: string;
  duration_sec: number;
  tick_rate_hz?: number;
  pose_publish_rate_hz?: number;
  bots: { kind: BotConfig["kind"]; count: number }[];
  expectations: { expression: string; description?: string }[];
}

function parseArgs(): {
  target: string;
  script: string;
  report: "junit" | "prom-pushgateway";
  pushgateway?: string;
  out?: string;
} {
  const args = new Map<string, string>();
  for (let i = 2; i < process.argv.length; i += 2) {
    args.set(process.argv[i].replace(/^--/, ""), process.argv[i + 1]);
  }
  if (!args.get("target")) throw new Error("--target ws://… is required");
  if (!args.get("script")) throw new Error("--script <yaml> is required");
  return {
    target: args.get("target")!,
    script: args.get("script")!,
    report: (args.get("report") as any) ?? "junit",
    pushgateway: args.get("pushgateway"),
    out: args.get("out"),
  };
}

function aggregate(outcomes: BotOutcome[]): BotRunStats {
  const agg: BotRunStats = {
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
  for (const o of outcomes) {
    agg.totalTicks += o.stats.totalTicks;
    agg.overrunTicks += o.stats.overrunTicks;
    agg.rttSamplesMs.push(...o.stats.rttSamplesMs);
    agg.duplicatePoseFrames += o.stats.duplicatePoseFrames;
    agg.totalPoseFrames += o.stats.totalPoseFrames;
    agg.lodChanges += o.stats.lodChanges;
    agg.durationSec = Math.max(agg.durationSec, o.stats.durationSec);
    agg.anchorResolveTimesMs.push(...o.stats.anchorResolveTimesMs);
    agg.agentBudgetExceeded += o.stats.agentBudgetExceeded;
    agg.legPromotions.push(...o.stats.legPromotions);
    agg.reactionFanoutMs.push(...o.stats.reactionFanoutMs);
    agg.voiceTokenAttempts += o.stats.voiceTokenAttempts;
    agg.voiceTokenSuccesses += o.stats.voiceTokenSuccesses;
    agg.voiceTokenWellformed += o.stats.voiceTokenWellformed;
    agg.voiceTokenFutureExpiry += o.stats.voiceTokenFutureExpiry;
    agg.voiceTokenProviderUnspecified += o.stats.voiceTokenProviderUnspecified;
    agg.speakerGrants += o.stats.speakerGrants;
  }
  return agg;
}

async function main(): Promise<void> {
  const opts = parseArgs();
  const yamlPath = path.resolve(opts.script);
  const script: ScriptFile = parseYaml(fs.readFileSync(yamlPath, "utf8"));

  const cfgs: BotConfig[] = [];
  for (const b of script.bots) {
    for (let i = 0; i < b.count; i++) {
      cfgs.push({
        target: opts.target,
        templateId: script.template_id,
        durationSec: script.duration_sec,
        tickRateHz: script.tick_rate_hz,
        posePublishRateHz: script.pose_publish_rate_hz,
        kind: b.kind,
      });
    }
  }
  console.log(
    `[ivx-bot-harness] script=${script.name} bots=${cfgs.length} ` +
      `template=${script.template_id} dur=${script.duration_sec}s`,
  );

  const outcomes = await Promise.all(cfgs.map(runBot));
  const agg = aggregate(outcomes);
  const evalResult = evaluateAll(agg, script.expectations as Expectation[]);

  console.log(
    `[ivx-bot-harness] passed=${evalResult.passed}/${script.expectations.length}`,
  );
  for (const f of evalResult.failed) {
    console.error(`  - FAIL: ${f.reason}`);
  }

  const summary: Record<string, number> = {};
  for (const name of Object.keys(metrics) as MetricName[]) {
    summary[name] = metrics[name](agg);
  }

  if (opts.report === "junit") {
    const out = opts.out ?? `bot-harness-${script.name}.junit.xml`;
    writeJUnit(out, script.name, evalResult, summary);
    console.log(`[ivx-bot-harness] junit -> ${out}`);
  } else {
    if (!opts.pushgateway)
      throw new Error("--pushgateway is required for prom report");
    await pushProm(opts.pushgateway, script.name, summary, evalResult);
    console.log(`[ivx-bot-harness] prom-pushgateway -> ${opts.pushgateway}`);
  }

  if (evalResult.failed.length > 0) process.exit(2);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
