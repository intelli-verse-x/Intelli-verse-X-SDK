# Avatar Replication — Integration Guide (All Engines · All Platforms)

**Audience:** anyone shipping an IntelliVerseX game with **shared 3D worlds** (multiple humans + AI avatars in the same room) on **any engine** (Unity, Unreal, JS / Three.js / Babylon, Godot, visionOS native Swift, Roblox, Flutter, native C++, Java/Android, Cocos2d-x, Defold, Web3) on **any platform** (Vision Pro, Quest, PSVR2, Pico, iOS, Android, WebXR-capable browser, PCVR, plain handheld).

**Read time:** ~10 minutes.
**Time to first running build:** ~30 minutes (Unity/Unreal), ~15 minutes (JS / Godot).

If you only need a **5-minute Unity prefab recipe**, read
[`Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/README.md`](../../Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/README.md) instead.

---

## Table of contents

1. [What this gives you](#1-what-this-gives-you)
2. [End-state architecture](#2-end-state-architecture)
3. [The wire (engine-agnostic)](#3-the-wire-engine-agnostic)
4. [Server-side prerequisites (already deployed)](#4-server-side-prerequisites-already-deployed)
5. [Engine-by-engine integration](#5-engine-by-engine-integration)
   - 5.1 Unity (any platform)
   - 5.2 Unreal Engine 5 (PCVR, Quest, PSVR2)
   - 5.3 JavaScript / TypeScript (Three.js, Babylon.js, A-Frame)
   - 5.4 Godot 4 (PCVR, mobile)
   - 5.5 visionOS native Swift / RealityKit
   - 5.6 Mobile-only / 2D-handheld engines (Cocos / Defold / Flutter / Java / native C++)
   - 5.7 Roblox / Web3
6. [Per-platform input + anchor playbook](#6-per-platform-input--anchor-playbook)
7. [Combining with voice + lip-sync (LiveKit)](#7-combining-with-voice--lip-sync-livekit)
8. [AI avatars in the same room — Phase-5 vision](#8-ai-avatars-in-the-same-room--phase-5-vision)
9. [Bandwidth budget + tuning](#9-bandwidth-budget--tuning)
10. [QA & bot harness](#10-qa--bot-harness)
11. [Troubleshooting](#11-troubleshooting)
12. [Reference index](#12-reference-index)

---

## 1. What this gives you

A single feature flag — *"avatar-replication-v1"* on a Nakama match — turns a regular multiplayer lobby into a **shared 3D world**:

* Every joining player publishes head + hand poses + face/finger expressions on a fast-path WebRTC SFU channel.
* Every other player receives those poses and drives a humanoid mesh via the `IIVXAvatar` (Unity / Unreal) or `IVXAvatar` (Swift / GDScript / TS) adapter.
* AI avatars from the LiveKit Agents worker join the same room, speak via TTS, lip-sync via `viseme.v1`, and (Phase 5) **see** any participant's video track to ground their replies.
* Voice is spatialised by LiveKit; anchors are co-aligned via the active `IIVXAnchorProvider`.

**Cross-device interop is built in.** A Vision Pro user, a Quest user, an iPhone user, and a Pixel user can all be in the same room — see [§6 cross-device matrix](#6-per-platform-input--anchor-playbook).

---

## 2. End-state architecture

```
┌───────────────────────────────────────────────────────────────┐
│                     Nakama match (any template)               │
│  - mp_create_match / mp_join_match / mp_voice_token RPCs       │
│  - Goja kernel templates (sync_turn, persistent_party, …)      │
│                                                                │
│  Side-channel: avatar_replication match (XR_POSE 0xF000+)      │
│  - Quantized head/hand/face/finger frames                      │
│  - LOD demotions, AOI culling, AVATAR_FALLBACK                 │
└───────────────┬───────────────────────────────────────────────┘
                │ WebSocket (kernel) + WebRTC SFU (LiveKit voice)
                ▼
┌───────────────────────────────────────────────────────────────┐
│ Client engines (any of these — all speak the same wire)        │
│                                                                │
│  Unity ── IVXAvatarReplicator.cs ── IIVXAvatar adapter         │
│  UE5   ── UIVXAvatarReplicator    ── IIVXAvatar (BP)          │
│  JS    ── IVXWebXRAdapter         ── peer-pose events          │
│  Godot ── IVXAvatarReplicator.gd  ── peer-pose signals         │
│  Swift ── IVXAvatarReplicator     ── RealityKit Entity         │
│                                                                │
│  All engines share: voice (LiveKit), lip-sync (viseme.v1),    │
│  anchors (IIVXAnchorProvider), AI host (Agents worker)         │
└───────────────────────────────────────────────────────────────┘
```

**Three independent channels per match:**

| Channel | Transport | Purpose | What flows on it |
| --- | --- | --- | --- |
| Kernel ops | WebSocket | game logic | turn-based opcodes, lobby state |
| Avatar replication | WebSocket fast-path | pose replication | `HEAD_POSE`/`LEFT_HAND_POSE`/`RIGHT_HAND_POSE`/`BLENDSHAPES`/`FINGER_CURLS`/`AVATAR_DESCRIPTOR`/`LOD_HINT`/`PEER_LEFT`/`AVATAR_FALLBACK` |
| LiveKit room | WebRTC SFU | voice + AI media | spatial audio, AI TTS audio, `viseme.v1` data channel, optional video for vision |

The channels are tied together by a stable `match_id`; the LiveKit room is named `ivx-match-${match_id}`.

---

## 3. The wire (engine-agnostic)

### Opcodes (`XR_POSE` range `0xF000`–`0xF008`)

| Code | Name | Direction | Payload |
|---|---|---|---|
| `0xF000` | `HEAD_POSE` | bidir | `{ pose: PoseQuantized }` (server stamps `user_id`) |
| `0xF001` | `LEFT_HAND_POSE` | bidir | `{ pose, grip_pct?, trigger_pct? }` |
| `0xF002` | `RIGHT_HAND_POSE` | bidir | `{ pose, grip_pct?, trigger_pct? }` |
| `0xF003` | `BLENDSHAPES` | bidir | `{ blendshapes: bytes, quant_profile }` (52 ARKit bytes) |
| `0xF004` | `FINGER_CURLS` | bidir | `{ is_left, finger_curls: bytes }` (15 bytes per hand) |
| `0xF005` | `AVATAR_DESCRIPTOR` | bidir | `IVXAvatarDescriptor` (`avatar_v1.proto`) |
| `0xF006` | `LOD_HINT` | server→client | `{ user_id, lod, reason }` |
| `0xF007` | `PEER_LEFT` | server→client | `{ user_id, reason }` |
| `0xF008` | `AVATAR_FALLBACK` | server→client | `{ user_id, reason }` |

### Quantization

`PoseQuantized` (defined in `avatar_replication.proto`):

```
px_mm, py_mm, pz_mm   : int32  (millimetres from anchor; ±32 km clamp range)
rot_packed            : uint32 (smallest-three quaternion: 2-bit drop index + 3×9-bit components, scaled by √2)
quant_profile         : uint32 (1 = default)
ts_ms                 : int64
confidence_pct        : uint32 (0..100, used for tracking-loss fallback)
```

Every engine ships an identical pack/unpack routine (`QuantizePose` / `DequantizePose` in Unity, `quantizePose` in JS, `IVXPoseQuantized.quantize` in Swift). They are **bit-for-bit compatible**.

---

## 4. Server-side prerequisites (already deployed)

If your team already shipped Phases 1–4 of the LiveKit migration, these are already true:

* Nakama runtime has the **`avatar-replication-v1`** Goja template registered (`data/modules/avatar_replication/`, compiled into `data/modules/build/index.js`).
* LiveKit SFU is running and `mp_voice_token` RPC mints valid join tokens.
* LiveKit Agents worker is running and joins matches as `agent-${persona_id}`.
* Feature flags:
  - `IVX_LIVEKIT_MULTIPLAYER_VOICE=true`
  - `IVX_LIVEKIT_MULTI_HUMAN_AI=true`
  - `IVX_LIVEKIT_AVATAR_ENABLED=true`
  - *(Phase 5)* `IVX_LIVEKIT_VISION_ENABLED=true` to give AI avatars vision

If your stack is fresh, see:
* `intelli-verse-kube-infra/nakama/multiplayer/README.md` — Nakama deployment.
* `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md` — phased flag-flip runbook.
* `docs/multiplayer/qa-gates.md` — per-phase bot-harness gates.

**You do NOT need to write or deploy any new server code to use this guide.**

---

## 5. Engine-by-engine integration

Every section below assumes you already authenticate the player via Nakama, get an `IIVXMatchSession`, and have your engine's voice/anchor providers wired (those are documented in `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md` and `docs/multiplayer/CROSS_ENGINE_3D_WORLD_E2E_ANALYSIS.md`).

### 5.1 Unity (any platform)

**Status:** ✅ first-class, prefab-ready.
**Files:** `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/IVXAvatarReplicator.cs`.

```csharp
// 1. After joining the match:
var session = await mp.JoinMatchAsync(matchId);

// 2. Drop IVXAvatarReplicator on a GameObject and wire the Inspector:
//    _localHead       → XR camera transform
//    _localLeftHand   → left controller transform
//    _localRightHand  → right controller transform
//    _anchorRoot      → your anchor offer's transform (optional but recommended)

// 3. Provide an IIVXAvatar factory and attach.
var rep = playersRoot.GetComponent<IVXAvatarReplicator>();
rep.SetAvatarFactory(desc => MyAvatarSpawner.SpawnFor(desc)); // returns IIVXAvatar
rep.Attach(session);

// 4. (Optional) hand-published face/finger frames from your tracker:
faceTracker.OnArkit52 += weights => rep.PublishBlendshapes(weights, IVXBlendshapeProfile.Arkit52);
handTracker.OnLeftCurls += curls => rep.PublishFingerCurls(isLeft: true, curls);
```

**Events-only mode** (no `IIVXAvatar` adapter) — listen to `OnPeerPose`, `OnPeerBlendshapes`, `OnPeerFinger`, `OnPeerLeft`, `OnPeerFallback`. The replicator dequantizes for you (`evt.Position` / `evt.Rotation`).

### 5.2 Unreal Engine 5 (PCVR, Quest, PSVR2)

**Status:** 🟡 wire ready, prefab pending Phase-6 (~2 days). The proto types are generated, the `IIVXAvatar` BP interface is defined, and the same opcodes work.

```cpp
// What you write today (until UIVXAvatarReplicator ships):
//   1. Subscribe to opcodes 0xF000–0xF008 via UIVXMatchSession::Subscribe.
//   2. On HEAD_POSE / hand pose, decode PoseQuantized (use the generated
//      smallest-three helper) and write to your USkeletalMeshComponent.
//   3. Tick a 30-Hz publish loop sampling the OpenXR HMD pose.
//
// Reference: SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift
//            (Swift port, ~95 LOC, semantically identical).
//
// Voice + lip-sync are already first-class:
UIVXLiveKitVoiceProvider* Voice = NewObject<UIVXLiveKitVoiceProvider>(this);
UIVXVoiceTokenClient::MintAsync(Req, OnSuccess, OnFailure);
UIVXLiveKitVisemeStream* VS = NewObject<UIVXLiveKitVisemeStream>(this);
VS->TargetMesh = AvatarSkeletalMesh;
```

The Phase-6 backlog item `P6-UE5-A` (in `CROSS_ENGINE_3D_WORLD_E2E_ANALYSIS.md`) is to ship `UIVXAvatarReplicator` as a UCLASS that mirrors `IVXAvatarReplicator.cs`.

### 5.3 JavaScript / TypeScript (Three.js, Babylon.js, A-Frame)

**Status:** ✅ pose adapter shipped; renderer is one screen of game code.
**Files:** `SDKs/javascript/packages/multiplayer/src/webxr/adapter.ts`.

```typescript
import { IVXWebXRAdapter, IVX_XR_OP } from "@intelliversex/multiplayer";

const session = await client.joinMatch(matchId);
const xr = new IVXWebXRAdapter(session, { provider: "three", headHz: 60, handHz: 30 });

xrSession.requestReferenceSpace("local-floor").then(refSpace => {
    xr.attach(xrSession, refSpace);
});

// Render peers — drop into your Three.js loop:
const peerObj: Record<string, THREE.Object3D> = {};
xr.onPeerPose(evt => {
    const o = peerObj[evt.user_id] ??= spawnAvatar(evt.user_id);   // your factory
    const bone = bonesOf(o)[evt.bone];                             // head / leftHand / rightHand
    bone.position.set(evt.pose.px_mm/1000, evt.pose.py_mm/1000, evt.pose.pz_mm/1000);
    bone.quaternion.copy(unpackSmallestThree(evt.pose.rot_packed)); // helper exported from adapter
});
xr.onPeerLeft(evt => { peerObj[evt.user_id]?.removeFromParent(); delete peerObj[evt.user_id]; });

// Voice + lip-sync:
import { mintVoiceToken } from "@intelliversex/multiplayer/voice";
const tok = await mintVoiceToken({ client, session, matchId });
const room = new Room(); await room.connect(tok.url, tok.token);

import { IVXLiveKitVisemeReceiver } from "@intelliversex/multiplayer/avatar";
const viseme = new IVXLiveKitVisemeReceiver();
viseme.attachLiveKitRoom(room);
viseme.onFrame(weights => myMorphRig.apply(weights));
```

**Babylon / A-Frame:** identical pattern — only the bone transform write changes.

### 5.4 Godot 4 (PCVR, mobile)

**Status:** 🟡 voice + viseme decoder shipped; replicator is on the Phase-6 backlog (`P6-Godot-A`, ~2 days).
**Files:** `SDKs/godot/addons/intelliversex/multiplayer/`.

```gdscript
# What's shipped:
var token := await IVXVoiceTokenClient.new().mint_async({
    "client": client, "session": session, "match_id": match_id, "spatial": true
})
var receiver := IVXLiveKitVisemeReceiver.new()
receiver.frame_received.connect(func(frame): my_avatar.apply_blendshapes(frame.weights))
# Wire your LiveKit GDExtension's data channel:
livekit_room.data_received.connect(receiver.on_livekit_data)

# What you write until IVXAvatarReplicator.gd ships:
#   - Subscribe to opcodes 0xF000–0xF008 on IVXMatchSession.
#   - Decode PoseQuantized.rot_packed (smallest-three helper in
#     SDKs/javascript/.../webxr/adapter.ts; ~30 LOC of GDScript).
#   - Apply to Skeleton3D bones.
```

### 5.5 visionOS native Swift / RealityKit

**Status:** ✅ first-class.
**Files:** `SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift`.

```swift
let session = try await mp.joinMatch(matchId)

let replicator = IVXAvatarReplicator(session: session, root: realityKitRoot)
replicator.publishHz = 60
replicator.attach { () -> (SIMD3<Float>, simd_quatf) in
    // Sample the active ARKit world origin
    let t = arSession.worldOrigin.transform
    return (t.translation, t.rotation)
}

// Voice + lip-sync via IVXLiveKitVoiceProvider (already first-class).
```

The Swift port uses RealityKit `Entity` directly; if you build on Unity-for-visionOS instead, use the Unity replicator (§5.1).

### 5.6 Mobile-only / 2D-handheld engines (Cocos / Defold / Flutter / Java / native C++)

**Status:** ➖ N/A for full 3D avatars — these engines are typically used for 2D / casual handheld games where players don't need a 6DoF avatar.

**What still works on these engines:**

* **Voice via LiveKit** — every engine has a native LiveKit binding (see `SDKs/MULTIPLAYER_KERNEL_ADAPTERS.md`).
* **Lip-sync (`viseme.v1`)** — pure JSON decoder is portable; we ship a Go-/JS-/GDScript-/C++-style decoder pattern in the adapter docs.
* **Cross-device participation as a "voice-only" peer** — the player in your Cocos / Defold / Flutter game can be in the same Vision Pro / Quest room and **be heard**, even if their avatar shows up to others as just a head billboard with a name tag.

**What needs explicit opt-in if you do want full avatars on mobile-2D engines:** ship a thin "head + speaking" avatar (no hands) using the events-only mode of your engine's binding. For Flutter/Java/native C++ this is roughly 80 LOC against the kernel `Subscribe<HEAD_POSE>` API.

### 5.7 Roblox / Web3

**Status:** ➖ Roblox — voice integration only (Roblox runs in its own sandbox; pose replication uses Roblox's own networking). Web3 — server-driven only (no XR participants).

* **Roblox** → use IntelliVerseX SDK for AI NPCs + Hiro live-ops + cross-game identity, but stay on Roblox VoiceChat for voice and Roblox replication for poses. See `SDKs/roblox/`.
* **Web3** → server-driven assets + on-chain identity. Avatars live on whatever engine the user's client is running.

---

## 6. Per-platform input + anchor playbook

| Platform | Local pose source | Anchor provider | Notes |
| --- | --- | --- | --- |
| **Vision Pro** (Unity) | XR Camera = head; no controllers; hand-tracking via `XRHandSubsystem` | `IVXAnchorFallback.BuildPcvrFakeOffer` for solo, OR Swift native `IVXSharePlayAnchorProvider` for SharePlay co-play | Persona-driven blendshapes from ARKit if you ship in full-immersive mode. PolySpatial bounded-volume is Phase-6. |
| **Vision Pro** (native Swift) | `arSession.worldOrigin` | Swift `IVXSharePlayAnchorProvider` | First-class today; see `SDKs/visionos/`. |
| **Quest 2/3/Pro/3S** | XR Camera + `OVRControllers` or `OVRHand` | `IVXMetaSpatialAnchorProvider` (Meta Cloud) | Quest 3/Pro: face tracking → blendshapes via `OVRFaceExpressions`. |
| **Pico Neo / Pico 4** | OpenXR camera + controllers | `IVXOpenXrMsftSpatialAnchorProvider` | Same code path as PCVR-OpenXR. |
| **PSVR2** (UE5) | OpenXR camera + Sense controllers | `IVXOpenXrMsftSpatialAnchorProvider` | Eye tracking → optional blendshapes. |
| **iOS — AR mode** | `ARFoundation.ARCamera` | `IVXARKitCollabAnchorProvider` (same physical room) OR `IVXARFoundationAnchorProvider` | No hands; head + voice + face blendshapes from ARFace. |
| **iOS — handheld 2D** | Screen-controlled virtual camera | `IVXAnchorFallback.BuildPcvrFakeOffer` | Voice + head only; renders other peers in 3D. |
| **Android — AR mode** | `ARFoundation.ARCamera` | `IVXARFoundationAnchorProvider` (Geospatial opt-in) | Same as iOS-AR; ARCore Geospatial gives global co-presence. |
| **Android — handheld 2D** | Screen-controlled virtual camera | fake offer | Voice + head only. |
| **WebXR browser** | `XRSession` viewer pose + input sources | WebXR `XRAnchor` (when supported) or fake | Three.js / Babylon / A-Frame all work. |
| **PCVR (SteamVR / Oculus PC)** | OpenXR camera + controllers | `IVXOpenXrMsftSpatialAnchorProvider` or fake | Highest fidelity; 90 Hz pose publishing recommended. |

**Same-physical-room vs different-rooms** — the wire is identical; only the anchor strategy differs:

* Same room → use a *cloud* anchor (Meta / ARKit Collab / ARCore Geospatial). Every peer resolves the same world coordinates.
* Different rooms → use a *fake* anchor. Every peer pretends a synthetic origin is the room.

---

## 7. Combining with voice + lip-sync (LiveKit)

The avatar replicator does **not** carry voice. Voice flows on LiveKit. Lip-sync flows on a LiveKit data channel (`viseme.v1`). The replicator's pose channel and LiveKit channel are independent — you need both for a full presence experience.

**Unity wiring (one prefab):**

```csharp
GameObject avatarRoot;                           // your humanoid prefab
avatarRoot.AddComponent<IVXAvatarReplicator>(); // pose I/O
avatarRoot.AddComponent<IVXLiveKitVoiceProvider>(); // voice mic + speakers
avatarRoot.AddComponent<IVXLiveKitVisemeStream>();  // ARKit-52 morph driver
avatarRoot.AddComponent<IVXLiveKitVisemeBinder>();  // auto-wires Room.DataReceived → viseme stream
```

That single GameObject is now a full **presence avatar** — pose, voice, and lip-sync all on the right channels.

---

## 8. AI avatars in the same room — Phase-5 vision

When `IVX_LIVEKIT_VISION_ENABLED=true` is flipped server-side:

1. The LiveKit Agents worker subscribes to participants' video tracks.
2. The configured VLM (Qwen3-VL or whatever you wired in `Intelliverse-X-AI/docs/livekit/PHASE_5_VISION_RESEARCH.md`) grounds AI replies in what it sees.
3. The AI replies flow through the **same** TTS → BlendshapeDriver → `viseme.v1` chain that already drives lip-sync.
4. **Zero client code changes.** Your Unity / Unreal / JS / Swift app already has everything it needs.

To opt a peer **into** vision (i.e. the AI sees what their device sees), publish their device camera as a LiveKit video track:

* iOS / Android — ARFoundation passthrough → `LocalVideoTrack`.
* Quest — Meta XR Passthrough Camera API → `LocalVideoTrack`.
* Vision Pro — `ARKitSession.CameraSource` → `LocalVideoTrack`.
* WebXR — getUserMedia → `LocalVideoTrack`.

Use a feature flag on your client UI ("Let the AI see what you see"); the SDK won't publish video unless you opt in.

---

## 9. Bandwidth budget + tuning

Per-peer publish rate at default settings (Unity replicator, Vision Pro / Quest):

| Channel | Hz | Bytes/frame | KB/s/peer (up + down) |
| --- | --- | --- | --- |
| Head | 30 | ~24 | 0.7 ↑ + (N-1) × 0.7 ↓ |
| Each hand | 30 | ~24 | 0.7 + (N-1) × 0.7 |
| Blendshapes | 30 | ~60 | 1.8 + (N-1) × 1.8 |
| Finger curls | 30 (each hand) | ~22 | 0.7 + (N-1) × 0.7 |
| **Total** for an 8-peer party | — | — | ≈ **45 KB/s up + down** with face + fingers, ≈ **8 KB/s** without. |

The replicator's idle suppression cuts real-world averages by 50–70% — players who hold still mostly send heartbeats.

**Cellular mobile recommendation:** drop hand Hz to 20, keep head at 30, face/fingers off unless you have a tracker. Net ~3 KB/s per peer.

**90 Hz PCVR:** raise head/hand to 60–72; face/fingers stay at 30. Net ~12 KB/s per peer with face on.

The server's AOI culling (`enable_aoi=true`, `aoi_radius_mm` in `AvatarReplicationParams`) automatically drops poses for peers outside your area-of-interest. Bandwidth in a 32-peer instance scales by sqrt(peers) rather than peers².

---

## 10. QA & bot harness

`tools/qa/multiplayer-bot-harness/scripts/avatar_party_8.yaml` — 8-bot party that joins a match, publishes synthetic poses + viseme frames at production rates, and asserts no `WARN_AVATAR_FALLBACK` / `LOW_BANDWIDTH` warnings.

`tools/qa/multiplayer-bot-harness/scripts/livekit_viseme_bandwidth.yaml` — viseme-channel bandwidth ceiling test.

Both are wired into the per-phase deployment runbook in `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md`. **Re-run them after any client change** that touches publish rates, idle suppression, or quantization.

Manual signoff: load `Intelli-verse-X-SDK/docs/multiplayer/qa-gates.md` §6 ("Avatar pose replication") for a 12-step regression checklist.

---

## 11. Troubleshooting

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| Remote peer's avatar is at the world origin and spins | You forgot `_anchorRoot` and one peer is in anchor-local while the other is in world space | Set `_anchorRoot` on **all** peers in the same match. |
| Remote peer's hands are at the floor | The peer is on a phone; they have no controller transforms | Render hands-less (idle-relaxed pose) for that peer. The factory sees the descriptor before any pose arrives. |
| Lip-sync is delayed by ~200 ms | Normal network jitter buffer | Lower `IVXLiveKitVisemeStream.JitterBufferMs` from 200 → 60. Trade some smoothness for latency. |
| AI avatar's mouth doesn't move | Mesh is missing ARKit-52 morph targets | Add 52 morph targets per ARKit blendshape names (or set a `BlendshapeNameMap` to your custom mesh's morph names). |
| `WARN_AVATAR_FALLBACK` in console | Server demoted the peer (bandwidth or load) | Adapter has already swapped to billboard; surface it in your UI as "Connection issues". |
| `ANCHOR_INCOMPAT` (50) error on join | Peers picked different anchor providers | Coordinate at lobby time: every peer must agree on an `AnchorOffer.kind` before publishing poses. |
| Phone peer's head pose is upside-down on Quest viewer | Different up-axes | The replicator quantizes in anchor-local space; if your anchors don't share an up-axis, fix the anchor offer. |

---

## 12. Reference index

**Schemas:**
* `Intelli-verse-X-SDK/schemas/multiplayer/templates/avatar_replication.proto` — wire schema.
* `Intelli-verse-X-SDK/schemas/avatar/avatar_v1.proto` — avatar descriptor + blendshape encoding.
* `Intelli-verse-X-SDK/schemas/multiplayer/opcodes.proto` — opcode ranges.

**Server templates:**
* `nakama/data/modules/avatar_replication/` — Goja kernel template (TypeScript source compiled into `nakama/data/modules/build/index.js`).
* `Intelliverse-X-AI/services/livekit-agent-worker/` — LiveKit Agents worker (TTS + viseme + Phase-5 vision).

**Per-engine SDK:**
* Unity — `Assets/Intelli-verse-X-SDK/MultiplayerKernel/`.
* Unreal — `SDKs/unreal/Source/IntelliVerseX/`.
* JS — `SDKs/javascript/packages/multiplayer/`.
* Godot — `SDKs/godot/addons/intelliversex/multiplayer/`.
* visionOS — `SDKs/visionos/Sources/IVXMultiplayer/`.
* Other engines — `SDKs/{cocos2dx,defold,flutter,java,cpp,web3,roblox}/`.

**Companion docs:**
* `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md` — Unity-specific dry run.
* `docs/multiplayer/CROSS_ENGINE_3D_WORLD_E2E_ANALYSIS.md` — non-Unity engines + edge cases.
* `docs/multiplayer/avatar-interop.md` — full IIVXAvatar interface spec.
* `docs/multiplayer/qa-gates.md` — per-phase QA gates.
* `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md` — phased deployment runbook.
* `Intelliverse-X-AI/docs/livekit/PHASE_5_VISION_RESEARCH.md` — vision-enabled AI avatars.

**This doc lives at:** `docs/multiplayer/AVATAR_REPLICATION_INTEGRATION_GUIDE.md`. Keep it updated when:

* You change opcode payloads (`avatar_replication.proto`).
* You ship a new engine SDK.
* You change the default Hz / epsilon / heartbeat in any replicator.
* You add a new platform-specific anchor provider.
