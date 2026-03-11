# Defold

> IntelliVerseX Lua module for Defold, with callback-based async API.

## Requirements

- Defold 1.6+
- [Nakama Defold client](https://github.com/heroiclabs/nakama-defold) v3.5+

## Installation

1. Add Nakama dependency to `game.project`:

```ini
[project]
dependencies = https://github.com/heroiclabs/nakama-defold/archive/refs/tags/v3.5.0.zip
```

2. Copy `intelliversex/` into your project
3. Require the module:

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
        print("Logged in: " .. ivx.get_username())
        ivx.fetch_profile(function(profile) pprint(profile) end)
    end)

    if not ivx.restore_session() then
        ivx.authenticate_device()
    end
end
```

## Nakama Client

Built on [nakama-defold](https://github.com/heroiclabs/nakama-defold) (98 stars, 14 forks).

## Source

[SDKs/defold/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/defold)
