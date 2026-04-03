# Cross-Platform SDK

**Skill ID:** `ivx-cross-platform`

---
name: ivx-cross-platform
description: >-
  Port IntelliVerseX SDK features between game engines and platforms. Use when the
  user says "port to Unreal", "port to Godot", "add JavaScript SDK", "cross-platform",
  "multi-engine", "feature parity", "platform support", "which features work on",
  "stub vs real",   or needs help understanding cross-platform coverage.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The IntelliVerseX SDK supports 10 platforms. Unity is the reference implementation with full coverage. Other platforms implement a subset of features, with the rest available as API-surface stubs so code compiles and can be incrementally implemented.

---

## When to Use

Ask your AI agent any of these:

- "Port my game from Unity to Godot"
- "Which features work on Unreal Engine?"
- "Convert the Hiro economy stub to a real implementation on Flutter"
- "What's the JavaScript equivalent of IVXLobbyManager?"
- "Add leaderboard support to the C++ SDK"
- "What's the coverage for the Defold SDK?"

---

## What the Agent Does

```mermaid
flowchart TD
    A[You: "Port to Godot"] --> B[Agent loads ivx-cross-platform skill]
    B --> C[Shows Godot feature coverage]
    C --> D[Maps Unity classes to Godot equivalents]
    D --> E[Identifies RPC contract from Unity reference]
    E --> F[Creates typed RPC wrapper in GDScript]
    F --> G[Builds manager with caching + signals]
    G --> H[Replaces stub with real implementation]
    H --> I[Updates coverage matrix]
```

---



## 1. Supported Platforms

| # | Platform | Language | Package Format | Min Version |
|---|----------|----------|---------------|-------------|
| 1 | **Unity** | C# | UPM (git) | 2021.3 LTS |
| 2 | **JavaScript / TypeScript** | TS | npm | Node 18+ |
| 3 | **Web3** | TS | npm (ethers.js) | Node 18+ |
| 4 | **Java / Android** | Java | Gradle / Maven | Java 11+ |
| 5 | **Flutter / Dart** | Dart | pub.dev | Dart 3.0+ |
| 6 | **Unreal Engine 5** | C++ / BP | .uplugin | UE 5.3+ |
| 7 | **Godot 4** | GDScript / C# | Addon | Godot 4.2+ |
| 8 | **Defold** | Lua | Library module | Defold 1.6+ |
| 9 | **C++** | C++ | CMake | C++17 |
| 10 | **Cocos2d-x** | C++ | CMake | v4.0+ |

---

## 2. Feature Coverage Matrix

### Legend

| Symbol | Meaning |
|--------|---------|
| **Y** | Real implementation — fully functional |
| **S** | Stub — API surface exists, calls compile, returns defaults or calls same RPCs via HTTP |
| **-** | Not applicable to this platform |

### Matrix

| Feature | Unity | JS/TS | Web3 | Java | Flutter | Unreal | Godot | Defold | C++ | Cocos |
|---------|-------|-------|------|------|---------|--------|-------|--------|-----|-------|
| **Core Bootstrap** | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| **Auth (Email/Pass)** | Y | Y | Y | Y | Y | S | S | S | S | S |
| **Auth (Social)** | Y | Y | S | Y | Y | S | S | S | S | S |
| **Auth (Guest)** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Economy** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Achievements** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Energy** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Inventory** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Progression** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Streaks** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Store** | Y | Y | S | Y | S | S | S | S | S | S |
| **Hiro Leaderboards** | Y | Y | S | Y | S | S | S | S | S | S |
| **Satori Analytics** | Y | S | S | S | S | S | S | S | S | S |
| **Satori Flags** | Y | S | S | S | S | S | S | S | S | S |
| **Satori Experiments** | Y | S | S | S | S | S | S | S | S | S |
| **Multiplayer Lobby** | Y | Y | - | Y | S | S | S | S | S | S |
| **Matchmaking** | Y | Y | - | Y | S | S | S | S | S | S |
| **Real-Time Socket** | Y | Y | - | Y | S | S | S | S | S | S |
| **Local Multiplayer** | Y | - | - | - | - | - | - | - | - | - |
| **Ads (LevelPlay)** | Y | - | - | Y | S | - | - | - | - | - |
| **Ads (Appodeal)** | Y | - | - | S | S | - | - | - | - | - |
| **Ads (AdMob)** | Y | - | - | Y | S | - | - | - | - | - |
| **IAP** | Y | - | - | Y | S | S | S | - | - | - |
| **Offerwall** | Y | - | - | S | S | - | - | - | - | - |
| **AI LLM Stack** | Y | S | S | S | S | S | S | S | S | S |
| **AI Voice** | Y | S | - | S | S | - | - | - | - | - |
| **Discord Social** | Y | S | - | - | - | S | S | - | - | - |
| **Quiz Provider** | Y | Y | S | Y | S | S | S | S | S | S |
| **Web3 Wallet** | S | S | Y | S | S | - | - | - | S | - |
| **NFT Integration** | S | S | Y | S | S | - | - | - | S | - |
| **Push Notifications** | Y | - | - | Y | Y | - | - | - | - | - |
| **KYC** | Y | S | Y | S | S | S | S | S | S | S |

### Coverage Summary

| Platform | Real (Y) | Stub (S) | N/A (-) | Y+S Coverage | Real Coverage |
|----------|---------|---------|---------|-------------|--------------|
| **Unity** | 91% | 6% | 3% | 97% | 91% |
| **JS/TS** | 51% | 37% | 12% | 88% | 51% |
| **Java** | 45% | 37% | 18% | 82% | 45% |
| **Flutter** | 42% | 42% | 16% | 84% | 42% |
| **Unreal** | 43% | 37% | 20% | 80% | 43% |
| **Godot** | 43% | 37% | 20% | 80% | 43% |
| **Web3** | 38% | 38% | 24% | 76% | 38% |
| **Defold** | 43% | 33% | 24% | 76% | 43% |
| **C++** | 32% | 42% | 26% | 74% | 32% |
| **Cocos** | 36% | 36% | 28% | 72% | 36% |

---

## 3. Unity-Only Features

These features currently have real implementations only in Unity:

| Feature | Why Unity-Only |
|---------|---------------|
| AI LLM Stack (full) | Requires AudioSource + WebSocket + native TTS |
| Discord Social (full) | Uses Discord Game SDK native plugin |
| Satori Analytics (full) | Deep integration with Unity lifecycle |
| Local Multiplayer | Requires same-device input splitting |
| Full Ad Mediation | Requires native ad SDK plugins |

Other platforms have stubs that return defaults or call HTTP equivalents.

---

## 4. Cross-Platform Architecture Pattern

Every SDK follows the same architectural pattern adapted to language idioms:

```
┌──────────────────────┐
│  Game Code           │  Your game logic
├──────────────────────┤
│  IVX{Feature}Manager │  Singleton manager (C# / TS class / Java class / etc.)
├──────────────────────┤
│  IVX{Feature}RpcClient│  Typed RPC wrappers calling Nakama
├──────────────────────┤
│  IVXNakamaClient     │  Platform HTTP/WebSocket client
└──────────────────────┘
```

### Language Adaptations

| Unity (C#) | JS/TS | Java | Godot (GDScript) | C++ |
|-----------|-------|------|-------------------|-----|
| `IVXEconomyManager.Instance` | `IVXEconomy.getInstance()` | `IVXEconomy.getInstance()` | `IVXEconomy.get_instance()` | `IVXEconomy::instance()` |
| `async Task<T>` | `async/await Promise<T>` | `CompletableFuture<T>` | `await signal` | `std::future<T>` |
| `[SerializeField]` | Constructor config | Builder pattern | `@export` | Config struct |
| `.asmdef` modules | npm workspace | Gradle modules | Plugin folder | CMake targets |

---

## 5. Class Mapping Table

| Unity C# Class | JS/TS | Java | Godot | C++ |
|----------------|-------|------|-------|-----|
| `IVXBootstrap` | `IVXClient` | `IVXClient` | `IVXAutoload` | `IVXClient` |
| `IVXAuthManager` | `IVXAuth` | `IVXAuth` | `IVXAuth` | `IVXAuth` |
| `IVXEconomyManager` | `IVXEconomy` | `IVXEconomy` | `IVXEconomy` | `IVXEconomy` |
| `IVXLobbyManager` | `IVXLobby` | `IVXLobby` | `IVXLobby` | `IVXLobby` |
| `IVXMatchmakingManager` | `IVXMatchmaking` | `IVXMatchmaking` | `IVXMatchmaking` | `IVXMatchmaking` |
| `IVXAdsManager` | N/A | `IVXAds` | N/A | N/A |
| `IVXQuizManager` | `IVXQuiz` | `IVXQuiz` | `IVXQuiz` | `IVXQuiz` |
| `IVXAISessionManager` | `IVXAISession` | `IVXAISession` | `IVXAISession` | `IVXAISession` |
| `IVXHiroCoordinator` | `IVXHiro` | `IVXHiro` | `IVXHiro` | `IVXHiro` |
| `IVXSatoriClient` | `IVXSatori` | `IVXSatori` | `IVXSatori` | `IVXSatori` |

---

## 6. Adding a Feature to a Non-Unity SDK

Follow this process to port a feature from the Unity reference to another platform:

### Step 1 — Identify the RPC Contract

Look at the Unity `IVX{Feature}RpcClient.cs` to find:
- RPC endpoint IDs (e.g. `hiro_economy_grant`)
- Request payload shape
- Response payload shape

### Step 2 — Create the RPC Wrapper

In the target SDK, create a typed wrapper that calls the same RPC:

```typescript
// JS/TS example
export class IVXEconomy {
  async grant(currency: string, amount: number): Promise<EconomyResponse> {
    return this.client.rpc("hiro_economy_grant", { currency, amount });
  }
}
```

### Step 3 — Create the Manager

Wrap the RPC client with convenience methods, caching, and events:

```typescript
export class IVXEconomy {
  private _balances: Map<string, number> = new Map();

  async grant(currency: string, amount: number): Promise<void> {
    const resp = await this.rpc.grant(currency, amount);
    this._balances.set(currency, resp.newBalance);
    this.emit("balanceChanged", currency, resp.newBalance);
  }

  getBalance(currency: string): number {
    return this._balances.get(currency) ?? 0;
  }
}
```

### Step 4 — Replace the Stub

Update the stub implementation to use the real manager. Mark the feature as **Y** in the coverage matrix.

### Step 5 — Test

Verify against the same Nakama server that the Unity client uses. The RPC contracts are identical.

---

## 7. SDK Directory Structure

```
SDKs/
├── javascript/       # npm / TypeScript
│   ├── src/
│   ├── package.json
│   └── tsconfig.json
├── java/             # Gradle / Android
│   ├── lib/src/main/java/com/intelliversex/
│   └── build.gradle
├── flutter/          # Dart / pub.dev
│   ├── lib/
│   └── pubspec.yaml
├── unreal/           # UE5 Plugin
│   ├── Source/
│   └── IntelliVerseX.uplugin
├── godot/            # Godot 4 Addon
│   ├── addons/intelliversex/
│   └── plugin.cfg
├── defold/           # Defold Library
│   ├── intelliversex/
│   └── game.project
├── cpp/              # CMake
│   ├── include/
│   ├── src/
│   └── CMakeLists.txt
├── cocos2dx/         # CMake
│   ├── include/
│   ├── src/
│   └── CMakeLists.txt
└── web3/             # TypeScript + ethers.js
    ├── src/
    ├── package.json
    └── tsconfig.json
```

---

## 8. Device / Runtime Platform Matrix

Beyond engines, the SDK also varies by **deployment target**. This matrix shows what works on each device class.

### Legend

| Symbol | Meaning |
|--------|---------|
| **Y** | Supported — real implementation or documented integration path |
| **P** | Partial — some features work, others have limitations |
| **A** | Adapter — works via platform adapter pattern (you implement the NDA-protected bridge) |
| **-** | Not applicable |

### Matrix

| Feature | Meta Quest | SteamVR | Vision Pro | PSVR2 | PS5 | Xbox Series | Switch | WebGL | AR (Mobile) |
|---------|-----------|---------|------------|-------|-----|-------------|--------|-------|-------------|
| **XR Detection** | Y | Y | Y | Y | - | - | - | P (WebXR) | Y |
| **Hand Tracking** | Y | P (OpenXR) | Y | - | - | - | - | - | - |
| **Eye Tracking** | Y | - | Y | Y | - | - | - | - | - |
| **Passthrough/AR** | Y | - | Y | Y | - | - | - | - | Y |
| **XR Input Adapter** | Y | Y | Y | Y | - | - | - | - | - |
| **Platform Auth** | - | - | - | - | A (PSN) | A (Xbox Live) | A (Nintendo) | - | - |
| **Platform Achievements** | - | - | - | - | A (Trophies) | A (Xbox Achievements) | - | - | - |
| **Platform Presence** | - | - | - | - | A | A | - | - | - |
| **Ads** | - | - | - | - | - | - | - | P (AdSense) | Y (Mobile) |
| **IAP** | Quest Store | Steam | App Store | - | A (PSN Store) | A (MS Store) | A (eShop) | - | Y (Mobile) |
| **Push Notifications** | - | - | - | - | P | P | - | P (Web Push) | Y |
| **Multiplayer** | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| **AI Voice** | Y | Y | Y | P | Y | Y | P | P (text only) | Y |
| **Hiro Live-Ops** | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| **Satori Analytics** | Y | Y | Y | Y | Y | Y | Y | Y | Y |

### Key Takeaways

- **Multiplayer, Hiro, and Satori** work everywhere — they're server-side via Nakama RPCs.
- **XR features** require engine-level support (Unity XR Management, Unreal XR System, Godot OpenXR).
- **Console platform services** (auth, achievements, presence, store) use the adapter pattern — the SDK provides the interface, you implement the NDA-protected bridge.
- **WebGL** has limitations on ads, audio recording, and push notifications but supports multiplayer and core game systems.
- **Monetization** varies significantly by device class — VR and console have no traditional ads.

### Relevant Documentation

| Platform | Documentation |
|----------|--------------|
| XR/VR/AR | `docs/platforms/xr-vr-ar.md` |
| Console (PS5/Xbox/Switch) | `docs/platforms/console.md` |
| WebGL | `docs/platforms/webgl.md` |
| Feature Coverage Matrix | `docs/FEATURE_COVERAGE_MATRIX.md` |

---

## Checklist

- [ ] Identified target platform and its current coverage
- [ ] Reviewed Unity reference implementation for the feature
- [ ] Extracted RPC contract (endpoint, request, response)
- [ ] Created typed RPC wrapper in target SDK
- [ ] Created manager with caching and events
- [ ] Replaced stub with real implementation
- [ ] Tested against shared Nakama server
- [ ] Updated coverage matrix in this document
- [ ] Device/runtime matrix reviewed for target deployment (VR/Console/WebGL)
- [ ] XR compile defines set if targeting VR/AR
- [ ] Console adapter implemented for platform services
