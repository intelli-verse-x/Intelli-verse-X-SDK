---
name: ivx-localization
description: >-
  Comprehensive game localization workflows covering store listing localization, in-game text
  l10n, font coverage analysis, RTL layout support, locale-aware asset variants, and ASO
  keyword translation. Use when the user says "localize my game", "translate store listing",
  "add language support", "RTL layout", "CJK fonts", "localized screenshots",
  "localization plan", "i18n", "l10n", "multi-language", "Arabic support",
  "Japanese localization", "localized keywords", "ASO translation", or needs help
  with any localization or internationalization workflow.
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

# IntelliVerseX Localization

## Overview

The Localization skill covers the full i18n/l10n lifecycle for games — from in-game text to store listings to screenshot localization to ASO keyword translation. It integrates with Content-Factory's `screenshot_localizer` pipeline and the GDD localization plan.

```
Localization Plan (from GDD)
       │
       ▼
┌── Localization Workflows ────────────────────────────────┐
│                                                           │
│  In-Game Text → Store Listings → Screenshots → Keywords   │
│       │              │               │             │      │
│  CSV/JSON        LLM Translate   Localizer     ASO Tool   │
│  per locale      per store       pipeline      per market  │
│                                                           │
└──────────────────────────────────────────────────────────┘
       │
       ▼
  Localized Assets on S3 / in Build
```

---

## 1. In-Game Text Localization

### String Table Format

All user-facing strings live in CSV or JSON string tables, one per language:

```
localization/
├── en.csv           # English (base)
├── es-419.csv       # Latin American Spanish
├── pt-BR.csv        # Brazilian Portuguese
├── fr-FR.csv        # French
├── de-DE.csv        # German
├── ja-JP.csv        # Japanese
├── ko-KR.csv        # Korean
├── zh-Hans.csv      # Simplified Chinese
├── ar-SA.csv        # Arabic
└── hi-IN.csv        # Hindi
```

### CSV Format

```csv
key,text,context,max_chars
main_menu.play,"Play",Button label,10
main_menu.settings,"Settings",Button label,15
game.correct,"Correct!",Feedback on right answer,20
game.wrong,"Wrong!",Feedback on wrong answer,20
game.score,"Score: {0}",Score display with number placeholder,25
store.season_pass_title,"Season Pass",IAP display name,30
```

### Unity Integration

```csharp
using IntelliVerseX.Localization;

var text = IVXLocalization.Get("main_menu.play");
var formatted = IVXLocalization.GetFormatted("game.score", score);

IVXLocalization.OnLanguageChanged += lang =>
{
    RefreshAllUI();
};
```

### Scanning for Hardcoded Strings

```bash
python tools/localization/scan_hardcoded.py --src Assets/_QuizVerse/Scripts/ \
  --ignore "Debug.Log" --ignore "nameof" \
  --output reports/hardcoded_strings.csv
```

---

## 2. Store Listing Localization

### LLM-Powered Translation

```bash
python tools/localization/translate_store.py \
  --base-locale en \
  --target-locales es-419,pt-BR,fr-FR,de-DE,ja-JP,ko-KR,zh-Hans \
  --input design/store-metadata.md \
  --output localization/store/
```

### Output Structure

```
localization/store/
├── en/
│   ├── title.txt
│   ├── short_description.txt
│   ├── full_description.txt
│   └── keywords.txt
├── es-419/
│   ├── title.txt
│   ├── short_description.txt
│   ├── full_description.txt
│   └── keywords.txt
└── ... (per locale)
```

### Character Limits Per Store

| Field | iOS | Google Play | Steam |
|-------|-----|-------------|-------|
| App Name | 30 chars | 30 chars | Unlimited |
| Short Description | 45 chars (subtitle) | 80 chars | N/A |
| Full Description | 4000 chars | 4000 chars | Unlimited |
| Keywords | 100 chars | N/A (tags) | Up to 20 tags |
| What's New | 4000 chars | 500 chars | N/A |

### Translation Quality Gate

Every translated store listing passes through Content-Factory's `council_audit`:

```python
from council_server.server import compliance_check

result = await compliance_check(
    content_metadata={"title": translated_title, "description": translated_desc},
    platform="ios",
    content_type="store_listing",
    brand_context=brand_entity,
)
```

---

## 3. Screenshot Localization

Uses Content-Factory's `screenshot_localizer` pipeline to overlay localized text on store screenshots.

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/screenshot_localizer \
  -H "Content-Type: application/json" \
  -d '{
    "base_screenshots_dir": "store_launch/ios/screenshots/",
    "locales": ["es-419", "pt-BR", "fr-FR", "de-DE", "ja-JP"],
    "overlay_text": {
      "screenshot_01": {"en": "Play Now!", "es-419": "¡Juega Ahora!"},
      "screenshot_02": {"en": "20+ Categories", "es-419": "20+ Categorías"}
    },
    "font_config": {
      "default": "Nunito-Bold",
      "ja-JP": "NotoSansJP-Bold",
      "ko-KR": "NotoSansKR-Bold",
      "zh-Hans": "NotoSansSC-Bold",
      "ar-SA": "NotoSansArabic-Bold",
      "hi-IN": "NotoSansDevanagari-Bold"
    }
  }'
```

### Output

```
store_launch/ios/screenshots/
├── en/
│   ├── iphone_6.7_01_1290x2796.png
│   └── iphone_6.7_02_1290x2796.png
├── es-419/
│   ├── iphone_6.7_01_1290x2796.png
│   └── iphone_6.7_02_1290x2796.png
└── ja-JP/
    ├── iphone_6.7_01_1290x2796.png
    └── iphone_6.7_02_1290x2796.png
```

---

## 4. Font Coverage

### Required Font Families by Script

| Script | Languages | Recommended Font | Fallback |
|--------|-----------|-----------------|----------|
| Latin | en, es, fr, de, pt, it | Nunito, Fredoka One | Arial |
| CJK | ja, ko, zh-Hans, zh-Hant | Noto Sans CJK | Source Han Sans |
| Arabic | ar | Noto Sans Arabic | Amiri |
| Devanagari | hi | Noto Sans Devanagari | Mangal |
| Thai | th | Noto Sans Thai | Tahoma |
| Cyrillic | ru, uk | Noto Sans | Arial |
| Korean | ko | Noto Sans KR | Malgun Gothic |

### Font Coverage Audit

```bash
python tools/localization/audit_fonts.py \
  --string-table localization/ja-JP.csv \
  --font Assets/Fonts/NotoSansJP-Regular.otf \
  --report reports/font_coverage_ja.md
```

The audit checks every character in the string table against the font's glyph coverage and reports missing glyphs.

### Unity Font Setup

```csharp
[Header("Localized Fonts")]
[SerializeField] private TMP_FontAsset _latinFont;
[SerializeField] private TMP_FontAsset _cjkFont;
[SerializeField] private TMP_FontAsset _arabicFont;
[SerializeField] private TMP_FontAsset _devanagariFont;

private TMP_FontAsset GetFontForLocale(string locale)
{
    return locale switch
    {
        "ja-JP" or "ko-KR" or "zh-Hans" or "zh-Hant" => _cjkFont,
        "ar-SA" or "he-IL" => _arabicFont,
        "hi-IN" => _devanagariFont,
        _ => _latinFont,
    };
}
```

---

## 5. RTL (Right-to-Left) Support

### Layout Mirroring Checklist

| Element | LTR (Default) | RTL (Arabic, Hebrew) |
|---------|--------------|---------------------|
| Text alignment | Left | Right |
| Reading order | Left → Right | Right → Left |
| Navigation flow | Left → Right | Right → Left |
| Progress bars | Fill left → right | Fill right → left |
| Scrolling lists | Standard | Mirrored |
| Icons with direction | → arrows | ← arrows |
| Number formatting | 1,234.56 | ١٬٢٣٤٫٥٦ (optional) |

### Unity RTL Implementation

```csharp
using IntelliVerseX.Localization;

public class IVXRTLLayout : MonoBehaviour
{
    [SerializeField] private RectTransform _container;

    private void OnEnable()
    {
        IVXLocalization.OnLanguageChanged += ApplyDirection;
        ApplyDirection(IVXLocalization.CurrentLocale);
    }

    private void ApplyDirection(string locale)
    {
        bool isRTL = locale is "ar-SA" or "he-IL" or "fa-IR" or "ur-PK";
        var scale = _container.localScale;
        scale.x = isRTL ? -1f : 1f;
        _container.localScale = scale;
    }
}
```

### TextMeshPro RTL

```csharp
textComponent.isRightToLeftText = IVXLocalization.IsRTL;
textComponent.alignment = IVXLocalization.IsRTL
    ? TextAlignmentOptions.Right
    : TextAlignmentOptions.Left;
```

---

## 6. Locale-Aware Asset Variants

Some visual assets need cultural adaptation beyond text translation.

### When to Create Variants

| Scenario | Action |
|----------|--------|
| Hand gestures (thumbs up, OK) | Check cultural meaning per market |
| Character clothing/modesty | Create MENA-appropriate variants |
| Color symbolism (red = luck in China, danger in West) | Adjust palette |
| Food/drink imagery | Remove alcohol for MENA, adjust dietary imagery |
| Holiday-themed content | Localize to regional holidays |
| Currency symbols | Use locale-appropriate currency |

### Asset Variant Structure

```
assets/characters/Hero/
├── default/
│   └── idle.png
├── ar-SA/
│   └── idle.png          # Culturally adapted variant
└── zh-Hans/
    └── idle.png          # Variant with lucky-red accent
```

### Content-Factory Integration

```bash
curl -X POST http://localhost:8001/pipelines/character_2d \
  -H "Content-Type: application/json" \
  -d '{
    "brand_id": "my-studio",
    "game_id": "my-game",
    "character_id": "hero",
    "locale_variants": ["ar-SA", "zh-Hans"],
    "cultural_notes": {
      "ar-SA": "Modest clothing, no exposed skin above elbow",
      "zh-Hans": "Add red and gold accent colors for lucky appeal"
    }
  }'
```

---

## 7. ASO Keyword Localization

### LLM-Powered Keyword Translation

Keywords are not direct translations — they need market-specific research.

```bash
python tools/localization/translate_keywords.py \
  --base-keywords "trivia,quiz,brain,knowledge,puzzle,multiplayer" \
  --target-locales es-419,pt-BR,ja-JP,ko-KR,de-DE \
  --output localization/keywords/
```

### Output: Per-Locale Keyword Files

```
localization/keywords/
├── en.txt          # trivia,quiz,brain,knowledge,puzzle,multiplayer
├── es-419.txt      # trivia,preguntas,cerebro,conocimiento,quiz,multijugador
├── ja-JP.txt       # クイズ,トリビア,脳トレ,知識,パズル,マルチプレイヤー
└── ko-KR.txt       # 퀴즈,상식,두뇌,지식,퍼즐,멀티플레이어
```

### Character Budget

| Store | Max Keyword Length | Strategy |
|-------|--------------------|----------|
| iOS | 100 characters total | Pack high-volume terms, no spaces |
| Google Play | 5 tags, each 30 chars | Use category-relevant phrases |
| Steam | 20 tags, each unlimited | Long-tail descriptive tags |

---

## 8. Localization QA

### Automated Checks

```bash
python tools/localization/qa_check.py \
  --base localization/en.csv \
  --translations localization/ \
  --report reports/localization_qa.md
```

### What It Checks

| Check | Description |
|-------|-------------|
| **Missing keys** | Keys in base not present in translation |
| **Extra keys** | Keys in translation not in base |
| **Placeholder mismatch** | `{0}`, `{1}` counts differ |
| **Max length exceeded** | Translation exceeds `max_chars` |
| **Empty translations** | Key present but value is empty |
| **Untranslated** | Value is identical to English (possible miss) |
| **HTML/markup broken** | Tags not properly closed |
| **Bidirectional mixing** | LTR text embedded in RTL without markers |

---

## Platform Notes

### Unity
- Use TextMeshPro for all text (supports CJK, Arabic, Devanagari)
- Set `Addressables` labels per locale for async font loading
- Use `SystemLanguage` for initial locale detection, allow user override

### Unreal
- Use `FText` and the Localization Dashboard
- String tables in `.csv` or `.po` format
- Locale-specific assets via Asset Manager

### Godot
- Use `TranslationServer` with `.po` or `.csv` files
- `tr()` function for runtime translation
- Font fallback chains in Theme resources

### Roblox
- Use `LocalizationService` with `.csv` translation tables
- `Translator:FormatByKey()` for parameterized strings

### WebGL / JavaScript
- Use `i18next` or similar library
- JSON string files loaded per locale
- CSS `direction: rtl` for Arabic/Hebrew layouts

---

## Checklist

- [ ] Localization plan created in `design/localization-plan.md` (via GDD skill)
- [ ] Base language string table created (`localization/en.csv`)
- [ ] All user-facing strings extracted (no hardcoded strings in code)
- [ ] String tables translated for P0 + P1 languages
- [ ] Font coverage verified for CJK, Arabic, Devanagari (if targeted)
- [ ] RTL layout support implemented and tested (if Arabic/Hebrew targeted)
- [ ] Store listings translated for all target languages
- [ ] Screenshots localized via `screenshot_localizer` pipeline
- [ ] ASO keywords researched and translated per market
- [ ] Locale-aware asset variants created (if culturally needed)
- [ ] Localization QA passed (no missing keys, placeholder mismatches, or overflows)
- [ ] User language preference saved and restored across sessions
