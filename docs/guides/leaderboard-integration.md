# Leaderboard Integration Guide

Learn how to integrate leaderboards into your game.

## Prerequisites

- Backend configured with Nakama
- User authentication working

## Setup

### 1. Create Leaderboard on Backend

Configure leaderboards in your Nakama console.

### 2. Submit Scores

```csharp
using IntelliVerseX.Leaderboard;

await IVXLeaderboardManager.Instance.SubmitScoreAsync("global", playerScore);
```

### 3. Display Leaderboard

```csharp
var entries = await IVXLeaderboardManager.Instance.GetLeaderboardAsync("global", 10);
foreach (var entry in entries)
{
    Debug.Log($"{entry.Rank}. {entry.Username}: {entry.Score}");
}
```

## See Also

- [Leaderboard Module](../modules/leaderboard.md)
- [Leaderboard Demo](../samples/leaderboard-demo.md)
