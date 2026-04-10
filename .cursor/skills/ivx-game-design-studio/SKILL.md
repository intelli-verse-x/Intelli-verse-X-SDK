---
name: ivx-game-design-studio
description: >-
  AI-assisted game design workflows for IntelliVerseX SDK games including GDD generation,
  brand identity, store metadata, localization planning, structured character bibles,
  media requirements, design review, systems mapping, balance tools, automated export
  to content-factory entity JSON, and fully-wired starter project generation for any
  of the 11 supported engines. Use when the user says "create GDD",
  "game design document", "design review", "systems mapping", "brainstorm game idea",
  "balance spreadsheet", "game pillars", "MDA framework", "player types",
  "design template", "economy model template", "level design doc", "brand identity",
  "store metadata", "app store listing", "localization plan", "character bible",
  "export GDD to JSON", "gdd_to_entity", "generate starter project",
  "boilerplate", "hello intelliverse", "scaffold game", "create demo project",
  "starter kit", or needs help with any game design documentation or workflow.
version: "3.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX Game Design Studio

## Overview

The Game Design Studio provides structured design workflows adapted from professional game development practices. It combines 15 document templates, AI-assisted brainstorming via `IVXAIContentGenerator`, design review checklists, systems dependency mapping, and a **Starter Project Generator** that outputs a fully-wired, runnable "Hello IntelliVerseX" game for any of the 11 supported engines. Every IVX game gets a design-first workflow that scales from solo developers to teams.

```
Brainstorm → Game Concept → Systems Map → Per-System GDD → Design Review → Engine Selection
    │              │              │              │               │              │
    ▼              ▼              ▼              ▼               ▼              ▼
 AI Assist    Concept Doc    Systems Index   GDD Sections    8-Point Check  Starter Project
                                                                                │
                                                                 ┌──────────────┼──────────────┐
                                                                 ▼              ▼              ▼
                                                            Auth + Login   Economy + Store  Retention
                                                            Hiro Systems   Satori Flags    Engagement
                                                            Leaderboards   Multiplayer     AI Features
```

---

## 1. Design Frameworks

### MDA Framework

Every game design starts with the MDA (Mechanics, Dynamics, Aesthetics) framework:

| Layer | Definition | Example (Trivia Game) |
|-------|-----------|----------------------|
| **Aesthetics** | The emotional experience players have | Challenge, Fellowship, Discovery |
| **Dynamics** | Emergent behavior from mechanics | Time pressure creates excitement |
| **Mechanics** | The formal rules and systems | Question timer, scoring, lifelines |

### Self-Determination Theory

Player motivation analysis through three needs:

| Need | Design Application | Example |
|------|-------------------|---------|
| **Autonomy** | Meaningful choices | Category selection, difficulty preference |
| **Competence** | Achievable mastery | Progressive difficulty, skill-based scoring |
| **Relatedness** | Social connection | Multiplayer, leaderboards, friend challenges |

### Bartle Player Types

Audience targeting for feature prioritization:

| Type | Motivated By | Features |
|------|-------------|----------|
| **Achiever** | Completion, mastery | Achievements, progression, collectibles |
| **Explorer** | Discovery, knowledge | Content variety, hidden features, lore |
| **Socializer** | Relationships | Chat, guilds, cooperative play |
| **Killer** | Competition | PvP, leaderboards, ranking |

---

## 2. Document Templates

### Available Templates

| Template | File | Purpose |
|----------|------|---------|
| **Game Concept** | `design/gdd/game-concept.md` | Initial vision, core loop, pillars |
| **Game Design Document** | `design/gdd/{system}-gdd.md` | Per-system detailed design |
| **Systems Index** | `design/gdd/systems-index.md` | All systems with dependencies |
| **Economy Model** | `design/gdd/economy-model.md` | Currency flows, pricing, balance |
| **Level Design Doc** | `design/gdd/level-design.md` | Level structure, pacing, difficulty |
| **Narrative Design** | `design/gdd/narrative.md` | Story, characters, dialog |
| **Art Bible** | `design/art-bible.md` | Visual style, color palette, reference |
| **Sound Bible** | `design/sound-bible.md` | Audio style, music, SFX |
| **Technical Design** | `docs/architecture/tdd-{feature}.md` | Architecture, data flow, APIs |
| **Pitch Document** | `design/pitch.md` | Investor/publisher pitch |
| **Faction Design** | `design/gdd/factions.md` | Faction mechanics, progression |
| **Character Sheet** | `design/gdd/characters/{name}.md` | Character stats, abilities, lore |
| **Test Plan** | `docs/quality/test-plan.md` | QA strategy, test matrix |
| **Post-Mortem** | `docs/post-mortem.md` | Retrospective analysis |
| **Sprint Plan** | `production/sprints/sprint-{n}.md` | Sprint goals, tasks, estimates |

### Creating Documents from Templates

```csharp
using IntelliVerseX.Design;

await IVXDesignAssistant.Instance.CreateFromTemplateAsync(
    template: DesignTemplate.GameConcept,
    outputPath: "design/gdd/game-concept.md"
);
```

---

## 3. AI-Assisted Brainstorming

### IVXDesignAssistant

Uses `IVXAIContentGenerator` to help with design workflows:

```csharp
using IntelliVerseX.Design;

var concepts = await IVXDesignAssistant.Instance.BrainstormAsync(new BrainstormRequest
{
    Genre = "casual trivia",
    TargetAudience = "mobile, ages 18-45",
    SessionLength = "5-10 minutes",
    Monetization = "F2P with ads and IAP",
    Constraints = new[] { "single developer", "3 month timeline" },
    Count = 3,
});

foreach (var concept in concepts)
{
    Debug.Log($"--- {concept.Title} ---");
    Debug.Log($"Core Loop: {concept.CoreLoop}");
    Debug.Log($"Hook: {concept.Hook}");
    Debug.Log($"Pillars: {string.Join(", ", concept.Pillars)}");
}
```

### GDD Section Assistance

```csharp
var section = await IVXDesignAssistant.Instance.DraftSectionAsync(
    system: "combat",
    section: GDDSection.CoreMechanics,
    context: existingGDDContent
);

Debug.Log(section.Markdown);
```

---

## 4. GDD Required Sections

Every system GDD must contain these 8 sections:

| # | Section | Purpose |
|---|---------|---------|
| 1 | **Overview** | What this system does, why it exists |
| 2 | **Core Mechanics** | Rules, formulas, data definitions |
| 3 | **Player Experience** | How it feels, MDA aesthetics |
| 4 | **Progression** | How the system evolves over time |
| 5 | **Economy Integration** | Currency flows, costs, rewards |
| 6 | **Technical Requirements** | Data structures, APIs, performance |
| 7 | **Edge Cases** | Failure modes, boundary conditions |
| 8 | **Acceptance Criteria** | Testable requirements for "done" |

### Design Review

```csharp
var review = await IVXDesignReview.Instance.ReviewAsync("design/gdd/combat-gdd.md");

Debug.Log($"Score: {review.Score}/100");
foreach (var issue in review.Issues)
{
    Debug.Log($"[{issue.Severity}] Section {issue.Section}: {issue.Message}");
}
```

Review checks for: missing sections, undefined formulas, untested edge cases, missing economy integration, and acceptance criteria completeness.

---

## 5. Systems Mapping

### Creating a Systems Index

```csharp
var mapper = new IVXSystemsMapper();

mapper.AddSystem("core_loop", priority: 1, dependencies: new string[] { });
mapper.AddSystem("progression", priority: 1, dependencies: new[] { "core_loop" });
mapper.AddSystem("economy", priority: 1, dependencies: new[] { "core_loop" });
mapper.AddSystem("store", priority: 2, dependencies: new[] { "economy" });
mapper.AddSystem("achievements", priority: 2, dependencies: new[] { "progression" });
mapper.AddSystem("social", priority: 3, dependencies: new[] { "core_loop" });
mapper.AddSystem("leaderboards", priority: 3, dependencies: new[] { "progression", "social" });

var index = mapper.BuildIndex();
Debug.Log(index.ToMarkdown());
```

### Dependency Graph Output

```
Priority 1 (Must Have)
├── core_loop
├── progression → [core_loop]
└── economy → [core_loop]

Priority 2 (Should Have)
├── store → [economy]
└── achievements → [progression]

Priority 3 (Nice to Have)
├── social → [core_loop]
└── leaderboards → [progression, social]
```

### Cycle Detection

```csharp
var cycles = mapper.DetectCycles();
if (cycles.Any())
{
    Debug.LogError($"Circular dependencies found: {string.Join(" → ", cycles)}");
}
```

---

## 6. Balance Spreadsheet Generator

### Creating Balance Tables

```csharp
var balancer = new IVXBalanceSheetGenerator();

balancer.AddProgressionTable("player_levels", new ProgressionConfig
{
    MaxLevel = 100,
    BaseXP = 100,
    GrowthFunction = GrowthFunction.Polynomial,
    GrowthExponent = 1.5f,
    Columns = new[] { "xp_required", "cumulative_xp", "coin_reward", "unlock" },
});

string csv = balancer.ExportCSV();
string markdown = balancer.ExportMarkdown();
```

### AI-Assisted Balancing

```csharp
var suggestions = await IVXDesignAssistant.Instance.AnalyzeBalanceAsync(
    balanceSheet: csv,
    constraints: new[] { "Reach max level in ~60 hours", "No level should take more than 2 hours" }
);

foreach (var suggestion in suggestions)
{
    Debug.Log($"Level {suggestion.Level}: {suggestion.Issue} → {suggestion.Fix}");
}
```

---

## 7. Pitch Document Generator

### Creating Pitch Decks

```csharp
var pitch = await IVXDesignAssistant.Instance.GeneratePitchAsync(new PitchRequest
{
    ConceptDocPath = "design/gdd/game-concept.md",
    TargetAudience = PitchAudience.Investor,
    IncludeMarketAnalysis = true,
    IncludeMonetizationProjection = true,
    IncludeCompetitorAnalysis = true,
});

await pitch.SaveAsync("design/pitch.md");
```

The generated pitch document includes: elevator pitch, market opportunity, competitive landscape, monetization model, development timeline, team capabilities, and ask/use of funds.

---

## 8. Cross-Platform Design Workflow

The design tools work identically across all platforms since they operate on markdown files and AI APIs:

| Tool | Unity | Unreal | Godot | JS/Node | All Others |
|------|-------|--------|-------|---------|------------|
| Templates | Editor window | Editor utility | Plugin panel | CLI tool | CLI tool |
| AI Brainstorm | Runtime + Editor | Editor only | Runtime + Editor | Runtime | Runtime |
| Design Review | Editor window | Editor utility | CLI | CLI | CLI |
| Systems Map | Editor window | Editor utility | CLI | CLI | CLI |
| Balance Sheet | Editor + runtime | Editor only | CLI | CLI | CLI |

---

## Platform-Specific Design Considerations

### VR Games

| Design Area | VR Guidance |
|-------------|------------|
| **Core loop** | Sessions must be satisfying in 15-30 min. Design loops that respect VR fatigue. |
| **UI/UX** | All menus must be world-space. No 2D overlays. Include comfort settings in GDD. |
| **Locomotion** | Document locomotion method in Technical Requirements section. |
| **Social** | VR social features (voice chat, avatars, gestures) require dedicated GDD sections. |

### Console Games

| Design Area | Console Guidance |
|-------------|-----------------|
| **Certification** | Include a "Platform Requirements" section in GDD for TRC/XR/Lotcheck items. |
| **Controller** | Document controller layouts per platform. Include accessibility remapping. |
| **Achievements** | Map GDD achievements to platform trophy/achievement systems. |

### Mobile / WebGL

| Design Area | Mobile/Web Guidance |
|-------------|-------------------|
| **Session design** | Short sessions (2-5 min) with clear stopping points. |
| **Onboarding** | FTUE must be under 60 seconds to first meaningful interaction. |
| **Offline** | Design for intermittent connectivity. |

---

## 9. Brand Identity

Every game lives under a brand. This section maps directly to `brand_entity.json` consumed by Content-Factory for asset generation.

### Template: `design/brand-identity.md`

```markdown
# Brand Identity

## Studio
- **Brand ID:** my-studio (lowercase, hyphenated — used as S3 prefix)
- **Studio Name:** My Studio
- **Tagline:** One-line tagline (appears on landing page, social, store)
- **Mission:** Why this studio exists (1-2 sentences)
- **Personality:** playful | serious | quirky | elegant | edgy
- **Primary Emotion:** joy | excitement | wonder | tension | nostalgia

## Audience
- **Target Audience:** Casual gamers aged 18-35
- **Age Range:** 18-35
- **Geographic Markets:** US, EU, APAC, LATAM
- **Languages:** en, es, fr, de, ja, ko, zh-Hans, pt-BR

## Visual Identity
- **Imagery Style:** 2D cartoon with bold outlines and vibrant colors
- **Color Palette:** #FF6B35 (primary), #1A1A2E (dark), #E8E8E8 (light)
- **Typography:** Fredoka One (display), Nunito (body)
```

### C# API

```csharp
var brand = await IVXDesignAssistant.Instance.DraftBrandIdentityAsync(new BrandIdentityRequest
{
    StudioName = "My Studio",
    Genre = "casual trivia",
    TargetAudience = "Casual gamers aged 18-35",
    Personality = BrandPersonality.Playful,
});
```

---

## 10. Store Metadata

Captures everything needed for App Store Connect, Google Play Console, Steam, Meta Quest Store, and console storefronts. Maps to `game_context.store_config`.

### Template: `design/store-metadata.md`

```markdown
# Store Metadata

## Identifiers
- **iOS Bundle ID:** com.mystudio.mygame
- **Android Package:** com.mystudio.mygame
- **Steam App ID:** (pending — fill after Steamworks registration)
- **Meta Quest App ID:** (pending)

## Descriptions
- **App Name:** My Game — Trivia Challenge
- **Short Description (80 chars):** Test your knowledge across 20+ categories!
- **Full Description:** (2000 chars for Apple, 4000 for Google Play)
  Challenge your brain with thousands of trivia questions across 20+ categories.
  Compete with friends, climb the leaderboards, and unlock 10 unique characters.
  Daily challenges, multiplayer battles, and a season pass with exclusive rewards.

## Keywords / Tags
- **iOS Keywords (100 chars):** trivia,quiz,brain,knowledge,puzzle,multiplayer,challenge
- **Google Play Tags:** trivia, quiz, brain games, knowledge, puzzle
- **Steam Tags:** Trivia, Quiz, Casual, Multiplayer, Educational

## Ratings
- **ESRB:** E (Everyone)
- **PEGI:** 3
- **IARC:** 3+
- **Content Descriptors:** None
- **Interactive Elements:** Users Interact, In-Game Purchases

## Categories
- **iOS Primary Category:** Games — Trivia
- **iOS Secondary Category:** Games — Puzzle
- **Google Play Category:** Game — Trivia
- **Steam Genre:** Casual
- **Steam Category:** Single-player, Multi-player, Steam Achievements

## Pricing
- **Model:** Free to Play
- **IAP Price Range:** $0.99 — $19.99
- **Subscription:** None
```

### C# API

```csharp
var storeMeta = await IVXDesignAssistant.Instance.DraftStoreMetadataAsync(new StoreMetadataRequest
{
    GameName = "My Game",
    Genre = "trivia",
    ShortDescription = "Test your knowledge across 20+ categories!",
    Platforms = new[] { Platform.iOS, Platform.Android, Platform.Steam },
    ContentRating = ContentRating.Everyone,
});
```

---

## 11. Localization Plan

Defines target languages, markets, and cultural considerations. Consumed by `ivx-localization` skill and the screenshot localizer pipeline.

### Template: `design/localization-plan.md`

```markdown
# Localization Plan

## Target Languages

| Priority | Language | Locale | Market | Notes |
|----------|----------|--------|--------|-------|
| P0 | English | en-US | US, UK, AU, CA | Base language |
| P1 | Spanish | es-419 | LATAM, US Hispanic | Latin American Spanish |
| P1 | Portuguese | pt-BR | Brazil | Brazilian Portuguese |
| P1 | French | fr-FR | France, Canada | |
| P1 | German | de-DE | Germany, Austria, Switzerland | |
| P2 | Japanese | ja-JP | Japan | CJK font required |
| P2 | Korean | ko-KR | South Korea | CJK font required |
| P2 | Simplified Chinese | zh-Hans | China (mainland) | CJK font, regulatory review |
| P3 | Arabic | ar-SA | MENA | RTL support required |
| P3 | Hindi | hi-IN | India | Devanagari font required |

## RTL Support
- **Required:** Yes (Arabic, Hebrew)
- **UI Mirroring:** Full layout flip for RTL locales

## Cultural Considerations
- Japan: Honorifics in character dialog, softer color palette for store screenshots
- China: Regulatory requirements for online games, age verification
- MENA: No alcohol/gambling imagery, modest character designs

## Localized Store Assets
- Screenshots: Localized text overlays for P0 + P1 languages
- Descriptions: Full translations for all target languages
- Keywords: Per-language ASO keyword research
```

---

## 12. Structured Character Bible

Rich character definitions that map directly to `brand_entity.characters` for Content-Factory asset generation. Every field here drives sprite generation, 3D modeling, voice synthesis, and promotional art.

### Template: `design/gdd/characters/{name}.md`

```markdown
# Character: {Name}

## Identity
- **Character ID:** hero (lowercase, used as file prefix)
- **Display Name:** The Explorer
- **Role:** hero | sidekick | villain | npc | boss | narrator
- **Species:** human | robot | animal | fantasy | alien
- **Age Group:** child | teen | young_adult | adult | elder | ageless
- **Gender:** male | female | neutral | non_binary

## Appearance (drives sprite/3D generation)
- **Full Description:** A young adventurer with a glowing backpack full of books,
  wearing a blue explorer's outfit with golden accents. Has bright green eyes
  and messy brown hair.
- **Static Features:** Blue explorer outfit, golden shoulder badges, glowing green backpack
- **Dynamic Features:** Hair moves with momentum, backpack glows brighter on correct answers

## Personality
- **Personality:** Curious, encouraging, always excited to learn
- **Catchphrase:** "Let's discover something amazing!"
- **Traits:** [curious, encouraging, energetic, friendly]
- **Goals:** Explore every category of knowledge in the universe
- **Fears:** Being stumped by a question they should know
- **Backstory:** Grew up in the Library of All Things, where every book is a portal

## Voice (drives TTS / voice line generation)
- **Voice Description:** Young, enthusiastic, warm — like a favorite teacher
- **Pitch:** medium-high
- **Speed:** slightly fast
- **Accent:** neutral American English

## Relationships
- sidekick: best_friend
- villain: rival

## Game Presence
- **Appears In Games:** [my-game-id]
- **Aliases:** [The Explorer, Quiz Master]
- **Is Visible:** true
- **Identifier In Scene:** HERO (used for Spine/animation references)
```

### C# API

```csharp
var character = await IVXDesignAssistant.Instance.DraftCharacterBibleAsync(new CharacterBibleRequest
{
    Name = "Hero",
    Role = CharacterRole.Hero,
    Genre = "trivia",
    ArtStyle = "2D cartoon",
    Personality = "curious, encouraging, energetic",
    Species = CharacterSpecies.Human,
});
```

---

## 13. Media & Sound Requirements

Defines audio, trailer, and social media specifications that drive Content-Factory pipelines.

### Template: `design/media-requirements.md`

```markdown
# Media & Sound Requirements

## Sound Config (drives ivx-game-audio-factory)
- **Genre Preset:** puzzle
- **BGM BPM Range:** 80-120
- **SFX Style:** bright_digital
- **Voice Lines:** Yes (per character, per action: idle, correct, wrong, celebrate)
- **Ambient:** Yes (per environment/theme)
- **Stingers:** victory, defeat, achievement_unlock, level_up, combo

## Trailer Specifications (drives ivx-store-launcher)
- **Duration:** 30-45 seconds
- **Resolution:** 1080p + 4K variant
- **Target Platforms:** YouTube, App Store, Google Play, Steam
- **Music:** Use generated BGM (upbeat variant)
- **Structure:** Hook (5s) → Gameplay (15s) → Features (10s) → CTA (5s)

## Social Media Sizes
- Twitter/X Banner: 1500x500
- Facebook Cover: 820x312
- Discord Banner: 960x540
- YouTube Thumbnail: 1280x720
- Instagram Post: 1080x1080
- TikTok Cover: 1080x1920

## Icon Requirements
- iOS: 1024x1024 (no rounded corners, no transparency)
- Android: 512x512 (adaptive icon layers if possible)
- Steam: 460x215 header, 231x87 + 467x181 capsules
```

---

## 14. GDD-to-Entity Export

The automated bridge between design documents and Content-Factory JSON. Converts the structured GDD sections above into `brand_entity.json` and `game_context.json` ready for S3 upload.

### Usage

```bash
python tools/asset-pipeline/gdd_to_entity.py \
  --gdd-dir design/ \
  --output-dir output/entities/ \
  --brand-id my-studio \
  --game-id my-game
```

### What It Reads

| GDD File | Maps To |
|----------|---------|
| `design/brand-identity.md` | `brand_entity.json` root fields |
| `design/store-metadata.md` | `game_context.store_config` |
| `design/localization-plan.md` | `brand_entity.languages`, `geographic_markets` |
| `design/gdd/characters/*.md` | `brand_entity.characters[]` |
| `design/gdd/game-concept.md` | `game_context` root (genre, platforms, features) |
| `design/media-requirements.md` | `game_context.sound_config`, `media` |

### What It Outputs

```
output/entities/
├── brand_entity.json      # Full brand + characters for Content-Factory
├── game_context.json      # Game-specific config for pipeline orchestration
└── export_report.md       # Summary of extracted fields + any warnings
```

### S3 Upload

```bash
aws s3 cp output/entities/brand_entity.json \
  s3://intelli-verse-x-media/agent-assets/my-studio/brand_entity.json

aws s3 cp output/entities/game_context.json \
  s3://intelli-verse-x-media/agent-assets/my-studio/my-game/game.json
```

### Then Generate Everything

```bash
python -m pipelines.runner run --config configs/pipelines/ivx_full_game.yaml \
  --brand_id my-studio --game_id my-game
```

---

## 15. Target Engine & Platform

Every GDD must declare which engine and language the game will be built in. This drives the Starter Project Generator (Section 16) and determines which SDK features are real vs. stub (see `ivx-cross-platform` skill).

### Template: `design/target-platform.md`

```markdown
# Target Engine & Platform

## Primary Engine
- **Engine:** unity | unreal | godot | roblox | defold | cocos2dx | javascript | cpp | java | flutter | web3
- **Engine Version:** 6000.3.6f1 (Unity) | 5.4 (Unreal) | 4.3 (Godot) | etc.
- **Language:** C# | C++ | GDScript | Luau | Lua | TypeScript | Java | Dart

## Deployment Targets
- **Primary Platform:** android | ios | webgl | steam | quest | ps5 | xbox | switch
- **Secondary Platforms:** [list additional targets]
- **Architecture:** mobile-first | desktop-first | vr-first | web-first | console-first

## SDK Feature Tiers (auto-populated from engine choice)
- **Tier 1 (Real):** Features with full implementations for your engine
- **Tier 2 (Stub → Real):** Features that compile but need HTTP backend calls
- **Tier 3 (N/A):** Features not applicable to your engine/platform

## Starter Project Options
- **Include Auth Demo:** true (guest + email + social sign-in flow)
- **Include Economy Demo:** true (wallet display, store, IAP wiring)
- **Include Retention Demo:** true (daily rewards, streaks, energy, achievements)
- **Include Social Demo:** true (leaderboards, friend challenges, Discord)
- **Include Multiplayer Demo:** false (set true if multiplayer is in scope)
- **Include AI Demo:** false (set true if AI features are in scope)
- **Demo UI Style:** modern-dark | modern-light | retro | minimal | custom
```

### Engine-to-Feature Mapping (auto-resolved)

| Engine | Auth | Economy | Achievements | Streaks | Energy | Leaderboard | Multiplayer | AI | Ads |
|--------|------|---------|-------------|---------|--------|-------------|-------------|-----|-----|
| **Unity** | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| **JavaScript** | Y | Y | Y | Y | Y | Y | Y | S | - |
| **Java/Android** | Y | Y | Y | Y | Y | Y | Y | S | Y |
| **Flutter** | Y | S | S | S | S | S | S | S | S |
| **Unreal** | S | S | S | S | S | S | S | S | - |
| **Godot** | S | S | S | S | S | S | S | S | - |
| **Roblox** | Y | Y | Y | Y | Y | Y | - | Y | - |
| **Defold** | S | S | S | S | S | S | S | S | - |
| **C++** | S | S | S | S | S | S | S | S | - |
| **Cocos2d-x** | S | S | S | S | S | S | S | S | - |
| **Web3** | Y | S | S | S | S | S | - | S | - |

Y = real implementation, S = stub (compiles, calls HTTP RPCs), - = not applicable.

### C# API

```csharp
var platform = await IVXDesignAssistant.Instance.ResolvePlatformFeaturesAsync(new PlatformRequest
{
    Engine = GameEngine.Unity,
    EngineVersion = "6000.3.6f1",
    PrimaryPlatform = DeployTarget.Android,
    SecondaryPlatforms = new[] { DeployTarget.iOS, DeployTarget.WebGL },
});

Debug.Log($"Real features: {string.Join(", ", platform.RealFeatures)}");
Debug.Log($"Stub features: {string.Join(", ", platform.StubFeatures)}");
```

---

## 16. Starter Project Generator

The crown jewel of the GDD workflow. After filling out the GDD sections (concept, brand, characters, economy, target engine), run the generator to produce a **complete, runnable project** with every IntelliVerseX SDK feature wired up and a demo UI that showcases all of them.

### What You Get

A fully functional "Hello IntelliVerseX" game project customized with your GDD data:

| Feature Area | What's Wired Up |
|-------------|----------------|
| **Bootstrap** | SDK initialization, config loading, DontDestroyOnLoad singletons |
| **Authentication** | Guest login, email/password, social sign-in (Google, Apple, Facebook), session persistence |
| **Economy** | Wallet display (coins + gems), store panel with purchasable items, IAP hooks, reward popups |
| **Achievements** | Achievement list panel, progress tracking, unlock animations, badge display |
| **Daily Rewards** | Calendar grid UI, streak counter, progressive reward tiers, streak freeze |
| **Energy System** | Energy bar, refill timer, energy purchase option |
| **Progression** | XP bar, level display, level-up celebration, unlock gates |
| **Leaderboards** | Global + friends tabs, rank display, score submission |
| **Streaks** | Daily login streak tracker, streak milestone rewards |
| **Store** | Categorized store (consumables, cosmetics, bundles, season pass), purchase flow |
| **Feature Flags** | Satori flag checks gating UI sections, A/B experiment hooks |
| **Analytics** | Satori event tracking on every user action (screen view, purchase, achievement) |
| **Multiplayer** | Lobby creation/join, matchmaking queue, real-time game state sync (if enabled) |
| **AI Chat** | AI NPC dialog panel, voice input/output hooks (if enabled) |
| **Settings** | Audio volume sliders, notification toggles, account management, logout |
| **Notifications** | Local notification scheduling, push token registration |
| **Deep Links** | URL scheme handling for friend invites and promotions |
| **FTUE** | First-time user experience tutorial flow (3-step onboarding) |

### Usage

```bash
python tools/boilerplate/generate_starter.py \
  --gdd-dir design/ \
  --engine unity \
  --output-dir output/starter-project/ \
  --brand-id my-studio \
  --game-id my-game
```

Or with explicit entity JSON (if already exported):

```bash
python tools/boilerplate/generate_starter.py \
  --brand-entity output/entities/brand_entity.json \
  --game-context output/entities/game_context.json \
  --engine godot \
  --output-dir output/starter-project/
```

### Supported Engines

| Engine | Output Structure | Demo UI |
|--------|-----------------|---------|
| **Unity** | Full UPM-compatible project with scenes, prefabs, C# scripts, and Canvas-based UI | Complete UGUI with panels for every feature |
| **JavaScript/TypeScript** | npm project with Express server, React frontend, WebSocket client | Browser-based SPA with all feature panels |
| **Godot 4** | Godot project with .tscn scenes, GDScript autoloads, Control-based UI | Godot UI with TabContainer for all features |
| **Roblox** | Rojo project with Luau modules, StarterGui, ReplicatedStorage | Roblox ScreenGui with frames for all features |
| **Java/Android** | Gradle project with Activities, Fragments, XML layouts | Material Design UI with ViewPager tabs |
| **Flutter** | pub.dev project with Dart widgets, Provider state management | Material 3 UI with BottomNavigationBar |
| **Unreal** | .uplugin with C++ classes, Blueprint widgets, UMG UI | UMG Widget Blueprint panels |
| **Defold** | Defold project with Lua modules, GUI scenes | Defold GUI nodes for each feature |
| **C++** | CMake project with ImGui demo UI | ImGui windows for each feature panel |
| **Cocos2d-x** | CMake project with C++ scenes | Cocos UI panels |
| **Web3** | npm project with ethers.js + React + wallet connect | Web3-enabled SPA with wallet + game features |

### What Gets Customized from GDD

The generator reads your GDD outputs and customizes:

| GDD Source | Customization |
|------------|---------------|
| `brand_entity.json` → `name` | Window title, splash screen, header text |
| `brand_entity.json` → `color_palette` | UI theme colors (primary, secondary, background) |
| `brand_entity.json` → `typography` | Font family in UI styles |
| `brand_entity.json` → `tagline` | Subtitle on splash/login screen |
| `brand_entity.json` → `characters[]` | Character avatars in leaderboard, profile |
| `game_context.json` → `game_id` | SDK bootstrap config, S3 asset paths |
| `game_context.json` → `platforms` | Platform-specific build settings |
| `game_context.json` → `store_config` | Store items, IAP product IDs, price tiers |
| `game_context.json` → `sound_config` | Audio bus config, BGM assignments per screen |
| `game_context.json` → `features` | Which demo panels to include/exclude |

### Generated Project Structure (Unity Example)

```
output/starter-project/
├── Assets/
│   ├── _MyGame/
│   │   ├── Scenes/
│   │   │   ├── Bootstrap.unity          # Auto-init scene (build index 0)
│   │   │   ├── Login.unity              # Auth flow scene
│   │   │   ├── MainMenu.unity           # Hub with all feature panels
│   │   │   └── GamePlay.unity           # Placeholder core gameplay
│   │   ├── Scripts/
│   │   │   ├── Bootstrap/
│   │   │   │   └── GameBootstrap.cs     # IVXBootstrap + config + DontDestroyOnLoad
│   │   │   ├── Auth/
│   │   │   │   └── AuthFlowController.cs # Guest → Email → Social sign-in
│   │   │   ├── UI/
│   │   │   │   ├── MainMenuController.cs
│   │   │   │   ├── WalletDisplay.cs     # Coin + gem counters (Hiro economy)
│   │   │   │   ├── StorePanel.cs        # Store items grid + purchase flow
│   │   │   │   ├── AchievementsPanel.cs
│   │   │   │   ├── DailyRewardsPanel.cs # Calendar grid + streak
│   │   │   │   ├── EnergyBar.cs         # Energy + refill timer
│   │   │   │   ├── ProgressionBar.cs    # XP + level
│   │   │   │   ├── LeaderboardPanel.cs  # Global + friends tabs
│   │   │   │   ├── StreakPanel.cs        # Login streak + milestones
│   │   │   │   ├── SettingsPanel.cs      # Audio, notifications, account
│   │   │   │   ├── FTUEController.cs     # 3-step onboarding
│   │   │   │   └── NotificationPanel.cs
│   │   │   ├── Analytics/
│   │   │   │   └── AnalyticsWiring.cs   # Satori event hooks on all actions
│   │   │   ├── Engagement/
│   │   │   │   ├── DailyRewardsClaimer.cs
│   │   │   │   ├── StreakTracker.cs
│   │   │   │   └── RetentionManager.cs  # Push scheduling, re-engagement
│   │   │   └── Multiplayer/             # (if enabled)
│   │   │       ├── LobbyController.cs
│   │   │       └── MatchmakingUI.cs
│   │   ├── Prefabs/
│   │   │   ├── [IVX Bootstrap].prefab
│   │   │   ├── StoreItemCard.prefab
│   │   │   ├── AchievementCard.prefab
│   │   │   ├── LeaderboardRow.prefab
│   │   │   └── DailyRewardSlot.prefab
│   │   ├── Resources/
│   │   │   └── IVXBootstrapConfig.asset # Pre-filled with game_id + server
│   │   └── _MyGame.asmdef
│   └── _IntelliVerseXSDK/              # SDK package (UPM reference)
├── Packages/
│   └── manifest.json                    # UPM manifest with SDK dependency
├── ProjectSettings/
│   └── ProjectSettings.asset           # Company name, product name from brand
└── README.md                           # Getting started guide
```

### Generated Project Structure (JavaScript Example)

```
output/starter-project/
├── src/
│   ├── index.ts                  # Entry point + IVXClient init
│   ├── config.ts                 # Game ID, server host from game_context
│   ├── auth/
│   │   └── auth-flow.ts          # Guest → email → social sign-in
│   ├── ui/
│   │   ├── App.tsx               # React root with router
│   │   ├── LoginScreen.tsx
│   │   ├── MainMenu.tsx          # Tab navigation hub
│   │   ├── WalletDisplay.tsx
│   │   ├── StorePanel.tsx
│   │   ├── AchievementsPanel.tsx
│   │   ├── DailyRewardsPanel.tsx
│   │   ├── EnergyBar.tsx
│   │   ├── LeaderboardPanel.tsx
│   │   ├── ProgressionBar.tsx
│   │   ├── SettingsPanel.tsx
│   │   └── FTUEOverlay.tsx
│   ├── analytics/
│   │   └── satori-wiring.ts      # Event tracking on all user actions
│   ├── engagement/
│   │   ├── daily-rewards.ts
│   │   ├── streak-tracker.ts
│   │   └── retention.ts
│   └── styles/
│       └── theme.css             # Brand colors + typography
├── package.json
├── tsconfig.json
└── README.md
```

### Generated Project Structure (Godot 4 Example)

```
output/starter-project/
├── project.godot                 # Project settings with game name
├── scenes/
│   ├── bootstrap.tscn            # Autoload scene
│   ├── login.tscn                # Auth flow
│   ├── main_menu.tscn            # TabContainer with all panels
│   └── gameplay.tscn             # Placeholder
├── scripts/
│   ├── autoload/
│   │   ├── ivx_bootstrap.gd      # IVXAutoload config + init
│   │   └── analytics.gd          # Satori event bus
│   ├── auth/
│   │   └── auth_flow.gd
│   ├── ui/
│   │   ├── wallet_display.gd
│   │   ├── store_panel.gd
│   │   ├── achievements_panel.gd
│   │   ├── daily_rewards.gd
│   │   ├── energy_bar.gd
│   │   ├── leaderboard_panel.gd
│   │   ├── progression_bar.gd
│   │   ├── settings_panel.gd
│   │   └── ftue.gd
│   └── engagement/
│       ├── daily_rewards_claimer.gd
│       └── streak_tracker.gd
├── addons/
│   └── intelliversex/            # SDK addon (git submodule or copy)
├── themes/
│   └── game_theme.tres           # Brand colors + fonts
└── README.md
```

### CLI Options

```
python tools/boilerplate/generate_starter.py --help

Options:
  --gdd-dir PATH          Path to design/ folder with GDD markdown files
  --brand-entity PATH     Direct path to brand_entity.json (skips GDD parsing)
  --game-context PATH     Direct path to game_context.json (skips GDD parsing)
  --engine ENGINE         Target engine: unity|unreal|godot|roblox|defold|
                          cocos2dx|javascript|cpp|java|flutter|web3
  --engine-version VER    Engine version (e.g., "6000.3.6f1" for Unity)
  --output-dir PATH       Where to write the generated project
  --brand-id ID           Brand identifier (used for S3 paths)
  --game-id ID            Game identifier
  --features FEATURES     Comma-separated feature list to include
                          (default: auth,economy,achievements,streaks,energy,
                          leaderboard,progression,store,analytics,settings,ftue)
  --include-multiplayer   Include multiplayer demo (off by default)
  --include-ai            Include AI chat demo (off by default)
  --ui-style STYLE        UI theme: modern-dark|modern-light|retro|minimal
  --dry-run               Print what would be generated without writing files
```

### How the Generator Works

```
┌──────────────────────┐     ┌───────────────────────┐
│ brand_entity.json    │────▶│                       │
│ game_context.json    │     │  generate_starter.py  │
│ target-platform.md   │────▶│                       │
└──────────────────────┘     │  1. Detect engine     │
                             │  2. Load templates     │
       ┌────────────────┐    │  3. Substitute vars    │
       │ templates/      │──▶│  4. Write project     │
       │  unity/         │    │  5. Write README      │
       │  javascript/    │    └───────────┬───────────┘
       │  godot/         │                │
       │  roblox/        │                ▼
       │  ...            │    ┌───────────────────────┐
       └────────────────┘    │ output/starter-project/ │
                             │  Runnable game with    │
                             │  all SDK features      │
                             │  wired up + demo UI    │
                             └───────────────────────┘
```

### Template Variables

Templates use `{{variable}}` placeholders that get replaced:

| Variable | Source | Example |
|----------|--------|---------|
| `{{game_name}}` | `brand_entity.name` or `game_context.name` | "QuizVerse" |
| `{{game_id}}` | `game_context.game_id` | "quiz-verse" |
| `{{brand_id}}` | `brand_entity.brand_id` | "my-studio" |
| `{{server_host}}` | Config or default | "nakama.intelli-verse-x.ai" |
| `{{server_port}}` | Config or default | "7350" |
| `{{server_key}}` | Config or default | "defaultkey" |
| `{{primary_color}}` | `brand_entity.color_palette[0]` | "#FF6B35" |
| `{{secondary_color}}` | `brand_entity.color_palette[1]` | "#1A1A2E" |
| `{{background_color}}` | `brand_entity.color_palette[2]` | "#E8E8E8" |
| `{{display_font}}` | `brand_entity.typography.display` | "Fredoka One" |
| `{{body_font}}` | `brand_entity.typography.body` | "Nunito" |
| `{{tagline}}` | `brand_entity.tagline` | "Test Your Brain!" |
| `{{bundle_id}}` | `game_context.store_config.ios_bundle_id` | "com.mystudio.mygame" |
| `{{package_name}}` | `game_context.store_config.android_package` | "com.mystudio.mygame" |
| `{{company_name}}` | `brand_entity.studio_name` | "My Studio" |
| `{{initial_coins}}` | `game_context.economy.initial_coins` or 100 | "100" |
| `{{initial_gems}}` | `game_context.economy.initial_gems` or 10 | "10" |
| `{{max_energy}}` | `game_context.energy.max` or 5 | "5" |
| `{{energy_refill_minutes}}` | `game_context.energy.refill_minutes` or 30 | "30" |

### What "Fully Wired" Means

Every generated script is connected end-to-end:

1. **Bootstrap** → Initializes IVXBootstrap with config → Authenticates (guest by default) → Loads Hiro systems → Initializes Satori
2. **Auth Flow** → Login screen with guest/email/social buttons → OnAuthSuccess fires → Transitions to MainMenu
3. **Wallet** → Subscribes to `IVXEconomyManager.OnBalanceChanged` → Updates coin/gem counters in real-time
4. **Store** → Loads store items from Hiro → Purchase button calls `IVXStore.PurchaseAsync()` → Success triggers wallet refresh + reward popup
5. **Achievements** → Loads from `IVXAchievements.ListAsync()` → Progress bars update on events → Claim button calls `ClaimAsync()`
6. **Daily Rewards** → Calendar shows 7-day grid → `IVXStreaks.ClaimDailyRewardAsync()` → Streak counter updates → Missed days shown
7. **Energy** → Bar shows `currentEnergy / maxEnergy` → Timer counts down to next refill → "Buy Energy" calls economy
8. **Progression** → XP bar fills on actions → Level-up triggers celebration → New unlocks gated by level
9. **Leaderboard** → Tabs for Global/Friends → `IVXLeaderboards.ListAsync()` → Submits score via `SubmitAsync()`
10. **Analytics** → Every screen transition calls `IVXSatori.TrackEvent("screen_view", {screen})` → Every purchase, achievement, login tracked
11. **Feature Flags** → `IVXSatori.GetFlagAsync("new_store_ui")` → Conditionally shows new vs. old store layout
12. **Settings** → Volume sliders persist to PlayerPrefs → Notification toggle → Logout calls `IVXAuth.SignOutAsync()`
13. **FTUE** → 3-step overlay on first launch → Highlights wallet, play button, daily rewards → Sets `ftue_completed` flag

---

## 17. Integration Mode (Wire Existing vs From Scratch)

The SDK supports two modes for integrating generated boilerplates into a game project:

### Option A: From Scratch
Generates a completely new project with all 18 features wired up. Best for new games.

```bash
python tools/boilerplate/generate_starter.py \
  --engine unity \
  --gdd ./gdd/ \
  --output ./my-new-game/
```

### Option B: Wire Into Existing Project
Scans an existing project, detects its architecture (e.g., Zenject, MVC, Signal Bus), and generates **only the missing SDK integration code**. Existing files are never overwritten; adapters bridge existing auth/economy to IVX.

**How to specify in GDD (`design/integration.md`):**
```markdown
## Integration Mode
- [ ] **From Scratch**
- [x] **Wire Existing**

### If Wire Existing:
- Project path: `./path/to/existing/project`
- Keep existing auth? [yes/no]
- Keep existing economy? [yes/no]
- Keep existing analytics? [yes/no]
- Features to add: [store, achievements, daily_rewards, energy, ftue, retention, ...]
```

**Commands:**
```bash
# 1. Analyze existing project
python tools/boilerplate/project_analyzer.py --project ./my-game/ --output project_profile.json

# 2. Wire IVX matching project patterns
python tools/boilerplate/wire_integrator.py --project ./my-game/ --profile project_profile.json --gdd ./gdd/
```

---

## Checklist

### Design Phase
- [ ] Game concept document created with core loop and pillars
- [ ] MDA analysis completed for target aesthetics
- [ ] Player type analysis done (Bartle/Quantic)
- [ ] Systems index created with dependency graph
- [ ] Per-system GDDs created with all 8 required sections
- [ ] Design review passed (score > 80/100) on all system GDDs
- [ ] Economy model document created
- [ ] Balance spreadsheets generated and reviewed
- [ ] Pitch document generated (if seeking funding)

### Identity & Metadata Phase
- [ ] Brand identity section completed with all fields
- [ ] Store metadata section completed for all target stores
- [ ] Localization plan created with priority tiers and RTL/CJK notes
- [ ] Character bibles created for all characters with appearance and voice fields
- [ ] Media & sound requirements defined (sound config, trailer specs, social sizes)

### Export Phase
- [ ] `gdd_to_entity.py` run to export `brand_entity.json` and `game_context.json`
- [ ] Exported JSON uploaded to S3 (or local path configured)

### Engine & Starter Project Phase
- [ ] Target engine and language declared in `design/target-platform.md`
- [ ] Engine version specified and validated against SDK compatibility
- [ ] Deployment targets listed (primary + secondary platforms)
- [ ] Feature tiers reviewed (real vs. stub for chosen engine)
- [ ] Starter project features selected (auth, economy, retention, multiplayer, AI)
- [ ] `generate_starter.py` run to produce fully-wired demo project
- [ ] Generated project compiles and runs without errors
- [ ] Auth flow tested (guest login → main menu)
- [ ] Economy wiring verified (wallet displays, store purchases work)
- [ ] Daily rewards / streaks display correctly
- [ ] Leaderboard loads and displays data
- [ ] Satori analytics events fire on all user actions
- [ ] Feature flags conditionally gate UI elements
- [ ] FTUE tutorial plays on first launch

### Platform-Specific
- [ ] VR comfort and session design documented (if targeting VR)
- [ ] Console certification requirements listed (if targeting consoles)
- [ ] Mobile session and onboarding constraints defined (if targeting mobile)
