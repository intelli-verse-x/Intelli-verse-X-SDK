# Satori Analytics Demo

Guide for setting up a test scene that exercises all Satori LiveOps features — event capture, identity properties, feature flags, experiments, live events, and messages — using `IVXSatoriClient`.

!!! note "No dedicated demo script"
    There is no pre-built `IVXSatoriDemo.cs` in the Demos folder. This page walks you through creating a minimal test scene using the `IVXSatoriClient` API directly.

---

## Prerequisites

- `IVXSatoriClient` component in the scene (singleton, `DontDestroyOnLoad`)
- A valid Nakama `IClient` and `ISession` (from `IVXBackendManager` or manual login)
- Satori RPC endpoints registered on the Nakama server

---

## Scene Setup

### Step 1 — Create the Test Scene

1. Create a new scene: `File > New Scene`
2. Add an empty GameObject named **"SatoriTestManager"**
3. Attach a new script `SatoriTestScene.cs`
4. Ensure `IVXSatoriClient` is in the scene (or auto-created via `IVXBootstrap`)

### Step 2 — Test Script

```csharp
using System.Collections.Generic;
using IntelliVerseX.Satori;
using IntelliVerseX.Backend;
using UnityEngine;

public class SatoriTestScene : MonoBehaviour
{
    private IVXSatoriClient _satori;

    private async void Start()
    {
        var backend = IVXBackendManager.Instance;
        await backend.ConnectAsync();

        _satori = IVXSatoriClient.Instance;
        _satori.Initialize(backend.Client, backend.Session);

        _satori.OnInitialized += (success) =>
            Debug.Log($"[Satori] Initialized: {success}");

        TestEvents();
        TestFlags();
        TestExperiments();
        TestLiveEvents();
        TestMessages();
    }
}
```

---

## Event Capture

Track custom analytics events with metadata.

```csharp
private async void TestEvents()
{
    // Single event
    await _satori.PublishEventAsync("level_complete", new Dictionary<string, string>
    {
        { "level", "3" },
        { "score", "9500" },
        { "time_seconds", "142" }
    });
    Debug.Log("[Satori] Event published: level_complete");

    // Batch events
    var events = new List<IVXSatoriEvent>
    {
        new() { Name = "item_purchased", Metadata = new() { { "item", "sword" }, { "price", "100" } } },
        new() { Name = "ad_watched",     Metadata = new() { { "placement", "rewarded" } } },
        new() { Name = "session_start",  Metadata = new() { { "platform", "android" } } }
    };
    await _satori.PublishEventsBatchAsync(events);
    Debug.Log($"[Satori] Batch published: {events.Count} events");
}
```

---

## Identity & Properties

Read and update player identity properties for segmentation.

```csharp
private async void TestIdentity()
{
    // Get current identity
    var identity = await _satori.GetIdentityAsync();
    Debug.Log($"[Satori] Identity: {identity}");

    // Update properties (used for audience targeting)
    await _satori.UpdatePropertiesAsync(new Dictionary<string, string>
    {
        { "vip_tier", "gold" },
        { "total_spend", "49.99" },
        { "preferred_mode", "versus" }
    });
    Debug.Log("[Satori] Properties updated");
}
```

---

## Feature Flags

Query feature flags to gate features dynamically.

```csharp
private async void TestFlags()
{
    // Get a specific flag
    var flag = await _satori.GetFlagAsync("new_shop_ui");
    Debug.Log($"[Satori] Flag 'new_shop_ui': {flag?.Value ?? "not found"}");

    // Get all flags
    var allFlags = await _satori.GetAllFlagsAsync();
    foreach (var f in allFlags)
        Debug.Log($"[Satori] Flag: {f.Name} = {f.Value}");
}
```

---

## Experiments

Retrieve A/B test variants assigned to the current player.

```csharp
private async void TestExperiments()
{
    // Get all active experiments
    var experiments = await _satori.GetExperimentsAsync();
    foreach (var exp in experiments)
        Debug.Log($"[Satori] Experiment: {exp.Name}, Variant: {exp.Variant}");

    // Get variant for a specific experiment
    var variant = await _satori.GetExperimentVariantAsync("onboarding_flow");
    Debug.Log($"[Satori] Onboarding variant: {variant}");
}
```

---

## Live Events

List, join, and claim rewards from time-limited live events.

```csharp
private async void TestLiveEvents()
{
    // List active live events
    var events = await _satori.ListLiveEventsAsync();
    foreach (var e in events)
        Debug.Log($"[Satori] Live event: {e.Name} ({e.StartTime} - {e.EndTime})");

    if (events.Count > 0)
    {
        // Join the first live event
        var eventId = events[0].Id;
        await _satori.JoinLiveEventAsync(eventId);
        Debug.Log($"[Satori] Joined: {events[0].Name}");

        // Claim reward
        var reward = await _satori.ClaimLiveEventRewardAsync(eventId);
        Debug.Log($"[Satori] Claimed reward: {reward}");
    }
}
```

---

## Messages

List, read, and manage in-app messages delivered via Satori.

```csharp
private async void TestMessages()
{
    // List unread messages
    var messages = await _satori.ListMessagesAsync();
    foreach (var m in messages)
        Debug.Log($"[Satori] Message: {m.Title} (read={m.IsRead})");

    if (messages.Count > 0)
    {
        // Mark as read
        await _satori.MarkMessageReadAsync(messages[0].Id);
        Debug.Log($"[Satori] Marked read: {messages[0].Title}");

        // Delete a message
        await _satori.DeleteMessageAsync(messages[0].Id);
        Debug.Log($"[Satori] Deleted: {messages[0].Id}");
    }
}
```

---

## Metrics

Query aggregated metrics for dashboards or in-game displays.

```csharp
private async void TestMetrics()
{
    var result = await _satori.QueryMetricsAsync("daily_active_users", "2026-03-01", "2026-04-01");
    Debug.Log($"[Satori] Metrics: {result}");
}
```

---

## How to Use

### Running the Test Scene

1. Create a new Unity scene
2. Add `IVXSatoriClient` and `IVXBackendManager` to the scene
3. Add the `SatoriTestScene` script above
4. Configure your Nakama connection settings
5. Press **Play** and check the Console for output

### Verifying on the Server

1. Open the Satori dashboard
2. Navigate to **Events** to see captured events
3. Check **Audiences** for player segmentation updates
4. Review **Experiments** for variant assignments
5. Verify **Live Events** participation

### Testing Without a Server

You can mock `IVXSatoriClient` by subclassing or using a test double that returns hardcoded data — useful for offline UI development.

---

## RPC Endpoint Reference

| RPC ID | Purpose |
|--------|---------|
| `satori_event` | Publish single event |
| `satori_events_batch` | Publish batch events |
| `satori_identity_get` | Get player identity |
| `satori_identity_update_properties` | Update identity properties |
| `satori_audiences_get_memberships` | Get audience memberships |
| `satori_flags_get` | Get specific flag |
| `satori_flags_get_all` | Get all flags |
| `satori_experiments_get` | Get experiments |
| `satori_experiments_get_variant` | Get experiment variant |
| `satori_live_events_list` | List live events |
| `satori_live_events_join` | Join live event |
| `satori_live_events_claim` | Claim live event reward |
| `satori_messages_list` | List messages |
| `satori_messages_read` | Mark message read |
| `satori_messages_delete` | Delete message |
| `satori_metrics_query` | Query metrics |

---

## See Also

- [Satori Module](../modules/satori.md)
- [Satori Integration Guide](../guides/satori-integration.md)
- [Nakama Integration Guide](../guides/nakama-integration.md)
