# 🎮 IntelliVerseX SDK

> **Complete modular game development SDK for Unity**

[![Unity](https://img.shields.io/badge/Unity-2023.3%2B-black.svg)](https://unity.com/)
[![Unity 6](https://img.shields.io/badge/Unity%206-Supported-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-2.0.0-orange.svg)](CHANGELOG.md)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔐 **Identity & Auth** | Multi-provider auth (Device, Email, Apple, Google) |
| ☁️ **Backend** | Nakama server connectivity out of the box |
| 💰 **Monetization** | IAP, Ads (LevelPlay, Appodeal, AdMob), Offerwalls |
| 📊 **Analytics** | Event tracking and user behavior analysis |
| 🌍 **Localization** | 12+ languages with RTL support |
| 💾 **Storage** | Secure cloud saves and local persistence |
| 🎯 **Leaderboards** | Global rankings with Nakama |
| 👥 **Social** | Friends, sharing, referrals |
| 🎲 **Quiz System** | Complete quiz game framework |
| 🎨 **UI Components** | Production-ready UI utilities |
| 😀 **Emoji Support** | TMP emoji conversion + production import hardening |

---

## 📦 Installation

### Method 1: Git URL (Recommended)

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

### Method 2: Specific Version (Production)

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v2.0.0"
  }
}
```

### Method 3: Package Manager UI

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter:
   ```
   https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK
   ```
4. Click **Add**

📖 See [INSTALLATION.md](INSTALLATION.md) for detailed setup instructions.

---

## 🚀 Quick Start

### 1. Run Project Setup

After installation:
1. Go to **IntelliVerseX > Project Setup & Validation**
2. Click **Apply All Required Settings**

### 2. Initialize SDK

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        // Initialize device identity
        IntelliVerseXUserIdentity.InitializeDevice();
        
        IVXLogger.Log("IntelliVerseX SDK Ready!");
    }
}
```

### 3. (Optional) Connect to Backend

```csharp
using IntelliVerseX.Backend;

async void Start()
{
    IntelliVerseXUserIdentity.InitializeDevice();
    
    bool connected = await IVXBackendService.Instance.InitializeAsync();
    Debug.Log($"Backend: {(connected ? "Connected" : "Offline")}");
}
```

### 4. (Optional) Enable Emoji Support

```csharp
using IntelliVerseX.Core;
using TMPro;

public class EmojiTextSample : MonoBehaviour
{
    public TMP_Text uiLabel;
    public TMP_SpriteAsset emojiSpriteAsset;

    void Start()
    {
        uiLabel.SetTextWithEmojiSprites("Welcome 😀 ❤️ 🚀", emojiSpriteAsset);
    }
}
```

Use `IntelliVerseX > Emoji > Setup & Validate` to configure emoji atlas texture settings for production platforms.

---

## 📋 Requirements

### Unity Version

| Version | Status |
|---------|--------|
| **Unity 6000.x** | ✅ Fully Supported |
| **Unity 2023.3 LTS** | ✅ Fully Supported |
| Unity 2022.x | ⚠️ May work, untested |
| Unity 2021.x | ❌ Not Supported |

### Platforms

- ✅ Android (API 21+)
- ✅ iOS (12+)
- ✅ WebGL
- ✅ Windows Standalone
- ✅ macOS Standalone

### Dependencies

#### Automatic (via UPM)
- TextMeshPro (3.0.6+)
- Newtonsoft.Json (3.2.1+)

#### Manual Installation Required
| Package | Purpose | Status |
|---------|---------|--------|
| [Nakama Unity](https://github.com/heroiclabs/nakama-unity) | Backend services | **Required** |
| [DOTween](http://dotween.demigiant.com/) | UI animations | **Required** |
| [Photon PUN2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) | Multiplayer | Optional |
| [Unity IAP](https://docs.unity3d.com/Manual/UnityIAP.html) | In-app purchases | Optional |
| [LevelPlay](https://developers.is.com/ironsource-mobile/unity/) | Ad mediation | Optional |
| [Native Share](https://github.com/yasirkula/UnityNativeShare) | Social sharing | Optional |

Use **IntelliVerseX > Setup Wizard** to install dependencies.

---

## 🧩 SDK Modules

| Module | Namespace | Description |
|--------|-----------|-------------|
| **Core** | `IntelliVerseX.Core` | Foundation, utilities, configs, logging |
| **Identity** | `IntelliVerseX.Identity` | Authentication, profiles, sessions |
| **Backend** | `IntelliVerseX.Backend` | Nakama integration, wallet, API |
| **Monetization** | `IntelliVerseX.Monetization` | IAP, Ads, Offers |
| **Analytics** | `IntelliVerseX.Analytics` | Event tracking |
| **Localization** | `IntelliVerseX.Localization` | Multi-language, RTL support |
| **Storage** | `IntelliVerseX.Storage` | Secure persistence |
| **Networking** | `IntelliVerseX.Networking` | HTTP requests |
| **Leaderboard** | `IntelliVerseX.Leaderboard` | Rankings, scores |
| **Social** | `IntelliVerseX.Social` | Friends, sharing |
| **Quiz** | `IntelliVerseX.Quiz` | Quiz game logic |
| **QuizUI** | `IntelliVerseX.QuizUI` | Quiz UI components |
| **UI** | `IntelliVerseX.UI` | UI utilities |
| **V2** | `IntelliVerseX.V2` | Next-gen features |

---

## 🎯 Samples

Import via **Package Manager > IntelliVerseX SDK > Samples**:

| Sample | Description |
|--------|-------------|
| **Getting Started** | Basic SDK setup and initialization |
| **Quiz Demo** | Complete quiz game implementation |
| **Localization** | Multi-language UI with RTL |
| **IAP Integration** | In-app purchase example |
| **Leaderboard** | Global rankings demo |
| **Social Features** | Friends and sharing |

---

## 🔧 Editor Tools

Access via **IntelliVerseX** menu:

| Tool | Description |
|------|-------------|
| **Project Setup & Validation** | Validate and fix project settings |
| **Setup Wizard** | Guided dependency installation |
| **Check Dependencies** | Verify all dependencies |

---

## 📁 Package Structure

```
_IntelliVerseXSDK/
├── package.json          # UPM manifest
├── README.md             # This file
├── CHANGELOG.md          # Version history
├── LICENSE               # MIT License
├── INSTALLATION.md       # Setup guide
│
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
│
├── Documentation~/       # Docs (not imported)
├── Samples~/             # Importable samples
└── Tests~/               # Unit tests
```

---

## 🧪 Testing

The SDK includes comprehensive tests:

```bash
# Run EditMode tests
Unity > Window > General > Test Runner > EditMode > Run All

# Run PlayMode tests
Unity > Window > General > Test Runner > PlayMode > Run All
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [INSTALLATION.md](INSTALLATION.md) | Detailed installation guide |
| [CHANGELOG.md](CHANGELOG.md) | Version history |
| [Documentation~/](Documentation~/) | Complete API docs |

---

## 🤝 Contributing

Contributions welcome! Please see our [Contributing Guide](../../CONTRIBUTING.md).

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🆘 Support

- **Documentation**: [Documentation~/](Documentation~/)
- **Issues**: [GitHub Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues)
- **Email**: sdk@intelliversex.com

---

<p align="center">
  Made with ❤️ by <a href="https://intelliversex.com">IntelliVerse-X</a>
</p>
