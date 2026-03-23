# IntelliVerseX C/C++ SDK

> Complete modular game development SDK for C/C++ — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- C++17 compiler (GCC 8+, Clang 7+, MSVC 2019+)
- CMake 3.14+
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) v2.8+

## Installation

### CMake (Recommended)

```cmake
# Add as subdirectory
add_subdirectory(path/to/intelliversex-cpp)
target_link_libraries(your_app PRIVATE intelliversex)

# Or via FetchContent
include(FetchContent)
FetchContent_Declare(intelliversex
    GIT_REPOSITORY https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
    SOURCE_SUBDIR SDKs/cpp
)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(your_app PRIVATE intelliversex)
```

### Manual

1. Build the Nakama C++ SDK
2. Copy `include/intelliversex/` and `src/` into your project
3. Compile and link against Nakama

## Quick Start

```cpp
#include <intelliversex/ivx.h>

int main() {
    auto& mgr = ivx::Manager::instance();

    ivx::Config cfg;
    cfg.host = "127.0.0.1";
    cfg.port = 7350;
    cfg.serverKey = "defaultkey";
    cfg.debugLogs = true;

    mgr.init(cfg);

    mgr.authDevice("", []() {
        auto& m = ivx::Manager::instance();
        printf("Logged in as: %s\n", m.username().c_str());

        m.fetchProfile([](const ivx::Profile& p) {
            printf("Display name: %s\n", p.displayName.c_str());
        });
    }, [](const ivx::Error& e) {
        fprintf(stderr, "Auth error: %s\n", e.message.c_str());
    });

    // Game loop
    while (running) {
        mgr.tick();
        // ... game logic ...
    }

    return 0;
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
| Static Library | Supported |
| Shared Library | Supported |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/cpp/).

## Nakama Client Library

This SDK wraps the official [Nakama C++ Client](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
