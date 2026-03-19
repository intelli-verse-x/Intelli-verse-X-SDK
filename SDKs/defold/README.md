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

## Quick Start (Auth flow matches Unity SDK)

1. **Configure** with host, port, and optional `game_id` (for backend `create_or_sync_user` RPC).
2. **Restore session** first; if none, **authenticate** (device, email, Google, or Apple).
3. On **auth_success**, identity is synced via `create_or_sync_user` when the backend provides it.

```lua
local ivx = require "intelliversex.ivx"

function init(self)
    ivx.configure({
        host = "127.0.0.1",
        port = 7350,
        server_key = "defaultkey",
        debug = true,
        game_id = "",  -- optional; required for create_or_sync_user RPC
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

    ivx.on("auth_error", function(message)
        print("Auth error: " .. message)
    end)

    ivx.on("error", function(message)
        print("Error: " .. message)
    end)

    -- Unity-style flow: restore then device auth
    if not ivx.restore_session() then
        ivx.authenticate_device()
    end
end
```

## Auth (aligned with Unity SDK)

Auth flow matches the Unity Asset Store SDK for smooth, error-free behaviour:

- **Restore first:** Call `ivx.restore_session()` at startup; if it returns `true`, skip login.
- **Then authenticate:** Device, email/password, Google token, or Apple token. All methods validate input and emit `auth_error` with clear messages.
- **Identity sync:** After Nakama auth, the SDK calls the `create_or_sync_user` RPC (same as Unity’s `IVXNakamaManager.SyncUserIdentity`) when your backend exposes it. Pass `game_id` in `configure()` for this. If the RPC is not present, auth still succeeds.
- **Session persistence:** Token and refresh token are saved; `device_id` is preserved across logout so the next device auth reuses it.

Subscribe to `auth_success` and `auth_error`; use `ivx.clear_session()` to log out.

## Features

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| create_or_sync_user (identity) | Supported (optional on server) |
| submit_score_and_sync (backend) | Supported (optional on server) |
| get_all_leaderboards (backend) | Supported (optional on server) |
| wallet_get_balances (backend) | Supported (optional on server) |
| Profile Management | Supported |
| Wallet / Economy | Hiro + optional V2 wallet RPCs |
| Leaderboards | Supported (native + backend RPCs) |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Real-time Socket | Supported |
| Hiro Systems | Via RPC |

## Backend alignment (Step 2)

When your backend uses the same RPCs as the Unity SDK, use:

- **submit_score_and_sync(score, callback)** — Same as Unity: sends device_id, game_id, user_id, score; server can return rewards and wallet update. Use instead of `submit_score(leaderboard_id, score)` when your backend expects this RPC.
- **fetch_all_leaderboards(limit, callback)** — Same as Unity `get_all_leaderboards`: returns all leaderboards in one call. Use when your backend provides this RPC.
- **fetch_wallet_balances(callback)** — Same as Unity `wallet_get_balances`: returns game_balance, global_balance. Use when your backend uses this RPC; otherwise keep using **fetch_wallet** (Hiro `hiro_economy_list`).

Existing **submit_score**, **fetch_leaderboard**, and **fetch_wallet** remain unchanged for raw Nakama / Hiro use.

## Paywall / Premium

Premium gating: call your backend entitlement RPC via **call_rpc**. If the RPC is missing, the callback gets `{ error = "..." }` — treat as non‑premium. Paywall UI is your game’s (GUI scene or store URL).

```lua
-- Example: gate premium feature by backend RPC
ivx.call_rpc("check_premium_status", "{}", function(data)
    if data.error then
        -- RPC not found or failed — show free flow
        return
    end
    if data.active then
        -- show premium content
    end
end)
```

## Running tests

- **Inside Defold (easiest):** Open this folder as a Defold project. In the **main** collection, add a game object, add a **script** component, set the script to **main/run_tests.script**. Press **Play** — results print in the Defold console.
- **From terminal:** Requires Lua, [busted](https://olivinelabs.com/busted/), and the Nakama Defold dependency on `LUA_PATH`. Then: `busted SDKs/defold/tests/test_ivx.lua`

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/defold/).

## Nakama Client Library

This SDK wraps the official [Nakama Defold Client](https://github.com/heroiclabs/nakama-defold) (98 stars, 14 forks).

## Publishing (GitHub, itch.io, developer portal)

The SDK is a Lua library; you publish the **source package** (this folder as a zip or Git dependency). Consumers then bundle their game for any Defold platform (Windows, macOS, Linux, HTML5, Android, iOS) via **Project → Bundle**. Full steps for GitHub releases, itch.io (asset or demo), and developer portal: see [PUBLISHING.md](PUBLISHING.md).

## License

MIT License — see [LICENSE](../../LICENSE)
