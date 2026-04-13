# Competitor Intelligence Skill

**Skill ID:** `ivx-competitor-intel`

---
name: ivx-competitor-intel
description: Upstream intelligence pipeline that auto-detects competitors and scrapes store listings
version: 1.0.0
trigger_phrases:
  - "run competitor analysis"
  - "scrape competitor stores"
  - "find keyword gaps"
  - "competitor intel"
---



## Overview
This skill is an upstream intelligence stage that feeds data to downstream pipelines (ASO, Marketing, Landing Page). It runs the `competitor_analysis.py` pipeline.

## Capabilities

1. **Auto-Detection**: Uses Perplexity Sonar (mocked) to find top 5-10 competitors based on the GDD's genre, target audience, and key features.
2. **Store Scraping**: Scrapes competitor store listings per locale (title, subtitle, keywords, descriptions, ratings, pricing).
3. **Keyword Gap Analysis**: Identifies keywords that competitors rank for but our game currently does not.
4. **Differentiator Matrix**: Analyzes what our game does better or differently than the competition.
5. **Output**: Generates a `competitor_intel.json` file.

## Usage

To run the competitor analysis:

```bash
python -m pipelines.runner run \
  --config configs/pipelines/competitor_analysis.yaml \
  --args '{"brand_id":"<brand_id>","game_id":"<game_id>"}'
```

## Integration
This pipeline is deeply integrated into the `ivx_full_game` orchestrator (Phase 3: Intelligence). Its output (`competitor_intel.json`) is explicitly passed to `aso_keywords.py`, `marketing_kit.py`, and `landing_page.py` to ensure all generated copy and keywords are highly differentiated and optimized against current market conditions.
