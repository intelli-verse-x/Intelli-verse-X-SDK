# Unreal Engine

> IntelliVerseX plugin for Unreal Engine 5, with full Blueprint and C++ support. The Unreal SDK mirrors the Unity architecture—`UIVXManager` on the `GameInstance`, Nakama-backed auth and storage, Hiro live-ops via RPC helpers, and optional AI, Discord, Satori, XR, and console (OnlineSubsystem) surfaces.

## Requirements

- Unreal Engine 5.3+
- [Nakama Unreal Plugin](https://github.com/heroiclabs/nakama-unreal) v2.8+ (Unreal module `NakamaUnreal`)
- C++17 compiler
- **Target platforms:** The plugin’s `IntelliVerseX.uplugin` currently allow-lists Win64, Mac, Linux, Android, and iOS. Console targets require additional first-party OnlineSubsystem plugins and platform SDKs (see [Console](console.md)).

## Installation

1. Copy `SDKs/unreal/` into your project’s `Plugins/IntelliVerseX/` directory (or merge into your monorepo’s `Plugins` tree).
2. Install the [Nakama Unreal Plugin](https://heroiclabs.com/docs/nakama/client-libraries/unreal/) per Heroic Labs’ instructions.
3. Enable both plugins in your `.uproject`. The exact **plugin name** shown in the Editor may be `Nakama`, `NakamaUnreal`, or similar depending on how the Nakama plugin is packaged—use the identifier your engine lists under **Edit → Plugins**:

```json
{
  "Plugins": [
    { "Name": "NakamaUnreal", "Enabled": true },
    { "Name": "IntelliVerseX", "Enabled": true }
  ]
}
```

4. Add the IntelliVerseX module to your game module’s `Build.cs`:

```csharp
PublicDependencyModuleNames.Add("IntelliVerseX");
```

5. Regenerate project files, build, and launch the Editor once so Blueprint types compile.

## Quick Start (C++)

```cpp
#include "IVXManager.h"

void AMyGameMode::BeginPlay()
{
    Super::BeginPlay();

    UIVXManager* IVX = GetGameInstance()->GetSubsystem<UIVXManager>();
    UIVXConfig* Config = LoadObject<UIVXConfig>(nullptr, TEXT("/Game/Config/DA_IVXConfig"));
    IVX->InitializeSDK(Config);

    IVX->OnInitialized.AddDynamic(this, &AMyGameMode::OnIVXReady);
    IVX->AuthenticateWithDevice(FString());
}
```

Bind `OnAuthenticated`, `OnError`, `OnProfileLoaded`, `OnWalletLoaded`, `OnLeaderboardFetched`, `OnStorageRead`, and `OnRpcResult` as needed. After a successful session exists, subsystems such as `UIVXHiroSystems` and `UIVXMultiplayer` expect `SetNakamaClient` to be called with the same `UNakamaClient` and `UNakamaSession` your game uses (often wired from a small C++ helper or gameplay code that holds Nakama references).

## Blueprint Quick Start

Blueprint-only projects can drive the SDK without a custom `GameMode` subclass by using **Level Blueprint** or a **Blueprint Actor** that runs on begin play.

**Recommended node graph (high level):**

1. **Event BeginPlay** → **Get Game Instance** → **Get Subsystem** (class: `UIVXManager`).
2. **Load Asset** or a **soft reference** to your `UIVXConfig` Data Asset (e.g. `/Game/Config/DA_IVXConfig`) → cast to `UIVXConfig` if needed.
3. Call **Initialize SDK** on the subsystem with that config.
4. From **On Initialized** (multicast on `UIVXManager`), call **Authenticate With Device** (pass an empty string to let the SDK generate/persist a device id) or **Authenticate With Email** / **Authenticate With Google** / **Authenticate With Apple** as appropriate.
5. Bind **On Authenticated** to gameplay logic (e.g. open main menu, call **Fetch Profile**, **Fetch Wallet**).
6. Bind **On Error** to print string or show a UI dialog.

**Tips:**

- Use **Validate Nakama Config** (static on `UIVXManager`) in editor or debug builds to catch empty host/key before shipping.
- **Restore Session** attempts to reload a stored Nakama session; **Clear Session** logs out locally.
- Categories in the Blueprint picker are grouped under **IntelliVerseX**, **IntelliVerseX|Auth**, **IntelliVerseX|Wallet**, etc., matching the C++ `UFUNCTION` metadata.

## API Surface

The tables below list the primary Blueprint-exposed types. Some Unity-style names you may see in cross-platform docs map to these Unreal types (see notes).

### Core and configuration

| Class | Access pattern | Description |
|-------|----------------|-------------|
| `UIVXManager` | `UGameInstance` subsystem | Central coordinator: init, auth (device, email, Google, Apple, custom), profile, wallet, leaderboards, storage, generic RPC, socket disconnect, diagnostics. |
| `UIVXConfig` | `UPrimaryData Asset` | Game id, Nakama host/port/server key, SSL, optional Cognito fields, analytics and debug flags. Create one asset per environment (dev/stage/prod). |

### Multiplayer and game modes

| Class / surface | Access pattern | Description |
|-----------------|----------------|-------------|
| `UIVXMultiplayer` | `Get IVX Multiplayer` (world context) | **Lobby:** create, join, leave, list. **Matchmaking:** start ticket, cancel. Requires `SetNakamaClient` + valid session. |
| `UIVXLobbySubsystem` | *Not a separate class* | Lobby APIs live on **`UIVXMultiplayer`** under category **IntelliVerseX\|Multiplayer\|Lobby**. |
| `UIVXMatchmaking` | *Not a separate class* | Matchmaking APIs live on **`UIVXMultiplayer`** under **IntelliVerseX\|Multiplayer\|Matchmaking**. |
| `UIVXGameModes` | `UGameInstance` subsystem | High-level mode orchestration: solo, local multiplayer, online versus/coop, ranked, turn-based; room/slot structs for UI and flow control. |

### Hiro live-ops (client)

| Class | Access pattern | Description |
|-------|----------------|-------------|
| `UIVXHiroClient` | Implemented as **`UIVXHiroSystems`** | Hiro RPC wrappers: **spin wheel** (get state, spin), **streaks** (get, update, claim milestone), **offerwall**, **friend quests**, **friend battles**, **retention**, **IAP trigger**, **smart ad** eligibility. |
| `UIVXHiroSystems` | `Get IVX Hiro Systems` (world context) | Same as above—use this type name in C++ and Blueprint search. |

### Satori

| Class | Access pattern | Description |
|-------|----------------|-------------|
| `UIVXSatoriSubsystem` | Implemented as **`UIVXSatori`** | Blueprint singleton (`GetInstance` with world context). Events, flags, experiments, live events—**stub surface**; wire to Satori HTTP/SDK when enabling production analytics. |
| `UIVXSatori` | `GetInstance` | Canonical Unreal type name. |

### AI

| Class | Access pattern | Description |
|-------|----------------|-------------|
| `UIVXAISessionManager` | Implemented as **`UIVXAIClient`** | REST client for **voice** sessions, **host** sessions, entitlements, personas; delegates for messages and errors. |
| `UIVXAIClient` | `Get IVX AI Client` | Canonical type for AI sessions. |
| `UIVXAINPCDialogManager` | `GetInstance` | NPC dialog orchestration (Blueprint surface; extend for your title). |
| `UIVXAIAssistant` | `GetInstance` | In-game assistant / help flows. |
| `UIVXAIModerator` | `GetInstance` | Content moderation hooks. |
| `UIVXAIContentGenerator` | `GetInstance` | Procedural content (e.g. **Generate Story** with JSON callback). |
| `UIVXAIProfiler` | `GetInstance` | AI usage / performance profiling helpers. |
| `UIVXAIVoiceServices` | `GetInstance` | Voice pipeline utilities complementing `UIVXAIClient`. |

### Social, XR, console

| Class | Access pattern | Description |
|-------|----------------|-------------|
| `UIVXDiscordSocial` | `GetInstance` | Primary Discord integration entry (presence, social graph patterns). |
| `UIVXDiscordMessages` | `GetInstance` | Message-related Discord helpers. |
| `UIVXDiscordModeration` | `GetInstance` | Moderation-oriented APIs. |
| `UIVXDiscordLinkedChannels` | `GetInstance` | Linked channel management. |
| `UIVXDiscordDebug` | `GetInstance` | Debug / diagnostics for Discord features. |
| `UIVXDiscordSettings` | `UObject` | Serialized Discord settings (not all functions Blueprint-public). |
| `UIVXXRHelper` | `UObject` / Blueprintable | VR/AR detection and helper utilities (HMD module–aware). |
| `UIVXConsoleSubsystem` | `UGameInstance` subsystem | PS5 / Xbox / Switch–style flows via **OnlineSubsystem**: platform user id, sign-in, achievements, presence. Requires correct platform OSS plugin and `DefaultEngine.ini` OnlineSubsystem configuration. |

## Feature Coverage

Cross-platform parity is expressed the same way as the master matrix: **Y** = supported for typical production use, **S** = stub, partial, or requires extra wiring/server work.

### Core (Nakama / IntelliVerseX)

| Feature | Unreal |
|---------|:------:|
| Init / config (`UIVXManager`, `UIVXConfig`) | Y |
| Device auth | Y |
| Email auth | Y |
| Google / Apple auth | Y |
| Profile | Y |
| Wallet | Y |
| Leaderboards | Y |
| Storage | Y |
| RPC | Y |
| Real-time socket | S |

### AI

| Feature | Unreal |
|---------|:------:|
| AI stack init (`UIVXAIClient`, related singletons) | Y |
| Voice | S |
| NPC dialog | S |
| Assistant | S |
| Moderator | S |
| Content generator | S |
| Profiler | S |

### Multiplayer

| Feature | Unreal |
|---------|:------:|
| Game modes (`UIVXGameModes`) | S |
| Lobby (`UIVXMultiplayer`) | Y |
| Matchmaking (`UIVXMultiplayer`) | Y |

### Hiro (live-ops)

| Feature | Unreal |
|---------|:------:|
| Spin wheel | Y |
| Streaks / retention-style systems | Y |
| Offerwall | Y |
| Friend quests / battles | Y |
| IAP trigger / smart ad | Y |

### Discord

| Feature | Unreal |
|---------|:------:|
| Social, messages, moderation, linked channels, debug | S |

### Satori

| Feature | Unreal |
|---------|:------:|
| Events, flags, experiments, live events | S |

## Platform-Specific Configuration

### Data Asset (`UIVXConfig`)

1. In the Content Browser, **Add → Miscellaneous → Data Asset**, pick **`UIVXConfig`**.
2. Set **Game Id** from the IntelliVerseX developer console or your backend team.
3. Set **Nakama Host**, **Port**, **Server Key**, and **Use SSL** to match your Nakama deployment (defaults point at IntelliVerseX-hosted REST endpoints in the template asset).
4. Optional: **Cognito** fields if your title uses the managed identity pool flow.
5. Toggle **Enable Analytics** and **Debug / Verbose Logging** per build flavor.

Reference the asset from C++ (`LoadObject`) or Blueprint (variable or soft pointer).

### `.uplugin` configuration

`Plugins/IntelliVerseX/IntelliVerseX.uplugin` declares:

- **Module** `IntelliVerseX` (Runtime, `Default` loading phase).
- **Platform allow list** for the module (Win64, Mac, Linux, Android, iOS). Extending to consoles usually means editing the allow list *and* satisfying platform-specific compile/link requirements under NDA.
- **Plugin dependency** on the Nakama plugin (listed as `"Name": "Nakama"` in the repo—align with the actual Nakama plugin id in your engine).

Keep **VersionName** in sync with the rest of the IntelliVerseX SDK when you vendor the plugin.

### `Build.cs` module dependencies

The IntelliVerseX module already depends on:

- `Core`, `CoreUObject`, `Engine`
- `HTTP`, `Json`, `JsonUtilities`
- `HeadMountedDisplay` (XR helpers)
- `NakamaUnreal`
- `OnlineSubsystem`, `OnlineSubsystemUtils` (console / platform account flows)

Your **game** module only needs `IntelliVerseX` in `PublicDependencyModuleNames` unless you call Nakama or OnlineSubsystem APIs directly—in that case add `NakamaUnreal` and/or the relevant `OnlineSubsystemXXX` modules.

### Packaging settings

- **Android / iOS:** Ensure internet permission and non-cleartext traffic rules align with `bUseSSL` and your Nakama host.
- **Dedicated server:** If you use the same plugin on a dedicated server target, verify Nakama keys and that you do not strip Blueprint-only paths required by your server build.
- **Console:** Configure **DefaultEngine.ini** with the correct `[/Script/OnlineSubsystemUtils.IOnlineSubsystem]` `DefaultPlatformService` and first-party OSS plugins; test `UIVXConsoleSubsystem` **Sign In With Platform** on dev kits early.

## Advanced Examples

### Example 1 — Hiro streaks via RPC

**Option A — Typed Hiro API (preferred when Nakama client + session are wired):**

After authentication, call `SetNakamaClient` on `UIVXHiroSystems` with your `UNakamaClient` and `UNakamaSession`, then call **Update Streak** with an `FIVXStreakDelegate` to receive `FIVXStreakState` (current streak, longest streak, milestones). **Get Streaks** refreshes read-only state; **Claim Streak Milestone** completes a milestone day.

**Option B — Generic RPC via `UIVXManager`:**

If your server exposes a custom RPC (e.g. `hiro_streaks_v2_update`) and you already use `CallRpc`:

```cpp
void AMyGameMode::RequestStreakUpdate()
{
    UIVXManager* IVX = GetGameInstance()->GetSubsystem<UIVXManager>();
    IVX->OnRpcResult.AddDynamic(this, &AMyGameMode::OnStreakRpcResult);
    IVX->CallRpc(TEXT("hiro_streaks_v2_update"), TEXT("{}"));
}

void AMyGameMode::OnStreakRpcResult(const FString& RpcId, const FString& ResponseJson)
{
    if (RpcId != TEXT("hiro_streaks_v2_update")) return;
    // Parse ResponseJson (e.g. with FJsonSerializer) and update UI
}
```

Replace the RPC id and payload JSON with the contract your Nakama / Hiro deployment actually registers.

### Example 2 — AI content generation

```cpp
#include "IVXAIContentGenerator.h"

void AMyGameMode::GenerateSideQuest()
{
    UIVXAIContentGenerator* Gen = UIVXAIContentGenerator::GetInstance(this);
    Gen->Initialize(nullptr); // Pass config UObject when your pipeline supplies one

    FIVXContentJsonDelegate OnDone;
    OnDone.BindDynamic(this, &AMyGameMode::OnQuestJsonReady);
    Gen->GenerateStory(TEXT("A short trivia side quest about Renaissance art"), OnDone);
}

void AMyGameMode::OnQuestJsonReady(const FString& Json)
{
    UE_LOG(LogTemp, Log, TEXT("Generated content: %s"), *Json);
}
```

In Blueprint: **Get IVX AI Content Generator** → **Initialize** → **Generate Story**, then bind the output delegate to a custom event that parses the JSON string into your quest struct or table row.

## Troubleshooting

| Symptom | Likely cause | What to try |
|---------|----------------|------------|
| `IntelliVerseX` module not found at compile time | Game module missing dependency | Add `IntelliVerseX` to `PublicDependencyModuleNames` in your `*.Build.cs`, regenerate VS/Xcode project files. |
| Nakama plugin missing / link errors | Nakama Unreal not installed or disabled | Install Heroic Labs plugin; enable in `.uproject`; confirm `NakamaUnreal` appears in IntelliVerseX `Build.cs` dependencies. |
| Blueprint nodes not showing | Plugin disabled or hot reload desync | Enable **IntelliVerseX** in Plugins; full Editor restart; run a **full** C++ rebuild (not Live Coding only). |
| Packaging fails on undefined symbols | Wrong target or stripped modules | Ensure packaging configuration includes the IntelliVerseX plugin for the target platform; check platform allow list in `.uplugin`. |
| OnlineSubsystem / console sign-in always fails | OSS not configured for the platform | Set correct `DefaultPlatformService`, enable Sony/Microsoft/Nintendo OSS plugins, test on hardware; verify `UIVXConsoleSubsystem` is obtained from the same `GameInstance` as gameplay. |
| Hiro / Multiplayer calls no-op | Client or session not set | After auth, call `SetNakamaClient` on `UIVXHiroSystems` and `UIVXMultiplayer` with valid `UNakamaClient` / `UNakamaSession`. |
| SSL / connection errors | Host or port mismatch | Run **Validate Nakama Config** on your `UIVXConfig`; verify firewall and `bUseSSL` vs port (443 vs 7350). |

## Nakama Client

Built on [nakama-unreal](https://github.com/heroiclabs/nakama-unreal) (249 stars, 74 forks). The Unreal plugin wraps the same Nakama REST and realtime primitives used across other IntelliVerseX platforms; server RPCs and Hiro modules behave consistently once sessions are established.

## Further Reading

- [Nakama Unreal documentation](https://heroiclabs.com/docs/nakama/client-libraries/unreal/) — installation, sessions, and sockets.
- [Nakama concepts](https://heroiclabs.com/docs/nakama/concepts/) — accounts, storage, leaderboards, multiplayer.
- [Hiro documentation](https://heroiclabs.com/docs/hiro/) — economy, live-ops, and RPC conventions (align server RPC ids with `UIVXHiroSystems` / `CallRpc` usage).
- [Satori documentation](https://heroiclabs.com/docs/satori/) — analytics and experimentation (for completing `UIVXSatori` stubs).
- [IntelliVerseX SDK documentation](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/) — cross-platform overview and module guides.
- [Console platforms](console.md) — PS5, Xbox Series X and S, Switch adapter notes.
- [XR / VR / AR](xr-vr-ar.md) — headset detection and platform matrix (Unity-first; Unreal `UIVXXRHelper` follows the same intent).

## Source

[SDKs/unreal/](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/tree/main/SDKs/unreal)
