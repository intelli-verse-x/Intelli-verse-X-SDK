---
name: ivx-asset-manager
description: >-
  Universal asset CRUD manager for any game. Add, replace, modify, or remove any
  asset type — 2D sprites, 3D models, sounds, scenes, UI elements, videos, fonts —
  whether human-created or AI-generated. Handles schema validation, manifest updates,
  S3 sync, and cross-reference integrity. Use when the user says "add asset",
  "replace sprite", "swap character", "update sound", "remove animation",
  "import my artwork", "replace AI art", "add custom model", "change background",
  "modify asset", "delete asset", "asset CRUD", "manual asset", "artist handoff",
  "import human art", "replace with my version", "hot-swap asset", "asset manager",
  "bulk replace", "asset inventory", or needs help managing game assets from any source.
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

# IntelliVerseX Asset Manager

## Overview

The Asset Manager handles the full **CRUD lifecycle** (Create, Read, Update, Delete) for any game asset — regardless of whether it was AI-generated or hand-crafted by a human artist. Every operation maintains schema compliance, updates manifests, preserves cross-references, and optionally syncs to S3/CDN.

```
┌──────────────────────────────────────────────────────────────┐
│                    Asset Manager                             │
│                                                              │
│   Human Artist ──┐                                           │
│                  ├──→ [ Validate ] → [ Register ] → Game     │
│   AI Pipeline ───┘        │              │                   │
│                      Schema Check   Manifest Update          │
│                      Size Check     S3 Sync                  │
│                      Format Check   Cross-ref Integrity      │
└──────────────────────────────────────────────────────────────┘
```

This skill is the bridge between human creativity and the SDK's standardized pipeline. An artist draws a character, records a sound, or sculpts a 3D model — this skill validates it, generates the metadata JSON, and integrates it seamlessly.

---

## Supported Asset Types

| Type | Schema | Files | Operations |
|------|--------|-------|------------|
| **2D Character** | `character-meta-v1` | `character.json` + sprites + views | Add / Replace / Modify / Delete |
| **Sprite Sheet** | `sprite-spec-v1` | `{action}.png` + `{action}_spec.json` | Add / Replace / Modify / Delete |
| **3D Character** | `character-3d-meta-v1` | `character_3d.json` + mesh + textures | Add / Replace / Modify / Delete |
| **Skeleton** | `skeleton-v1` | `skeleton.json` | Add / Replace / Modify |
| **State Machine** | `state-machine-v1` | `state_machine.json` | Add / Replace / Modify |
| **Sound** | `sound-manifest-v2` | `sound_manifest.json` + audio files | Add / Replace / Modify / Delete |
| **Video** | `video-meta-v1` | `video_meta.json` + video file | Add / Replace / Modify / Delete |
| **Static Model** | `model-meta-v1` | `model_meta.json` + model file | Add / Replace / Modify / Delete |
| **Scene/Environment** | Directory convention | Backgrounds, parallax layers, tilesets | Add / Replace / Delete |
| **UI Asset** | None (convention) | Icons, buttons, panels, fonts | Add / Replace / Delete |
| **Font** | None (convention) | `.ttf` / `.otf` / `.woff2` | Add / Replace / Delete |

---

## 1. ADD — Import a New Asset

### 1a. Add a Human-Created 2D Character

An artist provides sprite sheet PNGs. The asset manager validates them and creates the schema-compliant metadata.

```bash
# Step 1: Place artist files in the character directory
Characters/MyHero/
├── sprites/
│   ├── idle.png          # Artist's sprite sheet (3×2 grid, 512px cells)
│   ├── walk.png
│   ├── run.png
│   ├── jump.png
│   ├── attack.png
│   └── hurt.png
├── front.png             # 512×512 portrait
└── thumbnail.png         # 128×128 icon

# Step 2: Generate metadata from the sprite sheets
python tools/asset-pipeline/scaffold_character.py \
    --name MyHero \
    --output Characters/ \
    --from-existing \
    --rarity rare \
    --tags "warrior,human,fantasy"

# Step 3: Validate
python tools/asset-pipeline/validate_character.py --character Characters/MyHero/
```

**What `--from-existing` does:**
1. Scans `sprites/` for PNGs
2. Detects grid layout from image dimensions and cell size
3. Creates `{action}_spec.json` for each sprite sheet
4. Generates `character.json` with animations catalog
5. Validates everything against schemas

### 1b. Add a Sound Asset

```bash
# Single sound — adds to an existing manifest
python tools/asset-pipeline/manage_asset.py add sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "battle_theme" \
    --file "audio/battle_theme.ogg" \
    --category music \
    --duration-ms 240000 \
    --loop true \
    --bpm 140 \
    --volume 0.4

# Bulk add — import a folder of audio files
python tools/asset-pipeline/manage_asset.py add sound \
    --manifest RemoteAssets/sound_manifest.json \
    --from-directory audio/new_sfx/ \
    --category sfx
```

### 1c. Add a 3D Model (Human-Sculpted)

```bash
# Import a Blender/Maya model with auto-metadata extraction
python tools/asset-pipeline/manage_asset.py add model3d \
    --file models/treasure_chest.glb \
    --id treasure_chest \
    --type interactive \
    --textures textures/chest_albedo.png textures/chest_normal.png \
    --output Models/treasure_chest/

# For rigged characters — also generates skeleton.json + state_machine.json
python tools/asset-pipeline/manage_asset.py add character3d \
    --mesh models/knight.glb \
    --animations animations/*.glb \
    --textures textures/knight_*.png \
    --id Knight \
    --output Characters3D/Knight/
```

### 1d. Add a Scene / Background

```bash
python tools/asset-pipeline/manage_asset.py add scene \
    --background "scenes/forest_bg.png" \
    --parallax-layers "scenes/forest_layer1.png" "scenes/forest_layer2.png" "scenes/forest_layer3.png" \
    --id forest_level \
    --output Scenes/forest/
```

### 1e. Add a Video Asset

```bash
# Auto-extracts metadata via ffprobe
python tools/asset-pipeline/manage_asset.py add video \
    --file trailers/launch_trailer.mp4 \
    --id launch_trailer \
    --title "Launch Trailer" \
    --tags trailer,promotional \
    --output Videos/
```

### 1f. Add a UI Asset

```bash
python tools/asset-pipeline/manage_asset.py add ui \
    --files icons/*.png buttons/*.png \
    --atlas-name main_ui \
    --cell-size 64 \
    --output UI/
```

---

## 2. REPLACE — Swap an Asset

Replace preserves all references (manifests, character metadata, scene configs) while swapping the actual file.

### 2a. Replace a Sprite Sheet

An artist provides a better version of the idle animation.

```bash
python tools/asset-pipeline/manage_asset.py replace sprite \
    --character Characters/Quizzy/ \
    --action idle \
    --new-file artwork/quizzy_idle_v2.png \
    --cell-size 512 \
    --frames 8 \
    --fps 12
```

**What happens:**
1. Backs up the old `idle.png` → `idle.png.bak`
2. Copies new file to `sprites/idle.png`
3. Updates `idle_spec.json` (new frame count, fps)
4. Updates `character.json` animations section
5. Re-validates against schema
6. Reports changes

### 2b. Replace a Sound

```bash
# Replace a single sound in the manifest
python tools/asset-pipeline/manage_asset.py replace sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "battle_theme" \
    --new-file "audio/battle_theme_v2.ogg" \
    --duration-ms 260000

# Replace and upload to S3
python tools/asset-pipeline/manage_asset.py replace sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "correct_answer" \
    --new-file "audio/correct_v3.ogg" \
    --upload-s3
```

### 2c. Replace a 3D Mesh (Keep Rig and Animations)

```bash
# Swap mesh while preserving skeleton, animations, and state machine
python tools/asset-pipeline/manage_asset.py replace mesh \
    --character Characters3D/Knight/ \
    --new-mesh "models/knight_highpoly.glb" \
    --keep-skeleton \
    --keep-animations \
    --keep-state-machine
```

### 2d. Replace an AI-Generated Asset with Human Artwork

This is the most common workflow: AI generates a first pass, artist refines it.

```bash
# AI generated Quizzy's sprites. Artist refined the idle and walk.
python tools/asset-pipeline/manage_asset.py replace sprite \
    --character Characters/Quizzy/ \
    --action idle \
    --new-file artist_refined/quizzy_idle_final.png

python tools/asset-pipeline/manage_asset.py replace sprite \
    --character Characters/Quizzy/ \
    --action walk \
    --new-file artist_refined/quizzy_walk_final.png

# Validate the mixed character (some AI, some human)
python tools/asset-pipeline/validate_character.py --character Characters/Quizzy/
```

### 2e. Bulk Replace (Hot-Swap)

Replace all assets of a type at once — useful for art style changes or localization.

```bash
# Replace all character sprites with a new art style
python tools/asset-pipeline/manage_asset.py bulk-replace sprites \
    --source-dir artwork/pixel_art_style/ \
    --target-dir Characters/ \
    --match-by-name \
    --backup

# Replace all music tracks
python tools/asset-pipeline/manage_asset.py bulk-replace sounds \
    --manifest RemoteAssets/sound_manifest.json \
    --source-dir audio/orchestral_v2/ \
    --category music
```

---

## 3. MODIFY — Change Asset Properties

Modify metadata without replacing the file itself.

### 3a. Modify Character Properties

```bash
# Change rarity
python tools/asset-pipeline/manage_asset.py modify character \
    --character Characters/Quizzy/ \
    --rarity epic

# Add tags
python tools/asset-pipeline/manage_asset.py modify character \
    --character Characters/Quizzy/ \
    --add-tags "seasonal,halloween"

# Change unlock method
python tools/asset-pipeline/manage_asset.py modify character \
    --character Characters/Quizzy/ \
    --unlock-method season_pass \
    --unlock-cost '{"gems": 500}'

# Change animation FPS
python tools/asset-pipeline/manage_asset.py modify sprite \
    --character Characters/Quizzy/ \
    --action idle \
    --fps 12
```

### 3b. Modify Sound Properties

```bash
# Adjust volume
python tools/asset-pipeline/manage_asset.py modify sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "battle_theme" \
    --volume 0.3

# Change loop settings
python tools/asset-pipeline/manage_asset.py modify sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "ambient_rain" \
    --loop true \
    --loop-start-ms 2000 \
    --loop-end-ms 58000

# Move to different category
python tools/asset-pipeline/manage_asset.py modify sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "special_stinger" \
    --category stinger
```

### 3c. Modify 3D Character Properties

```bash
# Update animation loop/layer settings
python tools/asset-pipeline/manage_asset.py modify animation3d \
    --character Characters3D/Knight/ \
    --action attack \
    --loop false \
    --speed 1.2

# Add blend shapes list
python tools/asset-pipeline/manage_asset.py modify character3d \
    --character Characters3D/Knight/ \
    --blend-shapes '{"visemes": ["aa","ee","ih","oh","ou"], "expressions": ["happy","sad","angry","surprised"]}'
```

---

## 4. DELETE — Remove an Asset

Delete removes the file, cleans up metadata, and fixes cross-references.

### 4a. Delete a Sprite Animation

```bash
# Remove the "dance" animation from a character
python tools/asset-pipeline/manage_asset.py delete sprite \
    --character Characters/Quizzy/ \
    --action dance

# Removes: sprites/dance.png, sprites/dance_spec.json
# Updates: character.json animations section
# Validates: remaining character is still schema-compliant
```

### 4b. Delete a Sound

```bash
python tools/asset-pipeline/manage_asset.py delete sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "unused_sfx_old"
    
# Also remove the audio file and S3 object
python tools/asset-pipeline/manage_asset.py delete sound \
    --manifest RemoteAssets/sound_manifest.json \
    --id "unused_sfx_old" \
    --delete-file \
    --delete-s3
```

### 4c. Delete an Entire Character

```bash
# Dry run first — shows what would be deleted
python tools/asset-pipeline/manage_asset.py delete character \
    --character Characters/OldCharacter/ \
    --dry-run

# Actually delete (with backup)
python tools/asset-pipeline/manage_asset.py delete character \
    --character Characters/OldCharacter/ \
    --backup-to backups/
```

### 4d. Delete a Video

```bash
python tools/asset-pipeline/manage_asset.py delete video \
    --meta Videos/old_trailer/video_meta.json \
    --delete-file
```

---

## 5. INVENTORY — List and Audit Assets

### List all assets

```bash
# Full inventory of all game assets
python tools/asset-pipeline/manage_asset.py inventory \
    --root RemoteAssets/ \
    --format table

# Output:
# TYPE          ID              STATUS    SCHEMA    SIZE
# character_2d  Quizzy          valid     v1        2.4 MB
# character_2d  DragonLord      valid     v1        3.1 MB
# character_3d  Knight          valid     v1        15.2 MB
# sound         battle_theme    valid     v2        4.1 MB
# sound         correct_sfx     valid     v2        0.1 MB
# video         launch_trailer  valid     v1        45.0 MB
# model         treasure_chest  valid     v1        1.2 MB

# JSON output for CI/CD
python tools/asset-pipeline/manage_asset.py inventory \
    --root RemoteAssets/ \
    --format json > asset_inventory.json
```

### Audit for issues

```bash
# Find orphaned files (not referenced in any manifest)
python tools/asset-pipeline/manage_asset.py audit orphans \
    --root RemoteAssets/

# Find broken references (manifest points to missing files)
python tools/asset-pipeline/manage_asset.py audit broken-refs \
    --root RemoteAssets/

# Find schema violations
python tools/asset-pipeline/manage_asset.py audit schema \
    --root RemoteAssets/

# Full health check
python tools/asset-pipeline/manage_asset.py audit all \
    --root RemoteAssets/
```

---

## 6. SYNC — S3 and CDN Management

```bash
# Upload changed assets to S3
python tools/asset-pipeline/manage_asset.py sync push \
    --root RemoteAssets/ \
    --bucket intelli-verse-x-media \
    --prefix agent-assets/games/my-game/ \
    --changed-only

# Download latest from S3
python tools/asset-pipeline/manage_asset.py sync pull \
    --bucket intelli-verse-x-media \
    --prefix agent-assets/games/my-game/ \
    --output RemoteAssets/

# Diff local vs S3
python tools/asset-pipeline/manage_asset.py sync diff \
    --root RemoteAssets/ \
    --bucket intelli-verse-x-media \
    --prefix agent-assets/games/my-game/
```

---

## 7. Common Workflows

### Artist Handoff (AI → Human Refinement)

```
1. AI generates character       →  Content-Factory pipeline with ivx_export=True
2. Artist reviews AI output     →  Opens sprites in Photoshop/Aseprite
3. Artist refines selected      →  Saves refined PNGs
4. Replace refined sprites      →  manage_asset.py replace sprite --action idle --new-file refined_idle.png
5. Validate                     →  validate_character.py --character Characters/MyChar/
6. Sync to S3                   →  manage_asset.py sync push
```

### Style Change (Pixel Art → Vector)

```
1. Artist creates new style     →  New PNGs in artwork/vector_style/
2. Bulk replace all sprites     →  manage_asset.py bulk-replace sprites --source-dir artwork/vector_style/
3. Validate all characters      →  validate_character.py --character Characters/ --all
4. Sync                         →  manage_asset.py sync push --changed-only
```

### Sound Design Iteration

```
1. Sound designer records SFX   →  New .ogg files in audio/sfx_v3/
2. Replace sounds in manifest   →  manage_asset.py replace sound --id correct_answer --new-file audio/sfx_v3/correct.ogg
3. Adjust volume/timing         →  manage_asset.py modify sound --id correct_answer --volume 0.8
4. Validate manifest            →  validate_sound_manifest.py --manifest sound_manifest.json
5. Sync                         →  manage_asset.py sync push
```

### Adding a New Character Mid-Production

```
1. Artist provides all artwork  →  PNGs in artwork/NewCharacter/
2. Scaffold from existing       →  scaffold_character.py --name NewChar --from-existing
3. Set properties               →  manage_asset.py modify character --rarity epic --unlock-method event
4. Validate                     →  validate_character.py --character Characters/NewChar/
5. Add to game config           →  Update character roster / unlock conditions
6. Sync to S3                   →  manage_asset.py sync push
```

### Removing Deprecated Assets

```
1. Audit for unused             →  manage_asset.py audit orphans
2. Review list                  →  Developer confirms which to remove
3. Delete with backup           →  manage_asset.py delete character --character Characters/OldChar/ --backup-to backups/
4. Verify integrity             →  manage_asset.py audit broken-refs
5. Sync                         →  manage_asset.py sync push
```

---

## 8. Integration with Other Skills

| Skill | How Asset Manager Integrates |
|-------|------------------------------|
| **ivx-character-factory** | AI generates → Asset Manager imports, validates, and registers |
| **ivx-3d-character-pipeline** | AI generates 3D → Asset Manager registers mesh + skeleton + state machine |
| **ivx-game-audio-factory** | AI generates audio → Asset Manager adds to sound manifest |
| **ivx-environment-generator** | AI generates scenes → Asset Manager registers backgrounds + parallax layers |
| **ivx-store-launcher** | Asset Manager provides store icons, screenshots for submission |
| **ivx-asset-pipeline** | Asset Manager extends the pipeline with CRUD operations |
| **ivx-ugc-pipeline** | Player-created content validated by Asset Manager before publishing |
| **ivx-quality-gates** | Asset Manager runs schema validation as a quality gate |
| **ivx-devops-cicd** | `manage_asset.py audit all` runs in CI to catch issues |

---

## 9. Cross-Engine Compatibility

All Asset Manager operations produce engine-agnostic output:

| Operation | Unity | Unreal | Godot | Roblox | Web |
|-----------|-------|--------|-------|--------|-----|
| Add character | `IVXCharacterImporter` | `UE_IVXCharacterImporter` | `ivx_character_importer.gd` | `IVXModule` | `IVXLoader.js` |
| Replace sprite | Hot-reload via `AddressableAssets` | Hot-reload via `AssetManager` | `ResourceLoader.load()` | `ContentProvider` | Dynamic `<img>` swap |
| Modify sound | Update `AudioMixer` at runtime | Update `USoundMix` | Update `AudioBus` | Update `SoundService` | Update `GainNode` |
| Delete asset | Addressable cleanup | Asset registry removal | `.import` cleanup | Instance cleanup | Cache eviction |

---

## 10. Safety Features

| Feature | Description |
|---------|-------------|
| **Backup on replace** | Old file saved as `.bak` before replacement |
| **Dry run mode** | `--dry-run` flag shows what would change without changing it |
| **Schema validation** | Every add/replace/modify re-validates against the SDK schema |
| **Cross-reference check** | Delete operations verify no other manifest/config references the asset |
| **S3 versioning** | S3 objects are versioned; rollback is always possible |
| **Audit trail** | All operations logged to `asset_changelog.json` with timestamp, user, and diff |
| **Size guards** | Warns if sprite sheets exceed 4096px, audio exceeds 50MB, etc. |
| **Format validation** | Checks image format (PNG), audio format (ogg/mp3/wav), model format (glb/fbx) |

---

## Checklist

- [ ] `manage_asset.py` accessible in `tools/asset-pipeline/`
- [ ] Dependencies installed (`Pillow`, `jsonschema`, `boto3` for S3 sync)
- [ ] Asset root directory structure established
- [ ] Sound manifest exists (create from template if not)
- [ ] Schema validation passing for all existing assets
- [ ] S3 bucket configured (if using sync)
- [ ] CI/CD pipeline includes `manage_asset.py audit all`
- [ ] Artist handoff workflow documented for the team
- [ ] Backup strategy in place (`.bak` files or S3 versioning)
