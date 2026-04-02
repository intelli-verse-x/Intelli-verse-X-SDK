# IntelliVerseX Defold SDK

> Complete modular game development SDK for Defold — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

### AI Voice & Host (`ivx_ai`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```lua
local ivx_ai = require "intelliversex.ivx_ai"

ivx_ai.initialize({
    api_base_url = "https://ai.intelli-verse-x.ai",
    api_key = "your-key",
})

ivx_ai.on("voice_session_started", function(session_id)
    print("Session started: " .. session_id)
end)

ivx_ai.start_voice_session("persona-1", user_id, function(result, err)
    ivx_ai.send_text(result.session_id, "Hello!")
end)
```

### Multiplayer & Game Modes (`ivx_game_modes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```lua
local gm = require "intelliversex.ivx_game_modes"

gm.select_mode(gm.ONLINE_VERSUS, 4)
gm.add_player("Alice", true)
gm.set_player_ready(0, true)
if gm.can_start_match() then
    gm.start_match()
end
```

### Hiro Live-Ops Systems (`ivx_hiro`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```lua
local hiro = require "intelliversex.ivx_hiro"

hiro.initialize(nakama_client, session)

hiro.spin_wheel("daily_wheel", function(result, err)
    pprint(result)
end)
hiro.get_streak_state(function(state, err) ... end)
hiro.claim_streak(function(state, err) ... end)
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`ivx_discord`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```lua
local discord = require("intelliversex.ivx_discord")

discord.initialize({
    application_id = "YOUR_APP_ID",
    client_id = "YOUR_CLIENT_ID",
})

discord.update_presence("In Match", "Round 3 of 5")
discord.get_friends(function(friends, err)
    pprint(friends)
end)
```

### Satori Analytics (`ivx_satori`)

- Event capture, feature flags, A/B experiments, live events

```lua
local satori = require("intelliversex.ivx_satori")

satori.initialize({
    satori_url = "https://satori.example.com",
    api_key = "your-satori-key",
})

satori.capture_events({{name = "level_complete", value = "5"}}, function(ok, err)
    print("Events captured: " .. tostring(ok))
end)
local flags = satori.get_feature_flags()
```

## Requirements

- Defold 1.6+
- [Nakama Defold client](https://github.com/heroiclabs/nakama-defold) v3.5+

## Installation

1. Add the Nakama Defold dependency to your `game.project`:

```ini
[project]
dependencies = https://github.com/heroiclabs/nakama-defold/archive/refs/tags/v3.5.0.zip
```

2. Copy the `intelliversex/` folder into your project

3. Require the module in your scripts:

```lua
local ivx = require "intelliversex.ivx"
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

```lua
local ivx = require "intelliversex.ivx"

function init(self)
    ivx.configure({
        host = "127.0.0.1",
        port = 7350,
        server_key = "defaultkey",
        debug = true,
    })

    ivx.on("auth_success", function(session)
        print("Logged in as: " .. ivx.get_username())

        ivx.fetch_profile(function(profile)
            pprint(profile)
        end)

        ivx.fetch_wallet(function(wallet)
            pprint(wallet)
        end)
    end)

    ivx.on("error", function(message)
        print("Error: " .. message)
    end)

    if not ivx.restore_session() then
        ivx.authenticate_device()
    end
end
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
| Real-time Socket | ✅ Supported |
| AI Voice & Host | ✅ New in v5.8.0 |
| Multiplayer & Game Modes | ✅ New in v5.8.0 |
| Hiro Live-Ops Systems | ✅ New in v5.8.0 |
| Analytics | ✅ Supported |
| Discord Social SDK | ✅ New in v5.8.0 |
| Satori Analytics | ✅ New in v5.8.0 |

## Project Structure

```
intelliversex/
├── ivx.lua              # Core module
├── ivx_ai.lua           # AI voice & host client
├── ivx_game_modes.lua   # Multiplayer & game mode management
└── ivx_hiro.lua         # Hiro live-ops typed wrappers
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/defold/).

## Nakama Client Library

This SDK wraps the official [Nakama Defold Client](https://github.com/heroiclabs/nakama-defold) (98 stars, 14 forks).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Discord not connecting | Ensure `application_id` and `client_id` are valid and Discord app is approved |
| Satori events not captured | Check `satori_url` and `api_key` are correctly configured |

## License

MIT License — see [LICENSE](../../LICENSE)
