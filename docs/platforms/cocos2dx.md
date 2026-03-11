# Cocos2d-x Engine

> IntelliVerseX C++ library for Cocos2d-x, integrated via CMake.

## Requirements

- Cocos2d-x 4.0+
- CMake 3.10+
- C++17 compiler
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) v2.8+

## Installation

1. Install the [Nakama C++ SDK](https://heroiclabs.com/docs/nakama/client-libraries/cpp/)
2. Add to your CMake project:

```cmake
add_subdirectory(path/to/IntelliVerseX)
target_link_libraries(your_game PRIVATE intelliversex)
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
        printf("Logged in!\n");
    });

    schedule([&ivx](float dt) { ivx.tick(); }, 0.0f, "ivx_tick");
    return true;
}
```

## Nakama Client

Built on [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks) / [nakama-cocos2d-x](https://github.com/niceDev0908/nakama-cocos2d-x) (29 stars, 11 forks).

## Source

[SDKs/cocos2dx/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/cocos2dx)
