# IntelliVerseX SDK — Master Integration Prompt

> **Copy this entire prompt into any AI assistant (Cursor, ChatGPT, Claude, etc.) alongside your game project to integrate ALL IntelliVerseX SDK features in one shot.**

---

## Prompt

You are integrating the **IntelliVerseX SDK v5.7.0** into my game. The SDK provides a complete backend-as-a-service with AI, social, monetization, and engagement features. Follow every step below precisely.

---

### Step 0 — Prerequisites

```
Unity 2021.3+ or Unity 6
TextMeshPro (included in Unity)
Newtonsoft JSON (com.unity.nuget.newtonsoft-json)
Optional: Nakama SDK (com.heroiclabs.nakama-unity) for backend features
Optional: Discord Social SDK (com.discord.social-sdk) for Discord features
```

Install the IntelliVerseX UPM package:

```
Window > Package Manager > + > Add package from git URL:
https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK
```

---

### Step 1 — One-Drop Bootstrap (Recommended)

Create a single config asset, attach the bootstrap, and everything initializes automatically.

```csharp
// 1. Create config assets via Unity menu:
//    Assets > Create > IntelliVerseX > Bootstrap Config
//    Assets > Create > IntelliVerseX > AI > Configuration
//    Assets > Create > IntelliVerseX > Discord Config

// 2. Attach IVXBootstrap to a GameObject in your first scene.
//    Assign the Bootstrap Config in the Inspector.
//    Enable "Auto Initialize" (default).

// 3. That's it. On Start(), the bootstrap will:
//    - Authenticate via device ID (Nakama)
//    - Initialize all 33 Hiro live-ops systems
//    - Initialize Satori analytics
//    - Initialize all 7 AI subsystems
//    - Initialize all 11 Discord subsystems
//    - Initialize multiplayer game modes
//    - Apply platform optimizations

// 4. Listen for completion:
using IntelliVerseX.Bootstrap;

public class MyGame : MonoBehaviour
{
    void Start()
    {
        IVXBootstrap.Instance.OnBootstrapComplete += success =>
        {
            if (success)
                Debug.Log($"SDK ready! User: {IVXBootstrap.Instance.UserId}");
            else
                Debug.LogWarning("SDK initialized in offline mode.");
        };

        IVXBootstrap.Instance.OnModuleFailed += (module, error) =>
            Debug.LogError($"Module {module} failed: {error}");
    }
}
```

**Or generate all prefabs:** `IntelliVerseX > Generate All Prefabs` — creates `IVX_Bootstrap.prefab` and `IVX_AllManagers.prefab`.

---

### Step 2 — Manual Bootstrap (Advanced)

If you need fine-grained control over initialization order:

```csharp
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.AI;
using IntelliVerseX.Discord;
using IntelliVerseX.Hiro;
using IntelliVerseX.Satori;
using IntelliVerseX.GameModes;

public class ManualBootstrap : MonoBehaviour
{
    [SerializeField] private IVXAIConfig _aiConfig;
    [SerializeField] private IVXDiscordConfig _discordConfig;

    private async void Start()
    {
        // Phase 1: Backend auth
        string userId, userName, authToken;
        #if INTELLIVERSEX_HAS_NAKAMA
        var client = new Nakama.Client("http", "your-server.com", 7350, "your-key");
        var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        userId = session.UserId;
        userName = session.Username;
        authToken = session.AuthToken;

        // Phase 2: Hiro (33 live-ops systems)
        IVXHiroCoordinator.Instance.InitializeSystems(client, session);

        // Phase 3: Satori analytics
        IVXSatoriClient.Instance.Initialize(client, session);
        #else
        userId = "offline-player";
        userName = "Player";
        authToken = null;
        #endif

        // Phase 4: Discord
        IVXDiscordManager.Instance.Initialize(_discordConfig);

        // Phase 5: AI (7 subsystems)
        IVXAISessionManager.Instance.Initialize(userId, userName, authToken);
        IVXAINPCDialogManager.Instance.Initialize(_aiConfig);
        IVXAINPCDialogManager.Instance.SetAuthToken(authToken);
        IVXAIAssistant.Instance.Initialize(_aiConfig);
        IVXAIAssistant.Instance.SetAuthToken(authToken);
        IVXAIModerator.Instance.Initialize(_aiConfig);
        IVXAIContentGenerator.Instance.Initialize(_aiConfig);
        IVXAIProfiler.Instance.Initialize(_aiConfig, userId);
        IVXAIVoiceServices.Instance.Initialize(_aiConfig);

        // Phase 6: Multiplayer (auto-init)
        var _ = IVXGameModeManager.Instance;

        Debug.Log("All systems go!");
    }
}
```

---

### Step 3 — Identity & Authentication

```csharp
// Device auth (automatic via Bootstrap)
// Email auth:
#if INTELLIVERSEX_HAS_NAKAMA
var session = await client.AuthenticateEmailAsync("user@email.com", "password");
IVXHiroCoordinator.Instance.RefreshSession(session);
#endif

// Discord account linking:
IVXDiscordManager.Instance.LinkAccount(success =>
{
    if (success) Debug.Log($"Discord linked: {IVXDiscordManager.Instance.DiscordUserId}");
});

// Session persistence (built into Bootstrap):
// Saved to PlayerPrefs automatically when PersistSession = true
```

---

### Step 4 — Hiro Live-Ops (33 Systems)

After Hiro initialization, access any system:

```csharp
var hiro = IVXHiroCoordinator.Instance;

// Economy & Wallet
hiro.Economy.GrantCurrency("coins", 500);
hiro.Economy.GetWallet(wallet => Debug.Log($"Coins: {wallet.Coins}"));

// Leaderboards
hiro.Leaderboards.SubmitScore("global", 12500);
hiro.Leaderboards.GetTopN("global", 10, entries => { /* display */ });

// Streaks & Daily Rewards
hiro.Streaks.ClaimDaily(reward => Debug.Log($"Day {reward.Day}: {reward.Item}"));

// Spin Wheel
hiro.SpinWheel.Spin(prize => Debug.Log($"Won: {prize.Name}"));

// Achievements
hiro.Achievements.Unlock("first_kill");

// Energy / Stamina
hiro.Energy.Spend(1);
if (hiro.Energy.Current <= 0) ShowRefillDialog();

// Store / IAP
hiro.Store.GetCatalog(items => { /* display */ });
hiro.Store.Purchase("item_id", receipt => { /* validate */ });

// Offerwall
hiro.Offerwall.GetOffers(offers => { /* display */ });

// Teams / Guilds
hiro.Teams.CreateTeam("MyGuild");

// Mailbox
hiro.Mailbox.GetMessages(messages => { /* display */ });

// Retention
hiro.Retention.TrackSession();
hiro.StreakShield.ActivateShield();
hiro.SessionBoosters.ActivateBoost("xp_2x", 3600);

// Monetization
hiro.IAPTriggers.CheckTrigger("level_up", shouldShow => { if (shouldShow) ShowIAPOffer(); });
hiro.SmartAdTimer.RequestAd(adReady => { if (adReady) ShowAd(); });

// Social
hiro.FriendQuests.StartQuest("collect_100_gems");
hiro.FriendBattles.Challenge("friend_user_id");
```

---

### Step 5 — Satori Analytics

```csharp
var satori = IVXSatoriClient.Instance;

// Track custom events
satori.TrackEvent("level_complete", new Dictionary<string, string>
{
    { "level", "5" }, { "score", "12500" }, { "time_seconds", "45" }
});

// Feature flags
var flags = await satori.GetFeatureFlags();
if (flags.ContainsKey("new_ui") && flags["new_ui"] == "true") EnableNewUI();

// A/B experiments
var variant = await satori.GetExperiment("onboarding_flow");
LoadOnboardingVariant(variant);

// Player segmentation
satori.IdentifyProperties(new Dictionary<string, string>
{
    { "platform", Application.platform.ToString() },
    { "install_date", DateTime.UtcNow.ToString("O") }
});
```

---

### Step 6 — AI Conversational & LLM Stack

#### 6a. NPC Dialog System

```csharp
var npc = IVXAINPCDialogManager.Instance;

// Register an NPC
npc.RegisterNPC(new IVXAINPCProfile
{
    NpcId = "blacksmith",
    DisplayName = "Gorrak the Smith",
    Persona = "Gruff but kind dwarf blacksmith. Expert in rare metals.",
    KnowledgeBase = "weapons, armor, rare materials, forging techniques"
});

// Start a dialog session
npc.StartDialog("blacksmith", session =>
{
    Debug.Log($"Dialog started: {session.SessionId}");
});

// Send a message
npc.SendMessage("blacksmith", "Can you forge a dragon-slaying sword?", response =>
{
    Debug.Log($"NPC says: {response.Content}");
    if (response.Actions != null)
        foreach (var action in response.Actions)
            HandleNPCAction(action); // e.g. open shop, give quest
});
```

#### 6b. In-Game Assistant

```csharp
var assistant = IVXAIAssistant.Instance;

// Ask a question
assistant.Ask("How do I defeat the fire boss?", answer =>
    ShowHelpPopup(answer));

// Get contextual hints
assistant.GetHint("level_5_puzzle", hint =>
    ShowHintBubble(hint));

// Tutorial generation
assistant.GetTutorial("crafting_system", steps =>
    StartTutorialSequence(steps));

// Knowledge base search
assistant.SearchKnowledgeBase("enchantment recipes", results =>
    DisplaySearchResults(results));
```

#### 6c. Content Moderation

```csharp
var mod = IVXAIModerator.Instance;

// Classify a chat message
mod.ClassifyText(playerMessage, result =>
{
    if (result.IsSafe)
        BroadcastChat(playerMessage);
    else
        ShowWarning($"Blocked: {result.Category}");
});

// Filter with custom rules
mod.AddCustomRule(new IVXModerationRule
{
    Pattern = "cheat|hack|exploit",
    Action = IVXModerationActionType.Block,
    Reason = "Prohibited content"
});

mod.FilterText(playerMessage, filtered =>
    BroadcastChat(filtered));
```

#### 6d. AI Content Generation

```csharp
var gen = IVXAIContentGenerator.Instance;

// Generate a quest
gen.GenerateQuest(new IVXQuestTemplate
{
    Theme = "dragon_slaying",
    Difficulty = "hard",
    RewardTier = 3
}, quest => StartQuest(quest));

// Generate dialog
gen.GenerateDialogue("friendly_merchant", "player_buying_potion",
    lines => PlayDialogue(lines));

// Generate items
gen.GenerateItem("legendary_weapon", "fire_element",
    item => AddToInventory(item));
```

#### 6e. Player Behavior Profiling

```csharp
var profiler = IVXAIProfiler.Instance;

// Track events
profiler.TrackEvent("purchase", new Dictionary<string, object>
{
    { "item", "gem_pack_100" }, { "price", 4.99 }
});

// Get profile & predictions
profiler.GetPlayerProfile(profile =>
    Debug.Log($"Sessions: {profile.TotalSessions}, Cohort: {profile.Cohort}"));

profiler.PredictChurn((risk, factors) =>
{
    if (risk > 0.7f) SendRetentionOffer();
});

profiler.GetPersonalizationHints(hints =>
{
    foreach (var hint in hints)
        ApplyPersonalization(hint);
});

// Auto-tracking (session events, periodic flush)
profiler.StartAutoTracking();
```

#### 6f. Voice AI Services

```csharp
var voice = IVXAIVoiceServices.Instance;

// Text-to-Speech
voice.SynthesizeSpeech("Welcome, brave adventurer!", null, audioBytes =>
    PlayAudio(audioBytes));

// Speech-to-Text
voice.TranscribeAudio(micPcmData, 16000, result =>
    ProcessPlayerSpeech(result.Text));

// List available voices
voice.ListVoices(voices =>
    PopulateVoiceSelector(voices));

// Language detection
voice.DetectLanguage(audioPcmData, 16000, (lang, confidence) =>
    SetGameLanguage(lang));

// Streaming transcription
voice.StartStreamingTranscription();
// ... feed audio chunks ...
voice.StopStreamingTranscription();
```

#### 6g. AI Voice Personas & Host

```csharp
var ai = IVXAISessionManager.Instance;

// Set player context for personalized AI
ai.SetPlayerContext(new IVXAIPlayerContext
{
    Level = 42, Score = 15000, Difficulty = "hard"
});

// Start voice persona chat
ai.CreateVoiceSession("persona_id", session =>
{
    ai.StartVoiceStreaming(session.Id);
    ai.OnResponseReceived += msg => DisplayMessage(msg);
    ai.OnAudioReady += clip => audioSource.PlayOneShot(clip);
});

// AI Host commentary
ai.CreateHostSession("sports_commentator", session =>
{
    ai.OnResponseReceived += msg => ShowCommentary(msg);
});
```

---

### Step 7 — Discord Social SDK (11 Subsystems)

```csharp
// Account Linking (multiple flows)
IVXDiscordManager.Instance.LinkAccount(ok => Debug.Log($"Linked: {ok}"));
IVXDiscordManager.Instance.StartMobileOAuth2Flow(); // Mobile PKCE
IVXDiscordManager.Instance.StartConsoleOAuth2Flow(); // Console device code

// Rich Presence
var presence = IVXDiscordPresence.Instance;
presence.SetActivity("In Battle", "Fighting the Dragon Boss");
presence.SetParty("party_123", 3, 4);
presence.SetTimerFromNow(); // Elapsed time
presence.AddButton("Join Game", "https://yourgame.com/join");
presence.SetSupportedPlatforms(IVXActivityPlatforms.Desktop | IVXActivityPlatforms.Mobile);

// Friends & Relationships
var friends = IVXDiscordFriends.Instance;
friends.Refresh(); // Fetches unified game + Discord friends
friends.OnFriendsUpdated += list =>
    foreach (var f in list) Debug.Log($"{f.DisplayName} ({f.Source})");
friends.SendGameFriendRequest("player_123");
friends.BlockUser(discordUserId);

// Direct Messages
var dms = IVXDiscordMessages.Instance;
dms.SendDM(recipientDiscordId, "GG! Want to rematch?");
dms.GetDMHistory(recipientDiscordId, 20, messages =>
    foreach (var m in messages) DisplayMessage(m));
dms.GetDMSummaries(summaries => UpdateInbox(summaries));

// Lobbies
var lobby = IVXDiscordLobby.Instance;
lobby.CreateOrJoinLobby("ranked_match", 8, lobbyId =>
{
    lobby.SendMessage("I'm ready!");
    lobby.OnChatReceived += (sender, msg) => ShowChat(sender, msg);
});
lobby.CreateOrJoinLobbyWithMetadata("ranked", 8,
    new Dictionary<string, string> { { "map", "arena" } });
lobby.SetLobbyIdleTimeout(300f);

// Voice Chat
var voice = IVXDiscordVoice.Instance;
voice.JoinCall(lobby.CurrentLobbyId, () => Debug.Log("In voice!"));
voice.SetSelfMute(true);
voice.SetVADThreshold(0.3f);

// Game Invites
IVXDiscordInvites.Instance.SendInvite("friend_id", "Join my match!");
IVXDiscordInvites.Instance.OnInviteReceived += invite => ShowInviteUI(invite);

// Linked Channels
IVXDiscordLinkedChannels.Instance.SendToLinkedChannel("Boss defeated!");

// Moderation
var mod = IVXDiscordModeration.Instance;
mod.EnableAutoModeration(true);
mod.ReportUser(discordUserId, "Toxic behavior");

// Debug
IVXDiscordDebug.Instance.SetLogLevel(IVXDiscordLogLevel.Warning);
```

---

### Step 8 — Multiplayer & Game Modes

```csharp
var modes = IVXGameModeManager.Instance;

// Set game mode
modes.SetGameMode(IVXGameMode.OnlineVersus);
modes.SetMaxPlayers(4);

// Lobby system
var lobby = IVXLobbyManager.Instance;
lobby.GetRooms(rooms => DisplayRoomList(rooms));
lobby.CreateRoom("My Room", 4, room => Debug.Log($"Room: {room.RoomId}"));
lobby.JoinRoom("room_id");

// Matchmaking
var mm = IVXMatchmakingManager.Instance;
mm.StartMatchmaking(match => LoadMatch(match));
mm.OnMatchFound += match => ShowMatchFoundUI(match);

// Local Multiplayer
var local = IVXLocalMultiplayerManager.Instance;
local.SetLocalPlayers(2);
local.RegisterPlayer(0, "Player 1");
local.RegisterPlayer(1, "Player 2");
```

---

### Step 9 — Error Handling Pattern

Every SDK call should follow this pattern:

```csharp
try
{
    var manager = IVXSomeManager.Instance;
    if (manager == null || !manager.IsInitialized)
    {
        Debug.LogWarning("Manager not ready — use IVXBootstrap or call Initialize()");
        return;
    }

    manager.DoSomething(result =>
    {
        if (result != null)
            HandleSuccess(result);
        else
            HandleFallback();
    });
}
catch (Exception e)
{
    Debug.LogError($"SDK error: {e.Message}");
    ShowOfflineFallback();
}
```

---

### Step 10 — Demo Hub

Drop the `IVX_DemoHub` prefab (or `IVXDemoHub` component) into any scene to access all **16 interactive demos**:

| # | Demo | Features Shown |
|---|------|---------------|
| 1 | Discord Social | All 11 Discord subsystems |
| 2 | AI Voice Chat | Voice persona conversations |
| 3 | AI Host | Live game commentary |
| 4 | AI NPC Dialog | Branching NPC conversations |
| 5 | AI Assistant | Contextual help & tutorials |
| 6 | AI Moderation | Content classification & filtering |
| 7 | AI Content Gen | Quest/story/item generation |
| 8 | Spin Wheel | Daily reward wheel |
| 9 | Daily Streak | Login streak rewards |
| 10 | Offerwall | Ad monetization |
| 11 | Game Modes | Solo/Local/Online selection |
| 12 | Lobby | Online lobby & matchmaking |
| 13 | Identity & Auth | Authentication & sessions |
| 14 | Leaderboard | Score submission & rankings |
| 15 | AI Profiler | Behavior tracking & predictions |
| 16 | AI Voice Services | Standalone STT/TTS |

Generate all prefabs: `IntelliVerseX > Generate All Prefabs`

---

### Step 11 — Lifecycle Management

```csharp
// On application pause (mobile background)
void OnApplicationPause(bool paused)
{
    if (paused)
        IVXAIProfiler.Instance?.FlushEvents();
}

// On application quit
void OnApplicationQuit()
{
    IVXBootstrap.Instance?.Shutdown();
    IVXAIProfiler.Instance?.StopAutoTracking();
}
```

---

### Step 12 — Cross-Platform Installation

| Platform | Install Method |
|----------|---------------|
| **Unity** | UPM git URL (see Step 0) |
| **Unreal** | Copy `SDKs/unreal/` to `Plugins/IntelliVerseX/` |
| **Godot** | Copy `SDKs/godot/addons/intelliversex/` to `addons/` |
| **Defold** | Add `SDKs/defold/` as library dependency |
| **Cocos2d-x** | Add `SDKs/cocos2dx/Classes/IntelliVerseX/` to CMake |
| **JavaScript/TypeScript** | `npm install @intelliversex/sdk` |
| **Java/Android** | Add `SDKs/java/` as Gradle module |
| **Flutter** | `flutter pub add intelliversex_sdk` |
| **C++** | Add `SDKs/cpp/include/` to include path |
| **Web3** | `npm install @intelliversex/web3-sdk` |

---

### Step 12b — Cross-Platform Initialization Examples

For non-Unity platforms, initialization follows a similar pattern — create a client, configure, initialize modules:

#### JavaScript / TypeScript

```typescript
import {
  IVXClient, IVXAIAssistant, IVXAINPCDialogManager,
  IVXAIModerator, IVXAIContentGenerator, IVXAIProfiler,
  IVXAIVoiceServices, IVXDiscordMessages, IVXDiscordModeration
} from '@intelliversex/sdk';

// 1. Create and configure the client
const client = new IVXClient({
  apiBase: 'https://api.intelli-verse-x.ai',
  nakamaHost: 'your-server.com',
  nakamaPort: 7350,
  nakamaKey: 'your-key'
});

// 2. Initialize (authenticates, connects to backend)
await client.initialize();
const userId = client.userId;

// 3. Use any feature
const assistant = new IVXAIAssistant(client);
await assistant.initialize({ apiKey: 'YOUR_AI_KEY' });
const answer = await assistant.ask('How do I craft a sword?');

const npc = new IVXAINPCDialogManager(client);
await npc.initialize({ apiKey: 'YOUR_AI_KEY' });
npc.registerNPC({ npcId: 'blacksmith', persona: 'Gruff dwarf blacksmith' });
const response = await npc.sendMessage('blacksmith', 'Got any rare metals?');

const profiler = new IVXAIProfiler(client);
await profiler.initialize({ apiKey: 'YOUR_AI_KEY', playerId: userId });
profiler.trackEvent('purchase', { item: 'gem_pack', price: 4.99 });

const dms = new IVXDiscordMessages(client);
await dms.sendDM(recipientId, 'GG! Rematch?');
```

#### Flutter / Dart

```dart
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

// 1. Create client
final client = IVXClient(
  apiBase: 'https://api.intelli-verse-x.ai',
  nakamaHost: 'your-server.com',
  nakamaPort: 7350,
);

// 2. Initialize
await client.initialize();

// 3. AI Assistant
final assistant = IVXAIAssistant(client);
await assistant.initialize(apiKey: 'YOUR_AI_KEY');
final answer = await assistant.ask('How do I defeat the boss?');

// 4. NPC Dialog
final npc = IVXAINPCDialogManager(client);
await npc.initialize(apiKey: 'YOUR_AI_KEY');
npc.registerNPC(IVXNPCProfile(npcId: 'guard', persona: 'Stern city guard'));
final response = await npc.sendMessage('guard', 'Can I pass?');

// 5. Content Moderation
final mod = IVXAIModerator(client);
await mod.initialize(apiKey: 'YOUR_AI_KEY');
final result = await mod.classifyText(playerMessage);
if (result.isSafe) broadcastChat(playerMessage);

// 6. Discord Messages
final dms = IVXDiscordMessages(client);
await dms.sendDM(recipientId, 'Want to team up?');
```

#### Java / Android

```java
// 1. Create client
IVXClient client = IVXClient.builder()
    .apiBase("https://api.intelli-verse-x.ai")
    .nakamaHost("your-server.com")
    .nakamaPort(7350)
    .nakamaKey("your-key")
    .build();

// 2. Initialize
client.initialize().thenAccept(v -> {
    String userId = client.getUserId();

    // 3. AI features
    IVXAIAssistant assistant = IVXAIAssistant.getInstance();
    assistant.initialize("YOUR_AI_KEY");
    assistant.ask("How do I craft a sword?", answer -> {
        Log.d("IVX", "Answer: " + answer);
    });

    // 4. NPC Dialog
    IVXAINPCDialogManager npc = IVXAINPCDialogManager.getInstance();
    npc.initialize("YOUR_AI_KEY");
    npc.registerNPC("blacksmith", "Gruff dwarf blacksmith");
    npc.sendMessage("blacksmith", "Got any rare metals?", response -> {
        Log.d("IVX", "NPC: " + response.getContent());
    });

    // 5. Profiler
    IVXAIProfiler profiler = IVXAIProfiler.getInstance();
    profiler.initialize("YOUR_AI_KEY", userId);
    profiler.trackEvent("purchase", Map.of("item", "gem_pack", "price", "4.99"));

    // 6. Discord
    IVXDiscordMessages dms = IVXDiscordMessages.getInstance();
    dms.sendDM(recipientId, "GG! Rematch?");
});
```

#### Godot (GDScript)

```gdscript
# 1. Autoload IVXClient in project settings
var client = IVXClient.new()
client.api_base = "https://api.intelli-verse-x.ai"
client.nakama_host = "your-server.com"
await client.initialize()

# 2. AI Assistant
var assistant = IVXAIAssistant.new()
await assistant.initialize("YOUR_AI_KEY")
var answer = await assistant.ask("How do I defeat the boss?")
print("Answer: " + answer)

# 3. NPC Dialog
var npc = IVXAINPCDialogManager.new()
await npc.initialize("YOUR_AI_KEY")
npc.register_npc("blacksmith", "Gruff dwarf blacksmith")
var response = await npc.send_message("blacksmith", "Got any swords?")
print("NPC: " + response.content)

# 4. Profiler
var profiler = IVXAIProfiler.new()
await profiler.initialize("YOUR_AI_KEY", client.user_id)
profiler.track_event("purchase", {"item": "gem_pack", "price": 4.99})
```

---

### Feature Matrix (All 10 Platforms)

| Feature | Unity | UE5 | Godot | Defold | Cocos | JS | C++ | Java | Flutter | Web3 |
|---------|-------|-----|-------|--------|-------|-----|-----|------|---------|------|
| Bootstrap | Full | - | - | - | - | - | - | - | - | - |
| Identity | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Hiro (33 systems) | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | - |
| Satori Analytics | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | - |
| AI Voice/Host | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI NPC Dialog | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Assistant | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Moderation | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Content Gen | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Profiler | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| AI Voice Services | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Discord Social | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub |
| Multiplayer | Full | Stub | Stub | Stub | Stub | Stub | Stub | Stub | Stub | - |
| Demo UIs | Full | - | - | - | - | - | - | - | - | - |
| Platform Utils | Full | Partial | - | - | - | - | - | Partial | Partial | - |

**Full** = Production-ready implementation
**Stub** = API surface matching Unity, ready for backend wiring
**Partial** = Key features implemented
**-** = Not applicable for platform

---

### Recommended File Structure

```
Assets/
├── _IntelliVerseXSDK/          # SDK package
│   ├── Bootstrap/              # One-drop init
│   ├── AI/                     # 7 AI subsystems
│   ├── Discord/                # 11 Discord subsystems
│   ├── Hiro/                   # 33 live-ops systems
│   ├── Satori/                 # Analytics
│   ├── Multiplayer/            # Game modes
│   ├── Platform/               # Device optimizations
│   ├── Demos/                  # 16 demo scenes
│   └── Prefabs/                # Generated prefabs
├── Resources/
│   ├── IVXBootstrapConfig.asset
│   ├── IVXAIConfig.asset
│   └── IVXDiscordConfig.asset
└── Scenes/
    └── Main.scene              # IVXBootstrap attached to a GameObject
```

---

### Quick Onboarding Checklist

- [ ] Install IntelliVerseX SDK via UPM
- [ ] Create `IVXBootstrapConfig` asset (Assets > Create > IntelliVerseX > Bootstrap Config)
- [ ] Create `IVXAIConfig` asset (Assets > Create > IntelliVerseX > AI > Configuration)
- [ ] Create `IVXDiscordConfig` asset (Assets > Create > IntelliVerseX > Discord Config)
- [ ] Fill in server details in Bootstrap Config
- [ ] Fill in API key in AI Config
- [ ] Fill in Application ID in Discord Config
- [ ] Drag `IVXBootstrap` onto a GameObject in your first scene
- [ ] Assign the Bootstrap Config in the Inspector
- [ ] Press Play — all systems initialize automatically
- [ ] Open the Demo Hub to explore features: `IntelliVerseX > Generate All Prefabs`, drop `IVX_DemoHub`
- [ ] Start integrating features using the code samples above

---

*IntelliVerseX SDK v5.7.0 — 10 platforms, 70+ features, 1 prompt.*
