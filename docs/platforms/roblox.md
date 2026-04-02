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
IntelliVerseX = "intelliversex/ivx-roblox@5.9.0"
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

## Studio Plugin

The IntelliVerseX Studio Plugin provides a configuration panel:

1. Install from Creator Store or load `plugin/Plugin.server.lua`
2. Click the **IntelliVerseX** toolbar button
3. Enter your Game ID, Host, Server Key, and AI Base URL
4. Click **Test Connection** to verify
5. Click **Save Configuration**

## Troubleshooting

**"HTTP Requests are not enabled"**
Enable in Game Settings > Security > Allow HTTP Requests.

**"Auth failed: HTTP 0"**
The backend is unreachable. Check your host configuration and ensure the server is running.

**"No session for player"**
Call `IVX.authenticate(player)` or `IVX.enable_auto_auth()` before using other modules.
