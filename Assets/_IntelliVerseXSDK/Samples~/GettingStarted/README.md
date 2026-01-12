# Getting Started Sample

This sample demonstrates the basic setup and usage of the IntelliVerseX SDK.

## Contents

- **Scripts/**
  - `IVXGettingStartedDemo.cs` - Main demo script showing SDK initialization
  - `IVXGettingStartedUI.cs` - Simple UI to display SDK status
  
- **Scenes/**
  - `GettingStartedScene.unity` - Demo scene with SDK setup
  
- **Prefabs/**
  - `IVXDemoManager.prefab` - Pre-configured SDK manager

## Setup

1. Import this sample via Package Manager
2. Open `Scenes/GettingStartedScene.unity`
3. Press Play to see the SDK initialize

## What This Sample Demonstrates

1. **SDK Initialization**: How to initialize the SDK in your game
2. **Device Identity**: Automatic device-based user identification
3. **Logging**: Using the IVXLogger for debug output
4. **Backend Connection**: Optional backend connectivity (requires Nakama)
5. **Configuration**: Setting up SDK configuration

## Code Overview

### Basic Initialization

```csharp
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class MyGameManager : MonoBehaviour
{
    void Start()
    {
        // Initialize device identity
        IntelliVerseXUserIdentity.InitializeDevice();
        
        IVXLogger.Log("SDK initialized!");
    }
}
```

### With Backend Connection

```csharp
using IntelliVerseX.Backend;

async void Start()
{
    // Initialize and connect
    IntelliVerseXUserIdentity.InitializeDevice();
    
    var backend = IVXBackendService.Instance;
    if (backend != null)
    {
        bool connected = await backend.InitializeAsync();
        IVXLogger.Log($"Backend connected: {connected}");
    }
}
```

## Requirements

- IntelliVerseX SDK installed
- Newtonsoft.Json package
- (Optional) Nakama Unity SDK for backend features

## Next Steps

After completing this sample, try:
- **Quiz Demo**: Full quiz game implementation
- **Localization**: Multi-language support
- **IAP Integration**: In-app purchases
