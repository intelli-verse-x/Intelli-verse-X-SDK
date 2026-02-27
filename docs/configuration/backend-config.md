# Backend Configuration

Configure connection to the Nakama game server backend.

---

## Overview

The IntelliVerseX SDK connects to a Nakama server for:

- User authentication
- Cloud storage
- Leaderboards
- Matchmaking
- Real-time multiplayer
- Server-side logic (RPCs)

---

## Backend Settings in Config

### Using IntelliVerseXConfig

Navigate to **IntelliVerseX > Game Config** and expand Backend Settings:

| Setting | Description | Default |
|---------|-------------|---------|
| Nakama Server URL | Backend server hostname | Production server |
| Use SSL | Connect via HTTPS | `true` |
| Server Port | Connection port | `443` (SSL) / `7349` (non-SSL) |
| Timeout | Connection timeout (seconds) | `30` |

---

## Connection Configuration

### Production Settings

```csharp
// These are configured in IntelliVerseXConfig
// Default production server: nakama-rest.intelli-verse-x.ai:443
```

### Custom Server Setup

For self-hosted Nakama:

1. In `IntelliVerseXConfig`:
   - Set **Nakama Server URL** to your server address
   - Configure SSL based on your setup
   - Adjust port if needed

2. Or configure via code:
```csharp
// Not recommended - prefer config asset
// But available for special cases
```

---

## Server Key

!!! warning "Security"
    The server key is embedded in builds. Never use a secret key on the client side.
    
The default server key is configured in your Nakama server's `config.yml`. The client uses the public-facing server key for connection.

---

## Sessions

### Session Persistence

Sessions are automatically managed:

```csharp
// Sessions persist to secure storage
// Auto-refresh before expiry
// Manual refresh if needed:
await IVXNakamaManager.Instance.RefreshSessionAsync();
```

### Session Timeout

Configure server-side in Nakama `config.yml`:
```yaml
session:
  token_expiry_sec: 86400  # 24 hours
  refresh_token_expiry_sec: 604800  # 7 days
```

---

## Retry Logic

### Built-in Retry

The SDK handles transient failures:

```csharp
// Automatic retry for:
// - Network timeouts
// - Server 500 errors
// - Connection drops

// Configurable retry count
// Default: 3 attempts with exponential backoff
```

### Custom Retry Configuration

```csharp
// Configure in IntelliVerseXConfig
// - Max Retries: Number of retry attempts
// - Retry Delay: Base delay between retries
// - Use Exponential Backoff: true/false
```

---

## Connection Events

### Monitor Connection Status

```csharp
// Subscribe to connection events
IVXNakamaManager.OnConnectionStateChanged += (state) =>
{
    switch (state)
    {
        case ConnectionState.Connected:
            Debug.Log("Backend connected");
            break;
        case ConnectionState.Disconnected:
            Debug.Log("Backend disconnected");
            ShowReconnectUI();
            break;
        case ConnectionState.Reconnecting:
            Debug.Log("Reconnecting...");
            break;
    }
};
```

---

## Real-time Connection

### Socket Configuration

For real-time features (multiplayer, chat):

```csharp
// Socket connects automatically when needed
// Configure in IntelliVerseXConfig:
// - Heartbeat Interval: Keep-alive frequency
// - Reconnect on Disconnect: Auto-reconnect toggle
```

### Manual Socket Management

```csharp
// Connect socket manually
await IVXNakamaManager.Instance.ConnectSocketAsync();

// Disconnect when not needed
await IVXNakamaManager.Instance.DisconnectSocketAsync();
```

---

## Environment Switching

### Development vs Production

Create separate config assets for different environments:

1. Create `IntelliVerseXConfig_Dev.asset` for development
2. Create `IntelliVerseXConfig_Prod.asset` for production
3. Use preprocessor directives or build scripts to switch

```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    // Use dev config
#else
    // Use prod config
#endif
```

---

## SSL/TLS Settings

### Certificate Pinning

For enhanced security, enable certificate pinning:

```csharp
// Certificate pinning prevents MITM attacks
// Configure in IntelliVerseXConfig:
// - Enable Certificate Pinning: true
// - Certificate Fingerprint: SHA-256 hash
```

!!! note
    Update pinned certificates before they expire to avoid breaking the app.

---

## Offline Mode

### Handle Offline Gracefully

```csharp
// Check connectivity
if (Application.internetReachability == NetworkReachability.NotReachable)
{
    // Switch to offline mode
    Debug.Log("No internet - using cached data");
}

// Or use SDK connectivity check
if (!IVXNakamaManager.Instance.IsConnected)
{
    // Handle offline state
}
```

### Offline Queue

Operations attempted while offline are queued:

```csharp
// Operations automatically queue when offline
// Sync when connection restored
await IVXNakamaManager.Instance.SyncQueuedOperationsAsync();
```

---

## Debugging

### Enable Backend Logging

```csharp
// In IntelliVerseXConfig, set Log Level to Debug
// This logs all backend communication

// Or programmatically:
IVXLogger.SetLogLevel(LogLevel.Debug);
```

### View Request/Response

```csharp
// Enable verbose logging to see full payloads
// Warning: May contain sensitive data - for development only
```

---

## Best Practices

### 1. Connection Management

```csharp
// Let SDK manage connections
// Don't repeatedly connect/disconnect

// DON'T do this:
void Update()
{
    if (needsData)
        IVXNakamaManager.Connect(); // Bad!
}

// DO this:
void Start()
{
    // SDK auto-connects on initialization
}
```

### 2. Error Handling

```csharp
try
{
    var result = await IVXNakamaManager.Instance.CallRpcAsync("function", payload);
}
catch (NakamaException ex)
{
    // Handle specific error
    if (ex.StatusCode == 401)
    {
        // Session expired - re-authenticate
    }
}
```

### 3. Session Refresh

```csharp
// Sessions auto-refresh, but check before critical operations
if (IVXNakamaManager.Instance.IsSessionExpired)
{
    await IVXNakamaManager.Instance.RefreshSessionAsync();
}
```

---

## Server-Side Configuration

For backend administrators, key Nakama settings:

```yaml
# nakama/config.yml
name: "my-game"
data_dir: "./data/"

session:
  token_expiry_sec: 86400
  refresh_token_expiry_sec: 604800

socket:
  max_message_size_bytes: 4096
  write_wait_ms: 5000
  pong_wait_ms: 10000

runtime:
  js_entrypoint: "build/index.js"
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Check server URL, firewall, network |
| SSL handshake failed | Verify SSL cert is valid, date/time correct |
| Session expired | Call `RefreshSessionAsync()` or re-authenticate |
| Socket won't connect | Ensure authenticated first |

See [Runtime Issues](../troubleshooting/runtime-issues.md) for more solutions.
