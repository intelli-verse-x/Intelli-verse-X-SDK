# Environment Generator

**Skill ID:** `ivx-environment-generator`

---
name: ivx-environment-generator
description: >-
  AI-powered game environment and world asset generation: scene backgrounds, parallax layers,
  tilesets, skyboxes, world composition, and Unity/Unreal export. Use when the user says
  "generate environment", "create background", "parallax layers", "scene background",
  "tileset generation", "skybox", "world building", "level art", "terrain generation",
  "environment art", or needs AI-generated environments for any game engine.
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

The Environment Generator creates game-ready backgrounds, parallax layers, tilesets, skyboxes, and composed world scenes from text descriptions. Outputs follow the IntelliVerseX asset standard and include engine-specific import metadata for Unity, Unreal, Godot, and more.

```
Scene Description + Art Style
       │
       ▼
┌── Content-Factory ────────────────────────────────────┐
│  LLM Scene Planning → Image Gen (Fal/Stability)      │
│  → Layer Separation → Parallax Config → Tileset Cut   │
│  → World Composition → IVX Export → S3 Upload         │
└──────────────────────┬────────────────────────────────┘
                       │
           ┌───────────┼───────────┐
           ▼           ▼           ▼
      2D Parallax  Tilesets    3D Skybox
      Backgrounds  & Tiles     & HDRI
```

## Content-Factory API

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/environment \
  -H "Content-Type: application/json" \
  -d '{
    "game_id": "my-platformer",
    "scenes": [
      {
        "name": "enchanted_forest",
        "description": "A mystical forest with glowing mushrooms, ancient trees, and firefly particles",
        "style": "pixel_art",
        "mode": "parallax",
        "layers": 5
      },
      {
        "name": "crystal_cave",
        "description": "Underground cave with luminescent crystals and an underground river",
        "style": "pixel_art",
        "mode": "parallax",
        "layers": 4
      }
    ],
    "ivx_export": true
  }'
```

## Generated Output

### Parallax Background

```
environments/enchanted_forest/
├── layers/
│   ├── layer_0_sky.png           # Furthest (slowest scroll)
│   ├── layer_1_mountains.png
│   ├── layer_2_trees_far.png
│   ├── layer_3_trees_mid.png
│   └── layer_4_foreground.png    # Nearest (fastest scroll)
├── composed/
│   └── preview.png               # Full scene preview
└── ivx/
    ├── scene.json                # Scene metadata
    └── parallax_config.json      # Layer scroll speeds + offsets
```

### Tileset

```
environments/dungeon/
├── tileset.png                   # Sprite atlas (16×16 or 32×32 cells)
├── tile_rules.json               # Auto-tile rules (Wang tiles / bitmask)
├── composed/
│   └── sample_map.png
└── ivx/
    ├── tileset_spec.json         # Cell size, columns, rows, categories
    └── tile_rules.json           # Engine-importable auto-tile rules
```

### 3D Skybox / HDRI

```
environments/space_station/
├── skybox/
│   ├── skybox_front.png
│   ├── skybox_back.png
│   ├── skybox_left.png
│   ├── skybox_right.png
│   ├── skybox_top.png
│   └── skybox_bottom.png
├── hdri/
│   └── environment.hdr           # For PBR lighting
└── ivx/
    └── skybox_spec.json          # Face map, resolution, format
```

## Scene Metadata (scene.json)

```json
{
  "id": "enchanted_forest",
  "name": "Enchanted Forest",
  "description": "A mystical forest with glowing mushrooms",
  "style": "pixel_art",
  "mode": "parallax",
  "resolution": { "width": 1920, "height": 1080 },
  "layers": [
    { "name": "sky",            "file": "layers/layer_0_sky.png",           "scroll_speed": 0.1, "z_order": 0, "tiled_x": true },
    { "name": "mountains",      "file": "layers/layer_1_mountains.png",     "scroll_speed": 0.3, "z_order": 1, "tiled_x": true },
    { "name": "trees_far",      "file": "layers/layer_2_trees_far.png",     "scroll_speed": 0.5, "z_order": 2, "tiled_x": true },
    { "name": "trees_mid",      "file": "layers/layer_3_trees_mid.png",     "scroll_speed": 0.7, "z_order": 3, "tiled_x": true },
    { "name": "foreground",     "file": "layers/layer_4_foreground.png",    "scroll_speed": 1.0, "z_order": 4, "tiled_x": true }
  ],
  "lighting": {
    "ambient_color": "#1a2b3c",
    "light_sources": [
      { "type": "point", "color": "#44ff88", "intensity": 0.6, "position": [300, 400] }
    ]
  },
  "particles": [
    { "type": "fireflies", "density": 30, "color": "#88ffaa", "layer": 3 }
  ]
}
```

## Environment Modes

| Mode | Description | Output |
|------|-------------|--------|
| `parallax` | Multi-layer scrolling backgrounds | 3-7 PNG layers + scroll config |
| `tileset` | Tiling sprite atlas with auto-tile rules | Sprite atlas + Wang tile rules |
| `skybox` | 6-face cubemap for 3D games | 6 PNGs or 1 HDR |
| `panorama` | Wide panoramic background | Single wide image |
| `world` | Full composed world scene | Multiple backgrounds + object placements |

## Supported Styles

| Style | Best For |
|-------|----------|
| `pixel_art` | Platformers, retro games |
| `painterly` | RPGs, adventure games |
| `flat_vector` | Casual, puzzle games |
| `realistic` | Simulation, racing |
| `cartoon` | Kids games, platformers |
| `dark_gothic` | Horror, dark fantasy |
| `sci_fi` | Space games, cyberpunk |
| `low_poly` | 3D games, mobile-friendly |

## Engine Import

| Engine | Parallax | Tileset | Skybox |
|--------|----------|---------|--------|
| **Unity** | SpriteRenderer layers + scroll script | Tilemap + Rule Tile | Skybox material + 6-face cubemap |
| **Unreal** | PaperSprite layers + Blueprint scroll | TileMap UMG | Skybox sphere + material |
| **Godot** | ParallaxBackground + ParallaxLayer | TileMap + TileSet resource | WorldEnvironment + PanoramaSky |
| **Defold** | Parallax collection + go.animate | Tilemap component | Cubemap texture |
| **Roblox** | SurfaceGui layers on Parts | Terrain | Skybox Lighting properties |

## Platform Notes

### VR
- Skybox/HDRI mode strongly recommended for 360-degree immersion
- Generate at 4096×4096 per face for VR clarity
- Include reflection probes metadata for PBR

### Console
- 4K textures for PS5/Xbox, 1080p for Switch
- Pre-compose layers into single texture for Switch performance

### WebGL
- JPEG for backgrounds (smaller than PNG for photo-realistic)
- Limit parallax to 3 layers for mobile WebGL

### Mobile
- Max 1080p per layer for mid-range devices
- ASTC compression for Android, PVRTC for iOS
- Combine distant layers to reduce draw calls

## Checklist

- [ ] Content-Factory API accessible
- [ ] Scene descriptions provided with style and mode
- [ ] Pipeline triggered with `ivx_export: true`
- [ ] `scene.json` metadata generated
- [ ] All layer images have correct resolution
- [ ] Parallax scroll speeds create convincing depth
- [ ] Tilesets tile seamlessly (no visible seams)
- [ ] Skybox faces align at edges
- [ ] Assets imported into target engine
- [ ] Parallax scrolling working in-engine
- [ ] Lighting/particle hints applied
- [ ] VR: 4K+ skybox, stereo parallax if needed
- [ ] Console: texture budget verified
- [ ] WebGL: file sizes optimized
- [ ] Mobile: compression applied
