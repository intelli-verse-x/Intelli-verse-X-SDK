# 🎮 IntelliVerseX SDK

> **Complete modular game development SDK for Unity**

[![Unity](https://img.shields.io/badge/Unity-6000.3-black.svg)](https://unity.com/)
[![Unity 6](https://img.shields.io/badge/Unity%206-Supported-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-5.10.0-orange.svg)](CHANGELOG.md)

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
| 👥 **Social** | Friends, sharing, referrals, clans |
| 🎲 **Quiz System** | Complete quiz game framework |
| 🎨 **UI Components** | Production-ready UI utilities |
| 😀 **Emoji Support** | TMP emoji conversion + production import hardening |
| 🤖 **AI Voice & Host** | Conversational AI personas, voice streaming, game commentary |
| 🎰 **Hiro Systems** | Spin wheel, streaks, retention, offerwalls, friend quests |
| 🛡️ **Platform** | Deep links, foldable support, edge-to-edge, performance optimizer |
| 📈 **Satori** | Server-side analytics, A/B testing, live events |

---

## 📦 Installation

### Method 1: Git URL (Recommended)

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk"
  }
}
```

### Method 2: Specific Version (Production)

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk#v5.10.0"
  }
}
```

### Method 3: Package Manager UI

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter:
   ```
   https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk
   ```
4. Click **Add**

📖 See [INSTALLATION.md](INSTALLATION.md) for detailed setup instructions.

---

## 🚀 Quick Start

**Canonical path:** Control Center → Bootstrap Config → Play.

### 1. Open Control Center

After install: **IntelliVerseX → Control Center**

1. **Connect** — paste your Game ID (creates `IVXBootstrapConfig` if needed)
2. **Play** — **Add bootstrap to this scene**
3. Press Play in the Editor

Optional modules / deps: **IntelliVerseX → Advanced Setup** (not first-run).

### 2. Code init (optional)

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] IVXBootstrap bootstrap;

    async void Start()
    {
        // Prefer AutoInitialize on IVXBootstrap in the scene.
        await bootstrap.InitializeAsync();
    }
}
```

Do **not** use obsolete `IntelliVerseXManager` / `IntelliVerseXConfig` / device-only init as the public story.

### 3. Samples

Import **Test Scenes** for Auth, Wallet, Leaderboard, Ads. Getting Started is scripts-only today — use Control Center for real setup.

---

## 📋 Requirements

### Unity Version

| Version | Status |
|---------|--------|
| **Unity 6000.3** (package pin) | ✅ Supported |
| Unity 6000.x | ✅ Expected to work |
| Unity 2023.3 / older | ❌ Not the package target |

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
| **V2** | `IntelliVerseX.V2` | Next-gen profiles & wallet |
| **AI** | `IntelliVerseX.AI` | AI voice personas, host commentary, entitlements |
| **Hiro** | `IntelliVerseX.Hiro` | Spin wheel, streaks, retention, offerwalls, friend systems |
| **Satori** | `IntelliVerseX.Satori` | Server-side analytics & A/B testing |
| **Platform** | `IntelliVerseX.Platform` | Deep links, foldable, edge-to-edge, optimizer |
| **GameModes** | `IntelliVerseX.GameModes` | Solo, local multiplayer, online versus/coop, ranked, matchmaking, lobby |
| **Demos** | `IntelliVerseX.Demos` | Ready-to-run demo UIs for AI, spin wheel, streaks, offerwall, multiplayer |

---

## 🎯 Samples

Import via **Package Manager > IntelliVerseX SDK > Samples**:

| Sample | Description |
|--------|-------------|
| **Getting Started** | Basic SDK setup and initialization |
| **Test Scenes** | Pre-built scenes for Auth, Ads, Leaderboard, Wallet, Quiz, Friends, Clans |
| **Quiz Demo** | Complete quiz game implementation |
| **Localization** | Multi-language UI with RTL |
| **IAP Integration** | In-app purchase example |
| **Leaderboard** | Global rankings demo |
| **Social Features** | Friends and sharing |
| **AI Voice Chat Demo** | AI persona voice chat with push-to-talk (`_IntelliVerseXSDK/Demos/`) |
| **Spin Wheel Demo** | Animated spin wheel with prizes (`_IntelliVerseXSDK/Demos/`) |
| **Streak / Daily Rewards** | 7-day calendar with shields (`_IntelliVerseXSDK/Demos/`) |
| **Offerwall Demo** | Scrollable offer cards (`_IntelliVerseXSDK/Demos/`) |
| **Game Mode Selector** | Solo / Local / Online mode picker (`_IntelliVerseXSDK/Demos/`) |
| **Lobby Demo** | Online room browser, create, matchmaking (`_IntelliVerseXSDK/Demos/`) |

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
Intelli-verse-X-SDK/          # UPM Package (com.intelliversex.sdk)
├── package.json              # UPM manifest
├── README.md                 # This file
├── CHANGELOG.md              # Version history
├── Core/                     # Foundation & utilities
├── Identity/                 # Authentication
├── Backend/                  # Nakama integration
├── Monetization/             # IAP & Ads
├── Analytics/                # Event tracking
├── Localization/             # Multi-language
├── Storage/                  # Data persistence
├── Networking/               # Network layer
├── Leaderboard/              # Rankings
├── Social/                   # Friends, clans, sharing
├── Quiz/                     # Quiz logic + daily/weekly
├── QuizUI/                   # Quiz components
├── UI/                       # UI utilities
├── V2/                       # Next-gen profiles & wallet
├── Editor/                   # Editor tools & wizards
├── Samples~/                 # Importable UPM samples
├── Tests~/                   # Unit tests
└── Documentation~/           # Docs (not imported)

_IntelliVerseXSDK/            # Extended modules
├── AI/                       # AI voice, host, entitlements
├── Multiplayer/              # Game modes, lobby, matchmaking, local MP
├── Hiro/                     # Spin wheel, streaks, retention, offerwalls
├── Satori/                   # Server-side analytics
├── Platform/                 # Deep links, foldable, optimizer
├── Demos/                    # Ready-to-run demo UIs
└── IntroScene/               # Intro/splash screen assets
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
- **Issues**: [GitHub Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/issues)
- **Email**: sdk@intelliversex.com

---

<p align="center">
  Made with ❤️ by <a href="https://intelliversex.com">IntelliVerse-X</a>
</p>
