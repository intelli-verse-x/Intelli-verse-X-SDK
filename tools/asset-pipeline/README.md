# IntelliVerseX Asset Pipeline Tools

Automation tooling for the [Universal Asset Pipeline Standard v2.0](../../docs/guides/asset-pipeline-standard.md).

Covers **all** asset types for any 2D or 3D game: sprites, 3D models, rigged characters, textures, sounds, video, VFX, fonts, localization, and configs.

## Requirements

```bash
pip install Pillow jsonschema
```

## Tools

### `scaffold_character.py` — Create a new 2D character from templates

Generates a complete character folder with placeholder PNGs, spec files, and `character.json`.

```bash
# Minimal (mandatory animations only)
python scaffold_character.py --name Quizzy --output RemoteAssets/Characters/ --no-standard

# Full (mandatory + standard)
python scaffold_character.py --name Quizzy --output RemoteAssets/Characters/

# Complete (all tiers including extended)
python scaffold_character.py --name Quizzy --output RemoteAssets/Characters/ --with-extended --rarity epic --tier 3
```

### `generate_spritesheet.py` — Combine frames into a sprite sheet

Takes individual frame PNGs and produces a single sprite sheet + `_spec.json`.

```bash
python generate_spritesheet.py --frames "frames/idle_*.png" --output idle --cell-size 512 --fps 10
```

### `validate_specs.py` — Validate sprite sheet specs

Checks all `_spec.json` files against the schema and verifies PNG dimensions.

```bash
python validate_specs.py --directory RemoteAssets/Characters/
```

### `validate_sound_manifest.py` — Validate sound manifest

Checks `sound_manifest.json` for schema compliance, required sounds, and optionally verifies URLs.

```bash
python validate_sound_manifest.py --manifest RemoteAssets/sound_manifest.json
python validate_sound_manifest.py --manifest RemoteAssets/sound_manifest.json --check-urls
```

### `validate_character.py` — Validate a complete 2D character

Full validation of a character folder: metadata, views, all animation tiers.

```bash
python validate_character.py --character RemoteAssets/Characters/Quizzy/
python validate_character.py --character RemoteAssets/Characters/ --all
```

## Templates

Copy-ready template files in `templates/`:

### 2D Characters

| File | Purpose |
|------|---------|
| `character.json` | 2D character metadata template |
| `idle_spec.json` | Idle animation spec |
| `jump_spec.json` | Jump animation spec |
| `hurt_spec.json` | Hurt animation spec |
| `walk_spec.json` | Walk animation spec |
| `attack_spec.json` | Attack animation spec |

### 3D Rigged Characters

| File | Purpose |
|------|---------|
| `character_3d.json` | 3D rigged character metadata (mesh, skeleton, LODs, blend shapes, textures, materials, animations) |
| `skeleton.json` | Humanoid skeleton definition (bone hierarchy, IK chains, attachment sockets, finger mapping) |
| `state_machine.json` | Animation state machine (layers, states, transitions, blend trees, parameters) |

### Audio

| File | Purpose |
|------|---------|
| `sound_manifest.json` | Complete sound manifest with all minimum required sounds |

## Schemas

JSON Schema validation files in `schemas/`:

### 2D Assets

| Schema | Validates |
|--------|-----------|
| `sprite-spec-v1.json` | Sprite sheet `_spec.json` files |
| `character-meta-v1.json` | 2D `character.json` files |

### 3D Assets

| Schema | Validates |
|--------|-----------|
| `character-3d-meta-v1.json` | 3D rigged `character.json` (mesh, skeleton, blend shapes, textures, animations) |
| `model-meta-v1.json` | Static prop `model.json` (mesh, LODs, textures, collision) |
| `skeleton-v1.json` | Skeleton `skeleton.json` (bone hierarchy, humanoid mapping, IK) |
| `state-machine-v1.json` | Animation state machine (layers, states, blend trees, parameters) |

### Audio & Video

| Schema | Validates |
|--------|-----------|
| `sound-manifest-v2.json` | `sound_manifest.json` files |
| `video-meta-v1.json` | Video `_meta.json` files |

## Typical Workflows

### 2D Character
```
1. scaffold_character.py  →  Creates folder with placeholders
2. Artist replaces PNGs   →  Real artwork in correct dimensions
3. validate_character.py  →  Confirms everything is correct
4. Upload to S3/CDN       →  aws s3 sync
5. Update game manifest   →  Add character entry
```

### 3D Rigged Character
```
1. Copy templates/character_3d.json, skeleton.json, state_machine.json
2. Model in Blender/Maya  →  Export model.glb with rig + LODs
3. Bake textures           →  T_{Id}_Albedo.png, T_{Id}_Normal.png, T_{Id}_ARM.png
4. Export animation clips  →  idle.glb, walk.glb, etc.
5. Fill in character_3d.json with actual poly counts, bone counts, durations
6. Validate against schemas
7. Upload to S3/CDN → Update manifest.json characters_3d section
```

### Audio
```
1. Copy templates/sound_manifest.json
2. Record/generate audio   →  Follow format requirements from standard
3. validate_sound_manifest.py  →  Confirms completeness
4. Upload to S3/CDN
```
