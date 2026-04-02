# IntelliVerseX SDK — Master Integration Prompt

> **One prompt to integrate every SDK feature into any game project.**
> Copy, customize, paste into your AI coding assistant, and go.

**SDK Version:** 5.5.0  
**Supported Platforms:** Unity | Unreal Engine 5 | Godot 4 | Defold | Cocos2d-x | JavaScript/TypeScript | C++ | Java/Android | Flutter/Dart | Web3

---

## The Prompt

Copy everything inside the code block below:

````text
Integrate the IntelliVerseX SDK (v5.5.0) into my game project.
The SDK provides a complete game backend through Nakama + Hiro + Satori + IntelliVerseX AI.

### Backend Configuration
- Nakama server URL: [YOUR_NAKAMA_URL]
- Nakama server key: [YOUR_SERVER_KEY]
- AI backend URL: [YOUR_AI_URL]           (leave blank if not using AI)
- AI API key: [YOUR_AI_KEY]               (leave blank if not using AI)

---

## 1. SDK Initialization
Create a central manager that runs on app start:
1. Configure IVXConfig with the Nakama server URL and server key.
2. Call IVXManager.initialize() once.
3. Restore a saved session if one exists; otherwise authenticate with device ID.
4. On successful auth, fetch the player profile and wallet.

## 2. Player Identity & Profile
- Support authentication methods: device ID, email/password, Google Sign-In, Apple Sign-In, custom token.
- Fetch and cache the player profile (username, avatar URL, metadata JSON).
- Allow the player to update their display name and avatar.
- Persist the session token securely so the player does not re-authenticate on every launch.

## 3. Virtual Economy & Wallet
- Fetch the player's wallet balances (coins, gems, premium currency).
- Grant currency after match completions, daily rewards, or purchases.
- Use Hiro Economy system for a managed virtual store with catalog items.
- Display wallet balances in the HUD.

## 4. Leaderboards
- After each match, submit the score: provide leaderboard ID, score value, and optional metadata JSON.
- Display a leaderboard screen with global and friends-only tabs.
- Support multiple time windows: daily, weekly, all-time.
- Paginate results (20 per page) and highlight the current player's rank.

## 5. Cloud Storage
- Save player progress, settings, and inventory as JSON objects.
- Use the collection / key / userId pattern:
  - "progress" / "level_data" / playerId
  - "settings" / "preferences" / playerId
- Auto-save on key events: match end, level complete, settings change.
- Load on launch and merge with local state.

## 6. AI Voice & Host
Initialize IVXAIClient with the AI backend URL and API key.

### Voice Personas — Conversational AI characters
1. Call getPersonas() to list available AI personalities.
2. Display a persona selection grid (name, avatar, short bio).
3. On selection, call startVoiceSession(personaId, userId, language).
4. Build a chat UI with scrollable message bubbles.
5. Send player text via sendText(sessionId, text).
6. Poll for AI responses via pollMessages(sessionId, lastTimestamp).
7. Display AI responses as new chat bubbles.
8. On exit, call endVoiceSession(sessionId).

### AI Host — Real-time game commentary
1. When a match starts, call startHostSession(matchId, playerProfile).
2. During gameplay, send events: sendHostEvent(sessionId, "goal_scored", eventDataJson).
3. Display host commentary in a floating overlay at the top of the game screen.
4. End the host session when the match concludes.

### Entitlements — Premium AI access
1. Call checkEntitlement(userId) to see what the player has access to.
2. Lock premium personas behind the entitlement gate.
3. Show an upgrade prompt for locked personas.

## 7. Multiplayer & Game Modes

### Mode Selection
Use IVXGameModeManager to present a mode selector screen:
- **Solo** — Single player with optional AI bots.
- **Local Multiplayer** — Same-device play (hot-seat or split-screen).
- **Online Versus** — PvP via Nakama matchmaking.
- **Online Co-op** — Team-based cooperative play.
- **Ranked** — ELO-based competitive matchmaking.
- **Turn-Based** — Asynchronous multiplayer.

Call selectMode(mode) and display a player roster that shows slots, ready states, and teams.

### Lobby System
Use IVXLobbyManager:
1. Browse rooms: listRooms(filter) — show room name, player count, map, mode.
2. Create a room: createRoom(name, config) — name, max players, password, map selection.
3. Join a room: joinRoom(roomId, password) — navigate to the room screen.
4. Ready up: setReady(true) — when all players are ready, the host starts the match.
5. Leave room: leaveRoom().

### Matchmaking
Use IVXMatchmakingManager:
1. Quick match: findMatch(config) — auto-matched by skill level.
2. Ranked match: uses the player's ELO rating for fair pairing.
3. Show a "Searching…" animation with elapsed time.
4. On match found, display opponent info and transition to gameplay.
5. Allow cancel: cancelSearch().

### Local Multiplayer
Use IVXLocalMultiplayerManager:
1. Start a local session with the desired player count (2–4).
2. For hot-seat: cycle turns with endTurn() / jumpToPlayer(index).
3. For split-screen: calculate viewport rects with calculateSplitScreenRects(count).
4. Show a turn indicator or split-screen divider UI.

## 8. Hiro Live-Ops Systems
Initialize IVXHiroSystems with the Nakama client and session.

### Spin Wheel
- Fetch wheel state: hiro.spinWheel.get()
- Execute spin: hiro.spinWheel.spin()
- Build an animated wheel UI with labeled prize segments.
- Show spins remaining and cooldown timer.

### Daily Streaks
- Fetch streak state: hiro.streaks.get()
- Record daily login: hiro.streaks.update(streakId)
- Claim milestone rewards: hiro.streaks.claimMilestone(streakId, day)
- Display a 7-day reward calendar: claimed (check), today (glow), locked (grey).
- Show a fire icon with the current streak count.

### Offerwall
- List available offers: hiro.offerwall.get()
- Mark an offer as complete: hiro.offerwall.complete(offerId)
- Claim all pending rewards: hiro.offerwall.claimPending()
- Display offer cards with: icon, title, reward amount, progress bar, claim button.

### Friend Quests
- List active cooperative quests: hiro.friendQuests.getActive()
- Contribute progress: hiro.friendQuests.contribute(questId, progressDelta)
- Show quest cards with combined progress bar.

### Friend Battles
- Challenge a friend: hiro.friendBattles.challenge(friendId, score)
- List active battles: hiro.friendBattles.getActive()
- Show challenge cards with scores and timer.

### IAP Triggers
- After key events (level_complete, out_of_lives, boss_defeated), call:
  hiro.iapTrigger.check(eventType)
- If triggered, display a contextual purchase offer.

### Smart Ad Timer
- Before showing an ad, call: hiro.smartAdTimer.canShowAd(placement)
- Only show the ad if the response says it is optimal timing.
- Reduces ad fatigue and improves eCPM.

### Retention
- On each session start: hiro.retention.get() then hiro.retention.update()
- Use the returned data to personalize welcome-back messages.

## 9. Analytics (Satori)
- Identify the player on session start: satori.identify(userId, properties).
- Track custom events: satori.track("match_completed", { score, duration, mode }).
- Use live experiments for A/B testing UI variations.
- Use feature flags to gate new features.
- Segment players by behavior (whale, casual, new, churning).

## 10. Platform Utilities
- **Deep Links:** Register a URL scheme handler. On incoming link, parse the action (invite, reward_claim, match_join) and route accordingly.
- **Safe Area:** Apply safe area insets to all UI panels (notch, home indicator, rounded corners).
- **Foldable Devices:** Detect fold state changes and adapt layout (single-screen vs table-top vs book mode).
- **Performance Optimizer:** On startup, detect device tier (low/mid/high) and auto-set quality: texture resolution, shadow quality, particle count, LOD bias.

## Implementation Guidelines
- All async operations return Promises / Futures / Tasks / Coroutines. Always handle errors with try-catch or error callbacks.
- Use event listeners / delegates / signals to decouple UI from backend logic.
- Cache server responses locally to minimize round-trips (profile, wallet, leaderboard).
- Implement exponential-backoff retry for transient network failures.
- Never hardcode secrets in client code. Store server keys in config files excluded from version control.
- Log errors with the pattern: [ClassName] message — for easy filtering.

## Recommended File Structure
Create these integration files in your project:

- **GameBootstrap** — SDK init, auth flow, session restore
- **ProfileManager** — Player profile CRUD, avatar management
- **EconomyManager** — Wallet display, currency grants, store UI
- **LeaderboardManager** — Score submission, ranking display
- **StorageManager** — Save/load progress, auto-save triggers
- **AIManager** — Voice persona chat UI, host commentary overlay (optional)
- **MultiplayerManager** — Mode selector, lobby browser, matchmaking UI (optional)
- **LiveOpsManager** — Spin wheel, streaks, offerwall, friend quests (optional)
- **AnalyticsManager** — Event tracking, experiment evaluation (optional)
- **PlatformManager** — Deep links, safe area, device adaptation (optional)
````

---

## How to Use

### Step 1: Copy the Prompt

Select and copy the entire prompt block above (everything between the ```` markers).

### Step 2: Customize the Placeholders

Replace these bracketed values with your actual configuration:

| Placeholder | Description | Example |
|------------|-------------|---------|
| `[YOUR_NAKAMA_URL]` | Your Nakama server URL | `https://nakama.mygame.com` |
| `[YOUR_SERVER_KEY]` | Your Nakama server key | `defaultkey` |
| `[YOUR_AI_URL]` | IntelliVerseX AI backend URL | `https://ai.intelliversex.com` |
| `[YOUR_AI_KEY]` | Your AI API key | `ivx_ai_xxxxxxxxxxxx` |

### Step 3: Add Your Project Context

Before pasting the prompt, tell the AI assistant about your game:

```text
My project:
- Engine: [Unity 6 / Unreal 5.4 / Godot 4.3 / etc.]
- Language: [C# / C++ / GDScript / TypeScript / etc.]
- Game type: [casual puzzle / competitive FPS / RPG / etc.]
- Current state: [new project / existing project with auth already done / etc.]
- Features I want: [all / only sections 1-5 and 8 / etc.]
```

### Step 4: Iterate

The AI will generate integration code for your specific platform. Review it, test it, and ask follow-up questions for any section you want to refine.

---

## Platform Installation Quick Reference

### Unity (UPM)
```json
"com.intelliversex.sdk": "https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK#v5.5.0"
```
All managers are `MonoBehaviour` singletons with `DontDestroyOnLoad`. Demo UIs included.

### Unreal Engine 5
Copy `SDKs/unreal/` into your project's `Plugins/` folder. All types are Blueprint-callable via `UFUNCTION`/`USTRUCT`/`UENUM` macros.

### Godot 4
Copy `addons/intelliversex/` to your project. Enable in Project → Project Settings → Plugins. Add `IVXManager` as an Autoload.

### JavaScript / TypeScript
```bash
npm install @intelliversex/sdk
```
```typescript
import { IVXManager, IVXAIClient, IVXGameModes, IVXHiroSystems } from '@intelliversex/sdk';
```

### Web3 (TypeScript + ethers.js)
```bash
npm install @intelliversex/sdk-web3
```
Extends the base JS SDK with wallet connect, signature auth, NFT gating, and token queries.

### Java / Android
```groovy
implementation 'ai.intelli-verse-x:sdk:5.5.0'
```

### Flutter / Dart
```yaml
dependencies:
  intelliversex_sdk: ^5.5.0
```

### C++ (CMake)
```cmake
FetchContent_Declare(intelliversex
  GIT_REPOSITORY https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git
  GIT_TAG v5.5.0
  SOURCE_SUBDIR SDKs/cpp)
FetchContent_MakeAvailable(intelliversex)
```

### Cocos2d-x
Add `Classes/IntelliVerseX/` from `SDKs/cocos2dx/` to your `CMakeLists.txt`.

### Defold
Add as a library dependency in `game.project`:
```
https://github.com/intelli-verse-x/Intelli-verse-X-SDK/archive/v5.5.0.zip
```

---

## Feature Parity Matrix

| Feature | Unity | Unreal | Godot | Defold | Cocos | JS/TS | C++ | Java | Flutter | Web3 |
|---------|:-----:|:------:|:-----:|:------:|:-----:|:-----:|:---:|:----:|:-------:|:----:|
| Auth & Identity | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Player Profile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Wallet & Economy | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Leaderboards | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cloud Storage | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| AI Voice & Host | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Multiplayer & Modes | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Hiro Live-Ops | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Satori Analytics | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Platform Utilities | ✅ | ✅ | — | — | — | — | — | ✅ | ✅ | — |
| Demo UIs | ✅ | — | — | — | — | — | — | — | — | — |
| Web3 / NFT | — | — | — | — | — | — | — | — | — | ✅ |

---

## Minimal Integration Example (Any Platform)

If you only want the absolute basics (auth + profile + leaderboard), use this trimmed prompt:

````text
Integrate IntelliVerseX SDK v5.5.0 basics into my game:

Server: [YOUR_NAKAMA_URL], Key: [YOUR_SERVER_KEY]

1. Initialize SDK on app start, authenticate with device ID
2. Fetch and display player profile (username, avatar)
3. After each match, submit score to leaderboard "main"
4. Display top 20 global leaderboard with player's rank highlighted
5. Save player progress to cloud storage on level complete
6. Load progress on launch
````

---

*IntelliVerseX SDK v5.5.0 — One SDK, every platform, every feature.*
