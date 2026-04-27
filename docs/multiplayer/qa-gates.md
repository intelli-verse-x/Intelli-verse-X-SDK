# IVX Multiplayer — QA Gate Definitions & Sign-Off Template

This document defines the gates a multiplayer change MUST pass before
it is allowed to ride the staged-rollout train (see
`docs/multiplayer/staged-rollout.md`). Every PR that touches:

* `nakama/data/modules/src/multiplayer-kernel/**`
* `nakama/data/modules/avatar_replication/**`
* `nakama/data/modules/realtime_tick/**`
* `Intelli-verse-X-SDK/schemas/multiplayer/**.proto`
* Any adapter under `Intelli-verse-X-SDK/SDKs/<engine>/**` that
  implements `IIVXMultiplayer` / `IIVXMatchSession`
* Any kustomization under
  `intelli-verse-kube-infra/nakama/multiplayer/**`

…must include in its description a filled-out **Sign-Off Template**
(scroll to the bottom). The CI gates listed below are wired to block
merge without passing.

---

## Gate A — Conformance (automated, blocking)

| Suite | Where | Owner |
|-------|-------|-------|
| TS 12-test | `SDKs/javascript/packages/multiplayer/test/conformance.spec.ts` | SDK platform |
| Go 12-test | `nakama/data/modules/avatar_replication/conformance_test.go` | Backend |
| C# 12-test | `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Tests/ConformanceTests.cs` | Unity SDK |
| Wire-shape lint | `tools/proto-lint.sh` against `schemas/multiplayer/*.proto` | Schemas WG |

CI: `.github/workflows/multiplayer-conformance.yml`. Failure blocks
both PR merge AND canary rollout.

## Gate B — Synthetic bot validation (automated, blocking on canary)

The bot harness (`tools/qa/multiplayer-bot-harness/`) replays canonical
match scripts against the canary deployment:

| Script | Template | Pass criterion |
|--------|----------|----------------|
| `realtime_60hz.yaml` | realtime-tick-v1 | tick overrun < 1%, p99 RTT < 90ms |
| `avatar_party_8.yaml` | avatar-replication-v1 | LOD changes ≤ 1/min, dup-pose < 0.1% |
| `mr_anchor_4.yaml` | mixed-reality-anchor-v1 | anchor resolve < 5s p95 |
| `conv_party_agents.yaml` | conversational-party-v1 | agent budget never exceeded |
| `tournament_64.yaml` | tournament-v1 | leg-promotions monotonic |
| `live_event_300.yaml` | live-event-v1 | reaction-fan-out < 250ms p95 |

Canary cannot graduate to 5% prod traffic until ALL six pass three
consecutive runs with no flake.

## Gate C — Manual QA (human, blocking for breaking changes)

Trigger condition: PR is labelled `multiplayer:breaking` OR touches:

* Wire format (`schemas/multiplayer/*.proto`)
* Public adapter interfaces (`IIVXMultiplayer`, `IIVXMatchSession`,
  `IIVXVoice`, `IIVXAvatar`, `IIVXAnchorProvider`)
* Match template ids or opcode ranges
* DR / replication topology

Manual QA covers:

* Cross-engine play (Unity + JS + Unreal in the same match) for at
  least one realtime template.
* Mid-match adapter version drift (one client at HEAD, one at the
  current prod tag).
* Voice cutover when LiveKit goes unavailable mid-match.
* Anchor relocalization on Quest + iOS + WebXR in the same room.
* Reconnect after airplane-mode toggle (kills socket).

## Gate D — Performance (automated nightly, blocking on regression)

| Metric | Budget | Source |
|--------|--------|--------|
| Realtime tick CPU per match | ≤ 4ms p99 | `ivx_tick_duration_ms` |
| Avatar replication egress per pod | ≤ 50Mbit/s @ 8 avatars | `ivx_egress_bytes` |
| Heap growth per 1h match | ≤ 100MB | `process_resident_memory_bytes` |
| Match-create RTT | ≤ 250ms p95 | `ivx_match_create_seconds` |

A 10% regression vs. the last green nightly auto-blocks the train.

## Gate E — Security (automated + manual review)

* `gosec`, `govulncheck`, `npm audit`, Snyk on TS, Roslyn analyzers on C#.
* Manual: anchor secret material never logged; agent credentials never
  echoed back to clients; voice tokens never persisted in match
  results.

## Gate F — Documentation (manual, blocking)

Updated in this PR if applicable:

- [ ] `MULTIPLAYER_KERNEL_ADAPTERS.md` (cross-engine matrix)
- [ ] `docs/multiplayer/error-taxonomy.md` (new error codes)
- [ ] `docs/multiplayer/avatar-interop.md` (new bones / blend shapes)
- [ ] CHANGELOG.md for affected packages
- [ ] Migration notes if the change is breaking

---

## Sign-Off Template

Paste into every PR that triggers any gate above:

```markdown
### Multiplayer Sign-Off

| Gate | Status | Evidence |
|------|--------|----------|
| A — Conformance suites | ☐ pass / ☐ fail | <CI link> |
| B — Synthetic bot harness | ☐ pass / ☐ fail / ☐ N/A | <run id> |
| C — Manual QA cross-engine | ☐ pass / ☐ N/A | <session log> |
| D — Performance budgets | ☐ pass / ☐ regression accepted | <Grafana link> |
| E — Security scans | ☐ clean / ☐ accepted with waiver | <scan id> |
| F — Documentation updated | ☐ yes / ☐ N/A | <changed paths> |

#### Risk surface
* What could break for live games? <…>
* Rollback plan: <…>
* Canary cohort: <see staged-rollout.md>

#### Approvals
* Backend lead: @
* SDK platform lead: @
* SRE on-call: @
* Product: @
```

A merge is allowed when **every applicable gate is `pass` (or has an
explicit waiver from the noted owner)**. The canary deploy job in
`intelli-verse-kube-infra` reads this section out of the merge commit
and refuses to advance the train if any required gate is `fail`.
