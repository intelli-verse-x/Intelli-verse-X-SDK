# Store Launcher

**Skill ID:** `ivx-store-launcher`

---
name: ivx-store-launcher
description: >-
  AI-powered app store asset generation and automated submission: icons, screenshots,
  key art, feature graphics, trailers, and direct submission to App Store Connect and
  Google Play Console. Use when the user says "store assets", "app icon", "screenshots",
  "key art", "feature graphic", "app store submission", "google play", "app store connect",
  "trailer generation", "store listing", "ASO", "launch game", or needs store-ready assets
  or automated deployment.
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

The Store Launcher generates all assets required for app store listings — icons, screenshots, feature graphics, key art, promotional banners, and game trailers — and optionally submits them directly to App Store Connect and Google Play Console via their APIs. All outputs are sized to each store's exact requirements.

```
Game Name + Characters + Screenshots/Gameplay
       │
       ▼
┌── Content-Factory ──────────────────────────────────────┐
│  LLM Asset Planning → Image Gen (key art, icon)         │
│  → Screenshot Composition → Trailer Assembly (FFmpeg)   │
│  → Store-Spec Sizing → Metadata Generation              │
│  → Optional: API Submit to App Store / Google Play      │
└──────────────────────┬──────────────────────────────────┘
                       │
         ┌─────────────┼──────────────┐
         ▼             ▼              ▼
    Store Assets   Trailer.mp4   Store Metadata
    (sized per     (30-60 sec)   (title, desc,
     platform)                    keywords)
```

## Content-Factory API

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/store_launch \
  -H "Content-Type: application/json" \
  -d '{
    "game_id": "brain-quest",
    "game_name": "Brain Quest - Trivia Challenge",
    "description": "Test your knowledge across 20+ categories with fun characters!",
    "genre": "puzzle",
    "characters": ["Quizzy", "Atlas", "Luna"],
    "character_assets_dir": "characters/",
    "gameplay_screenshots": ["screen1.png", "screen2.png", "screen3.png"],
    "platforms": ["ios", "android", "steam"],
    "generate_trailer": true,
    "ivx_export": true
  }'
```

## Generated Output

```
store_launch/
├── ios/
│   ├── icon_1024x1024.png                    # App Store icon (no rounded corners)
│   ├── screenshots/
│   │   ├── iphone_6.7_01_1290x2796.png       # iPhone 6.7" (required)
│   │   ├── iphone_6.7_02_1290x2796.png
│   │   ├── iphone_6.7_03_1290x2796.png
│   │   ├── iphone_6.5_01_1284x2778.png       # iPhone 6.5"
│   │   ├── ipad_12.9_01_2048x2732.png        # iPad 12.9"
│   │   └── ipad_12.9_02_2048x2732.png
│   └── preview/
│       └── app_preview_1080x1920.mp4          # 15-30 sec app preview
├── android/
│   ├── icon_512x512.png                       # Google Play icon
│   ├── feature_graphic_1024x500.png           # Feature graphic (required)
│   ├── screenshots/
│   │   ├── phone_01_1080x1920.png
│   │   ├── phone_02_1080x1920.png
│   │   ├── phone_03_1080x1920.png
│   │   ├── tablet_01_1920x1200.png
│   │   └── tablet_02_1920x1200.png
│   └── promo/
│       └── promo_video.mp4                    # YouTube promo video
├── steam/
│   ├── header_460x215.png                     # Steam store header
│   ├── capsule_231x87.png                     # Small capsule
│   ├── capsule_467x181.png                    # Large capsule
│   ├── hero_3840x1240.png                     # Library hero
│   ├── logo_1280x720.png                      # Library logo
│   ├── page_bg_1438x810.png                   # Store page background
│   └── screenshots/
│       ├── screenshot_01_1920x1080.png
│       └── screenshot_02_1920x1080.png
├── trailer/
│   ├── trailer_1080p.mp4                      # 30-60 second game trailer
│   ├── trailer_4k.mp4                         # 4K variant
│   └── thumbnail.png                          # Trailer thumbnail
├── social/
│   ├── twitter_banner_1500x500.png
│   ├── facebook_cover_820x312.png
│   ├── discord_banner_960x540.png
│   └── youtube_thumbnail_1280x720.png
└── ivx/
    └── store_manifest.json                    # All assets + metadata
```

## Store Manifest (ivx/store_manifest.json)

```json
{
  "game_id": "brain-quest",
  "game_name": "Brain Quest - Trivia Challenge",
  "short_description": "Test your knowledge across 20+ categories!",
  "full_description": "Challenge your brain with thousands of trivia questions...",
  "genre": "puzzle",
  "content_rating": "everyone",
  "keywords": ["trivia", "quiz", "brain", "knowledge", "puzzle"],
  "platforms": {
    "ios": {
      "bundle_id": "com.studio.brainquest",
      "icon": "ios/icon_1024x1024.png",
      "screenshots": {
        "iphone_6.7": ["ios/screenshots/iphone_6.7_01_1290x2796.png"],
        "iphone_6.5": ["ios/screenshots/iphone_6.5_01_1284x2778.png"],
        "ipad_12.9": ["ios/screenshots/ipad_12.9_01_2048x2732.png"]
      },
      "app_preview": "ios/preview/app_preview_1080x1920.mp4"
    },
    "android": {
      "package_name": "com.studio.brainquest",
      "icon": "android/icon_512x512.png",
      "feature_graphic": "android/feature_graphic_1024x500.png",
      "screenshots": {
        "phone": ["android/screenshots/phone_01_1080x1920.png"],
        "tablet": ["android/screenshots/tablet_01_1920x1200.png"]
      }
    },
    "steam": {
      "app_id": null,
      "header": "steam/header_460x215.png",
      "capsule_small": "steam/capsule_231x87.png",
      "capsule_large": "steam/capsule_467x181.png",
      "hero": "steam/hero_3840x1240.png",
      "logo": "steam/logo_1280x720.png",
      "screenshots": ["steam/screenshots/screenshot_01_1920x1080.png"]
    }
  },
  "trailer": {
    "file_1080p": "trailer/trailer_1080p.mp4",
    "file_4k": "trailer/trailer_4k.mp4",
    "duration_sec": 45,
    "thumbnail": "trailer/thumbnail.png"
  },
  "social": {
    "twitter_banner": "social/twitter_banner_1500x500.png",
    "facebook_cover": "social/facebook_cover_820x312.png",
    "discord_banner": "social/discord_banner_960x540.png",
    "youtube_thumbnail": "social/youtube_thumbnail_1280x720.png"
  }
}
```

## Automated Submission

### App Store Connect (iOS)

```python
from utils.store_submit import AppStoreSubmitter

submitter = AppStoreSubmitter(
    issuer_id="your-issuer-id",
    key_id="your-key-id",
    key_file="AuthKey_XXXX.p8"
)

await submitter.upload(
    bundle_id="com.studio.brainquest",
    manifest="store_launch/ivx/store_manifest.json",
    ipa_path="builds/brainquest.ipa"
)
```

### Google Play Console (Android)

```python
from utils.store_submit import GooglePlaySubmitter

submitter = GooglePlaySubmitter(
    service_account_json="google-play-service-account.json"
)

await submitter.upload(
    package_name="com.studio.brainquest",
    manifest="store_launch/ivx/store_manifest.json",
    aab_path="builds/brainquest.aab",
    track="internal"  # internal → alpha → beta → production
)
```

## Supported Stores

| Store | Icon Size | Screenshot Sizes | Extras |
|-------|-----------|-----------------|--------|
| **App Store** | 1024×1024 | 1290×2796, 1284×2778, 2048×2732 | App Preview video |
| **Google Play** | 512×512 | 1080×1920, 1920×1200 | Feature Graphic 1024×500 |
| **Steam** | 460×215 header | 1920×1080 | Hero, capsules, logo |
| **Meta Quest** | 1024×1024 | 2560×1440 | Cover art 3:1 |
| **Nintendo eShop** | 1024×1024 | 1280×720 | Banner 1920×1080 |
| **Xbox** | 584×800 | 1920×1080 | Hero art 1920×1080 |
| **PlayStation** | 512×512 | 1920×1080 | Background 1920×1080 |
| **Epic Games** | 400×400 | 1920×1080 | Offer image 2560×1440 |
| **itch.io** | 630×500 | Any | Cover image, banner |

## Trailer Generation

The pipeline uses FFmpeg to compose a game trailer from:

1. Character promo art (zoom/pan animations)
2. Gameplay screenshots (crossfade transitions)
3. Feature callouts (text overlays)
4. Generated music (from Game Audio Factory)
5. Logo reveal (ending shot)

## Platform Notes

### VR Stores
- Meta Quest Store requires 2560×1440 landscape screenshots
- Include 360-degree environment shots if applicable
- Trailer should demonstrate spatial interaction

### Console Stores
- Each console has strict asset requirements — manifest validates per-platform
- ESRB/PEGI rating metadata included in store_manifest.json

### Steam
- Capsule images are make-or-break for discovery
- Generate multiple A/B variants for testing
- Include animated GIF for store page

## Checklist

- [ ] Content-Factory API accessible
- [ ] Game info provided (name, description, genre, characters)
- [ ] Gameplay screenshots or character assets available
- [ ] Target platforms specified
- [ ] Pipeline triggered with `ivx_export: true`
- [ ] `store_manifest.json` generated with all platform assets
- [ ] Icon sizes match platform requirements exactly
- [ ] Screenshots match platform resolution requirements
- [ ] Feature graphic / capsule images look compelling
- [ ] Trailer generated (30-60 seconds, engaging)
- [ ] Social media banners generated
- [ ] Metadata (title, description, keywords) reviewed
- [ ] Store API credentials configured (for auto-submit)
- [ ] Submission tested on internal/alpha track first
- [ ] VR: 2560×1440 landscape screenshots included
- [ ] Console: platform-specific rating metadata added
