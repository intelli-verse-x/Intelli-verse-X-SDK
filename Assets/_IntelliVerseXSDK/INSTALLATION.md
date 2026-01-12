# 📦 IntelliVerseX SDK Installation Guide

> Complete guide for installing and configuring the IntelliVerseX SDK v2.0.0 in your Unity project.

---

## 📋 Requirements

### Unity Version
- **Minimum:** Unity 2021.3 LTS
- **Recommended:** Unity 2022.3 LTS or Unity 6000+
- **Tested On:** Unity 6000.2.8f1

### Platform Support
| Platform | Status | Notes |
|----------|--------|-------|
| Android | ✅ Full | API 21+ |
| iOS | ✅ Full | iOS 12+ |
| WebGL | ✅ Full | With limitations |
| Windows | ✅ Full | Standalone |
| macOS | ✅ Full | Standalone |

---

## 🚀 Quick Start Installation

### Method 1: Git URL (Recommended)

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

### Method 2: With Specific Version (Recommended for Production)

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v2.0.0"
  }
}
```

### Method 3: Package Manager UI

1. Open **Window > Package Manager**
2. Click the **+** button in the top-left
3. Select **Add package from git URL...**
4. Enter:
   ```
   https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK
   ```
5. Click **Add**

### Method 4: Local Development

1. Clone the repository:
   ```bash
   git clone https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
   ```
2. In Unity, go to **Window > Package Manager**
3. Click **+** > **Add package from disk...**
4. Navigate to `Intelli-verse-X-Unity-SDK/Assets/_IntelliVerseXSDK/package.json`
5. Click **Open**

---

## 📦 What Gets Installed

When you install via Git URL, the **entire `_IntelliVerseXSDK` folder** is imported as a package:

```
Packages/
└── com.intelliversex.sdk/
    ├── Core/                 # Foundation & utilities
    ├── Identity/             # Authentication
    ├── Backend/              # Nakama integration
    ├── Monetization/         # IAP & Ads
    ├── Analytics/            # Event tracking
    ├── Localization/         # Multi-language
    ├── Storage/              # Data persistence
    ├── Networking/           # Network layer
    ├── Leaderboard/          # Rankings
    ├── Social/               # Friends & sharing
    ├── Quiz/                 # Quiz logic
    ├── QuizUI/               # Quiz components
    ├── UI/                   # UI utilities
    ├── V2/                   # Next-gen features
    ├── Editor/               # Editor tools
    ├── Examples/             # Code examples
    ├── Documentation/        # API docs
    ├── Icons/                # Package icons
    ├── IntroScene/           # Intro scene assets
    └── IAP/                  # In-app purchase helpers
```

---

## 📦 Installing Dependencies

The IntelliVerseX SDK requires several external dependencies. After installing the SDK, use the built-in dependency manager:

### Automatic Installation

1. Go to **Window > IntelliVerseX > Dependency Manager**
2. Review the list of required and optional dependencies
3. Click **Install All Required** or install individually

### Manual Installation

#### Required Dependencies

| Dependency | Installation Method | Purpose |
|------------|---------------------|---------|
| **Nakama Unity** | [UnityPackage](https://github.com/heroiclabs/nakama-unity/releases) | Backend services, leaderboards, auth |
| **DOTween** | [Asset Store](http://dotween.demigiant.com/) | UI animations |

#### Optional Dependencies

| Dependency | Installation Method | Purpose |
|------------|---------------------|---------|
| **Photon PUN2** | [Asset Store](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) | Multiplayer |
| **Native Share** | Git URL: `https://github.com/yasirkula/UnityNativeShare.git` | Social sharing |
| **Apple Sign-In** | [UnityPackage](https://github.com/lupidan/apple-signin-unity/releases) | iOS authentication |
| **LevelPlay** | UPM: `com.unity.services.levelplay` | Ad mediation |
| **Unity IAP** | UPM: `com.unity.purchasing` | In-app purchases |
| **Appodeal** | Git URL: `https://github.com/appodeal/appodeal-unity-plugin-upm.git` | Ad mediation |

### Installing via Git URL

For packages with Git URL support, add to `manifest.json`:

```json
{
  "dependencies": {
    "com.yasirkula.nativeshare": "https://github.com/yasirkula/UnityNativeShare.git"
  }
}
```

---

## ⚙️ Initial Setup

### Step 1: Run Setup Wizard

After installation, the setup wizard should appear automatically. If not:

1. Go to **Window > IntelliVerseX > Setup Wizard**
2. Follow the guided setup process

### Step 2: Configure Backend

1. Create a backend configuration:
   - **Assets > Create > IntelliVerseX > Backend Config**
2. Enter your Nakama server details:
   - **Host:** Your Nakama server URL
   - **Port:** 443 (HTTPS) or 7350 (HTTP)
   - **Server Key:** Your Nakama server key
   - **Use SSL:** Enable for production

### Step 3: Initialize SDK

In your game's initialization script:

```csharp
using IntelliVerseX.Core;
using IntelliVerseX.Identity;
using IntelliVerseX.Backend;

public class GameInitializer : MonoBehaviour
{
    async void Start()
    {
        // Initialize device identity
        IntelliVerseXUserIdentity.InitializeDevice();
        
        // Initialize backend connection
        bool connected = await IVXBackendService.Instance.InitializeAsync();
        
        if (connected)
        {
            Debug.Log("IntelliVerseX SDK initialized successfully!");
        }
    }
}
```

---

## 🧩 Module Overview

The SDK is organized into modular packages. All modules are included by default:

| Module | Namespace | Purpose |
|--------|-----------|---------|
| **Core** | `IntelliVerseX.Core` | Foundation, utilities, base classes |
| **Identity** | `IntelliVerseX.Identity` | User authentication, profiles |
| **Backend** | `IntelliVerseX.Backend` | Nakama integration, API calls |
| **Monetization** | `IntelliVerseX.Monetization` | IAP, Ads, Offers |
| **Analytics** | `IntelliVerseX.Analytics` | Event tracking |
| **Localization** | `IntelliVerseX.Localization` | Multi-language support |
| **Storage** | `IntelliVerseX.Storage` | Save/load data |
| **Networking** | `IntelliVerseX.Networking` | Network requests |
| **Leaderboard** | `IntelliVerseX.Leaderboard` | Rankings, scores |
| **Social** | `IntelliVerseX.Social` | Friends, sharing |
| **Quiz** | `IntelliVerseX.Quiz` | Quiz game logic |
| **QuizUI** | `IntelliVerseX.QuizUI` | Quiz UI components |
| **UI** | `IntelliVerseX.UI` | General UI utilities |
| **V2** | `IntelliVerseX.V2` | Next-gen features |

---

## 📱 Platform-Specific Setup

### Android

1. Set minimum API level to 21+ in Player Settings
2. Enable Custom Main Manifest if using push notifications
3. Add internet permission (automatic)

### iOS

1. Set minimum iOS version to 12.0+
2. For Apple Sign-In:
   - Enable "Sign in with Apple" capability in Xcode
   - Add entitlements file
3. For push notifications:
   - Enable Push Notifications capability
   - Configure APNs certificates

### WebGL

1. Some features have limitations:
   - Native sharing not available
   - File system access limited
2. Configure CORS on your Nakama server
3. Use WebGL-specific ad providers

---

## 🎯 Import Samples

Import samples via Package Manager:

1. Open **Window > Package Manager**
2. Find **IntelliVerseX SDK**
3. Expand the **Samples** section
4. Click **Import** next to the sample you want

Available samples:
- **Quiz Demo** - Complete quiz game
- **Localization** - Multi-language demo
- **IAP Integration** - Purchase flow
- **Leaderboard** - Global rankings
- **Social Features** - Friends & sharing

---

## 🔍 Verification

After installation, verify everything is working:

### Check Package Manager

1. Open **Window > Package Manager**
2. Find **IntelliVerseX SDK** in the list
3. Verify version shows `2.0.0`

### Check Console for Errors

Look for any compilation errors related to:
- Missing assembly references
- Missing dependencies

### Run Dependency Check

```
Window > IntelliVerseX > Dependency Manager > Verify All
```

### Test Basic Functionality

```csharp
using UnityEngine;
using IntelliVerseX.Core;

public class SDKTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("IntelliVerseX SDK loaded successfully!");
        Debug.Log($"Package: com.intelliversex.sdk v2.0.0");
    }
}
```

---

## 🔄 Updating the SDK

### Via Package Manager

1. Open **Window > Package Manager**
2. Find **IntelliVerseX SDK**
3. Click **Update** if available

### Via manifest.json

Update the version tag:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v2.1.0"
  }
}
```

### Migration Notes

Check `CHANGELOG.md` for breaking changes when updating major versions.

---

## 🆘 Troubleshooting

### "Assembly not found" errors

**Solution:** Ensure all dependencies are installed via the Dependency Manager.

### "Nakama authentication failed"

**Solutions:**
1. Verify server URL and port
2. Check server key is correct
3. Ensure SSL settings match server configuration
4. Check network connectivity

### "Missing script" on prefabs

**Solution:** Reimport the SDK package:
1. Remove package from manifest.json
2. Delete `Packages/com.intelliversex.sdk` folder
3. Re-add the package

### Package not appearing in Package Manager

**Solutions:**
1. Check manifest.json syntax is correct
2. Ensure Git URL is accessible
3. Try refreshing: **Assets > Refresh**

### Build errors on iOS

**Solutions:**
1. Ensure Xcode project has required capabilities
2. Check minimum iOS version (12.0+)
3. Verify Apple Sign-In entitlements if using

### Build errors on Android

**Solutions:**
1. Check minimum API level (21+)
2. Resolve Gradle conflicts
3. Check for duplicate libraries

---

## 📚 Next Steps

1. **Run Setup Wizard:** Window > IntelliVerseX > Setup Wizard
2. **Import Samples:** Package Manager > IntelliVerseX SDK > Samples
3. **Read Documentation:** Check the `Documentation/` folder
4. **Configure Backend:** Set up Nakama server connection

---

## 📞 Support

- **GitHub Issues:** [Report Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues)
- **Email:** sdk@intelliversex.com

---

*Thank you for using IntelliVerseX SDK v2.0.0!*
