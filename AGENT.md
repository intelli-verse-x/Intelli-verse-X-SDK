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
| **Unity / .NET** | `Assets/_IntelliVerseXSDK/` | C# | nakama-unity |
| **Unreal Engine** | `SDKs/unreal/` | C++ / Blueprints | nakama-unreal |
| **Godot Engine** | `SDKs/godot/` | GDScript | nakama-godot |
| **Defold** | `SDKs/defold/` | Lua | nakama-defold |
| **Cocos2d-x** | `SDKs/cocos2dx/` | C++ | nakama-cpp |
| **JavaScript** | `SDKs/javascript/` | TypeScript | nakama-js |
| **C / C++** | `SDKs/cpp/` | C++ | nakama-cpp |
| **Java / Android** | `SDKs/java/` | Java | nakama-java |

---

## 📦 SDK MODULES

| Module | Namespace | Purpose | Key Classes |
|--------|-----------|---------|-------------|
| **Core** | `IntelliVerseX.Core` | SDK lifecycle, config | `IntelliVerseXConfig` |
| **Identity** | `IntelliVerseX.Identity` | Authentication | `IVXIdentityManager` |
| **Backend** | `IntelliVerseX.Backend` | Nakama integration | `IVXBackendManager` |
| **Analytics** | `IntelliVerseX.Analytics` | Event tracking | `IVXAnalyticsManager` |
| **Monetization** | `IntelliVerseX.Monetization` | Ads, IAP | `IVXAdsManager`, `IVXIAPManager` |
| **Localization** | `IntelliVerseX.Localization` | Language support | `IVXLocalizationManager` |
| **Social** | `IntelliVerseX.Social` | Referrals, sharing | `IVXSocialManager` |
| **Leaderboard** | `IntelliVerseX.Leaderboard` | Rankings | `IVXLeaderboardManager` |
| **Quiz** | `IntelliVerseX.Quiz` | Quiz game support | `IVXQuizManager` |
| **Storage** | `IntelliVerseX.Storage` | Cloud save | `IVXStorageService` |
| **Editor** | `IntelliVerseX.Editor` | Editor tools | `IVXSetupWizard` |

---

## FOLDER STRUCTURE

```
Intelli-verse-X-Unity-SDK/
|-- Assets/_IntelliVerseXSDK/      # Unity SDK (UPM Package)
|   |-- Core/                      # SDK core, configuration
|   |-- Identity/                  # Authentication providers
|   |-- Backend/                   # Nakama integration
|   |-- Analytics/                 # Event tracking
|   |-- Monetization/              # Ads and IAP
|   |-- Localization/              # Language support
|   |-- Social/                    # Social features
|   |-- Leaderboard/               # Rankings
|   |-- Quiz/                      # Quiz support
|   |-- Storage/                   # Cloud storage
|   |-- Hiro/                      # Hiro systems integration
|   |-- Satori/                    # Satori analytics
|   |-- V2/                        # Next-gen features
|   +-- Editor/                    # Editor tools
|-- SDKs/
|   |-- unreal/                    # Unreal Engine 5 Plugin
|   |   +-- Source/IntelliVerseX/  # C++ source
|   |-- godot/                     # Godot 4 Addon
|   |   +-- addons/intelliversex/  # GDScript source
|   |-- defold/                    # Defold Library Module
|   |   +-- intelliversex/         # Lua source
|   |-- cocos2dx/                  # Cocos2d-x / CMake
|   |   +-- Classes/IntelliVerseX/ # C++ source
|   |-- javascript/                # npm / TypeScript
|   |   +-- src/                   # TypeScript source
|   |-- cpp/                       # Native C++ / CMake
|   |   |-- include/intelliversex/ # Public headers
|   |   +-- src/                   # Implementation
|   +-- java/                      # Java / Gradle / Android
|       +-- src/main/java/         # Java source
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
| **Changelog** | `Assets/_IntelliVerseXSDK/CHANGELOG.md` |

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
