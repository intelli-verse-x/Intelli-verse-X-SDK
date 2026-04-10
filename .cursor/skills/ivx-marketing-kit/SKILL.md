---
name: ivx-marketing-kit
description: Generates promotional marketing assets including newsletters, influencer kits, community graphics, and animated GIFs
version: 1.0.0
trigger_phrases:
  - "generate marketing kit"
  - "create newsletter assets"
  - "build influencer kit"
  - "generate community graphics"
  - "extract trailer gifs"
---

# IntelliVerseX Marketing Kit Skill

## Overview
This skill generates a complete suite of promotional assets to support game launches. It runs the `marketing_kit.py` pipeline and consumes `competitor_intel.json` (if available) to ensure messaging highlights unique differentiators rather than generic copy.

## Capabilities

1. **Newsletter Assets**: Generates 3 HTML email templates (launch, update, event) via an MJML-style builder.
2. **Influencer Kit**: Bundles a fact sheet PDF, logo/screenshot assets, usage guidelines, and a brand voice summary.
3. **Community Graphics**: Generates 6 platform-sized variants (Reddit banners, Discord server icons/banners, forum headers, Twitch banners, Steam community assets).
4. **Animated GIFs**: Extracts loops from the game trailer and creates sprite-based GIFs via FFmpeg.

## Usage

To generate the marketing kit, run the pipeline:

```bash
python -m pipelines.runner run \
  --config configs/pipelines/marketing_kit.yaml \
  --args '{"brand_id":"<brand_id>","game_id":"<game_id>"}'
```

## Integration
This pipeline is fully integrated into the `ivx_full_game` orchestrator, running in Phase 4 (Marketing & Distribution) after the `competitor_analysis` stage.
