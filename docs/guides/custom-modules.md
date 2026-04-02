# Custom Modules Guide

Extend the IntelliVerseX Unity SDK with game-specific or studio-specific features while staying inside the same layering, naming, and assembly rules as first-party modules.

---

## Overview

### When to create a custom module

- You need **game-specific services** (rewards, seasons, narrative flags) that should not live in `_IntelliVerseXSDK` forks.
- You want a **clean boundary** between “platform SDK” and “our title,” with optional open-source or private packages per game.
- You are integrating **custom Nakama RPCs** or third-party REST APIs behind a single manager API for UI and gameplay code.

### Why follow this structure

- **Assembly definitions** keep compile times down and enforce dependency direction (your code references Core/Backend; Core does not reference you).
- **ScriptableObject config** matches how `IVXBootstrapConfig`, `IVXDiscordConfig`, and other IVX modules expose tunables to designers.
- **Singleton managers** align with `IVXSafeSingleton<T>` and existing modules (Discord, Multiplayer, AI) so lifecycle and `DontDestroyOnLoad` behave predictably.

---

## Module Structure

Place custom code under a folder that is **yours** (e.g. `Assets/_YourGame/IntelliVerseX.Rewards/` or `Packages/com.yourstudio.ivx.rewards/`). A typical layout:

```
IntelliVerseX.YourModule/
├── IntelliVerseX.YourModule.asmdef      # Runtime assembly
├── Runtime/
│   ├── IVXYourModuleConfig.cs          # ScriptableObject settings
│   ├── IVXYourModuleManager.cs         # Singleton service / MonoBehaviour
│   ├── IVXYourModuleModels.cs          # DTOs, enums (optional)
│   └── Services/
│       └── IVXYourModuleApiClient.cs   # HTTP or thin Nakama wrapper (optional)
├── Editor/
│   ├── IntelliVerseX.YourModule.Editor.asmdef
│   ├── IVXYourModuleConfigEditor.cs    # Custom inspector (optional)
│   └── IVXYourModuleMenu.cs            # Menu items (optional)
└── README.md                           # Team notes (optional)
```

| Path | Role |
|------|------|
| `*.asmdef` | Defines the assembly name and references to IntelliVerseX (and Unity) |
| `Runtime/*` | Everything that ships in player builds |
| `IVXYourModuleConfig` | Designer-facing toggles, endpoints, feature flags |
| `IVXYourModuleManager` | Public API, events, coordination |
| `Editor/*` | Inspectors and `IntelliVerseX/...` menu entries; never referenced by runtime asmdef |

---

## Step 1: Assembly Definition

Create `IntelliVerseX.YourModule.asmdef` next to your `Runtime` scripts. Reference **`IntelliVerseX.Core`** at minimum. Add **`IntelliVerseX.Backend`** when you call Nakama or `IVXNakamaRPC`.

**Runtime example:**

```json
{
  "name": "IntelliVerseX.YourModule",
  "rootNamespace": "IntelliVerseX.YourModule",
  "references": [
    "IntelliVerseX.Core",
    "IntelliVerseX.Backend"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**Editor assembly** (`Editor/IntelliVerseX.YourModule.Editor.asmdef`) should reference your runtime asmdef and **`IntelliVerseX.Editor`** (and optionally `UnityEditor` is implicit):

```json
{
  "name": "IntelliVerseX.YourModule.Editor",
  "rootNamespace": "IntelliVerseX.YourModule.Editor",
  "references": [
    "IntelliVerseX.YourModule",
    "IntelliVerseX.Editor"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

---

## Step 2: Module Config

Use the **`IVX` prefix** and a clear module name. Store only data — no static service locator here.

```csharp
using UnityEngine;

namespace IntelliVerseX.YourModule
{
    [CreateAssetMenu(
        fileName = "IVXYourModuleConfig",
        menuName = "IntelliVerseX/Your Module Config",
        order = 100)]
    public sealed class IVXYourModuleConfig : ScriptableObject
    {
        [Header("Backend")]
        [SerializeField] private string _claimRewardRpcId = "your_game_claim_reward";

        [Header("Behavior")]
        [SerializeField] private bool _enableVerboseLogs;

        public string ClaimRewardRpcId => _claimRewardRpcId;
        public bool EnableVerboseLogs => _enableVerboseLogs;
    }
}
```

---

## Step 3: Module Manager

Use **`IVXSafeSingleton<T>`** from `IntelliVerseX.Core` — this is the SDK’s standard base for MonoBehaviour singletons (thread-safe lazy creation, `DontDestroyOnLoad`, `OnInitialize` / `OnCleanup`). Project documentation sometimes refers to “IVX singleton managers”; this is the concrete type to extend.

Override **`OnInitialize()`** instead of `Awake` for startup logic. Use **`OnCleanup()`** for teardown (called from the base `OnDestroy` path).

```csharp
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.YourModule
{
    public sealed class IVXYourModuleManager : IVXSafeSingleton<IVXYourModuleManager>
    {
        [SerializeField] private IVXYourModuleConfig _config;

        private bool _ready;

        public bool IsReady => _ready;

        public event Action<int> OnCreditsGranted;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (_config == null)
            {
                Debug.LogError($"[{nameof(IVXYourModuleManager)}] Config is not assigned.");
                return;
            }
            _ready = true;
            Debug.Log($"[{nameof(IVXYourModuleManager)}] Initialized.");
        }

        protected override void OnCleanup()
        {
            _ready = false;
            base.OnCleanup();
        }

        /// <summary>
        /// Example public API — perform work on a background thread where appropriate.
        /// </summary>
        public async Task GrantCreditsAsync(int amount)
        {
            if (!_ready || _config == null) return;
            // See Step 6 for Nakama RPC wiring
            await Task.Yield();
            OnCreditsGranted?.Invoke(amount);
        }
    }
}
```

**Events:** Prefer `Action` / `Action<T>` for decoupling UI from services, matching `IVXBootstrap` and Discord managers.

**Do not** allocate in `Update`; cache references in `OnInitialize` or `Awake`.

---

## Step 4: Editor Integration

Keep editor-only code in the **Editor** asmdef under namespace **`IntelliVerseX.YourModule.Editor`** (or `IntelliVerseX.Editor` for shared studio tools if your team agrees).

**Custom inspector (optional):**

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.YourModule.Editor
{
    [CustomEditor(typeof(IVXYourModuleConfig))]
    public sealed class IVXYourModuleConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Assign RPC ids that exist on your Nakama server.", MessageType.Info);
        }
    }
}
#endif
```

**Menu item (optional):**

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    public static class IVXYourModuleMenu
    {
        [MenuItem("IntelliVerseX/Your Module/Create Config Asset")]
        public static void CreateConfig()
        {
            var asset = ScriptableObject.CreateInstance<IntelliVerseX.YourModule.IVXYourModuleConfig>();
            ProjectWindowUtil.CreateAsset(asset, "IVXYourModuleConfig.asset");
        }
    }
}
#endif
```

---

## Step 5: Register Module (IVXBootstrap)

`IVXBootstrap` is **sealed** and owns a fixed initialization pipeline (platform → backend → Hiro/Satori → Discord → AI → multiplayer). Third-party modules are not registered inside that class.

**Recommended patterns:**

1. **Subscribe to bootstrap completion** — After `IVXBootstrap` finishes, touch your manager or run async init:

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;
using IntelliVerseX.YourModule;

public sealed class IVXYourModuleRegistration : MonoBehaviour
{
    private async void Start()
    {
        var boot = IVXBootstrap.Instance;
        if (boot == null) return;

        if (!boot.IsInitialized)
            await boot.InitializeAsync();

        if (boot.IsInitialized)
            _ = IVXYourModuleManager.Instance;
    }
}
```

Alternatively, subscribe only to **`OnBootstrapComplete`** (without awaiting) if another component triggers `InitializeAsync` and you want a single event-driven hook.

2. **Execution order** — Place your registrar component **after** `IVXBootstrap` in the Script Execution Order project settings if you must avoid awaiting.

3. **Manual auth** — If `AutoDeviceAuth` is off, call `IVXBootstrap.SetAuth` before `InitializeAsync`, then initialize your module when the session is ready.

---

## Step 6: Nakama RPC Integration

Use **`IVXNakamaRPC`** from `IntelliVerseX.Backend` for typed JSON, or `IClient.RpcAsync` directly if you already hold `IClient` and `ISession`.

**Pattern with `IVXNakamaRPC`:**

```csharp
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using Nakama;

public static class IVXYourModuleBackend
{
    public static async Task<ClaimRewardResponse> ClaimRewardAsync(
        IClient client,
        ISession session,
        IVXYourModuleConfig config,
        string rewardId)
    {
        var req = new ClaimRewardRequest { rewardId = rewardId };
        return await IVXNakamaRPC.CallRPC<ClaimRewardRequest, ClaimRewardResponse>(
            client,
            session,
            config.ClaimRewardRpcId,
            req);
    }
}

[System.Serializable]
public class ClaimRewardRequest { public string rewardId; }

[System.Serializable]
public class ClaimRewardResponse { public bool ok; public string error; }
```

Resolve `IClient` / `ISession` from your game’s Nakama bootstrap path (often the same session `IVXBootstrap` established). Never embed server keys in client code; use `IVXBootstrapConfig` and environment-appropriate builds.

---

## Best Practices

| Topic | Guideline |
|-------|-----------|
| **Naming** | Public types: `IVX` + PascalCase (`IVXSeasonPassManager`). Interfaces: `IIVX...` |
| **Namespace** | `IntelliVerseX.YourModule` for runtime; `.Editor` suffix for editor |
| **Null safety** | Guard `Instance`, `Config`, and Nakama session before RPC |
| **Logging** | `Debug.Log($"[{nameof(IVXYourModuleManager)}] ...")` |
| **Hot paths** | No LINQ, no `GetComponent`/`Find` in `Update`, avoid per-frame allocations |
| **Secrets** | No API keys in ScriptableObjects committed to public repos; use server RPC |

---

## Example: Custom Rewards Module (end-to-end)

1. Add **`IntelliVerseX.Rewards.asmdef`** with references to **Core** and **Backend**.
2. Create **`IVXRewardsConfig`** with `claimRpcId` and max daily claims.
3. Implement **`IVXRewardsManager : IVXSafeSingleton<IVXRewardsManager>`** with `TryClaimAsync()` calling `IVXNakamaRPC.CallRPC`.
4. Add **`IVXRewardsRegistration`** `MonoBehaviour` that waits for `IVXBootstrap` and then enables daily UI.
5. Implement Nakama RPC `your_game_claim_reward` server-side to validate cooldowns and grant Hiro currency.

This mirrors how first-party modules isolate **config → manager → backend** without modifying package sources under `Assets/Intelli-verse-X-SDK/` in consumer projects.

---

## Cross-Platform Considerations

The Unity SDK targets **Android, iOS, WebGL, Standalone**. Custom modules should:

- **Abstract network and file I/O** behind interfaces if you plan to share logic with headless tools or other engines.
- Avoid **platform-specific APIs** in the core runtime type; use `#if UNITY_IOS` small shims or separate platform assemblies.
- For **WebGL**, prefer Nakama HTTP/WebSocket paths already supported by the main SDK; test IL2CPP stripping with link.xml if you use heavy reflection for RPC DTOs.

If you later port concepts to **JavaScript / Godot / Unreal**, keep DTOs and RPC ids stable so the same Nakama server serves all clients.

---

## See Also

- [Core module](../modules/core.md) — `IVXSafeSingleton`, foundations
- [Backend module](../modules/backend.md) — Nakama client patterns
- [Architecture ADR index](../architecture/adr/README.md) — structural decisions
- [Nakama integration](./nakama-integration.md) — RPC and server expectations
- [API: Backend](../api/backend.md) — public backend types
- Repository **`.cursor/naming-and-style.md`** and **`architecture.md`** (naming, layers) — authoritative conventions for IntelliVerseX SDK contributors
