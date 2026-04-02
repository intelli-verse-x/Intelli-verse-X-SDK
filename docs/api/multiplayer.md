# Multiplayer API Reference

Complete API reference for the Multiplayer & Game Modes module.

---

## IVXGameModeManager

Central hub for mode selection, player slot management, teams, readiness, and match lifecycle phases.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXGameModeManager` (static) | Singleton accessor; auto-creates if missing |
| `CurrentConfig` | `IVXMatchConfig` | Active match configuration |
| `CurrentMode` | `IVXGameMode` | Current game mode derived from config |
| `Phase` | `IVXMatchPhase` | Current match lifecycle phase |
| `Players` | `IReadOnlyList<IVXPlayerSlot>` | All player slots |
| `PlayerCount` | `int` | Occupied slot count |
| `CanStartMatch` | `bool` | Whether minimum players are met and humans are ready |
| `IsHost` | `bool` | Whether the local client is the host |
| `LocalPlayer` | `IVXPlayerSlot` | Primary local slot (first player) |

### Methods

#### SelectMode

```csharp
public void SelectMode(IVXGameMode mode, int maxPlayers = 0)
```

Applies mode-specific defaults and resets lobby state.

**Parameters:**
- `mode` - Target game mode
- `maxPlayers` - Maximum player count (0 uses mode default)

**Example:**
```csharp
IVXGameModeManager.Instance.SelectMode(IVXGameMode.OnlineVersus, maxPlayers: 2);
```

---

#### SetConfig

```csharp
public void SetConfig(IVXMatchConfig config)
```

Replaces the current match configuration and resets lobby state.

**Parameters:**
- `config` - New match configuration

**Example:**
```csharp
var cfg = IVXMatchConfig.Local(maxPlayers: 4);
cfg.AllowBots = true;
IVXGameModeManager.Instance.SetConfig(cfg);
```

---

#### AddLocalPlayer

```csharp
public IVXPlayerSlot AddLocalPlayer(string displayName, int inputDeviceIndex = -1)
```

Adds a same-device player to the roster.

**Parameters:**
- `displayName` - Player display name
- `inputDeviceIndex` - Input device index (-1 for default)

**Returns:** `IVXPlayerSlot` or `null` if the roster is full

---

#### AddRemotePlayer

```csharp
public IVXPlayerSlot AddRemotePlayer(string userId, string displayName)
```

Adds an online player to the roster.

**Parameters:**
- `userId` - Remote player user ID
- `displayName` - Player display name

**Returns:** `IVXPlayerSlot` or `null` if full

---

#### AddBot

```csharp
public IVXPlayerSlot AddBot(string botName)
```

Adds a bot player if `CurrentConfig.AllowBots` is true.

**Parameters:**
- `botName` - Bot display name

**Returns:** `IVXPlayerSlot` or `null` if bots are disallowed or roster is full

---

#### RemovePlayer

```csharp
public void RemovePlayer(int slotIndex)
```

Removes a player by slot index. Slot 0 (primary local player) is protected and cannot be removed.

**Parameters:**
- `slotIndex` - Slot index to remove

---

#### SetPlayerReady

```csharp
public void SetPlayerReady(int slotIndex, bool ready)
```

Toggles the ready state for a player slot.

**Parameters:**
- `slotIndex` - Target slot index
- `ready` - Ready flag

---

#### SetPlayerTeam

```csharp
public void SetPlayerTeam(int slotIndex, IVXTeam team)
```

Assigns a player to a team.

**Parameters:**
- `slotIndex` - Target slot index
- `team` - Team assignment

---

#### StartMatch

```csharp
public void StartMatch()
```

Begins loading phase. Requires `CanStartMatch` to be true.

**Example:**
```csharp
if (IVXGameModeManager.Instance.CanStartMatch)
    IVXGameModeManager.Instance.StartMatch();
```

---

#### BeginGameplay

```csharp
public void BeginGameplay()
```

Transitions to the `InProgress` phase after scene loading completes.

---

#### EndMatch

```csharp
public void EndMatch()
```

Transitions to the `Finished` phase.

---

#### ReturnToLobby

```csharp
public void ReturnToLobby()
```

Resets readiness and returns to the lobby phase.

---

#### Reset

```csharp
public void Reset()
```

Clears config, all players, and phase state.

---

#### SetPhase

```csharp
public void SetPhase(IVXMatchPhase phase)
```

Forces a specific phase transition.

**Parameters:**
- `phase` - Target match phase

---

#### GetTeamPlayers

```csharp
public List<IVXPlayerSlot> GetTeamPlayers(IVXTeam team)
```

Returns all players assigned to a team.

---

#### GetLocalPlayers / GetRemotePlayers

```csharp
public List<IVXPlayerSlot> GetLocalPlayers()
public List<IVXPlayerSlot> GetRemotePlayers()
```

Partitions the roster by `IsLocal` flag.

---

#### IsModeAvailable

```csharp
public bool IsModeAvailable(IVXGameMode mode)
```

Checks network reachability for online modes.

**Returns:** `true` if the mode can be used

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnModeChanged` | `Action<IVXGameMode>` | Mode changed after `SelectMode` / `SetConfig` |
| `OnPlayerAdded` | `Action<IVXPlayerSlot>` | Player added to the roster |
| `OnPlayerRemoved` | `Action<IVXPlayerSlot>` | Player removed from the roster |
| `OnPlayerReadyChanged` | `Action<IVXPlayerSlot>` | Ready flag updated for a slot |
| `OnAllPlayersReady` | `Action<IVXMatchConfig>` | Fired when `CanStartMatch` becomes true |
| `OnMatchPhaseChanged` | `Action<IVXMatchPhase, IVXMatchPhase>` | Phase transition (previous, next) |
| `OnConfigChanged` | `Action<IVXMatchConfig>` | New config applied |

---

## IVXLobbyManager

Online room lifecycle: create, join, list, leave, ready state, and host moderation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXLobbyManager` (static) | Singleton instance |
| `IsInRoom` | `bool` | Whether the client is in a room |
| `CurrentRoom` | `IVXRoomInfo` | Current room info or `null` |
| `CachedRooms` | `IReadOnlyList<IVXRoomInfo>` | Last fetched room listing |
| `IsRefreshing` | `bool` | Whether a list request is in flight |

### Methods

#### RefreshRoomList

```csharp
public void RefreshRoomList(IVXRoomFilter filter = null)
```

Fetches or updates the cached room listing.

**Parameters:**
- `filter` - Optional filter criteria

**Example:**
```csharp
IVXLobbyManager.Instance.RefreshRoomList(new IVXRoomFilter { Limit = 15, OnlyAvailable = true });
```

---

#### StartAutoRefresh / StopAutoRefresh

```csharp
public void StartAutoRefresh(float intervalSeconds = 5f, IVXRoomFilter filter = null)
public void StopAutoRefresh()
```

Starts or stops periodic room list refresh.

**Parameters:**
- `intervalSeconds` - Refresh interval
- `filter` - Optional filter criteria

---

#### CreateRoom

```csharp
public void CreateRoom(IVXCreateRoomRequest request)
```

Creates a new room and enters as host.

**Parameters:**
- `request` - Room creation request with `RoomName`, `Config`, and optional `Password`

**Example:**
```csharp
IVXLobbyManager.Instance.CreateRoom(new IVXCreateRoomRequest
{
    RoomName = "Open Trivia",
    Config = IVXMatchConfig.OnlineVersus(2),
    Password = null
});
```

---

#### JoinRoom

```csharp
public void JoinRoom(IVXJoinRoomRequest request)
```

Joins a room by ID with optional password.

**Parameters:**
- `request` - Join request with `RoomId` and optional `Password`

---

#### LeaveRoom

```csharp
public void LeaveRoom()
```

Exits the current room and calls `IVXGameModeManager.ReturnToLobby()`.

---

#### SetReady

```csharp
public void SetReady(bool ready)
```

Toggles local ready state and forwards to the game mode manager.

---

#### KickPlayer

```csharp
public void KickPlayer(int slotIndex)
```

Host-only: removes a remote player from the room.

**Parameters:**
- `slotIndex` - Slot index to kick

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnRoomListUpdated` | `Action<List<IVXRoomInfo>>` | Room listing refreshed |
| `OnRoomJoined` | `Action<IVXJoinRoomResponse>` | Join completed |
| `OnRoomCreated` | `Action<IVXCreateRoomResponse>` | Room created |
| `OnRoomLeft` | `Action` | Left room |
| `OnLobbyEvent` | `Action<IVXLobbyEvent>` | Structured lobby notification |
| `OnError` | `Action<string>` | Error message |

---

## IVXMatchmakingManager

Quick-match and ranked search with progress callbacks and match-found delivery.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXMatchmakingManager` (static) | Singleton instance |
| `IsSearching` | `bool` | Whether a search is active |
| `SearchElapsed` | `float` | Seconds since search start |
| `MaxSearchTime` | `float` | Timeout in seconds (default 60) |
| `TicketId` | `string` | Active matchmaker ticket ID (Nakama) |

### Methods

#### StartSearch

```csharp
public void StartSearch(IVXMatchConfig config = null)
```

Begins a matchmaking search. Sets the config and transitions to the `Matchmaking` phase.

**Parameters:**
- `config` - Match configuration (optional, uses current if null)

---

#### CancelSearch

```csharp
public void CancelSearch()
```

Cancels the active search and clears the ticket.

---

#### QuickMatch

```csharp
public void QuickMatch()
```

Shorthand for `StartSearch(IVXMatchConfig.OnlineVersus())`.

**Example:**
```csharp
IVXMatchmakingManager.Instance.QuickMatch();
```

---

#### RankedMatch

```csharp
public void RankedMatch()
```

Shorthand for `StartSearch(IVXMatchConfig.Ranked())`.

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnSearchStarted` | `Action` | Search began |
| `OnSearchProgress` | `Action<float>` | Elapsed seconds update |
| `OnMatchFound` | `Action<IVXMatchFoundResult>` | Match found |
| `OnSearchCancelled` | `Action<string>` | Search cancelled with reason |
| `OnError` | `Action<string>` | Error message |

---

## IVXLocalMultiplayerManager

Same-device play: hot-seat turns, round counting, timers, and split-screen viewport helpers.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXLocalMultiplayerManager` (static) | Singleton instance |
| `IsSessionActive` | `bool` | Whether a local session is running |
| `CurrentTurnIndex` | `int` | Index within local players |
| `CurrentRound` | `int` | 1-based round counter |
| `CurrentTurnPlayer` | `IVXPlayerSlot` | Active player slot |
| `IsHotSeat` | `bool` | Turn-based mode flag |
| `TurnTimeRemaining` | `float` | Seconds left or -1 if no limit |

### Methods

#### StartSession

```csharp
public void StartSession(bool hotSeat = true, float turnTimeLimitSeconds = 0f)
```

Starts a local multiplayer session. Requires at least 2 local players.

**Parameters:**
- `hotSeat` - Enable turn-based rotation (`false` for simultaneous play)
- `turnTimeLimitSeconds` - Per-turn time limit (0 for no limit)

**Example:**
```csharp
IVXLocalMultiplayerManager.Instance.StartSession(hotSeat: true, turnTimeLimitSeconds: 20f);
```

---

#### EndSession

```csharp
public void EndSession()
```

Finishes the local session and sets phase to `Finished`.

---

#### EndTurn

```csharp
public void EndTurn()
```

Advances to the next player's turn, or starts a new round if all players have played.

---

#### JumpToPlayer

```csharp
public void JumpToPlayer(int slotIndex)
```

Forces the active player to a specific slot index.

**Parameters:**
- `slotIndex` - Target slot index

---

#### CalculateSplitScreenRects

```csharp
public static Rect[] CalculateSplitScreenRects(int playerCount)
```

Returns normalized viewport rects for 2, 3, or 4 player split-screen layouts.

**Parameters:**
- `playerCount` - Number of players (2--4)

**Returns:** Array of normalized `Rect` values

---

#### ApplySplitScreen

```csharp
public static void ApplySplitScreen(Camera[] cameras)
```

Applies split-screen rects to an array of cameras.

**Parameters:**
- `cameras` - Player cameras to configure

**Example:**
```csharp
IVXLocalMultiplayerManager.ApplySplitScreen(playerCameras);
```

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnTurnStarted` | `Action<IVXPlayerSlot>` | Turn started for a player |
| `OnTurnEnded` | `Action<IVXPlayerSlot>` | Turn ended for a player |
| `OnRoundCompleted` | `Action<int>` | Round completed (round index) |
| `OnLocalSessionStarted` | `Action<List<IVXPlayerSlot>>` | Session started with participant list |
| `OnLocalSessionEnded` | `Action` | Session ended |

---

## Data Models

### IVXGameMode (enum)

| Value | Description |
|-------|-------------|
| `Solo` | Single player |
| `LocalMultiplayer` | Shared device |
| `OnlineMultiplayer` | General online |
| `OnlineCoop` | Cooperative online |
| `OnlineVersus` | Competitive online |
| `TurnBased` | Async / turn-style |
| `RankedMatch` | Skill-rated queue |

### IVXMatchPhase (enum)

| Value | Description |
|-------|-------------|
| `None` | No active match |
| `Lobby` | In lobby |
| `Matchmaking` | Searching for match |
| `Loading` | Loading game scene |
| `InProgress` | Match in progress |
| `Finished` | Match completed |
| `Paused` | Match paused |
| `Disconnected` | Connection lost |

### IVXMatchConfig

| Property | Type | Description |
|----------|------|-------------|
| `Mode` | `IVXGameMode` | Game mode |
| `MinPlayers` | `int` | Minimum players required |
| `MaxPlayers` | `int` | Maximum player capacity |
| `TeamCount` | `int` | Number of teams |
| `MaxPlayersPerTeam` | `int` | Max players per team |
| `AllowBots` | `bool` | Whether bots are permitted |
| `AllowSpectators` | `bool` | Whether spectators are permitted |
| `MatchDurationSeconds` | `float` | Match time limit |
| `Transport` | `IVXNetworkTransport` | Network transport type |
| `RoomLabel` | `string` | Room display label |
| `IsPublic` | `bool` | Public room visibility |
| `Password` | `string` | Room password |
| `CustomProperties` | `Dictionary<string, string>` | Custom metadata |
| `TurnTimeLimitSeconds` | `float` | Per-turn time limit |
| `AutoStartWhenReady` | `bool` | Auto-start when all ready |
| `CountdownSeconds` | `float` | Pre-start countdown |
| `RequiresNetwork` | `bool` (computed) | Whether mode needs network |
| `IsTeamBased` | `bool` (computed) | Whether mode uses teams |

#### Factory Methods

```csharp
public static IVXMatchConfig Solo()
public static IVXMatchConfig Local(int maxPlayers)
public static IVXMatchConfig OnlineVersus(int maxPlayers, IVXNetworkTransport transport = NakamaRealtime)
public static IVXMatchConfig OnlineCoop(int maxPlayers)
public static IVXMatchConfig Ranked()
```

### IVXPlayerSlot

| Property | Type | Description |
|----------|------|-------------|
| `SlotIndex` | `int` | Slot position |
| `DisplayName` | `string` | Player name |
| `UserId` | `string` | User identifier |
| `IsHuman` | `bool` | Human vs. bot |
| `IsLocal` | `bool` | Same-device player |
| `IsHost` | `bool` | Room host |
| `Team` | `IVXTeam` | Team assignment |
| `ReadyState` | `IVXPlayerReadyState` | Ready status |
| `AvatarKey` | `string` | Avatar asset key |
| `CustomData` | `Dictionary<string, string>` | Custom metadata |
| `InputDeviceIndex` | `int` | Input device index |

### IVXRoomInfo

| Property | Type | Description |
|----------|------|-------------|
| `RoomId` | `string` | Room identifier |
| `RoomName` | `string` | Display name |
| `HostName` | `string` | Host player name |
| `PlayerCount` | `int` | Current players |
| `MaxPlayers` | `int` | Capacity |
| `Mode` | `IVXGameMode` | Game mode |
| `IsPasswordProtected` | `bool` | Password required |
| `IsInProgress` | `bool` | Match already started |
| `CreatedAt` | `long` | Creation timestamp |
| `PingMs` | `int` | Estimated ping |
| `CustomProperties` | `Dictionary<string, string>` | Custom metadata |
| `HasSpace` | `bool` | Whether there is room to join |

### IVXMatchFoundResult

| Property | Type | Description |
|----------|------|-------------|
| `MatchId` | `string` | Match identifier |
| `OpponentUserId` | `string` | Opponent user ID |
| `OpponentDisplayName` | `string` | Opponent display name |
| `OpponentRating` | `int` | Opponent skill rating |
| `EstimatedPingMs` | `int` | Estimated latency |
| `Transport` | `IVXNetworkTransport` | Transport used |

---

## See Also

- [Multiplayer Module Guide](../modules/multiplayer.md)
- [Backend Module](../modules/backend.md)
- [Discord Module](../modules/discord.md)
- [Hiro Module](../modules/hiro.md)
