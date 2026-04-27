# Cross-Engine 3D-World E2E Analysis — LiveKit Voice + Avatars + Vision

**Companion document to:** `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md`
**Scope:** Unreal Engine 5, JavaScript / Three.js / Babylon.js, Godot 4,
visionOS (Swift), and the second-tier engines (Flutter, Java/Android,
native C++, Cocos2d-x, Defold, Web3, Roblox).

**Reference matrix:** `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md`,
`docs/livekit/PHASE_5_VISION_RESEARCH.md`, this turn's `CHANGELOG`.

---

## TL;DR

A game developer in **any** non-Unity language can now reach the same
five end-states that a Unity developer can:

1. **Multiplayer kernel** — every adapter speaks the same wire protocol.
2. **LiveKit voice** — first-class on Unity / JS / UE5 / visionOS / Web3
   (decorates JS); voice-token *mint helpers* now ship on Unity / JS /
   UE5 / Godot, so any of those stacks can request a token in 1-2 lines.
3. **Avatar lip-sync (`viseme.v1`)** — first-class on Unity / JS / UE5 /
   visionOS / Godot (pure-GDScript decoder).
4. **Vision-enabled AI avatar** — server-side change only (Phase-5 worker),
   so *every* engine inherits it transparently. The client work is the
   exact same TTS+blendshape pipeline that already works.
5. **Spatial anchors** — Unity has the broadest coverage (Meta + ARKit
   Collab + OpenXR-MSFT + AR Foundation + QR fallback). UE5 / JS / Godot
   currently rely on engine-native AR plugins — see §11 for status &
   roadmap per engine.

Newly-shipped this turn:

| Engine | New file | What it closes |
|---|---|---|
| JS / TS | `voice/token-client.ts` (`mintVoiceToken()`) | Manual JSON RPC for `mp_voice_token` |
| JS / TS | `avatar/livekit-viseme-receiver.ts` (`attachLiveKitRoom()`) | Manual `Room.on('dataReceived')` plumbing |
| Unreal | `IVXVoiceTokenClient.h/.cpp` (`MintAsync`) | Blueprint had no node for `mp_voice_token` |
| Godot | `multiplayer/ivx_voice_token_client.gd` | No GDScript helper for `mp_voice_token` |
| Godot | `multiplayer/ivx_livekit_viseme_receiver.gd` | No JSON-decoder for `viseme.v1` data channel |
| **Unity** | **`MultiplayerKernel/Avatar/IVXAvatarReplicator.cs`** | **Remote-human pose replication on Unity (closed the last critical-path gap — AI avatars already worked end-to-end via the agent worker; humans now ride the same wire)** |

---

## §1. Engine-by-engine readiness matrix

| Capability                                 | UE5 (C++/BP) | JS / TS / Three.js | Godot 4 (GDScript) | visionOS (Swift) | Flutter / Java / C++ / Cocos / Defold / Web3 / Roblox |
| ------------------------------------------ | :----------: | :----------------: | :----------------: | :--------------: | :---------------------------------------------------: |
| Multiplayer kernel (mp_create_match etc.)  | ✅           | ✅                 | ✅                 | ✅               | ✅ (all)                                              |
| `mp_voice_token` typed mint helper         | **✅ (new)** | **✅ (new)**       | **✅ (new)**       | ✅               | ⚠ raw RPC (1-2 lines, see §6 cookbook)                |
| LiveKit voice provider                     | ✅           | ✅                 | ⚠ BYO GDExtension  | ✅               | ⚠ BYO native binding (see §11)                        |
| LiveKit `viseme.v1` receiver               | ✅           | **✅ (auto-bind new)** | **✅ (new)**    | 🟡 in voice prov. | ⚠ pure-decoder pattern is portable (§6)               |
| WebXR / native-XR pose adapter             | ✅ via OpenXR | ✅ (`webxr/adapter`)| ✅ via OpenXR      | ✅ ARKit          | ➖ N/A (mobile-only / no XR)                          |
| Spatial anchors                            | 🟡 OpenXR    | 🟡 WebXR `XRAnchor` | 🟡 OpenXR / ARCore | ✅ ARKit Collab  | ➖ Out of scope (mobile-2D / server-only adapters)    |
| Avatar replicator (Unity)                  | n/a (Unity row) | n/a            | n/a                | n/a              | n/a — see Unity column: **✅ `IVXAvatarReplicator.cs` (new)** |
| Avatar replicator (other engines)          | 🔴 (proto only) | 🔴 (proto only) | 🔴 (proto only)    | ✅ `IVXAvatarReplicator.swift` | ➖ N/A (no XR avatars) |
| Vision-enabled AI avatar (server-side)     | ✅ inherits  | ✅ inherits        | ✅ inherits        | ✅ inherits      | ✅ inherits                                           |

**Legend:** ✅ first-class · 🟡 partially landed · ⚠ requires
engine-native binding (documented BYO path) · 🔴 client-side gap to be
filled by game · ➖ not applicable for that engine's typical surface.

---

## §2. Unreal Engine 5 — end-to-end dry run

### 2.1 What now works in C++ / Blueprints

```cpp
// 1. Bootstrap Nakama (uses the Heroic Labs UE plugin)
UNakamaClient*  Client  = UNakamaManager::CreateClient(...);
UNakamaSession* Session = await Client->AuthenticateDevice(...);
UIVXMultiplayer* Mp = UIVXMultiplayer::GetInstance(this);
Mp->SetNakamaClient(Client, Session);

// 2. Create or join a match
Mp->CreateLobby("Trivia Night", 8, true,
    FIVXLobbyDelegate::CreateLambda([this](bool ok, const FIVXLobby& Lobby) {
        if (!ok) return;
        // 3. Mint a LiveKit voice token via the new helper
        FIVXMintVoiceTokenRequest Req;
        Req.Client   = Client; Req.Session = Session;
        Req.MatchId  = Lobby.LobbyId;     // or the kernel match_id
        Req.bSpatial = true;              // multi-human Party
        UIVXVoiceTokenClient::MintAsync(Req,
            FIVXMintVoiceTokenSuccess::CreateLambda([this](const FIVXVoiceSessionToken& T) {
                // 4. Connect LiveKit, then attach the viseme stream to the AI avatar mesh
                Voice = NewObject<UIVXLiveKitVoiceProvider>(this);
                Voice->Connect(T);
                VisemeStream = NewObject<UIVXLiveKitVisemeStream>(this);
                VisemeStream->TargetMesh = AvatarSkeletalMesh; // your USkeletalMeshComponent
                VisemeStream->SetArkit52NameMap(MyArkit52BlendshapeNames); // 52 FNames
                // The agent worker will publish `viseme.v1` packets that
                // flow through Voice → Room::OnDataReceived → VisemeStream.
            }),
            FIVXMintVoiceTokenFailure::CreateLambda([](const FString& C, const FString& M) {
                UE_LOG(LogTemp, Error, TEXT("voice token failed: %s %s"), *C, *M);
            })
        );
    })
);
```

### 2.2 Edge cases & must-handle exceptions

| Scenario | Code path | What you must handle |
| --- | --- | --- |
| `bad_args` (null Client/Session/match_id) | `UIVXVoiceTokenClient::MintAsync` early-out | Validate before calling — guard your UI button |
| `session_expired` (Nakama session) | Returns code `session_expired` | Refresh via `UNakamaClient::RefreshSession(...)` then retry |
| `voice_unconfigured` (kernel returned empty) | Kernel feature flag off OR LiveKit env missing | Surface "voice unavailable" UI; fall back to text chat |
| `rpc_failed` (5xx / network) | Returns Nakama error message | Exponential backoff (250 ms × 2 up to 8 s) — same as Unity |
| Token expires mid-match | `UIVXLiveKitVoiceProvider::OnVoiceUnavailable` fires `expired` reason | Re-mint with the new helper, call `Connect(NewToken)` |
| Cohort fallback (`mp.kernel.livekit.cohort=low`) | Kernel returns `provider=None` | Disable voice UI; the kernel will route AI text-only |
| `viseme.v1` topic but mesh has no `MorphTarget` for the FName | `USkeletalMeshComponent::SetMorphTarget` no-op | Ship a default `BlendshapeNameMap` that covers all 52 ARKit indices |
| Multi-avatar lobby (Phase-5) — different `publisher_identity` per agent | One `UIVXLiveKitVisemeStream` per agent recommended | Filter by `publisher_identity` (a Phase-5 envelope field) — see PHASE_5_VISION_RESEARCH §12 |

### 2.3 Must-haves the game team owns (not in SDK)

* **`UIVXAvatarReplicator`** — the proto + opcodes ship in
  `schemas/multiplayer/avatar_replication.proto`, but the UE5
  C++/Blueprint component that publishes head/hand poses does not.
  Mirror Unity's `IIVXAvatar` interface and use the existing
  `IVX_XR_OP` opcode constants. Roughly 200 lines of UCLASS.
* **Anchor providers for ARKit/ARCore on UE5** — UE has its own
  `ARSession` + `UARGeoAnchor`. We have not yet wrapped that into an
  `IIVXAnchorProvider` C++ class. For Phase-4/5 voice + avatar this is
  not required (LiveKit + the kernel work without spatial anchors); it
  becomes required only when the game wants device-anchored worlds.

### 2.4 Build / packaging notes

* `IVXVoiceTokenClient.h/.cpp` are inside the existing `IntelliVerseX`
  module and link against `Json` + `JsonUtilities`, which are already
  in `IntelliVerseX.Build.cs`. No new dependency.
* `NAKAMA_UE_HAS_SESSION_ISEXPIRED` define guards the optional
  `IsExpired()` check. Plugin versions ≥ 2.4 expose it; older versions
  silently skip the check (the RPC will surface a real 401 instead).

---

## §3. JavaScript / TypeScript — Three.js, Babylon.js, A-Frame

### 3.1 What now works in 30 lines

```ts
import { Client } from "@heroiclabs/nakama-js";
import { Room } from "livekit-client";
import {
  IVXNakamaMultiplayer, mintVoiceToken, IVXLiveKitVoiceProvider,
  IVXLiveKitVisemeReceiver,
} from "@intelliversex/multiplayer";

const client  = new Client(...);
const session = await client.authenticateDevice(...);
const socket  = client.createSocket();
await socket.connect(session, true);

const mp = new IVXNakamaMultiplayer({ client, session, socket });
await mp.initialize();
const { match_id } = await mp.createMatch({ templateId: "sync-turn-v1", ... });
const matchSession = await mp.joinMatch(match_id);

// === Voice + Avatar (the two new helpers) ===
const token = await mintVoiceToken({
  client, session, matchId: match_id, spatial: false,
});
const voice = new IVXLiveKitVoiceProvider(/* lk room ctor */);
await voice.connect(token);

const receiver = new IVXLiveKitVisemeReceiver();
const detach = receiver.attachLiveKitRoom(voice.room as Room);
receiver.driveMesh(threeHeadMesh, arkitToMorphMap);
// All five `on('frame'…)` events fire automatically.
```

### 3.2 Edge cases & must-handle exceptions

| Scenario | Code path | What you must handle |
| --- | --- | --- |
| `IVXVoiceTokenError("bad_args"\|"session_expired"\|"rpc_failed"\|"voice_unconfigured"\|"decode_failed"\|"invalid_token")` | All thrown by `mintVoiceToken` | `try { await mintVoiceToken(…) } catch (e: IVXVoiceTokenError) { switch(e.code) … }` |
| `livekit-client` not bundled | `attachLiveKitRoom(room)` accepts an EventEmitter shape | The helper compiles even if the dep is missing — just pass any object with `.on/.off("dataReceived", cb)` |
| Browser blocks autoplay until user gesture | LiveKit `Room.startAudio()` | Wire to a "Tap to enable voice" UI button |
| Page hidden tab → audio context suspends | Browser AudioContext state | Resume on `visibilitychange`; LiveKit handles re-publish |
| WebXR `XRSession` ends mid-match | `IVXWebXRAdapter.detach()` listens for `xrSession.end` | No-op for voice; pose publication just stops |
| **Memory: `morphTargetInfluences` is borrowed, not copied** | `receiver.driveMesh(target, map)` | Don't reuse the receiver across two meshes simultaneously — make two receivers |
| `dataReceived` fires before the receiver's `header` arrived | Receiver tolerates out-of-order frames | Header / footer events are *advisory*; the renderer only needs `frame` |
| Three.js `MorphTargetDictionary` missing some ARKit names | `arkitToMorphMap` returns undefined index | Build the map with `Object.entries(mesh.morphTargetDictionary)` and skip absent indices |

### 3.3 Must-haves the game team owns

* **`IVXAvatarReplicator` for WebXR** — `webxr/adapter.ts` already
  publishes head + hand poses. The "render peer avatar" code (decode
  `IVX_XR_OP.HEAD_POSE` → set `Object3D` matrix) is one screen of code
  in the consuming game; we don't ship a renderer because the
  scene-graph is engine-specific (Three.js vs. Babylon vs. A-Frame).
* **WebXR anchors (`XRAnchor`)** — landed in WebXR Module v3 (Quest
  Browser ≥ 38, visionOS Safari ≥ 2.0). The IVX kernel proto is ready;
  a 60-line wrapper (`webxr/anchor-provider.ts`) is the next gap. Not
  required for Phase-4 voice + avatar.

### 3.4 Build / packaging notes

* `mintVoiceToken` is exported from `@intelliversex/multiplayer/voice`
  and re-exported from the top-level `index.ts`. No new dep.
* `attachLiveKitRoom` accepts a structural type (`{ on, off? }`) so
  consumers don't pull `livekit-client` into their type graph unless
  they want to.
* Tree-shaking-friendly — `mintVoiceToken` is a free function, not a
  class. ~ 1 KB minified + gzip.

### 3.5 Three.js end-to-end dry-run script

```ts
// 1. Wire WebXR adapter to a joined match (already supported).
const xrAdapter = new IVXWebXRAdapter(matchSession);
xrAdapter.attach(xrSession, await xrSession.requestReferenceSpace("local-floor"));

// 2. Hook AI avatar lip-sync to a glTF head loaded via GLTFLoader.
const headMesh = gltf.scene.getObjectByName("HeadMesh") as THREE.Mesh;
const arkitToMorph: Record<number, number> = {};
for (const [name, idx] of Object.entries(headMesh.morphTargetDictionary ?? {})) {
  const arkitIdx = ARKIT52_NAMES.indexOf(name);
  if (arkitIdx >= 0) arkitToMorph[arkitIdx] = idx as number;
}
receiver.driveMesh(headMesh, arkitToMorph);

// 3. Render-loop: just `renderer.setAnimationLoop(() => renderer.render(scene, camera))`.
//    The receiver writes morphTargetInfluences[]; Three.js picks them up automatically.
```

Edge-case dry-run highlights:

* **Quest 3 Browser, two avatars in lobby:** create two
  `IVXLiveKitVisemeReceiver` instances, set
  `publisher_identity_filter` on each (matches the Phase-5 envelope
  field). Both attach to the same Room — payload routing is by topic
  + filter, no extra wiring.
* **Mid-session token rotation (after 1 h):** the kernel emits
  `voice-unavailable` with `reason="expired"`. Catch in the provider's
  `on('voice-unavailable')` and re-mint via `mintVoiceToken`.
* **Mobile Safari on iOS (no WebXR):** WebXR adapter `attach()`
  silently no-ops; voice + viseme still work over WebRTC.

---

## §4. Godot 4 (GDScript) — end-to-end dry run

### 4.1 What now works in pure GDScript

```gdscript
# 1. Bootstrap Nakama (Heroic Labs Godot addon).
var nakama_client = Nakama.create_client(...)
var nakama_session = await nakama_client.authenticate_device_async(...)

# 2. Multiplayer kernel.
var kernel := IVXMultiplayerKernel.new(nakama_client, nakama_session)
await kernel.initialize()
var resp := await kernel.create_match({"template_id": "sync-turn-v1"})
var session = await kernel.join_match(resp.match_id)

# 3. Mint a LiveKit voice token via the new helper.
var voice_client := IVXVoiceTokenClient.new()
var token := await voice_client.mint_async({
    "client":   nakama_client,
    "session":  nakama_session,
    "match_id": resp.match_id,
    "spatial":  false,
})
if token.has("error"):
    push_warning("voice token failed: %s — %s" % [token.error, token.message])
else:
    # 4a. Hand the token to a LiveKit GDExtension (BYO; the popular one is
    #     `livekit-client-godot`). Pass `token.url` and `token.token`.
    livekit.connect_to_room(token.url, token.token)
    # 4b. Decode `viseme.v1` with the new pure-GDScript receiver.
    var receiver := IVXLiveKitVisemeReceiver.new()
    receiver.on_frame.connect(func(frame): _apply_blendshapes(frame.blendshapes))
    livekit.data_received.connect(func(payload, topic):
        receiver.on_livekit_data(payload, topic))
```

### 4.2 Edge cases & must-handle exceptions

| Scenario | Code path | What you must handle |
| --- | --- | --- |
| `bad_args`/`session_expired`/`rpc_failed`/`voice_unconfigured`/`decode_failed`/`invalid_token` | `IVXVoiceTokenClient.mint_async` returns `{"error": …, "message": …}` | Branch on `token.error` — never throw, easier on the GDScript control flow |
| **No first-class LiveKit binding for Godot** | BYO via [`livekit-client-godot`](https://github.com/livekit/client-sdk-godot) (community) or the C++ binding compiled as a GDExtension | The voice-token mint helper produces the same `url` + `token` shape every binding expects |
| Receiver-side `payload` sometimes arrives as `String` (some bindings) and sometimes as `PackedByteArray` | Receiver auto-detects via `typeof()` | No action needed — both paths are tolerated |
| Out-of-order frames | Receiver drops any frame with `frame_seq < last_frame_seq`, logs to `dropped_frames` | Surface `receiver.diagnostics()["dropped_frames"]` in QA HUD |
| Multi-avatar lobby | Two `IVXLiveKitVisemeReceiver` instances with different `publisher_identity_filter` | Same pattern as JS; the Phase-5 envelope already carries `publisher_identity` |
| Godot session expires mid-match | `nakama_session.expired` becomes true | Refresh with `nakama_client.session_refresh_async(...)` then re-mint |

### 4.3 Must-haves the game team owns

* **LiveKit GDExtension** — Godot does not ship a native binding.
  Two options:
  * **Recommended:** community `livekit-client-godot` GDExtension (C++
    binding around `livekit-rtc`) — exposes `Room.connect(url, token)`
    and a `data_received(payload, topic)` signal. Compiles cleanly on
    Linux / macOS / Windows / Android / iOS.
  * **Fallback:** route audio through the in-engine WebRTC
    (`WebRTCPeerConnection`) — works for 1:1 voice but not for
    multi-publisher / data-channel viseme. Don't pick this for Phase 4+.
* **Avatar replicator** — same situation as UE5: proto is ready, but
  there is no GDScript replicator that publishes `IVX_XR_OP.HEAD_POSE`
  frames. ~120 lines of GDScript.

### 4.4 Build / packaging notes

* Both new files (`ivx_voice_token_client.gd`,
  `ivx_livekit_viseme_receiver.gd`) are pure GDScript — no GDExtension,
  no engine modules, no native compile.
* `class_name` registration shows them in the Godot 4 dropdown so
  designers can wire them as resources in the editor.

---

## §5. visionOS (Swift / RealityKit) — already first-class

`SDKs/visionos/Sources/IVXMultiplayer/Voice/IVXLiveKitVoiceProvider.swift`
+ `Avatar/IVXAvatarReplicator.swift` already ship full parity.

### 5.1 Voice token mint (existing pattern)

```swift
let resp = try await client.rpc(session: session, id: "mp_voice_token",
    payload: """{"match_id":"\(matchId)","can_publish":true,"can_subscribe":true,"spatial":true}""")
let token = try JSONDecoder().decode(IVXVoiceSessionToken.self, from: resp.payload!.data(using: .utf8)!)
try await voice.connect(token: token)
```

We have not added a typed `IVXVoiceTokenClient.swift` yet because the
visionOS `URLSession` + `JSONDecoder` pattern above is idiomatic and
non-controversial. **Followup task** if we want true parity.

### 5.2 Anchors

`ARKit Collaborative Session` is wired through
`IVXSpatialFrame.swift`. visionOS has had this since day one.

### 5.3 Edge cases visionOS has that nobody else does

* **SharePlay required for shared anchors** — devices NOT on a
  SharePlay session use ARKit Collab via Bonjour mesh; visionOS
  silently transitions when SharePlay starts.
* **GroupActivity lifecycle** — the Swift adapter listens to
  `GroupSession` end and calls `voice.disconnect()` automatically.
  Don't disconnect twice.

---

## §6. Second-tier engines (Flutter / Java / native C++ / Cocos / Defold / Web3 / Roblox)

These adapters all have **multiplayer kernel parity** but **don't** ship
native LiveKit bindings. The voice path is "BYO LiveKit native lib + use
the kernel RPC for token minting".

### 6.1 The portable mint pattern (any language with a Nakama client)

```text
POST  rpc/mp_voice_token
body  {"match_id":"<id>","can_publish":true,"can_subscribe":true,"spatial":false}

response (LiveKit-mode kernel)
{
  "provider":     1,                   // EIVXVoiceProvider.LiveKit
  "token":        "<JWT>",
  "url":          "wss://livekit-sfu.aicart.svc.cluster.local",
  "room_id":      "ivx-<matchId>",
  "identity":     "<userId>",
  "expires_at_ms": 1735689600000,
  "can_publish":   true,
  "can_subscribe": true,
  "spatial":       false,
  "region":        "us-west-2"
}
```

### 6.2 Per-engine status

| Engine | Multiplayer | Mint helper | LiveKit binding | Recommendation |
| --- | --- | --- | --- | --- |
| **Flutter / Dart** | ✅ `ivx_multiplayer_kernel.dart` | ⚠ raw `client.rpc(...)` (5 lines) | Use [`livekit_client`](https://pub.dev/packages/livekit_client) (official) | Ship `IVXVoiceTokenClient.dart` next sprint — same code shape as TS |
| **Java / Android** | ✅ `IVXMultiplayerKernel.java` | ⚠ raw RPC | Use `io.livekit:livekit-android` | Ship `IVXVoiceTokenClient.java` next sprint |
| **Native C++** | ✅ `ivx_multiplayer_kernel.h` | ⚠ raw RPC via `nakama-cpp` | Use `livekit-rtc` (Rust) via FFI | Higher-effort; defer until a real C++ game ships |
| **Cocos2d-x** | ✅ re-exports C++ | ⚠ inherits C++ | Same path as native C++ | Defer with C++ |
| **Defold (Lua)** | ✅ `multiplayer_kernel.lua` | ⚠ raw RPC | No native LiveKit binding for Defold today | Defold games on the kernel run **text/turn-based** modes; voice is out-of-scope until a Defold-native LiveKit extension exists |
| **Web3 (TS)** | ✅ decorates JS adapter | ✅ via JS `mintVoiceToken` | ✅ via JS provider | **No additional work** — it's the JS path with an extra signer |
| **Roblox** | ⚠ RPC-only bridge | n/a — Roblox Voice Chat uses Roblox's native mic; the kernel isn't asked to mint a LiveKit token | Use Roblox Voice Chat or a server-relay model | LiveKit + viseme on Roblox is **not on the roadmap** |

### 6.3 Flutter cookbook (no helper yet, but here's the working code)

```dart
final resp = await client.rpc(
  session: session,
  id: "mp_voice_token",
  payload: jsonEncode({
    "match_id":      matchId,
    "can_publish":   true,
    "can_subscribe": true,
    "spatial":       false,
  }),
);
final token = jsonDecode(resp.payload!) as Map<String, dynamic>;
final room = Room();
await room.connect(token['url'], token['token']);
room.events.on<DataReceivedEvent>((e) {
  if (e.topic != 'viseme.v1') return;
  // decode same JSON envelope as the JS receiver
});
```

### 6.4 Java cookbook

```java
String payload = "{\"match_id\":\"" + matchId + "\",\"can_publish\":true,\"can_subscribe\":true,\"spatial\":false}";
Rpc resp = client.rpc(session, "mp_voice_token", payload).get();
JSONObject token = new JSONObject(resp.getPayload());
Room room = new Room();
room.connect(token.getString("url"), token.getString("token"));
```

(Same envelope as JS / UE5 / Godot — that's the point of the kernel.)

---

## §7. Common edge cases (every engine)

| Scenario | Why it happens | Engine-agnostic fix |
| --- | --- | --- |
| `voice_unconfigured` | LiveKit env vars not set on Nakama, OR `IVX_LIVEKIT_*` feature flag is off, OR the user is on a `low` cohort | Disable voice UI; the kernel will steer AI to text-only |
| Token expires mid-match | Default lifetime 1 h; long sessions exceed it | Catch `voice-unavailable` (or your binding's equivalent), re-mint, reconnect |
| Two avatars publish viseme — frames mix | Both publishers use topic `viseme.v1` | Use the new `publisher_identity` envelope field (Phase-5) and per-avatar receiver filters |
| Cohort downgrade (live A/B test moves user to `low`) | Satori experiment flips `mp.kernel.livekit.cohort` | The kernel responds with `provider=None` on the next mint — handle gracefully |
| Rejoin after disconnect | Nakama session reconnects but LiveKit room is gone | After `transport-state-changed → Connected`, mint a new token and re-connect LiveKit |
| Vision flag (Phase-5) flipped on for one persona only | Per-persona feature flag from the layered taxonomy | Server-side change; clients only see better contextual responses |
| Multi-language (CN/JP) viseme drift | Agent worker uses ARKit-52 names; some TTS profiles emit OVR-60 | Receiver tolerates extra indices; map any unknown index to nearest ARKit-52 (`blendshape-map.ts` provides the table) |

---

## §8. Must-haves the game team still owns (cross-engine)

These are **not in the SDK** for any engine and would be ~ 100-300 lines
of game code regardless of language. They consume the contracts the
SDK ships:

1. **Per-engine `IVXAvatarReplicator`** publishing
   `IVX_XR_OP.HEAD_POSE`/`LEFT_HAND_POSE`/`RIGHT_HAND_POSE` from the
   active XR session at 60 Hz / 30 Hz.
2. **Peer-avatar renderer** — decode the same opcodes and write the
   transform onto a peer avatar `Object3D`/`AActor`/`Node3D`/etc.
3. **AI-avatar prefab** — a single skeletal mesh + the engine's
   `IVXLiveKitVisemeStream`/`Receiver` wired to morph targets.
4. **Anchor UX** — "Look at the QR" / "Walk around the room" / "Share
   spatial frame" — the SDK ships providers; the game ships the UX.

The Unity reference implementation for #1-#4 is documented in
`docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md` §6-§9 and ports
1-to-1 to other engines.

---

## §9. Vision-enabled AI avatar — what every engine inherits for free

Phase-5 is **server-side only** (extends `livekit-agent-worker`):

* The agent worker subscribes to participant *video* tracks (camera
  publish from the human side).
* Frame-samples them through a VLM (Qwen3-VL or SmolVLM2) at 1/3 fps
  idle / 1 fps speaking.
* Feeds the VLM's output text into the *existing* TTS →
  `BlendshapeDriver` → `viseme.v1` pipeline.

Therefore: **on the client side, no engine needs to change**. The same
`UIVXLiveKitVisemeStream` / `IVXLiveKitVisemeReceiver` you wired for
Phase-4 receives Phase-5 packets indistinguishably.

What the game *can* opt into per engine:

* Enable camera publish in the LiveKit binding (`Room.localParticipant.setCameraEnabled(true)`).
* Show a "I can see you" indicator when the agent's `metadata.vision_active=true`.
* Toggle vision off via the layered feature flag (`IVX_LIVEKIT_VISION_ENABLED=false` per room) — single command rollback.

---

## §10. Verification dry-run checklist (any engine)

Run this after wiring up your engine's LiveKit binding:

| Step | Expected outcome |
| --- | --- |
| 1. Bootstrap Nakama (auth → session) | `session.expired == false` |
| 2. `mintVoiceToken({ matchId, spatial:false })` | Returns `{ token, url, ... }` (no error) |
| 3. LiveKit `Room.connect(url, token)` | `Room.state == Connected` within 2 s |
| 4. Subscribe to `dataReceived` with the receiver | Receives `header → frame×N → footer` cycle when AI host is in match |
| 5. Watch `receiver.diagnostics().dropped_frames` over 60 s | Should stay 0 with `livekit_viseme_bandwidth.yaml` bot harness in mp.kernel canary |
| 6. Toggle `IVX_LIVEKIT_AVATAR_ENABLED=false` (operator runbook) | New mints return `provider=None`; existing rooms stay connected (graceful drain) |
| 7. Mid-session, kill agent worker pod (`kubectl delete pod -n aicart …`) | Within 30 s, KEDA respawns; LiveKit clients see a `participant_left` then `participant_joined` for `ai-host-1` |

If any step fails, follow the rollback runbook in
`Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md` §6.

---

## §11. Phase-6 backlog (cross-engine parity follow-ups)

The work that closes the *remaining* gaps but is **not** required for
Phase-4 / Phase-5 production:

| ID | Engine | Item | Effort |
| --- | --- | --- | --- |
| P6-UE5-A | Unreal | `UIVXAvatarReplicator` (head/hands/blendshapes pose pump) | 2 d |
| P6-UE5-B | Unreal | `UIVXARFoundationEquivAnchorProvider` over `UARSession` | 3 d |
| P6-JS-A  | JS / Three.js | `IVXAvatarReplicator` (Three.js / Babylon variants) | 2 d |
| P6-JS-B  | JS | `webxr/anchor-provider.ts` over WebXR `XRAnchor` API | 2 d |
| P6-Godot-A | Godot | `IVXAvatarReplicator` GDScript pump | 2 d |
| P6-Godot-B | Godot | LiveKit GDExtension build pipeline (CI for Linux/macOS/Win/Android/iOS) | 5 d |
| P6-Swift-A | visionOS | `IVXVoiceTokenClient.swift` typed helper (parity polish) | 0.5 d |
| P6-Flutter | Flutter | `IVXVoiceTokenClient.dart` + `IVXLiveKitVisemeReceiver.dart` | 1 d |
| P6-Java    | Java/Android | `IVXVoiceTokenClient.java` + receiver | 1 d |
| P6-CPP     | Native C++ | `livekit-rtc` FFI binding + `IVXVoiceTokenClient.{h,cpp}` | 5 d |

None of these block the production-validation runbook for Phases 1-5;
they are pure DX polish for engines we don't ship a flagship game on
yet.

---

## §12. What to read next

* For Unity-specific deep dives: `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md`
* For the multi-avatar lobby + vision flag taxonomy: `Intelliverse-X-AI/docs/livekit/PHASE_5_VISION_RESEARCH.md` §12-§13
* For the deploy / rollback runbook (operator-owned): `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md`
* For per-phase sign-off + bot harness gates: `Intelliverse-X-AI/docs/livekit/phase-{1,2,3,4}-signoff.md`

---

*Last updated: 2026-04-26 — by the LiveKit-migration agent after
shipping the JS / UE5 / Godot voice-token + viseme parity helpers.*
