# IntelliVerseX Defold SDK

> Complete modular game development SDK for Defold — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.5.0

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
| AI Voice & Host | ✅ New in v5.5.0 |
| Multiplayer & Game Modes | ✅ New in v5.5.0 |
| Hiro Live-Ops Systems | ✅ New in v5.5.0 |
| Analytics | ✅ Supported |

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

## License

MIT License — see [LICENSE](../../LICENSE)
