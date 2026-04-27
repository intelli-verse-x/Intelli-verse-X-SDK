# IVX Avatar Interop Spec — v1

**Stable:** wire frozen at schema_version = 1.
**Schema source of truth:** `schemas/avatar/avatar_v1.proto`.
**Replication transport:** `AvatarReplicationMatch` (template id `avatar-replication-v1`, opcodes 0xF000–0xFFFF).

The goal of this spec is exactly one outcome: **a Quest user, a Vision Pro
user, a WebXR user, and a flat-screen Unity user can stand in the same room
and see each other's avatars correctly animated, on the first frame, every
time, with no per-platform branching at the application layer.**

Every section below is part of the contract. Anything an adapter does
beyond this spec is a vendor feature, not a portable feature.

---

## 1. Reference skeleton — `ivx_humanoid_v1`

19 joints, ordered as `HumanoidJoint` in `avatar_v1.proto`. Joints are in
**right-handed Y-up** local-to-parent transforms, **meters**, **radians**.
Bind pose is T-pose, arms parallel to floor, palms down, feet shoulder-width.

| Index | Enum                | Bone name (canonical)         | Parent           | Notes                                  |
|-------|---------------------|-------------------------------|------------------|----------------------------------------|
| 0     | HJ_HIPS             | `Hips`                        | (root)           | World origin of the avatar.            |
| 1     | HJ_SPINE            | `Spine`                       | Hips             |                                        |
| 2     | HJ_CHEST            | `Chest`                       | Spine            | Optional `UpperChest` collapsed here.  |
| 3     | HJ_NECK             | `Neck`                        | Chest            |                                        |
| 4     | HJ_HEAD             | `Head`                        | Neck             | Camera/HMD anchor — eye height = +0.10m. |
| 5     | HJ_LEFT_SHOULDER    | `LeftShoulder`                | Chest            |                                        |
| 6     | HJ_LEFT_UPPER_ARM   | `LeftUpperArm`                | LeftShoulder     |                                        |
| 7     | HJ_LEFT_LOWER_ARM   | `LeftLowerArm`                | LeftUpperArm     | "Forearm" alias accepted on import.    |
| 8     | HJ_LEFT_HAND        | `LeftHand`                    | LeftLowerArm     | Wrist anchor; finger pose joins here.  |
| 9     | HJ_RIGHT_SHOULDER   | `RightShoulder`               | Chest            |                                        |
| 10    | HJ_RIGHT_UPPER_ARM  | `RightUpperArm`               | RightShoulder    |                                        |
| 11    | HJ_RIGHT_LOWER_ARM  | `RightLowerArm`               | RightUpperArm    |                                        |
| 12    | HJ_RIGHT_HAND       | `RightHand`                   | RightLowerArm    |                                        |
| 13    | HJ_LEFT_UPPER_LEG   | `LeftUpperLeg`                | Hips             |                                        |
| 14    | HJ_LEFT_LOWER_LEG   | `LeftLowerLeg`                | LeftUpperLeg     |                                        |
| 15    | HJ_LEFT_FOOT        | `LeftFoot`                    | LeftLowerLeg     |                                        |
| 16    | HJ_RIGHT_UPPER_LEG  | `RightUpperLeg`               | Hips             |                                        |
| 17    | HJ_RIGHT_LOWER_LEG  | `RightLowerLeg`               | RightUpperLeg    |                                        |
| 18    | HJ_RIGHT_FOOT       | `RightFoot`                   | RightLowerLeg    |                                        |

> Indices 19–31 are reserved. Adapters MUST ignore unknown joints.

### 1.1 Source-format alias table

These aliases are accepted on import (case-insensitive, underscores and
spaces equivalent). Anything outside this table is rejected by `glb-import`
with a structured error so artists fix the source, not adapters.

| Canonical          | Mixamo                          | Unity Humanoid       | RPM (Ready Player Me) | Mecanim             | Meta Avatars         |
|--------------------|---------------------------------|----------------------|-----------------------|---------------------|----------------------|
| `Hips`             | `mixamorig:Hips`                | `Hips`               | `Hips`                | `Hips`              | `b_Pelvis`           |
| `Spine`            | `mixamorig:Spine`               | `Spine`              | `Spine`               | `Spine`             | `b_Spine0`           |
| `Chest`            | `mixamorig:Spine1` (+ Spine2)   | `Chest` (or UpperChest) | `Spine1`           | `Chest`             | `b_Spine1`           |
| `Neck`             | `mixamorig:Neck`                | `Neck`               | `Neck`                | `Neck`              | `b_Neck0`            |
| `Head`             | `mixamorig:Head`                | `Head`               | `Head`                | `Head`              | `b_Head`             |
| `LeftUpperArm`     | `mixamorig:LeftArm`             | `LeftUpperArm`       | `LeftArm`             | `LeftUpperArm`      | `b_LeftArmUpper`     |
| `LeftLowerArm`     | `mixamorig:LeftForeArm`         | `LeftLowerArm`       | `LeftForeArm`         | `LeftLowerArm`      | `b_LeftArmLower`     |
| `LeftHand`         | `mixamorig:LeftHand`            | `LeftHand`           | `LeftHand`            | `LeftHand`          | `b_LeftHandWrist`    |
| `LeftUpperLeg`     | `mixamorig:LeftUpLeg`           | `LeftUpperLeg`       | `LeftUpLeg`           | `LeftUpperLeg`      | `b_LeftLegUpper`     |
| `LeftLowerLeg`     | `mixamorig:LeftLeg`             | `LeftLowerLeg`       | `LeftLeg`             | `LeftLowerLeg`      | `b_LeftLegLower`     |
| `LeftFoot`         | `mixamorig:LeftFoot`            | `LeftFoot`           | `LeftFoot`            | `LeftFoot`          | `b_LeftFootBall`     |
| (right side)       | (mirror of above)               | (mirror)             | (mirror)              | (mirror)            | (mirror)             |

`LeftShoulder`/`RightShoulder` are optional — if missing in the source,
import sets them to identity transforms parented to `Chest`. This keeps
RPM and visionOS Personas (which may collapse the shoulder to the chest)
working without manual editing.

### 1.2 Fingers — extension joints

Finger joints are NOT part of the 19-joint replicated body stream. They
ride on `OP_XR_FINGER_POSE` as 5 fingers × 3 joints = 15 packed `uint8`
curls per hand (0..255). The naming spec for source FBX/GLB:

```
LeftHand
  ├── LeftThumb1, LeftThumb2, LeftThumb3
  ├── LeftIndex1, LeftIndex2, LeftIndex3
  ├── LeftMiddle1, LeftMiddle2, LeftMiddle3
  ├── LeftRing1, LeftRing2, LeftRing3
  └── LeftLittle1, LeftLittle2, LeftLittle3
```

Importer SHALL accept Mixamo's `LeftHandThumb1`-style names with the same
canonical mapping; the curls are derived from local-to-parent rotation
around the bone's primary flex axis (Z by convention).

---

## 2. Mesh format — GLB / GLTF 2.0

| Property                | Required value                                                |
|-------------------------|---------------------------------------------------------------|
| Container               | `.glb` (preferred) or `.gltf` + `.bin`                        |
| Coordinate system       | Right-handed, Y-up, meters, T-pose bind                       |
| Up axis on import       | Y-up                                                          |
| Mesh                    | Single scene, single armature                                 |
| Triangles per LOD       | LOD0 ≤ 25k, LOD1 ≤ 12k, LOD2 ≤ 5k, LOD3 ≤ 800 (billboard sprite) |
| Materials               | PBR Metal/Roughness; max 4 materials per LOD                  |
| Textures                | KTX2/Basis preferred; PNG fallback. ≤ 2k×2k                   |
| Skin weights            | Max 4 bones per vertex, weights sum to 1.0                    |
| Morph targets (face)    | Required for talking avatars (see §3)                         |
| Embedded animation      | NOT used — pose comes from network                            |
| Avatar height range     | 1.30 m – 2.10 m (eye height); reject outside                  |

### 2.1 Required GLTF extras

The glb root extras MUST contain:

```json
{
  "ivx_avatar": {
    "schema_version": 1,
    "skeleton_profile": "ivx_humanoid_v1",
    "blendshape_profile": "arkit_52",
    "lods": ["lod0.glb", "lod1.glb", "lod2.glb", "billboard.png"],
    "fingerprint_sha256": "<hex>"
  }
}
```

`AvatarDescriptor.fingerprint_sha256` MUST equal SHA-256 over the LOD0
mesh + skeleton + blendshape names (binary stable). This is what
late-joiners use to verify they downloaded the same avatar the publisher
shipped.

---

## 3. Blendshape profiles

Three profiles are accepted on the wire. Adapters declare support in
`OP_CLIENT_HELLO`:

| Profile id     | Count | Source                         | Wire encoding                         |
|----------------|-------|--------------------------------|---------------------------------------|
| `arkit_52`     | 52    | Apple ARKit blendshapes        | `bytes blendshapes` = 52 × `uint8`    |
| `ovr_60`       | 60    | Meta OVRFaceExpressions        | mapped to ARKit 52 + 8 extras         |
| `vrm_69`       | 69    | VRM 1.0 expression set         | mapped to ARKit 52 (extras dropped)   |
| `none`         | 0     | flat-screen / fallback         | empty payload                         |

Server-side normalization: `AvatarReplicationMatch` re-stamps incoming
face frames into the recipient's declared `blendshape_profile` capability
using the canonical 52-key mapping below. Recipients on `none` get
zero-length face payloads with `WARN_AVATAR_FALLBACK` once at join.

### 3.1 Canonical 52-key ARKit map

The wire ordering is **identical to ARKit's `ARFaceAnchor.blendShapes`
keyed enumeration order** (Apple stable 2017+). Index = enum ordinal:

```
0  browDownLeft        13 eyeWideLeft         26 mouthLeft           39 mouthShrugUpper
1  browDownRight       14 eyeWideRight        27 mouthLowerDownLeft  40 mouthSmileLeft
2  browInnerUp         15 jawForward          28 mouthLowerDownRight 41 mouthSmileRight
3  browOuterUpLeft     16 jawLeft             29 mouthPressLeft      42 mouthStretchLeft
4  browOuterUpRight    17 jawOpen             30 mouthPressRight     43 mouthStretchRight
5  cheekPuff           18 jawRight            31 mouthPucker         44 mouthUpperUpLeft
6  cheekSquintLeft     19 mouthClose          32 mouthRight          45 mouthUpperUpRight
7  cheekSquintRight    20 mouthDimpleLeft     33 mouthRollLower      46 noseSneerLeft
8  eyeBlinkLeft        21 mouthDimpleRight    34 mouthRollUpper      47 noseSneerRight
9  eyeBlinkRight       22 mouthFrownLeft      35 mouthShrugLower     48 tongueOut
10 eyeLookDownLeft     23 mouthFrownRight     36 (reserved)          49 (reserved)
11 eyeLookDownRight    24 mouthFunnel         37 (reserved)          50 (reserved)
12 eyeLookInLeft       25 (continued in       38 (reserved)          51 (reserved)
                          Apple's own order)
```

`(reserved)` slots ride at zero on the wire and are reserved for future
ARKit extensions; clients MUST clamp unknown weights to 0.

### 3.2 RPM / OVR / VRM mapping

| Source key (RPM half-x)  | ARKit canonical key       | Notes                                |
|--------------------------|---------------------------|--------------------------------------|
| `eyeBlinkLeft`           | `eyeBlinkLeft`            | identity                             |
| `mouthSmile_L`           | `mouthSmileLeft`          | identity                             |
| `viseme_aa`              | `mouthFunnel` 0.7 + `jawOpen` 0.5 | linear blend                  |
| `viseme_ou`              | `mouthPucker`             |                                      |
| `mouthRollLower_L+R`     | `mouthRollLower`          | average L/R                          |

The full mapping table ships in
`SDKs/javascript/packages/multiplayer/src/avatar/blendshape-map.ts` (and
mirrored in the Unity / Swift adapters). Updates to that table are
versioned by `schema_version`; bumping the table without bumping the
schema is forbidden.

---

## 4. Sources & adapter behavior

| Source             | Skeleton              | Blendshapes  | Notes                                                      |
|--------------------|-----------------------|--------------|------------------------------------------------------------|
| Meta Avatars (OVR) | OVR humanoid          | `ovr_60`     | Use Meta SDK on Quest; otherwise `mesh_url` to baked GLB.  |
| Vision Pro Persona | proprietary           | none (use viseme stream) | `mesh_url` empty. Recipients render a placeholder + receive viseme stream over `OP_AGENT_VISEME_STREAM`-shaped frames. |
| Ready Player Me    | RPM humanoid          | RPM (mapped) | Pull GLB at first sight; cache by fingerprint.             |
| VRM 1.0            | VRM humanoid          | `vrm_69`     | License flags MUST be honored (see `meta` extras).         |
| `ivx_native`       | `ivx_humanoid_v1`     | `arkit_52`   | Authored to spec.                                          |
| `fallback_billboard` | none                | none         | Single quad with player head photo or initials.            |

### 4.1 First-sight algorithm (every adapter implements this)

1. Receive `AvatarDescriptor` from kernel `OP_PLAYER_JOINED` (or snapshot on join).
2. If `mesh_url` is set, kick off `fetch(mesh_url)` and verify
   `sha256(stream) == fingerprint_sha256`. Mismatch → fall back to
   billboard, surface `WARN_AVATAR_FALLBACK`.
3. Until LOD0 is loaded, render a **billboard** at HeadPose so the user
   never sees a "missing person".
4. Once LOD0 is in, swap. LOD upgrades happen on `OP_XR_AVATAR_LOD`
   (server-driven, see §5).

### 4.2 Vision Pro Persona special case

Apple does not let third-party apps export a Persona mesh. The visionOS
adapter therefore:
- Sends `AvatarDescriptor.source = AVATAR_SOURCE_VISIONOS_PERSONA` with
  `mesh_url = ""`.
- Streams head pose, hand pose, and the system-supplied viseme weights
  mapped to `arkit_52` (jawOpen + mouth shape subset).
- Other clients render a **stylized stand-in head** parented to HeadPose
  with viseme animation. This is documented as expected behavior and
  shown in the platform notes.

---

## 5. LOD policy

| LOD | Triangles | Used when                                      | Send rate                          |
|-----|-----------|------------------------------------------------|------------------------------------|
| 0   | ≤ 25k     | Within 1 × `aoi_radius_mm`                     | full head/hand/body/face/finger    |
| 1   | ≤ 12k     | Within 2 × `aoi_radius_mm`                     | head + hand + face (no body / finger) |
| 2   | ≤ 5k      | Within 3 × `aoi_radius_mm`                     | head + hand only                   |
| 3   | billboard | Beyond 3 × `aoi_radius_mm`, or low bandwidth   | head only                          |

`AvatarReplicationMatch` evaluates LOD per-tick and emits
`OP_XR_AVATAR_LOD { lod, reason }` when the band changes. Reason codes:
`"distance"`, `"bandwidth"`, `"occluded"`, `"near"`.

Clients MUST NOT promote LOD on their own — promotion is server-driven.
This prevents bandwidth-cheating clients from forcing peers into LOD 0.

---

## 6. Conformance checklist (per adapter)

A platform adapter is "interop-conformant" when ALL of:

- [ ] Skeleton: imports the alias table; rejects sources outside it.
- [ ] Coordinate system: Y-up, meters, T-pose; verified by integration test.
- [ ] Blendshapes: ARKit 52 indexing matches §3.1 byte order.
- [ ] LOD: all four LODs honored; billboard fallback always present.
- [ ] Authority: never accepts pose payloads with `user_id != self`.
- [ ] Fingerprint: validates `fingerprint_sha256` before applying LOD0 mesh.
- [ ] First-sight: billboard rendered before mesh resolves.
- [ ] Quantization: head/hand pose published as `PoseQuantized` proto, not raw float.

The 12-test conformance suite (see `docs/multiplayer/conformance.md`) has
seven tests dedicated to this checklist (interop-01 through interop-07).

---

## 7. License & content flags

`AvatarDescriptor` MUST carry license metadata in the GLB extras when the
source bears restrictions (VRM, RPM commercial). The kernel respects:

| Flag                         | Server behavior                                   |
|------------------------------|---------------------------------------------------|
| `vrm.allowedUserName: "OnlyAuthor"` | Reject from public matches; allow only if `user_id == author_id`. |
| `commercialUssageName: "Disallow"` | Server tags match label `commercial:false`; recipients in monetized rooms get `fallback_billboard`. |
| `violentUssageName: "Disallow"` | Match labels `violence:false`; UGC pipeline blocks weapons. |
| `sexualUssageName: "Disallow"` | Match labels `nsfw:false`; moderation pipeline elevates priority. |

These flags are surfaced to game logic via match labels so games can
choose to enforce or display them.

---

## 8. Versioning

| schema_version | Breaking change                                                            |
|----------------|-----------------------------------------------------------------------------|
| 1              | Initial release. Frozen.                                                    |
| 2 (planned)    | Add `morphHints` for non-ARKit profiles; finger curls become 16-bit.        |

Adapters declare supported versions in `OP_CLIENT_HELLO.capabilities.avatar_versions`.
The kernel sends each peer's avatar in the highest version both ends understand.
Mismatch falls back to `schema_version=1`.

---

*Reference implementations: `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/`,
`SDKs/javascript/packages/multiplayer/src/avatar/`, `SDKs/visionos/Sources/IVXMultiplayer/Avatar/`.*
