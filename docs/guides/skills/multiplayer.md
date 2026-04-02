# Skill: Multiplayer

**Skill ID:** `ivx-multiplayer`

Adds real-time multiplayer to your game -- game modes, lobby management, matchmaking, and in-match networking over Nakama sockets.

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

## Game Modes

| Mode | Description | Networking |
|------|-------------|-----------|
| `Solo` | Single-player, local only | None |
| `LocalMultiplayer` | Same device, multiple players | None (Unity only) |
| `OnlineLobby` | Player creates/joins a room, waits, starts | Nakama socket |
| `QuickMatch` | Auto-matched immediately | Nakama matchmaker |
| `RankedMatch` | Skill-based matchmaking with rating | Nakama matchmaker + leaderboard |

---

## Lobby System

### Creating a Lobby

```csharp
var lobby = await IVXLobbyManager.Instance.CreateLobbyAsync(new LobbyOptions {
    MaxPlayers = 4,
    IsPrivate = false,
    Label = "trivia-room",
    Metadata = new Dictionary<string, string> {
        { "category", "science" }, { "difficulty", "medium" }
    },
});
// Share lobby.JoinCode with friends
```

### Joining

```csharp
// By code (user-friendly)
var lobby = await IVXLobbyManager.Instance.JoinLobbyByCodeAsync("ABC123");

// By ID (direct)
var lobby = await IVXLobbyManager.Instance.JoinLobbyAsync(lobbyId);
```

### Events

```csharp
IVXLobbyManager.Instance.OnPlayerJoined += (player) => UpdatePlayerList();
IVXLobbyManager.Instance.OnPlayerLeft += (player) => UpdatePlayerList();
IVXLobbyManager.Instance.OnGameStarting += (countdown) => ShowCountdown(countdown);
```

### Starting the Game

```csharp
if (IVXLobbyManager.Instance.IsHost)
    await IVXLobbyManager.Instance.StartGameAsync();
```

---

## Matchmaking

```csharp
var options = new MatchmakingOptions {
    MinPlayers = 2,
    MaxPlayers = 4,
    RankRange = 200,
    Query = "+category:science",
    Timeout = TimeSpan.FromSeconds(30),
};

IVXMatchmakingManager.Instance.OnMatchFound += (match) => TransitionToGameplay(match);
IVXMatchmakingManager.Instance.OnMatchmakingTimeout += () => ShowRetryUI();

await IVXMatchmakingManager.Instance.StartMatchmakingAsync(options);
```

---

## Real-Time Networking

```csharp
// Send
IVXMatchNetwork.Instance.SendToAll(opCode: 1, data: jsonPayload);
IVXMatchNetwork.Instance.SendTo(opCode: 1, data: jsonPayload, targetUserId);

// Receive
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
| `1` | Game action (answer, move) |
| `2` | Score/state update |
| `3` | Player status change (ready, typing) |
| `10` | Host command (start round, skip) |
| `99` | Heartbeat / keep-alive |

---

## Match Lifecycle

```
Lobby (waiting) -> Countdown (3..2..1) -> InGame (playing) -> Results (scores)
```

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

---

## Cross-Platform Support

| Platform | Lobby | Matchmaking | Real-Time Socket | Local |
|----------|-------|-------------|-----------------|-------|
| Unity | Full | Full | Full | Full |
| JS/TS | Full | Full | Full | N/A |
| Java | Full | Full | Full | N/A |
| Flutter | RPC | RPC | Planned | N/A |
| Godot | RPC | RPC | Planned | N/A |
| Unreal | RPC | RPC | Planned | N/A |

---

## Best Practices

1. **Use op codes** -- define an enum, never send raw strings
2. **Keep payloads small** -- serialize deltas, not full state
3. **Handle disconnections** -- listen to `OnPlayerDisconnected`, show reconnect UI
4. **Use server authority for ranked** -- never trust the client for scoring
5. **Test with simulated latency** -- use Nakama's `--socket.ping_period`
6. **Cap lobby size** -- 2-8 is the sweet spot for real-time games

---

## Completion Checklist

- [ ] `EnableMultiplayer` toggled on in `IVXBootstrapConfig`
- [ ] Game mode set via `IVXGameModeManager`
- [ ] Lobby creation and join flow tested
- [ ] Matchmaking with timeout handling tested
- [ ] Real-time data send/receive verified
- [ ] Match lifecycle transitions display correct UI
- [ ] Disconnection handling implemented
