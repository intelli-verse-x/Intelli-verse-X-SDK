# Cocos2d-x Engine

> IntelliVerseX **C++17** static library for Cocos2d-x 4.x, integrated via **CMake** and linked against [nakama-cpp](https://github.com/heroiclabs/nakama-cpp). Callbacks use `std::function`; the core manager exposes `tick()` for Nakama’s async pump.

## Requirements

- **Cocos2d-x** 4.0 or compatible fork
- **CMake** 3.10+
- **C++17** toolchain (Xcode, MSVC, NDK clang)
- **Nakama C++ SDK** — the bundled `CMakeLists.txt` fetches **nakama-cpp** via `FetchContent` at tag **v2.8.5** (override with your own `add_subdirectory` if you vendor it)

## Installation

1. Vendor or submodule the `SDKs/cocos2dx` tree into your game repository.

2. From your game’s `CMakeLists.txt`, add the IntelliVerseX library and link it:

```cmake
add_subdirectory(path/to/SDKs/cocos2dx)
target_link_libraries(your_game PRIVATE intelliversex)
```

3. Ensure **include paths** resolve `IntelliVerseX/*.h` (the target exports `${CMAKE_CURRENT_SOURCE_DIR}/Classes` as a public include directory).

4. The default build pulls **nakama-cpp** from GitHub; offline or CI builds should use `FetchContent` cache or a pre-downloaded `nakama-sdk` source tree.

## Quick Start

```cpp
#include "IntelliVerseX/IVXManager.h"
#include "cocos2d.h"

USING_NS_CC;

bool GameScene::init()
{
    auto& ivx = IntelliVerseX::IVXManager::getInstance();

    IntelliVerseX::IVXConfig config;
    config.gameId = "YOUR_GAME_UUID";
    config.nakamaHost = "127.0.0.1";
    config.nakamaPort = 7350;
    config.nakamaServerKey = "defaultkey";
    config.useSSL = false;
    config.enableDebugLogs = true;

    ivx.initialize(config);
    ivx.authenticateDevice("", []() {
        cocos2d::log("IVX: logged in");
    });

    this->schedule([](float /*dt*/) {
        IntelliVerseX::IVXManager::getInstance().tick();
    }, 0.0f, "ivx_tick");

    return true;
}
```

`IVXManager::validateConfig` exists for upfront checks (host, port range, server key format).

## API Surface

### IVXManager

| Area | Methods (selected) |
|------|---------------------|
| Lifecycle | `getInstance()`, `initialize(config)`, `isInitialized()`, `tick()` |
| Auth | `authenticateDevice`, `authenticateEmail`, `authenticateGoogle`, `authenticateApple`, `authenticateCustom` (each with success/error callbacks) |
| Session | `restoreSession()`, `clearSession()`, `hasValidSession()`, `getUserId()`, `getUsername()` |
| Profile | `fetchProfile`, `updateProfile` |
| Wallet | `fetchWallet`, `grantCurrency` (Hiro economy RPCs) |
| Leaderboards | `submitScore`, `fetchLeaderboard` |
| Storage | `writeStorage`, `readStorage` (JSON strings) |
| RPC | `callRpc(rpcId, payloadJson, ...)` |

`SDK_VERSION` is a `static constexpr const char*` on `IVXManager`.

### IVXConfig

| Field | Purpose |
|-------|---------|
| `gameId` | IntelliVerseX platform game UUID |
| `nakamaHost`, `nakamaPort`, `nakamaServerKey`, `useSSL` | Nakama REST / RPC endpoint |
| `cognitoRegion`, `cognitoUserPoolId`, `cognitoClientId` | Optional AWS Cognito hooks |
| `enableAnalytics`, `enableDebugLogs`, `verboseLogging` | Diagnostics |
| `getScheme()`, `getBaseUrl()` | URL helpers |

### IVXHiroSystems

Singleton `IVXHiroSystems::getInstance()`. Hiro RPCs are dispatched through **`IVXManager::callRpc`** internally (no separate `initialize` — authenticate first).

| Category | Methods |
|----------|---------|
| Spin wheel | `spinWheel(wheelId, ...)`, `getWheelConfig(wheelId, ...)` |
| Streaks | `getStreak`, `claimStreak` |
| Offerwall | `getOffers`, `claimOffer` |
| Friends | `listFriends`, `addFriend`, `removeFriend`, `blockUser` |
| Retention / monetization | `getRetention`, `updateRetention`, `checkIapTrigger`, `canShowSmartAd` |

Structs include `SpinWheelReward`, `StreakInfo`, `OfferwallItem`, `FriendInfo`, `IVXRetentionState`, `IVXIapTriggerResult`, `IVXSmartAdResult` (see `IVXHiroSystems.h`).

### IVXGameModes

Local + online **mode selection**, player slots, rooms, and matchmaking-shaped APIs. On Cocos2d-x, **lobby/matchmaking rows are stub (`S`)** in the [coverage matrix](../FEATURE_COVERAGE_MATRIX.md): signatures exist for `createRoom`, `joinRoom`, `listRooms`, `findMatch`, etc., but wire to your Nakama match/room RPCs for production.

### AI stack

| Class | Role |
|-------|------|
| **IVXAIClient** | Voice sessions, host sessions, entitlements, personas (`initialize`, `startVoiceSession`, `endVoiceSession`, `sendText`, `startHostSession`, `sendHostEvent`, `checkEntitlement`, `getPersonas`, …). |
| **IVXAIVoiceServices** | TTS/STT shaped API — **stub (`S`)** on Cocos for voice paths per matrix. |
| **IVXAINPCDialogManager**, **IVXAIAssistant**, **IVXAIModerator**, **IVXAIContentGenerator**, **IVXAIProfiler** | LLM / content stack — **stub (`S`)** where marked in matrix; use for compile-time parity with Unity. |

### Discord & Satori

**IVXDiscordSocial** and related Discord modules, plus **IVXSatori**, mirror Unity’s surface but are **stub (`S`)** on Cocos: safe for UI scaffolding, mock responses until native bridges exist.

### IVXTypes

Shared aliases: `SuccessCallback`, `ErrorCallback`, `ProfileCallback`, `WalletCallback`, `LeaderboardCallback`, `StorageCallback`, `RpcCallback`, etc.

## Feature Coverage (Cocos column)

Abbreviated from [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md).

| Area | Coverage |
|------|:--------:|
| Core init, auth, profile, wallet, leaderboards, storage, RPC, events/callbacks | Y |
| Real-time Nakama socket | - (use nakama-cpp directly if needed) |
| AI initialize / host | Y |
| AI voice (session-shaped) | S |
| AI LLM classes (NPC, assistant, moderator, content gen, profiler) | S |
| Game modes / lobby / matchmaking (`IVXGameModes`) | S |
| Hiro spin wheel | Y |
| Hiro streaks, offerwall, retention, IAP trigger, smart ad | Y (friend quests/battles `-` in matrix) |
| Discord / Satori | S |
| Deep links | - |
| Bootstrap | Y |

## Platform-Specific: `tick()` and builds

### Cocos scheduler integration

Nakama-cpp processes work on a background runner; **`IVXManager::tick()`** must be called regularly on the main thread (or your chosen thread model per nakama-cpp docs). Typical Cocos pattern:

```cpp
schedule([](float /*dt*/) {
    IntelliVerseX::IVXManager::getInstance().tick();
}, 0.0f, "ivx_tick");
```

Use a **zero or small interval**; do not starve `tick()` during loading screens or the callback queue may backlog.

### Android

- Enable **C++17** in `Application.mk` / Gradle NDK `cppFlags` (`-std=c++17`).
- Align **ABI filters** (`arm64-v8a`, `armeabi-v7a`) with the prebuilt **nakama-cpp** artifacts you link.
- If `FetchContent` runs on a machine without network, vendor nakama-cpp into your tree and point `add_subdirectory` at it.

### iOS / macOS

- Add `intelliversex` to your Xcode target’s **Linked Frameworks and Libraries** (static lib).
- Ensure **Header Search Paths** include the IntelliVerseX `Classes` directory.
- Bitcode: follow your Cocos template defaults; nakama-cpp v2.8.x supports modern Xcode toolchains.

### Windows

- Use the same **runtime library** (`/MD` vs `/MT`) across Cocos, IntelliVerseX, and nakama-cpp to avoid link errors.

## Advanced Examples

### Hiro spin wheel (after auth)

```cpp
#include "IntelliVerseX/IVXHiroSystems.h"

using namespace IntelliVerseX;

IVXHiroSystems::getInstance().getWheelConfig("daily_wheel",
    [](const std::string& json) {
        cocos2d::log("Wheel config: %s", json.c_str());
    },
    [](const std::string& err) {
        cocos2d::log("Hiro error: %s", err.c_str());
    });

IVXHiroSystems::getInstance().spinWheel("daily_wheel",
    [](const SpinWheelReward& r) {
        cocos2d::log("Reward %s x%d", r.rewardId.c_str(), r.amount);
    },
    [](const std::string& err) {
        cocos2d::log("Spin failed: %s", err.c_str());
    });
```

RPC names in this build include `hiro_spin_wheel`, `hiro_spin_wheel_config` — align server RPC registration.

### Authentication flow (device + profile)

```cpp
auto& ivx = IVXManager::getInstance();

ivx.initialize(config);
ivx.authenticateDevice("",
    [&ivx]() {
        ivx.fetchProfile(
            [](const IVXProfile& p) {
                cocos2d::log("Hello %s", p.displayName.c_str());
            },
            [](const std::string& e) {
                cocos2d::log("Profile error: %s", e.c_str());
            });
    },
    [](const std::string& e) {
        cocos2d::log("Auth error: %s", e.c_str());
    });
```

`IVXProfile` and other structs are defined in `IVXTypes.h`.

### AI host (init + session)

```cpp
#include "IntelliVerseX/IVXAIClient.h"

auto& ai = IntelliVerseX::IVXAIClient::getInstance();
ai.initialize("https://ai.intelli-verse-x.ai", "your-api-key", true);

HostProfile hp;
hp.displayName = "Caster";

ai.startHostSession("match-001", hp,
    [](const AISessionResponse& s) {
        cocos2d::log("Host session %s", s.sessionId.c_str());
    },
    [](const std::string& err) { cocos2d::log("%s", err.c_str()); });
```

Voice-heavy paths may remain **stub** until provider wiring is complete; check the matrix before shipping VO features.

## Troubleshooting

### CMake / FetchContent failures

`FetchContent_Declare(nakama-sdk ...)` requires network on first configure. Use `CMAKE_TLS_VERIFY`, corporate proxies, or vendor the tag **v2.8.5** source and replace `FetchContent` with a local `add_subdirectory`.

### nakama-cpp linking errors

- Mixing **debug/release** CRT on Windows causes unresolved symbols — build all targets in the same configuration.
- Duplicate symbols: ensure only **one** copy of nakama-cpp is linked (avoid linking both a prebuilt `.a` and a second static build).

### Android NDK

- **Undefined reference** to C++ std symbols: use `c++_shared` or `c++_static` consistently with Cocos.
- **16 KB page size** (newer Google Play requirements): rebuild native deps with an NDK that emits compliant ELFs; track nakama-cpp and Cocos release notes.

### iOS build / bitcode

If linking fails after Xcode upgrades, rebuild **intelliversex** and **nakama-cpp** with the same Xcode toolchain.

### RPC / JSON mismatches

Cocos Hiro uses snake_case JSON fields in several parsers (`reward_id`, `current_streak`, …). Server payloads must match or parsing yields empty structs without throwing.

### Missing `tick()`

Symptoms: delayed auth callbacks, stuck RPCs. Always schedule `tick()` on a running scene (or global director scheduler).

## Nakama Client

Built on [nakama-cpp](https://github.com/heroiclabs/nakama-cpp). Community projects such as [nakama-cocos2d-x](https://github.com/niceDev0908/nakama-cocos2d-x) illustrate alternate glue layers; this SDK uses the official C++ client inside `IVXManager`.

## Source

- [SDKs/cocos2dx/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/cocos2dx)
- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md)

## Further Reading

- [Nakama C++ client guide](https://heroiclabs.com/docs/nakama/client-libraries/cpp/)
- [Cocos2d-x programming guide](https://docs.cocos2d-x.org/)
- [IntelliVerseX Cocos README](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/blob/main/SDKs/cocos2dx/README.md)
- [Live ops skill](../../.cursor/skills/ivx-live-ops/SKILL.md)
- [Cross-platform skill](../../.cursor/skills/ivx-cross-platform/SKILL.md)
