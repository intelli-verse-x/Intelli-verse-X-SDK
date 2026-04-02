# Defold

> IntelliVerseX **Lua** modules for Defold 1.6+, using the official [Nakama Defold client](https://github.com/heroiclabs/nakama-defold). All network I/O is **callback-based** (Nakama’s Lua API pattern), not coroutine-wrapped.

## Requirements

- **Defold** 1.6 or newer
- **Nakama Defold** dependency (example tag **v3.5.0** — pin the archive URL to the version you tested)
- Copied **`intelliversex/`** tree from `SDKs/defold/` into your project (or equivalent Defold library layout)

## Installation

1. Add the Nakama dependency to `game.project`:

```ini
[project]
dependencies = https://github.com/heroiclabs/nakama-defold/archive/refs/tags/v3.5.0.zip
```

2. Copy `SDKs/defold/intelliversex/` into your project so scripts resolve as `intelliversex.*`.

3. Require the core module:

```lua
local ivx = require "intelliversex.ivx"
```

4. After authentication, initialize subsystems that need the live Nakama client and session:

```lua
local ivx_hiro = require "intelliversex.ivx_hiro"
local multiplayer = require "intelliversex.multiplayer"
```

## Quick Start

```lua
local ivx = require "intelliversex.ivx"

function init(self)
    ivx.configure({
        game_id = "YOUR_GAME_UUID",
        host = "127.0.0.1",
        port = 7350,
        server_key = "defaultkey",
        use_ssl = false,
        debug = true,
    })

    ivx.on("auth_success", function(session)
        print("Logged in: " .. ivx.get_username())
        ivx.fetch_profile(function(profile) pprint(profile) end)
    end)

    ivx.on("auth_error", function(msg)
        print("Auth error: " .. tostring(msg))
    end)

    if not ivx.restore_session() then
        ivx.authenticate_device()
    end
end
```

Default `configure` targets IntelliVerseX cloud-style settings when you omit fields; always set **`game_id`** from the [developer console](https://intelli-verse-x.ai/developers) for production telemetry and RPC routing.

## API Surface

### `ivx` core (`intelliversex.ivx`)

| Function / field | Description |
|------------------|-------------|
| `SDK_VERSION` | String, e.g. `"5.8.0"`. |
| `configure(opts)` | `game_id`, `host`, `port`, `server_key`, `use_ssl`, `debug`. Creates Nakama client with `nakama.ENGINE_DEFOLD`. |
| `is_initialized()` | Returns whether `configure` ran. |
| `on(event, fn)` | Single callback slot per event name (see below). |
| `authenticate_device(id?)` | Device auth; optional explicit device id. |
| `authenticate_email(email, password, create)` | Email/password. |
| `authenticate_google(token)` | Google token. |
| `authenticate_apple(token)` | Apple token. |
| `authenticate_custom(custom_id)` | Custom id auth. |
| `restore_session()` | Restores token from save file; returns boolean. |
| `clear_session()` | Clears tokens and disconnects socket. |
| `disconnect_socket()` | Disconnects real-time socket. |
| `has_valid_session()` | Boolean session + expiry check. |
| `get_user_id()` / `get_username()` | Session accessors. |
| `fetch_profile(cb)` / `update_profile(...)` | Account profile. |
| `fetch_wallet(cb)` / `grant_currency(id, amount, cb)` | Hiro economy RPCs (`hiro_economy_list`, `hiro_economy_grant`). |
| `submit_score(id, score, cb)` / `fetch_leaderboard(id, limit, cb)` | Leaderboards. |
| `write_storage(collection, key, value_table, cb)` / `read_storage(collection, key, cb)` | User storage (JSON-encoded). |
| `call_rpc(rpc_id, payload_json, cb)` | Generic RPC; decodes JSON payload for callback. |
| `connect_socket(cb)` | Opens Nakama socket after auth. |
| `multiplayer` | Table re-export: `require "intelliversex.multiplayer"` API mounted at `ivx.multiplayer`. |

**Events** for `ivx.on`: `auth_success`, `auth_error`, `error`, `profile`, `wallet` (and internal logging). Replace callbacks by calling `on` again with a new function.

### `ivx_hiro` (`intelliversex.ivx_hiro`)

| API | Description |
|-----|-------------|
| `initialize(client, session)` | Required before any Hiro RPC. |
| `on(event, fn)` | Optional events: `spin_wheel_result`, `streak_updated`, `streak_claimed`, `offerwall_updated`, `friend_quest_updated`, `friend_battle_result`. |
| `spin_wheel_get(cb)` / `spin_wheel_spin(cb)` | Fortune wheel. |
| `streaks_get(cb)` / `streaks_update(id, cb)` / `streaks_claim(id, milestone, cb)` | Streak systems. |
| `offerwall_get(cb)` / `offerwall_complete(id, cb)` / `offerwall_claim_pending(cb)` | Offer wall. |
| `friend_quests_get(cb)` / `friend_quests_contribute(id, amount?, cb)` | Friend quests. |
| `friend_battles_challenge(friend_id, score, cb)` / `friend_battles_get(cb)` | Friend battles. |
| `M.retention.get(cb)` / `M.retention.update(cb)` | Daily-login / retention RPCs (`hiro_retention_get`, `hiro_retention_update`). |
| `M.iap_trigger.check(event_type, cb)` | IAP trigger check. |
| `M.smart_ad_timer.can_show_ad(placement, cb)` | Smart ad timer. |

Defold Hiro RPC paths use slash form (e.g. `hiro/streaks/get`) inside the module — ensure your Nakama RPC registrations match your server.

### `multiplayer` (`intelliversex.multiplayer`)

| Function | Description |
|----------|-------------|
| `initialize(client, session)` | Required. |
| `on(event, fn)` | `error`, `lobby_created`, `lobby_joined`, `lobby_left`, `lobbies_listed`, `matchmaking_started`, `matchmaking_cancelled`. |
| `create_lobby(name, max_players, is_public, cb)` | Creates lobby via RPC. |
| `join_lobby(lobby_id, cb)` / `leave_lobby(lobby_id, cb)` / `list_lobbies(cb)` | Lobby lifecycle. |
| `start_matchmaking(min_p, max_p, rank_range?, cb)` / `cancel_matchmaking(ticket_id, cb)` | Matchmaking. |

### Other bundled modules

The repo includes **AI** (`ivx_ai`, `ai_*`), **Discord** (`ivx_discord`, `discord_*`), **Satori** (`ivx_satori`), **game modes** (`ivx_game_modes`), and **deep links** (`deep_links`) mirroring other platforms. Discord and Satori are **stub** (`S`) where noted in the matrix; AI voice/host are implemented at **Y** level for init/session paths, with LLM helpers often **S**.

## Feature Coverage (Defold column)

Abbreviated from [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md).

| Area | Coverage |
|------|:--------:|
| Core init, auth, profile, wallet, leaderboards, storage, RPC | Y |
| Real-time socket | Y |
| Events / callbacks | Y |
| AI initialize / voice / host | Y |
| AI LLM stack (NPC, assistant, moderator, content gen, etc.) | S |
| Lobby / matchmaking | Y |
| Hiro (spin, streaks, offerwall, retention, friends, IAP, smart ad) | Y |
| Discord / Satori | S |
| Deep links | Y |
| Bootstrap (`configure` + submodule `initialize`) | Y |

## Callbacks vs Coroutines

Defold’s Nakama integration is **callback-driven**: every async function takes a final `function(result, err)` (or result-only) that runs on the main thread when the HTTP/WebSocket work completes.

- **Do not** assume Defold coroutines (`coroutine.wrap`) will automatically suspend inside `nakama.*` calls unless you build that wrapper yourself.
- **Order matters**: register `ivx.on("auth_success", ...)` *before* calling `authenticate_device`, or you may miss the first success if the network returns immediately from cache.
- **Single handler per event** on `ivx.on`: the table stores one function per key; for multiple listeners, forward from one dispatcher function.
- **Chaining**: nest callbacks or build a small local helper to sequence `fetch_profile` → `fetch_wallet` → `ivx_hiro.initialize`.

Submodule wiring pattern (after you have a Nakama **client** table and **session**):

```lua
ivx.on("auth_success", function(session)
    ivx_hiro.initialize(nakama_client, session)
    multiplayer.initialize(nakama_client, session)
end)
```

The stock `ivx` module creates its client inside `configure()` as a local variable and does not export `get_client()`. Options: (1) create the Nakama client once with `nakama.create_client` using the same host/port/key as `ivx.configure` and pass that reference into both `ivx` (via a small fork that accepts an external client) and `ivx_hiro` / `multiplayer`, or (2) add a one-line `M.get_client()` return in your copy of `ivx.lua` for development. See `SDKs/defold/examples/basic_example.lua` for the core auth → profile → wallet flow without Hiro.

## Advanced Examples

### Hiro “daily” retention + streaks (callbacks)

```lua
local ivx = require "intelliversex.ivx"
local ivx_hiro = require "intelliversex.ivx_hiro"

-- After configure + auth_success, and ivx_hiro.initialize(client, session):

ivx_hiro.retention.get(function(state, err)
    if err then pprint(err) return end
    pprint(state)
    ivx_hiro.retention.update(function(updated, err2)
        pprint(updated)
    end)
end)

ivx_hiro.streaks_get(function(list, err)
    if err then return end
    pprint(list)
    ivx_hiro.streaks_update("daily_login", function(st, e2)
        ivx_hiro.streaks_claim("daily_login", "7", function(final, e3)
            pprint(final)
        end)
    end)
end)
```

### Multiplayer lobby flow

```lua
local mp = require "intelliversex.multiplayer"

mp.on("lobby_created", function(data) pprint(data) end)
mp.on("error", function(msg) print(msg) end)

mp.create_lobby("Ranked Room", 4, true, function(lobby)
    mp.join_lobby(lobby.lobby_id, function(joined)
        pprint(joined)
    end)
end)

mp.list_lobbies(function(lobbies)
    pprint(lobbies)
end)
```

RPC ids such as `create_lobby`, `join_lobby` must exist on your Nakama server (same contract as JS `IVXMultiplayer`).

## Troubleshooting

### Dependency fetch failed

Defold downloads dependencies from GitHub ZIP URLs. Corporate firewalls and pinned TLS interceptors can block downloads. Mirror the Nakama zip to internal Artifactory and point `dependencies` to your mirror.

### Lua `require` path errors

Scripts must live under a path included in **custom resources** / **include dirs** for your project. The folder name must be `intelliversex` with submodules as `intelliversex/ivx.lua`, etc. Typos like `require "IntelliVerseX.ivx"` will fail on case-sensitive builds.

### HTML5 build issues

Nakama over **HTTPS/WSS** from browser targets requires CORS and correct `use_ssl`. WASM/HTML5 may restrict mixed content — do not call `http` APIs from an `https` game page. Test WebSockets in the browser devtools network tab.

### Session file location

Sessions use `sys.get_save_file("intelliversex", "session")`. Clearing app data resets auth. Desktop vs HTML5 save semantics differ; verify persistence per platform.

### Hiro RPC 404 / empty payload

If RPC ids differ between server and client (slash vs underscore), register aliases on the server or align the Lua module with your Go/Lua server RPC names.

## Nakama Client

Built on [nakama-defold](https://github.com/heroiclabs/nakama-defold). Engine identifier `nakama.ENGINE_DEFOLD` is set on the client for server-side analytics.

## Source

- [SDKs/defold/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/defold)
- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md)

## Further Reading

- [Nakama Defold documentation](https://heroiclabs.com/docs/nakama/client-libraries/defold/)
- [Defold manual — Lua](https://defold.com/manuals/lua/)
- [IntelliVerseX live-ops skill](../../.cursor/skills/ivx-live-ops/SKILL.md)
- [Cross-platform skill](../../.cursor/skills/ivx-cross-platform/SKILL.md)
