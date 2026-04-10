---
name: ivx-sdk-setup
description: >-
  Set up and configure the IntelliVerseX Game SDK for Unity or cross-platform engines.
  Use when the user says "set up IntelliVerseX", "integrate SDK", "bootstrap SDK",
  "add IntelliVerseX to my project", "configure game ID", "UPM package setup",
  or needs help with initial SDK integration for Unity, Unreal, Godot, JavaScript,
  Java, Flutter, C++, Cocos2d-x, Defold, or Web3.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX SDK Setup & Configuration

## Overview

This skill walks through the complete setup of the IntelliVerseX Game SDK, from package installation to verified initialization. The SDK supports 10 platforms; Unity is the reference implementation.

---

## When to Use

Ask your AI agent any of these:

- "Set up IntelliVerseX in my Unity project"
- "Add IntelliVerseX to my Node.js game server"
- "Bootstrap the SDK for Godot 4"
- "Configure my game ID and Nakama connection"
- "Add IntelliVerseX to my Flutter app"
- "Integrate the SDK into my Unreal project"

---

## What the Agent Does

```mermaid
flowchart LR
    A[You: "Set up IntelliVerseX"] --> B[Agent loads ivx-sdk-setup skill]
    B --> C[Detects your platform]
    C --> D[Installs the package]
    D --> E[Creates bootstrap config]
    E --> F[Fills in server credentials]
    F --> G[Enables feature modules]
    G --> H[Verifies initialization]
```

### Step-by-step:

1. **Package installation** -- UPM for Unity, npm for JS, Gradle for Java, pub.dev for Flutter, CMake for C++, addon for Godot, etc.
2. **Bootstrap config creation** -- ScriptableObject in Unity, config object in other platforms.
3. **Credential setup** -- GameId, ServerHost, ServerPort, ServerKey from the developer dashboard.
4. **Feature toggles** -- Enable only the modules you need (Hiro, Satori, Discord, AI, Multiplayer, Platform).
5. **Initialization verification** -- Ensures `OnBootstrapComplete` fires without warnings.
6. **Troubleshooting** -- Handles missing assembly references, empty GameId, connection timeouts, and Nakama not found.

---



## Unity Installation (UPM)

### Step 1 — Add the UPM Package

Open **Window > Package Manager > + > Add package from git URL** and paste:

```
https://github.com/intelli-verse-x/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK
```

Alternatively, add directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

### Step 2 — Create the Bootstrap Config

1. In the Project window, right-click and select **Create > IntelliVerseX > Bootstrap Config**.
2. Name it `IVXBootstrapConfig` and place it in a `Resources/` or `_Config/` folder.

### Step 3 — Fill in Required Fields

| Field | Where to Get It | Example |
|-------|----------------|---------|
| **GameId** | [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers) — create a project and copy the Game ID | `a1b2c3d4-...` |
| **ServerHost** | Nakama server host provided in the developer dashboard | `nakama.intelli-verse-x.ai` |
| **ServerPort** | Server port (typically 7350 for gRPC, 7351 for HTTP) | `7350` |
| **ServerKey** | Nakama server key from the dashboard | `defaultkey` |

### Step 4 — Toggle Feature Modules

The config exposes toggles for each subsystem. Enable only what you need:

| Toggle | Purpose | Default |
|--------|---------|---------|
| `EnableHiro` | Live-ops, economy, achievements, streaks | `true` |
| `EnableSatori` | Analytics, feature flags, A/B experiments | `true` |
| `EnableDiscord` | Discord rich presence and social overlay | `false` |
| `EnableAI` | AI voice host, NPC dialog, content generation | `false` |
| `EnableMultiplayer` | Lobby, matchmaking, real-time networking | `false` |
| `EnablePlatform` | Platform-specific services (leaderboards, auth) | `true` |

Disabled modules are fully stripped — no initialization cost, no network calls.

### Step 5 — Add the Bootstrap Component

1. Create an empty GameObject in your first scene (e.g. `[IVX Bootstrap]`).
2. Add the **IVXBootstrap** component.
3. Assign the `IVXBootstrapConfig` asset to the **Config** field.
4. Check **Auto Initialize** to bootstrap on `Awake()`.

The component uses `DontDestroyOnLoad` and is a singleton accessible via `IVXBootstrap.Instance`.

### Step 6 — Verify Initialization

```csharp
using IntelliVerseX.Core;
using UnityEngine;

public class GameEntryPoint : MonoBehaviour
{
    private void Start()
    {
        IVXBootstrap.Instance.OnBootstrapComplete += OnReady;
    }

    private void OnReady()
    {
        Debug.Log("[GameEntryPoint] IntelliVerseX SDK initialized");
    }
}
```

Check the console for:
- `[IVXBootstrap] Bootstrap complete — all enabled modules ready`

If you see warnings, see the troubleshooting section below.

---

## Cross-Platform Quick Starts

### JavaScript / TypeScript

```bash
npm install @intelliversex/sdk
```

```typescript
import { IVXClient } from "@intelliversex/sdk";

const client = new IVXClient({
  gameId: "YOUR_GAME_ID",
  serverHost: "nakama.intelli-verse-x.ai",
  serverPort: 7350,
  serverKey: "defaultkey",
});
await client.initialize();
```

### Java / Android (Gradle)

```groovy
// build.gradle
dependencies {
    implementation 'com.intelliversex:sdk:5.8.0'
}
```

```java
IVXClient client = new IVXClient.Builder()
    .setGameId("YOUR_GAME_ID")
    .setServerHost("nakama.intelli-verse-x.ai")
    .build();
client.initialize();
```

### Flutter / Dart (pub.dev)

```yaml
# pubspec.yaml
dependencies:
  intelliversex_sdk: ^5.8.0
```

```dart
final client = IVXClient(
  gameId: 'YOUR_GAME_ID',
  serverHost: 'nakama.intelli-verse-x.ai',
);
await client.initialize();
```

### Godot 4 (Addon)

1. Copy `addons/intelliversex/` into your project's `addons/` directory.
2. Enable the plugin in **Project > Project Settings > Plugins**.
3. Configure via the **IVXAutoload** singleton:

```gdscript
IVXAutoload.game_id = "YOUR_GAME_ID"
IVXAutoload.server_host = "nakama.intelli-verse-x.ai"
await IVXAutoload.initialize()
```

### Defold (Library Module)

Add to `game.project` under `[dependencies]`:

```
https://github.com/intelli-verse-x/defold-sdk/archive/main.zip
```

### C++ (CMake)

```cmake
FetchContent_Declare(
  intelliversex
  GIT_REPOSITORY https://github.com/intelli-verse-x/cpp-sdk.git
  GIT_TAG v5.8.0
)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(${PROJECT_NAME} PRIVATE intelliversex::sdk)
```

### Cocos2d-x (CMake)

Same CMake pattern as the C++ SDK. Link against `intelliversex::sdk` in your `CMakeLists.txt`.

### Unreal Engine 5 (Plugin)

1. Clone or download the plugin into `Plugins/IntelliVerseX/`.
2. Enable in **Edit > Plugins > IntelliVerseX**.
3. Add to your `.Build.cs`:

```csharp
PublicDependencyModuleNames.Add("IntelliVerseX");
```

4. Configure in **Project Settings > Plugins > IntelliVerseX**: set GameId, ServerHost, ServerKey.

---

## Troubleshooting

### Missing Assembly Definition References

**Symptom:** Compiler error about missing types from `IntelliVerseX.*`.

**Fix:** Add the required `.asmdef` reference to your assembly definition file. The SDK's runtime assemblies follow the pattern `IntelliVerseX.{Module}`.

### "Nakama not installed" Warning

**Symptom:** Console warning at startup about Nakama client not found.

**Fix:** The UPM package bundles Nakama. If you see this, check that the package imported correctly — reimport via Package Manager or delete `Library/` and reimport.

### Empty GameId Warning

**Symptom:** `[IVXBootstrap] GameId is empty — cannot initialize`.

**Fix:** Open your `IVXBootstrapConfig` ScriptableObject and enter the Game ID from [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers).

### Initialization Hangs

**Symptom:** `OnBootstrapComplete` never fires.

**Fix:**
1. Verify `ServerHost` and `ServerPort` are reachable (`ping` or browser check).
2. Ensure `ServerKey` matches the Nakama server configuration.
3. Check Unity console for socket timeout errors.

---

## XR / VR / AR Setup

### Meta Quest (VR)

1. Install the **Meta XR SDK** via Unity Package Manager.
2. In **Project Settings > XR Plug-in Management**, enable the **Oculus** loader for Android.
3. Add the scripting define `INTELLIVERSEX_HAS_META_XR` in **Player Settings > Scripting Define Symbols**.
4. The SDK auto-detects Quest via `IVXXRPlatformHelper` and provides hand/eye tracking and passthrough flags.

### Apple Vision Pro (visionOS)

1. Install the **Unity PolySpatial** and **Apple visionOS** packages.
2. Enable the **Apple** XR loader in XR Plug-in Management.
3. The SDK matches the loader name containing "Apple", "Vision", or "PolySpatial".
4. `IVXXRPlatformHelper.GetRecommendedSettings()` returns visionOS-optimized values (90 fps, world-space UI, hand tracking preferred).

### SteamVR / OpenXR (PC VR)

1. Install the **OpenXR Plugin** via UPM.
2. Add `INTELLIVERSEX_HAS_OPENXR` to scripting defines for full subsystem queries.
3. On Windows standalone builds, the SDK auto-classifies OpenXR as `SteamVR`.

### PSVR2

1. Obtain the **PlayStation VR2** Unity plugin from Sony's partner portal (NDA required).
2. The SDK detects loaders containing "PlayStation" or "PSVR" and sets `XRPlatformType.PSVR2`.
3. Recommended settings: 90 fps, world-space UI, eye tracking available, no hand tracking.

### AR Foundation (ARKit / ARCore)

1. Install **AR Foundation** and the platform-specific provider (ARKit XR Plugin or ARCore XR Plugin).
2. Add `INTELLIVERSEX_HAS_ARFOUNDATION` to scripting defines.
3. `IVXARHelper` wraps plane detection, image tracking, light estimation, and world mesh availability.

### Godot 4 XR

The addon includes `IVXXRHelper` which auto-detects OpenXR interfaces. Call `detect_xr_platform()` after the XR interface is initialized.

### C++ XR (OpenXR)

Enable in CMake:

```cmake
cmake -DIVX_ENABLE_OPENXR=ON ..
```

This links the OpenXR loader and defines `IVX_HAS_OPENXR` for real platform detection.

---

## Console Setup (PS5 / Xbox / Switch)

Console integration uses an **adapter pattern** — you implement `IIVXConsoleAdapter` with your console's proprietary SDK (NDA-protected), and register it with `IVXConsoleManager`:

```csharp
// In your platform-specific bootstrap
IVXConsoleManager.Instance.RegisterAdapter(new PS5ConsoleAdapter());
```

The adapter interface covers: `GetPlatformUserIdAsync()`, `SignInWithPlatformAsync()`, `UnlockAchievementAsync()`, `SetPresenceAsync()`, `SupportsFeature()`.

### Unreal Engine

Use `UIVXConsoleSubsystem` which wraps Unreal's `IOnlineSubsystem` for cross-console identity, achievements, and presence — works automatically with Epic Online Services, Steam, PlayStation Network, or Xbox Live depending on your project's online subsystem config.

### Configuration

Create an `IVXConsoleConfig` ScriptableObject via **Create > IntelliVerseX > Console Configuration** to toggle auto-sign-in and set default presence.

---

## WebGL Build Notes

- WebGL builds use `.jslib` plugins for ads (AdSense/Applixir) and payment APIs.
- Audio recording (`IVXAIAudioRecorder`) is disabled on WebGL — use text-only AI.
- WebSocket connections require the server to support HTTPS with valid CORS headers.
- The JS SDK exports `IVXWebXRHelper` for browser-based WebXR (Quest Browser, Chrome).

---

## Checklist

- [ ] UPM package added and compiled without errors
- [ ] `IVXBootstrapConfig` created with valid `GameId`
- [ ] `IVXBootstrap` component on a scene GameObject with config assigned
- [ ] `OnBootstrapComplete` fires without warnings
- [ ] Feature toggles set for your project's needs
- [ ] XR compile defines added if targeting VR/AR
- [ ] Console adapter registered if targeting PS5/Xbox/Switch
- [ ] WebGL-specific limitations reviewed if targeting browser
