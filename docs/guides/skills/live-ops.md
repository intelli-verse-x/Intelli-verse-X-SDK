# Skill: Live Operations

**Skill ID:** `ivx-live-ops`

Sets up 33+ server-side engagement, retention, and competition systems powered by Hiro (metagame) and Satori (analytics/experiments).

---

## When to Use

Ask your AI agent any of these:

- "Add daily rewards with a 7-day calendar"
- "Set up a fortune wheel with weighted rewards"
- "Add achievements and a badge system"
- "Configure A/B testing for the onboarding flow"
- "Set up a 6-tier league system"
- "Add a season pass with free and premium tracks"
- "Set up daily missions that rotate every 24 hours"
- "Add login streaks with escalating rewards"
- "Enable feature flags for gradual rollout"

---

## What the Agent Does

```mermaid
flowchart TD
    A[You: "Add daily rewards"] --> B[Agent loads ivx-live-ops skill]
    B --> C[Verifies Hiro + Satori enabled]
    C --> D{Which system?}
    D -->|Engagement| E[Daily Rewards / Missions / Fortune Wheel / Streaks]
    D -->|Retention| F[Season Pass / Goals / Friend Streaks]
    D -->|Competition| G[Leagues / Tournaments / Leaderboards]
    D -->|Economy| H[Currency / Energy / Inventory / Store]
    D -->|Analytics| I[Events / Flags / A/B / Live Events]
    E --> J[Configures via IVXHiroCoordinator]
    F --> J
    G --> J
    H --> J
    I --> K[Configures via IVXSatoriClient]
```

---

## Two Server Systems

### Hiro -- 33+ Metagame Systems

Handles all game economy, engagement, and progression systems via Nakama RPCs.

```csharp
IVXHiroCoordinator hiro = IVXHiroCoordinator.Instance;
```

### Satori -- Analytics + Live Config

Handles real-time analytics, segmentation, feature flags, A/B testing, and live events.

```csharp
IVXSatoriClient satori = IVXSatoriClient.Instance;
```

---

## System Catalog

### Engagement Systems

| System | Manager | What It Does |
|--------|---------|-------------|
| Economy | `IVXEconomyManager` | Currencies, wallets, grants, purchases |
| Energy | `IVXEnergyManager` | Time-based energy with refill timers |
| Achievements | `IVXAchievementManager` | Progress-based unlocks with rewards |
| Streaks | `IVXStreakManager` | Login streaks, play streaks |
| Daily Rewards | `IVXDailyRewardManager` | Calendar-based daily reward claims |
| Daily Missions | `IVXDailyMissionsManager` | Rotating daily mission sets |
| Fortune Wheel | `IVXFortuneWheelManager` | Spin-to-win with weighted rewards |
| Badges | `IVXBadgeManager` | Collectible profile badges |
| Characters | `IVXCharacterManager` | Collectible/upgradeable characters |

### Retention Systems

| System | Manager | What It Does |
|--------|---------|-------------|
| Season Pass | `IVXSeasonPassManager` | Free + premium track with milestones |
| Goals | `IVXGoalsManager` | Weekly/monthly long-term objectives |
| Friend Streaks | `IVXFriendStreakManager` | Cooperative streak mechanics |
| Retention v2 | `IVXRetentionManager` | Day-N rewards, comeback bonuses |
| Progression | `IVXProgressionManager` | XP, levels, prestige |

### Competition Systems

| System | Manager | What It Does |
|--------|---------|-------------|
| Leagues | `IVXLeagueManager` | Ranked leagues with promotion/relegation |
| Tournaments | `IVXTournamentManager` | Scheduled competitive events |
| Event Leaderboards | `IVXEventLeaderboardManager` | Time-limited competitive events |
| Challenges | `IVXChallengeManager` | Daily/weekly/monthly challenges |

### Analytics Systems

| System | Class | What It Does |
|--------|-------|-------------|
| Event Tracking | `IVXSatoriClient` | Track player events |
| Feature Flags | `IVXSatoriClient` | Toggle features remotely |
| A/B Experiments | `IVXSatoriClient` | Test variants with segments |
| Live Events | `IVXSatoriClient` | Time-limited events with rewards |
| Messages | `IVXSatoriClient` | Inbox messages and rewards |

---

## Code Examples

### Economy

```csharp
await IVXEconomyManager.Instance.GrantAsync("coins", 500);
bool success = await IVXEconomyManager.Instance.SpendAsync("coins", 200);
long balance = IVXEconomyManager.Instance.GetBalance("coins");
```

### Achievements

```csharp
await IVXAchievementManager.Instance.UpdateProgressAsync("win_10_games", 1);
IVXAchievementManager.Instance.OnAchievementUnlocked += (ach) =>
    ShowAchievementPopup(ach.Name, ach.Reward);
```

### Streaks

```csharp
var streak = await IVXStreakManager.Instance.GetCurrentStreakAsync("daily_login");
if (streak.CanClaim)
{
    var reward = await IVXStreakManager.Instance.ClaimAsync("daily_login");
    ShowRewardPopup(reward);
}
```

### Feature Flags

```csharp
var flags = await IVXSatoriClient.Instance.GetFlagsAsync();
if (flags.GetBool("enable_new_ui", defaultValue: false))
    EnableNewUI();
int dailyRewardCoins = flags.GetInt("daily_reward_coins", defaultValue: 100);
```

### A/B Experiments

```csharp
var experiments = await IVXSatoriClient.Instance.GetExperimentsAsync();
var variant = experiments.GetVariant("onboarding_flow");
switch (variant.Name)
{
    case "control":   ShowClassicOnboarding(); break;
    case "variant_a": ShowSimplifiedOnboarding(); break;
    case "variant_b": ShowVideoOnboarding(); break;
}
```

### Live Events

```csharp
var liveEvents = await IVXSatoriClient.Instance.GetLiveEventsAsync();
foreach (var evt in liveEvents.Active)
    ShowLiveEventBanner(evt);
```

---

## RPC Pattern

All Hiro calls follow the same typed RPC pattern:

```csharp
var response = await IVXHiroRpcClient.Instance.CallAsync<AchievementList>(
    rpcId: "hiro_achievements_list",
    payload: new { }
);
if (response.Success)
    foreach (var ach in response.Data.Achievements)
        Debug.Log($"{ach.Name}: {ach.Progress}/{ach.Target}");
```

---

## Server-Side Config

All Hiro configs live in Nakama storage:

| Collection | Content |
|-----------|---------|
| `hiro_configs/economy` | Currency definitions, exchange rates |
| `hiro_configs/achievements` | Achievement definitions and rewards |
| `hiro_configs/store` | Store items, pricing, rotation |
| `hiro_configs/energy` | Energy types, refill rates |

Satori configs are managed through the Satori dashboard or `satori_configs` storage.

---

## Adding a New Live-Ops Manager

Follow this folder structure:

```
IntelliVerseX.{SystemName}/
├── IVX{SystemName}Manager.cs
├── IVX{SystemName}Models.cs
├── IVX{SystemName}RpcClient.cs
├── IVX{SystemName}Config.cs
└── IntelliVerseX.{SystemName}.asmdef
```

---

## Completion Checklist

- [ ] `EnableHiro` and `EnableSatori` toggled on in `IVXBootstrapConfig`
- [ ] Hiro coordinator initializes without errors after auth
- [ ] Economy grants and spends work correctly
- [ ] Satori events are being tracked (verify in dashboard)
- [ ] Feature flags return expected values
- [ ] A/B experiment variants are assigned correctly
- [ ] Live events display when active
- [ ] Achievement progress updates and unlock callbacks fire
