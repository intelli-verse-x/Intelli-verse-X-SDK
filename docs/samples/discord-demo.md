# Discord Social Demo

Sample scene demonstrating the full Discord Social SDK integration — account linking, rich presence, friends, DMs, lobbies, voice chat, invites, and moderation.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Demos/IVXDiscordSocialDemo.cs`

This sample demonstrates:

- Discord account linking & OAuth2 (desktop + mobile)
- Rich Presence (activity, party, timers, buttons)
- Friends list with online/in-game status
- Direct messages (send, history, summaries)
- Lobby creation, chat, and voice channels
- Game invites (send, receive, accept/decline)
- Content moderation with Discord metadata

---

## Scene Hierarchy

```
Canvas (Screen Space — Overlay, 1920×1080 reference)
├── Background
├── DiscordSocialRoot
│   ├── Header ("DISCORD SOCIAL SDK")
│   ├── TabBar
│   │   ├── Account
│   │   ├── Presence
│   │   ├── Friends
│   │   ├── DMs
│   │   ├── Lobby/Voice
│   │   ├── Invites
│   │   └── Moderation
│   ├── ContentArea
│   │   ├── Panel_0 (Account)
│   │   ├── Panel_1 (Presence)
│   │   ├── Panel_2 (Friends)
│   │   ├── Panel_3 (DMs)
│   │   ├── Panel_4 (Lobby/Voice)
│   │   ├── Panel_5 (Invites)
│   │   └── Panel_6 (Moderation)
│   └── Footer (status bar)
```

---

## Key Components

### Account Linking

```csharp
var mgr = IVXDiscordManager.Instance;
mgr.Initialize();
mgr.LinkAccount();

// Mobile OAuth2 flow
mgr.StartMobileOAuth2Flow("mygame://", ok =>
    Debug.Log($"Mobile OAuth2: {ok}"));

// Listen for account events
mgr.OnAccountLinked += (userId, username) =>
    Debug.Log($"Linked: {username} ({userId})");
mgr.OnAccountUnlinked += () =>
    Debug.Log("Account unlinked.");
```

### Rich Presence

```csharp
var pr = IVXDiscordPresence.Instance;
pr.SetActivity("Ranked Match", "In Queue");
pr.StartTimer();
pr.SetParty("party_id", currentSize: 2, maxSize: 4, "join_secret");
pr.AddButton("Discord", "https://discord.com");
pr.SetStatusDisplayType(IVXStatusDisplayType.Details);
```

### Friends & Relationships

```csharp
var friends = IVXDiscordFriends.Instance;
friends.Refresh();
friends.OnFriendsUpdated += (list) =>
{
    foreach (var f in list)
        Debug.Log($"{f.DisplayName} online={f.IsOnline} inGame={f.IsInGame}");
};
friends.SendGameFriendRequest("username", ok => Debug.Log($"Sent: {ok}"));
friends.BlockUser(userId, ok => Debug.Log($"Blocked: {ok}"));
```

### Lobby & Voice

```csharp
var lobby = IVXDiscordLobby.Instance;
lobby.CreateOrJoinLobby("secret", "{\"demo\":true}");
lobby.SendMessage("Hello lobby!");
lobby.OnMessageReceived += (sender, message) =>
    Debug.Log($"{sender}: {message}");

var voice = IVXDiscordVoice.Instance;
voice.JoinCall(lobby.CurrentLobbyId);
voice.SetSelfMute(true);
voice.SetInputVolume(150f);
voice.SetVADThreshold(true, -35f);
```

### Game Invites

```csharp
var invites = IVXDiscordInvites.Instance;
invites.SendInvite(targetUserId, "Join my session!");
invites.OnInviteReceived += (invite) =>
{
    Debug.Log($"From: {invite.InviterName}, Party: {invite.PartyCurrentSize}/{invite.PartyMaxSize}");
    invites.AcceptInvite(invite);
};
```

### Moderation

```csharp
var mod = IVXDiscordModeration.Instance;
mod.ProcessModerationMetadata(messageId, metadata);
mod.StartVoiceModerationCapture(lobbyId);
mod.ReportUser(userId, "Reason", ok => Debug.Log($"Reported: {ok}"));
mod.OnModerationDecisionReceived += (decision) =>
    Debug.Log($"Action={decision.Action} Severity={decision.Severity}");
```

---

## How to Use

### Running the Sample

1. Add `IVXDiscordSocialDemo` to any GameObject
2. Ensure `IVXDiscordManager`, `IVXDiscordPresence`, `IVXDiscordFriends`, `IVXDiscordLobby`, `IVXDiscordVoice`, `IVXDiscordInvites`, and `IVXDiscordModeration` are in the scene
3. Configure `IVXDiscordConfig` with your Discord Application ID
4. Press **Play**

### Testing Account Linking

1. Click the **Account** tab
2. Click **"Initialize SDK"** then **"Link Discord Account"**
3. Complete the OAuth2 flow in the browser
4. Observe the account status update in the panel

### Testing Rich Presence

1. Switch to the **Presence** tab
2. Enter details and state text
3. Click **"Set Activity"** — check your Discord profile for the activity
4. Try **"Start Timer"**, **"Set Party"**, and **"Add Buttons"**

### Testing Voice Chat

1. Switch to **Lobby/Voice** tab
2. Click **"Create/Join Lobby"** with a secret
3. Click **"Join Voice"** to connect to the voice channel
4. Use **Mute/Deafen** toggles and volume sliders

---

## Customization

### Adding More Tabs

Edit `IVXDiscordSocialDemo.cs` — add entries to the tab labels array and implement a new `BuildPanel` case.

### Changing the Theme

Modify the color constants at the top of the script (`COL_BG`, `COL_PANEL`, `COL_ACCENT`, `COL_HIGHLIGHT`).

---

## See Also

- [Discord Module](../modules/discord.md)
- [Discord Integration Guide](../guides/discord-integration.md)
