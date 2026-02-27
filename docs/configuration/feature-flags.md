# Feature Flags

Enable or disable SDK features to customize functionality.

---

## Overview

Feature flags allow you to:

- Enable only the features you need
- Reduce SDK footprint
- A/B test features
- Gradually roll out functionality

---

## Module Enable Flags

### Core Modules

In **IntelliVerseX > Game Config**:

| Flag | Default | Description |
|------|---------|-------------|
| Enable Backend | ✅ | Nakama server connection |
| Enable Identity | ✅ | User identity management |
| Enable Analytics | ✅ | Event tracking |
| Enable Logging | ✅ | Debug logging |

### Social Modules

| Flag | Default | Description |
|------|---------|-------------|
| Enable Friends | ❌ | Friends system |
| Enable Sharing | ✅ | Share/invite features |
| Enable Referrals | ✅ | Referral codes |

### Monetization Modules

| Flag | Default | Description |
|------|---------|-------------|
| Enable Ads | ✅ | Advertising |
| Enable IAP | ✅ | In-app purchases |

### Content Modules

| Flag | Default | Description |
|------|---------|-------------|
| Enable Quiz | ❌ | Daily/Weekly quiz |
| Enable Leaderboards | ✅ | Rankings |
| Enable Localization | ✅ | Multi-language |
| Enable Storage | ✅ | Data persistence |
| Enable Cross-Promo | ❌ | More Of Us feature |

---

## How to Configure

### Via Inspector

1. Select your `IntelliVerseXConfig` asset
2. In the Inspector, expand **Feature Flags**
3. Toggle features on/off
4. Changes apply on next initialization

### Via Code (Runtime)

```csharp
// Not recommended for most features
// But available for dynamic flags:

IntelliVerseXSDK.Instance.SetFeatureEnabled("Ads", false);
IntelliVerseXSDK.Instance.SetFeatureEnabled("Quiz", true);
```

---

## Conditional Compilation

Disabled modules are excluded via compile flags:

```csharp
// In your code, check if feature is available:
#if IVX_ENABLE_ADS
    IVXAdsManager.ShowInterstitial();
#else
    Debug.Log("Ads disabled");
#endif
```

### Available Defines

| Define | Feature |
|--------|---------|
| `IVX_ENABLE_ADS` | Advertising |
| `IVX_ENABLE_IAP` | In-app purchases |
| `IVX_ENABLE_QUIZ` | Quiz system |
| `IVX_ENABLE_FRIENDS` | Friends system |
| `IVX_ENABLE_LEADERBOARDS` | Leaderboards |
| `IVX_ENABLE_ANALYTICS` | Analytics |
| `IVX_ENABLE_LOCALIZATION` | Localization |
| `IVX_ENABLE_CROSSPROMO` | Cross-promotion |

---

## Runtime Feature Checks

### Check Feature Status

```csharp
// Check if a feature is enabled
if (IntelliVerseXSDK.Instance.IsFeatureEnabled("Ads"))
{
    ShowAdButton();
}
else
{
    HideAdButton();
}
```

### Feature-Safe Access

```csharp
// Safe pattern for optional features
IVXAdsManager ads = IntelliVerseXSDK.Instance.GetModule<IVXAdsManager>();
if (ads != null)
{
    ads.ShowInterstitial();
}
```

---

## Platform-Specific Flags

### WebGL Restrictions

Some features are auto-disabled on WebGL:

| Feature | WebGL Status |
|---------|--------------|
| Ads | Limited |
| IAP | Not supported |
| Native Share | Limited |

### Editor Behavior

| Flag | Editor Default |
|------|----------------|
| Use Test Ads | ✅ Always |
| Log Level | Debug |
| Mock Purchases | ✅ |

---

## Development Flags

### Debug Features

| Flag | Purpose |
|------|---------|
| Enable Debug Logging | Verbose logs |
| Use Test Ads | Safe test mode |
| Mock Purchases | Simulate IAP |
| Skip Intro | Fast iteration |

```csharp
// In IntelliVerseXConfig:
#if DEVELOPMENT_BUILD || UNITY_EDITOR
[SerializeField] private bool _debugLogging = true;
#endif
```

---

## A/B Testing

### Remote Feature Flags

Connect to your A/B testing service:

```csharp
// Example integration with remote config
public async Task LoadRemoteFlags()
{
    var flags = await RemoteConfigService.GetFlags();
    
    IntelliVerseXSDK.SetFeatureEnabled("NewUI", flags.newUiEnabled);
    IntelliVerseXSDK.SetFeatureEnabled("Quiz", flags.quizEnabled);
}
```

### Gradual Rollout

```csharp
// Roll out to percentage of users
float rolloutPercentage = 0.25f; // 25%
bool enableForUser = HashUserId(userId) < rolloutPercentage;
IntelliVerseXSDK.SetFeatureEnabled("NewFeature", enableForUser);
```

---

## Feature Dependencies

Some features require others:

```
Quiz → Backend (required)
Leaderboards → Backend (required)
Friends → Backend (required)
IAP → Analytics (recommended)
Ads → Analytics (recommended)
Localization → Storage (optional, for caching)
```

Enabling Quiz without Backend will show a warning.

---

## Custom Feature Flags

### Add Your Own Flags

```csharp
[Serializable]
public class GameFeatureFlags
{
    [Tooltip("Enable seasonal content")]
    public bool enableHolidayContent = false;
    
    [Tooltip("New gameplay mode")]
    public bool enableBattleRoyale = false;
    
    [Tooltip("Beta feature testing")]
    public bool enableBetaFeatures = false;
}

// Add to IntelliVerseXConfig
public GameFeatureFlags customFlags;
```

### Access Custom Flags

```csharp
var config = IntelliVerseXSDK.Instance.Config;
if (config.customFlags.enableHolidayContent)
{
    LoadHolidayAssets();
}
```

---

## Optimization Impact

### Build Size Reduction

Disabling features reduces build size:

| Disabled Feature | ~Savings |
|------------------|----------|
| Ads (all) | ~5-10 MB |
| IAP | ~2-3 MB |
| Quiz | ~500 KB |
| Cross-Promo | ~300 KB |

### Runtime Performance

Disabled modules:
- Don't allocate memory
- Don't run Update loops
- Reduce initialization time

---

## Best Practices

### 1. Minimal Enabled Set

```csharp
// Start with essentials:
// - Backend ✅
// - Identity ✅
// - Analytics ✅
// Add features as needed
```

### 2. Platform Optimization

```csharp
// Different flags per platform
#if UNITY_WEBGL
    config.enableAds = false;
    config.enableIAP = false;
#endif
```

### 3. Safe Feature Access

```csharp
// Always null-check optional features
public void ShowLeaderboard()
{
    var leaderboards = IntelliVerseXSDK.GetModule<IVXLeaderboardManager>();
    if (leaderboards == null)
    {
        Debug.LogWarning("Leaderboards not enabled");
        return;
    }
    leaderboards.ShowUI();
}
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Feature not working | Check if enabled in config |
| Missing component | Verify dependencies enabled |
| Editor works, build fails | Check conditional compilation |
| Config not applied | Ensure using correct config asset |

---

## Summary

Feature flags let you:

1. ✅ Customize SDK to your needs
2. ✅ Reduce build size
3. ✅ Improve performance
4. ✅ A/B test features
5. ✅ Platform-specific builds

Configure once in `IntelliVerseXConfig`, and the SDK handles the rest.
