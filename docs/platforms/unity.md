# Unity Engine / .NET

> The Unity SDK is the most feature-complete IntelliVerseX implementation, with native support for Hiro, Satori, monetization, localization, social features, and more.

## Requirements

| Unity Version | Status |
|---------------|--------|
| **Unity 6000.x** | Fully Supported |
| **Unity 2023.3 LTS** | Fully Supported |
| Unity 2022.x | May work |
| Unity 2021.x | Not Supported |

**Platforms:** Android, iOS, WebGL, Windows, macOS

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
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK#v5.1.0"
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

## Nakama Client

Built on [nakama-unity](https://github.com/heroiclabs/nakama-unity) (468 stars, 82 forks).

## Full Documentation

See the [Unity modules documentation](../modules/index.md) for complete API reference.

## Source

[Assets/\_IntelliVerseXSDK/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/Assets/_IntelliVerseXSDK)
