# AI Agent Skills for IntelliVerseX SDK

Automate your game SDK integration with 7 purpose-built AI agent skills. Works with **Cursor**, **Windsurf**, **Claude Code**, **Devin**, **OpenAI Codex**, and any agent that reads `SKILL.md` files.

---

## Quick Start

### Already Using Cursor or Windsurf?

Skills are included in the repository. Clone the repo and they activate automatically:

```bash
git clone https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
cd Intelli-verse-X-Unity-SDK
# Open in Cursor or Windsurf — skills are auto-discovered from .cursor/skills/
```

Then type a natural language prompt like:

> "Set up IntelliVerseX in my Unity project"

The agent loads the matching skill and walks you through the entire process.

### Using Claude Code?

```bash
# Add the IntelliVerseX marketplace
/plugin marketplace add https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK

# Install a specific skill
/plugin install ivx-sdk-setup
```

### Using SkillsGate? (Works with 18+ AI agents)

```bash
skillsgate add @intelliversex/ivx-sdk-setup
skillsgate add @intelliversex/ivx-monetization
# ... or install all at once
skillsgate add @intelliversex/ivx-sdk-setup @intelliversex/ivx-monetization @intelliversex/ivx-multiplayer @intelliversex/ivx-ai-integration @intelliversex/ivx-live-ops @intelliversex/ivx-quiz-content @intelliversex/ivx-cross-platform
```

---

## The 7 Skills at a Glance

| # | Skill | What It Does | Say This to Activate |
|---|-------|-------------|---------------------|
| 1 | **ivx-sdk-setup** | Install and bootstrap the SDK on any of 10 platforms | "Set up IntelliVerseX" / "integrate SDK" |
| 2 | **ivx-monetization** | Wire ads, IAP, offerwalls, and revenue strategy | "Monetize my game" / "add ads" / "set up offerwall" |
| 3 | **ivx-multiplayer** | Add lobbies, matchmaking, real-time networking | "Add multiplayer" / "create lobby" / "set up matchmaking" |
| 4 | **ivx-ai-integration** | Integrate AI voice host, NPC dialog, content gen | "Add AI host" / "set up AI NPC" / "AI voice chat" |
| 5 | **ivx-live-ops** | Set up Hiro live-ops + Satori analytics | "Add daily rewards" / "set up leagues" / "fortune wheel" |
| 6 | **ivx-quiz-content** | Build quiz content pipelines with S3 + LLM | "Add quiz" / "set up daily quiz" / "generate trivia" |
| 7 | **ivx-cross-platform** | Port features between 10 game engines | "Port to Unreal" / "port to Godot" / "feature parity" |

---

## Skill 1: SDK Setup (`ivx-sdk-setup`)

**What it does:** Walks you through the complete SDK installation, from package manager to verified initialization. Covers all 10 platforms.

**When to use it:**

- Starting a new game project and need the IntelliVerseX SDK
- Migrating from another backend to IntelliVerseX
- Setting up a second platform (e.g., already have Unity, now adding Godot)

**Example prompts:**

- "Set up IntelliVerseX in my Unity project"
- "Add IntelliVerseX to my Node.js game server"
- "Bootstrap the SDK for Godot 4"
- "Configure my game ID and Nakama connection"

**What the agent will do:**

1. Install the package for your platform (UPM, npm, Gradle, pub.dev, CMake, etc.)
2. Create the bootstrap config (ScriptableObject in Unity, config object elsewhere)
3. Fill in required fields: GameId, ServerHost, ServerPort, ServerKey
4. Enable/disable feature modules based on your needs
5. Verify initialization fires successfully
6. Troubleshoot common issues (missing references, empty gameId, connection timeouts)

**Platforms covered:** Unity, JavaScript/TypeScript, Java/Android, Flutter/Dart, Godot 4, Defold, C++, Cocos2d-x, Unreal Engine 5, Web3

---

## Skill 2: Monetization (`ivx-monetization`)

**What it does:** Configures all revenue streams -- ads (LevelPlay/Appodeal/AdMob), in-app purchases, offerwalls (Pubscale/Xsolla), and server-side reward validation.

**When to use it:**

- Adding ads to your game for the first time
- Setting up in-app purchases with Apple/Google
- Integrating an offerwall for alternative revenue
- Choosing a monetization strategy for your game genre

**Example prompts:**

- "Monetize my casual puzzle game"
- "Set up LevelPlay ads with rewarded video"
- "Add Pubscale offerwall and wire it to the wallet"
- "Configure IAP for a coins pack and remove-ads purchase"
- "Add server-side reward validation for rewarded ads"

**What the agent will do:**

1. Help you choose the right strategy based on your game genre
2. Walk through ad provider setup (dashboard account, app keys, ad unit IDs)
3. Create and configure `IVXAdsConfig` or `IVXOfferwallConfig` ScriptableObjects
4. Wire reward callbacks to the wallet system
5. Set up test IDs for development
6. Configure waterfall failover between ad networks
7. Enable server-side validation via Nakama RPC

**Revenue streams covered:**

| Stream | SDK Class | Setup Time |
|--------|-----------|-----------|
| Banner / Interstitial / Rewarded Ads | `IVXAdsManager` | 15 min |
| In-App Purchases | `IVXIAPManager` | 30 min |
| Offerwall (Pubscale + Xsolla) | `IVXOfferwallManager` | 20 min |
| Season Pass | `IVXSeasonPassManager` | 20 min |

---

## Skill 3: Multiplayer (`ivx-multiplayer`)

**What it does:** Adds real-time multiplayer including game modes, lobby management, matchmaking, and in-match networking over Nakama sockets.

**When to use it:**

- Adding online play to a single-player game
- Building a lobby system where friends can join
- Setting up competitive matchmaking with skill-based ranking
- Handling real-time game state synchronization

**Example prompts:**

- "Add online multiplayer to my trivia game"
- "Create a lobby system where players can invite friends"
- "Set up ranked matchmaking with ELO rating"
- "Add real-time score syncing between 4 players"
- "Handle player disconnection and reconnection"

**What the agent will do:**

1. Configure game modes (Solo, LocalMultiplayer, OnlineLobby, QuickMatch, RankedMatch)
2. Set up lobby creation, joining, and host controls
3. Wire matchmaking with configurable player count, rank range, and timeout
4. Implement real-time data send/receive with op codes
5. Handle the full match lifecycle (Lobby -> Countdown -> InGame -> Results)
6. Add disconnection handling and reconnect logic

**Key code patterns the agent uses:**

```csharp
// Create a lobby
var lobby = await IVXLobbyManager.Instance.CreateLobbyAsync(new LobbyOptions {
    MaxPlayers = 4, IsPrivate = false, Label = "trivia-room"
});

// Start matchmaking
await IVXMatchmakingManager.Instance.StartMatchmakingAsync(new MatchmakingOptions {
    MinPlayers = 2, MaxPlayers = 4, RankRange = 200
});

// Real-time data
IVXMatchNetwork.Instance.SendToAll(opCode: 1, data: jsonPayload);
```

---

## Skill 4: AI Integration (`ivx-ai-integration`)

**What it does:** Integrates 7 AI subsystems -- voice host, NPC dialog, content generation, moderation, profiling, and more. Supports OpenAI, Azure, Anthropic, and self-hosted LLMs.

**When to use it:**

- Adding an AI quiz host or narrator
- Creating intelligent NPCs that respond naturally
- Generating game content (questions, levels, descriptions) with AI
- Adding content moderation to chat
- Building player behavior profiles for personalization

**Example prompts:**

- "Add an AI voice host for my trivia game"
- "Set up NPC dialog with GPT-4o"
- "Generate trivia questions using AI"
- "Add content moderation to player chat"
- "Configure Ollama as a self-hosted LLM provider"
- "Set up AI player profiling"

**What the agent will do:**

1. Create `IVXAIConfig` with your chosen provider (IntelliVerseX/OpenAI/Azure/Anthropic/Custom)
2. Securely inject API keys at runtime (never hardcoded)
3. Set up the appropriate subsystem:
   - `IVXAISessionManager` for conversation sessions
   - `IVXAINPCDialogManager` for in-game NPC conversations
   - `IVXAIAssistant` for hints and help
   - `IVXAIModerator` for content filtering
   - `IVXAIContentGenerator` for structured content output
   - `IVXAIProfiler` for player behavior analysis
   - `IVXAIVoiceServices` for real-time voice streaming
4. Configure mock mode for testing without API costs
5. Set token limits and rate limiting for cost control

**Provider support:**

| Provider | Models | Voice | Cost Model |
|----------|--------|-------|-----------|
| IntelliVerseX (Managed) | GPT-4o, Claude, etc. | Yes | Per-token, billed to project |
| OpenAI | GPT-4o, o1, o3 | Yes | Your OpenAI billing |
| Azure OpenAI | Same as OpenAI | Yes | Your Azure billing |
| Anthropic | Claude Opus/Sonnet/Haiku | No | Your Anthropic billing |
| Custom | Ollama, vLLM, LiteLLM | Varies | Self-hosted |

---

## Skill 5: Live Operations (`ivx-live-ops`)

**What it does:** Sets up 33+ server-side engagement systems powered by Hiro (metagame) and Satori (analytics/experiments). From daily rewards to A/B testing.

**When to use it:**

- Adding daily rewards, login streaks, or fortune wheels
- Setting up achievements, badges, or league systems
- Configuring feature flags and A/B experiments
- Building a season pass with free and premium tracks
- Adding live events and rotating store content

**Example prompts:**

- "Add daily rewards with a 7-day calendar"
- "Set up a fortune wheel with weighted rewards"
- "Add achievements and a badge system"
- "Configure A/B testing for the onboarding flow"
- "Set up a 6-tier league system with promotion/relegation"
- "Add a season pass with free and premium tracks"
- "Set up daily missions that rotate every 24 hours"

**What the agent will do:**

1. Verify Hiro and Satori are enabled in `IVXBootstrapConfig`
2. Set up the specific system via `IVXHiroCoordinator`:
   - Economy (currencies, grants, spending)
   - Energy (time-based refills)
   - Achievements (progress tracking, rewards)
   - Streaks (daily login, play streaks)
   - Store (rotating inventory)
   - Challenges (daily/weekly/monthly)
3. Configure Satori for analytics, flags, experiments, and live events
4. Wire server-side config via Nakama storage collections

**Systems available (33+):**

| Category | Systems |
|----------|---------|
| **Engagement** | Daily Rewards, Daily Missions, Fortune Wheel, Streaks, Achievements, Badges |
| **Retention** | Season Pass, Weekly/Monthly Goals, Friend Streaks, Retention v2, Winback |
| **Competition** | Leagues (6-tier), Tournaments, Event Leaderboards |
| **Economy** | Economy, Energy, Inventory, Store, Unlockables |
| **Social** | Teams, Friend Quests, Friend Battles, Chat Moderation |
| **Analytics** | Events, Feature Flags, A/B Experiments, Live Events, Messages |

---

## Skill 6: Quiz Content Pipeline (`ivx-quiz-content`)

**What it does:** Builds an automated quiz content system using S3-hosted JSON files and LLM-based content generation with CI/CD.

**When to use it:**

- Building a trivia or quiz game
- Setting up daily/weekly rotating quiz content
- Automating content generation with GPT-4o
- Creating a hybrid online/offline quiz experience

**Example prompts:**

- "Set up a daily quiz system with S3"
- "Generate trivia questions using GPT-4o and upload to S3"
- "Create a GitHub Action that generates fresh quiz content daily"
- "Add offline fallback for quiz content"
- "Set up weekly themed quizzes (prediction, fortune, emoji)"

**What the agent will do:**

1. Design the S3 bucket structure (`daily/`, `weekly/`, `categories/`)
2. Define JSON schemas for daily and weekly quizzes
3. Configure `IVXS3QuizProvider` with your bucket URL
4. Set up `IVXHybridQuizProvider` for offline fallback
5. Create the Python script for LLM-based content generation
6. Write the GitHub Action for automated daily/weekly generation
7. Add custom quiz types by implementing `IIVXQuizProvider`

**Content pipeline:**

```
GPT-4o -> Python Script -> S3 Bucket -> IVXS3QuizProvider -> Game Client
                                           |
                                    IVXLocalQuizProvider (fallback)
```

---

## Skill 7: Cross-Platform Porting (`ivx-cross-platform`)

**What it does:** Guides porting IntelliVerseX SDK features between 10 game engines. Maps Unity APIs to equivalent calls on each platform.

**When to use it:**

- Launching your game on a second engine (Unity -> Godot, Unreal, etc.)
- Checking which features are available on a target platform
- Converting a stub to a real implementation on a non-Unity SDK
- Understanding the architecture differences between SDKs

**Example prompts:**

- "Port my game from Unity to Godot"
- "Which features work on Unreal Engine?"
- "Convert the Hiro economy stub to real implementation on Flutter"
- "What's the JavaScript equivalent of IVXLobbyManager?"
- "Add leaderboard support to the C++ SDK"

**What the agent will do:**

1. Show the feature coverage matrix for your target platform
2. Map Unity C# classes to the target language equivalents
3. Walk through the RPC contract for the feature you want to port
4. Create the typed RPC wrapper in the target language
5. Build the manager with caching and events
6. Replace the stub and update the coverage matrix

**Class mapping (reference):**

| Unity (C#) | JavaScript | Java | Godot | C++ |
|------------|-----------|------|-------|-----|
| `IVXBootstrap` | `IVXClient` | `IVXClient` | `IVXAutoload` | `IVXClient` |
| `IVXLobbyManager` | `IVXLobby` | `IVXLobby` | `IVXLobby` | `IVXLobby` |
| `IVXAISessionManager` | `IVXAISession` | `IVXAISession` | `IVXAISession` | `IVXAISession` |
| `IVXHiroCoordinator` | `IVXHiro` | `IVXHiro` | `IVXHiro` | `IVXHiro` |

---

## How Skills Work Under the Hood

Each skill is a `SKILL.md` file with:

1. **YAML frontmatter** -- `name`, `description`, and trigger phrases that tell the AI agent when to activate the skill
2. **Structured instructions** -- Step-by-step guidance, code examples, configuration tables, and checklists
3. **Progressive disclosure** -- The agent loads only the skill needed for the current task, keeping context windows lean

Skills do not execute code themselves. They guide the AI agent through the correct integration steps, with the agent making edits to your project files.

### Skill File Locations

```
.cursor/skills/
├── ivx-sdk-setup/SKILL.md
├── ivx-monetization/SKILL.md
├── ivx-multiplayer/SKILL.md
├── ivx-ai-integration/SKILL.md
├── ivx-live-ops/SKILL.md
├── ivx-quiz-content/SKILL.md
└── ivx-cross-platform/SKILL.md
```

---

## Compatibility Matrix

| AI Agent | Supported | Install Method |
|----------|-----------|---------------|
| **Cursor** | Native | Auto-discovered from `.cursor/skills/` |
| **Windsurf** | Native | Same `.cursor/skills/` folder |
| **Claude Code** | Native | `/plugin marketplace add` from repo URL |
| **Devin AI** | Native | Auto-discovers `SKILL.md` in cloned repos |
| **OpenAI Codex** | Via plugin | `.codex-plugin/plugin.json` wrapper |
| **SkillsGate** | CLI install | `skillsgate add @intelliversex/ivx-*` |
| **Killer Skills** | Directory | Listed at killer-skills.com |

---

## Frequently Asked Questions

**Q: Do I need all 7 skills?**
No. Each skill is independent. Install only what you need. Most games start with `ivx-sdk-setup` and one or two others.

**Q: Do skills make API calls or run code?**
No. Skills are documentation that guides your AI agent. The agent reads the skill and then makes edits to your project. No hidden network calls, no telemetry.

**Q: Can I customize a skill?**
Yes. Edit the `SKILL.md` file directly. Add your project-specific conventions, remove sections you don't need, or add new examples.

**Q: Do skills work offline?**
Yes. Skills are local markdown files. Your AI agent reads them from disk. No internet required for the skills themselves (though the SDK features they configure may need a network connection).

**Q: What if I'm not using an AI agent?**
The skills are also excellent reference documentation. Open any `SKILL.md` in your editor and follow the steps manually.

---

## Platform-Specific Skill Coverage

All 7 skills include guidance for XR, console, and WebGL deployment targets:

| Platform | Covered In Skills |
|----------|------------------|
| **Meta Quest (VR)** | sdk-setup (XR compile defines, input), multiplayer (VR latency, world-space lobby), AI (spatial audio), live-ops (VR notifications), quiz (VR UI) |
| **Apple Vision Pro** | sdk-setup (PolySpatial setup), AI (on-device ML), cross-platform (device matrix) |
| **PSVR2** | sdk-setup (Sony plugin), AI (content disclosure TRC), cross-platform (device matrix) |
| **SteamVR / OpenXR** | sdk-setup (OpenXR defines), cross-platform (device matrix) |
| **PS5 / Xbox / Switch** | sdk-setup (console adapter pattern), monetization (console store IAP), multiplayer (platform cert), live-ops (trophy sync), AI (secure key storage) |
| **WebGL / Browser** | sdk-setup (WebGL build notes), monetization (AdSense, no native IAP), multiplayer (CORS, TLS), AI (no mic recording), quiz (IndexedDB cache) |
| **AR (ARKit/ARCore)** | sdk-setup (AR Foundation defines), cross-platform (device matrix) |

---

## Related Guides

- [Monetization Strategy Guide](monetization-strategy.md)
- [Offerwall Integration Guide](offerwall-integration.md)
- [IAP Integration Guide](iap-integration.md)
- [Ad Integration Guide](ad-integration.md)
- [Multiplayer Integration Guide](multiplayer-integration.md)
- [AI Getting Started Guide](ai-getting-started.md)
- [Hiro Live-Ops Integration](hiro-integration.md)
- [Quiz Content Pipeline](quiz-content-pipeline.md)

## Platform Guides

- [XR / VR / AR Platforms](../platforms/xr-vr-ar.md) — Meta Quest, SteamVR, Vision Pro, PSVR2, AR Foundation
- [Console Platforms](../platforms/console.md) — PS5, Xbox Series, Nintendo Switch
- [WebGL / Browser](../platforms/webgl.md) — Unity WebGL deployment
- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md) — Full feature-by-platform breakdown

---

*IntelliVerseX SDK v5.8.0 -- 7 skills, 10 platforms, one natural language interface.*
