# Platform Module

Cross-platform utilities for adaptive rendering, deep linking, safe-area layout, and foldable-device detection.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Platform` |
| **Assembly** | `IntelliVerseX.Platform` |
| **Dependencies** | None |
| **Platforms** | Android, iOS, WebGL, Standalone |

The Platform module solves device-specific concerns that surface across all supported targets. Each class operates independently -- attach only the components your game needs.

| Class | Type | Purpose |
|---|---|---|
| `IVXPlatformOptimizer` | Singleton MonoBehaviour | FPS targeting, battery-aware throttling, dynamic resolution |
| `IVXDeepLinkManager` | Singleton MonoBehaviour | Incoming URI routing and event dispatch |
| `IVXEdgeToEdgeHelper` | Static utility | Safe-area queries and RectTransform anchoring |
| `IVXFoldableHelper` | Singleton MonoBehaviour | Form-factor detection, screen-change events |

---

## IVXPlatformOptimizer

Applies device-appropriate frame-rate targets and battery-aware rendering adjustments at runtime. When battery drops below 20 %, the optimizer automatically lowers the target frame rate and restores it when the device begins charging or recovers.

```csharp
public sealed class IVXPlatformOptimizer : MonoBehaviour
{
    // Read-only state
    public static bool IsLowPowerMode { get; }
    public static float BatteryLevel { get; }       // 0..1, returns 1 when unknown

    // Runtime overrides
    public static void SetTargetFrameRate(int fps);
    public static void SetRenderScale(float scale);  // 0.25..1.0
}
```

### Inspector fields

| Field | Default | Description |
|---|---|---|
| `_mobileTargetFps` | `60` | Target FPS on Android / iOS |
| `_desktopTargetFps` | `-1` | Target FPS on desktop (`-1` = platform default) |
| `_lowPowerTargetFps` | `30` | FPS cap when battery falls below threshold |
| `_enableBatteryThrottling` | `true` | Enable automatic low-power mode |
| `_batteryCheckIntervalSec` | `30` | Seconds between battery-level checks |

### Usage

```csharp
// The optimizer is a singleton -- add it to a persistent GameObject.
// On Awake it calls DontDestroyOnLoad and applies platform defaults.

// Override FPS at runtime (e.g. during a cutscene)
IVXPlatformOptimizer.SetTargetFrameRate(30);

// Lower render resolution on a weaker device
IVXPlatformOptimizer.SetRenderScale(0.75f);

// Query power state before launching a heavy effect
if (IVXPlatformOptimizer.IsLowPowerMode)
{
    // skip particle burst
}
```

### Behaviour summary

| Platform | Default FPS | Sleep Timeout |
|---|---|---|
| Android / iOS | 60 | `NeverSleep` |
| Desktop / WebGL | Platform default | Unchanged |

When `_enableBatteryThrottling` is active, the optimizer checks `SystemInfo.batteryLevel` on the configured interval and transitions between normal and low-power modes automatically.

---

## IVXDeepLinkManager

Processes incoming deep links, universal links, and app links. Subscribers receive both the raw URI and a parsed (path, query) pair.

```csharp
public sealed class IVXDeepLinkManager : MonoBehaviour
{
    // Events
    public static event Action<string> OnDeepLinkReceived;
    public static event Action<string, string> OnDeepLinkParsed; // (path, query)

    // State
    public static string LastDeepLink { get; }

    // Consume and clear the pending link
    public static string ConsumePendingDeepLink();
}
```

### Usage

```csharp
// Subscribe early (e.g. in a bootstrap scene)
IVXDeepLinkManager.OnDeepLinkParsed += HandleDeepLink;

private void HandleDeepLink(string path, string query)
{
    switch (path)
    {
        case "/invite":
            JoinRoom(query);
            break;
        case "/promo":
            ApplyPromoCode(query);
            break;
    }
}
```

```csharp
// Cold-start handling -- check for a link that arrived before listeners were ready
string pending = IVXDeepLinkManager.ConsumePendingDeepLink();
if (pending != null)
{
    ProcessLink(pending);
}
```

### How it works

1. On `Awake`, the manager subscribes to `Application.deepLinkActivated`.
2. If the app was cold-started via a URI, `Application.absoluteURL` is processed immediately.
3. The raw URI is stored in `LastDeepLink` and broadcast via `OnDeepLinkReceived`.
4. The URI is parsed into path and query segments and broadcast via `OnDeepLinkParsed`. If parsing fails, the full URI is passed as the path with an empty query.
5. Call `ConsumePendingDeepLink()` to retrieve and clear the stored link.

---

## IVXEdgeToEdgeHelper

Static helper for edge-to-edge / full-screen display on modern devices. Provides safe-area queries so UI elements avoid notches, rounded corners, and system gesture bars.

```csharp
public static class IVXEdgeToEdgeHelper
{
    // Queries
    public static Rect SafeArea { get; }             // Screen coordinates
    public static Rect SafeAreaNormalized { get; }    // 0..1 range
    public static bool HasNotch { get; }

    // Apply safe area to a RectTransform
    public static void ApplySafeArea(RectTransform target);
}
```

### Usage

```csharp
// Apply safe-area anchors to the root Canvas panel
[SerializeField] private RectTransform _safePanel;

private void Start()
{
    IVXEdgeToEdgeHelper.ApplySafeArea(_safePanel);
}
```

```csharp
// React to notch presence
if (IVXEdgeToEdgeHelper.HasNotch)
{
    // adjust header padding
}
```

```csharp
// Manual anchor math using normalised values
var norm = IVXEdgeToEdgeHelper.SafeAreaNormalized;
_panel.anchorMin = new Vector2(norm.x, norm.y);
_panel.anchorMax = new Vector2(norm.x + norm.width, norm.y + norm.height);
```

`ApplySafeArea` should be called once after layout or whenever the screen orientation changes. On devices without a notch or cutout, the safe area equals the full screen and anchors remain at `(0,0)` to `(1,1)`.

---

## IVXFoldableHelper

Detects foldable device states and screen-configuration changes. Games can subscribe to layout events to adapt UI for tablet, tabletop, or split-screen modes.

```csharp
public sealed class IVXFoldableHelper : MonoBehaviour
{
    // Enums
    public enum DeviceFormFactor { Phone, Tablet, Foldable, Desktop, Unknown }
    public enum FoldState { Unknown, Flat, HalfOpen, Folded }

    // Events
    public static event Action<int, int> OnScreenConfigChanged;   // (width, height)
    public static event Action<bool> OnLargeScreenChanged;        // true = tablet-sized

    // Properties
    public static float ScreenDiagonalInches { get; }
    public static bool IsLargeScreen { get; }                     // >= 6.5"
    public static DeviceFormFactor FormFactor { get; }
    public static float AspectRatio { get; }                      // always >= 1
}
```

### Usage

```csharp
// Adapt layout when the screen configuration changes (e.g. fold/unfold)
IVXFoldableHelper.OnScreenConfigChanged += (w, h) =>
{
    Debug.Log($"New resolution: {w}x{h}");
    RebuildLayout();
};

// React specifically when crossing the tablet threshold
IVXFoldableHelper.OnLargeScreenChanged += isLarge =>
{
    _sidePanel.SetActive(isLarge);
};
```

```csharp
// One-time form-factor check at startup
switch (IVXFoldableHelper.FormFactor)
{
    case IVXFoldableHelper.DeviceFormFactor.Phone:
        ApplyPhoneLayout();
        break;
    case IVXFoldableHelper.DeviceFormFactor.Tablet:
        ApplyTabletLayout();
        break;
    case IVXFoldableHelper.DeviceFormFactor.Desktop:
        ApplyDesktopLayout();
        break;
}
```

### Form-factor detection logic

| Platform | Diagonal >= 6.5" | Result |
|---|---|---|
| Android / iOS | Yes | `Tablet` |
| Android / iOS | No | `Phone` |
| Editor / Standalone | Any | `Desktop` |
| Other | Any | `Unknown` |

`OnScreenConfigChanged` fires every time the screen width or height changes (orientation flip, window resize, fold event). `OnLargeScreenChanged` fires only when the diagonal crosses the 6.5-inch threshold.

---

## Best Practices

### Combine Platform and Edge-to-Edge

```csharp
public class PlatformBootstrap : MonoBehaviour
{
    [SerializeField] private RectTransform _safePanel;

    private void Start()
    {
        IVXEdgeToEdgeHelper.ApplySafeArea(_safePanel);

        IVXFoldableHelper.OnScreenConfigChanged += (_, _) =>
        {
            IVXEdgeToEdgeHelper.ApplySafeArea(_safePanel);
        };
    }
}
```

### Battery-Aware VFX

```csharp
private void SpawnExplosion()
{
    if (IVXPlatformOptimizer.IsLowPowerMode)
    {
        SpawnLightweightExplosion();
    }
    else
    {
        SpawnFullExplosion();
    }
}
```

### Deep Link with Scene Guard

```csharp
private void HandleDeepLink(string path, string query)
{
    if (SceneManager.GetActiveScene().name != "MainMenu")
    {
        _deferredPath = path;
        _deferredQuery = query;
        return;
    }

    RouteLink(path, query);
}
```

---

## Related Documentation

- [Core Module](core.md) -- Foundation utilities
- [Identity Module](identity.md) -- Authentication (may interact with deep links for social sign-in)
- [Analytics Module](analytics.md) -- Platform events and device telemetry
