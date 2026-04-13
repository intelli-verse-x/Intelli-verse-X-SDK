# IntelliVerseX Godot Engine SDK

> Complete modular game development SDK for Godot — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```gdscript
var ai_client := IVXAIClient.new()
add_child(ai_client)
ai_client.initialize("https://ai.intelli-verse-x.ai", "your-key")

var session = await ai_client.start_voice_session("persona-1", user_id)
await ai_client.send_text(session.session_id, "Hello!")
var personas = await ai_client.get_personas()
```

### Multiplayer & Game Modes (`IVXGameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```gdscript
var game_modes := IVXGameModes.new()
add_child(game_modes)

game_modes.select_mode(IVXGameModes.GameMode.ONLINE_VERSUS, 4)
game_modes.add_player("Alice", true)
game_modes.set_player_ready(0, true)
if game_modes.can_start_match():
    game_modes.start_match()
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```gdscript
var hiro := IVXHiroSystems.new()
add_child(hiro)
hiro.initialize(nakama_client, session)

var spin = await hiro.spin_wheel("daily_wheel")
var streak = await hiro.get_streak_state()
await hiro.claim_streak()
var offers = await hiro.get_offerwall_state()
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`IVXDiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```gdscript
var discord = IVXDiscordSocial.new()
add_child(discord)
discord.initialize({
    "application_id": "YOUR_APP_ID",
    "client_id": "YOUR_CLIENT_ID",
})

await discord.update_presence("In Match", "Round 3 of 5")
var friends = await discord.get_friends()
```

### Satori Analytics (`IVXSatori`)

- Event capture, feature flags, A/B experiments, live events

```gdscript
var satori = IVXSatori.new()
add_child(satori)
satori.initialize({
    "satori_url": "https://satori.example.com",
    "api_key": "your-satori-key",
})

await satori.capture_events([{"name": "level_complete", "value": "5"}])
var flags = await satori.get_feature_flags()
```

## Requirements

- Godot 4.2+ (tested with 4.6.x)

### Platform Support

- **Godot**: Windows, macOS, Linux, Android, iOS, Web (HTML5), Meta Quest (via OpenXR)

### VR / XR Support

The addon includes **`IVXXRHelper`** for OpenXR-oriented VR/XR platform detection and helper APIs—useful when shipping to Meta Quest and other OpenXR targets.

## Dependencies

You **should** add the **official Nakama Godot addon** (Heroic Labs) for real backend (auth, profile, wallet, leaderboards, storage, RPC). Without it the project still opens using a built-in stub, but backend calls will fail until Nakama is installed.

**Use only the official Heroic Labs client.** Do **not** use “Nakama Client in GDScript” (Asset Library #433) — that is a different, older addon with a different API and is not compatible with IntelliVerseX.

**Get the official Nakama addon from:**

| Source | Link / How to install |
|--------|------------------------|
| **GitHub (recommended)** | [github.com/heroiclabs/nakama-godot](https://github.com/heroiclabs/nakama-godot) — clone or download, then copy the **`addons/com.heroiclabs.nakama`** folder into your project’s `addons/` folder. Use the default branch for Godot 4. |
| **Godot Asset Library** | In Godot: **Project → AssetLib** → search **“Nakama”** and pick the **official** one (by Heroic Labs / novabyte, not “Nakama Client in GDScript”). Or use GitHub if the Asset Library only shows Godot 3 or community clients. |
| **Docs** | [heroiclabs.com/docs/nakama/client-libraries/godot](https://heroiclabs.com/docs/nakama/client-libraries/godot/) |

## Setting Up Nakama Server

The SDK requires a [Nakama](https://heroiclabs.com/nakama/) game server for backend features.

**Quick start with Docker:**

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

**Heroic Labs Cloud:** For production, use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

See [Nakama documentation](https://heroiclabs.com/docs/nakama/) for full setup instructions.

## Installation

1. Copy `addons/intelliversex/` into your project's `addons/` folder.
2. **(Recommended)** Install the Nakama addon (see [Dependencies](#dependencies) above): copy the **`addons/com.heroiclabs.nakama`** folder from the [nakama-godot](https://github.com/heroiclabs/nakama-godot) repo into your project’s `addons/` folder.
3. In **Project → Project Settings → Plugins**, enable **IntelliVerseX** and **Nakama** (both should appear if the addon includes `plugin.cfg`).

## Running the example (this repo)

1. Open `SDKs/godot` as a project in Godot 4.2+.
2. **Start a Nakama server** (required for auth/profile/wallet). For example with Docker:
   ```bash
   docker run -d -p 7350:7350 heroiclabs/nakama
   ```
   Or use your existing Nakama host and set **Server Host** / **Port** on the example node.
3. Local Nakama is usually **HTTP** — in the Inspector on **IntelliVerseXExample** leave **Use Ssl** off unless your server uses HTTPS.
4. Press **F5** to run the main scene. If you see a connection error, ensure Nakama is running and host/port match.

## Quick Start

```gdscript
extends Node

var config: IVXConfig

func _ready() -> void:
    config = IVXConfig.new()
    config.nakama_host = "127.0.0.1"
    config.nakama_port = 7350
    config.nakama_server_key = "defaultkey"
    config.enable_debug_logs = true

    IntelliVerseX.initialized.connect(_on_initialized)
    IntelliVerseX.auth_success.connect(_on_auth_success)
    IntelliVerseX.error.connect(_on_error)

    IntelliVerseX.initialize(config)

    if not IntelliVerseX.restore_session():
        IntelliVerseX.authenticate_device()


func _on_initialized() -> void:
    print("IntelliVerseX SDK ready!")


func _on_auth_success(session) -> void:
    print("Logged in as: ", IntelliVerseX.username)

    var profile = await IntelliVerseX.fetch_profile()
    print("Profile: ", profile)

    var wallet = await IntelliVerseX.fetch_wallet()
    print("Wallet: ", wallet)


func _on_error(message: String) -> void:
    push_error("IntelliVerseX error: " + message)
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
| AI Voice & Host | 🔶 Stub |
| Multiplayer & Game Modes | 🔶 Stub |
| Hiro Live-Ops Systems | 🔶 Stub |
| Analytics | ✅ Supported |
| Discord Social SDK | 🔶 Stub |
| Satori Analytics | 🔶 Stub |
| Monetization | ✅ Supported |

> 🔶 **Stub** = Full API surface exists. Methods log warnings and return empty/mock data. Zero code changes needed when backend support ships.

## Troubleshooting

**Only IntelliVerseX appears in Project Settings → Plugins; I don’t see Nakama.**  
This project adds a minimal `plugin.cfg` and `plugin.gd` inside `addons/com.heroiclabs.nakama` so **Nakama** appears in the list. If you still don’t see it, ensure the folder `addons/com.heroiclabs.nakama` is present and contains `plugin.cfg` and `plugin.gd`; then restart Godot or re-scan the project.

**I installed “Nakama Client in GDScript” (Asset Library #433) and get errors.**  
That addon (by Snopek) is **not** compatible with IntelliVerseX. It uses a different API and folder name (`addons/nakama-client`). Remove it and install the **official** Heroic Labs client from [GitHub](https://github.com/heroiclabs/nakama-godot): copy the folder **`addons/com.heroiclabs.nakama`** into your project’s `addons/` folder. Your `addons/` folder should contain `com.heroiclabs.nakama` (with files like `client/nakama.gd`, `client/nakama_client.gd`), not `nakama-client` or “Nakama Client”.

**Where exactly do I copy the official addon?**  
From the [nakama-godot](https://github.com/heroiclabs/nakama-godot) repo you get a ZIP or clone. Inside it there is a folder `addons/com.heroiclabs.nakama`. Copy that **whole folder** into your Godot project’s `addons/` directory, so you have `your_project/addons/com.heroiclabs.nakama/`. Do not copy only a subfolder like “client” or “nakama client”.

**I re-added Nakama from GitHub and Nakama no longer appears in Plugins.**  
The upstream nakama-godot repo does not include a `plugin.cfg` or `plugin.gd`. This SDK adds them so "Nakama" shows in Project Settings → Plugins. After replacing or re-adding `addons/com.heroiclabs.nakama` from upstream, copy the **`plugin.cfg`** and **`plugin.gd`** files from this repo's `addons/com.heroiclabs.nakama/` folder into your addon folder so the plugin appears again.

**Auth failed: Could not connect to the server at http(s)://…**  
The game could not reach the Nakama server. Start Nakama (e.g. `docker run -d -p 7350:7350 heroiclabs/nakama`) or set **Server Host** / **Port** in the Inspector to where Nakama runs. For local Nakama, use **Use Ssl** = false (HTTP); use true only if your server has HTTPS.

**Discord not connecting.**  
Ensure `application_id` and `client_id` are valid and your Discord app is approved. Check that the Discord desktop client is running.

**Satori events not captured.**  
Check `satori_url` and `api_key` are correctly configured. Verify the Satori server is reachable from your machine.

**AI features not working.**  
Verify the AI API endpoint and key are set in the config. Check Output for connection error details.

## Godot MCP (optional — let AI access the project)

You can connect **Godot to Cursor via MCP** so an AI assistant can open your project, run it, and read debug output instead of guessing.

**Requirements:** Node.js, npm, Godot installed, and an AI that supports MCP (e.g. Cursor with MCP enabled).

**Setup:**

1. **Install a Godot MCP server** (e.g. [Coding-Solo/godot-mcp](https://github.com/Coding-Solo/godot-mcp)):
   ```bash
   git clone https://github.com/Coding-Solo/godot-mcp.git
   cd godot-mcp
   npm install
   npm run build
   ```
2. **Add it in Cursor:** **Settings → Features → MCP → Add new MCP server**
   - **Command:** `node`
   - **Args:** `C:\path\to\godot-mcp\build\index.js` (Windows) or `/absolute/path/to/godot-mcp/build/index.js` (Mac/Linux)
   - **Name:** e.g. `godot`
3. **Optional:** Set `GODOT_PATH` to your Godot executable if it isn't found automatically.

Then you can ask the AI to "run the Godot project at SDKs/godot and show errors" or "launch the Godot editor for this project"; the MCP server will run Godot and return debug output so the AI can check for errors without assumptions.

## Project Structure

```
addons/intelliversex/
├── core/
│   └── ivx_manager.gd          # Core manager autoload
├── ai/
│   └── ivx_ai_client.gd        # AI voice & host client
├── gamemodes/
│   └── ivx_game_modes.gd       # Multiplayer & game mode management
└── hiro/
    └── ivx_hiro_systems.gd     # Hiro live-ops typed wrappers
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/godot/).

## Nakama Client Library

This SDK wraps the official [Nakama Godot Client](https://github.com/heroiclabs/nakama-godot) (737 stars, 88 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
