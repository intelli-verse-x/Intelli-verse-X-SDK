# Roblox SDK

**Skill ID:** `ivx-roblox`

---
name: ivx-roblox
description: IntelliVerseX Roblox SDK integration — AI NPCs, Hiro Live-Ops, and Cross-Game Identity for Roblox experiences
version: 1.0.0
author: IntelliVerseX
allowed-tools:
  - Read
  - Edit
  - Shell
  - Grep
  - Glob
---



Integrate AI-powered NPCs, Hiro Live-Ops engagement systems, and cross-game player identity into Roblox experiences.

## When to Use

- Adding AI NPC conversations to a Roblox experience
- Implementing daily rewards, streaks, season passes, or other live-ops features
- Syncing player profiles, wallets, or save data across multiple Roblox experiences or cross-platform games

## SDK Location

- Source: `SDKs/roblox/src/`
- Plugin: `SDKs/roblox/plugin/Plugin.server.lua`
- Examples: `SDKs/roblox/examples/`
- Docs: `docs/platforms/roblox.md`

## Quick Setup

```lua
-- ServerScript in ServerScriptService
local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
    game_id = "YOUR_GAME_ID",
    debug = true,
})

IVX.enable_auto_auth()
```

**Prerequisite:** Enable HTTP Requests in Game Settings > Security.

## Module Overview

### AI Module (`IVX.AI`)

| Sub-module | Purpose |
|-----------|---------|
| `IVX.AI.NPC` | LLM-powered NPC dialog (start, send message, end, history) |
| `IVX.AI.Voice` | Text-to-Speech / Speech-to-Text sessions |
| `IVX.AI.Assistant` | Conversational AI assistant sessions |
| `IVX.AI.ContentGenerator` | Procedural text, structured data, quiz generation |
| `IVX.AI.Moderator` | Real-time text moderation (single + batch) |
| `IVX.AI.Profiler` | Player behavior profiling, churn prediction |

### Hiro Live-Ops (`IVX.Hiro`)

12 engagement systems, all backed by Nakama Hiro RPCs:

`SpinWheel`, `Streaks`, `DailyRewards`, `DailyMissions`, `Achievements`, `SeasonPass`, `Leagues`, `FortuneWheel`, `Tournaments`, `Goals`, `Retention`, `FriendStreaks`

Each system follows the pattern: `IVX.Hiro.<System>.get(player)` / `.claim(player)` / etc.

### Cross-Game Identity (`IVX.Identity`)

- `fetch_profile(player)` / `update_profile(player, fields)`
- `fetch_wallet(player)` / `grant_currency(player, id, amount)`
- `read_storage(player, collection, key)` / `write_storage(player, collection, key, value)`

## Roblox-Specific Notes

- **All HTTP calls are server-side only.** Use `IVX.Remotes` to bridge data to clients via RemoteEvents/RemoteFunctions.
- **Auth is automatic.** `IVX.enable_auto_auth()` maps each player's Roblox UserId to a Nakama identity on join.
- **No monetization module.** Roblox handles Robux via `MarketplaceService`. Use it directly for IAP.
- **No leaderboard/matchmaking module.** Use Roblox's native `OrderedDataStore` and `MemoryStoreService`.

## Common Patterns

### AI NPC with RemoteEvent bridge

```lua
-- Server
IVX.Remotes.on_server_event("TalkToNPC", function(player, msg)
    local resp = IVX.AI.NPC.send_message(dialog_id, msg)
    IVX.Remotes.fire_client("NPCReply", player, resp)
end)
```

### Daily Rewards with RemoteFunction

```lua
-- Server
IVX.Remotes.on_server_invoke("ClaimDaily", function(player)
    return IVX.Hiro.DailyRewards.claim(player)
end)
```
