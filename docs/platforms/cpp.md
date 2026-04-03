# C / C++

> IntelliVerseX native C++ library, built with CMake. Suitable for custom engines, embedded systems, console toolchains, and native applications that need a thin Nakama-backed SDK without a managed runtime.

This page is the **canonical reference** for the `SDKs/cpp` tree; when in doubt, compare with `CMakeLists.txt` and `include/intelliversex/*.h` in the same commit.

## Requirements

- **C++17** compiler (GCC 8+, Clang 7+, MSVC 2019+)
- **CMake** 3.14+
- **[Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp)** — fetched automatically by the IntelliVerseX CMake build (pinned tag in `SDKs/cpp/CMakeLists.txt`, e.g. v2.8.x)
- **libcurl** — `find_package(CURL REQUIRED)`; must be available to the linker on your platform

Optional:

- **OpenXR loader + headers** — only when `IVX_ENABLE_OPENXR=ON` for real XR platform detection

## Installation

### CMake (add_subdirectory)

```cmake
add_subdirectory(path/to/Intelli-verse-X-SDK/SDKs/cpp)
target_link_libraries(your_app PRIVATE intelliversex)
```

Ensure your project already satisfies **CURL** (or install via vcpkg/Conan/system packages) before configuring.

### FetchContent

```cmake
include(FetchContent)
FetchContent_Declare(intelliversex
    GIT_REPOSITORY https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git
    SOURCE_SUBDIR SDKs/cpp
)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(your_app PRIVATE intelliversex)
```

The first configure may take longer while **nakama-cpp** is fetched and built.

## Quick Start

```cpp
#include <intelliversex/ivx.h>

int main() {
    auto& mgr = ivx::Manager::instance();

    ivx::Config cfg;
    cfg.host = "127.0.0.1";
    cfg.port = 7350;
    cfg.serverKey = "defaultkey";
    cfg.debugLogs = true;

    mgr.init(cfg);
    mgr.authDevice("", []() {
        auto& m = ivx::Manager::instance();
        printf("Logged in: %s\n", m.username().c_str());
    });

    while (running) {
        mgr.tick();
    }
    return 0;
}
```

**macOS note:** link **Security.framework** / **SystemConfiguration** only if your stack or CURL backend requires them; the minimal IVX + nakama-cpp path is usually satisfied by CURL’s own dependencies.

**MSVC note:** If you see STL mismatch errors, ensure `/MD` or `/MDd` matches nakama-cpp and CURL binaries (all Release or all Debug).

After authentication, wire **Hiro** to the same Nakama client/session. `ivx::HiroSystems::setNakamaClient(Nakama::NClientPtr, Nakama::NSessionPtr)` requires the underlying nakama-cpp handles. The stock `ivx::Manager` keeps those as private members; typical integrations either construct a parallel `DefaultClient` for Hiro, extend the manager in a fork, or add a thin accessor in your game-specific wrapper. See `ivx_hiro_systems.h` for the exact signature.

## API Surface

Primary types live in the **`ivx`** namespace unless noted. Include everything from `<intelliversex/ivx.h>` or pick headers individually.

| Type / header | Role |
|---------------|------|
| `ivx::Manager` (`ivx_manager.h`) | Singleton: init, auth, profile, wallet, leaderboards, storage, RPC, session persistence, `tick()` |
| `ivx::Config` (`ivx_config.h`) | Host, port, server key, TLS, debug flags, optional `gameId` |
| `ivx::GameModes` (`ivx_game_modes.h`) | **Multiplayer coordination**: mode enum, player slots, rooms, lobby create/join/list, matchmaking hooks, match lifecycle callbacks |
| `ivx::HiroSystems` (`ivx_hiro_systems.h`) | **Hiro / live-ops RPC surface**: spin wheel, streaks, offerwall, friend quests/battles, retention, IAP trigger, smart ad timer |
| `ivx::DiscordSocial` (`ivx_discord_social.h`) | Discord-oriented social stubs: config, friends, invites, rich presence hooks |
| `ivx::IVXSatori` (`IVXSatori.h`) | Satori-style analytics stub: identity, events, flags, experiments, live events |
| `ivx::AIClient` (`ivx_ai_client.h`) | REST AI: voice/host sessions, text, entitlements, personas |
| `ivx::IVXAIAssistant`, `ivx::IVXAIContentGenerator`, `ivx::IVXAIModerator`, `ivx::IVXAINPCDialogManager`, `ivx::IVXAIProfiler`, `ivx::IVXAIVoiceServices` | Higher-level AI helpers (headers prefixed `IVXAI*` in `include/intelliversex/`) |
| `ivx::IVXDiscordMessages`, `ivx::IVXDiscordModeration`, `ivx::IVXDiscordLinkedChannels`, `ivx::IVXDiscordDebug` | Discord DM / moderation / debug stubs |
| `intelliversex::IVXXRHelper` (`ivx_xr.h`) | XR platform detection and capability reporting |
| OpenXR integration | When built with `IVX_ENABLE_OPENXR`, **`IVX_HAS_OPENXR`** is defined and `IVXXRHelper::detectPlatform()` can query the OpenXR runtime; without it, detection returns `XRPlatform::None` and logs a stub message |

**Naming note:** There is no separate type named `Multiplayer` or `HiroClient` in C++; use **`ivx::GameModes`** for lobby/matchmaking and **`ivx::HiroSystems`** for Hiro RPCs.

## Feature Coverage

Legend: **Y** = supported in this SDK layer, **S** = stub / partial / platform-dependent, **—** = not exposed here (use Nakama or custom code).

| Area | C++ |
|------|-----|
| Device / email / Google / Apple / custom auth | Y |
| Session restore / profile / wallet / leaderboards / storage / RPC | Y |
| Real-time socket (Nakama realtime) | S |
| AI: init, host session, REST session flow | Y |
| AI: voice pipeline | S |
| Game modes & lobby (IVX abstraction) | S |
| Hiro: spin, streaks, offerwall, friends, retention, monetization helpers | Y |
| Discord social / DMs / moderation (IVX abstraction) | S |
| Satori (IVX abstraction) | S |
| XR detection | S without OpenXR; Y with OpenXR build |

## Build Options

CMake options defined in `SDKs/cpp/CMakeLists.txt`:

| Option | Default | Meaning |
|--------|---------|---------|
| `IVX_BUILD_SHARED` | `OFF` | Build `intelliversex` as **shared** library (`IVX_EXPORTS` defined). `ON` = shared; `OFF` = static. |
| `IVX_ENABLE_OPENXR` | `OFF` | Find **OpenXR**, link `OpenXR::openxr_loader`, define **`IVX_HAS_OPENXR`** for real XR detection in `ivx_xr.cpp`. |
| `IVX_BUILD_TESTS` | `OFF` | Reserved for C++ unit tests when added to the tree. |
| `IVX_BUILD_EXAMPLES` | `OFF` | Adds `examples/` subdirectory when present. |

**Nakama fetch options** (passed into the bundled nakama-cpp FetchContent):

- `NAKAMA_BUILD_TESTS` / `NAKAMA_BUILD_EXAMPLES` — keep `OFF` for faster SDK builds.

**Platform-specific notes:**

- **Windows:** Ensure CURL is on `CMAKE_PREFIX_PATH` or supplied by vcpkg (`curl` package).
- **Linux:** Install `libcurl4-openssl-dev` (Debian/Ubuntu) or equivalent.
- **macOS:** `brew install curl` or use Xcode’s system libraries if compatible.
- **iOS / embedded:** You may need a prebuilt CURL and a toolchain file; linking nakama-cpp + CURL on mobile is non-trivial — validate with your platform team.
- **C++ standard:** Enforced with `CMAKE_CXX_STANDARD 17` and `CMAKE_CXX_STANDARD_REQUIRED ON`.

## Threading Model

- **`ivx::Manager`**, **`ivx::GameModes`**, **`ivx::HiroSystems`**, **`ivx::AIClient`**, and related singletons are designed for **single-threaded use** from your **main or game thread** (same thread that calls `init()`).
- **`tick()`** must be called regularly on that thread. **Queued Nakama and internal callbacks are dispatched inside `tick()`**, not from a background thread you do not control.
- If you must share the SDK with worker threads, **serialize all calls** with a mutex and still invoke **`tick()`** only from the designated thread, or refactor to post work to your main loop.

This model matches typical engine main loops and avoids data races on session/client pointers.

## Advanced Examples

### Hiro: achievements-style flow via RPC helpers

Hiro features in C++ are exposed as methods on **`ivx::HiroSystems`** that ultimately depend on Nakama RPCs and server modules. After login:

```cpp
ivx::HiroSystems::instance().setNakamaClient(clientPtr, sessionPtr);

ivx::HiroSystems::instance().getSpinWheel(
    [](const ivx::SpinWheelState& s) {
        // drive UI from s.rewards, s.spinsRemaining, etc.
    },
    [](const std::string& err) {
        fprintf(stderr, "Spin wheel error: %s\n", err.c_str());
    });
```

Use **`updateStreak`**, **`getOfferwall`**, **`getActiveFriendQuests`**, and **`challengeFriend`** the same way: always pass success and error callbacks; ensure **`tick()`** runs so completions fire.

### Multiplayer lobby (GameModes)

```cpp
auto& gm = ivx::GameModes::instance();
gm.onMatchFound = [](const ivx::RoomInfo& room) {
    // transition to loading scene, sync MatchConfig from room
};
gm.selectMode(ivx::GameMode::OnlineVersus);
gm.createRoom("MyRoom", ivx::MatchConfig{}, [](const ivx::RoomInfo& info) {
    printf("Created room %s\n", info.roomId.c_str());
});
```

Combine with your own transport (Nakama matchmaker, custom relay, etc.) for production traffic; the IVX layer models **rooms and readiness**, not low-level packets.

## Troubleshooting

### CMake: nakama-cpp not found or fails to build

- Confirm **Git** is available to FetchContent and that outbound HTTPS is allowed.
- Pin or override the `GIT_TAG` in `SDKs/cpp/CMakeLists.txt` only if you need a specific nakama-cpp release; mismatched APIs may break the wrapper.
- Clear `CMakeCache.txt` and rebuild after changing fetch tags.

### CURL not found

- Install development packages for libcurl and set **`CMAKE_PREFIX_PATH`** to the install root.
- On Windows with vcpkg: `vcpkg install curl` and pass `-DCMAKE_TOOLCHAIN_FILE=.../scripts/buildsystems/vcpkg.cmake`.

### C++17 errors

- Verify **/std:c++17** (MSVC) or **-std=c++17** (GCC/Clang). Older compilers must be upgraded; the SDK sets standard properties on the `intelliversex` target, but your executable target must also use C++17 if it includes SDK headers inline.

### OpenXR: detection always `None`

- Build with **`-DIVX_ENABLE_OPENXR=ON`** and install the **OpenXR loader** for your device (SteamVR, Meta, etc.).
- Without **`IVX_HAS_OPENXR`**, `IVXXRHelper::detectPlatform()` is intentionally a **stub** that returns `XRPlatform::None`.

### Link errors duplicate symbols

- Prefer **one** of static or shared `intelliversex` for the whole binary; mixing multiple static copies can duplicate globals. Use `IVX_BUILD_SHARED=ON` if you need a single DLL/SO boundary.

### Verbose diagnostics

- Set **`cfg.debugLogs = true`** in `ivx::Config` during development to surface IntelliVerseX and Nakama-related logging from the manager path.
- XR stub builds print a clear message when **`IVX_HAS_OPENXR`** is undefined; enable OpenXR to replace stubs with runtime queries.

## Cross-compilation

When targeting consoles or embedded Linux, prebuild **nakama-cpp** and **CURL** for the same ABI as your game, then point CMake at those artifacts via `CMAKE_FIND_ROOT_PATH` or a custom toolchain file. FetchContent may still run on the host; ensure the **generator** and **compiler** passed to the Nakama subbuild match your cross target, or vendor prebuilt `nakama-cpp` and replace `FetchContent` with `find_package` in a private fork. Validate **DNS and TLS** on device: Nakama hostnames must resolve from the hardware under test.

## Security and configuration

- **Never ship `defaultkey` in production builds.** Use a server key that matches your Nakama deployment and restrict key usage with appropriate server configuration.
- Prefer **TLS** (`cfg.useTls = true` per `ivx::Config`) when connecting to hosts over the public internet; local development may use plaintext on loopback only.
- **Session files**: `ivx::Manager` persists session material to disk paths derived from its internal layout. On shared machines, ensure the process user directory is not world-readable.
- **API keys** for `ivx::AIClient` and third-party services belong in secure storage or environment injection at build time — not in checked-in source.

## Versioning

The C++ library version string is exposed as **`ivx::Manager::VERSION`** (see `ivx_manager.h`) and tracks the IntelliVerseX release (e.g. 5.8.0). The **SOVERSION** on the shared library target is set in CMake for ABI-aware linking when `IVX_BUILD_SHARED=ON`. When upgrading, rebuild all dependents; nakama-cpp upgrades may require code changes if Heroic Labs changes client APIs.

## Nakama Client

The C++ SDK is built on **[nakama-cpp](https://github.com/heroiclabs/nakama-cpp)**. Version alignment is defined in `SDKs/cpp/CMakeLists.txt` (`FetchContent_Declare` for `nakama-sdk`). For advanced realtime or low-level APIs, use the **`Nakama::`** types from nakama-cpp alongside `ivx::Manager`.

## Source layout

| Path | Contents |
|------|----------|
| `SDKs/cpp/CMakeLists.txt` | Targets, FetchContent for nakama-cpp, CURL, optional OpenXR |
| `SDKs/cpp/include/intelliversex/` | Public headers (`ivx.h` umbrella + modular headers) |
| `SDKs/cpp/src/` | `.cpp` implementations (manager, Hiro, game modes, AI, Discord, Satori, XR) |
| `SDKs/cpp/examples/` | Optional samples when `IVX_BUILD_EXAMPLES=ON` |

## Source

- **[SDKs/cpp/](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/tree/main/SDKs/cpp)** — CMake project, headers under `include/intelliversex/`, implementations under `src/`.

## Further Reading

- [CMake FetchContent](https://cmake.org/cmake/help/latest/module/FetchContent.html) — how nested dependencies (nakama-cpp) are populated.
- [Nakama documentation](https://heroiclabs.com/docs/nakama/) — authentication, storage, RPC, and server runtime.
- [nakama-cpp repository](https://github.com/heroiclabs/nakama-cpp) — client API and platform notes.
- [OpenXR specification](https://www.khronos.org/openxr/) — loader, instance creation, and runtime requirements for **`IVX_ENABLE_OPENXR`**.
- IntelliVerseX repo: **`.cursor/skills/ivx-multiplayer/SKILL.md`**, **`.cursor/skills/ivx-live-ops/SKILL.md`**, **`.cursor/skills/ivx-ai-integration/SKILL.md`** — conceptual maps for multiplayer, Hiro/Satori-style live ops, and AI features (Unity-oriented but useful for backend contracts).

When planning server-side RPC names and payloads for Hiro, cross-check the **Unity** IntelliVerseX module or your game’s Nakama **TypeScript/Lua** modules so C++ `ivx::HiroSystems` calls stay aligned with deployed server logic.
