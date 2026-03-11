# IntelliVerseX Unreal Engine SDK

> Complete modular game development SDK for Unreal Engine — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- Unreal Engine 5.3+
- [Nakama Unreal Plugin](https://github.com/heroiclabs/nakama-unreal) v2.8+
- C++17 compiler

## Installation

### As an Engine Plugin

1. Clone or copy the `SDKs/unreal/` folder into your project's `Plugins/IntelliVerseX/` directory
2. Install the [Nakama Unreal Plugin](https://heroiclabs.com/docs/nakama/client-libraries/unreal/)
3. Enable both plugins in your `.uproject` file:

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

### Configuration

1. Create an `IVXConfig` Data Asset: **Content Browser > Add > Miscellaneous > Data Asset > IVXConfig**
2. Fill in your Nakama server details (host, port, server key)
3. Reference it in your initialization code

## Quick Start

### Blueprint

1. Get the `IVXManager` subsystem from your Game Instance
2. Call `InitializeSDK` with your config asset
3. Call `AuthenticateWithDevice` to sign in
4. Bind to `OnInitialized` and `OnError` events

### C++

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

void AMyGameMode::OnIVXReady()
{
    UIVXManager* IVX = GetGameInstance()->GetSubsystem<UIVXManager>();
    IVX->FetchProfile();
    IVX->FetchWallet();
}
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Profile Management | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Hiro Systems | Via RPC |
| Real-time Multiplayer | Planned |
| Analytics | Planned |
| Monetization | Planned |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/unreal/).

## Nakama Client Library

This SDK wraps the official [Nakama Unreal Client](https://github.com/heroiclabs/nakama-unreal) (249 stars, 74 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
