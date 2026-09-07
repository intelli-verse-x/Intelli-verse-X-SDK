# Getting Started with IntelliVerseX SDK

Unity **6000.3** · package `com.intelliversex.sdk` **5.9.0**

## Install

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk"
  }
}
```

## First run

1. **IntelliVerseX → Control Center**
2. Paste **Game ID** on Connect (creates `IVXBootstrapConfig`)
3. **Add bootstrap to this scene** → Play

Advanced modules: **IntelliVerseX → Advanced Setup**

## Code

```csharp
using IntelliVerseX.Bootstrap;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] IVXBootstrap bootstrap;

    async void Start()
    {
        await bootstrap.InitializeAsync();
    }
}
```

Prefer **AutoInitialize** on `IVXBootstrap` in the scene.

## Avoid (obsolete)

- `IntelliVerseXManager` / `IntelliVerseXConfig`
- Device-only `InitializeDevice` as “SDK ready”
- Old paths `Assets/_IntelliVerseXSDK` or `#v2.0.0`

## Next

Import the **Test Scenes** sample for Auth, Wallet, Leaderboard, Ads.
See [INSTALLATION.md](../INSTALLATION.md) and [README.md](../README.md).
