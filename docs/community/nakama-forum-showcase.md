# IntelliVerseX SDK -- 8-Platform Nakama Client SDK

> **Copy-paste this post to [forum.heroiclabs.com](https://forum.heroiclabs.com/) as a new topic in the Showcase or Community category.**

---

## Title

**IntelliVerseX SDK -- Unified Nakama Client for 8 Game Engines (Unity, Unreal, Godot, Defold, Cocos2d-x, JavaScript, C++, Java)**

---

## Post Body

Hey Nakama community!

We are excited to share **IntelliVerseX SDK** -- an open-source, multi-platform client SDK that wraps the official Nakama client libraries for **8 game engines and platforms**, giving you a unified API surface for authentication, profiles, wallets, leaderboards, storage, and RPCs across all of them.

### What is IntelliVerseX?

IntelliVerseX is a modular SDK layer that sits on top of the official Nakama client libraries. Instead of writing platform-specific Nakama integration code for each engine, you get the same `IVXManager` API everywhere:

- **Authenticate** (device, email, Google, Apple, custom)
- **Profile** management (get/update display name, avatar, metadata)
- **Wallet** (economy list, grant currency via Hiro-style RPCs)
- **Leaderboards** (submit scores, fetch records)
- **Cloud Storage** (read/write with caller-supplied collection and key)
- **Custom RPCs** (call any server RPC with JSON payload)
- **Metadata sync** (SDK version, platform, engine sent to server after auth)

### Supported Platforms

| Platform | Language | Nakama Client Library | Status |
|----------|----------|-----------------------|--------|
| **Unity** | C# | [nakama-unity](https://github.com/heroiclabs/nakama-unity) v3.13 | Stable |
| **Unreal Engine** | C++ | [nakama-unreal](https://github.com/heroiclabs/nakama-unreal) v2.8+ | Stable |
| **Godot 4** | GDScript | [nakama-godot](https://github.com/heroiclabs/nakama-godot) v3.0 | Stable |
| **Defold** | Lua | [nakama-defold](https://github.com/heroiclabs/nakama-defold) v3.5 | Stable |
| **Cocos2d-x** | C++ | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) v2.8+ | Stable |
| **JavaScript / TypeScript** | TS | [nakama-js](https://github.com/heroiclabs/nakama-js) v2.8+ | Stable |
| **C / C++** | C++ | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) v2.8+ | Stable |
| **Java / Android** | Java | [nakama-java](https://github.com/heroiclabs/nakama-java) v2.5 | Stable |

### Quick Start

**1. Run the sample Nakama server:**

```bash
git clone https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git
cd Intelli-verse-X-SDK/server
docker-compose up -d
```

This starts Nakama 3.37 + PostgreSQL with all 10 IntelliVerseX RPCs pre-registered (Go runtime).

**2. Connect any SDK:**

All SDKs default to our cloud host (`nakama-rest.intelli-verse-x.ai:443`). For local development, override the host to `127.0.0.1:7350`.

### Code Examples

**Unity (C#):**

```csharp
using IntelliVerseX.Backend;

var manager = IVXNakamaManager.Instance;
await manager.AuthenticateDeviceAsync();
var wallet = await manager.GetWalletBalance("my_game");
Debug.Log($"Balance: {wallet}");
```

**JavaScript / TypeScript:**

```typescript
import { IVXManager } from '@intelliversex/sdk';

const ivx = IVXManager.getInstance();
await ivx.init({ host: 'nakama-rest.intelli-verse-x.ai', port: 443, useSSL: true });
await ivx.authenticateDevice();
const wallet = await ivx.getWallet();
console.log('Wallet:', wallet);
```

**Godot 4 (GDScript):**

```gdscript
var ivx = preload("res://addons/intelliversex/core/ivx_manager.gd").new()
ivx.configure({ host = "nakama-rest.intelli-verse-x.ai", port = 443, use_ssl = true })
await ivx.authenticate_device()
var wallet = await ivx.get_wallet()
print("Wallet: ", wallet)
```

**Java / Android:**

```java
IVXManager ivx = IVXManager.getInstance();
ivx.init(new IVXConfig());
ivx.authenticateDevice(result -> {
    System.out.println("Authenticated: " + result.getUserId());
    ivx.getWallet(wallet -> System.out.println("Wallet: " + wallet));
});
```

### Sample Server RPCs

The included Go server implements all RPCs the SDKs expect:

| RPC | Used by | Purpose |
|-----|---------|---------|
| `ivx_sync_metadata` | All 8 SDKs | Store SDK metadata on user account |
| `hiro_economy_list` | All 8 SDKs | List wallet currencies |
| `hiro_economy_grant` | All 8 SDKs | Grant currency to wallet |
| `create_or_sync_user` | Unity | Create/sync game user identity |
| `submit_score_and_sync` | Unity | Submit score + calculate reward |
| `get_all_leaderboards` | Unity | Get daily/weekly/all-time boards |
| `get_wallet_balance` | Unity | Get coin balance |
| `update_wallet_balance` | Unity | Increment/set balance |
| `calculate_score_reward` | Unity | Preview score reward |
| `update_game_reward_config` | Unity | Persist reward config |

### Links

- **GitHub**: [github.com/intelli-verse-x/Intelli-verse-X-SDK](https://github.com/intelli-verse-x/Intelli-verse-X-SDK)
- **Latest Release (v5.2.0)**: [Releases](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases/tag/v5.2.0) -- includes per-SDK zip bundles
- **Server Project**: [server/](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/tree/main/server) -- docker-compose + Go RPCs
- **License**: MIT

### Community Plugin

IntelliVerseX is available as a **drop-in Nakama client layer** for any of the 8 supported platforms. Each SDK wraps the official Heroic Labs client library for its platform, adding:

- Managed auth flows with session persistence
- Automatic metadata sync after authentication
- Wallet management via Hiro-compatible RPCs
- Unified API for leaderboards and cloud storage

Install per platform:

| Platform | Install |
|----------|---------|
| Unity | Import `.unitypackage` from [Releases](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases) |
| Unreal | Copy `SDKs/unreal/` into your project's `Plugins/` folder |
| Godot | Copy `addons/intelliversex/` into your project |
| Defold | Add as a [library dependency](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/tree/main/SDKs/defold) |
| JavaScript | `npm install @intelliversex/sdk` |
| Java | Add JitPack dependency (see [SDKs/java/README.md](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/tree/main/SDKs/java)) |
| C++ / Cocos2d-x | CMake `find_package(intelliversex-cpp)` (see SDK READMEs) |

### Feedback Welcome

We would love to hear your feedback, suggestions, and contributions. Feel free to open issues on GitHub or reply to this thread.

Thanks to the Heroic Labs team for building Nakama -- it has been a great foundation for our multi-platform SDK!
