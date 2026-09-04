# World-class Control Plane Plan

**Status:** Plan (research-backed)  
**Date:** 4 Sep 2026  
**Companion:** [SYSTEM_DESIGN.md](SYSTEM_DESIGN.md)  
**Sources:** Graphify (`nakama/graphify-out`, 11,457 nodes), Firecrawl (Nakama Unity guide, Amplitude Wizard, Firebase Unity setup), existing `IVXControlCenter`, `IVXNManager`, `COMPLETE_RPC_REFERENCE.md`

This plan turns “paste Game ID → Welcome to IntelliVerse → one panel for everything” into an implementable control plane: **onboarding, request management, API catalog, and bug-proof shipping**.

---

## 1. North star (the tada)

**User does one thing:** paste a Game ID (UUID).

**SDK does the rest:**

1. Resolve game from dashboard (`POST msapi.../api/games/game/info` already named on `IVXBootstrapConfig`).
2. Write host, port, SSL, display name onto `IVXBootstrapConfig`. Never ask for server key in the kid path if the dashboard returns a client-safe bootstrap token; otherwise use a one-time “server key” field with a Show/Hide toggle (Stripe-style secret).
3. Ensure Nakama package + Newtonsoft (silent install).
4. Add `IVX Bootstrap` to the active scene.
5. Device-authenticate (Heroic Labs pattern: persist `deviceId`, `AuthenticateDeviceAsync`, refresh session if near expiry, catch `ApiResponseException`).
6. Call **`create_or_sync_user`** (Graphify/docs: this is the **foundation RPC**; wallets and leaderboards fail with “Identity not found” without it).
7. Show **Welcome to IntelliVerse** with the game name, connection green, and “You’re in.”

If any step fails, the panel stays on that step with **one sentence + one button**. No 10 tabs.

Analogues (Firecrawl):

| Product | Pattern we copy |
|---------|-----------------|
| **Amplitude Wizard** | One key in; wizard installs and wires the rest (including agent mode). |
| **Firebase Unity** | Project exists in a console first; the client only drops config. We collapse their 5 steps into **Game ID = the config**. |
| **Nakama Unity** | Device auth + session refresh + typed exceptions + Android “Use System TLS”. |
| **Stripe keys** | Publishable vs secret. Game ID is public; server key is secret and never in git. |

---

## 2. What Graphify says is the core (do not invent a second core)

These are the real hubs. The control plane must expose them, not hide a parallel stack.

| Core | Why it matters | Control panel must show |
|------|----------------|-------------------------|
| **GameID System** | Namespaces `{gameID}_wallets`, `{gameID}_profiles`, leaderboards. Wrong ID = empty world, not a crash. | Game ID, isolation collections, “this game vs global wallet” |
| **`create_or_sync_user`** | First RPC; identity sync gate on `IVXNManager.IsIdentitySynced` | Identity: synced / missing / retry |
| **`.RegisterRpc()` + trace logger** | Every RPC gets a runtime context + trace id | Live request list with trace ids |
| **Wallet storage + `create_or_get_wallet`** | Server authoritative; cache + loading + errors + offline (docs best practices) | Wallet health, last sync, errors |
| **`kioskArcadeRateLimited` / error `retry_after_ms`** | Rate limit already exists in modules and in the JS multiplayer envelope | Retry-After honored by one client bus |
| **`nakama_js_health` / HealthCache probe** | HTTP 200 on `/healthcheck` can lie if JS bundle failed | Dual health: Nakama HTTP + JS RPC |
| **`safeRpc()` (MCP analytics)** | Wrap RPC so one failure doesn’t kill the tool | Same idea on Unity: never unhandled RPC |
| **Generated RPC index vs hand-written docs** | Docs have lagged (50+ missing RPCs). Truth = `npm run docs:rpc-index` | Panel API tab reads **generated** index, not the wiki |

**Invariant:** `IVXBootstrap` + `IVXNManager` must share **one** Game ID and session. Today bootstrap config and `IntelliVerseXConfig` can diverge — that is a bug factory. Unify on `IVXBootstrapConfig`.

---

## 3. One control panel (information architecture)

Rename the Editor window to **IntelliVerse** (not “Setup Wizard”). Three modes, same window.

```text
┌─────────────────────────────────────────────────────────────┐
│  IntelliVerse                         ● Connected  QuizVerse │
│  Welcome, {gameName}.  Game ID  126bf539-…                   │
├──────────┬──────────────────────────────────────────────────┤
│ Home     │  Status grid: Identity · Wallet · Live-ops · MP  │
│ Traffic  │  In-flight RPCs, retries, circuit, last errors   │
│ APIs     │  Generated RPC catalog, filter by module         │
│ Modules  │  Toggles from IVXBootstrapConfig (Advanced)      │
│ Logs     │  Correlation id, copy-to-clipboard               │
└──────────┴──────────────────────────────────────────────────┘
```

### Home (“Welcome to IntelliVerse”)

- Big state: **Not connected / Connecting / Welcome**.
- Game name, Game ID, host, session user id (truncated).
- Next action only: Paste ID, Retry, Open scene, Play.
- Kid never sees Hiro/Satori names until Modules.

### Traffic (request management)

All SDK network goes through **`IVXRequestBus`** (new, Core). Managers must not call `_client.RpcAsync` directly.

| Concern | Policy |
|---------|--------|
| Timeout | Default 8s RPC, 15s auth; per-call override |
| Retry | Exponential backoff + jitter; **only** 5xx, timeout, `retry_after_ms` |
| Idempotency | Client opcode UUID (already on multiplayer envelope) reused for RPCs |
| Circuit breaker | Open after N failures per RPC id; half-open probe |
| Rate limit | Honor `retry_after_ms`; coalesce duplicate in-flight calls (same RPC+payload hash) |
| Cancel | `CancellationToken` on domain reload / scene unload |
| Trace | `trace_parent` + Game ID on every call (matches Nakama `RuntimeLoggerWithTraceId`) |
| Envelope | `{ code, detail, retry_after_ms, min_required_version }` — already in JS multiplayer; **make this the Unity standard** |
| Offline | Queue non-mutating telemetry; never queue wallet grants |

The Traffic tab is a live table: time, RPC id, attempt, status, latency, retry-at. This is how we keep the SDK “many requests” without silent storms.

### APIs

- Load `RPC_INDEX_GENERATED.md` (or JSON export) at Editor time; fallback “index missing — run `npm run docs:rpc-index`”.
- Group: Identity, Wallet, Social, Quiz, Tournaments, Health.
- Each row: name, auth required, whether client calls it, last result.
- **Contract test:** Unity EditMode test fails if a shipped facade RPC id is not in the generated index (kills doc/code drift).

### Modules

- Mirrors `IVXBootstrapConfig` toggles.
- Default **off** for Discord/AI; Hiro/Satori on only after identity sync.
- Advanced Setup wizard becomes a button here, not a second home.

### Logs

- Filter by correlation id.
- One-click copy for support.

---

## 4. Runtime “Welcome” (in the game, not only Editor)

A tiny, branded overlay (uGUI, SDK-owned, not a consumer theme):

- First successful `OnBootstrapComplete(true)` → “Welcome to IntelliVerse” 2.5s, then fade.
- Fail → “Couldn’t reach IntelliVerse” + Retry (calls RequestBus).
- Disable via config `_showWelcomeOverlay` for production games that have their own splash.

This is the emotional “tada.” Editor panel is for developers; overlay is for Play Mode proof.

---

## 5. Robustness engineering (fewer bugs, not more features)

| Layer | What we add | Bug it prevents |
|-------|-------------|-----------------|
| Config | Single SO; Game ID validated as UUID or legacy slug | Empty world / wrong namespace |
| Auth | Device id persistence; session refresh; Android System TLS checklist in Control Center | Sudden Android TLS failures (Nakama docs) |
| Identity | Hard gate: no wallet/leaderboard RPC until `IsIdentitySynced` | “Identity not found” |
| Requests | One bus, no parallel retry loops in each manager | Retry storms, duplicate spends |
| Server | Health RPC first; plugin try/catch (already in `InitModule`) | One plugin killing JS runtime |
| Contracts | Generated RPC index vs Unity string literals | Calling dead RPCs |
| Tests | EditMode bus tests (retry, coalesce, circuit); PlayMode connect with mock/local Nakama | Regressions on ship |
| CI | `unity test` + `validate.ps1` + `nakama_js_health` | Green HTTP / dead RPCs |
| Observability | Traffic tab + AnalyticsAlerts timing (already wrapping server RPCs) | Blind production |

**Wallet (from Graphify-linked docs):** cache, loading states, graceful errors, offline **read** only. Never treat leaderboard score as wallet balance.

---

## 6. Scalability (ship features in days)

Keep the factory from SYSTEM_DESIGN, with these extra rails:

1. **Flag** on bootstrap (default off).
2. **RPC** in `data/modules` + appears in generated index.
3. **Facade** that only talks to `IVXRequestBus`.
4. **Home tile** auto-appears when the flag is on and the RPC exists (no new menu).
5. **EditMode** test: happy path + 429 with `retry_after_ms`.

If a feature needs a new Editor tab, it is rejected. It gets a Home tile or a Modules row.

---

## 7. Build sequence (best path, not a rewrite)

### Phase A — Magic connect (1 slice)

- Control Center: **only Game ID field** on first run.
- Resolve + write config + bootstrap in scene.
- Device auth + `create_or_sync_user`.
- Welcome state in Editor + optional Play Mode overlay.
- Unify `IVXNManager` to read `IVXBootstrapConfig` (stop dual config).

### Phase B — Request bus

- `IVXRequestBus` in Core.
- Migrate `IVXNManager.Rpc` path onto the bus (highest traffic).
- Traffic tab: in-flight + last 50.

### Phase C — API catalog + contracts

- Consume generated RPC index in the panel.
- EditMode: facade RPC ids ⊂ generated index.
- Dual health: TCP + `nakama_js_health`.

### Phase D — Harden

- Circuit breaker, coalescing, Android TLS check.
- Wallet/identity gates in the bus (policy by RPC name prefix).
- Chaos: drop network in Play Mode, assert overlay + retry.

### Phase E — Thin Advanced Setup

- Extract remaining wizard tabs behind Modules → Advanced.
- No new top-level menus.

Do **not** merge dual Asset trees in this plan (ADR-002). Do **not** add UGS. Do **not** fork Nakama Go.

---

## 8. Success metrics (world-class is measurable)

| Metric | Target |
|--------|--------|
| Time to “Welcome” from package import | < 10 minutes, one paste |
| Fields a kid must type | **1** (Game ID); 0 if dashboard deep-link later |
| Uncaught RPC exceptions in samples | 0 |
| Duplicate in-flight identical RPCs | 0 (coalesced) |
| Facade RPC not in generated index | CI fail |
| Identity-not-found in first-run sample | 0 |
| New feature: extra Editor menus | 0 |

---

## 9. Script ownership for this plan

| New / changed | Responsibility |
|---------------|----------------|
| `IVXControlCenter.cs` | Game ID paste, Welcome UI, tabs Home/Traffic/APIs/Modules/Logs |
| `IVXWelcomeOverlay.cs` (runtime, uGUI) | In-game tada / fail / retry |
| `IVXGameDirectory.cs` (Editor+runtime) | Resolve Game ID → host/port/ssl |
| `IVXRequestBus.cs` | All RPC/HTTP policy |
| `IVXRequestProbeWindow` (or Traffic view) | Live requests |
| `IVXRpcCatalog.cs` | Load generated index |
| `IVXNManager.cs` | Use bus + bootstrap config only |
| `IVXBootstrap.cs` | Order: client → device auth → create_or_sync_user → modules |
| Nakama `InitModule` | Unchanged pattern; health first |

---

## 10. Decision

**Best of the best here is not more modules.** It is: **one credential, one bus, one panel, one generated API truth, one welcome.**

Next implementation slice: **Phase A** (Game ID → Welcome) on top of the Control Center already in the repo.
