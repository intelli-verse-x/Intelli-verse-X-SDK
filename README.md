# IntelliVerseX SDK

> **Complete modular game development SDK** — Integrate Auth, Identity, Analytics, Backend (Nakama), Social/Referrals, Monetization, and more into your games across **8 platforms**.

[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-5.1.0-orange.svg)](CHANGELOG.md)
[![Documentation](https://img.shields.io/badge/Docs-Online-blue.svg)](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/)

<p align="center">
  <a href="https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/"><strong>Read the Full Documentation</strong></a>
</p>

---

## Client Libraries

IntelliVerseX provides official SDK wrappers for all major game engines and platforms, built on top of the [Nakama](https://heroiclabs.com/nakama/) open-source game server.

| Platform | Language | Getting Started | Source |
|----------|----------|----------------|--------|
| **Unity Engine / .NET** | C# | [Guide](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/getting-started/quickstart/) | [Assets/\_IntelliVerseXSDK](Assets/_IntelliVerseXSDK/) |
| **Unreal Engine** | C++ / Blueprints | [Guide](SDKs/unreal/README.md) | [SDKs/unreal](SDKs/unreal/) |
| **Godot Engine** | GDScript | [Guide](SDKs/godot/README.md) | [SDKs/godot](SDKs/godot/) |
| **Defold** | Lua | [Guide](SDKs/defold/README.md) | [SDKs/defold](SDKs/defold/) |
| **Cocos2d-x Engine** | C++ | [Guide](SDKs/cocos2dx/README.md) | [SDKs/cocos2dx](SDKs/cocos2dx/) |
| **JavaScript** | TypeScript / JS | [Guide](SDKs/javascript/README.md) | [SDKs/javascript](SDKs/javascript/) |
| **C / C++** | C++ | [Guide](SDKs/cpp/README.md) | [SDKs/cpp](SDKs/cpp/) |
| **Java / Android** | Java | [Guide](SDKs/java/README.md) | [SDKs/java](SDKs/java/) |

Each SDK wraps the official [Nakama client library](https://heroiclabs.com/docs/nakama/client-libraries/) for its platform, adding IntelliVerseX features like managed auth flows, automatic metadata sync, wallet management, and Hiro/Satori system integration.

---

## Features

| Feature | Unity | Unreal | Godot | Defold | Cocos2d-x | JS | C++ | Java |
|---------|:-----:|:------:|:-----:|:------:|:---------:|:--:|:---:|:----:|
| Device Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Email Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Google Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Apple Auth | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Profile Management | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Wallet / Economy | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Leaderboards | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Cloud Storage | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| RPC Calls | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Real-time Socket | Yes | -- | Yes | Yes | -- | Yes | -- | -- |
| Hiro Systems | Yes | RPC | RPC | RPC | RPC | RPC | RPC | RPC |
| Satori Analytics | Yes | -- | -- | -- | -- | -- | -- | -- |
| Monetization (Ads/IAP) | Yes | -- | -- | -- | -- | -- | -- | -- |
| Localization | Yes | -- | -- | -- | -- | -- | -- | -- |
| Social / Friends | Yes | -- | -- | -- | -- | -- | -- | -- |
| Quiz System | Yes | -- | -- | -- | -- | -- | -- | -- |

**Yes** = Full native support | **RPC** = Available via server RPC calls | **--** = Planned

---

## Quick Start (Unity)

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        IntelliVerseXUserIdentity.InitializeDevice();
        IVXLogger.Log("IntelliVerseX SDK Ready!");
    }
}
```

For other platforms, see the [Getting Started](#client-libraries) links above.

---

## Architecture

All IntelliVerseX SDKs share a consistent architecture:

```
Your Game
    |
    v
+----------------------------------------------+
|          IntelliVerseX SDK (IVXManager)       |
|  Auth | Profile | Wallet | Leaderboards | RPC |
+----------------------------------------------+
    |
    v
+----------------------------------------------+
|       Nakama Client Library (per-platform)    |
+----------------------------------------------+
    |
    v
+----------------------------------------------+
|         Nakama Server + Hiro + Satori        |
+----------------------------------------------+
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
|   +-- _IntelliVerseXSDK/        # Unity SDK (UPM Package)
|-- SDKs/
|   |-- unreal/                    # Unreal Engine 5 Plugin
|   |-- godot/                     # Godot 4 Addon
|   |-- defold/                    # Defold Library Module
|   |-- cocos2dx/                  # Cocos2d-x / CMake
|   |-- javascript/                # npm / TypeScript
|   |-- cpp/                       # Native C++ / CMake
|   +-- java/                      # Java / Gradle / Android
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
