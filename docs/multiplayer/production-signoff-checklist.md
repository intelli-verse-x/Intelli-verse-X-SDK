# IVX Multiplayer — Production Sign-Off Checklist

This is the **single document** that closes a multiplayer release.
It is filled out **once per release**, attached to the rollout
ticket, and signed by the four named owners. A release cannot
graduate past Phase 5 (`regional`) of the staged rollout playbook
until this checklist is fully signed.

The checklist is split into three columns:

* **Server / Kernel** — Nakama runtime modules, schemas, k8s tier.
* **Adapters / Clients** — Unity, JS, Unreal, Swift (visionOS),
  Lua (Roblox), C++ console, Pico XR.
* **Pillar 8 cohort** — Hiro LiveOps cohort flag(s) that this release
  enables.

Each row should be **`pass / fail / N/A`** with evidence linked.

---

## 0. Release header

| Field                  | Value |
|------------------------|-------|
| Release tag            |       |
| Build SHA              |       |
| Pillar 8 cohort id     |       |
| Rollout ticket         |       |
| Targeted phases        | 1 → 7 / 1 → 5 / 1 → 3 / etc. |
| Roll-back image tag    |       |

---

## 1. Conformance & QA gates (Gates A–F from `qa-gates.md`)

| Item                                                | Server / Kernel | Adapters / Clients | Pillar 8 cohort |
|-----------------------------------------------------|-----------------|--------------------|-----------------|
| Gate A — TS + Go + C# conformance suites green      |                 |                    | N/A             |
| Gate B — All 6 bot-harness scripts green ×3 runs    |                 |                    |                 |
| Gate C — Manual QA cross-engine (if `breaking`)     | N/A             |                    |                 |
| Gate D — Performance budgets within SLO             |                 |                    |                 |
| Gate E — Security scans clean / waived              |                 |                    | N/A             |
| Gate F — Documentation updated                      |                 |                    |                 |

## 2. Production scaling pillar

| Item                                                | Server / Kernel | Adapters / Clients | Pillar 8 cohort |
|-----------------------------------------------------|-----------------|--------------------|-----------------|
| HPA + KEDA scaler tested (drove ≥ 2× scale-out)     |                 | N/A                | N/A             |
| Topology routing config-map deployed                |                 |                    | N/A             |
| DR replicator green for 6h prior to release         |                 | N/A                | N/A             |
| OTel sidecar reporting traces + logs to collector   |                 | N/A                | N/A             |
| PDB respected during simulated drain               |                 | N/A                | N/A             |

## 3. Adapters & clients

| Item                                                | Server / Kernel | Adapters / Clients | Pillar 8 cohort |
|-----------------------------------------------------|-----------------|--------------------|-----------------|
| Unity adapter — `IVXMultiplayer.dll` rebuilt        | N/A             |                    | N/A             |
| JS adapter — `@intelliversex/multiplayer` published | N/A             |                    | N/A             |
| Unreal adapter — plugin recompiled                  | N/A             |                    | N/A             |
| Swift / visionOS — package version bumped           | N/A             |                    | N/A             |
| Lua / Roblox — model version pushed                 | N/A             |                    | N/A             |
| C++ console — header-version compatible             | N/A             |                    | N/A             |
| Pico XR — Unity preset re-validated                 | N/A             |                    | N/A             |

## 4. Pillar 8 cohort tie-in

| Item                                                | Server / Kernel | Adapters / Clients | Pillar 8 cohort |
|-----------------------------------------------------|-----------------|--------------------|-----------------|
| Cohort flag walks dev → staging → canary → prod     | N/A             | N/A                |                 |
| Bot harness ran with cohort flag enabled            |                 | N/A                |                 |
| Manual QA covered both cohort branches              | N/A             |                    |                 |

---

## 5. Risk register

| Risk                                  | Likelihood | Impact | Mitigation |
|---------------------------------------|------------|--------|------------|
| (e.g. KEDA fails to scale)            |            |        |            |
| (e.g. voice provider regression)      |            |        |            |
| (e.g. anchor relocalization on Quest) |            |        |            |

## 6. Rollback plan

* Image tag to roll back to:
* `kubectl rollout undo deployment/intelliverse-nakama-multiplayer`
* If schema-breaking: `proto/multiplayer/*.proto` is forward-compatible
  per `qa-gates.md` Gate A — verify the *previous* server image speaks
  the *new* schema before publishing the new clients.
* Hiro cohort flag rollback: <link to LiveOps console>.

## 7. Sign-off

By signing below, each owner acknowledges:

* They have read and verified the gates above.
* The rollout is on the staged-rollout train; halt rules in
  `staged-rollout.md` Phase 6 / 7 will be honoured.
* If any single gate is `fail`, an explicit waiver is attached with
  a 30-day remediation date and an issue link.

| Role                         | Name | Date | Signature |
|------------------------------|------|------|-----------|
| Backend on-call lead         |      |      |           |
| SDK platform lead            |      |      |           |
| SRE on-call                  |      |      |           |
| Product / LiveOps owner      |      |      |           |

---

> The signed copy goes into `docs/sign-offs/<release-tag>.md` and is
> what the auditor — and our future selves — will read when something
> goes wrong six months from now.
