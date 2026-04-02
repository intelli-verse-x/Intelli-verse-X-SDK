# Skill: Cross-Platform Porting

**Skill ID:** `ivx-cross-platform`

Guides porting IntelliVerseX SDK features between 10 game engines. Maps Unity API classes to their equivalents on each platform, with step-by-step instructions for promoting stubs to real implementations.

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

## Supported Platforms

| # | Platform | Language | Package Format | Min Version |
|---|----------|----------|---------------|-------------|
| 1 | Unity | C# | UPM (git) | 2021.3 LTS |
| 2 | JavaScript / TypeScript | TS | npm | Node 18+ |
| 3 | Web3 | TS | npm (ethers.js) | Node 18+ |
| 4 | Java / Android | Java | Gradle / Maven | Java 11+ |
| 5 | Flutter / Dart | Dart | pub.dev | Dart 3.0+ |
| 6 | Unreal Engine 5 | C++ / BP | .uplugin | UE 5.3+ |
| 7 | Godot 4 | GDScript / C# | Addon | Godot 4.2+ |
| 8 | Defold | Lua | Library module | Defold 1.6+ |
| 9 | C++ | C++ | CMake | C++17 |
| 10 | Cocos2d-x | C++ | CMake | v4.0+ |

---

## Feature Coverage Summary

| Platform | Real (Y) | Stub (S) | N/A (-) | Real Coverage |
|----------|---------|---------|---------|--------------|
| Unity | 91% | 6% | 3% | 91% |
| JS/TS | 51% | 37% | 12% | 51% |
| Java | 45% | 37% | 18% | 45% |
| Flutter | 42% | 42% | 16% | 42% |
| Unreal | 43% | 37% | 20% | 43% |
| Godot | 43% | 37% | 20% | 43% |
| Web3 | 38% | 38% | 24% | 38% |
| Defold | 43% | 33% | 24% | 43% |
| C++ | 32% | 42% | 26% | 32% |
| Cocos | 36% | 36% | 28% | 36% |

Legend: **Y** = real implementation, **S** = stub (API compiles, returns defaults or calls HTTP RPCs), **-** = not applicable.

---

## Class Mapping

| Unity (C#) | JavaScript | Java | Godot | C++ |
|------------|-----------|------|-------|-----|
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

### Language Adaptations

| Concept | Unity (C#) | JS/TS | Java | Godot | C++ |
|---------|-----------|-------|------|-------|-----|
| Singleton | `.Instance` | `.getInstance()` | `.getInstance()` | `.get_instance()` | `::instance()` |
| Async | `async Task<T>` | `async/await Promise<T>` | `CompletableFuture<T>` | `await signal` | `std::future<T>` |
| Config | `[SerializeField]` | Constructor | Builder | `@export` | Config struct |
| Modules | `.asmdef` | npm workspace | Gradle modules | Plugin folder | CMake targets |

---

## Porting a Feature (5-Step Process)

### Step 1 -- Identify the RPC Contract

Look at the Unity `IVX{Feature}RpcClient.cs`:

- RPC endpoint ID (e.g. `hiro_economy_grant`)
- Request payload shape
- Response payload shape

### Step 2 -- Create the RPC Wrapper

```typescript
// JavaScript example
export class IVXEconomy {
  async grant(currency: string, amount: number): Promise<EconomyResponse> {
    return this.client.rpc("hiro_economy_grant", { currency, amount });
  }
}
```

### Step 3 -- Create the Manager

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

### Step 4 -- Replace the Stub

Update the existing stub file to use the real manager implementation. The API surface stays identical.

### Step 5 -- Test

Verify against the same Nakama server the Unity client uses. RPC contracts are identical across all platforms.

---

## SDK Directory Structure

```
SDKs/
├── javascript/      src/, package.json, tsconfig.json
├── java/            lib/src/main/java/com/intelliversex/, build.gradle
├── flutter/         lib/, pubspec.yaml
├── unreal/          Source/, IntelliVerseX.uplugin
├── godot/           addons/intelliversex/, plugin.cfg
├── defold/          intelliversex/, game.project
├── cpp/             include/, src/, CMakeLists.txt
├── cocos2dx/        include/, src/, CMakeLists.txt
└── web3/            src/, package.json, tsconfig.json
```

---

## Unity-Only Features

| Feature | Reason |
|---------|--------|
| AI LLM Stack (full) | Requires AudioSource + WebSocket + native TTS |
| Discord Social (full) | Uses Discord Game SDK native plugin |
| Satori Analytics (full) | Deep integration with Unity lifecycle |
| Local Multiplayer | Requires same-device input splitting |
| Full Ad Mediation | Requires native ad SDK plugins |

Other platforms have stubs that return defaults or call HTTP equivalents.

---

## Completion Checklist

- [ ] Identified target platform and its current coverage
- [ ] Reviewed Unity reference implementation for the feature
- [ ] Extracted RPC contract (endpoint, request, response)
- [ ] Created typed RPC wrapper in target SDK
- [ ] Created manager with caching and events
- [ ] Replaced stub with real implementation
- [ ] Tested against shared Nakama server
- [ ] Updated coverage matrix
