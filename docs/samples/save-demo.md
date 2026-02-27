# Save System Demo

This sample demonstrates the data persistence system.

## Features

- Auto-save functionality
- Cloud sync support
- Multiple save slots
- Data encryption

## Code Example

```csharp
using IntelliVerseX.Storage;

// Save game state
IVXStorageManager.Instance.Save("player_data", playerData);

// Load game state
var data = IVXStorageManager.Instance.Load<PlayerData>("player_data");
```

## See Also

- [Storage Module](../modules/storage.md)
