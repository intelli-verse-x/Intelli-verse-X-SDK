# Localization API Reference

Complete API reference for the Localization module.

---

## IVXLanguageManager

Multi-language localization manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXLanguageManager` | Singleton instance |
| `CurrentLanguage` | `string` | Current language code |
| `AvailableLanguages` | `List<string>` | Supported languages |
| `IsInitialized` | `bool` | Initialization status |

---

### Methods

#### GetText

```csharp
public string GetText(string key)
```

Gets localized text by key.

**Returns:** Localized string or key if not found

**Example:**
```csharp
string title = IVXLanguageManager.Instance.GetText("main_menu_title");
```

---

#### GetText (with default)

```csharp
public string GetText(string key, string defaultValue)
```

Gets localized text with fallback.

---

#### GetText (with parameters)

```csharp
public string GetText(string key, params object[] args)
```

Gets localized text with format parameters.

**Example:**
```csharp
// CSV: greeting,Hello {0}!
string text = IVXLanguageManager.Instance.GetText("greeting", playerName);
// Result: "Hello John!"
```

---

#### SetLanguage

```csharp
public void SetLanguage(string languageCode)
```

Changes the current language.

**Parameters:**
- `languageCode` - ISO 639-1 code (e.g., "en", "es", "ja")

**Example:**
```csharp
IVXLanguageManager.Instance.SetLanguage("es"); // Spanish
```

---

#### SetLanguageFromSystem

```csharp
public void SetLanguageFromSystem()
```

Sets language based on device settings.

---

#### GetSystemLanguage

```csharp
public string GetSystemLanguage()
```

Gets the device's language code.

---

#### HasKey

```csharp
public bool HasKey(string key)
```

Checks if a localization key exists.

---

#### ReloadLocalizations

```csharp
public void ReloadLocalizations()
```

Reloads localization data from files.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnLanguageChanged` | `Action<string>` | Language changed |
| `OnLocalizationsLoaded` | `Action` | Data reloaded |

**Example:**
```csharp
IVXLanguageManager.OnLanguageChanged += (lang) =>
{
    Debug.Log($"Language changed to: {lang}");
    RefreshAllText();
};
```

---

## Language Codes

| Code | Language |
|------|----------|
| `en` | English |
| `es` | Spanish |
| `fr` | French |
| `de` | German |
| `it` | Italian |
| `pt` | Portuguese |
| `ru` | Russian |
| `ja` | Japanese |
| `ko` | Korean |
| `zh` | Chinese (Simplified) |
| `ar` | Arabic |

---

## CSV Format

```csv
key,en,es,fr,de
welcome,Welcome!,¡Bienvenido!,Bienvenue!,Willkommen!
play,Play,Jugar,Jouer,Spielen
settings,Settings,Configuración,Paramètres,Einstellungen
```

---

## See Also

- [Localization Module Guide](../modules/localization.md)
- [Localization Setup Guide](../guides/localization-setup.md)
