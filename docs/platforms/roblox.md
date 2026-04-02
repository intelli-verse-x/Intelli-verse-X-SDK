# Roblox Platform Guide

> IntelliVerseX SDK for Roblox — AI/LLM, Hiro Live-Ops, Cross-Game Identity

## Overview

The Roblox SDK is a **lightweight, focused package** that ships only the features Roblox doesn't provide natively:

- **AI / LLM Stack** — NPC dialog, voice (TTS/STT), content generation, moderation, player profiling
- **Hiro Live-Ops** — 12 engagement systems (spin wheels, streaks, daily rewards, season pass, leagues, achievements, tournaments, etc.)
- **Cross-Game Identity** — Sync player profiles, wallets, and cloud storage across Roblox AND non-Roblox games via Nakama

Features that Roblox handles natively (leaderboards, matchmaking, analytics, monetization, chat, voice) are **intentionally excluded** to avoid redundancy.

## Installation

### Option 1: Wally (Rojo workflow)

Add to your `wally.toml`:

```toml
[dependencies]
IntelliVerseX = "intelliversex/ivx-roblox@5.8.0"
```

Then run:

```bash
wally install
```

### Option 2: Direct Copy

Copy `SDKs/roblox/src/` into `ServerScriptService/IntelliVerseX` in your Roblox project.

### Option 3: Studio Plugin

Install the IntelliVerseX plugin from the Roblox Creator Store for a GUI configuration panel.

## Setup

### 1. Enable HTTP Requests

In Roblox Studio: **Game Settings > Security > Allow HTTP Requests > ON**

### 2. Get Your Game ID

Register at [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers) to get a Game ID.

### 3. Initialize (ServerScript)

```lua
local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
    game_id = "your-game-uuid",
    debug = true,
})

IVX.enable_auto_auth()
```

## Architecture

```
┌─────────────────┐     ┌───────────────────┐     ┌─────────────────┐
│  Client          │     │  Server            │     │  Backend         │
│  (LocalScript)   │     │  (ServerScript)    │     │  (Nakama + AI)   │
├─────────────────┤     ├───────────────────┤     ├─────────────────┤
│ UI / Game Logic  │────>│ IVX SDK            │────>│ Nakama REST API  │
│                  │ RE  │  - Auth            │ HTTP│ AI/LLM Endpoints │
│                  │<────│  - AI              │<────│ Hiro RPCs        │
│                  │ RE  │  - Hiro            │     │                  │
│                  │     │  - Identity        │     │                  │
└─────────────────┘     └───────────────────┘     └─────────────────┘
       RE = RemoteEvent/RemoteFunction
```

**Key constraint:** `HttpService` is server-side only in Roblox. All backend calls must go through ServerScripts. The SDK's `Remotes` module provides a clean bridge for client-server communication.

## Module Guide

### AI Module

#### NPC Dialog

```lua
local npc = {
    npc_id = "merchant_01",
    persona_id = "friendly_merchant",
    name = "Elara",
    system_prompt = "You are a friendly merchant in a fantasy world.",
}

local dialog, err = IVX.AI.NPC.start_dialog(npc, tostring(player.UserId))
local response = IVX.AI.NPC.send_message(dialog.dialog_id, "What do you sell?")
IVX.AI.NPC.end_dialog(dialog.dialog_id)
```

#### Content Moderation

```lua
local result = IVX.AI.Moderator.check_text(player_message)
if result and result.flagged then
    -- Block the message
end
```

#### Content Generation

```lua
local quest = IVX.AI.ContentGenerator.generate_text("Generate a side quest for a level 10 warrior")
local quiz = IVX.AI.ContentGenerator.generate_quiz("Ancient Egypt", 5, "medium")
```

### Hiro Live-Ops

```lua
-- Daily rewards
local status = IVX.Hiro.DailyRewards.get_status(player)
local reward = IVX.Hiro.DailyRewards.claim(player)

-- Streaks
IVX.Hiro.Streaks.update(player)
local streaks = IVX.Hiro.Streaks.get(player)

-- Season Pass
local pass = IVX.Hiro.SeasonPass.get(player)
IVX.Hiro.SeasonPass.add_xp(player, 100)

-- Leagues
IVX.Hiro.Leagues.submit_score(player, 5000)
local league = IVX.Hiro.Leagues.get(player)
```

### Cross-Game Identity

```lua
-- Profile (synced across all games)
local profile = IVX.Identity.fetch_profile(player)
IVX.Identity.update_profile(player, { display_name = "NewName" })

-- Wallet (shared currency)
local wallet = IVX.Identity.fetch_wallet(player)
IVX.Identity.grant_currency(player, "gems", 100)

-- Cloud Storage (cross-game save data)
IVX.Identity.write_storage(player, "saves", "slot1", { level = 42 })
local save = IVX.Identity.read_storage(player, "saves", "slot1")
```

## What's NOT Included (and Why)

| Feature | Roblox Native Service | Use Instead |
|---------|----------------------|-------------|
| Leaderboards | `OrderedDataStore` + `leaderstats` | Native Roblox |
| Matchmaking | `MemoryStoreService` + `TeleportService` | Native Roblox |
| Monetization | `MarketplaceService` (Robux) | Native Roblox |
| Analytics | `AnalyticsService` | Native Roblox |
| Voice Chat | `VoiceChatService` | Native Roblox |
| Text Chat | `TextChatService` | Native Roblox |
| Cloud Storage | `DataStoreService` | Native (use Identity for cross-game) |

## Complete API Reference

### Core (`IVX`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `configure` | `(opts: table) -> ()` | Initialize the SDK with game_id, host, keys |
| `on` | `(event: string, fn: function) -> ()` | Register callback: `"auth_success"`, `"auth_error"` |
| `is_initialized` | `() -> boolean` | Check if SDK is configured |
| `authenticate` | `(player: Player) -> (boolean, string?)` | Authenticate a player via Nakama |
| `enable_auto_auth` | `() -> ()` | Auto-authenticate on PlayerAdded, cleanup on PlayerRemoving |
| `call_rpc` | `(player: Player, rpc_id: string, payload: string?) -> (any?, string?)` | Call any Nakama RPC |

### AI — NPC (`IVX.AI.NPC`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `start_dialog` | `(config: NPCConfig, player_id: string) -> (any?, string?)` | Start NPC conversation |
| `send_message` | `(dialog_id: string, message: string) -> (any?, string?)` | Send player message, get response |
| `end_dialog` | `(dialog_id: string) -> (boolean, string?)` | End conversation |
| `get_history` | `(dialog_id: string) -> ({any}?, string?)` | Fetch dialog history |

**NPCConfig fields:** `npc_id` (string), `persona_id` (string), `name` (string), `system_prompt` (string?), `knowledge_base` (string?), `max_turns` (number?)

### AI — Voice (`IVX.AI.Voice`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `start_session` | `(persona_id: string, user_id: string) -> (any?, string?)` | Start TTS/STT session |
| `end_session` | `(session_id: string) -> (boolean, string?)` | End voice session |
| `send_text` | `(session_id: string, text: string) -> (any?, string?)` | Send text for synthesis |
| `poll_messages` | `(session_id: string) -> ({any}?, string?)` | Poll for new messages |

### AI — Assistant (`IVX.AI.Assistant`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `create_session` | `(persona_id, user_id, context?) -> (any?, string?)` | Start assistant session |
| `send_message` | `(session_id: string, message: string) -> (any?, string?)` | Chat with assistant |
| `end_session` | `(session_id: string) -> (boolean, string?)` | End session |
| `list_personas` | `() -> ({any}?, string?)` | List available personas |

### AI — ContentGenerator (`IVX.AI.ContentGenerator`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `generate_text` | `(prompt: string, params?: table) -> (any?, string?)` | Generate text (quests, dialog, lore) |
| `generate_structured` | `(schema: string, params?: table) -> (any?, string?)` | Generate structured game data |
| `generate_quiz` | `(topic, count?, difficulty?) -> (any?, string?)` | Generate quiz questions |

### AI — Moderator (`IVX.AI.Moderator`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `check_text` | `(text: string, context?: string) -> (ModerationResult?, string?)` | Moderate text |
| `check_username` | `(username: string) -> (ModerationResult?, string?)` | Moderate a username |
| `check_batch` | `(texts: {string}) -> ({ModerationResult}?, string?)` | Batch-moderate |

**ModerationResult:** `{ flagged: boolean, categories: table?, scores: table?, filtered_text: string? }`

### AI — Profiler (`IVX.AI.Profiler`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `track_event` | `(player_id, event, properties?) -> (boolean, string?)` | Submit behavior event |
| `get_profile` | `(player_id: string) -> (any?, string?)` | Get player profile/segments |
| `get_recommendations` | `(player_id, context?) -> (any?, string?)` | Get personalization hints |
| `predict_churn` | `(player_id: string) -> (any?, string?)` | Churn prediction score |

### Hiro Live-Ops (`IVX.Hiro`)

All Hiro methods take `(player: Player)` as the first argument and return `(any?, string?)`.

| System | Methods |
|--------|---------|
| `SpinWheel` | `get`, `spin` |
| `Streaks` | `get`, `update`, `claim(player, streak_id)` |
| `DailyRewards` | `get_status`, `claim` |
| `DailyMissions` | `list`, `complete(player, id)`, `claim(player, id)` |
| `Achievements` | `list`, `claim(player, id)`, `update_progress(player, id, progress)` |
| `SeasonPass` | `get`, `claim_tier(player, tier)`, `add_xp(player, amount)` |
| `Leagues` | `get`, `get_leaderboard(player, league_id?)`, `submit_score(player, score)` |
| `FortuneWheel` | `get`, `spin` |
| `Tournaments` | `list`, `join(player, id)`, `submit_score(player, id, score)`, `get_leaderboard(player, id)` |
| `Goals` | `get_weekly`, `get_monthly`, `update_progress(player, id, progress)`, `claim(player, id)` |
| `Retention` | `get`, `update` |
| `FriendStreaks` | `get`, `update(player, friend_id)`, `claim(player, id)` |

### Identity (`IVX.Identity`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `fetch_profile` | `(player) -> (profile?, string?)` | Cross-game profile |
| `update_profile` | `(player, fields) -> (boolean, string?)` | Update display_name, avatar_url, lang_tag |
| `fetch_wallet` | `(player) -> (any?, string?)` | Cross-game currency balances |
| `grant_currency` | `(player, currency_id, amount) -> (any?, string?)` | Grant currency |
| `read_storage` | `(player, collection, key) -> (any?, string?)` | Read cross-game data |
| `write_storage` | `(player, collection, key, value) -> (boolean, string?)` | Write cross-game data |

### Remotes (`IVX.Remotes`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `get_event` | `(name: string) -> RemoteEvent` | Get or create RemoteEvent |
| `get_function` | `(name: string) -> RemoteFunction` | Get or create RemoteFunction |
| `fire_client` | `(event, player, ...) -> ()` | Fire event to one player |
| `fire_all` | `(event, ...) -> ()` | Fire event to all players |
| `on_server_event` | `(event, callback) -> ()` | Listen for client events |
| `on_server_invoke` | `(func, callback) -> ()` | Register server-side function |

---

## Security Model

| Layer | How It Works |
|-------|-------------|
| **Transport** | All HTTP uses HTTPS (TLS 1.2+). Plaintext HTTP never used in production. |
| **Server key** | Sent as `Basic base64(key:)` in the Authorization header. Never exposed to clients. |
| **Session tokens** | Bearer tokens stored server-side only. Players never see raw tokens. |
| **Client isolation** | Clients communicate exclusively through RemoteEvents/Functions — they cannot call `HttpService`. |
| **DataStore encryption** | Session tokens cached in DataStoreService are server-accessible only (Roblox enforces this). |

**Best practices:**

- Never pass raw tokens to clients via RemoteEvents
- Always validate client input before passing to SDK methods
- Use `pcall` around SDK calls to handle unexpected errors gracefully
- Rotate your server key periodically via the developer dashboard

---

## Performance & Limits

| Resource | Roblox Limit | SDK Usage | Headroom |
|----------|-------------|-----------|----------|
| HTTP requests | 500/minute | ~1 per SDK call | High — even 100 players doing 2 calls/min = 200/min |
| DataStore reads | 60 + 10×players/min | 1 per player join | Very high |
| DataStore writes | 60 + 10×players/min | 1 per player join | Very high |
| Memory | ~4 GB server | ~1 KB/player (tokens) | Negligible |
| Module load time | N/A | ~10ms for all 34 scripts | One-time cost |
| AI response latency | N/A | 500ms–2s typical | Show typing indicator |

**Optimization tips:**

- Cache Hiro data client-side for display; only call server for mutations
- Use `on_server_invoke` (RemoteFunction) for request/response patterns
- Use `on_server_event` (RemoteEvent) for fire-and-forget patterns
- Batch multiple UI updates into a single RemoteEvent fire

---

## Studio Plugin

The IntelliVerseX Studio Plugin provides a configuration panel:

1. Install from Creator Store or load `plugin/Plugin.server.lua`
2. Click the **IntelliVerseX** toolbar button
3. Enter your Game ID, Host, Server Key, and AI Base URL
4. Click **Test Connection** to verify
5. Click **Save Configuration**

Settings persist across Studio sessions via `plugin:SetSetting()`.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `"HTTP Requests are not enabled"` | HttpService disabled | Game Settings > Security > Allow HTTP Requests > ON |
| `"Auth failed: HTTP 0"` | Backend unreachable or timeout | Check host config; verify internet in Studio |
| `"Auth failed: HTTP 401"` | Wrong server key | Verify `server_key` matches your dashboard |
| `"No session for player"` | Player not authenticated | Call `IVX.enable_auto_auth()` before other modules |
| `"RPC failed: HTTP 404"` | RPC not registered | Verify Hiro modules are configured on backend |
| `"SDK not initialized"` | `configure()` not called | Ensure `IVX.configure()` runs before `authenticate()` |
| AI returns nil | Missing API key or wrong URL | Check `ai_base_url` and `ai_api_key` in config |
| DataStore warnings | Studio has limited DataStore access | Enable Studio Access to API Services in Game Settings |

---

## Further Reading

- [Roblox Integration Guide](../guides/roblox-integration.md) — Step-by-step walkthrough with genre patterns
- [AI Getting Started](../guides/ai-getting-started.md) — Deep dive into AI capabilities
- [Hiro Integration](../guides/hiro-integration.md) — All Hiro systems explained
- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md) — Cross-platform feature comparison
