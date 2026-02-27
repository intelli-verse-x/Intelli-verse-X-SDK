# Localization Module

The Localization module provides comprehensive multi-language support with automatic device language detection and cloud synchronization.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Localization` |
| **Assembly** | `IntelliVerseX.Localization` |
| **Format** | JSON-based language files |

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXLanguageManager` | Main localization interface |
| `IVXLanguageConfig` | Language configuration |
| `IVXLocalizedText` | TextMeshPro localization component |
| `IVXLocalizedImage` | Image localization component |

---

## Quick Start

```csharp
using IntelliVerseX.Localization;

// Get localized string
string welcomeText = IVXLanguageManager.GetString("welcome_message");

// Get with formatting
string greeting = IVXLanguageManager.GetString("hello_player", playerName);
```

---

## IVXLanguageManager

```csharp
public static class IVXLanguageManager
{
    // State
    public static bool IsInitialized { get; }
    public static string CurrentLanguage { get; }
    public static List<string> AvailableLanguages { get; }
    
    // Events
    public static event Action<string> OnLanguageChanged;
    
    // Initialize
    public static void Initialize(IVXLanguageConfig config);
    
    // Language selection
    public static void SetLanguage(string languageCode);
    public static void SetLanguageFromDevice();
    
    // Get strings
    public static string GetString(string key);
    public static string GetString(string key, params object[] args);
    
    // Check availability
    public static bool HasKey(string key);
}
```

---

## Language Files

### File Structure

```
Assets/_IntelliVerseXSDK/Runtime/Localization/Languages/
├── en.json     # English (default)
├── es.json     # Spanish
├── fr.json     # French
├── de.json     # German
├── pt.json     # Portuguese
├── ar.json     # Arabic
├── ja.json     # Japanese
├── ko.json     # Korean
└── zh.json     # Chinese
```

### File Format

```json
{
  "language_name": "English",
  "language_code": "en",
  "strings": {
    "welcome_message": "Welcome to the game!",
    "hello_player": "Hello, {0}!",
    "score_format": "Score: {0:N0}",
    "time_format": "Time: {0}:{1:D2}",
    
    "menu": {
      "play": "Play",
      "settings": "Settings",
      "quit": "Quit"
    },
    
    "errors": {
      "network": "Network error. Please try again.",
      "login_failed": "Login failed: {0}"
    }
  }
}
```

---

## UI Components

### IVXLocalizedText

Automatically localizes TextMeshPro text:

```csharp
// Add to TextMeshProUGUI component
// Set localization key in inspector
```

**Inspector Properties:**
- `Localization Key` - The string key to look up
- `Format Args` - Optional format arguments
- `Auto Update` - Update when language changes

**Code Usage:**
```csharp
[RequireComponent(typeof(TMP_Text))]
public class IVXLocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    
    // Change key at runtime
    public void SetKey(string key)
    {
        localizationKey = key;
        UpdateText();
    }
    
    // Set with arguments
    public void SetKeyWithArgs(string key, params object[] args);
}
```

### IVXLocalizedImage

Localizes images based on language:

```csharp
[RequireComponent(typeof(Image))]
public class IVXLocalizedImage : MonoBehaviour
{
    [SerializeField] private string imageKey;
    
    // Automatically loads correct image from:
    // Resources/Localization/{languageCode}/{imageKey}
}
```

---

## Language Selection

### Automatic Detection

```csharp
// Detect device language on startup
void Start()
{
    IVXLanguageManager.SetLanguageFromDevice();
}
```

### Manual Selection

```csharp
// Language selector UI
public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    
    void Start()
    {
        // Populate dropdown
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(
            IVXLanguageManager.AvailableLanguages
                .Select(code => GetLanguageName(code))
                .ToList()
        );
        
        // Set current selection
        int currentIndex = IVXLanguageManager.AvailableLanguages
            .IndexOf(IVXLanguageManager.CurrentLanguage);
        languageDropdown.value = currentIndex;
        
        // Handle changes
        languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
    }
    
    void OnLanguageSelected(int index)
    {
        string code = IVXLanguageManager.AvailableLanguages[index];
        IVXLanguageManager.SetLanguage(code);
    }
}
```

---

## String Formatting

### Basic Substitution

```json
{
  "hello_player": "Hello, {0}!"
}
```

```csharp
// Output: "Hello, John!"
IVXLanguageManager.GetString("hello_player", "John");
```

### Number Formatting

```json
{
  "score": "Score: {0:N0}",
  "percentage": "{0:P0}",
  "currency": "${0:F2}"
}
```

```csharp
// Output: "Score: 1,234,567"
IVXLanguageManager.GetString("score", 1234567);

// Output: "50%"
IVXLanguageManager.GetString("percentage", 0.5f);
```

### Pluralization

```json
{
  "coins_one": "{0} coin",
  "coins_other": "{0} coins"
}
```

```csharp
string coinText = coins == 1 
    ? IVXLanguageManager.GetString("coins_one", coins)
    : IVXLanguageManager.GetString("coins_other", coins);
```

---

## RTL Support

For right-to-left languages (Arabic, Hebrew):

```csharp
// Check if current language is RTL
bool isRTL = IVXLanguageManager.IsRTL;

// UI auto-adjusts when using IVXLocalizedText
```

**RTL-Aware UI:**
```csharp
public class RTLAwarePanel : MonoBehaviour
{
    void Start()
    {
        IVXLanguageManager.OnLanguageChanged += OnLanguageChanged;
        UpdateLayout();
    }
    
    void OnLanguageChanged(string newLanguage)
    {
        UpdateLayout();
    }
    
    void UpdateLayout()
    {
        var layout = GetComponent<HorizontalLayoutGroup>();
        if (IVXLanguageManager.IsRTL)
        {
            layout.reverseArrangement = true;
            layout.childAlignment = TextAnchor.MiddleRight;
        }
        else
        {
            layout.reverseArrangement = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
        }
    }
}
```

---

## Configuration

### IVXLanguageConfig

```csharp
[CreateAssetMenu(fileName = "LanguageConfig", menuName = "IntelliVerse-X/Language Config")]
public class IVXLanguageConfig : ScriptableObject
{
    [Header("Settings")]
    public string defaultLanguage = "en";
    public bool detectDeviceLanguage = true;
    public bool saveLanguagePreference = true;
    
    [Header("Available Languages")]
    public List<LanguageInfo> languages;
    
    [Header("Fallback")]
    public string fallbackLanguage = "en";
    public bool showKeyOnMissing = false; // Show key if translation missing
}

[Serializable]
public class LanguageInfo
{
    public string code;        // "en", "es", "fr"
    public string nativeName;  // "English", "Español", "Français"
    public bool isRTL;         // Right-to-left
    public Sprite flagIcon;
}
```

---

## Supported Languages

| Code | Language | Native Name | RTL |
|------|----------|-------------|-----|
| `en` | English | English | No |
| `es` | Spanish | Español | No |
| `fr` | French | Français | No |
| `de` | German | Deutsch | No |
| `pt` | Portuguese | Português | No |
| `it` | Italian | Italiano | No |
| `ru` | Russian | Русский | No |
| `ja` | Japanese | 日本語 | No |
| `ko` | Korean | 한국어 | No |
| `zh` | Chinese | 中文 | No |
| `ar` | Arabic | العربية | Yes |
| `he` | Hebrew | עברית | Yes |

---

## Adding New Language

1. Create language JSON file:

```json
// Assets/_IntelliVerseXSDK/Runtime/Localization/Languages/xx.json
{
  "language_name": "New Language",
  "language_code": "xx",
  "strings": {
    // Copy keys from en.json and translate
  }
}
```

2. Register in config:

```csharp
config.languages.Add(new LanguageInfo
{
    code = "xx",
    nativeName = "New Language",
    isRTL = false
});
```

---

## Best Practices

### 1. Key Naming

```json
{
  // Use hierarchical keys
  "menu.settings.title": "Settings",
  "menu.settings.sound": "Sound",
  
  // Or nested objects
  "menu": {
    "settings": {
      "title": "Settings",
      "sound": "Sound"
    }
  }
}
```

### 2. Context for Translators

```json
{
  "buy_button": "Buy",
  "_buy_button_context": "Button text for purchasing items"
}
```

### 3. Handle Missing Keys

```csharp
string text = IVXLanguageManager.GetString("unknown_key");
// Returns key name if not found (with config option)
// Or returns empty string
```

### 4. Async Loading

```csharp
// Language files load asynchronously
await IVXLanguageManager.InitializeAsync();
// Or use callback
IVXLanguageManager.Initialize(config, onComplete: () =>
{
    // Languages loaded
});
```

---

## Related Documentation

- [Localization Demo](../samples/localization-demo.md) - Sample implementation
- [Localization Setup Guide](../guides/localization-setup.md) - Adding languages
