# Getting Started with IntelliVerseX SDK

This guide will walk you through the initial setup and basic usage of the IntelliVerseX SDK.

## Prerequisites

Before you begin, ensure you have:

- Unity 2023.3 LTS or newer (Unity 6 recommended)
- Git (for package installation)
- A Unity project with API Compatibility Level set to .NET Standard 2.1 or higher

## Installation

### Method 1: Git URL (Recommended)

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

### Method 2: Specific Version

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v2.0.0"
  }
}
```

### Method 3: Package Manager UI

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter: `https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK`
4. Click **Add**

## Initial Setup

### Step 1: Run Project Setup

After installation, run the Project Setup wizard:

1. Go to **IntelliVerseX > Project Setup & Validation**
2. Click **Run Validation** to check your project
3. Click **Fix All Issues** or **Apply All Required Settings**

This will:
- Add required Tags and Layers
- Configure Scripting Define Symbols
- Validate project settings

### Step 2: Install External Dependencies

Some features require external dependencies:

1. Go to **IntelliVerseX > Setup Wizard**
2. Follow the steps to install:
   - Newtonsoft.Json (auto-installed via UPM)
   - Nakama Unity SDK (manual download)
   - DOTween (optional, for animations)

### Step 3: Configure Backend (Optional)

If using backend features:

1. Create a config asset: **Assets > Create > IntelliVerseX > Backend Config**
2. Enter your Nakama server details
3. Assign the config to the IntelliVerseX Manager

## Quick Start Code

### Basic Initialization

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;
using IntelliVerseX.Backend;

public class GameInitializer : MonoBehaviour
{
    async void Start()
    {
        // Initialize device identity
        IntelliVerseXUserIdentity.InitializeDevice();
        
        // Optional: Connect to backend
        if (IVXBackendService.Instance != null)
        {
            bool connected = await IVXBackendService.Instance.InitializeAsync();
            if (connected)
            {
                IVXLogger.Log("Connected to backend!");
            }
        }
        
        IVXLogger.Log("IntelliVerseX SDK Ready!");
    }
}
```

### Using Analytics

```csharp
using IntelliVerseX.Analytics;

// Track a custom event
IVXAnalyticsService.Instance.TrackEvent("level_complete", new Dictionary<string, object>
{
    { "level", 5 },
    { "score", 1000 },
    { "time", 120.5f }
});
```

### Using Localization

```csharp
using IntelliVerseX.Localization;

// Get localized string
string welcomeText = IVXLocalizationService.Instance.GetString("welcome_message");

// Change language
IVXLocalizationService.Instance.SetLanguage("es");
```

## Import Samples

Import sample projects to learn more:

1. Open **Window > Package Manager**
2. Select **IntelliVerseX SDK**
3. Expand **Samples**
4. Click **Import** next to the desired sample

Available samples:
- **Getting Started**: Basic setup and initialization
- **Quiz Demo**: Complete quiz game
- **Localization**: Multi-language UI
- **IAP Integration**: In-app purchases
- **Leaderboard**: Global rankings
- **Social Features**: Friends and sharing

## Next Steps

- Read the [Configuration Guide](configuration.md) for advanced setup
- Explore [Module Documentation](modules/index.md) for each SDK feature
- Check [Samples](samples.md) for complete implementation examples
- Review [API Reference](api/index.md) for detailed class documentation

## Troubleshooting

If you encounter issues:

1. Run **IntelliVerseX > Project Setup & Validation** to check configuration
2. Check **IntelliVerseX > Check Dependencies** for missing dependencies
3. See [Troubleshooting Guide](troubleshooting.md) for common issues
