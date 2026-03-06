# Unreal Engine

> IntelliVerseX plugin for Unreal Engine 5, with full Blueprint and C++ support.

## Requirements

- Unreal Engine 5.3+
- [Nakama Unreal Plugin](https://github.com/heroiclabs/nakama-unreal) v2.8+
- C++17 compiler

## Installation

1. Copy `SDKs/unreal/` into your project's `Plugins/IntelliVerseX/` directory
2. Install the [Nakama Unreal Plugin](https://heroiclabs.com/docs/nakama/client-libraries/unreal/)
3. Enable both plugins in your `.uproject`:

```json
{
  "Plugins": [
    { "Name": "NakamaUnreal", "Enabled": true },
    { "Name": "IntelliVerseX", "Enabled": true }
  ]
}
```

4. Add module dependency in your `Build.cs`:

```csharp
PublicDependencyModuleNames.Add("IntelliVerseX");
```

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

## Key Classes

| Class | Description |
|-------|-------------|
| `UIVXManager` | Central GameInstance subsystem |
| `UIVXConfig` | Data Asset for server configuration |

## Nakama Client

Built on [nakama-unreal](https://github.com/heroiclabs/nakama-unreal) (249 stars, 74 forks).

## Source

[SDKs/unreal/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/unreal)
