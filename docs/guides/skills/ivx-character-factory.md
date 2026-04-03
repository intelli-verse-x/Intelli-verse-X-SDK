# Character Factory

**Skill ID:** `ivx-character-factory`

---
name: ivx-character-factory
description: >-
  AI-powered 2D character generation for any game engine. Generates full sprite sheets,
  expression grids, emotional states, topic skins, and promo art — all validated against
  IntelliVerseX SDK schemas. Use when the user says "generate character", "create sprites",
  "character factory", "pixel art character", "expression sheet", "emotional states",
  "topic skins", "character variants", "AI character", "sprite generation",
  "generate game character", or needs AI-generated 2D game characters.
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



## Overview

The Character Factory uses the Content-Factory AI pipeline to generate production-ready 2D game characters from text descriptions. Every output conforms to IntelliVerseX SDK schemas (`character-meta-v1`, `sprite-spec-v1`) and is directly importable into Unity, Unreal, Godot, Defold, Roblox, or any engine that reads sprite sheets.

```
Text Description
       │
       ▼
┌── Content-Factory ──────────────────────────────┐
│  LLM prompt engineering → Fal Nano Banana Pro   │
│  → Per-cell rembg (isnet-anime) → Alpha harden  │
│  → IVX Schema Export → S3 Upload                │
└──────────────────────┬──────────────────────────┘
                       │
                       ▼
              SDK-Valid Output
     character.json  (character-meta-v1)
     sprites/*_spec.json  (sprite-spec-v1)
     sprites/*.png  (horizontal strips)
     expressions/expressions_front.png
     views/front.png, thumbnail.png
```

## Content-Factory API

### Trigger via MCP

```
trigger_pipeline character_2d --game_id my-game --character "A brave robot named Bolt with glowing blue eyes, pixel art style"
```

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/character_2d \
  -H "Content-Type: application/json" \
  -d '{
    "game_id": "my-game",
    "characters": [
      {
        "name": "Bolt",
        "description": "A brave robot with glowing blue eyes and a jetpack",
        "style": "pixel_art"
      }
    ],
    "ivx_export": true
  }'
```

### Trigger via Python

```python
from pipelines.animation.character_2d import Character2DAnimationPipeline

pipeline = Character2DAnimationPipeline()
result = await pipeline.run({
    "game_id": "my-game",
    "characters": [{"name": "Bolt", "description": "A brave robot..."}],
    "style": "pixel_art",
    "ivx_export": True,
})
```

## Generated Output

### Folder Structure

```
characters/Bolt/
├── sprites/
│   ├── idle.png              # 6-frame horizontal strip (3072×512)
│   ├── walk.png              # 6-frame strip
│   ├── run.png               # 6-frame strip
│   ├── jump.png              # 4-frame strip
│   ├── attack.png            # 6-frame strip
│   ├── hurt.png              # 3-frame strip
│   ├── death.png             # 4-frame strip
│   └── cast.png              # 5-frame strip
├── expressions/
│   └── expressions_front.png # 3×2 grid: neutral, happy, sad, angry, surprised, thinking
├── emotional_states/
│   ├── determined.png
│   ├── happy.png
│   ├── legendary.png
│   ├── proud.png
│   ├── sad.png
│   └── sleeping.png
├── promo/
│   ├── app_icon.png          # 1:1
│   ├── thumbnail.png         # 16:9
│   ├── key_art.png           # 16:9
│   └── social_banner.png     # 3:1
├── topic_skins/
│   ├── anime.png
│   ├── sport.png
│   └── holiday.png
└── ivx/                      # SDK-compliant exports
    ├── character.json         # Validates: character-meta-v1
    ├── sprites/
    │   ├── idle_spec.json     # Validates: sprite-spec-v1
    │   ├── walk_spec.json
    │   ├── run_spec.json
    │   ├── jump_spec.json
    │   ├── attack_spec.json
    │   ├── hurt_spec.json
    │   ├── death_spec.json
    │   └── cast_spec.json
    └── views/
        ├── front.png
        └── thumbnail.png
```

## SDK Schema Compliance

### character.json (character-meta-v1)

```json
{
  "id": "Bolt",
  "display_name": "Bolt",
  "description": "A brave robot with glowing blue eyes and a jetpack",
  "rarity": "common",
  "tier": 1,
  "tags": ["robot", "pixel_art"],
  "views": {
    "front": "views/front.png",
    "thumbnail": "views/thumbnail.png"
  },
  "animations": {
    "idle":   { "tier": "mandatory", "frames": 6, "fps": 10 },
    "walk":   { "tier": "standard",  "frames": 6, "fps": 10 },
    "run":    { "tier": "standard",  "frames": 6, "fps": 14 },
    "jump":   { "tier": "mandatory", "frames": 4, "fps": 12 },
    "attack": { "tier": "standard",  "frames": 6, "fps": 14 },
    "hurt":   { "tier": "mandatory", "frames": 3, "fps": 10 },
    "death":  { "tier": "extended",  "frames": 4, "fps": 10 },
    "cast":   { "tier": "extended",  "frames": 5, "fps": 12 }
  },
  "sounds": {
    "on_select": "sfx_character_select",
    "on_hurt": "sfx_character_hurt",
    "on_victory": "stinger_victory"
  }
}
```

### Sprite Spec (sprite-spec-v1)

```json
{
  "action": "idle",
  "frames": 6,
  "cell_size": 512,
  "fps": 10,
  "loop": true,
  "layout": { "columns": 6, "rows": 1 }
}
```

## IVX Export Module

The export layer transforms Content-Factory native output to SDK-compliant format:

```python
# content-factory/utils/ivx_exporter.py

TIER_MAP = {
    "idle": "mandatory", "walk": "standard", "run": "standard",
    "jump": "mandatory", "hurt": "mandatory", "attack": "standard",
    "death": "extended", "cast": "extended",
}
FPS_MAP = {
    "idle": 10, "walk": 10, "run": 14, "jump": 12,
    "hurt": 10, "attack": 14, "death": 10, "cast": 12,
}
LOOP_MAP = {
    "idle": True, "walk": True, "run": True, "jump": False,
    "attack": False, "hurt": False, "death": False, "cast": False,
}

def export_character_meta(name, description, sprites, views, style="pixel_art"):
    char_id = re.sub(r'[^A-Za-z0-9]', '', name)
    return {
        "id": char_id,
        "display_name": name,
        "description": description,
        "rarity": "common",
        "tier": 1,
        "tags": [style],
        "views": {
            "front": views.get("front", "views/front.png"),
            "thumbnail": views.get("thumbnail", views.get("front", "views/front.png")),
        },
        "animations": {
            action: {
                "tier": TIER_MAP.get(action, "extended"),
                "frames": data["frames"],
                "fps": FPS_MAP.get(action, 10),
            }
            for action, data in sprites.items()
        },
        "sounds": {},
    }

def export_sprite_spec(action, frames, cell_size=512):
    return {
        "action": action,
        "frames": frames,
        "cell_size": cell_size,
        "fps": FPS_MAP.get(action, 10),
        "loop": LOOP_MAP.get(action, False),
        "layout": {"columns": frames, "rows": 1},
    }
```

## Validation

After generation, validate with SDK tools:

```bash
python tools/asset-pipeline/validate_character.py --character characters/Bolt/ivx/ --all
python tools/asset-pipeline/validate_specs.py --directory characters/Bolt/ivx/sprites/
```

## Supported Styles

| Style | Prompt Modifier | Best For |
|-------|----------------|----------|
| `pixel_art` | "32-bit pixel art, clean edges" | Platformers, retro, indie |
| `cartoon` | "Cartoon style, bold outlines" | Casual, kids games |
| `anime` | "Anime style, cel-shaded" | RPGs, visual novels |
| `realistic` | "Semi-realistic, detailed" | Simulation, sports |
| `chibi` | "Chibi proportions, cute" | Gacha, idle games |
| `flat_vector` | "Flat design, minimal shading" | Puzzle, UI-heavy games |

## Frame Counts Per Action

| Action | Frames | Cell Size | Layout | Tier |
|--------|:------:|:---------:|:------:|------|
| idle | 6 | 512×512 | 6×1 | mandatory |
| walk | 6 | 512×512 | 6×1 | standard |
| run | 6 | 512×512 | 6×1 | standard |
| jump | 4 | 512×512 | 4×1 | mandatory |
| attack | 6 | 512×512 | 6×1 | standard |
| hurt | 3 | 512×512 | 3×1 | mandatory |
| death | 4 | 512×512 | 4×1 | extended |
| cast | 5 | 512×512 | 5×1 | extended |

## Engine Import

| Engine | Import Method |
|--------|--------------|
| **Unity** | Sprite Editor → slice by cell size → AnimationClip per strip |
| **Unreal** | Paper2D Flipbook → import strip → set frame count |
| **Godot** | AnimatedSprite2D → load strip → set hframes |
| **Defold** | Atlas → tile source from strip → set frame dimensions |
| **Roblox** | Decal/SurfaceGui → individual frames extracted from strip |
| **Cocos2d-x** | SpriteFrame → create from strip rect |

## Platform Notes

### VR
- Generate at 1024×1024 cell size for VR readability
- Include front + side + back views for spatial presence

### Console
- Texture budgets: Switch (512 max), PS5/Xbox (1024+ fine)
- Pre-generate topic skins for seasonal events

### WebGL
- Use 256×256 cells to reduce download size
- Compress PNGs with pngquant before deployment

### Mobile
- 512×512 is optimal for most mobile GPUs
- Generate @2x variants for high-DPI screens

## Checklist

- [ ] Content-Factory API accessible (local Docker or cloud)
- [ ] Character description provided with name and style
- [ ] Pipeline triggered with `ivx_export: true`
- [ ] `character.json` validates against `character-meta-v1`
- [ ] All `*_spec.json` validate against `sprite-spec-v1`
- [ ] Sprite strips have transparent backgrounds (alpha channel)
- [ ] Expression sheet has 6 emotions in 3×2 grid
- [ ] Emotional state PNGs generated (6 states)
- [ ] Promo assets generated (icon, thumbnail, key art, banner)
- [ ] Assets uploaded to S3 or local output directory
- [ ] Sprites imported into target engine
- [ ] AnimationClips/Flipbooks created from strips
- [ ] VR: cell size increased to 1024 if targeting Quest/VisionPro
- [ ] Console: texture budget verified
- [ ] WebGL: PNGs compressed
