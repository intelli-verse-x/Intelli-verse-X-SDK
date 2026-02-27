# Analytics API Reference

Complete API reference for the Analytics module.

---

## IVXAnalyticsManager

Event tracking and analytics manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXAnalyticsManager` | Singleton instance |
| `UserId` | `string` | Current user ID |
| `IsInitialized` | `bool` | Initialization status |

---

### Event Tracking Methods

#### TrackEvent

```csharp
public void TrackEvent(string eventName)
```

Tracks a simple event.

**Example:**
```csharp
IVXAnalyticsManager.Instance.TrackEvent("level_started");
```

---

#### TrackEvent (with parameters)

```csharp
public void TrackEvent(string eventName, Dictionary<string, object> parameters)
```

Tracks an event with parameters.

**Example:**
```csharp
IVXAnalyticsManager.Instance.TrackEvent("level_completed", new Dictionary<string, object>
{
    { "level", 5 },
    { "score", 1500 },
    { "time_seconds", 120 }
});
```

---

#### TrackEvent (with single parameter)

```csharp
public void TrackEvent(string eventName, string paramName, object paramValue)
```

Tracks an event with a single parameter.

**Example:**
```csharp
IVXAnalyticsManager.Instance.TrackEvent("item_purchased", "item_id", "sword_01");
```

---

### Screen Tracking

#### TrackScreen

```csharp
public void TrackScreen(string screenName)
```

Tracks a screen view.

**Example:**
```csharp
IVXAnalyticsManager.Instance.TrackScreen("MainMenu");
IVXAnalyticsManager.Instance.TrackScreen("Settings");
```

---

### User Properties

#### SetUserProperty

```csharp
public void SetUserProperty(string name, string value)
```

Sets a user property.

**Example:**
```csharp
IVXAnalyticsManager.Instance.SetUserProperty("player_level", "10");
IVXAnalyticsManager.Instance.SetUserProperty("is_premium", "true");
```

---

#### SetUserId

```csharp
public void SetUserId(string userId)
```

Sets the user ID for analytics.

---

### Revenue Tracking

#### TrackPurchase

```csharp
public void TrackPurchase(
    string productId,
    decimal price,
    string currency,
    string transactionId = null)
```

Tracks a purchase event.

**Example:**
```csharp
IVXAnalyticsManager.Instance.TrackPurchase(
    "premium_bundle",
    4.99m,
    "USD",
    "txn_12345"
);
```

---

#### TrackAdRevenue

```csharp
public void TrackAdRevenue(AdRevenueData revenueData)
```

Tracks ad revenue.

---

### Session Methods

#### StartSession

```csharp
public void StartSession()
```

Starts a new analytics session.

---

#### EndSession

```csharp
public void EndSession()
```

Ends the current session.

---

### Flush Methods

#### Flush

```csharp
public void Flush()
```

Immediately sends queued events.

---

## Standard Events

Recommended event names for consistency:

| Event | Parameters | Description |
|-------|------------|-------------|
| `tutorial_begin` | - | Tutorial started |
| `tutorial_complete` | - | Tutorial finished |
| `level_start` | `level` | Level started |
| `level_end` | `level`, `success`, `score` | Level finished |
| `item_purchase` | `item_id`, `item_name`, `price` | Virtual purchase |
| `ad_impression` | `ad_type`, `network` | Ad shown |
| `login` | `method` | User logged in |
| `sign_up` | `method` | New registration |

---

## Example Usage

```csharp
public class GameAnalytics : MonoBehaviour
{
    void Start()
    {
        IVXAnalyticsManager.Instance.TrackScreen("GameScene");
        IVXAnalyticsManager.Instance.TrackEvent("game_started");
    }
    
    public void OnLevelComplete(int level, int score, float time)
    {
        IVXAnalyticsManager.Instance.TrackEvent("level_complete", new Dictionary<string, object>
        {
            { "level", level },
            { "score", score },
            { "time_seconds", time },
            { "deaths", _deathCount }
        });
    }
    
    public void OnItemPurchased(string itemId, int cost)
    {
        IVXAnalyticsManager.Instance.TrackEvent("item_purchase", new Dictionary<string, object>
        {
            { "item_id", itemId },
            { "cost", cost },
            { "currency", "coins" }
        });
    }
}
```

---

## See Also

- [Analytics Module Guide](../modules/analytics.md)
