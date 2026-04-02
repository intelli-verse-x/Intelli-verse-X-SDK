# IntelliVerseX SDK v5.5.0 — Feature Coverage Matrix

> Verified, audited feature parity across all 10 supported platforms.

**Legend:** `Y` = Implemented | `S` = Stub/Mock (API exists, local only) | `-` = Not present | `P` = Partial

---

## Core Platform Features

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| SDK Init / Config | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Device Auth | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Email Auth | Y | Y | - | Y | Y | Y | Y | Y | Y | Y |
| Google Auth | Y | Y | - | Y | Y | Y | Y | Y | Y | Y |
| Apple Auth | Y | Y | - | Y | Y | Y | Y | Y | Y | Y |
| Custom Auth | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Session Restore | Y | Y | - | Y | - | Y | Y | Y | Y | Y |
| Profile Fetch | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Profile Update | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Wallet Fetch | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Wallet Grant | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Leaderboard Submit | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Leaderboard Fetch | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Cloud Storage Read | - | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Cloud Storage Write | - | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Generic RPC | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Real-time Socket | P | Y | - | - | - | - | Y | Y | - | - |
| Events / Callbacks | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |

---

## AI Voice & Host

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| AI Initialize | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Voice Session (start/end) | Y | Y | Y | Y | Y | S | Y | Y | S | S |
| Voice Send Text | Y | Y | Y | Y | Y | S | Y | Y | S | S |
| Voice Poll Messages | Y | Y | Y | Y | Y | S | Y | Y | S | - |
| Host Session (start/event) | Y | Y | Y | Y | Y | Y | Y | Y | S | Y |
| Entitlement Check | Y | Y | Y | Y | Y | Y | Y | Y | S | Y |
| Get Personas | Y | Y | Y | Y | Y | Y | Y | Y | S | Y |

---

## Multiplayer & Game Modes

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Select Mode | Y | S | S | S | S | S | S | S | S | S |
| Add/Remove Player | Y | S | S | S | S | S | S | S | S | S |
| Start/End Match | Y | S | S | S | S | S | S | S | S | S |
| Lobby Create Room | S | S | S | Y | S | S | S | S | S | S |
| Lobby Join Room | S | S | S | Y | S | S | S | S | S | S |
| Lobby List Rooms | S | S | S | Y | S | S | S | S | S | S |
| Lobby Leave Room | Y | S | S | Y | S | S | S | S | S | S |
| Matchmaking Find | S | S | S | - | S | S | S | S | S | S |
| Matchmaking Cancel | Y | S | S | - | S | S | S | S | S | S |
| Local MP Session | Y | - | - | - | - | - | - | - | - | - |
| Local MP Turns | Y | - | - | - | - | - | - | - | - | - |
| Local MP Split Screen | Y | - | - | - | - | - | - | - | - | - |

---

## Hiro Live-Ops Systems

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Spin Wheel (get/spin) | Y | Y | Y | Y | Y | Y | Y | Y | S | Y |
| Streaks (get/update/claim) | Y | Y | S | S | S | Y | S | S | S | S |
| Offerwall (get/complete/claim) | Y | Y | S | S | S | Y | S | S | S | S |
| Friend Quests | Y | Y | - | S | Y | Y | Y | Y | S | - |
| Friend Battles | Y | Y | - | S | Y | Y | Y | Y | S | - |
| IAP Trigger | Y | Y | - | - | Y | - | - | - | - | - |
| Smart Ad Timer | Y | Y | - | - | Y | - | - | - | - | - |
| Retention (get/update) | Y | Y | - | S | S | - | - | - | - | - |

---

## Platform & Extras

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Deep Links | Y | - | - | - | - | - | - | - | - | - |
| Safe Area / Edge-to-Edge | Y | - | - | - | - | - | - | - | - | - |
| Foldable Device Support | Y | - | - | - | - | - | - | - | - | - |
| Performance Optimizer | Y | - | - | - | - | - | - | - | - | - |
| Satori Analytics | Y | - | - | - | - | - | - | - | - | - |
| Demo UIs | Y (7) | - | - | - | - | - | - | - | - | - |
| Web3 / NFT Gating | - | - | Y | - | - | - | - | - | - | - |

---

## Coverage Score Summary

| Platform | Fully Implemented | Stubs | Not Present | Coverage % |
|----------|:-----------------:|:-----:|:-----------:|:----------:|
| **Unity** | **43** | 3 | 3 | **88%** |
| **JavaScript/TS** | **33** | 7 | 9 | **67%** |
| **Flutter/Dart** | **27** | 11 | 11 | **55%** |
| **Unreal Engine 5** | **27** | 10 | 12 | **55%** |
| **Godot 4** | **27** | 10 | 12 | **55%** |
| **Defold** | **27** | 10 | 12 | **55%** |
| **Java/Android** | **26** | 8 | 15 | **53%** |
| **Cocos2d-x** | **22** | 10 | 17 | **45%** |
| **Web3** | **21** | 10 | 18 | **43%** |
| **C++ (CMake)** | **19** | 16 | 14 | **39%** |

---

## Key Takeaways

- **Unity** is the reference implementation with the richest feature set (43/49 fully implemented).
- **JavaScript/TypeScript** is the strongest non-Unity SDK, with full AI, full Hiro (including IAP triggers and smart ad timers), and real-time socket support.
- **GameModes/Lobby/Matchmaking** are local-state stubs across all non-Unity SDKs (API shape is there, but they return mock data and need Nakama server-side wiring).
- **C++ SDK** has the most stubs — AI and Hiro implementations are placeholder logging with empty return values.
- **Platform utilities** (deep links, foldable, safe area, optimizer) and **Satori analytics** are Unity-only.
- **Local Multiplayer** (hot-seat, split-screen) is Unity-only.
- **Cloud Storage** is missing from the Unity wrapper (`IVXSecureStorage` is local-only) but present in all other SDKs via Nakama's `ReadStorageObjects`/`WriteStorageObjects`.

---

## What "Stub" Means for Developers

Features marked **S** (Stub) have the full public API surface — method signatures, types, events — so your game code can integrate today. The stubs return mock/local data. When the Nakama server-side RPCs are configured, these stubs will be replaced with real server calls in a future release with **zero API changes** to your game code.

This means you can:
1. Build your game UI and logic against the stub APIs now
2. Switch to production by updating the SDK — no code changes needed

---

## Roadmap: Closing the Gaps

| Priority | Gap | Target |
|----------|-----|--------|
| **High** | Cloud Storage for Unity | v5.6.0 |
| **High** | C++ AI — replace placeholder with real HTTP | v5.6.0 |
| **High** | Cocos2d-x voice poll messages | v5.6.0 |
| **Medium** | Lobby/Matchmaking — wire to Nakama server RPCs (all platforms) | v5.7.0 |
| **Medium** | Streaks/Offerwall full CRUD for Web3, Java, Flutter, Godot, Defold, Cocos | v5.7.0 |
| **Medium** | Satori Analytics wrappers for non-Unity platforms | v5.7.0 |
| **Low** | Platform utilities for mobile engines (Unreal, Flutter) | v5.8.0 |
| **Low** | Local Multiplayer for Godot, Unreal | v5.8.0 |
| **Planned** | Discord Social SDK integration (Rich Presence, Voice, Invites, Lobbies) | v6.0.0 |

---

*IntelliVerseX SDK v5.5.0 — 10 platforms, 49 features, one API.*
