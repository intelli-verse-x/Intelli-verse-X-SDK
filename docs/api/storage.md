# Storage API Reference

The Storage module provides persistent data management for your game.

## Namespace

```csharp
using IntelliVerseX.Storage;
```

## IVXStorageManager

Main class for data persistence operations.

### Methods

| Method | Description |
|--------|-------------|
| `Save<T>(string key, T data)` | Saves data with a key |
| `Load<T>(string key)` | Loads data by key |
| `Delete(string key)` | Deletes data by key |
| `Exists(string key)` | Checks if key exists |
| `Clear()` | Clears all stored data |

### Example

```csharp
// Save player progress
var progress = new PlayerProgress { level = 5, score = 1200 };
IVXStorageManager.Instance.Save("progress", progress);

// Load player progress
var loaded = IVXStorageManager.Instance.Load<PlayerProgress>("progress");
```

## See Also

- [Storage Module](../modules/storage.md)
