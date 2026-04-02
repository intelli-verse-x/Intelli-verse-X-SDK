# Satori Analytics API Reference

Complete API reference for the Satori module providing live-ops analytics, feature flags, experiments, and messaging through Nakama RPCs.

---

## IVXSatoriClient

Singleton entry point for all Satori operations: events, identity, audiences, flags, experiments, live events, messages, and metrics.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXSatoriClient` | Singleton instance |
| `IsInitialized` | `bool` | Whether the client has been initialized |

### Methods

#### Initialize

```csharp
public void Initialize(IClient client, ISession session)
```

Initializes the Satori RPC client. Must be called before any async methods.

**Parameters:**
- `client` - Nakama `IClient`
- `session` - Authenticated Nakama `ISession`

**Throws:** `ArgumentNullException` if `client` or `session` is null

**Example:**
```csharp
IVXSatoriClient.Instance.Initialize(nakamaClient, nakamaSession);
```

---

#### RefreshSession

```csharp
public void RefreshSession(ISession session)
```

Swaps the session on the RPC client after Nakama token refresh.

**Parameters:**
- `session` - New authenticated `ISession`

---

#### CaptureEventAsync

```csharp
public async Task CaptureEventAsync(string eventName, Dictionary<string, string> metadata = null)
```

Sends a single analytics event to the server.

**Parameters:**
- `eventName` - Event identifier
- `metadata` - Optional key-value metadata

**Example:**
```csharp
await IVXSatoriClient.Instance.CaptureEventAsync("level_complete");

await IVXSatoriClient.Instance.CaptureEventAsync("purchase_started", new Dictionary<string, string>
{
    ["sku"] = "gem_pack_small",
    ["currency"] = "USD"
});
```

---

#### CaptureEventsBatchAsync

```csharp
public async Task<IVXSatoriEventBatchResponse> CaptureEventsBatchAsync(List<IVXSatoriEvent> events)
```

Sends multiple events in a single batch.

**Parameters:**
- `events` - List of `IVXSatoriEvent` objects

**Returns:** `IVXSatoriEventBatchResponse` with `submitted` and `captured` counts

---

#### GetIdentityAsync

```csharp
public async Task<IVXSatoriIdentity> GetIdentityAsync()
```

Fetches the merged identity state (default, custom, and computed properties).

**Returns:** `IVXSatoriIdentity`

**Example:**
```csharp
var id = await IVXSatoriClient.Instance.GetIdentityAsync();
foreach (var kv in id.customProperties)
    Debug.Log($"{kv.Key}={kv.Value}");
```

---

#### UpdateIdentityAsync

```csharp
public async Task UpdateIdentityAsync(Dictionary<string, string> defaultProperties = null, Dictionary<string, string> customProperties = null)
```

Updates writable identity properties. Either dictionary may be `null`.

**Parameters:**
- `defaultProperties` - Default properties to update
- `customProperties` - Custom properties to update

---

#### GetAudienceMembershipsAsync

```csharp
public async Task<List<string>> GetAudienceMembershipsAsync()
```

Returns the list of audience IDs the player belongs to.

**Returns:** `List<string>` of audience IDs

**Example:**
```csharp
var audiences = await IVXSatoriClient.Instance.GetAudienceMembershipsAsync();
bool isVip = audiences.Contains("vip_segment");
```

---

#### GetFlagAsync

```csharp
public async Task<IVXSatoriFlag> GetFlagAsync(string flagName, string defaultValue = null)
```

Reads a single feature flag by name.

**Parameters:**
- `flagName` - Flag identifier
- `defaultValue` - Fallback value if the flag is not found

**Returns:** `IVXSatoriFlag`

**Example:**
```csharp
var flag = await IVXSatoriClient.Instance.GetFlagAsync("new_store_ui", "false");
if (flag.enabled && flag.value == "true")
    EnableNewStoreUi();
```

---

#### GetAllFlagsAsync

```csharp
public async Task<List<IVXSatoriFlag>> GetAllFlagsAsync(List<string> names = null)
```

Reads all feature flags, optionally filtered by name.

**Parameters:**
- `names` - Optional list of flag names to filter

**Returns:** `List<IVXSatoriFlag>`

---

#### GetExperimentsAsync

```csharp
public async Task<IVXSatoriExperimentsResponse> GetExperimentsAsync()
```

Fetches all experiments and the player's assigned variants.

**Returns:** `IVXSatoriExperimentsResponse`

---

#### GetExperimentVariantAsync

```csharp
public async Task<IVXSatoriExperimentVariant> GetExperimentVariantAsync(string experimentId)
```

Fetches the player's variant assignment for a single experiment.

**Parameters:**
- `experimentId` - Experiment identifier

**Returns:** `IVXSatoriExperimentVariant` or `null` if not assigned

**Example:**
```csharp
var variant = await IVXSatoriClient.Instance.GetExperimentVariantAsync("exp_onboarding");
if (variant != null)
{
    string layout = variant.config.GetValueOrDefault("layout", "control");
    ApplyLayout(layout);
}
```

---

#### GetLiveEventsAsync

```csharp
public async Task<IVXSatoriLiveEventsResponse> GetLiveEventsAsync(List<string> names = null)
```

Lists live events, optionally filtered by name.

**Parameters:**
- `names` - Optional event name filter

**Returns:** `IVXSatoriLiveEventsResponse`

---

#### JoinLiveEventAsync

```csharp
public async Task<bool> JoinLiveEventAsync(string eventId)
```

Joins a live event.

**Parameters:**
- `eventId` - Event identifier

**Returns:** `true` on success

---

#### ClaimLiveEventAsync

```csharp
public async Task<IVXSatoriLiveEventReward> ClaimLiveEventAsync(string eventId, string gameId = null)
```

Claims a live event reward.

**Parameters:**
- `eventId` - Event identifier
- `gameId` - Optional game ID

**Returns:** `IVXSatoriLiveEventReward`

---

#### GetMessagesAsync

```csharp
public async Task<IVXSatoriMessagesResponse> GetMessagesAsync()
```

Fetches inbox messages.

**Returns:** `IVXSatoriMessagesResponse`

---

#### ReadMessageAsync

```csharp
public async Task<IVXSatoriMessageReadResponse> ReadMessageAsync(string messageId, string gameId = null)
```

Marks a message as read and returns any attached reward.

**Parameters:**
- `messageId` - Message identifier
- `gameId` - Optional game ID

**Returns:** `IVXSatoriMessageReadResponse`

---

#### DeleteMessageAsync

```csharp
public async Task<bool> DeleteMessageAsync(string messageId)
```

Deletes an inbox message.

**Parameters:**
- `messageId` - Message identifier

**Returns:** `true` on success

---

#### QueryMetricAsync

```csharp
public async Task<IVXSatoriMetricQueryResponse> QueryMetricAsync(string metricId, string startDate = null, string endDate = null, string granularity = null)
```

Queries aggregated metric series.

**Parameters:**
- `metricId` - Metric identifier
- `startDate` - Start date string (optional)
- `endDate` - End date string (optional)
- `granularity` - Bucket granularity (e.g. `"day"`, `"hour"`)

**Returns:** `IVXSatoriMetricQueryResponse`

**Example:**
```csharp
var series = await IVXSatoriClient.Instance.QueryMetricAsync(
    "daily_active_users", "2026-03-01", "2026-03-31", "day");
foreach (var pt in series.dataPoints)
    Debug.Log($"{pt.bucket}: count={pt.count}");
```

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnInitialized` | `Action<bool>` | Fired after successful `Initialize` |

---

## IVXSatoriRpcClient

Low-level RPC wrapper for Satori server calls.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `EnableDebugLogs` | `bool` (static) | Toggle RPC trace logging |

### Methods

#### CallAsync<T>

```csharp
public async Task<SatoriRpcResponse<T>> CallAsync<T>(string rpcId, object payload = null)
```

Typed RPC call with JSON envelope. Returns `default` on failure.

---

#### CallVoidAsync

```csharp
public async Task<bool> CallVoidAsync(string rpcId, object payload = null)
```

Fire-and-forget RPC. Returns `false` on failure.

---

#### UpdateSession

```csharp
public void UpdateSession(ISession session)
```

Swaps the session after token refresh.

---

## Data Models

### IVXSatoriEvent

| Property | Type | Description |
|----------|------|-------------|
| `name` | `string` | Event name |
| `timestamp` | `string` | ISO timestamp |
| `metadata` | `Dictionary<string, string>` | Event metadata |

### IVXSatoriEventBatchResponse

| Property | Type | Description |
|----------|------|-------------|
| `submitted` | `int` | Events submitted |
| `captured` | `int` | Events captured by the server |

### IVXSatoriIdentity

| Property | Type | Description |
|----------|------|-------------|
| `defaultProperties` | `Dictionary<string, string>` | Default identity properties |
| `customProperties` | `Dictionary<string, string>` | Custom identity properties |
| `computedProperties` | `Dictionary<string, string>` | Server-computed properties |

### IVXSatoriFlag

| Property | Type | Description |
|----------|------|-------------|
| `name` | `string` | Flag name |
| `value` | `string` | Flag value string |
| `enabled` | `bool` | Whether the flag is enabled |

### IVXSatoriExperiment

| Property | Type | Description |
|----------|------|-------------|
| `id` | `string` | Experiment identifier |
| `name` | `string` | Display name |
| `description` | `string` | Experiment description |
| `status` | `string` | Status (e.g. `"running"`, `"completed"`) |
| `variant` | `IVXSatoriExperimentVariant` | Assigned variant |

### IVXSatoriExperimentVariant

| Property | Type | Description |
|----------|------|-------------|
| `id` | `string` | Variant identifier |
| `name` | `string` | Variant name |
| `config` | `Dictionary<string, string>` | Variant configuration values |

### IVXSatoriLiveEvent

| Property | Type | Description |
|----------|------|-------------|
| `id` | `string` | Event identifier |
| `name` | `string` | Display name |
| `description` | `string` | Event description |
| `startAt` | `long` | Start timestamp |
| `endAt` | `long` | End timestamp |
| `status` | `string` | Current status |
| `joined` | `bool` | Whether the player joined |
| `claimed` | `bool` | Whether the reward was claimed |
| `hasReward` | `bool` | Whether a reward is available |
| `config` | `Dictionary<string, string>` | Event configuration |
| `IsActive` | `bool` (computed) | Whether the event is currently active |
| `IsUpcoming` | `bool` (computed) | Whether the event has not started |
| `IsEnded` | `bool` (computed) | Whether the event has ended |

### IVXSatoriLiveEventReward

| Property | Type | Description |
|----------|------|-------------|
| `currencies` | `Dictionary<string, float>` | Currency rewards |
| `items` | `Dictionary<string, int>` | Item rewards |

### IVXSatoriMessage

| Property | Type | Description |
|----------|------|-------------|
| `id` | `string` | Message identifier |
| `title` | `string` | Message title |
| `body` | `string` | Message body text |
| `imageUrl` | `string` | Image URL |
| `metadata` | `Dictionary<string, string>` | Message metadata |
| `hasReward` | `bool` | Whether a reward is attached |
| `createdAt` | `long` | Creation timestamp |
| `expiresAt` | `long` | Expiry timestamp |
| `readAt` | `long` | Read timestamp |
| `consumedAt` | `long` | Consumed timestamp |
| `IsRead` | `bool` (computed) | Whether the message has been read |
| `IsConsumed` | `bool` (computed) | Whether the message has been consumed |

### IVXSatoriMetricQueryResponse

| Property | Type | Description |
|----------|------|-------------|
| `metricId` | `string` | Metric identifier |
| `dataPoints` | `List<IVXSatoriMetricDataPoint>` | Aggregated data points |

### IVXSatoriMetricDataPoint

| Property | Type | Description |
|----------|------|-------------|
| `bucket` | `string` | Time bucket label |
| `count` | `int` | Count in bucket |
| `sum` | `float` | Sum of values |
| `min` | `float` | Minimum value |
| `max` | `float` | Maximum value |
| `avg` | `float` | Average value |

---

## See Also

- [Satori Module Guide](../modules/satori.md)
- [Satori Integration Guide](../guides/satori-integration.md)
- [Backend Module](../modules/backend.md)
- [Hiro Module](../modules/hiro.md)
