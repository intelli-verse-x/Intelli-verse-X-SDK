# Localization Setup Guide

Add multi-language support to your game.

---

## Overview

The IntelliVerseX localization system provides:

- **Multiple languages** - UTF-8 support for all languages
- **CSV-based** - Easy to edit with spreadsheets
- **Runtime switching** - Change language without restart
- **Fallback system** - Missing translations gracefully handled
- **TextMeshPro integration** - Full font support

---

## Quick Start

### 1. Create Localization File

Create `Assets/_IntelliVerseXSDK/Resources/Localization/localization.csv`:

```csv
key,en,es,fr,de,ja,ko,zh
welcome_message,Welcome!,¡Bienvenido!,Bienvenue!,Willkommen!,ようこそ!,환영합니다!,欢迎!
play_button,Play,Jugar,Jouer,Spielen,プレイ,플레이,开始游戏
settings,Settings,Configuración,Paramètres,Einstellungen,設定,설정,设置
```

### 2. Initialize

```csharp
using IntelliVerseX.Localization;

// Localization loads automatically with SDK
// Or manual initialization:
IVXLanguageManager.Instance.Initialize();
```

### 3. Get Translations

```csharp
// Get localized string
string message = IVXLanguageManager.Instance.GetText("welcome_message");
Debug.Log(message); // "Welcome!" (or translated)
```

---

## CSV File Format

### Structure

```csv
key,en,es,fr,de,ja,ko,zh
```

- **key**: Unique identifier (snake_case recommended)
- **Language codes**: ISO 639-1 codes (en, es, fr, etc.)

### Best Practices

```csv
# ✅ Good keys
main_menu_title,Main Menu,...
btn_confirm,Confirm,...
error_network,Network Error,...

# ❌ Bad keys
MainMenuTitle,...        # Use snake_case
1_button,...              # Don't start with number
this is a key,...         # No spaces
```

---

## Language Codes

| Code | Language |
|------|----------|
| en | English |
| es | Spanish |
| fr | French |
| de | German |
| it | Italian |
| pt | Portuguese |
| ru | Russian |
| ja | Japanese |
| ko | Korean |
| zh | Chinese (Simplified) |
| ar | Arabic |
| hi | Hindi |

---

## Getting Translations

### Basic Usage

```csharp
using IntelliVerseX.Localization;

// Get text by key
string title = IVXLanguageManager.Instance.GetText("main_menu_title");

// With fallback
string text = IVXLanguageManager.Instance.GetText("missing_key", "Default Text");
```

### With Parameters

```csharp
// CSV: greeting,Hello {0}!,¡Hola {0}!,...
string greeting = IVXLanguageManager.Instance.GetText("greeting", playerName);
// Result: "Hello John!"
```

### Multiple Parameters

```csharp
// CSV: score_message,{0} scored {1} points!,...
string message = IVXLanguageManager.Instance.GetText("score_message", 
    playerName, 
    score.ToString()
);
// Result: "John scored 100 points!"
```

---

## Changing Language

### Set Language

```csharp
// Change to Spanish
IVXLanguageManager.Instance.SetLanguage("es");

// Change to Japanese
IVXLanguageManager.Instance.SetLanguage("ja");
```

### Get Current Language

```csharp
string currentLang = IVXLanguageManager.Instance.CurrentLanguage;
Debug.Log($"Current language: {currentLang}");
```

### Language Changed Event

```csharp
IVXLanguageManager.OnLanguageChanged += (newLanguage) =>
{
    Debug.Log($"Language changed to: {newLanguage}");
    RefreshAllUI();
};
```

---

## Auto-Detect Language

```csharp
// Use system language
IVXLanguageManager.Instance.SetLanguageFromSystem();

// Get system language code
string systemLang = IVXLanguageManager.Instance.GetSystemLanguage();
```

---

## UI Components

### LocalizedText Component

Add to any TextMeshPro component:

```csharp
using IntelliVerseX.Localization;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string _localizationKey;
    
    private TMP_Text _text;
    
    void Start()
    {
        _text = GetComponent<TMP_Text>();
        UpdateText();
        
        IVXLanguageManager.OnLanguageChanged += OnLanguageChanged;
    }
    
    void OnDestroy()
    {
        IVXLanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
    
    void OnLanguageChanged(string newLanguage)
    {
        UpdateText();
    }
    
    void UpdateText()
    {
        _text.text = IVXLanguageManager.Instance.GetText(_localizationKey);
    }
}
```

### Usage

1. Add `LocalizedText` component to GameObject with TMP_Text
2. Set `_localizationKey` in Inspector
3. Text updates automatically when language changes

---

## Language Selector UI

```csharp
using IntelliVerseX.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private Dropdown _languageDropdown;
    
    private readonly string[] _languages = { "en", "es", "fr", "de", "ja", "ko", "zh" };
    private readonly string[] _languageNames = 
    { 
        "English", "Español", "Français", "Deutsch", 
        "日本語", "한국어", "中文" 
    };
    
    void Start()
    {
        // Populate dropdown
        _languageDropdown.ClearOptions();
        _languageDropdown.AddOptions(new List<string>(_languageNames));
        
        // Set current selection
        int currentIndex = System.Array.IndexOf(
            _languages, 
            IVXLanguageManager.Instance.CurrentLanguage
        );
        _languageDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        
        // Handle changes
        _languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
    }
    
    void OnLanguageSelected(int index)
    {
        IVXLanguageManager.Instance.SetLanguage(_languages[index]);
    }
}
```

---

## Font Management

### Font Assets per Language

Some languages require different fonts:

```csharp
[System.Serializable]
public class LanguageFont
{
    public string languageCode;
    public TMP_FontAsset font;
}

public class FontManager : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset _defaultFont;
    [SerializeField] private LanguageFont[] _languageFonts;
    
    void Start()
    {
        IVXLanguageManager.OnLanguageChanged += UpdateFont;
        UpdateFont(IVXLanguageManager.Instance.CurrentLanguage);
    }
    
    void UpdateFont(string language)
    {
        var fontConfig = System.Array.Find(_languageFonts, f => f.languageCode == language);
        var font = fontConfig?.font ?? _defaultFont;
        
        // Update all TMP_Text components
        foreach (var text in FindObjectsOfType<TMP_Text>())
        {
            text.font = font;
        }
    }
}
```

### Recommended Fonts

| Language | Font |
|----------|------|
| Latin (en, es, fr, de) | Roboto, Open Sans |
| Japanese | Noto Sans JP |
| Korean | Noto Sans KR |
| Chinese | Noto Sans SC |
| Arabic | Noto Sans Arabic |

---

## Right-to-Left Languages

### Handle RTL

```csharp
void OnLanguageChanged(string language)
{
    bool isRTL = language == "ar" || language == "he";
    
    if (isRTL)
    {
        // Flip UI layout
        SetRTLLayout(true);
    }
    else
    {
        SetRTLLayout(false);
    }
}

void SetRTLLayout(bool rtl)
{
    foreach (var layout in FindObjectsOfType<HorizontalLayoutGroup>())
    {
        layout.reverseArrangement = rtl;
    }
}
```

---

## Pluralization

### Handle Plurals

```csharp
// CSV:
// item_count_0,No items,...
// item_count_1,1 item,...
// item_count_many,{0} items,...

public string GetPluralText(string baseKey, int count)
{
    string key;
    if (count == 0)
        key = $"{baseKey}_0";
    else if (count == 1)
        key = $"{baseKey}_1";
    else
        key = $"{baseKey}_many";
    
    return IVXLanguageManager.Instance.GetText(key, count.ToString());
}

// Usage
string text = GetPluralText("item_count", 5);
// Result: "5 items"
```

---

## Best Practices

### 1. Organize Keys

```csv
# Group by feature
# Menu
menu_play,...
menu_settings,...
menu_quit,...

# Settings
settings_audio,...
settings_graphics,...
settings_language,...

# Gameplay
game_score,...
game_level,...
game_gameover,...
```

### 2. Context in Keys

```csv
# Include context for translators
btn_start,Start,...           # Button text
title_start,Start Game,...    # Title text
msg_start,Ready to start?,... # Message text
```

### 3. Avoid Hardcoded Strings

```csharp
// ❌ Bad
scoreText.text = "Score: " + score;

// ✅ Good
scoreText.text = IVXLanguageManager.Instance.GetText("score_label", score.ToString());
```

### 4. Test All Languages

```csharp
// Debug: Cycle through languages
[ContextMenu("Test All Languages")]
void TestLanguages()
{
    StartCoroutine(CycleLanguages());
}

IEnumerator CycleLanguages()
{
    string[] langs = { "en", "es", "fr", "de", "ja", "ko", "zh" };
    foreach (var lang in langs)
    {
        IVXLanguageManager.Instance.SetLanguage(lang);
        Debug.Log($"Testing: {lang}");
        yield return new WaitForSeconds(2f);
    }
}
```

---

## Troubleshooting

### Translation Not Found

```csharp
// Check if key exists
if (!IVXLanguageManager.Instance.HasKey("my_key"))
{
    Debug.LogWarning("Missing localization key: my_key");
}
```

### CSV Parse Errors

Common issues:
- Missing/extra commas
- Unescaped quotes
- Wrong encoding (use UTF-8)

### Characters Not Displaying

1. Check font supports the character set
2. Verify CSV saved as UTF-8
3. Create font atlas with needed characters

---

## See Also

- [Localization Module](../modules/localization.md)
- [Configuration](../configuration/index.md)
