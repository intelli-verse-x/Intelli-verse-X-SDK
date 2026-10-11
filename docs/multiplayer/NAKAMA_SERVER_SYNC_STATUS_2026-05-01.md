# Nakama Multiplayer SDK Sync Status - 2026-05-01

**Nakama image checked:** `sha256:b3bc5a3628bea34ca5814c4357b0e94ce69533d375c7b2b6e80765bc26265f30`
**Status:** Reference JS SDK synced; full cross-platform SDK parity is still in
progress. Do not claim 100% all-platform parity yet.

This document records the current sync state between this SDK repository and the
deployed Nakama multiplayer/agent server surface.

## Server Surface To Match

The deployed Nakama server exposes these multiplayer and agent RPCs/templates:

- `mp_create_match`
- `mp_list_templates`
- `mp_read_match_result`
- `mp_voice_token`
- `mp_agent_spawn`
- `mp_agent_despawn`
- `mp_agent_list_personas`
- `mp_agent_speak`
- `sync-turn-v1`
- `async-turn-v1`
- `lobby-handoff-v1`
- `tournament-v1`
- `live-event-v1`
- `persistent-party-v1`
- `conversational-party-v1`
- `mixed-reality-anchor-v1`
- `avatar-replication-v1`
- `realtime-tick-v1`

## What Is In Sync

The SDK repo contains the canonical multiplayer schemas:

- `schemas/multiplayer/envelope.proto`
- `schemas/multiplayer/kernel.proto`
- `schemas/multiplayer/opcodes.proto`
- `schemas/multiplayer/services/agent.proto`
- `schemas/multiplayer/services/voice.proto`
- `schemas/multiplayer/templates/*.proto`

The SDK has adapter coverage for many platform families:

- Unity/C# under `Assets/Intelli-verse-X-SDK`
- JavaScript/TypeScript under `SDKs/javascript/packages/multiplayer`
- C++ under `SDKs/cpp`
- Java/Android under `SDKs/java`
- Flutter/Dart under `SDKs/flutter`
- Unreal under `SDKs/unreal`
- Godot under `SDKs/godot`
- Roblox/Luau under `SDKs/roblox`
- Defold/Lua under `SDKs/defold`
- Web3/TypeScript under `SDKs/web3`
- visionOS/Swift under `SDKs/visionos`

Observed coverage:

- `mp_create_match` is implemented in several adapters, including JS, C++,
  Java, Flutter, Godot, Roblox, Unreal, Web3, and visionOS examples.
- `mp_list_templates` and `mp_read_match_result` are implemented in JS, Java,
  Flutter, Godot, Defold, C++, Web3, and Roblox.
- `mp_agent_spawn`, `mp_agent_despawn`, `mp_agent_list_personas`, and
  `mp_agent_speak` are implemented in JS, Java, Flutter, Godot, Defold, C++,
  and Web3.
- `mp_voice_token` is implemented in JS, Godot, Unreal, and documented Unity
  paths.
- `avatar-replication-v1` has JS/WebXR, Unity, and visionOS paths.
- `conversational-party-v1` appears in JS Discord activity and Roblox examples.
- QA scripts exist under `tools/qa/multiplayer-bot-harness/scripts`.

## What Is Not Yet In Sync

Agent skill RPCs are not yet exposed across every SDK adapter:

- JS, Java, Flutter, Godot, Defold, C++, and Web3 expose
  `mp_agent_spawn`.
- JS, Java, Flutter, Godot, Defold, C++, and Web3 expose
  `mp_agent_despawn`.
- JS, Java, Flutter, Godot, Defold, C++, and Web3 expose
  `mp_agent_list_personas`.
- JS, Java, Flutter, Godot, Defold, C++, and Web3 expose
  `mp_agent_speak`.
- Unity, Unreal, Roblox, Cocos2d-x bridge, and visionOS still need matching
  typed wrappers.
- No cross-platform `AgentSkillClient` implementation was found.

Core metadata RPC parity is uneven:

- `mp_create_match` is broadly implemented.
- `mp_list_templates` and `mp_read_match_result` are implemented in JS, Java,
  Flutter, Godot, Defold, C++, Web3, and Roblox. Unity, Unreal, Cocos2d-x
  bridge, and visionOS still need matching convenience wrappers.

Build/test parity is not green:

- JS multiplayer conformance tests passed: `12/12`.
- JS multiplayer type-check passed.
- JS multiplayer package build passed, including declaration output.
- Multiplayer bot harness installs, builds, and passes the synthetic smoke
  script.
- Java SDK build passed after moving the multiplayer adapter to a buildable
  RPC-first surface. Realtime socket helpers are deferred until the Nakama Java
  transport API is version-pinned and verified.
- Flutter tests could not run because `flutter` was not available locally.
- Web3 SDK build passed after fixing TypeScript declaration errors.
- C++ CMake configure is blocked by the transitive `nakama-cpp`
  `optional-lite` package dependency on this machine.

## Verification Commands Run

```bash
cd SDKs/javascript/packages/multiplayer
npm test
```

Result: passed, `12` conformance tests.

```bash
cd SDKs/javascript/packages/multiplayer
npm run lint
```

Result: passed.

```bash
cd SDKs/javascript/packages/multiplayer
npm run build
```

Result: passed, including declaration output.

```bash
cd tools/qa/multiplayer-bot-harness
npm install
npm run build
node dist/runner.js --target synthetic --script scripts/smoke_synthetic.yaml --report junit --out /tmp/ivx-smoke-synthetic.junit.xml
```

Result: passed. Synthetic harness smoke passed `2/2` expectations.

```bash
cd SDKs/java
./gradlew test
```

Result: failed.

Main issue: `IVXMultiplayerKernel.java` targets a Nakama Java API shape that no
longer matches the dependency in this Gradle project. Missing/changed types and
future APIs include `MatchData`, `MatchPresenceEvent`, `SocketClient.addListener`,
`SocketClient.connect`, and `ListenableFuture` vs `CompletableFuture`.

```bash
cd SDKs/flutter
flutter test
```

Result: not run. `flutter` command not available locally.

```bash
cd tools/qa/multiplayer-bot-harness
npm run build
```

Result: passed after switching the local SDK dependency from unsupported
`workspace:*` to a relative `file:` dependency.

## Required Work For 100% Sync

1. Add typed Agent Skills wrappers to Unity, Unreal, Roblox, Cocos2d-x bridge,
   and visionOS.
2. Add `listTemplates` and `readMatchResult` wrappers to Unity, Unreal,
   Cocos2d-x bridge, and visionOS.
3. Add a version-pinned Java realtime socket transport. Java currently covers
   the stable multiplayer/agent RPC surface.
4. Install or vendor C++ `optional-lite` so clean CMake configure can complete.
5. Add a real live Nakama target mode to the bot harness, instead of relying on
   synthetic mode when a turnkey SDK factory is not present.
6. Install/configure Flutter SDK and run Flutter tests.
7. Run bot harness scripts against a live Nakama canary/prod endpoint.
8. Add CI gates that fail when SDK adapters drift from `schemas/multiplayer`.
9. Add end-to-end demo runs per platform:
   - sync
   - async
   - lobby
   - conversational AI avatar
   - avatar/XR
   - realtime tick

## Current Sign-Off

Use this status:

> JS, Java, Flutter, Godot, Defold, C++, and Web3 now expose the deployed Nakama
> multiplayer/agent RPC surface. JS, Java, Web3, and the bot harness pass the
> available local checks. The whole SDK repository is not yet 100% synchronized
> across every platform because Unity, Unreal, Roblox, Cocos2d-x bridge, and
> visionOS still need typed wrapper parity, Flutter tooling is unavailable
> locally, C++ clean configure needs `optional-lite`, and live canary/prod
> bot-harness runs are still pending.

Do not use:

> The SDK is fully synced for all platforms and all Agent Skills.
