# Leaderboard Demo

Sample scene demonstrating leaderboard functionality.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_LeaderboardTest.unity`

This sample demonstrates:

- Viewing global leaderboards
- Submitting scores
- Filtering by time period
- Friends-only rankings
- Player rank display

---

## Scene Hierarchy

```
Canvas
├── LeaderboardPanel
│   ├── TabButtons
│   │   ├── GlobalTab
│   │   ├── FriendsTab
│   │   └── AroundMeTab
│   ├── TimeFilterDropdown
│   ├── ScoreList (ScrollView)
│   │   └── ScoreEntryPrefab
│   └── PlayerRankDisplay
├── SubmitScorePanel
│   ├── ScoreInput
│   └── SubmitButton
└── LoadingOverlay
```

---

## Key Components

### LeaderboardDemoController.cs

```csharp
using IntelliVerseX.Leaderboard;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardDemoController : MonoBehaviour
{
    [SerializeField] private Transform _scoreListContainer;
    [SerializeField] private GameObject _scoreEntryPrefab;
    [SerializeField] private TMP_Text _playerRankText;
    [SerializeField] private TMP_Dropdown _timeFilter;
    
    private const string LEADERBOARD_ID = "high_scores";
    
    async void Start()
    {
        await RefreshLeaderboard();
    }
    
    public async void OnTabChanged(int tabIndex)
    {
        switch (tabIndex)
        {
            case 0:
                await ShowGlobalLeaderboard();
                break;
            case 1:
                await ShowFriendsLeaderboard();
                break;
            case 2:
                await ShowAroundMe();
                break;
        }
    }
    
    async System.Threading.Tasks.Task ShowGlobalLeaderboard()
    {
        var entries = await IVXLeaderboardManager.Instance.GetTopScoresAsync(
            LEADERBOARD_ID,
            limit: 100
        );
        
        DisplayEntries(entries);
    }
    
    async System.Threading.Tasks.Task ShowAroundMe()
    {
        var entries = await IVXLeaderboardManager.Instance.GetScoresAroundPlayerAsync(
            LEADERBOARD_ID,
            limit: 10
        );
        
        DisplayEntries(entries);
    }
    
    void DisplayEntries(List<LeaderboardEntry> entries)
    {
        // Clear existing
        foreach (Transform child in _scoreListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create entries
        foreach (var entry in entries)
        {
            var instance = Instantiate(_scoreEntryPrefab, _scoreListContainer);
            var display = instance.GetComponent<ScoreEntryDisplay>();
            display.Setup(entry.Rank, entry.Username, entry.Score);
        }
    }
    
    public async void SubmitScore(int score)
    {
        await IVXLeaderboardManager.Instance.SubmitScoreAsync(LEADERBOARD_ID, score);
        await RefreshLeaderboard();
    }
}
```

### ScoreEntryDisplay.cs

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreEntryDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Image _background;
    
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightColor;
    
    public void Setup(int rank, string username, long score, bool isCurrentPlayer = false)
    {
        _rankText.text = $"#{rank}";
        _usernameText.text = username;
        _scoreText.text = FormatScore(score);
        _background.color = isCurrentPlayer ? _highlightColor : _normalColor;
    }
    
    string FormatScore(long score)
    {
        if (score >= 1000000)
            return $"{score / 1000000f:F1}M";
        if (score >= 1000)
            return $"{score / 1000f:F1}K";
        return score.ToString();
    }
}
```

---

## How to Use

### Running the Sample

1. Open `IVX_LeaderboardTest.unity`
2. Ensure authenticated (run auth demo first)
3. Press **Play**

### Viewing Leaderboards

- **Global**: Top 100 players worldwide
- **Friends**: Only friends who played
- **Around Me**: Your rank ±5 positions

### Submitting Scores

1. Enter score in input field
2. Click **Submit**
3. Watch leaderboard update

---

## Leaderboard Configuration

### Create Leaderboard (Server)

Leaderboards are created on your Nakama server:

```typescript
// Server-side leaderboard creation
nk.leaderboardCreate(
    "high_scores",           // ID
    true,                    // Authoritative
    "desc",                  // Sort order
    "set",                   // Operator (set, best, inc)
    "0 0 * * 1"             // Reset schedule (weekly)
);
```

### Client Configuration

In `IntelliVerseXConfig`, configure:

- Default leaderboard ID
- Entries per page
- Cache duration

---

## Time Filters

```csharp
public enum LeaderboardTimeFilter
{
    AllTime,
    Daily,
    Weekly,
    Monthly
}

// Usage
var entries = await IVXLeaderboardManager.Instance.GetTopScoresAsync(
    leaderboardId,
    timeFilter: LeaderboardTimeFilter.Weekly
);
```

---

## See Also

- [Leaderboards Module](../modules/leaderboards.md)
- [Nakama Integration Guide](../guides/nakama-integration.md)
