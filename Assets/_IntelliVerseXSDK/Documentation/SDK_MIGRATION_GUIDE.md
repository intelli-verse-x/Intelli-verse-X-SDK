# IntelliVerseX SDK Migration Guide
**Version**: 1.0  
**Last Updated**: December 2024  
**Audience**: Game developers migrating to IntelliVerseX SDK

---

## 📋 Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Migration Checklist](#migration-checklist)
4. [Component-by-Component Guide](#component-by-component-guide)
5. [Code Examples](#code-examples)
6. [Common Pitfalls](#common-pitfalls)
7. [Testing & Validation](#testing--validation)

---

## 🎯 Overview

The IntelliVerseX SDK provides reusable base classes and components for:
- **Backend Integration**: Nakama authentication, leaderboards, wallets, adaptive rewards
- **UI Components**: Leaderboard panels, wallet displays, currency formatting
- **Configuration**: ScriptableObject-based config system
- **Identity Management**: Cross-game user identity and progression

### Migration Benefits
- **93% code reduction** (1,300 lines → 100 lines per game)
- **5-minute leaderboard setup** (down from 1 day)
- **2-minute wallet display** (down from 4 hours)
- **Config-driven architecture** (no hardcoded values)

---

## ✅ Prerequisites

Before migrating, ensure:
1. ✅ Unity 2021.3+ installed
2. ✅ Nakama Unity SDK imported (3.x+)
3. ✅ TextMeshPro package installed
4. ✅ IntelliVerseXUserIdentity singleton in scene
5. ✅ IntelliVerseXConfig asset created at `Resources/IntelliVerseX/{GameName}Config.asset`

---

## 📝 Migration Checklist

### Phase 1: Nakama Manager Migration (30 minutes)
- [ ] Copy `/SDK/Backend/IVXNakamaManager.cs` to your project
- [ ] Copy `/SDK/Backend/IVXNakamaModels.cs` to your project
- [ ] Update your game's Nakama manager to extend `IVXNakamaManager`
- [ ] Override `GetLogPrefix()` and `GetConfigResourcePath()`
- [ ] Remove duplicate methods (keep only game-specific logic)
- [ ] Update namespace imports: `using YourGame.SDK` → `using IntelliVerseX.Backend`
- [ ] Test compilation and runtime functionality

### Phase 2: UI Components Migration (20 minutes)
- [ ] Copy `/SDK/UI/IVXLeaderboardManager.cs` to your project
- [ ] Copy `/SDK/UI/IVXWalletDisplay.cs` to your project
- [ ] Replace existing leaderboard UI with `IVXLeaderboardManager` component
- [ ] Replace wallet TextMeshPro components with `IVXWalletDisplay`
- [ ] Remove manual wallet update code
- [ ] Test UI updates and animations

### Phase 3: Configuration Validation (10 minutes)
- [ ] Verify `IntelliVerseXConfig` asset has correct Nakama settings
- [ ] Test config loading in Editor and build
- [ ] Confirm session persistence works (PlayerPrefs)
- [ ] Validate adaptive reward calculation

---

## 🔧 Component-by-Component Guide

### 1. Nakama Manager Migration

#### Before (QuizVerse Pattern - 700 lines)
```csharp
using UnityEngine;
using Nakama;
using System.Threading.Tasks;

public class QuizVerseNakamaManager : MonoBehaviour
{
    // ❌ Hardcoded config
    private string scheme = "https";
    private string host = "nakama-rest.intelli-verse-x.ai";
    private int port = 443;
    private string serverKey = "defaultkey";
    private string gameId = "quizverse";
    
    private IClient client;
    private ISession session;
    
    public async Task InitializeAsync()
    {
        // ❌ Manual client creation
        client = new Client(scheme, host, port, serverKey);
        
        // ❌ Manual auth logic
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        session = await client.AuthenticateDeviceAsync(deviceId);
        
        // ❌ Manual RPC call for identity sync
        var payload = JsonUtility.ToJson(new { game_id = gameId });
        await client.RpcAsync(session, "create_or_sync_user", payload);
    }
    
    public async Task<LeaderboardResponse> GetAllLeaderboards(int limit)
    {
        // ❌ 100+ lines of manual JSON parsing
        var payload = JsonUtility.ToJson(new { game_id = gameId, limit = limit });
        var result = await client.RpcAsync(session, "get_all_leaderboards", payload);
        // ... JSON deserialization ...
    }
    
    public async Task<long> SubmitScore(int score, string currencyId)
    {
        // ❌ 150+ lines of score submission + adaptive rewards
        // ... complex logic ...
    }
    
    // ❌ 400+ more lines of wallet, leaderboard, reward logic
}
```

#### After (SDK Pattern - 100 lines)
```csharp
using UnityEngine;
using IntelliVerseX.Backend;
using System.Threading.Tasks;

public class QuizVerseNakamaManager : IVXNakamaManager
{
    // ✅ Override customization hooks
    protected override string GetLogPrefix() => "[QUIZVERSE]";
    
    protected override string GetConfigResourcePath() => "IntelliVerseX/QuizVerseConfig";
    
    // ✅ Add ONLY game-specific methods
    public async Task<long> SubmitQuizScore(int score, string category, string difficulty)
    {
        // ✅ Call base SDK method
        var result = await SubmitScore(score, "quiz_points");
        
        // ✅ Add QuizVerse-specific tracking
        TrackQuizMetrics(category, difficulty, score);
        
        return result;
    }
    
    private void TrackQuizMetrics(string category, string difficulty, int score)
    {
        // QuizVerse-specific analytics
        Debug.Log($"[QUIZVERSE] {category} quiz ({difficulty}): {score} points");
    }
    
    // ✅ All other methods inherited from IVXNakamaManager:
    // - InitializeAsync()
    // - AuthenticateAndSyncIdentity()
    // - SubmitScore()
    // - GetAllLeaderboards()
    // - UpdateWalletBalance()
    // - GetWalletBalance()
    // - CalculateScoreReward()
    // - UpdateGameRewardConfig()
    // - ResetWinStreak()
}
```

**Lines Saved**: 700 → 100 (600 lines / 86% reduction)

---

### 2. Leaderboard UI Migration

#### Before (Manual Implementation - 300 lines)
```csharp
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuizVerseLeaderboard : MonoBehaviour
{
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private QuizVerseNakamaManager nakamaManager;
    
    private List<GameObject> currentEntries = new List<GameObject>();
    
    public async void RefreshLeaderboard()
    {
        // ❌ Manual data fetching
        var data = await nakamaManager.GetAllLeaderboards(50);
        
        // ❌ Manual UI clearing
        foreach (var entry in currentEntries)
        {
            Destroy(entry);
        }
        currentEntries.Clear();
        
        // ❌ Manual entry creation
        foreach (var record in data.daily.records)
        {
            var entry = Instantiate(entryPrefab, entriesContainer);
            entry.transform.Find("RankText").GetComponent<TMP_Text>().text = $"#{record.rank}";
            entry.transform.Find("UsernameText").GetComponent<TMP_Text>().text = record.username;
            entry.transform.Find("ScoreText").GetComponent<TMP_Text>().text = record.score.ToString();
            currentEntries.Add(entry);
        }
    }
    
    // ❌ 200+ more lines for tabs, caching, filtering, etc.
}
```

#### After (SDK Component - 0 lines!)
```csharp
// ✅ NO CODE NEEDED!
// Just attach IVXLeaderboardManager component in Inspector:
// 1. Drag IVXLeaderboardManager.cs to GameObject
// 2. Assign nakamaManager reference
// 3. Assign UI references (panel, container, entryPrefab)
// 4. Enable "autoRefreshOnShow"
// 5. Done! Component handles everything automatically
```

**Inspector Setup (2 minutes)**:
```
LeaderboardPanel (GameObject)
└── IVXLeaderboardManager (Component)
    ├── Nakama Manager: QuizVerseNakamaManager
    ├── Leaderboard Panel: LeaderboardPanel
    ├── Entries Container: ScrollView/Viewport/Content
    ├── Entry Prefab: LeaderboardEntry
    ├── Player Rank Text: PlayerRankText
    ├── Daily Button: DailyTab
    ├── Weekly Button: WeeklyTab
    ├── Monthly Button: MonthlyTab
    ├── Alltime Button: AlltimeTab
    ├── Global Button: GlobalTab
    ├── Entries Per Page: 50
    ├── Auto Refresh On Show: ✓
    └── Cache Time Seconds: 30
```

**Lines Saved**: 300 → 0 (100% reduction)

---

### 3. Wallet Display Migration

#### Before (Manual Updates - 150 lines)
```csharp
using UnityEngine;
using TMPro;

public class QuizVerseWallet : MonoBehaviour
{
    [SerializeField] private TMP_Text gemsText;
    [SerializeField] private TMP_Text coinsText;
    private IntelliVerseXUserIdentity identity;
    
    private void Start()
    {
        identity = FindObjectOfType<IntelliVerseXUserIdentity>();
        identity.OnWalletUpdated += UpdateWalletDisplay;
        UpdateWalletDisplay();
    }
    
    private void UpdateWalletDisplay()
    {
        // ❌ Manual balance fetching
        long gems = identity.GetGameWalletBalance("gems");
        long coins = identity.GetGameWalletBalance("coins");
        
        // ❌ Manual formatting
        gemsText.text = FormatNumber(gems);
        coinsText.text = FormatNumber(coins);
    }
    
    private string FormatNumber(long value)
    {
        // ❌ 50+ lines of number abbreviation logic
        if (value < 1000) return value.ToString();
        if (value < 1_000_000) return $"{value / 1000f:F1}K";
        // ... etc
    }
    
    // ❌ 80+ more lines for animations, icons, etc.
}
```

#### After (SDK Component - 0 lines!)
```csharp
// ✅ NO CODE NEEDED!
// Just attach IVXWalletDisplay component to TextMeshPro:
// 1. Select TextMeshPro GameObject for gems
// 2. Add IVXWalletDisplay component
// 3. Set Wallet Type: Game
// 4. Set Currency ID: "gems"
// 5. Set Prefix: "💎 "
// 6. Enable Abbreviate Large Numbers: ✓
// 7. Enable Animate Changes: ✓
// 8. Done! Auto-updates from this point forward
```

**Inspector Setup (30 seconds per wallet)**:
```
GemsText (TextMeshPro)
└── IVXWalletDisplay (Component)
    ├── Wallet Type: Game
    ├── Currency ID: "gems"
    ├── Balance Text: (auto-assigned)
    ├── Prefix: "💎 "
    ├── Suffix: ""
    ├── Format: "N0"
    ├── Abbreviate Large Numbers: ✓
    ├── Animate Changes: ✓
    ├── Animation Duration: 0.5
    └── Animation Curve: EaseInOut
```

**Lines Saved**: 150 → 0 (100% reduction)

---

### 4. Adaptive Rewards Migration

#### Before (QuizVerse Models)
```csharp
using QuizVerse.SDK; // ❌ Game-specific namespace

var rewardConfig = new RewardConfigBuilder() // ❌ No prefix
    .WithMultiplier(10.0)
    .Build();
    
var response = await nakamaManager.CalculateScoreReward(100, "gems");
Debug.Log($"Reward: {response.currency_rewarded}"); // ❌ No namespace
```

#### After (SDK Models)
```csharp
using IntelliVerseX.Backend; // ✅ SDK namespace

var rewardConfig = IVXRewardConfigBuilder.CreateQuizConfig() // ✅ IVX prefix
    .WithMultiplier(10.0)
    .WithMaxReward(500)
    .AddMilestone(100, 50)
    .Build();
    
var response = await nakamaManager.CalculateScoreReward(100, "gems");
Debug.Log($"Reward: {response.currency_rewarded}"); // ✅ Same API
```

**Reward Config Presets**:
```csharp
// Quiz games (high frequency, moderate rewards)
var config = IVXRewardConfigBuilder.CreateQuizConfig();

// Action games (low frequency, high rewards)
var config = IVXRewardConfigBuilder.CreateActionConfig();

// Casual games (very high frequency, small rewards)
var config = IVXRewardConfigBuilder.CreateCasualConfig();

// Custom config
var config = IVXRewardConfigBuilder.CreateCustomConfig()
    .WithMultiplier(25.0)
    .WithMaxReward(1000)
    .WithMinScore(50)
    .WithCurrency("stars")
    .AddMilestone(100, 10)
    .AddMilestone(500, 50)
    .AddMilestone(1000, 150)
    .AddStreakMultiplier(3, 1.2)
    .AddStreakMultiplier(5, 1.5)
    .Build();
```

---

## 🔥 Common Pitfalls

### 1. Forgetting to Update Namespace Imports
**Problem**: Compilation errors after migration
```csharp
// ❌ Old namespace
using QuizVerse.SDK;

// ✅ New namespace
using IntelliVerseX.Backend;
```

**Solution**: Find/Replace across all files

---

### 2. Not Removing Duplicate Methods
**Problem**: `QuizVerseNakamaManager` still has 700 lines of duplicate code

**Solution**: Delete methods that exist in `IVXNakamaManager`:
- ❌ Remove: `InitializeAsync()`, `AuthenticateAndSyncIdentity()`, `SubmitScore()`, `GetAllLeaderboards()`, `UpdateWalletBalance()`, `GetWalletBalance()`, `CalculateScoreReward()`, `UpdateGameRewardConfig()`, `ResetWinStreak()`
- ✅ Keep: Game-specific methods like `SubmitQuizScore()`, `TrackQuizMetrics()`, etc.

---

### 3. Incorrect Config Path
**Problem**: `NullReferenceException` when accessing `sdkConfig`

**Solution**: Override `GetConfigResourcePath()` with correct path:
```csharp
protected override string GetConfigResourcePath() 
{
    return "IntelliVerseX/QuizVerseConfig"; // ✅ Matches asset location
}
```

**Verify**: Config asset must exist at `Resources/IntelliVerseX/QuizVerseConfig.asset`

---

### 4. Not Assigning Nakama Manager Reference
**Problem**: `IVXLeaderboardManager` doesn't fetch data

**Solution**: In Inspector, drag your game's Nakama manager to the `nakamaManagerComponent` field:
```
IVXLeaderboardManager (Component)
├── Nakama Manager Component: QuizVerseNakamaManager ← Drag here!
```

---

### 5. Missing IntelliVerseXUserIdentity Singleton
**Problem**: `IVXWalletDisplay` shows "Identity not found" error

**Solution**: Ensure `IntelliVerseXUserIdentity` is in the scene (usually on GameManager):
```
GameManager (GameObject)
└── IntelliVerseXUserIdentity (Component) ← Required for wallets!
```

---

### 6. UI References Not Assigned
**Problem**: Leaderboard entries don't appear

**Solution**: Verify all UI references are assigned in Inspector:
```
IVXLeaderboardManager
├── Leaderboard Panel: LeaderboardPanel ← Must be assigned
├── Entries Container: ScrollView/Content ← Must be assigned
├── Entry Prefab: LeaderboardEntry ← Must be assigned
└── Player Rank Text: PlayerRankText ← Optional but recommended
```

---

### 7. Entry Prefab Missing Expected Hierarchy
**Problem**: Leaderboard entries show blank text

**Solution**: Entry prefab must have this structure:
```
LeaderboardEntry (Prefab)
├── RankText (TextMeshPro) ← Must be named "RankText"
├── UsernameText (TextMeshPro) ← Must be named "UsernameText"
└── ScoreText (TextMeshPro) ← Must be named "ScoreText"
```

**Alternative**: Override `DisplayLeaderboardData()` for custom hierarchy

---

## ✅ Testing & Validation

### Unit Tests
```csharp
using NUnit.Framework;
using UnityEngine;
using IntelliVerseX.Backend;

public class SDKMigrationTests
{
    [Test]
    public void NakamaManager_Inherits_IVXNakamaManager()
    {
        var manager = new GameObject().AddComponent<QuizVerseNakamaManager>();
        Assert.IsInstanceOf<IVXNakamaManager>(manager);
    }
    
    [Test]
    public void Config_Loads_Successfully()
    {
        var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/QuizVerseConfig");
        Assert.IsNotNull(config);
        Assert.AreEqual("quizverse", config.gameId);
    }
    
    [Test]
    public void RewardConfigBuilder_Creates_Valid_Config()
    {
        var config = IVXRewardConfigBuilder.CreateQuizConfig().Build();
        Assert.AreEqual(10.0, config.score_multiplier);
        Assert.AreEqual(500, config.max_reward);
    }
}
```

### Integration Checklist
- [ ] Nakama manager initializes without errors
- [ ] Session persists across app restarts (PlayerPrefs)
- [ ] Score submission updates leaderboards
- [ ] Adaptive rewards calculate correctly
- [ ] Leaderboard UI populates with real data
- [ ] Wallet displays update when balance changes
- [ ] Tab buttons switch leaderboard periods
- [ ] Player rank highlights correctly
- [ ] Number abbreviation works (1.5K, 2.3M)
- [ ] Animations play smoothly

### Performance Validation
- [ ] Leaderboard refresh completes in < 2 seconds
- [ ] Wallet updates don't cause frame drops
- [ ] Memory usage stable over 10+ refreshes
- [ ] No memory leaks (check Profiler)

---

## 📞 Support & Resources

### Documentation
- **SDK Completion Report**: `/SDK/Docs/SDK_COMPLETION_REPORT.md`
- **IVXNakamaManager API**: See code comments in `IVXNakamaManager.cs`
- **IVXNakamaModels API**: See code comments in `IVXNakamaModels.cs`
- **UI Components Guide**: See code comments in `IVXLeaderboardManager.cs` and `IVXWalletDisplay.cs`

### Getting Help
- **Slack**: #sdk-migration
- **Email**: sdk-support@intelliverse-x.ai
- **GitHub Issues**: Tag with `sdk-migration`

### Example Projects
- **QuizVerse**: Reference implementation (after Phase 1 completion)
- **ActionVerse**: Coming soon (Phase 3)
- **CasualVerse**: Coming soon (Phase 3)

---

## 🎓 Best Practices

### 1. Extend, Don't Modify
```csharp
// ✅ Good - extend SDK base class
public class MyGameNakamaManager : IVXNakamaManager
{
    protected override string GetLogPrefix() => "[MYGAME]";
}

// ❌ Bad - modify SDK files directly
// (Changes will be lost on SDK updates)
```

### 2. Use Presets When Possible
```csharp
// ✅ Good - use preset for common game types
var config = IVXRewardConfigBuilder.CreateQuizConfig();

// ❌ Bad - manually configure everything
var config = IVXRewardConfigBuilder.CreateCustomConfig()
    .WithMultiplier(10.0)
    .WithMaxReward(500)
    .WithMinScore(10)
    .WithCurrency("gems")
    .AddMilestone(100, 10)
    .AddMilestone(500, 50)
    .AddStreakMultiplier(3, 1.1)
    .AddStreakMultiplier(5, 1.2)
    .AddStreakMultiplier(10, 1.5)
    .Build();
// (Only do this if presets don't fit your needs)
```

### 3. Subscribe to Events
```csharp
// ✅ Good - react to SDK events
leaderboardManager.OnLeaderboardDataUpdated += (data) => {
    Debug.Log($"Leaderboards updated: {data.daily.records.Count} daily records");
    UpdateAchievements(data);
};

// ❌ Bad - poll for changes
void Update() {
    if (leaderboardManager.GetCachedData() != null) {
        // Check every frame - inefficient!
    }
}
```

### 4. Override Minimally
```csharp
// ✅ Good - override only what you need
public class MyGameNakamaManager : IVXNakamaManager
{
    protected override string GetLogPrefix() => "[MYGAME]";
    
    // Add game-specific method
    public async Task TrackAchievement(string achievementId) {
        // Game-specific logic
    }
}

// ❌ Bad - override base functionality
public class MyGameNakamaManager : IVXNakamaManager
{
    public override async Task InitializeAsync() {
        // DON'T override unless absolutely necessary!
        // You'll lose SDK features and bug fixes
    }
}
```

---

## 🚀 Migration Time Estimates

| Component | Lines Before | Lines After | Time | Difficulty |
|-----------|--------------|-------------|------|------------|
| Nakama Manager | 700 | 100 | 30 min | Medium |
| Leaderboard UI | 300 | 0 | 5 min | Easy |
| Wallet Display | 150 | 0 | 2 min | Easy |
| Adaptive Rewards | 150 | 10 | 5 min | Easy |
| **TOTAL** | **1,300** | **110** | **42 min** | **Easy-Medium** |

**Expected Results**:
- ✅ 92% code reduction
- ✅ Faster integration times for future games
- ✅ Consistent UI/UX across platform
- ✅ Easier maintenance and bug fixes

---

**Ready to migrate?** Start with the [Migration Checklist](#-migration-checklist) and follow the [Component-by-Component Guide](#-component-by-component-guide). Good luck! 🎉
