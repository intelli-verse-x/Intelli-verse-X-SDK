# Platform Utilities API Reference

Complete API reference for the Platform module providing adaptive rendering, deep linking, safe-area layout, and foldable-device detection.

---

## IVXPlatformOptimizer

Singleton MonoBehaviour applying device-appropriate frame-rate targets and battery-aware rendering adjustments.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsLowPowerMode` | `bool` (static) | Whether the device is in low-power mode |
| `BatteryLevel` | `float` (static) | Battery charge level (0.0--1.0; returns 1.0 when unknown) |

### Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `_mobileTargetFps` | `int` | `60` | Target FPS on Android / iOS |
| `_desktopTargetFps` | `int` | `-1` | Target FPS on desktop (`-1` = platform default) |
| `_lowPowerTargetFps` | `int` | `30` | FPS cap when battery is below threshold |
| `_enableBatteryThrottling` | `bool` | `true` | Enable automatic low-power mode |
| `_batteryCheckIntervalSec` | `float` | `30` | Seconds between battery-level checks |

### Methods

#### SetTargetFrameRate

```csharp
public static void SetTargetFrameRate(int fps)
```

Overrides the target frame rate at runtime.

**Parameters:**
- `fps` - Target frames per second

**Example:**
```csharp
IVXPlatformOptimizer.SetTargetFrameRate(30);
```

---

#### SetRenderScale

```csharp
public static void SetRenderScale(float scale)
```

Sets the dynamic render resolution scale.

**Parameters:**
- `scale` - Render scale (0.25--1.0)

**Example:**
```csharp
IVXPlatformOptimizer.SetRenderScale(0.75f);
```

---

## IVXFoldableHelper

Singleton MonoBehaviour detecting foldable device states and screen configuration changes.

### Enums

#### DeviceFormFactor

| Value | Description |
|-------|-------------|
| `Phone` | Mobile phone (< 6.5" diagonal) |
| `Tablet` | Tablet (>= 6.5" diagonal on mobile) |
| `Foldable` | Foldable device |
| `Desktop` | Desktop / Editor / Standalone |
| `Unknown` | Unrecognized platform |

#### FoldState

| Value | Description |
|-------|-------------|
| `Unknown` | State not available |
| `Flat` | Fully open |
| `HalfOpen` | Partially folded (tabletop mode) |
| `Folded` | Fully folded |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ScreenDiagonalInches` | `float` (static) | Calculated screen diagonal in inches |
| `IsLargeScreen` | `bool` (static) | Whether diagonal >= 6.5 inches |
| `FormFactor` | `DeviceFormFactor` (static) | Detected device form factor |
| `AspectRatio` | `float` (static) | Screen aspect ratio (always >= 1) |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnScreenConfigChanged` | `Action<int, int>` (static) | Screen resolution changed (width, height) |
| `OnLargeScreenChanged` | `Action<bool>` (static) | Large-screen threshold crossed |

### Usage

```csharp
IVXFoldableHelper.OnScreenConfigChanged += (w, h) => RebuildLayout();

IVXFoldableHelper.OnLargeScreenChanged += isLarge =>
{
    sidePanel.SetActive(isLarge);
};

switch (IVXFoldableHelper.FormFactor)
{
    case IVXFoldableHelper.DeviceFormFactor.Phone:
        ApplyPhoneLayout();
        break;
    case IVXFoldableHelper.DeviceFormFactor.Tablet:
        ApplyTabletLayout();
        break;
}
```

---

## IVXEdgeToEdgeHelper

Static utility for safe-area queries and RectTransform anchoring on devices with notches and system gesture bars.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `SafeArea` | `Rect` (static) | Safe area in screen coordinates |
| `SafeAreaNormalized` | `Rect` (static) | Safe area in normalized 0--1 range |
| `HasNotch` | `bool` (static) | Whether the device has a notch or cutout |

### Methods

#### ApplySafeArea

```csharp
public static void ApplySafeArea(RectTransform target)
```

Applies safe-area anchors to a RectTransform so UI avoids notches and system bars.

**Parameters:**
- `target` - RectTransform to adjust

**Example:**
```csharp
[SerializeField] private RectTransform _safePanel;

private void Start()
{
    IVXEdgeToEdgeHelper.ApplySafeArea(_safePanel);
}
```

---

## IVXDeepLinkManager

Singleton MonoBehaviour processing incoming deep links, universal links, and app links.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `LastDeepLink` | `string` (static) | Most recent deep link URI |

### Methods

#### ConsumePendingDeepLink

```csharp
public static string ConsumePendingDeepLink()
```

Retrieves and clears the stored pending deep link. Useful for cold-start handling.

**Returns:** The pending URI or `null`

**Example:**
```csharp
string pending = IVXDeepLinkManager.ConsumePendingDeepLink();
if (pending != null)
    ProcessLink(pending);
```

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnDeepLinkReceived` | `Action<string>` (static) | Raw deep link URI received |
| `OnDeepLinkParsed` | `Action<string, string>` (static) | Parsed deep link (path, query) |

### Usage

```csharp
IVXDeepLinkManager.OnDeepLinkParsed += (path, query) =>
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
};
```

---

## See Also

- [Platform Module Guide](../modules/platform.md)
- [Core Module](../modules/core.md)
- [Identity Module](../modules/identity.md)
