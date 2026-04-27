# IntelliVerseX Unity SDK — End-to-end analysis for a Unity dev shipping a 3D world to mobile, visionOS, Quest, VR + AR

> **Scope.** You are a Unity developer building **one Unity 6 project** that ships a shared 3D world to:
>
> 1. Mobile (iOS + Android, handheld).
> 2. Apple Vision Pro (visionOS, full-immersive Unity build, no PolySpatial requirement).
> 3. Meta Quest 2 / 3 / Pro (Android-XR via Meta XR SDK).
> 4. Generic VR (PCVR / SteamVR / Pico) via OpenXR.
> 5. Mobile AR (ARCore + ARKit) via AR Foundation.
>
> You want all the LiveKit-era capabilities we shipped in Phases 1-4 (multiplayer voice, AI host voice, lip-synced AI avatar) and the *forward-compatibility* for Phase 5 (vision-enabled AI avatars), with the same Unity codebase, on a single build pipeline.
>
> This memo tells you (a) **what works today**, (b) **what to install / define / drop in a scene**, and (c) **where the seams are** — what needs platform-specific code, and what can stay in shared Unity logic.

---

## 0. TL;DR — the 8-line answer

1. **Voice (LiveKit) and AI-host voice (LiveKit Agents) work unchanged on every platform** that has the LiveKit Unity SDK working. That's mobile, visionOS, Quest, PCVR, AR Foundation. **No platform-specific code needed.**
2. **Lip-synced AI avatar (`viseme.v1` data channel + ARKit-52 morphs) works unchanged on every platform** as long as your avatar mesh has ARKit-52 morph targets. Drop `IVXLiveKitVisemeStream` + `IVXLiveKitVisemeBinder` on the avatar root — done.
3. **Spatial anchor providers are platform-specific.** Quest → `IVXMetaSpatialAnchorProvider`. iOS group play → `IVXARKitCollabAnchorProvider`. HoloLens / Pico / generic OpenXR → `IVXOpenXrMsftSpatialAnchorProvider`. Plain ARCore/ARKit → **the new `IVXARFoundationAnchorProvider` shipped in this commit**. PCVR + visionOS standalone → `IVXAnchorFallback.BuildPcvrFakeOffer`.
4. **Avatar replication wire is shipped, AND the Unity-side `IVXAvatarReplicator` MonoBehaviour now ships too.** Drop `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/IVXAvatarReplicator.cs` on a GameObject, point it at your local head + hand transforms, and call `replicator.Attach(matchSession)` after joining the match. Either provide an `IIVXAvatar` factory via `SetAvatarFactory(...)` (recommended) or subscribe to `OnPeerPose`/`OnPeerBlendshapes` to drive your own scene graph.
5. **Phase 5 (vision-enabled AI avatars) requires zero client work.** When you flip `IVX_LIVEKIT_VISION_ENABLED=true` server-side, the same Phase-4 avatar starts "looking at" the human's video track and replies grounded in what it sees — same LiveKit room, same `viseme.v1` channel, same Unity components.
6. **Mint voice tokens through the new `IVXVoiceTokenClient.MintAsync(...)` helper** (shipped this turn). Don't hand-roll the `mp_voice_token` RPC payload.
7. **Apple Vision Pro split:** the Swift visionOS SDK (`SDKs/visionos/`) is an *alternative* native path for RealityKit games. If you're shipping Unity-on-visionOS (full-immersive metal renderer), use the Unity SDK exactly like Quest — the LiveKit Unity SDK supports visionOS as a Unity build target.
8. **PolySpatial (Apple's bounded-volume mode) is not yet wired.** It needs an extra Unity package and a different render path; we treat it as a *separate target* with the same multiplayer kernel but no current avatar-mesh adapter. Bounded-volume avatars are on the Phase-5+ roadmap.

---

## 1. Capability × platform readiness matrix

Legend: ✅ = ships in box · 🟡 = partial / requires you to wire one MonoBehaviour · ⚠ = define-gated, you install the vendor SDK · 🔴 = gap — not yet shipped · ➖ = not applicable to platform.

| Capability | iOS / Android (handheld) | iOS AR-mode (handheld) | visionOS (Unity full-immersive) | Quest 2 / 3 / Pro | Pico Neo 3 / 4 | PCVR (SteamVR / Index) |
|---|---|---|---|---|---|---|
| **Nakama transport** (`IVXNakamaMultiplayer`) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Multiplayer voice** (`IVXLiveKitVoiceProvider`) | ⚠ `IVX_LIVEKIT` | ⚠ | ⚠ | ⚠ | ⚠ | ⚠ |
| **AI host voice (multi-human)** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **AI host voice (1:1)** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Mint voice token (`mp_voice_token`)** | ✅ via `IVXVoiceTokenClient` (NEW) | ✅ | ✅ | ✅ | ✅ | ✅ |
| **AI avatar lip-sync (`viseme.v1`)** — `IVXLiveKitVisemeStream` + `IVXLiveKitVisemeBinder` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Avatar pose replication wire (`avatar_replication.proto`)** | ✅ generated | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Avatar replicator MonoBehaviour** (drives a Unity skeleton from the wire) | ✅ `IVXAvatarReplicator.cs` (NEW) | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Spatial anchor — vendor cloud** | ➖ | ⚠ ARKit Collab (`UNITY_IOS`) | ⚠ ARKit Collab | ⚠ Meta Cloud (`IVX_META_XR`) | ⚠ MSFT-OpenXR | ⚠ MSFT-OpenXR |
| **Spatial anchor — AR Foundation generic (NEW)** | ➖ | ⚠ `INTELLIVERSEX_HAS_ARFOUNDATION` (Android too) | ⚠ | 🟡 (Meta is preferred) | 🟡 | ➖ |
| **Spatial anchor — Geospatial (lat/lng/alt)** | ➖ | ⚠ `INTELLIVERSEX_HAS_ARGEO` opt-in (Android only today) | ➖ | ➖ | ➖ | ➖ |
| **Anchor fallback (PCVR fake-floor / QR marker)** | ✅ `IVXAnchorFallback.BuildPcvrFakeOffer` / `BuildQrOffer` | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Hand tracking pose replication** | ➖ | ➖ | ✅ (visionOS hand bones) | ✅ Meta XR | 🟡 Pico XR (Pico 4) | 🟡 OpenXR ext |
| **Eye tracking (gaze for vision-enabled avatars)** | ➖ | ➖ | ✅ visionOS | ✅ Quest Pro / Quest 3 | ➖ | 🟡 Index/Vive Pro Eye |
| **Passthrough / mixed reality compositing** | ✅ AR Foundation | ✅ AR Foundation | 🟡 PolySpatial only | ✅ Meta Passthrough | ✅ Pico passthrough | ➖ |
| **Phase-5 vision-enabled AI avatar** | ✅ flag-only (server) | ✅ | ✅ | ✅ | ✅ | ✅ |
| **3DGS avatar (Phase-4 §10) — RealityKit ECS port** | ➖ | ➖ | 🟡 Swift native only (`SDKs/visionos/`) | 🔴 not yet on Unity | 🔴 | 🔴 |
| **Apple PolySpatial (bounded volume)** | ➖ | ➖ | 🔴 separate render path, not wired | ➖ | ➖ | ➖ |

**Reading the matrix.** Almost every cell is green or define-gated. The remaining items you should plan around are:
1. ~~The `IVXAvatarReplicator` MonoBehaviour~~ **— shipped this turn.** Drop on a GameObject, point at head/hand transforms, `Attach(session)`. See §7 for the prefab recipe.
2. The 3DGS avatar adapter on Unity (visionOS native ships a Swift port; Unity Gaussian-splat support is coming through `com.unity.rendering.gaussiansplats` — out of scope today, voice + viseme + skeleton avatars work without it).
3. PolySpatial bounded-volume — separate scope, doesn't block any of (1)–(8).
4. ARCore Geospatial — opt-in, only meaningful if you actually want a single shared world coordinate frame across distant devices outdoors.

---

## 2. The single project structure

You ship **one Unity 6 project**. Build configs differ; scenes don't.

```
Assets/
├─ _IntelliVerseXSDK/                 ← UPM package (drop in via Package Manager)
│  ├─ MultiplayerKernel/
│  │  ├─ API/                          ← IIVXMultiplayer / IIVXVoice / IIVXAvatar / IIVXAnchorProvider
│  │  ├─ Adapters/                     ← IVXNakamaMultiplayer, IVXMatchSession
│  │  ├─ Voice/
│  │  │  ├─ IVXLiveKitVoiceProvider.cs       ← Phase 1-3
│  │  │  ├─ IVXLiveKitVisemeStream.cs        ← Phase 4
│  │  │  ├─ IVXLiveKitVisemeBinder.cs        ← Phase 4 DX
│  │  │  └─ IVXVoiceTokenClient.cs           ← NEW (this turn)
│  │  ├─ Anchor/
│  │  │  ├─ IVXMetaSpatialAnchorProvider.cs           ← Quest
│  │  │  ├─ IVXARKitCollabAnchorProvider.cs           ← iOS SharePlay
│  │  │  ├─ IVXOpenXrMsftSpatialAnchorProvider.cs     ← HoloLens / Pico
│  │  │  └─ IVXARFoundationAnchorProvider.cs          ← NEW (this turn)
│  │  └─ XR/
│  │     ├─ IVXPicoXRBootstrapper.cs
│  │     └─ PICO_XR_NOTES.md
│  ├─ Backend/                         ← IVXNakamaManager (extend per game)
│  └─ Platform/
│     ├─ IVXARHelper.cs                ← AR Foundation lifecycle
│     └─ IVXXRPlatformHelper.cs        ← detects Quest / OpenXR / visionOS / WMR / AR Foundation
│
└─ _MyGame/
   ├─ Scenes/
   │  └─ World3D.unity                  ← single scene; XR rig is platform-conditional
   ├─ Prefabs/
   │  ├─ AIAvatar.prefab                ← skeleton + ARKit-52 morphs + IVXLiveKitVisemeStream + IVXLiveKitVisemeBinder
   │  ├─ HumanAvatar.prefab             ← drives off avatar_replication.proto
   │  ├─ XRRig.iOS.prefab               ← AR Foundation rig
   │  ├─ XRRig.Quest.prefab             ← Meta XR rig
   │  ├─ XRRig.OpenXR.prefab            ← OpenXR rig (Pico / SteamVR / WMR)
   │  └─ XRRig.VisionOS.prefab          ← visionOS Unity rig
   └─ Scripts/
      ├─ MyGameNakamaManager.cs        ← extends IVXNakamaManager
      └─ MyGameAvatarFactory.cs        ← returns IIVXAvatar from your prefab pool
                                          (the replicator itself ships in the SDK as
                                           Assets/Intelli-verse-X-SDK/MultiplayerKernel/
                                           Avatar/IVXAvatarReplicator.cs)
```

**Single-scene rule.** `World3D.unity` references all four `XRRig` prefabs as inactive children. A `RuntimeXRRigSelector` MonoBehaviour activates exactly one based on `IVXXRPlatformHelper.Instance.ActivePlatform`. This way you don't fork the scene per platform.

---

## 3. The 60-second bootstrap sequence (every platform)

```csharp
using IntelliVerseX.Backend;
using IntelliVerseX.MultiplayerKernel.Adapters;
using IntelliVerseX.MultiplayerKernel.API;
using IntelliVerseX.MultiplayerKernel.Voice;

public sealed class World3DBoot : MonoBehaviour
{
    [SerializeField] private MyGameNakamaManager _nakama;
    [SerializeField] private IVXLiveKitVoiceProvider _voice;
    [SerializeField] private IVXLiveKitVisemeBinder  _binder;

    private async void Start()
    {
        await _nakama.InitializeAsync();

        var multiplayer = new IVXNakamaMultiplayer(_nakama);
        await multiplayer.InitializeAsync();

        var session = await multiplayer.JoinByTemplateAsync(
            templateId: "shared-3d-world-v1",
            options: new IVXJoinOptions
            {
                Capabilities = new IVXClientCapabilities
                {
                    HasVoice           = true,
                    HasAvatar          = true,
                    HasSpatialAnchors  = IVXXRPlatformHelper.Instance.IsXRActive,
                    BlendshapeProfile  = IVXBlendshapeProfile.Arkit52,
                }
            });

        var token = await IVXVoiceTokenClient.MintAsync(
            _nakama, session.MatchId,
            canPublish: true, canSubscribe: true, spatial: true);

        await _voice.ConnectAsync(token);
        _binder.Bind(_voice);   // viseme.v1 lip-sync starts flowing
    }
}
```

This is **the same bootstrap on every platform**. The platform-specific deltas live entirely in:
* Which `XRRig` prefab the selector activates.
* Which `IIVXAnchorProvider` instance you hand to the kernel (one `if/else` chain — see §5).
* Which scripting defines you flip in Player Settings.

---

## 4. Per-platform set-up checklist

### 4.1 iOS / Android (mobile, no XR)

- **UPM packages:**
  - `com.intelliversex.sdk` (this SDK)
  - `io.livekit.unity` (LiveKit Unity SDK)
  - `com.unity.nuget.newtonsoft-json`
- **Scripting defines:** `IVX_LIVEKIT`
- **Player Settings:** IL2CPP, ARM64, .NET Standard 2.1, mic permission text in iOS plist.
- **Rig prefab:** none — first-person camera + virtual joystick.
- **Anchor provider:** none — you don't need spatial anchors for handheld 3D worlds.

### 4.2 iOS / Android (AR Foundation)

- **Add UPM:** `com.unity.xr.arfoundation` (5.x) + `com.unity.xr.arkit` + `com.unity.xr.arcore`.
- **Add scripting define:** `INTELLIVERSEX_HAS_ARFOUNDATION` on top of the mobile defines.
- **Optional:** `com.google.ar.core.arfoundation.extensions` + define `INTELLIVERSEX_HAS_ARGEO` if you want Geospatial co-location.
- **Rig prefab:** `XRRig.iOS.prefab` (AR Foundation `ARSession` + `ARCameraManager`).
- **Anchor provider:**
  - iOS with SharePlay group: `new IVXARKitCollabAnchorProvider()` *(SharePlay-grade co-location)*.
  - **Anything else (Android ARCore, iOS without SharePlay): `new IVXARFoundationAnchorProvider()`** *(NEW, ships this turn — was previously a fake-floor fall through)*.

### 4.3 Apple Vision Pro (Unity full-immersive)

- **Build target:** visionOS (Unity 6 supports it as a player target).
- **UPM:** `com.unity.xr.visionos` + LiveKit Unity SDK (visionOS arm64 builds available since LK Unity ≥ 2.5).
- **Scripting defines:** `IVX_LIVEKIT;INTELLIVERSEX_HAS_VISIONOS`.
- **Rig prefab:** `XRRig.VisionOS.prefab` — `XROrigin` with visionOS hand-tracking + gaze sources.
- **Anchor provider:** none today (visionOS shared anchors require ARKitSession + SharePlay through native code; the provider lives in the Swift `SDKs/visionos/` SDK, not the Unity SDK). For solo and remote co-located play: `IVXAnchorFallback.BuildPcvrFakeOffer()` (the floor-relative anchor).
- **PolySpatial (bounded volume):** **not yet wired.** Use full-immersive metal mode for now.
- **Eye tracking:** available; feed `XRGaze` into your `IIVXAvatar.ApplyHeadPose` for the looker-toward-AI gaze cue.

### 4.4 Meta Quest 2 / 3 / Pro

- **UPM:** Meta XR SDK (from Meta developer site or Asset Store) + LiveKit Unity SDK.
- **Scripting defines:** `IVX_LIVEKIT;IVX_META_XR;INTELLIVERSEX_HAS_META_XR`.
- **Manifest permissions:** `com.oculus.permission.USE_SCENE`, `com.oculus.permission.USE_ANCHOR_API`, `RECORD_AUDIO`.
- **Rig prefab:** `XRRig.Quest.prefab` — `OVRCameraRig` + `OVRHand` left/right.
- **Anchor provider:** `new IVXMetaSpatialAnchorProvider()` (Meta Cloud Anchors).
- **Hand tracking:** wired into `IIVXAvatar.ApplyHandPose` via Meta XR's `OVRHandTracking`.
- **Eye tracking:** Quest Pro / Quest 3 only — feeds the same gaze pose as visionOS.

### 4.5 PCVR / SteamVR / Pico / WMR (OpenXR)

- **UPM:** `com.unity.xr.openxr` + LiveKit Unity SDK.
- **Scripting defines:** `IVX_LIVEKIT;INTELLIVERSEX_HAS_OPENXR` *(plus `IVX_HAS_PICO_XR` on Pico)*.
- **Rig prefab:** `XRRig.OpenXR.prefab` — `XROrigin` + `XRController` left/right.
- **Anchor provider:** `new IVXOpenXrMsftSpatialAnchorProvider()` if the runtime advertises `XR_MSFT_spatial_anchor` (HoloLens 2, WMR, Pico). Otherwise `IVXAnchorFallback.BuildPcvrFakeOffer()`.
- **Pico-specific tweaks:** drop `IVXPicoXRBootstrapper` on the rig.

---

## 5. Picking an `IIVXAnchorProvider` at runtime

The kernel doesn't care which provider you use, but you have to choose one per platform. The recommended runtime selector:

```csharp
public static IIVXAnchorProvider PickAnchorProvider()
{
    // 1. Quest cloud anchors — best fidelity on Meta hardware.
#if IVX_META_XR
    var meta = new IVXMetaSpatialAnchorProvider();
    if (meta.IsAvailable) return meta;
#endif

    // 2. iOS SharePlay group sessions — best for living-room co-play.
#if UNITY_IOS && !UNITY_EDITOR
    var arkit = new IVXARKitCollabAnchorProvider();
    if (arkit.IsAvailable) return arkit;
#endif

    // 3. OpenXR MSFT — HoloLens, Pico, WMR.
#if INTELLIVERSEX_HAS_OPENXR
    var msft = new IVXOpenXrMsftSpatialAnchorProvider();
    if (msft.IsAvailable) return msft;
#endif

    // 4. Plain ARCore / ARKit / AR Foundation HMDs — NEW this turn.
#if INTELLIVERSEX_HAS_ARFOUNDATION
    var arf = new IVXARFoundationAnchorProvider { UseGeospatial = false };
    if (arf.IsAvailable) return arf;
#endif

    // 5. Last resort — fake floor for PCVR spectators / unsupported.
    return null; // kernel will use IVXAnchorFallback.BuildPcvrFakeOffer().
}
```

You stamp the chosen provider into the `JoinByTemplateAsync` call so the server-side `mixed-reality-anchor-v1` template knows which token format to expect.

---

## 6. Building the AI avatar prefab (the lip-sync end of LiveKit)

This is the prefab you instance once for every AI avatar in the room. It's identical on every platform.

```
AIAvatar.prefab
├─ Root (Transform)
│  ├─ IVXLiveKitVisemeStream    (TargetMesh = SkinnedMeshRenderer below; BlendshapeNameMap = ARKit-52)
│  └─ IVXLiveKitVisemeBinder    (auto-binds to the LiveKit Room — see §3)
├─ Mesh
│  └─ SkinnedMeshRenderer (52 ARKit blendshapes named per Apple's spec: jawOpen, mouthSmileLeft, …)
└─ Audio
   └─ AudioSource (3D, the LiveKit RemoteAudioTrack drives this — handled by the LK SDK)
```

**Naming map:** call `IVXLiveKitVisemeStream.SetArkit52NameMap(orderedNames)` once at `Awake` with your 52 blendshape names — the receiver translates `viseme.v1` byte indices → mesh weights.

**That's the entirety of the visual lip-sync wiring.** It's the same prefab on Quest, iOS, Android, Pico, visionOS, and PCVR.

---

## 7. `IVXAvatarReplicator` — the prefab recipe

**This used to be the single non-trivial gap on Unity. It's now closed.** The component lives at `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/IVXAvatarReplicator.cs` and:

1. Subscribes to all 9 avatar-replication opcodes (`HEAD_POSE`/`LEFT_HAND_POSE`/`RIGHT_HAND_POSE`/`BLENDSHAPES`/`FINGER_CURLS`/`AVATAR_DESCRIPTOR`/`LOD_HINT`/`PEER_LEFT`/`AVATAR_FALLBACK` — see `IVXAvatarOp` in `Wire/IVXWireConstants.cs`).
2. Routes incoming poses to either an `IIVXAvatar` adapter (via the factory) or to `OnPeerPose`/`OnPeerBlendshapes` events for games that drive their own scene graph.
3. Publishes the local player's head + hand poses with smallest-three quaternion compression that is bit-for-bit compatible with the JS WebXR adapter (`SDKs/javascript/.../webxr/adapter.ts`) and the Go avatar-replication template.
4. Idle-suppresses publishes when the local pose hasn't moved past the configured epsilon, with a 1 Hz heartbeat so the server keeps the peer warm.
5. Disposes per-peer adapters on `PEER_LEFT`.

**Prefab recipe (5 minutes):**

```csharp
// 1. Create an empty GameObject in your "Players" scene.
// 2. Add the IVXAvatarReplicator component.
// 3. Drag your XR camera into _localHead.
// 4. Drag your left/right controller transforms into _localLeftHand / _localRightHand.
// 5. (Optional) Drag your AnchorRoot transform into _anchorRoot so all peers share a frame.
// 6. From your match-bootstrap code, after JoinMatchAsync(...) succeeds:
var replicator = GetComponent<IVXAvatarReplicator>();
replicator.SetAvatarFactory(desc => MyAvatarFactory.Spawn(desc)); // returns an IIVXAvatar
replicator.Attach(matchSession);
// 7. On match end, replicator.Detach() (also runs automatically on OnDisable / OnDestroy).
```

If you don't yet have an `IIVXAvatar` adapter, omit the factory and listen to `OnPeerPose` instead — the event already carries dequantized `Vector3 Position` and `Quaternion Rotation` for direct `Transform` writes.

The visionOS Swift port (`SDKs/visionos/Sources/IVXMultiplayer/Avatar/IVXAvatarReplicator.swift`) and the JS WebXR adapter remain the cross-engine references — the Unity component publishes the same `IVXPoseQuantized` wire shape they do.

---

## 8. How Phase 5 (vision-enabled AI avatars) lights up — zero Unity work

When the operator flips `IVX_LIVEKIT_VISION_ENABLED=true` server-side:

1. The same Phase-4 `livekit-agent-worker` *additionally* subscribes to your `LocalParticipant`'s video track (your front-facing camera or your virtual XR head camera).
2. The VLM groundings flow into the **same** TTS → BlendshapeDriver → `viseme.v1` chain. Your avatar now responds with "I see you raised your hand — that's the answer."
3. **Zero changes to your prefab, zero changes to your Unity code.** The viseme stream you already drive is the same; the audio track is the same; the Unity SDK is unaware of the vision leg.

What you *can* opt in to (still zero Unity SDK code, just camera config):
* Publish your XR rig's *world-camera* as a video track if you want the AI to see what you see in MR/AR (passthrough scene). The Meta XR Passthrough Camera API and visionOS `ARKitSession.CameraSource` both feed straight into LiveKit `LocalVideoTrack`.

---

## 9. Build pipeline — one project, six players

You ship **one** Unity 6 project. Build pipeline:

| Player | Build target | Required defines | Anchor provider | Tested LiveKit SDK |
|---|---|---|---|---|
| iOS handheld | iOS | `IVX_LIVEKIT` | none | LK Unity ≥ 2.5 (arm64 + sim) |
| iOS AR | iOS | `IVX_LIVEKIT;INTELLIVERSEX_HAS_ARFOUNDATION` | ARKitCollab → ARFoundation | LK Unity ≥ 2.5 |
| Android handheld | Android | `IVX_LIVEKIT` | none | LK Unity ≥ 2.5 |
| Android AR | Android | `IVX_LIVEKIT;INTELLIVERSEX_HAS_ARFOUNDATION` *(+ `INTELLIVERSEX_HAS_ARGEO` opt)* | ARFoundation | LK Unity ≥ 2.5 |
| visionOS | visionOS | `IVX_LIVEKIT;INTELLIVERSEX_HAS_VISIONOS` | none (fallback) | LK Unity ≥ 2.5 visionOS arm64 |
| Quest | Android | `IVX_LIVEKIT;IVX_META_XR;INTELLIVERSEX_HAS_META_XR` | MetaSpatial | LK Unity ≥ 2.5 |
| Pico | Android | `IVX_LIVEKIT;IVX_HAS_PICO_XR;INTELLIVERSEX_HAS_OPENXR` | OpenXR-MSFT | LK Unity ≥ 2.5 |
| PCVR | StandaloneWindows64 | `IVX_LIVEKIT;INTELLIVERSEX_HAS_OPENXR` | OpenXR-MSFT or fake | LK Unity ≥ 2.5 |

CI (GitHub Actions / Cloud Build) — six matrix entries, all from the same `Assets/`. Unit-test asmdef `IntelliVerseX.MultiplayerKernel.Tests` runs on EditMode for every entry.

---

## 10. The honest list of things that **don't** yet work on Unity

These are the items you should plan around, not be surprised by:

| # | Gap | Severity | Mitigation |
|---|---|---|---|
| 1 | ~~`MyGameAvatarReplicator` MonoBehaviour~~ | ~~High~~ → **Closed** | `IVXAvatarReplicator.cs` shipped this turn at `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/`. See §7. |
| 2 | 3D Gaussian-Splat avatar adapter on Unity | Medium — only matters if you want photo-realistic avatars beyond skinned mesh | Use skinned-mesh avatars on Unity until `com.unity.rendering.gaussiansplats` lands, or run the Swift native path on Vision Pro. |
| 3 | Apple PolySpatial (bounded volume) | Medium — only matters if you target the visionOS *home environment* mode | Ship Unity full-immersive on Vision Pro for v1; revisit when PolySpatial XR Plug-in is GA on Unity 6.x. |
| 4 | visionOS *shared* anchors from Unity | Medium — only matters for SharePlay co-located visionOS play | Use the Swift `SDKs/visionos/` path for SharePlay co-play; Unity build keeps fallback floor anchors. |
| 5 | ARCore Geospatial server-side resolve | Low | Built-in via the new `IVXARFoundationAnchorProvider.UseGeospatial=true`; you must add `INTELLIVERSEX_HAS_ARGEO` and the ARCore Extensions package. |
| 6 | Quest hand-finger curl wire encoding | Low | `IIVXAvatar.ApplyFingerCurls` exists; the Meta XR adapter that writes those bytes is not yet shipped — you write 30 LOC against `OVRHand.GetFingerConfidence`. |

Everything else in §1 is shipped, define-gated, or trivially flippable.

---

## 11. What changed in this commit

| File | Why |
|---|---|
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/IVXVoiceTokenClient.cs` | **NEW.** Typed Unity helper around `mp_voice_token` Nakama RPC so you don't hand-roll the JSON shape every time. Cheatsheet snippets now compile. |
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Anchor/IVXARFoundationAnchorProvider.cs` | **NEW.** Define-gated AR Foundation anchor provider. Closes the gap where plain ARCore / iOS-without-SharePlay had to use `IVXAnchorFallback.BuildPcvrFakeOffer`. Supports an opt-in Geospatial mode. |
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/IVXAvatarReplicator.cs` | **NEW.** Prefab-ready replicator MonoBehaviour for remote-human avatars. Closes the last critical-path gap. AI avatars already worked end-to-end (their poses come from the LiveKit Agents worker); this turn unlocks live multi-human shared 3D worlds on Unity. |
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/README.md` | **NEW.** Prefab recipe + factory pattern + idle/heartbeat tuning notes. |
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Wire/IVXWireConstants.cs` | Added `IVXAvatarOp` (HEAD_POSE / LEFT_HAND_POSE / RIGHT_HAND_POSE / BLENDSHAPES / FINGER_CURLS / AVATAR_DESCRIPTOR / LOD_HINT / PEER_LEFT / AVATAR_FALLBACK). |
| `docs/multiplayer/UNITY_3D_WORLD_E2E_ANALYSIS.md` | Updated readiness matrix + §7 prefab recipe + Phase-6 backlog (avatar replicator removed). |

---

## 12. Recommended Phase-6 backlog

Ranked by cost-vs-pain:

1. ~~`IVXAvatarReplicator.cs` MonoBehaviour~~ **— shipped.**
2. **Add `IVXAnchorProvider.ARFoundation = 8` to the proto + enum** — currently `IVXARFoundationAnchorProvider` reports as `QRFallback`. Trivial schema bump (`schemas/multiplayer/templates/mixed_reality_anchor.proto`), regenerate, retest bot harness.
3. **`IVXMetaXRAvatarAdapter.cs`** — wire Meta hand+face tracking into `IIVXAvatar.ApplyBlendshapes` / `ApplyFingerCurls` so Quest avatars get full facial + finger fidelity. The replicator's `PublishBlendshapes(...)` / `PublishFingerCurls(...)` API is the integration point.
4. **PolySpatial render-path adapter** — `IVXVisionOSPolySpatialBootstrapper`, separate scope from Unity full-immersive.
5. **Unity Gaussian-Splat avatar adapter** — when `com.unity.rendering.gaussiansplats` ships GA, port the Swift `IVXAvatarReplicator` 3DGS path to it (the wire is identical; only the adapter that consumes `IIVXAvatar.ApplyHeadPose` differs).

---

## 13. Conclusion

For the user's actual question — **can a Unity dev ship a single 3D world game across mobile, visionOS, Quest, VR + AR with conversational, vision-capable, lip-synced AI avatars and AI-human voice lobbies, with a single flag to flip vision on / off?** — the answer is:

* **Yes for voice + lip-synced avatars + AI host (single-human and multi-human) on every target.** All Phase 1-4 capabilities are platform-portable and currently shipped as `define + drop a prefab` integrations.
* **Yes for vision (Phase 5) when it ships server-side, with zero Unity SDK changes.**
* **Yes for spatial anchors on every target after this commit** (Quest, iOS-SharePlay, OpenXR-MSFT, AR Foundation, fallback). Before this commit there was a hole on plain ARCore + iOS-without-SharePlay; the new `IVXARFoundationAnchorProvider` closes it.
* **All critical-path gaps for human avatars are now closed.** `IVXAvatarReplicator.cs` ships in this commit (see §7). AI avatars already worked end-to-end via the LiveKit Agents worker without needing this component.
* **PolySpatial bounded-volume on visionOS is the only target with a render-path question mark** — recommended: ship full-immersive Unity on Vision Pro for v1.

You can start building today. The 60-second bootstrap in §3 is real, the prefab structure in §6 is real, the platform defines in §4 are real, the runbook lives in `Intelliverse-X-AI/docs/livekit/MIGRATION_FINAL_SIGNOFF.md`, and the QA harness lives in `tools/qa/multiplayer-bot-harness/`.

— *2026-04-26, post-Phase-4 sign-off, post-Phase-5 research*
