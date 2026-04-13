# UA & Marketing Strategy

**Skill ID:** `ivx-ua-marketing-strategy`

---
name: ivx-ua-marketing-strategy
description: >-
  End-to-end UA economics, ASO, geo-targeting, and marketing strategy generation
  for IntelliVerseX SDK games. Produces a complete, data-driven marketing plan covering
  feature-to-cohort mapping, advanced ASO with competitor keyword gap analysis, ROAS-ranked
  country priority lists, payback period models (7/14/30 day), ad creative briefs, and
  Content-Factory pipeline integration for automated report generation. Use when the user
  says "marketing strategy", "ASO optimization", "user acquisition", "UA economics",
  "geo targeting", "ROAS analysis", "payback period", "CPI analysis", "ad spend plan",
  "country targeting", "keyword strategy", "store optimization", "app store listing",
  "competitor analysis", "ad creative brief", "marketing audit", "growth strategy",
  "CAC analysis", "LTV model", "retention monetization", or needs help with any UA,
  ASO, or marketing planning workflow.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
  - WebSearch
---



## Overview

This skill generates a complete, data-driven User Acquisition and Marketing Strategy for any game built with the IntelliVerseX SDK. It produces an actionable plan covering every layer of the marketing funnel — from store listing optimization through paid acquisition to payback economics — all grounded in the game's actual features, economy, supported languages, and competitive landscape.

```
Game Data (GDD, Features, Economy, Languages)
       │
       ▼
┌── Analysis Pipeline ────────────────────────────────────────────────┐
│                                                                      │
│  1. Feature Inventory ──────────► Feature-to-Cohort Mapping          │
│  2. Language Inventory ─────────► ROAS-Ranked Country Matrix         │
│  3. Economy Data ───────────────► Sink/Source Ratio + LTV Model      │
│  4. Competitor Research ────────► Keyword Gap Analysis                │
│  5. Platform Benchmarks ────────► CPI × Retention → Payback Model    │
│                                                                      │
└────────────────────────────┬─────────────────────────────────────────┘
                             │
        ┌────────────────────┼──────────────────────┐
        ▼                    ▼                      ▼
  ASO Playbook      UA Economics Plan       Ad Creative Briefs
  (per locale)      (per country tier)      (per cohort × platform)
        │                    │                      │
        └────────────────────┼──────────────────────┘
                             ▼
                    Unified Marketing Plan
                    (.md + .json + .pdf + .docx)
```

## Content-Factory Pipeline Integration

This skill maps directly to the `game_marketing_audit` pipeline in Content-Factory.

### Trigger via CLI

```bash
python -m pipelines.runner run \
  --config configs/pipelines/game_marketing_audit.yaml \
  --args '{
    "game_name": "Quiz-Verse",
    "game_genre": "trivia",
    "platforms": ["Android", "iOS", "WebGL"],
    "languages": ["en","es","pt","hi","ar","de","fr","ja","id","ko","ru","zh"],
    "features_file": "path/to/QUIZVERSE_COMPLETE_FEATURES_LIST.md",
    "economy_file": "path/to/COIN_ECONOMY_SYSTEM.md",
    "gdd_file": "path/to/GDD.md",
    "competitors": ["Trivia Crack", "Quizlet", "Lumosity", "QuizUp"]
  }'
```

### Trigger via REST

```bash
curl -X POST http://localhost:8001/pipelines/game_marketing_audit \
  -H "Content-Type: application/json" \
  -d '{
    "brand_id": "intelliversex",
    "game_id": "quiz-verse",
    "competitors": ["Trivia Crack", "Quizlet", "Lumosity"],
    "research_competitors": true
  }'
```

### Output Structure

```
.working_dir/game_marketing_audit/
├── marketing_audit.json          # Structured data (Pydantic model)
├── marketing_audit.md            # Human-readable report
├── exports/
│   ├── UA_Economics_and_Geo_Targeting.pdf
│   └── UA_Economics_and_Geo_Targeting.docx
└── assets/
    └── payback_chart.png         # Visual payback period chart
```

---

## Workflow: Step-by-Step

### Step 1: Gather Game Data

Before running the analysis, collect these inputs:

| Input | Source | Required? |
|-------|--------|-----------|
| **Feature list** | Game's `QUIZVERSE_COMPLETE_FEATURES_LIST.md` or equivalent | Yes |
| **Economy data** | `COIN_ECONOMY_SYSTEM.md` or Hiro Economy config | Yes (for LTV model) |
| **GDD** | `GDD.md` or game design document | Recommended |
| **Supported languages** | Game's localization config | Yes (drives country ranking) |
| **Platforms** | Android, iOS, WebGL, Steam, etc. | Yes |
| **Competitor list** | Top 3-5 competitors in the same genre | Recommended |

### Step 2: Feature-to-Cohort Mapping

Map every game feature to the audience cohort it attracts most.

**Framework: Etermax Cohort Segmentation**

| Cohort | Age | Motivation | Feature Match Pattern |
|--------|-----|------------|----------------------|
| Competitive Socializers | 18-35 | Beat friends, social status, bragging | PvP, multiplayer, leaderboards, team battles, staking |
| Brain Trainers | 40+ | Mental acuity, daily habit, learning | Daily challenge, streaks, categories, mastery |
| Students / Learners | 13-24 | Study aid, academic review, UGC | Content generation, flashcards, doc upload, voice input |
| Party / Family | 10-60 (groups) | Social fun, family time, group entertainment | Same-device multiplayer, AI host, casual modes |

**For each feature, determine:**
1. Which cohort it primarily serves
2. Which ad platform that cohort trusts for downloads
3. Which language/region the feature resonates most with
4. What ad creative angle works best

### Step 3: Advanced ASO Analysis

#### 3.1 Keyword Categories

For each supported language, produce three keyword tiers:

| Tier | Definition | Example (Trivia Game) |
|------|-----------|----------------------|
| **Must-Win** | High volume, high competition. Must rank top 10. | "trivia game", "quiz app", "brain training" |
| **Blue Ocean** | Medium volume, zero/low competition. Unique to your game. | "ai quiz maker", "quiz from pdf", "5v5 trivia" |
| **Long Tail** | Low volume per keyword, but many of them. Organic accumulators. | "quiz with friends online", "daily brain quiz for adults" |

#### 3.2 Competitor Gap Analysis

For each competitor, identify:

| Dimension | Analysis |
|-----------|----------|
| Keywords they dominate | Which keywords they rank #1-3 for |
| Keywords they're weak on | Where they rank >10 or not at all |
| Features we beat them on | Where our game is objectively better |
| Features they beat us on | Where we need to catch up or differentiate |
| Their ASO metadata | Title, subtitle, keywords, description structure |

#### 3.3 iOS Metadata Optimization

```
TITLE (30 chars max):
  [Game Name]: [Primary KW] & [Secondary KW]
  Every character counts. Do NOT waste on generic words.

SUBTITLE (30 chars max):
  [Tertiary KW] & [Quaternary KW]
  Do NOT repeat any word from the title — Apple indexes both.

KEYWORDS FIELD (100 chars max):
  Comma-separated, no spaces after commas.
  Do NOT repeat words from title or subtitle.
  Do NOT use plurals if singular is in title (Apple matches both).
  Pack with blue ocean and long-tail keywords.

FULL DESCRIPTION (4000 chars):
  First 252 chars appear before "more" tap — front-load keywords.
  2-3% keyword density for primary terms.
  Use bullet points for features (improves readability AND indexing).
  End with FAQ-style content for long-tail keyword capture.
```

#### 3.4 Google Play Metadata Optimization

```
TITLE (50 chars max):
  More space than iOS. Use ALL of it. Every word is indexed.

SHORT DESCRIPTION (80 chars max):
  Google indexes this heavily. Front-load primary keywords.
  Include a number ("30 quiz modes") — numbers increase CTR.
  End with "Free!" if applicable — converts 10-15% better.

FULL DESCRIPTION (4000 chars):
  Google uses NLP to extract keywords — natural language works.
  Mention every game mode by name (each is a potential keyword).
  List all supported languages (increases discoverability).
  Include competitor-relevant terms naturally.
  Use line breaks and bullet points for scannability.
```

### Step 4: UA Unit Economics Model

The payback model is the core economic framework:

```
CPI → Install → Retention Decay → ARPDAU × Retained Days = Cumulative Revenue
                                                                    │
                                                         Compare to CPI
                                                                    │
                                                    ┌───────────────┼───────────────┐
                                                    ▼               ▼               ▼
                                              7-Day Payback   14-Day Payback  30-Day Payback
```

#### 4.1 Retention Decay Model

Use the standard exponential decay:

```
Retention(d) = D1_retention × e^(-λ × (d-1))

Where:
  D1_retention = Day-1 retention (typically 35-45% for casual/trivia)
  λ = decay constant (calibrate from D7 and D30 data)
  
Example (Quiz-Verse post-fix):
  D1 = 40%, D7 = 18%, D30 = 8%
  λ ≈ 0.115
```

#### 4.2 Cumulative Revenue per Install

```
LTV(d) = Σ(i=0 to d) [Retention(i) × ARPDAU]

Example:
  ARPDAU = $0.15, D1 = 40%
  LTV(7) = $0.48
  LTV(14) = $0.63
  LTV(30) = $0.88
  LTV(90) = $1.32
  LTV(365) = $1.80
```

#### 4.3 Payback Period Calculation

```
Payback Day = first d where LTV(d) ≥ CPI

If CPI = $0.30 → Payback ≈ Day 3 (Tier 1 country, scale aggressively)
If CPI = $0.80 → Payback ≈ Day 25 (Tier 2, steady growth)
If CPI = $2.00 → Payback ≈ Day 90+ (Tier 3, targeted only)
```

### Step 5: ROAS-Ranked Country Matrix

Rank every targetable country by expected ROAS:

**Ranking Formula:**
```
ROAS_Score = (ARPDAU_country / CPI_country) × Language_Match_Multiplier × Market_Size_Factor

Where:
  Language_Match_Multiplier:
    1.5 if game has native localization for that country's language
    1.0 if English works (English-speaking or bilingual country)
    0.5 if no language match (add the language or skip)
  
  Market_Size_Factor:
    log10(daily_available_volume) / 5  (normalizes to 0-1)
```

**Country Tiers:**

| Tier | Payback Window | Strategy | Budget Allocation |
|------|---------------|----------|-------------------|
| **Tier 1** (7-day payback) | CPI < $0.50 | Scale aggressively. Cash-flow positive from week 1. | 50% of budget |
| **Tier 2** (14-day payback) | CPI $0.50-$1.20 | Steady growth. Higher LTV offsets longer payback. | 30% of budget |
| **Tier 3** (30+ day payback) | CPI > $1.20 | Targeted only. Subscription push, retargeting, Apple Search Ads. | 20% of budget |

### Step 6: Ad Creative Briefs

For each cohort × platform × language combination:

| Field | Description |
|-------|-------------|
| **Cohort** | Which audience segment |
| **Platform** | Meta, Google, TikTok, YouTube, Apple Search, etc. |
| **Format** | UGC Reel, Static, Carousel, Playable, Search Text, Pre-roll |
| **Language** | Creative language + cultural adaptation notes |
| **Concept** | 1-2 sentence creative concept |
| **Copy** | Headline / ad text |
| **CTA** | Call to action button text |
| **Hook** | First 3 seconds (for video) or hero image (for static) |
| **Est. CTR** | Expected click-through rate based on format + platform |
| **Refresh Cadence** | How often to replace (14-28 days typically) |

### Step 7: Generate Output Documents

The pipeline produces:

1. **Structured JSON** (`marketing_audit.json`) — Machine-readable, importable to dashboards
2. **Markdown Report** (`marketing_audit.md`) — Human-readable, versioned in git
3. **PDF** (optional) — Shareable with stakeholders
4. **DOCX** (optional) — Editable by marketing team

---

## Document Sections (Output Schema)

The generated marketing plan contains these sections:

### Section 1: Feature → Audience → Localization → Platform Pipeline
- Complete mapping of every game feature to its target audience
- Per-feature: languages that resonate, trusted download platform, content stickiness score

### Section 2: Advanced ASO
- Current ASO score assessment (0-100)
- Competitor keyword gap analysis (must-win, blue ocean, long-tail)
- Optimized metadata per platform (iOS title/subtitle/keywords, Google Play title/short desc/full desc)
- Full keyword strategy per supported language with search volumes
- Competitor feature gap heatmap
- ASO execution playbook (phased, week-by-week)

### Section 3: UA Unit Economics
- CPI by platform and cohort
- Retention → revenue accumulation model (D0 through D365)
- Payback period by CPI tier (7/14/30 day)
- CAC per paying user across platforms

### Section 4: ROAS-Ranked Country Priority List
- Every targetable country ranked by expected ROAS
- Columns: language match, primary audience, CPI, ARPDAU, D7/D30 ROAS, LTV, rationale
- Tier classification (7-day / 14-day / 30+ day payback)
- Budget allocation by tier

### Section 5: Ad Creative Briefs
- Per-tier creative strategy (volume, value, LTV optimized)
- Creative concepts per cohort × platform × language
- Refresh cadence and production budget

### Section 6: Revenue Model
- ARPDAU variance by market tier (eCPM, IAP conversion, blended)
- Blended portfolio ARPDAU under different country mix scenarios
- Monthly revenue projections at various DAU levels

### Section 7: Execution Roadmap
- Phase 1-4 timeline (Foundation, Expansion, Scale, Optimize)
- Weekly budget ramp
- KPI targets per phase
- Decision framework (scale/hold/kill thresholds)

### Section 8: Seasonal Calendar
- Monthly ad spend multipliers
- Event-driven campaigns (holidays, exam seasons, cultural events)
- Cohort-specific seasonal hooks

---

## Integration with Other IVX Skills

| Skill | Integration Point |
|-------|-------------------|
| `ivx-economy-simulator` | Economy health feeds into LTV model and payback calculations |
| `ivx-store-launcher` | ASO metadata and screenshots deploy via Store Launcher |
| `ivx-landing-page` | Landing page generation uses cohort definitions and creative angles |
| `ivx-analytics-pipeline` | Post-launch: actual D1/D7/D30 data replaces projections |
| `ivx-live-ops` | Satori experiments/flags enable A/B testing recommended by this plan |
| `ivx-localization` | Store listing localization uses the keyword strategy per language |
| `ivx-monetization` | Economy data and IAP catalog feed into revenue projections |
| `ivx-game-design-studio` | GDD provides feature inventory for cohort mapping |

---

## Hiro/Satori Integration

The marketing plan recommends specific backend configurations:

### Satori Experiments (A/B Testing)

| Experiment | What It Tests | Required For |
|------------|--------------|--------------|
| `onboarding_variant` | Which onboarding flow converts better | D1 retention optimization |
| `coin_price_test` | IAP pricing sensitivity | Revenue optimization |
| `ad_frequency_test` | Optimal rewarded ad frequency per session | ARPDAU optimization |
| `screenshot_test` | Which cohort screenshot set converts better | ASO optimization |

### Satori Feature Flags

| Flag | Purpose |
|------|---------|
| `ua_campaign_active` | Enable/disable acquisition-specific features |
| `subscription_push` | Enable aggressive subscription prompts for Tier 3 markets |
| `exam_season_mode` | Activate seasonal student-focused content |
| `holiday_event` | Activate seasonal family/party content |

### Satori Audiences

| Audience | Definition | Marketing Use |
|----------|-----------|---------------|
| `organic_user` | Installed without ad attribution | Baseline retention benchmark |
| `paid_tier1` | Acquired from Tier 1 countries | Monitor D7 ROAS |
| `paid_tier3` | Acquired from Tier 3 countries | Push subscription offers |
| `churning_d3` | No session in 72h | Retargeting audience for Meta/Google |

---

## Checklist

- [ ] Game feature list collected (all modes, mechanics, differentiators)
- [ ] Economy data gathered (source/sink analysis, ARPDAU)
- [ ] Supported languages confirmed (each language unlocks countries)
- [ ] Competitor list defined (3-5 direct competitors)
- [ ] Content-Factory `game_marketing_audit` pipeline configured
- [ ] Pipeline executed and JSON/Markdown outputs reviewed
- [ ] ASO metadata deployed to App Store Connect and Google Play Console
- [ ] Localized store listings deployed for all supported languages
- [ ] App Preview Video recorded and uploaded
- [ ] In-app review prompt implemented
- [ ] Screenshots created (8 per platform, cohort-specific sets)
- [ ] Google Play Custom Store Listings created (1 per cohort)
- [ ] Satori experiments created for onboarding and pricing tests
- [ ] Feature flags created for seasonal/campaign toggles
- [ ] Tier 1 campaigns launched (7-day payback countries)
- [ ] D1/D7 retention monitored; kill/scale decisions applied
- [ ] Creative refresh scheduled (every 14-21 days)
- [ ] PDF/DOCX reports generated for stakeholder sharing
