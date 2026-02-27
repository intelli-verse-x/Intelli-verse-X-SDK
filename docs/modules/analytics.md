# Analytics Module

The Analytics module provides unified event tracking across multiple analytics providers with automatic backend integration.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Analytics` |
| **Assembly** | `IntelliVerseX.Analytics` |
| **Providers** | Firebase Analytics, AppsFlyer, Custom Backend |

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXAnalyticsManager` | Main analytics interface |
| `IVXAnalyticsConfig` | Configuration settings |
| `IVXAnalyticsEvent` | Event data container |

---

## Quick Start

```csharp
using IntelliVerseX.Analytics;

// Track simple event
IVXAnalyticsManager.LogEvent("level_start");

// Track event with parameters
IVXAnalyticsManager.LogEvent("level_complete", new Dictionary<string, object>
{
    { "level_id", 5 },
    { "score", 1000 },
    { "stars", 3 },
    { "duration", 120f }
});
```

---

## IVXAnalyticsManager

The central analytics interface that routes events to all configured providers.

```csharp
public static class IVXAnalyticsManager
{
    // State
    public static bool IsInitialized { get; }
    
    // Initialize
    public static void Initialize(IVXAnalyticsConfig config);
    
    // Log events
    public static void LogEvent(string eventName);
    public static void LogEvent(string eventName, Dictionary<string, object> parameters);
    public static void LogEvent(IVXAnalyticsEvent analyticsEvent);
    
    // User properties
    public static void SetUserId(string userId);
    public static void SetUserProperty(string property, string value);
    
    // Screen tracking
    public static void LogScreenView(string screenName, string screenClass = null);
    
    // Revenue events  
    public static void LogPurchase(string productId, decimal price, string currency);
}
```

---

## Standard Events

### Game Progress Events

```csharp
// Level start
IVXAnalyticsManager.LogEvent("level_start", new Dictionary<string, object>
{
    { "level_id", currentLevel },
    { "difficulty", "normal" }
});

// Level complete
IVXAnalyticsManager.LogEvent("level_complete", new Dictionary<string, object>
{
    { "level_id", currentLevel },
    { "score", finalScore },
    { "time", completionTime },
    { "retries", retryCount }
});

// Level fail
IVXAnalyticsManager.LogEvent("level_fail", new Dictionary<string, object>
{
    { "level_id", currentLevel },
    { "reason", "time_expired" }
});
```

### Monetization Events

```csharp
// Purchase
IVXAnalyticsManager.LogPurchase(
    productId: "coins_1000",
    price: 4.99m,
    currency: "USD"
);

// Ad watched
IVXAnalyticsManager.LogEvent("ad_watched", new Dictionary<string, object>
{
    { "ad_type", "rewarded" },
    { "placement", "bonus_coins" },
    { "completed", true }
});
```

### Social Events

```csharp
// Friend added
IVXAnalyticsManager.LogEvent("friend_added", new Dictionary<string, object>
{
    { "method", "search" }
});

// Share
IVXAnalyticsManager.LogEvent("share", new Dictionary<string, object>
{
    { "content_type", "score" },
    { "platform", "twitter" }
});
```

### Engagement Events

```csharp
// Tutorial step
IVXAnalyticsManager.LogEvent("tutorial_step", new Dictionary<string, object>
{
    { "step", 3 },
    { "step_name", "first_battle" }
});

// Tutorial complete
IVXAnalyticsManager.LogEvent("tutorial_complete", new Dictionary<string, object>
{
    { "duration", tutorialTime }
});
```

---

## Screen Tracking

```csharp
public class MyScreen : MonoBehaviour
{
    void OnEnable()
    {
        // Track screen view
        IVXAnalyticsManager.LogScreenView(
            screenName: "Store",
            screenClass: nameof(StoreScreen)
        );
    }
}
```

---

## User Properties

```csharp
// Set user ID
IVXAnalyticsManager.SetUserId(userId);

// Set properties for segmentation
IVXAnalyticsManager.SetUserProperty("player_level", "25");
IVXAnalyticsManager.SetUserProperty("subscription_tier", "premium");
IVXAnalyticsManager.SetUserProperty("favorite_game_mode", "battle_royale");
```

---

## Configuration

### IVXAnalyticsConfig

```csharp
[CreateAssetMenu(fileName = "AnalyticsConfig", menuName = "IntelliVerse-X/Analytics Config")]
public class IVXAnalyticsConfig : ScriptableObject
{
    [Header("Providers")]
    public bool enableFirebase = true;
    public bool enableAppsFlyer = true;
    public bool enableBackendAnalytics = true;
    
    [Header("Firebase")]
    public bool firebaseDebugMode;
    
    [Header("AppsFlyer")]
    public string appsFlyerDevKey;
    public string appsFlyerAppId;
    
    [Header("Settings")]
    public bool logInEditor = true;
    public bool enableBatchProcessing = true;
    public int batchSize = 10;
    public float batchInterval = 30f;
}
```

---

## Custom Analytics Provider

Extend analytics with custom providers:

```csharp
public interface IIVXAnalyticsProvider
{
    void Initialize();
    void LogEvent(string eventName, Dictionary<string, object> parameters);
    void SetUserId(string userId);
    void SetUserProperty(string property, string value);
}

// Custom implementation
public class MyAnalyticsProvider : IIVXAnalyticsProvider
{
    public void Initialize()
    {
        // Initialize your analytics SDK
    }
    
    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        // Send to your analytics backend
    }
    
    // ... other methods
}

// Register custom provider
IVXAnalyticsManager.AddProvider(new MyAnalyticsProvider());
```

---

## Backend Analytics

Events are automatically sent to the Nakama backend:

```csharp
// Backend automatically receives analytics events
// Stored with user context for server-side analysis

// Example RPC for custom analytics processing
await IVXNakamaManager.RpcAsync("analytics/process", new
{
    userId,
    eventName,
    parameters,
    timestamp = DateTime.UtcNow
});
```

---

## Debug & Testing

### Editor Logging

```csharp
#if UNITY_EDITOR
// Events are logged to console in Editor
// Check Console for: [IVXAnalytics] event_name: {parameters}
#endif
```

### Debug Mode

Enable verbose logging in the config:

```csharp
var config = ScriptableObject.CreateInstance<IVXAnalyticsConfig>();
config.logInEditor = true;
IVXAnalyticsManager.Initialize(config);
```

---

## Event Naming Guidelines

| Do | Don't |
|----|-------|
| `level_complete` | `LevelComplete` |
| `purchase_item` | `Purchase Item` |
| `tutorial_step_3` | `tutorial step 3` |
| snake_case | PascalCase or spaces |

### Reserved Parameters

Standard parameter names:
- `level_id` - Level identifier
- `score` - Player score
- `duration` - Time in seconds
- `currency` - Currency code (USD, EUR)
- `value` - Monetary value
- `item_id` - Item identifier

---

## Best Practices

### 1. Event Naming Convention

```csharp
// Use snake_case for event names
// Use descriptive, consistent names
IVXAnalyticsManager.LogEvent("button_click"); // Good
IVXAnalyticsManager.LogEvent("ButtonClick");  // Avoid
```

### 2. Parameter Types

```csharp
// Use appropriate types
var parameters = new Dictionary<string, object>
{
    { "level", 5 },           // int for numbers
    { "score", 1234.5f },     // float for decimals
    { "name", "Boss Battle" }, // string for text
    { "success", true }        // bool for flags
};
```

### 3. Don't Over-Track

```csharp
// ❌ Don't track every frame
void Update()
{
    // BAD: Too frequent
    IVXAnalyticsManager.LogEvent("player_position", pos);
}

// ✅ Track meaningful events
void OnLevelComplete()
{
    // GOOD: Meaningful event
    IVXAnalyticsManager.LogEvent("level_complete", data);
}
```

### 4. Privacy Compliance

```csharp
// Don't track PII without consent
// ❌ BAD
IVXAnalyticsManager.SetUserProperty("email", user.email);

// ✅ GOOD
IVXAnalyticsManager.SetUserProperty("user_tier", "premium");
```

---

## Predefined Events Reference

| Event | Parameters |
|-------|------------|
| `app_open` | `source`, `campaign` |
| `login` | `method` |
| `signup` | `method` |
| `level_start` | `level_id`, `difficulty` |
| `level_complete` | `level_id`, `score`, `stars`, `duration` |
| `level_fail` | `level_id`, `reason` |
| `purchase` | `product_id`, `value`, `currency` |
| `ad_watch` | `ad_type`, `placement`, `completed` |
| `tutorial_start` | - |
| `tutorial_complete` | `duration` |
| `share` | `content_type`, `platform` |

---

## Related Documentation

- [Analytics Demo](../samples/analytics-demo.md) - Sample implementation
- [Privacy Guide](../guides/privacy.md) - GDPR/CCPA compliance
