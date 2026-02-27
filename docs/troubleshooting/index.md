# Troubleshooting

Common issues and solutions for the IntelliVerseX SDK.

---

## Quick Diagnostics

Before troubleshooting, run the SDK diagnostic:

```csharp
// In Unity Editor: IntelliVerse-X → Diagnostics → Run Check
// Or in code:
var report = IVXDiagnostics.RunCheck();
Debug.Log(report);
```

---

## Common Issues

<div class="grid cards" markdown>

-   :material-hammer-wrench:{ .lg .middle } **Build Errors**

    ---

    Compilation and build-time issues

    [:octicons-arrow-right-24: Build Errors](build-errors.md)

-   :material-bug:{ .lg .middle } **Runtime Issues**

    ---

    Errors during gameplay

    [:octicons-arrow-right-24: Runtime Issues](runtime-issues.md)

-   :material-cellphone:{ .lg .middle } **Platform-Specific**

    ---

    Android, iOS, WebGL specific problems

    [:octicons-arrow-right-24: Platform Issues](platform-specific.md)

-   :material-frequently-asked-questions:{ .lg .middle } **FAQ**

    ---

    Frequently asked questions

    [:octicons-arrow-right-24: FAQ](faq.md)

</div>

---

## Quick Fixes

### SDK Not Initializing

**Symptom:** `IntelliVerseXSDK.IsInitialized` is always `false`

**Solution:**
```csharp
// 1. Check config exists
var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseXConfig");
if (config == null)
{
    Debug.LogError("Config not found! Create via Assets → Create → IntelliVerse-X → Config");
}

// 2. Initialize manually
await IntelliVerseXSDK.InitializeAsync(config);

// 3. Check for errors in console
```

---

### Backend Connection Failed

**Symptom:** `IVXNakamaManager.IsConnected` is `false`, connection errors in console

**Checklist:**
1. ✅ Backend URL is correct
2. ✅ Server is running and accessible
3. ✅ SSL certificate is valid (if using HTTPS)
4. ✅ Device has internet connection
5. ✅ Firewall/proxy not blocking connection

**Solution:**
```csharp
// Test connection
try
{
    await IVXNakamaManager.ConnectAsync();
    Debug.Log("Connected successfully!");
}
catch (Exception ex)
{
    Debug.LogError($"Connection failed: {ex.Message}");
    
    // Check specific error
    if (ex is HttpRequestException)
    {
        Debug.Log("Network issue - check internet connection");
    }
    else if (ex.Message.Contains("certificate"))
    {
        Debug.Log("SSL certificate issue");
    }
}
```

---

### Authentication Failing

**Symptom:** Login fails with "Unauthorized" or similar error

**Common Causes:**

| Error | Cause | Solution |
|-------|-------|----------|
| `Unauthorized` | Invalid credentials | Check email/password |
| `User not found` | Account doesn't exist | Register first |
| `Token expired` | Session expired | Re-authenticate |
| `Invalid server key` | Wrong server key in config | Update config |

**Solution:**
```csharp
try
{
    await IVXAuthService.LoginAsync(email, password);
}
catch (AuthException ex) when (ex.Code == "Unauthorized")
{
    ShowError("Invalid email or password");
}
catch (AuthException ex) when (ex.Code == "UserNotFound")
{
    ShowRegisterPrompt();
}
```

---

### Ads Not Showing

**Symptom:** `IVXAdsManager.IsRewardedAdReady()` always returns `false`

**Checklist:**
1. ✅ Ad SDK initialized
2. ✅ Ad unit IDs are correct
3. ✅ Test mode enabled for development
4. ✅ Device is not ad-blocked
5. ✅ Ad provider dashboard shows requests

**Solution:**
```csharp
// Enable verbose logging
IVXAdsManager.EnableDebugLogging(true);

// Check initialization
IVXAdsManager.OnInitialized += (success) =>
{
    Debug.Log($"Ads initialized: {success}");
};

// Check ad load events
IVXAdsManager.OnRewardedAdLoaded += () =>
{
    Debug.Log("Rewarded ad loaded");
};

IVXAdsManager.OnRewardedAdFailed += (error) =>
{
    Debug.LogError($"Rewarded ad failed: {error}");
};
```

---

### Data Not Saving

**Symptom:** Saved data lost after restart

**Checklist:**
1. ✅ Using correct storage API
2. ✅ Calling save before app close
3. ✅ Storage permissions granted (Android)
4. ✅ Not clearing app data

**Solution:**
```csharp
// Ensure data persists
IVXSecureStorage.SetObject("game_save", saveData);

// Verify it was saved
var loaded = IVXSecureStorage.GetObject<SaveData>("game_save");
if (loaded == null)
{
    Debug.LogError("Save failed!");
}

// For cloud sync, ensure connected
if (IVXNakamaManager.IsConnected)
{
    await IVXCloudStorage.SyncAsync();
}
```

---

### Leaderboard Empty

**Symptom:** Leaderboard returns empty list

**Checklist:**
1. ✅ Backend connection active
2. ✅ Leaderboard ID matches server config
3. ✅ Scores have been submitted
4. ✅ Leaderboard hasn't reset

**Solution:**
```csharp
// Check leaderboard exists
var entries = await IVXLeaderboardManager.GetTopScoresAsync("weekly_highscore");

if (entries == null || entries.Length == 0)
{
    // Check if leaderboard exists on server
    Debug.Log("No entries - leaderboard may be empty or not exist");
    
    // Submit a test score
    await IVXLeaderboardManager.SubmitScoreAsync("weekly_highscore", 100);
}
```

---

## Error Codes Reference

| Code | Meaning | Action |
|------|---------|--------|
| `IVX_001` | SDK not initialized | Call `Initialize()` first |
| `IVX_002` | Config not found | Create config asset |
| `IVX_003` | Backend unreachable | Check network/URL |
| `IVX_004` | Authentication failed | Check credentials |
| `IVX_005` | Session expired | Re-authenticate |
| `IVX_006` | Permission denied | Check user permissions |
| `IVX_007` | Resource not found | Check resource exists |
| `IVX_008` | Rate limited | Wait and retry |
| `IVX_009` | Invalid parameter | Check input values |
| `IVX_010` | Platform not supported | Check platform |

---

## Debug Mode

Enable comprehensive debugging:

```csharp
// Enable all debug features
IntelliVerseXSDK.EnableDebugMode(true);

// Or specific features
IVXLogger.SetLevel(LogLevel.Verbose);
IVXAdsManager.EnableDebugLogging(true);
IVXNakamaManager.EnableVerboseLogging(true);
```

---

## Getting Help

If you can't resolve an issue:

1. **Check Documentation** - Search this documentation
2. **Review Samples** - Check sample scenes for working examples
3. **GitHub Issues** - Search existing issues or create new one
4. **Support** - Contact support@intelli-verse-x.ai

**When reporting issues, include:**
- Unity version
- SDK version
- Platform (Android/iOS/WebGL/etc.)
- Error message (full text)
- Steps to reproduce
- Diagnostic report output
