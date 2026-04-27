# IntelliVerseX Multiplayer Kernel — Cross-Engine Adapter Map

The IntelliVerseX Multiplayer Kernel is a **single server brain** (Nakama +
the kernel JS/TS module + the `realtime_tick` Go plugin) that speaks one
wire protocol — `schemas/multiplayer/*.proto` — to **eleven** clients.

Every client adapter implements the **same** logical contract:

```
IIVXMultiplayer
 ├─ initialize() / shutdown()
 ├─ createMatch(req)              -> CreateMatchResponse
 ├─ joinMatch(matchId)            -> IIVXMatchSession
 ├─ createAndJoin(req)            -> IIVXMatchSession
 ├─ onTransportStateChanged(h)
 └─ onKernelError(h)

IIVXMatchSession
 ├─ subscribe(opcode, h) / subscribeRange(from, to, h)
 ├─ send(opcode, payload)          // adapter stamps {h:{s,t,u}}
 ├─ leave() / dispose()
 ├─ onWelcome / onPlayerJoined / onPlayerLeft / onMatchEnded / onError
 └─ onStateChanged
```

Game code written against this contract ports across engines without any
behavioural rewrite.

## Adapter file map

| Engine / Runtime    | Path                                                                                                  | Underlying client                  |
| ------------------- | ----------------------------------------------------------------------------------------------------- | ---------------------------------- |
| Unity (C#)          | `Assets/Intelli-verse-X-SDK/MultiplayerKernel/`                                                       | Nakama-Unity (`com.heroiclabs.nakama`) |
| JavaScript / TS     | `SDKs/javascript/packages/multiplayer/src/`                                                           | `@heroiclabs/nakama-js`            |
| Unreal Engine 5     | `SDKs/unreal/Source/IntelliVerseX/Public/IVXMultiplayerKernel.h` + `Private/IVXMultiplayerKernel.cpp` | `Nakama` UE plugin                 |
| Godot 4             | `SDKs/godot/addons/intelliversex/multiplayer/ivx_multiplayer_kernel.gd`                               | `addons/com.heroiclabs.nakama`     |
| Flutter / Dart      | `SDKs/flutter/lib/src/multiplayer/ivx_multiplayer_kernel.dart`                                        | `nakama` (pub.dev)                 |
| Java / Android      | `SDKs/java/src/main/java/com/intelliversex/sdk/multiplayer/IVXMultiplayerKernel.java`                 | `com.heroiclabs.nakama:nakama-java` |
| C++ (native)        | `SDKs/cpp/include/intelliversex/ivx_multiplayer_kernel.h` + `src/intelliversex/ivx_multiplayer_kernel.cpp` | `nakama-cpp`                  |
| Cocos2d-x           | `SDKs/cocos2dx/Classes/IntelliVerseX/IVXMultiplayerKernel.h` (re-exports the C++ adapter)             | `nakama-cpp` via Cocos2d-x build   |
| Defold              | `SDKs/defold/intelliversex/multiplayer_kernel.lua` (Lua facade over a C++ extension)                  | `nakama-cpp` via Defold extension  |
| Web3                | `SDKs/web3/src/IVXMultiplayerKernelWeb3.ts` (decorates the JS adapter)                                | JS adapter + ethers.js signer      |
| Roblox              | `SDKs/roblox/...` (RPC-only bridge — see `roblox/README.md`)                                          | RPC bridge to Nakama RPC layer     |

## Wire protocol

Source of truth: `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.

Envelope on the wire (every match-data message):

```json
{
  "h": { "s": <seq>, "t": <match_time_ms>, "u": <uuid_v4> },
  "p": <opcode_specific_payload>
}
```

Reserved opcode ranges (see `opcodes.proto`):

| Range            | Owner                                |
| ---------------- | ------------------------------------ |
| `0x0000–0x0FFF`  | Kernel core (welcome, player_joined, …) |
| `0x1000–0x1FFF`  | SyncTurnMatch                        |
| `0x2000–0x2FFF`  | AsyncTurnMatch                       |
| `0x3000–0x3FFF`  | LobbyHandoffMatch                    |
| `0x4000–0x4FFF`  | RealtimeTickMatch (Go plugin)        |
| `0x8000–0x8FFF`  | TournamentOrchestrator               |
| `0x9000–0x9FFF`  | LiveEventRoom                        |
| `0xA000–0xAFFF`  | PersistentPartyRoom                  |
| `0xC000–0xCFFF`  | Game-defined (QuizVerse: `0xC100..0xC1FF`) |

## Production rollout policy

QuizVerse is the first game on the kernel; all other games inherit the
proven policy:

1. **Build** the JS kernel (`cd nakama/data/modules && npm run build`) and
   the Go plugin (`bash nakama/scripts/build-plugin.sh`).
2. **Deploy** to staging behind feature flag `mp.kernel.enabled=true` with
   `mp.kernel.percent=0`.
3. **QA via the conformance suite** (12 invariant tests) — see
   `docs/multiplayer-kernel/CONFORMANCE.md`.
4. **Canary 1%** → soak 24h → check parity dashboard
   (`mp_parity_violation` rate < 0.1%, `mp_router_auto_rollback` count = 0).
5. **Expand** to 10% → 50% → 100% with the same gate at each step.
6. **Photon strip** (PUN/Realtime/Chat; Voice retained behind a separate
   provider) once 100% has soaked for 7 days.

Per-game knobs (set in Satori console):

* `mp.kernel.enabled` (bool — kill-switch)
* `mp.kernel.percent` (int 0..100)
* `mp.kernel.per_mode_percent` (JSON dict — overrides per game mode)
* `mp.kernel.allowlist_user_ids` / `mp.kernel.denylist_user_ids`
* `mp.kernel.shadow_mode` (bool — Photon canonical + kernel observer)
* `mp.kernel.auto_rollback_threshold_pct` (int 0..100; default 5)

QuizVerse-specific glue lives at:

* `Assets/_QuizVerse/Scripts/MultiPlayer/Kernel/Rollout/QvRolloutFlags.cs`
* `Assets/_QuizVerse/Scripts/MultiPlayer/Kernel/Rollout/QvMultiplayerRouter.cs`
* `Assets/_QuizVerse/Scripts/MultiPlayer/Kernel/Rollout/QvParityLogger.cs`
* `Assets/_QuizVerse/Scripts/MultiPlayer/Kernel/Rollout/QvRolloutHealth.cs`

Other games copy this `Rollout/` folder pattern with a per-game prefix.

## Cross-platform feature parity

| Feature                        | Unity | JS  | UE5 | Godot | Flutter | Java | C++ | Cocos | Defold | Web3 |
| ------------------------------ | :---: | :-: | :-: | :---: | :-----: | :--: | :-: | :---: | :----: | :--: |
| SyncTurnMatch                  | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| AsyncTurnMatch                 | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| LobbyHandoffMatch              | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| RealtimeTickMatch (Go module)  | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| TournamentOrchestrator         | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| LiveEventRoom                  | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| PersistentPartyRoom            | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| ConversationalPartyMatch       | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| LiveKit voice (XR / agents)    | ✅    | ✅  | ✅  | ⚠️    | ⚠️      | ⚠️   | ⚠️  | ⚠️    | ⚠️     | ✅   |
| AI agent kernel (`IIVXAgent`)  | ✅    | ✅  | ✅  | ✅    | ✅      | ✅   | ✅  | ✅    | ✅     | ✅   |
| WebRTC handoff (P2P)           | ✅    | ✅  | ✅  | ⚠️    | ⚠️      | ⚠️   | ✅  | ⚠️    | ⚠️     | ✅   |

Legend: ✅ first-class · ⚠️ requires engine-native voice plugin (see
per-engine README).

## What "first-class" means for the new wave adapters

The Wave 2 adapters (Unreal, Godot, Flutter, Java, C++, Cocos2d-x, Defold,
Web3) implement the **full** `IIVXMultiplayer` / `IIVXMatchSession`
contract — match create / join / leave, opcode subscribe / range-subscribe
/ send, transport-state observability, error fan-out.

They reach the kernel through their engine's **official Nakama client**, so
ALL kernel templates (every row in the table above) are reachable from day
one — the templates are server-side and the wire protocol is identical
across clients.

Engine-native conveniences (e.g. Blueprint nodes for Unreal, Godot signals,
Dart streams, Flutter `Provider` integration) live in
`SDKs/<engine>/examples/` — those examples consume the contract in this
file; they don't extend it.

## Production sign-off (Pillar 10)

Each adapter ships with:

* A README describing its build wiring & engine-specific gotchas.
* A conformance test harness in `SDKs/<engine>/tests/` that exercises the
  12-invariant kernel test suite (welcome, ack, dedup, late-join, rejoin,
  reorder, idle, terminate, agent-presence, voice-mute, signed-payload,
  rate-limit). Adapters that fail any invariant fail CI and can't ship.
* A SLO board entry in `docs/multiplayer-kernel/SLO.md`:
  * P95 send→ack < 120 ms
  * Reconnect success > 99% within 10 s
  * Parity-violation rate < 0.1% (vs. canonical Photon path during shadow)
* A staged-rollout policy identical to QuizVerse's.

If your game is on engine X and you don't see it in `SDKs/X/`, the
contract is portable: implement `IIVXMultiplayer` over your engine's
Nakama bindings and you're done. The server doesn't care which client
you are.
