# Migration Guide

Upgrade from previous versions of #IntelliVerseX SDK.

---

## Migration Overview

| From Version | To Version | Difficulty |
|--------------|------------|------------|
| 4.x → 5.0 | 5.0 | Medium |
| 3.x → 5.0 | 5.0 | Major |
| 2.x → 5.0 | 5.0 | Significant rewrite |

---

## Migrating from 4.x to 5.0

### Breaking Changes

1. **Namespace changes**
2. **API simplifications**
3. **Removed deprecated features**
4. **Updated event signatures**

---

### Step 1: Update Package

1. Remove old package folder
2. Import new package via UPM
3. Let Unity reimport

---

### Step 2: Update Namespaces

```csharp
// Old (4.x)
using IntelliVerseX.SDK;
using IntelliVerseX.SDK.Core;
using IntelliVerseX.SDK.Auth;

// New (5.0)
using IntelliVerseX.Core;
using IntelliVerseX.Identity;
using IntelliVerseX.Backend;
```

**Find and replace in your project:**

| Old | New |
|-----|-----|
| `IntelliVerseX.SDK.Core` | `IntelliVerseX.Core` |
| `IntelliVerseX.SDK.Auth` | `IntelliVerseX.Identity` |
| `IntelliVerseX.SDK.Backend` | `IntelliVerseX.Backend` |
| `IntelliVerseX.SDK.Social` | `IntelliVerseX.Social` |
| `IntelliVerseX.SDK.Monetization` | `IntelliVerseX.Monetization` |

---

### Step 3: Update Initialization

```csharp
// Old (4.x)
IntelliVerseXSDK.Initialize(config, () =>
{
    Debug.Log("Initialized");
});

// New (5.0)
await IntelliVerseXSDK.Instance.InitializeAsync();
Debug.Log("Initialized");

// Or with callback style:
IntelliVerseXSDK.Instance.InitializeAsync().ContinueWith(t =>
{
    Debug.Log("Initialized");
}, TaskScheduler.FromCurrentSynchronizationContext());
```

---

### Step 4: Update Authentication

```csharp
// Old (4.x)
AuthManager.Login(email, password, (user) =>
{
    Debug.Log($"Logged in: {user.id}");
}, (error) =>
{
    Debug.LogError(error);
});

// New (5.0)
try
{
    var identity = await IntelliVerseXUserIdentity.Instance
        .LoginWithEmailAsync(email, password);
    Debug.Log($"Logged in: {identity.UserId}");
}
catch (Exception ex)
{
    Debug.LogError(ex.Message);
}
```

---

### Step 5: Update Event Subscriptions

```csharp
// Old (4.x)
IntelliVerseXSDK.OnInitialized += OnSDKInitialized;
AuthManager.OnLoginSuccess += OnLogin;
AuthManager.OnLoginError += OnLoginError;

// New (5.0)
IntelliVerseXSDK.OnInitialized += OnSDKInitialized; // Same
IntelliVerseXUserIdentity.OnAuthStateChanged += OnAuthStateChanged;
// Single event handles both success and failure states
```

---

### Step 6: Update Manager Access

```csharp
// Old (4.x)
var adsManager = IntelliVerseXSDK.Instance.GetManager<AdsManager>();
adsManager.ShowInterstitial();

// New (5.0)
IVXAdsManager.Instance.ShowInterstitial();
// Or: IntelliVerseXSDK.Instance.GetModule<IVXAdsManager>().ShowInterstitial();
```

---

### Step 7: Update Storage Calls

```csharp
// Old (4.x)
StorageManager.Save("key", data, encrypted: true);
var data = StorageManager.Load<PlayerData>("key");

// New (5.0)
IVXSecureStorage.SetObject("key", data);
var data = IVXSecureStorage.GetObject<PlayerData>("key");
```

---

### Step 8: Config Asset Migration

The config structure has changed:

```csharp
// 1. Create new IntelliVerseXConfig via IntelliVerseX > Game Config
// 2. Copy values from old config
// 3. Delete old config asset
// 4. Assign new config to SDK prefab
```

---

## Migrating from 3.x to 5.0

!!! warning "Major Migration"
    This is a significant migration requiring substantial changes.

### Key Differences

| Area | 3.x | 5.0 |
|------|-----|-----|
| Architecture | Monolithic | Modular |
| Async | Callbacks | async/await |
| Events | Delegates | UnityEvents + C# events |
| Managers | Static | Singleton pattern |
| Config | ScriptableObject | Enhanced ScriptableObject |

### Migration Steps

1. **Backup your project**
2. **Remove old SDK completely**
3. **Import SDK 5.0 fresh**
4. **Rewrite integration code** (follow quickstart guide)
5. **Test thoroughly**

---

## Migrating from 2.x to 5.0

!!! danger "Complete Rewrite Required"
    SDK 2.x used a completely different architecture. We recommend following the [Quick Start Guide](../getting-started/quickstart.md) as if starting fresh.

---

## Deprecated Features Removed in 5.0

The following were deprecated in 4.x and removed in 5.0:

| Removed | Replacement |
|---------|-------------|
| `AuthManager.LoginCallback` | `LoginWithEmailAsync` |
| `StorageManager.Save/Load` | `IVXSecureStorage.Set/Get` |
| `NetworkManager` | `IVXNakamaManager` |
| `AdManager` | `IVXAdsManager` |
| `UserManager.CurrentUser` | `IntelliVerseXUserIdentity.CurrentUser` |

---

## Assembly Definition Changes

| Old Assembly | New Assembly |
|--------------|--------------|
| `IntelliVerseX.SDK` | `IntelliVerseX.Core` |
| `IntelliVerseX.SDK.Auth` | `IntelliVerseX.Auth` |
| `IntelliVerseX.SDK.UI` | `IntelliVerseX.UI` |

Update your assembly definition references accordingly.

---

## Common Migration Issues

### Issue: Cannot find type 'AuthManager'

**Solution:** Replace with `IntelliVerseXUserIdentity`

```csharp
// Old
AuthManager.Login(...)

// New
IntelliVerseXUserIdentity.Instance.LoginWithEmailAsync(...)
```

---

### Issue: Callback methods no longer compile

**Solution:** Convert to async/await or use continuation

```csharp
// Old
DoSomething(onSuccess, onError);

// New (async)
try
{
    var result = await DoSomethingAsync();
    OnSuccess(result);
}
catch (Exception ex)
{
    OnError(ex);
}

// New (callback style)
DoSomethingAsync().ContinueWith(task =>
{
    if (task.Exception != null)
        OnError(task.Exception);
    else
        OnSuccess(task.Result);
}, TaskScheduler.FromCurrentSynchronizationContext());
```

---

### Issue: Config asset is invalid

**Solution:** Create fresh config

1. Delete old config asset
2. Go to IntelliVerseX > Game Config
3. Configure settings
4. Assign to SDK prefab

---

### Issue: Events not firing

**Solution:** Update event subscription

```csharp
// Old
IntelliVerseXSDK.OnUserLogin += OnLogin;
IntelliVerseXSDK.OnUserLogout += OnLogout;

// New
IntelliVerseXUserIdentity.OnAuthStateChanged += (state) =>
{
    if (state == AuthState.Authenticated)
        OnLogin(IntelliVerseXUserIdentity.Instance.CurrentUser);
    else if (state == AuthState.Unauthenticated)
        OnLogout();
};
```

---

## Testing Your Migration

### Checklist

- [ ] SDK initializes without errors
- [ ] User can authenticate
- [ ] User data persists correctly
- [ ] Ads show properly
- [ ] IAP products load
- [ ] Leaderboards work
- [ ] Analytics events fire
- [ ] No warnings in console

### Test Script

```csharp
public class MigrationTest : MonoBehaviour
{
    async void Start()
    {
        // Test initialization
        Debug.Log("Testing initialization...");
        await IntelliVerseXSDK.Instance.InitializeAsync();
        Debug.Log("✓ Initialized");
        
        // Test auth
        Debug.Log("Testing authentication...");
        await IntelliVerseXUserIdentity.Instance.AuthenticateGuestAsync();
        Debug.Log($"✓ Authenticated: {IntelliVerseXUserIdentity.Instance.CurrentUser.UserId}");
        
        // Test storage
        Debug.Log("Testing storage...");
        IVXSecureStorage.SetString("migration_test", "success");
        var value = IVXSecureStorage.GetString("migration_test");
        Debug.Assert(value == "success", "Storage test failed");
        Debug.Log("✓ Storage working");
        
        // Add more tests as needed
        Debug.Log("Migration tests complete!");
    }
}
```

---

## Getting Help

If you encounter issues during migration:

1. Check [Troubleshooting](../troubleshooting/index.md) docs
2. Search [GitHub Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues)
3. Post on [GitHub Discussions](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/discussions)
4. Contact support: sdk@intelli-verse-x.ai
