# IntelliVerseX Publishing Checklist

> Master checklist of all platforms where the SDK, tools, assets, and studio presence should be published for maximum reach across the gaming ecosystem (170+ countries).

**Last Updated:** 2026-04-03 | **SDK Version:** 5.8.0 (see [`Assets/Intelli-verse-X-SDK/package.json`](../Assets/Intelli-verse-X-SDK/package.json)) | **MCP server-card:** repo validated; **production URL** still requires ops deploy ([`infra/mcp-well-known/README.md`](../infra/mcp-well-known/README.md))

---

## 1. AI Agent Skill / MCP Registries

| # | Platform | Type | Status | Priority | Notes |
|---|----------|------|--------|----------|-------|
| 1 | **Smithery** (smithery.ai) | MCP Registry | Ready | P0 | [`smithery.yaml`](../smithery.yaml); CI validates [`.well-known/mcp/server-card.json`](../.well-known/mcp/server-card.json) vs `smithery.yaml` ([`tools/mcp/validate_server_card.py`](../tools/mcp/validate_server_card.py)). Deploy static card on MCP origin: [`infra/mcp-well-known/README.md`](../infra/mcp-well-known/README.md). Details: [`docs/platforms/mcp-smithery-publish.md`](platforms/mcp-smithery-publish.md) |
| 2 | **Cursor Skills** | Agent Skill | Ready | P0 | [`.gitignore`](../.gitignore) allows **`.cursor/skills/`** (see [`.cursor/skills/README.md`](../.cursor/skills/README.md)) and **`.agents/skills/`** (see [`.agents/skills/README.md`](../.agents/skills/README.md)) for generic `~/.agents/skills/` discovery. Shipped in-repo: [`skills/intelliversex-game-sdk/SKILL.md`](../skills/intelliversex-game-sdk/SKILL.md) (MCP skill) + [`skills/platforms/`](../skills/platforms/README.md) + narrative guides in [`docs/guides/skills/`](guides/skills/index.md) (35+ topics; not all are separate `SKILL.md` packs). |
| 3 | **Windsurf / Codeium Skills** | Agent Skill | Ready | P0 | Same `SKILL.md` pattern; submit via Codeium marketplace using repo URL + [`skills/intelliversex-game-sdk/SKILL.md`](../skills/intelliversex-game-sdk/SKILL.md) as sample |
| 4 | **OpenAI Codex Plugins** | Plugin | Ready | P0 | [`.codex-plugin/plugin.json`](../.codex-plugin/plugin.json) — 7 skill IDs + MCP block |
| 5 | **Claude Code Skills** | Skill | Ready | P1 | YAML frontmatter in [`skills/intelliversex-game-sdk/SKILL.md`](../skills/intelliversex-game-sdk/SKILL.md); marketplace install per [`docs/guides/skills/index.md`](guides/skills/index.md) |
| 6 | **Gemini Code Assist** | MCP | Ready | P1 | Add MCP URL from `smithery.yaml` in IDE settings; same endpoint as row 1 |
| 7 | **SkillsGate** | Aggregator | Pending | P1 | CLI-based publish; confirm live CLI/package names before scripting |
| 8 | **Killer Skills** | Directory | Pending | P1 | Submit repo link when directory accepts listings |
| 9 | **PolySkill** | Aggregator | Pending | P2 | Claude Code skill hub — submit when form is available |

**Skill documentation map:** Narrative guides live under [`docs/guides/skills/`](guides/skills/index.md). The **Codex plugin** lists seven skill IDs in [`.codex-plugin/plugin.json`](../.codex-plugin/plugin.json). **Smithery-oriented MCP skill** package: [`skills/intelliversex-game-sdk/SKILL.md`](../skills/intelliversex-game-sdk/SKILL.md). For Cursor auto-discovery, add per-topic `SKILL.md` under [`.cursor/skills/`](../.cursor/skills/README.md) or extend [`skills/`](../skills/README.md).

### Flash sprint — owner actions (human checklist)

Complete these outside the repo (accounts, dashboards, infra). Track dates in your team calendar.

| Step | Owner action |
|------|----------------|
| Version / branch | Use **`main`** (or your release branch) for public links. Listing copy should match **5.8.0** unless you bump [`package.json`](../Assets/Intelli-verse-X-SDK/package.json). |
| MCP ops | **Verified:** `GET /api/mcp` without auth → **401** (OK). **Prod deploy (ops):** serve [`.well-known/mcp/server-card.json`](../.well-known/mcp/server-card.json) at `https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json` — runbook [`infra/mcp-well-known/README.md`](../infra/mcp-well-known/README.md) (nginx, ALB, S3, or app route). **Repo cannot push to your AWS account.** After deploy, expect **200** on that URL; CI workflow [`mcp-server-card.yml`](../.github/workflows/mcp-server-card.yml) logs an informational probe. |
| Smithery | Log in, connect GitHub repo, publish MCP from `smithery.yaml`; add skill path `skills/intelliversex-game-sdk/` if the UI requests a Skill package. |
| Codex | Install plugin from repo via `.codex-plugin/plugin.json`; smoke-test skill **`ivx-sdk-setup`**. |
| Cursor policy | **Done in repo:** `.gitignore` allows `.cursor/skills/`; [`.cursor/skills/README.md`](../.cursor/skills/README.md) documents usage. |
| Windsurf / Codeium | Marketplace submission with repo URL + `skills/intelliversex-game-sdk/SKILL.md`. |
| Claude Code | `/plugin marketplace add` + install; verify against current Anthropic docs. |
| Gemini | MCP URL in Code Assist settings; verify tools or health. |
| Aggregators | SkillsGate, Killer Skills, PolySkill — submit when each platform is ready; set rows 7–9 to **Published** when live. |

---

## 2. Package Registries

| # | Platform | Package Type | Status | Priority | Notes |
|---|----------|-------------|--------|----------|-------|
| 10 | **npm** (npmjs.com) | JS/TS SDK | Pending | P0 | `SDKs/javascript/` — `npm publish` |
| 11 | **pub.dev** | Flutter/Dart | Pending | P0 | `SDKs/flutter/` — `dart pub publish` |
| 12 | **Maven Central** | Java/Android | Pending | P0 | `SDKs/java/` — Gradle publish |
| 13 | **NuGet** | Unity .NET | Pending | P1 | UPM package; consider NuGet mirror |
| 14 | **PyPI** | Python tools | Pending | P2 | `tools/` utilities if applicable |
| 15 | **Wally** (wally.run) | Roblox/Luau | Pending | P0 | `SDKs/roblox/wally.toml` — `wally publish` |
| 16 | **crates.io** | Rust bindings | Pending | P2 | Future — if Rust SDK is added |
| 17 | **CocoaPods / SPM** | iOS native | Pending | P2 | Future — for iOS-native wrappers |
| 18 | **OpenUPM** (openupm.com) | Unity UPM | Pending | P0 | `docs/openupm-submission.md` — submit to OpenUPM registry |
| 19 | **Conan Center** (conan.io) | C/C++ SDK | Pending | P1 | `docs/conan-center-submission.md` — submit Conan recipe |
| 20 | **vcpkg** (vcpkg.io) | C/C++ SDK | Pending | P1 | `docs/vcpkg-PR-description.md` — submit vcpkg port |

---

## 3. Game Engine Asset Stores

| # | Platform | Engine | Status | Priority | Notes |
|---|----------|--------|--------|----------|-------|
| 21 | **Unity Asset Store** | Unity | Pending | P0 | UPM package; requires Asset Store publisher account |
| 22 | **Unreal Marketplace** | Unreal | Pending | P0 | `SDKs/unreal/` — Submit as Code Plugin |
| 23 | **Godot Asset Library** | Godot | Pending | P0 | `SDKs/godot/` — assetlib.godotengine.org |
| 24 | **Defold Asset Portal** | Defold | Pending | P1 | `SDKs/defold/` — defold.com/assets |
| 25 | **Roblox Creator Store** | Roblox | Pending | P0 | Studio Plugin from `SDKs/roblox/plugin/` |
| 26 | **Cocos Store** | Cocos | Pending | P2 | `SDKs/cocos2dx/` |
| 27 | **GameMaker Marketplace** | GameMaker | Pending | P2 | Future — if GameMaker SDK is added |
| 28 | **Construct Addon Store** | Construct | Pending | P2 | Future |

---

## 4. Mobile / Desktop / Console Stores (for published games)

| # | Platform | Reach | Status | Priority | Notes |
|---|----------|-------|--------|----------|-------|
| 29 | **Apple App Store** | iOS, macOS, tvOS, visionOS | Planning | P0 | For games built with SDK |
| 30 | **Google Play Store** | Android | Planning | P0 | For games built with SDK |
| 31 | **Steam** | PC, Mac, Linux, SteamOS, Steam Deck | Planning | P0 | Steamworks integration |
| 32 | **Microsoft Store / Xbox** | Windows, Xbox | Planning | P1 | Xbox GDK required |
| 33 | **PlayStation Store** | PS5, PS4 | Planning | P1 | Sony PS Partners required |
| 34 | **Nintendo eShop** | Switch | Planning | P1 | Nintendo Developer Portal |
| 35 | **Epic Games Store** | PC, Mac | Planning | P1 | Epic publishing tools |
| 36 | **Meta Quest Store** | Quest 2/3/Pro | Planning | P1 | Meta Developer Hub |
| 37 | **Samsung Galaxy Store** | Samsung devices | Planning | P2 | Samsung Developer portal |
| 38 | **Huawei AppGallery** | Huawei devices (China, SEA) | Planning | P2 | Huawei Developer Console |
| 39 | **Amazon Appstore** | Fire tablets, Fire TV | Planning | P2 | Amazon Developer portal |

---

## 5. Web Game Platforms

| # | Platform | Reach | Status | Priority | Notes |
|---|----------|-------|--------|----------|-------|
| 40 | **Roblox** | 70M+ DAU, 170+ countries | **SDK Ready** | P0 | `SDKs/roblox/` published |
| 41 | **itch.io** | Indie focused | Pending | P0 | WebGL builds; high indie visibility |
| 42 | **Newgrounds** | Web games | Pending | P1 | WebGL/HTML5 exports |
| 43 | **CrazyGames** | 30M+ monthly | Pending | P1 | HTML5/WebGL; rev-share model |
| 44 | **Poki** | 50M+ monthly | Pending | P1 | HTML5/WebGL; partnership model |
| 45 | **Kongregate** | Web/mobile | Pending | P2 | If still accepting submissions |
| 46 | **GameJolt** | Indie community | Pending | P2 | WebGL + desktop builds |
| 47 | **Y8** | Casual web games | Pending | P2 | HTML5/WebGL |
| 48 | **Miniclip** | Mobile/web | Pending | P2 | Partnership model |

---

## 6. Social / Messaging Game Platforms

| # | Platform | Reach | Status | Priority | Notes |
|---|----------|-------|--------|----------|-------|
| 49 | **Discord Activities** | 150M+ monthly | Pending | P0 | Embedded apps via Discord SDK |
| 50 | **Telegram Mini Apps** | 800M+ users | Pending | P0 | WebApp/WebGL; huge growth |
| 51 | **Facebook Instant Games** | 2.9B users | Pending | P1 | HTML5; FB Gaming platform |
| 52 | **Snapchat Minis** | 750M+ monthly | Pending | P1 | HTML5 mini-games |
| 53 | **WeChat Mini Games** | 1.2B users (China) | Pending | P2 | Requires China entity |
| 54 | **LINE Games** | Japan, SEA | Pending | P2 | LINE Developer Console |
| 55 | **Viber Games** | Europe, SEA | Pending | P2 | Viber Developer platform |

---

## 7. Game Asset Marketplaces (Spritesheets, Sounds, Mini-Games)

| # | Platform | Asset Types | Status | Priority | Notes |
|---|----------|------------|--------|----------|-------|
| 56 | **itch.io** (assets) | Sprites, sounds, tools | Pending | P0 | Free/paid asset pages |
| 57 | **Unity Asset Store** | Sprites, sounds, prefabs | Pending | P0 | Unity-specific asset bundles |
| 58 | **Roblox Creator Store** (models) | Models, plugins, audio | Pending | P0 | Publish SDK + example models |
| 59 | **OpenGameArt** | Sprites, sounds, music | Pending | P1 | Free/CC licensed community |
| 60 | **Kenney.nl** | Game assets | Pending | P2 | Partnership/cross-promotion |
| 61 | **GameDev Market** | All asset types | Pending | P1 | Curated marketplace |
| 62 | **Humble Bundle** (asset packs) | Bundles | Pending | P2 | Partnership for visibility |
| 63 | **Turbosquid / Sketchfab** | 3D models | Pending | P2 | If publishing 3D assets |
| 64 | **Freesound** | Sound effects | Pending | P2 | Community audio |
| 65 | **Unreal Marketplace** (assets) | UE5 assets | Pending | P1 | Unreal-specific packs |
| 66 | **Godot Asset Library** (assets) | Godot projects | Pending | P1 | Example projects + assets |

---

## 8. Developer Tools & Launch Platforms

| # | Platform | Purpose | Status | Priority | Notes |
|---|----------|---------|--------|----------|-------|
| 67 | **GitHub** | Source, releases, Actions | Published | P0 | Main repo |
| 68 | **GitHub Marketplace** | Actions/Apps | Pending | P1 | CI/CD action for SDK integration |
| 69 | **Product Hunt** | Launch visibility | Pending | P0 | For SDK launch campaign |
| 70 | **Hacker News** | Developer reach | Pending | P0 | Show HN post |
| 71 | **Dev.to** | Technical articles | Pending | P0 | Integration tutorials |
| 72 | **Medium** | Thought leadership | Pending | P1 | Game dev AI articles |
| 73 | **Roblox DevForum** | Roblox developers | Pending | P0 | Community resource thread |
| 74 | **Roblox Creator Hub** | Official Roblox docs | Pending | P0 | Submit as featured resource |
| 75 | **IndieDB** | Indie game discovery | Pending | P1 | SDK tool listing |
| 76 | **AlternativeTo** | Software discovery | Pending | P2 | List as alternative to PlayFab/GameSparks |

---

## 9. Game Backend & BaaS Ecosystem

| # | Platform | Type | Status | Priority | Notes |
|---|----------|------|--------|----------|-------|
| 77 | **Heroic Labs / Nakama** | Backend ecosystem | Published | P0 | Core dependency; listed in Nakama community |
| 78 | **Hiro by Heroic Labs** | Live-ops | Published | P0 | Tight integration |
| 79 | **Roblox Open Cloud** | Roblox API | Pending | P1 | For cross-experience data sync marketing |
| 80 | **AWS for Games** | Cloud partner | Pending | P2 | Partner program listing |
| 81 | **Google Cloud for Games** | Cloud partner | Pending | P2 | Partner program listing |

---

## 10. Studio Portfolio & Gaming Ecosystem Presence

| # | Platform | Purpose | Status | Priority | Notes |
|---|----------|---------|--------|----------|-------|
| 82 | **intelli-verse-x.ai** | Official website | Published | P0 | Landing page, docs, dashboard |
| 83 | **LinkedIn** (company page) | B2B visibility | Pending | P0 | Company page + showcase |
| 84 | **Twitter/X** | Community | Pending | P0 | Dev updates, SDK releases |
| 85 | **Discord** (community server) | Developer community | Pending | P0 | Support, showcase, feedback |
| 86 | **YouTube** | Tutorials | Pending | P0 | Integration walkthroughs |
| 87 | **Roblox Group** | Roblox community | Pending | P0 | Official Roblox developer group |
| 88 | **GDC Vault / Talks** | Industry presence | Pending | P2 | Conference submissions |
| 89 | **Game Dev Directories** | Discovery | Pending | P1 | gamedev.net, game-development.com, etc. |

---

## Roblox-Specific Publishing (NEW)

Platforms specific to the Roblox ecosystem that need separate attention:

| # | Platform | What to Publish | Status | Priority | Action Required |
|---|----------|----------------|--------|----------|-----------------|
| R1 | **Wally Registry** (wally.run) | `ivx-roblox` package | Pending | P0 | `wally publish` — requires Wally account |
| R2 | **Roblox Creator Store** | Studio Plugin (config panel) | Pending | P0 | Upload `Plugin.server.lua` via Creator Hub; set price = Free |
| R3 | **Roblox Creator Hub** | Featured resource listing | Pending | P0 | Apply at create.roblox.com/resources |
| R4 | **Roblox DevForum** | Community Resources thread | Pending | P0 | Post in Community Resources category; include tutorial + demo place |
| R5 | **Roblox Creator Store** (models) | Example NPC models with IVXNpcKey attribute | Pending | P1 | Upload pre-configured NPC models for drag-and-drop |
| R6 | **Roblox Open Cloud API** | Marketing integration | Pending | P1 | Demonstrate cross-experience data sync via Open Cloud |
| R7 | **Roblox Creator Store** (audio) | SDK notification sounds | Pending | P2 | Achievement unlocked, daily reward, spin wheel SFX |
| R8 | **Roblox YouTube / DevRel** | Tutorial video | Pending | P1 | 5-minute "AI NPCs in Roblox with IntelliVerseX" video |
| R9 | **Roblox Talent Hub** | Hiring visibility | Pending | P2 | Post SDK-related contract opportunities |
| R10 | **Roblox UGC Program** | UGC items with SDK integration | Pending | P2 | Cross-promote via avatar items |

---

## Summary by Priority

| Priority | Count | Description |
|----------|-------|-------------|
| **P0** | 41 | Must-publish for launch. Includes npm, Wally, Unity Asset Store, Roblox Creator Store, Smithery, major web/social platforms |
| **P1** | 30 | High-value, publish within first quarter. Console stores, secondary marketplaces, developer communities |
| **P2** | 28 | Nice-to-have. Regional stores, niche platforms, future partnerships |
| **Total** | **99** | Across 10 categories |

---

## Repository updates (agent / MCP / skills) — 2026-04-03

Done in-repo (documentation only; no change to runtime SDK code):

- **Broken links fixed:** Platform docs under `docs/platforms/` no longer pointed to `.cursor/skills/.../SKILL.md` (paths not on GitHub). They now reference **`docs/guides/skills/ivx-*.md`** for the seven Codex-aligned topics.
- **`docs/guides/ai-agent-skills.md`** and **`docs/guides/skills/index.md`** updated to state that **canonical guides** live under `docs/guides/skills/`, MCP skill under `skills/intelliversex-game-sdk/SKILL.md`, and **`.cursor/skills/`** may be committed (see [`.gitignore`](../.gitignore)).
- **`skills/README.md`** adds a **Codex skill ID → guide** table linking to `docs/guides/skills/`.
- **`.gitignore`:** `.cursor/*` is ignored except **`.cursor/skills/**`**; [`.cursor/skills/README.md`](../.cursor/skills/README.md) explains usage.
- **`skills/platforms/`:** Per-engine **`SKILL.md`** stubs map each `SDKs/<engine>/` (plus `unity-upm` → `Assets/Intelli-verse-X-SDK/`) to [`docs/platforms/`](../docs/platforms/index.md).
- **MCP static server card:** [`tools/mcp/validate_server_card.py`](../tools/mcp/validate_server_card.py) + [`.github/workflows/mcp-server-card.yml`](../.github/workflows/mcp-server-card.yml); production deployment steps [`infra/mcp-well-known/README.md`](../infra/mcp-well-known/README.md); [`docs/platforms/mcp-smithery-publish.md`](platforms/mcp-smithery-publish.md) updated with response contract and verification table.

---

## Remaining tasks for you (cannot be completed inside the repo)

### Section 1 — AI agent / MCP / skills (platform skills)

| Item | Status in table | Your action |
|------|-----------------|-------------|
| Smithery MCP publish + static `server-card` on origin | Ready (repo); **prod URL** pending ops | Apply [`infra/mcp-well-known/README.md`](../infra/mcp-well-known/README.md) on `mcp.intelli-verse-x.ai`; then Smithery UI + [`mcp-smithery-publish.md`](platforms/mcp-smithery-publish.md) verification table |
| Cursor row | **Ready** (row 2) | Optional: add more per-topic `SKILL.md` under [`.cursor/skills/`](../.cursor/skills/README.md) |
| Windsurf / Codeium | Ready | Submit marketplace entry (human) |
| OpenAI Codex | Ready | Install plugin in product; smoke-test |
| Claude Code | Ready | Marketplace + install |
| Gemini Code Assist | Ready | MCP URL in IDE |
| SkillsGate, Killer Skills, PolySkill | Pending | Submit when each service accepts listings |

### Sections 2–10 — package registries, stores, social, etc.

Almost all rows are still **Pending** or **Planning**. Publishing requires **accounts, credentials, builds, fees, and store review** — track each row in the tables above; nothing here is auto-completed by doc edits.

---

## Checklist for Each Publish

Before publishing to any platform:

- [ ] README / description matches platform formatting requirements
- [ ] Screenshots / media assets prepared (1280x720 minimum)
- [ ] Version number consistent across all manifests (currently 5.8.0)
- [ ] License file included (MIT)
- [ ] No secrets or API keys in published artifacts
- [ ] CHANGELOG up to date
- [ ] Links to documentation site working
- [ ] Contact/support email configured
