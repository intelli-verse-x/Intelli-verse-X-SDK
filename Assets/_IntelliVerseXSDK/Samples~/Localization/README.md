# Localization Sample

This sample demonstrates the multi-language support features of the IntelliVerseX SDK.

## Features Demonstrated

- Dynamic language switching
- RTL (Right-to-Left) layout support
- Localized string loading
- Localized asset loading
- Language persistence

## Supported Languages

The SDK supports 12+ languages out of the box:
- English (en)
- Spanish (es)
- French (fr)
- German (de)
- Italian (it)
- Portuguese (pt)
- Russian (ru)
- Japanese (ja)
- Korean (ko)
- Chinese Simplified (zh-CN)
- Chinese Traditional (zh-TW)
- Arabic (ar) - RTL
- Hebrew (he) - RTL

## Setup

1. Import this sample via Package Manager
2. Open the `LocalizationDemoScene` scene
3. Press Play
4. Use the language selector dropdown to switch languages

## Key Scripts

### Using Localized Strings
```csharp
using IntelliVerseX.Localization;

// Get localized string
string text = IVXLocalization.Get("menu.start_button");

// Get with parameters
string greeting = IVXLocalization.Get("greeting", playerName);

// Change language
IVXLocalization.SetLanguage("es");

// Get current language
string currentLang = IVXLocalization.CurrentLanguage;
```

### RTL Support
```csharp
using IntelliVerseX.Localization;

// Check if current language is RTL
bool isRTL = IVXLocalization.IsRTL;

// The SDK automatically handles:
// - Text alignment
// - Layout direction
// - UI mirroring
```

## Localization Files

Create localization files in `Resources/Localization/`:
```
Resources/
└── Localization/
    ├── en.json
    ├── es.json
    ├── ar.json
    └── ...
```

### File Format
```json
{
  "menu": {
    "start_button": "Start Game",
    "settings": "Settings",
    "quit": "Quit"
  },
  "game": {
    "score": "Score: {0}",
    "time_remaining": "Time: {0}s"
  }
}
```

## Dependencies

- IntelliVerseX.Core
- IntelliVerseX.Localization
- TextMeshPro (for RTL text support)
