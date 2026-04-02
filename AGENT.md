# 🎮 IntelliVerseX Unity SDK — Project Intelligence Index

> 🤖 **Context Optimized**
> This file is a lightweight index. Load specific modules below for details.

---

## 📚 CONTEXT MODULES

| Module | Content | Path |
|--------|---------|------|
| **Context Authority** | Vision, Principles, Rules | `.cursor/context.md` |
| **Architecture** | System Structure, Dependencies | `.cursor/architecture.md` |
| **Naming & Style** | Conventions, Templates | `.cursor/naming-and-style.md` |
| **Assumptions** | Explicit Assumptions | `.cursor/assumptions.md` |
| **Non-Goals** | Scope Boundaries | `.cursor/NON_GOALS.md` |

---

## PROJECT SPECS

| Property | Value |
|----------|-------|
| **Project** | IntelliVerseX Multi-Platform SDK |
| **Type** | Unity UPM Package + Cross-Platform SDKs |
| **Unity Version** | 6000.2.8f1 |
| **Platforms** | Unity, Unreal, Godot, Defold, Cocos2d-x, JS, C++, Java/Android |
| **Package Name** | com.intelliversex.sdk |

---

## PLATFORM SDKs

| Platform | Location | Language | Nakama Client |
|----------|----------|----------|---------------|
| **Unity / .NET** | `Assets/Intelli-verse-X-SDK/` + `Assets/_IntelliVerseXSDK/` | C# | nakama-unity |
| **Unreal Engine** | `SDKs/unreal/` | C++ / Blueprints | nakama-unreal |
| **Godot Engine** | `SDKs/godot/` | GDScript | nakama-godot |
| **Defold** | `SDKs/defold/` | Lua | nakama-defold |
| **Cocos2d-x** | `SDKs/cocos2dx/` | C++ | nakama-cpp |
| **JavaScript** | `SDKs/javascript/` | TypeScript | nakama-js |
| **C / C++** | `SDKs/cpp/` | C++ | nakama-cpp |
| **Java / Android** | `SDKs/java/` | Java | nakama-java |
| **Flutter / Dart** | `SDKs/flutter/` | Dart | nakama-dart |
| **Web3** | `SDKs/web3/` | TypeScript | nakama-js + ethers |

---

## 📦 SDK MODULES

### Core Package (`Assets/Intelli-verse-X-SDK/`)

| Module | Namespace | Purpose | Key Classes |
|--------|-----------|---------|-------------|
| **Core** | `IntelliVerseX.Core` | SDK lifecycle, config, logging | `IntelliVerseXConfig`, `IVXLogger`, `IVXSafeSingleton<T>` |
| **Identity** | `IntelliVerseX.Identity` | Authentication, sessions | `IntelliVerseXUserIdentity`, `UserSessionManager`, `APIManager` |
| **Auth** | `IntelliVerseX.Auth` | Login/register UI panels | `IVXPanelLogin`, `IVXPanelRegister`, `IVXCanvasAuth` |
| **Backend** | `IntelliVerseX.Backend` | Nakama integration, wallet | `IVXNakamaManager`, `IVXBackendService`, `IVXWalletManager` |
| **Analytics** | `IntelliVerseX.Analytics` | Event tracking | `IVXAnalyticsManager`, `IVXEventTracker` |
| **Monetization** | `IntelliVerseX.Monetization` | Ads, IAP, offerwalls | `IVXAdsManager`, `IVXIAPManager`, `IVXOfferwallManager` |
| **Localization** | `IntelliVerseX.Localization` | 12+ languages, RTL | `IVXLocalizationService`, `IVXLanguageManager` |
| **Social** | `IntelliVerseX.Social` | Friends, clans, sharing | `IVXFriendsManager`, `IVXClanManager`, `IVXShareService` |
| **Leaderboard** | `IntelliVerseX.Leaderboard` | Rankings, scores | `IVXGLeaderboardManager` |
| **Quiz** | `IntelliVerseX.Quiz` | Quiz game framework | `IVXQuizSessionManager`, `IVXDailyQuizManager`, `IVXWeeklyQuizManager` |
| **Storage** | `IntelliVerseX.Storage` | Secure persistence | `IVXSecureStorage` |
| **Networking** | `IntelliVerseX.Networking` | HTTP requests | `IVXNetworkRequest` |
| **UI** | `IntelliVerseX.UI` | UI utilities, wallet display | `IVXUIManager`, `IVXWalletDisplay` |
| **V2** | `IntelliVerseX.V2` | Next-gen profiles, wallet | `IVXNManager`, `IVXNProfileManager`, `IVXNWalletManager` |
| **Editor** | `IntelliVerseX.Editor` | Setup wizards, tools | `IVXSDKSetupWizard`, `IVXProjectSetup` |

### Extended Modules (`Assets/_IntelliVerseXSDK/`)

| Module | Namespace | Purpose | Key Classes |
|--------|-----------|---------|-------------|
| **AI** | `IntelliVerseX.AI` | AI voice personas, host commentary | `IVXAISessionManager`, `IVXAIApiClient`, `IVXAIWebSocketClient` |
| **Hiro** | `IntelliVerseX.Hiro` | Server-authoritative game systems | `IVXHiroCoordinator`, `IVXSpinWheelSystem`, `IVXStreaksSystem` |
| **Satori** | `IntelliVerseX.Satori` | Server-side analytics, A/B tests | `IVXSatoriClient`, `IVXSatoriRpcClient` |
| **Platform** | `IntelliVerseX.Platform` | Device utilities | `IVXPlatformOptimizer`, `IVXDeepLinkManager` |
| **Demos** | `IntelliVerseX.Demos` | Ready-to-run demo UIs | `IVXAIVoiceChatDemo`, `IVXSpinWheelDemo`, `IVXStreakDemo` |

---

## FOLDER STRUCTURE

```
Intelli-verse-X-SDK/
|-- Assets/
|   |-- Intelli-verse-X-SDK/       # Unity UPM Package (com.intelliversex.sdk)
|   |   |-- Core/                  # Foundation, config, logging
|   |   |-- Identity/              # Auth providers, sessions
|   |   |-- Auth/                  # Login/Register UI panels
|   |   |-- Backend/               # Nakama integration, wallet
|   |   |-- Analytics/             # Event tracking
|   |   |-- Monetization/          # Ads, IAP, offerwalls
|   |   |-- Localization/          # 12+ languages, RTL
|   |   |-- Social/                # Friends, clans, sharing
|   |   |-- Leaderboard/           # Rankings, scores
|   |   |-- Quiz/                  # Quiz + daily/weekly
|   |   |-- Storage/               # Secure persistence
|   |   |-- Networking/            # HTTP layer
|   |   |-- UI/                    # UI utilities
|   |   |-- V2/                    # Next-gen profiles
|   |   |-- Editor/                # Setup wizards, tools
|   |   |-- Samples~/              # Importable UPM samples
|   |   +-- Tests~/                # Unit tests
|   +-- _IntelliVerseXSDK/         # Extended modules
|       |-- AI/                    # AI voice, host, entitlements
|       |-- Hiro/                  # Spin wheel, streaks, retention
|       |-- Satori/                # Server-side analytics
|       |-- Platform/              # Deep links, foldable, optimizer
|       +-- Demos/                 # Ready-to-run demo UIs
|-- SDKs/                          # Cross-platform SDKs
|   |-- unreal/                    # Unreal Engine 5 Plugin
|   |-- godot/                     # Godot 4 Addon
|   |-- defold/                    # Defold Library Module
|   |-- cocos2dx/                  # Cocos2d-x / CMake
|   |-- javascript/                # npm / TypeScript
|   |-- cpp/                       # Native C++ / CMake
|   |-- java/                      # Java / Gradle / Android
|   |-- flutter/                   # Flutter / Dart (pub.dev)
|   +-- web3/                      # Web3 / TypeScript (ethers.js)
|-- docs/                          # MkDocs documentation
+-- .github/workflows/             # CI/CD
```

---

## 🔗 THIRD-PARTY DEPENDENCIES

| Dependency | Location | Purpose | Status |
|------------|----------|---------|--------|
| **Nakama** | `Assets/Nakama/` | Backend services | Read-Only |
| **Photon PUN2** | `Assets/Photon/` | Multiplayer | Read-Only |
| **Appodeal** | `Assets/Appodeal/` | Ad mediation | Read-Only |
| **LevelPlay** | `Assets/LevelPlay/` | Ad mediation | Read-Only |
| **Apple Auth** | `Assets/AppleAuth/` | iOS Sign-In | Read-Only |
| **DOTween** | `Assets/Plugins/Demigiant/` | Animations | Read-Only |

---

## 🏗️ ARCHITECTURE OVERVIEW

```
Consumer Game
    │
    ▼
┌─────────────────────────────────────────────┐
│           SDK PUBLIC API LAYER              │
│  IntelliVerseX.* (Managers, Services)       │
├─────────────────────────────────────────────┤
│           SDK INTERNAL LAYER                │
│  Internal implementations, utilities        │
├─────────────────────────────────────────────┤
│           THIRD-PARTY LAYER                 │
│  Nakama, Photon, Appodeal, Apple Auth       │
└─────────────────────────────────────────────┘
```

### Design Patterns

| Pattern | Implementation | Location |
|---------|---------------|----------|
| Singleton | `IVX*Manager.Instance` | All managers |
| Service Locator | `IntelliVerseXManager` | Core |
| Strategy | Auth providers | Identity |
| Observer | C# events | Throughout |
| Factory | Object creation | Various |

---

## 📐 CODING STANDARDS (Quick Reference)

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXIdentityManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private Fields | `_camelCase` | `_isInitialized` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Events | `On` + PascalCase | `OnAuthStateChanged` |

### Rules

- ✅ Use `[SerializeField]` over public fields
- ✅ Cache component references in `Awake()`
- ✅ Use `?.` operator for null safety
- ✅ XML documentation on all public APIs
- ❌ No LINQ in Update loops
- ❌ No GetComponent in hot paths
- ❌ No hardcoded secrets

---

## 🚫 READ-ONLY ZONES

These folders are **NEVER** to be modified:

```
Assets/Nakama/
Assets/Photon/
Assets/Appodeal/
Assets/LevelPlay/
Assets/AppleAuth/
Assets/Plugins/Demigiant/
Assets/Plugins/NativeFilePicker/
Library/
Temp/
Logs/
```

---

## 🔗 QUICK LINKS

| Resource | Path |
|----------|------|
| **Workflow Guide** | `AGENTS.md` |
| **System Map (Master Index)** | `.cursor/SYSTEM_MAP.md` |
| **Context Authority** | `.cursor/context.md` |
| **Architecture** | `.cursor/architecture.md` |
| **Non-Goals** | `.cursor/NON_GOALS.md` |
| **AI Guardrails** | `.cursor/AI_GUARDRAILS.md` |
| **Anti-Patterns** | `.cursor/ANTI_PATTERNS.md` |
| **Naming Guide** | `.cursor/naming-and-style.md` |
| **Assumptions** | `.cursor/assumptions.md` |
| **Context Validator (CI + Local)** | `tools/context/validate_context.py` |
| **Changelog** | `CHANGELOG.md` (root) + `Assets/Intelli-verse-X-SDK/CHANGELOG.md` |

---

## 💡 AI TIPS

> **Before editing any script:**
> 1. Check this index for module location
> 2. Load relevant context from `.cursor/`
> 3. Verify against `NON_GOALS.md`
> 4. Follow naming conventions

> **When creating new scripts:**
> 1. Place in correct module folder
> 2. Use `IntelliVerseX.[Module]` namespace
> 3. Use `IVX` prefix for public types
> 4. Add XML documentation
> 5. Update this index

---

*For detailed workflows, see `AGENTS.md`*
