# IntelliVerseX SDK v6.0.0 — Feature Coverage Matrix

> Honest, audited parity across engines. Prefer this matrix over marketing tables.
> Last updated for package `com.intelliversex.sdk` **6.0.0** (Unity **6000.3**).

**Legend**

| Mark | Meaning |
|------|---------|
| `Y` | Production implementation in this engine SDK |
| `R` | Available via shared Nakama/Hiro **RPC** (thin client wrapper) |
| `P` | Partial (subset of APIs or platform-gated) |
| `S` | Stub / placeholder API (may throw, no-op, or return mock data) |
| `-` | Not present / not planned for this engine |

> **Unity is the reference implementation.** Non-Unity SDKs are strongest for auth, wallet, leaderboards, storage, and generic RPC. AI, Discord, Satori, and many live-ops UIs are Unity-first.

---

## Trust notes (v6.0.0)

- Install pin: `#v6.0.0` at `Packages/com.intelliversex.sdk`
- Bootstrap reports `IVXBootstrapStatus` (`Online` / `Offline` / `Partial` / `Failed`) — do not treat a bool alone as “backend up”
- Social login buttons are **hidden by default** until token providers are wired
- Canonical wallet API: `IVXNWalletManager` (not legacy HTTP stubs)

---

## Core Platform Features

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos | Roblox |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|:------:|
| SDK Init / Config | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Device Auth | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Email Auth | Y | Y | - | Y | Y | Y | Y | Y | Y | Y | - |
| Google Auth | P | P | - | P | P | P | P | P | P | P | - |
| Apple Auth | P | P | - | P | P | P | P | P | P | P | - |
| Session Restore | Y | Y | - | Y | P | Y | Y | Y | Y | Y | - |
| Profile Fetch/Update | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | R |
| Wallet (dual) | Y | R | R | R | R | R | R | R | R | R | R |
| Leaderboards | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | Native |
| Cloud Storage | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | Native |
| Generic RPC | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y | R |
| Real-time Socket | Y | Y | - | P | P | P | Y | Y | P | P | Native |

---

## AI / Discord / Satori

| Feature | Unity | Other engines |
|---------|:-----:|:-------------:|
| AI subsystems (7) | Y (optional `com.intelliversex.sdk.ai`) | Mostly `S` — use HTTP/RPC until ported |
| Discord Social | Y (optional package) | `S` |
| Satori Analytics | Y | `S` / `R` where documented |

---

## Multiplayer

| Feature | Unity | JS/TS | Godot | Defold | Unreal | Others |
|---------|:-----:|:-----:|:-----:|:------:|:------:|:------:|
| Lobby / Matchmaking | Y | Y | Y | Y | P | `S` / `-` |
| Real-time match | Y | Y | Y | Y | P | Mostly `-` |
| Local multiplayer | Y | - | - | - | - | - |

---

## Hiro / Live-Ops

| Feature | Unity | Non-Unity |
|---------|:-----:|:---------:|
| Economy, streaks, rewards, seasons, etc. | Y managers + UI | Prefer `R` (same Nakama RPCs). UI managers are Unity-first. |

Marking every engine `Y` for Hiro UI is **incorrect** — use `R` unless a native manager exists.

---

## Monetization / Quiz / Notifications

| Feature | Unity | Non-Unity |
|---------|:-----:|:---------:|
| Ads / IAP | Y | `-` (platform stores / native) |
| Quiz pipelines | Y | `-` / `S` |
| Push notifications | P | `-` |

---

## Coverage summary (honest)

| Platform | Production depth | Guidance |
|----------|------------------|----------|
| **Unity 6** | Reference (~full lifecycle) | Ship here first |
| **JS/TS** | Strong Nakama + lobby | Best non-Unity client |
| **Godot / Defold** | Auth + RPC + MP sockets | Good for online backends |
| **Unreal / Flutter / Java / C++ / Cocos** | Auth + RPC wrappers | Expect stubs outside core |
| **Web3** | Wallet auth + RPC | Not a full game SDK |
| **Roblox** | Thin IVX + Roblox native | Use Roblox for LB/MP/IAP |

---

## What “Stub” means

`S` = public API may exist for forward compatibility, but behavior is incomplete. Do not claim production readiness for stub cells. Prefer hiding unfinished UI (finish-or-hide) over throwing at runtime.

---

*IntelliVerseX SDK v6.0.0 — honest matrix: Unity reference, RPC elsewhere, stubs labeled.*
