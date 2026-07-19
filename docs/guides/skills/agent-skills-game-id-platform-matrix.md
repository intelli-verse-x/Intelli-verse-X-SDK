# Agent Skills Game-ID Platform Matrix

This page separates two concepts that are easy to confuse:

- **AI coding Agent Skills** are `SKILL.md` guides that help an IDE agent set up
  IntelliVerseX features in a game project.
- **Live multiplayer AI agents** are Nakama runtime personas controlled through
  RPCs such as `mp_agent_spawn`, `mp_agent_list_personas`, and
  `mp_agent_speak`.

## Current Sign-Off

**AI coding Agent Skills:** documented and available in this repository for the
game-development lifecycle.

**Live in-game Agent Skills SDK integration:** the JavaScript reference adapter
is fully synced with the Nakama server. Typed wrappers are now also present in
Web3, Godot, Defold, C++, Flutter, and Java RPC-first adapters. Unity, Unreal,
Roblox, Cocos2d-x convenience wrappers, and visionOS still need parity work.

Do not claim:

> Every language and platform has live typed Agent Skills integration.

Use this instead:

> Every platform has a documented `gameId` configuration path and generic RPC
> access. JavaScript, Web3, Godot, Defold, C++, Flutter, and Java expose typed
> live Agent Skills wrappers. Remaining platform adapters can call the same
> Nakama RPCs through their generic RPC layer today, but still need first-class
> typed wrappers for full parity.

## Required Game ID

Every title should have a platform Game ID from the IntelliVerseX dashboard or
from:

```bash
POST https://msapi.intelli-verse-x.io/api/games/game/info
```

Use this `gameId` for dashboard correlation, metadata sync, leaderboard
partitioning, game-specific content, and multiplayer match creation.

## Live Nakama Agent RPCs

These are the server RPCs an SDK must expose for first-class live Agent Skills:

- `mp_agent_list_personas`
- `mp_agent_spawn`
- `mp_agent_despawn`
- `mp_agent_speak`

Related multiplayer RPCs:

- `mp_create_match`
- `mp_list_templates`
- `mp_read_match_result`
- `mp_voice_token`

## Platform Matrix

| Platform | Language | `gameId` config | `mp_create_match` | Typed Agent RPCs | Generic RPC fallback | Documentation status |
|---|---|---:|---:|---:|---:|---|
| Unity | C# | Yes | Yes | Pending | Yes | Platform docs exist; Agent RPC wrapper docs pending |
| Unity WebGL | C# | Yes | Yes | Pending | Yes | Platform docs exist; Agent RPC wrapper docs pending |
| JavaScript / Web | TypeScript / JS | Yes | Yes | Yes | Yes | Reference path synced |
| Web3 | TypeScript | Yes | Yes | Yes | Yes | Typed Agent wrappers delegate to JS reference adapter |
| Unreal | C++ / Blueprints | Partial | Yes | Pending | Yes | Platform docs exist; typed Agent wrappers pending |
| Godot | GDScript | Yes | Yes | Yes | Yes | Typed Agent and metadata wrappers added |
| Roblox | Luau | Yes | Partial | Pending | Yes | Platform docs exist; typed Agent wrappers pending |
| Defold | Lua | Yes | Yes | Yes | Yes | Lua facade exposes typed Agent wrappers through native RPC binding |
| C / C++ | C++ | Yes | Yes | Yes | Yes | Typed Agent wrappers added; CMake configure still needs `optional-lite` dependency setup |
| Cocos2d-x | C++ | Yes | Partial | Pending | Yes | Platform docs exist; can use C++ core once bridge exposes wrappers |
| Java / Android | Java | Yes | Yes | Yes | Yes | Java build green with RPC-first adapter; realtime socket helpers deferred |
| Flutter / Dart | Dart | Yes | Yes | Yes | Yes | Typed Agent wrappers added; local Flutter toolchain not installed for verification |
| visionOS | Swift | Example-level | Example-level | Pending | Pending | Platform docs exist; native transport is not full parity |
| Quest / Oculus / OpenXR | C# / C++ / TS | Via Unity/Unreal/Godot/WebXR | Via engine adapter | Pending except JS/WebXR | Yes | XR docs exist; device QA pending |
| Consoles | C# / C++ | Via Unity/Unreal | Via engine adapter | Pending | Yes | NDA-safe docs exist; platform QA pending |
| watchOS | Swift | Not first-class | Pending | Pending | Pending | No full live Agent Skills sign-off |

## Game Kind Coverage

The live Nakama multiplayer server supports these game experience classes:

- Synchronous turn-based games: `sync-turn-v1`
- Asynchronous turn-based games: `async-turn-v1`
- Lobby handoff flows: `lobby-handoff-v1`
- Tournament brackets: `tournament-v1`
- Live event rooms: `live-event-v1`
- Persistent parties: `persistent-party-v1`
- Conversational AI parties: `conversational-party-v1`
- Mixed reality anchors: `mixed-reality-anchor-v1`
- High-frequency avatar replication: `avatar-replication-v1`
- Realtime tick games: `realtime-tick-v1`

This server coverage does not automatically mean every SDK adapter has a
first-class typed helper for every template. When typed helpers are missing, use
`mp_create_match` with the template ID and pass template-specific `template_init`
through the generic RPC/multiplayer layer.

## JavaScript Reference Example

```typescript
const templates = await multiplayer.listTemplates();

const created = await multiplayer.createMatch({
  templateId: "conversational-party-v1",
  gameId: "YOUR_GAME_ID",
  templateInit: {
    allow_agents: true,
    agent_personas: ["ivx-icebreaker"],
    max_members: 0
  }
});

const personas = await multiplayer.listAgentPersonas();
const spawned = await multiplayer.spawnAgent({
  match_id: created.match_id,
  persona_id: personas.personas[0].persona_id,
  spawn_reason: "game_host"
});

await multiplayer.agentSpeak({
  match_id: created.match_id,
  agent_id: spawned.agent_id,
  text: "Welcome to the room.",
  locale: "en-US"
});
```

## Generic RPC Fallback Shape

Adapters without typed Agent RPC wrappers should call the same server RPCs
through their generic Nakama RPC helper:

```json
{
  "id": "mp_agent_spawn",
  "payload": {
    "match_id": "<match-id>",
    "persona_id": "ivx-icebreaker",
    "spawn_reason": "game_host"
  }
}
```

## Required Work For Full All-Platform Sign-Off

1. Add typed Agent RPC wrappers to Unity, Unreal, Roblox, Cocos2d-x bridge, and
   visionOS.
2. Add `listTemplates` and `readMatchResult` typed wrappers to Unity, Unreal,
   Roblox, Cocos2d-x bridge, and visionOS.
3. Add a version-pinned Java realtime socket adapter; the Java SDK currently
   keeps build parity through the stable RPC surface.
4. Install or vendor the C++ `optional-lite` dependency so CMake can configure
   `nakama-cpp` from a clean machine.
5. Verify Flutter with an installed Flutter SDK.
6. Run the multiplayer bot harness against a live Nakama canary/prod endpoint.
7. Add per-platform demo scenes or examples for sync, async, lobby,
   conversational AI avatar, avatar/XR, realtime tick, tournament, live event,
   and persistent party flows.
8. Record platform/device QA evidence for Web, PC, mobile, tablet, Quest/Oculus,
   visionOS, console, and watchOS before using a full end-to-end sign-off.
