# Core API Reference

Complete API reference for the Core module.

---

## IntelliVerseXSDK

Main SDK entry point and orchestrator.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IntelliVerseXSDK` | Singleton instance |
| `IsInitialized` | `bool` | SDK initialization status |
| `Config` | `IntelliVerseXConfig` | Configuration asset |
| `Version` | `string` | SDK version string |

### Methods

#### InitializeAsync

```csharp
public async Task InitializeAsync()
```

Initializes the SDK and all enabled modules.

**Returns:** `Task` - Completes when initialization is done

**Example:**
```csharp
await IntelliVerseXSDK.Instance.InitializeAsync();
```

---

#### GetModule<T>

```csharp
public T GetModule<T>() where T : IIVXModule
```

Gets a module instance by type.

**Type Parameters:**
- `T` - Module type implementing `IIVXModule`

**Returns:** Module instance or `null` if not enabled

**Example:**
```csharp
var ads = IntelliVerseXSDK.Instance.GetModule<IVXAdsManager>();
```

---

#### IsFeatureEnabled

```csharp
public bool IsFeatureEnabled(string featureName)
```

Checks if a feature is enabled.

**Parameters:**
- `featureName` - Feature identifier

**Returns:** `true` if enabled

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnInitialized` | `Action` | Fired when SDK initializes |
| `OnInitializationFailed` | `Action<Exception>` | Fired on init failure |

---

## IVXLogger

Centralized logging system.

### Methods

#### Log

```csharp
public static void Log(string message, LogLevel level = LogLevel.Info)
```

Logs a message at the specified level.

**Parameters:**
- `message` - Log message
- `level` - Log level (Debug, Info, Warning, Error)

**Example:**
```csharp
IVXLogger.Log("Player logged in", LogLevel.Info);
IVXLogger.Log("Connection retry", LogLevel.Debug);
```

---

#### SetLogLevel

```csharp
public static void SetLogLevel(LogLevel level)
```

Sets minimum log level for output.

**Parameters:**
- `level` - Minimum level to display

---

### Log Levels

| Level | Value | Description |
|-------|-------|-------------|
| `Debug` | 0 | Verbose debugging |
| `Info` | 1 | General information |
| `Warning` | 2 | Potential issues |
| `Error` | 3 | Errors |
| `None` | 4 | Disable logging |

---

## IVXSafeSingleton<T>

Thread-safe singleton base class.

### Usage

```csharp
public class MyManager : IVXSafeSingleton<MyManager>
{
    protected override void Initialize()
    {
        // One-time initialization
    }
}

// Access
MyManager.Instance.DoSomething();
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `T` | Singleton instance |
| `IsInitialized` | `bool` | Initialization status |

---

## IntelliVerseXConfig

ScriptableObject holding SDK configuration.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `GameName` | `string` | Game display name |
| `GameId` | `string` | Unique game identifier |
| `LogLevel` | `LogLevel` | Logging verbosity |
| `DebugMode` | `bool` | Enable debug features |

### Module Flags

| Property | Type | Default |
|----------|------|---------|
| `EnableBackend` | `bool` | `true` |
| `EnableIdentity` | `bool` | `true` |
| `EnableAnalytics` | `bool` | `true` |
| `EnableAds` | `bool` | `true` |
| `EnableIAP` | `bool` | `true` |
| `EnableQuiz` | `bool` | `false` |
| `EnableFriends` | `bool` | `false` |

---

## IIVXModule

Interface for SDK modules.

### Methods

```csharp
public interface IIVXModule
{
    Task InitializeAsync();
    void Shutdown();
    bool IsInitialized { get; }
}
```

---

## IVXCoroutineRunner

MonoBehaviour for running coroutines from non-MonoBehaviour classes.

### Methods

#### Run

```csharp
public static Coroutine Run(IEnumerator routine)
```

Starts a coroutine.

**Example:**
```csharp
IVXCoroutineRunner.Run(MyCoroutine());
```

---

#### RunDelayed

```csharp
public static void RunDelayed(Action action, float delay)
```

Runs an action after a delay.

**Example:**
```csharp
IVXCoroutineRunner.RunDelayed(() => Debug.Log("Delayed!"), 2f);
```

---

## Utility Classes

### IVXExtensions

Extension methods for common operations.

```csharp
// String extensions
"hello".IsNullOrEmpty()  // false
"".IsNullOrEmpty()       // true

// Collection extensions
list.IsNullOrEmpty()     // Check if null or empty
list.ToJson()            // Serialize to JSON
```

---

### IVXJsonUtility

JSON serialization helpers.

```csharp
// Serialize
string json = IVXJsonUtility.ToJson(myObject);

// Deserialize
MyClass obj = IVXJsonUtility.FromJson<MyClass>(json);

// Pretty print
string pretty = IVXJsonUtility.ToJsonPretty(myObject);
```

---

## See Also

- [Core Module Guide](../modules/core.md)
- [Configuration](../configuration/index.md)
