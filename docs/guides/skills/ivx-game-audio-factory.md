# Game Audio Factory

**Skill ID:** `ivx-game-audio-factory`

---
name: ivx-game-audio-factory
description: >-
  AI-powered game audio generation: background music, sound effects, voice lines, and
  ambient audio. Outputs a sound_manifest.json validated against the IntelliVerseX SDK
  sound-manifest-v2 schema. Use when the user says "generate game audio", "create sound
  effects", "game music", "BGM generation", "SFX pack", "sound manifest", "voice lines",
  "ambient audio", "audio suite", "game sounds", or needs AI-generated audio for a game.
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

The Game Audio Factory generates complete audio suites for games — background music, sound effects, stingers, ambient loops, UI sounds, and character voice lines. All output is packaged as a `sound_manifest.json` validated against the IntelliVerseX SDK `sound-manifest-v2` schema, directly consumable by any game engine.

```
Game Description + Genre
       │
       ▼
┌── Content-Factory ──────────────────────────────────┐
│  LLM Audio Planning → Beatoven/Lyria (BGM)          │
│  → SFX Generation → ElevenLabs (Voice)              │
│  → Council QA → IVX Schema Export → S3 Upload       │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
              sound_manifest.json
              (sound-manifest-v2 compliant)
              + audio files (ogg/wav/mp3)
```

## Content-Factory API

### Trigger via MCP

```
trigger_pipeline game_sound --game_id my-puzzle --genre puzzle --character_names Quizzy,Bear
```

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/game_sound \
  -H "Content-Type: application/json" \
  -d '{
    "game_id": "my-puzzle",
    "game_name": "Brain Quest",
    "genre": "puzzle",
    "character_names": ["Quizzy", "Bear"],
    "ivx_export": true
  }'
```

## Generated Output

```
game_sound/
├── audio/
│   ├── music/
│   │   ├── music_menu.mp3
│   │   ├── music_gameplay.mp3
│   │   └── music_victory.mp3
│   ├── sfx/
│   │   ├── sfx_correct_answer.ogg
│   │   ├── sfx_wrong_answer.ogg
│   │   ├── sfx_button_click.ogg
│   │   ├── sfx_coin_collect.ogg
│   │   ├── sfx_level_complete.ogg
│   │   └── sfx_countdown_tick.ogg
│   ├── stinger/
│   │   ├── stinger_victory.ogg
│   │   ├── stinger_defeat.ogg
│   │   └── stinger_combo.ogg
│   ├── ambient/
│   │   └── ambient_default.ogg
│   └── ui/
│       ├── ui_button_click.ogg
│       ├── ui_menu_open.ogg
│       └── ui_page_turn.ogg
├── sound_manifest.json               # Content-factory native format
└── ivx/
    └── sound_manifest.json            # Validates: sound-manifest-v2
```

## SDK Schema Compliance (sound-manifest-v2)

```json
{
  "version": "2.0",
  "base_url": "https://cdn.example.com/audio/",
  "categories": {
    "ui": {
      "ui_button_click": { "url": "ui/ui_button_click.ogg", "format": "ogg", "duration_ms": 120, "volume": 0.5 },
      "ui_menu_open":    { "url": "ui/ui_menu_open.ogg",    "format": "ogg", "duration_ms": 300, "volume": 0.5 }
    },
    "sfx": {
      "sfx_correct_answer": { "url": "sfx/sfx_correct_answer.ogg", "format": "ogg", "duration_ms": 500, "volume": 0.7 },
      "sfx_wrong_answer":   { "url": "sfx/sfx_wrong_answer.ogg",   "format": "ogg", "duration_ms": 400, "volume": 0.7 },
      "sfx_coin_collect":   { "url": "sfx/sfx_coin_collect.ogg",   "format": "ogg", "duration_ms": 300, "volume": 0.6 }
    },
    "stinger": {
      "stinger_victory": { "url": "stinger/stinger_victory.ogg", "format": "ogg", "duration_ms": 2500, "volume": 0.8 },
      "stinger_defeat":  { "url": "stinger/stinger_defeat.ogg",  "format": "ogg", "duration_ms": 2000, "volume": 0.7 }
    },
    "music": {
      "music_menu": {
        "url": "music/music_menu.mp3", "format": "mp3",
        "duration_ms": 180000, "loop": true,
        "loop_start_ms": 4000, "loop_end_ms": 176000,
        "volume": 0.4, "bpm": 120
      },
      "music_gameplay": {
        "url": "music/music_gameplay.mp3", "format": "mp3",
        "duration_ms": 240000, "loop": true, "volume": 0.35, "bpm": 130
      }
    },
    "ambient": {
      "ambient_default": { "url": "ambient/ambient_default.ogg", "format": "ogg", "duration_ms": 60000, "loop": true, "volume": 0.15 }
    }
  },
  "mixer_groups": {
    "Master":  { "default_volume": 1.0 },
    "Music":   { "default_volume": 0.4, "parent": "Master" },
    "SFX":     { "default_volume": 0.7, "parent": "Master" },
    "UI":      { "default_volume": 0.5, "parent": "Master" },
    "Ambient": { "default_volume": 0.2, "parent": "Master" },
    "Voice":   { "default_volume": 0.9, "parent": "Master" }
  }
}
```

## IVX Export: Category Mapping

Content-factory uses different category names. The export layer maps them:

| Content-Factory Category | SDK Category |
|-------------------------|-------------|
| `music` | `music` |
| `stingers` | `stinger` |
| `timer_sfx` | `sfx` |
| `gameplay_sfx` | `sfx` |
| `notification_sfx` | `notif` |
| `mode_sfx` | `sfx` |
| `asmr` | `ambient` |

## Audio Providers

| Provider | Type | Output |
|----------|------|--------|
| **Beatoven** | Background music | MP3, configurable BPM/genre/mood |
| **Lyria** (Google) | Background music | MP3, high quality |
| **ElevenLabs** | Voice lines / TTS | WAV/MP3, character voices |
| **PiAPI** | Song generation | MP3, lyrics + melody |

## Genre-Specific Audio Suites

| Genre | BGM Tracks | SFX Count | Stingers | Ambient |
|-------|:----------:|:---------:|:--------:|:-------:|
| Puzzle / Quiz | 3 (menu, gameplay, victory) | 12-15 | 3 | 1 |
| Platformer | 4 (menu, overworld, underground, boss) | 20-25 | 4 | 2 |
| RPG | 6 (menu, town, field, dungeon, battle, boss) | 30+ | 5 | 3 |
| Racing | 3 (menu, race, results) | 15-20 | 3 | 2 |
| Horror | 3 (menu, explore, chase) | 20-25 | 5 | 3 |

## Validation

```bash
python tools/asset-pipeline/validate_sound_manifest.py \
  --manifest game_sound/ivx/sound_manifest.json
```

## Platform Notes

### VR
- Spatial audio metadata (position, falloff) added via engine integration
- Ambient loops critical for immersion

### Console
- WAV preferred for low-latency SFX on PS5/Xbox
- Music streaming from CDN for large files

### WebGL
- OGG Vorbis for broad browser support
- Preload critical SFX, lazy-load music

### Mobile
- OGG for Android, AAC/MP3 for iOS fallback
- Compress to 128kbps for music, 96kbps for SFX

## Checklist

- [ ] Content-Factory API accessible
- [ ] Game description and genre provided
- [ ] Character names provided (for voice line association)
- [ ] Pipeline triggered with `ivx_export: true`
- [ ] `sound_manifest.json` validates against `sound-manifest-v2`
- [ ] All audio files playable (no corruption)
- [ ] Music tracks loop cleanly (check loop points)
- [ ] SFX are short and punchy (< 2 seconds)
- [ ] Stingers have appropriate impact
- [ ] Mixer groups configured with sensible defaults
- [ ] Audio imported into target engine
- [ ] Mixer/bus routing set up in engine
- [ ] VR: spatial audio configured
- [ ] Console: format compliance checked
- [ ] WebGL: browser codec compatibility verified
- [ ] Mobile: file sizes optimized
