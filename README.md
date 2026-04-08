<h1 align="center">IntelliVerseX SDK</h1>

<p align="center">
The AI-native, 11-engine game development platform.<br>
From game design document to live retention — one SDK, any engine, every platform.
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="MIT License"></a>
  <a href="CHANGELOG.md"><img src="https://img.shields.io/badge/Version-5.8.0-orange.svg" alt="Version"></a>
  <a href="https://intelli-verse-x.github.io/Intelli-verse-X-SDK/"><img src="https://img.shields.io/badge/Docs-Online-blue.svg" alt="Documentation"></a>
  <a href="https://openupm.com/packages/com.intelliversex.sdk"><img src="https://img.shields.io/npm/v/com.intelliversex.sdk?label=openupm&registry_uri=https://package.openupm.com" alt="OpenUPM"></a>
  <a href=".cursor/skills"><img src="https://img.shields.io/badge/AI%20Skills-31-blueviolet" alt="31 AI Skills"></a>
  <a href="https://discord.gg/YVPxPFftMQ"><img src="https://img.shields.io/badge/Discord-Join-5865F2?logo=discord&logoColor=white" alt="Discord"></a>
</p>

---

## Why IntelliVerseX

Most game SDKs solve one problem — ads, analytics, or multiplayer. IntelliVerseX covers the **entire game development lifecycle** from initial concept to live operations, across 11 engines and every major platform.

| What You Get | Without IVX | With IVX |
|-------------|-------------|----------|
| **Game design** | Blank page, no structure | AI-assisted GDD generation with 15 templates, MDA framework, systems mapping |
| **Asset pipeline** | Manual folder chaos | Scaffolded characters (2D/3D), schema-validated assets, spritesheet automation |
| **Backend** | Build from scratch or vendor lock-in | Open-source Nakama + 33 Hiro live-ops systems + Satori analytics, self-hosted or cloud |
| **AI features** | Months of integration | 7 AI subsystems (NPC dialog, voice host, moderation, content gen) — drop in and go |
| **Monetization** | Fragmented SDKs per network | Unified ads (LevelPlay/Appodeal/AdMob), IAP, offerwalls with server validation |
| **Live ops** | Build every system manually | Daily rewards, streaks, season pass, achievements, tournaments — config-driven |
| **Analytics** | Third-party dashboards, no control | Owned event taxonomy, funnels, cohorts, export to BigQuery/Snowflake |
| **Cross-platform** | Rewrite for each engine | Same API surface across Unity, Unreal, Godot, Roblox, and 7 more |
| **Quality gates** | Hope for the best | Schema validation, context engineering checks, CI-ready tooling |
| **Retention** | Guesswork | Economy simulation, A/B testing, smart notifications, remote config |

---

## The Complete Game Development Lifecycle

IntelliVerseX is organized around the stages every game goes through:

```
 DESIGN          BUILD           SHIP            GROW
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│ GDD     │ -> │ Assets  │ -> │ Backend │ -> │ LiveOps │
│ Systems │    │ Code    │    │ CI/CD   │    │ Monetize│
│ Economy │    │ AI      │    │ Release │    │ Retain  │
│ Balance │    │ Multi   │    │ Deploy  │    │ Analyze │
└─────────┘    └─────────┘    └─────────┘    └─────────┘
     │              │              │              │
     ▼              ▼              ▼              ▼
 Design Studio  Asset Pipeline  DevOps/CICD   Analytics
 Economy Sim    Procedural AI   Quality Gates Remote Config
 Narrative      Accessibility   Crashlytics   Notifications
                Security        Packaging     Economy Health
```

### Phase 1: Design Your Game

Before writing a single line of code, IntelliVerseX helps you design:

| Tool | What It Does |
|------|-------------|
| [**Game Design Studio**](.cursor/skills/ivx-game-design-studio/SKILL.md) | AI-assisted GDD generation with 15 document templates, 8-section design review, systems dependency mapping, MDA framework, Bartle player types, brand identity, store metadata, character bibles, and a **Starter Project Generator** that outputs a fully-wired "Hello IntelliVerseX" game for any of the 11 supported engines — auth, economy, daily rewards, streaks, leaderboards, achievements, analytics, and FTUE all working out of the box. |
| [**Economy Simulator**](.cursor/skills/ivx-economy-simulator/SKILL.md) | Currency flow modeling (sources, sinks, exchange rates). Monte Carlo simulation over 90+ days. Inflation/deflation detection. Gini coefficient monitoring. Reward curve designer. Store pricing A/B test integration. |
| [**Narrative Engine**](.cursor/skills/ivx-narrative-engine/SKILL.md) | Branching dialog trees. Story state machine with flags, variables, conditions. Character relationship tracking. Ink and Yarn Spinner import. AI-generated dialog for dynamic NPCs. Cutscene sequencer. |

### Phase 2: Build Your Game

Once your design is solid, build across any engine:

| Tool | What It Does |
|------|-------------|
| [**Asset Pipeline**](.cursor/skills/ivx-asset-pipeline/SKILL.md) | 2D character scaffolding with placeholder PNGs and animation specs. 3D rigged character templates (mesh, skeleton, LODs, blend shapes, materials, state machine). Sprite sheet generation from frames. Sound manifest with required audio categories. 9 JSON Schemas for validation. |
| [**AI Integration**](.cursor/skills/ivx-ai-integration/SKILL.md) | 7 AI subsystems: session management, NPC dialog, assistant/hints, content moderation, structured content generation, player profiling, voice/TTS streaming. Supports OpenAI, Azure, Anthropic, and self-hosted models. |
| [**Procedural AI**](.cursor/skills/ivx-procedural-ai/SKILL.md) | AI-powered level generation (graph-based rooms, tile maps). NPC personality and dialog tree generation. Loot tables with rarity and stat rolls. Quest chains with branching objectives. Texture/sprite variations via DALL-E. Deterministic seed-based fallbacks for offline play. |
| [**Multiplayer**](.cursor/skills/ivx-multiplayer/SKILL.md) | Solo, local, online lobby, quick match, and ranked modes. Lobby create/join with events. Nakama matchmaking. Real-time match networking with op codes. Cross-platform match lifecycle. |
| [**Accessibility**](.cursor/skills/ivx-accessibility/SKILL.md) | Color blind simulation and correction. Screen reader bridges (TalkBack, VoiceOver, Narrator). Dynamic font scaling. Input remapping (one-handed, switch control, eye tracking). Subtitle system with speaker identification. WCAG 2.1 / Xbox XAG / CVAA compliance audit. |
| [**Security**](.cursor/skills/ivx-security-anticheat/SKILL.md) | Server-authoritative validation. Memory tamper detection. Speed hack prevention. Replay validation for competitive modes. Encrypted local save data with cloud sync. Rate limiting and anomaly detection. Ban/suspension system. |

### Phase 3: Ship Your Game

Deploy to every store and platform with confidence:

| Tool | What It Does |
|------|-------------|
| [**SDK Setup**](.cursor/skills/ivx-sdk-setup/SKILL.md) | One-drop bootstrap for any engine. Single config ScriptableObject. Auto-detects platform (mobile, VR, console, web). Toggles for each subsystem. 16 built-in demo UIs. |
| [**DevOps / CI/CD**](.cursor/skills/ivx-devops-cicd/SKILL.md) | GitHub Actions templates for all 11 engines. Unity Cloud Build integration. Automated SemVer version bumping. Asset bundle build + CDN upload. Code signing (Android keystore, iOS provisioning). Store submission (Play Store, App Store, Steam, itch.io, Meta Quest). |
| [**Quality Gates**](.cursor/skills/ivx-quality-gates/SKILL.md) | Context validation (loading order, memory JSON schema). Runtime safety guard (no editor code in builds). TODO/FIXME baseline gating. Asset schema validation in CI. UPM package export. Multi-platform release bundle packaging. |
| [**Crashlytics**](.cursor/skills/ivx-crashlytics/SKILL.md) | Automatic crash/exception capture. ANR detection on Android. Breadcrumb trails (last 50 player actions). Crash grouping and deduplication. Sentry, Firebase, or self-hosted via Nakama. Alert webhooks to Slack/Discord. |
| [**Cross-Platform**](.cursor/skills/ivx-cross-platform/SKILL.md) | Port features between 11 engines using the same RPC contract. Feature coverage matrix with Y/Stub/Planned status per platform. 5-step porting process. VR, console, and WebGL adaptation guides. |

### Phase 4: Grow Your Game

Keep players engaged and revenue growing after launch:

| Tool | What It Does |
|------|-------------|
| [**Live Ops**](.cursor/skills/ivx-live-ops/SKILL.md) | 33+ Hiro systems: economy, energy, inventory, achievements, progression, streaks, event leaderboards, store, challenges, teams, unlockables, retention, spin wheel, daily missions, season pass, leagues, tournaments. All config-driven via Nakama storage. |
| [**Analytics Pipeline**](.cursor/skills/ivx-analytics-pipeline/SKILL.md) | Event taxonomy with 6 categories. Funnel builder (onboarding, monetization, retention, social). Cohort analysis (D1/D7/D30 retention, LTV projections). Export to BigQuery, Snowflake, Redshift, S3. Pre-built dashboard templates. Cross-platform event SDK. |
| [**Monetization**](.cursor/skills/ivx-monetization/SKILL.md) | LevelPlay, Appodeal, AdMob with waterfall failover. Rewarded ads with server validation. Pubscale and Xsolla offerwalls. IAP with Apple/Google receipt validation. Genre-specific monetization strategy. |
| [**Remote Config**](.cursor/skills/ivx-remote-config/SKILL.md) | Server-driven configuration without app updates. Typed schemas with validation. Conditional configs by platform, country, segment, or A/B variant. Real-time propagation via Nakama socket. Version tracking with instant rollback. |
| [**Notifications**](.cursor/skills/ivx-notification-orchestration/SKILL.md) | Push notification scheduling (FCM, APNs, Web Push). Smart send-time optimization based on player activity patterns. Templated messages with deep linking. A/B testing notification content. Frequency caps and quiet hours. |
| [**UGC Pipeline**](.cursor/skills/ivx-ugc-pipeline/SKILL.md) | User-generated content upload/download for levels, skins, quizzes, mods. AI moderation for text and images. Content browser with search, ratings, and reporting. Creator profiles with attribution and rewards. Cross-platform content sharing. |
| [**Quiz Content**](.cursor/skills/ivx-quiz-content/SKILL.md) | S3-hosted daily and weekly quiz pipelines. LLM-powered question generation. Local/hybrid/S3 providers. GitHub Actions cron for automated content. Disk + memory caching. |

---

## 11 Engine SDKs

| Platform | Language | Source | Guide |
|----------|----------|--------|-------|
| **Unity** | C# | [Assets/Intelli-verse-X-SDK](Assets/Intelli-verse-X-SDK/) | [Quickstart](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/getting-started/quickstart/) |
| **Unreal Engine** | C++ / Blueprints | [SDKs/unreal](SDKs/unreal/) | [Guide](SDKs/unreal/README.md) |
| **Godot 4** | GDScript | [SDKs/godot](SDKs/godot/) | [Guide](SDKs/godot/README.md) |
| **Roblox** | Luau | [SDKs/roblox](SDKs/roblox/) | [Guide](SDKs/roblox/README.md) |
| **Defold** | Lua | [SDKs/defold](SDKs/defold/) | [Guide](SDKs/defold/README.md) |
| **Cocos2d-x** | C++ | [SDKs/cocos2dx](SDKs/cocos2dx/) | [Guide](SDKs/cocos2dx/README.md) |
| **JavaScript** | TypeScript / JS | [SDKs/javascript](SDKs/javascript/) | [Guide](SDKs/javascript/README.md) |
| **C / C++** | C++ | [SDKs/cpp](SDKs/cpp/) | [Guide](SDKs/cpp/README.md) |
| **Java / Android** | Java | [SDKs/java](SDKs/java/) | [Guide](SDKs/java/README.md) |
| **Flutter / Dart** | Dart | [SDKs/flutter](SDKs/flutter/) | [Guide](SDKs/flutter/README.md) |
| **Web3** | TypeScript | [SDKs/web3](SDKs/web3/) | [Guide](SDKs/web3/README.md) |

### Feature Coverage Matrix

| Feature | Unity | Unreal | Godot | Roblox | JS | Java | Flutter | C++ | Defold | Cocos | Web3 |
|---------|:-----:|:------:|:-----:|:------:|:--:|:----:|:-------:|:---:|:------:|:-----:|:----:|
| Auth (Device/Email/Social) | Full | Full | Full | Auto | Full | Full | Full | Full | Full | Full | Wallet |
| Wallet / Economy | Full | Full | Full | Full | Full | Full | Full | Full | Full | Full | Full |
| Leaderboards | Full | Full | Full | Native | Full | Full | Full | Full | Full | Full | Full |
| Cloud Storage | Full | Full | Full | Full | Full | Full | Full | Full | Full | Full | Full |
| Hiro Live-Ops (33 systems) | Full | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC | RPC |
| Satori Analytics | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI (7 subsystems) | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Multiplayer (real-time) | Full | -- | Full | Native | Full | -- | -- | -- | Full | -- | -- |
| Monetization (Ads/IAP) | Full | -- | -- | Native | -- | -- | -- | -- | -- | -- | -- |
| Discord Social | Full | Stub | Stub | -- | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| XR/VR/AR | Full | Full | Full | -- | WebXR | -- | -- | Full | -- | -- | -- |
| Console (PS5/Xbox/Switch) | Full | Full | -- | -- | -- | -- | -- | -- | -- | -- | -- |

**Full** = native feature set | **RPC** = available via server calls | **Stub** = wrapper ready, implementation in progress | **Native** = platform handles natively

### Deployment Targets

| Target | Engines | Highlights |
|--------|---------|-----------|
| **Meta Quest (VR)** | Unity, Unreal, Godot, C++ | Hand/eye tracking, passthrough, spatial UI |
| **SteamVR / OpenXR** | Unity, Unreal, Godot, C++ | Generic OpenXR, controller + hand input |
| **Apple Vision Pro** | Unity | PolySpatial, gaze input, spatial audio |
| **PSVR2** | Unity, Unreal | Eye tracking, adaptive triggers |
| **PS5 / Xbox / Switch** | Unity, Unreal | Console adapter pattern, platform auth, achievements |
| **Android / iOS** | Unity, Flutter, Java | Full mobile feature set |
| **WebGL / Browser** | Unity, JS | WebXR, browser ads, IndexedDB cache |
| **AR (ARKit / ARCore)** | Unity, Unreal | Plane detection, image tracking |

---

## Quick Start

### Unity (3 steps)

**1. Install** — Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK#v5.8.0"
  }
}
```

**2. Bootstrap** — Run **IntelliVerseX > Generate All Prefabs**, drag `IVX_Bootstrap.prefab` into your first scene, configure the Bootstrap Config asset.

**3. Go** — Listen for ready and start building:

```csharp
using IntelliVerseX.Bootstrap;

public class GameInit : MonoBehaviour
{
    void Start()
    {
        IVXBootstrap.OnBootstrapComplete += success =>
        {
            Debug.Log($"IntelliVerseX Ready! User: {IVXBootstrap.Instance.UserId}");
        };
    }
}
```

No server yet? The SDK works in **offline mode** with mock data — press Play and explore the 16 built-in demo UIs.

### Other Engines

Each SDK follows the same pattern: install the package, configure the server connection, authenticate, and start calling APIs.

```gdscript
# Godot
var ivx = IVXClient.new()
ivx.configure("localhost", 7350, "defaultkey")
var session = await ivx.authenticate_device(OS.get_unique_id())
```

```lua
-- Roblox
local IVX = require(game.ServerScriptService.IntelliVerseX)
IVX.configure({ game_id = "YOUR_GAME_ID", debug = true })
IVX.enable_auto_auth()
```

```typescript
// JavaScript
const ivx = new IVXClient("localhost", 7350, "defaultkey");
const session = await ivx.authenticateDevice(deviceId);
```

### Backend (Nakama)

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

Or use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

---

## 31 AI Agent Skills

Every skill is a purpose-built workflow that any AI coding agent can execute. Works with **Cursor, Windsurf, Claude Code, Devin, OpenAI Codex, GitHub Copilot**, and any agent that reads `SKILL.md` files.

### Design Phase

| Skill | Trigger Phrases | What It Does |
|-------|----------------|-------------|
| [**ivx-game-design-studio**](.cursor/skills/ivx-game-design-studio/SKILL.md) | "create GDD", "game design", "brainstorm", "generate starter project", "boilerplate", "hello intelliverse" | 15 GDD templates, AI brainstorming, design review, systems mapping, balance tools, **engine selection + fully-wired starter project generator for all 11 engines** |
| [**ivx-economy-simulator**](.cursor/skills/ivx-economy-simulator/SKILL.md) | "economy design", "balance economy" | Currency flow modeling, Monte Carlo simulation, inflation detection, reward curves |
| [**ivx-narrative-engine**](.cursor/skills/ivx-narrative-engine/SKILL.md) | "dialog system", "branching dialog" | Conversation trees, story state machine, character relationships, Ink/Yarn import |

### Build Phase

| Skill | Trigger Phrases | What It Does |
|-------|----------------|-------------|
| [**ivx-sdk-setup**](.cursor/skills/ivx-sdk-setup/SKILL.md) | "set up IntelliVerseX" | Bootstrap on any engine, config wizard, XR/console/WebGL detection |
| [**ivx-asset-pipeline**](.cursor/skills/ivx-asset-pipeline/SKILL.md) | "scaffold character", "spritesheet" | 2D/3D character scaffolding, sprite sheet generation, sound manifests, schema validation |
| [**ivx-asset-manager**](.cursor/skills/ivx-asset-manager/SKILL.md) | "replace sprite", "add asset", "swap sound" | **CRUD for any asset** — add, replace, modify, delete sprites/sounds/3D models/scenes/videos from any source (human or AI), with schema validation, manifest updates, audit, and S3 sync |
| [**ivx-character-factory**](.cursor/skills/ivx-character-factory/SKILL.md) | "generate character", "create sprites" | **AI-generated** full sprite sheets, expression grids, emotional states, topic skins — validated against SDK schemas |
| [**ivx-3d-character-pipeline**](.cursor/skills/ivx-3d-character-pipeline/SKILL.md) | "3D character", "rigged model" | Text/image → 3D mesh (Trellis/Meshy), rigged skeleton, animation presets, FBX export, hybrid 3D-to-2D sprite rendering |
| [**ivx-game-audio-factory**](.cursor/skills/ivx-game-audio-factory/SKILL.md) | "game audio", "sound effects" | Full BGM/SFX/voice suites with `sound_manifest.json` validated against SDK schema |
| [**ivx-environment-generator**](.cursor/skills/ivx-environment-generator/SKILL.md) | "generate environment", "parallax" | Scene backgrounds, parallax layers, tilesets, skyboxes, world composition with Unity export |
| [**ivx-avatar-studio**](.cursor/skills/ivx-avatar-studio/SKILL.md) | "create avatar", "photo to avatar" | One-shot 3D avatar from a single photo, expression-driven animation, interactive conversation with TTS |
| [**ivx-ai-integration**](.cursor/skills/ivx-ai-integration/SKILL.md) | "add AI host", "AI NPC" | 7 AI subsystems: NPC dialog, voice, moderation, content gen, profiling |
| [**ivx-procedural-ai**](.cursor/skills/ivx-procedural-ai/SKILL.md) | "generate levels", "procedural content" | AI level/NPC/loot/quest generation with deterministic fallbacks |
| [**ivx-multiplayer**](.cursor/skills/ivx-multiplayer/SKILL.md) | "add multiplayer" | Lobbies, matchmaking, real-time networking, cross-platform matches |
| [**ivx-accessibility**](.cursor/skills/ivx-accessibility/SKILL.md) | "accessibility", "color blind mode" | Color blind filters, screen readers, input remapping, WCAG/XAG audit |
| [**ivx-security-anticheat**](.cursor/skills/ivx-security-anticheat/SKILL.md) | "anti-cheat", "secure save" | Server validation, tamper detection, replay checks, ban system |

### Ship Phase

| Skill | Trigger Phrases | What It Does |
|-------|----------------|-------------|
| [**ivx-devops-cicd**](.cursor/skills/ivx-devops-cicd/SKILL.md) | "CI/CD", "build pipeline" | GitHub Actions for 11 engines, version bumping, store submission |
| [**ivx-quality-gates**](.cursor/skills/ivx-quality-gates/SKILL.md) | "quality gates", "CI validation" | Context validation, runtime safety, TODO gating, UPM export, release packaging |
| [**ivx-crashlytics**](.cursor/skills/ivx-crashlytics/SKILL.md) | "crash reporting", "error tracking" | Crash capture, ANR detection, breadcrumbs, Sentry integration, alert webhooks |
| [**ivx-cross-platform**](.cursor/skills/ivx-cross-platform/SKILL.md) | "port to Godot" | Feature porting between 11 engines, coverage matrix, VR/console/WebGL guides |
| [**ivx-store-launcher**](.cursor/skills/ivx-store-launcher/SKILL.md) | "store assets", "app submission" | App icon, screenshots, key art, trailer generation + automated App Store Connect and Google Play submission |
| [**ivx-localization**](.cursor/skills/ivx-localization/SKILL.md) | "localize my game", "translate store" | Store listing localization, in-game text l10n, font coverage audit, RTL support, locale-aware asset variants, ASO keyword translation |
| [**ivx-landing-page**](.cursor/skills/ivx-landing-page/SKILL.md) | "landing page", "game website" | **Council-reviewed** responsive landing page saturated with real game assets — animated sprites, expression grids, parallax environments, audio previews, device-framed screenshots, trailer embed, pricing, press kit, SEO, deploy to S3/Netlify/Vercel. 4 variants: full page, coming-soon, pricing, press kit |

### Grow Phase

| Skill | Trigger Phrases | What It Does |
|-------|----------------|-------------|
| [**ivx-live-ops**](.cursor/skills/ivx-live-ops/SKILL.md) | "daily rewards", "season pass" | 33+ Hiro systems, Satori A/B tests, feature flags, live events |
| [**ivx-analytics-pipeline**](.cursor/skills/ivx-analytics-pipeline/SKILL.md) | "add analytics", "track events" | Event taxonomy, funnels, cohorts, BigQuery/Snowflake export, dashboards |
| [**ivx-monetization**](.cursor/skills/ivx-monetization/SKILL.md) | "monetize my game" | Ads (3 networks), IAP, offerwalls, server validation, genre strategy |
| [**ivx-remote-config**](.cursor/skills/ivx-remote-config/SKILL.md) | "remote config" | Server-driven config, typed schemas, conditional overrides, rollback |
| [**ivx-notification-orchestration**](.cursor/skills/ivx-notification-orchestration/SKILL.md) | "push notifications" | FCM/APNs/Web Push, smart send-time, deep linking, A/B testing |
| [**ivx-ugc-pipeline**](.cursor/skills/ivx-ugc-pipeline/SKILL.md) | "user-generated content" | UGC upload/moderation/sharing, content browser, creator rewards |
| [**ivx-quiz-content**](.cursor/skills/ivx-quiz-content/SKILL.md) | "set up daily quiz" | S3 quiz pipelines, LLM generation, caching, GitHub Actions cron |
| [**ivx-roblox**](.cursor/skills/ivx-roblox/SKILL.md) | "Roblox integration" | AI NPCs, Hiro live-ops, cross-game identity for Roblox experiences |

### Wiring Skills into Your AI Coding Tool

Skills are `SKILL.md` files that tell your AI coding assistant exactly how to integrate each IntelliVerseX feature. Once wired, you just describe what you want in natural language and the AI follows the skill's instructions.

<details>
<summary><strong>Cursor (Recommended)</strong></summary>

Skills auto-activate when the repo is your workspace. Three ways to set up:

**Option A: Clone the SDK repo directly (simplest)**

```bash
git clone https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git
```

Open the cloned folder in Cursor. All 31 skills in `.cursor/skills/` are detected automatically. No configuration needed.

**Option B: Add skills to your existing game project**

1. Copy the `.cursor/skills/` folder into your game project root:

```bash
# From your game project root
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/
```

2. Your project structure should look like:

```
your-game/
├── .cursor/
│   └── skills/
│       ├── ivx-sdk-setup/SKILL.md
│       ├── ivx-live-ops/SKILL.md
│       ├── ivx-monetization/SKILL.md
│       └── ... (31 skills)
├── Assets/           # Unity
├── Source/            # Unreal
└── ...
```

3. Open your project in Cursor. Skills are detected on the next agent interaction.

**Option C: Cherry-pick only the skills you need**

```bash
mkdir -p .cursor/skills/ivx-sdk-setup
mkdir -p .cursor/skills/ivx-live-ops
mkdir -p .cursor/skills/ivx-monetization

# Copy only the skills you want
cp /path/to/Intelli-verse-X-SDK/.cursor/skills/ivx-sdk-setup/SKILL.md .cursor/skills/ivx-sdk-setup/
cp /path/to/Intelli-verse-X-SDK/.cursor/skills/ivx-live-ops/SKILL.md .cursor/skills/ivx-live-ops/
cp /path/to/Intelli-verse-X-SDK/.cursor/skills/ivx-monetization/SKILL.md .cursor/skills/ivx-monetization/
```

**Verifying skills are active:**

1. Open Cursor Settings (Cmd/Ctrl + Shift + J) and check the Skills section
2. Or start a chat and ask: *"What IntelliVerseX skills do you have access to?"*
3. The agent will list all detected skills from `.cursor/skills/`

**Using skills:**

Just describe what you want. Cursor matches your intent to the right skill:

- *"Set up IntelliVerseX in my Unity project"* → triggers `ivx-sdk-setup`
- *"Add daily rewards and a season pass"* → triggers `ivx-live-ops`
- *"Monetize my game with rewarded ads"* → triggers `ivx-monetization`
- *"Add AI NPC dialog"* → triggers `ivx-ai-integration`
- *"Create a GDD for my puzzle game"* → triggers `ivx-game-design-studio`
- *"Generate a starter project for Godot"* → triggers `ivx-game-design-studio` (Starter Project Generator)

**Wiring the MCP Server (optional, for backend management):**

1. Open Cursor Settings → MCP
2. Add a new server with this config:

```json
{
  "mcpServers": {
    "intelliversex": {
      "url": "https://mcp.intelli-verse-x.ai/api/mcp"
    }
  }
}
```

Or for self-hosted Nakama:

```json
{
  "mcpServers": {
    "intelliversex": {
      "command": "npx",
      "args": ["@intelliversex/mcp-server"],
      "env": {
        "NAKAMA_HOST": "127.0.0.1",
        "NAKAMA_PORT": "7350",
        "NAKAMA_SERVER_KEY": "defaultkey"
      }
    }
  }
}
```

3. Now you can ask the agent things like *"Show me all players"*, *"Grant 1000 coins to user X"*, or *"Create an A/B experiment for store pricing"*.

</details>

<details>
<summary><strong>VS Code + GitHub Copilot</strong></summary>

Copilot Chat reads workspace files as context. Skills work when referenced or present in the workspace.

**Step 1: Add skills to your project**

```bash
# From your game project root
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/
```

**Step 2: Create a Copilot instructions file**

Create `.github/copilot-instructions.md` in your project root:

```markdown
# Copilot Instructions

This project uses IntelliVerseX SDK with AI agent skills.

When I ask about game development features, check the relevant skill file
in `.cursor/skills/` for detailed integration instructions:

- SDK setup: `.cursor/skills/ivx-sdk-setup/SKILL.md`
- Live ops (daily rewards, streaks, seasons): `.cursor/skills/ivx-live-ops/SKILL.md`
- Monetization (ads, IAP, offerwalls): `.cursor/skills/ivx-monetization/SKILL.md`
- AI integration (NPC, voice, moderation): `.cursor/skills/ivx-ai-integration/SKILL.md`
- Multiplayer (lobbies, matchmaking): `.cursor/skills/ivx-multiplayer/SKILL.md`
- Analytics (events, funnels, cohorts): `.cursor/skills/ivx-analytics-pipeline/SKILL.md`
- Economy (simulation, balance): `.cursor/skills/ivx-economy-simulator/SKILL.md`
- Game design (GDD, brainstorm): `.cursor/skills/ivx-game-design-studio/SKILL.md`
- Narrative (dialog, story): `.cursor/skills/ivx-narrative-engine/SKILL.md`
- Procedural content (levels, NPCs, loot): `.cursor/skills/ivx-procedural-ai/SKILL.md`
- Accessibility: `.cursor/skills/ivx-accessibility/SKILL.md`
- Security & anti-cheat: `.cursor/skills/ivx-security-anticheat/SKILL.md`
- CI/CD & DevOps: `.cursor/skills/ivx-devops-cicd/SKILL.md`
- Quality gates: `.cursor/skills/ivx-quality-gates/SKILL.md`
- Crash reporting: `.cursor/skills/ivx-crashlytics/SKILL.md`
- Cross-platform porting: `.cursor/skills/ivx-cross-platform/SKILL.md`
- Remote config: `.cursor/skills/ivx-remote-config/SKILL.md`
- Push notifications: `.cursor/skills/ivx-notification-orchestration/SKILL.md`
- UGC pipeline: `.cursor/skills/ivx-ugc-pipeline/SKILL.md`
- Quiz content: `.cursor/skills/ivx-quiz-content/SKILL.md`
- Asset pipeline: `.cursor/skills/ivx-asset-pipeline/SKILL.md`
- Roblox: `.cursor/skills/ivx-roblox/SKILL.md`
- Localization: `.cursor/skills/ivx-localization/SKILL.md`
- Landing page: `.cursor/skills/ivx-landing-page/SKILL.md`

Always read the full SKILL.md file before providing integration guidance.
```

**Step 3: Use with Copilot Chat**

1. Open Copilot Chat (Ctrl+Shift+I or Cmd+Shift+I)
2. Reference a skill file directly:

```
@workspace /explain #file:.cursor/skills/ivx-live-ops/SKILL.md
How do I add daily rewards to my game?
```

Or ask naturally and let it find the right skill:

```
How do I add monetization to my Unity game? Check the IVX skills.
```

**Step 4: Copilot Edits (multi-file)**

For larger integrations, use Copilot Edits:

1. Open Copilot Edits (Ctrl+Shift+I → switch to Edits mode)
2. Add the relevant SKILL.md to the working set
3. Add your target files
4. Describe what you want: *"Following the ivx-live-ops skill, add daily rewards to my GameManager"*

</details>

<details>
<summary><strong>Claude Code (Terminal)</strong></summary>

Claude Code reads project files and uses them as context during conversations.

**Step 1: Add skills to your project**

```bash
cd your-game-project
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/
```

**Step 2: Create a CLAUDE.md file**

Create `CLAUDE.md` in your project root so Claude Code auto-discovers the skills:

```markdown
# Project Context

This project uses IntelliVerseX SDK. Agent skills for every feature
are located in `.cursor/skills/`. Each subdirectory contains a SKILL.md
with complete integration instructions, code examples, and checklists.

When asked to integrate any IntelliVerseX feature, read the corresponding
SKILL.md file first, then follow its instructions step by step.

## Available Skills

- ivx-sdk-setup — Bootstrap and configure the SDK
- ivx-live-ops — Hiro + Satori live operations (33+ systems)
- ivx-monetization — Ads, IAP, offerwalls
- ivx-ai-integration — AI voice, NPC dialog, moderation
- ivx-multiplayer — Lobbies, matchmaking, real-time
- ivx-analytics-pipeline — Event taxonomy, funnels, export
- ivx-economy-simulator — Currency flows, Monte Carlo, pricing
- ivx-game-design-studio — GDD templates, design review, starter project generator
- ivx-narrative-engine — Branching dialog, story state, cutscenes
- ivx-procedural-ai — Level/NPC/loot/quest generation
- ivx-accessibility — Color blind, screen reader, WCAG audit
- ivx-security-anticheat — Server validation, tamper detection
- ivx-devops-cicd — CI/CD pipelines for 11 engines
- ivx-quality-gates — Context validation, release packaging
- ivx-crashlytics — Crash capture, breadcrumbs, alerts
- ivx-cross-platform — Porting between 11 engines
- ivx-remote-config — Server-driven configuration
- ivx-notification-orchestration — Push notifications, A/B testing
- ivx-ugc-pipeline — User-generated content moderation
- ivx-quiz-content — S3-hosted quiz pipelines
- ivx-asset-pipeline — Character scaffolding, spritesheets
- ivx-roblox — Roblox AI NPCs, Hiro, identity
- ivx-localization — Store, text, font, RTL, keyword localization
- ivx-landing-page — Council-reviewed game landing pages
```

**Step 3: Use naturally**

```bash
claude
```

Then in the conversation:

```
> Read .cursor/skills/ivx-live-ops/SKILL.md and add daily rewards to my game
> Set up the IntelliVerseX SDK following the skill instructions
> What skills are available for monetization?
```

Claude Code will read the SKILL.md and follow its configuration, code examples, and checklist.

**Step 4: Use the /read shortcut**

```
> /read .cursor/skills/ivx-monetization/SKILL.md
> Now wire rewarded ads with server validation into my GameManager.cs
```

</details>

<details>
<summary><strong>Windsurf</strong></summary>

Windsurf (by Codeium) supports the same `.cursor/skills/` format natively.

**Step 1: Add skills to your project**

```bash
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/
```

**Step 2: Open in Windsurf**

Open your project folder in Windsurf. Skills in `.cursor/skills/` are automatically detected by Cascade (Windsurf's AI agent).

**Step 3: Use naturally**

Open Cascade (the AI panel) and describe what you need:

- *"Set up IntelliVerseX in my Unity project"*
- *"Add a season pass using the live ops skill"*
- *"Wire crash reporting with Sentry"*

Cascade reads the matching SKILL.md and follows its instructions, just like Cursor.

**Wiring the MCP Server:**

1. Open Windsurf Settings → MCP Servers
2. Add the same config as Cursor (see Cursor section above)

</details>

<details>
<summary><strong>Antigravity</strong></summary>

Antigravity supports intent-based workflows through its agent system.

**Step 1: Add skills and create the agents config**

```bash
# Copy skills
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/

# Create Antigravity agents directory
mkdir -p .agents
```

**Step 2: Create `.agents/README.md`**

```markdown
# Antigravity Agent Configuration

## IntelliVerseX Skills

This project uses IntelliVerseX SDK agent skills for game development.
Skills are stored in `.cursor/skills/` as SKILL.md files.

## Intent Detection

When the developer describes a task, match their intent to the right skill:

| Intent Keywords | Skill File |
|----------------|-----------|
| setup, bootstrap, configure, install | `.cursor/skills/ivx-sdk-setup/SKILL.md` |
| daily rewards, streaks, season pass, live ops, achievements | `.cursor/skills/ivx-live-ops/SKILL.md` |
| ads, IAP, monetize, offerwall, rewarded | `.cursor/skills/ivx-monetization/SKILL.md` |
| AI, NPC, dialog, voice, moderation | `.cursor/skills/ivx-ai-integration/SKILL.md` |
| multiplayer, lobby, matchmaking | `.cursor/skills/ivx-multiplayer/SKILL.md` |
| analytics, events, funnels, cohorts | `.cursor/skills/ivx-analytics-pipeline/SKILL.md` |
| economy, currency, balance, simulation | `.cursor/skills/ivx-economy-simulator/SKILL.md` |
| GDD, game design, brainstorm | `.cursor/skills/ivx-game-design-studio/SKILL.md` |
| dialog, narrative, story, branching | `.cursor/skills/ivx-narrative-engine/SKILL.md` |
| procedural, generate levels, loot | `.cursor/skills/ivx-procedural-ai/SKILL.md` |
| accessibility, color blind, screen reader | `.cursor/skills/ivx-accessibility/SKILL.md` |
| anti-cheat, security, tamper, ban | `.cursor/skills/ivx-security-anticheat/SKILL.md` |
| CI/CD, build pipeline, deploy | `.cursor/skills/ivx-devops-cicd/SKILL.md` |
| quality, validation, gates | `.cursor/skills/ivx-quality-gates/SKILL.md` |
| crash, error, ANR, breadcrumb | `.cursor/skills/ivx-crashlytics/SKILL.md` |
| port, cross-platform, engine | `.cursor/skills/ivx-cross-platform/SKILL.md` |
| remote config, feature flag | `.cursor/skills/ivx-remote-config/SKILL.md` |
| push notification, FCM, APNs | `.cursor/skills/ivx-notification-orchestration/SKILL.md` |
| UGC, user content, moderation | `.cursor/skills/ivx-ugc-pipeline/SKILL.md` |
| quiz, trivia, S3 content | `.cursor/skills/ivx-quiz-content/SKILL.md` |
| scaffold, spritesheet, asset pipeline | `.cursor/skills/ivx-asset-pipeline/SKILL.md` |
| roblox, luau | `.cursor/skills/ivx-roblox/SKILL.md` |
| localize, translate, i18n, l10n, RTL, CJK | `.cursor/skills/ivx-localization/SKILL.md` |
| landing page, website, pricing page, press kit | `.cursor/skills/ivx-landing-page/SKILL.md` |

## Workflow

1. Detect intent from the developer's message
2. Read the matching SKILL.md file completely
3. Follow its Overview, Configuration, Key Classes, Code Examples, and Checklist
4. Verify all checklist items are satisfied before marking complete
```

**Step 3: Use naturally**

Open your project in Antigravity and describe what you need. The agent reads `.agents/README.md`, matches your intent, reads the right SKILL.md, and follows it.

</details>

<details>
<summary><strong>Devin / OpenAI Codex / Other Agents</strong></summary>

Any AI agent that can read files from a repository can use IntelliVerseX skills.

**Step 1: Add skills to your project**

```bash
cp -r /path/to/Intelli-verse-X-SDK/.cursor/skills/ .cursor/skills/
```

**Step 2: Point the agent at the skill**

When starting a task, include the skill path in your prompt:

```
Read the file .cursor/skills/ivx-live-ops/SKILL.md and follow its
instructions to add daily rewards, streaks, and a spin wheel to my
Unity game. The main game manager is at Assets/Scripts/GameManager.cs.
```

**Step 3: For autonomous agents (Devin, Sweep, etc.)**

Create a `.devin/instructions.md` or equivalent config that references the skills:

```markdown
When working on this project, check .cursor/skills/ for IntelliVerseX
integration guides. Each SKILL.md contains complete setup instructions,
code examples, configuration, and a verification checklist.
```

**General pattern for any agent:**

1. Copy the `.cursor/skills/` folder into your project
2. Tell the agent where the skills are (via instructions file or prompt)
3. Ask for what you need in natural language
4. The agent reads the SKILL.md and follows it

</details>

<details>
<summary><strong>Quick Reference: Which files go where</strong></summary>

| AI Tool | Skills Location | Config File | Auto-Detection |
|---------|----------------|-------------|:--------------:|
| **Cursor** | `.cursor/skills/*/SKILL.md` | None needed | Yes |
| **Windsurf** | `.cursor/skills/*/SKILL.md` | None needed | Yes |
| **Claude Code** | `.cursor/skills/*/SKILL.md` | `CLAUDE.md` (recommended) | With CLAUDE.md |
| **VS Code + Copilot** | `.cursor/skills/*/SKILL.md` | `.github/copilot-instructions.md` | With instructions |
| **Antigravity** | `.cursor/skills/*/SKILL.md` | `.agents/README.md` | With README |
| **Devin** | `.cursor/skills/*/SKILL.md` | `.devin/instructions.md` | With instructions |
| **Any other agent** | `.cursor/skills/*/SKILL.md` | Prompt reference | Manual |

**All tools use the same SKILL.md files.** The only difference is how each tool discovers them.

</details>

---

## Architecture

```
Your Game
    │
    ▼
┌───────────────────────────────────────────────────────────────────┐
│                    IntelliVerseX SDK                               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ │
│  │ Auth     │ │ Economy  │ │ Social   │ │ AI (x7)  │ │ Ads    │ │
│  │ Identity │ │ Wallet   │ │ Friends  │ │ NPC/Voice│ │ IAP    │ │
│  │ Profile  │ │ Store    │ │ Leaderbd │ │ Content  │ │ Offerw │ │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘ │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ │
│  │ Analytics│ │ Multipyr │ │ Narrative│ │ Access-  │ │ Remote │ │
│  │ Funnels  │ │ Lobby    │ │ Dialog   │ │ ibility  │ │ Config │ │
│  │ Cohorts  │ │ Match    │ │ Story    │ │ Color/UI │ │ Flags  │ │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘ │
└───────────────────────────────────────────────────────────────────┘
    │                           │
    ▼                           ▼
┌───────────────────────┐   ┌───────────────────────────────────────┐
│  Nakama Server        │   │  IntelliVerseX AI Backend             │
│  + Hiro (33 systems)  │   │  Voice │ Host │ Moderation │ PCG     │
│  + Satori (analytics) │   │  Content Gen │ Profiler │ Assistant  │
└───────────────────────┘   └───────────────────────────────────────┘
```

---

## Asset Pipeline Tools

Production-ready Python CLI tools for any game project. No engine dependency.

```bash
pip install Pillow jsonschema
```

| Tool | Command | Output |
|------|---------|--------|
| **Scaffold 2D Character** | `python tools/asset-pipeline/scaffold_character.py --name Hero --output assets/characters/` | Character folder with placeholder PNGs, animation specs, and `character.json` |
| **Generate Spritesheet** | `python tools/asset-pipeline/generate_spritesheet.py --frames "frames/idle_*.png" --output idle` | Optimized sprite sheet + `_spec.json` |
| **Validate Sprites** | `python tools/asset-pipeline/validate_specs.py --directory assets/characters/` | Schema compliance + dimension checks |
| **Validate Character** | `python tools/asset-pipeline/validate_character.py --character assets/characters/ --all` | Full metadata + views + animation tier validation |
| **Validate Audio** | `python tools/asset-pipeline/validate_sound_manifest.py --manifest assets/audio/sound_manifest.json` | Schema + required sound coverage check |
| **GDD-to-Entity Export** | `python tools/asset-pipeline/gdd_to_entity.py --gdd-dir design/ --output-dir output/entities/ --brand-id my-studio --game-id my-game` | `brand_entity.json` + `game_context.json` for Content-Factory |
| **Starter Project Generator** | `python tools/boilerplate/generate_starter.py --engine unity --gdd-dir design/ --output-dir output/starter/ --brand-id my-studio --game-id my-game` | Fully-wired game project with auth, economy, daily rewards, streaks, leaderboards, achievements, analytics, FTUE — all connected to IVX SDK |

**10 templates** (2D specs, 3D rigged characters, skeletons, state machines, sound manifests) and **9 JSON Schemas** included.

### Project Boilerplate & Integration

Generate a **complete, runnable game project** or **wire into an existing project** for any of the 11 supported engines with every IntelliVerseX SDK feature wired up. 

#### Mode 1: From Scratch
Creates a brand new project with all 18 features (Auth, Economy, Store, Achievements, Daily Rewards, Energy, Leaderboards, Progression, Settings, FTUE, Retention, Analytics) fully implemented and compiling. All 11 engine templates produce **100% compilable, functional projects** out of the box using local SDK stubs.

```bash
python tools/boilerplate/generate_starter.py \
  --engine unity \
  --brand-entity output/entities/brand_entity.json \
  --game-context output/entities/game_context.json \
  --output-dir output/starter-project/ \
  --brand-id my-studio --game-id my-game
```

#### Mode 2: Wire Into Existing Project
Scans your existing game codebase, detects its architecture (e.g., MVC, Zenject, Signal Bus, Zustand), and generates **only the missing SDK integration code**. Existing files are never overwritten; smart adapters bridge your existing auth/economy to IVX.

```bash
# 1. Analyze existing project
python tools/boilerplate/project_analyzer.py --project ./my-game/ --output project_profile.json

# 2. Wire IVX matching project patterns
python tools/boilerplate/wire_integrator.py --project ./my-game/ --profile project_profile.json --gdd output/entities/
```

| Engine | What You Get |
|--------|-------------|
| **Unity** | Compilable project with scenes, C# scripts, Canvas UI, prefabs, .asmdef, and 19 functional SDK stubs |
| **JavaScript** | npm + React + TypeScript SPA — compiling with local `@intelliversex/sdk` stub package |
| **Godot 4** | Compilable GDScript project with `.tscn` scenes, autoloads, and `addons/intelliversex/` stubs |
| **Roblox** | Rojo + Luau modules with ScreenGui and functional `IntelliVerseX` ModuleScript stub |
| **Java/Android** | Compilable Gradle project with Material Design UI fragments and local `com.intelliversex.sdk` stubs |
| **Flutter** | Compilable Dart project with Material 3 UI screens and local `intelliversex_sdk` pub package |
| **Unreal** | Compilable C++ plugin with UMG widget base classes, `.uproject`, and `.Build.cs` |
| **Defold** | Functional Lua modules with `.gui` scenes, `.collection` files, and native extension stubs |
| **C++** | Compilable CMake project with ImGui demo UI and local `ivx/sdk-cpp` headers |
| **Cocos2d-x** | Compilable CMake project with Cocos UI panels and local `ivx` headers |
| **Web3** | Compilable TypeScript + React + ethers.js project with MetaMask connect and Satori analytics |

The tools read your GDD exports (`brand_entity.json`, `game_context.json`) and customize everything: game name, colors, fonts, tagline, economy config, store items, and SDK server settings. See `ivx-game-design-studio` skill for full documentation.

---

## Content-Factory Integration (AI Asset Generation)

IntelliVerseX is the only SDK that pairs game infrastructure with AI content generation. The [Content-Factory](https://github.com/Intelli-verse-X/content-factory) integration turns text descriptions into production-ready game assets — characters, sprites, 3D models, audio, environments, and store listings.

### The Cost Difference

| Asset | Manual Cost | Content-Factory | Savings |
|-------|:----------:|:---------------:|:-------:|
| Full character sprite set (8 actions) | $500–$2,000 | $0.50–$2.00 | 99.9% |
| 3D rigged character (mesh + skeleton + anims) | $2,000–$10,000 | $2–$5 | 99.9% |
| Expression sheet (6 emotions) | $300–$800 | $0.30–$1.00 | 99.9% |
| Game audio suite (BGM + SFX) | $700–$4,000 | $2–$8 | 99.7% |
| Store assets (icon, screenshots, key art, trailer) | $1,400–$5,500 | $3–$12 | 99.8% |
| **Total per game** | **$4,700–$22,300** | **$7–$28** | **99.8%** |

### Pipelines

```bash
# Generate a complete 2D character with SDK-compliant output
python -m pipelines.runner run --config configs/pipelines/ivx_character_2d.yaml \
  --brand_id my-brand --game_id my-game

# Generate a full game audio suite with sound_manifest.json
python -m pipelines.runner run --config configs/pipelines/ivx_game_sound.yaml \
  --game_id my-game --game_name "My Game"

# Generate a council-reviewed landing page
python -m pipelines.runner run --config configs/pipelines/ivx_landing_page.yaml \
  --param brand_id=my-brand --param game_id=my-game --param variant=landing_page

# Generate EVERYTHING for a game in one command
python -m pipelines.runner run --config configs/pipelines/ivx_full_game.yaml \
  --brand_id my-brand --game_id my-game
```

### IVX Schema Validation

Every generated asset is validated against SDK schemas before delivery:

```bash
# Validate a character's IVX export
python -m utils.ivx.validate characters/Hero/ivx/ --all

# Validate a single file
python -m utils.ivx.validate game_sound/ivx/sound_manifest.json --schema sound-manifest-v2
```

### 8 Content Generation Skills

| Skill | What It Generates |
|-------|------------------|
| **ivx-character-factory** | Sprite sheets, expressions, emotional states, topic skins, promo art |
| **ivx-3d-character-pipeline** | 3D mesh, rigged skeleton, animation clips, blend shapes, FBX/GLB export |
| **ivx-game-audio-factory** | BGM, SFX, stingers, ambient, voice lines + `sound_manifest.json` |
| **ivx-environment-generator** | Parallax layers, tilesets, skyboxes, world scenes |
| **ivx-store-launcher** | App icons, screenshots, key art, trailers + automated store submission |
| **ivx-avatar-studio** | One-shot 3D avatar from a photo, expression animation, interactive TTS |
| **ivx-localization** | Localized store listings, screenshots, ASO keywords, font coverage audit |
| **ivx-landing-page** | Council-reviewed landing pages with animated sprites, audio previews, parallax scenes, press kits (4 variants) |

---

## Asset Coverage Matrix

IntelliVerseX + Content-Factory covers **68 asset types** across in-game, app store, and promotional categories. **43 are fully automated, 57 are automated or assisted (84%).**

Legend: **AUTO** = fully automated | **SEMI** = template/scaffold provided | **TOOL** = SDK tool assists | **NONE** = not covered yet

<details>
<summary><strong>In-Game Assets (29 types — 83% coverage)</strong></summary>

| Asset | Description | Mobile | PC/Steam | Console | WebGL | VR/AR | Roblox |
|-------|-------------|--------|----------|---------|-------|-------|--------|
| 2D Character Sprites | 8 actions, 3-8 frames | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| 2D Expression Grid | 6 emotions, 3x2 grid | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| 2D Emotional States | Full-body emotion poses | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| 2D Topic Skins | Character variants per theme | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| 3D Character Mesh | GLB/FBX rigged model | AUTO | AUTO | AUTO | N/A | AUTO | AUTO |
| 3D Skeleton | Humanoid bone mapping | AUTO | AUTO | AUTO | N/A | AUTO | AUTO |
| 3D Animations | Per-action FBX clips | AUTO | AUTO | AUTO | N/A | AUTO | AUTO |
| 3D State Machine | Animation graph | AUTO | AUTO | AUTO | N/A | AUTO | AUTO |
| 3D PBR Textures | Albedo, normal, ARM | SEMI | SEMI | SEMI | N/A | SEMI | SEMI |
| 3D LODs | Level-of-detail meshes | AUTO | AUTO | AUTO | N/A | AUTO | N/A |
| 3D Blend Shapes | Visemes + expressions | AUTO | AUTO | AUTO | N/A | AUTO | N/A |
| Background Music | BGM per game mode | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Sound Effects | Gameplay SFX | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Stingers | Victory/defeat/achieve | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| UI Sounds | Click, hover, transition | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Ambient Audio | Environmental loops | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Voice Lines | Character voice acting | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Parallax Backgrounds | 3-7 layer scrolling | AUTO | AUTO | AUTO | AUTO | N/A | AUTO |
| Tilesets | Auto-tiling sprite atlas | SEMI | SEMI | SEMI | SEMI | N/A | N/A |
| Skyboxes / HDRI | 6-face cubemap or HDR | SEMI | SEMI | SEMI | N/A | SEMI | SEMI |
| UI Icons / Buttons | In-game UI elements | TOOL | TOOL | TOOL | TOOL | TOOL | TOOL |
| Fonts | Game typography | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Particle Effects | VFX / particle systems | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Shaders / Materials | Visual effects | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Dialogue / Narrative | Branching dialog trees | SEMI | SEMI | SEMI | SEMI | SEMI | SEMI |
| Level Layout / Maps | Level geometry | SEMI | SEMI | SEMI | SEMI | SEMI | SEMI |

**22 AUTO + 6 SEMI + 1 TOOL + 0 NONE**

</details>

<details>
<summary><strong>App Store / Distribution Assets (20 types — 90% coverage)</strong></summary>

| Asset | Description | iOS | Android | Steam | Meta Quest | Switch | Xbox/PS | Epic | itch.io |
|-------|-------------|-----|---------|-------|------------|--------|---------|------|---------|
| App Icon | Platform-sized icon | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Screenshots (Phone) | Store listing | AUTO | AUTO | N/A | N/A | N/A | N/A | N/A | N/A |
| Screenshots (Tablet) | iPad / tablet | AUTO | AUTO | N/A | N/A | N/A | N/A | N/A | N/A |
| Screenshots (Desktop) | 1920x1080 | N/A | N/A | AUTO | N/A | AUTO | AUTO | AUTO | AUTO |
| Screenshots (VR) | 2560x1440 | N/A | N/A | N/A | AUTO | N/A | N/A | N/A | N/A |
| Feature Graphic | Google Play 1024x500 | N/A | AUTO | N/A | N/A | N/A | N/A | N/A | N/A |
| Capsule Images | Steam capsules | N/A | N/A | AUTO | N/A | N/A | N/A | N/A | N/A |
| Hero Image | Steam 3840x1240 | N/A | N/A | AUTO | N/A | N/A | N/A | N/A | N/A |
| Key Art | Main promotional art | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| App Preview Video | 15-30 sec video | AUTO | AUTO | N/A | N/A | N/A | N/A | N/A | N/A |
| Short Description | 80-char tagline | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Full Description | Detailed listing | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Store Title | App name | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Keywords / Tags | ASO keywords | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |
| Content Rating | ESRB/PEGI/IARC | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | N/A |
| Privacy Policy URL | Required for stores | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | N/A |
| Localized Screenshots | Per-language | AUTO | AUTO | AUTO | N/A | N/A | N/A | N/A | N/A |
| Localized Descriptions | Per-language text | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | N/A |
| Localized Keywords | Per-language ASO | AUTO | AUTO | AUTO | N/A | N/A | N/A | N/A | N/A |
| Auto Store Submission | API upload | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO | AUTO |

**20 AUTO + 0 SEMI + 0 NONE**

</details>

<details>
<summary><strong>Promotional / Marketing Assets (19 types — 79% coverage)</strong></summary>

| Asset | Description | Status |
|-------|-------------|--------|
| Game Trailer (30-60 sec) | Characters + gameplay + music | AUTO |
| 4K Trailer Variant | High-res trailer | AUTO |
| Trailer Thumbnail | YouTube/store thumbnail | AUTO |
| Social: Twitter/X Banner | 1500x500 | AUTO |
| Social: Facebook Cover | 820x312 | AUTO |
| Social: Discord Banner | 960x540 | AUTO |
| Social: YouTube Thumbnail | 1280x720 | AUTO |
| Character Promo Art | Per-character key art | AUTO |
| Character Thumbnails | 16:9 character showcase | AUTO |
| Press Kit | Logos, screenshots, descriptions | AUTO |
| Devlog / Blog Posts | Development updates | AUTO |
| Newsletter Assets | Email headers, banners | AUTO |
| Ad Creatives | Playable ads, banner ads | AUTO |
| Influencer Kit | Assets for streamers | AUTO |
| Community Graphics | Reddit, forums, Discord | AUTO |
| Animated GIFs | Steam store, social media | AUTO |
| **Game Landing Page** | Responsive HTML — hero, features, trailer, pricing, SEO | **AUTO** |
| **Coming Soon Page** | Pre-launch email capture + countdown | **AUTO** |
| **Pricing Page** | Standalone pricing tiers | **AUTO** |

**19 AUTO + 0 SEMI + 0 NONE**

</details>

### Coverage Summary

| Category | Total | AUTO | SEMI | TOOL | NONE | Coverage |
|----------|-------|------|------|------|------|----------|
| **In-Game** | 29 | 22 | 6 | 1 | 0 | 100% |
| **App Store** | 20 | 20 | 0 | 0 | 0 | 100% |
| **Promotional** | 19 | 19 | 0 | 0 | 0 | 100% |
| **TOTAL** | **68** | **61** | **6** | **1** | **0** | **100%** |

**61 out of 68 asset types are fully automated. All 68 are automated or assisted.**

---

## MCP Server (50+ Backend Tools)

Manage your game backend directly from AI coding agents via the Model Context Protocol.

```json
{
  "mcpServers": {
    "intelliversex": {
      "url": "https://mcp.intelli-verse-x.ai/api/mcp"
    }
  }
}
```

| Category | Tools | Examples |
|----------|-------|---------|
| **Nakama** | Health, RPC, build, deploy, restart, auth | `nakama_health`, `nakama_rpc`, `nakama_build_deploy` |
| **Hiro** | Config get/set for all 33 systems | `hiro_config_get economy`, `hiro_config_set store` |
| **Satori** | Flags, experiments, live events, messages | `flag_toggle`, `experiment_setup`, `live_event_schedule` |
| **Players** | Inspect, search, wallet, inventory, mailbox | `player_inspect`, `wallet_grant`, `inventory_grant` |
| **Analytics** | Events, metrics, alerts, data lake | `events_timeline`, `metrics_set_alert`, `datalake_manual_export` |
| **Storage** | List, read, write any collection | `storage_list`, `storage_read`, `storage_write` |

---

## Repository Structure

```
Intelli-verse-X-SDK/
├── Assets/
│   ├── Intelli-verse-X-SDK/        # Unity SDK (UPM Package)
│   └── _IntelliVerseXSDK/          # AI, Hiro, Satori, Platform, Demos
├── SDKs/
│   ├── unreal/                      # Unreal Engine 5 Plugin
│   ├── godot/                       # Godot 4 Addon
│   ├── roblox/                      # Roblox / Luau (Wally)
│   ├── defold/                      # Defold Library Module
│   ├── cocos2dx/                    # Cocos2d-x / CMake
│   ├── javascript/                  # npm / TypeScript
│   ├── cpp/                         # Native C++ / CMake
│   ├── java/                        # Java / Gradle / Android
│   ├── flutter/                     # Flutter / Dart (pub.dev)
│   └── web3/                        # Web3 / TypeScript (ethers.js)
├── tools/
│   ├── asset-pipeline/              # Character scaffolding, spritesheets, validation
│   ├── scripts/                     # UPM export, release packaging
│   └── context/                     # Context engineering validation
├── .cursor/skills/                  # 31 AI agent skills
├── docs/                            # MkDocs documentation
├── .github/workflows/               # CI/CD
└── README.md                        # This file
```

---

## Underlying Technology

Built on proven open-source foundations:

| Component | Technology | Role |
|-----------|-----------|------|
| **Game Server** | [Nakama](https://heroiclabs.com/nakama/) (open source) | Auth, storage, leaderboards, real-time multiplayer, RPCs |
| **Live-Ops** | [Hiro](https://heroiclabs.com/hiro/) | 33 game systems (economy, energy, achievements, streaks, store, etc.) |
| **Analytics** | [Satori](https://heroiclabs.com/satori/) | Event tracking, feature flags, A/B experiments, live events |
| **Multiplayer** | Nakama + Photon PUN2 | Real-time networking, lobbies, matchmaking |
| **AI** | OpenAI / Azure / Anthropic / Self-hosted | NPC dialog, voice, moderation, content generation |
| **Ads** | LevelPlay + Appodeal + AdMob | Waterfall mediation with server validation |

---

## Documentation

[**Full Documentation Site**](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/)

| Topic | Link |
|-------|------|
| Getting Started | [Quickstart](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/getting-started/quickstart/) |
| Platform SDKs | [Platforms](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/) |
| API Reference | [API](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/api/core/) |
| AI Agent Skills | [Skills Guide](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/guides/ai-agent-skills/) |
| XR/VR/AR | [XR Guide](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/xr-vr-ar/) |
| Console | [Console Guide](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/platforms/console/) |
| Troubleshooting | [FAQ](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/troubleshooting/faq/) |
| Changelog | [Changelog](https://intelli-verse-x.github.io/Intelli-verse-X-SDK/changelog/) |

---

## Contributing

We welcome contributions for all platforms. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## License

MIT License — see [LICENSE](LICENSE)

---

## Support

- [Issues](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/issues)
- [Discussions](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/discussions)
- [Discord](https://discord.gg/YVPxPFftMQ)
- Email: support@intelli-verse-x.ai

---

<p align="center">Made with care by <a href="https://intelliversex.com">IntelliVerse-X</a></p>
