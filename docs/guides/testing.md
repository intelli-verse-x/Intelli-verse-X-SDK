# Testing Guide

This guide covers **offline / mock behaviors**, **automated tests**, **Editor tooling**, **CI**, and a **pre-release checklist** for IntelliVerseX integrations.

---

## Overview

You will typically combine:

1. **Configuration-driven behavior** — feature toggles on `IVXBootstrapConfig`, AI mock mode on `IVXAIConfig`, and ads test/consent flags on `IVXAdsConfig`.
2. **Automatic fallbacks** — some multiplayer APIs generate **mock lobbies or matches** when Nakama is not connected (useful in Editor and on CI agents without a server).
3. **Unity Test Framework** — Play Mode and Edit Mode tests in an assembly that references the SDK and `nunit.framework`.

!!! tip "Start simple"
    Prove **bootstrap + one vertical** (e.g. identity or leaderboard) in Play Mode before investing in full headless CI.

---

## Mock mode and offline testing

### IVXAIConfig.MockMode

**Mock mode for AI** is defined on **`IVXAIConfig`**, not on `IVXBootstrapConfig` itself. Link an `IVXAIConfig` asset from your **`IVXBootstrapConfig`** (`AIConfig` field). When **`MockMode`** is enabled, AI managers return **canned responses** and avoid outbound HTTP calls — ideal for UI iteration and automated runs without inference backends.

!!! note "Bootstrap config location"
    Create assets via **Assets > Create > IntelliVerseX > Bootstrap Config** and **AI Config** (see package menus). Assign the AI asset on the bootstrap ScriptableObject.

```csharp
using IntelliVerseX.Bootstrap;
using IntelliVerseX.AI;
using UnityEngine;

public static class AiMockDiagnostics
{
    public static void LogAiMock(IVXBootstrapConfig bootstrap)
    {
        if (bootstrap?.AIConfig is not IVXAIConfig ai)
        {
            Debug.Log("No IVXAIConfig assigned on bootstrap.");
            return;
        }
        Debug.Log($"IVXAIConfig.MockMode = {ai.MockMode}");
    }
}
```

### Lobby and matchmaking (multiplayer package)

- **`IVXLobbyManager`** lists rooms from Nakama when available; **if the client is not connected**, it **falls back to generated mock rooms** so UI can be exercised.
- **`IVXMatchmakingManager`** can complete a **mock match** after a short delay when a real matchmaker result is unavailable — useful for flow testing without a populated server.

!!! warning "Production"
    Always validate **real Nakama** behavior in a staging environment; mocks only approximate UX, not authoritative room state.

### Wallet, Satori, and backend-heavy modules

Without a live backend, behavior depends on **which managers are initialized** and **feature toggles** (e.g. `EnableSatori` on `IVXBootstrapConfig`). For local work:

- Use **SDK test scenes** and sample controllers that **degrade gracefully** (e.g. profile demos with optional mock snapshots).
- Prefer **explicit mocks** in *your* test code over assuming a single global “mock everything” flag exists for wallet or Satori.

---

## Unit testing with NUnit

### Test assembly setup

The package includes sample test assemblies under `Assets/Intelli-verse-X-SDK/Tests~/` (e.g. `IntelliVerseX.Tests.asmdef`) using:

- `UNITY_INCLUDE_TESTS`
- **`nunit.framework.dll`** as a precompiled reference

In your game project, create **Edit Mode** or **Play Mode** test assemblies that **reference** the IntelliVerseX assemblies you need (Core, Backend, etc.).

### Example: initialization guard

```csharp
using NUnit.Framework;
using IntelliVerseX.Core;

public class SdkBootstrapTests
{
    [Test]
    public void Manager_Reports_NotInitialized_BeforeInitialize()
    {
        // If a prior test initialized the SDK, reset domain or use a fresh test scene.
        Assert.IsFalse(IntelliVerseXManager.IsInitialized);
    }
}
```

!!! note "Test isolation"
    Use **Play Mode** tests with a **clean scene** or `[UnitySetUp]` that resets static singletons where the SDK allows it; static init order can leak between tests.

### Example: storage / privacy helpers

Local storage helpers (e.g. `IVXSecureStorage`, `IVXPrivacyManager`) can be covered in **Edit Mode** where appropriate, as long as you do not require a full Nakama session:

```csharp
using NUnit.Framework;
using IntelliVerseX.Storage;

public class PrivacyExportTests
{
    [Test]
    public void ExportAllData_ReturnsNonEmptyJson()
    {
        string json = IVXPrivacyManager.ExportAllData();
        Assert.IsNotNull(json);
        StringAssert.Contains("export_date", json);
    }
}
```

---

## Play Mode testing (Unity Test Framework)

Use **Play Mode** tests for flows that need `MonoBehaviour` lifecycle, scene loading, or async initialization.

### Authentication-style flow (sketch)

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AuthFlowPlayModeTests
{
    [UnityTest]
    public IEnumerator Bootstrap_LoadsWithoutThrowing()
    {
        // Load a scene that contains IVXBootstrap + config, or create objects in code.
        yield return null;
        // Assert on UI or manager state after one frame
        Assert.IsTrue(true);
    }
}
```

### Leaderboard submission

Test **UI + callback** wiring against **staging Nakama**, or use **demo behavior** that surfaces mock rows when disconnected — document which environment your test expects.

---

## Editor tooling

### IVXBootstrapConfig validation

In the Editor, `IVXBootstrapConfig` **`OnValidate`** warns on:

- Empty **Game ID**
- **127.0.0.1 / localhost** host in shipping contexts
- Default **server key**
- Invalid **port** range

Fix warnings before cutting a release build.

### IVXTestSceneNavigator

**`IVXTestSceneNavigator`** (Core) provides an **in-game overlay** to jump between SDK **test scenes** (Auth, Ads, Leaderboard, Wallet, quizzes, Profile, etc.) and toggle visibility with a key (default **F8**). Add it to a validation scene for **manual QA** and designer walkthroughs.

### Verbose logging

Enable **`DebugLogging`** on `IVXBootstrapConfig` while debugging failing init or module order issues.

---

## CI/CD tips

### Headless Unity

Use Unity’s **batchmode** to run tests on a build agent:

```bash
/Applications/Unity/Hub/Editor/6000.2.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "$GITHUB_WORKSPACE" \
  -runTests -testPlatform playmode \
  -testResults ./TestResults.xml \
  -logFile -
```

Adjust **Editor path** and **Unity version** to match your project (`ProjectVersion.txt`).

### GitHub Actions (illustrative)

```yaml
jobs:
  unity-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: game-ci/unity-test-runner@v4
        with:
          unityVersion: 6000.2.8f1
          testMode: playmode
```

!!! warning "Licenses and caches"
    Game CI images require **Unity license activation** (serial or ULF). Plan for **caching** Library folder for faster runs.

---

## Testing checklist (before release)

- [ ] **Bootstrap** — Valid Game ID, non-default Nakama host/key for production, SSL flag correct.
- [ ] **Identity** — Sign-in, session restore, and sign-out paths tested on device.
- [ ] **Analytics / Satori** — Events only after **consent** where required; staging project receives traffic.
- [ ] **Ads** — GDPR/CCPA/COPPA flags on `IVXAdsConfig` match store listing; UMP / ATT flows tested on real devices.
- [ ] **Multiplayer** — Real lobby/list + matchmaking against staging; mock paths only in Editor or test builds.
- [ ] **AI** — `IVXAIConfig.MockMode` **off** in production assets.
- [ ] **Storage** — `IVXPrivacyManager.DeleteAllData` and export paths exercised for GDPR/CCPA support pages.

---

## See also

- [Troubleshooting FAQ](../troubleshooting/faq.md)
- [Build errors](../troubleshooting/build-errors.md)
