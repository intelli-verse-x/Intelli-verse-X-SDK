# 🏗️ Architecture Rules — IntelliVerseX Unity SDK

> **Authority:** Defines how the system is allowed to be structured
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## 📐 Module Boundaries

### SDK Module Map

```
IntelliVerseX SDK Architecture
═══════════════════════════════════════════════════════════════════

                    ┌─────────────────────────────────────┐
                    │         CONSUMER GAME               │
                    │    (Uses SDK via public APIs)       │
                    └───────────────┬─────────────────────┘
                                    │
    ════════════════════════════════╪════════════════════════════════
                         SDK PUBLIC BOUNDARY
    ════════════════════════════════╪════════════════════════════════
                                    │
    ┌───────────────────────────────┼───────────────────────────────┐
    │                               ▼                               │
    │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐           │
    │  │  Identity   │  │  Backend    │  │ Monetization│           │
    │  │  Module     │  │  Module     │  │  Module     │           │
    │  │             │  │             │  │             │           │
    │  │ - Auth      │  │ - Nakama    │  │ - Ads       │           │
    │  │ - Providers │  │ - Sessions  │  │ - IAP       │           │
    │  │ - Tokens    │  │ - Storage   │  │ - Offerwall │           │
    │  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘           │
    │         │                │                │                   │
    │         └────────────────┼────────────────┘                   │
    │                          │                                    │
    │                          ▼                                    │
    │              ┌─────────────────────┐                          │
    │              │     CORE MODULE     │                          │
    │              │                     │                          │
    │              │ - Configuration     │                          │
    │              │ - Lifecycle         │                          │
    │              │ - Events            │                          │
    │              │ - Utilities         │                          │
    │              └──────────┬──────────┘                          │
    │                         │                                     │
    └─────────────────────────┼─────────────────────────────────────┘
                              │
    ══════════════════════════╪══════════════════════════════════════
                   THIRD-PARTY BOUNDARY (READ-ONLY)
    ══════════════════════════╪══════════════════════════════════════
                              │
    ┌─────────────────────────┼─────────────────────────────────────┐
    │                         ▼                                     │
    │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐          │
    │  │ Nakama  │  │ Photon  │  │Appodeal │  │AppleAuth│          │
    │  └─────────┘  └─────────┘  └─────────┘  └─────────┘          │
    └───────────────────────────────────────────────────────────────┘
```

### Module Responsibilities

| Module | Responsibility | Public API |
|--------|---------------|------------|
| **Core** | SDK lifecycle, config, shared utilities | `IntelliVerseXManager`, `IVXConfig` |
| **Identity** | Authentication, user identity | `IVXIdentityManager`, `IVXAuthProvider` |
| **Backend** | Nakama integration, data persistence | `IVXBackendManager`, `IVXStorageService` |
| **Analytics** | Event tracking, telemetry | `IVXAnalyticsManager` |
| **Monetization** | Ads, IAP, offerwall | `IVXAdsManager`, `IVXIAPManager` |
| **Localization** | Language support | `IVXLocalizationManager` |
| **Social** | Referrals, sharing | `IVXSocialManager` |
| **Leaderboard** | Rankings | `IVXLeaderboardManager` |
| **Quiz** | Quiz game support | `IVXQuizManager` |

---

## 🔗 Allowed Dependencies

### Dependency Matrix

```
FROM ↓ / TO →     Core   Identity   Backend   Analytics   Monetization   Localization   Social   Leaderboard
─────────────────────────────────────────────────────────────────────────────────────────────────────────────
Core              -      ❌         ❌        ❌          ❌             ❌             ❌       ❌
Identity          ✅     -          ✅        ✅          ❌             ❌             ❌       ❌
Backend           ✅     ✅         -         ✅          ❌             ❌             ❌       ❌
Analytics         ✅     ❌         ❌        -           ❌             ❌             ❌       ❌
Monetization      ✅     ❌         ❌        ✅          -              ❌             ❌       ❌
Localization      ✅     ❌         ❌        ❌          ❌             -              ❌       ❌
Social            ✅     ✅         ✅        ✅          ❌             ❌             -        ❌
Leaderboard       ✅     ✅         ✅        ✅          ❌             ❌             ❌       -
```

### Dependency Rules

1. **Core depends on nothing** - Core is the foundation
2. **All modules may depend on Core** - Core provides shared utilities
3. **No circular dependencies** - A → B means B cannot → A
4. **No cross-feature dependencies** - Monetization cannot depend on Social
5. **Backend/Identity may cross-reference** - Auth requires backend, backend requires auth

---

## 🏛️ Runtime vs Editor Rules

### Runtime Assembly Rules

```yaml
runtime_assemblies:
  - IntelliVerseX.Core
  - IntelliVerseX.Identity
  - IntelliVerseX.Backend
  - IntelliVerseX.Analytics
  - IntelliVerseX.Monetization
  - IntelliVerseX.Localization
  - IntelliVerseX.Social
  - IntelliVerseX.Leaderboard
  - IntelliVerseX.Quiz

runtime_rules:
  - No UnityEditor namespace imports
  - No #if UNITY_EDITOR for core logic
  - No Editor-only APIs
  - Must work in builds
```

### Editor Assembly Rules

```yaml
editor_assemblies:
  - IntelliVerseX.Editor
  - IntelliVerseX.*.Editor (per-module)

editor_rules:
  - May reference all runtime assemblies
  - May use UnityEditor namespace
  - Must be in Editor/ folders
  - Never included in builds
```

### Platform-Specific Code

```csharp
// ✅ CORRECT: Platform defines for runtime behavior
#if UNITY_ANDROID
    // Android-specific implementation
#elif UNITY_IOS
    // iOS-specific implementation
#elif UNITY_WEBGL
    // WebGL-specific implementation
#endif

// ❌ WRONG: Editor code in runtime
#if UNITY_EDITOR
    UnityEditor.EditorUtility.DisplayDialog(...); // VIOLATION!
#endif
```

---

## 📦 ScriptableObject Usage Rules

### When to Use ScriptableObjects

| Use Case | ScriptableObject | Alternative |
|----------|------------------|-------------|
| SDK Configuration | ✅ Yes | - |
| Feature Flags | ✅ Yes | - |
| Static Data | ✅ Yes | - |
| Runtime State | ❌ No | Manager classes |
| User Data | ❌ No | Backend storage |
| Secrets | ❌ No | Secure storage |

### ScriptableObject Patterns

```csharp
// ✅ CORRECT: Configuration ScriptableObject
[CreateAssetMenu(fileName = "IVXConfig", menuName = "IntelliVerseX/Configuration")]
public class IVXConfig : ScriptableObject
{
    [Header("Backend")]
    [SerializeField] private string _nakamaHost;
    [SerializeField] private int _nakamaPort;
    
    public string NakamaHost => _nakamaHost;
    public int NakamaPort => _nakamaPort;
}

// ❌ WRONG: Runtime state in ScriptableObject
public class IVXRuntimeState : ScriptableObject
{
    public bool IsLoggedIn; // VIOLATION: Runtime state
    public string SessionToken; // VIOLATION: Sensitive data
}
```

---

## 🔌 Extension Points

### Supported Extension Patterns

| Pattern | Use Case | Implementation |
|---------|----------|----------------|
| **Interface** | Custom auth providers | `IIVXAuthProvider` |
| **Events** | React to SDK events | `IVXEvents.OnUserAuthenticated` |
| **Callbacks** | Async operations | `Action<T>` parameters |
| **Inheritance** | Custom managers | `IVXBaseManager<T>` |

### Extension Examples

```csharp
// ✅ CORRECT: Interface-based extension
public interface IIVXAuthProvider
{
    UniTask<IVXAuthResult> AuthenticateAsync();
    void SignOut();
}

// Consumer implements custom provider
public class MyCustomAuthProvider : IIVXAuthProvider
{
    public async UniTask<IVXAuthResult> AuthenticateAsync() { ... }
    public void SignOut() { ... }
}

// Register with SDK
IVXIdentityManager.RegisterProvider(new MyCustomAuthProvider());
```

---

## 🚫 Forbidden Patterns

### Architecture Anti-Patterns

| Pattern | Why Forbidden | Alternative |
|---------|---------------|-------------|
| God class | Violates SRP | Split by responsibility |
| Circular dependency | Untestable, fragile | Dependency injection |
| Static state | Hard to test, race conditions | Instance-based managers |
| Deep inheritance | Rigid, hard to change | Composition |
| Service locator abuse | Hidden dependencies | Constructor injection |

### Code Anti-Patterns

```csharp
// ❌ FORBIDDEN: God class
public class IVXEverythingManager
{
    public void DoAuth() { }
    public void DoAds() { }
    public void DoAnalytics() { }
    // VIOLATION: Too many responsibilities
}

// ❌ FORBIDDEN: Static mutable state
public static class IVXGlobals
{
    public static string CurrentUserId; // VIOLATION: Mutable static
    public static bool IsInitialized;   // VIOLATION: Mutable static
}

// ❌ FORBIDDEN: Circular dependency
// In Identity module:
using IntelliVerseX.Monetization; // VIOLATION: Identity → Monetization

// In Monetization module:
using IntelliVerseX.Identity; // VIOLATION: Monetization → Identity (circular!)
```

---

## 📁 Folder Structure Rules

### Required Structure

```
Assets/_IntelliVerseXSDK/
├── Core/
│   ├── IntelliVerseX.Core.asmdef
│   ├── IntelliVerseXConfig.cs
│   └── [Core utilities]
├── Identity/
│   ├── IntelliVerseX.Identity.asmdef
│   └── [Auth providers, managers]
├── Backend/
│   ├── IntelliVerseX.Backend.asmdef
│   └── [Nakama integration]
├── Analytics/
│   ├── IntelliVerseX.Analytics.asmdef
│   └── [Event tracking]
├── Monetization/
│   ├── IntelliVerseX.Monetization.asmdef
│   ├── Ads/
│   └── IAP/
├── Localization/
│   ├── IntelliVerseX.Localization.asmdef
│   └── [Language support]
├── Social/
│   ├── IntelliVerseX.Social.asmdef
│   └── [Referrals, sharing]
├── Leaderboard/
│   ├── IntelliVerseX.Leaderboard.asmdef
│   └── [Rankings]
├── Editor/
│   ├── IntelliVerseX.Editor.asmdef
│   └── [Editor tools, wizards]
├── Examples/
│   └── [Sample implementations]
└── Documentation~/
    └── [External docs]
```

### Folder Rules

1. **One asmdef per module** - Clear compilation boundaries
2. **Editor code in Editor/ folders** - Never in Runtime
3. **Tests in Tests/ folders** - Separate from production code
4. **Samples in Examples/ or Samples~/** - Optional import
5. **Documentation in Documentation~/** - Not imported by Unity

---

## 🔄 State Management

### State Ownership

| State Type | Owner | Access Pattern |
|------------|-------|----------------|
| SDK initialization | `IntelliVerseXManager` | Singleton |
| User session | `IVXIdentityManager` | Events + Properties |
| Configuration | `IVXConfig` | ScriptableObject |
| Analytics queue | `IVXAnalyticsManager` | Internal |
| Ad state | `IVXAdsManager` | Events |

### State Transition Rules

```csharp
// ✅ CORRECT: State changes through manager methods
IVXIdentityManager.Instance.SignInAsync(credentials);

// ❌ WRONG: Direct state manipulation
IVXIdentityManager.Instance._currentUser = newUser; // VIOLATION!
```

---

## ✅ Architecture Checklist

Before implementing any feature:

- [ ] Identified which module owns this feature
- [ ] Verified dependencies are allowed
- [ ] No circular dependencies introduced
- [ ] Runtime code has no Editor dependencies
- [ ] ScriptableObjects used appropriately
- [ ] Extension points considered
- [ ] No forbidden patterns used
- [ ] Folder structure followed

---

*If a change violates this file, do not implement it. Propose an alternative or update this document with justification.*
