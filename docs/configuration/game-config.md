# Game Configuration

The `IntelliVerseXConfig` ScriptableObject controls core SDK behavior and game identification.

---

## Creating the Config

**Menu:** `Assets → Create → IntelliVerse-X → IntelliVerseX Config`

Place the created asset in `Assets/Resources/IntelliVerseXConfig.asset` for automatic loading.

---

## Configuration Reference

### IntelliVerseXConfig

```csharp
[CreateAssetMenu(fileName = "IntelliVerseXConfig", menuName = "IntelliVerse-X/IntelliVerseX Config")]
public class IntelliVerseXConfig : ScriptableObject
{
    [Header("Game Identification")]
    [Tooltip("Unique identifier for your game")]
    public string gameId;
    
    [Tooltip("Game version (synced with Application.version)")]
    public string gameVersion;
    
    [Tooltip("Display name of the game")]
    public string gameName;
    
    [Header("Backend")]
    [Tooltip("Nakama server URL")]
    public string backendUrl = "nakama-rest.intelli-verse-x.ai";
    
    [Tooltip("Nakama server port")]
    public int backendPort = 443;
    
    [Tooltip("Nakama server key")]
    public string serverKey = "defaultkey";
    
    [Tooltip("Use SSL/TLS connection")]
    public bool useSSL = true;
    
    [Header("Features")]
    [Tooltip("Enable SDK debug logging")]
    public bool enableLogging = true;
    
    [Tooltip("Log level (Verbose, Debug, Info, Warning, Error)")]
    public LogLevel logLevel = LogLevel.Info;
    
    [Tooltip("Enable analytics tracking")]
    public bool enableAnalytics = true;
    
    [Tooltip("Enable crash reporting")]
    public bool enableCrashReporting = true;
    
    [Header("Session")]
    [Tooltip("Auto-connect to backend on start")]
    public bool autoConnect = true;
    
    [Tooltip("Session timeout in seconds")]
    public float sessionTimeout = 300f;
    
    [Tooltip("Reconnect automatically on disconnect")]
    public bool autoReconnect = true;
    
    [Tooltip("Maximum reconnection attempts")]
    public int maxReconnectAttempts = 5;
    
    [Header("Storage")]
    [Tooltip("Enable data encryption")]
    public bool encryptLocalData = true;
    
    [Tooltip("Enable cloud sync")]
    public bool enableCloudSync = true;
    
    [Header("Platform")]
    [Tooltip("Supported platforms")]
    public PlatformFlags supportedPlatforms;
}

[Flags]
public enum PlatformFlags
{
    None = 0,
    Android = 1 << 0,
    iOS = 1 << 1,
    WebGL = 1 << 2,
    Windows = 1 << 3,
    macOS = 1 << 4,
    Linux = 1 << 5,
    All = ~0
}
```

---

## Inspector View

```
┌─────────────────────────────────────────────────────────────┐
│ IntelliVerseX Config                                        │
├─────────────────────────────────────────────────────────────┤
│ ▼ Game Identification                                       │
│   Game Id         [my-awesome-game                    ]     │
│   Game Version    [1.0.0                              ]     │
│   Game Name       [My Awesome Game                    ]     │
│                                                             │
│ ▼ Backend                                                   │
│   Backend Url     [nakama-rest.intelli-verse-x.ai    ]     │
│   Backend Port    [443                                ]     │
│   Server Key      [defaultkey                         ]     │
│   Use SSL         [✓]                                       │
│                                                             │
│ ▼ Features                                                  │
│   Enable Logging        [✓]                                 │
│   Log Level             [Info                         ▼]    │
│   Enable Analytics      [✓]                                 │
│   Enable Crash Report   [✓]                                 │
│                                                             │
│ ▼ Session                                                   │
│   Auto Connect          [✓]                                 │
│   Session Timeout       [300                          ]     │
│   Auto Reconnect        [✓]                                 │
│   Max Reconnect         [5                            ]     │
│                                                             │
│ ▼ Storage                                                   │
│   Encrypt Local Data    [✓]                                 │
│   Enable Cloud Sync     [✓]                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Loading Configuration

### Automatic Loading

Place config at `Resources/IntelliVerseXConfig`:

```csharp
// SDK automatically loads from Resources
IntelliVerseXSDK.Initialize();
```

### Manual Loading

```csharp
// Load specific config
var config = Resources.Load<IntelliVerseXConfig>("MyConfig");
IntelliVerseXSDK.Initialize(config);

// Or create at runtime
var config = ScriptableObject.CreateInstance<IntelliVerseXConfig>();
config.gameId = "my-game";
config.backendUrl = "my-server.com";
IntelliVerseXSDK.Initialize(config);
```

---

## Environment-Specific Configs

Create separate configs for different environments:

```
Resources/
├── Config/
│   ├── IntelliVerseXConfig_Dev.asset
│   ├── IntelliVerseXConfig_Staging.asset
│   └── IntelliVerseXConfig_Prod.asset
```

```csharp
// Load based on environment
string configPath = GetEnvironmentConfigPath();
var config = Resources.Load<IntelliVerseXConfig>(configPath);
IntelliVerseXSDK.Initialize(config);

string GetEnvironmentConfigPath()
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    return "Config/IntelliVerseXConfig_Dev";
#elif STAGING
    return "Config/IntelliVerseXConfig_Staging";
#else
    return "Config/IntelliVerseXConfig_Prod";
#endif
}
```

---

## Accessing Configuration

```csharp
// Get current config
var config = IntelliVerseXSDK.Config;

// Access values
Debug.Log($"Game: {config.gameName}");
Debug.Log($"Backend: {config.backendUrl}:{config.backendPort}");
Debug.Log($"Logging: {config.enableLogging}");
```

---

## Configuration Validation

The SDK validates configuration on initialization:

```csharp
void OnValidate()
{
    // Called in editor when values change
    if (string.IsNullOrEmpty(gameId))
    {
        Debug.LogWarning("Game ID is required");
    }
    
    if (backendPort <= 0 || backendPort > 65535)
    {
        Debug.LogError("Invalid backend port");
    }
    
    if (sessionTimeout < 60)
    {
        Debug.LogWarning("Session timeout less than 60 seconds not recommended");
    }
}
```

---

## Best Practices

### 1. Use Consistent Game ID

```csharp
// Use reverse domain notation
config.gameId = "com.yourstudio.gamename";
```

### 2. Match Application Version

```csharp
// Keep in sync with Player Settings
config.gameVersion = Application.version;
```

### 3. Secure Production Settings

```csharp
// Production config should have:
config.enableLogging = false;           // Disable verbose logs
config.autoReconnect = true;            // Enable resilience
config.encryptLocalData = true;         // Secure storage
config.enableCrashReporting = true;     // Track issues
```

### 4. Don't Include Secrets in Config

```csharp
// ❌ Don't store API keys in ScriptableObjects
public string secretApiKey;

// ✅ Use secure storage or environment
public string GetApiKey() => IVXSecureStorage.GetString("api_key");
```

---

## Related Configuration

- [Backend Config](backend-config.md) - Detailed backend settings
- [Ads Config](ads-config.md) - Advertising configuration
- [Feature Flags](feature-flags.md) - Feature toggles
