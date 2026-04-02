# Roblox Integration Guide

> **Time to first AI NPC response: ~15 minutes** (including account setup and Studio configuration).

---

## What You Get

The IntelliVerseX Roblox SDK gives your experience three capabilities that Roblox doesn't provide natively:

| Capability | Modules | What It Does |
|-----------|---------|-------------|
| **AI / LLM Stack** | NPC, Voice, Assistant, ContentGenerator, Moderator, Profiler | Dynamic NPC dialog, TTS/STT, procedural content, real-time chat moderation, player behavior profiling |
| **Hiro Live-Ops** | SpinWheel, Streaks, DailyRewards, DailyMissions, Achievements, SeasonPass, Leagues, FortuneWheel, Tournaments, Goals, Retention, FriendStreaks | 12 server-authoritative engagement systems that go far beyond Roblox's basic economy |
| **Cross-Game Identity** | Identity | Sync player profiles, wallets, and save data across Roblox experiences AND non-Roblox games (mobile, PC, console) |

!!! tip "Zero redundancy"
    We intentionally exclude leaderboards, matchmaking, monetization, analytics, and chat because Roblox already provides them. You get only what's genuinely missing.

---

## Prerequisites

| Requirement | Details |
|-------------|---------|
| **Roblox Studio** | Latest version from [create.roblox.com](https://create.roblox.com) |
| **IntelliVerseX Account** | Register at [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers) |
| **Game ID** | Created in the developer dashboard after registration |
| **HTTP Requests** | Must be enabled in Game Settings > Security |
| **Server Key** | Provided in your dashboard (default: `defaultkey` for development) |

---

## Step 1: Installation

Choose one of three installation methods:

### Option A: Wally + Rojo (Recommended for Teams)

If you use the [Rojo](https://rojo.space) + [Wally](https://wally.run) workflow:

```toml title="wally.toml"
[package]
name = "your-studio/your-game"
version = "1.0.0"
realm = "server"

[dependencies]
IntelliVerseX = "intelliversex/ivx-roblox@5.9.0"
```

```bash
wally install
```

Wally places the SDK in `Packages/IntelliVerseX`. Your `default.project.json` should map it to `ServerScriptService`:

```json title="default.project.json (snippet)"
{
  "ServerScriptService": {
    "Packages": {
      "$path": "Packages"
    }
  }
}
```

### Option B: Direct Copy (Simplest)

1. Download or clone the `SDKs/roblox/src/` folder from [GitHub](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/tree/main/SDKs/roblox/src)
2. In Roblox Studio, right-click **ServerScriptService** > **Insert Object** > **Folder** > Name it `IntelliVerseX`
3. Drag the `.lua` files into the folder, preserving the `AI/` and `Hiro/` sub-folder structure
4. Ensure every file is a **ModuleScript** (not Script or LocalScript)

### Option C: Studio Plugin

Install the IntelliVerseX plugin from the Creator Store. It provides a toolbar button with a config panel — useful for non-Rojo workflows.

---

## Step 2: Enable HTTP Requests

This is **mandatory**. The SDK communicates with backend services via `HttpService`.

1. Open Roblox Studio
2. Go to **Game Settings** (Home tab > Game Settings)
3. Navigate to **Security**
4. Toggle **Allow HTTP Requests** to **ON**
5. Click **Save**

!!! warning "Published experiences"
    HTTP Requests must also be enabled for the published version. This setting persists per-place, so enable it before your first publish.

---

## Step 3: Configure the SDK

Create a **Script** (not LocalScript) in **ServerScriptService**:

```lua title="ServerScriptService/GameInit.server.lua"
local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
    game_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",  -- from your dashboard
    server_key = "your-server-key",                      -- from your dashboard
    debug = true,                                        -- set false in production
})

-- Automatically authenticate every player on join
-- and clean up sessions on leave
IVX.enable_auto_auth()

-- Listen for auth events
IVX.on("auth_success", function(player, session)
    print(player.Name .. " authenticated -> Nakama user " .. session.user_id)
end)

IVX.on("auth_error", function(player, err)
    warn("Auth failed for " .. player.Name .. ": " .. err)
end)
```

### Configuration Options

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `game_id` | string | `""` | **Required.** Your game UUID from the dashboard |
| `host` | string | `"nakama-rest.intelli-verse-x.ai"` | Nakama backend host |
| `port` | number | `443` | Backend port |
| `server_key` | string | `"defaultkey"` | Nakama server key |
| `use_ssl` | boolean | `true` | Use HTTPS |
| `ai_base_url` | string | `"https://ai.intelli-verse-x.ai"` | AI/LLM endpoint base |
| `ai_api_key` | string | `""` | AI API key (if required) |
| `debug` | boolean | `false` | Enable verbose logging |

---

## Step 4: Add AI NPCs to Your Experience

### 4a. Server Setup

```lua title="ServerScriptService/NPCServer.server.lua"
local IVX = require(game.ServerScriptService.IntelliVerseX)

-- Define NPC personalities
local NPCS = {
    shopkeeper = {
        npc_id = "shopkeeper_01",
        persona_id = "friendly_merchant",
        name = "Elara the Merchant",
        system_prompt = "You are Elara, a friendly merchant in a fantasy RPG. "
            .. "You sell potions, weapons, and armor. Be helpful but try to upsell. "
            .. "Keep responses under 3 sentences. Use the player's name when possible.",
        max_turns = 20,
    },
    guard = {
        npc_id = "gate_guard",
        persona_id = "stern_guard",
        name = "Captain Thorne",
        system_prompt = "You are Captain Thorne, the stern but fair castle gate guard. "
            .. "Only let players through if they can prove they have a quest. "
            .. "You speak in short, military sentences.",
        max_turns = 10,
    },
}

-- Track active dialogs
local activeDialogs = {}

IVX.Remotes.on_server_event("IVX_StartNPCDialog", function(player, npcKey)
    local npcConfig = NPCS[npcKey]
    if not npcConfig then return end

    -- End existing dialog if any
    if activeDialogs[player.UserId] then
        IVX.AI.NPC.end_dialog(activeDialogs[player.UserId])
    end

    local session, err = IVX.AI.NPC.start_dialog(npcConfig, tostring(player.UserId))
    if session then
        activeDialogs[player.UserId] = session.dialog_id
        IVX.Remotes.fire_client("IVX_NPCResponse", player, {
            npc_name = npcConfig.name,
            message = session.greeting or "Hello, traveler!",
            dialog_id = session.dialog_id,
        })
    else
        warn("[NPC] Start failed: " .. (err or "unknown"))
    end
end)

IVX.Remotes.on_server_event("IVX_SendNPCMessage", function(player, message)
    local dialogId = activeDialogs[player.UserId]
    if not dialogId then return end

    -- Moderate input first
    local modResult = IVX.AI.Moderator.check_text(message)
    if modResult and modResult.flagged then
        IVX.Remotes.fire_client("IVX_NPCResponse", player, {
            message = "[Message blocked by moderation]",
        })
        return
    end

    local response, err = IVX.AI.NPC.send_message(dialogId, message)
    if response then
        IVX.Remotes.fire_client("IVX_NPCResponse", player, {
            message = response.text or response.message or "",
        })
    end
end)

IVX.Remotes.on_server_event("IVX_EndNPCDialog", function(player)
    local dialogId = activeDialogs[player.UserId]
    if dialogId then
        IVX.AI.NPC.end_dialog(dialogId)
        activeDialogs[player.UserId] = nil
    end
end)

game.Players.PlayerRemoving:Connect(function(player)
    local dialogId = activeDialogs[player.UserId]
    if dialogId then
        IVX.AI.NPC.end_dialog(dialogId)
        activeDialogs[player.UserId] = nil
    end
end)
```

### 4b. Client Setup

```lua title="StarterPlayerScripts/NPCClient.client.lua"
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local Players = game:GetService("Players")
local player = Players.LocalPlayer

-- Wait for SDK remotes to be created
local remotes = ReplicatedStorage:WaitForChild("IntelliVerseXRemotes")
local startDialog = remotes:WaitForChild("IVX_StartNPCDialog")
local sendMessage = remotes:WaitForChild("IVX_SendNPCMessage")
local endDialog = remotes:WaitForChild("IVX_EndNPCDialog")
local npcResponse = remotes:WaitForChild("IVX_NPCResponse")

-- Listen for NPC responses
npcResponse.OnClientEvent:Connect(function(data)
    -- Display in your UI (chat bubble, dialog box, etc.)
    print("[NPC] " .. (data.npc_name or "NPC") .. ": " .. data.message)
end)

-- Example: trigger when player clicks an NPC model
local function onNPCClicked(npcModel)
    local npcKey = npcModel:GetAttribute("IVXNpcKey") -- e.g., "shopkeeper"
    if npcKey then
        startDialog:FireServer(npcKey)
    end
end

-- Example: send a player message
local function sendPlayerMessage(text)
    sendMessage:FireServer(text)
end
```

### 4c. Set Up NPC Models

On your NPC model in the workspace, add a **String Attribute**:

- **Name:** `IVXNpcKey`
- **Value:** `shopkeeper` (or `guard`, etc. — must match a key in your `NPCS` table)

Add a `ClickDetector` or `ProximityPrompt` to trigger `onNPCClicked`.

---

## Step 5: Add Hiro Live-Ops

### Daily Rewards + Streaks

```lua title="ServerScriptService/LiveOps.server.lua"
local IVX = require(game.ServerScriptService.IntelliVerseX)

-- Daily reward status (client calls this on join to update UI)
IVX.Remotes.on_server_invoke("IVX_GetDailyRewards", function(player)
    local status, err = IVX.Hiro.DailyRewards.get_status(player)
    if status then
        return { ok = true, data = status }
    end
    return { ok = false, error = err }
end)

-- Claim daily reward
IVX.Remotes.on_server_invoke("IVX_ClaimDailyReward", function(player)
    local result, err = IVX.Hiro.DailyRewards.claim(player)
    if result then
        IVX.Hiro.Streaks.update(player)
        return { ok = true, data = result }
    end
    return { ok = false, error = err }
end)

-- Streak info
IVX.Remotes.on_server_invoke("IVX_GetStreaks", function(player)
    local data, err = IVX.Hiro.Streaks.get(player)
    return { ok = data ~= nil, data = data, error = err }
end)
```

### Season Pass

```lua
-- Get current season pass state
IVX.Remotes.on_server_invoke("IVX_GetSeasonPass", function(player)
    local data, err = IVX.Hiro.SeasonPass.get(player)
    return { ok = data ~= nil, data = data, error = err }
end)

-- Award XP when player completes a game action
local function awardSeasonXP(player, amount)
    IVX.Hiro.SeasonPass.add_xp(player, amount)
end

-- Claim a tier reward
IVX.Remotes.on_server_invoke("IVX_ClaimSeasonTier", function(player, tier)
    if type(tier) ~= "number" then return { ok = false, error = "Invalid tier" } end
    local data, err = IVX.Hiro.SeasonPass.claim_tier(player, tier)
    return { ok = data ~= nil, data = data, error = err }
end)
```

### Spin Wheel / Fortune Wheel

```lua
IVX.Remotes.on_server_invoke("IVX_SpinWheel", function(player)
    local result, err = IVX.Hiro.SpinWheel.spin(player)
    return { ok = result ~= nil, data = result, error = err }
end)

IVX.Remotes.on_server_invoke("IVX_SpinFortuneWheel", function(player)
    local result, err = IVX.Hiro.FortuneWheel.spin(player)
    return { ok = result ~= nil, data = result, error = err }
end)
```

### Leagues & Tournaments

```lua
-- Submit score to league
IVX.Remotes.on_server_invoke("IVX_SubmitLeagueScore", function(player, score)
    if type(score) ~= "number" then return { ok = false } end
    local data, err = IVX.Hiro.Leagues.submit_score(player, score)
    return { ok = data ~= nil, data = data, error = err }
end)

-- Join a tournament
IVX.Remotes.on_server_invoke("IVX_JoinTournament", function(player, tournamentId)
    if type(tournamentId) ~= "string" then return { ok = false } end
    local data, err = IVX.Hiro.Tournaments.join(player, tournamentId)
    return { ok = data ~= nil, data = data, error = err }
end)
```

---

## Step 6: Cross-Game Identity

This is the unique-value module that lets you sync player data across **multiple Roblox experiences and non-Roblox games**.

```lua title="ServerScriptService/CrossGame.server.lua"
local IVX = require(game.ServerScriptService.IntelliVerseX)

-- Fetch cross-game profile (display name, avatar, metadata from ANY game)
IVX.Remotes.on_server_invoke("IVX_GetProfile", function(player)
    local profile, err = IVX.Identity.fetch_profile(player)
    return { ok = profile ~= nil, data = profile, error = err }
end)

-- Update display name (synced everywhere)
IVX.Remotes.on_server_invoke("IVX_UpdateDisplayName", function(player, name)
    if type(name) ~= "string" or #name < 1 or #name > 50 then
        return { ok = false, error = "Invalid name" }
    end
    local ok, err = IVX.Identity.update_profile(player, { display_name = name })
    return { ok = ok, error = err }
end)

-- Cross-game wallet (shared currency across all your games)
IVX.Remotes.on_server_invoke("IVX_GetWallet", function(player)
    local data, err = IVX.Identity.fetch_wallet(player)
    return { ok = data ~= nil, data = data, error = err }
end)

-- Save progress (accessible from any game using IntelliVerseX)
IVX.Remotes.on_server_invoke("IVX_SaveProgress", function(player, data)
    if type(data) ~= "table" then return { ok = false, error = "Invalid data" } end
    local ok, err = IVX.Identity.write_storage(player, "game_progress", "main", data)
    return { ok = ok, error = err }
end)

-- Load progress
IVX.Remotes.on_server_invoke("IVX_LoadProgress", function(player)
    local data, err = IVX.Identity.read_storage(player, "game_progress", "main")
    return { ok = data ~= nil, data = data, error = err }
end)
```

---

## Genre-Specific Integration Patterns

### Obby / Platformer

| Feature | Usage |
|---------|-------|
| Daily Rewards | Reward coins on daily login to unlock custom trails |
| Streaks | 7-day streak grants exclusive checkpoint skip |
| Achievements | "Completed World 3", "100 Deaths", "Speed Run Under 60s" |
| AI Content | Generate random obstacle descriptions or NPC hints |
| Cross-Game | Share coin balance between your obby and your tycoon |

### Tycoon / Simulator

| Feature | Usage |
|---------|-------|
| Season Pass | Premium track with exclusive machines/upgrades |
| Leagues | Weekly earnings leaderboard with tier-based rewards |
| Daily Missions | "Earn 10K coins", "Rebirth 3 times", "Collect 50 items" |
| Spin Wheel | Free spin every 4 hours, premium currency spin |
| AI Profiler | Detect idle players and offer comeback incentives |

### RPG / Adventure

| Feature | Usage |
|---------|-------|
| AI NPCs | Every merchant, quest-giver, and companion has unique personality |
| Content Generator | Procedural side quests, loot descriptions, dungeon lore |
| Tournaments | Weekly boss-kill competitions |
| Goals | Monthly goal: "Defeat 100 enemies" for legendary gear |
| Cross-Game Identity | Character profile visible across your RPG universe |

### Horror / Story

| Feature | Usage |
|---------|-------|
| AI NPCs | Creepy AI-driven characters that remember conversation context |
| Content Moderator | Filter player-created notes/messages |
| Retention | Track session depth, offer cliffhanger rewards |
| Friend Streaks | "Survive together 3 nights in a row" bonuses |

### Quiz / Trivia

| Feature | Usage |
|---------|-------|
| AI Content Generator | Generate quiz questions on any topic with difficulty scaling |
| Leagues | Weekly quiz leagues with promotion/relegation |
| Daily Missions | "Answer 20 questions correctly today" |
| Achievements | Subject mastery badges |

---

## Advanced Topics

### Input Validation

Always validate data coming from clients before passing to the SDK:

```lua
IVX.Remotes.on_server_invoke("IVX_UpdateProfile", function(player, fields)
    -- Validate types
    if type(fields) ~= "table" then
        return { ok = false, error = "Invalid input" }
    end

    -- Whitelist allowed fields
    local safe = {}
    if type(fields.display_name) == "string" and #fields.display_name <= 50 then
        safe.display_name = fields.display_name
    end
    if type(fields.avatar_url) == "string" and #fields.avatar_url <= 500 then
        safe.avatar_url = fields.avatar_url
    end

    local ok, err = IVX.Identity.update_profile(player, safe)
    return { ok = ok, error = err }
end)
```

### Rate Limiting

Prevent abuse by throttling client requests:

```lua
local lastRequest = {}
local COOLDOWN = 1 -- seconds

IVX.Remotes.on_server_invoke("IVX_SpinWheel", function(player)
    local now = os.clock()
    if lastRequest[player.UserId] and now - lastRequest[player.UserId] < COOLDOWN then
        return { ok = false, error = "Too fast" }
    end
    lastRequest[player.UserId] = now

    local result, err = IVX.Hiro.SpinWheel.spin(player)
    return { ok = result ~= nil, data = result, error = err }
end)
```

### Error Handling Pattern

All SDK functions return `(result?, error?)`. Use this consistently:

```lua
local function safeSdkCall(fn, ...)
    local ok, result, err = pcall(fn, ...)
    if not ok then
        warn("[IVX] pcall error: " .. tostring(result))
        return nil, "Internal error"
    end
    return result, err
end

-- Usage
IVX.Remotes.on_server_invoke("IVX_ClaimDaily", function(player)
    local data, err = safeSdkCall(IVX.Hiro.DailyRewards.claim, player)
    return { ok = data ~= nil, data = data, error = err }
end)
```

### Custom RPC Calls

Call any Nakama RPC directly:

```lua
local result, err = IVX.call_rpc(player, "my_custom_rpc", '{"key": "value"}')
```

### Using with Existing DataStores

The SDK stores only authentication tokens in DataStoreService. It won't conflict with your existing DataStores. Your game's save data, settings, and progress should continue using your own DataStore implementation. Use `IVX.Identity` only for data you want shared across multiple games.

---

## Performance Considerations

| Concern | Guidance |
|---------|----------|
| **HTTP budget** | Roblox allows 500 HTTP requests/minute. Each SDK call uses 1 request. Batch where possible. |
| **DataStore budget** | Auth uses 1 GetAsync + 1 SetAsync per player join. Well within limits. |
| **Memory** | The SDK keeps session tokens in-memory (a few KB per player). |
| **Module load** | All 34 ModuleScripts load on first `require`. Takes ~10ms. |
| **AI latency** | AI NPC responses typically take 500ms-2s depending on prompt complexity. Show a typing indicator. |

---

## Security Model

| Layer | Protection |
|-------|-----------|
| **Transport** | All HTTP traffic uses HTTPS (TLS 1.2+) |
| **Auth** | Server key sent via Base64-encoded Basic auth header |
| **Sessions** | Bearer tokens stored server-side only; never sent to clients |
| **Client isolation** | Clients interact only via RemoteEvents/Functions; no direct HTTP access |
| **Input validation** | Always validate client data before passing to SDK functions |
| **Rate limiting** | Implement server-side cooldowns on expensive operations |

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `"HTTP Requests are not enabled"` | HttpService disabled | Game Settings > Security > Allow HTTP Requests > ON |
| `"Auth failed: HTTP 0"` | Backend unreachable | Check host config; verify internet connectivity |
| `"Auth failed: HTTP 401"` | Invalid server key | Check `server_key` matches your dashboard |
| `"Auth failed: HTTP 404"` | Wrong host or endpoint | Verify `host` is correct (not the AI URL) |
| `"No session for player"` | Player not authenticated | Call `IVX.enable_auto_auth()` before using modules |
| `"RPC failed: HTTP 404"` | RPC not registered on backend | Verify the Hiro module is configured on your Nakama instance |
| `AI NPC returns empty` | Missing AI API key | Set `ai_api_key` in configuration |
| `Timeout / HTTP 0 on AI calls` | AI endpoint slow/down | Increase retry or check `ai_base_url` |

---

## Migration from DataStore-Only to IntelliVerseX

If you have an existing experience using only DataStores:

1. **Add IntelliVerseX alongside** — the SDK doesn't replace your DataStores
2. **Start with AI NPCs** — lowest risk, highest visible impact
3. **Add Live-Ops gradually** — start with DailyRewards, then expand to SeasonPass/Leagues
4. **Enable Cross-Game Identity last** — only when you have multiple experiences to sync

---

## Full Working Example

See the `SDKs/roblox/examples/` folder for complete, runnable scripts:

| File | What It Demonstrates |
|------|---------------------|
| `ai_npc_server.lua` | AI NPC dialog with multiple personas |
| `hiro_daily_rewards.lua` | Daily rewards + streaks + spin wheel |
| `cross_game_profile.lua` | Cross-game profile sync + cloud saves |

---

## Next Steps

- [Roblox Platform Reference](../platforms/roblox.md) — Full API reference
- [AI Getting Started](ai-getting-started.md) — Deep dive into AI capabilities
- [Hiro Integration](hiro-integration.md) — All 33 Hiro systems explained
- [Nakama Backend Setup](nakama-integration.md) — Configure your backend
