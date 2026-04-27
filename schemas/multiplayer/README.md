# IVX Multiplayer Kernel — Proto3 Schemas

**Single source of truth** for every byte that crosses the kernel/template
boundary on every adapter (Unity, JS, Unreal, Godot, Flutter, Java, C++,
Cocos2d-x, Defold, Web3, Roblox bridge, Discord Activities, console builds,
visionOS, WebXR).

## Layout

```
schemas/multiplayer/
├── envelope.proto                # MatchEnvelope, ErrorEnvelope, ClientHello/ServerHello
├── opcodes.proto                 # Opcode enum + reserved ranges (single registry)
├── kernel.proto                  # Kernel-core message bodies (Presence, MatchResult, ...)
├── templates/                    # One file per match template
│   ├── sync_turn.proto
│   ├── async_turn.proto
│   ├── realtime_tick.proto
│   ├── lobby_handoff.proto
│   ├── tournament.proto
│   ├── live_event.proto
│   ├── persistent_party.proto
│   ├── avatar_replication.proto      # XR
│   ├── mixed_reality_anchor.proto    # XR
│   └── conversational_party.proto    # social/voice-first
├── services/                     # Cross-cutting kernel services
│   ├── agent.proto               # IIVXAgent (AI agents as first-class peers)
│   └── moderation.proto          # SafetyDecisionLog + actions
└── games/                        # Per-game opcodes (0xC000-0xCFFF range)
    └── quizverse.proto           # QuizVerse (0xC100-0xC1FF)
```

## Reserved opcode ranges (opcodes.proto)

| Range            | Owner                                        |
| ---------------- | -------------------------------------------- |
| `0x0000-0x0FFF`  | Kernel core                                  |
| `0x1000-0x1FFF`  | Social / ConversationalParty                 |
| `0x2000-0x2FFF`  | AI agents (IIVXAgent)                        |
| `0x3000-0x3FFF`  | Moderation                                   |
| `0x4000-0x4FFF`  | SyncTurnMatch                                |
| `0x5000-0x5FFF`  | AsyncTurnMatch                               |
| `0x6000-0x6FFF`  | RealtimeTickMatch                            |
| `0x7000-0x7FFF`  | LobbyHandoffMatch                            |
| `0x8000-0x8FFF`  | TournamentOrchestrator                       |
| `0x9000-0x9FFF`  | LiveEventRoom                                |
| `0xA000-0xAFFF`  | PersistentPartyRoom                          |
| `0xB000-0xBFFF`  | MixedRealityAnchorMatch                      |
| `0xC000-0xCFFF`  | Game-specific (QuizVerse uses 0xC100-0xC1FF) |
| `0xD000-0xEFFF`  | Reserved                                     |
| `0xF000-0xFFFF`  | XR pose fast-path                            |

## Stability rules

1. **Field numbers MUST NOT be reused.** Add new fields with the next free number.
2. **Enum values MUST NOT be repurposed.** Reserve gaps; never delete.
3. **`schema_version`** in `MatchHeader` is bumped only on **breaking** changes; both server and client must agree on the major version. The kernel emits `ERROR_SCHEMA_TOO_OLD` / `ERROR_SERVER_TOO_OLD` with `min_required_version` so clients can guide users to upgrade.
4. **Idempotency:** every client → server opcode that mutates state carries a `client_opcode_uuid`. The server dedupes within a 60-second window.
5. **Reserved fields** in `MatchHeader` (100–999) MUST NOT be removed; they're forward-compat slots.

## Code generation

Run the codegen pipeline to produce TypeScript (server runtime + JS adapter) and C# (Unity adapter) sources:

```bash
cd Intelli-verse-X-SDK/tools/codegen
pnpm install   # one-time
pnpm gen
```

Generated outputs (do not edit by hand):
- `data/modules/src/multiplayer-kernel/proto/v1/` — TypeScript for Nakama runtime + JS adapter
- `Assets/_IntelliVerseXSDK/Multiplayer/Generated/V1/` — C# for Unity adapter
- `SDKs/javascript/multiplayer/src/proto/v1/` — TypeScript for npm package
- Future: Go (`server-go-modules/multiplayer-realtime/proto/v1/`), Dart, Java, C++, Unreal headers.

## Validation

The codegen pipeline runs `buf lint` + `buf breaking` before emitting code. CI fails any PR that:
- introduces a reserved-field reuse
- repurposes an enum value
- removes a public message or field without a major version bump
- references an opcode outside its reserved range

## Conformance

The 12-test conformance suite at `tools/conformance/` exercises every template and service against the generated types. Adapters MUST pass conformance before being merged.
