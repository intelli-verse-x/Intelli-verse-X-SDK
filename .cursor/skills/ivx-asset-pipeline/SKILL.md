---
name: ivx-asset-pipeline
description: >-
  Universal asset pipeline for game projects: 2D/3D character scaffolding, spritesheet
  generation, sound manifest management, and schema-based validation. Use when the user
  says "scaffold character", "create character", "generate spritesheet", "sprite sheet",
  "validate assets", "sound manifest", "character template", "3D character setup",
  "animation spec", "asset validation", "skeleton template", "state machine template",
  "blend shapes", "LOD setup", "asset standard", "asset pipeline", or needs help with
  any game asset creation, organization, or validation workflow.
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

# IntelliVerseX Asset Pipeline

## Overview

The Asset Pipeline provides a **Universal Asset Pipeline Standard** for any game project on any engine. It includes Python CLI tools for scaffolding 2D/3D characters, generating sprite sheets, and validating assets against JSON Schema definitions. All tools produce engine-agnostic output (PNG + JSON) that can be imported into Unity, Unreal, Godot, or any other engine.

```
tools/asset-pipeline/
├── scaffold_character.py      → Create 2D character folder + placeholders
├── generate_spritesheet.py    → Combine frames into sprite sheet + spec
├── validate_specs.py          → Validate sprite specs against schema
├── validate_character.py      → Full 2D character validation
├── validate_sound_manifest.py → Audio manifest validation
├── templates/                 → 10 copy-ready JSON templates
└── schemas/                   → 9 JSON Schema validation files
```

---

## 1. Requirements

```bash
pip install Pillow jsonschema
```

These are the only dependencies. All tools run with Python 3.8+.

---

## 2. 2D Character Scaffolding

### scaffold_character.py

Creates a complete character folder with placeholder PNGs, animation specs, and metadata — ready for artists to replace with real artwork.

```bash
# Minimal: mandatory animations only (idle, jump, hurt)
python tools/asset-pipeline/scaffold_character.py \
    --name Quizzy --output RemoteAssets/Characters/ --no-standard

# Standard: mandatory + standard animations (walk, run, attack)
python tools/asset-pipeline/scaffold_character.py \
    --name Quizzy --output RemoteAssets/Characters/

# Full: all tiers including extended (dance, wave, think, sleep, special, death, spawn)
python tools/asset-pipeline/scaffold_character.py \
    --name DragonLord --output RemoteAssets/Characters/ \
    --with-extended --rarity epic --tier 3
```

### Animation Tiers

| Tier | Animations | Required For |
|------|-----------|-------------|
| **Mandatory** | `idle`, `jump`, `hurt` | Every character |
| **Standard** | `walk`, `run`, `attack` | Playable characters |
| **Extended** | `dance`, `wave`, `think`, `sleep`, `special`, `death`, `spawn` | Premium/featured characters |

### Output Structure

```
Characters/Quizzy/
├── character.json          ← Metadata (rarity, tags, views, animations, sounds)
├── front.png               ← 512×512 front view placeholder
├── back.png                ← 512×512 back view placeholder
├── thumbnail.png           ← 128×128 thumbnail placeholder
└── sprites/
    ├── idle.png            ← Sprite sheet (3×2 grid, 6 frames)
    ├── idle_spec.json      ← Animation spec (fps, loop, layout, events)
    ├── jump.png
    ├── jump_spec.json
    ├── hurt.png
    ├── hurt_spec.json
    ├── walk.png            ← (if standard tier)
    ├── walk_spec.json
    └── ...
```

### character.json Format

```json
{
    "id": "Quizzy",
    "display_name": "Quizzy",
    "description": "Replace with Quizzy's description.",
    "rarity": "common",
    "unlock_method": "default",
    "unlock_cost": {},
    "tier": 1,
    "tags": [],
    "views": {
        "front": "front.png",
        "back": "back.png",
        "thumbnail": "thumbnail.png"
    },
    "animations": {
        "idle": { "tier": "mandatory", "frames": 6, "fps": 10 },
        "jump": { "tier": "mandatory", "frames": 4, "fps": 12 },
        "hurt": { "tier": "mandatory", "frames": 4, "fps": 10 }
    },
    "sounds": {
        "on_select": "sfx_character_select",
        "on_hurt": "sfx_character_hurt",
        "on_victory": "stinger_victory"
    }
}
```

---

## 3. Sprite Sheet Generation

### generate_spritesheet.py

Combines individual frame PNGs into a single optimized sprite sheet with companion `_spec.json`.

```bash
# Basic: combine idle frames into a sheet
python tools/asset-pipeline/generate_spritesheet.py \
    --frames "frames/idle_*.png" --output idle --cell-size 512

# Custom FPS and non-looping
python tools/asset-pipeline/generate_spritesheet.py \
    --frames "frames/death_*.png" --output death \
    --cell-size 256 --fps 12 --no-loop

# Ping-pong animation
python tools/asset-pipeline/generate_spritesheet.py \
    --frames "frames/dance_*.png" --output dance \
    --cell-size 512 --ping-pong
```

### Features

| Feature | Description |
|---------|------------|
| **Natural sort** | `frame_01`, `frame_02`, ... `frame_10` sorted correctly |
| **Auto layout** | Calculates optimal columns x rows grid |
| **Max dimension** | Warns if sheet exceeds 4096px (GPU texture limit) |
| **Auto resize** | Frames resized to `--cell-size` if dimensions don't match |
| **Spec output** | Companion `_spec.json` with frame count, FPS, layout, loop mode |

### Sprite Spec Format

```json
{
    "action": "idle",
    "frames": 6,
    "cell_size": 512,
    "fps": 10,
    "loop": true,
    "ping_pong": false,
    "layout": {
        "columns": 3,
        "rows": 2
    }
}
```

---

## 4. 3D Character Templates

### character_3d.json

Complete metadata template for rigged 3D characters:

| Section | Contents |
|---------|---------|
| **mesh** | File path, poly count, vertex count, material count, submeshes |
| **lods** | LOD levels with poly counts and screen percentage thresholds |
| **skeleton** | Bone count, root bone, humanoid mapping, IK chains, physics bones, attachment sockets |
| **blend_shapes** | Visemes (lip sync), expressions, corrective shapes |
| **textures** | Albedo, normal, ARM (ambient occlusion + roughness + metallic) with size and sRGB flags |
| **materials** | PBR material definitions with shader, submesh binding, and properties |
| **animations** | Clip definitions with duration, loop flag, layer, and root motion |
| **state_machine** | Reference to animation state machine JSON |
| **bounds** | Bounding box for culling and collision |
| **sounds** | Character sound event bindings |

### skeleton.json

Humanoid skeleton definition:

| Section | Contents |
|---------|---------|
| **bones** | Full bone hierarchy from Hips down (spine, arms, legs, fingers) |
| **humanoid_mapping** | Maps bone names to engine-standard humanoid slots |
| **ik_chains** | Left/right foot and hand IK with root, tip, and pole bones |
| **attachment_points** | Weapon, shield, helmet, and mount socket names |
| **finger_mapping** | Per-finger bone chain mapping for hand tracking / VR |

### state_machine.json

Animation state machine template:

| Section | Contents |
|---------|---------|
| **layers** | Base layer + override layers (upper body, face) |
| **states** | Idle, locomotion (blend tree), jump, fall, land, hit react, death |
| **transitions** | Condition-based transitions with crossfade durations |
| **blend_trees** | 1D speed-based locomotion blend (idle → walk → run) |
| **parameters** | Speed, IsGrounded, IsHit, IsDead, AttackTrigger |

### Workflow

```
1. Copy templates/character_3d.json, skeleton.json, state_machine.json
2. Model in Blender/Maya → Export model.glb with rig + LODs
3. Bake textures → T_{Id}_Albedo.png, T_{Id}_Normal.png, T_{Id}_ARM.png
4. Export animation clips → idle.glb, walk.glb, etc.
5. Fill in character_3d.json with actual poly counts, bone counts, durations
6. Validate against schemas
7. Upload to S3/CDN → Update manifest
```

---

## 5. Sound Manifest

### sound_manifest.json Template

Complete audio manifest covering all minimum required sounds for a game:

| Category | Required Sounds | Format |
|----------|----------------|--------|
| **UI** | button_click, button_hover, screen_transition, popup_open, popup_close, error, success, countdown_tick, countdown_finish | OGG/MP3 |
| **SFX** | character_select, correct_answer, wrong_answer, streak_increment, powerup_activate, powerup_expire, level_complete, coin_collect, xp_gain | OGG/MP3 |
| **Stingers** | victory, defeat, draw, achievement_unlock, streak_milestone, new_best | OGG/MP3 |
| **Music** | main_menu, gameplay_normal, gameplay_tense, results, tutorial | OGG/MP3 |
| **Ambient** | lobby, crowd_cheer | OGG/MP3 |
| **Voice** | announcer_ready, announcer_go, announcer_times_up | OGG/MP3 |

### Validation

```bash
# Schema compliance check
python tools/asset-pipeline/validate_sound_manifest.py \
    --manifest RemoteAssets/sound_manifest.json

# Also verify all audio URLs are accessible
python tools/asset-pipeline/validate_sound_manifest.py \
    --manifest RemoteAssets/sound_manifest.json --check-urls
```

---

## 6. Asset Validation

### validate_specs.py — Sprite Sheet Validation

```bash
python tools/asset-pipeline/validate_specs.py --directory RemoteAssets/Characters/
```

Checks:
- All `_spec.json` files match `schemas/sprite-spec-v1.json` schema
- PNG dimensions match spec (cell_size x columns, cell_size x rows)
- Frame count is consistent between spec and grid layout

### validate_character.py — Full Character Validation

```bash
# Single character
python tools/asset-pipeline/validate_character.py \
    --character RemoteAssets/Characters/Quizzy/

# All characters in directory
python tools/asset-pipeline/validate_character.py \
    --character RemoteAssets/Characters/ --all
```

Checks:
- `character.json` has all required fields
- Required views exist (front.png, thumbnail.png)
- All declared animations have sprite sheets + specs
- Mandatory animation tier is complete
- PNG dimensions are correct

---

## 7. JSON Schemas

### Available Schemas

| Schema | File | Validates |
|--------|------|-----------|
| **Sprite Spec** | `schemas/sprite-spec-v1.json` | `*_spec.json` animation specs |
| **2D Character** | `schemas/character-meta-v1.json` | 2D `character.json` metadata |
| **3D Character** | `schemas/character-3d-meta-v1.json` | 3D rigged character metadata |
| **Static Model** | `schemas/model-meta-v1.json` | Props, environment models |
| **Skeleton** | `schemas/skeleton-v1.json` | Bone hierarchy and humanoid mapping |
| **State Machine** | `schemas/state-machine-v1.json` | Animation state machines |
| **Sound Manifest** | `schemas/sound-manifest-v2.json` | Audio manifests |
| **Video Meta** | `schemas/video-meta-v1.json` | Video asset metadata |

### Using Schemas in CI

```yaml
# GitHub Actions example
- name: Validate game assets
  run: |
    pip install Pillow jsonschema
    python tools/asset-pipeline/validate_specs.py --directory assets/characters/
    python tools/asset-pipeline/validate_character.py --character assets/characters/ --all
    python tools/asset-pipeline/validate_sound_manifest.py --manifest assets/audio/sound_manifest.json
```

---

## 8. Cross-Engine Compatibility

All pipeline outputs are engine-agnostic (PNG + JSON). Import into any engine:

| Engine | Sprite Sheet Import | 3D Model Import | Sound Import |
|--------|-------------------|-----------------|-------------|
| **Unity** | `Sprite Editor` → slice by grid using spec | `ModelImporter` with character_3d.json | `AudioClip` from manifest URLs |
| **Unreal** | `Paper2D Flipbook` or `UMG Image` | FBX/glTF import with skeleton.json mapping | `USoundWave` from manifest |
| **Godot** | `AnimatedSprite2D` with `SpriteFrames` | glTF import with `AnimationTree` from state_machine.json | `AudioStream` from manifest |
| **Defold** | `Atlas` from sprite sheet | glTF support via extension | Defold sound component |
| **Cocos2d-x** | `SpriteFrameCache` from sheet | 3D model support in Cocos Creator | `AudioEngine` from manifest |
| **JavaScript** | Canvas/WebGL texture atlas | Three.js glTF loader | Web Audio API from URLs |
| **Roblox** | Decal/ImageLabel from sheet | MeshPart from model | Sound instance from URLs |

### Engine-Specific Importers

The SDK provides optional importer scripts for each engine:

```csharp
// Unity: auto-import from character.json
var character = IVXCharacterImporter.Import("RemoteAssets/Characters/Quizzy/character.json");
// Sets up AnimationClips, SpriteRenderer, and AudioSource references automatically
```

---

## 9. Integration with Other Skills

| Skill | Integration |
|-------|------------|
| **ivx-procedural-ai** | AI-generated characters use `scaffold_character.py` output format |
| **ivx-quiz-content** | Quiz character assets validated by the pipeline before upload |
| **ivx-ugc-pipeline** | User-created characters validated against schemas before publishing |
| **ivx-devops-cicd** | Asset validation runs as a CI step in build pipelines |
| **ivx-crashlytics** | Asset loading errors tracked with breadcrumbs |

---

## Platform-Specific Asset Considerations

### VR Platforms

| Asset Type | VR Guidance |
|-----------|------------|
| **3D Characters** | LOD 0 should be high quality (inspectable up close). Blend shapes required for face tracking on Quest Pro / Vision Pro. |
| **Textures** | 2048x2048 minimum for VR. Consider 4K for important characters. sRGB for albedo, linear for normal/ARM. |
| **Audio** | Spatial audio format. Include attenuation curves in sound manifest. |

### Console Platforms

| Asset Type | Console Guidance |
|-----------|-----------------|
| **Characters** | Console certification may require specific LOD thresholds. PS5: min 2 LOD levels. |
| **Textures** | BC7 compression for consoles. Include platform-specific texture variants. |
| **Audio** | Platform-specific audio formats (Opus for Switch, ATRAC9 for PlayStation). |

### Mobile / WebGL

| Asset Type | Mobile/Web Guidance |
|-----------|-------------------|
| **Sprite Sheets** | Max 2048x2048 for mobile GPU compatibility. Use `--cell-size 256` for mobile. |
| **3D Models** | Target < 10K triangles for mobile characters. 2 LOD levels minimum. |
| **Audio** | OGG Vorbis for Android/WebGL, AAC for iOS. Keep total audio under 50MB for mobile. |

---

## Checklist

- [ ] Python dependencies installed (`Pillow`, `jsonschema`)
- [ ] 2D character scaffolded with appropriate animation tier
- [ ] Artist has replaced all placeholder PNGs with real artwork
- [ ] Sprite sheets generated from individual frames
- [ ] `validate_character.py` passes for all characters
- [ ] `validate_specs.py` passes for all sprite specs
- [ ] Sound manifest created from template with all required categories
- [ ] `validate_sound_manifest.py` passes (with `--check-urls` if hosted)
- [ ] 3D character metadata filled in from template (if using 3D)
- [ ] Skeleton and state machine JSONs populated (if using 3D rigged)
- [ ] Asset validation added to CI pipeline
- [ ] VR LOD and blend shape requirements met (if targeting VR)
- [ ] Console texture compression configured (if targeting consoles)
- [ ] Mobile sprite sheet size within 2048x2048 (if targeting mobile)
