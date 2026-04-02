# IntelliVerseX Unreal Engine SDK

> Complete modular game development SDK for Unreal Engine — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.5.0

### AI Voice & Host (`UIVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```cpp
#include "IVXAIClient.h"

UIVXAIClient* AI = GetGameInstance()->GetSubsystem<UIVXAIClient>();
AI->Initialize(TEXT("https://ai.intelli-verse-x.ai"), TEXT("your-key"));

FIVXHostProfile Host;
Host.PersonaId = TEXT("persona-1");
Host.DisplayName = TEXT("QuizBot");
AI->StartHostSession(TEXT("match-123"), Host);
AI->OnMessageReceived.AddDynamic(this, &AMyMode::OnAIMessage);
```

### Multiplayer & Game Modes (`UIVXGameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```cpp
#include "IVXGameModes.h"

UIVXGameModes* GM = GetGameInstance()->GetSubsystem<UIVXGameModes>();
FIVXMatchConfig Config;
Config.Mode = EIVXGameMode::OnlineVersus;
Config.MaxPlayers = 4;
GM->SelectMode(Config);
GM->AddPlayer(TEXT("Alice"), true);
GM->SetPlayerReady(0, true);
if (GM->CanStartMatch()) GM->StartMatch();
```

### Hiro Live-Ops Systems (`UIVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```cpp
#include "IVXHiroSystems.h"

UIVXHiroSystems* Hiro = GetGameInstance()->GetSubsystem<UIVXHiroSystems>();
Hiro->Initialize(NakamaClient, NakamaSession);
Hiro->SpinWheel(TEXT("daily_wheel"));
Hiro->OnSpinResult.AddDynamic(this, &AMyMode::OnSpinResult);
Hiro->GetStreakState();
Hiro->ClaimStreak();
```

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
| Device Auth | ✅ Supported |
| Email Auth | ✅ Supported |
| Google Auth | ✅ Supported |
| Apple Auth | ✅ Supported |
| Profile Management | ✅ Supported |
| Wallet / Economy | ✅ Supported |
| Leaderboards | ✅ Supported |
| Cloud Storage | ✅ Supported |
| RPC Calls | ✅ Supported |
| AI Voice & Host | ✅ New in v5.5.0 |
| Real-time Multiplayer & Game Modes | ✅ New in v5.5.0 |
| Hiro Live-Ops Systems | ✅ New in v5.5.0 |
| Analytics | ✅ Supported |
| Monetization | ✅ Supported |

## Project Structure

```
Source/IntelliVerseX/
├── Public/
│   ├── IVXManager.h            # Core manager subsystem
│   ├── IVXAIClient.h           # AI voice & host client
│   ├── IVXGameModes.h          # Multiplayer & game mode management
│   └── IVXHiroSystems.h        # Hiro live-ops typed wrappers
└── Private/
    ├── IVXAIClient.cpp
    ├── IVXGameModes.cpp
    └── IVXHiroSystems.cpp
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/unreal/).

## Nakama Client Library

This SDK wraps the official [Nakama Unreal Client](https://github.com/heroiclabs/nakama-unreal) (249 stars, 74 forks).

## Testing

1. Add this plugin to a project (e.g. copy to `Plugins/IntelliVerseX/`), enable it and Nakama, add `IntelliVerseX` to your game module's `Build.cs`.
2. In a Game Mode (C++ or Blueprint), get `UIVXManager` from the Game Instance, call `InitializeSDK` with an `IVXConfig` (or defaults), then `AuthenticateWithDevice`; bind to `OnInitialized` / `OnError` and run `FetchProfile` / `FetchWallet` when ready.
3. Press Play and check **Output Log** (filter: `IVX`) for init, auth, profile, and wallet messages.
4. For a full checklist (build, Game Mode, PIE, Nakama), see **RELEASE.md** and, if you use it, the IVX_Test project's **TESTING_SDK.md**.

## Releasing

See **[RELEASE.md](RELEASE.md)** for publishing to GitHub, Unreal Marketplace, and GameDev Market (packaging, versioning, submission).

## License

MIT License — see [LICENSE](../../LICENSE)
