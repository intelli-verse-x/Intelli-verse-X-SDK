# IntelliVerseX Universal Asset Pipeline Standard

**Version:** 2.0.0  
**Applies to:** All IntelliVerseX games — 2D, 3D, hybrid, any engine, any language  
**Last Updated:** 2026-04-02

---

## Overview

This is the **single source of truth** for how every game asset is named, structured, delivered, and validated. Whether you're making a 2D quiz game, a 3D RPG, or a Roblox experience — all assets follow this standard so that:

- CDN delivery, caching, and versioning work identically
- Runtime loaders on any engine consume the same JSON specs
- Artists, audio designers, and 3D modelers have one template to follow
- Automation tooling validates everything before deploy

**Verified against:** QuizVerse's `GameAssetManifestService` (CDN manifest, sprite sheets, sound manifest, video streaming, badges, leagues, progress HUD).

---

## Table of Contents

1. [Master CDN Folder Structure](#1-master-cdn-folder-structure)
2. [Master Game Manifest](#2-master-game-manifest)
3. [2D Sprite Sheets & Animation](#3-2d-sprite-sheets--animation)
4. [2D Character Standard](#4-2d-character-standard)
5. [3D Models — Static Props](#5-3d-models--static-props)
6. [3D Rigged Characters](#6-3d-rigged-characters)
7. [Textures — All Types](#7-textures--all-types)
8. [Sound & Music](#8-sound--music)
9. [Video & Cinematics](#9-video--cinematics)
10. [VFX & Particles](#10-vfx--particles)
11. [UI Assets](#11-ui-assets)
12. [Fonts & Typography](#12-fonts--typography)
13. [Localization](#13-localization)
14. [Game Config / Data Files](#14-game-config--data-files)
15. [Naming Convention Master Table](#15-naming-convention-master-table)
16. [Versioning & Cache Busting](#16-versioning--cache-busting)
17. [Platform Budget Guidelines](#17-platform-budget-guidelines)
18. [Runtime Integration (Multi-Engine)](#18-runtime-integration-multi-engine)
19. [Automation & Validation Tools](#19-automation--validation-tools)
20. [Checklists](#20-checklists)

---

## 1. Master CDN Folder Structure

All remote/streaming assets live under a game's S3/CloudFront bucket. Root: `RemoteAssets/`.

```
RemoteAssets/
├── manifest.json                          # Master game manifest (§2)
├── sound_manifest.json                    # Sound ID → URL mapping (§8)
│
├── Characters/                            # 2D sprite characters (§4)
│   └── {CharacterId}/
│       ├── character.json
│       ├── front.png
│       ├── back.png
│       ├── thumbnail.png
│       └── sprites/
│           ├── {anim}.png
│           └── {anim}_spec.json
│
├── Characters3D/                          # 3D rigged characters (§6)
│   └── {CharacterId}/
│       ├── character.json
│       ├── model.glb
│       ├── model_lod1.glb
│       ├── model_lod2.glb
│       ├── skeleton.json
│       ├── thumbnail.png
│       ├── textures/
│       │   ├── T_{Id}_Albedo.png
│       │   ├── T_{Id}_Normal.png
│       │   ├── T_{Id}_ARM.png
│       │   ├── T_{Id}_Emissive.png
│       │   └── T_{Id}_Mask.png
│       └── animations/
│           ├── {anim}.glb
│           └── {anim}_clip.json
│
├── Models/                                # 3D static props/environment (§5)
│   └── {ModelId}/
│       ├── model.glb
│       ├── model.json
│       └── textures/
│           └── T_{Id}_{MapType}.png
│
├── Sounds/                                # Audio (§8)
│   ├── Music/
│   │   └── music_{context}.mp3
│   ├── SFX/
│   │   ├── ui_{action}.wav
│   │   ├── sfx_{event}.wav
│   │   └── stinger_{outcome}.wav
│   ├── Ambient/
│   │   └── ambient_{scene}.wav
│   └── Voice/
│       └── {locale}/
│           └── {line_id}.mp3
│
├── Video/                                 # Streaming video (§9)
│   ├── {video_id}.mp4
│   └── {video_id}_meta.json
│
├── VFX/                                   # Particles & effects (§10)
│   ├── Sprites/
│   │   └── vfx_{effect}.png
│   ├── Sheets/
│   │   ├── vfx_{effect}.png
│   │   └── vfx_{effect}_spec.json
│   └── Textures/
│       └── vfx_{effect}_{type}.png
│
├── UI/                                    # UI chrome (§11)
│   ├── Icons/
│   │   └── ico_{name}.png
│   ├── Backgrounds/
│   │   └── bg_{name}.png
│   ├── Badges/
│   │   └── {pillar}/
│   │       └── badge_{name}.png
│   └── Emojis/
│       └── {emoji_set}/
│           └── {emoji_id}.png
│
├── Fonts/                                 # Font assets (§12)
│   ├── {FontFamily}.ttf
│   ├── {FontFamily}_sdf.json
│   └── {FontFamily}_atlas.png
│
├── Leagues/                               # League/rank icons
│   └── {tier_name}.png
│
├── Progress/                              # Progress HUD assets
│   └── {asset_name}.png
│
├── Localization/                          # Translations (§13)
│   ├── strings_{locale}.json
│   └── questions_{locale}.csv
│
└── Config/                                # Game data (§14)
    ├── {config_name}.json
    └── {catalog_name}.json
```

---

## 2. Master Game Manifest

The `manifest.json` is the entry point the runtime loader reads on launch. It registers **every** asset available on CDN.

```json
{
  "version": "3.0",
  "game_id": "quiz-verse",
  "base_url": "https://cdn.example.com/RemoteAssets/",
  "total_assets": 847,
  "total_size_mb": 125.4,
  "updated_at": "2026-04-02T10:00:00Z",

  "characters": {
    "Quizzy": {
      "views": {
        "front": "Characters/Quizzy/front.png?v=a1b2c3",
        "thumbnail": "Characters/Quizzy/thumbnail.png?v=d4e5f6"
      },
      "sprites": {
        "idle": {
          "spritesheet": "Characters/Quizzy/sprites/idle.png?v=abc123",
          "spec": "Characters/Quizzy/sprites/idle_spec.json?v=def456",
          "frames": 6,
          "cell_size": 512,
          "layout": "3x2"
        }
      }
    }
  },

  "characters_3d": {
    "Knight": {
      "model": "Characters3D/Knight/model.glb?v=abc",
      "lods": ["Characters3D/Knight/model_lod1.glb", "Characters3D/Knight/model_lod2.glb"],
      "skeleton": "Characters3D/Knight/skeleton.json",
      "textures": {
        "albedo": "Characters3D/Knight/textures/T_Knight_Albedo.png",
        "normal": "Characters3D/Knight/textures/T_Knight_Normal.png",
        "arm": "Characters3D/Knight/textures/T_Knight_ARM.png"
      },
      "animations": {
        "idle": "Characters3D/Knight/animations/idle.glb",
        "walk": "Characters3D/Knight/animations/walk.glb"
      }
    }
  },

  "models": {
    "treasure_chest": {
      "model": "Models/treasure_chest/model.glb",
      "textures": { "albedo": "Models/treasure_chest/textures/T_chest_Albedo.png" }
    }
  },

  "videos": {
    "intro_cinematic": {
      "url": "Video/intro_cinematic.mp4",
      "meta": "Video/intro_cinematic_meta.json"
    }
  },

  "badges": {
    "science": ["Badges/science/badge_novice.png", "Badges/science/badge_expert.png"]
  },

  "leagues": {
    "bronze": "Leagues/bronze.png",
    "silver": "Leagues/silver.png",
    "gold": "Leagues/gold.png"
  },

  "progress": {
    "streak_flame": "Progress/streak_flame.png",
    "xp_bar_fill": "Progress/xp_bar_fill.png"
  },

  "vfx": {
    "confetti": { "sheet": "VFX/Sheets/vfx_confetti.png", "spec": "VFX/Sheets/vfx_confetti_spec.json" },
    "sparkle": { "sprite": "VFX/Sprites/vfx_sparkle.png" }
  },

  "fonts": {
    "Montserrat": { "ttf": "Fonts/Montserrat.ttf", "sdf": "Fonts/Montserrat_sdf.json", "atlas": "Fonts/Montserrat_atlas.png" }
  },

  "localization": {
    "en": "Localization/strings_en.json",
    "es": "Localization/strings_es.json",
    "ar": "Localization/strings_ar.json"
  }
}
```

This matches QuizVerse's `GameAssetManifestService` patterns: `GetCharacterSpriteSheetUrl`, `GetCharacterViewUrl`, `GetBadgeAssetUrl`, `GetLeagueIconUrl`, `GetProgressAssetUrl`, `GetVideoUrl`, `GetAssetUrl`, `GetAudioUrl`.

---

## 3. 2D Sprite Sheets & Animation

### 3.1 Sprite Sheet Spec (`_spec.json`)

Every sprite sheet has a companion JSON spec — the contract between art and runtime.

```json
{
  "action": "idle",
  "frames": 6,
  "cell_size": 512,
  "fps": 10,
  "loop": true,
  "ping_pong": false,
  "layout": { "columns": 3, "rows": 2 },
  "hitbox": { "x": 128, "y": 64, "width": 256, "height": 384 },
  "events": [
    { "frame": 2, "name": "footstep" },
    { "frame": 4, "name": "dust_vfx" }
  ],
  "sound_sync": { "3": "sfx_sword_swing", "5": "sfx_impact" },
  "tags": ["combat", "melee"]
}
```

### 3.2 Field Reference

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `action` | string | yes | — | Must match filename (snake_case) |
| `frames` | int | yes | — | Total frame count |
| `cell_size` | int | yes | — | Width=height of each cell (px, square) |
| `fps` | float | no | 10 | Playback speed; 0 = use engine default |
| `loop` | bool | no | true | Loop after last frame |
| `ping_pong` | bool | no | false | Forward-backward looping |
| `layout.columns` | int | no | auto | Grid columns |
| `layout.rows` | int | no | auto | Grid rows |
| `hitbox` | object | no | null | Collision rect (x, y, w, h) in cell-local px |
| `events` | array | no | [] | Frame→event triggers |
| `sound_sync` | object | no | {} | Frame index → sound manifest ID |
| `tags` | array | no | [] | Searchable labels |

### 3.3 Layout Rules

- Frames fill **left→right**, then **top→bottom**
- Last row may have empty trailing cells
- Max sheet: **4096×4096** (mobile GPU safe)
- Power-of-two dimensions recommended

---

## 4. 2D Character Standard

### 4.1 Required Animations

| Tier | Animations | Ship-blocking? |
|------|-----------|----------------|
| **Mandatory** | `idle`, `jump`, `hurt` | Yes |
| **Standard** | `walk`, `run`, `attack` | Expected |
| **Extended** | `dance`, `wave`, `think`, `sleep`, `special`, `death`, `spawn` | Game-specific |

### 4.2 Character Metadata (`character.json`)

```json
{
  "id": "Quizzy",
  "display_name": "Quizzy",
  "description": "The curious quiz master mascot",
  "type": "2d",
  "rarity": "common",
  "unlock_method": "default",
  "unlock_cost": {},
  "tier": 1,
  "tags": ["mascot", "quiz"],
  "views": { "front": "front.png", "back": "back.png", "thumbnail": "thumbnail.png" },
  "animations": {
    "idle":   { "tier": "mandatory", "frames": 6, "fps": 10 },
    "jump":   { "tier": "mandatory", "frames": 4, "fps": 12 },
    "hurt":   { "tier": "mandatory", "frames": 4, "fps": 10 },
    "walk":   { "tier": "standard",  "frames": 8, "fps": 10 },
    "run":    { "tier": "standard",  "frames": 6, "fps": 14 },
    "attack": { "tier": "standard",  "frames": 5, "fps": 12 }
  },
  "sounds": {
    "on_select": "sfx_character_select",
    "on_hurt": "sfx_character_hurt",
    "on_victory": "stinger_victory"
  }
}
```

---

## 5. 3D Models — Static Props

### 5.1 Folder Structure

```
Models/{model_id}/
├── model.glb                    # Runtime GLTF binary (Draco-compressed)
├── model_lod1.glb               # LOD 1 (50% polys)
├── model_lod2.glb               # LOD 2 (25% polys)
├── model.json                   # Metadata
├── model_source.fbx             # Source (NOT deployed to CDN)
├── textures/
│   ├── T_{id}_Albedo.png        # Base color / diffuse
│   ├── T_{id}_Normal.png        # Tangent-space normal map
│   ├── T_{id}_ARM.png           # Packed: AO(R), Roughness(G), Metallic(B)
│   ├── T_{id}_Emissive.png      # Emission map (optional)
│   └── T_{id}_Height.png        # Displacement/parallax (optional)
└── collision/
    └── model_collision.glb      # Simplified collision mesh (optional)
```

### 5.2 Model Metadata (`model.json`)

```json
{
  "id": "treasure_chest",
  "name": "Treasure Chest",
  "type": "static_prop",
  "version": "1.0.0",
  "format": "glb",
  "poly_count": 3200,
  "vertex_count": 2400,
  "material_count": 1,
  "lod_levels": [
    { "level": 0, "file": "model.glb",      "poly_count": 3200 },
    { "level": 1, "file": "model_lod1.glb",  "poly_count": 1600 },
    { "level": 2, "file": "model_lod2.glb",  "poly_count": 800 }
  ],
  "textures": {
    "albedo":   { "file": "textures/T_chest_Albedo.png",   "size": [1024, 1024] },
    "normal":   { "file": "textures/T_chest_Normal.png",   "size": [1024, 1024] },
    "arm":      { "file": "textures/T_chest_ARM.png",      "size": [1024, 1024] }
  },
  "materials": [
    {
      "name": "M_Chest_Wood",
      "shader": "pbr_standard",
      "textures": ["albedo", "normal", "arm"],
      "properties": { "roughness_scale": 1.0, "metallic_scale": 0.0 }
    }
  ],
  "bounds": { "center": [0, 0.5, 0], "size": [1.2, 1.0, 0.8] },
  "collision": { "type": "box", "file": "collision/model_collision.glb" },
  "tags": ["prop", "loot", "interactive"],
  "pivot": "bottom_center"
}
```

---

## 6. 3D Rigged Characters

### 6.1 Folder Structure

```
Characters3D/{CharacterId}/
├── character.json               # Character + rig metadata
├── model.glb                    # Rigged mesh (LOD0) with bind pose
├── model_lod1.glb               # LOD 1
├── model_lod2.glb               # LOD 2
├── skeleton.json                # Bone hierarchy & IK chains
├── thumbnail.png                # 256×256 icon
├── textures/
│   ├── T_{Id}_Albedo.png
│   ├── T_{Id}_Normal.png
│   ├── T_{Id}_ARM.png
│   ├── T_{Id}_Emissive.png
│   └── T_{Id}_Mask.png          # Channel-packed detail mask (R=skin, G=cloth, B=metal, A=accent)
├── animations/
│   ├── idle.glb                 # Skeletal animation clips
│   ├── idle_clip.json           # Animation metadata
│   ├── walk.glb
│   ├── walk_clip.json
│   ├── run.glb
│   ├── attack_01.glb
│   ├── attack_combo.glb         # Multi-hit combo sequence
│   ├── hit_react.glb
│   ├── death.glb
│   ├── emote_wave.glb           # Social emotes
│   └── additive/
│       ├── aim_offset.glb       # Additive aim layer
│       └── breathing.glb        # Additive idle overlay
├── blend_shapes/
│   └── face_shapes.json         # Blend shape / morph target catalog
└── state_machine.json           # Animation state graph definition
```

### 6.2 Character Metadata (`character.json` — 3D)

```json
{
  "id": "Knight",
  "display_name": "Dark Knight",
  "type": "3d_rigged",
  "rarity": "epic",
  "version": "1.2.0",

  "mesh": {
    "file": "model.glb",
    "poly_count": 18500,
    "vertex_count": 14200,
    "material_count": 3,
    "submeshes": ["body", "armor", "weapon"]
  },

  "lods": [
    { "level": 0, "file": "model.glb",      "poly_count": 18500, "screen_pct": 0.5 },
    { "level": 1, "file": "model_lod1.glb",  "poly_count": 9000,  "screen_pct": 0.2 },
    { "level": 2, "file": "model_lod2.glb",  "poly_count": 3500,  "screen_pct": 0.05 }
  ],

  "skeleton": {
    "file": "skeleton.json",
    "bone_count": 67,
    "root_bone": "Hips",
    "humanoid_mapped": true,
    "ik_chains": {
      "left_foot":  { "root": "LeftUpperLeg",  "tip": "LeftFoot",  "pole": "LeftKnee" },
      "right_foot": { "root": "RightUpperLeg", "tip": "RightFoot", "pole": "RightKnee" },
      "left_hand":  { "root": "LeftUpperArm",  "tip": "LeftHand",  "pole": "LeftElbow" },
      "right_hand": { "root": "RightUpperArm", "tip": "RightHand", "pole": "RightElbow" }
    },
    "physics_bones": ["Cape_01", "Cape_02", "Cape_03", "HairTail_01", "HairTail_02"],
    "attachment_points": {
      "right_hand_weapon": "RightHandSocket",
      "left_hand_shield": "LeftHandSocket",
      "head_helmet": "HeadSocket",
      "back_mount": "SpineSocket"
    }
  },

  "blend_shapes": {
    "file": "blend_shapes/face_shapes.json",
    "groups": {
      "visemes": ["Viseme_AA", "Viseme_E", "Viseme_I", "Viseme_O", "Viseme_U",
                   "Viseme_CH", "Viseme_FF", "Viseme_TH", "Viseme_PP", "Viseme_SS"],
      "expressions": ["Brow_Up", "Brow_Down", "Brow_Angry",
                       "Eye_Blink_L", "Eye_Blink_R", "Eye_Wide",
                       "Mouth_Smile", "Mouth_Frown", "Mouth_Open",
                       "Jaw_Open", "Nose_Scrunch", "Cheek_Puff"],
      "correctives": ["UpperArm_Twist_L", "UpperArm_Twist_R"]
    }
  },

  "textures": {
    "albedo":   { "file": "textures/T_Knight_Albedo.png",   "size": [2048, 2048], "srgb": true },
    "normal":   { "file": "textures/T_Knight_Normal.png",   "size": [2048, 2048], "srgb": false },
    "arm":      { "file": "textures/T_Knight_ARM.png",      "size": [2048, 2048], "srgb": false,
                  "channels": { "R": "ambient_occlusion", "G": "roughness", "B": "metallic" } },
    "emissive": { "file": "textures/T_Knight_Emissive.png", "size": [1024, 1024], "srgb": true },
    "mask":     { "file": "textures/T_Knight_Mask.png",     "size": [1024, 1024], "srgb": false,
                  "channels": { "R": "skin_region", "G": "cloth_region", "B": "metal_region", "A": "accent_color" } }
  },

  "materials": [
    {
      "name": "M_Knight_Body",
      "shader": "pbr_standard",
      "submesh": "body",
      "textures": ["albedo", "normal", "arm", "mask"],
      "properties": { "tint_color": [1,1,1,1], "roughness_scale": 1.0 }
    },
    {
      "name": "M_Knight_Armor",
      "shader": "pbr_metallic",
      "submesh": "armor",
      "textures": ["albedo", "normal", "arm", "emissive"],
      "properties": { "emission_intensity": 2.0, "metallic_override": 0.9 }
    },
    {
      "name": "M_Knight_Weapon",
      "shader": "pbr_standard",
      "submesh": "weapon",
      "textures": ["albedo", "normal", "arm"]
    }
  ],

  "animations": {
    "idle":          { "file": "animations/idle.glb",          "duration_sec": 2.0,  "loop": true,  "layer": "base" },
    "walk":          { "file": "animations/walk.glb",          "duration_sec": 1.0,  "loop": true,  "layer": "base", "root_motion": true, "speed_curve": true },
    "run":           { "file": "animations/run.glb",           "duration_sec": 0.8,  "loop": true,  "layer": "base", "root_motion": true },
    "attack_01":     { "file": "animations/attack_01.glb",     "duration_sec": 0.9,  "loop": false, "layer": "base",
      "events": [
        { "time_sec": 0.25, "name": "sfx_swing" },
        { "time_sec": 0.4,  "name": "hitbox_on",  "data": { "damage": 25 } },
        { "time_sec": 0.6,  "name": "hitbox_off" },
        { "time_sec": 0.5,  "name": "vfx_slash_trail" }
      ]
    },
    "attack_combo":  { "file": "animations/attack_combo.glb",  "duration_sec": 2.2,  "loop": false, "layer": "base",
      "sections": [
        { "name": "hit_1", "start_sec": 0.0,  "end_sec": 0.7 },
        { "name": "hit_2", "start_sec": 0.7,  "end_sec": 1.4 },
        { "name": "hit_3", "start_sec": 1.4,  "end_sec": 2.2 }
      ]
    },
    "hit_react":     { "file": "animations/hit_react.glb",     "duration_sec": 0.5,  "loop": false, "layer": "base" },
    "death":         { "file": "animations/death.glb",         "duration_sec": 1.8,  "loop": false, "layer": "base" },
    "emote_wave":    { "file": "animations/emote_wave.glb",    "duration_sec": 2.0,  "loop": false, "layer": "override" },
    "aim_offset":    { "file": "animations/additive/aim_offset.glb",  "duration_sec": 0.0, "loop": true, "layer": "additive", "additive": true },
    "breathing":     { "file": "animations/additive/breathing.glb",   "duration_sec": 3.0, "loop": true, "layer": "additive", "additive": true, "weight": 0.3 }
  },

  "state_machine": "state_machine.json",

  "bounds": { "center": [0, 1.0, 0], "size": [1.0, 2.0, 1.0] },
  "sounds": {
    "on_select": "sfx_character_select",
    "on_hurt": "sfx_character_hurt",
    "on_death": "sfx_character_death",
    "on_footstep": "sfx_footstep_armor"
  },
  "tags": ["humanoid", "melee", "knight"]
}
```

### 6.3 Skeleton Definition (`skeleton.json`)

```json
{
  "format": "ivx_skeleton_v1",
  "root": "Hips",
  "bone_count": 67,
  "humanoid_mapping": {
    "hips": "Hips",
    "spine": "Spine", "chest": "Spine1", "upper_chest": "Spine2",
    "neck": "Neck", "head": "Head",
    "left_shoulder": "LeftShoulder", "left_upper_arm": "LeftUpperArm",
    "left_lower_arm": "LeftLowerArm", "left_hand": "LeftHand",
    "right_shoulder": "RightShoulder", "right_upper_arm": "RightUpperArm",
    "right_lower_arm": "RightLowerArm", "right_hand": "RightHand",
    "left_upper_leg": "LeftUpperLeg", "left_lower_leg": "LeftLowerLeg", "left_foot": "LeftFoot", "left_toes": "LeftToeBase",
    "right_upper_leg": "RightUpperLeg", "right_lower_leg": "RightLowerLeg", "right_foot": "RightFoot", "right_toes": "RightToeBase"
  },
  "finger_mapping": {
    "left_thumb": ["LeftHandThumb1", "LeftHandThumb2", "LeftHandThumb3"],
    "left_index": ["LeftHandIndex1", "LeftHandIndex2", "LeftHandIndex3"],
    "left_middle": ["LeftHandMiddle1", "LeftHandMiddle2", "LeftHandMiddle3"],
    "left_ring": ["LeftHandRing1", "LeftHandRing2", "LeftHandRing3"],
    "left_pinky": ["LeftHandPinky1", "LeftHandPinky2", "LeftHandPinky3"],
    "right_thumb": ["RightHandThumb1", "RightHandThumb2", "RightHandThumb3"],
    "right_index": ["RightHandIndex1", "RightHandIndex2", "RightHandIndex3"],
    "right_middle": ["RightHandMiddle1", "RightHandMiddle2", "RightHandMiddle3"],
    "right_ring": ["RightHandRing1", "RightHandRing2", "RightHandRing3"],
    "right_pinky": ["RightHandPinky1", "RightHandPinky2", "RightHandPinky3"]
  },
  "extra_bones": [
    { "name": "Cape_01", "parent": "Spine2", "type": "dynamic" },
    { "name": "Cape_02", "parent": "Cape_01", "type": "dynamic" },
    { "name": "Cape_03", "parent": "Cape_02", "type": "dynamic" },
    { "name": "HairTail_01", "parent": "Head", "type": "dynamic" },
    { "name": "HairTail_02", "parent": "HairTail_01", "type": "dynamic" },
    { "name": "RightHandSocket", "parent": "RightHand", "type": "socket" },
    { "name": "LeftHandSocket",  "parent": "LeftHand",  "type": "socket" },
    { "name": "HeadSocket",      "parent": "Head",      "type": "socket" },
    { "name": "SpineSocket",     "parent": "Spine2",    "type": "socket" }
  ]
}
```

### 6.4 Animation State Machine (`state_machine.json`)

```json
{
  "format": "ivx_state_machine_v1",
  "layers": [
    {
      "name": "base",
      "type": "override",
      "default_state": "idle",
      "states": {
        "idle":     { "clip": "idle",     "transitions": [
          { "to": "walk",  "condition": "speed > 0.1" },
          { "to": "attack_01", "trigger": "attack" },
          { "to": "hit_react", "trigger": "hit" },
          { "to": "death", "trigger": "die" }
        ]},
        "walk":     { "clip": "walk",     "transitions": [
          { "to": "idle",  "condition": "speed < 0.1" },
          { "to": "run",   "condition": "speed > 3.0" }
        ]},
        "run":      { "clip": "run",      "transitions": [
          { "to": "walk",  "condition": "speed < 3.0" },
          { "to": "idle",  "condition": "speed < 0.1" }
        ]},
        "attack_01":{ "clip": "attack_01", "transitions": [
          { "to": "attack_combo", "trigger": "attack", "window_sec": [0.5, 0.8] },
          { "to": "idle",  "on_complete": true }
        ]},
        "attack_combo": { "clip": "attack_combo", "transitions": [
          { "to": "idle",  "on_complete": true }
        ]},
        "hit_react":{ "clip": "hit_react", "transitions": [
          { "to": "idle",  "on_complete": true }
        ]},
        "death":    { "clip": "death",    "transitions": [] }
      },
      "blend_trees": {
        "locomotion": {
          "parameter": "speed",
          "clips": [
            { "clip": "idle", "threshold": 0.0 },
            { "clip": "walk", "threshold": 1.5 },
            { "clip": "run",  "threshold": 5.0 }
          ]
        }
      }
    },
    {
      "name": "additive",
      "type": "additive",
      "states": {
        "breathing": { "clip": "breathing", "weight": 0.3, "always_on": true },
        "aim":       { "clip": "aim_offset", "weight_param": "aim_weight" }
      }
    },
    {
      "name": "override",
      "type": "override",
      "mask": "upper_body",
      "states": {
        "emote_wave": { "clip": "emote_wave", "transitions": [
          { "to": null, "on_complete": true }
        ]}
      }
    }
  ],
  "parameters": {
    "speed":      { "type": "float",   "default": 0.0 },
    "aim_weight": { "type": "float",   "default": 0.0 },
    "attack":     { "type": "trigger" },
    "hit":        { "type": "trigger" },
    "die":        { "type": "trigger" }
  }
}
```

### 6.5 Required 3D Animations

| Tier | Animations | Notes |
|------|-----------|-------|
| **Mandatory** | `idle`, `walk`, `hit_react`, `death` | Ship-blocking |
| **Standard** | `run`, `attack_01`, `emote_wave`, `breathing` (additive) | Expected |
| **Extended** | `jump`, `fall`, `land`, `crouch_idle`, `crouch_walk`, `attack_combo`, `dodge_roll`, `aim_offset`, `swim`, `climb` | Game-specific |

---

## 7. Textures — All Types

### 7.1 Naming Convention

```
T_{AssetId}_{MapType}.{ext}

Map Types:
  Albedo           Base color / diffuse
  Normal           Tangent-space normal
  ARM              Packed: AO(R), Roughness(G), Metallic(B)
  Metallic         Standalone metallic (if not using ARM)
  Roughness        Standalone roughness (if not using ARM)
  AO               Ambient occlusion
  Emissive         Emission / glow
  Height           Displacement / parallax
  Mask             Channel-packed detail mask (game-specific channels)
  Opacity          Alpha/transparency (if not in Albedo alpha)
  Curvature        Curvature map for edge wear
  Subsurface       Subsurface scattering color (skin, wax)
  Thickness        Translucency thickness
```

### 7.2 Texture Profiles

| Profile | Use Case | Format | Color Space | Max Size (Mobile) | Max Size (Desktop) |
|---------|----------|--------|-------------|--------------------|--------------------|
| **PBR Standard** | 3D models | PNG | sRGB (albedo/emissive), Linear (others) | 1024² | 4096² |
| **Stylized / Toon** | Cel-shaded 3D | PNG | sRGB | 1024² | 2048² |
| **UI Sprite** | UI elements | PNG | sRGB | 512² | 2048² |
| **UI Atlas** | Sprite atlas packs | PNG | sRGB | 2048² | 4096² |
| **Particle / VFX** | Particle textures | PNG+Alpha | sRGB | 256² | 1024² |
| **Lightmap** | Baked lighting | EXR/HDR | Linear | 1024² | 4096² |
| **Cubemap / Skybox** | Environment | HDR/PNG | Linear | 512² per face | 2048² per face |
| **Icon** | App/UI icons | PNG | sRGB | 256² | 512² |
| **Thumbnail** | Preview images | PNG/JPEG | sRGB | 128² | 256² |

### 7.3 Channel Packing Standard

To reduce texture count, pack grayscale maps into RGB(A) channels:

| Pack Name | R | G | B | A |
|-----------|---|---|---|---|
| **ARM** | Ambient Occlusion | Roughness | Metallic | — |
| **MRAO** | Metallic | Roughness | AO | — |
| **Detail Mask** | Region 1 | Region 2 | Region 3 | Region 4 |
| **Flow Map** | Direction X | Direction Y | Speed | — |

### 7.4 Texture Compression Recommendations

| Platform | Opaque | With Alpha | Normal Maps | HDR |
|----------|--------|------------|-------------|-----|
| **Android** | ETC2 RGB | ETC2 RGBA | ETC2 RG | ASTC 6×6 |
| **iOS** | ASTC 6×6 | ASTC 6×6 | ASTC 6×6 | ASTC 6×6 |
| **WebGL** | Basis Universal | Basis Universal | Basis Universal | — |
| **Desktop** | BC7 / DXT5 | BC7 | BC5 | BC6H |
| **Console** | BC7 | BC7 | BC5 | BC6H |

---

## 8. Sound & Music

(Unchanged from v1 — see Sound Manifest Schema, Sound ID conventions, audio format requirements, and minimum sound set in the [schemas](#19-automation--validation-tools).)

### Quick Reference

```
Sound ID format:  {category}_{descriptive_name}
Categories:       ui_, sfx_, music_, stinger_, ambient_, notif_, voice_, reward_, streak_
Manifest file:    sound_manifest.json (see schemas/sound-manifest-v2.json)
Formats:          WAV (SFX), MP3 (music/voice), OGG (ambient)
```

---

## 9. Video & Cinematics

QuizVerse uses **URL-streamed video** via `VideoPlayer` — no bundled video files.

### 9.1 Video Metadata (`{video_id}_meta.json`)

```json
{
  "id": "intro_cinematic",
  "title": "Welcome to QuizVerse",
  "url": "Video/intro_cinematic.mp4",
  "format": "mp4",
  "codec": "h264",
  "resolution": { "width": 1920, "height": 1080 },
  "duration_sec": 32.5,
  "file_size_mb": 18.2,
  "bitrate_kbps": 4500,
  "audio_codec": "aac",
  "audio_channels": 2,
  "has_alpha": false,
  "loop": false,
  "render_texture_size": [1920, 1080],
  "adaptive_streams": [
    { "quality": "1080p", "url": "Video/intro_cinematic_1080p.mp4", "bitrate_kbps": 4500 },
    { "quality": "720p",  "url": "Video/intro_cinematic_720p.mp4",  "bitrate_kbps": 2500 },
    { "quality": "480p",  "url": "Video/intro_cinematic_480p.mp4",  "bitrate_kbps": 1200 }
  ],
  "subtitles": {
    "en": "Video/subs/intro_cinematic_en.vtt",
    "es": "Video/subs/intro_cinematic_es.vtt"
  },
  "tags": ["intro", "cinematic", "skippable"]
}
```

### 9.2 Video Format Requirements

| Type | Codec | Resolution | Bitrate | Max Duration | Max Size |
|------|-------|-----------|---------|--------------|----------|
| Cinematic | H.264 / H.265 | 1080p | 4–8 Mbps | 5 min | 300 MB |
| Background loop | H.264 | 720p | 2 Mbps | 30 sec | 10 MB |
| Tutorial clip | H.264 | 720p | 2 Mbps | 60 sec | 15 MB |
| Question media | H.264 | 480p | 1.2 Mbps | 15 sec | 3 MB |

---

## 10. VFX & Particles

### 10.1 VFX Asset Types

| Type | Files | Description |
|------|-------|-------------|
| **Single sprite** | `vfx_{name}.png` | One-shot particle image |
| **Sprite sheet** | `vfx_{name}.png` + `vfx_{name}_spec.json` | Animated particle (uses same spec as §3) |
| **Flipbook texture** | `vfx_{name}_flipbook.png` | Grid of frames for GPU particles |
| **Gradient texture** | `vfx_{name}_gradient.png` | Color ramp for lifetime color |
| **Noise texture** | `vfx_{name}_noise.png` | Noise/distortion for effects |
| **3D VFX mesh** | `vfx_{name}.glb` | Mesh-based VFX (slashes, trails) |

### 10.2 Naming Convention

```
vfx_{effect_category}_{descriptive_name}

Categories:
  vfx_hit_       → Impact/damage effects
  vfx_env_       → Environmental (rain, dust, fog)
  vfx_ui_        → UI particles (confetti, sparkles)
  vfx_proj_      → Projectile trails
  vfx_spell_     → Magic/ability effects
  vfx_status_    → Status effects (burn, freeze, poison)
```

---

## 11. UI Assets

### 11.1 Icon Naming

```
ico_{category}_{name}.png

Categories:
  ico_nav_       → Navigation (back, home, settings)
  ico_action_    → Actions (play, share, copy)
  ico_social_    → Social (friends, chat, leaderboard)
  ico_reward_    → Rewards (coin, gem, xp, star)
  ico_status_    → Status (online, offline, locked)
  ico_mode_      → Game mode icons
```

### 11.2 Badge Convention (matches QuizVerse `GetBadgeAssetUrl`)

```
Badges/{pillar}/{badge_name}.png

Pillar examples: science, history, geography, entertainment, sports
Badge naming:    badge_{tier}.png (badge_novice, badge_expert, badge_master)
Dimensions:      128×128 (standard), 256×256 (detail view)
```

### 11.3 Background Naming

```
bg_{context}_{variant}.png

Examples: bg_menu_default.png, bg_gameplay_night.png, bg_profile_gradient.png
```

---

## 12. Fonts & Typography

### 12.1 Font Bundle Structure

```
Fonts/{FontFamily}/
├── {FontFamily}-Regular.ttf         # Primary weight
├── {FontFamily}-Bold.ttf            # Bold weight
├── {FontFamily}-Italic.ttf          # Italic (optional)
├── {FontFamily}_sdf.json            # SDF generation parameters
├── {FontFamily}_atlas_latin.png     # SDF atlas — Latin characters
├── {FontFamily}_atlas_cjk.png       # SDF atlas — CJK (optional)
├── {FontFamily}_atlas_arabic.png    # SDF atlas — Arabic (optional)
└── font_meta.json                   # Font metadata
```

### 12.2 Font Metadata (`font_meta.json`)

```json
{
  "family": "Montserrat",
  "category": "sans-serif",
  "weights": ["Regular", "Bold", "Italic"],
  "scripts_supported": ["latin", "latin_extended", "cyrillic", "vietnamese"],
  "sdf_settings": {
    "atlas_resolution": 4096,
    "padding": 5,
    "point_size": 90,
    "packing_method": "optimum",
    "render_mode": "sdf"
  },
  "fallback_chain": ["NotoSans", "NotoSansArabic", "NotoSansCJK"],
  "license": "OFL-1.1"
}
```

---

## 13. Localization

### 13.1 String Tables (`strings_{locale}.json`)

```json
{
  "locale": "en",
  "version": "2.1",
  "strings": {
    "menu.play": "Play",
    "menu.settings": "Settings",
    "quiz.correct": "Correct!",
    "quiz.wrong": "Wrong answer",
    "reward.claim": "Claim Reward",
    "character.{id}.name": "Quizzy",
    "character.{id}.description": "The quiz master mascot"
  }
}
```

### 13.2 Locale Codes

Follow BCP-47: `en`, `es`, `fr`, `de`, `pt-BR`, `ar`, `hi`, `ja`, `ko`, `zh-CN`, `zh-TW`, `ru`, `id`, `tr`, `th`, `vi`.

---

## 14. Game Config / Data Files

### 14.1 Remote Config Structure

```
Config/
├── game_config.json          # Feature flags, tuning values
├── scoring_config.json       # Score formulas, multipliers
├── iap_catalog.json          # In-app purchase definitions
├── season_config.json        # Season/battle pass definition
└── matchmaking_config.json   # Matchmaking parameters
```

### 14.2 Config Metadata

```json
{
  "config_id": "game_config",
  "version": "3.2.0",
  "min_client_version": "5.0.0",
  "schema_version": 1,
  "updated_at": "2026-04-02T10:00:00Z",
  "data": { }
}
```

---

## 15. Naming Convention Master Table

### Files

| Asset Type | Pattern | Example |
|-----------|---------|---------|
| 2D Sprite sheet | `{anim}.png` | `idle.png` |
| 2D Sprite spec | `{anim}_spec.json` | `idle_spec.json` |
| 2D Character meta | `character.json` | — |
| 3D Model | `model.glb` | — |
| 3D Model meta | `model.json` | — |
| 3D Character meta | `character.json` | — |
| 3D Skeleton | `skeleton.json` | — |
| 3D State machine | `state_machine.json` | — |
| 3D Animation clip | `{anim}.glb` + `{anim}_clip.json` | `walk.glb` |
| Texture (PBR) | `T_{Id}_{MapType}.png` | `T_Knight_Albedo.png` |
| VFX sprite | `vfx_{effect}.png` | `vfx_hit_spark.png` |
| Sound (UI) | `ui_{action}.wav` | `ui_button_click.wav` |
| Sound (SFX) | `sfx_{event}.wav` | `sfx_coin_collect.wav` |
| Sound (Music) | `music_{context}.mp3` | `music_menu.mp3` |
| Video | `{video_id}.mp4` | `intro_cinematic.mp4` |
| Video meta | `{video_id}_meta.json` | — |
| Font | `{Family}-{Weight}.ttf` | `Montserrat-Bold.ttf` |
| Font SDF atlas | `{Family}_atlas_{script}.png` | `Montserrat_atlas_latin.png` |
| Icon | `ico_{cat}_{name}.png` | `ico_nav_back.png` |
| Badge | `badge_{tier}.png` | `badge_expert.png` |
| Background | `bg_{context}_{variant}.png` | `bg_menu_default.png` |
| Localization | `strings_{locale}.json` | `strings_es.json` |
| Config | `{config_name}.json` | `game_config.json` |
| League icon | `{tier}.png` | `gold.png` |
| Progress HUD | `{asset_name}.png` | `streak_flame.png` |

### IDs

| Type | Convention | Example |
|------|-----------|---------|
| 2D Character | PascalCase | `Quizzy`, `DragonLord` |
| 3D Character | PascalCase | `Knight`, `ShadowMage` |
| Animation | snake_case | `idle`, `attack_combo`, `emote_wave` |
| Sound ID | category_snake_case | `sfx_coin_collect`, `music_menu` |
| Model | snake_case | `treasure_chest`, `wall_torch` |
| Texture | `T_` + PascalCase + `_MapType` | `T_Knight_ARM` |
| VFX | `vfx_` + snake_case | `vfx_hit_spark` |
| Video | snake_case | `intro_cinematic` |
| Config | snake_case | `game_config` |
| Locale | BCP-47 | `en`, `pt-BR`, `zh-CN` |

---

## 16. Versioning & Cache Busting

All manifest URLs include content-hash query parameters:

```
Characters/Quizzy/sprites/idle.png?v=a1b2c3d4
```

- When artists update an asset, the build pipeline re-hashes and bumps `?v=`
- Runtime loader's disk cache TTL: **30 days** (matches QuizVerse)
- Memory cache: **LRU with 50 MB cap** (matches QuizVerse)
- Manifest cache TTL: **24 hours** with background refresh

---

## 17. Platform Budget Guidelines

### 2D Characters

| Platform | Max Sheet Size | Max Cell Size | Max Animations |
|----------|---------------|--------------|----------------|
| Mobile | 4096×4096 | 512×512 | 15 |
| WebGL | 4096×4096 | 512×512 | 20 |
| Desktop | 8192×8192 | 1024×1024 | 40 |

### 3D Characters

| Platform | Max Polys | Max Bones | Max Texture Size | Max Anim Clips | Max Blend Shapes |
|----------|----------|-----------|-----------------|----------------|-----------------|
| Mobile | 15,000 | 50 | 1024² | 10 | 20 |
| WebGL | 25,000 | 65 | 2048² | 15 | 30 |
| Desktop | 50,000 | 100 | 4096² | 30 | 60 |
| Console | 100,000 | 150 | 4096² | 50 | 100 |

### 3D Static Props

| Platform | Max Polys | Max Texture Size | Max Materials |
|----------|----------|-----------------|---------------|
| Mobile | 5,000 | 512² | 1 |
| WebGL | 10,000 | 1024² | 2 |
| Desktop | 50,000 | 2048² | 4 |
| Console | 100,000 | 4096² | 8 |

### Audio

| Platform | Max Simultaneous | Music Format | SFX Format | Max Total Audio MB |
|----------|-----------------|-------------|-----------|-------------------|
| Mobile | 8 channels | MP3 128kbps | WAV 22kHz | 50 MB |
| WebGL | 6 channels | MP3 128kbps | WAV 22kHz | 30 MB |
| Desktop | 32 channels | MP3 320kbps | WAV 44.1kHz | 200 MB |
| Console | 64 channels | OGG Q8 | WAV 48kHz | 500 MB |

---

## 18. Runtime Integration (Multi-Engine)

### Unity

```csharp
// 2D Sprite Animation
var animator = GetComponent<CharacterSpriteAnimator>();
animator.CharacterNameProp = "Quizzy";
animator.Play("idle");
animator.PlayOnce("jump", onDone: () => animator.Play("idle"));

// 3D Character (future SDK)
var char3D = GetComponent<IVXCharacter3DController>();
char3D.LoadCharacter("Knight");
char3D.SetTrigger("attack");

// Video streaming (matches QuizVerse pattern)
videoPlayer.url = GameAssetManifestService.Instance.GetVideoUrl("intro_cinematic.mp4");

// Badges (matches QuizVerse pattern)
string url = GameAssetManifestService.Instance.GetBadgeAssetUrl("science", "badge_expert.png");

// Leagues (matches QuizVerse pattern)
string url = GameAssetManifestService.Instance.GetLeagueIconUrl("gold");
```

### Unreal Engine 5

```cpp
auto* Loader = UIVXAssetLoader::Get();
Loader->LoadCharacter3D(TEXT("Knight"), [](UIVXCharacter3D* Char) {
    Char->PlayAnimation(TEXT("idle"));
});
```

### Godot 4

```gdscript
var loader = IVXAssetLoader.instance
var knight = await loader.load_character_3d("Knight")
knight.play("idle")
```

### JavaScript / TypeScript

```typescript
const loader = new IVXAssetLoader({ cdnBase: 'https://cdn.example.com/RemoteAssets/' });
const knight = await loader.loadCharacter3D('Knight');
knight.play('idle');

const sprite = new IVXSpriteAnimator(canvas, { character: 'Quizzy' });
await sprite.preload(['idle', 'jump']);
sprite.play('idle');
```

### Roblox Luau

```lua
local IVX = require(game.ReplicatedStorage.IVXLoader)
local char = IVX:LoadCharacter("Knight")
char:PlayAnimation("idle")
```

### Flutter / Dart

```dart
final loader = IVXAssetLoader(cdnBase: 'https://cdn.example.com/RemoteAssets/');
final animator = IVXSpriteAnimator(character: 'Quizzy');
await animator.preload(['idle', 'jump']);
animator.play('idle');
```

---

## 19. Automation & Validation Tools

All tools live in `tools/asset-pipeline/`.

| Tool | Description | Command |
|------|-------------|---------|
| `scaffold_character.py` | Generate full 2D character folder from template | `python scaffold_character.py --name Quizzy --output RemoteAssets/Characters/` |
| `generate_spritesheet.py` | Combine frame PNGs into sheet + spec | `python generate_spritesheet.py --frames "frames/idle_*.png" --output idle` |
| `validate_specs.py` | Validate all `_spec.json` against schema | `python validate_specs.py --directory RemoteAssets/Characters/` |
| `validate_sound_manifest.py` | Validate sound manifest | `python validate_sound_manifest.py --manifest sound_manifest.json` |
| `validate_character.py` | Full 2D character folder validation | `python validate_character.py --character RemoteAssets/Characters/Quizzy/` |

### JSON Schemas (in `tools/asset-pipeline/schemas/`)

| Schema | Validates |
|--------|-----------|
| `sprite-spec-v1.json` | 2D `_spec.json` files |
| `sound-manifest-v2.json` | `sound_manifest.json` |
| `character-meta-v1.json` | 2D `character.json` |
| `character-3d-meta-v1.json` | 3D `character.json` |
| `model-meta-v1.json` | Static prop `model.json` |
| `skeleton-v1.json` | 3D `skeleton.json` |
| `state-machine-v1.json` | `state_machine.json` |
| `video-meta-v1.json` | Video `_meta.json` |
| `game-manifest-v3.json` | Master `manifest.json` |

---

## 20. Checklists

### New 2D Character

- [ ] `character.json` with type `"2d"`
- [ ] `front.png` (512×512), `thumbnail.png` (128×128)
- [ ] Tier 1 animations: `idle`, `jump`, `hurt` (sheet + spec each)
- [ ] All specs pass `validate_specs.py`
- [ ] Passes `validate_character.py`
- [ ] Sound IDs exist in `sound_manifest.json`
- [ ] Added to `manifest.json` characters section

### New 3D Rigged Character

- [ ] `character.json` with type `"3d_rigged"`
- [ ] `model.glb` (bind pose, rigged mesh)
- [ ] LODs: `model_lod1.glb`, `model_lod2.glb`
- [ ] `skeleton.json` with humanoid mapping
- [ ] `state_machine.json` with locomotion + combat states
- [ ] Textures: Albedo, Normal, ARM at minimum
- [ ] Tier 1 animations: `idle`, `walk`, `hit_react`, `death`
- [ ] `thumbnail.png` (256×256)
- [ ] Poly count within platform budget
- [ ] Bone count within platform budget
- [ ] Added to `manifest.json` characters_3d section

### New 3D Static Model

- [ ] `model.glb` + `model.json`
- [ ] LODs if > 5000 polys
- [ ] PBR textures (Albedo + Normal + ARM minimum)
- [ ] Texture naming: `T_{id}_{MapType}.png`
- [ ] Collision mesh if interactive
- [ ] Within platform poly budget

### New Sound Set

- [ ] All minimum UI/SFX/stinger/music/ambient sounds present
- [ ] Format matches requirements (sample rate, bit depth, channels)
- [ ] `sound_manifest.json` updated
- [ ] Passes `validate_sound_manifest.py`

### New Video

- [ ] `{id}.mp4` + `{id}_meta.json`
- [ ] Adaptive streams (1080p, 720p, 480p) if cinematic
- [ ] Subtitles for supported locales
- [ ] Duration within type limit

### New Locale

- [ ] `strings_{locale}.json` with all keys from `strings_en.json`
- [ ] RTL flag set if applicable (ar, he, fa)
- [ ] Font SDF atlas for script coverage
- [ ] Registered in `manifest.json` localization section
