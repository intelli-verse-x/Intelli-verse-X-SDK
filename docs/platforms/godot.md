# Godot Engine

> IntelliVerseX addon for **Godot 4**, using **GDScript** with full **async/await** support. The runtime loads the official [Nakama Godot client](https://github.com/heroiclabs/nakama-godot) when present, or falls back to a built-in stub so the project still opens without the addon installed.

**SDK version (manager):** `5.8.0` (see `IntelliVerseX.SDK_VERSION` in `ivx_manager.gd`).

---

## Requirements

- **Godot 4.2+**
- **[Nakama Godot addon](https://github.com/heroiclabs/nakama-godot)** v3.5+ (recommended path: `addons/com.heroiclabs.nakama/`) for real backend connectivity

---

## Installation

1. Install the [Nakama Godot addon](https://heroiclabs.com/docs/nakama/client-libraries/godot/) from Heroic Labs.
2. Copy `addons/intelliversex/` from [SDKs/godot/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/godot) into your project’s `addons/` folder.
3. Enable **both** plugins under **Project → Project Settings → Plugins** (Nakama + IntelliVerseX).

The IntelliVerseX editor plugin registers an autoload singleton named **`IntelliVerseX`** pointing at `res://addons/intelliversex/core/ivx_manager.gd`.

---

## Quick Start

```gdscript
extends Node

func _ready() -> void:
    var config := IVXConfig.new()
    config.nakama_host = "127.0.0.1"
    config.nakama_port = 7350
    config.nakama_server_key = "defaultkey"
    config.enable_debug_logs = true

    IntelliVerseX.initialize(config)
    IntelliVerseX.auth_success.connect(_on_auth)

    if not IntelliVerseX.restore_session():
        IntelliVerseX.authenticate_device()

func _on_auth(session) -> void:
    print("Logged in: ", IntelliVerseX.username)
    var profile = await IntelliVerseX.fetch_profile()
    print("Profile: ", profile)
```

---

## API Surface

Use these types as your map of what ships in `addons/intelliversex/`. Most subsystems are **nodes** or **RefCounted** types you instantiate and add to the tree (or hold as resources)—only **`IntelliVerseX`** is registered as an autoload by the plugin.

| Symbol | Kind | Role |
|--------|------|------|
| **IntelliVerseX** | Autoload (`Node`) | Central coordinator: Nakama client/session/socket, auth, profile, wallet, storage, RPC, socket connect, session restore. Emits core lifecycle signals. |
| **IVXConfig** | `Resource` | Exportable game ID, Nakama host/port/server key/SSL, optional Cognito fields, analytics and debug flags. |
| **IVXMultiplayer** | `Node` | Lobby create/join/leave/list, matchmaking start/cancel; wraps Nakama multiplayer APIs. |
| **IVXGameModes** | `Node` | Game mode selection, room lifecycle, player slots, ready state, match start/end; **`match_found`** and related signals. |
| **IVXDiscordSocial** | `Node` | Discord-shaped social API (presence, friends, lobbies, voice, invites, DMs)—**stub** on Godot; use for API alignment with Unity. |
| **IVXDiscordModeration** | `Node` | Moderation hooks (stub). |
| **IVXDiscordMessages** | `Node` | Messaging helpers (stub). |
| **IVXDiscordLinkedChannels** | `Node` | Linked channels (stub). |
| **IVXDiscordDebug** | `Node` | Debug logging bridge (`log_received` signal). |
| **IVXHiroSystems** | `Node` | Hiro live-ops over Nakama RPC: **spin wheel**, **streaks**, **offerwall**, **retention**, **IAP trigger**, **smart ad timer**, **friend quests**, **friend battles**. |
| **IVXSatori** | `RefCounted` | Satori-shaped analytics (events, flags, experiments, live events)—**stub**; wire to real Satori or backend when needed. |
| **IVXAIClient** | `Node` | HTTP client for IntelliVerseX AI gateway: **voice sessions**, **text in session**, **host sessions/events**, **entitlements**, **personas**. |
| **IVXAINPCDialogManager** | `Node` | NPC dialog session API (**stub**; matches Unity surface). |
| **IVXAIAssistant** | `Node` | In-game assistant (**stub**). |
| **IVXAIModerator** | `Node` | Content moderation (**stub**). |
| **IVXAIContentGenerator** | `Node` | Generative content (**stub**). |
| **IVXAIProfiler** | `Node` | Player profiling (**stub**). |
| **IVXAIVoiceServices** | `Node` | TTS/STT-shaped services (**stub**). |
| **IVXDeepLinks** | `Node` | Deep link routing; emits **`deep_link_received`**. |
| **IVXXRHelper** | `Node` | XR platform detection and capability flags (`IVXXRHelper` in `ivx_xr_helper.gd`; script path `ivx_xr_helper.gd`). |
| **IVXNakamaStub** | `class_name` / fallback | Used when the official Nakama addon is missing so the project loads; not for production. |

---

## Feature Coverage

Values below are the **Godot 4** column from the repo’s audited matrix. Legend: **Y** = implemented | **S** = stub/mock (API present, local or non-functional backend) | **P** = partial | **-** = not applicable or not present.

### Core platform

| Feature | Godot |
|---------|:-----:|
| SDK init / config | Y |
| Device auth | Y |
| Email auth | Y |
| Google auth | P |
| Apple auth | P |
| Custom auth | Y |
| Session restore | Y |
| Profile fetch | Y |
| Profile update | Y |
| Wallet fetch | Y |
| Wallet grant | Y |
| Leaderboard submit / fetch | Y / Y |
| Cloud storage read / write | Y / Y |
| Generic RPC | Y |
| Real-time socket | Y |
| Events / callbacks | Y |

### AI voice and host

| Feature | Godot |
|---------|:-----:|
| AI initialize | Y |
| Voice session (start/end) | Y |
| Voice send text | Y |
| Voice poll messages | Y |
| Host session (start/event) | Y |
| Entitlement check | Y |
| Get personas | Y |

### AI LLM stack

| Feature | Godot |
|---------|:-----:|
| NPC dialog manager | S |
| AI assistant | S |
| AI content moderator | S |
| AI content generator | S |
| AI player profiler | S |
| AI voice services (TTS/STT) | S |

### Multiplayer and game modes

| Feature | Godot |
|---------|:-----:|
| Select mode | S |
| Add/remove player | S |
| Start/end match | S |
| Lobby create / join / list / leave | Y |
| Matchmaking find / cancel | Y / Y |
| Local multiplayer (session/turns/split) | - |

### Hiro live-ops

| Feature | Godot |
|---------|:-----:|
| Spin wheel (get/spin) | Y |
| Streaks (get/update/claim) | Y |
| Offerwall (get/complete/claim) | Y |
| Friend quests / battles | Y / Y |
| IAP trigger | Y |
| Smart ad timer | Y |
| Retention (get/update) | Y |

### Discord social SDK

| Feature | Godot |
|---------|:-----:|
| Init/connect, OAuth, presence, friends, lobby/chat, voice, invites, DMs, moderation, linked channels, debug, settings | **S** (all listed areas use stub/social-shaped APIs) |

### Satori analytics

| Feature | Godot |
|---------|:-----:|
| Initialize/authenticate, capture events, feature flags, experiments, live events, identity update/logout | **S** |

### Platform and extras

| Feature | Godot |
|---------|:-----:|
| Deep links | Y |
| Bootstrap (one-drop style init) | Y |
| Safe area / foldables / perf optimizer / demo UIs | - |
| Web3 / WebGL | - / - |

### XR (selected)

| Feature | Godot |
|---------|:-----:|
| XR platform detection | Y |
| Meta Quest / SteamVR (runtime support) | S / S |
| Linux / SteamOS | S |

For the full cross-platform comparison, see [FEATURE_COVERAGE_MATRIX.md](../FEATURE_COVERAGE_MATRIX.md).

---

## Key Classes (summary)

| Class | Description |
|-------|-------------|
| `IVXConfig` | Resource for server, identity, and debug configuration. |
| `IntelliVerseX` (autoload) | Singleton manager for Nakama session and core game backend calls. |

---

## Signals Reference

### IntelliVerseX (`ivx_manager.gd`)

| Signal | Payload | When |
|--------|---------|------|
| `initialized` | — | `initialize()` completed and client is ready. |
| `auth_success` | `session` | Device/email/social auth succeeded. |
| `auth_error` | `message: String` | Auth failed or session invalid. |
| `profile_loaded` | `profile: Dictionary` | Profile data loaded (use this for UI refresh when profile changes). |
| `wallet_updated` | `wallet: Dictionary` | Wallet balances updated. |
| `error` | `message: String` | General SDK errors (e.g. not initialized). |

### IVXMultiplayer

| Signal | Payload |
|--------|---------|
| `lobby_created` | `lobby: Dictionary` |
| `lobby_joined` | `lobby: Dictionary` |
| `lobby_left` | `lobby_id: String` |
| `lobbies_listed` | `lobbies: Array` |
| `matchmaking_started` | `ticket: Dictionary` |
| `matchmaking_cancelled` | `ticket_id: String` |
| `multiplayer_error` | `message: String` |

### IVXGameModes

| Signal | Payload |
|--------|---------|
| `mode_changed` | `mode: GameMode` |
| `player_added` / `player_removed` | slot + info |
| `player_ready_changed` | `slot: int`, `ready: bool` |
| `match_started` / `match_ended` | — |
| **`match_found`** | `match_info: Dictionary` |
| `room_created` / `room_joined` | `room_id: String` |
| `room_left` | — |
| `room_list_updated` | `rooms: Array` |
| `search_cancelled` | — |

### IVXHiroSystems

| Signal | Payload |
|--------|---------|
| `spin_wheel_result` | `reward: Dictionary` |
| `streak_updated` | `streak: Dictionary` |
| `streak_claimed` | `streak_id`, `milestone`, `reward` |
| `offerwall_updated` | `offers: Array` |
| `retention_updated` | `state: Dictionary` |
| `iap_trigger_result` | `result: Dictionary` |
| `smart_ad_timer_result` | `result: Dictionary` |
| `friend_quest_updated` | `quest: Dictionary` |
| `friend_battle_result` | `result: Dictionary` |

### IVXAIClient

| Signal | Payload |
|--------|---------|
| `voice_session_started` | `session_id: String` |
| `message_received` | `session_id`, `message: Dictionary` |
| `host_message_received` | `session_id`, `message: Dictionary` |
| `entitlement_changed` | `user_id`, `entitled: bool` |

### IVXDiscordSocial

| Signal | Payload |
|--------|---------|
| `discord_ready` | `provisional: bool` |
| `discord_error` | `error_message: String` |
| `invite_received` | `invite: Dictionary` |
| `join_request` | `user_id`, `username` |
| `lobby_message_received` | `message: Dictionary` |
| `voice_state_update` | `user_id`, `speaking: bool` |
| `friends_updated` | `friends: Array` |
| `lobby_joined` / `lobby_left` | lobby id / — |
| `voice_joined` / `voice_left` | channel id / — |

### IVXDeepLinks

| Signal | Payload |
|--------|---------|
| `deep_link_received` | `route: String`, `params: Dictionary` |

### IVXXRHelper

| Signal | Payload |
|--------|---------|
| `xr_platform_detected` | `platform: XRPlatform` |
| `tracking_state_changed` | `state: TrackingState` |
| `xr_focus_changed` | `has_focus: bool` |

### IVXDiscordDebug

| Signal | Payload |
|--------|---------|
| `log_received` | `level: int`, `message: String`, `source: String` |

---

## Advanced Examples

### 1. Hiro spin wheel with `await` (GDScript)

 Instantiate `IVXHiroSystems` after the player has a Nakama session (same `nakama_client` and `session` references you get from successful auth via `IntelliVerseX`).

```gdscript
extends Node

@onready var _hiro: IVXHiroSystems = IVXHiroSystems.new()

func _ready() -> void:
    add_child(_hiro)
    IntelliVerseX.auth_success.connect(_on_authenticated)
    _hiro.spin_wheel_result.connect(_on_spin_result)

func _on_authenticated(_session) -> void:
    _hiro.initialize(IntelliVerseX.nakama_client, IntelliVerseX.nakama_session)
    var state := await _hiro.spin_wheel_get()
    print("Spin wheel state: ", state)

func spin_daily() -> void:
    var reward := await _hiro.spin_wheel_spin()
    print("Spin RPC result: ", reward)

func _on_spin_result(reward: Dictionary) -> void:
    print("Signal spin_wheel_result: ", reward)
```

### 2. AI NPC dialog flow (stubs + real voice client)

 For **production voice/host**, use **`IVXAIClient`** against your IntelliVerseX AI base URL. The **`IVXAINPCDialogManager`** class exists for **API parity** with Unity but **emits warnings** and returns empty data until implemented—structure your game so you can swap in real dialog later.

```gdscript
extends Node

var _ai: IVXAIClient
var _npc: IVXAINPCDialogManager

func _ready() -> void:
    _ai = IVXAIClient.new()
    _npc = IVXAINPCDialogManager.new()
    add_child(_ai)
    add_child(_npc)

    _ai.initialize("https://your-ai-gateway.example", "optional-api-key")

    IntelliVerseX.auth_success.connect(_on_auth)

func _on_auth(_session) -> void:
    var uid := IntelliVerseX.user_id
    var voice := await _ai.start_voice_session("host-persona", uid)
    if voice.has("session_id"):
        await _ai.send_text(voice["session_id"], "Hello from Godot!")

    # Stub NPC path: register/start when IVXAINPCDialogManager is implemented server-side
    _npc.initialize(null)
    _npc.set_auth_token(IntelliVerseX.nakama_session.token if IntelliVerseX.nakama_session else "")
    var dialog := await _npc.start_dialog("shop_keeper", uid, "first_visit")
    print("NPC dialog session (stub): ", dialog)
```

---

## Troubleshooting

| Symptom | Likely cause | What to do |
|---------|----------------|------------|
| IntelliVerseX plugin / addon not loading | Wrong folder layout or plugin disabled | Ensure `addons/intelliversex/plugin.cfg` exists; enable the plugin under **Project Settings → Plugins**. Reload the project. |
| “Could not load Nakama” or silent stub behavior | Official Heroic Labs addon missing or wrong package | Install `addons/com.heroiclabs.nakama` from [nakama-godot](https://github.com/heroiclabs/nakama-godot). Remove conflicting addons (e.g. `nakama-client` Snopek variant)—see warning in `ivx_manager.gd`. |
| Nakama connection / TLS errors | Host, port, `nakama_use_ssl`, or server key mismatch | Align `IVXConfig` with your server. For local dev, often `http` + port `7350` + `defaultkey`. |
| Export fails or missing symbols | GDScript-only addon not included in export | Confirm `addons/intelliversex/**` is **not** excluded in **Export → Resources**; filter defaults usually include `addons/`. |
| Signal not firing | Connected after emit, or wrong node | Connect in `_ready()` before calling async methods that emit; connect on the **same instance** you call (`_hiro.spin_wheel_result`, not a duplicate node). |
| `match_found` never received | Game modes vs Nakama multiplayer | `match_found` lives on **`IVXGameModes`**, not `IVXMultiplayer`; use the correct node for your flow. |
| OpenXR / headset issues | Export template, XR mode, or platform | Enable **XR** in project, pick the correct **OpenXR** target, and use **`IVXXRHelper`** only as a detector—Godot XR still requires proper export presets (Quest, SteamVR, etc.). |
| Satori/Discord “not yet implemented” spam | Expected for stubs | `IVXSatori` and Discord nodes log stub warnings; integrate real Discord/Satori SDKs or backend proxies when you need live behavior. |

---

## Nakama Client

Built on **[nakama-godot](https://github.com/heroiclabs/nakama-godot)** (official Heroic Labs client). `IntelliVerseX` creates the HTTP client and can open a real-time socket using the same addon. Refer to [Nakama documentation](https://heroiclabs.com/docs/nakama/concepts/) for matchmaking, storage, and RPC semantics.

---

## Source

- Addon and examples: [SDKs/godot/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/godot)
- Broader SDK repo: [Intelli-verse-X-Unity-SDK](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK)

---

## Further Reading

- [Nakama client libraries — Godot](https://heroiclabs.com/docs/nakama/client-libraries/godot/)
- [Nakama Godot on GitHub](https://github.com/heroiclabs/nakama-godot)
- [IntelliVerseX feature coverage matrix](../FEATURE_COVERAGE_MATRIX.md) (all platforms, Godot column)
- [Godot Engine — XR / OpenXR](https://docs.godotengine.org/en/stable/tutorials/xr/index.html)
- IntelliVerseX Godot README in-repo: `SDKs/godot/README.md` (feature overview and snippet index)
- [.cursor/skills/ivx-cross-platform/SKILL.md](../../.cursor/skills/ivx-cross-platform/SKILL.md) — cross-engine parity and stub expectations
