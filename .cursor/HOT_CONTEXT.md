# 🔥 Hot Context - Quick Reference

> **Purpose:** Fast-loading context for common operations. Load this FIRST, then AGENT.md only if needed.
> **Last Updated:** 2026-01-20

---

## 🚀 AI Quick Start

Before ANY task:
1. Check `.cursor/NON_GOALS.md` for scope boundaries
2. Check `.cursor/architecture.md` for layer rules
3. For code changes, follow `.cursor/naming-and-style.md`

---

## Most-Used Classes (Top 10)

| Class | Path | Quick Purpose |
|-------|------|---------------|
| `IntelliVerseXConfig` | `Core/IntelliVerseXConfig.cs` | SDK configuration |
| `IVXIdentityManager` | `Identity/` | Authentication, user identity |
| `IVXBackendManager` | `Backend/` | Nakama integration |
| `IVXAdsManager` | `Monetization/Ads/` | Ad mediation |
| `IVXIAPManager` | `Monetization/IVXIAPManager.cs` | In-app purchases |
| `IVXAnalyticsManager` | `Analytics/` | Event tracking |
| `IVXLocalizationManager` | `Localization/` | Language support |
| `IVXSocialManager` | `Social/` | Referrals, sharing |
| `IVXLeaderboardManager` | `Leaderboard/` | Rankings |
| `IVXNManager` | `V2/Manager/` | Next-gen manager |

---

## Common Patterns (Copy-Paste Ready)

### Singleton Access
```csharp
if (IVXIdentityManager.Instance != null)
{
    IVXIdentityManager.Instance.MethodName();
}
// OR
IVXIdentityManager.Instance?.MethodName();
```

### Singleton Cleanup-Safe Access (Pattern)
Use this pattern when objects can be destroyed during shutdown/scene unload.
```csharp
if (SomeManager.HasInstance)
{
    var instance = SomeManager.Instance;
    if (instance != null)
    {
        // unsubscribe, stop work, etc.
    }
}
```

### Event Subscription (Memory Safe)
```csharp
private void OnEnable() => IVXIdentityManager.OnAuthStateChanged += HandleAuthChanged;
private void OnDisable() => IVXIdentityManager.OnAuthStateChanged -= HandleAuthChanged;
```

### Async Pattern with Error Handling
```csharp
private async Task DoSomethingAsync()
{
    try
    {
        await SomeOperation();
    }
    catch (Exception ex)
    {
        Debug.LogError($"[{nameof(ClassName)}] Error: {ex.Message}");
    }
}
```

### ScriptableObject Configuration
```csharp
[CreateAssetMenu(fileName = "IVXConfig", menuName = "IntelliVerseX/Configuration")]
public class IVXFeatureConfig : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private bool _enabled = true;
    
    public bool Enabled => _enabled;
}
```

---

## Quick Namespace Reference

| Folder | Namespace |
|--------|-----------|
| `Core/` | `IntelliVerseX.Core` |
| `Identity/` | `IntelliVerseX.Identity` |
| `Backend/` | `IntelliVerseX.Backend` |
| `Analytics/` | `IntelliVerseX.Analytics` |
| `Monetization/` | `IntelliVerseX.Monetization` |
| `Localization/` | `IntelliVerseX.Localization` |
| `Social/` | `IntelliVerseX.Social` |
| `Leaderboard/` | `IntelliVerseX.Leaderboard` |
| `Editor/` | `IntelliVerseX.Editor` |

---

## Read-Only Zones (NEVER Touch)

```
Assets/Nakama/           - Third-party SDK
Assets/Photon/           - Third-party SDK
Assets/Appodeal/         - Third-party SDK
Assets/LevelPlay/        - Third-party SDK
Assets/AppleAuth/        - Third-party SDK
Assets/Plugins/Demigiant/ - Third-party (DOTween)
Library/, Temp/, Logs/   - Unity-managed
```

---

## Layer Architecture (Quick Reference)

```
Valid: Consumer Game → SDK Public API → SDK Internal → Third-Party
Invalid: Consumer Game → Third-Party (direct)

SDK Public API MUST NOT expose:
- Internal implementation details
- Third-party types directly
- Mutable state
```

---

## File Templates

| Need | Template |
|------|----------|
| New Manager | `.cursor/examples/MANAGER_TEMPLATE.cs` |
| New Service | `.cursor/examples/SERVICE_TEMPLATE.cs` |
| New Interface | `.cursor/examples/INTERFACE_TEMPLATE.cs` |
| New Config | `.cursor/examples/CONFIG_TEMPLATE.cs` |

---

## Naming Quick Reference

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXIdentityManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private Fields | `_camelCase` | `_isInitialized` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Events | `On` + PascalCase | `OnAuthStateChanged` |
| Async Methods | + `Async` suffix | `SignInAsync()` |

---

## UPM Quick Reference

| Need | Location |
|------|----------|
| Package manifest | `Assets/_IntelliVerseXSDK/package.json` |
| Changelog | `Assets/_IntelliVerseXSDK/CHANGELOG.md` |
| Documentation | `Assets/_IntelliVerseXSDK/Documentation~/` |

---

## More Of Us (Cross-Promo) — Quick Ops

**Key files**
- UI controller: `Assets/_IntelliVerseXSDK/MoreOfUs/UI/IVXMoreOfUsCanvas.cs`
- Prefab builder (Editor): `Assets/_IntelliVerseXSDK/MoreOfUs/Editor/IVXMoreOfUsPrefabBuilder.cs`

**Prefab generation**
- Menu: `IntelliVerseX → More Of Us → Build Canvas Prefab` (or `Build All Prefabs`)
- Adds to scene: `IntelliVerseX → More Of Us → Add To Current Scene`

**Editor platform simulation**
- Uses `UnityEditor.EditorUserBuildSettings.activeBuildTarget` for Android vs iOS filtering.
- If build target is Standalone/WebGL, expected behavior is “no apps” / panel not shown.

---

## Setup Wizard — Version Check

- Setup Wizard: `Assets/_IntelliVerseXSDK/Editor/IVXSDKSetupWizard.cs`
- Shows current SDK version and checks GitHub Releases for updates.

---

*Last verified: 2026-01-13 | See AGENT.md for full context*
