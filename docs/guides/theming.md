# Theming & UI Customization Guide

This guide explains how to keep IntelliVerseX-driven screens visually aligned with your game: palette, typography, imagery, light/dark variants, and module-specific accents.

---

## Overview

The SDK’s UI layer is built on **Unity UI (uGUI)** and **TextMeshPro (TMP)**. There is no single global “skin” file that restyles every prefab automatically; instead, you get **consistent patterns** you can extend:

- **Prefab + component overrides** — swap fonts, colors, and sprites on SDK UI prefabs you instantiate or subclass.
- **Quiz module themes** — first-class `Color` and label fields for weekly/daily quiz presentations (`IVXWeeklyQuizTheme`, `IVXDailyQuizTheme`).
- **Your own `ScriptableObject` theme asset** — central place for primary/secondary/accent/background/surface values that your UI code applies at runtime.

!!! tip "Design goal"
    Treat the SDK as a **starting layout**: clone prefabs into your project, wire them to your theme asset, and keep one source of truth for brand colors.

---

## Color configuration

Define a small palette (primary, secondary, accent, background, surface) and apply it when building or refreshing UI.

### Example: central palette on a ScriptableObject

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "MyGame/UI Theme")]
public sealed class MyGameUiTheme : ScriptableObject
{
    public Color primary = new Color(0.25f, 0.45f, 0.95f);
    public Color secondary = new Color(0.15f, 0.65f, 0.55f);
    public Color accent = new Color(0.98f, 0.72f, 0.18f);
    public Color background = new Color(0.06f, 0.07f, 0.10f);
    public Color surface = new Color(0.12f, 0.14f, 0.18f);
}
```

### Example: applying palette to uGUI elements

```csharp
using UnityEngine;
using UnityEngine.UI;

public sealed class MyThemeApplier : MonoBehaviour
{
    [SerializeField] private MyGameUiTheme _theme;
    [SerializeField] private Image _background;
    [SerializeField] private Image _panel;
    [SerializeField] private Graphic[] _primaryAccents;

    private void Awake()
    {
        if (_theme == null) return;
        if (_background != null) _background.color = _theme.background;
        if (_panel != null) _panel.color = _theme.surface;
        foreach (var g in _primaryAccents)
            if (g != null) g.color = _theme.primary;
    }
}
```

### Quiz module colors (built-in)

Weekly quiz flows expose structured theme data (primary, secondary, accent) per quiz type. You can read the active theme and mirror it in your own UI:

```csharp
using IntelliVerseX.Quiz.WeeklyQuiz;
using UnityEngine;

public void ApplyWeeklyQuizLook(IVXWeeklyQuizType type)
{
    IVXWeeklyQuizTheme t = IVXWeeklyQuizThemes.GetTheme(type);
    Debug.Log($"{t.themeName}: primary={t.primaryColor}, accent={t.accentColor}");
}
```

!!! note "Daily quiz"
    Daily quiz uses the same idea via `IVXDailyQuizThemes` / `IVXDailyQuizTheme` — inspect those types for per-mode colors and strings.

---

## Font configuration

1. Import your font and create a **TextMeshPro Font Asset** (`Window > TextMeshPro > Font Asset Creator`).
2. Assign the asset on each **TMP_Text** (or TMP component on SDK prefabs you duplicate into your project).
3. For dynamic localization, keep one TMP font asset per script/weight you need, and swap **Material Presets** if you use SDF outline/glow per theme.

!!! warning "Don’t edit package originals"
    Prefer **duplicating** SDK sample prefabs into your game assembly so package updates do not overwrite your font references.

---

## UI sprite customization

- Collect buttons, icons, and panel backgrounds in a **Sprite Atlas** (or addressable group) owned by your game.
- Replace **Image.sprite** on duplicated prefabs, or drive sprites from your theme asset:

```csharp
[CreateAssetMenu(menuName = "MyGame/UI Theme (Sprites)")]
public sealed class MyGameUiSpriteTheme : ScriptableObject
{
    public Sprite primaryButton;
    public Sprite secondaryButton;
    public Sprite panelBackground;
    public Sprite iconClose;
}
```

At runtime, assign these to `Image` components when opening SDK-related screens (popups, loading overlays, etc.). See the [UI module](../modules/ui.md) for component names like `IVXPopupManager` and prefab-driven flows.

---

## Dark mode

The SDK does not ship a single global dark-mode switch. Implement it in three layers:

1. **Persisted preference** — e.g. `PlayerPrefs` key `ui_theme` = `light` | `dark` | `system`.
2. **System detection (optional)** — on mobile, read OS appearance if you expose native code; in Editor, default to light unless overridden.
3. **Apply** — load either a light or dark `ScriptableObject` theme (or two color tables) and re-run your applier on all visible canvases.

```csharp
public enum UiAppearance { Light, Dark }

public static class UiThemePreference
{
    private const string Key = "ivx_ui_appearance";

    public static UiAppearance Load()
    {
        int v = PlayerPrefs.GetInt(Key, (int)UiAppearance.Light);
        return (UiAppearance)Mathf.Clamp(v, 0, 1);
    }

    public static void Save(UiAppearance value)
    {
        PlayerPrefs.SetInt(Key, (int)value);
        PlayerPrefs.Save();
    }
}
```

Fire an application-wide event after `Save` so open screens refresh without a scene reload.

---

## Per-module theming

| Area | What to customize |
|------|-------------------|
| **Quiz** | `IVXWeeklyQuizTheme` / `IVXDailyQuizTheme` colors, emoji, button text, completion copy — extend or replace static theme tables in your fork, or overlay your own UI that reads `GetTheme(type)`. |
| **Leaderboard** | Duplicate demo/test prefabs; apply your palette to rank rows, headers, and medals. Use live Nakama data in production; samples may show placeholder rows. |
| **Profile / social** | Profile sample controllers support mock fallback when the backend is unavailable — style those panels like the rest of your game and gate mock UI behind dev-only builds if needed. |

!!! tip "Module boundaries"
    Keep **network truth** in managers and **presentation** in your themed prefabs so you can reskin without touching backend code.

---

## Runtime theme switching

1. Hold references to root canvases or a registry of `Graphic` elements you own.
2. When the user picks a new theme, update the active `ScriptableObject` reference (or enum) and call a single `ApplyTheme()` that sets colors, TMP style sheets (if any), and sprites.
3. For popups managed by `IVXPopupManager`, ensure newly opened popups run through the same applier (e.g. on `OnEnable`).

```csharp
public event System.Action<MyGameUiTheme> OnThemeChanged;

public void SetTheme(MyGameUiTheme theme)
{
    _active = theme;
    OnThemeChanged?.Invoke(_active);
}
```

---

## Creating a custom theme ScriptableObject

Use Unity’s `CreateAssetMenu` so designers can author themes without code changes:

```csharp
using TMPro;
using UnityEngine;

namespace MyGame.UI
{
    [CreateAssetMenu(fileName = "IVX_CustomTheme", menuName = "IntelliVerseX/Custom UI Theme")]
    public sealed class IVXCustomUiTheme : ScriptableObject
    {
        [Header("Brand")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public Color accentColor = Color.yellow;

        [Header("Surfaces")]
        public Color backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        public Color surfaceColor = new Color(0.12f, 0.12f, 0.15f);

        [Header("Typography")]
        public TMP_FontAsset primaryFont;
        public float baseFontSize = 28f;
    }
}
```

Assign the asset on a bootstrap or UI root object, then propagate to child screens on load and whenever the theme changes.

---

## See also

- [UI module](../modules/ui.md) — popups, toasts, loading overlay, navigation.
- [UI demo sample](../samples/ui-demo.md) — reference wiring and prefab patterns.
