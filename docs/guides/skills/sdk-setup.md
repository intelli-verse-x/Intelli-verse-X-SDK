# Skill: SDK Setup

**Skill ID:** `ivx-sdk-setup`

Walks you through the complete IntelliVerseX SDK installation, from package manager to verified initialization, across all 10 supported platforms.

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

## Platform-Specific Installation

### Unity (UPM)

The agent adds the package via the Unity Package Manager:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

Then creates a bootstrap config and component:

```csharp
IVXBootstrap.Instance.OnBootstrapComplete += () =>
{
    Debug.Log("[GameEntryPoint] IntelliVerseX SDK initialized");
};
```

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

### Java / Android

```groovy
dependencies {
    implementation 'com.intelliversex:sdk:5.8.0'
}
```

### Flutter / Dart

```yaml
dependencies:
  intelliversex_sdk: ^5.8.0
```

### Godot 4

Copy `addons/intelliversex/` into your project, enable the plugin, configure via `IVXAutoload`.

### Unreal Engine 5

Clone plugin to `Plugins/IntelliVerseX/`, add `PublicDependencyModuleNames.Add("IntelliVerseX")`.

### C++ / Cocos2d-x

```cmake
FetchContent_Declare(intelliversex
  GIT_REPOSITORY https://github.com/intelli-verse-x/cpp-sdk.git
  GIT_TAG v5.8.0)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(${PROJECT_NAME} PRIVATE intelliversex::sdk)
```

### Defold

Add dependency in `game.project`.

---

## Feature Toggle Reference

| Toggle | Purpose | Default |
|--------|---------|---------|
| `EnableHiro` | Live-ops, economy, achievements, streaks | `true` |
| `EnableSatori` | Analytics, feature flags, A/B experiments | `true` |
| `EnableDiscord` | Discord rich presence and social overlay | `false` |
| `EnableAI` | AI voice host, NPC dialog, content generation | `false` |
| `EnableMultiplayer` | Lobby, matchmaking, real-time networking | `false` |
| `EnablePlatform` | Platform-specific services (leaderboards, auth) | `true` |

Disabled modules are fully stripped -- no initialization cost, no network calls.

---

## Troubleshooting

| Problem | Symptom | Fix |
|---------|---------|-----|
| Missing assembly reference | Compiler error about missing types | Add the required `.asmdef` reference |
| Nakama not installed | Console warning at startup | Reimport the UPM package |
| Empty GameId | `[IVXBootstrap] GameId is empty` | Fill in the GameId in your config |
| Initialization hangs | `OnBootstrapComplete` never fires | Check server host/port/key are reachable |

---

## Completion Checklist

The agent verifies all of these before marking setup complete:

- [ ] Package installed and compiled without errors
- [ ] Bootstrap config created with valid GameId
- [ ] Bootstrap component added to scene with config assigned
- [ ] `OnBootstrapComplete` fires without warnings
- [ ] Feature toggles set for your project's needs
