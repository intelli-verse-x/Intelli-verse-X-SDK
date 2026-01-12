# 🎮 IntelliVerseX Unity SDK

> **Complete modular game development SDK for Unity** — Integrate Auth, Identity, Analytics, Backend (Nakama), Social/Referrals, Monetization, and more into your Unity games.

[![Unity 2023](https://img.shields.io/badge/Unity-2023.3%2B-black.svg)](https://unity.com/)
[![Unity 6](https://img.shields.io/badge/Unity%206-Supported-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-2.0.0-orange.svg)](Assets/_IntelliVerseXSDK/CHANGELOG.md)
[![CI](https://img.shields.io/badge/CI-Passing-success.svg)](.github/workflows/unity-tests.yml)

---

## 📦 Quick Install

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

**With specific version tag:**

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v2.0.0"
  }
}
```

**Via Package Manager UI:**
1. **Window > Package Manager**
2. **+** > **Add package from git URL...**
3. Enter: `https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK`

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔐 **Identity & Auth** | Device, Email, Apple, Google authentication |
| ☁️ **Backend** | Nakama server integration |
| 💰 **Monetization** | IAP, Ads (LevelPlay, Appodeal, AdMob) |
| 📊 **Analytics** | Event tracking, user behavior |
| 🌍 **Localization** | 12+ languages, RTL support |
| 💾 **Storage** | Secure cloud & local saves |
| 🎯 **Leaderboards** | Global rankings |
| 👥 **Social** | Friends, sharing, referrals |
| 🎲 **Quiz System** | Complete quiz framework |
| 🎨 **UI Components** | Production-ready utilities |

---

## 📋 Requirements

| Unity Version | Status |
|---------------|--------|
| **Unity 6000.x** | ✅ Fully Supported |
| **Unity 2023.3 LTS** | ✅ Fully Supported |
| Unity 2022.x | ⚠️ May work |
| Unity 2021.x | ❌ Not Supported |

**Platforms:** Android, iOS, WebGL, Windows, macOS

---

## 🚀 Quick Start

```csharp
using UnityEngine;
using IntelliVerseX.Core;
using IntelliVerseX.Identity;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        IntelliVerseXUserIdentity.InitializeDevice();
        IVXLogger.Log("IntelliVerseX SDK Ready!");
    }
}
```

After installing, run: **IntelliVerseX > Project Setup & Validation**

---

## 🧩 SDK Modules

| Module | Description |
|--------|-------------|
| Core | Foundation, utilities, logging |
| Identity | Authentication, sessions |
| Backend | Nakama integration, API |
| Monetization | IAP, Ads |
| Analytics | Event tracking |
| Localization | Multi-language |
| Storage | Data persistence |
| Networking | HTTP layer |
| Leaderboard | Rankings |
| Social | Friends, sharing |
| Quiz/QuizUI | Quiz game system |
| UI | UI utilities |
| V2 | Next-gen features |

---

## 📁 Repository Structure

```
Intelli-verse-X-Unity-SDK/
├── Assets/
│   └── _IntelliVerseXSDK/     ← UPM Package (this is what gets installed)
│       ├── package.json
│       ├── Core/
│       ├── Identity/
│       ├── Backend/
│       ├── Editor/
│       ├── Samples~/
│       ├── Tests~/
│       └── Documentation~/
├── .github/
│   └── workflows/
│       └── unity-tests.yml    ← CI for Unity 2023 & Unity 6
├── Packages/
├── ProjectSettings/
└── README.md                  ← This file
```

---

## 🧪 CI/CD

This repository includes GitHub Actions that test the SDK on:
- Unity 2023.3 LTS (EditMode + PlayMode)
- Unity 6 (EditMode + PlayMode)
- Build validation for Windows, Android, WebGL

---

## 📚 Documentation

- [Installation Guide](Assets/_IntelliVerseXSDK/INSTALLATION.md)
- [Getting Started](Assets/_IntelliVerseXSDK/Documentation~/getting-started.md)
- [Changelog](Assets/_IntelliVerseXSDK/CHANGELOG.md)
- [Troubleshooting](Assets/_IntelliVerseXSDK/Documentation~/troubleshooting.md)

---

## 🎯 Samples

After installing, import samples via Package Manager:
- **Getting Started** - Basic setup
- **Quiz Demo** - Full quiz game
- **Localization** - Multi-language UI
- **IAP Integration** - Purchases
- **Leaderboard** - Rankings
- **Social** - Friends & sharing

---

## 🔧 For SDK Developers

This repository is a Unity project for SDK development:

1. Clone: `git clone https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK.git`
2. Open in Unity 2023.3+
3. SDK code: `Assets/_IntelliVerseXSDK/`
4. Tests: `Window > General > Test Runner`

---

## 📄 License

MIT License - see [LICENSE](LICENSE)

---

## 🆘 Support

- [Issues](https://github.com/AhmedTaha-97/Intelli-verse-X-Unity-SDK/issues)
- Email: sdk@intelliversex.com

---

<p align="center">Made with ❤️ by <a href="https://intelliversex.com">IntelliVerse-X</a></p>
