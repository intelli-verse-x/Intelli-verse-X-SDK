---
name: ivx-3d-character-pipeline
description: >-
  AI-powered 3D character generation with mesh, rigging, animation, and FBX/GLB export.
  Outputs conform to IntelliVerseX SDK schemas (character-3d-meta-v1, skeleton-v1,
  state-machine-v1). Use when the user says "3D character", "generate 3D model",
  "rigged character", "FBX character", "3D to sprites", "mesh generation",
  "character rigging", "motion library", "skeletal animation", "blend shapes",
  "3D game character", or needs 3D character assets for any game engine.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX 3D Character Pipeline

## Overview

The 3D Character Pipeline generates production-ready 3D game characters from text descriptions or 2D reference images. It produces rigged meshes, skeletal animations, blend shapes, and optionally renders 2D sprite sheets from 3D — all validated against IntelliVerseX SDK schemas (`character-3d-meta-v1`, `skeleton-v1`, `state-machine-v1`).

```
Input (text or image)
       │
       ▼
┌── Content-Factory ──────────────────────────────────┐
│  Mesh Gen (Trellis/Meshy) → MeshRigger (skeleton)   │
│  → AnimationApplicator (presets) → BlenderRenderer   │
│  → IVX Schema Export → S3 Upload                    │
└──────────────────────┬──────────────────────────────┘
                       │
            ┌──────────┼──────────┐
            ▼          ▼          ▼
         3D Mode    Hybrid     2D-Only
         GLB/FBX    FBX +      Sprite
         + anims    sprites    sheets
```

## Modes

| Mode | Output | Best For |
|------|--------|----------|
| `3d` | GLB mesh + rigged FBX + animation FBX clips | 3D games (Unity, Unreal, Godot 3D) |
| `hybrid` | All of 3D mode + Blender-rendered 2D sprite sheets | Games needing both 3D assets and 2D fallbacks |
| `2d` | 2D sprite sheets only (rendered from generated 3D) | 2D games wanting 3D consistency across views |

## Content-Factory API

### Trigger via MCP

```
trigger_pipeline character_3d --game_id my-game --mode hybrid --character "An armored knight with a glowing sword"
```

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/character_3d \
  -H "Content-Type: application/json" \
  -d '{
    "game_id": "my-game",
    "mode": "hybrid",
    "characters": [
      {
        "name": "Knight",
        "description": "An armored knight with a glowing sword and heavy shield"
      }
    ],
    "actions": ["idle", "walk", "run", "attack", "hurt", "death"],
    "generate_expressions": true,
    "ivx_export": true
  }'
```

## Generated Output

### 3D Mode

```
characters/Knight/
├── mesh/
│   ├── Knight.glb                    # Generated mesh
│   └── mesh_spec.json                # Mesh generation metadata
├── rigged/
│   └── Knight_rigged.fbx             # Rigged with skeletal hierarchy
├── animations/
│   ├── idle.fbx                      # Per-action animation clips
│   ├── walk.fbx
│   ├── run.fbx
│   ├── attack.fbx
│   ├── hurt.fbx
│   └── death.fbx
└── ivx/                              # SDK-compliant exports
    ├── character_3d.json              # Validates: character-3d-meta-v1
    ├── skeleton.json                  # Validates: skeleton-v1
    ├── state_machine.json             # Validates: state-machine-v1
    ├── animations/
    │   ├── idle.glb
    │   ├── walk.glb
    │   └── ...
    └── textures/
        ├── T_Knight_Albedo.png
        ├── T_Knight_Normal.png
        └── T_Knight_ARM.png
```

### Hybrid Mode (adds sprites/)

```
characters/Knight/
├── ... (all 3D output above)
├── sprites/                          # Blender-rendered 2D sheets
│   ├── idle.png                      # Horizontal strip from 3D render
│   ├── walk.png
│   └── ...
└── ivx/
    ├── ... (all 3D schemas above)
    └── sprites/                      # 2D sprite specs
        ├── idle_spec.json            # Validates: sprite-spec-v1
        └── ...
```

## SDK Schema Compliance

### character_3d.json (character-3d-meta-v1)

```json
{
  "id": "Knight",
  "display_name": "Knight",
  "type": "3d_rigged",
  "rarity": "common",
  "version": "1.0.0",
  "mesh": {
    "file": "Knight.glb",
    "poly_count": 8500,
    "vertex_count": 12000,
    "material_count": 2,
    "submeshes": ["body", "armor"]
  },
  "lods": [
    { "level": 0, "file": "Knight.glb", "poly_count": 8500, "screen_pct": 0.5 }
  ],
  "skeleton": {
    "file": "skeleton.json",
    "bone_count": 65,
    "root_bone": "Hips",
    "humanoid_mapped": true,
    "ik_chains": {
      "left_foot":  { "root": "LeftUpperLeg",  "tip": "LeftFoot",  "pole": "LeftKnee" },
      "right_foot": { "root": "RightUpperLeg", "tip": "RightFoot", "pole": "RightKnee" }
    },
    "attachment_points": {
      "right_hand_weapon": "RightHandSocket",
      "left_hand_shield": "LeftHandSocket"
    }
  },
  "textures": {
    "albedo": { "file": "textures/T_Knight_Albedo.png", "size": [2048, 2048], "srgb": true },
    "normal": { "file": "textures/T_Knight_Normal.png", "size": [2048, 2048], "srgb": false }
  },
  "materials": [
    { "name": "M_Knight_Body", "shader": "pbr_standard", "submesh": "body", "textures": ["albedo", "normal"] }
  ],
  "animations": {
    "idle":   { "file": "animations/idle.glb",   "duration_sec": 2.0, "loop": true,  "layer": "base" },
    "walk":   { "file": "animations/walk.glb",   "duration_sec": 1.0, "loop": true,  "layer": "base", "root_motion": true },
    "attack": { "file": "animations/attack.glb", "duration_sec": 0.8, "loop": false, "layer": "base" },
    "hurt":   { "file": "animations/hurt.glb",   "duration_sec": 0.5, "loop": false, "layer": "base" },
    "death":  { "file": "animations/death.glb",  "duration_sec": 1.8, "loop": false, "layer": "base" }
  },
  "state_machine": "state_machine.json",
  "bounds": { "center": [0, 1.0, 0], "size": [1.0, 2.0, 1.0] },
  "sounds": { "on_footstep": "sfx_footstep", "on_hurt": "sfx_character_hurt" },
  "tags": ["humanoid", "armored"]
}
```

## Mesh Generation Providers

| Provider | Model | Output | Strengths |
|----------|-------|--------|-----------|
| **Trellis** (via PiAPI) | Image/text → 3D | GLB | Stylized characters, game-ready topology |
| **Meshy** (via PiAPI) | Image/text → 3D | GLB | Realistic characters, higher detail |

## Rigging & Animation

| Stage | Tool | Input → Output |
|-------|------|---------------|
| **Mesh → Rig** | `MeshRigger` | GLB → FBX with skeleton hierarchy |
| **Rig → Animate** | `AnimationApplicator` | Rigged FBX + preset → animated FBX per action |
| **Text → Motion** | HY-Motion | Text prompt → FBX/BVH clip |
| **Video → Motion** | `VideoToMotionPipeline` | Video reference → motion capture FBX |
| **3D → 2D** | `BlenderSpriteRenderer` | Rigged model → 2D sprite sheet per action |

## Motion Library

Generate a full library of motion clips categorized by game genre:

```bash
curl -X POST http://localhost:8001/pipelines/motion_library \
  -d '{"game_id": "my-rpg", "game_type": "rpg", "ivx_export": true}'
```

Output: FBX clips + `motion_library.json` manifest with categories (idle, locomotion, combat, emote, interaction).

## Engine Import

| Engine | 3D Import | Animation Import |
|--------|----------|-----------------|
| **Unity** | FBX → Humanoid Rig → Avatar | FBX → AnimationClip → Animator Controller |
| **Unreal** | FBX → Skeletal Mesh → Skeleton | FBX → Animation Sequence → Anim Blueprint |
| **Godot** | GLB → MeshInstance3D → Skeleton3D | GLB → AnimationPlayer → AnimationTree |
| **Roblox** | GLB → MeshPart → Humanoid | Motor6D animation |

## Platform Notes

### VR
- Target 10K-15K polys for Quest standalone
- Include LOD1 at 5K polys for distance rendering
- Blend shapes critical for avatar expression in social VR

### Console
- PS5/Xbox: up to 50K polys, full PBR textures at 4K
- Switch: 8K-12K polys, 1K textures, simplified shaders

### Mobile
- 3K-8K polys depending on device tier
- Compress textures with ASTC (Android) / PVRTC (iOS)

## Checklist

- [ ] Content-Factory API accessible
- [ ] Character description or reference image provided
- [ ] Mode selected (`3d`, `hybrid`, or `2d`)
- [ ] Pipeline triggered with `ivx_export: true`
- [ ] `character_3d.json` validates against `character-3d-meta-v1`
- [ ] `skeleton.json` validates against `skeleton-v1`
- [ ] `state_machine.json` validates against `state-machine-v1`
- [ ] Mesh imported into target engine
- [ ] Rig mapped to engine's humanoid system
- [ ] Animation clips loaded and playing
- [ ] Blend shapes working (if applicable)
- [ ] Texture maps applied (albedo, normal, ARM)
- [ ] VR: poly count under budget, LODs generated
- [ ] Console: texture compression applied
- [ ] Mobile: mesh optimized for target poly budget
