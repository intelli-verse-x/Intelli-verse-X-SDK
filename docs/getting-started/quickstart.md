# Quick Start

Get IntelliVerseX SDK running in your project in under 5 minutes.

---

## Prerequisites

- [x] SDK installed via [Installation Guide](installation.md)
- [x] Unity 2023.3+ or Unity 6

---

## Step 1: Run Project Setup

After installing the SDK, configure your project:

1. Open **IntelliVerseX > Project Setup & Validation** from the menu bar
2. Click **Run Validation** to check your project configuration
3. Click **Apply All Required Settings** to automatically fix any issues

This sets up:

- Required Tags (`Player`, `Enemy`, etc.)
- Required Layers (`UI`, `Ground`, etc.)
- Scripting Define Symbols (`INTELLIVERSEX_SDK`)

!!! success "Validation Complete"
    You should see a green checkmark indicating all settings are configured.

---

## Step 2: Create Your Initialization Script

Create a new C# script called `GameInitializer.cs`:

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

/// <summary>
/// Initialize IntelliVerseX SDK at game startup.
/// Attach this to a GameObject in your first scene.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableDebugLogs = true;

    private void Awake()
    {
        // Configure logging
        IVXLogger.EnableDebugLogs = enableDebugLogs;
    }

    private void Start()
    {
        // Initialize device identity (generates unique device ID)
        IntelliVerseXUserIdentity.InitializeDevice();
        
        IVXLogger.Log("IntelliVerseX SDK initialized successfully!");
        IVXLogger.Log($"Device ID: {IntelliVerseXUserIdentity.DeviceId}");
    }
}
```

---

## Step 3: Add to Scene

1. Create an empty GameObject in your first/main scene
2. Name it `GameInitializer`
3. Attach the `GameInitializer.cs` script
4. Mark it with `DontDestroyOnLoad` (optional, for persistence)

---

## Step 4: Run and Verify

1. Press **Play** in the Unity Editor
2. Check the Console for these log messages:

```
[IVX] IntelliVerseX SDK initialized successfully!
[IVX] Device ID: abc123-def456-...
```

!!! success "SDK Ready"
    If you see these logs, the SDK is working correctly!

---

## Step 5: (Optional) Connect to Backend

If you're using backend features (leaderboards, wallets, friends), add Nakama integration:

### Create a Nakama Manager

```csharp
using UnityEngine;
using IntelliVerseX.Backend;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

/// <summary>
/// Example Nakama manager for your game.
/// Extends IVXNakamaManager to inherit all core functionality.
/// </summary>
public class MyGameNakamaManager : IVXNakamaManager
{
    public static MyGameNakamaManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        base.Awake();
    }

    protected override string GetLogPrefix() => "[MyGame]";
    
    protected override string GetConfigResourcePath() => "IntelliVerseX/MyGameConfig";
}
```

### Update Your Initializer

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private MyGameNakamaManager nakamaManager;
    
    private async void Start()
    {
        // Initialize device identity
        IntelliVerseXUserIdentity.InitializeDevice();
        IVXLogger.Log("Device initialized");
        
        // Connect to backend
        if (nakamaManager != null)
        {
            bool success = await nakamaManager.InitializeAsync();
            
            if (success)
            {
                IVXLogger.Log("Connected to backend!");
                
                // Sync identity with backend
                IntelliVerseXUserIdentity.SyncFromUserSessionManager();
            }
            else
            {
                IVXLogger.LogError("Failed to connect to backend");
            }
        }
    }
}
```

---

## Expected Logs

When running with backend connection:

```
[IVX] Device initialized: abc123-def456-...
[MyGame] Initializing Nakama client...
[MyGame] Config loaded: mygame @ https://nakama-rest.intelli-verse-x.ai:443
[MyGame] Authenticating with device...
[MyGame] Authentication successful: user_xyz
[IVX] Connected to backend!
```

---

## Minimal Working Example

Here's a complete minimal setup:

```csharp title="MinimalSetup.cs"
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class MinimalSetup : MonoBehaviour
{
    void Start()
    {
        // That's it! SDK is ready to use.
        IntelliVerseXUserIdentity.InitializeDevice();
        
        // Log some info
        Debug.Log($"[MyGame] Device: {IntelliVerseXUserIdentity.DeviceId}");
        Debug.Log($"[MyGame] Game ID: {IntelliVerseXUserIdentity.GameId}");
    }
}
```

---

## Next Steps

Now that the SDK is initialized, explore these features:

<div class="grid cards" markdown>

-   :material-account-key:{ .lg .middle } __Authentication__

    ---
    
    Add email, social, and guest login.
    
    [:octicons-arrow-right-24: Identity Module](../modules/identity.md)

-   :material-trophy:{ .lg .middle } __Leaderboards__

    ---
    
    Add global rankings to your game.
    
    [:octicons-arrow-right-24: Leaderboard Module](../modules/leaderboards.md)

-   :material-cash:{ .lg .middle } __Monetization__

    ---
    
    Integrate ads and in-app purchases.
    
    [:octicons-arrow-right-24: Monetization Module](../modules/monetization.md)

-   :material-translate:{ .lg .middle } __Localization__

    ---
    
    Support multiple languages.
    
    [:octicons-arrow-right-24: Localization Module](../modules/localization.md)

</div>

---

## Troubleshooting

### Logs not appearing

1. Check `IVXLogger.EnableDebugLogs = true`
2. Verify the script is attached to an active GameObject
3. Check Console filter settings

### Device ID is null

1. Ensure `InitializeDevice()` is called before accessing `DeviceId`
2. Check that the script runs in `Start()` or later (not `Awake`)

### Backend connection fails

1. Check your internet connection
2. Verify your Game Configuration asset exists
3. Check the Nakama server is accessible

[:octicons-arrow-right-24: Full Troubleshooting Guide](../troubleshooting/index.md)
