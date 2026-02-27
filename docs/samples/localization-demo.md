# Localization Demo

This sample demonstrates multi-language support implementation.

## Features

- RTL language support
- Dynamic language switching
- Fallback languages
- TMPro integration

## Setup

1. Add localization files to `Resources/Localization`
2. Configure supported languages
3. Use `IVXLocalizer` for translations

## Code Example

```csharp
using IntelliVerseX.Localization;

// Get translated text
string welcomeText = IVXLocalizer.GetString("welcome_message");

// Change language
IVXLocalizer.SetLanguage("ar"); // Arabic
```

## See Also

- [Localization Module](../modules/localization.md)
- [Localization Setup Guide](../guides/localization-setup.md)
