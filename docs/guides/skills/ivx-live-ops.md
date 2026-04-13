# Live Operations

**Skill ID:** `ivx-live-ops`

---
name: ivx-live-ops
description: >-
  Set up live operations features in IntelliVerseX SDK games using Hiro and Satori.
  Use when the user says "add live ops", "set up daily rewards", "add season pass",
  "configure Satori", "add achievements", "set up leagues", "fortune wheel",
  "spin wheel", "daily missions", "friend streaks", "retention", "A/B testing",
  "feature flags", "live events", "badges", "tournaments", or needs help with
  any engagement/retention mechanic.
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

Live-ops in the IntelliVerseX SDK is powered by two server-side systems:

- **Hiro** — 33+ game systems (economy, energy, achievements, streaks, events, etc.)
- **Satori** — Analytics, feature flags, A/B experiments, live events, and messaging

Both are coordinated client-side through `IVXHiroCoordinator` and `IVXSatoriClient`, initialized automatically by `IVXBootstrap` after Nakama authentication.

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



## 1. Hiro Systems

### IVXHiroCoordinator

The coordinator initializes and manages all Hiro sub-systems. It is available after bootstrap:

```csharp
using IntelliVerseX.Backend;

IVXHiroCoordinator hiro = IVXHiroCoordinator.Instance;
```

### Core Systems

| System | Manager | Purpose |
|--------|---------|---------|
| **Economy** | `IVXEconomyManager` | Currencies, wallets, grants, purchases |
| **Energy** | `IVXEnergyManager` | Time-based energy with refill timers |
| **Inventory** | `IVXInventoryManager` | Items, stacking, consumption |
| **Achievements** | `IVXAchievementManager` | Progress-based unlocks with rewards |
| **Progression** | `IVXProgressionManager` | XP, levels, prestige |
| **Streaks** | `IVXStreakManager` | Login streaks, play streaks |
| **EventLeaderboards** | `IVXEventLeaderboardManager` | Time-limited competitive events |
| **Store** | `IVXStoreManager` | Virtual store with rotating inventory |
| **Challenges** | `IVXChallengeManager` | Daily/weekly/monthly challenges |
| **Teams** | `IVXTeamManager` | Clans, guilds, team-based play |
| **Unlockables** | `IVXUnlockableManager` | Progression-gated content |
| **Retention** | `IVXRetentionManager` | Day-N rewards, comeback bonuses |

### RPC Pattern

All Hiro calls follow the same pattern through `IVXHiroRpcClient`:

```csharp
using IntelliVerseX.Backend;

// Generic typed RPC call
var response = await IVXHiroRpcClient.Instance.CallAsync<AchievementList>(
    rpcId: "hiro_achievements_list",
    payload: new { }
);

if (response.Success)
{
    foreach (var achievement in response.Data.Achievements)
    {
        Debug.Log($"{achievement.Name}: {achievement.Progress}/{achievement.Target}");
    }
}
else
{
    Debug.LogWarning($"RPC failed: {response.Error}");
}
```

The `HiroRpcResponse<T>` wrapper provides `Success`, `Data`, `Error`, and `StatusCode`.

---

## 2. Satori Analytics & Live Config

### IVXSatoriClient

Satori provides real-time analytics, segmentation, and remote configuration.

#### Tracking Events

```csharp
using IntelliVerseX.Analytics;

IVXSatoriClient.Instance.TrackEvent("level_complete", new Dictionary<string, string>
{
    { "level", "5" },
    { "score", "1200" },
    { "time_sec", "45" },
});
```

#### Feature Flags

```csharp
var flags = await IVXSatoriClient.Instance.GetFlagsAsync();

if (flags.GetBool("enable_new_ui", defaultValue: false))
{
    EnableNewUI();
}

int dailyRewardAmount = flags.GetInt("daily_reward_coins", defaultValue: 100);
```

#### A/B Experiments

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

#### Live Events

```csharp
var liveEvents = await IVXSatoriClient.Instance.GetLiveEventsAsync();

foreach (var evt in liveEvents.Active)
{
    Debug.Log($"Live event: {evt.Name} — ends {evt.EndTime}");
    ShowLiveEventBanner(evt);
}
```

#### Messages

```csharp
var messages = await IVXSatoriClient.Instance.GetMessagesAsync();

foreach (var msg in messages.Unread)
{
    ShowInboxMessage(msg.Title, msg.Body, msg.Rewards);
}
```

---

## 3. Planned Live-Ops Managers

These managers are being added to the SDK. Each follows the same architecture: RPC client + data models + manager class + dedicated `.asmdef`.

| Manager | Status | Purpose |
|---------|--------|---------|
| `IVXPushNotificationManager` | Planned | Server-triggered push notifications |
| `IVXDailyMissionsManager` | Planned | Rotating daily mission sets |
| `IVXLeagueManager` | Planned | Ranked leagues with promotion/relegation |
| `IVXFortuneWheelManager` | Planned | Spin-to-win with weighted rewards |
| `IVXBadgeManager` | Planned | Collectible profile badges |
| `IVXRetentionManager` | In Progress | Day-N rewards, comeback mechanics |
| `IVXSeasonPassManager` | Planned | Free + premium track with milestones |
| `IVXGoalsManager` | Planned | Long-term player goals |
| `IVXFriendStreakManager` | Planned | Cooperative streak mechanics |
| `IVXCharacterManager` | Planned | Collectible/upgradeable characters |
| `IVXTournamentManager` | Planned | Scheduled competitive tournaments |

### Adding a New Manager

Follow this pattern for each:

```
IntelliVerseX.{SystemName}/
├── IVX{SystemName}Manager.cs      // Singleton manager
├── IVX{SystemName}Models.cs       // Request/response data models
├── IVX{SystemName}RpcClient.cs    // Typed RPC wrappers
├── IVX{SystemName}Config.cs       // ScriptableObject config (if needed)
└── IntelliVerseX.{SystemName}.asmdef
```

---

## 4. Economy Quick Reference

### Granting Currency

```csharp
await IVXEconomyManager.Instance.GrantAsync("coins", 500);
await IVXEconomyManager.Instance.GrantAsync("gems", 10);
```

### Spending Currency

```csharp
bool success = await IVXEconomyManager.Instance.SpendAsync("coins", 200);
if (!success) ShowInsufficientFundsUI();
```

### Checking Balance

```csharp
long coins = IVXEconomyManager.Instance.GetBalance("coins");
```

---

## 5. Achievement Example

```csharp
// Update progress
await IVXAchievementManager.Instance.UpdateProgressAsync("win_10_games", 1);

// Listen for unlocks
IVXAchievementManager.Instance.OnAchievementUnlocked += (achievement) =>
{
    ShowAchievementPopup(achievement.Name, achievement.Reward);
};
```

---

## 6. Streak Example

```csharp
var streak = await IVXStreakManager.Instance.GetCurrentStreakAsync("daily_login");

Debug.Log($"Current streak: {streak.Count} days");
Debug.Log($"Next reward: {streak.NextReward}");

if (streak.CanClaim)
{
    var reward = await IVXStreakManager.Instance.ClaimAsync("daily_login");
    ShowRewardPopup(reward);
}
```

---

## 7. Server-Side Configuration

All Hiro system configs live in Nakama storage under the `hiro_configs` collection. Use the Nakama console or the MCP tools to edit them:

```
hiro_configs/economy     — currency definitions, exchange rates
hiro_configs/achievements — achievement definitions and rewards
hiro_configs/store       — store items, pricing, rotation
hiro_configs/energy      — energy types, refill rates
```

Satori configs are managed through the Satori dashboard or `satori_configs` storage.

---

## Platform-Specific Live-Ops

### VR Platforms

| System | VR Guidance |
|--------|------------|
| **Push Notifications** | VR headsets typically don't display push notifications during sessions. Use in-game notification UI (world-space popups) for live events and streak reminders. |
| **Daily Login** | VR sessions are shorter and less frequent than mobile. Adjust streak forgiveness windows (e.g. 48-hour grace period instead of 24). |
| **Achievement Popups** | Use world-space achievement toast anchored to the player's peripheral vision (not center-screen). |
| **Store UI** | Virtual store must be a 3D world-space panel in VR. Use `IVXXRPlatformHelper.GetRecommendedSettings().UIScale` for sizing. |

### Console Platforms

| Platform | Live-Ops Requirements |
|----------|----------------------|
| **PlayStation 5** | Trophies must mirror Hiro achievements. Use `IVXConsoleManager.UnlockAchievementAsync()` alongside `IVXAchievementManager`. Activity Cards can display live event status. |
| **Xbox Series** | Xbox achievements are separate from Hiro. Sync both systems. Xbox Game Pass games should have generous free-tier season passes. |
| **Nintendo Switch** | No native achievement system. Hiro achievements are the only achievement layer on Switch. |

**Certification:** All timed live events must handle clock manipulation (users changing system time). Hiro validates server-side, so this is handled automatically.

### WebGL / Browser

| System | WebGL Notes |
|--------|------------|
| **Push Notifications** | Use the Web Push API for browser notifications (requires user permission and a service worker). Not available in all browsers. |
| **Offline Streaks** | IndexedDB can cache streak state, but browser storage may be cleared. Always re-sync with Nakama on reconnect. |
| **Satori Analytics** | Events fire via HTTPS — works identically in browser. Ensure Nakama server allows CORS from your hosting domain. |
| **Feature Flags** | Cache flags in `localStorage` for instant availability on page load, then refresh from Satori asynchronously. |

---

## Checklist

- [ ] `EnableHiro` and `EnableSatori` toggled on in `IVXBootstrapConfig`
- [ ] Hiro coordinator initializes without errors after auth
- [ ] Economy grants and spends work correctly
- [ ] Satori events are being tracked (verify in dashboard)
- [ ] Feature flags return expected values
- [ ] A/B experiment variants are assigned correctly
- [ ] Live events display when active
- [ ] Achievement progress updates and unlock callbacks fire
- [ ] VR notification UI uses world-space popups (if targeting VR)
- [ ] Console achievements synced with platform trophies (if targeting consoles)
- [ ] WebGL push notification and caching strategy configured (if targeting browser)
