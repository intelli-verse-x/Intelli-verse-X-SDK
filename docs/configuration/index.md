# Configuration

This section covers all configuration options for the IntelliVerseX SDK.

---

## Configuration Files

| File | Purpose | Location |
|------|---------|----------|
| `IntelliVerseXConfig` | Main SDK configuration | `Resources/` |
| `GameConfig` | Game-specific settings | `Resources/` |
| `BackendConfig` | Nakama connection settings | `Resources/` |
| `AdsConfig` | Ad network configuration | `Resources/` |

---

## Quick Links

<div class="grid cards" markdown>

-   :material-cog:{ .lg .middle } **Game Config**

    ---

    Core game settings, feature flags, and identification

    [:octicons-arrow-right-24: Game Config](game-config.md)

-   :material-server:{ .lg .middle } **Backend Config**

    ---

    Nakama server connection and authentication

    [:octicons-arrow-right-24: Backend Config](backend-config.md)

-   :material-advertisements:{ .lg .middle } **Ads Config**

    ---

    Advertising network setup and ad unit IDs

    [:octicons-arrow-right-24: Ads Config](ads-config.md)

-   :material-toggle-switch:{ .lg .middle } **Feature Flags**

    ---

    Enable/disable SDK features at runtime

    [:octicons-arrow-right-24: Feature Flags](feature-flags.md)

</div>

---

## Configuration Overview

### Creating Configuration Assets

1. **Menu:** `Assets → Create → IntelliVerse-X → Configuration`
2. Select the configuration type to create
3. Place in `Resources/` folder for auto-loading

### Loading Configuration

Configuration is automatically loaded on SDK initialization:

```csharp
// Automatic loading
IntelliVerseXSDK.Initialize(); // Loads from Resources

// Or manual loading
var config = Resources.Load<IntelliVerseXConfig>("IntelliVerseXConfig");
IntelliVerseXSDK.Initialize(config);
```

---

## Environment Configuration

### Development vs Production

```csharp
[CreateAssetMenu(menuName = "IntelliVerse-X/Environment Config")]
public class EnvironmentConfig : ScriptableObject
{
    [Header("Environment")]
    public Environment currentEnvironment = Environment.Development;
    
    [Header("Development")]
    public string devBackendUrl; // Prefer loading from config/keys.json (key: devBackendUrl). See config/keys.example.json.
    public bool devEnableLogging = true;
    public bool devUseTestAds = true;
    
    [Header("Production")]
    public string prodBackendUrl = "nakama-rest.intelli-verse-x.ai:443";
    public bool prodEnableLogging = false;
    public bool prodUseTestAds = false;
}

public enum Environment
{
    Development,
    Staging,
    Production
}
```

### Switching Environments

```csharp
// In code
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    IntelliVerseXSDK.SetEnvironment(Environment.Development);
#else
    IntelliVerseXSDK.SetEnvironment(Environment.Production);
#endif
```

---

## Platform-Specific Configuration

Some settings vary by platform:

```csharp
[Serializable]
public class PlatformSettings
{
    [Header("Android")]
    public string androidAppId;
    public string androidAdUnitId;
    
    [Header("iOS")]  
    public string iosAppId;
    public string iosAdUnitId;
    
    [Header("WebGL")]
    public string webglBackendUrl;
}
```

---

## Runtime Configuration

### Remote Config

Settings can be overridden from the server:

```csharp
// Fetch remote config
await IVXRemoteConfig.FetchAsync();

// Get values
int maxRetries = IVXRemoteConfig.GetInt("max_retries", defaultValue: 3);
bool featureEnabled = IVXRemoteConfig.GetBool("new_feature_enabled", defaultValue: false);
```

### Feature Flags at Runtime

```csharp
// Check feature flags
if (IVXFeatureFlags.IsEnabled("weekly_quiz"))
{
    ShowWeeklyQuizButton();
}

// Update flags from server
await IVXFeatureFlags.RefreshAsync();
```

---

## Configuration Best Practices

### 1. Keep Secrets Secure

All sensitive values (API keys, auth URLs, backend URLs) live in a **single common file** so they are not hardcoded. Copy `config/keys.example.json` to `config/keys.json`, fill in values, and do not commit `config/keys.json`. See [config/README.md](../../config/README.md).

```csharp
// ❌ Don't hardcode secrets
public string apiKey = "sk_live_xxxx";

// ✅ Load from the common config file (config/keys.json)
// Unity: use IVXSecretsLoader.Load() or read config/keys.json from project root (see AuthService).
// Or use environment variables: Environment.GetEnvironmentVariable("API_KEY");
```

### 2. Use ScriptableObjects

```csharp
// ✅ ScriptableObjects are Unity-friendly
[CreateAssetMenu(menuName = "IntelliVerse-X/My Config")]
public class MyConfig : ScriptableObject { }
```

### 3. Validate Configuration

```csharp
void OnValidate()
{
    // Validate config in editor
    if (string.IsNullOrEmpty(backendUrl))
    {
        Debug.LogError("Backend URL is required!");
    }
}
```

---

## Next Steps

- [Game Config](game-config.md) - Configure core game settings
- [Backend Config](backend-config.md) - Set up server connection
- [Ads Config](ads-config.md) - Configure advertising
- [Feature Flags](feature-flags.md) - Toggle features
