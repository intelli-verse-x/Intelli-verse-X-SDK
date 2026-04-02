# IntelliVerseX Cocos2d-x SDK

> Complete modular game development SDK for Cocos2d-x — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```cpp
#include "IntelliVerseX/IVXAIClient.h"

auto& ai = IntelliVerseX::IVXAIClient::getInstance();
ai.initialize("https://ai.intelli-verse-x.ai", "your-key");

ai.startVoiceSession("persona-1", userId, [](const auto& session) {
    printf("Session: %s\n", session.sessionId.c_str());
});
ai.getPersonas([](const auto& personas) { ... });
```

### Multiplayer & Game Modes (`IVXGameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```cpp
#include "IntelliVerseX/IVXGameModes.h"

auto& gm = IntelliVerseX::IVXGameModes::getInstance();
gm.selectMode(IntelliVerseX::GameMode::OnlineVersus, 4);
gm.addPlayer("Alice", true);
gm.setPlayerReady(0, true);
if (gm.canStartMatch()) gm.startMatch();
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```cpp
#include "IntelliVerseX/IVXHiroSystems.h"

auto& hiro = IntelliVerseX::IVXHiroSystems::getInstance();
hiro.initialize(nakamaClient, session);

hiro.spinWheel("daily_wheel", [](const auto& result) { ... });
hiro.getStreakState([](const auto& state) { ... });
hiro.claimStreak([](const auto& state) { ... });
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`IVXDiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```cpp
#include "IntelliVerseX/IVXDiscordSocial.h"

auto& discord = IntelliVerseX::IVXDiscordSocial::getInstance();
discord.initialize({"YOUR_APP_ID", "YOUR_CLIENT_ID"});

discord.updatePresence("In Match", "Round 3 of 5");
discord.getFriends([](const auto& friends) { /* ... */ });
```

### Satori Analytics (`IVXSatori`)

- Event capture, feature flags, A/B experiments, live events

```cpp
#include "IntelliVerseX/IVXSatori.h"

auto& satori = IntelliVerseX::IVXSatori::getInstance();
satori.initialize({"https://satori.example.com", "your-satori-key"});

satori.captureEvents({{"level_complete", "5"}});
satori.getFeatureFlags([](const auto& flags) { /* ... */ });
```

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
| AI Voice & Host | ✅ New in v5.8.0 |
| Multiplayer & Game Modes | ✅ New in v5.8.0 |
| Hiro Live-Ops Systems | ✅ New in v5.8.0 |
| Analytics | ✅ Supported |
| Discord Social SDK | ✅ New in v5.8.0 |
| Satori Analytics | ✅ New in v5.8.0 |

## Project Structure

```
Classes/IntelliVerseX/
├── IVXManager.h            # Core manager
├── IVXAIClient.h / .cpp    # AI voice & host client
├── IVXGameModes.h / .cpp   # Multiplayer & game mode management
└── IVXHiroSystems.h / .cpp # Hiro live-ops typed wrappers
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/cocos2dx/).

## Nakama Client Library

This SDK wraps the official [Nakama Cocos2d-x Client](https://github.com/niceDev0908/nakama-cocos2d-x) (29 stars, 11 forks) via the [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Discord not connecting | Ensure application ID and client ID are valid and Discord app is approved |
| Satori events not captured | Check Satori URL and API key are correctly configured |
| Linker errors | Ensure Nakama C++ SDK is linked and `intelliversex` target is added to your CMake project |

## License

MIT License — see [LICENSE](../../LICENSE)
