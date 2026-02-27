# Runtime Issues

Common runtime errors and solutions.

---

## Initialization Issues

### SDK Not Initializing

**Symptom:** `IntelliVerseXSDK.IsInitialized` always `false`

**Cause 1:** Config not found
```csharp
// Check if config exists
var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseXConfig");
if (config == null)
{
    Debug.LogError("Config not found at Resources/IntelliVerseXConfig");
}
```

**Solution:** Create config via `Assets → Create → IntelliVerse-X → IntelliVerseX Config`

**Cause 2:** Initialization not awaited
```csharp
// ❌ Wrong - not awaited
void Start()
{
    IntelliVerseXSDK.InitializeAsync(); // Fire and forget
    // SDK not ready here!
}

// ✅ Correct - properly awaited
async void Start()
{
    await IntelliVerseXSDK.InitializeAsync();
    // SDK is ready
}
```

---

### NullReferenceException on Start

**Error:**
```
NullReferenceException: Object reference not set to an instance of an object
IVXSomeManager.DoSomething()
```

**Cause:** Accessing SDK before initialization

**Solution:**
```csharp
async void Start()
{
    // Always initialize first
    if (!IntelliVerseXSDK.IsInitialized)
    {
        await IntelliVerseXSDK.InitializeAsync();
    }
    
    // Now safe to use
    IVXSomeManager.DoSomething();
}
```

---

## Connection Issues

### Connection Timeout

**Symptom:** Backend connection never completes

**Debug:**
```csharp
try
{
    await IVXNakamaManager.ConnectAsync();
}
catch (TimeoutException)
{
    Debug.LogError("Connection timed out");
    // Check: Is server running? Is URL correct? Is internet available?
}
```

**Solutions:**
1. Verify backend URL in config
2. Check internet connectivity
3. Test server is reachable
4. Check firewall/proxy settings

---

### Connection Lost During Gameplay

**Symptom:** Realtime features stop working

**Solution:**
```csharp
// Subscribe to connection events
IVXNakamaManager.OnDisconnected += () =>
{
    // Show reconnecting UI
    ShowReconnectingOverlay();
};

IVXNakamaManager.OnReconnected += () =>
{
    // Hide overlay, refresh data
    HideReconnectingOverlay();
    RefreshGameData();
};
```

---

## Authentication Issues

### Token Expired

**Error:**
```
AuthException: Session token has expired
```

**Solution:**
```csharp
// Auto-refresh is enabled by default
// If disabled, handle manually:
IVXAuthService.OnSessionExpired += async () =>
{
    await IVXAuthService.RefreshSessionAsync();
};
```

---

### Social Login Cancelled

**Error:**
```
AuthException: User cancelled login
```

**Solution:**
```csharp
try
{
    await IVXAuthService.LoginWithGoogleAsync(token);
}
catch (AuthException ex) when (ex.Message.Contains("cancelled"))
{
    // User cancelled - don't show error, just return
    return;
}
catch (AuthException ex)
{
    // Actual error
    ShowError(ex.Message);
}
```

---

## Monetization Issues

### Ads Not Loading

**Symptom:** `IsRewardedAdReady()` always `false`

**Debug:**
```csharp
IVXAdsManager.EnableDebugLogging(true);

IVXAdsManager.OnRewardedAdFailed += (error) =>
{
    Debug.LogError($"Ad load failed: {error}");
};
```

**Common causes:**
1. Test mode not enabled in development
2. Ad unit IDs incorrect
3. Ad provider not properly initialized
4. Device has ad blocker

---

### Purchase Failed

**Error:**
```
IAPException: Purchase could not be completed
```

**Solutions:**
1. Check product IDs match store configuration
2. Verify store account is set up correctly
3. For testing, use test accounts
4. Check internet connection

```csharp
IVXIAPManager.OnPurchaseFailed += (error) =>
{
    switch (error)
    {
        case "PurchaseCancelled":
            // User cancelled - no action needed
            break;
        case "ProductNotAvailable":
            ShowError("Product temporarily unavailable");
            break;
        case "NetworkError":
            ShowError("Please check your connection");
            break;
        default:
            ShowError($"Purchase failed: {error}");
            break;
    }
};
```

---

## Performance Issues

### High Memory Usage

**Symptom:** App crashes on low-end devices

**Solutions:**
1. Disable unused modules:
   ```csharp
   IntelliVerseXSDK.DisableModule(IVXModule.Quiz);
   IntelliVerseXSDK.DisableModule(IVXModule.MoreOfUs);
   ```

2. Reduce cache sizes:
   ```csharp
   IVXConfig.MaxCachedLeaderboards = 3;
   IVXConfig.MaxCachedFriends = 50;
   ```

3. Clear caches periodically:
   ```csharp
   IVXCacheManager.ClearAll();
   ```

---

### Frame Rate Drops

**Symptom:** Stuttering when SDK operations occur

**Cause:** Synchronous operations blocking main thread

**Solution:** Use async properly:
```csharp
// ❌ Blocking
var data = IVXSomeManager.GetData().Result;

// ✅ Non-blocking
var data = await IVXSomeManager.GetDataAsync();
```

---

## Data Issues

### Data Not Persisting

**Symptom:** Data lost after app restart

**Checklist:**
1. Using correct storage API?
2. Saving before app close?
3. Storage permissions granted?

**Solution:**
```csharp
// Ensure save completes before exit
void OnApplicationPause(bool paused)
{
    if (paused)
    {
        // Save critical data
        IVXSecureStorage.SetObject("game_state", currentState);
    }
}
```

---

### Cloud Sync Conflicts

**Symptom:** Data different between devices

**Solution:**
```csharp
// Handle conflicts
IVXStorageManager.OnConflict += (local, cloud) =>
{
    // Choose resolution strategy
    if (local.Timestamp > cloud.Timestamp)
    {
        IVXStorageManager.ResolveConflict(local);
    }
    else
    {
        IVXStorageManager.ResolveConflict(cloud);
    }
};
```

---

## Debugging Tips

### Enable Verbose Logging

```csharp
// Maximum logging
IVXLogger.SetLevel(LogLevel.Verbose);

// Or enable debug mode
IntelliVerseXSDK.EnableDebugMode(true);
```

### Network Inspection

```csharp
// Log all network calls
IVXNakamaManager.EnableVerboseLogging(true);
```

### Check Runtime State

```csharp
// Print diagnostic info
Debug.Log($"SDK Initialized: {IntelliVerseXSDK.IsInitialized}");
Debug.Log($"Connected: {IVXNakamaManager.IsConnected}");
Debug.Log($"Authenticated: {IVXAuthService.IsAuthenticated}");
Debug.Log($"User ID: {IVXAuthService.CurrentUser?.Id}");
```

---

## Still Stuck?

1. Run SDK diagnostics: `IntelliVerse-X → Diagnostics → Run Check`
2. Check [GitHub Issues](https://github.com/AhamedAzmi/IntelliVerseX/issues)
3. Post on [GitHub Discussions](https://github.com/AhamedAzmi/IntelliVerseX/discussions)
