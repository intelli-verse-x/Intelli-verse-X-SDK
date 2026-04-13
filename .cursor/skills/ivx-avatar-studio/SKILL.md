---
name: ivx-avatar-studio
description: >-
  AI-powered avatar creation from a single photo: one-shot 3D head reconstruction,
  expression-driven reenactment, real-time facial animation, and interactive conversation
  with TTS. Based on GAGAvatar (3D Gaussian Splatting). Use when the user says "create
  avatar", "avatar from photo", "3D avatar", "face avatar", "avatar studio", "facial
  reenactment", "expression animation", "talking avatar", "avatar chat", "photo to avatar",
  "real-time avatar", or needs AI-generated avatars for games, social apps, or metaverse.
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

# IntelliVerseX Avatar Studio

## Overview

The Avatar Studio creates expressive 3D avatars from a single photo using GAGAvatar (Gaussian Avatars with Geometric-aware Appearance Normalization). It supports real-time facial reenactment, expression-driven animation, and interactive conversations with TTS — enabling custom player avatars, NPC portraits, streamers, and virtual hosts.

```
Single Photo
     │
     ▼
┌── Content-Factory / GAGAvatar ──────────────────────┐
│  3DGS Reconstruction → Appearance Normalization     │
│  → Expression Transfer → Real-time Rendering        │
│  → TTS Integration → Interactive Chat               │
└──────────────────────┬──────────────────────────────┘
                       │
           ┌───────────┼───────────┐
           ▼           ▼           ▼
      3D Avatar    Animated     Interactive
      Model        Sequences    Conversation
      (3DGS)       (MP4/GIF)   (WebSocket)
```

## Content-Factory API

### Create Avatar from Photo

```bash
curl -X POST http://localhost:8001/pipelines/avatar_create \
  -H "Content-Type: application/json" \
  -d '{
    "source_photo": "https://example.com/photo.jpg",
    "avatar_name": "PlayerAvatar",
    "style": "realistic",
    "generate_expressions": true,
    "ivx_export": true
  }'
```

### Drive Avatar with Expressions

```bash
curl -X POST http://localhost:8001/pipelines/avatar_animate \
  -d '{
    "avatar_id": "PlayerAvatar",
    "driving_video": "driving_expressions.mp4",
    "output_format": "mp4"
  }'
```

### Interactive Conversation

```bash
curl -X POST http://localhost:8001/pipelines/avatar_chat \
  -d '{
    "avatar_id": "PlayerAvatar",
    "message": "Welcome to Brain Quest! Ready for a challenge?",
    "voice_id": "elli_friendly",
    "emotion": "excited"
  }'
```

## Generated Output

### Avatar Package

```
avatars/PlayerAvatar/
├── model/
│   ├── avatar.ply                    # 3D Gaussian Splatting model
│   ├── appearance_volume.npz         # Tri-plane appearance features
│   └── metadata.json                 # Generation parameters
├── expressions/
│   ├── neutral.png                   # Rendered expression frames
│   ├── happy.png
│   ├── sad.png
│   ├── angry.png
│   ├── surprised.png
│   └── thinking.png
├── animations/
│   ├── greeting.mp4                  # Pre-rendered animation sequences
│   ├── nodding.mp4
│   ├── laughing.mp4
│   └── talking_idle.mp4
├── interactive/
│   ├── chat_config.json              # TTS + expression mapping config
│   └── emotion_map.json              # Text emotion → facial expression mapping
└── ivx/
    ├── avatar_meta.json              # SDK-compliant avatar metadata
    ├── expression_sheet.png          # 3×2 grid (matches character expression format)
    └── thumbnail.png                 # 512×512 avatar portrait
```

### Avatar Metadata (ivx/avatar_meta.json)

```json
{
  "id": "PlayerAvatar",
  "display_name": "Player Avatar",
  "type": "3dgs_avatar",
  "source": "single_photo",
  "model": {
    "file": "model/avatar.ply",
    "format": "3dgs",
    "gaussian_count": 50000,
    "appearance_volume": "model/appearance_volume.npz"
  },
  "expressions": {
    "neutral":   { "file": "expressions/neutral.png",   "blendshape_weights": {} },
    "happy":     { "file": "expressions/happy.png",     "blendshape_weights": {"mouthSmile": 0.8, "cheekPuff": 0.3} },
    "sad":       { "file": "expressions/sad.png",       "blendshape_weights": {"mouthFrown": 0.7, "browDown": 0.5} },
    "angry":     { "file": "expressions/angry.png",     "blendshape_weights": {"browDown": 0.9, "jawClench": 0.6} },
    "surprised": { "file": "expressions/surprised.png", "blendshape_weights": {"mouthOpen": 0.8, "browUp": 0.9} },
    "thinking":  { "file": "expressions/thinking.png",  "blendshape_weights": {"browUp": 0.3, "eyeSquint": 0.4} }
  },
  "animations": {
    "greeting":     { "file": "animations/greeting.mp4",     "duration_sec": 2.0, "loop": false },
    "nodding":      { "file": "animations/nodding.mp4",      "duration_sec": 1.5, "loop": true },
    "laughing":     { "file": "animations/laughing.mp4",     "duration_sec": 2.5, "loop": false },
    "talking_idle": { "file": "animations/talking_idle.mp4", "duration_sec": 3.0, "loop": true }
  },
  "voice": {
    "provider": "elevenlabs",
    "voice_id": "elli_friendly",
    "language": "en"
  },
  "bounds": { "center": [0, 0, 0], "radius": 0.3 },
  "tags": ["avatar", "3dgs", "interactive"]
}
```

## Use Cases

### 1. Custom Player Avatars
Players upload a selfie → get a 3D avatar that reacts to gameplay:
- Correct answer → happy expression
- Wrong answer → sad expression
- Victory → celebration animation
- Defeat → disappointed animation

### 2. AI Game Host / NPC
Create an AI host character from concept art:
- Real-time lip sync during TTS narration
- Emotion-appropriate expressions based on game context
- Interactive Q&A with players

### 3. Streamer / Content Creator Avatars
Create an avatar from a streamer's photo:
- Real-time facial tracking from webcam
- Expression transfer for live streaming
- Green-screen ready output

### 4. Social / Metaverse Profiles
User avatar for social features:
- Profile picture variants (6 expressions)
- Animated greeting for friend requests
- In-game emotes using expression presets

## Expression-Driven Animation

The avatar supports real-time expression transfer via ARKit blendshape coefficients:

```python
EXPRESSION_BLENDSHAPES = {
    "happy":     {"mouthSmile": 0.8, "cheekPuff": 0.3},
    "sad":       {"mouthFrown": 0.7, "browDown": 0.5},
    "angry":     {"browDown": 0.9, "jawClench": 0.6},
    "surprised": {"mouthOpen": 0.8, "browUp": 0.9},
    "thinking":  {"browUp": 0.3, "eyeSquint": 0.4},
    "excited":   {"mouthSmile": 0.9, "browUp": 0.6, "cheekPuff": 0.4},
}
```

### Driving Sources

| Source | Method | Real-time? |
|--------|--------|:----------:|
| Webcam | ARKit/MediaPipe face tracking | Yes |
| Video | Pre-recorded expression video | No |
| Text | LLM emotion detection → expression map | Yes |
| Game Events | Event → emotion → expression | Yes |

## Interactive Conversation Pipeline

```
User Text Input
     │
     ▼
LLM (emotion detection + response)
     │
     ├── response_text: "Great answer!"
     └── emotion: "excited"
              │
              ▼
     ElevenLabs TTS (audio)
     GAGAvatar (visemes + expression)
              │
              ▼
     Synchronized talking avatar video/stream
```

### Chat Configuration

```json
{
  "avatar_id": "PlayerAvatar",
  "llm_provider": "openai",
  "llm_model": "gpt-4o",
  "tts_provider": "elevenlabs",
  "voice_id": "elli_friendly",
  "emotion_detection": true,
  "response_format": "audio_video",
  "max_response_length": 150,
  "personality": "Friendly quiz host who encourages players and celebrates their knowledge"
}
```

## Platform Notes

### VR
- 3DGS rendering via custom VR shader for stereo output
- Head tracking drives gaze direction
- Spatial audio for voice output

### Mobile
- Pre-rendered expression frames (static images) for low-end devices
- Real-time mode requires GPU with 3DGS support (high-end only)
- Fallback: 2D expression sheet with crossfade transitions

### WebGL
- WebGPU required for real-time 3DGS rendering
- Fallback: server-side rendering streamed as video
- Pre-rendered animations for broad compatibility

### Console
- Full 3DGS rendering on PS5/Xbox Series X
- Switch: pre-rendered fallback

## Engine Integration

| Engine | 3DGS Rendering | Expression Control | Voice Integration |
|--------|:-------------:|:-----------------:|:----------------:|
| **Unity** | Custom renderer (compute shader) | BlendShape API | AudioSource + lip sync |
| **Unreal** | Niagara particle system | Morph Targets | MetaSound + visemes |
| **Godot** | Custom shader (particles) | BlendShape tracks | AudioStreamPlayer |
| **Web** | Three.js + WebGPU | JavaScript blendshape driver | Web Audio API |

## Checklist

- [ ] Content-Factory API accessible
- [ ] Source photo provided (frontal, well-lit, neutral expression)
- [ ] Avatar created with `ivx_export: true`
- [ ] `avatar_meta.json` generated
- [ ] Expression sheet (3×2 grid) matches SDK format
- [ ] Expression PNGs rendered for all 6 emotions
- [ ] Pre-rendered animations generated (greeting, nodding, laughing, talking_idle)
- [ ] Thumbnail generated (512×512)
- [ ] Interactive chat configured (LLM + TTS)
- [ ] Voice model selected and tested
- [ ] Emotion detection working (text → expression)
- [ ] Real-time rendering tested on target platform
- [ ] VR: stereo rendering verified
- [ ] Mobile: fallback mode tested on low-end devices
- [ ] WebGL: WebGPU path or video fallback working
- [ ] Console: performance budget verified
