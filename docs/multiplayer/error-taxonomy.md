# IVX Multiplayer — Error & Warning Taxonomy

**Status:** stable contract.
**Schema source of truth:** [`schemas/multiplayer/envelope.proto`](../../schemas/multiplayer/envelope.proto)
**Mirrors:** `data/modules/src/multiplayer-kernel/types.ts`,
`Assets/Intelli-verse-X-SDK/MultiplayerKernel/Wire/IVXWireConstants.cs`,
`SDKs/javascript/packages/multiplayer/src/wire/constants.ts`.

This document is the **canonical retry-policy reference**. Every adapter,
kernel template, and dashboard must obey these semantics. Codes are stable
forever; new codes are appended, never repurposed.

---

## Surface

A failure that ends or interrupts an operation is an **Error**
(`OP_ERROR = 0x0008`). A failure that **does not** end the operation is a
**Warning** (`OP_WARN_*` opcodes 0x0013–0x0018, also delivered as a generic
`WarningEnvelope`).

```
ErrorEnvelope        — operation failed; caller decides retry
WarningEnvelope      — operation succeeded but degraded; never end-of-match
```

Adapters expose two callbacks:

```ts
session.onError(env => /* surface to user / retry per table */);
session.onWarning(env => /* log + telemetry; gameplay continues */);
```

---

## ErrorCode reference

Codes are grouped by range. **Range tells you the retry policy at a glance.**

### 1–9 · Schema, time, frame

| Code | Name | Caller action |
|---|---|---|
| 1 | `SCHEMA_TOO_OLD` | Upgrade client. Adapter surfaces `min_required_version`. |
| 2 | `SERVER_TOO_OLD` | Wait for server upgrade; client may opt-in to compat shim. |
| 3 | `BAD_PAYLOAD` | Drop & log. Almost always a bug — never retry blind. |
| 4 | `SEQ_GAP` | Issue `MATCH_RESUME { last_seen_seq }`. Adapter handles. |
| 5 | `UNKNOWN_OPCODE` | Drop. Indicates client/server schema drift. |
| 6 | `DUPLICATE_OPCODE` | Idempotency hit — server already processed. No-op. |
| 7 | `CLOCK_SKEW_EXTREME` | Run `NETWORK_CLOCK_PING/PONG`. Adapter retries automatically. |
| 8 | `MATCH_STATE_LARGE` | Fatal for one client; server may snapshot-resync. |

### 20–29 · Capacity & membership

| Code | Name | Caller action |
|---|---|---|
| 20 | `MATCH_FULL` | Use matchmaking to find another instance. |
| 21 | `MATCH_NOT_FOUND` | Match ended/expired. Do not retry. |
| 22 | `NOT_A_MEMBER` | Re-establish presence (rejoin). |
| 23 | `RATE_LIMITED` | Backoff with `retry_after_ms` jitter (±25%). |
| 24 | `FLAPPING` | Hard ban (default 60 s) — surface to user, do not retry. |
| 25 | `MATCH_ENDED` | Match-fatal. Show end-screen. |
| 26 | `SESSION_REPLACED` | Same user_id signed in elsewhere; close socket. |

### 30–39 · Auth & permission

| Code | Name | Caller action |
|---|---|---|
| 30 | `PERMISSION_DENIED` | Surface to user; do not retry. |
| 31 | `KICKED` | Show modal; do not auto-rejoin. |
| 32 | `BANNED` | Hard fail. |
| 33 | `NOT_AUTHORIZED` | Refresh auth token, retry once. |

### 40–49 · Agent

| Code | Name | Caller action |
|---|---|---|
| 40 | `BAD_PERSONA` | Persona id invalid; do not retry. |
| 41 | `BUDGET_EXCEEDED` | Agent muted by kernel. Game continues. |
| 42 | `AGENT_PROVIDER_DOWN` | Kernel auto-fails-over to degraded provider. |

### 50–59 · XR / spatial

| Code | Name | Caller action |
|---|---|---|
| 50 | `ANCHOR_INCOMPAT` | Use QR/marker fallback; or PCVR pseudo-anchor. |
| 51 | `ANCHOR_LOST` | Initiate relocalization; opt out to fake anchor after timeout. |

### 60–69 · Voice

| Code | Name | Caller action |
|---|---|---|
| 60 | `VOICE_UNAVAILABLE` | Mute UI, gameplay continues. |
| 61 | `VOICE_PERMISSION_DENIED` | Surface OS-permission UX. |

### 70–79 · Moderation

| Code | Name | Caller action |
|---|---|---|
| 70 | `MODERATION_BLOCKED` | Terminal for that utterance. Appeal flow only. |

### 80–89 · Lifecycle (match-fatal)

| Code | Name | Caller action |
|---|---|---|
| 80 | `TIMEOUT` | Match terminated. Show end-screen. |
| 81 | `QUORUM_LOST` | Match terminated. |
| 82 | `DURATION_EXCEEDED` | Match terminated. |
| 83 | `STATE_OVERFLOW` | Match terminated; report bug. |

### 90–99 · Capability

| Code | Name | Caller action |
|---|---|---|
| 90 | `CAPABILITY_UNSUPPORTED` | Drop the feature; degrade gracefully. |

### 100–119 · Infra (transient, kernel may auto-recover)

| Code | Name | Caller action |
|---|---|---|
| 100 | `OVERLOAD` | Backoff then retry. Kernel may shed load. |
| 101 | `PERSISTENCE_DEGRADED` | Match continues; result envelope may be delayed. |
| 102 | `TICK_OVERRUN_DEGRADED` | Reduce input rate; kernel may force LOW_BANDWIDTH. |
| 103 | `PROVIDER_UNAVAILABLE` | LiveKit / ASR / LLM down — degraded path engaged. |

### 999 · Catch-all

| Code | Name | Caller action |
|---|---|---|
| 999 | `INTERNAL` | Log + alert; treat as transient. Pillar-10 alerts on rate. |

---

## WarningCode reference

Warnings never end a match. Each maps 1:1 to a typed `WARN_*` opcode body.

| Code | Name | Trigger |
|---|---|---|
| 1 | `RATE_LIMITED` | Per-user budget hit (e.g. ConvParty reactions). |
| 2 | `TICK_OVERRUN` | Server tick took longer than budget. |
| 3 | `MATCH_STATE_LARGE` | Approaching state-size guard. |
| 4 | `AVATAR_FALLBACK` | LOD demoted (distance / bandwidth / occlusion). |
| 5 | `DEPRECATED_CLIENT` | Client version sunset coming; upgrade to avoid `SCHEMA_TOO_OLD`. |
| 6 | `STATE_REBUILT` | Resume couldn't replay; full snapshot delivered. |
| 7 | `LOW_BANDWIDTH` | Server is throttling tick rate / fidelity. |
| 8 | `AGENT_DEGRADED` | Agent failover (small model / static response). |
| 9 | `CLOCK_REALIGN` | Server stepped client clock by > drift threshold. |

---

## Adapter contract

```text
1. ALWAYS surface the integer code, even when the adapter's generated enum
   predates a value. (Forward-compat — never silently drop.)
2. Errors in the 1-9 / 80-89 ranges are TERMINAL for the operation.
3. Errors in the 100-119 range are TRANSIENT and the adapter SHOULD retry
   with exponential backoff capped at 30s.
4. Warnings are NEVER fatal — log + emit OnWarning, do not throw.
5. When `retry_after_ms` is present, honour it (jittered ±25%).
6. When `min_required_version` is present, surface it to the upgrade UX.
```

---

## Dashboard SLOs (Pillar 10)

The SLO board groups errors by range so a single alert fires per cluster:

```
slo: error_rate_terminal     = sum(code in [80..89, 25])  / total < 0.1%
slo: error_rate_transient    = sum(code in [100..119])    / total < 1.0%
slo: error_rate_capability   = sum(code in [90..99])      / total < 0.5%
slo: warn_rate_avatar_fallback = warn_code == 4           / total < 5%
slo: warn_rate_tick_overrun  = warn_code == 2             / total < 0.5%
```

Each maps to a Grafana panel + PagerDuty rule (see
`docs/multiplayer/slo-board.md`).
