# API Reference

Complete API documentation for the IntelliVerseX SDK.

---

## Namespaces

| Namespace | Description |
|-----------|-------------|
| `IntelliVerseX.Core` | Core utilities, logging, configuration |
| `IntelliVerseX.Identity` | User identity and session management |
| `IntelliVerseX.Backend` | Nakama integration and networking |
| `IntelliVerseX.Auth` | Authentication services |
| `IntelliVerseX.Social` | Friends, sharing, social features |
| `IntelliVerseX.Monetization` | IAP, ads, offerwalls |
| `IntelliVerseX.Analytics` | Event tracking and analytics |
| `IntelliVerseX.Localization` | Multi-language support |
| `IntelliVerseX.Storage` | Local and cloud storage |
| `IntelliVerseX.Leaderboard` | Leaderboards and rankings |
| `IntelliVerseX.Quiz` | Daily and weekly quizzes |
| `IntelliVerseX.UI` | UI components and utilities |
| `IntelliVerseX.MoreOfUs` | Cross-game promotion |

---

## Quick Reference

### Initialization

```csharp
// Initialize SDK
await IntelliVerseXSDK.InitializeAsync();

// Check initialization
if (IntelliVerseXSDK.IsInitialized) { /* ready */ }
```

### Authentication

```csharp
// Login
await IVXAuthService.LoginAsync(email, password);
await IVXAuthService.LoginWithDeviceIdAsync();
await IVXAuthService.LoginWithGoogleAsync(token);

// Logout
await IVXAuthService.LogoutAsync();

// Check auth state
if (IVXAuthService.IsAuthenticated) { /* logged in */ }
```

### Backend

```csharp
// Connect
await IVXNakamaManager.ConnectAsync();

// RPC calls
var result = await IVXNakamaManager.RpcAsync<T>("rpc_name", payload);

// Realtime
await IVXNakamaManager.JoinMatchAsync(matchId);
```

### Storage

```csharp
// Local storage
IVXSecureStorage.SetObject("key", data);
var data = IVXSecureStorage.GetObject<T>("key");

// Cloud storage
await IVXCloudStorage.SetAsync("collection", "key", data);
var data = await IVXCloudStorage.GetAsync<T>("collection", "key");
```

### Leaderboards

```csharp
// Submit score
await IVXLeaderboardManager.SubmitScoreAsync("leaderboard_id", score);

// Get scores
var entries = await IVXLeaderboardManager.GetTopScoresAsync("leaderboard_id", limit: 50);
```

### Analytics

```csharp
// Log event
IVXAnalyticsManager.LogEvent("event_name", parameters);

// Set user property
IVXAnalyticsManager.SetUserProperty("property", "value");
```

---

## Assembly Reference

### Runtime Assemblies

| Assembly | Contains |
|----------|----------|
| `IntelliVerseX.Core` | Core, Logger, Config, Utils |
| `IntelliVerseX.Identity` | UserIdentity, Session |
| `IntelliVerseX.Backend` | Nakama, Wallet, Backend services |
| `IntelliVerseX.Auth` | Auth UI panels |
| `IntelliVerseX.Social` | Friends, Share, Rate |
| `IntelliVerseX.Monetization` | IAP, Ads, Offerwall |
| `IntelliVerseX.Analytics` | Analytics providers |
| `IntelliVerseX.Localization` | Language, Localized components |
| `IntelliVerseX.Storage` | SecureStorage, CloudStorage |
| `IntelliVerseX.Leaderboard` | Leaderboard services |
| `IntelliVerseX.Quiz` | Quiz logic |
| `IntelliVerseX.QuizUI` | Quiz UI components |
| `IntelliVerseX.UI` | UI utilities, Popup, Toast |
| `IntelliVerseX.MoreOfUs` | Cross-promotion |
| `IntelliVerseX.Networking` | Network utilities |
| `IntelliVerseX.V2` | Version 2 compatibility |

### Editor Assemblies

| Assembly | Contains |
|----------|----------|
| `IntelliVerseX.Editor` | Setup wizard, inspectors, tools |

---

## Core Classes

### IntelliVerseXSDK

Main SDK entry point.

```csharp
public static class IntelliVerseXSDK
{
    // Initialization
    static Task InitializeAsync();
    static Task InitializeAsync(IntelliVerseXConfig config);
    
    // State
    static bool IsInitialized { get; }
    static IntelliVerseXConfig Config { get; }
    
    // Modules
    static void EnableModule(IVXModule module);
    static void DisableModule(IVXModule module);
    static bool IsModuleEnabled(IVXModule module);
}
```

### IVXLogger

Logging utility.

```csharp
public static class IVXLogger
{
    static void Log(string message);
    static void Log(string category, string message);
    static void LogWarning(string message);
    static void LogError(string message);
    static void SetLevel(LogLevel level);
}
```

### IVXSafeSingleton<T>

Thread-safe singleton base class.

```csharp
public abstract class IVXSafeSingleton<T> : MonoBehaviour 
    where T : IVXSafeSingleton<T>
{
    static T Instance { get; }
    static bool IsInitialized { get; }
}
```

---

## Events Reference

### Authentication Events

```csharp
IVXAuthService.OnLoggedIn += (userId) => { };
IVXAuthService.OnLoggedOut += () => { };
IVXAuthService.OnAuthFailed += (error) => { };
```

### Connection Events

```csharp
IVXNakamaManager.OnConnected += () => { };
IVXNakamaManager.OnDisconnected += () => { };
IVXNakamaManager.OnReconnecting += () => { };
IVXNakamaManager.OnReconnected += () => { };
```

### Social Events

```csharp
IVXFriendsManager.OnFriendRequestReceived += (request) => { };
IVXFriendsManager.OnFriendAdded += (friend) => { };
IVXFriendsManager.OnFriendRemoved += (userId) => { };
IVXFriendsManager.OnPresenceChanged += (userId, status) => { };
```

### Monetization Events

```csharp
IVXIAPManager.OnPurchaseComplete += (result) => { };
IVXIAPManager.OnPurchaseFailed += (error) => { };
IVXAdsManager.OnRewardedAdCompleted += (reward) => { };
```

---

## Types Reference

### Enums

```csharp
public enum LogLevel { Verbose, Debug, Info, Warning, Error }
public enum FriendState { Friend, Pending, Blocked }
public enum IVXProductType { Consumable, NonConsumable, Subscription }
public enum ToastType { Default, Success, Warning, Error, Info }
```

### Data Classes

```csharp
public class IVXFriend { /* See Social module */ }
public class IVXProduct { /* See Monetization module */ }
public class IVXLeaderboardEntry { /* See Leaderboard module */ }
public class IVXQuizQuestion { /* See Quiz module */ }
```

---

## Module-Specific API

For detailed API documentation per module:

- [Core API](core.md)
- [Identity API](identity.md)
- [Backend API](backend.md)
- [Social API](social.md)
- [Monetization API](monetization.md)
- [Analytics API](analytics.md)
- [Localization API](localization.md)
- [Storage API](storage.md)
- [Leaderboard API](leaderboards.md)
- [Quiz API](quiz.md)
- [UI API](ui.md)
