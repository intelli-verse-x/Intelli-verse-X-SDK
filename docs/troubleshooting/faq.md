# Frequently Asked Questions

Common questions about the IntelliVerseX SDK.

---

## General

### What Unity versions are supported?

The SDK supports:
- **Minimum:** Unity 2021.3 LTS
- **Recommended:** Unity 2023.3 LTS or Unity 6

### Which platforms does the SDK support?

| Platform | Support Level |
|----------|---------------|
| Android | ✅ Full |
| iOS | ✅ Full |
| WebGL | ⚠️ Partial |
| Windows | ✅ Full |
| macOS | ✅ Full |
| Linux | 🔄 Experimental |

WebGL has limitations with some native features (IAP, certain ads).

### Is the SDK free to use?

Yes, the SDK is free and open-source under the MIT license. Backend services may have associated costs depending on your usage tier.

---

## Setup & Installation

### How do I install the SDK?

**Recommended: Git URL (via Package Manager)**
```
https://github.com/AhamedAzmi/IntelliVerseX.git?path=/Assets/_IntelliVerseXSDK
```

See [Installation Guide](../getting-started/installation.md) for all methods.

### Where should I place the configuration file?

Configuration files should be in the `Resources` folder:
```
Assets/Resources/IntelliVerseXConfig.asset
```

This allows automatic loading with `Resources.Load<>()`.

### How do I update the SDK?

**Via Package Manager:**
1. Open Window → Package Manager
2. Find IntelliVerseX SDK
3. Click "Update" button

**Via Git:**
```bash
# Pull latest changes
git pull origin main
```

---

## Backend & Networking

### Do I need my own Nakama server?

For development, you can use the shared development server. For production, you'll need your own Nakama instance.

**Development:** `nakama-rest.intelli-verse-x.ai:443`

**Production:** Deploy your own via [Heroic Labs](https://heroiclabs.com/) or self-host.

### How do I handle offline mode?

The SDK includes offline support:

```csharp
// Check connection
if (!IVXNakamaManager.IsConnected)
{
    // Use cached data
    var cachedData = IVXSecureStorage.GetObject<GameData>("cached_data");
}

// Auto-reconnect is enabled by default
IVXNakamaManager.OnReconnected += SyncOfflineData;
```

### What happens if the server is down?

The SDK handles server outages gracefully:
- Local data remains accessible
- Actions are queued for later sync
- Auto-reconnect attempts are made
- Fallback to cached data where possible

---

## Authentication

### What authentication methods are available?

- Email/Password
- Device ID (anonymous/guest)
- Google Sign-In
- Apple Sign-In  
- Facebook Login
- Custom authentication

### How do I implement "guest" accounts?

```csharp
// Device-based authentication (guest)
await IVXAuthService.LoginWithDeviceIdAsync();

// Later, link to full account
await IVXAuthService.LinkEmailAsync(email, password);
```

### Can users link multiple auth providers?

Yes, users can link multiple authentication methods:

```csharp
// User logged in with Google, link email
await IVXAuthService.LinkEmailAsync(email, password);

// Now they can sign in with either method
```

---

## IAP & Monetization

### How do I test in-app purchases?

**Android:** Use test accounts or license testing in Play Console

**iOS:** Use sandbox tester accounts in App Store Connect

**Code:**
```csharp
#if UNITY_EDITOR
// Editor simulates successful purchases
#endif
```

### Why are ads not showing in the Editor?

Ads don't display in the Unity Editor. Test on device or use:

```csharp
IVXAdsManager.EnableEditorSimulation(true);
// Shows debug UI instead of real ads
```

### How do I integrate my own ad network?

Implement the `IIVXAdProvider` interface:

```csharp
public class MyAdProvider : IIVXAdProvider
{
    public bool IsRewardedReady() { /* ... */ }
    public void ShowRewarded(Action onComplete, Action onFailed) { /* ... */ }
    // ... other methods
}

IVXAdsManager.AddProvider(new MyAdProvider());
```

---

## Data & Storage

### Is player data encrypted?

Yes, `IVXSecureStorage` encrypts data by default using AES-256.

### How do I backup/restore player progress?

```csharp
// Backup (save to cloud)
await IVXCloudStorage.SetAsync("saves", "current", playerData);

// Restore (load from cloud)
var data = await IVXCloudStorage.GetAsync<PlayerData>("saves", "current");
```

### Where is data stored locally?

| Platform | Location |
|----------|----------|
| Android | Application.persistentDataPath |
| iOS | Application.persistentDataPath |
| WebGL | IndexedDB |
| Desktop | Application.persistentDataPath |

---

## Leaderboards & Social

### Can I create custom leaderboards?

Leaderboards are defined on the Nakama server. Contact the backend team or configure them in your server modules.

### How many friends can a user have?

The default limit is 1000 friends. This is configurable on the server.

### Are leaderboards real-time?

Leaderboards update in near real-time. Scores are reflected immediately after submission.

---

## Performance

### Does the SDK impact performance?

The SDK is optimized for mobile:
- Zero allocations in update loops
- Pooled network connections
- Cached data access
- Async operations to prevent frame drops

### How can I reduce SDK memory usage?

```csharp
// Disable unused modules
IntelliVerseXSDK.DisableModule(IVXModule.Analytics);
IntelliVerseXSDK.DisableModule(IVXModule.Quiz);

// Reduce cache sizes
IVXConfig.MaxCachedLeaderboards = 3;
IVXConfig.MaxCachedFriends = 50;
```

### Does the SDK work with IL2CPP?

Yes, the SDK is fully compatible with IL2CPP builds. Ensure link.xml preserves necessary types.

---

## Localization

### How do I add a new language?

1. Create JSON file in `Localization/Languages/xx.json`
2. Copy keys from `en.json`
3. Translate values
4. Add language to config

See [Localization Module](../modules/localization.md) for details.

### Does the SDK support RTL languages?

Yes, Arabic and Hebrew are supported with RTL text rendering.

---

## Troubleshooting

### How do I enable debug logging?

```csharp
IVXLogger.SetLevel(LogLevel.Verbose);
// or
IntelliVerseXSDK.EnableDebugMode(true);
```

### Where can I find error logs?

- **Unity Console:** All logs appear here
- **Device Logs:** Via `adb logcat` (Android) or Xcode (iOS)
- **Backend Logs:** In Nakama console

### The SDK crashes on startup - what do I check?

1. Config file exists and is valid
2. All required dependencies are imported
3. Assembly definitions are correct
4. No conflicting package versions

---

## Still Have Questions?

- 📖 Check the [full documentation](../index.md)
- 🐛 Report issues on [GitHub](https://github.com/AhamedAzmi/IntelliVerseX/issues)
- 📧 Contact support@intelli-verse-x.ai
