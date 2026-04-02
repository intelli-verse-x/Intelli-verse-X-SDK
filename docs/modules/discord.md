# Discord Social SDK Integration

The Discord module wraps the [Discord Social SDK](https://discord.com/developers/docs/social-sdk/overview) to add social features that drive player retention, organic growth, and community engagement. It bridges IntelliVerseX game systems (multiplayer, live-ops, leaderboards) with Discord's 200M+ monthly active user social graph.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Discord` |
| **Assembly** | `IntelliVerseX.Discord` |
| **Dependencies** | `IntelliVerseX.Core`, `IntelliVerseX.Backend`, `IntelliVerseX.GameModes` |
| **Compile define** | `INTELLIVERSEX_HAS_DISCORD` (auto-set when `com.discord.social-sdk ≥ 0.1.0` is installed) |

### Components

| Component | Responsibility |
|-----------|---------------|
| `IVXDiscordManager` | SDK initialization, OAuth2 account linking, connection lifecycle |
| `IVXDiscordPresence` | Rich Presence — activity, party, timer, leaderboard rank |
| `IVXDiscordFriends` | Unified friends list (Discord + Nakama merge) |
| `IVXDiscordLobby` | Discord lobby text chat, IVX room bridging |
| `IVXDiscordVoice` | Voice calls — mute, deafen, per-participant volume |
| `IVXDiscordInvites` | Send/receive game invites, "Ask to Join" flow |
| `IVXDiscordLinkedChannels` | Bridge in-game chat to Discord server text channels |
| `IVXDiscordConfig` | ScriptableObject holding all configuration |

---

## Why Discord Integration Matters

| Capability | Impact |
|------------|--------|
| **Rich Presence** | Every player becomes organic marketing — activity is visible to all Discord friends |
| **Game Invites** | Viral re-engagement loops; one-click join from Discord |
| **Voice Chat** | Session length increases ~83% when players communicate in-game |
| **Linked Channels** | Community stays active 24/7 — players chat in Discord even when not in-game |
| **Unified Friends** | Single friends list across Discord + Nakama reduces social friction |

---

## Setup

### Prerequisites

1. A [Discord Developer Portal](https://discord.com/developers/applications) account
2. An **Application** created with its **Application ID**
3. Rich Presence art assets uploaded (1024 × 1024 PNG) under **Rich Presence → Art Assets**
4. OAuth2 redirect URI configured (e.g. `https://localhost` for desktop, custom scheme for mobile)
5. For **communication features** (voice, lobbies, linked channels): apply for increased rate limits via the Developer Portal

### Unity Installation

#### 1. Install the Discord Social SDK package

Add the Discord Social SDK UPM package to your project. The IntelliVerseX assembly definition automatically defines `INTELLIVERSEX_HAS_DISCORD` when `com.discord.social-sdk ≥ 0.1.0` is detected:

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

!!! info "Stub Mode"
    If the Discord package is not installed, all IVX Discord components run in **stub mode** — calls succeed with mock data and log messages. This lets you develop and test game logic without the Discord SDK present.

#### 2. Create the configuration asset

**Assets → Create → IntelliVerseX → Discord Config**

This creates an `IVXDiscordConfig` ScriptableObject. Fill in:

- **Application ID** — from the Developer Portal
- **Client ID** — usually the same as Application ID
- **Redirect URI** — must match the Developer Portal entry
- **Large Image Asset Key** — the key you uploaded for Rich Presence
- **Community Invite URL** — your Discord server invite link
- **Store Page URL** — your game's store or download page

#### 3. Add components to a persistent GameObject

Attach all seven components to a single GameObject marked `DontDestroyOnLoad`:

```
DiscordSocial (GameObject)
├── IVXDiscordManager      (handles init + auth)
├── IVXDiscordPresence     (Rich Presence)
├── IVXDiscordFriends      (unified friends list)
├── IVXDiscordLobby        (lobbies + text chat)
├── IVXDiscordVoice        (voice calls)
├── IVXDiscordInvites      (game invites)
└── IVXDiscordLinkedChannels (channel bridging)
```

Assign the `IVXDiscordConfig` asset to `IVXDiscordManager`'s config field in the Inspector.

### Other Platforms

| Platform | Import |
|----------|--------|
| **JavaScript / TypeScript** | `import { IVXDiscordSocial } from '@intelliversex/sdk'` |
| **Java / Android** | `com.intelliversex.sdk.discord.IVXDiscordSocial` |
| **Flutter / Dart** | `import 'package:ivx_discord_social/ivx_discord_social.dart'` |
| **Unreal Engine 5** | `UIVXDiscordSocial` (Blueprint-callable) |
| **Godot 4** | `IVXDiscordSocial` autoload |
| **C++ (native)** | `ivx::DiscordSocial::instance()` |
| **Cocos2d-x** | `IntelliVerseX::IVXDiscordSocial::getInstance()` |
| **Defold** | `require("intelliversex.ivx_discord")` |

---

## Features

### 1. Account Linking

OAuth2 authentication with Discord. Supports three account states:

| State | Description |
|-------|-------------|
| **Not linked** | Player uses the game without Discord |
| **Provisional** | Non-Discord user gets a temporary Discord identity for social features |
| **Fully linked** | Player authorized via OAuth2 — full Discord social graph available |

#### Unity (C#)

```csharp
using IntelliVerseX.Discord;

public class DiscordAuth : MonoBehaviour
{
    void Start()
    {
        var mgr = IVXDiscordManager.Instance;
        mgr.OnAccountLinked += HandleLinked;
        mgr.OnError += HandleError;

        mgr.Initialize();
    }

    public void OnLinkButtonPressed()
    {
        IVXDiscordManager.Instance.LinkAccount();
    }

    public void OnPlayWithoutDiscord()
    {
        IVXDiscordManager.Instance.CreateProvisionalAccount(success =>
        {
            if (success) LoadMainMenu();
        });
    }

    private void HandleLinked(string userId, string username)
    {
        Debug.Log($"Linked as {username} ({userId})");
        LoadMainMenu();
    }

    private void HandleError(string error)
    {
        ShowErrorDialog(error);
    }
}
```

#### TypeScript

```typescript
import { IVXDiscordSocial } from '@intelliversex/sdk';

const discord = IVXDiscordSocial.getInstance();

await discord.initialize({ applicationId: '123456789012345678' });

// Full OAuth2 link
const user = await discord.linkAccount();
console.log(`Linked as ${user.username}`);

// Provisional (no Discord account required)
await discord.createProvisionalAccount();
```

#### Java (Android)

```java
import com.intelliversex.sdk.discord.IVXDiscordSocial;

IVXDiscordSocial discord = IVXDiscordSocial.getInstance();
discord.initialize(applicationId, this);

discord.setOnAccountLinked((userId, username) -> {
    Log.d("Discord", "Linked as " + username);
});

discord.linkAccount();
```

---

### 2. Rich Presence

Auto-updates the player's Discord profile with game activity. Every Discord friend sees what your player is doing — free organic marketing.

#### What gets displayed

| Rich Presence Field | Source |
|----|-----|
| **Details** (line 1) | Game mode & match status (from `IVXGameModeManager`) |
| **State** (line 2) | Score, queue status, or live-ops event |
| **Party** | Lobby size & "Join" button (from `IVXLobbyManager`) |
| **Timestamps** | Elapsed play time |
| **Large image** | Game logo (configured in `IVXDiscordConfig`) |
| **Button 1** | "Play Now" → store page URL |
| **Button 2** | "Join Community" → Discord server invite |

#### Unity (C#)

```csharp
var presence = IVXDiscordPresence.Instance;

// Basic activity
presence.SetActivity("Ranked Match on Arena", "Score: 1,500");

// Party info (enables "Ask to Join" on Discord)
presence.SetParty(
    partyId: "lobby_abc123",
    currentSize: 2,
    maxSize: 4,
    joinSecret: "secret_abc123"
);

// Leaderboard rank
presence.SetLeaderboardRank("Global", rank: 42, score: 98_500);

// Live-ops events (Hiro integration)
presence.SetLiveOpsEvent("Day 15 Streak", "Won Legendary Chest!");

// Start elapsed timer
presence.StartTimer();

// Auto-sync from all IVX systems at once
presence.SyncFromGameState();

// Clear everything
presence.ClearPresence();
```

#### TypeScript

```typescript
const discord = IVXDiscordSocial.getInstance();

discord.setActivity({
    details: 'Ranked Match on Arena',
    state: 'Score: 1,500',
    party: { id: 'lobby_abc123', size: 2, max: 4 },
    timestamps: { start: Date.now() },
    buttons: [
        { label: 'Play Now', url: 'https://store.example.com/mygame' },
        { label: 'Join Community', url: 'https://discord.gg/mygame' },
    ],
});
```

!!! tip "Auto-Presence"
    When `IVXDiscordConfig.AutoPresence` is enabled (default), `IVXDiscordPresence` polls game state every `PresenceUpdateInterval` seconds (default 15) and pushes updates to Discord automatically. You only need to call `SetActivity` for manual overrides.

---

### 3. Unified Friends List

Merges Discord relationships with Nakama in-game friends into a single view.

#### `IVXUnifiedFriend` Data Model

| Field | Type | Description |
|-------|------|-------------|
| `Source` | `IVXFriendSource` | `Discord`, `Game`, or `Both` |
| `DisplayName` | `string` | Preferred display name |
| `DiscordUserId` | `string` | Discord ID (null if game-only) |
| `GameUserId` | `string` | Nakama ID (null if Discord-only) |
| `AvatarUrl` | `string` | Avatar URL (prefers Discord avatar) |
| `IsOnline` | `bool` | Online on any game or Discord |
| `IsInGame` | `bool` | Currently playing this game |
| `ActivityText` | `string` | Rich Presence activity text |
| `CanInvite` | `bool` | Whether this friend can be invited |

#### Unity (C#)

```csharp
var friends = IVXDiscordFriends.Instance;

friends.OnFriendsUpdated += list =>
{
    foreach (var f in list)
    {
        Debug.Log($"{f.DisplayName} [{f.Source}] " +
                  $"Online={f.IsOnline} InGame={f.IsInGame}");
    }
};

friends.OnFriendJoinedGame += friend =>
{
    ShowNotification($"{friend.DisplayName} is now playing!");
};

// Fetch and merge Discord + Nakama friends
friends.Refresh();

// Filter helpers
var inGame = friends.GetInGameFriends();
var discordOnly = friends.GetBySource(IVXFriendSource.Discord);

// Quick counts
Debug.Log($"Online: {friends.OnlineCount}, In-game: {friends.InGameCount}");
```

#### TypeScript

```typescript
const discord = IVXDiscordSocial.getInstance();
const friends = await discord.getUnifiedFriends();

const inGame = friends.filter(f => f.isInGame);
console.log(`${inGame.length} friends currently playing`);
```

---

### 4. Discord Lobbies

Bridges IVX rooms to Discord lobbies, providing text chat and serving as the foundation for voice and invites.

#### Unity (C#)

```csharp
var lobby = IVXDiscordLobby.Instance;

lobby.OnLobbyJoined += lobbyId =>
{
    Debug.Log($"Joined Discord lobby {lobbyId}");
};

lobby.OnMessageReceived += (sender, message) =>
{
    AppendToChatUI($"{sender}: {message}");
};

// Auto-bridge from an IVX room
lobby.BridgeIVXRoom("room_abc123", "{\"gameMode\":\"ranked\"}");

// Manual create/join
lobby.CreateOrJoinLobby("my_lobby_secret");

// Send text chat
lobby.SendMessage("Good game!");

// Fetch history
lobby.FetchChatHistory(limit: 100, messages =>
{
    foreach (var msg in messages)
        AppendToChatUI(msg);
});

// Leave
lobby.LeaveLobby();
```

!!! note "Auto-Bridging"
    When `IVXDiscordConfig.BridgeLobbiesToDiscord` is `true` (default), call `BridgeIVXRoom()` from your `IVXLobbyManager.OnRoomCreated` handler. The lobby secret is automatically prefixed with `ivx_` to avoid collisions.

---

### 5. Voice Chat

Discord-powered voice communication within game lobbies. No need to build your own voice infrastructure.

#### Unity (C#)

```csharp
var voice = IVXDiscordVoice.Instance;

voice.OnCallJoined += () => ShowVoiceUI();
voice.OnCallLeft += () => HideVoiceUI();

voice.OnParticipantSpeaking += userId =>
{
    HighlightSpeaker(userId);
};

voice.OnParticipantsChanged += participants =>
{
    RefreshVoiceParticipantList(participants);
};

// Join voice when entering a lobby
voice.AutoJoinFromLobby();

// Manual join
voice.JoinCall(lobbyId);

// Mute / deafen
voice.SetSelfMute(true);
voice.SetSelfDeafen(true);

// Volume controls (0–200, 100 = normal)
voice.SetInputVolume(120f);   // mic boost
voice.SetOutputVolume(80f);   // lower speakers
voice.SetParticipantVolume("user_123", 50f);  // quiet one person

// Leave
voice.LeaveCall();
```

#### `IVXVoiceParticipant` Data Model

| Field | Type | Description |
|-------|------|-------------|
| `UserId` | `string` | Discord user ID |
| `DisplayName` | `string` | Display name |
| `IsMuted` | `bool` | Self-muted |
| `IsDeafened` | `bool` | Self-deafened |
| `IsSpeaking` | `bool` | Currently transmitting audio |
| `Volume` | `float` | Per-participant volume (0–200) |

---

### 6. Game Invites

Send and receive game invites through Discord. Supports both direct invites and "Ask to Join" from Discord profiles.

#### Invite Flow

```
┌─────────────┐     SendInvite()     ┌──────────────┐
│  Player A    │ ──────────────────▶  │  Player B    │
│  (in-game)   │                      │  (on Discord)│
└─────────────┘                      └──────┬───────┘
                                            │
                                     Clicks "Accept"
                                            │
                                            ▼
                                  OnInviteAccepted(joinSecret)
                                            │
                                    Auto-join lobby
                                    + voice chat
```

#### Unity (C#)

```csharp
var invites = IVXDiscordInvites.Instance;

// Handle incoming invites
invites.OnInviteReceived += invite =>
{
    ShowInviteDialog(
        $"{invite.InviterName} invited you!",
        $"{invite.ActivityDetails} ({invite.PartyCurrentSize}/{invite.PartyMaxSize})",
        onAccept: () => invites.AcceptInvite(invite),
        onDecline: () => invites.DeclineInvite(invite)
    );
};

// Handle "Ask to Join" requests
invites.OnJoinRequested += (requesterId, requesterName) =>
{
    ShowJoinRequestDialog(
        $"{requesterName} wants to join your game",
        onApprove: () => invites.ApproveJoinRequest(requesterId),
        onDeny: () => invites.DenyJoinRequest(requesterId)
    );
};

// Handle accepted invite (transition to session)
invites.OnInviteAccepted += joinSecret =>
{
    IVXDiscordLobby.Instance.CreateOrJoinLobby(joinSecret);
    IVXDiscordVoice.Instance.AutoJoinFromLobby();
};

// Send an invite to a friend
invites.SendInvite("discord_user_id_456", "Come play ranked!");

// Register callbacks (called automatically, but can be re-registered)
invites.RegisterCallbacks();
```

#### `IVXGameInvite` Data Model

| Field | Type | Description |
|-------|------|-------------|
| `InviterUserId` | `string` | Discord user ID of the inviter |
| `InviterName` | `string` | Display name |
| `InviterAvatarUrl` | `string` | Avatar image URL |
| `JoinSecret` | `string` | Secret token to join the session |
| `ActivityDetails` | `string` | What the inviter is doing |
| `PartyCurrentSize` | `int` | Current party member count |
| `PartyMaxSize` | `int` | Maximum party capacity |

---

### 7. Linked Channels

Bridge in-game chat (clan chat, world chat, etc.) to a Discord server text channel. Messages flow bidirectionally — players can participate in game chat from Discord even when not in-game.

#### Unity (C#)

```csharp
var channels = IVXDiscordLinkedChannels.Instance;

channels.OnChannelLinked += (lobbyId, channelId) =>
{
    Debug.Log($"Lobby {lobbyId} linked to Discord channel {channelId}");
};

channels.OnLinkedMessageReceived += (sender, message) =>
{
    AppendToWorldChat($"[Discord] {sender}: {message}");
};

// Link a lobby to a Discord channel
// Requires the player to have Manage Channel permission in the server
channels.LinkChannel(
    lobbyId: IVXDiscordLobby.Instance.CurrentLobbyId,
    guildId: 123456789012345678,
    channelId: 987654321098765432
);

// Send a message from in-game to the linked Discord channel
channels.SendToLinkedChannel("GG everyone!");

// Unlink
channels.UnlinkChannel();
```

!!! warning "Permission Required"
    Linking a channel requires the player to have **Manage Channel** permission in the target Discord server. This is typically used by clan leaders or server admins.

---

## Integration with IntelliVerseX Systems

The Discord module listens to events across the IVX ecosystem and translates them into Discord actions automatically:

| IVX Event | Discord Action | Component |
|-----------|---------------|-----------|
| `IVXGameModeManager.OnModeChanged` | Presence: "Playing Ranked 2v2" | `IVXDiscordPresence` |
| `IVXMatchmakingManager.OnSearching` | Presence: "Searching for match…" | `IVXDiscordPresence` |
| `IVXLobbyManager.OnRoomCreated` | Auto-create Discord lobby + set party | `IVXDiscordLobby` + `IVXDiscordPresence` |
| `IVXLobbyManager.OnRoomJoined` | Auto-join Discord lobby + voice | `IVXDiscordLobby` + `IVXDiscordVoice` |
| `IVXLobbyManager.OnRoomLeft` | Leave Discord lobby + voice | `IVXDiscordLobby` + `IVXDiscordVoice` |
| `IVXHiro.Streaks.OnUpdate` | Presence: "🔥 Day 15 Streak" | `IVXDiscordPresence` |
| `IVXHiro.SpinWheel.OnSpin` | Presence: "Won Legendary Chest!" | `IVXDiscordPresence` |
| `IVXLeaderboard.OnScoreSubmitted` | Presence: "Rank #42 on Global" | `IVXDiscordPresence` |
| `IVXAISession.OnVoiceStarted` | Presence: "Chatting with AI Host" | `IVXDiscordPresence` |
| `IVXFriendsManager.OnFriendAdded` | Merged into unified friends list | `IVXDiscordFriends` |

### Wiring Example

```csharp
public class DiscordIntegrationWiring : MonoBehaviour
{
    void Start()
    {
        // Auto-bridge lobbies
        IVXLobbyManager.Instance.OnRoomCreated += roomId =>
        {
            IVXDiscordLobby.Instance.BridgeIVXRoom(roomId);
            IVXDiscordPresence.Instance.SetParty(
                roomId,
                currentSize: 1,
                maxSize: IVXLobbyManager.Instance.MaxPlayers,
                joinSecret: roomId
            );
        };

        // Auto-join voice on lobby entry
        IVXDiscordLobby.Instance.OnLobbyJoined += _ =>
        {
            IVXDiscordVoice.Instance.AutoJoinFromLobby();
        };

        // Clean up on lobby exit
        IVXLobbyManager.Instance.OnRoomLeft += () =>
        {
            IVXDiscordPresence.Instance.ClearParty();
        };

        // Leaderboard rank updates
        IVXLeaderboard.Instance.OnScoreSubmitted += (board, rank, score) =>
        {
            IVXDiscordPresence.Instance.SetLeaderboardRank(board, rank, score);
        };
    }
}
```

---

## Configuration Reference

All fields on the `IVXDiscordConfig` ScriptableObject:

### Application

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `ApplicationId` | `long` | — | Discord Application ID from the Developer Portal |

### OAuth2

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `ClientId` | `string` | — | OAuth2 Client ID (same as Application ID for most games) |
| `RedirectUri` | `string` | `https://localhost` | OAuth2 redirect URI registered in the Developer Portal |

### Rich Presence

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `AutoPresence` | `bool` | `true` | Auto-update Rich Presence from IVX game state |
| `PresenceUpdateInterval` | `float` | `15` | Seconds between Rich Presence refreshes (5–120) |
| `LargeImageAssetKey` | `string` | `game_logo` | Asset key uploaded to the Developer Portal |
| `LargeImageText` | `string` | `""` | Tooltip text on hover over the large image |

### Lobbies & Voice

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `EnableVoiceChat` | `bool` | `true` | Enable Discord voice chat in multiplayer lobbies |
| `MaxVoiceLobbySize` | `int` | `8` | Max voice participants per lobby (2–25) |
| `BridgeLobbiesToDiscord` | `bool` | `true` | Auto-bridge IVX rooms to Discord lobbies |

### Community

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `CommunityInviteUrl` | `string` | `""` | Discord server invite URL for the "Join Community" button |
| `StorePageUrl` | `string` | `""` | Store/download URL for the "Play Now" button |

---

## Events Reference

### IVXDiscordManager

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnConnected` | `Action` | Discord client connected |
| `OnDisconnected` | `Action` | Discord client disconnected |
| `OnAccountLinked` | `Action<string, string>` | Account linked — `(userId, username)` |
| `OnAccountUnlinked` | `Action` | Account unlinked |
| `OnError` | `Action<string>` | SDK error — `(errorMessage)` |

### IVXDiscordPresence

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnPresenceUpdated` | `Action` | Rich Presence was pushed to Discord |

### IVXDiscordFriends

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnFriendsUpdated` | `Action<IReadOnlyList<IVXUnifiedFriend>>` | Friends list refreshed |
| `OnFriendOnline` | `Action<IVXUnifiedFriend>` | A friend came online |
| `OnFriendOffline` | `Action<IVXUnifiedFriend>` | A friend went offline |
| `OnFriendJoinedGame` | `Action<IVXUnifiedFriend>` | A friend started playing this game |

### IVXDiscordLobby

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnLobbyJoined` | `Action<ulong>` | Joined a lobby — `(lobbyId)` |
| `OnLobbyLeft` | `Action` | Left the current lobby |
| `OnMessageReceived` | `Action<string, string>` | Chat message — `(senderName, message)` |
| `OnMemberJoined` | `Action<string>` | Member joined lobby — `(userId)` |
| `OnMemberLeft` | `Action<string>` | Member left lobby — `(userId)` |

### IVXDiscordVoice

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnCallJoined` | `Action` | Joined a voice call |
| `OnCallLeft` | `Action` | Left a voice call |
| `OnParticipantSpeaking` | `Action<string>` | Participant is speaking — `(userId)` |
| `OnParticipantsChanged` | `Action<IReadOnlyList<IVXVoiceParticipant>>` | Participant list changed |

### IVXDiscordInvites

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnInviteReceived` | `Action<IVXGameInvite>` | Incoming game invite |
| `OnJoinRequested` | `Action<string, string>` | "Ask to Join" request — `(requesterId, requesterName)` |
| `OnInviteAccepted` | `Action<string>` | Invite accepted — `(joinSecret)` |
| `OnInviteSent` | `Action<string>` | Invite sent — `(targetUserId)` |

### IVXDiscordLinkedChannels

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnChannelLinked` | `Action<ulong, ulong>` | Channel linked — `(lobbyId, channelId)` |
| `OnChannelUnlinked` | `Action` | Channel unlinked |
| `OnLinkedMessageReceived` | `Action<string, string>` | Message from Discord — `(senderName, message)` |

---

## Platform Compatibility

Platform support follows the Discord Social SDK's native availability:

| Platform | C++ | Unity | Unreal |
|----------|-----|-------|--------|
| Windows | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| macOS | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Linux | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Android | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| iOS | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Xbox | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| PlayStation | :white_check_mark: | :white_check_mark: | :white_check_mark: |

!!! info "Console Platforms"
    Xbox and PlayStation support requires additional agreements with Discord and the respective platform holders. Contact your Discord developer relations representative.

### Feature Availability by Platform

| Feature | Desktop | Mobile | Console | WebGL |
|---------|---------|--------|---------|-------|
| Rich Presence | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Account Linking | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Friends List | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Lobbies + Text Chat | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Voice Chat | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Game Invites | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Linked Channels | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |

!!! warning "WebGL"
    The Discord Social SDK is a native library and does not support WebGL builds. Discord features are unavailable on WebGL — the IVX module gracefully degrades to stub mode.

---

## Rate Limits

The Discord Social SDK separates features into two scope tiers:

### Presence Scopes (default — no application required)

| Scope | Features | Rate Limit |
|-------|----------|-----------|
| `rp.read` | Read Rich Presence of relationships | — |
| `rp.write` | Update own Rich Presence | 5 updates / 20 seconds |

### Communication Scopes (requires application)

| Scope | Features | Rate Limit |
|-------|----------|-----------|
| `dm.read` | Read lobby / DM messages | Standard API limits |
| `dm.write` | Send lobby / DM messages | 5 messages / 5 seconds per channel |
| `voice.read` | Receive voice audio | — |
| `voice.write` | Transmit voice audio | — |
| `lobby.read` | Read lobby state | — |
| `lobby.write` | Create/modify lobbies | — |

!!! note "Production Access"
    Communication scopes (`dm.*`, `voice.*`, `lobby.*`) require applying for production access in the **Discord Developer Portal → Social SDK → Communication Features**. During development, these work in test mode with up to 25 users.

---

## Best Practices

### 1. Always set Rich Presence

Rich Presence is free organic marketing. Every player's Discord profile advertises your game to their entire friend list. Set it on every scene transition.

### 2. Include both action buttons

Configure `StorePageUrl` and `CommunityInviteUrl` in `IVXDiscordConfig`. These appear as "Play Now" and "Join Community" buttons under the player's presence — direct conversion from impressions.

### 3. Use provisional accounts

Not every player has Discord. `CreateProvisionalAccount()` gives non-Discord users access to social features without requiring a Discord account, then gently prompts them to link later for the full experience.

### 4. Auto-bridge lobbies

Wire `IVXLobbyManager.OnRoomCreated` to `IVXDiscordLobby.BridgeIVXRoom()`. This gives every multiplayer session free text + voice chat with zero additional infrastructure.

### 5. Sync party info for "Ask to Join"

Always call `SetParty()` with a `joinSecret` when the player is in a lobby. This enables the "Ask to Join" button on their Discord profile — a powerful organic re-engagement vector.

### 6. Handle offline invite acceptance (deep links)

When a player accepts a Discord invite while the game is not running, the game launches with the `joinSecret` as a deep link parameter. Handle this in your startup flow:

```csharp
void Start()
{
    string joinSecret = GetDeepLinkJoinSecret();
    if (!string.IsNullOrEmpty(joinSecret))
    {
        IVXDiscordManager.Instance.Initialize();
        IVXDiscordLobby.Instance.CreateOrJoinLobby(joinSecret);
    }
}
```

### 7. Never store Discord friend data locally

Discord friend data is ephemeral and must be fetched fresh. Do not persist `IVXUnifiedFriend` objects to disk — this violates the Discord Developer Terms of Service and the data goes stale immediately.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| "Discord Social SDK package not detected" log | `com.discord.social-sdk` not installed | Install the UPM package; the compile define is set automatically |
| `OnError("No IVXDiscordConfig provided")` | Config asset not assigned | Assign `IVXDiscordConfig` to `IVXDiscordManager` in the Inspector |
| Rich Presence not visible | Presence update interval too high, or Discord desktop app not running | Lower `PresenceUpdateInterval`; ensure Discord is running |
| Voice chat disabled | `EnableVoiceChat` is `false` in config | Enable in `IVXDiscordConfig` |
| Lobby bridging not working | `BridgeLobbiesToDiscord` is `false` | Enable in `IVXDiscordConfig` |
| Communication features fail in production | Missing production access approval | Apply in Developer Portal → Social SDK → Communication Features |
| "Already initialized" warning | `Initialize()` called more than once | Guard with `IVXDiscordManager.Instance.IsInitialized` check |

---

## Related Documentation

- [Social Module](social.md) — Nakama-native friends, sharing, referrals
- [Backend Module](backend.md) — Nakama integration
- [Leaderboards Module](leaderboards.md) — Leaderboard integration with Rich Presence
- [Platform Guides](../platforms/index.md) — Per-platform SDK documentation
- [Discord Developer Portal](https://discord.com/developers/applications) — Application management
- [Discord Social SDK Docs](https://discord.com/developers/docs/social-sdk/overview) — Official SDK reference
