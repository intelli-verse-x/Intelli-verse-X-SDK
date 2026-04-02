# Multiplayer Integration

Step-by-step integration for the IntelliVerseX **Game Modes / Multiplayer** module (`IntelliVerseX.GameModes`). Four singleton managers coordinate mode selection, online lobbies, matchmaking, and same-device play.

---

## Overview

The module splits responsibilities across:

| Manager | Responsibility |
|---------|------------------|
| `IVXGameModeManager` | Mode selection, match configuration, player slots, readiness, teams, and match lifecycle |
| `IVXLobbyManager` | Online rooms: list, create, join, leave, ready, host kick, optional auto-refresh |
| `IVXMatchmakingManager` | Search, cancel, quick match, and ranked match entry points |
| `IVXLocalMultiplayerManager` | Hot-seat turns, turn timers, and split-screen camera helpers |

**Game modes** (`IVXGameMode`): `Solo`, `LocalMultiplayer`, `OnlineVersus`, `OnlineCoop`, `RankedMatch`, `TurnBased`, `OnlineMultiplayer`.

**Local play** uses multiple human slots on one device (`AddLocalPlayer`, split-screen, optional hot-seat via `IVXLocalMultiplayerManager`).

**Online play** uses `IVXLobbyManager` for rooms and `IVXMatchmakingManager` for automated pairing; remote participants appear in `IVXGameModeManager` via `AddRemotePlayer` (typically after join or match-found flows).

**Match phases** (`IVXMatchPhase`) you will drive or observe through the APIs below: `None`, `Lobby`, `Matchmaking`, `Loading`, `InProgress`, `Finished`.

!!! tip "Singleton access"
    Each manager exposes `Instance` and auto-creates a dedicated `GameObject` with `DontDestroyOnLoad` when first accessed. You can also place components in the scene if you prefer explicit wiring.

---

## Prerequisites

1. **Package / assembly** — Include the SDK assembly that defines `IntelliVerseX.GameModes` (typically `IntelliVerseX.Multiplayer`) in your game project.
2. **Namespace** — `using IntelliVerseX.GameModes;`
3. **Backend (online)** — For real Nakama-backed lobbies and matchmaking, configure the IntelliVerseX backend module and define `INTELLIVERSEX_HAS_NAKAMA` where applicable. Without it, lobby and matchmaking still run using built-in fallbacks suitable for editor testing.
4. **UI layer** — Plan screens for mode pickers, lobby lists, searching state, and in-match HUD; all manager calls are safe to invoke from UI event handlers.

!!! note "API surface in this guide"
    This guide documents only the public methods listed for the four managers. Types such as `IVXMatchConfig`, `IVXCreateRoomRequest`, and `IVXJoinRoomRequest` are used where those methods require them.

---

## Step 1 — Choose a game mode

Call `IVXGameModeManager.Instance.SelectMode(IVXGameMode, maxPlayers)` to apply mode-specific defaults, or `SetConfig(IVXMatchConfig)` for a fully custom configuration.

Use `IsModeAvailable(IVXGameMode)` before showing or enabling online modes when the device may be offline.

### Examples

**Solo**

```csharp
var gm = IVXGameModeManager.Instance;
if (!gm.IsModeAvailable(IVXGameMode.Solo)) { /* handle */ return; }
gm.SelectMode(IVXGameMode.Solo, maxPlayers: 0);
```

**Local multiplayer (same device)**

```csharp
IVXGameModeManager.Instance.SelectMode(IVXGameMode.LocalMultiplayer, maxPlayers: 4);
```

**Online versus**

```csharp
if (!IVXGameModeManager.Instance.IsModeAvailable(IVXGameMode.OnlineVersus)) return;
IVXGameModeManager.Instance.SelectMode(IVXGameMode.OnlineVersus, maxPlayers: 2);
```

**Ranked**

```csharp
if (!IVXGameModeManager.Instance.IsModeAvailable(IVXGameMode.RankedMatch)) return;
IVXGameModeManager.Instance.SelectMode(IVXGameMode.RankedMatch, maxPlayers: 0);
```

**Custom config**

```csharp
var config = new IVXMatchConfig { Mode = IVXGameMode.OnlineCoop, MaxPlayers = 4, MinPlayers = 2 };
IVXGameModeManager.Instance.SetConfig(config);
```

After `SelectMode` or `SetConfig`, build your slot list with `AddLocalPlayer`, `AddRemotePlayer`, and `AddBot` as required (see Steps 2–8).

---

## Step 2 — Solo mode (simplest setup)

Solo is the smallest integration path: one local human slot and a short lifecycle.

1. `SelectMode(IVXGameMode.Solo, 0)` (or `SetConfig` equivalent).
2. `SetConfig` / `SelectMode` already seeds a primary local player internally; add more locals only if your design needs them.
3. When your scene is ready to load gameplay assets, call `StartMatch()` so the flow enters the loading stage, then `BeginGameplay()` when simulation should run.
4. When the round ends, call `EndMatch()`. To play again without changing mode, use `ReturnToLobby()` and repeat ready/start, or `Reset()` to clear everything.

```csharp
var gm = IVXGameModeManager.Instance;
gm.SelectMode(IVXGameMode.Solo, 0);
gm.SetPlayerReady(0, true);
gm.StartMatch();
// ... after load completes ...
gm.BeginGameplay();
// ... round over ...
gm.EndMatch();
```

---

## Step 3 — Local multiplayer (hot-seat and split-screen)

Same-device multiplayer combines `IVXGameModeManager` (slots) with `IVXLocalMultiplayerManager` (turns and viewports).

### Register players

1. `SelectMode(IVXGameMode.LocalMultiplayer, maxPlayers)`.
2. For each couch player, call `AddLocalPlayer(displayName, inputDeviceIndex)` until you reach the desired count (respect `CurrentConfig.MaxPlayers`).
3. Optional: `SetPlayerTeam` for team layouts.

### Hot-seat (turns)

1. `StartSession(hotSeat: true, turnTimeLimitSeconds)` — pass a positive limit in seconds for timed turns, or `0` for no timer.
2. When the active player finishes, call `EndTurn()`.
3. Use `JumpToPlayer(slotIndex)` for debug tools or sanctioned skips.
4. `EndSession()` when the couch session should end.

### Split-screen (simultaneous views)

1. `StartSession(hotSeat: false, turnTimeLimitSeconds: 0f)` if you want a session flag without turn enforcement, or manage cameras independently after adding locals.
2. Build a `Camera[]` in slot order.
3. `IVXLocalMultiplayerManager.CalculateSplitScreenRects(playerCount)` returns normalized `Rect` values.
4. `IVXLocalMultiplayerManager.ApplySplitScreen(cameras)` assigns each camera’s `rect`.

```csharp
var gm = IVXGameModeManager.Instance;
gm.SelectMode(IVXGameMode.LocalMultiplayer, 4);
gm.AddLocalPlayer("P2"); // beyond the default first local slot
var locals = gm.GetLocalPlayers();
var cams = new[] { player1Cam, player2Cam };
IVXLocalMultiplayerManager.ApplySplitScreen(cams);
IVXLocalMultiplayerManager.Instance.StartSession(hotSeat: true, turnTimeLimitSeconds: 30f);
```

---

## Step 4 — Online lobby (create, join, auto-refresh, ready)

`IVXLobbyManager` handles room listing and membership. It works with `IVXGameModeManager` for config and slots.

### Refresh and browse

- `RefreshRoomList()` — single fetch.
- `StartAutoRefresh(intervalSeconds, filter)` — periodic refresh; pair with `StopAutoRefresh()` when leaving the browser UI.

### Create a room

Build an `IVXCreateRoomRequest` with at least `Config` set (and optional `RoomName`, `Password`). Then:

```csharp
IVXLobbyManager.Instance.CreateRoom(new IVXCreateRoomRequest
{
    RoomName = "Friday Quiz",
    Config = IVXMatchConfig.OnlineVersus(2)
});
```

### Join a room

```csharp
IVXLobbyManager.Instance.JoinRoom(new IVXJoinRoomRequest
{
    RoomId = selectedRoomId,
    Password = passwordOrNull
});
```

### Ready state and host actions

- `SetReady(bool)` — toggles readiness for the local player (slot 0) through `IVXGameModeManager.SetPlayerReady`.
- `KickPlayer(slotIndex)` — host-only removal of another slot; invalid indices and non-host callers are ignored by design.

### Leave

`LeaveRoom()` clears room membership and calls `IVXGameModeManager.Instance.ReturnToLobby()`.

!!! warning "Create request validation"
    `CreateRoom` requires a non-null `request.Config`. Omitting it surfaces an error through the lobby error path rather than creating a room.

---

## Step 5 — Quick matchmaking

`IVXMatchmakingManager` drives search and cancellation.

1. **Configurable search** — `StartSearch(IVXMatchConfig)` with the rules you want (defaults apply if you pass `null` per implementation).
2. **Shortcut** — `QuickMatch()` starts a search with the default online-versus configuration.
3. **While searching** — show UI; subscribe to manager events in your code if you use them (not required for calling the listed methods).
4. **Cancel** — `CancelSearch()` stops the active search and returns flow toward lobby-style state.
5. **Outcome** — On success, the implementation adds the opponent via `AddRemotePlayer` and advances toward loading; on timeout or cancel, search ends without a match.

Typical usage:

```csharp
IVXMatchmakingManager.Instance.QuickMatch();
// later, if the player backs out:
IVXMatchmakingManager.Instance.CancelSearch();
```

---

## Step 6 — Ranked matchmaking

Ranked entry mirrors quick match but uses the ranked preset:

```csharp
IVXMatchmakingManager.Instance.RankedMatch();
```

You may instead call `StartSearch(IVXMatchConfig.Ranked())` (or an equivalent `IVXMatchConfig` you build) if you need explicit control while still using the same manager API.

Cancel ranked searches with `CancelSearch()` the same way as quick match.

---

## Step 7 — Match lifecycle (loading → gameplay → end → lobby)

Use `IVXGameModeManager` for the canonical lifecycle:

| Stage | What to call |
|-------|----------------|
| Lobby / staging | `ReturnToLobby()` resets human readiness while keeping config; `Reset()` clears everything including phase |
| Enough players ready | `SetPlayerReady` until `CanStartMatch` is satisfied (bots may already count as ready when allowed) |
| Start loading | `StartMatch()` |
| Begin simulation | `BeginGameplay()` after assets and sync are ready |
| End of round | `EndMatch()` |
| Play again | `ReturnToLobby()` to re-ready, or `Reset()` to tear down |

Online flows align with this: after `JoinRoom` / matchmaking adds `AddRemotePlayer`, drive the same `StartMatch` → `BeginGameplay` → `EndMatch` sequence your gameplay already uses for solo/local.

```csharp
void OnAllHumansReady()
{
    IVXGameModeManager.Instance.StartMatch();
}

void OnLoadPipelineComplete()
{
    IVXGameModeManager.Instance.BeginGameplay();
}

void OnRoundComplete()
{
    IVXGameModeManager.Instance.EndMatch();
}

void OnPlayAgain()
{
    IVXGameModeManager.Instance.ReturnToLobby();
}
```

---

## Step 8 — Teams and bots

### Teams

`SetPlayerTeam(int slotIndex, IVXTeam team)` assigns cooperative or competitive groupings. Query participants with `GetTeamPlayers(IVXTeam)`.

```csharp
var gm = IVXGameModeManager.Instance;
gm.SetPlayerTeam(0, IVXTeam.TeamA);
gm.SetPlayerTeam(1, IVXTeam.TeamB);
var teamA = gm.GetTeamPlayers(IVXTeam.TeamA);
```

### Bots

`AddBot(string botName)` fills a slot when `CurrentConfig.AllowBots` is true. Bots are created in a ready state suitable for auto-start rules.

### Partitioning slots

- `GetLocalPlayers()` — same-device humans.
- `GetRemotePlayers()` — networked humans.
- `RemovePlayer(int slotIndex)` — drop a slot (slot `0`, the primary local player, cannot be removed).

---

## Troubleshooting

| Symptom | Things to check |
|---------|------------------|
| Online mode buttons do nothing offline | Call `IsModeAvailable` for `OnlineVersus`, `OnlineCoop`, `RankedMatch`, `OnlineMultiplayer` before exposing UI. |
| `StartMatch` logs a warning | Ensure minimum players and readiness: use `SetPlayerReady` for humans; verify `AddLocalPlayer` / `AddRemotePlayer` counts vs `MinPlayers` / `MaxPlayers` on `IVXMatchConfig`. |
| `AddBot` returns null | Confirm `AllowBots` on the active `IVXMatchConfig` and that you have free slots under `MaxPlayers`. |
| `CreateRoom` fails immediately | `IVXCreateRoomRequest.Config` must be non-null. |
| `JoinRoom` errors | Verify `IVXJoinRoomRequest.RoomId`, password when `IsPasswordProtected`, and that the room has space. |
| Local session will not start | `IVXLocalMultiplayerManager.StartSession` requires at least two local players from `GetLocalPlayers()`. |
| Split-screen looks wrong | Pass one `Camera` per local player in slot order to `ApplySplitScreen`; rect layout supports 1–4 players. |
| Stuck in search | Invoke `CancelSearch()`; ensure you only call `StartSearch` / `QuickMatch` / `RankedMatch` once at a time. |
| After leave, state feels stale | `LeaveRoom` already calls `ReturnToLobby()`; use `Reset()` if you need a full teardown. |

---

## See also

- [Multiplayer & Game Modes (module reference)](../modules/multiplayer.md) — full API tables, events, and configuration types
- [Nakama Integration](nakama-integration.md) — backend connection, socket, and realtime prerequisites for production online play
- [Backend configuration](../configuration/backend-config.md) — server endpoints and feature flags
- [Troubleshooting: runtime issues](../troubleshooting/runtime-issues.md) — general SDK diagnostics

---

*All manager types in this guide live in the namespace `IntelliVerseX.GameModes`.*
