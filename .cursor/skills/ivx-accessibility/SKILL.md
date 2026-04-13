---
name: ivx-accessibility
description: >-
  Add accessibility features to IntelliVerseX SDK games including color blind support,
  screen readers, scalable UI, input remapping, and certification compliance.
  Use when the user says "accessibility", "color blind mode", "screen reader",
  "font scaling", "input remapping", "WCAG compliance", "subtitle system",
  "high contrast", "one-handed play", "switch control", "accessibility audit",
  "Xbox accessibility", "PlayStation accessibility", "CVAA", "Section 508",
  or needs help with any accessibility feature or certification.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX Accessibility

## Overview

The Accessibility module provides drop-in features for making games playable by everyone. It covers visual, auditory, motor, and cognitive accessibility needs, with built-in compliance checkers for WCAG 2.1, CVAA (21st Century Communications and Video Accessibility Act), Xbox Accessibility Guidelines (XAG), and PlayStation accessibility requirements.

```
IVXAccessibilityManager (central coordinator)
      │
      ├── IVXColorBlindFilter     → Simulation + correction shaders
      ├── IVXScreenReaderBridge   → TalkBack / VoiceOver / Narrator
      ├── IVXScalableUI           → Dynamic font sizing + layouts
      ├── IVXInputRemapper        → Custom bindings + one-handed modes
      ├── IVXSubtitleSystem       → Captions with speaker ID + settings
      ├── IVXHighContrastMode     → High contrast UI + outlines
      └── IVXAccessibilityAudit   → WCAG / XAG compliance checker
```

---

## 1. Configuration

### IVXAccessibilityConfig ScriptableObject

Create via **Create > IntelliVerseX > Accessibility Configuration**.

| Field | Description |
|-------|-------------|
| `EnableAccessibility` | Master toggle |
| `DefaultFontScale` | Initial font scale multiplier (default 1.0) |
| `MinFontScale` / `MaxFontScale` | Allowed range (default 0.75–2.0) |
| `DefaultColorBlindMode` | `None`, `Deuteranopia`, `Protanopia`, `Tritanopia` |
| `EnableScreenReader` | Toggle screen reader integration |
| `SubtitleDefaults` | Default subtitle size, background opacity, speaker colors |
| `ShowAccessibilityOnFirstLaunch` | Prompt accessibility settings on first run |
| `HighContrastDefault` | Start in high contrast mode (default false) |

---

## 2. Color Blind Support

### IVXColorBlindFilter

Post-processing shader that simulates or corrects for color vision deficiencies:

```csharp
using IntelliVerseX.Accessibility;

IVXColorBlindFilter.Instance.SetMode(ColorBlindMode.Deuteranopia);
```

### Modes

| Mode | Affects | Prevalence |
|------|---------|-----------|
| `None` | No filter | — |
| `Deuteranopia` | Red-green (green cone) | 6% of males |
| `Protanopia` | Red-green (red cone) | 2% of males |
| `Tritanopia` | Blue-yellow | 0.01% |
| `Achromatopsia` | Complete color blindness | 0.003% |

### Simulation Mode (Design Tool)

```csharp
IVXColorBlindFilter.Instance.SetSimulationMode(ColorBlindMode.Deuteranopia);
```

Use in the editor to preview how your game looks to color blind players.

### Color Palette Helper

```csharp
var palette = IVXColorBlindFilter.Instance.GetSafePalette(paletteSize: 8);

foreach (var color in palette)
{
    Debug.Log($"Safe color: #{ColorUtility.ToHtmlStringRGB(color)}");
}
```

Returns colors distinguishable across all common color vision types.

---

## 3. Screen Reader Integration

### IVXScreenReaderBridge

Bridges game UI to platform screen readers:

```csharp
IVXScreenReaderBridge.Instance.Announce("Level 5 complete. Score: 12,500 points.");

IVXScreenReaderBridge.Instance.SetFocusDescription(
    element: startButton,
    label: "Start Game",
    hint: "Double tap to begin playing"
);
```

### Platform Screen Readers

| Platform | Screen Reader | Integration |
|----------|--------------|-------------|
| **Android** | TalkBack | `AccessibilityService` API |
| **iOS** | VoiceOver | `UIAccessibility` protocol |
| **Windows** | Narrator / NVDA | `UI Automation` API |
| **macOS** | VoiceOver | `NSAccessibility` protocol |
| **Xbox** | Narrator | Xbox Accessibility API |
| **PlayStation** | System TTS | Sony TRC accessibility API |
| **WebGL** | Browser screen reader | ARIA attributes on DOM overlay |

### Accessible Navigation

```csharp
IVXScreenReaderBridge.Instance.SetNavigationOrder(new[]
{
    titleText,
    playButton,
    settingsButton,
    quitButton,
});
```

---

## 4. Scalable UI

### IVXScalableUI

Dynamic font sizing and layout adaptation:

```csharp
IVXScalableUI.Instance.SetFontScale(1.5f);

IVXScalableUI.Instance.OnFontScaleChanged += (newScale) =>
{
    RefreshAllTextElements();
};
```

### Auto-Scaling Components

| Component | Behavior |
|-----------|---------|
| **Text** | Font size multiplied by scale factor |
| **Buttons** | Touch targets scale with font (min 44x44 dp) |
| **Spacing** | Padding and margins scale proportionally |
| **Icons** | Icon size scales with adjacent text |
| **Layouts** | Scroll views auto-expand, grids reflow |

### Touch Target Validation

```csharp
var violations = IVXScalableUI.Instance.ValidateTouchTargets(minSize: 44);

foreach (var v in violations)
{
    Debug.LogWarning($"Touch target too small: {v.Element.name} ({v.Width}x{v.Height})");
}
```

---

## 5. Input Remapping

### IVXInputRemapper

Custom input bindings with accessibility profiles:

```csharp
IVXInputRemapper.Instance.SetProfile(InputProfile.OneHanded);

IVXInputRemapper.Instance.Remap("jump", new InputBinding
{
    Keyboard = KeyCode.Space,
    Gamepad = GamepadButton.South,
    Touch = TouchGesture.Tap,
});
```

### Built-In Profiles

| Profile | Description |
|---------|------------|
| `Default` | Standard control scheme |
| `OneHanded` | All actions accessible with one hand |
| `SwitchControl` | Sequential scanning of UI elements |
| `EyeTracking` | Gaze-based selection (requires hardware) |
| `VoiceControl` | Voice commands for actions (uses IVX AI voice) |
| `Custom` | User-defined bindings |

### Hold vs Toggle

```csharp
IVXInputRemapper.Instance.SetHoldToToggle("sprint", true);
IVXInputRemapper.Instance.SetHoldToToggle("aim", true);
```

---

## 6. Subtitle System

### IVXSubtitleSystem

Configurable captions for dialog, narration, and sound effects:

```csharp
IVXSubtitleSystem.Instance.Show(new Subtitle
{
    Speaker = "Professor Oak",
    SpeakerColor = Color.green,
    Text = "Welcome to the world of trivia!",
    Duration = 3.0f,
    IsNarration = false,
});

IVXSubtitleSystem.Instance.ShowSoundEffect("[explosion rumbles]");
```

### Subtitle Settings

| Setting | Options | Default |
|---------|---------|---------|
| **Size** | Small, Medium, Large, Extra Large | Medium |
| **Background** | Transparent, 50%, 75%, Opaque | 75% |
| **Position** | Bottom, Top | Bottom |
| **Speaker Colors** | On/Off | On |
| **Sound Effects** | On/Off | Off |
| **Font** | System, Dyslexia-Friendly (OpenDyslexic) | System |
| **Letter Spacing** | Normal, Wide | Normal |

---

## 7. High Contrast Mode

### IVXHighContrastMode

Enhanced visual clarity for low-vision players:

```csharp
IVXHighContrastMode.Instance.Enable();

IVXHighContrastMode.Instance.SetOutlineColor(Color.yellow);
IVXHighContrastMode.Instance.SetOutlineWidth(3f);
IVXHighContrastMode.Instance.SetBackgroundDimming(0.7f);
```

### Features

| Feature | Effect |
|---------|--------|
| **UI outlines** | Bright outlines around all interactive elements |
| **Background dimming** | Non-interactive areas dimmed |
| **Text contrast** | Enforced minimum contrast ratio (4.5:1) |
| **Enemy outlines** | Gameplay entities highlighted with distinct colors |
| **Colorblind-safe indicators** | Shape + color redundancy on all status indicators |

---

## 8. Accessibility Settings Menu

### Drop-In Prefab

The SDK includes a ready-to-use accessibility settings panel:

```csharp
IVXAccessibilityMenu.Instance.Show();
```

The menu includes:
- Font size slider
- Color blind mode selector
- Subtitle toggle and settings
- High contrast toggle
- Input remapping interface
- Screen reader toggle
- Motion reduction toggle
- Screen shake toggle

### Persistence

Settings are saved to `PlayerPrefs` (Unity) or platform-equivalent storage and restored on next launch.

---

## 9. Accessibility Audit

### IVXAccessibilityAudit

Automated compliance checking:

```csharp
var report = await IVXAccessibilityAudit.Instance.RunAuditAsync(AuditStandard.WCAG_21_AA);

Debug.Log($"Score: {report.Score}/100");
Debug.Log($"Pass: {report.PassCount}, Fail: {report.FailCount}, Warning: {report.WarningCount}");

foreach (var issue in report.Issues)
{
    Debug.Log($"[{issue.Severity}] {issue.Guideline}: {issue.Description}");
    Debug.Log($"  Element: {issue.Element}, Fix: {issue.SuggestedFix}");
}
```

### Supported Standards

| Standard | Coverage | Required By |
|----------|---------|-------------|
| **WCAG 2.1 AA** | Perceivable, Operable, Understandable, Robust | Web, EU regulations |
| **WCAG 2.1 AAA** | Enhanced WCAG criteria | Best practice |
| **Xbox XAG** | Xbox Accessibility Guidelines (23 guidelines) | Xbox certification |
| **PlayStation** | Sony TRC accessibility requirements | PS certification |
| **CVAA** | Communications accessibility for chat/voice | US law (multiplayer games) |
| **Section 508** | US federal accessibility requirements | US government contracts |
| **Apple HIG** | Apple Human Interface Guidelines accessibility | App Store |
| **Google Play** | Google accessibility requirements | Play Store |

### Audit Categories

| Category | Checks |
|----------|--------|
| **Visual** | Contrast ratios, color-only information, text scaling, animation |
| **Auditory** | Subtitles, visual alternatives for audio cues, volume controls |
| **Motor** | Touch target sizes, input remapping, timing requirements, hold-vs-toggle |
| **Cognitive** | Reading level, consistent navigation, error prevention, help text |

---

## 10. Cross-Platform API

| Engine | Class / Module | Core API |
|--------|---------------|----------|
| **Unity** | `IVXAccessibilityManager` | Full feature set |
| **Unreal** | `UIVXAccessibilitySubsystem` | Color blind, subtitles, input remap, audit |
| **Godot** | `IVXAccessibility` autoload | Color blind, subtitles, input remap |
| **JavaScript** | `IVXAccessibility` | ARIA integration, font scaling, color blind CSS |
| **Roblox** | `IVX.Accessibility` | Subtitles, font scaling (Roblox handles most a11y) |
| **Java** | `IVXAccessibility` | TalkBack bridge, font scaling |
| **Flutter** | `IvxAccessibility` | Semantics integration, font scaling |
| **C++** | `IVXAccessibility` | Color blind, subtitles, input remap |

---

## Platform-Specific Accessibility

### VR Platforms

| Feature | VR Guidance |
|---------|------------|
| **Subtitles** | World-space subtitles anchored in front of the player (not head-locked). |
| **Color blind** | Apply post-processing filter to the VR camera rig. |
| **Comfort** | Vignette, snap turn, seated mode, teleport locomotion are accessibility features in VR. |
| **One-handed** | Support single-controller gameplay for players with one hand. |
| **Motion** | Provide static backgrounds option and reduce camera shake. |

### Console Platforms

| Platform | Accessibility Requirements |
|----------|--------------------------|
| **Xbox** | XAG compliance is strongly recommended (23 guidelines). Required for Xbox Accessibility badge. |
| **PlayStation** | Sony TRC includes accessibility requirements. System-level text-to-speech available. |
| **Switch** | Minimal platform requirements. Font size and color options are best practice. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **ARIA** | IVX generates ARIA attributes on DOM overlay elements for screen reader compatibility. |
| **Keyboard nav** | Full keyboard navigation with visible focus indicators for web builds. |
| **Prefers-reduced-motion** | Respects CSS `prefers-reduced-motion` media query. |
| **Prefers-contrast** | Respects CSS `prefers-contrast` media query for high contrast mode. |

---

## Checklist

- [ ] `EnableAccessibility` toggled on in `IVXBootstrapConfig`
- [ ] Accessibility settings menu integrated and accessible from main menu
- [ ] Color blind filter tested in all three common modes
- [ ] Screen reader integration tested on target platforms
- [ ] Font scaling tested at 0.75x through 2.0x without layout breaks
- [ ] All touch targets >= 44x44 dp
- [ ] Input remapping available for all game actions
- [ ] Subtitles available for all dialog and narration
- [ ] Sound effect captions available for gameplay-critical audio
- [ ] High contrast mode tested on all key screens
- [ ] WCAG 2.1 AA audit passed (score > 80)
- [ ] XAG compliance reviewed (if targeting Xbox)
- [ ] Sony TRC accessibility requirements met (if targeting PlayStation)
- [ ] CVAA compliance verified for chat/voice features (if multiplayer)
- [ ] VR comfort settings included (if targeting VR)
- [ ] WebGL ARIA attributes and keyboard navigation verified (if targeting browser)
