# Multiplayer

**Skill ID:** `ivx-multiplayer`

---
name: ivx-multiplayer
description: >-
  Add multiplayer features to IntelliVerseX SDK games including lobbies, matchmaking,
  game modes, and real-time networking. Use when the user says "add multiplayer",
  "create lobby", "set up matchmaking", "add game modes", "real-time multiplayer",
  "online mode", "local multiplayer", "split screen", "Nakama socket", or needs
  help with any multiplayer integration.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The SDK provides a unified multiplayer layer over Nakama real-time sockets. It covers game mode selection, lobby management, matchmaking, and in-match networking. The API surface is consistent across platforms, though real-time socket features are fully implemented only in Unity, JS, and Java.

---

## When to Use

Ask your AI agent any of these:

- "Add online multiplayer to my trivia game"
- "Create a lobby system where players can invite friends"
- "Set up ranked matchmaking with ELO rating"
- "Add real-time score syncing between 4 players"
- "Handle player disconnection and reconnection"
- "Set up local split-screen multiplayer"

---

## What the Agent Does

```mermaid
flowchart TD
    A[You: "Add multiplayer"] --> B[Agent loads ivx-multiplayer skill]
    B --> C[Enables multiplayer in IVXBootstrapConfig]
    C --> D{Which mode?}
    D -->|Lobby| E[Creates lobby system]
    D -->|Quick Match| F[Configures matchmaker]
    D -->|Ranked| G[Adds skill-based matching]
    D -->|Local| H[Sets up same-device play]
    E --> I[Wires real-time networking]
    F --> I
    G --> I
    I --> J[Implements match lifecycle]
```

---



## 1. Game Modes

### Supported Modes

| Mode | Description | Networking |
|------|-------------|-----------|
| `Solo` | Single-player, local only | None |
| `LocalMultiplayer` | Same device, multiple players | None (Unity only) |
| `OnlineLobby` | Player creates/joins a room, waits, then starts | Nakama socket |
| `QuickMatch` | Auto-matched into a game immediately | Nakama matchmaker |
| `RankedMatch` | Skill-based matchmaking with rating | Nakama matchmaker + leaderboard |

### IVXGameModeManager

```csharp
using IntelliVerseX.Multiplayer;

IVXGameModeManager.Instance.SetMode(GameMode.OnlineLobby);

IVXGameModeManager.Instance.OnModeChanged += (newMode) =>
{
    Debug.Log($"[GameMode] Switched to {newMode}");
};

GameMode current = IVXGameModeManager.Instance.CurrentMode;
```

---

## 2. Lobby System

### IVXLobbyManager

Lobbies are Nakama matches in a "waiting" state. The lobby creator controls when the game starts.

#### Creating a Lobby

```csharp
using IntelliVerseX.Multiplayer;

var options = new LobbyOptions
{
    MaxPlayers = 4,
    IsPrivate = false,
    Label = "trivia-room",
    Metadata = new Dictionary<string, string>
    {
        { "category", "science" },
        { "difficulty", "medium" },
    },
};

IVXLobby lobby = await IVXLobbyManager.Instance.CreateLobbyAsync(options);
Debug.Log($"Lobby created: {lobby.Id} — share code: {lobby.JoinCode}");
```

#### Joining a Lobby

```csharp
// By join code (user-friendly)
IVXLobby lobby = await IVXLobbyManager.Instance.JoinLobbyByCodeAsync("ABC123");

// By lobby ID (direct)
IVXLobby lobby = await IVXLobbyManager.Instance.JoinLobbyAsync(lobbyId);
```

#### Lobby Events

```csharp
IVXLobbyManager.Instance.OnPlayerJoined += (player) =>
    Debug.Log($"{player.DisplayName} joined");

IVXLobbyManager.Instance.OnPlayerLeft += (player) =>
    Debug.Log($"{player.DisplayName} left");

IVXLobbyManager.Instance.OnLobbyUpdated += (lobby) =>
    RefreshLobbyUI(lobby);

IVXLobbyManager.Instance.OnGameStarting += (countdown) =>
    ShowCountdown(countdown);
```

#### Starting the Game

Only the lobby host can start:

```csharp
if (IVXLobbyManager.Instance.IsHost)
{
    await IVXLobbyManager.Instance.StartGameAsync();
}
```

#### Leaving

```csharp
IVXLobbyManager.Instance.LeaveLobby();
```

---

## 3. Matchmaking

### IVXMatchmakingManager

For `QuickMatch` and `RankedMatch`, the SDK wraps Nakama's matchmaker.

```csharp
using IntelliVerseX.Multiplayer;

var options = new MatchmakingOptions
{
    MinPlayers = 2,
    MaxPlayers = 4,
    RankRange = 200,           // ± ELO range (RankedMatch only)
    Query = "+category:science", // Nakama matchmaker query
    Timeout = TimeSpan.FromSeconds(30),
};

IVXMatchmakingManager.Instance.OnMatchFound += (match) =>
{
    Debug.Log($"Match found: {match.MatchId} with {match.Players.Count} players");
    TransitionToGameplay(match);
};

IVXMatchmakingManager.Instance.OnMatchmakingTimeout += () =>
{
    Debug.Log("No match found within timeout");
    ShowRetryUI();
};

await IVXMatchmakingManager.Instance.StartMatchmakingAsync(options);
```

#### Cancelling

```csharp
IVXMatchmakingManager.Instance.CancelMatchmaking();
```

---

## 4. Real-Time Networking

### Nakama Socket Connection

The SDK manages the socket lifecycle automatically. After authentication, the socket is connected if any multiplayer module is enabled.

```csharp
// Send data to all players in the match
IVXMatchNetwork.Instance.SendToAll(opCode: 1, data: jsonPayload);

// Send to a specific player
IVXMatchNetwork.Instance.SendTo(opCode: 1, data: jsonPayload, targetUserId);

// Receive data
IVXMatchNetwork.Instance.OnDataReceived += (senderId, opCode, data) =>
{
    switch (opCode)
    {
        case 1: HandleAnswerSubmitted(senderId, data); break;
        case 2: HandleScoreUpdate(senderId, data); break;
    }
};
```

### Op Code Conventions

| Op Code | Purpose |
|---------|---------|
| `1` | Game action (answer, move, etc.) |
| `2` | Score/state update |
| `3` | Player status change (ready, typing) |
| `10` | Host command (start round, skip) |
| `99` | Heartbeat / keep-alive |

---

## 5. Match Lifecycle

```
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌─────────┐
│  Lobby   │ ──> │ Countdown│ ──> │ In-Game  │ ──> │ Results │
│ (waiting)│     │ (3..2..1)│     │ (playing)│     │ (scores)│
└─────────┘     └──────────┘     └──────────┘     └─────────┘
```

### State Events

```csharp
IVXMatchLifecycle.Instance.OnStateChanged += (state) =>
{
    switch (state)
    {
        case MatchState.Lobby:     ShowLobbyUI(); break;
        case MatchState.Countdown: ShowCountdown(); break;
        case MatchState.InGame:    StartGameplay(); break;
        case MatchState.Results:   ShowResults(); break;
    }
};
```

### Server-Authoritative State

For competitive modes, the server drives the match state via Nakama match handler RPCs. The client sends inputs; the server validates and broadcasts the authoritative state.

---

## 6. Cross-Platform Multiplayer

| Platform | Lobby | Matchmaking | Real-Time Socket | Local Multiplayer |
|----------|-------|-------------|-----------------|-------------------|
| **Unity** | Full | Full | Full | Full |
| **JS/TS** | Full | Full | Full | N/A |
| **Java** | Full | Full | Full | N/A |
| **Flutter** | Stub | Stub | Stub | N/A |
| **Godot** | Stub | Stub | Planned | N/A |
| **Unreal** | Stub | Stub | Planned | N/A |
| **C++** | Stub | Stub | Planned | N/A |
| **Defold** | Stub | Stub | Planned | N/A |
| **Cocos2d-x** | Stub | Stub | Planned | N/A |
| **Web3** | N/A | N/A | N/A | N/A |

"Stub" means the API surface exists so your code compiles, but the implementation calls through to the same Nakama RPCs using the platform's HTTP client. Real-time sockets require platform-specific WebSocket support.

---

## 7. Best Practices

1. **Always use op codes** — never send raw strings. Define an enum for clarity.
2. **Keep payloads small** — serialize only deltas, not full state.
3. **Handle disconnections** — listen to `OnPlayerDisconnected` and show reconnect UI.
4. **Use server authority for ranked** — never trust the client for scoring.
5. **Test with simulated latency** — use Nakama's `--socket.ping_period` for realism.
6. **Cap lobby size** — more players = more network traffic. 2-8 is the sweet spot for real-time.

---

## Platform-Specific Multiplayer

### VR Platforms

| Consideration | Guidance |
|--------------|----------|
| **Latency** | VR is sensitive to network jitter — use interpolation and prediction for remote player positions. Target < 50ms round-trip for comfort. |
| **Lobby UI** | Lobbies must use world-space UI in VR. Use `IVXXRPlatformHelper.GetRecommendedSettings().UseWorldSpaceUI` to detect. |
| **Voice chat** | VR multiplayer benefits heavily from spatial voice chat. Wire Nakama party chat or platform voice (Meta Quest Party, Steam Voice). |
| **Comfort** | Never teleport or move the player unexpectedly based on network state. Smooth transitions only. |
| **Cross-play** | Nakama matchmaking is platform-agnostic. Quest players can match with PC/mobile — add a `platform` metadata field to matchmaking queries for optional filtering. |

### Console Platforms

| Platform | Requirement |
|----------|------------|
| **PlayStation** | Must use PlayStation Network for friends list and party system. Nakama handles gameplay matchmaking; PSN handles social layer. TRC requires handling PSN suspension gracefully. |
| **Xbox** | Must integrate Xbox Live for multiplayer sessions and party chat. Use `UIVXConsoleSubsystem` in Unreal or `IVXConsoleManager` in Unity alongside Nakama. XR certification requires activity feeds. |
| **Nintendo Switch** | Must use Nintendo Account for friends. Local wireless play uses Nintendo's ad-hoc networking — Nakama covers online-only. Lotcheck requires specific disconnection UX flows. |

### WebGL / Browser

| Consideration | Guidance |
|--------------|----------|
| **WebSockets** | Browser WebSocket connections require `wss://` (TLS). Ensure your Nakama server has valid HTTPS certificates. |
| **CORS** | The Nakama server must return proper CORS headers (`Access-Control-Allow-Origin`). |
| **Tab focus** | Browsers throttle background tabs — handle reconnection when the user returns to the tab. Listen for `visibilitychange` events. |
| **SharedArrayBuffer** | Some networking features require `Cross-Origin-Isolation` headers (`COOP`/`COEP`). See `docs/platforms/webgl.md`. |

---

## Checklist

- [ ] `EnableMultiplayer` toggled on in `IVXBootstrapConfig`
- [ ] Game mode set via `IVXGameModeManager`
- [ ] Lobby creation and join flow tested
- [ ] Matchmaking with timeout handling tested
- [ ] Real-time data send/receive verified
- [ ] Match lifecycle transitions display correct UI
- [ ] Disconnection handling implemented
- [ ] VR lobby uses world-space UI if targeting VR
- [ ] Console first-party social requirements reviewed
- [ ] WebGL WebSocket TLS and CORS verified
