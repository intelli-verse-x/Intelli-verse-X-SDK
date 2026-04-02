# IntelliVerseX SDK for Roblox

> AI/LLM Stack + Hiro Live-Ops + Cross-Game Identity for Roblox experiences.

**Version:** 5.8.0 | **Language:** Luau | **Runtime:** Roblox Server + Studio Plugin

## Why This SDK?

Roblox natively handles auth, leaderboards, storage, matchmaking, and monetization. This SDK ships **only what Roblox doesn't have**:

| Module | What It Does |
|--------|-------------|
| **AI / LLM** | NPC dialog, voice (TTS/STT), content generation, moderation, player profiling |
| **Hiro Live-Ops** | Spin wheels, streaks, daily rewards, season pass, leagues, achievements, tournaments |
| **Cross-Game Identity** | Sync profiles, wallets, and progress across Roblox AND non-Roblox games via Nakama |

## Quick Start

### 1. Install

**Option A: Wally (recommended for Rojo users)**
```bash
# wally.toml
[dependencies]
IntelliVerseX = "intelliversex/ivx-roblox@5.8.0"
```

**Option B: Copy into your project**
Copy the `src/` folder into `ServerScriptService/IntelliVerseX`.

**Option C: Studio Plugin**
Install from the Roblox Creator Store for the configuration panel.

### 2. Configure (ServerScript)

```lua
local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
    game_id = "your-game-uuid",
    -- host = "nakama-rest.intelli-verse-x.ai",  -- default
    -- ai_base_url = "https://ai.intelli-verse-x.ai",  -- default
    debug = true,
})

-- Auto-authenticate players on join
IVX.enable_auto_auth()
```

### 3. Use AI NPCs

```lua
local npc_config = {
    npc_id = "guard_01",
    persona_id = "stern_guard",
    name = "Captain Thorne",
    system_prompt = "You are a stern but fair castle guard.",
}

IVX.Remotes.on_server_event("TalkToNPC", function(player, message)
    local dialog, err = IVX.AI.NPC.start_dialog(npc_config, tostring(player.UserId))
    if dialog then
        local response = IVX.AI.NPC.send_message(dialog.dialog_id, message)
        IVX.Remotes.fire_client("NPCReply", player, response)
    end
end)
```

### 4. Daily Rewards + Streaks

```lua
IVX.Remotes.on_server_invoke("ClaimDaily", function(player)
    local reward, err = IVX.Hiro.DailyRewards.claim(player)
    IVX.Hiro.Streaks.update(player)
    return reward
end)
```

### 5. Cross-Game Profile Sync

```lua
-- Read profile synced across all your games
local profile = IVX.Identity.fetch_profile(player)

-- Save progress accessible from any game
IVX.Identity.write_storage(player, "progress", "slot1", { level = 42 })
```

## Architecture

All HTTP calls go through **ServerScripts** (Roblox requirement). The `Remotes` module bridges server-to-client communication.

```
Client (LocalScript)          Server (ServerScript)           Backend
    |                              |                              |
    |-- RemoteEvent/Function ----->|                              |
    |                              |-- HttpService:RequestAsync ->|
    |                              |<--- JSON Response -----------|
    |<---- RemoteEvent ------------|                              |
```

## API Reference

### Core
- `IVX.configure(opts)` — Initialize the SDK
- `IVX.authenticate(player)` — Auth a player via Nakama
- `IVX.enable_auto_auth()` — Auto-auth on PlayerAdded
- `IVX.call_rpc(player, rpc_id, payload?)` — Generic Nakama RPC

### AI (`IVX.AI`)
- `IVX.AI.NPC.start_dialog(config, player_id)` — Start NPC conversation
- `IVX.AI.NPC.send_message(dialog_id, text)` — Send player message
- `IVX.AI.Voice.start_session(persona_id, user_id)` — TTS/STT session
- `IVX.AI.Assistant.create_session(persona, user, context?)` — AI assistant
- `IVX.AI.ContentGenerator.generate_text(prompt, params?)` — Generate content
- `IVX.AI.Moderator.check_text(text, context?)` — Content moderation
- `IVX.AI.Profiler.get_profile(player_id)` — Player behavior profile

### Hiro Live-Ops (`IVX.Hiro`)
- `IVX.Hiro.SpinWheel.spin(player)` — Spin the wheel
- `IVX.Hiro.Streaks.get/update/claim(player)` — Streaks
- `IVX.Hiro.DailyRewards.get_status/claim(player)` — Daily rewards
- `IVX.Hiro.DailyMissions.list/complete/claim(player)` — Missions
- `IVX.Hiro.Achievements.list/claim/update_progress(player)` — Achievements
- `IVX.Hiro.SeasonPass.get/claim_tier/add_xp(player)` — Season pass
- `IVX.Hiro.Leagues.get/submit_score(player)` — 6-tier leagues
- `IVX.Hiro.Tournaments.list/join/submit_score(player)` — Tournaments
- `IVX.Hiro.Goals.get_weekly/get_monthly/claim(player)` — Goals
- `IVX.Hiro.Retention.get/update(player)` — Retention tracking
- `IVX.Hiro.FriendStreaks.get/update/claim(player)` — Friend streaks
- `IVX.Hiro.FortuneWheel.get/spin(player)` — Fortune wheel

### Identity (`IVX.Identity`)
- `IVX.Identity.fetch_profile(player)` — Cross-game profile
- `IVX.Identity.update_profile(player, fields)` — Update profile
- `IVX.Identity.fetch_wallet(player)` — Cross-game wallet
- `IVX.Identity.grant_currency(player, id, amount)` — Grant currency
- `IVX.Identity.read_storage(player, collection, key)` — Read cloud data
- `IVX.Identity.write_storage(player, collection, key, value)` — Write cloud data

## Requirements

- Roblox Studio or a live Roblox experience
- **Enable HTTP Requests**: Game Settings > Security > Allow HTTP Requests
- IntelliVerseX account + Game ID from [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers)

## License

MIT License — see LICENSE in the project root.
