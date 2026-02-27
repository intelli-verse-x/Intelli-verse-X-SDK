# Leaderboards API Reference

The Leaderboard module provides ranking and score management.

## Namespace

```csharp
using IntelliVerseX.Leaderboard;
```

## IVXLeaderboardManager

Main class for leaderboard operations.

### Methods

| Method | Description |
|--------|-------------|
| `SubmitScoreAsync(string leaderboardId, long score)` | Submit a score |
| `GetLeaderboardAsync(string leaderboardId, int limit)` | Get top scores |
| `GetPlayerRankAsync(string leaderboardId)` | Get current player's rank |

### Events

| Event | Description |
|-------|-------------|
| `OnScoreSubmitted` | Fired when score submission completes |
| `OnLeaderboardLoaded` | Fired when leaderboard data is retrieved |

### Example

```csharp
// Submit score
await IVXLeaderboardManager.Instance.SubmitScoreAsync("global", 5000);

// Get top 10
var entries = await IVXLeaderboardManager.Instance.GetLeaderboardAsync("global", 10);
```

## See Also

- [Leaderboard Module](../modules/leaderboard.md)
- [Leaderboard Demo](../samples/leaderboard-demo.md)
