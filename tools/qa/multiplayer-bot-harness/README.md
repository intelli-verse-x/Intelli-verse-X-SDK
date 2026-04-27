# IVX Multiplayer — Synthetic Bot Validation Harness

Replays canonical match scripts against any environment (`local`,
`canary`, `prod`) and asserts SLO-level invariants. Wired as **Gate B**
in `docs/multiplayer/qa-gates.md`.

## Layout

```
multiplayer-bot-harness/
├── README.md                       (you are here)
├── package.json                    (TS runtime + script-driver)
├── tsconfig.json
├── src/
│   ├── runner.ts                   (loads YAML, spawns N bots)
│   ├── bot.ts                      (single bot session via @intelliversex/multiplayer)
│   ├── assertions.ts               (SLO assertions — tick overrun, p99 RTT, etc.)
│   └── reporters/
│       ├── junit.ts                (CI-friendly output)
│       └── prom-pushgateway.ts     (writes ivx_qa_* metrics)
└── scripts/
    ├── realtime_60hz.yaml          (RealtimeTickMatch — 8 bots, 5 min)
    ├── avatar_party_8.yaml         (AvatarReplicationMatch — 8 avatars)
    ├── mr_anchor_4.yaml            (MixedRealityAnchorMatch — 4 viewers)
    ├── conv_party_agents.yaml      (ConversationalPartyMatch — 2 humans + 2 agents)
    ├── tournament_64.yaml          (Tournament template — 64 bots)
    └── live_event_300.yaml         (LiveEventRoom — 300 viewers)
```

The runner consumes the same `@intelliversex/multiplayer` adapter that
ships in apps, so a green run also proves the JS SDK works end-to-end
against the target deployment.

## Running locally

```bash
cd tools/qa/multiplayer-bot-harness
npm install
npx ts-node src/runner.ts \
  --target ws://127.0.0.1:7350 \
  --script scripts/realtime_60hz.yaml \
  --report junit
```

## CI integration

The canary job in `intelli-verse-kube-infra/.github/workflows/canary.yml`
runs:

```bash
npx ts-node src/runner.ts \
  --target wss://nk-canary.intelliverse-x.com \
  --script scripts/<script>.yaml \
  --report prom-pushgateway \
  --pushgateway https://prom-push.observability.svc.cluster.local:9091
```

…for every script under `scripts/`. The job halts the canary's
graduation to 5% prod if any single script fails three consecutive
runs.

## Pass criteria (mirrors qa-gates.md / Gate B)

| Script                  | Template                  | Must hold                              |
|-------------------------|---------------------------|----------------------------------------|
| realtime_60hz.yaml      | realtime-tick-v1          | tick overrun < 1%, p99 RTT < 90ms      |
| avatar_party_8.yaml     | avatar-replication-v1     | LOD changes ≤ 1/min, dup-pose < 0.1%   |
| mr_anchor_4.yaml        | mixed-reality-anchor-v1   | anchor resolve < 5s p95                |
| conv_party_agents.yaml  | conversational-party-v1   | agent budget never exceeded            |
| tournament_64.yaml      | tournament-v1             | leg-promotions monotonic               |
| live_event_300.yaml     | live-event-v1             | reaction-fan-out < 250ms p95           |

The `assertions.ts` module reads these from each script's
`expectations:` block, so adjusting an SLO is a one-file change.
