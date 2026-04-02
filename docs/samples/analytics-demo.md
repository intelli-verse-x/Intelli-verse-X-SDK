# Analytics Demo

This sample shows **event tracking**, **custom properties**, **session lifecycle**, and how data reaches the server via **Nakama RPC**, with notes on **Satori** for games that emit analytics through that channel.

## Components

| Type | Role |
|------|------|
| `IVXAnalyticsManager` | Singleton-style manager: `Initialize(IClient, ISession)`, RPC-backed events. |
| RPCs | `quizverse_log_event`, `quizverse_track_session_start`, `quizverse_track_session_end` |

## Setup

1. Call **`IVXAnalyticsManager.SetGameId(yourGameId)`** before **`Initialize`**.
2. After Nakama session is valid:

```csharp
using IntelliVerseX.Analytics;
using Nakama;

IVXAnalyticsManager.SetGameId("my-game-id");
IVXAnalyticsManager.Instance.Initialize(nakamaClient, nakamaSession);
// Auto-starts session tracking on Initialize
```

3. Optionally subscribe to **`OnEventTracked`**, **`OnSessionStarted`**, **`OnSessionEnded`**, **`OnError`**.

## Event tracking

```csharp
var ok = await IVXAnalyticsManager.Instance.TrackEvent("level_complete", new Dictionary<string, object>
{
    { "level", 5 },
    { "score", 1200 },
    { "time_seconds", 120 },
    { "difficulty", "hard" }
});
```

**Screen views** and **purchases** wrap `TrackEvent`:

```csharp
await IVXAnalyticsManager.Instance.TrackScreen("Shop", new Dictionary<string, object> {
    { "tab", "featured" }
});

await IVXAnalyticsManager.Instance.TrackPurchase("coin_pack_100", 0.99m, "USD");
```

## Custom properties (user dimensions)

```csharp
IVXAnalyticsManager.Instance.SetUserProperty("player_tier", "gold");
IVXAnalyticsManager.Instance.SetUserProperties(new Dictionary<string, object> {
    { "favorite_mode", "daily_quiz" },
    { "install_source", "organic" }
});

var copy = IVXAnalyticsManager.Instance.GetUserProperties();
```

Properties are held **client-side** and merged into payloads as your serialization layer defines; ensure server RPCs accept extended fields.

## Session tracking

- **`TrackSessionStart`** is invoked from **`Initialize`** (creates a session key and records start time).
- Call **`TrackSessionEnd`** on app pause, quit, or logout:

```csharp
private void OnApplicationPause(bool paused)
{
    if (paused)
        _ = IVXAnalyticsManager.Instance.TrackSessionEnd();
}
```

Duration uses `Time.realtimeSinceStartup` delta.

## Error handling

| Situation | Behavior |
|-----------|----------|
| Not initialized | `TrackEvent` returns `false`; warning logged. |
| Expired session | Error logged; returns `false`. |
| RPC failure | Exception logged; **`OnError`** invoked. |

## Satori integration

For **Satori**-backed experiments and live ops events, use the dedicated Satori module / `IVXSatori` where your project wires Nakama + Satori keys. Keep **one** primary funnel for monetization-critical events to avoid double counting; route debug builds through verbose `IVXAnalyticsManager` logs only.

**Cross-SDK:** JavaScript `ivx-satori`, Godot/Unreal Satori bindings mirror the same event names documented in [Satori API](../api/satori.md).

## Logout / reset

```csharp
IVXAnalyticsManager.Instance.Reset();
```

Ends session if needed, clears user properties, and clears init flag.

## See also

- [Analytics module](../modules/analytics.md)
- [Satori API](../api/satori.md)
- [Satori integration guide](../guides/satori-integration.md)
