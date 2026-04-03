# IntelliVerseX SDK & Agent Skills — End-to-End Analysis

**Date:** April 2, 2026
**Scope:** Full SDK, 22 AI agent skills, tools, CI/CD, cross-platform coverage
**Audience:** Indie studios, AAA teams, investors, platform partners

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [SDK Maturity Assessment](#2-sdk-maturity-assessment)
3. [Skill-by-Skill Gap Analysis](#3-skill-by-skill-gap-analysis)
4. [Studio Perspective Analysis](#4-studio-perspective-analysis)
5. [Platform Coverage Analysis](#5-platform-coverage-analysis)
6. [Engine Coverage Analysis](#6-engine-coverage-analysis)
7. [Competitive Landscape](#7-competitive-landscape)
8. [Recommended Additions](#8-recommended-additions)
9. [Skill Publishing Strategy](#9-skill-publishing-strategy)
10. [Risk Assessment](#10-risk-assessment)
11. [Roadmap Recommendations](#11-roadmap-recommendations)

---

## 1. Executive Summary

IntelliVerseX SDK is a **lifecycle-spanning game development platform** with 22 AI agent skills, 11 engine SDKs, 33+ Hiro live-ops systems, 7 AI subsystems, and production-tested Unity tooling. It occupies a unique position: no other SDK covers the full arc from game design document through asset creation, backend integration, shipping, and live retention.

### Strengths

- **Lifecycle coverage** — From GDD templates to live A/B testing. No competitor covers this range.
- **AI-native** — 7 subsystems (NPC dialog, voice, moderation, content gen, profiling) plus procedural content generation. AI is a first-class citizen, not a bolt-on.
- **Open-source backend** — Nakama + Hiro + Satori means self-hostable, no vendor lock-in, no per-MAU fees at scale.
- **22 agent skills** — Skills work with Cursor, Claude Code, Windsurf, Devin, Copilot, and any SKILL.md-compatible agent. This is a distribution moat.
- **Real production code** — 312+ C# files across Unity SDK modules, not just documentation.
- **Context engineering** — `.cursor/` authority system, memory JSON, anti-patterns, and context validation in CI. This is rare sophistication.

### Weaknesses

- **Unity-heavy** — Non-Unity SDKs are Nakama wrappers with stubs for AI, Hiro, Discord, Satori, and advanced multiplayer. The feature gap is wide.
- **Aspirational APIs** — Several skill documents describe `IVX*` classes that exist in documentation but may not have corresponding implementation. Live-ops has 10+ "Planned" managers.
- **No automated security scanning** — No CodeQL, dependency review, or SAST in CI.
- **CI depends on secrets** — Unity tests and builds are skipped without `UNITY_LICENSE` / `UNITY_SERIAL`, meaning most forks and PRs run without test coverage.
- **Music generation is a stub** — `IVXMusicGenerator` is explicitly documented as a wrapper placeholder.

---

## 2. SDK Maturity Assessment

### Module Maturity Matrix

| Module | Unity Status | Non-Unity Status | Production Confidence |
|--------|:------------:|:----------------:|:---------------------:|
| **Core / Bootstrap** | Production | Real (Nakama clients) | High |
| **Identity / Auth** | Production | Real (device/email/social) | High |
| **Backend (Nakama)** | Production | Real (RPC, storage, leaderboards) | High |
| **Wallet / Economy** | Production | Real (Hiro RPC) | High |
| **Leaderboards** | Production | Real | High |
| **Cloud Storage** | Production | Real | High |
| **Analytics (Satori)** | Production | Stub | Medium |
| **Social / Friends** | Production | Stub/Planned | Medium |
| **Localization** | Production | Not started | Low (Unity only) |
| **Quiz System** | Production | Not started | Low (Unity only) |
| **Monetization (Ads/IAP)** | Production | Not started | Low (Unity only) |
| **AI (7 subsystems)** | Production | Stub | Medium |
| **Multiplayer (real-time)** | Production | Partial (JS, Godot, Defold) | Medium |
| **Hiro Live-Ops (33 systems)** | Production | RPC-accessible | Medium-High |
| **Discord Social** | Production | Stub | Medium |
| **Platform (XR/Console/WebGL)** | Production | Partial (Unreal, Godot VR) | Medium |
| **V2 APIs (next-gen)** | In Progress | Not started | Low |
| **Notifications** | In Progress | Not started | Low |
| **Retention (streaks, daily rewards)** | Production | RPC-accessible | Medium-High |
| **Seasons / Fortune Wheel** | Production | RPC-accessible | Medium |
| **Tournaments / Leagues** | In Progress | RPC-accessible | Medium |

### Code Volume

| Area | Files | Assessment |
|------|------:|-----------|
| `Assets/Intelli-verse-X-SDK/` (core UPM) | ~175 .cs | Production-quality, well-structured |
| `Assets/_IntelliVerseXSDK/` (extended) | ~137 .cs | Production, depends on server setup |
| Assembly definitions | 19+ | Clean module boundaries |
| SDKs/ (non-Unity) | 11 dirs | README + real Nakama code; AI/Hiro/Discord stubbed |
| Agent Skills | 22 SKILL.md | Comprehensive documentation, some aspirational APIs |
| Tools | 30 files | Functional Python CLI + PowerShell packaging |
| CI/CD Workflows | 10 | Good coverage; Unity tests gated on secrets |

---

## 3. Skill-by-Skill Gap Analysis

### Design Phase Skills

#### ivx-game-design-studio
| Aspect | Status | Gap |
|--------|--------|-----|
| GDD templates (15) | Documented | Template files at `design/gdd/...` may not exist until first use — no pre-seeded templates in repo |
| AI brainstorming | Documented | Depends on `IVXAIContentGenerator` which requires API key and server |
| Design review (8-section) | Documented | No automated scoring — manual AI review only |
| Systems mapping | Documented | No visual graph output (text/JSON only) |
| **Recommendation** | | Ship pre-seeded GDD templates as actual files. Add Mermaid diagram generation for systems maps. Add a design-review scoring rubric that works offline. |

#### ivx-economy-simulator
| Aspect | Status | Gap |
|--------|--------|-----|
| Currency flow modeling | Documented | No standalone simulator binary — requires Unity or JS runtime |
| Monte Carlo simulation | Documented | Only "full sim" on Unity + JS; other engines monitor-only |
| Hiro config deployment | Documented | Config format must match Hiro schema exactly — no validation tool |
| **Recommendation** | | Build a standalone CLI economy simulator (Python or Node) that works without any engine. Add Hiro config schema validation before deploy. |

#### ivx-narrative-engine
| Aspect | Status | Gap |
|--------|--------|-----|
| Branching dialog | Documented | No visual dialog editor — code/JSON only |
| Ink/Yarn import | Documented | Import utilities may be aspirational — no `.cs` found for parser |
| AI dialog generation | Documented | Requires AI backend + API key |
| **Recommendation** | | Verify Ink/Yarn importers exist in codebase. Consider a web-based dialog editor tool. |

### Build Phase Skills

#### ivx-asset-pipeline
| Aspect | Status | Gap |
|--------|--------|-----|
| 2D scaffolding | **Real** — `scaffold_character.py` exists | Works, tested |
| 3D templates | **Real** — JSON templates exist | No actual 3D model generation (templates only) |
| Spritesheet generation | **Real** — `generate_spritesheet.py` exists | Requires Pillow |
| Validation | **Real** — 5 validators, 9 schemas | Not wired into CI/CD |
| **Recommendation** | | Add asset validation to CI pipeline. Add 3D model import validation (FBX/glTF header checks). |

#### ivx-ai-integration
| Aspect | Status | Gap |
|--------|--------|-----|
| 7 AI subsystems | Real Unity code | Non-Unity: all stubs |
| Multi-provider support | Real | No cost estimation or token budget tooling |
| Mock mode | Real | Good for development |
| **Recommendation** | | Add token budget calculator. Add cost estimation per session. Priority: port NPC dialog to at least Unreal + Godot. |

#### ivx-procedural-ai
| Aspect | Status | Gap |
|--------|--------|-----|
| Level generation | Documented | Graph-based approach described; implementation depth unclear |
| NPC generation | Documented | Personality + behavior trees |
| Music generation | **Explicitly stub** | `IVXMusicGenerator` is a wrapper placeholder for Suno/Udio |
| Asset generation (DALL-E) | Documented | Cost and content policy compliance not addressed |
| **Recommendation** | | Remove music stub from marketing or label clearly as "coming soon." Add DALL-E cost calculator. Add content policy filter for generated assets. |

#### ivx-multiplayer
| Aspect | Status | Gap |
|--------|--------|-----|
| Unity lobbies/matchmaking | Real | Full lifecycle |
| Non-Unity real-time | Partial | JS, Godot, Defold have socket; others stub |
| Server authority | Documented | Pattern described, implementation is game-specific |
| **Recommendation** | | Priority: port real-time to Unreal (large market). Provide server-authority example project. |

#### ivx-accessibility
| Aspect | Status | Gap |
|--------|--------|-----|
| Color blind simulation | Documented | No automated screenshot comparison tool |
| Screen reader bridge | Documented | TalkBack/VoiceOver/Narrator integration is complex — verify implementation |
| WCAG audit | Documented | No CLI audit tool — manual AI review only |
| **Recommendation** | | Build automated accessibility audit that checks contrast ratios, touch target sizes, and font scale ranges. This is a major differentiator if real. |

#### ivx-security-anticheat
| Aspect | Status | Gap |
|--------|--------|-----|
| Server validation | Documented | Pattern-based, requires per-game implementation |
| Memory tamper detection | Documented | Client-side only — explicitly weak on WebGL |
| Ban system | Documented | Requires Nakama server-side implementation |
| **Recommendation** | | Provide server-side Nakama TypeScript hooks for common validation patterns (score validation, economy transaction validation). |

### Ship Phase Skills

#### ivx-devops-cicd
| Aspect | Status | Gap |
|--------|--------|-----|
| GitHub Actions templates | 10 real workflows | Good coverage |
| Unity builds | Real but secret-gated | Forks/PRs get no test coverage |
| Store submission | Documented | `SubmitAsync` API may be aspirational |
| Version management | Partially real | Hard-coded `5.8.0` in platform validation workflow |
| **Recommendation** | | Fix version sourcing in CI (read from `package.json`). Add free-tier Unity CI option documentation. Provide store submission scripts (fastlane + steamcmd). |

#### ivx-quality-gates
| Aspect | Status | Gap |
|--------|--------|-----|
| Context validation | **Real** — `validate_context.py` | Works in CI |
| TODO baseline | **Real** but baseline file may be missing | `todo_fixme_cs.tsv` absence causes CI failure |
| UPM export | **Real** — `reorganize_for_upm.py` | Works |
| Asset validation in CI | **Not wired** | Tools exist but no workflow runs them |
| **Recommendation** | | Wire asset-pipeline validators into CI. Ensure baseline TSV is committed. Add VR/console/WebGL-specific quality gates. |

#### ivx-crashlytics
| Aspect | Status | Gap |
|--------|--------|-----|
| Multi-backend support | Documented (Nakama/Sentry/Firebase/Backtrace) | Many backends increases testing surface |
| ANR detection | Documented | Android-specific |
| Breadcrumbs | Documented | Useful pattern |
| **Recommendation** | | Pick one recommended backend (Sentry) and make it the default path with full example. Others as documented alternatives. |

#### ivx-cross-platform
| Aspect | Status | Gap |
|--------|--------|-----|
| Feature matrix | Real and honest | Stub-heavy for non-Unity |
| Porting guide | Documented | 5-step process |
| VR/Console/WebGL guides | Documented | Platform-specific sections |
| **Recommendation** | | Add automated feature parity checker that compares non-Unity SDKs against Unity API surface. |

### Grow Phase Skills

#### ivx-live-ops
| Aspect | Status | Gap |
|--------|--------|-----|
| Hiro managers (economy, energy, streaks, etc.) | Real Unity code | 10+ managers listed as "Planned" |
| Satori integration | Real | Flags, experiments, events |
| **Recommendation** | | Prioritize shipping: Push, DailyMissions, SeasonPass, League. These are the highest-value retention mechanics. |

#### ivx-analytics-pipeline
| Aspect | Status | Gap |
|--------|--------|-----|
| Event taxonomy | Documented | No pre-built taxonomy file in repo |
| Funnel builder | Documented | API-level only |
| Data lake export | Documented | BigQuery/Snowflake/Redshift/S3 |
| **Recommendation** | | Ship a default event taxonomy JSON with 50+ common game events. Add a funnel visualization tool (web dashboard). |

#### ivx-monetization
| Aspect | Status | Gap |
|--------|--------|-----|
| Ad mediation | Real Unity code | Unity-only; no other engine |
| IAP | Real with server validation | Good |
| Offerwalls | Real (Pubscale, Xsolla) | Good |
| **Recommendation** | | Provide monetization guidance for non-Unity (native Android/iOS ads for Flutter/Java, Steam for C++). |

#### ivx-remote-config
| Aspect | Status | Gap |
|--------|--------|-----|
| Server-driven config | Documented | Overlaps with Satori flags and Hiro configs |
| Typed schemas | Documented | Good pattern |
| **Recommendation** | | Clarify when to use Remote Config vs. Satori Flags vs. Hiro Configs. Single decision tree. |

#### ivx-notification-orchestration
| Aspect | Status | Gap |
|--------|--------|-----|
| FCM/APNs/Web Push | Documented | Requires server-side implementation |
| Smart send-time | Documented | Requires player activity data |
| Roblox | Explicitly excluded | Platform limitation |
| **Recommendation** | | Provide Nakama server-side hooks for FCM/APNs. Smart send-time needs data pipeline from analytics. |

#### ivx-ugc-pipeline
| Aspect | Status | Gap |
|--------|--------|-----|
| Upload/download | Documented | Requires S3 or similar |
| AI moderation | Documented | Depends on AI subsystem |
| Creator rewards | Documented | Economy integration |
| **Recommendation** | | Provide a minimal S3 + CloudFront setup guide. Add COPPA/GDPR considerations for UGC. |

---

## 4. Studio Perspective Analysis

### Solo Indie Developer

**Profile:** 1 person, limited budget, shipping to mobile or PC, using Unity or Godot.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| Quick backend setup | Nakama Docker one-liner | Need managed hosting option clearly documented |
| Auth without building it | Device + email + social auth | Works great |
| Leaderboards | Drop-in | Works great |
| Monetization | Full ad mediation + IAP | Unity only — Godot devs get nothing |
| Live ops | Config-driven daily rewards, streaks | Requires Hiro config setup — needs a "5 minute live ops" tutorial |
| Analytics | Satori events | Need simpler "just track these 10 events" starter |
| AI skills | Design + build + ship workflow | Enormous productivity multiplier |

**Verdict:** Excellent for Unity indie devs. Significant gap for Godot/Defold/other engine indie devs who get auth/backend but no monetization, AI, or rich live-ops client code.

**Top 3 asks:**
1. "5-minute live ops" quickstart with pre-configured daily rewards + streaks
2. Godot monetization bridge (at minimum, rewarded ad callback)
3. Simpler analytics starter (10 must-track events pre-configured)

---

### Mid-Size Indie Studio (5-20 people)

**Profile:** Team with dedicated art, design, and engineering. Shipping to mobile + PC + possibly console. Using Unity or Unreal.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| GDD process | Game Design Studio skill | Templates need to exist as actual files |
| Economy design | Economy Simulator | Need standalone tool (not engine-dependent) |
| Asset pipeline | Scaffolding + validation | Good for standardization across team |
| CI/CD | 10 GitHub Actions workflows | Unity CI needs secrets — document workaround |
| Multiplayer | Lobbies + matchmaking | Good for Unity; Unreal needs real-time |
| Remote config | Server-driven tuning | Overlaps with flags — needs clarity |
| A/B testing | Satori experiments | Good |
| Security | Anti-cheat patterns | Need server-side hooks, not just client patterns |
| UGC | Full pipeline documented | Need S3 setup guide |

**Verdict:** Strong value proposition. The skill-driven workflow is a significant productivity multiplier for a team. Economy simulator and design studio are differentiators no competitor offers.

**Top 3 asks:**
1. Server-side Nakama hooks library (validation, anti-cheat, notifications)
2. Standalone economy simulator CLI
3. Pre-seeded GDD templates as downloadable files

---

### AAA Studio

**Profile:** 100+ people. Shipping to console + PC + mobile. Using Unreal or proprietary engine. Custom backend likely.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| Console support | PS5/Xbox/Switch adapter pattern | Real adapter code requires NDA SDKs — can only provide pattern |
| Custom backend | Nakama-centric architecture | Studios with custom backends need adapter layer |
| Scale (millions MAU) | Nakama is horizontally scalable | Need load testing documentation |
| GDPR/COPPA compliance | Minimal coverage | Need compliance tooling skill |
| Accessibility certification | WCAG/XAG/TRC documented | Need automated audit tool, not just checklist |
| Anti-cheat | Patterns documented | AAA needs EAC/BattlEye integration, not homebrew |
| Analytics | Export to data warehouses | Good — BigQuery/Snowflake support |
| Live ops at scale | 33 Hiro systems | Good — config-driven is the right approach |
| Asset pipeline at scale | Python CLI tools | Need integration with Perforce/asset servers |

**Verdict:** IVX is most valuable to AAA as a **rapid prototyping and live-ops framework**, not as a replacement for their core tech stack. The AI skills, economy simulation, and design studio are useful regardless of engine. The Hiro/Satori backend can serve as a microservice alongside their existing infra.

**Top 3 asks:**
1. Backend adapter interface (use IVX patterns with custom servers)
2. Compliance/legal skill (GDPR, COPPA, PEGI, ESRB tooling)
3. Perforce/large-asset integration for asset pipeline

---

### VR/AR Studio

**Profile:** Building for Meta Quest, Vision Pro, PSVR2, or AR.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| XR platform detection | Real (Unity, Unreal, Godot, C++) | Good |
| Spatial UI for live ops | Documented in skills | Need XR-specific UI prefabs |
| Hand tracking input | Documented | Platform-specific |
| VR accessibility | Documented (subtitle placement, spatial audio) | Need comfort settings (FOV, snap turn, vignette) |
| VR monetization | Explicitly noted as different | Meta Quest store IAP path needed |
| VR multiplayer | Documented | Need voice chat integration |

**Verdict:** Good foundation. VR studios benefit from backend + live ops + analytics. Missing comfort settings, spatial UI prefabs, and VR-specific monetization paths.

**Top 3 asks:**
1. VR comfort settings skill (FOV vignette, locomotion options, snap turn)
2. Spatial UI prefabs for live-ops screens (daily rewards in VR space)
3. Meta Quest store IAP integration guide

---

### Mobile F2P Studio

**Profile:** Building a free-to-play mobile game. Monetization is critical. Using Unity.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| Ad mediation | LevelPlay + Appodeal + AdMob | Excellent |
| IAP with validation | Server-side receipt validation | Excellent |
| Offerwalls | Pubscale + Xsolla | Good |
| Retention mechanics | Streaks, daily rewards, spin wheel, season pass | Core strength |
| A/B testing | Satori experiments | Good |
| Push notifications | Documented | Needs server-side implementation |
| IDFA/ATT handling | Not documented | Gap for iOS 14.5+ |
| Ad ROAS tracking | Not documented | Gap for UA optimization |

**Verdict:** Strongest studio-type fit. IVX was clearly built with F2P mobile in mind. Almost everything needed is present. Missing IDFA/ATT tracking and ad attribution/ROAS integration.

**Top 3 asks:**
1. IDFA/ATT handling skill (iOS privacy framework)
2. Ad attribution integration (Adjust, AppsFlyer, Singular)
3. Server-side notification hooks

---

### Web/HTML5 Game Studio

**Profile:** Building browser games or Progressive Web Apps.

| Need | IVX Delivers | Gap |
|------|-------------|-----|
| WebGL deployment | Documented | Good |
| Browser ads | AdSense/Applixir noted | No SDK-level integration |
| Offline support | IndexedDB cache mentioned | Pattern only |
| PWA support | Web Push documented | Good |
| Social sharing | Not documented for web | Missing |
| WebGL performance | Bundle size concern noted | No compression tooling |

**Verdict:** Adequate for backend/live-ops. Web-specific monetization and sharing need work.

---

## 5. Platform Coverage Analysis

### Platform Readiness Score

| Platform | Backend | Auth | Economy | Live Ops | Ads/IAP | AI | Multiplayer | Analytics | Score |
|----------|:-------:|:----:|:-------:|:--------:|:-------:|:--:|:-----------:|:---------:|:-----:|
| **Android** | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 8 | **97%** |
| **iOS** | 10 | 10 | 10 | 10 | 10 | 10 | 10 | 8 | **97%** |
| **WebGL** | 10 | 8 | 10 | 8 | 4 | 6 | 8 | 6 | **75%** |
| **Windows/Mac** | 10 | 10 | 10 | 10 | 2 | 10 | 10 | 8 | **88%** |
| **Meta Quest** | 10 | 8 | 10 | 6 | 2 | 8 | 8 | 6 | **72%** |
| **PS5** | 8 | 6 | 8 | 8 | 0 | 6 | 6 | 6 | **60%** |
| **Xbox** | 8 | 6 | 8 | 8 | 0 | 6 | 6 | 6 | **60%** |
| **Switch** | 6 | 4 | 6 | 6 | 0 | 4 | 4 | 4 | **42%** |
| **Apple Vision Pro** | 8 | 8 | 8 | 4 | 0 | 6 | 4 | 4 | **52%** |

*Scores: 0 = none, 2 = documented, 4 = stub, 6 = partial, 8 = functional, 10 = production*

---

## 6. Engine Coverage Analysis

### Engine Feature Depth

| Engine | Core SDK | Extended (AI/Hiro/Satori) | Tooling | Documentation | Overall |
|--------|:--------:|:-------------------------:|:-------:|:-------------:|:-------:|
| **Unity** | Production | Production | Full | Excellent | **A** |
| **Unreal** | Real Nakama | Stub | CI build | Good README | **C+** |
| **Godot** | Real Nakama | Stub | CI (via platform) | Good README | **C+** |
| **JavaScript/TS** | Real Nakama | Stub | npm CI | Good README | **C** |
| **Java/Android** | Real Nakama | Stub | Gradle CI | Good README | **C** |
| **Flutter/Dart** | Real Nakama | Stub | Dart CI | Good README | **C** |
| **C++** | Real Nakama | Stub | CMake CI | Good README | **C** |
| **Roblox** | Purpose-built | AI + Hiro + Identity | None | Good README | **B-** |
| **Defold** | Real Nakama | Stub | None | README | **C-** |
| **Cocos2d-x** | Real Nakama | Stub | CMake CI | README | **C-** |
| **Web3** | Real Nakama + ethers | Stub | None | README | **C-** |

### Critical Gap: The "Unreal Problem"

Unreal Engine has the second-largest game development market share after Unity. IntelliVerseX's Unreal SDK is a thin Nakama wrapper with stubs for everything else. **This is the single biggest market gap.**

Recommendation: Invest in Unreal parity for at least: Auth, Economy, Hiro RPC client, and one showcase feature (AI NPC dialog or live-ops daily rewards).

---

## 7. Competitive Landscape

### Feature Comparison

| Feature | IntelliVerseX | PlayFab | GameSparks | AccelByte | Beamable | LootLocker |
|---------|:------------:|:-------:|:----------:|:---------:|:--------:|:----------:|
| Open-source backend | Nakama | No | No | No | Partial | No |
| Self-hostable | Yes | No | No | Yes | No | No |
| Game design tools | Yes | No | No | No | No | No |
| AI integration (NPC/voice) | Yes | No | No | No | No | No |
| Economy simulation | Yes | No | No | No | No | No |
| Procedural content gen | Yes | No | No | No | No | No |
| AI agent skills (22) | Yes | No | No | No | No | No |
| Multi-engine (11) | Yes | 4 | 3 | 5 | 1 | 4 |
| Live ops systems | 33+ | ~15 | ~12 | ~20 | ~10 | ~8 |
| Ad mediation built-in | Yes | No | No | No | No | No |
| Asset pipeline tools | Yes | No | No | No | No | No |
| Context engineering | Yes | No | No | No | No | No |
| Free tier | Unlimited (self-host) | 100K MAU | Discontinued | Enterprise | Free tier | Free tier |
| Price at 1M MAU | $0 (self-host) | $10K+/mo | N/A | Custom | $2K+/mo | $500+/mo |

### Unique Differentiators (No Competitor Has These)

1. **AI agent skills** — 22 purpose-built workflows that any AI coding tool can execute
2. **Game design studio** — GDD templates, economy simulation, systems mapping
3. **Procedural AI content** — Level, NPC, loot, quest generation with deterministic fallbacks
4. **Context engineering** — `.cursor/` authority system with CI validation
5. **Asset pipeline** — Schema-validated character scaffolding and spritesheet tools
6. **Full lifecycle coverage** — Design → Build → Ship → Grow in one SDK

---

## 8. Recommended Additions

### Priority 1: High-Impact, High-Feasibility (Do Now)

| # | New Skill / Addition | Why | Effort | Impact |
|---|---------------------|-----|--------|--------|
| 1 | **ivx-attribution-privacy** | IDFA/ATT, GDPR consent, COPPA compliance, ad attribution (Adjust/AppsFlyer). Every mobile game needs this. No competitor SDK bundles it. | Medium | Very High |
| 2 | **ivx-onboarding-ftue** | First-time user experience templates, tutorial flow builder, progressive disclosure patterns, FTUE funnel tracking. #1 retention lever. | Medium | Very High |
| 3 | **ivx-social-viral** | Referral systems, share-to-earn, invite deep links, social proof mechanics, friend challenges. #1 organic growth lever. | Medium | High |
| 4 | **Default event taxonomy file** | Ship a `taxonomy.json` with 50+ common game events pre-defined. Drop into any project. | Low | High |
| 5 | **Wire asset-pipeline into CI** | Add `tools/asset-pipeline/validate_*` to a GitHub Actions workflow. Already have the tools. | Low | Medium |
| 6 | **Fix CI version sourcing** | Replace hard-coded `5.8.0` in `platform-sdks-validation.yml` with dynamic read from `package.json`. | Low | Medium |
| 7 | **Pre-seeded GDD templates** | Ship the 15 GDD templates as actual markdown files, not just API descriptions. | Low | High |

### Priority 2: Competitive Moat (Next Quarter)

| # | New Skill / Addition | Why | Effort | Impact |
|---|---------------------|-----|--------|--------|
| 8 | **ivx-player-segmentation** | Whale/dolphin/minnow classification, behavioral clustering, churn prediction, LTV modeling. Drives monetization and retention targeting. | High | Very High |
| 9 | **ivx-vr-comfort** | VR comfort settings (FOV vignette, locomotion options, snap/smooth turn, seated/standing detection, motion sickness reduction). Every VR game needs this. | Medium | High (VR) |
| 10 | **ivx-localization-pipeline** | Expand beyond Unity-only. AI-assisted translation, string extraction, RTL support, locale testing tool, translation memory. | Medium | High |
| 11 | **ivx-server-hooks** | Nakama TypeScript server-side hooks library: score validation, economy transaction checks, matchmaking rules, notification dispatch. Bridges the client-server gap. | High | Very High |
| 12 | **ivx-testing-automation** | Automated gameplay testing, screenshot comparison, performance benchmarking, regression detection. Missing entirely. | High | High |
| 13 | **Standalone economy simulator CLI** | Python/Node CLI that runs Monte Carlo simulations without any game engine. Useful for design meetings. | Medium | High |

### Priority 3: Market Expansion (Future)

| # | New Skill / Addition | Why | Effort | Impact |
|---|---------------------|-----|--------|--------|
| 14 | **ivx-unreal-native** | Native Unreal plugin with C++ Hiro client, Blueprints for daily rewards/achievements/economy. Addresses biggest engine gap. | Very High | Very High |
| 15 | **ivx-web-monetization** | Browser-specific monetization: Web Monetization API, crypto tipping, AdSense integration, paywall. Growing web game market. | Medium | Medium |
| 16 | **ivx-perforce-integration** | Large-asset pipeline integration for Perforce, Git LFS, asset servers. AAA requirement. | Medium | Medium (AAA) |
| 17 | **ivx-compliance-legal** | PEGI/ESRB age rating helper, loot box disclosure, regional regulation checker (Belgium, Netherlands, Japan gacha). Legal risk reduction. | Medium | High |
| 18 | **ivx-community-management** | Discord bot integration, in-game feedback, community event scheduling, player councils, sentiment analysis. | Medium | Medium |

### Improvements to Existing Skills

| Skill | Improvement |
|-------|------------|
| **ivx-live-ops** | Ship the 10 "Planned" managers (Push, DailyMissions, League, SeasonPass, FortuneWheel, Badge, Goals, FriendStreak, Character, Tournament) |
| **ivx-remote-config** | Add decision tree: when to use Remote Config vs Satori Flags vs Hiro Configs |
| **ivx-roblox** | Add checklist section, match depth of other 21 skills |
| **ivx-procedural-ai** | Remove music stub from marketing or implement with a real provider |
| **ivx-crashlytics** | Pick Sentry as recommended default, simplify the multi-backend story |
| **ivx-security-anticheat** | Add server-side Nakama hooks (not just client patterns) |
| **ivx-accessibility** | Build automated contrast ratio + touch target checker |
| **ivx-quality-gates** | Wire asset validators into CI, commit baseline TSV |

---

## 9. Skill Publishing Strategy

### Where to Publish Skills

Skills are the SDK's **distribution moat**. They should be available everywhere developers use AI coding tools.

#### Tier 1: Primary Channels (Publish Immediately)

| Channel | Format | Reach | Action |
|---------|--------|-------|--------|
| **GitHub Repository** | `.cursor/skills/*.md` | All Cursor users | Already done |
| **OpenUPM / npm** | Package with skills embedded | Unity + JS developers | Include skills in UPM package metadata |
| **Cursor Marketplace / Skills Registry** | SKILL.md format | All Cursor users globally | Submit all 22 skills to Cursor's skill directory |
| **Claude Code Plugin Marketplace** | Plugin with skills | All Claude Code users | `@intelliversex` plugin namespace |
| **Windsurf Skills** | SKILL.md compatible | All Windsurf users | Same format, add to their registry |
| **SkillsGate** | SKILL.md compatible | 18+ AI agent tools | Already referenced in README |
| **GitHub Marketplace** | GitHub Action + skills | CI/CD users | Publish `intelliversex-setup` action |

#### Tier 2: Developer Communities (Publish Within 30 Days)

| Channel | Format | Reach | Action |
|---------|--------|-------|--------|
| **Awesome Lists** | Link + description | GitHub community | Submit to awesome-gamedev, awesome-unity, awesome-nakama |
| **Dev.to / Hashnode / Medium** | Tutorial articles | Developer community | "How to build a game in 1 hour with AI skills" |
| **YouTube** | Video tutorials | Visual learners | Skill-by-skill walkthrough series |
| **Reddit** | Posts in r/gamedev, r/unity3d, r/unrealengine, r/indiedev | Reddit community | Launch announcement + demo |
| **Discord** | IntelliVerseX server + gamedev servers | Direct community | Skill announcement channel |
| **Product Hunt** | Product launch | Tech early adopters | "22 AI Skills for Game Development" |
| **Hacker News** | Show HN post | Tech community | "Show HN: 22 AI agent skills that cover the full game dev lifecycle" |
| **IndieDB / itch.io** | Tool listing | Indie developers | List as a free game development tool |

#### Tier 3: Industry & Enterprise (Publish Within 90 Days)

| Channel | Format | Reach | Action |
|---------|--------|-------|--------|
| **GDC / Unite / Unreal Fest** | Conference talk + booth | Industry professionals | "AI-Native Game Development: From GDD to Live Ops" |
| **Unity Asset Store** | UPM package listing | Unity developers | Paid or free listing with skills |
| **Unreal Marketplace** | Plugin listing | Unreal developers | When Unreal parity exists |
| **Heroic Labs Partner Page** | Integration listing | Nakama users | Official Hiro/Satori integration partner |
| **AWS / GCP / Azure Marketplaces** | SaaS listing | Cloud-hosted game backends | Managed IntelliVerseX backend offering |
| **Academic / Game Jams** | Free license + starter kit | Students, jam participants | "Build your jam game in 48 hours with IVX" |
| **Devin / Codex / Copilot Workspaces** | Agent-compatible format | Enterprise AI users | Ensure SKILL.md compatibility |

#### Tier 4: Specialized Registries

| Channel | Format | Target |
|---------|--------|--------|
| **MCP Tool Registry (Smithery)** | MCP server | AI agent users |
| **Composio** | Tool integration | AI workflow automation |
| **LangChain / LlamaIndex** | Tool definition | LLM application developers |
| **Hugging Face** | Model + tools | AI/ML community |
| **VS Code Extension Marketplace** | Extension with skills | VS Code users |

### Skill Distribution Format Matrix

| Format | Where Used | Files Needed |
|--------|-----------|-------------|
| **SKILL.md** (primary) | Cursor, Claude Code, Windsurf, Devin, any agent | 1 file per skill |
| **MCP Tools** | Smithery, Cursor MCP, Claude Desktop | Server + tool definitions |
| **GitHub Action** | GitHub CI/CD | `action.yml` + scripts |
| **npm Package** | JS/TS ecosystem | `package.json` + skills |
| **UPM Package** | Unity ecosystem | Unity `package.json` + skills |
| **PyPI Package** | Python ecosystem (tools) | `setup.py` + CLI |
| **Docker Image** | Self-hosted backend | `Dockerfile` + compose |
| **REST API** | SaaS offering | API endpoints + docs |

### Publishing Priority Matrix

```
                    HIGH REACH
                        |
    Cursor Registry  ---+--- GitHub + npm + UPM
    Claude Plugin       |    Dev.to articles
    Windsurf            |    YouTube tutorials
                        |
  LOW EFFORT -----------+----------- HIGH EFFORT
                        |
    Awesome lists    ---+--- Unity Asset Store
    Reddit/HN           |    Unreal Marketplace
    Discord             |    GDC talk
                        |    AWS Marketplace
                    LOW REACH
```

**Recommended launch sequence:**
1. **Week 1:** GitHub repo (done), Cursor registry, Claude plugin, Windsurf registry, SkillsGate
2. **Week 2:** Product Hunt launch, Hacker News Show HN, Reddit posts, Dev.to launch article
3. **Week 3-4:** YouTube series (1 video per skill phase), Discord community launch
4. **Month 2:** Unity Asset Store, npm package, PyPI for tools, Smithery MCP
5. **Month 3:** Conference submissions (GDC, Unite), enterprise partner outreach
6. **Month 4+:** Unreal Marketplace (when parity exists), AWS/GCP listings

---

## 10. Risk Assessment

### Technical Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|-----------|-----------|
| Aspirational APIs in skills don't match implementation | High | Medium | Audit every `IVX*` symbol in skills against actual codebase |
| Non-Unity SDKs remain stubs indefinitely | High | Medium | Set quarterly parity targets; prioritize Unreal + Godot |
| Nakama version incompatibility | Medium | Low | Pin Nakama 3.x in requirements; test on upgrades |
| AI provider API changes (OpenAI, etc.) | Medium | High | Abstract behind `IIVXAIProvider`; test quarterly |
| CI gives false confidence (secret-gated tests) | Medium | High | Add free-tier test options; document limitations |

### Business Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|-----------|-----------|
| Unity pricing changes (repeat of 2023) | High | Low | Multi-engine investment hedges this |
| Nakama/Heroic Labs acquisition or shutdown | Medium | Low | Self-hostable; fork rights under Apache 2.0 |
| Competitor copies skill-based distribution | Medium | Medium | First-mover advantage; depth of 22 skills is hard to replicate |
| Developer overwhelm (too many skills) | Medium | Medium | Clear "start here" path; phase-based organization |
| Skills perceived as "vaporware documentation" | High | Medium | Implementation audit; mark Planned vs Shipped |

### Market Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|-----------|-----------|
| AI coding tools fragment (no standard format) | Medium | Medium | SKILL.md is simple enough to port; maintain adapters |
| Engine market shifts (e.g., Godot surge) | Medium | Medium | Already have 11 engines; deepen top 3 |
| Mobile F2P market saturation | Low | High | Diversify to PC, console, VR, web |

---

## 11. Roadmap Recommendations

### Q2 2026 (Now → June)

**Theme: Foundation Integrity**

- [ ] **Audit** — Verify every `IVX*` class mentioned in skills exists in codebase. Tag Planned vs. Shipped.
- [ ] **Ship** — Pre-seeded GDD templates, default event taxonomy, baseline TSV fix
- [ ] **CI** — Wire asset validators, fix version sourcing, add CodeQL security scan
- [ ] **Publish** — Cursor registry, Claude plugin, Windsurf, SkillsGate, Product Hunt launch
- [ ] **New skills** — `ivx-attribution-privacy`, `ivx-onboarding-ftue`

### Q3 2026 (July → September)

**Theme: Cross-Platform Parity**

- [ ] **Unreal** — Native C++ Hiro client + Blueprints for economy, achievements, daily rewards
- [ ] **Godot** — GDScript wrappers for monetization (rewarded ads at minimum)
- [ ] **Ship** — 5 of 10 "Planned" Hiro managers (DailyMissions, SeasonPass, League, Push, Badge)
- [ ] **New skills** — `ivx-player-segmentation`, `ivx-server-hooks`, `ivx-social-viral`
- [ ] **Publish** — Unity Asset Store, npm/PyPI packages, YouTube tutorial series

### Q4 2026 (October → December)

**Theme: Enterprise & Scale**

- [ ] **Server hooks** — Nakama TypeScript library for validation, notifications, anti-cheat
- [ ] **Testing** — Automated gameplay testing skill
- [ ] **Compliance** — GDPR/COPPA/PEGI skill
- [ ] **Ship** — Remaining 5 "Planned" Hiro managers
- [ ] **Publish** — GDC submission, AWS Marketplace, Unreal Marketplace
- [ ] **New skills** — `ivx-testing-automation`, `ivx-compliance-legal`, `ivx-vr-comfort`

### 2027 Vision

- **30+ skills** covering every aspect of game development
- **Full parity** on Unity, Unreal, Godot (top 3 engines)
- **Managed cloud offering** for studios that don't want to self-host
- **Marketplace** for community-contributed skills
- **Enterprise tier** with SLA, dedicated support, compliance certification

---

## Summary

IntelliVerseX SDK occupies a **unique and defensible position** in the game development tools market. No competitor covers the full lifecycle from game design to live retention, and no competitor has an AI agent skill distribution strategy. The 22 skills are the primary moat.

**Immediate priorities:**
1. Ensure every documented API has real implementation (audit)
2. Ship `ivx-attribution-privacy` and `ivx-onboarding-ftue` (highest-value gaps)
3. Publish skills to Cursor, Claude, Windsurf registries (distribution)
4. Invest in Unreal parity (market expansion)
5. Pre-seed GDD templates and event taxonomy (developer experience)

The SDK has the potential to become **the standard AI-native game development platform** if execution matches the documented ambition.
