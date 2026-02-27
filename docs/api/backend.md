# Backend API Reference

Complete API reference for the Backend (Nakama) module.

---

## IVXNakamaManager

Nakama server connection and communication manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXNakamaManager` | Singleton instance |
| `IsConnected` | `bool` | Connection status |
| `Session` | `ISession` | Current Nakama session |
| `Socket` | `ISocket` | Realtime socket |
| `Client` | `Client` | Nakama client |

---

### Connection Methods

#### ConnectAsync

```csharp
public async Task ConnectAsync()
```

Connects to the Nakama server.

---

#### ConnectSocketAsync

```csharp
public async Task ConnectSocketAsync()
```

Establishes realtime socket connection.

**Example:**
```csharp
await IVXNakamaManager.Instance.ConnectAsync();
await IVXNakamaManager.Instance.ConnectSocketAsync();
```

---

#### DisconnectSocketAsync

```csharp
public async Task DisconnectSocketAsync()
```

Closes the socket connection.

---

### Authentication Methods

#### AuthenticateCustomAsync

```csharp
public async Task<ISession> AuthenticateCustomAsync(
    string customId,
    string username = null,
    bool create = true)
```

Authenticates with a custom identifier.

**Parameters:**
- `customId` - Unique identifier
- `username` - Optional username
- `create` - Create if doesn't exist

**Returns:** Nakama session

---

#### RefreshSessionAsync

```csharp
public async Task<ISession> RefreshSessionAsync()
```

Refreshes the current session token.

---

### RPC Methods

#### RpcAsync

```csharp
public async Task<IApiRpc> RpcAsync(string id, string payload = "{}")
```

Calls a server-side RPC function.

**Parameters:**
- `id` - RPC function name
- `payload` - JSON payload

**Returns:** RPC response

**Example:**
```csharp
var response = await IVXNakamaManager.Instance.RpcAsync(
    "get_daily_rewards",
    JsonUtility.ToJson(new { userId = "123" })
);
var rewards = JsonUtility.FromJson<RewardsResponse>(response.Payload);
```

---

### Storage Methods

#### WriteStorageObjectAsync

```csharp
public async Task WriteStorageObjectAsync(
    string collection,
    string key,
    string value,
    int readPermission = 1,
    int writePermission = 1)
```

Writes data to server storage.

**Parameters:**
- `collection` - Storage collection name
- `key` - Object key
- `value` - JSON value
- `readPermission` - Read permission (0-3)
- `writePermission` - Write permission (0-1)

**Example:**
```csharp
await IVXNakamaManager.Instance.WriteStorageObjectAsync(
    "player_data",
    "inventory",
    JsonUtility.ToJson(inventory),
    readPermission: 1,  // Owner only
    writePermission: 1  // Owner only
);
```

---

#### WriteStorageObjectsAsync

```csharp
public async Task WriteStorageObjectsAsync(IEnumerable<WriteStorageObject> objects)
```

Writes multiple objects in a batch.

---

#### ReadStorageObjectAsync

```csharp
public async Task<IApiStorageObject> ReadStorageObjectAsync(
    string collection,
    string key,
    string userId = null)
```

Reads data from server storage.

**Parameters:**
- `collection` - Storage collection
- `key` - Object key
- `userId` - Optional user ID (defaults to current user)

**Returns:** Storage object or `null`

---

#### ReadStorageObjectsAsync

```csharp
public async Task<IApiStorageObjects> ReadStorageObjectsAsync(
    IEnumerable<ReadStorageObjectId> objectIds)
```

Reads multiple objects in a batch.

---

#### DeleteStorageObjectsAsync

```csharp
public async Task DeleteStorageObjectsAsync(
    IEnumerable<DeleteStorageObjectId> objectIds)
```

Deletes storage objects.

---

### Leaderboard Methods

#### WriteLeaderboardRecordAsync

```csharp
public async Task<IApiLeaderboardRecord> WriteLeaderboardRecordAsync(
    string leaderboardId,
    long score,
    long subscore = 0,
    string metadata = null)
```

Submits a score to a leaderboard.

**Parameters:**
- `leaderboardId` - Leaderboard identifier
- `score` - Score value
- `subscore` - Secondary score
- `metadata` - Optional JSON metadata

**Example:**
```csharp
await IVXNakamaManager.Instance.WriteLeaderboardRecordAsync(
    "high_scores",
    score: 1500,
    metadata: JsonUtility.ToJson(new { level = 10 })
);
```

---

#### ListLeaderboardRecordsAsync

```csharp
public async Task<IApiLeaderboardRecordList> ListLeaderboardRecordsAsync(
    string leaderboardId,
    int limit = 10,
    string cursor = null)
```

Gets leaderboard entries.

---

#### ListLeaderboardRecordsAroundOwnerAsync

```csharp
public async Task<IApiLeaderboardRecordList> ListLeaderboardRecordsAroundOwnerAsync(
    string leaderboardId,
    string ownerId,
    int limit = 10)
```

Gets entries around a specific player.

---

### Notification Methods

#### ListNotificationsAsync

```csharp
public async Task<IApiNotificationList> ListNotificationsAsync(
    int limit = 50,
    string cacheableCursor = null)
```

Lists user notifications.

---

#### DeleteNotificationsAsync

```csharp
public async Task DeleteNotificationsAsync(IEnumerable<string> notificationIds)
```

Deletes notifications.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnConnected` | `Action` | Server connected |
| `OnDisconnected` | `Action` | Server disconnected |
| `OnConnectionStateChanged` | `Action<ConnectionState>` | State changed |
| `OnError` | `Action<Exception>` | Error occurred |

---

## ConnectionState Enum

```csharp
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
```

---

## Permission Levels

### Read Permissions

| Value | Level | Description |
|-------|-------|-------------|
| 0 | None | No one can read |
| 1 | Owner | Only owner |
| 2 | Friends | Owner and friends |
| 3 | Public | Anyone |

### Write Permissions

| Value | Level | Description |
|-------|-------|-------------|
| 0 | None | No one can write |
| 1 | Owner | Only owner |

---

## See Also

- [Backend Module Guide](../modules/backend.md)
- [Nakama Integration Guide](../guides/nakama-integration.md)
- [Backend Configuration](../configuration/backend-config.md)
