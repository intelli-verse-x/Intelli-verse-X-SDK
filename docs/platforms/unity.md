# Unity Engine / .NET

> The Unity SDK is the most feature-complete IntelliVerseX implementation, with native support for Hiro, Satori, monetization, localization, social features, and more.

## Requirements

| Unity Version | Status |
|---------------|--------|
| **Unity 6000.x** | Fully Supported |
| **Unity 2023.3 LTS** | Fully Supported |
| Unity 2022.x | May work |
| Unity 2021.x | Not Supported |

**Platforms:** Android, iOS, WebGL, Windows, macOS, Linux

**XR / VR / AR:** Meta Quest, SteamVR (OpenXR), Apple Vision Pro (visionOS), PSVR2, Windows Mixed Reality, AR Foundation (ARKit/ARCore). See [XR/VR/AR guide](xr-vr-ar.md).

**Consoles:** PS5, Xbox Series X|S, Nintendo Switch (via adapter pattern, NDA SDKs required). See [Console guide](console.md).

## Installation

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

With specific version tag:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v5.8.0"
  }
}
```

## Quick Start

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

For production, prefer dropping an **`IVXBootstrap`** prefab (or scene object) and assigning **`IVXBootstrapConfig`** so Nakama, optional Discord, and feature modules initialize in one place. See [Platform-specific configuration](#platform-specific-configuration) below.

## API Surface

Key Unity entry points and managers. Names reflect the public types in the package; a few rows note **primary types** where aliases or split APIs are common in docs or samples.

| Class | Role |
|-------|------|
| **IVXBootstrap** | One-drop scene initializer: loads `IVXBootstrapConfig`, starts core services, optional Discord, and module hooks. |
| **IVXBootstrapConfig** | ScriptableObject wiring for bootstrap (Nakama host, keys, Discord asset reference, feature toggles). |
| **IntelliVerseXConfig** | Core game ScriptableObject / serialized config (server URLs, game id, nested **`IVXAdsConfig`**, feature flags). Often referred to informally as “game config”; there is no separate type named `IVXGameConfig`. |
| **IntelliVerseXUserIdentity** | Device and linked auth flows, session restore hooks, profile identifiers for backend calls. |
| **IVXAISessionManager** | AI voice/host session lifecycle, persona and entitlement integration with the AI backend. |
| **IVXAINPCDialogManager** | Registers NPC profiles, starts dialog sessions, sends player lines, surfaces `OnNPCResponse` / `OnNPCAction`. |
| **IVXAIAssistant** | In-game assistant queries (ask/search) against the IVX AI assistant HTTP API. |
| **IVXAIModerator** | Content moderation requests (text/media policies) for UGC or chat. |
| **IVXAIContentGenerator** | Server-driven content generation (trivia, copy, structured payloads) for live ops. |
| **IVXAIProfiler** | Player modeling / profiling calls for personalization and segmentation. |
| **IVXAIVoiceServices** | TTS/STT and related voice pipeline for hosts and NPCs. |
| **IVXDiscordManager** | Discord Social SDK entry point: init, connect, error routing. |
| **IVXDiscordPresence** | Rich presence and activity updates (requires **`IVXDiscordConfig`**). |
| **IVXDiscordFriends** | Unified friends, invites, and social graph helpers on top of the Discord stack. |
| **IVXGameModeManager** | Match phase state machine (lobby → matchmaking → in-match); shared by lobby and matchmaking. |
| **IVXLobbyManager** | Nakama-backed (or mock) room create/join/list/leave. |
| **IVXMatchmakingManager** | Matchmaker tickets, search progress events, cancel; integrates with **`IIVXNakamaRealtimeProvider`** when Nakama is present. |
| **IVXHiroRpcClient** | Typed wrapper over Nakama `RpcAsync` for Hiro: `CallAsync<T>` / `CallVoidAsync`, session refresh, JSON envelope (`HiroRpcResponse<T>`). Used by spin wheel, streaks, retention, offerwall RPCs, and higher-level systems. |
| **IVXSatoriManager** | *Not a shipped type name.* Use **`IVXSatoriClient`** and **`IVXSatoriRpcClient`** (initialize from bootstrap / your service). |
| **IVXSatoriClient** / **IVXSatoriRpcClient** | Satori analytics and server RPCs (events, flags, experiments, live events). |
| **IVXWalletManager** | Static API for wallet fetch/grant and economy hooks via Nakama. |
| **IVXLeaderboardManager** | *Not a shipped type name.* The leaderboard API is **`IVXGLeaderboardManager`** (static) and **`IVXGLeaderboard`** (scene component). |
| **IVXGLeaderboardManager** / **IVXGLeaderboard** | Leaderboard submit/fetch (`IVXGLeaderboardManager` static) and MonoBehaviour façade (`IVXGLeaderboard`). |
| **IVXIAPManager** | Purchase flow, receipt validation hooks, catalog integration. |
| **IVXAdsManager** | Interstitial, rewarded, banner orchestration (network-specific backends). |
| **IVXOfferwallManager** | Offerwall display and completion callbacks (Pubscale/Xsolla-style integrations). |
| **IVXLocalizationManager** | *Not a shipped type name.* Primary entry point is **`IVXLocalizationService`** (singleton-style access) plus **`IVXLocalizationHelper`** utilities and **`IVXLocalizedText`** for UI. |
| **IVXLocalizationService** / **IVXLocalizationHelper** | Runtime language selection, string resolution, RTL; works with Unity Localization when installed. |
| **IVXCloudStorageManager** | *Not a standalone shipped type.* Server-side object storage in the matrix is **not** exposed as a dedicated Unity manager—use Nakama **`IClient` read/write storage** from the Backend module, or **`IVXSecureStorage`** for device-local secrets. |
| **IVXSecureStorage** | Local encrypted/safe persistence (not Nakama cloud storage—see [Feature coverage](#feature-coverage)). |
| **IVXQuizManager** | *No type with this exact name in the package.* Use **`IVXDailyQuizManager`**, **`IVXWeeklyQuizManager`**, **`IVXQuizPlayController`**, and related Quiz assembly types per mode. |
| **IVXDailyQuizManager** / **IVXWeeklyQuizManager** / **IVXQuizPlayController** | Daily/weekly quiz pipelines and play UI. |
| **IVXXRPlatformHelper** | XR runtime detection (Quest, SteamVR, OpenXR, etc.) and capability queries. |
| **IVXConsoleManager** | Console adapter surface (PS5/Xbox/Switch) for builds with NDA platform SDKs—stub-friendly until wired. |

**Related systems (often composed with `IVXHiroRpcClient` or dedicated MonoBehaviours):** `IVXDailyRewardsBackendService`, `IVXDailyMissionsManager`, `IVXSeasonPassManager`, `IVXFortuneWheelManager`, `IVXLeagueManager`, `IVXTournamentManager`, `IVXAchievementManager`, `IVXBadgeManager`, `IVXFriendStreakManager`, `IVXRetentionManager`, `IVXGoalsManager`, `IVXCharacterManager`, `IVXPushNotificationManager`, `IVXDeepLinkManager`, and Hiro subsystem types under `IntelliVerseX.Hiro.Systems`.

## Feature Coverage

Values are taken from the [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md) for **Unity** only.

**Legend:** `Y` = Implemented | `S` = Stub / partial platform adapter | `P` = Partial | `-` = Not present

### Core

| Feature | Unity |
|---------|:-----:|
| SDK Init / Config | Y |
| Device Auth | Y |
| Email Auth | Y |
| Google Auth | P |
| Apple Auth | P |
| Custom Auth | Y |
| Session Restore | Y |
| Profile Fetch | Y |
| Profile Update | Y |
| Wallet Fetch | Y |
| Wallet Grant | Y |
| Leaderboard Submit | Y |
| Leaderboard Fetch | Y |
| Cloud Storage Read | - |
| Cloud Storage Write | - |
| Generic RPC | Y |
| Real-time Socket | P |
| Events / Callbacks | Y |

### AI Voice

| Feature | Unity |
|---------|:-----:|
| AI Initialize | Y |
| Voice Session (start/end) | Y |
| Voice Send Text | Y |
| Voice Poll Messages | Y |
| Host Session (start/event) | Y |
| Entitlement Check | Y |
| Get Personas | Y |

### AI LLM Stack

| Feature | Unity |
|---------|:-----:|
| NPC Dialog Manager | Y |
| AI Assistant | Y |
| AI Content Moderator | Y |
| AI Content Generator | Y |
| AI Player Profiler | Y |
| AI Voice Services (TTS/STT) | Y |

### Multiplayer

| Feature | Unity |
|---------|:-----:|
| Select Mode | Y |
| Add/Remove Player | Y |
| Start/End Match | Y |
| Lobby Create Room | Y |
| Lobby Join Room | Y |
| Lobby List Rooms | Y |
| Lobby Leave Room | Y |
| Matchmaking Find | Y |
| Matchmaking Cancel | Y |
| Local MP Session | Y |
| Local MP Turns | Y |
| Local MP Split Screen | Y |

### Hiro

| Feature | Unity |
|---------|:-----:|
| Spin Wheel (get/spin) | Y |
| Streaks (get/update/claim) | Y |
| Offerwall (get/complete/claim) | Y |
| Friend Quests | Y |
| Friend Battles | Y |
| IAP Trigger | Y |
| Smart Ad Timer | Y |
| Retention (get/update) | Y |

### Discord

| Feature | Unity |
|---------|:-----:|
| Discord Init / Connect | Y |
| Account Linking (OAuth2) | Y |
| Rich Presence / Activity | Y |
| Unified Friends List | Y |
| Lobby / Text Chat | Y |
| Voice Chat | Y |
| Game Invites | Y |
| Direct Messages | Y |
| Moderation | Y |
| Linked Channels | Y |
| Debug / Logging | Y |
| Social Settings | Y |

### Satori

| Feature | Unity |
|---------|:-----:|
| Initialize / Authenticate | Y |
| Capture Events | Y |
| Feature Flags | Y |
| Experiments / A-B Testing | Y |
| Live Events | Y |
| Identity Update / Logout | Y |

### Platform Extras

| Feature | Unity |
|---------|:-----:|
| Deep Links | Y |
| Safe Area / Edge-to-Edge | Y |
| Foldable Device Support | Y |
| Performance Optimizer | Y |
| Bootstrap (one-drop init) | Y |
| Demo UIs | Y (16) |
| Web3 / NFT Gating | - |
| WebGL / Web | Y |

### Nakama Live Systems

| Feature | Unity |
|---------|:-----:|
| Push Notifications | Y |
| Daily Rewards (Backend) | Y |
| Daily Missions | Y |
| League System (6-tier) | Y |
| Fortune Wheel | Y |
| Achievements | Y |
| Badges (56 badges) | Y |
| Retention v2 + Winback | Y |
| Season Pass | Y |
| Weekly/Monthly Goals | Y |
| Friend Streaks | Y |
| Character System | Y |
| Tournaments | Y |

### XR / Console (Unity column)

| Feature | Unity |
|---------|:-----:|
| XR Platform Detection | Y |
| Meta Quest Support | Y |
| SteamVR Support | Y |
| Apple Vision Pro Support | S |
| AR Foundation / ARKit / ARCore | Y |
| PSVR2 Support | - |
| Hand Tracking Info | S |
| Eye Tracking Info | S |
| Passthrough / MR Info | S |
| Console (PS5/Xbox/Switch) | S |
| Linux / SteamOS | S |
| tvOS | - |
| visionOS | S |

At **v5.8.0**, Unity is the reference platform (**~82%** fully implemented in the audited matrix). For cross-engine comparison, keep using the full matrix.

## Platform-Specific Configuration

### ScriptableObjects and assets

| Asset / type | Purpose |
|--------------|---------|
| **IntelliVerseXConfig** | Primary game configuration (backend URLs, identifiers, ads block via **`IVXAdsConfig`**). |
| **IVXBootstrapConfig** | Bootstrap wiring: Nakama, AI, Discord **`IVXDiscordConfig`** reference, module enable flags. |
| **IVXDiscordConfig** | Discord application id, scopes, redirect URIs, feature toggles for Social SDK. |
| **IVXAIConfig** | Base URL, API keys, timeouts for AI/NPC/assistant endpoints (referenced by AI managers). |

Create assets from the Unity menu where **`CreateAssetMenu`** is defined (e.g. *IntelliVerseX → Discord Config*). The **SDK Setup Wizard** (Editor) can generate common assets under `Assets/_IntelliVerseXSDK/`.

### Define symbols (automatic)

The editor **`IVXDefineSymbolManager`** detects optional third-party assemblies and adds **`INTELLIVERSEX_*`** defines so runtime code can compile against Nakama, Photon, DOTween, ad mediation, etc.

| Symbol | When set |
|--------|----------|
| `INTELLIVERSEX_SDK` | Always (SDK present). |
| `INTELLIVERSEX_HAS_NAKAMA` | Nakama assemblies detected. |
| `INTELLIVERSEX_HAS_PHOTON` | Photon PUN / Realtime detected. |
| `INTELLIVERSEX_HAS_DOTWEEN` | DOTween detected. |
| `INTELLIVERSEX_HAS_APPODEAL` | Appodeal SDK detected. |
| `INTELLIVERSEX_HAS_LEVELPLAY` | Unity LevelPlay detected. |
| `INTELLIVERSEX_HAS_NATIVE_SHARE` | Native Share plugin detected. |
| `INTELLIVERSEX_HAS_APPLE_SIGNIN` | Apple Auth detected. |

Game code uses these in `#if` regions—for example, **`IVXMatchmakingManager`** only calls `RemoveMatchmakerAsync` when **`INTELLIVERSEX_HAS_NAKAMA`** is defined. If you expect Nakama but the symbol is missing, the package is not referenced or assemblies failed to compile.

!!! note "Naming in docs vs code"
    Some guides abbreviate optional flags as `IVX_NAKAMA` or `IVX_HIRO`. The **actual** symbols managed by the SDK are the **`INTELLIVERSEX_HAS_*`** and **`INTELLIVERSEX_SDK`** names above. Hiro and Satori do not use separate global define symbols in this manager—they are enabled when their assemblies are included in the project.

### Assembly definitions

Each functional area ships as an **`.asmdef`** (see [Module reference – Assembly Definitions](../modules/index.md#assembly-definitions)): `IntelliVerseX.Core`, `IntelliVerseX.Identity`, `IntelliVerseX.Backend`, `IntelliVerseX.AI`, `IntelliVerseX.Hiro`, `IntelliVerseX.Satori`, `IntelliVerseX.Demos`, etc. Reference only the assemblies your game needs to keep compile times and IL2CPP builds lean.

## Advanced Examples

### Hiro-style daily rewards (Nakama RPC)

Use **`IVXDailyRewardsBackendService`** after the player has a valid Nakama session (typically via your Nakama bootstrap / **`IVXNManager`**). The service uses the same RPC ids as the backend (`daily_rewards_get_state`, `daily_rewards_claim`, `daily_rewards_get_calendar`).

```csharp
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.DailyRewards;

public class DailyRewardPrompt : MonoBehaviour
{
    public async Task TryClaimDailyAsync()
    {
        var svc = IVXDailyRewardsBackendService.Instance;
        if (svc == null)
        {
            Debug.LogError("Add IVXDailyRewardsBackendService to the scene.");
            return;
        }

        var state = await svc.GetStateAsync();
        if (state == null || !state.canClaim)
            return;

        var result = await svc.ClaimTodayAsync();
        if (result != null)
            Debug.Log($"Claimed day {result.day}, streak {result.streak}");
    }
}
```

For other Hiro systems, instantiate **`IVXHiroRpcClient`** with `IClient` + `ISession` and call your RPC id with `CallAsync<T>(...)`.

### AI NPC conversation

```csharp
using UnityEngine;
using IntelliVerseX.AI;

public class ShopkeeperNPC : MonoBehaviour
{
    [SerializeField] private IVXAIConfig _aiConfig;
    [SerializeField] private IVXAINPCProfile _profile; // configured in Inspector or via code

    private void Start()
    {
        var mgr = IVXAINPCDialogManager.Instance;
        mgr.Initialize(_aiConfig);
        mgr.RegisterNPC(_profile);

        mgr.OnNPCResponse += (sessionId, text) =>
            Debug.Log($"NPC: {text}");
    }

    public void OpenDialog(string playerId)
    {
        var mgr = IVXAINPCDialogManager.Instance;
        mgr.StartDialog(_profile.NpcId, playerId, "Standing near the shop counter.");

        // After StartDialog, capture session id from callback or ActiveSessions if needed,
        // then mgr.SendMessage(sessionId, playerLine, _ => { });
    }
}
```

### Multiplayer matchmaking

```csharp
using UnityEngine;
using IntelliVerseX.GameModes;

public class PlayOnlineButton : MonoBehaviour
{
    private void OnEnable()
    {
        IVXMatchmakingManager.Instance.OnMatchFound += OnMatchFound;
        IVXMatchmakingManager.Instance.OnSearchCancelled += reason =>
            Debug.LogWarning($"Matchmaking cancelled: {reason}");
    }

    private void OnDisable()
    {
        IVXMatchmakingManager.Instance.OnMatchFound -= OnMatchFound;
    }

    public void QuickPlay()
    {
        // Assign IVXNManager (or provider) on IVXMatchmakingManager in the Inspector for live Nakama.
        IVXMatchmakingManager.Instance.QuickMatch();
    }

    private void OnMatchFound(IVXMatchFoundResult result)
    {
        Debug.Log($"Matched: {result.OpponentDisplayName}");
    }
}
```

## Troubleshooting

| Symptom | Likely cause | What to do |
|---------|----------------|------------|
| `INTELLIVERSEX_HAS_NAKAMA` missing; multiplayer or Hiro RPCs never hit the server | Nakama package not imported or asmdef not referenced | Add **nakama-unity** per [Backend](../modules/backend.md). Re-open the project or trigger a script recompile so **`IVXDefineSymbolManager`** runs. |
| Nakama connection / RPC fails immediately | Wrong host/port/scheme, TLS mismatch, or expired session | Verify **`IntelliVerseXConfig`** / bootstrap server URL. Refresh session with **`IntelliVerseXUserIdentity`** (or your auth flow) before **`IVXHiroRpcClient`**. |
| WebGL build: HTTP failures or “CORS” in browser console | Browser blocks cross-origin calls | Serve Nakama and AI APIs with proper **CORS** headers for your WebGL origin; use **HTTPS** consistently. Test behind the same site or a configured proxy. |
| Android: cleartext HTTP blocked | Android 9+ network security | Use **HTTPS** or add a **network security config** for dev hosts only (not for production cleartext). |
| Android manifest merge errors with ads / Play Services | Conflicting `AndroidManifest.xml` from plugins | Use Unity’s **Custom Main Manifest** and resolve merger conflicts; align **minSdk** with mediation SDKs. |
| iOS: Sign in with Apple or keychain errors | Missing capability or entitlement | Enable **Sign In with Apple** capability; ensure **`INTELLIVERSEX_HAS_APPLE_SIGNIN`** when using AppleAuth. |
| Discord features no-op | Missing **`IVXDiscordConfig`** or uninitialized **`IVXDiscordManager`** | Assign config asset on the manager; complete OAuth flow per [Discord module](../modules/discord.md). |
| IL2CPP strip removes Nakama or JSON types | Code stripping too aggressive | Link.xml / preserve attributes for affected assemblies if you see deserialization failures at runtime. |

## Nakama Client

Built on [nakama-unity](https://github.com/heroiclabs/nakama-unity) (468 stars, 82 forks).

## Full Documentation

See the [Unity modules documentation](../modules/index.md) for complete API reference.

## Source

[Assets/\_IntelliVerseXSDK/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/Assets/_IntelliVerseXSDK)

## Further Reading

- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md) — All platforms side by side.
- [Modules index](../modules/index.md) — Architecture, dependencies, assembly list.
- [API reference index](../api/index.md) — Generated-style API pages: [Core](../api/core.md), [Identity](../api/identity.md), [Backend](../api/backend.md), [AI](../api/ai.md), [Hiro](../api/hiro.md), [Multiplayer](../api/multiplayer.md), [Satori](../api/satori.md), [Discord](../api/discord.md), [Monetization](../api/monetization.md), [Localization](../api/localization.md), [Leaderboards](../api/leaderboards.md), [Quiz](../api/quiz.md), [Platform](../api/platform.md), [Demos](../modules/demos.md).
- [Getting started / Quickstart](../getting-started/quickstart.md) — End-to-end bootstrap checklist.
- [XR / VR / AR](xr-vr-ar.md) and [Console](console.md) platform guides.
- [Configuration: feature flags](../configuration/feature-flags.md) — Remote and local toggles.
