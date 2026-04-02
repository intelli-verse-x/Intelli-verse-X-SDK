# Discord Social SDK API Reference

Complete API reference for the Discord Social SDK module.

---

## IVXDiscordManager

Main entry point for Discord SDK initialization, OAuth2 account linking, and social settings.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXDiscordManager` | Singleton instance |
| `IsInitialized` | `bool` | Whether the Discord SDK has been initialized |
| `IsAccountLinked` | `bool` | Whether a Discord account is currently linked |
| `LinkedUserId` | `string` | Discord user ID of the linked account |
| `LinkedUserName` | `string` | Discord username of the linked account |

### Methods

#### Initialize

```csharp
public void Initialize(IVXDiscordConfig config)
```

Initializes the Discord Social SDK with the provided configuration asset.

**Parameters:**
- `config` - `IVXDiscordConfig` ScriptableObject with application settings

**Example:**
```csharp
IVXDiscordManager.Instance.Initialize(discordConfig);
```

---

#### LinkAccount

```csharp
public void LinkAccount()
```

Starts the OAuth2 account linking flow using the configured redirect URI.

**Example:**
```csharp
IVXDiscordManager.Instance.OnAccountLinked += (id, name) => Debug.Log($"Linked: {name}");
IVXDiscordManager.Instance.LinkAccount();
```

---

#### StartMobileOAuth2Flow

```csharp
public void StartMobileOAuth2Flow(string redirectScheme, Action<bool> onComplete)
```

Starts the mobile OAuth2 + PKCE linking flow with the app's custom URL scheme.

**Parameters:**
- `redirectScheme` - Custom URL scheme (e.g. `"mygame://"`)
- `onComplete` - Callback with success status

**Example:**
```csharp
IVXDiscordManager.Instance.StartMobileOAuth2Flow("mygame://", success =>
{
    Debug.Log(success ? "Mobile link succeeded" : "Mobile link failed");
});
```

---

#### StartConsoleOAuth2Flow

```csharp
public void StartConsoleOAuth2Flow(Action<string> onCodeReceived, Action<bool> onComplete)
```

Starts the console device-code OAuth2 flow. Display the received code to the user on screen.

**Parameters:**
- `onCodeReceived` - Callback delivering the device code string
- `onComplete` - Callback with success status

**Example:**
```csharp
IVXDiscordManager.Instance.StartConsoleOAuth2Flow(
    code => ShowDeviceCodeUi(code),
    success => Debug.Log($"Console link: {success}"));
```

---

#### CreateProvisionalAccount

```csharp
public void CreateProvisionalAccount(Action<bool> onComplete)
```

Creates a provisional Discord-backed identity for social features without full OAuth.

**Parameters:**
- `onComplete` - Callback with success status

---

#### MergeProvisionalAccount

```csharp
public void MergeProvisionalAccount(string externalAuthToken, Action<bool> onComplete)
```

Upgrades a provisional account to a fully linked account using an external auth token.

**Parameters:**
- `externalAuthToken` - Bearer or backend auth token
- `onComplete` - Callback with success status

---

#### SetPublisherId

```csharp
public void SetPublisherId(string publisherId)
```

Sets the publisher ID at runtime for cross-game shared authentication.

**Parameters:**
- `publisherId` - Publisher identifier string

---

#### UpdateToken

```csharp
public void UpdateToken(string newBearerToken)
```

Refreshes the Discord bearer token after external browser or backend renewal.

**Parameters:**
- `newBearerToken` - New OAuth2 bearer token

---

#### RegisterAuthorizeRequestCallback

```csharp
public void RegisterAuthorizeRequestCallback(Action callback)
```

Registers a callback invoked when Discord requests authorization from the client (entry-point linking).

**Parameters:**
- `callback` - Action to invoke when authorization is requested

---

#### RemoveAuthorizeRequestCallback

```csharp
public void RemoveAuthorizeRequestCallback()
```

Removes the authorization request callback, blocking entry-point linking during gameplay.

---

#### OpenConnectedGamesSettingsInDiscord

```csharp
public void OpenConnectedGamesSettingsInDiscord()
```

Opens the Connected Games settings page in the Discord client.

---

#### OpenUserProfileInDiscord

```csharp
public void OpenUserProfileInDiscord(string userId)
```

Opens a user's profile page in the Discord client.

**Parameters:**
- `userId` - Discord user ID to view

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnAccountLinked` | `Action<string, string>` | Fired when linking completes (userId, userName) |
| `OnAccountUnlinked` | `Action` | Fired when the account is unlinked |
| `OnAuthorizeRequested` | `Action` | Fired when Discord requests entry-point authorization |

---

## IVXDiscordPresence

Rich Presence management for activity display, party info, buttons, and RPC-only mode.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXDiscordPresence` | Singleton instance |
| `IsRPCOnlyMode` | `bool` | Whether presence is running in desktop RPC-only mode |

### Methods

#### SetActivity

```csharp
public void SetActivity(string details, string state)
```

Sets the Rich Presence activity lines.

**Parameters:**
- `details` - Primary activity text (e.g. `"Ranked Match"`)
- `state` - Secondary state text (e.g. `"Score: 1,500"`)

**Example:**
```csharp
IVXDiscordPresence.Instance.SetActivity("Ranked Match", "Score: 1,500");
```

---

#### SetParty

```csharp
public void SetParty(string partyId, int currentSize, int maxSize, string joinSecret = null)
```

Sets party information for group invites.

**Parameters:**
- `partyId` - Unique party identifier
- `currentSize` - Current number of members
- `maxSize` - Maximum party capacity
- `joinSecret` - Secret used for join invites (optional)

---

#### StartTimer

```csharp
public void StartTimer()
```

Starts the elapsed-time counter on the Rich Presence display.

---

#### ClearPresence

```csharp
public void ClearPresence()
```

Removes all Rich Presence data from the user's Discord profile.

---

#### SetFieldUrls

```csharp
public void SetFieldUrls(string stateUrl = null, string detailsUrl = null)
```

Sets clickable URLs on state and details lines.

**Parameters:**
- `stateUrl` - URL for the state line
- `detailsUrl` - URL for the details line

---

#### SetAssetUrls

```csharp
public void SetAssetUrls(string largeUrl = null, string smallUrl = null)
```

Sets clickable URLs on the large and small image assets.

**Parameters:**
- `largeUrl` - URL for the large image
- `smallUrl` - URL for the small image

---

#### SetStatusDisplayType

```csharp
public void SetStatusDisplayType(IVXStatusDisplayType displayType)
```

Controls which fields appear in the presence card (Name, State, Details).

**Parameters:**
- `displayType` - Display type enum value

---

#### SetSupportedPlatforms

```csharp
public void SetSupportedPlatforms(IVXActivityPlatforms platforms)
```

Restricts which platforms show the activity.

**Parameters:**
- `platforms` - Bitflag combination of `Desktop`, `Mobile`, etc.

---

#### AddButton

```csharp
public void AddButton(string label, string url)
```

Adds a clickable button to the presence card (max 2).

**Parameters:**
- `label` - Button text
- `url` - Button URL

---

#### ClearButtons

```csharp
public void ClearButtons()
```

Removes all buttons from the presence card.

---

#### SetInviteCoverImage

```csharp
public void SetInviteCoverImage(string assetKey)
```

Sets the cover art image for invite embeds.

**Parameters:**
- `assetKey` - Discord Rich Presence art asset key

---

#### SetSmallImage

```csharp
public void SetSmallImage(string assetKey, string text)
```

Sets the small overlay image and hover text.

**Parameters:**
- `assetKey` - Art asset key
- `text` - Hover tooltip text

---

#### InitializeRPCOnly

```csharp
public void InitializeRPCOnly(long applicationId)
```

Initializes presence in desktop RPC-only mode without full SDK authentication.

**Parameters:**
- `applicationId` - Discord application ID as a `long`

---

#### SyncFromGameState

```csharp
public void SyncFromGameState()
```

Reads current IVX game systems and pushes presence automatically.

---

## IVXDiscordVoice

Voice chat management including mute, deafen, VAD, and raw audio callbacks.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXDiscordVoice` | Singleton instance |
| `IsInCall` | `bool` | Whether the user is currently in a voice call |

### Methods

#### JoinCall

```csharp
public void JoinCall(string lobbyId)
```

Joins the voice channel associated with a lobby.

**Parameters:**
- `lobbyId` - Lobby identifier

---

#### LeaveCall

```csharp
public void LeaveCall()
```

Leaves the current voice call.

---

#### AutoJoinFromLobby

```csharp
public void AutoJoinFromLobby()
```

Automatically joins voice when the player enters a lobby with voice enabled.

---

#### SetSelfMute / SetSelfDeafen

```csharp
public void SetSelfMute(bool muted)
public void SetSelfDeafen(bool deafened)
```

Toggles local mute or deafen state.

**Parameters:**
- `muted` / `deafened` - State flag

---

#### SetInputVolume / SetOutputVolume

```csharp
public void SetInputVolume(float volume)
public void SetOutputVolume(float volume)
```

Adjusts microphone input or speaker output volume (0.0 -- 1.0).

---

#### SetParticipantVolume

```csharp
public void SetParticipantVolume(string userId, float volume)
```

Adjusts volume for a specific participant.

**Parameters:**
- `userId` - Target user
- `volume` - Volume level (0.0 -- 1.0)

---

#### SetVADThreshold

```csharp
public void SetVADThreshold(bool useCustom, float thresholdDb = -35f)
```

Configures Voice Activity Detection threshold.

**Parameters:**
- `useCustom` - Use custom threshold instead of auto
- `thresholdDb` - Threshold in decibels

---

#### JoinCallWithAudioCallbacks

```csharp
public void JoinCallWithAudioCallbacks(
    string lobbyId,
    IVXAudioReceivedCallback onReceived,
    IVXAudioCapturedCallback onCaptured)
```

Joins voice with raw PCM audio callbacks for FMOD, Wwise, or custom audio pipelines.

**Parameters:**
- `lobbyId` - Lobby identifier
- `onReceived` - Delegate for incoming audio frames
- `onCaptured` - Delegate for captured microphone frames

---

#### SetSelfMuteAll / SetSelfDeafenAll / EndAllCalls

```csharp
public void SetSelfMuteAll(bool muted)
public void SetSelfDeafenAll(bool deafened)
public void EndAllCalls()
```

Global voice controls across all active calls.

---

#### GetParticipantVoiceState

```csharp
public IVXVoiceState GetParticipantVoiceState(string userId)
```

Returns the voice state (muted, deafened, speaking) for a participant.

**Returns:** `IVXVoiceState` or `null`

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnParticipantMuteChanged` | `Action<string, bool>` | Participant mute state changed |
| `OnParticipantDeafenChanged` | `Action<string, bool>` | Participant deafen state changed |
| `OnParticipantSpeaking` | `Action<string, bool>` | Participant started/stopped speaking |
| `OnParticipantListUpdated` | `Action` | Voice participant list changed |

---

## IVXDiscordLobby

Lobby lifecycle, metadata, idle timeout, and text chat.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXDiscordLobby` | Singleton instance |
| `IsInLobby` | `bool` | Whether the user is in a lobby |
| `CurrentLobby` | `IVXDiscordLobbyInfo` | Current lobby information |

### Methods

#### CreateOrJoinLobby

```csharp
public void CreateOrJoinLobby(string secret, Dictionary<string, string> metadata = null)
```

Creates or joins a lobby by secret, with optional metadata.

**Parameters:**
- `secret` - Lobby join secret
- `metadata` - Key-value metadata pairs

---

#### CreateOrJoinLobbyWithMetadata

```csharp
public void CreateOrJoinLobbyWithMetadata(
    string secret,
    Dictionary<string, string> lobbyMetadata,
    Dictionary<string, string> userMetadata,
    Action<IVXDiscordLobbyInfo> onComplete)
```

Creates or joins with separate lobby-level and user-level metadata.

**Parameters:**
- `secret` - Lobby secret
- `lobbyMetadata` - Lobby-wide metadata
- `userMetadata` - Per-user metadata
- `onComplete` - Callback with lobby info

---

#### BridgeIVXRoom

```csharp
public void BridgeIVXRoom(string ivxRoomId, Dictionary<string, string> roomMetadata = null)
```

Bridges an IntelliVerseX multiplayer room to a Discord lobby.

**Parameters:**
- `ivxRoomId` - IVX room identifier
- `roomMetadata` - Optional metadata for the bridge

---

#### UpdateLobbyMemberMetadata

```csharp
public void UpdateLobbyMemberMetadata(string json)
```

Updates the local user's metadata in the current lobby.

**Parameters:**
- `json` - JSON string of metadata key-value pairs

---

#### SetLobbyIdleTimeout

```csharp
public void SetLobbyIdleTimeout(int seconds)
```

Sets the idle timeout before the lobby is automatically disbanded.

**Parameters:**
- `seconds` - Timeout in seconds (default 300, max 604800)

---

#### SendMessage

```csharp
public void SendMessage(string content)
```

Sends a text chat message to the current lobby.

---

#### FetchChatHistory

```csharp
public void FetchChatHistory(int limit, Action<List<IVXLobbyChatMessage>> callback)
```

Fetches recent chat history for the current lobby.

**Parameters:**
- `limit` - Maximum messages to retrieve
- `callback` - Callback with message list

---

#### LeaveLobby

```csharp
public void LeaveLobby()
```

Leaves the current lobby.

---

#### GetLobbyInfo

```csharp
public IVXDiscordLobbyInfo GetLobbyInfo()
```

Returns the current lobby information snapshot.

**Returns:** `IVXDiscordLobbyInfo` with `LobbyId`, `Secret`, `MemberCount`, `VoiceActive`, `Metadata`, `LobbyMetadata`, `MemberIds`

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnMessageReceived` | `Action<IVXLobbyChatMessage>` | Text message received in the lobby |
| `OnMemberJoined` | `Action<string>` | A member joined the lobby |
| `OnMemberLeft` | `Action<string>` | A member left the lobby |

---

## IVXDiscordFriends

Unified friends list with dual-layer relationship APIs for Discord and game (Nakama) layers.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXDiscordFriends` | Singleton instance |
| `Friends` | `IReadOnlyList<IVXUnifiedFriend>` | Current merged friends list |

### Methods

#### Refresh

```csharp
public void Refresh()
```

Refreshes the unified friends list from both Discord and game backends.

**Example:**
```csharp
IVXDiscordFriends.Instance.OnFriendsUpdated += list => BindFriendsUi(list);
IVXDiscordFriends.Instance.Refresh();
```

---

#### GetInGameFriends

```csharp
public List<IVXUnifiedFriend> GetInGameFriends()
```

Returns friends who are currently in the game.

---

#### GetBySource

```csharp
public List<IVXUnifiedFriend> GetBySource(IVXFriendSource source)
```

Filters friends by source (`Discord`, `Game`, `Both`).

---

#### SendGameFriendRequest / SendDiscordFriendRequest

```csharp
public void SendGameFriendRequest(string username)
public void SendDiscordFriendRequest(string username)
```

Sends a friend request via the game backend or Discord respectively.

---

#### AcceptGameFriendRequest / AcceptDiscordFriendRequest

```csharp
public void AcceptGameFriendRequest(string userId)
public void AcceptDiscordFriendRequest(string userId)
```

Accepts an incoming friend request.

---

#### BlockUser / UnblockUser

```csharp
public void BlockUser(string userId)
public void UnblockUser(string userId)
```

Blocks or unblocks a user across both layers.

---

#### GetBlockedUsers / GetPendingRequests

```csharp
public List<IVXUnifiedFriend> GetBlockedUsers()
public List<IVXUnifiedFriend> GetPendingRequests()
```

Returns blocked users or pending friend requests for UI lists.

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnFriendsUpdated` | `Action<List<IVXUnifiedFriend>>` | Friends list refreshed |
| `OnFriendRequestReceived` | `Action<IVXUnifiedFriend>` | Incoming friend request |
| `OnFriendRequestAccepted` | `Action<IVXUnifiedFriend>` | Request accepted |
| `OnFriendRemoved` | `Action<string>` | Friend removed (userId) |
| `OnUserBlocked` | `Action<string>` | User blocked |
| `OnUserUnblocked` | `Action<string>` | User unblocked |

---

## IVXDiscordMessages

Direct message operations: send, edit, history, summaries, and Discord client integration.

### Methods

#### SendDM

```csharp
public void SendDM(string recipientId, string message, Action onSuccess = null, Action<string> onError = null)
```

Sends a direct message to a Discord user.

**Parameters:**
- `recipientId` - Target Discord user ID
- `message` - Message content
- `onSuccess` - Success callback
- `onError` - Error callback with message

---

#### EditDM

```csharp
public void EditDM(string recipientId, string messageId, string newContent, Action onSuccess = null, Action<string> onError = null)
```

Edits a previously sent direct message.

---

#### GetDMHistory

```csharp
public void GetDMHistory(string recipientId, int limit, Action<List<IVXDirectMessage>> onComplete)
```

Retrieves DM conversation history.

**Parameters:**
- `recipientId` - Discord user ID
- `limit` - Maximum messages to fetch
- `onComplete` - Callback with message list

---

#### GetDMSummaries

```csharp
public void GetDMSummaries(Action<List<IVXDMSummary>> onComplete)
```

Retrieves summaries of all DM conversations.

---

#### SetShowingChat

```csharp
public void SetShowingChat(bool showing)
```

Suppresses desktop notifications while the in-game chat is focused.

---

#### OpenMessageInDiscord

```csharp
public void OpenMessageInDiscord(string messageId)
```

Opens a specific message in the Discord client.

---

#### OpenDMSettingsInDiscord

```csharp
public void OpenDMSettingsInDiscord()
```

Opens the DM-related settings page in the Discord client.

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnDMReceived` | `Action<IVXDirectMessage>` | New DM received |
| `OnDMUpdated` | `Action<IVXDirectMessage>` | DM edited |
| `OnDMDeleted` | `Action<string>` | DM deleted (messageId) |

---

## IVXDiscordModeration

Moderation tools for message metadata processing, voice capture, and user reporting.

### Methods

#### ProcessModerationMetadata

```csharp
public IVXModerationDecision ProcessModerationMetadata(string messageId, Dictionary<string, string> metadata)
```

Parses moderation metadata keys (`action`, `reason`, `severity`, `flagged`) into a structured decision.

**Returns:** `IVXModerationDecision` with `Action` (`Show`, `Hide`, `Blur`, `Replace`), `Reason`, `Severity`, `Replacement`

---

#### StartVoiceModerationCapture

```csharp
public void StartVoiceModerationCapture(string lobbyId)
```

Begins capturing voice audio for moderation analysis.

---

#### StopVoiceModerationCapture

```csharp
public void StopVoiceModerationCapture()
```

Stops voice moderation capture.

---

#### ReportUser

```csharp
public void ReportUser(string userId, string reason, Action<bool> onComplete = null)
```

Submits a user report to Discord.

**Parameters:**
- `userId` - Reported user ID
- `reason` - Report reason text
- `onComplete` - Callback with success status

---

#### EnableAutoModeration

```csharp
public void EnableAutoModeration(bool enabled)
```

Toggles automatic moderation processing at runtime.

---

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `AutoModerateEnabled` | `bool` | Current auto-moderation state |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnVoiceDataCaptured` | `Action<string, byte[], int, int>` | Voice data captured (lobbyId, pcm, sampleRate, channels) |

---

## IVXDiscordLinkedChannels

Bridges in-game chat to Discord server text channels.

### Methods

#### LinkChannel

```csharp
public void LinkChannel(string lobbyId, string guildId, string channelId)
```

Links a lobby to a Discord server text channel for bidirectional messaging.

**Parameters:**
- `lobbyId` - Lobby identifier
- `guildId` - Discord server (guild) ID
- `channelId` - Discord channel ID

---

#### SendToLinkedChannel

```csharp
public void SendToLinkedChannel(string message)
```

Sends a message to the linked Discord channel.

---

#### UnlinkChannel

```csharp
public void UnlinkChannel()
```

Removes the channel link.

---

## IVXDiscordConfig

ScriptableObject holding all Discord integration settings.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ApplicationId` | `string` | Discord application (snowflake) ID |
| `ClientId` | `string` | OAuth2 client ID |
| `RedirectUri` | `string` | Registered OAuth2 redirect URI |
| `AutoPresence` | `bool` | Auto-sync presence from game state |
| `PresenceUpdateInterval` | `float` | Refresh interval in seconds (5--120) |
| `LargeImageAssetKey` | `string` | Large image art asset key |
| `LargeImageText` | `string` | Large image hover text |
| `SmallImageAssetKey` | `string` | Small overlay image key |
| `SmallImageText` | `string` | Small image hover text |
| `InviteCoverImageKey` | `string` | Asset key for invite cover art |
| `EnableVoiceChat` | `bool` | Enable voice chat features |
| `MaxVoiceLobbySize` | `int` | Voice lobby capacity (2--25) |
| `BridgeLobbiesToDiscord` | `bool` | Bridge IVX lobbies to Discord |
| `CommunityInviteUrl` | `string` | Default community invite URL |
| `StorePageUrl` | `string` | Default store page URL |
| `MobileRedirectScheme` | `string` | Deep link scheme for mobile OAuth |
| `PublisherId` | `string` | Publisher ID for cross-game auth |
| `EnableEntryPointLinking` | `bool` | Allow entry-point linking flows |
| `EnableAutoModeration` | `bool` | Toggle auto-moderation |
| `ModerationWebhookUrl` | `string` | Webhook URL for moderation pipelines |
| `EnableDirectMessages` | `bool` | Enable DM features |
| `DmHistoryLimit` | `int` | Default DM history batch size (1--200) |
| `EnableDebugLogging` | `bool` | Enable SDK diagnostic logging |

---

## Data Models

### IVXUnifiedFriend

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string` | User identifier |
| `DisplayName` | `string` | Display name |
| `DiscordRelationshipType` | `IVXRelationshipType` | Discord-layer relationship |
| `GameRelationshipType` | `IVXRelationshipType` | Game-layer relationship |
| `IsOnline` | `bool` | Online status |
| `IsInGame` | `bool` | Currently in-game |

### IVXDirectMessage

| Property | Type | Description |
|----------|------|-------------|
| `MessageId` | `string` | Message identifier |
| `SenderId` | `string` | Sender user ID |
| `Content` | `string` | Message text |
| `Timestamp` | `long` | Unix timestamp |
| `ModerationMetadata` | `Dictionary<string, string>` | Moderation metadata if present |

### IVXDMSummary

| Property | Type | Description |
|----------|------|-------------|
| `RecipientId` | `string` | Conversation partner ID |
| `LastMessagePreview` | `string` | Preview of the last message |
| `UnreadCount` | `int` | Number of unread messages |

### IVXDiscordLobbyInfo

| Property | Type | Description |
|----------|------|-------------|
| `LobbyId` | `string` | Lobby identifier |
| `Secret` | `string` | Join secret |
| `MemberCount` | `int` | Current member count |
| `VoiceActive` | `bool` | Whether voice is active |
| `Metadata` | `Dictionary<string, string>` | Lobby metadata |
| `LobbyMetadata` | `Dictionary<string, string>` | Lobby-level metadata |
| `MemberIds` | `List<string>` | List of member user IDs |

---

## See Also

- [Discord Module Guide](../modules/discord.md)
- [Social Module](../modules/social.md)
- [Backend Module](../modules/backend.md)
