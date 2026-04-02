# Localization Demo

This sample covers **language switching**, **RTL**, **fonts**, **key-based strings**, and **runtime locale changes** using the IntelliVerseX localization stack.

## Two complementary APIs

| Layer | Types | Best for |
|-------|-------|----------|
| Config-driven | `IVXLanguageManager` | Simple code ↔ `IntelliVerseXConfig.supportedLanguages`, PlayerPrefs `IVX_Language`. |
| Provider-driven | `IVXLocalizationService`, `IIVXLocalizationProvider`, `IVXCSVLocalizationProvider` | CSV tables, async load, many keys. |

## Language switching

**IVXLanguageManager** (static):

```csharp
using IntelliVerseX.Localization;

// After IntelliVerseXConfig is available
IVXLanguageManager.Initialize(config);

// Switch at runtime (persists to PlayerPrefs)
IVXLanguageManager.SetLanguage("es");
IVXLanguageManager.OnLanguageChanged += code => RebindAllLocalizedTexts();
```

**IVXLocalizationService** (singleton):

```csharp
IVXLocalizationService.Instance.SetProvider(new IVXCSVLocalizationProvider());
await IVXLocalizationService.Instance.SetLanguageAsync("fr");
string play = IVXLocalizationService.Instance.GetString("Button_Play", "Play");
```

## RTL support

For Arabic and other RTL locales:

- Add **`IVXRTLLayoutComponent`** to roots that should mirror layout when `IVXLocalizationService` / language manager reports an RTL code (e.g. `ar`).
- After `SetLanguage`, refresh layout: `Canvas.ForceUpdateCanvases()` if needed.
- Test **alignment** on `TextMeshProUGUI` (often `Right` + RTL font features).

## Font loading

- Assign **TMP font assets** per script or use **fallback** TMP settings for Latin + Arabic + CJK.
- For heavy CJK, consider dynamic OS fonts on mobile where licensing allows.
- Keep font **materials** consistent with your UI theme when swapping languages.

## Key-based text

Prefer **stable keys** over English literals:

```csharp
// IVXLocalizationService
status.text = IVXLocalizationService.Instance.GetString("HUD_LevelProgress", "Level {0}");

// IVXLocalizedText component: set key in Inspector; component pulls on enable / language change
```

`IVXLocalizationHelper` offers small conveniences for formatting; prefer one pipeline per screen to avoid double providers.

## Runtime locale change

1. Update language via `IVXLanguageManager.SetLanguage` or `IVXLocalizationService.SetLanguageAsync`.
2. Raise UI refresh: subscribe to **`IVXLocalizationService.OnLanguageChanged`** or **`IVXLanguageManager.OnLanguageChanged`**.
3. Rebind **`IVXLocalizedText`** instances (they typically listen or expose `Refresh()` if implemented).
4. Persist selection — both systems use PlayerPrefs keys (`IVX_Language`, `IVX_SelectedLanguage`).

## Setup checklist

1. Add CSV or string tables under a load path your `IVXCSVLocalizationProvider` expects.
2. List supported languages on **`IntelliVerseXConfig`** for the manager path.
3. Call **`Initialize`** / **`SetProvider`** during bootstrap before first UI frame.

## Testing matrix

Smoke-test **three** locales per release: one LTR Latin, one RTL (e.g. `ar`), and one CJK or extended Latin to catch font and overflow issues in shop and quiz layouts.

## See also

- [Localization module](../modules/localization.md)
- [Localization setup guide](../guides/localization-setup.md)
