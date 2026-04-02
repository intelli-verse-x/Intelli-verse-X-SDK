# IntelliVerseX SDK

> **Complete modular game development SDK** — Integrate Auth, Identity, Analytics, Backend (Nakama), Social/Referrals, Monetization, and more into your games across **10 platforms**.

[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-5.8.0-orange.svg)](CHANGELOG.md)
[![Documentation](https://img.shields.io/badge/Docs-Online-blue.svg)](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/)
[![openupm](https://img.shields.io/npm/v/com.intelliversex.sdk?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.intelliversex.sdk)

<p align="center">
  <a href="https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/"><strong>Read the Full Documentation</strong></a>
</p>

---

## Client Libraries

IntelliVerseX provides official SDK wrappers for all major game engines and platforms, built on top of the [Nakama](https://heroiclabs.com/nakama/) open-source game server.

| Platform | Language | Getting Started | Source |
|----------|----------|----------------|--------|
| **Unity Engine / .NET** | C# | [Guide](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/getting-started/quickstart/) | [Assets/Intelli-verse-X-SDK](Assets/Intelli-verse-X-SDK/) |
| **Unreal Engine** | C++ / Blueprints | [Guide](SDKs/unreal/README.md) | [SDKs/unreal](SDKs/unreal/) |
| **Godot Engine** | GDScript | [Guide](SDKs/godot/README.md) | [SDKs/godot](SDKs/godot/) |
| **Defold** | Lua | [Guide](SDKs/defold/README.md) | [SDKs/defold](SDKs/defold/) |
| **Cocos2d-x Engine** | C++ | [Guide](SDKs/cocos2dx/README.md) | [SDKs/cocos2dx](SDKs/cocos2dx/) |
| **Roblox** | Luau | [Guide](SDKs/roblox/README.md) | [SDKs/roblox](SDKs/roblox/) |
| **JavaScript** | TypeScript / JS | [Guide](SDKs/javascript/README.md) | [SDKs/javascript](SDKs/javascript/) |
| **C / C++** | C++ | [Guide](SDKs/cpp/README.md) | [SDKs/cpp](SDKs/cpp/) |
| **Java / Android** | Java | [Guide](SDKs/java/README.md) | [SDKs/java](SDKs/java/) |
| **Flutter / Dart** | Dart | [Guide](SDKs/flutter/README.md) | [SDKs/flutter](SDKs/flutter/) |
| **Web3** | TypeScript | [Guide](SDKs/web3/README.md) | [SDKs/web3](SDKs/web3/) |

Each SDK wraps the official [Nakama client library](https://heroiclabs.com/docs/nakama/client-libraries/) for its platform, adding IntelliVerseX features like managed auth flows, automatic metadata sync, wallet management, and Hiro/Satori system integration.

---

## Features

| Feature | Unity | Unreal | Godot | Defold | Cocos2d-x | JS | C++ | Java | Flutter | Web3 |
|---------|:-----:|:------:|:-----:|:------:|:---------:|:--:|:---:|:----:|:-------:|:----:|
| Device Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Email Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | -- |
| Google Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | -- |
| Apple Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | -- |
| Wallet Auth | -- | -- | -- | -- | -- | -- | -- | -- | -- | Yes |
| Profile Management | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Wallet / Economy | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Leaderboards | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Cloud Storage | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| RPC Calls | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Real-time Socket | Yes | -- | Yes | Yes | -- | Yes | -- | -- | -- | -- |
| Hiro Systems | Yes | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC |
| NFT / Token Queries | -- | -- | -- | -- | -- | -- | -- | -- | -- | Yes |
| Token Gating | -- | -- | -- | -- | -- | -- | -- | -- | -- | Yes |
| Discord Social SDK | Yes | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Satori Analytics | Yes | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Voice / Host | Yes | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI LLM Stack (6 modules) | Yes | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Monetization (Ads/IAP) | Yes | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| XR Platform Detection | Yes | Yes | Yes | -- | -- | WebXR | Yes | -- | -- | -- |
| Console Adapters (PS5/Xbox/Switch) | Yes | Yes | -- | -- | -- | -- | -- | -- | -- | -- |
| Localization | Yes | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| Social / Friends | Yes | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| Quiz System | Yes | -- | -- | -- | -- | -- | -- | -- | -- | -- |
| Retention / Streaks | Yes | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC |
| Spin Wheel / Engagement | Yes | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC |
| Platform Optimizer | Yes | -- | -- | -- | -- | -- | -- | -- | -- | -- |

**Yes** = Full native support | **RPC** = Available via server RPC calls | **--** = Planned

### Deployment Targets

Beyond the 10 engine SDKs, the following device/platform targets are supported:

| Target | Engines | Key Features |
|--------|---------|-------------|
| **Meta Quest (VR)** | Unity, Unreal, Godot, C++ | Hand/eye tracking, passthrough, XR input adapter |
| **SteamVR / OpenXR** | Unity, Unreal, Godot, C++ | Generic OpenXR, controller + hand input |
| **Apple Vision Pro** | Unity | PolySpatial, gaze input, spatial UI |
| **PSVR2** | Unity, Unreal | Eye tracking, adaptive triggers, passthrough |
| **PS5 / Xbox Series / Switch** | Unity, Unreal | Console adapter pattern (NDA SDKs), platform auth, achievements, presence |
| **WebGL / Browser** | Unity, JS/TS | WebXR, browser ads (AdSense), IndexedDB cache |
| **AR (ARKit / ARCore)** | Unity, Unreal | Plane detection, image tracking, light estimation |

📖 [XR/VR/AR Guide](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/xr-vr-ar/) | [Console Guide](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/console/) | [WebGL Guide](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/webgl/)

---

## Quick Start (Unity)

### 1. Install

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK#v5.8.0"
  }
}
```

### 2. One-Drop Setup

1. Run **IntelliVerseX > Generate All Prefabs** from the menu bar
2. Drag `IVX_Bootstrap.prefab` into your first scene
3. Configure the Bootstrap Config asset with your server details

### 3. Listen for Ready

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        IVXBootstrap.OnBootstrapComplete += success =>
        {
            Debug.Log($"IntelliVerseX SDK Ready! Auth: {success}");
            Debug.Log($"User: {IVXBootstrap.Instance.UserId}");
        };
    }
}
```

> **No server yet?** The SDK works in offline mode with mock data — press Play and explore the 16 built-in demo UIs through the Demo Hub.

### 4. Setting Up Nakama (backend)

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

Or use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

For other platforms, see the [Getting Started](#client-libraries) links above.

---

## Architecture

All IntelliVerseX SDKs share a consistent architecture:

```
Your Game
    |
    v
+-----------------------------------------------------------+
|              IntelliVerseX SDK (IVXManager)                |
|  Auth | Wallet | Social | Leaderboards | Quiz | Ads | IAP |
+-----------------------------------------------------------+
    |                           |
    v                           v
+---------------------------+  +----------------------------+
| Nakama Client (per-plat)  |  |  IntelliVerseX AI Backend  |
+---------------------------+  |  Voice | Host | Entitlement |
    |                          +----------------------------+
    v
+-----------------------------------------------------------+
|   Nakama Server + Hiro (Economy, Streaks, Spin Wheel,     |
|   Offerwalls, Retention) + Satori (Analytics, A/B Tests)  |
+-----------------------------------------------------------+
```

Each platform SDK provides:
- **IVXManager** — Central coordinator (singleton/subsystem pattern)
- **IVXConfig** — Server configuration (host, port, SSL, debug)
- **Auth** — Device, email, Google, Apple, custom authentication with session persistence
- **Profile** — Fetch and update user profiles
- **Wallet** — Economy integration via Hiro RPCs
- **Leaderboards** — Submit scores and fetch rankings
- **Storage** — Cloud save/load via Nakama storage
- **RPC** — Direct calls to any server-side RPC endpoint
- **Metadata Sync** — Automatic SDK version, platform, and engine reporting

---

## Repository Structure

```
Intelli-verse-X-Unity-SDK/
|-- Assets/
|   |-- Intelli-verse-X-SDK/      # Unity SDK (UPM Package)
|   +-- _IntelliVerseXSDK/        # AI, Hiro, Satori, Platform, Demos
|-- SDKs/
|   |-- unreal/                    # Unreal Engine 5 Plugin
|   |-- godot/                     # Godot 4 Addon
|   |-- defold/                    # Defold Library Module
|   |-- cocos2dx/                  # Cocos2d-x / CMake
|   |-- roblox/                    # Roblox / Luau (Wally)
|   |-- javascript/                # npm / TypeScript
|   |-- cpp/                       # Native C++ / CMake
|   |-- java/                      # Java / Gradle / Android
|   |-- flutter/                   # Flutter / Dart (pub.dev)
|   +-- web3/                      # Web3 / TypeScript (ethers.js)
|-- docs/                          # MkDocs documentation
|-- .github/workflows/             # CI/CD
|-- tools/                         # Dev utilities
+-- README.md                      # This file
```

---

## Underlying Nakama Client Libraries

Each SDK is built on top of the official Heroic Labs Nakama client:

| Platform | Nakama Client | Stars | Repository |
|----------|---------------|-------|------------|
| Unity / .NET | nakama-unity | 468 | [heroiclabs/nakama-unity](https://github.com/heroiclabs/nakama-unity) |
| Unreal Engine | nakama-unreal | 249 | [heroiclabs/nakama-unreal](https://github.com/heroiclabs/nakama-unreal) |
| Godot Engine | nakama-godot | 737 | [heroiclabs/nakama-godot](https://github.com/heroiclabs/nakama-godot) |
| Defold | nakama-defold | 98 | [heroiclabs/nakama-defold](https://github.com/heroiclabs/nakama-defold) |
| Cocos2d-x | nakama-cocos2d-x | 29 | [heroiclabs/nakama-cocos2d-x](https://github.com/niceDev0908/nakama-cocos2d-x) |
| JavaScript | nakama-js | 218 | [heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) |
| C / C++ | nakama-cpp | 87 | [heroiclabs/nakama-cpp](https://github.com/heroiclabs/nakama-cpp) |
| Java / Android | nakama-java | 37 | [heroiclabs/nakama-java](https://github.com/heroiclabs/nakama-java) |
| Flutter / Dart | nakama (pub.dev) | 148 | [heroiclabs/nakama-dart](https://github.com/heroiclabs/nakama-dart) |
| Web3 | nakama-js + ethers | 218 / 7.9k | [heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) + [ethers-io/ethers.js](https://github.com/ethers-io/ethers.js) |

---

## Documentation

**[Full Documentation Site](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/)**

Quick links:
- [Getting Started](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/getting-started/quickstart/)
- [Platform SDKs](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/)
- [API Reference](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/api/core/)
- [Troubleshooting](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/troubleshooting/faq/)
- [Changelog](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/changelog/)

---

## AI Agent Skills

Automate your SDK integration with 7 purpose-built AI agent skills. Works with Cursor, Windsurf, Claude Code, Devin, OpenAI Codex, and any agent that reads `SKILL.md` files.

| Skill | What It Does | Trigger Phrases |
|-------|-------------|----------------|
| **ivx-sdk-setup** | Install and bootstrap the SDK on any platform | "Set up IntelliVerseX" |
| **ivx-monetization** | Wire ads, IAP, offerwalls, and revenue strategy | "Monetize my game" |
| **ivx-multiplayer** | Add lobbies, matchmaking, real-time networking | "Add multiplayer" |
| **ivx-ai-integration** | Integrate AI voice, NPC dialog, content gen | "Add AI host" |
| **ivx-live-ops** | Set up Hiro + Satori (33+ systems) | "Add daily rewards" |
| **ivx-quiz-content** | Build quiz pipelines with S3 + LLM | "Set up daily quiz" |
| **ivx-cross-platform** | Port features between 10 engines | "Port to Godot" |

### Install

**Cursor / Windsurf:** Clone the repo — skills auto-activate from `.cursor/skills/`.

**Claude Code:**
```bash
/plugin marketplace add https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK
```

**SkillsGate (18+ AI agents):**
```bash
skillsgate add @intelliversex/ivx-sdk-setup @intelliversex/ivx-monetization @intelliversex/ivx-multiplayer @intelliversex/ivx-ai-integration @intelliversex/ivx-live-ops @intelliversex/ivx-quiz-content @intelliversex/ivx-cross-platform
```

📖 [Full Skills Documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/guides/ai-agent-skills/)

---

## MCP Server (Nakama + Hiro + Satori)

The IntelliVerseX MCP server exposes 50+ tools for managing your game backend directly from AI coding agents.

**Capabilities:**
- **Nakama:** Health check, RPC calls, build/deploy, restart, auth, account management
- **Hiro:** Config get/set for economy, achievements, energy, streaks, store, challenges, tutorials, unlockables
- **Satori:** Config get/set, feature flags, A/B experiments, live events, messages
- **Player Ops:** Inspect, search, wallet view/grant/reset, inventory grant, mailbox
- **Analytics:** Events timeline, metrics, alerts, webhooks, data lake
- **Infra:** Storage CRUD, config import/export, cache invalidation, taxonomy management

### Connect

**Cursor / Windsurf (MCP settings):**
```json
{
  "mcpServers": {
    "intelliversex": {
      "url": "https://mcp.intelli-verse-x.ai/api/mcp"
    }
  }
}
```

> **Self-hosted?** If running Nakama locally, use the stdio transport instead:
> ```json
> {
>   "mcpServers": {
>     "intelliversex": {
>       "command": "npx",
>       "args": ["@intelliversex/mcp-server"],
>       "env": { "NAKAMA_HOST": "127.0.0.1", "NAKAMA_PORT": "7350", "NAKAMA_SERVER_KEY": "defaultkey" }
>     }
>   }
> }
> ```

**Smithery:**
```bash
npx smithery install @intelliversex/game-sdk
```

📖 [MCP Server Documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/modules/backend/)

---

## Contributing

We welcome contributions for all platforms. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## License

MIT License - see [LICENSE](LICENSE)

---

## Support

- [Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues)
- [Discussions](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/discussions)
- [Discord](https://discord.gg/intelliversex)
- Email: support@intelli-verse-x.ai

---

<p align="center">Made with care by <a href="https://intelliversex.com">IntelliVerse-X</a></p>
