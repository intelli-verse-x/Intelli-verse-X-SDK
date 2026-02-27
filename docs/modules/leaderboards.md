# Leaderboard Module

The Leaderboard module provides global and friend-based leaderboards with real-time updates through the Nakama backend.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Leaderboard` |
| **Assembly** | `IntelliVerseX.Leaderboard` |
| **Backend** | Nakama Leaderboards |

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXLeaderboardManager` | Main leaderboard interface |
| `IVXLeaderboardEntry` | Single leaderboard entry |
| `IVXLeaderboardConfig` | Leaderboard configuration |

---

## Quick Start

```csharp
using IntelliVerseX.Leaderboard;

// Submit score
await IVXLeaderboardManager.SubmitScoreAsync("weekly_highscore", 15000);

// Get top scores
var entries = await IVXLeaderboardManager.GetTopScoresAsync("weekly_highscore", 10);
```

---

## IVXLeaderboardManager

```csharp
public static class IVXLeaderboardManager
{
    // Events
    public static event Action<string, IVXLeaderboardEntry[]> OnLeaderboardLoaded;
    public static event Action<string, int> OnRankChanged;
    
    // Submit scores
    public static async Task<IVXLeaderboardEntry> SubmitScoreAsync(
        string leaderboardId, 
        long score, 
        string subscore = null,
        Dictionary<string, string> metadata = null);
    
    // Get scores
    public static async Task<IVXLeaderboardEntry[]> GetTopScoresAsync(
        string leaderboardId, 
        int limit = 10);
    
    public static async Task<IVXLeaderboardEntry[]> GetScoresAroundUserAsync(
        string leaderboardId, 
        int limit = 10);
    
    public static async Task<IVXLeaderboardEntry[]> GetFriendsScoresAsync(
        string leaderboardId, 
        int limit = 10);
    
    // Get player's entry
    public static async Task<IVXLeaderboardEntry> GetPlayerEntryAsync(string leaderboardId);
    
    // Get player's rank
    public static async Task<int> GetPlayerRankAsync(string leaderboardId);
}
```

---

## Leaderboard Types

### Global Leaderboard

All players worldwide:

```csharp
// Get global top 100
var globalTop = await IVXLeaderboardManager.GetTopScoresAsync(
    "global_highscore",
    limit: 100
);

foreach (var entry in globalTop)
{
    Debug.Log($"#{entry.Rank} {entry.Username}: {entry.Score}");
}
```

### Friends Leaderboard

Only friends' scores:

```csharp
// Get friends' scores
var friendsBoard = await IVXLeaderboardManager.GetFriendsScoresAsync(
    "weekly_challenge",
    limit: 50
);
```

### Around Me

Scores around the current player:

```csharp
// Get 5 above and 5 below player
var aroundMe = await IVXLeaderboardManager.GetScoresAroundUserAsync(
    "monthly_tournament",
    limit: 11  // 5 above + player + 5 below
);
```

---

## IVXLeaderboardEntry

```csharp
public class IVXLeaderboardEntry
{
    public string UserId { get; }
    public string Username { get; }
    public string DisplayName { get; }
    public string AvatarUrl { get; }
    
    public int Rank { get; }
    public long Score { get; }
    public string Subscore { get; }
    
    public DateTime SubmitTime { get; }
    public DateTime ExpiryTime { get; }
    
    public Dictionary<string, string> Metadata { get; }
    
    // Convenience
    public bool IsCurrentUser { get; }
}
```

---

## Score Submission

### Basic Submission

```csharp
// Submit a score
await IVXLeaderboardManager.SubmitScoreAsync("daily_challenge", playerScore);
```

### With Metadata

```csharp
// Submit with additional data
var entry = await IVXLeaderboardManager.SubmitScoreAsync(
    "weekly_tournament",
    score: playerScore,
    subscore: $"{stars}:{timeBonus}",  // Tiebreaker
    metadata: new Dictionary<string, string>
    {
        { "level", currentLevel.ToString() },
        { "character", selectedCharacter },
        { "platform", Application.platform.ToString() }
    }
);

Debug.Log($"New rank: #{entry.Rank}");
```

### Score Operators

Different leaderboards can have different scoring rules:

| Operator | Behavior |
|----------|----------|
| `Set` | Always update score |
| `Best` | Only if new score is better |
| `Increment` | Add to existing score |
| `Decrement` | Subtract from existing score |

```csharp
// Configure in backend - SDK submits, backend applies operator
```

---

## Leaderboard Reset Schedules

| Leaderboard | Reset Period |
|-------------|--------------|
| `daily_*` | Every day at 00:00 UTC |
| `weekly_*` | Every Monday at 00:00 UTC |
| `monthly_*` | First of month at 00:00 UTC |
| `seasonal_*` | Every 3 months |
| `alltime_*` | Never resets |

---

## UI Integration

### Display Leaderboard

```csharp
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform entryContainer;
    [SerializeField] private LeaderboardEntryUI entryPrefab;
    [SerializeField] private TMP_Text playerRankText;
    
    async void LoadLeaderboard(string leaderboardId)
    {
        // Show loading
        ShowLoading(true);
        
        try
        {
            // Get top scores
            var entries = await IVXLeaderboardManager.GetTopScoresAsync(
                leaderboardId, 
                limit: 50
            );
            
            // Clear existing
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);
            
            // Populate UI
            foreach (var entry in entries)
            {
                var ui = Instantiate(entryPrefab, entryContainer);
                ui.SetData(entry);
                
                // Highlight current player
                if (entry.IsCurrentUser)
                    ui.Highlight();
            }
            
            // Show player's rank
            var playerEntry = await IVXLeaderboardManager.GetPlayerEntryAsync(leaderboardId);
            if (playerEntry != null)
            {
                playerRankText.text = $"Your Rank: #{playerEntry.Rank}";
            }
        }
        finally
        {
            ShowLoading(false);
        }
    }
}
```

### Entry UI Component

```csharp
public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private Image backgroundImage;
    
    public void SetData(IVXLeaderboardEntry entry)
    {
        rankText.text = $"#{entry.Rank}";
        usernameText.text = entry.DisplayName ?? entry.Username;
        scoreText.text = entry.Score.ToString("N0");
        
        // Load avatar
        if (!string.IsNullOrEmpty(entry.AvatarUrl))
            LoadAvatar(entry.AvatarUrl);
    }
    
    public void Highlight()
    {
        backgroundImage.color = highlightColor;
    }
}
```

---

## Real-Time Updates

Subscribe to leaderboard changes:

```csharp
IVXLeaderboardManager.OnRankChanged += (leaderboardId, newRank) =>
{
    if (leaderboardId == "weekly_challenge")
    {
        ShowNotification($"New rank: #{newRank}!");
        RefreshLeaderboardUI();
    }
};
```

---

## Best Practices

### 1. Cache Results

```csharp
private Dictionary<string, IVXLeaderboardEntry[]> _cachedLeaderboards = new();
private Dictionary<string, DateTime> _cacheTime = new();
private const float CACHE_DURATION_SECONDS = 30f;

async Task<IVXLeaderboardEntry[]> GetCachedLeaderboard(string id)
{
    // Check cache
    if (_cachedLeaderboards.TryGetValue(id, out var cached))
    {
        if ((DateTime.Now - _cacheTime[id]).TotalSeconds < CACHE_DURATION_SECONDS)
            return cached;
    }
    
    // Fetch fresh
    var entries = await IVXLeaderboardManager.GetTopScoresAsync(id);
    _cachedLeaderboards[id] = entries;
    _cacheTime[id] = DateTime.Now;
    return entries;
}
```

### 2. Handle Offline

```csharp
public async Task SubmitScoreWithOfflineSupport(string id, long score)
{
    if (IVXNakamaManager.IsConnected)
    {
        await IVXLeaderboardManager.SubmitScoreAsync(id, score);
    }
    else
    {
        // Queue for later
        QueueOfflineScore(id, score);
    }
}
```

### 3. Prevent Cheating

```csharp
// Scores are validated server-side
// Submit metadata for verification
await IVXLeaderboardManager.SubmitScoreAsync(
    "verified_scores",
    score,
    metadata: new Dictionary<string, string>
    {
        { "game_version", Application.version },
        { "session_id", currentSessionId },
        { "checksum", CalculateScoreChecksum(score) }
    }
);
```

### 4. Pagination

```csharp
// For large leaderboards, paginate
int offset = 0;
const int PAGE_SIZE = 50;

async Task LoadNextPage()
{
    var entries = await IVXLeaderboardManager.GetTopScoresAsync(
        "global_alltime",
        limit: PAGE_SIZE,
        offset: offset
    );
    
    offset += PAGE_SIZE;
    AppendToUI(entries);
}
```

---

## Configuration

### Define Leaderboards (Backend)

Leaderboards are configured on the Nakama server:

```lua
-- In nakama/modules/leaderboards.lua
local leaderboards = {
    {
        id = "daily_challenge",
        sort_order = "desc",  -- Higher is better
        operator = "best",    -- Keep best score
        reset_schedule = "0 0 * * *"  -- Daily at midnight
    },
    {
        id = "weekly_tournament", 
        sort_order = "desc",
        operator = "set",     -- Always update
        reset_schedule = "0 0 * * 1"  -- Weekly on Monday
    }
}
```

---

## Error Handling

```csharp
try
{
    var entries = await IVXLeaderboardManager.GetTopScoresAsync(leaderboardId);
    DisplayEntries(entries);
}
catch (LeaderboardNotFoundException)
{
    ShowError("Leaderboard not available");
}
catch (NetworkException)
{
    ShowError("Please check your connection");
    ShowCachedData();
}
```

---

## Related Documentation

- [Leaderboard Demo](../samples/leaderboard-demo.md) - Sample implementation
- [Nakama Leaderboards](https://heroiclabs.com/docs/leaderboards/) - Backend details
