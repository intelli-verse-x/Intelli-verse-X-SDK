# IntelliVerseX C/C++ SDK

> Complete modular game development SDK for C/C++ — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

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

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`ivx::DiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```cpp
#include <intelliversex/ivx_discord_social.h>

auto& discord = ivx::DiscordSocial::instance();
discord.initialize({"YOUR_APP_ID", "YOUR_CLIENT_ID"});

discord.updatePresence("In Match", "Round 3 of 5");
discord.getFriends([](const std::vector<ivx::Friend>& friends) { /* ... */ });
```

### Satori Analytics (`ivx::Satori`)

- Event capture, feature flags, A/B experiments, live events

```cpp
#include <intelliversex/ivx_satori.h>

auto& satori = ivx::Satori::instance();
satori.initialize({"https://satori.example.com", "your-satori-key"});

satori.captureEvents({{"level_complete", "5"}});
satori.getFeatureFlags([](const std::vector<ivx::FeatureFlag>& flags) { /* ... */ });
```

## Requirements

- C++17 compiler (GCC 8+, Clang 7+, MSVC 2019+)
- CMake 3.14+
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) v2.8+

### Platform Support

- **C++**: Windows, macOS, Linux, Android (NDK), iOS, VR (via OpenXR)

### VR / XR Support

The SDK provides **`intelliversex::IVXXRHelper`** (`include/intelliversex/ivx_xr.h`) for OpenXR-oriented VR/XR platform detection and utilities.

## Installation

### CMake (Recommended)

```cmake
# Add as subdirectory
add_subdirectory(path/to/intelliversex-cpp)
target_link_libraries(your_app PRIVATE intelliversex)

# Or via FetchContent
include(FetchContent)
FetchContent_Declare(intelliversex
    GIT_REPOSITORY https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git
    SOURCE_SUBDIR SDKs/cpp
)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(your_app PRIVATE intelliversex)
```

### Manual

1. Build the Nakama C++ SDK
2. Copy `include/intelliversex/` and `src/` into your project
3. Compile and link against Nakama

## Setting Up Nakama Server

The SDK requires a [Nakama](https://heroiclabs.com/nakama/) game server for backend features.

**Quick start with Docker:**

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

**Heroic Labs Cloud:** For production, use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

See [Nakama documentation](https://heroiclabs.com/docs/nakama/) for full setup instructions.

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
| AI Voice & Host | 🔶 Stub |
| Multiplayer & Game Modes | 🔶 Stub |
| Hiro Live-Ops Systems | 🔶 Stub |
| Analytics | ✅ Supported |
| Discord Social SDK | 🔶 Stub |
| Satori Analytics | 🔶 Stub |
| Static Library | ✅ Supported |
| Shared Library | ✅ Supported |

> 🔶 **Stub** = Full API surface exists. Methods log warnings and return empty/mock data. Zero code changes needed when backend support ships.

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

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/cpp/).

## Nakama Client Library

This SDK wraps the official [Nakama C++ Client](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Discord not connecting | Ensure application ID and client ID are valid and Discord app is approved |
| Satori events not captured | Check Satori URL and API key are correctly configured |
| Linker errors | Ensure Nakama C++ SDK is linked and `intelliversex` target is added to your `target_link_libraries` |

## License

MIT License — see [LICENSE](../../LICENSE)
