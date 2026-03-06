# IntelliVerseX Cocos2d-x SDK

> Complete modular game development SDK for Cocos2d-x — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- Cocos2d-x 4.0+
- CMake 3.10+
- C++17 compiler
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) v2.8+

## Installation

1. Install the [Nakama C++ SDK](https://heroiclabs.com/docs/nakama/client-libraries/cpp/)
2. Add IntelliVerseX to your CMake project:

```cmake
add_subdirectory(path/to/IntelliVerseX)
target_link_libraries(your_game PRIVATE intelliversex)
```

3. Include the header:

```cpp
#include "IntelliVerseX/IVXManager.h"
```

## Quick Start

```cpp
#include "IntelliVerseX/IVXManager.h"

bool GameScene::init()
{
    auto& ivx = IntelliVerseX::IVXManager::getInstance();

    IntelliVerseX::IVXConfig config;
    config.nakamaHost = "127.0.0.1";
    config.nakamaPort = 7350;
    config.nakamaServerKey = "defaultkey";
    config.enableDebugLogs = true;

    ivx.initialize(config);

    ivx.authenticateDevice("", []() {
        auto& mgr = IntelliVerseX::IVXManager::getInstance();
        printf("Logged in as: %s\n", mgr.getUsername().c_str());

        mgr.fetchProfile([](const IntelliVerseX::IVXProfile& profile) {
            printf("Display name: %s\n", profile.displayName.c_str());
        });

        mgr.fetchWallet([](const std::string& wallet) {
            printf("Wallet: %s\n", wallet.c_str());
        });
    });

    // Call in your game loop
    schedule([&ivx](float dt) { ivx.tick(); }, 0.0f, "ivx_tick");

    return true;
}
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| Profile Management | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Hiro Systems | Via RPC |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/cocos2dx/).

## Nakama Client Library

This SDK wraps the official [Nakama Cocos2d-x Client](https://github.com/niceDev0908/nakama-cocos2d-x) (29 stars, 11 forks) via the [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
