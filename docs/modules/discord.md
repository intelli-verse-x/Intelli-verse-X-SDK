# Discord Social SDK Integration

The Discord module wraps the [Discord Social SDK](https://discord.com/developers/docs/social-sdk/overview) so IntelliVerseX games get social features that support retention, growth, and community engagement. It connects your game systems (multiplayer, live-ops, leaderboards) with Discord’s social graph.

---

## 1. Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Discord` |
| **Assembly** | `IntelliVerseX.Discord` |
| **Dependencies** | `IntelliVerseX.Core`, `IntelliVerseX.Backend`, `IntelliVerseX.GameModes` |
| **Compile define** | `INTELLIVERSEX_HAS_DISCORD` (auto-set when `com.discord.social-sdk ≥ 0.1.0` is installed) |

### Components

| Component | Responsibility |
|-----------|----------------|
| `IVXDiscordManager` | SDK init, OAuth2 and advanced account linking, tokens, social deep links |
| `IVXDiscordPresence` | Rich Presence (basic + advanced: URLs, buttons, platforms, RPC-only) |
| `IVXDiscordFriends` | Unified friends list + dual-layer relationship APIs (game + Discord) |
| `IVXDiscordLobby` | Lobbies, metadata, idle timeout, text chat, history |
| `IVXDiscordVoice` | Voice: mute/deafen, VAD, audio callbacks (FMOD/Wwise), global controls |
| `IVXDiscordInvites` | Game invites, Ask to Join, join requests |
| `IVXDiscordLinkedChannels` | Bridge in-game chat to Discord server channels |
| `IVXDiscordMessages` | Direct messages: send, edit, history, summaries, chat visibility, deep links |
| `IVXDiscordModeration` | Message metadata, webhook story, voice capture for moderation, reporting |
| `IVXDiscordDebug` | Log level, Unity + custom log sinks |
| `IVXDiscordConfig` | ScriptableObject configuration |

!!! info "Stub mode"
    If the Discord UPM package is not installed, IVX Discord types run in **stub mode**: safe no-ops or mock data with logs, so game code compiles and runs without the native SDK.

---

## 2. Setup & Configuration (`IVXDiscordConfig`)

### Prerequisites

1. [Discord Developer Portal](https://discord.com/developers/applications) application and **Application ID**
2. Rich Presence art assets (e.g. 1024×1024 PNG) under **Rich Presence → Art Assets**
3. OAuth2 redirect URIs (desktop `https://localhost`, mobile custom URL scheme, console flows as applicable)
4. For **communication** scopes (DMs, lobbies, voice): apply for production access where required

### Install the Discord Social SDK

The IntelliVerseX assembly defines `INTELLIVERSEX_HAS_DISCORD` when the package is present:

```json
{
    "versionDefines": [
        {
            "name": "com.discord.social-sdk",
            "expression": "0.1.0",
            "define": "INTELLIVERSEX_HAS_DISCORD"
        }
    ]
}
```

### Create the config asset

**Assets → Create → IntelliVerseX → Discord Config** → assign the asset to `IVXDiscordManager`.

### Recommended GameObject layout

Attach Discord components to a persistent `DontDestroyOnLoad` object (subset as needed):

```
DiscordSocial (GameObject)
├── IVXDiscordManager
├── IVXDiscordPresence
├── IVXDiscordFriends
├── IVXDiscordLobby
├── IVXDiscordVoice
├── IVXDiscordInvites
├── IVXDiscordLinkedChannels
├── IVXDiscordMessages
├── IVXDiscordModeration
└── IVXDiscordDebug
```

### `IVXDiscordConfig` fields

| Section | Field | Description |
|---------|--------|---------------|
| **Application** | `ApplicationId` | Discord application (snowflake) ID |
| **OAuth2** | `ClientId`, `RedirectUri` | OAuth client id and registered redirect URI |
| **Rich Presence** | `AutoPresence`, `PresenceUpdateInterval` | Auto-sync from game state; refresh interval (5–120 s) |
| | `LargeImageAssetKey`, `LargeImageText` | Large image key and hover text |
| | `SmallImageAssetKey`, `SmallImageText` | Small overlay image and text |
| | `InviteCoverImageKey` | Asset key for invite cover art |
| **Lobbies & Voice** | `EnableVoiceChat`, `MaxVoiceLobbySize`, `BridgeLobbiesToDiscord` | Voice enable, cap (2–25), IVX↔Discord lobby bridging |
| **Community** | `CommunityInviteUrl`, `StorePageUrl` | Default “Join Community” / “Play Now” targets |
| **Account Linking** | `MobileRedirectScheme` | Deep link scheme for mobile OAuth (e.g. `mygame://`) |
| | `PublisherId` | Publisher id for shared / cross-game auth |
| | `EnableEntryPointLinking` | Allow flows that start from Discord entry points |
| **Moderation** | `EnableAutoModeration`, `ModerationWebhookUrl` | Toggle + server webhook for moderation pipelines |
| **Direct Messages** | `EnableDirectMessages`, `DmHistoryLimit` | DM feature toggle; default history batch size (1–200) |
| **Debug** | `EnableDebugLogging` | Mirrors intent for SDK diagnostics (see `IVXDiscordDebug`) |

---

## 3. Account Linking

### States

| State | Meaning |
|-------|---------|
| **Not linked** | Game runs without Discord OAuth |
| **Provisional** | Temporary Discord-backed identity for social features without full link |
| **Fully linked** | OAuth2 completed — full graph and comms scopes as approved |

### Basic OAuth2

=== "C#"

    ```csharp
    using IntelliVerseX.Discord;

    IVXDiscordManager.Instance.Initialize(config);
    IVXDiscordManager.Instance.OnAccountLinked += (id, name) => { /* ... */ };
    IVXDiscordManager.Instance.LinkAccount();
    ```

=== "TypeScript"

    ```typescript
    const discord = IVXDiscordSocial.getInstance();
    await discord.initialize({ applicationId: '...' });
    const user = await discord.linkAccount();
    ```

### Entry-point linking from Discord

When the user starts linking from Discord, the client can request authorization:

```csharp
mgr.OnAuthorizeRequested += () => ShowLinkUi();
mgr.RegisterAuthorizeRequestCallback(() => StartLinkFlowFromDiscord());
// When inappropriate (match, cutscene):
mgr.RemoveAuthorizeRequestCallback();
```

### Mobile PKCE

Use the mobile OAuth2 + PKCE path with your app’s URL scheme (must align with `MobileRedirectScheme` / portal config):

```csharp
mgr.StartMobileOAuth2Flow("mygame://", success => { /* ... */ });
```

### Console device code

Display the code returned to the user on TV/console:

```csharp
mgr.StartConsoleOAuth2Flow(
    code => ShowDeviceCodeUi(code),
    success => { /* ... */ });
```

### Provisional accounts

```csharp
mgr.CreateProvisionalAccount(ok => { if (ok) ContinueAsGuestSocial(); });
```

Merge later when you have a backend or OAuth token:

```csharp
mgr.MergeProvisionalAccount(externalAuthToken, ok => { /* ... */ });
```

### Publisher-level linking

- Set `PublisherId` on the config and/or call `SetPublisherId(string)` at runtime for cross-game shared authentication with Discord.

### Token refresh / external browser

```csharp
mgr.UpdateToken(newBearerToken);
```

---

## 4. Rich Presence

### Basic activity

```csharp
var p = IVXDiscordPresence.Instance;
p.SetActivity("Ranked Match", "Score: 1,500");
p.SetParty("lobby_abc", 2, 4, joinSecret: "secret_abc");
p.StartTimer();
p.SyncFromGameState(); // reads IVX game systems when implemented
p.ClearPresence();
```

### Field URLs & asset URLs

Clickable links on state/details lines and large/small images:

```csharp
p.SetFieldUrls(stateUrl: "https://...", detailsUrl: "https://...");
p.SetAssetUrls(largeUrl: "https://...", smallUrl: "https://...");
```

### Status display type & platforms

```csharp
p.SetStatusDisplayType(IVXStatusDisplayType.Details); // Name | State | Details
p.SetSupportedPlatforms(IVXActivityPlatforms.Desktop | IVXActivityPlatforms.Mobile);
```

### Buttons & invite cover

```csharp
p.AddButton("Watch Trailer", "https://...");
p.ClearButtons();
p.SetInviteCoverImage("invite_cover_key"); // also config default via `InviteCoverImageKey`
p.SetSmallImage("badge_key", "Season 2");
```

### RPC-only mode

Desktop-only path: presence updates **without** full `Connect()` / manager auth:

```csharp
p.InitializeRPCOnly(applicationIdLong);
```

`IsRPCOnlyMode` reflects this state.

!!! tip "Auto Presence"
    With `AutoPresence` enabled, `IVXDiscordPresence` periodically calls into your push path using `PresenceUpdateInterval`.

---

## 5. Unified Friends List & Relationship Management

`IVXUnifiedFriend` includes `DiscordRelationshipType` and `GameRelationshipType` (`IVXRelationshipType`: None, Friend, PendingIncoming, PendingOutgoing, Blocked) — **dual relationship model** for Discord vs Nakama layers.

### Refresh & filters

```csharp
var f = IVXDiscordFriends.Instance;
f.OnFriendsUpdated += list => { /* bind UI */ };
f.Refresh();
var ingame = f.GetInGameFriends();
var discordOnly = f.GetBySource(IVXFriendSource.Discord);
```

### Game (Nakama) friend requests

`SendGameFriendRequest`, `SendGameFriendRequestById`, `AcceptGameFriendRequest`, `RejectGameFriendRequest`, `CancelGameFriendRequest`, `RemoveGameFriend`.

### Discord friend requests

`SendDiscordFriendRequest`, `SendDiscordFriendRequestById`, `AcceptDiscordFriendRequest`, `RejectDiscordFriendRequest`, `CancelDiscordFriendRequest`, `RemoveDiscordAndGameFriend`.

### Block / unblock

`BlockUser`, `UnblockUser` — plus `GetBlockedUsers()` and `GetPendingRequests()` for UI lists.

### Events

`OnFriendRequestReceived`, `OnFriendRequestAccepted`, `OnFriendRemoved`, `OnUserBlocked`, `OnUserUnblocked`, plus online/in-game presence events.

---

## 6. Direct Messages (`IVXDiscordMessages`)

Requires `EnableDirectMessages` where applicable.

| Capability | API |
|------------|-----|
| Send | `SendDM(recipientId, message, onSuccess, onError)` |
| Edit | `EditDM(recipientId, messageId, newContent, ...)` |
| History | `GetDMHistory(recipientId, limit, onComplete)` |
| Summaries | `GetDMSummaries(onComplete)` |
| Chat visibility / notifications | `SetShowingChat(bool)` — suppress desktop notifications while in-game chat is focused |
| Open in Discord | `OpenMessageInDiscord(messageId)` |
| DM-related settings | `OpenDMSettingsInDiscord()` |

Models: `IVXDirectMessage` (incl. `ModerationMetadata`), `IVXDMSummary`. Events: `OnDMReceived`, `OnDMUpdated`, `OnDMDeleted`.

Default history limit is aligned with `IVXDiscordConfig.DmHistoryLimit`.

---

## 7. Lobbies (`IVXDiscordLobby`)

| Capability | API |
|------------|-----|
| Create / join | `CreateOrJoinLobby(secret, metadata)`, `BridgeIVXRoom(ivxRoomId, roomMetadata)` |
| Metadata | `CreateOrJoinLobbyWithMetadata(secret, lobbyMetadata, userMetadata, onComplete)`, `UpdateLobbyMemberMetadata(json)`, `GetLobbyInfo` → `IVXDiscordLobbyInfo` |
| Idle timeout | `SetLobbyIdleTimeout(seconds)` — default 300, max 604800 |
| Chat | `SendMessage`, `OnMessageReceived`, `FetchChatHistory(limit, callback)` |
| Leave | `LeaveLobby` |

`IVXDiscordLobbyInfo` exposes `LobbyId`, `Secret`, `MemberCount`, `VoiceActive`, `Metadata`, `LobbyMetadata`, `MemberIds`.

---

## 8. Voice Chat (`IVXDiscordVoice`)

### Core

`JoinCall(lobbyId)`, `LeaveCall`, `AutoJoinFromLobby()`, `SetSelfMute`, `SetSelfDeafen`, `SetInputVolume` / `SetOutputVolume` / `SetParticipantVolume`.

### VAD

```csharp
v.SetVADThreshold(useCustom: true, thresholdDb: -35f);
```

### Audio callbacks (FMOD / Wwise / custom)

```csharp
v.JoinCallWithAudioCallbacks(
    lobbyId,
    onReceived: (userId, data, samplesPerChannel, sampleRate, channels, ref muteFrame) => { },
    onCaptured: (data, samplesPerChannel, sampleRate, channels) => { });
```

Delegates: `IVXAudioReceivedCallback`, `IVXAudioCapturedCallback`.

### Global controls & state

`SetSelfMuteAll`, `SetSelfDeafenAll`, `EndAllCalls`, `GetParticipantVoiceState(userId)`.

Events include `OnParticipantMuteChanged`, `OnParticipantDeafenChanged`, speaking and participant list updates.

---

## 9. Game Invites (`IVXDiscordInvites`)

```csharp
var inv = IVXDiscordInvites.Instance;
inv.OnInviteReceived += invite => { /* show UI */ };
inv.OnJoinRequested += (requesterId, name) => { /* Ask to Join */ };
inv.OnInviteAccepted += joinSecret => { IVXDiscordLobby.Instance.CreateOrJoinLobby(joinSecret); };
inv.SendInvite(discordUserId, "Join my lobby!");
inv.AcceptInvite(invite);
inv.DeclineInvite(invite);
inv.ApproveJoinRequest(requesterId);
inv.DenyJoinRequest(requesterId);
inv.RegisterCallbacks();
```

Rich Presence `joinSecret` + lobby bridging ties invites to sessions.

---

## 10. Linked Channels (`IVXDiscordLinkedChannels`)

Bridge IVX/world chat to a Discord text channel (bidirectional where supported):

```csharp
var ch = IVXDiscordLinkedChannels.Instance;
ch.LinkChannel(lobbyId, guildId, channelId);
ch.SendToLinkedChannel("GG!");
ch.UnlinkChannel();
```

!!! warning "Permissions"
    Linking typically requires appropriate **Manage Channel** (or equivalent) permissions in the target server.

---

## 11. Moderation (`IVXDiscordModeration`)

| Area | Behavior |
|------|----------|
| **Message metadata** | `ProcessModerationMetadata(messageId, metadata)` parses keys like `action`, `reason`, `replacement`, `severity`, `message_id`, `flagged`, `content_flagged` into `IVXModerationDecision` (`IVXModerationAction`: Show, Hide, Blur, Replace). |
| **Server-side webhook** | Configure `ModerationWebhookUrl` + your backend to fan out alerts / ML scoring. |
| **Voice capture** | `StartVoiceModerationCapture(lobbyId)` / `StopVoiceModerationCapture()` with `OnVoiceDataCaptured(lobbyId, pcm, sampleRate, channels)`. |
| **Reporting** | `ReportUser(userId, reason, onComplete)`. |

`EnableAutoModeration` on config aligns with `IVXDiscordModeration.AutoModerateEnabled` / `EnableAutoModeration(bool)`.

---

## 12. Debug & Logging (`IVXDiscordDebug`)

- Enum `IVXDiscordLogLevel`: Verbose, Debug, Info, Warning, Error
- `SetLogLevel`, `SetLogCallback` for file or analytics sinks
- `OnLogMessage` for each filtered line
- `IVXDiscordConfig.EnableDebugLogging` complements native SDK verbosity

---

## 13. Social Settings (`IVXDiscordManager`)

- `OpenConnectedGamesSettingsInDiscord()` — Connected Games / DM-related settings
- `OpenUserProfileInDiscord(userId)` — jump to a user profile in the Discord client

DM-specific entry: `IVXDiscordMessages.OpenDMSettingsInDiscord()`.

---

## 14. API Reference — Key Classes

| Class | Role |
|-------|------|
| `IVXDiscordConfig` | All Discord integration settings |
| `IVXDiscordManager` | Init, link/unlink, provisional, PKCE, device code, publisher id, merge, token, settings |
| `IVXDiscordPresence` | Rich Presence + advanced presence + RPC-only |
| `IVXDiscordFriends` | Unified list + relationship + block APIs |
| `IVXDiscordLobby` | Lobbies, metadata, timeout, chat, history |
| `IVXDiscordVoice` | Voice, VAD, callbacks, global controls |
| `IVXDiscordInvites` | Invites and join requests |
| `IVXDiscordLinkedChannels` | Server channel bridge |
| `IVXDiscordMessages` | DMs |
| `IVXDiscordModeration` | Metadata, voice capture, reports |
| `IVXDiscordDebug` | Logging |
| `IVXUnifiedFriend`, `IVXGameInvite`, `IVXDiscordLobbyInfo`, `IVXDirectMessage`, `IVXDMSummary` | Data models |

---

## Integration with IntelliVerseX

| IVX source | Typical Discord action |
|------------|-------------------------|
| `IVXGameModeManager` / matchmaking | Presence lines, state |
| `IVXLobbyManager` | `BridgeIVXRoom`, party, lobby + voice |
| Leaderboards / Hiro | Presence for rank and live-ops |
| `IVXAISessionManager` | Optional presence: “Chatting with AI host” |

---

## Platform notes

Native Discord Social SDK does not target **WebGL**; IVX falls back to stub mode. Console/desktop/mobile follow Discord’s platform matrix and your program agreements.

---

## Related links

- [Social module](social.md) — Nakama friends
- [Backend module](backend.md)
- [Discord Social SDK overview](https://discord.com/developers/docs/social-sdk/overview)
