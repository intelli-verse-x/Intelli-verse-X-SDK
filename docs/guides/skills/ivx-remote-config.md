# Remote Config

**Skill ID:** `ivx-remote-config`

---
name: ivx-remote-config
description: >-
  Set up server-driven remote configuration for IntelliVerseX SDK games.
  Use when the user says "remote config", "server config", "feature flags",
  "dynamic config", "config without app update", "A/B config", "conditional config",
  "config rollback", "server-driven settings", "live tuning", or needs help with
  any remote configuration system.
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

Remote Config lets you change game behavior without shipping app updates. Built on Nakama storage and Satori feature flags, it provides typed config schemas, conditional overrides (by platform, country, player segment, or A/B variant), real-time propagation, and instant rollback.

```
Server (Nakama Storage + Satori Flags)
        │
        ├── Base Config (default values)
        ├── Conditional Overrides (platform, country, segment)
        └── A/B Variants (experiment-driven)
        │
        ▼
IVXRemoteConfigManager (client)
        │
        ├── Schema Validation
        ├── Local Cache (persist across sessions)
        └── Change Notification Events
```

---

## 1. Configuration

### IVXRemoteConfigSettings ScriptableObject

Create via **Create > IntelliVerseX > Remote Config Settings**.

| Field | Description |
|-------|-------------|
| `EnableRemoteConfig` | Master toggle |
| `FetchStrategy` | `OnSessionStart`, `Periodic`, `RealTime` |
| `PeriodicIntervalSec` | Fetch interval for `Periodic` strategy (default 300) |
| `CacheStrategy` | `Memory`, `Disk`, `Both` |
| `CacheTTLSec` | How long cached values remain valid (default 3600) |
| `FallbackToDefaults` | Use local defaults if fetch fails (recommended: true) |
| `StrictSchema` | Reject config values that don't match the schema |

---

## 2. Defining Config Schemas

### Typed Config Definitions

```csharp
using IntelliVerseX.RemoteConfig;

public class GameBalanceConfig : IVXRemoteConfigSchema
{
    public override string ConfigKey => "game_balance";

    [RemoteField("starting_coins", defaultValue: 500)]
    public int StartingCoins { get; set; }

    [RemoteField("xp_per_level", defaultValue: 100)]
    public int XpPerLevel { get; set; }

    [RemoteField("ad_cooldown_sec", defaultValue: 180)]
    public int AdCooldownSec { get; set; }

    [RemoteField("enable_daily_bonus", defaultValue: true)]
    public bool EnableDailyBonus { get; set; }

    [RemoteField("difficulty_multiplier", defaultValue: 1.0f)]
    public float DifficultyMultiplier { get; set; }
}
```

### Registering Schemas

```csharp
IVXRemoteConfigManager.Instance.RegisterSchema<GameBalanceConfig>();
IVXRemoteConfigManager.Instance.RegisterSchema<StoreConfig>();
IVXRemoteConfigManager.Instance.RegisterSchema<UIConfig>();
```

---

## 3. Reading Config Values

### Typed Access

```csharp
var balance = IVXRemoteConfigManager.Instance.Get<GameBalanceConfig>();

int coins = balance.StartingCoins;
bool dailyBonus = balance.EnableDailyBonus;
float difficulty = balance.DifficultyMultiplier;
```

### Raw Access

```csharp
int coins = IVXRemoteConfigManager.Instance.GetInt("game_balance.starting_coins", 500);
string theme = IVXRemoteConfigManager.Instance.GetString("ui.theme", "default");
bool enabled = IVXRemoteConfigManager.Instance.GetBool("features.new_mode", false);
```

### Change Notifications

```csharp
IVXRemoteConfigManager.Instance.OnConfigChanged += (changedKeys) =>
{
    if (changedKeys.Contains("game_balance"))
    {
        RefreshGameBalance();
    }
};
```

---

## 4. Conditional Overrides

### Platform Conditions

```csharp
var condition = new ConfigCondition
{
    Key = "game_balance.ad_cooldown_sec",
    Platform = RuntimePlatform.IPhonePlayer,
    Value = "120",
};

IVXRemoteConfigManager.Instance.AddCondition(condition);
```

### Server-Side Conditions (via Satori)

Conditions can be set server-side using Satori audiences:

| Condition Type | Example | Satori Mechanism |
|---------------|---------|-----------------|
| **Platform** | iOS gets different store layout | Audience: `platform_ios` |
| **Country** | JP players see localized prices | Audience: `country_jp` |
| **Player Segment** | Whales see premium offers | Audience: `segment_whale` |
| **A/B Variant** | Variant B gets harder difficulty | Experiment: `difficulty_test` |
| **Install Age** | New players get bonus coins | Audience: `install_age_lt_7d` |
| **Version** | v2.1+ gets new feature | Audience: `app_version_gte_2.1` |

### Override Priority

```
A/B Experiment Variant (highest)
  → Per-User Override (personalizer)
    → Audience-Based Override (Satori)
      → Platform Override
        → Base Config (lowest)
```

---

## 5. Real-Time Config Updates

### WebSocket Propagation

With `FetchStrategy = RealTime`, config changes propagate instantly via the Nakama socket:

```csharp
IVXRemoteConfigManager.Instance.FetchStrategy = ConfigFetchStrategy.RealTime;

IVXRemoteConfigManager.Instance.OnConfigChanged += (changedKeys) =>
{
    Debug.Log($"Config updated in real-time: {string.Join(", ", changedKeys)}");
};
```

### Manual Fetch

```csharp
await IVXRemoteConfigManager.Instance.FetchAsync();
```

---

## 6. Config Versioning and Rollback

### Version Tracking

Every config change is versioned server-side:

```csharp
var info = IVXRemoteConfigManager.Instance.GetVersionInfo("game_balance");
Debug.Log($"Config version: {info.Version}, last updated: {info.UpdatedAt}");
```

### Rollback

```csharp
await IVXRemoteConfigManager.Instance.RollbackAsync("game_balance", targetVersion: 3);
```

Rollback restores the previous config values in Nakama storage and triggers a config change notification to all connected clients.

---

## 7. Editor Preview

### Config Preview Window

In the Unity Editor, open **IntelliVerseX > Remote Config Preview** to:

- View all registered schemas and their current values
- Simulate different platforms, countries, and segments
- Preview A/B experiment variants
- Test config changes before pushing to production

```csharp
#if UNITY_EDITOR
[MenuItem("IntelliVerseX/Remote Config Preview")]
public static void ShowPreview()
{
    IVXRemoteConfigPreviewWindow.ShowWindow();
}
#endif
```

---

## 8. Cross-Platform API

| Engine | Class / Module | Read API |
|--------|---------------|----------|
| **Unity** | `IVXRemoteConfigManager` | `Get<T>()`, `GetInt()`, `GetString()` |
| **Unreal** | `UIVXRemoteConfigSubsystem` | `GetInt()`, `GetString()`, `GetBool()` |
| **Godot** | `IVXRemoteConfig` autoload | `get_int()`, `get_string()`, `get_bool()` |
| **JavaScript** | `IVXRemoteConfig` | `getInt()`, `getString()`, `getBool()` |
| **Roblox** | `IVX.RemoteConfig` | `IVX.RemoteConfig.get(key, default)` |
| **Java** | `IVXRemoteConfig` | `getInt()`, `getString()`, `getBoolean()` |
| **Flutter** | `IvxRemoteConfig` | `getInt()`, `getString()`, `getBool()` |
| **C++** | `IVXRemoteConfig` | `GetInt()`, `GetString()`, `GetBool()` |

---

## Platform-Specific Remote Config

### VR Platforms

| Feature | VR Guidance |
|---------|------------|
| **Performance tuning** | Use remote config to adjust render quality, LOD distances, and particle counts per headset model. |
| **Comfort settings** | Remotely tune vignette intensity, snap turn angles, and locomotion speed for comfort optimization. |
| **Controller layouts** | Push controller mapping updates for new headset firmware without app updates. |

### Console Platforms

| Platform | Remote Config Notes |
|----------|-------------------|
| **PlayStation / Xbox** | Config changes must not alter gameplay in ways that affect certification. Cosmetic and balance changes are safe. |
| **Switch** | Nintendo requires disclosure if server-driven content changes are used. Include in submission documentation. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Initial load** | Cache config in `localStorage` for instant availability. Fetch fresh values asynchronously after load. |
| **Offline** | Cached config values are used when offline. `IVXRemoteConfigManager` emits `OnFetchFailed` for UI feedback. |

---

## Checklist

- [ ] `EnableRemoteConfig` toggled on in `IVXBootstrapConfig`
- [ ] Config schemas defined for game balance, store, UI, and features
- [ ] Fetch strategy chosen (`OnSessionStart` for most games, `RealTime` for live-ops)
- [ ] Local fallback defaults set for all config fields
- [ ] Conditional overrides configured for platform-specific values
- [ ] A/B experiment configs linked to Satori experiments
- [ ] Config change listeners wired to refresh affected systems
- [ ] Editor preview window tested with different segments
- [ ] Rollback procedure documented and tested
- [ ] VR performance configs tuned per headset (if targeting VR)
- [ ] Console certification impact reviewed (if targeting consoles)
- [ ] WebGL localStorage caching configured (if targeting browser)
