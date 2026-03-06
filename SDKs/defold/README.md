# IntelliVerseX Defold SDK

> Complete modular game development SDK for Defold — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

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
| Real-time Socket | Supported |
| Hiro Systems | Via RPC |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/defold/).

## Nakama Client Library

This SDK wraps the official [Nakama Defold Client](https://github.com/heroiclabs/nakama-defold) (98 stars, 14 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
