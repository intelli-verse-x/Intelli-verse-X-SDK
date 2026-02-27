# Storage Module

The Storage module provides secure local storage with optional cloud synchronization through the Nakama backend.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Storage` |
| **Assembly** | `IntelliVerseX.Storage` |
| **Backend** | Local storage + Nakama cloud sync |

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXSecureStorage` | Encrypted local storage |
| `IVXCloudStorage` | Backend-synced storage |
| `IVXStorageManager` | Unified storage interface |
| `IVXSaveSystem` | Game save management |

---

## IVXSecureStorage

Encrypted local storage for sensitive data.

```csharp
public static class IVXSecureStorage
{
    // Save data
    public static void SetString(string key, string value);
    public static void SetInt(string key, int value);
    public static void SetFloat(string key, float value);
    public static void SetBool(string key, bool value);
    public static void SetObject<T>(string key, T obj);
    
    // Load data
    public static string GetString(string key, string defaultValue = "");
    public static int GetInt(string key, int defaultValue = 0);
    public static float GetFloat(string key, float defaultValue = 0f);
    public static bool GetBool(string key, bool defaultValue = false);
    public static T GetObject<T>(string key, T defaultValue = default);
    
    // Management
    public static bool HasKey(string key);
    public static void DeleteKey(string key);
    public static void DeleteAll();
}
```

### Usage

```csharp
using IntelliVerseX.Storage;

// Save player preferences
IVXSecureStorage.SetString("player_name", "Aria");
IVXSecureStorage.SetInt("high_score", 50000);
IVXSecureStorage.SetBool("music_enabled", true);

// Save complex objects
var settings = new GameSettings
{
    SoundVolume = 0.8f,
    MusicVolume = 0.5f,
    Difficulty = "Hard"
};
IVXSecureStorage.SetObject("game_settings", settings);

// Load data
string name = IVXSecureStorage.GetString("player_name", "Guest");
int score = IVXSecureStorage.GetInt("high_score", 0);
var loadedSettings = IVXSecureStorage.GetObject<GameSettings>("game_settings");
```

---

## IVXCloudStorage

Server-synced storage using Nakama.

```csharp
public static class IVXCloudStorage
{
    // Events
    public static event Action<string> OnSyncComplete;
    public static event Action<string, Exception> OnSyncFailed;
    
    // Async operations
    public static async Task<T> GetAsync<T>(string collection, string key);
    public static async Task SetAsync<T>(string collection, string key, T value);
    public static async Task DeleteAsync(string collection, string key);
    
    // List objects
    public static async Task<List<StorageObject>> ListAsync(string collection, int limit = 100);
    
    // Sync control
    public static async Task SyncAsync();
    public static void EnableAutoSync(float intervalSeconds);
    public static void DisableAutoSync();
}
```

### Collections

| Collection | Purpose | Permission |
|------------|---------|------------|
| `user_data` | Private user data | Read/Write: Owner only |
| `public_data` | Public profile data | Read: All, Write: Owner |
| `game_saves` | Save game data | Read/Write: Owner only |
| `settings` | User preferences | Read/Write: Owner only |

### Usage

```csharp
// Save to cloud
await IVXCloudStorage.SetAsync("game_saves", "slot_1", new SaveData
{
    Level = 15,
    Coins = 50000,
    Inventory = playerInventory,
    Timestamp = DateTime.UtcNow
});

// Load from cloud
var saveData = await IVXCloudStorage.GetAsync<SaveData>("game_saves", "slot_1");

// List all saves
var saves = await IVXCloudStorage.ListAsync("game_saves");
foreach (var save in saves)
{
    Debug.Log($"Save: {save.Key}, Updated: {save.UpdateTime}");
}
```

---

## IVXStorageManager

Unified interface for local/cloud storage.

```csharp
public static class IVXStorageManager
{
    // Configuration
    public static void Initialize(StorageConfig config);
    
    // Save/Load with automatic sync
    public static async Task SaveAsync<T>(string key, T data, StorageLocation location = StorageLocation.Both);
    public static async Task<T> LoadAsync<T>(string key, StorageLocation location = StorageLocation.Local);
    
    // Sync management
    public static async Task SyncToCloudAsync();
    public static async Task SyncFromCloudAsync();
    public static bool HasPendingSync { get; }
}

public enum StorageLocation
{
    Local,    // Local storage only
    Cloud,    // Cloud storage only
    Both      // Sync between local and cloud
}
```

---

## IVXSaveSystem

High-level game save management.

```csharp
public class IVXSaveSystem
{
    public static readonly int MaxSaveSlots = 5;
    
    // Events
    public static event Action<int> OnSaveComplete;
    public static event Action<int> OnLoadComplete;
    public static event Action<int, Exception> OnSaveFailed;
    
    // Save operations
    public static async Task SaveGameAsync(int slot, GameSaveData data);
    public static async Task<GameSaveData> LoadGameAsync(int slot);
    public static async Task DeleteSaveAsync(int slot);
    
    // Save info
    public static async Task<SaveSlotInfo[]> GetSaveSlotsAsync();
    public static bool HasSave(int slot);
    
    // Auto-save
    public static void EnableAutoSave(float intervalMinutes);
    public static void DisableAutoSave();
    public static void TriggerAutoSave();
}

public class SaveSlotInfo
{
    public int slot;
    public bool hasData;
    public DateTime lastSaved;
    public string description;
    public int playTimeMinutes;
}
```

### Usage

```csharp
// Save game
var saveData = new GameSaveData
{
    PlayerLevel = player.Level,
    PlayerPosition = player.transform.position,
    Inventory = player.Inventory.ToArray(),
    QuestProgress = questManager.GetProgress(),
    PlayTime = gameTime
};

await IVXSaveSystem.SaveGameAsync(saveSlot, saveData);

// Load game
var loadedData = await IVXSaveSystem.LoadGameAsync(saveSlot);
if (loadedData != null)
{
    player.Level = loadedData.PlayerLevel;
    player.transform.position = loadedData.PlayerPosition;
    // ... restore state
}

// Get available saves for UI
var slots = await IVXSaveSystem.GetSaveSlotsAsync();
foreach (var slot in slots)
{
    if (slot.hasData)
    {
        Debug.Log($"Slot {slot.slot}: {slot.description} - {slot.lastSaved}");
    }
}
```

---

## Data Encryption

### Encryption Configuration

```csharp
[CreateAssetMenu(fileName = "StorageConfig", menuName = "IntelliVerse-X/Storage Config")]
public class StorageConfig : ScriptableObject
{
    [Header("Security")]
    public bool enableEncryption = true;
    public EncryptionLevel encryptionLevel = EncryptionLevel.AES256;
    
    [Header("Cloud Sync")]
    public bool enableCloudSync = true;
    public float autoSyncInterval = 60f; // seconds
    public bool syncOnAppPause = true;
    
    [Header("Compression")]
    public bool enableCompression = true;
}

public enum EncryptionLevel
{
    None,
    AES128,
    AES256
}
```

### Custom Encryption Key

```csharp
// Override default encryption key (for advanced users)
IVXSecureStorage.SetEncryptionKey(customKey);
```

---

## Conflict Resolution

When cloud and local data conflict:

```csharp
public enum ConflictResolution
{
    PreferCloud,    // Server data wins
    PreferLocal,    // Local data wins
    PreferNewer,    // Most recent timestamp wins
    AskUser         // Prompt user to choose
}

// Configure conflict resolution
IVXStorageManager.ConflictResolution = ConflictResolution.PreferNewer;

// Handle conflicts manually
IVXStorageManager.OnConflict += (local, cloud) =>
{
    // Show UI to let user choose
    ShowConflictResolutionUI(local, cloud, (choice) =>
    {
        if (choice == "local")
            IVXStorageManager.ResolveConflict(local);
        else
            IVXStorageManager.ResolveConflict(cloud);
    });
};
```

---

## Best Practices

### 1. Key Naming

```csharp
// Use descriptive, namespaced keys
IVXSecureStorage.SetInt("game.player.level", level);
IVXSecureStorage.SetInt("game.player.coins", coins);
IVXSecureStorage.SetObject("game.settings", settings);
```

### 2. Error Handling

```csharp
try
{
    await IVXCloudStorage.SetAsync("game_saves", "slot_1", data);
}
catch (StorageSyncException ex)
{
    // Handle sync failure - data saved locally
    Debug.LogWarning($"Cloud sync failed: {ex.Message}");
    ShowToast("Saved locally. Will sync when online.");
}
```

### 3. Offline Support

```csharp
// Always save locally first for offline support
IVXSecureStorage.SetObject("current_save", saveData);

// Then sync to cloud when possible
if (IVXNakamaManager.IsConnected)
{
    await IVXCloudStorage.SetAsync("game_saves", "current", saveData);
}
```

### 4. Data Validation

```csharp
var loadedData = IVXSecureStorage.GetObject<SaveData>("save");
if (loadedData == null || !loadedData.IsValid())
{
    // Data corrupted or missing
    loadedData = new SaveData(); // Use defaults
}
```

---

## Platform Notes

| Feature | Android | iOS | WebGL | Standalone |
|---------|---------|-----|-------|------------|
| Local Storage | ✅ | ✅ | ✅ | ✅ |
| Encryption | ✅ | ✅ | ⚠️ | ✅ |
| Cloud Sync | ✅ | ✅ | ✅ | ✅ |
| Auto-Save | ✅ | ✅ | ✅ | ✅ |

⚠️ WebGL encryption is less secure due to browser limitations

---

## Related Documentation

- [Save System Demo](../samples/save-demo.md) - Sample implementation
- [Nakama Storage Docs](https://heroiclabs.com/docs/storage-collections/) - Backend details
