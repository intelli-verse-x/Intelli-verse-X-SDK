# IVX Console Platform Notes

The IVX C++ multiplayer adapter compiles unchanged against the public
`nakama-cpp` client on Windows, macOS, and Linux. To target Nintendo
Switch, Sony PlayStation 5, or Microsoft Xbox / GDK you swap in the
matching `IConsoleBridge` implementation (`ivx_console_bridge.h`) at
build time.

## Build flags

| Platform | CMake flag (`-D…`) | Source file |
|----------|--------------------|-------------|
| Xbox / GDK | `IVX_PLATFORM_XBOX=ON` | `src/platform/xbox/ivx_xbox_bridge.cpp` |
| PS5 / libNp | `IVX_PLATFORM_PS5=ON`  | `src/platform/ps5/ivx_ps5_bridge.cpp` |
| Switch / NEX | `IVX_PLATFORM_SWITCH=ON` | `src/platform/switch/ivx_switch_bridge.cpp` |
| Desktop (default) | _(no flag)_ | `src/platform/desktop/ivx_desktop_bridge.cpp` |

Exactly one of the platform translation units is compiled per build —
the others are guarded by `#if defined(IVX_PLATFORM_*)` so an SDK
dependency miss never causes a phantom linker error.

## What ships in this repo vs. NDA

This repo carries:

* `ivx_console_bridge.h` — the public bridge header (no NDA exposure).
* Empty platform stubs for the three consoles.
* The desktop bridge (works in CI, dev, and on all open platforms).

The real platform adapters live in licensed-only repos:

* `IVX_Platform_Xbox` — links Microsoft GDK + XSAPI.
* `IVX_Platform_PS5` — links Sony libNp + libSecure.
* `IVX_Platform_Switch` — links Nintendo NEX + nn::account.

Each of those repos drops a single `.cpp` into
`SDKs/cpp/src/platform/<console>/` overriding the stub here and is
delivered through Conan/recipe to studios with the matching dev licence.

## Identity flow

```
                 ┌─────────────────────────┐
                 │  PlatformBridge         │
                 │  (Xbox/PS5/Switch)      │
                 └──────────┬──────────────┘
                            │ ConsoleIdentity
                            ▼
                 ┌─────────────────────────┐
                 │  IVXMultiplayerKernel   │
                 │  (C++ adapter)          │
                 └──────────┬──────────────┘
              authenticateCustom(platform_user_id, identity_ticket)
                            ▼
                 ┌─────────────────────────┐
                 │  Nakama (kernel)        │
                 │  authority validates    │
                 │  identity_ticket via    │
                 │  platform JWKS          │
                 └─────────────────────────┘
```

The IVX server-side runtime (Go module
`data/modules/multiplayer-kernel/identity.go`) verifies the platform
ticket against the documented platform JWKS endpoint. The client never
proves identity to itself — only the server does — so a tampered
client cannot impersonate another platform user.

## Per-cert checklist

| Concern | Xbox | PS5 | Switch |
|---------|:----:|:---:|:------:|
| Graceful offline mode | ✅ | ✅ | ✅ |
| Sign-out mid-match | ✅ | ✅ | ✅ |
| Parental privilege check | ✅ | ✅ | ✅ |
| Cross-network play opt-in toggle | ✅ | ✅ | ✅ |
| Invite send via platform | ✅ | ✅ | ✅ |
| TRC/TCR string surface | ✅ | ✅ | ✅ |

The IVX `IConsoleBridge` surfaces enough hooks (`SubscribeNetwork
Availability`, `RequestIdentity`, etc.) to satisfy each of these without
the IVX core ever importing a console SDK header.

## Voice on console

`livekit-cpp` does not ship official PS5 / Switch builds. On those
platforms the IVX voice provider falls back to:

| Platform | Voice path |
|----------|-----------|
| Xbox / GDK | LiveKit + WebRTC over GDK; spatial audio supported |
| PS5 | Sony NP voice chat via `libVoice`; IVX wraps it as an `IIVXVoice` provider |
| Switch | NEX `nn::nex::voice` (NEX-only) + LiveKit on docked-network builds |

Each voice provider is independently selectable per session via the
kernel `voice_provider_capability` event so a cross-play match can put
two PC players on LiveKit while a PS5 player uses Sony voice chat.
