# IVXAvatarReplicator — Prefab Recipe

`IVXAvatarReplicator` is the Unity MonoBehaviour that drives **remote-human**
avatars in a shared 3D world. It rides the canonical avatar-replication wire
(`avatar_replication.proto`, opcodes `0xF000–0xF008`) and is bit-for-bit
compatible with the JS WebXR adapter (`SDKs/javascript/.../webxr/adapter.ts`),
the Go server template (`data/modules/avatar_replication/`), and the visionOS
Swift port (`SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift`).

**AI avatars do NOT need this component.** Their poses come from the LiveKit
Agents worker (TTS → blendshapes → `viseme.v1`). This component is the single
piece that unlocks live multi-human presence on Unity.

---

## TL;DR — 5-minute integration

1. Add a `GameObject` to your gameplay scene (e.g. `XRRig/PlayersRoot`).
2. Add the `IVXAvatarReplicator` component.
3. Drag references in:
   * **Local Head** → your XR camera transform (`MainCamera` under `XR Origin`).
   * **Local Left Hand / Right Hand** → controller transforms.
   * **Anchor Root** *(optional)* → a transform shared by all peers (the
     `AnchorOffer` you got from `IIVXAnchorProvider`). Leave null only if
     every peer shares the world origin (PCVR / single-room single-anchor
     setups).
4. From your match-bootstrap code:

```csharp
// After IIVXMultiplayer.JoinMatchAsync(...) succeeds:
var replicator = playersRoot.GetComponent<IVXAvatarReplicator>();
replicator.SetAvatarFactory(desc => MyAvatarSpawner.Spawn(desc)); // returns IIVXAvatar
replicator.Attach(matchSession);

// On match end (replicator also runs Detach() automatically on disable/destroy):
replicator.Detach();
```

That's it for humans. The replicator:
* Subscribes to `HEAD_POSE` / `LEFT_HAND_POSE` / `RIGHT_HAND_POSE` /
  `BLENDSHAPES` / `FINGER_CURLS` / `AVATAR_DESCRIPTOR` / `LOD_HINT` /
  `PEER_LEFT` / `AVATAR_FALLBACK`.
* Routes inbound poses through your `IIVXAvatar` adapter
  (`ApplyHeadPose`, `ApplyHandPose`, `ApplyBlendshapes`, `ApplyFingerCurls`,
  `SetLOD`, `FallbackToBillboard`).
* Publishes the local player's head + hand poses at 30 Hz (configurable).
* Idle-suppresses publishes when the local player is still, with a 1 Hz
  heartbeat so the server keeps the peer warm.
* Disposes per-peer adapters on `PEER_LEFT`.

---

## Two integration modes

### Mode 1 — IIVXAvatar factory (recommended)

Implement `IIVXAvatar` once for your project (skinned humanoid, RPM, VRM,
Meta Avatar SDK, Persona, …) and supply it via:

```csharp
replicator.SetAvatarFactory(descriptor => {
    var avatar = AvatarPool.Acquire(descriptor.UserId);
    return avatar.GetComponent<IIVXAvatar>(); // your adapter
});
```

The replicator instantiates one adapter per remote `user_id` on first pose,
calls `LoadAsync(descriptor)` if an `AVATAR_DESCRIPTOR` arrives, and disposes
on `PEER_LEFT`.

### Mode 2 — Events-only (no adapter)

If your project drives a custom puppet (Animation Rigging, Final IK,
hand-rolled bone transforms), skip the factory and listen to events:

```csharp
replicator.OnPeerPose += evt => {
    var go = peers.Resolve(evt.UserId);
    switch (evt.Bone) {
        case IVXAvatarBone.Head:      go.Head.SetPositionAndRotation(evt.Position, evt.Rotation); break;
        case IVXAvatarBone.LeftHand:  go.LeftHand.SetPositionAndRotation(evt.Position, evt.Rotation); break;
        case IVXAvatarBone.RightHand: go.RightHand.SetPositionAndRotation(evt.Position, evt.Rotation); break;
    }
};
replicator.OnPeerBlendshapes += evt => peers.Resolve(evt.UserId).BlendshapeRig.Apply(evt.Weights);
replicator.OnPeerLeft        += evt => peers.Despawn(evt.UserId);
```

`evt.Position` / `evt.Rotation` are already dequantized — you can write them
straight to a `Transform`.

---

## Wiring face + finger tracking

The Unity `Update()` loop only publishes head + hand transforms. For
blendshapes and finger curls (which usually come from a separate tracker —
ARKit `ARFaceManager`, Meta XR `OVRFaceExpressions`, Persona, …) call:

```csharp
// In your face tracker's per-frame callback:
replicator.PublishBlendshapes(arkit52WeightsAsByteArray, IVXBlendshapeProfile.Arkit52);

// In your hand tracker's per-frame callback:
replicator.PublishFingerCurls(isLeft: true,  leftFingerCurlsBytes);
replicator.PublishFingerCurls(isLeft: false, rightFingerCurlsBytes);
```

`Publish*` honours the same idle suppression / heartbeat pattern.

---

## Anchor frame discipline

Pose components are quantized in **anchor-local** space when `_anchorRoot` is
set. This is the right thing to do whenever your peers shared an anchor offer
(`IIVXAnchorProvider` returned an `AnchorOffer`):

```
IVXAnchorRoot (Transform)
    ├─ XR Origin (Camera, Controllers)
    ├─ IVXAvatarReplicator  (anchorRoot ← IVXAnchorRoot)
    └─ AvatarSpawnRoot
```

Without an anchor (PCVR fake offer, mobile 3rd-person), leave `_anchorRoot`
null and the replicator quantizes in world space. **Mixing the two on the
same match is a bug** — make sure every peer in a session is consistent.

---

## Tuning knobs

| Field | Default | When to change |
| --- | --- | --- |
| `_headHz` | 30 | Bump to 60–72 on Quest / PSVR2 if you have bandwidth headroom; drop to 20 on cellular mobile MR. |
| `_handHz` | 30 | Same logic. Hands are the dominant bandwidth consumer in finger-tracking builds — keep ≤ 30 on cellular. |
| `_posEpsilonM` | 0.001 (1 mm) | Increase to 0.005–0.01 for low-bandwidth tiers; decrease for surgery / training-grade fidelity. |
| `_rotEpsilonDeg` | 0.5 | Decrease to 0.1 for ultra-precise gestural games; increase to 1–2 for casual social MR. |
| `_heartbeatMs` | 1000 | Lower (e.g. 500 ms) when your server's idle-eviction is aggressive; raise (e.g. 2000 ms) to save bandwidth in social lobbies. |

The replicator never publishes faster than `_headHz` / `_handHz`, even when
the local pose is changing rapidly — those are hard caps.

---

## Diagnostics

Enable `_verboseLogging` in the Inspector to log Attach/Detach. The Unity
console + the LiveKit RTP layer should never see a "rate-limited" warning
under default Hz; if you do, you've forgotten to set `_anchorRoot` and your
poses are exceeding the ±32 m position clamp range.

`replicator.RemoteUserIds` exposes the live set of tracked remote peers —
useful in QA overlays and bot-harness assertions
(`tools/qa/multiplayer-bot-harness/scripts/avatar_party_8.yaml`).

---

## Edge cases the replicator handles for you

| Scenario | Behaviour |
| --- | --- |
| Echo of our own pose (server fan-out doesn't filter sender) | Dropped before adapter / events fire (`IsLocal()` check). |
| `PEER_LEFT` arrives before final pose | Adapter disposed; subsequent late poses no-op. |
| `AVATAR_DESCRIPTOR` arrives before first pose | Adapter spawned via factory + `LoadAsync(descriptor)` invoked. |
| `LOD_HINT` arrives for unknown peer | No-op (we only set LOD on adapters we already track). |
| Quaternion `q.w` close to zero after rounding | Replaced with `Quaternion.identity`. |
| `Detach()` + `Attach(otherSession)` mid-game | Idle baselines + adapter map are reset cleanly. |
| Anchor root rotates between frames | Pose is sampled in anchor-local each frame, so the published wire is anchor-stable even while the world moves. |

---

## What this component is NOT

* It does **not** mint Nakama matches. Use `IIVXMultiplayer.JoinMatchAsync(...)`.
* It does **not** load or render avatar meshes. Supply the `IIVXAvatar` factory.
* It does **not** handle voice / lip-sync. Use `IVXLiveKitVoiceProvider` +
  `IVXLiveKitVisemeStream` + `IVXLiveKitVisemeBinder`.
* It does **not** publish world-space "user" pose if you've set
  `_anchorRoot` — anchor-local is the wire. That's intentional.

---

## Reference reading

* `Assets/Intelli-verse-X-SDK/MultiplayerKernel/API/IIVXAvatar.cs` — adapter
  contract.
* `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Wire/IVXWireConstants.cs` —
  `IVXAvatarOp` constants.
* `schemas/multiplayer/templates/avatar_replication.proto` — wire schema.
* `SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift` —
  Swift sibling, semantically identical apart from publishing in
  Vision Pro `worldOrigin` space.
* `SDKs/javascript/packages/multiplayer/src/webxr/adapter.ts` — JS sibling,
  identical quantization math.
* `docs/multiplayer/avatar-interop.md` — full wire spec.
* `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md` §7 — readiness matrix
  and prefab recipe (this README is the long form).
