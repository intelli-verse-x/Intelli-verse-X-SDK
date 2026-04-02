# IntelliVerseX C/C++ SDK

> Complete modular game development SDK for C/C++ — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.5.0

### AI Voice & Host (`ivx::AIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```cpp
#include <intelliversex/ivx_ai_client.h>

auto& ai = ivx::AIClient::instance();
ai.initialize("https://ai.intelli-verse-x.ai", "your-key");

ai.startVoiceSession("persona-1", userId, [](const ivx::AISessionResponse& s) {
    printf("Session: %s\n", s.sessionId.c_str());
});
ai.getPersonas([](const std::vector<ivx::AIPersona>& personas) { ... });
ai.checkEntitlement(userId, [](const ivx::AIEntitlement& e) { ... });
```

### Multiplayer & Game Modes (`ivx::GameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```cpp
#include <intelliversex/ivx_game_modes.h>

auto& gm = ivx::GameModes::instance();
gm.selectMode(ivx::GameMode::OnlineVersus, 4);
gm.addPlayer("Alice", true);
gm.setPlayerReady(0, true);
if (gm.canStartMatch()) gm.startMatch();
```

### Hiro Live-Ops Systems (`ivx::HiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```cpp
#include <intelliversex/ivx_hiro_systems.h>

auto& hiro = ivx::HiroSystems::instance();
hiro.initialize(nakamaClient, session);

hiro.spinWheel("daily_wheel", [](const ivx::SpinWheelState& s) { ... });
hiro.getStreakState([](const ivx::StreakState& s) { ... });
hiro.claimStreak([](const ivx::StreakState& s) { ... });
```

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
| Device Auth | ✅ Supported |
| Email Auth | ✅ Supported |
| Google Auth | ✅ Supported |
| Apple Auth | ✅ Supported |
| Custom Auth | ✅ Supported |
| Profile Management | ✅ Supported |
| Wallet / Economy | ✅ Supported |
| Leaderboards | ✅ Supported |
| Cloud Storage | ✅ Supported |
| RPC Calls | ✅ Supported |
| AI Voice & Host | ✅ New in v5.5.0 |
| Multiplayer & Game Modes | ✅ New in v5.5.0 |
| Hiro Live-Ops Systems | ✅ New in v5.5.0 |
| Analytics | ✅ Supported |
| Static Library | ✅ Supported |
| Shared Library | ✅ Supported |

## Project Structure

```
include/intelliversex/
├── ivx.h                  # Core manager
├── ivx_manager.h          # Manager implementation
├── ivx_ai_client.h        # AI voice & host client
├── ivx_ai_types.h         # AI data types
├── ivx_game_modes.h       # Multiplayer & game mode management
└── ivx_hiro_systems.h     # Hiro live-ops typed wrappers

src/
├── ivx_ai_client.cpp      # AI client implementation
├── ivx_game_modes.cpp     # Game modes implementation
└── ivx_hiro_systems.cpp   # Hiro systems implementation
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/cpp/).

## Nakama Client Library

This SDK wraps the official [Nakama C++ Client](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
