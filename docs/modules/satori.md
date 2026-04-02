# Satori Analytics Module

The Satori module exposes **Heroic Labs Satori–style live-ops and analytics** through Nakama RPCs. The Unity client sends events, reads identity properties, audiences, feature flags, experiments, live events, inbox messages, and metrics; the server validates and forwards to your configured Satori pipeline. All traffic uses the same authenticated Nakama session as the rest of IntelliVerseX.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Satori` |
| **Assembly** | `IntelliVerseX.Satori` |
| **Dependencies** | `IntelliVerseX.Core`, `IntelliVerseX.Backend`, Nakama Unity SDK, Newtonsoft.Json |
| **Entry point** | `IVXSatoriClient` (singleton `MonoBehaviour`) |
| **RPC client** | `IVXSatoriRpcClient` (low-level RPC wrapper; constructed inside `Initialize`) |

### Components

| Component | Responsibility |
|-----------|----------------|
| `IVXSatoriClient` | Public API: events, identity, audiences, flags, experiments, live events, messages, metrics |
| `IVXSatoriRpcClient` | `CallAsync<T>`, `CallVoidAsync`, `UpdateSession`; JSON envelope `SatoriRpcResponse<T>` |

---

## Setup

### Prerequisites

- Nakama **Unity SDK** (`com.heroiclabs.nakama-unity`) with `INTELLIVERSEX_HAS_NAKAMA` defined.
- A valid Nakama `IClient` and authenticated `ISession` (see [Backend module](backend.md)).
- Server-side RPC handlers registered for the IDs used by `IVXSatoriClient` (e.g. `satori_event`, `satori_flags_get`, …)—see [guides/satori-integration.md](../guides/satori-integration.md).

### Initialization contract

`IVXSatoriClient` must be **initialized** before any async API runs. `Initialize` is synchronous and immediately sets the internal `IVXSatoriRpcClient` and raises `OnInitialized(true)`. `RefreshSession` swaps the session on the RPC client after token refresh.

```csharp
public void Initialize(IClient client, ISession session);
public void RefreshSession(ISession session);
```

`Initialize` throws `ArgumentNullException` if `client` or `session` is null.

---

### Path A — `IVXBootstrap` (component ensured, then wire Nakama)

With **`EnableSatori`** enabled on your `IVXBootstrapConfig` (default `true`), `IVXBootstrap.InitializeAsync` **creates** an `IVX_SatoriClient` child object and adds `IVXSatoriClient` when needed. The bootstrap **does not** call `Initialize(IClient, ISession)`; you supply Nakama client and session once auth is ready.

Typical pattern: subscribe to `OnBootstrapComplete`, rebuild `IClient` from the same host/port/key as the bootstrap config, restore `ISession` from the bootstrap auth token, then initialize Satori (and Hiro) together:

```csharp
using IntelliVerseX.Bootstrap;
using IntelliVerseX.Hiro;
using IntelliVerseX.Satori;
using Nakama;
using UnityEngine;

public sealed class PostBootstrapNakamaWireup : MonoBehaviour
{
    private void OnEnable()
    {
        IVXBootstrap.Instance.OnBootstrapComplete += OnBootstrapComplete;
    }

    private void OnDisable()
    {
        if (IVXBootstrap.Instance != null)
            IVXBootstrap.Instance.OnBootstrapComplete -= OnBootstrapComplete;
    }

    private void OnBootstrapComplete(bool success)
    {
        if (!success) return;

#if INTELLIVERSEX_HAS_NAKAMA
        var boot = IVXBootstrap.Instance;
        var cfg = boot.Config;
        var scheme = cfg.UseSSL ? "https" : "http";
        var client = new Client(scheme, cfg.ServerHost, cfg.ServerPort, cfg.ServerKey);
        var session = Session.Restore(boot.AuthToken);

        IVXHiroCoordinator.Instance.InitializeSystems(client, session);
        IVXSatoriClient.Instance.Initialize(client, session);
#endif
    }
}
```

If you **disable** `AutoDeviceAuth` and call `SetAuth` before `InitializeAsync`, you are responsible for consistent `IClient` / `ISession` values when calling `Initialize`.

---

### Path B — Manual (no bootstrap wiring)

Add `IVXSatoriClient` to a persistent scene object (or rely on bootstrap only to create the GameObject), then call `Initialize` after Nakama authentication—same order as in the integration prompt:

```csharp
using IntelliVerseX.Satori;
using Nakama;
using UnityEngine;

public sealed class ManualSatoriBootstrap : MonoBehaviour
{
    private async void Start()
    {
#if INTELLIVERSEX_HAS_NAKAMA
        var client = new Client("http", "your-server.com", 7350, "defaultkey");
        var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);

        if (IVXSatoriClient.Instance == null)
        {
            var go = new GameObject("IVX_SatoriClient");
            DontDestroyOnLoad(go);
            go.AddComponent<IVXSatoriClient>();
        }

        IVXSatoriClient.Instance.Initialize(client, session);
#endif
    }
}
```

### Session refresh

After Nakama issues a new session (e.g. token refresh):

```csharp
IVXSatoriClient.Instance.RefreshSession(newSession);
```

---

## Event capture

### Single event

```csharp
await IVXSatoriClient.Instance.CaptureEventAsync("level_complete");

await IVXSatoriClient.Instance.CaptureEventAsync("purchase_started", new Dictionary<string, string>
{
    ["sku"] = "gem_pack_small",
    ["currency"] = "USD"
});
```

### Batch

```csharp
var events = new List<IVXSatoriEvent>
{
    new IVXSatoriEvent("tutorial_step", new Dictionary<string, string> { ["step"] = "1" }),
    new IVXSatoriEvent("tutorial_step", new Dictionary<string, string> { ["step"] = "2" })
};

IVXSatoriEventBatchResponse batch =
    await IVXSatoriClient.Instance.CaptureEventsBatchAsync(events);

Debug.Log($"submitted={batch.submitted}, captured={batch.captured}");
```

`IVXSatoriEvent` exposes `name`, `timestamp`, and `metadata`. `IVXSatoriEventBatchResponse` exposes `submitted` and `captured`.

---

## Identity properties

Fetch merged identity state (default, custom, and computed properties as returned by the server):

```csharp
IVXSatoriIdentity id = await IVXSatoriClient.Instance.GetIdentityAsync();

foreach (var kv in id.defaultProperties)
    Debug.Log($"default.{kv.Key}={kv.Value}");
foreach (var kv in id.customProperties)
    Debug.Log($"custom.{kv.Key}={kv.Value}");
foreach (var kv in id.computedProperties)
    Debug.Log($"computed.{kv.Key}={kv.Value}");
```

Update writable properties:

```csharp
await IVXSatoriClient.Instance.UpdateIdentityAsync(
    defaultProperties: new Dictionary<string, string> { ["platform"] = Application.platform.ToString() },
    customProperties: new Dictionary<string, string> { ["favorite_mode"] = "ranked" });
```

Either dictionary may be `null` if you only need to update one side.

---

## Audiences

```csharp
List<string> audienceIds = await IVXSatoriClient.Instance.GetAudienceMembershipsAsync();
bool inVip = audienceIds.Contains("vip_segment");
```

---

## Feature flags

### Read one flag

```csharp
IVXSatoriFlag flag = await IVXSatoriClient.Instance.GetFlagAsync("new_store_ui", defaultValue: "false");

Debug.Log($"{flag.name} value={flag.value} enabled={flag.enabled}");
```

`IVXSatoriFlag` exposes `name`, `value`, and `enabled`.

### Read all flags (optional name filter)

```csharp
List<IVXSatoriFlag> all = await IVXSatoriClient.Instance.GetAllFlagsAsync();

List<IVXSatoriFlag> subset = await IVXSatoriClient.Instance.GetAllFlagsAsync(
    new List<string> { "ads_enabled", "seasonal_theme" });
```

### Gating features in game code

Use `enabled` and/or parse `value` for thresholds. Always provide a safe default path when RPC fails (see [Error handling](#error-handling)).

```csharp
IVXSatoriFlag beta = await IVXSatoriClient.Instance.GetFlagAsync("beta_hud", "0");
bool showBetaHud = beta.enabled && (beta.value == "1" || beta.value.Equals("true", StringComparison.OrdinalIgnoreCase));

if (showBetaHud)
    EnableBetaHud();
else
    EnableStableHud();
```

---

## Experiments / A–B testing

### All experiments and assignments

```csharp
IVXSatoriExperimentsResponse resp = await IVXSatoriClient.Instance.GetExperimentsAsync();

foreach (IVXSatoriExperiment ex in resp.experiments)
{
    Debug.Log($"{ex.name} ({ex.id}) status={ex.status} variant={ex.variant?.name}");
}
```

`IVXSatoriExperiment` exposes `id`, `name`, `description`, `status`, and `variant` (`IVXSatoriExperimentVariant`).

### Single experiment variant

```csharp
IVXSatoriExperimentVariant variant =
    await IVXSatoriClient.Instance.GetExperimentVariantAsync("exp_onboarding_flow");

if (variant != null)
{
    string layout = variant.config.TryGetValue("layout", out var v) ? v : "control";
    ApplyOnboardingLayout(layout);
}
```

`IVXSatoriExperimentVariant` exposes `id`, `name`, and `config` (`Dictionary<string, string>`).

---

## Live events

### List events (optional filter by names)

```csharp
IVXSatoriLiveEventsResponse live = await IVXSatoriClient.Instance.GetLiveEventsAsync();

foreach (IVXSatoriLiveEvent ev in live.events)
{
    if (ev.IsActive)
        ShowLiveEventCard(ev.name, ev.description);
    else if (ev.IsUpcoming)
        ScheduleReminder(ev.startAt);
}

// Filtered fetch
var onlyThese = new List<string> { "weekend_double_xp" };
IVXSatoriLiveEventsResponse filtered =
    await IVXSatoriClient.Instance.GetLiveEventsAsync(onlyThese);
```

`IVXSatoriLiveEvent` includes `id`, `name`, `description`, `startAt`, `endAt`, `status`, `joined`, `claimed`, `hasReward`, `config`, and helpers `IsActive`, `IsUpcoming`, `IsEnded`.

### Join and claim

```csharp
bool joined = await IVXSatoriClient.Instance.JoinLiveEventAsync(eventId);
if (!joined)
    ShowJoinFailedToast();

IVXSatoriLiveEventReward reward =
    await IVXSatoriClient.Instance.ClaimLiveEventAsync(eventId, gameId: null);

foreach (var c in reward.currencies)
    CreditCurrency(c.Key, c.Value);
foreach (var item in reward.items)
    GrantItem(item.Key, item.Value);
```

---

## Messages (inbox)

```csharp
IVXSatoriMessagesResponse inbox = await IVXSatoriClient.Instance.GetMessagesAsync();

foreach (IVXSatoriMessage msg in inbox.messages)
{
    if (msg.IsConsumed) continue;
    RenderInboxRow(msg.title, msg.body, msg.imageUrl, msg.hasReward);
}

IVXSatoriMessageReadResponse read =
    await IVXSatoriClient.Instance.ReadMessageAsync(messageId, gameId: null);

if (read?.reward != null)
    ApplyReward(read.reward);

bool deleted = await IVXSatoriClient.Instance.DeleteMessageAsync(messageId);
```

`IVXSatoriMessage` exposes `id`, `title`, `body`, `imageUrl`, `metadata`, `hasReward`, `createdAt`, `expiresAt`, `readAt`, `consumedAt`, plus `IsRead` and `IsConsumed`. `IVXSatoriMessageReadResponse` exposes `message` and `reward` (`IVXSatoriLiveEventReward`).

---

## Metrics

Query aggregated metric series with optional range and granularity (format is whatever your server expects and documents):

```csharp
IVXSatoriMetricQueryResponse series = await IVXSatoriClient.Instance.QueryMetricAsync(
    metricId: "daily_active_users",
    startDate: "2026-03-01",
    endDate: "2026-03-31",
    granularity: "day");

foreach (IVXSatoriMetricDataPoint pt in series.dataPoints)
{
    Debug.Log($"bucket={pt.bucket} count={pt.count} sum={pt.sum} min={pt.min} max={pt.max} avg={pt.avg}");
}
```

`IVXSatoriMetricQueryResponse` exposes `metricId` and `dataPoints`. Each `IVXSatoriMetricDataPoint` exposes `bucket`, `count`, `sum`, `min`, `max`, and `avg`.

---

## Events and callbacks

| Event | Signature | When it fires |
|-------|-----------|----------------|
| `OnInitialized` | `Action<bool>` | Immediately after a successful `Initialize`; argument is `true` (failure before init throws or never reaches this path for success) |

Subscribe before or right before `Initialize`:

```csharp
IVXSatoriClient.Instance.OnInitialized += ok =>
{
    if (ok)
        Debug.Log("Satori RPC client ready.");
};

IVXSatoriClient.Instance.Initialize(client, session);
```

`IsInitialized` is `true` after `Initialize` completes.

---

## Error handling

1. **Not initialized** — Any public async method calls `EnsureReady()` internally. If `Initialize` was not called, it throws `InvalidOperationException`: *"IVXSatoriClient is not initialized. Call Initialize() first."*

2. **`Initialize` arguments** — `ArgumentNullException` if `client` or `session` is null.

3. **RPC layer (`IVXSatoriRpcClient`)** — `CallAsync` / `CallVoidAsync` throw `InvalidOperationException` if the session is null or expired. On transport or parse failures, `CallAsync` returns `default` (`null` for reference types) and logs errors; `CallVoidAsync` returns `false` on failure. Server-side errors in the JSON envelope are logged as warnings; failed calls return defaults.

4. **Defensive game code** — Null-check reference results (`GetExperimentVariantAsync`, `ReadMessageAsync`) and treat empty DTOs as “no data”:

```csharp
var variant = await IVXSatoriClient.Instance.GetExperimentVariantAsync(id);
if (variant == null)
    UseControlExperience();

var read = await IVXSatoriClient.Instance.ReadMessageAsync(mid);
if (read?.message == null)
    RefreshInboxUi();
```

5. **Debug logging** — Toggle RPC trace logs: `IVXSatoriRpcClient.EnableDebugLogs = false` for release builds.

---

## Cross-platform status

| Platform | Support |
|----------|---------|
| Unity | **Y** — Full `IVXSatoriClient` / Nakama RPC path |
| Unreal | **S** — Stub / separate native integration (see platform SDKs) |
| Godot | **S** |
| Defold | **S** |
| Cocos2d-x | **S** |
| JavaScript / TypeScript | **S** |
| C++ | **S** |
| Java (Android) | **S** |
| Flutter | **S** |
| Web3 | **S** |

“**S**” means not provided by this Unity module; use the corresponding IntelliVerseX SDK or custom bridge for that stack.

---

## API summary (`IVXSatoriClient`)

| Area | Methods |
|------|---------|
| Initialization | `Initialize(IClient, ISession)`, `RefreshSession(ISession)` |
| Events | `CaptureEventAsync(string, Dictionary<string,string>?)`, `CaptureEventsBatchAsync(List<IVXSatoriEvent>)` |
| Identity | `GetIdentityAsync()`, `UpdateIdentityAsync(Dictionary<string,string>?, Dictionary<string,string>?)` |
| Audiences | `GetAudienceMembershipsAsync()` |
| Flags | `GetFlagAsync(string, string)`, `GetAllFlagsAsync(List<string>?)` |
| Experiments | `GetExperimentsAsync()`, `GetExperimentVariantAsync(string)` |
| Live events | `GetLiveEventsAsync(List<string>?)`, `JoinLiveEventAsync(string)`, `ClaimLiveEventAsync(string, string?)` |
| Messages | `GetMessagesAsync()`, `ReadMessageAsync(string, string?)`, `DeleteMessageAsync(string)` |
| Metrics | `QueryMetricAsync(string, string?, string?, string?)` |

---

## See also

- [Satori integration guide](../guides/satori-integration.md) — server RPCs, environment setup, and operational checklist  
- [Satori API reference](../api/satori.md) — detailed request/response contracts  
- [Feature coverage matrix](../FEATURE_COVERAGE_MATRIX.md) — platform and module coverage  
- [Backend module](backend.md) — Nakama client, session, and auth  
- [Hiro module](hiro.md) — live-ops systems that may consume Satori-driven personalization  
