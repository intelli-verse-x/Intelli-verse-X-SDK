# IVX Multiplayer — Staged Rollout Playbook

This playbook is the operational counterpart to
`docs/multiplayer/qa-gates.md`. It defines exactly **how** a green
build graduates from a feature branch to 100% production traffic
across all three regions, and exactly **when** to halt or roll back.

The playbook is opinionated by design: every phase has automated halt
conditions and named human owners. If a step is blocked and you can't
resolve it inside the noted SLA, you halt the train.

---

## Cohorts (in graduation order)

| # | Cohort                | Surface                                   | Auto-halt SLA |
|---|-----------------------|-------------------------------------------|---------------|
| 1 | `dev`                 | preview env, conformance suite, PR review | 30 min        |
| 2 | `staging`             | staging cluster, full bot harness         | 60 min        |
| 3 | `canary` (1% prod)    | us-east only, internal users only         | 30 min        |
| 4 | `canary` (5% prod)    | us-east only, public users tagged `canary`| 60 min        |
| 5 | `regional`            | us-east 100%                              | 4 h soak      |
| 6 | `multi-region`        | us-east + us-west 100%                    | 12 h soak     |
| 7 | `global`              | us-east + us-west + eu-central 100%       | 24 h soak     |

The staged rollout is wired to **Pillar 8 cohort flags** (Hiro
LiveOps), not just Kubernetes traffic splitting, so that the *content
configuration* of a release also rides the same gates as the
*server binary* of a release.

---

## Phase 1 — `dev` (PR + conformance)

| Owner | SDK platform on-call |
|-------|----------------------|
| Trigger | PR merged to `master` after Gate A pass |
| Halt | Any conformance suite red OR `multiplayer-conformance.yml` red |

Actions:

1. CI runs the TS + Go + C# conformance suites.
2. CI builds the multiplayer Nakama image, tags `:pr-<sha>`.
3. Bot harness is launched against `dev` cluster (`scripts/realtime_60hz.yaml`,
   `scripts/avatar_party_8.yaml`) — 1 run each, must pass.
4. Branch protection refuses to merge if any of the above are red.

## Phase 2 — `staging`

| Owner | Backend on-call |
|-------|-----------------|
| Trigger | Merge to `master` |
| Halt | Any bot-harness script fails in staging OR PrometheusRule fires |

Actions:

1. ArgoCD syncs `nakama-multiplayer` to staging.
2. Synthetic bot harness runs all 6 canonical scripts (Gate B).
3. Manual QA cross-engine smoke test (Gate C) for any PR labelled
   `multiplayer:breaking`.
4. Soak 60 min, watch Grafana `IVX Multiplayer — SLO Dashboard`.

If green, mark the deploy `staging-green` (label added by the bot to
the merge commit).

## Phase 3 — `canary` 1%

| Owner | SRE on-call + Backend on-call |
|-------|-------------------------------|
| Trigger | `staging-green` label set |
| Halt | Any of: tick overrun > 1%, `IVXMultiplayerMatchEndedAnomaly` fires, p99 RTT > 90ms |

Actions:

1. ArgoCD progresses canary to us-east only.
2. Routing layer adds the `canary=true` weight=1% rule.
3. Synthetic bot harness runs against the canary URL **continuously**
   for 30 min — three consecutive green passes required.
4. SRE manually checks: KEDA scale-up healthy, DR replicator green,
   no spike in `ivx_voice_unavailable_total`.

## Phase 4 — `canary` 5%

Same as Phase 3 but weight=5% and SLA is 60 min. Halt rules unchanged.

## Phase 5 — `regional` (us-east 100%)

| Owner | SRE on-call |
|-------|-------------|
| SLA | 4 h soak |
| Halt | Any PrometheusRule fires |

Actions:

1. Drop the canary rule, point all us-east traffic at the new image.
2. Watch SLO dashboard for 4 hours.
3. Backend on-call signs off the soak in the rollout ticket.

## Phase 6 — `multi-region`

Apply the same image to `us-west` via `region-west-overlay.yaml`.
Soak 12 hours. Halt criteria unchanged.

## Phase 7 — `global`

Apply to `eu-central`. **Important**: any data-residency-flagged
match (`tags.eu_only=true`) is already routed exclusively to
eu-central by `topology-routing-configmap.yaml`. The 24h soak validates
the regulated flow as well as the general flow.

---

## Halt protocol (any phase)

1. SRE on-call hits **`Pause Rollout`** in ArgoCD (this also halts the
   bot harness so it doesn't fight the rollback).
2. Page Backend on-call.
3. Decide within 15 min:
   * **Roll back image** (default for any production-impact halt):
     `kubectl rollout undo deployment/intelliverse-nakama-multiplayer`.
   * **Hold and patch**: only if the issue is containable (e.g. one
     bad voice provider) AND the SRE *and* Backend on-calls both
     agree.
4. Post-mortem: filed within 48 h, linked to the rollout ticket, with
   one line per: detection, halt time, root cause, blast radius,
   fix, and the gate that *should* have caught this earlier.

---

## Pillar-8 cohort tie-in

Every multiplayer rollout cohort above is mirrored to a **Hiro
LiveOps cohort** of the same name. So a server-side feature flag flip
(e.g. enabling `mixed-reality-anchor-v1` for a new game) walks
through the same dev → staging → canary → regional → multi-region →
global chain as the binary itself does.

That means the **production sign-off checklist** in
`docs/multiplayer/production-signoff-checklist.md` is filled out
**once per release**, covering both the binary and the cohort flag —
not once per surface area.
