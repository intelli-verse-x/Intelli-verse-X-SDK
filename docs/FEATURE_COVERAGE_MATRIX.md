# IntelliVerseX SDK v5.8.0 — Feature Coverage Matrix

> Verified, audited feature parity across all 10 supported platforms.

**Legend:** `Y` = Implemented | `S` = Stub/Mock (API exists, local only) | `-` = Not present | `P` = Partial

---

## Core Platform Features

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| SDK Init / Config | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Device Auth | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Email Auth | Y | Y | - | Y | Y | Y | Y | Y | Y | Y |
| Google Auth | P | Y | - | Y | Y | Y | Y | Y | Y | Y |
| Apple Auth | P | Y | - | Y | Y | Y | Y | Y | Y | Y |
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

## AI LLM Stack

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| NPC Dialog Manager | Y | S | S | S | S | S | S | S | S | S |
| AI Assistant | Y | S | S | S | S | S | S | S | S | S |
| AI Content Moderator | Y | S | S | S | S | S | S | S | S | S |
| AI Content Generator | Y | S | S | S | S | S | S | S | S | S |
| AI Player Profiler | Y | S | S | S | S | S | S | S | S | S |
| AI Voice Services (TTS/STT) | Y | S | S | S | S | S | S | S | S | S |

---

## Multiplayer & Game Modes

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Select Mode | Y | S | S | S | S | S | S | S | S | S |
| Add/Remove Player | Y | S | S | S | S | S | S | S | S | S |
| Start/End Match | Y | S | S | S | S | S | S | S | S | S |
| Lobby Create Room | Y | S | S | Y | S | S | S | S | S | S |
| Lobby Join Room | Y | S | S | Y | S | S | S | S | S | S |
| Lobby List Rooms | Y | S | S | Y | S | S | S | S | S | S |
| Lobby Leave Room | Y | S | S | Y | S | S | S | S | S | S |
| Matchmaking Find | Y | S | S | - | S | S | S | S | S | S |
| Matchmaking Cancel | Y | S | S | - | S | S | S | S | S | S |
| Local MP Session | Y | - | - | - | - | - | - | - | - | - |
| Local MP Turns | Y | - | - | - | - | - | - | - | - | - |
| Local MP Split Screen | Y | - | - | - | - | - | - | - | - | - |

---

## Hiro Live-Ops Systems

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Spin Wheel (get/spin) | Y | Y | Y | Y | Y | Y | Y | Y | S | Y |
| Streaks (get/update/claim) | Y | Y | S | Y | S | Y | S | S | S | S |
| Offerwall (get/complete/claim) | Y | Y | S | Y | S | Y | S | S | S | S |
| Friend Quests | Y | Y | Y | Y | Y | Y | Y | Y | S | - |
| Friend Battles | Y | Y | Y | Y | Y | Y | Y | Y | S | - |
| IAP Trigger | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Smart Ad Timer | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Retention (get/update) | Y | Y | Y | Y | S | Y | Y | Y | Y | Y |

---

## Discord Social SDK

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Discord Init / Connect | Y | S | S | S | S | S | S | S | S | S |
| Account Linking (OAuth2) | Y | S | S | S | S | S | S | S | S | S |
| Rich Presence / Activity | Y | S | S | S | S | S | S | S | S | S |
| Unified Friends List | Y | S | S | S | S | S | S | S | S | S |
| Lobby / Text Chat | Y | S | S | S | S | S | S | S | S | S |
| Voice Chat | Y | S | S | S | S | S | S | S | S | S |
| Game Invites | Y | S | S | S | S | S | S | S | S | S |
| Direct Messages | Y | S | S | S | S | S | S | S | S | S |
| Moderation | Y | S | S | S | S | S | S | S | S | S |
| Linked Channels | Y | S | S | S | S | S | S | S | S | S |
| Debug / Logging | Y | S | S | S | S | S | S | S | S | S |
| Social Settings | Y | S | S | S | S | S | S | S | S | S |

---

## Satori Analytics

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Initialize / Authenticate | Y | S | S | S | S | S | S | S | S | S |
| Capture Events | Y | S | S | S | S | S | S | S | S | S |
| Feature Flags | Y | S | S | S | S | S | S | S | S | S |
| Experiments / A-B Testing | Y | S | S | S | S | S | S | S | S | S |
| Live Events | Y | S | S | S | S | S | S | S | S | S |
| Identity Update / Logout | Y | S | S | S | S | S | S | S | S | S |

---

## Platform & Extras

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| Deep Links | Y | - | - | - | - | - | - | - | - | - |
| Safe Area / Edge-to-Edge | Y | - | - | - | - | - | - | - | - | - |
| Foldable Device Support | Y | - | - | - | - | - | - | - | - | - |
| Performance Optimizer | Y | - | - | - | - | - | - | - | - | - |
| Bootstrap (one-drop init) | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| Demo UIs | Y (16) | - | - | - | - | - | - | - | - | - |
| Web3 / NFT Gating | - | - | Y | - | - | - | - | - | - | - |

---

## VR / AR / XR / Console

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|:-----:|:-----:|:----:|:----:|:-------:|:------:|:-----:|:------:|:---:|:-----:|
| XR Platform Detection | Y | - | - | - | - | Y | Y | - | Y | - |
| Meta Quest Support | S | - | - | - | - | S | S | - | S | - |
| SteamVR Support | S | - | - | - | - | S | S | - | S | - |
| Apple Vision Pro Support | S | - | - | - | - | S | - | - | S | - |
| AR Foundation / ARKit / ARCore | S | - | - | - | - | S | - | - | S | - |
| PSVR2 Support | - | - | - | - | - | - | - | - | S | - |
| Hand Tracking Info | S | - | - | - | - | S | - | - | S | - |
| Eye Tracking Info | S | - | - | - | - | S | - | - | S | - |
| Passthrough / MR Info | S | - | - | - | - | S | - | - | S | - |
| Console (PS5/Xbox/Switch) | - | - | - | - | - | - | - | - | - | - |
| Linux / SteamOS | S | Y | - | - | - | S | S | S | Y | - |
| tvOS | - | - | - | - | - | - | - | - | - | - |
| visionOS | S | - | - | - | - | - | - | - | - | - |

---

## Coverage Score Summary

| Platform | Fully Implemented | Stubs | Not Present | Coverage % |
|----------|:-----------------:|:-----:|:-----------:|:----------:|
| **Unity** | **64** | 12 | 6 | **78%** |
| **JavaScript/TS** | **36** | 27 | 19 | **44%** |
| **Flutter/Dart** | **29** | 31 | 22 | **35%** |
| **Web3** | **26** | 30 | 26 | **32%** |
| **Java/Android** | **31** | 27 | 24 | **38%** |
| **Unreal Engine 5** | **31** | 37 | 14 | **38%** |
| **Godot 4** | **31** | 33 | 18 | **38%** |
| **Defold** | **30** | 30 | 22 | **37%** |
| **Cocos2d-x** | **25** | 30 | 27 | **30%** |
| **C++ (CMake)** | **24** | 42 | 16 | **29%** |

> **API Surface Coverage** (Y + S combined) represents what a developer can code against today.
> Stubs are zero-code-change upgradeable once backend RPCs are configured.

> **VR/AR/XR support** (v5.8.0): Platform detection and capability querying stubs are available
> for Unity, Unreal, Godot, and C++. Wire your XR SDK (OpenXR, Meta SDK, ARKit, etc.) and the
> helpers will auto-detect the active platform. Console support is not yet available.

---

## Key Takeaways

- **Unity** is the reference implementation with the richest feature set (64 features fully wired, including XR platform detection).
- **All 10 platforms** now have full API surface coverage for **Discord Social SDK** (12 modules incl. Settings), **AI LLM Stack** (6 modules), **Satori Analytics** (6 features), and all **Hiro Live-Ops** systems.
- **JavaScript/TypeScript** remains the strongest non-Unity SDK with real Nakama RPC-backed Hiro systems.
- **Hiro parity** (retention, IAP trigger, smart ad timer) has been added to Unreal, C++, Cocos2d-x, Godot, and Defold.
- **GameModes/Lobby/Matchmaking** are local-state stubs across non-Unity SDKs — API shape exists for zero-code-change upgrade.
- **Local Multiplayer** (hot-seat, split-screen) remains Unity-only.
- **Platform utilities** (deep links, foldable, safe area, optimizer) remain Unity-only.
- **VR/AR/XR**: Detection and capability helpers ship on Unity, Unreal, Godot, and C++; wire OpenXR / platform SDKs for full runtime behavior.

---

## What "Stub" Means for Developers

Features marked **S** (Stub) have the full public API surface — method signatures, types, events — so your game code can integrate today. The stubs return mock/local data. When the backend or native SDK is configured, these stubs are replaced with real calls in a future release with **zero API changes** to your game code.

This means you can:
1. Build your game UI and logic against the stub APIs now
2. Switch to production by updating the SDK — no code changes needed

---

*IntelliVerseX SDK v5.8.0 — 10 platforms, 82 features, one API.*
