# Multiplayer & Game Modes

The Multiplayer & Game Modes module unifies how IntelliVerseX titles select play modes, manage player slots and match phases, browse and join online rooms, run quick-match and ranked matchmaking, and drive same-device sessions with hot-seat turns and split-screen helpers. Four singleton `MonoBehaviour` components coordinate with `IVXMatchConfig` and slot models so UI and gameplay code can stay decoupled from transport details; when `IntelliVerseX.Backend` and Nakama are available, lobby listing and matchmaking use server-backed flows, with sensible fallbacks for editor and offline testing.

---

## 2. Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.GameModes` |
| **Assembly** | `IntelliVerseX.Multiplayer` |
| **Dependencies** | `IntelliVerseX.Core`, `IntelliVerseX.Backend` (optional — Nakama-backed lobby and matchmaking when `INTELLIVERSEX_HAS_NAKAMA` is defined) |
| **Pattern** | Four singleton `MonoBehaviour` managers; each auto-creates via `Instance` if not placed in the scene |

### Components

| Component | Responsibility |
|-----------|----------------|
| `IVXGameModeManager` | Central mode selection, player slots, teams, readiness, and match lifecycle phases |
| `IVXLobbyManager` | Online room lifecycle: list, create, join, leave, ready state, host kick, optional auto-refresh |
| `IVXMatchmakingManager` | Quick-match / ranked search, progress callbacks, match-found payload |
| `IVXLocalMultiplayerManager` | Same-device play: hot-seat turns, rounds, timers, split-screen viewport helpers |

---

## 3. Game Mode Manager

`IVXGameModeManager` is the hub for **mode selection**, **player management**, and **match lifecycle**. It exposes the current `IVXMatchConfig`, enumerates `IVXPlayerSlot` entries, and transitions `Phase` through lobby, loading, in-progress, and finished states. `Instance` auto-creates a dedicated GameObject with `DontDestroyOnLoad` when none exists.

### Properties

| Member | Description |
|--------|-------------|
| `Instance` (static) | Singleton accessor; creates `[IVXGameModeManager]` if missing |
| `CurrentConfig` | Active `IVXMatchConfig` |
| `CurrentMode` | `IVXGameMode` derived from `CurrentConfig` |
| `Phase` | Current `IVXMatchPhase` |
| `Players` | `IReadOnlyList<IVXPlayerSlot>` |
| `PlayerCount` | Occupied slot count |
| `CanStartMatch` | Minimum players met and humans ready (bots count as ready when allowed) |
| `IsHost` | Whether the local client is host |
| `LocalPlayer` | Primary local slot (first player) |

### Events

| Event | Description |
|-------|-------------|
| `OnModeChanged` | Mode changed after `SelectMode` / `SetConfig` |
| `OnPlayerAdded` / `OnPlayerRemoved` | Slot list mutations |
| `OnPlayerReadyChanged` | Ready flag updated for a slot |
| `OnAllPlayersReady` | Fired when `CanStartMatch` becomes true |
| `OnMatchPhaseChanged` | Phase transition (previous, next) |
| `OnConfigChanged` | New config applied |

### Methods (signatures)

| Method | Role |
|--------|------|
| `SelectMode(IVXGameMode mode, int maxPlayers = 0)` | Apply mode-specific defaults |
| `SetConfig(IVXMatchConfig config)` | Replace config and reset lobby state |
| `AddLocalPlayer(string displayName, int inputDeviceIndex = -1)` | Add same-device player |
| `AddRemotePlayer(string userId, string displayName)` | Add online player |
| `AddBot(string botName)` | Add bot if `CurrentConfig.AllowBots` |
| `RemovePlayer(int slotIndex)` | Remove slot (slot 0 protected) |
| `SetPlayerReady(int slotIndex, bool ready)` | Toggle ready |
| `SetPlayerTeam(int slotIndex, IVXTeam team)` | Team assignment |
| `SetPhase(IVXMatchPhase phase)` | Force phase |
| `StartMatch()` | Begin load (requires `CanStartMatch`) |
| `BeginGameplay()` | Enter `InProgress` |
| `EndMatch()` | Enter `Finished` |
| `ReturnToLobby()` | Reset readiness; lobby phase |
| `Reset()` | Clear config, players, phase |
| `GetTeamPlayers(IVXTeam team)` | Players on a team |
| `GetLocalPlayers()` / `GetRemotePlayers()` | Partition by `IsLocal` |
| `IsModeAvailable(IVXGameMode mode)` | Network reachability for online modes |

### Example: mode selection and lobby readiness

```csharp
using IntelliVerseX.GameModes;
using UnityEngine;

public class MatchSetupUi : MonoBehaviour
{
    private void OnEnable()
    {
        var gm = IVXGameModeManager.Instance;
        gm.OnModeChanged += OnModeChanged;
        gm.OnPlayerAdded += OnSlotChanged;
        gm.OnAllPlayersReady += OnEveryoneReady;
        gm.OnMatchPhaseChanged += OnPhase;
    }

    private void OnDisable()
    {
        var gm = IVXGameModeManager.Instance;
        gm.OnModeChanged -= OnModeChanged;
        gm.OnPlayerAdded -= OnSlotChanged;
        gm.OnAllPlayersReady -= OnEveryoneReady;
        gm.OnMatchPhaseChanged -= OnPhase;
    }

    public void ChooseOnlineVersus()
    {
        IVXGameModeManager.Instance.SelectMode(IVXGameMode.OnlineVersus, maxPlayers: 2);
    }

    public void ChooseLocalParty()
    {
        IVXGameModeManager.Instance.SelectMode(IVXGameMode.LocalMultiplayer, maxPlayers: 4);
        IVXGameModeManager.Instance.AddLocalPlayer("Player 2", inputDeviceIndex: 1);
    }

    private void OnModeChanged(IVXGameMode mode) => Debug.Log($"Mode: {mode}");
    private void OnSlotChanged(IVXPlayerSlot s) => Debug.Log($"Joined: {s.DisplayName}");
    private void OnEveryoneReady(IVXMatchConfig cfg) => Debug.Log("Ready to start.");
    private void OnPhase(IVXMatchPhase oldPhase, IVXMatchPhase newPhase) =>
        Debug.Log($"Phase {oldPhase} -> {newPhase}");
}
```

### Example: custom config, bots, and lifecycle

```csharp
using IntelliVerseX.GameModes;

public static class MatchFlow
{
    public static void StartCasualWithBots()
    {
        var cfg = IVXMatchConfig.Local(maxPlayers: 4);
        cfg.AllowBots = true;
        cfg.MinPlayers = 2;
        cfg.AutoStartWhenReady = true;
        var gm = IVXGameModeManager.Instance;
        gm.SetConfig(cfg);
        gm.AddBot("QuizBot");
        gm.SetPlayerReady(0, true);
    }

    public static void HostStartsLoading()
    {
        var gm = IVXGameModeManager.Instance;
        if (!gm.CanStartMatch) return;
        gm.StartMatch();
    }

    public static void SceneLoaded()
    {
        IVXGameModeManager.Instance.BeginGameplay();
    }

    public static void ShowResults()
    {
        IVXGameModeManager.Instance.EndMatch();
    }
}
```

---

## 4. Online Lobby

`IVXLobbyManager` handles **room CRUD-style operations** (create, join, list, leave), optional **auto-refresh** of the browser list, and **lobby events** (ready, kick, synthetic join/leave when not on Nakama). It keeps `CurrentRoom` and `CachedRooms` in sync with the last successful refresh. When Nakama is linked through `IVXNakamaManager`, list/create/join use realtime match APIs; otherwise mock data supports UI development.

### Properties

| Member | Description |
|--------|-------------|
| `Instance` (static) | Singleton |
| `IsInRoom` | Client joined a room |
| `CurrentRoom` | `IVXRoomInfo` or null |
| `CachedRooms` | Last listing (`IReadOnlyList<IVXRoomInfo>`) |
| `IsRefreshing` | List request in flight |

### Events

| Event | Description |
|-------|-------------|
| `OnRoomListUpdated` | Listing refreshed |
| `OnRoomJoined` | Join completed (`IVXJoinRoomResponse`) |
| `OnRoomCreated` | Create completed (`IVXCreateRoomResponse`) |
| `OnRoomLeft` | Left room |
| `OnLobbyEvent` | Structured `IVXLobbyEvent` (ready, kick, etc.) |
| `OnError` | Human-readable error string |

### Methods (signatures)

| Method | Role |
|--------|------|
| `RefreshRoomList(IVXRoomFilter filter = null)` | Fetch/update `CachedRooms` |
| `StartAutoRefresh(float intervalSeconds = 5f, IVXRoomFilter filter = null)` | Periodic refresh |
| `StopAutoRefresh()` | Stop coroutine |
| `CreateRoom(IVXCreateRoomRequest request)` | Create and enter as host |
| `JoinRoom(IVXJoinRoomRequest request)` | Join by id (+ password) |
| `LeaveRoom()` | Exit; calls `IVXGameModeManager.ReturnToLobby()` |
| `SetReady(bool ready)` | Local ready; forwards to game mode manager |
| `KickPlayer(int slotIndex)` | Host-only; removes remote slot |

### Example: browse, create, and join

```csharp
using System.Collections.Generic;
using IntelliVerseX.GameModes;
using UnityEngine;

public class LobbyBrowser : MonoBehaviour
{
    private void Start()
    {
        var lobby = IVXLobbyManager.Instance;
        lobby.OnRoomListUpdated += OnList;
        lobby.OnRoomCreated += OnCreated;
        lobby.OnRoomJoined += OnJoined;
        lobby.OnError += Debug.LogWarning;

        lobby.RefreshRoomList(new IVXRoomFilter { Limit = 15, OnlyAvailable = true });
        lobby.StartAutoRefresh(intervalSeconds: 5f);
    }

    private void OnList(List<IVXRoomInfo> rooms)
    {
        foreach (var r in rooms)
            Debug.Log($"{r.RoomName} {r.PlayerCount}/{r.MaxPlayers}");
    }

    public void HostCasual()
    {
        var req = new IVXCreateRoomRequest
        {
            RoomName = "Open Trivia",
            Config = IVXMatchConfig.OnlineVersus(2),
            Password = null
        };
        IVXLobbyManager.Instance.CreateRoom(req);
    }

    public void JoinSelected(string roomId, string password)
    {
        IVXLobbyManager.Instance.JoinRoom(new IVXJoinRoomRequest
        {
            RoomId = roomId,
            Password = password
        });
    }

    private void OnCreated(IVXCreateRoomResponse res)
    {
        if (res.Success) Debug.Log($"Hosted: {res.RoomId}");
    }

    private void OnJoined(IVXJoinRoomResponse res)
    {
        if (res.Success) Debug.Log($"Players in room: {res.Players?.Count ?? 0}");
    }
}
```

### Example: ready and host moderation

```csharp
using IntelliVerseX.GameModes;

public class LobbyHud
{
    public void ToggleReady(bool ready) => IVXLobbyManager.Instance.SetReady(ready);

    public void RemoveGuest(int slotIndex)
    {
        if (IVXGameModeManager.Instance.IsHost)
            IVXLobbyManager.Instance.KickPlayer(slotIndex);
    }
}
```

---

## 5. Matchmaking

`IVXMatchmakingManager` runs **search** loops (mock or Nakama matchmaker), surfaces **progress** via elapsed time, and delivers an **`IVXMatchFoundResult`** when pairing succeeds. `QuickMatch()` and `RankedMatch()` are thin wrappers over `StartSearch` with `IVXMatchConfig.OnlineVersus()` and `IVXMatchConfig.Ranked()` respectively. Cancelling restores lobby phase on the game mode manager.

### Properties

| Member | Description |
|--------|-------------|
| `Instance` (static) | Singleton |
| `IsSearching` | Search active |
| `SearchElapsed` | Seconds since search start |
| `MaxSearchTime` | Timeout (default 60s) |
| `TicketId` | Active ticket id when using Nakama |

### Events

| Event | Description |
|-------|-------------|
| `OnSearchStarted` | Search began |
| `OnSearchProgress` | `float` elapsed seconds |
| `OnMatchFound` | `IVXMatchFoundResult` |
| `OnSearchCancelled` | Reason string (user cancel or timeout) |
| `OnError` | Error string |

### Methods (signatures)

| Method | Role |
|--------|------|
| `StartSearch(IVXMatchConfig config = null)` | Begin search; sets config and `Matchmaking` phase |
| `CancelSearch()` | Stop search and clear ticket |
| `QuickMatch()` | `StartSearch(IVXMatchConfig.OnlineVersus())` |
| `RankedMatch()` | `StartSearch(IVXMatchConfig.Ranked())` |

### Example: search UI

```csharp
using IntelliVerseX.GameModes;
using UnityEngine;

public class MatchmakingUi : MonoBehaviour
{
    private void OnEnable()
    {
        var mm = IVXMatchmakingManager.Instance;
        mm.OnSearchStarted += () => ShowSpinner(true);
        mm.OnSearchProgress += t => UpdateTimerLabel(t);
        mm.OnMatchFound += OnFound;
        mm.OnSearchCancelled += reason => { ShowSpinner(false); Debug.Log(reason); };
    }

    public void FindQuickGame() => IVXMatchmakingManager.Instance.QuickMatch();

    public void FindRanked() => IVXMatchmakingManager.Instance.RankedMatch();

    public void Abort() => IVXMatchmakingManager.Instance.CancelSearch();

    private void OnFound(IVXMatchFoundResult r)
    {
        ShowSpinner(false);
        Debug.Log($"Matched vs {r.OpponentDisplayName} ({r.MatchId})");
    }
}
```

### Example: custom config search

```csharp
using IntelliVerseX.GameModes;

public static class CustomQueue
{
    public static void QueueCoop()
    {
        var cfg = IVXMatchConfig.OnlineCoop(4);
        IVXMatchmakingManager.Instance.MaxSearchTime = 90f;
        IVXMatchmakingManager.Instance.StartSearch(cfg);
    }
}
```

---

## 6. Local Multiplayer

`IVXLocalMultiplayerManager` layers **hot-seat turns**, **round counting**, and optional **per-turn timers** on top of slots already registered in `IVXGameModeManager`. For simultaneous same-screen play, start a session with `hotSeat: false` (no turn rotation). **Split-screen** helpers assign normalized `Camera.rect` values for two-, three-, or four-player layouts.

### Properties

| Member | Description |
|--------|-------------|
| `Instance` (static) | Singleton |
| `IsSessionActive` | Session running |
| `CurrentTurnIndex` | Index within local players |
| `CurrentRound` | 1-based round counter |
| `CurrentTurnPlayer` | Active `IVXPlayerSlot` |
| `IsHotSeat` | Turn-based mode flag |
| `TurnTimeRemaining` | Seconds left, or -1 if no limit |

### Events

| Event | Description |
|-------|-------------|
| `OnTurnStarted` / `OnTurnEnded` | Turn boundaries |
| `OnRoundCompleted` | `int` completed round index |
| `OnLocalSessionStarted` | `List<IVXPlayerSlot>` participants |
| `OnLocalSessionEnded` | Session stopped |

### Methods (signatures)

| Method | Role |
|--------|------|
| `StartSession(bool hotSeat = true, float turnTimeLimitSeconds = 0f)` | Needs ≥2 local players |
| `EndSession()` | Finish session; sets phase `Finished` |
| `EndTurn()` | Advance turn / round |
| `JumpToPlayer(int slotIndex)` | Force active player |
| `CalculateSplitScreenRects(int playerCount)` | Static; normalized rects |
| `ApplySplitScreen(Camera[] cameras)` | Static; apply rects |

### Example: hot-seat trivia

```csharp
using IntelliVerseX.GameModes;
using UnityEngine;

public class LocalTriviaRound : MonoBehaviour
{
    private void StartParty()
    {
        var gm = IVXGameModeManager.Instance;
        gm.SelectMode(IVXGameMode.LocalMultiplayer, 4);
        gm.AddLocalPlayer("P2", 1);
        gm.AddLocalPlayer("P3", 2);

        var local = IVXLocalMultiplayerManager.Instance;
        local.OnTurnStarted += p => HighlightSeat(p.SlotIndex);
        local.OnRoundCompleted += completedRound => Debug.Log($"Round {completedRound} done");
        local.StartSession(hotSeat: true, turnTimeLimitSeconds: 20f);
    }

    public void PlayerPressedDone() => IVXLocalMultiplayerManager.Instance.EndTurn();
}
```

### Example: split-screen cameras

```csharp
using IntelliVerseX.GameModes;
using UnityEngine;

public class SplitScreenSetup : MonoBehaviour
{
    [SerializeField] private Camera[] _playerCameras;

    private void OnEnable()
    {
        IVXLocalMultiplayerManager.ApplySplitScreen(_playerCameras);
    }

    private void LogLayouts(int count)
    {
        Rect[] rects = IVXLocalMultiplayerManager.CalculateSplitScreenRects(count);
        for (int i = 0; i < rects.Length; i++)
            Debug.Log($"P{i}: {rects[i]}");
    }
}
```

---

## 7. Data Models

### `IVXGameMode` (enum)

| Value | Typical use |
|-------|-------------|
| `Solo` | Single player |
| `LocalMultiplayer` | Shared device |
| `OnlineMultiplayer` | General online bucket |
| `OnlineCoop` | Cooperative online |
| `OnlineVersus` | Competitive online |
| `TurnBased` | Async / turn-style matchmaking filters |
| `RankedMatch` | Skill-rated queue |

### `IVXMatchPhase` (enum)

Core lifecycle: `None` → `Lobby` → `Matchmaking` (optional) → `Loading` → `InProgress` → `Finished`. The SDK also defines `Paused` and `Disconnected` for extended state machines.

### `IVXMatchConfig` (class)

Serializable match description: `Mode`, `MinPlayers`, `MaxPlayers`, `TeamCount`, `MaxPlayersPerTeam`, `AllowBots`, `AllowSpectators`, `MatchDurationSeconds`, `Transport` (`IVXNetworkTransport`), `RoomLabel`, `IsPublic`, `Password`, `CustomProperties`, `TurnTimeLimitSeconds`, `AutoStartWhenReady`, `CountdownSeconds`. Factory helpers: `Solo()`, `Local(int maxPlayers)`, `OnlineVersus(int maxPlayers, IVXNetworkTransport transport = NakamaRealtime)`, `OnlineCoop(int maxPlayers)`, `Ranked()`. Computed: `RequiresNetwork`, `IsTeamBased`.

### `IVXPlayerSlot` (class)

`SlotIndex`, `DisplayName`, `UserId`, `IsHuman`, `IsLocal`, `IsHost`, `Team`, `ReadyState` (`IVXPlayerReadyState`), `AvatarKey`, `CustomData`, `InputDeviceIndex`.

### `IVXRoomInfo` (class)

`RoomId`, `RoomName`, `HostName`, `PlayerCount`, `MaxPlayers`, `Mode`, `IsPasswordProtected`, `IsInProgress`, `CreatedAt`, `PingMs`, `CustomProperties`, `HasSpace`.

### `IVXMatchFoundResult` (class)

`MatchId`, `OpponentUserId`, `OpponentDisplayName`, `OpponentRating`, `EstimatedPingMs`, `Transport`.

### Related request/event types (lobby)

| Type | Role |
|------|------|
| `IVXCreateRoomRequest` | `RoomName`, `Config`, `Password` |
| `IVXJoinRoomRequest` | `RoomId`, `Password` |
| `IVXRoomFilter` | Mode filter, availability, limits, property filters |
| `IVXLobbyEvent` / `IVXLobbyEventType` | Structured lobby notifications |

---

## 8. Events / Callbacks Reference

| Source | Event | Delegate summary |
|--------|-------|------------------|
| `IVXGameModeManager` | `OnModeChanged` | `IVXGameMode` |
| | `OnPlayerAdded` / `OnPlayerRemoved` | `IVXPlayerSlot` |
| | `OnPlayerReadyChanged` | `IVXPlayerSlot` |
| | `OnAllPlayersReady` | `IVXMatchConfig` |
| | `OnMatchPhaseChanged` | `IVXMatchPhase` old, `IVXMatchPhase` new |
| | `OnConfigChanged` | `IVXMatchConfig` |
| `IVXLobbyManager` | `OnRoomListUpdated` | `List<IVXRoomInfo>` |
| | `OnRoomJoined` | `IVXJoinRoomResponse` |
| | `OnRoomCreated` | `IVXCreateRoomResponse` |
| | `OnRoomLeft` | No args |
| | `OnLobbyEvent` | `IVXLobbyEvent` |
| | `OnError` | `string` |
| `IVXMatchmakingManager` | `OnSearchStarted` | No args |
| | `OnSearchProgress` | `float` elapsed |
| | `OnMatchFound` | `IVXMatchFoundResult` |
| | `OnSearchCancelled` | `string` reason |
| | `OnError` | `string` |
| `IVXLocalMultiplayerManager` | `OnTurnStarted` / `OnTurnEnded` | `IVXPlayerSlot` |
| | `OnRoundCompleted` | `int` round |
| | `OnLocalSessionStarted` | `List<IVXPlayerSlot>` |
| | `OnLocalSessionEnded` | No args |

---

## 9. Error Handling

- **Game mode manager**: `AddLocalPlayer`, `AddRemotePlayer`, and `AddBot` return `null` when the roster is full, config is missing, or bots are disallowed. `StartMatch()` logs and returns when `CanStartMatch` is false. `RemovePlayer` ignores invalid indices and never removes slot `0` (primary local player).
- **Lobby manager**: Invalid create/join requests invoke `OnError` with a short message (`Invalid create room request.`, `Invalid room ID.`, `Room … not found.`, `Password required.`, `Room is full.`, Nakama connectivity errors). Always subscribe to `OnError` for UI toasts.
- **Matchmaking**: `StartSearch` warns if already searching; `CancelSearch` is safe when idle. Timeout and user cancel both route through `OnSearchCancelled`.
- **Local multiplayer**: `StartSession` warns and returns if fewer than two local players exist.

```csharp
IVXLobbyManager.Instance.OnError += msg => ShowToast(msg);
IVXGameModeManager.Instance.OnConfigChanged += cfg =>
{
    if (cfg == null) return;
    if (!cfg.AllowBots) HideAddBotButton();
};
```

---

## 10. Cross-Platform Status

| Capability | Standalone / Mobile | WebGL | Notes |
|------------|---------------------|-------|-------|
| `IVXGameModeManager` | Supported | Supported | Pure client state |
| `IVXLocalMultiplayerManager` | Supported | Supported | Input indices depend on Unity input stack |
| Split-screen helpers | Supported | Supported | Uses `Camera.rect` |
| `IsModeAvailable` (online modes) | Uses `Application.internetReachability` | Same | Does not guarantee Nakama auth |
| `IVXLobbyManager` listing / rooms | Full with Nakama + socket | Same constraint as Nakama build | Without `INTELLIVERSEX_HAS_NAKAMA`, mock rooms |
| `IVXMatchmakingManager` | Full with Nakama matchmaker | Same | Mock match after random delay offline |

---

## 11. See Also

- [Backend module](backend.md) — Nakama client, session, and socket setup used by lobby and matchmaking
- [Social module](social.md) — Friends and presence that often pair with online modes
- [Discord module](discord.md) — Rich presence and Discord lobbies alongside IVX rooms
- [Hiro module](hiro.md) — Server metagame systems (ranked progression, challenges) complementary to match results
