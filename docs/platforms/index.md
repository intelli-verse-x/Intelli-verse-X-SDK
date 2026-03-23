# Platform SDKs

IntelliVerseX provides official SDK wrappers for all major game engines and platforms. Each SDK wraps the official [Nakama client library](https://heroiclabs.com/docs/nakama/client-libraries/) for its platform, adding managed auth flows, automatic metadata sync, wallet management, and Hiro/Satori integration.

---

## Supported Platforms

| Platform | Language | Nakama Client | Status |
|----------|----------|---------------|--------|
| [Unity Engine / .NET](unity.md) | C# | [nakama-unity](https://github.com/heroiclabs/nakama-unity) | Stable |
| [Unreal Engine](unreal.md) | C++ / Blueprints | [nakama-unreal](https://github.com/heroiclabs/nakama-unreal) | Beta |
| [Godot Engine](godot.md) | GDScript | [nakama-godot](https://github.com/heroiclabs/nakama-godot) | Beta |
| [Defold](defold.md) | Lua | [nakama-defold](https://github.com/heroiclabs/nakama-defold) | Beta |
| [Cocos2d-x Engine](cocos2dx.md) | C++ | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) | Beta |
| [JavaScript](javascript.md) | TypeScript / JS | [nakama-js](https://github.com/heroiclabs/nakama-js) | Beta |
| [C / C++](cpp.md) | C++ | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) | Beta |
| [Java / Android](java.md) | Java | [nakama-java](https://github.com/heroiclabs/nakama-java) | Beta |
| [Flutter / Dart](flutter.md) | Dart | [nakama](https://pub.dev/packages/nakama) | Beta |
| [Web3 (Thirdweb / Moralis)](web3.md) | TypeScript | [nakama-js](https://github.com/heroiclabs/nakama-js) + [ethers](https://docs.ethers.org/) | Beta |

---

## Feature Matrix

| Feature | Unity | Unreal | Godot | Defold | Cocos2d-x | JS | C++ | Java | Flutter | Web3 |
|---------|:-----:|:------:|:-----:|:------:|:---------:|:--:|:---:|:----:|:-------:|:----:|
| Device Auth | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Email Auth | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | -- |
| Social Auth | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | -- |
| Wallet Auth | -- | -- | -- | -- | -- | -- | -- | -- | -- | :white_check_mark: |
| Profile | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Wallet | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Leaderboards | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Storage | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| RPC | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Real-time | :white_check_mark: | Planned | :white_check_mark: | :white_check_mark: | Planned | :white_check_mark: | Planned | Planned | Planned | Planned |
| NFT / Tokens | -- | -- | -- | -- | -- | -- | -- | -- | -- | :white_check_mark: |
| Token Gating | -- | -- | -- | -- | -- | -- | -- | -- | -- | :white_check_mark: |
| Hiro | Native | Via RPC | Via RPC | Via RPC | Via RPC | Via RPC | Via RPC | Via RPC | Via RPC | Via RPC |
| Satori | Native | Planned | Planned | Planned | Planned | Planned | Planned | Planned | Planned | Planned |
| Monetization | Native | Planned | Planned | Planned | Planned | Planned | Planned | Planned | Planned | Planned |

---

## Common Architecture

All IntelliVerseX SDKs follow the same architectural pattern:

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

### Core Components

Every platform SDK provides:

- **IVXManager** — Central coordinator singleton
- **IVXConfig** — Server configuration (host, port, SSL, debug logging)
- **Authentication** — Device, email, Google, Apple, and custom auth with persistent sessions
- **Profile** — Fetch and update user profiles
- **Wallet** — Economy integration via Hiro RPCs
- **Leaderboards** — Submit scores and fetch rankings
- **Storage** — Cloud save/load via Nakama storage
- **RPC** — Direct calls to any server-side endpoint
- **Metadata Sync** — Automatic SDK version, platform, and engine reporting

---

## Getting Started

Choose your platform and follow the Getting Started guide:

=== "Unity"

    See [Unity Getting Started](unity.md) for UPM installation and setup.

=== "Unreal"

    See [Unreal Getting Started](unreal.md) for plugin installation.

=== "Godot"

    See [Godot Getting Started](godot.md) for addon installation.

=== "Defold"

    See [Defold Getting Started](defold.md) for library module setup.

=== "Cocos2d-x"

    See [Cocos2d-x Getting Started](cocos2dx.md) for CMake integration.

=== "JavaScript"

    See [JavaScript Getting Started](javascript.md) for npm installation.

=== "C/C++"

    See [C/C++ Getting Started](cpp.md) for CMake integration.

=== "Java/Android"

    See [Java Getting Started](java.md) for Gradle setup.

=== "Flutter/Dart"

    See [Flutter Getting Started](flutter.md) for pub.dev installation.

=== "Web3"

    See [Web3 Getting Started](web3.md) for wallet auth and NFT integration.
