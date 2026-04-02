# Multiplayer Demo

Two sample scripts demonstrating the online lobby system and game mode selection — room browsing, room creation, matchmaking, and a card-based mode picker with per-mode configuration.

---

## Scene Overview

**Location:**

- `Assets/_IntelliVerseXSDK/Demos/IVXLobbyDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXGameModeSelectorDemo.cs`

This sample demonstrates:

- Scrollable room browser with live room cards
- Room creation with mode, max players, and visibility settings
- Quick matchmaking with animated search and opponent reveal
- Game mode selection (Solo, Local, Versus, Co-op, Ranked, Turn-Based)
- Per-mode configuration panels (local player names, online quick match)
- Integration with `IVXGameModeManager`

---

## Lobby Demo — IVXLobbyDemo

### Scene Hierarchy

```
Canvas
├── Background
├── Root
│   ├── Header ("ONLINE LOBBY" + online count + refresh)
│   ├── TabBar (Browse Rooms | Create Room | Matchmaking)
│   ├── TabContent
│   │   ├── BrowsePanel (scrollable room cards)
│   │   ├── CreatePanel (name, mode, max players, visibility)
│   │   └── MatchPanel
│   │       ├── IdleGroup ("QUICK MATCH" + Find button)
│   │       ├── SearchGroup (animated dots + timer + cancel)
│   │       └── FoundGroup (opponent card + accept)
│   └── StatusBar
```

### Room Card Data

```csharp
new IVXRoomInfo
{
    RoomName = "Champions Arena",
    HostName = "AlphaGamer",
    PlayerCount = 2,
    MaxPlayers = 4,
    PingMs = 32,
    Mode = IVXGameMode.OnlineVersus,
    IsPasswordProtected = false,
    IsInProgress = false
};
```

### Room Browser

Each room card displays:

- Room name with lock/in-progress badges
- Host name
- Game mode badge (color-coded: Versus=red, Co-op=blue, Turn-Based=gold)
- Player count (highlighted when full)
- Ping (green < 50ms, white < 80ms, red > 80ms)
- JOIN / FULL / LIVE button

### Room Creation

```csharp
// The Create Room tab lets users configure:
// - Room name (text input)
// - Mode: Versus | Co-op | Turn-Based (button selector)
// - Max players: 2 | 3 | 4 (button selector)
// - Visibility: Public | Private (toggle)

// On create:
IVXGameModeManager.Instance.SelectMode(selectedMode);
Debug.Log($"Created \"{roomName}\" - {mode}, {maxPlayers}P, {visibility}");
```

### Matchmaking Flow

```csharp
// 1. Click "FIND MATCH" — starts search coroutine
// 2. Animated "Searching..." with elapsed timer
// 3. After 3–7 seconds, opponent found:
var mockNames = new[] { "Challenger99", "SpeedDemon", "BrainMaster" };
_oppName.text = mockNames[Random.Range(0, mockNames.Length)];
_oppRating.text = $"Rating: {Random.Range(800, 2200)}";
_oppPing.text = $"Ping: {Random.Range(20, 90)}ms";
// 4. Click "ACCEPT" to proceed
```

---

## Game Mode Selector — IVXGameModeSelectorDemo

### Scene Hierarchy

```
Canvas
├── Background
├── Root
│   ├── Header ("SELECT GAME MODE")
│   ├── ScrollArea (mode cards)
│   │   ├── SOLO PLAY (green accent)
│   │   ├── LOCAL MULTIPLAYER (blue, "LOCAL" tag)
│   │   ├── ONLINE VERSUS (red, "ONLINE" tag)
│   │   ├── ONLINE CO-OP (purple, "ONLINE" tag)
│   │   ├── RANKED MATCH (gold, "ONLINE" tag)
│   │   └── TURN BASED (teal)
│   └── ConfigPanel (context-sensitive)
│       ├── Solo → "START GAME" button
│       ├── Local → player count ±, name inputs, "START"
│       └── Online → "QUICK MATCH" | "BROWSE ROOMS"
```

### Mode Selection & Configuration

```csharp
// Selecting a mode
IVXGameModeManager.Instance.SelectMode(IVXGameMode.LocalMultiplayer);

// Configuring local multiplayer players
for (int i = 1; i < playerCount; i++)
    IVXGameModeManager.Instance.AddLocalPlayer($"Player {i + 1}", i);

// Starting the match
IVXGameModeManager.Instance.StartMatch();
```

### Available Game Modes

| Mode | Players | Tag | Accent |
|------|---------|-----|--------|
| Solo | 1 | — | Green |
| Local Multiplayer | 2–4 | LOCAL | Blue |
| Online Versus | 2 | ONLINE | Red |
| Online Co-op | 2–4 | ONLINE | Purple |
| Ranked Match | 2 | ONLINE | Gold |
| Turn Based | 2+ | — | Teal |

---

## How to Use

### Running the Lobby Demo

1. Add `IVXLobbyDemo` to a Canvas
2. Press **Play**
3. Browse the **8 mock rooms** in the room list
4. Switch to **Create Room** to configure and create
5. Switch to **Matchmaking** and click **FIND MATCH**

### Running the Mode Selector

1. Add `IVXGameModeSelectorDemo` to a Canvas
2. Press **Play**
3. Tap a mode card — the config panel updates dynamically
4. For Local Multiplayer, adjust player count and enter names
5. Click **START** or **QUICK MATCH**

### Testing Tab Switching

The lobby demo auto-cancels matchmaking search when switching away from the Matchmaking tab.

---

## Customization

### Adding Room Data

Replace `MOCK_ROOMS` with live data from your Nakama or Photon backend using `IVXRoomInfo` structs.

### Adding Game Modes

Extend the `MODES` array in `IVXGameModeSelectorDemo.cs` with new `ModeEntry` structs. Add a corresponding `IVXGameMode` enum value.

### Theming

Both demos use named color constants at the top of each script. Adjust `COL_BG`, `COL_CARD`, `COL_ACCENT`, etc. to match your game's palette.

---

## See Also

- [Multiplayer Module](../modules/multiplayer.md)
- [Multiplayer Integration Guide](../guides/multiplayer-integration.md)
