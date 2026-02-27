# Core Module

The Core module provides foundation utilities used by all other IntelliVerseX SDK modules.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Core` |
| **Assembly** | `IntelliVerseX.Core` |
| **Dependencies** | None |

---

## Key Classes

### IntelliVerseXConfig

ScriptableObject for game configuration.

```csharp
// Create via menu: Assets > Create > IntelliVerse-X > Game Configuration

[CreateAssetMenu(fileName = "GameConfig", menuName = "IntelliVerse-X/Game Configuration")]
public class IntelliVerseXConfig : ScriptableObject
{
    // Game identity
    public string gameId;
    public string gameName;
    
    // Backend
    public bool useSharedBackend;
    public string nakamaScheme;
    public string nakamaHost;
    public int nakamaPort;
    public string nakamaServerKey;
    
    // Features
    public bool enableGuestAccounts;
    public bool enableAutoLogin;
    public bool enableLeaderboards;
    public bool enableWallets;
    public bool enableAds;
    public bool enableIAP;
    public bool enableMultiplayer;
    
    // Localization
    public SystemLanguage defaultLanguage;
    public SystemLanguage[] supportedLanguages;
    
    // Debug
    public bool enableDebugLogs;
}
```

**Usage:**
```csharp
// Load config from Resources
var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/GameConfig");

// Validate configuration
if (config.IsValid())
{
    Debug.Log($"Game: {config.gameName} ({config.gameId})");
}
```

---

### IVXLogger

Static logging utility with log level support.

```csharp
public static class IVXLogger
{
    public static bool EnableDebugLogs = true;
    
    public static void Log(string message, Object context = null);
    public static void LogWarning(string message, Object context = null);
    public static void LogError(string message, Object context = null);
    public static void LogException(Exception exception, Object context = null);
}
```

**Usage:**
```csharp
// Enable/disable debug logs
IVXLogger.EnableDebugLogs = true;

// Log messages
IVXLogger.Log("Initialized successfully");
IVXLogger.LogWarning("Connection slow");
IVXLogger.LogError("Failed to authenticate");

try
{
    // risky operation
}
catch (Exception ex)
{
    IVXLogger.LogException(ex);
}
```

**Output format:**
```
[IVX] Initialized successfully
[IVX] Connection slow
[IVX] Failed to authenticate
```

---

### IVXSafeSingleton<T>

Thread-safe singleton base class for MonoBehaviours.

```csharp
public abstract class IVXSafeSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; }
    public static bool HasInstance { get; }
    
    protected virtual void OnSingletonAwake() { }
    protected virtual void OnSingletonDestroy() { }
}
```

**Usage:**
```csharp
public class GameManager : IVXSafeSingleton<GameManager>
{
    public int Score { get; private set; }
    
    protected override void OnSingletonAwake()
    {
        // Called once when singleton initializes
        Score = 0;
    }
    
    public void AddScore(int points)
    {
        Score += points;
    }
}

// Access from anywhere
GameManager.Instance.AddScore(100);
```

---

### IVXSceneManager

Scene loading utility with transition support.

```csharp
public class IVXSceneManager : MonoBehaviour
{
    public static IVXSceneManager Instance { get; }
    
    public void LoadScene(string sceneName);
    public void LoadSceneAsync(string sceneName, Action onComplete = null);
    public void ReloadCurrentScene();
}
```

**Usage:**
```csharp
// Simple scene load
IVXSceneManager.Instance.LoadScene("MainMenu");

// Async load with callback
IVXSceneManager.Instance.LoadSceneAsync("GameLevel", () =>
{
    Debug.Log("Scene loaded!");
});

// Reload current scene
IVXSceneManager.Instance.ReloadCurrentScene();
```

---

### IVXAudioManager

Audio playback manager.

```csharp
public class IVXAudioManager : MonoBehaviour
{
    public static IVXAudioManager Instance { get; }
    
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SFXVolume { get; set; }
    
    public void PlayMusic(AudioClip clip, bool loop = true);
    public void PlaySFX(AudioClip clip);
    public void StopMusic();
    public void PauseMusic();
    public void ResumeMusic();
}
```

---

### IVXUtilities

Common utility functions.

```csharp
public static class IVXUtilities
{
    // Time formatting
    public static string FormatTime(float seconds);
    public static string FormatTimeRemaining(DateTime endTime);
    
    // Number formatting
    public static string FormatNumber(long number);
    public static string FormatCurrency(decimal amount, string currency);
    
    // String utilities
    public static string Truncate(string value, int maxLength);
    public static bool IsValidEmail(string email);
    
    // Platform detection
    public static bool IsMobile { get; }
    public static bool IsEditor { get; }
}
```

**Usage:**
```csharp
// Format time
string time = IVXUtilities.FormatTime(125.5f); // "2:05"

// Format large numbers
string score = IVXUtilities.FormatNumber(1500000); // "1.5M"

// Validate email
if (IVXUtilities.IsValidEmail(email))
{
    // proceed with registration
}

// Platform check
if (IVXUtilities.IsMobile)
{
    // show mobile UI
}
```

---

### IVXEmojiTextUtility

TextMeshPro emoji support.

```csharp
public static class IVXEmojiTextUtility
{
    public static string ConvertToTMPEmoji(string text);
    public static bool HasEmoji(string text);
}
```

**Usage:**
```csharp
// Convert emoji characters to TMP sprite format
string displayText = IVXEmojiTextUtility.ConvertToTMPEmoji("Hello 😀");
textMeshPro.text = displayText;
```

---

## Configuration Classes

### IVXNakamaConfig

Nakama server configuration constants.

```csharp
public static class IVXNakamaConfig
{
    public const string PROTOCOL = "https";
    public const string HOST = "nakama-rest.intelli-verse-x.ai";
    public const int PORT = 443;
    public const string SERVER_KEY = "defaultkey";
}
```

### IVXAdsConfig

Ads configuration.

```csharp
[Serializable]
public class IVXAdsConfig
{
    public bool enableAds;
    public string androidAppId;
    public string iosAppId;
    public string rewardedAdUnitId;
    public string interstitialAdUnitId;
    public string bannerAdUnitId;
}
```

### IVXPhotonConfig

Photon multiplayer configuration.

```csharp
public static class IVXPhotonConfig
{
    public const string APP_ID = "fa2f730e-1c81-4d01-b11f-708680dcaf37";
    public const string REGION = "us";
}
```

---

## Best Practices

### Logging

```csharp
// DO: Use IVXLogger for SDK-related logs
IVXLogger.Log("User authenticated");

// DON'T: Use Debug.Log directly (bypasses log filtering)
Debug.Log("User authenticated"); // Avoid
```

### Configuration Loading

```csharp
// DO: Load once and cache
private IntelliVerseXConfig _config;

void Awake()
{
    _config = Resources.Load<IntelliVerseXConfig>("IntelliVerseX/GameConfig");
}

// DON'T: Load repeatedly
void Update()
{
    var config = Resources.Load<IntelliVerseXConfig>("..."); // Avoid
}
```

### Singleton Access

```csharp
// DO: Check if instance exists before accessing
if (IVXSceneManager.Instance != null)
{
    IVXSceneManager.Instance.LoadScene("Menu");
}

// OR use null-conditional
IVXSceneManager.Instance?.LoadScene("Menu");
```

---

## Related Documentation

- [Identity Module](identity.md) - Uses Core utilities
- [Backend Module](backend.md) - Extends Core configuration
- [Configuration Guide](../configuration/index.md) - Full configuration reference
