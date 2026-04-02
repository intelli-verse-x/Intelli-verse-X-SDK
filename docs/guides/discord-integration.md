# Discord Social SDK Integration

This guide walks you through enabling Discord Social features in a Unity game with the IntelliVerseX SDK. All APIs referenced here match the [Discord module](../modules/discord.md) documentation.

---

## Overview

The Discord module wraps the [Discord Social SDK](https://discord.com/developers/docs/social-sdk/overview) so your title can participate in Discord’s social graph alongside IntelliVerseX systems (multiplayer, live-ops, leaderboards). Typical capabilities include:

| Area | What you can do |
|------|-----------------|
| **Rich Presence** | Show game state, party size, join secrets, buttons, and assets in Discord |
| **Voice** | Join lobby voice, mute/deafen, volumes, VAD, optional FMOD/Wwise-style audio hooks |
| **Lobbies** | Create or join Discord lobbies by secret, sync metadata, lobby text chat, idle timeout |
| **Direct messages** | Send, edit, fetch history, summaries, suppress notifications while in-game chat is focused |
| **Moderation** | Parse moderation metadata, optional voice capture for pipelines, user reports, webhook + auto-moderation flags |
| **Linked channels** | Bridge in-game chat to a Discord server text channel |
| **Social / settings** | Open Discord UI for connected games, user profiles, and DM settings |

!!! info "Stub mode"
    If the `com.discord.social-sdk` UPM package is not installed, IVX Discord types compile and run in **stub mode** (safe no-ops or mock data with logs). **WebGL** is not a native target for the Discord Social SDK; expect stub behavior there as well.

---

## Prerequisites

| Requirement | Notes |
|-------------|--------|
| **Discord Developer Portal application** | Create an app at [Discord Developer Portal](https://discord.com/developers/applications) and note the **Application ID** |
| **OAuth2 credentials** | Client ID and redirect URIs registered for your platforms (see Step 1) |
| **Rich Presence art (optional but recommended)** | Large/small image keys uploaded under **Rich Presence → Art Assets** in the portal |
| **Unity** | **2021.3 LTS** or newer (IntelliVerseX targets modern Unity; confirm against your SDK release notes) |
| **`com.discord.social-sdk`** | Version **≥ 0.1.0** so `INTELLIVERSEX_HAS_DISCORD` is defined from the IVX assembly version defines |
| **Nakama backend** | Used by IntelliVerseX for auth and game-side friends; Discord features compose with `IVXBootstrap` / backend as described in the Discord module |
| **Production access (when applicable)** | For **communication** scopes (DMs, lobbies, voice), follow Discord’s program requirements |

---

## Step 1 — Create a Discord application

1. Open the [Discord Developer Portal](https://discord.com/developers/applications) and select **New Application**.
2. Under **General Information**, copy the **Application ID** (snowflake). This maps to `IVXDiscordConfig.ApplicationId`.
3. Open **OAuth2**:
    - Copy the **Client ID** into your `IVXDiscordConfig` (`ClientId` field in the module table).
    - Add **Redirects** that match how your game completes OAuth:
        - **Desktop**: commonly `https://localhost` (as noted in the module prerequisites).
        - **Mobile**: a custom URL scheme aligned with `MobileRedirectScheme` (e.g. `mygame://`) and the same value in the portal.
        - **Console**: use the device-code style flow described in the module; follow Discord’s console guidance.
4. Under **Rich Presence → Art Assets**, upload keys you will reference from code or config (`LargeImageAssetKey`, `SmallImageAssetKey`, `InviteCoverImageKey`, etc.).
5. Review Discord’s documentation for scopes and production eligibility for DMs, lobbies, and voice.

!!! warning "Redirect URI mismatch"
    A typo or missing redirect in the portal is one of the most common reasons OAuth fails. The value in `IVXDiscordConfig.RedirectUri` must match an authorized redirect exactly.

---

## Step 2 — Configure the SDK

Discord settings live primarily on an **`IVXDiscordConfig`** ScriptableObject (**Assets → Create → IntelliVerseX → Discord Config**), assigned either to **`IVXDiscordManager`** in the Inspector or to **`IVXBootstrapConfig`** for one-drop bootstrap.

### Option A — Bootstrap (recommended for full SDK)

1. Select your **`IVXBootstrapConfig`** asset.
2. Under **Module Configs**, assign the **`IVXDiscordConfig`** reference (the field is serialized as a `ScriptableObject`; it must reference a `IVXDiscordConfig` instance).
3. Ensure **Enable Discord** is checked so `IVXBootstrap` runs Discord initialization during `InitializeAsync()`.

### Option B — Standalone `IVXDiscordManager`

1. Add `IVXDiscordManager` (and sibling components you need) to a **persistent** `DontDestroyOnLoad` GameObject, or drop in **`IVX_DiscordManager.prefab`** from the SDK.
2. Assign the **`IVXDiscordConfig`** asset on the **`IVXDiscordManager`** component in the Inspector.

### Application ID and OAuth fields

| Config field (see module) | Purpose |
|----------------------------|---------|
| `ApplicationId` | Discord application snowflake ID |
| `ClientId` | OAuth2 client id |
| `RedirectUri` | Must match Developer Portal redirect |
| `MobileRedirectScheme` | Deep link scheme for mobile PKCE (e.g. `mygame://`) |

Optional toggles such as **`EnableVoiceChat`**, **`BridgeLobbiesToDiscord`**, **`EnableDirectMessages`**, and **`DmHistoryLimit`** are documented in the module’s `IVXDiscordConfig` table—set them before shipping builds that rely on those features.

!!! tip "Setup Wizard"
    Use **IntelliVerseX → SDK Setup Wizard** (Discord section) for package detection, **Create Discord Config**, and **Add Discord Manager** shortcuts, as described in the Discord module.

---

## Step 3 — Initialize Discord

### Manual initialization

```csharp
using IntelliVerseX.Discord;

// Assign IVXDiscordConfig on IVXDiscordManager in the Inspector, then:
IVXDiscordManager.Instance.Initialize(); // uses serialized config

// Or pass the asset explicitly:
// IVXDiscordManager.Instance.Initialize(myDiscordConfig);
```

### Account linking (after init)

```csharp
IVXDiscordManager.Instance.OnAccountLinked += (id, name) =>
{
    // Persist link state, refresh UI, etc.
};
IVXDiscordManager.Instance.LinkAccount();
```

### Bootstrap auto-initialization

If **`IVXBootstrap`** is present with **`Enable Discord`** and a valid **`IVXDiscordConfig`** on **`IVXBootstrapConfig`**, Discord initializes during **`InitializeAsync()`** (including **`Start()`** when **`Auto Initialize`** is enabled). You do not need to call **`Initialize()`** again unless you manage Discord outside bootstrap.

!!! note "Entry-point linking"
    When linking starts from Discord, handle **`OnAuthorizeRequested`** and **`RegisterAuthorizeRequestCallback`** as in the module; call **`RemoveAuthorizeRequestCallback`** when a full-screen flow would be inappropriate (match, cutscene).

---

## Step 4 — Rich Presence

Use **`IVXDiscordPresence`** for activity lines, party info, timers, URLs, buttons, and optional RPC-only desktop mode.

### Basic activity, party, and timer

```csharp
using IntelliVerseX.Discord;

var p = IVXDiscordPresence.Instance;
p.SetActivity("Ranked Match", "Score: 1,500");
p.SetParty("lobby_abc", 2, 4, joinSecret: "secret_abc");
p.StartTimer();
p.SyncFromGameState(); // reads IVX game systems when implemented
p.ClearPresence();
```

### Field URLs, assets, status, platforms, buttons

```csharp
p.SetFieldUrls(stateUrl: "https://...", detailsUrl: "https://...");
p.SetAssetUrls(largeUrl: "https://...", smallUrl: "https://...");
p.SetStatusDisplayType(IVXStatusDisplayType.Details); // Name | State | Details
p.SetSupportedPlatforms(IVXActivityPlatforms.Desktop | IVXActivityPlatforms.Mobile);
p.AddButton("Watch Trailer", "https://...");
p.ClearButtons();
p.SetInviteCoverImage("invite_cover_key");
p.SetSmallImage("badge_key", "Season 2");
```

### RPC-only mode (desktop)

```csharp
p.InitializeRPCOnly(applicationIdLong);
// IsRPCOnlyMode reflects this state
```

!!! tip "Auto Presence"
    With **`AutoPresence`** enabled on **`IVXDiscordConfig`**, presence can refresh on an interval controlled by **`PresenceUpdateInterval`** (module range **5–120 s**).

---

## Step 5 — Voice chat

Use **`IVXDiscordVoice`** after you have an active lobby context (lobby id from **`IVXDiscordLobby`**, e.g. **`CurrentLobbyId`**).

### Join, leave, auto-join, mute, deafen, volumes

```csharp
using IntelliVerseX.Discord;

var v = IVXDiscordVoice.Instance;
var lobby = IVXDiscordLobby.Instance;

// After joining a lobby (see Step 6):
v.JoinCall(lobby.CurrentLobbyId);
v.AutoJoinFromLobby();
v.SetSelfMute(true);
v.SetSelfDeafen(false);
v.SetInputVolume(0.9f);
v.SetOutputVolume(0.85f);
v.SetParticipantVolume("987654321098765432", 0.7f); // Discord user id string

v.LeaveCall();
```

### Voice Activity Detection (VAD)

```csharp
v.SetVADThreshold(useCustom: true, thresholdDb: -35f);
```

### Optional audio callbacks (FMOD / Wwise / custom)

```csharp
v.JoinCallWithAudioCallbacks(
    lobby.CurrentLobbyId,
    onReceived: (userId, data, samplesPerChannel, sampleRate, channels, ref muteFrame) => { },
    onCaptured: (data, samplesPerChannel, sampleRate, channels) => { });
```

### Global controls

```csharp
v.SetSelfMuteAll(true);
v.SetSelfDeafenAll(false);
v.EndAllCalls();
var state = v.GetParticipantVoiceState("987654321098765432");
```

Subscribe to voice-related events listed in the module (**`OnParticipantMuteChanged`**, **`OnParticipantDeafenChanged`**, speaking and participant list updates).

!!! warning "Permissions and scopes"
    Voice requires appropriate Discord permissions and approved scopes. **`EnableVoiceChat`** and **`MaxVoiceLobbySize`** on **`IVXDiscordConfig`** gate integration behavior.

---

## Step 6 — Lobbies

**`IVXDiscordLobby`** manages Discord lobbies, metadata, idle timeout, text chat, and history.

### Create, join, bridge from an IVX room

```csharp
using IntelliVerseX.Discord;

var lobby = IVXDiscordLobby.Instance;

lobby.CreateOrJoinLobby("my_session_secret", metadata: "{\"mode\":\"ranked\"}");
lobby.BridgeIVXRoom(ivxRoomId: "room_42", roomMetadata: "{\"map\":\"arena_3\"}");

lobby.CreateOrJoinLobbyWithMetadata(
    "my_session_secret",
    lobbyMetadata: "{\"region\":\"eu\"}",
    userMetadata: "{\"role\":\"captain\"}",
    onComplete: lobbyId => { /* Discord lobby id */ });
```

### Metadata and info

```csharp
lobby.UpdateLobbyMemberMetadata("{\"ready\":true}");
lobby.SetLobbyIdleTimeout(600); // seconds; default 300, max 604800 per module
lobby.GetLobbyInfo(info =>
{
    if (info == null) return;
    // info.LobbyId, Secret, MemberCount, VoiceActive, Metadata, LobbyMetadata, MemberIds
});
```

### Text chat and leave

```csharp
lobby.OnMessageReceived += (senderName, message) => { /* UI */ };
lobby.SendMessage("Hello from the game!");
lobby.FetchChatHistory(50, lines => { /* history */ });
lobby.LeaveLobby();
```

!!! note "Lobby discovery"
    The Discord module documentation emphasizes **create/join by secret** and **`BridgeIVXRoom`** for IVX multiplayer alignment. It does **not** document a public lobby search API; use **shared secrets**, **Rich Presence** join secrets, and **`IVXDiscordInvites`** to route players into the same session.

---

## Step 7 — Direct messages

**`IVXDiscordMessages`** requires **`EnableDirectMessages`** where applicable (see **`IVXDiscordConfig`**).

### Send, edit, history, summaries

```csharp
using System.Collections.Generic;
using IntelliVerseX.Discord;

var msgs = IVXDiscordMessages.Instance;

msgs.OnDMReceived += dm => { /* IVXDirectMessage */ };
msgs.OnDMUpdated += dm => { };
msgs.OnDMDeleted += messageId => { };

ulong peer = 987654321098765432UL;
msgs.SendDM(peer, "Hello!", onSuccess: id => { }, onError: err => { });
msgs.EditDM(peer, 1000123456789012345UL, newContent: "Edited text", onSuccess: () => { }, onError: err => { });
msgs.GetDMHistory(peer, limit: 50, onComplete: (List<IVXDirectMessage> list) => { });
msgs.GetDMSummaries(onComplete: summaries => { /* List<IVXDMSummary> */ });
```

### Notifications while in-game chat is focused

```csharp
msgs.SetShowingChat(true); // may suppress desktop DM notifications while focused
```

### Deep links into Discord

```csharp
msgs.OpenMessageInDiscord(1000123456789012345UL);
msgs.OpenDMSettingsInDiscord();
```

---

## Step 8 — Moderation

**`IVXDiscordModeration`** covers **metadata parsing**, **optional voice capture**, **user reports**, and **webhook-oriented** server workflows—not a separate documented IVX API for “server ban” or “server mute” by those names.

### User reports

```csharp
using IntelliVerseX.Discord;

IVXDiscordModeration.Instance.ReportUser(
    987654321098765432UL,
    reason: "Harassment",
    onComplete: ok => { });
```

### Message metadata → presentation decision

```csharp
using System.Collections.Generic;
using IntelliVerseX.Discord;

var mod = IVXDiscordModeration.Instance;
// Keys like action, reason, replacement, severity, message_id, flagged, content_flagged
// map to IVXModerationDecision / IVXModerationAction (Show, Hide, Blur, Replace)
var metadataDictionary = new Dictionary<string, string>();
mod.ProcessModerationMetadata(1000123456789012345UL, metadataDictionary);
```

### Voice capture for moderation pipelines

```csharp
var mod = IVXDiscordModeration.Instance;
mod.OnVoiceDataCaptured += (lobbyId, pcm, sampleRate, channels) => { /* forward to backend */ };
mod.StartVoiceModerationCapture(9000123456789012345UL);
mod.StopVoiceModerationCapture();
```

### Auto-moderation and webhooks

- Set **`EnableAutoModeration`** and **`ModerationWebhookUrl`** on **`IVXDiscordConfig`** for server-side pipelines (module).
- At runtime, align with **`IVXDiscordModeration.AutoModerateEnabled`** / **`EnableAutoModeration(bool)`**.

```csharp
IVXDiscordModeration.Instance.EnableAutoModeration(true);
```

!!! note "Ban, mute, and blocking"
    The [Discord module](../modules/discord.md) does **not** list **`BanUser`** / **`MuteUser`** on **`IVXDiscordModeration`**. Server-level sanctions are typically enforced in Discord or your backend (e.g. consuming **`ModerationWebhookUrl`** events). To **block** a user in the unified social layer, use **`IVXDiscordFriends.BlockUser`** (documented under **Unified Friends List** in the same module).

---

## Step 9 — Linked channels

**`IVXDiscordLinkedChannels`** bridges in-game chat to a Discord **guild** text channel.

```csharp
using IntelliVerseX.Discord;

var lobby = IVXDiscordLobby.Instance;
var ch = IVXDiscordLinkedChannels.Instance;
ch.LinkChannel(
    lobby.CurrentLobbyId,
    guildId: 1100123456789012345UL,
    channelId: 1200123456789012345UL);
ch.SendToLinkedChannel("GG!");
ch.UnlinkChannel();
```

!!! warning "Permissions"
    Linking usually requires appropriate **Manage Channel** (or equivalent) permissions on the target server.

---

## Step 10 — Social settings

### Discord client settings and profiles

```csharp
using IntelliVerseX.Discord;

IVXDiscordManager.Instance.OpenConnectedGamesSettingsInDiscord();
IVXDiscordManager.Instance.OpenUserProfileInDiscord(987654321098765432UL);
```

### DM-specific settings

```csharp
IVXDiscordMessages.Instance.OpenDMSettingsInDiscord();
```

---

## Troubleshooting

| Symptom | Likely cause | What to check |
|---------|----------------|---------------|
| **`No IVXDiscordConfig provided`** | Manager initialized without config | Assign config on **`IVXDiscordManager`** or pass into **`Initialize(config)`**; ensure **`IVXBootstrapConfig.DiscordConfig`** is set when using bootstrap |
| **Empty or wrong Application ID** | `ApplicationId` not set on asset | Open **`IVXDiscordConfig`** and paste the snowflake from the Developer Portal |
| **OAuth fails / invalid redirect** | Redirect mismatch | Portal **OAuth2 → Redirects** must match **`RedirectUri`** and mobile scheme must match **`MobileRedirectScheme`** |
| **Stub logs only** | Package missing or WebGL | Install **`com.discord.social-sdk`**; expect stubs on **WebGL** per module |
| **Voice silent or cannot connect** | Permissions, scopes, or lobby state | Confirm **`EnableVoiceChat`**, user granted voice permissions, **`JoinCall`** uses a valid **`CurrentLobbyId`**, **`MaxVoiceLobbySize`** |
| **DM APIs no-op** | Feature disabled | **`EnableDirectMessages`** on config |
| **Linked channel send fails** | Bot/app lacks guild permissions | Bot role, channel overwrites, **`LinkChannel`** IDs |

Enable **`IVXDiscordDebug`** (log level, callbacks) and **`EnableDebugLogging`** on config when diagnosing native SDK behavior.

---

## See also

- [Discord module](../modules/discord.md) — full API surface, prefabs, and platform notes
- [Discord API reference](../api/discord.md) — Full API detail for all Discord Social SDK components
- [Discord demo sample](../samples/discord-demo.md) — hands-on prefab **`IVX_DiscordSocialDemo`** / hub wiring (add sample doc when published)

---

## Quick reference — managers

| Type | Role |
|------|------|
| `IVXDiscordManager` | Init, OAuth, linking, tokens, deep links, social settings entry points |
| `IVXDiscordPresence` | Rich Presence and RPC-only mode |
| `IVXDiscordFriends` | Unified friends + relationships (including block) |
| `IVXDiscordLobby` | Lobbies, metadata, chat, timeout |
| `IVXDiscordVoice` | Voice join/mute/volumes/VAD/callbacks |
| `IVXDiscordInvites` | Invites, Ask to Join, join requests |
| `IVXDiscordLinkedChannels` | Guild channel bridge |
| `IVXDiscordMessages` | DMs |
| `IVXDiscordModeration` | Metadata, voice capture, reports |
| `IVXDiscordDebug` | Logging sinks and levels |
| `IVXDiscordConfig` | ScriptableObject configuration |

This table mirrors the Discord module’s component overview for fast navigation.
