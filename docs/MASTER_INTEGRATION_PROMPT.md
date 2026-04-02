# IntelliVerseX SDK — Master Integration Prompt

> **Copy this entire prompt into any AI assistant (Cursor, ChatGPT, Claude, etc.) alongside your game project to integrate ALL IntelliVerseX SDK features in one shot.**

---

## Prompt

You are integrating the **IntelliVerseX SDK v5.8.0** into my game. The SDK provides a complete backend-as-a-service with AI, social, monetization, and engagement features. Follow every step below precisely.

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

### Step 0.5 — Register Your Game & Get Your Game ID

Every game needs a **Game ID** (UUID) from the IntelliVerseX platform. This ID links your game to leaderboards, wallets, analytics, ads, and all backend services.

**Option A — Dashboard (recommended):**

1. Sign in at [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers)
2. Create a new project → copy the **Game ID** from the project settings

**Option B — API (programmatic):**

```bash
# 1. Authenticate — get a bearer token
TOKEN=$(curl -s -X POST 'https://api.intelli-verse-x.ai/api/admin/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"email":"your-email@example.com","password":"your-password"}' \
  | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('accessToken',''))")

# 2. Create your game
curl -s -X POST 'https://msapi.intelli-verse-x.io/api/games/game/info' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"gameTitle": "My Awesome Game"}'
```

**Response:**

```json
{
  "status": true,
  "message": "Game created successfully",
  "data": {
    "gameId": "83e9cbd5-3883-4fec-8344-0d2d3ca35be3"
  }
}
```

Copy the `gameId` UUID — you will paste it into your config in Step 1.

| API | URL | Purpose |
|-----|-----|---------|
| **Admin Auth** | `POST https://api.intelli-verse-x.ai/api/admin/auth/login` | Get bearer token |
| **Create Game** | `POST https://msapi.intelli-verse-x.io/api/games/game/info` | Register a game, returns `gameId` |

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
        var bootstrap = IVXBootstrap.Instance;
        if (bootstrap == null)
        {
            Debug.LogWarning("IVXBootstrap not in scene — add it to your first scene.");
            return;
        }

        if (bootstrap.IsInitialized)
        {
            OnSDKReady();
            return;
        }

        bootstrap.OnBootstrapComplete += success =>
        {
            if (success)
                OnSDKReady();
            else
                Debug.LogWarning("SDK bootstrap had issues — some features may be offline.");
        };

        bootstrap.OnModuleFailed += (module, error) =>
            Debug.LogWarning($"Module {module} failed: {error}");
    }

    void OnSDKReady()
    {
        Debug.Log($"SDK ready! User: {IVXBootstrap.Instance.UserId}");
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
        string userId, userName, authToken;

        // Phase 1: Backend auth
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

        // Phase 6: Multiplayer
        IVXGameModeManager.Instance.SelectMode(IVXGameMode.Solo);

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
IVXSatoriClient.Instance.RefreshSession(session);
#endif

// Discord account linking (no callback — listen to events instead):
IVXDiscordManager.Instance.LinkAccount();

// Session persistence (built into Bootstrap):
// Saved to PlayerPrefs automatically when PersistSession = true
```

---

### Step 4 — Hiro Live-Ops (33 Systems)

All Hiro systems use **async/await** with `*Async` method names. After Hiro initialization, access any system:

```csharp
var hiro = IVXHiroCoordinator.Instance;

// Economy — Donations & Rewarded Videos
var donation = await hiro.Economy.RequestDonationAsync("daily_donation");
var giveResult = await hiro.Economy.GiveDonationAsync("target_user_id", "daily_donation", 1);
var reward = await hiro.Economy.ClaimDonationsAsync(new[] { "daily_donation" });
var videoReward = await hiro.Economy.CompleteRewardedVideoAsync();

// Leaderboards
await hiro.Leaderboards.SubmitScoreAsync("global", 12500);
var records = await hiro.Leaderboards.GetRecordsAsync("global", limit: 10);
var allBoards = await hiro.Leaderboards.ListAsync();

// Streaks
var streaks = await hiro.Streaks.GetAsync();
var updated = await hiro.Streaks.UpdateAsync("daily_login");
var milestone = await hiro.Streaks.ClaimMilestoneAsync("daily_login", 7);

// Spin Wheel
var wheelConfig = await hiro.SpinWheel.GetAsync();
var spinResult = await hiro.SpinWheel.SpinAsync("free");

// Achievements
var achievements = await hiro.Achievements.ListAsync();
var progress = await hiro.Achievements.AddProgressAsync("first_kill", 1);
var claimed = await hiro.Achievements.ClaimAsync("first_kill");

// Energy / Stamina
var energyState = await hiro.Energy.GetAsync();
bool spent = await hiro.Energy.SpendAsync("stamina", 1);
var refilled = await hiro.Energy.RefillAsync("stamina");

// Store / IAP
var catalog = await hiro.Store.ListAsync();
var purchase = await hiro.Store.PurchaseAsync("weapons_section", "sword_01");

// Offerwall
var offers = await hiro.Offerwall.GetAsync();
var completed = await hiro.Offerwall.CompleteOfferAsync("offer_1", "provider", "tx_123");

// Teams / Guilds
var teamData = await hiro.Teams.GetAsync("group_id");
await hiro.Teams.UpdateStatAsync("group_id", "wins", 1);
var teamWallet = await hiro.Teams.GetWalletAsync("group_id");

// Mailbox
var messages = await hiro.Mailbox.ListAsync();
var claimMsg = await hiro.Mailbox.ClaimAsync("msg_id");
await hiro.Mailbox.ClaimAllAsync();
await hiro.Mailbox.DeleteAsync("msg_id");

// Retention
var retentionState = await hiro.Retention.GetAsync();
var heartbeat = await hiro.Retention.HeartbeatAsync();
await hiro.Retention.CompleteOnboardingStepAsync(1);
var comebackBonus = await hiro.Retention.ClaimComebackBonusAsync();

// Streak Shield
var shieldState = await hiro.StreakShield.GetAsync();
var activated = await hiro.StreakShield.ActivateAsync();

// Session Boosters
var boosters = await hiro.SessionBoosters.GetAsync();
var boost = await hiro.SessionBoosters.ActivateAsync("xp_2x_booster");
var freeClaim = await hiro.SessionBoosters.ClaimFreeAsync();

// IAP Triggers
var trigger = await hiro.IAPTriggers.EvaluateAsync("level_up", 5);
await hiro.IAPTriggers.DismissAsync("trigger_id");
await hiro.IAPTriggers.RecordConversionAsync("trigger_id", "receipt_data");

// Smart Ad Timer
var adState = await hiro.SmartAdTimer.GetAsync();
var canShow = await hiro.SmartAdTimer.CanShowAsync("rewarded");
await hiro.SmartAdTimer.RecordImpressionAsync("rewarded", "level_complete");

// Friend Quests
var quests = await hiro.FriendQuests.GetAsync();
await hiro.FriendQuests.AcceptAsync("quest_id", "partner_user_id");
await hiro.FriendQuests.ReportProgressAsync("quest_id", 10);

// Friend Battles
var battles = await hiro.FriendBattles.GetAsync();
await hiro.FriendBattles.SendChallengeAsync("friend_user_id", "quiz_mode", 0);
await hiro.FriendBattles.AcceptChallengeAsync("challenge_id");
await hiro.FriendBattles.SubmitScoreAsync("challenge_id", 950);
```

---

### Step 5 — Satori Analytics

```csharp
var satori = IVXSatoriClient.Instance;

// Track custom events
await satori.CaptureEventAsync("level_complete", new Dictionary<string, string>
{
    { "level", "5" }, { "score", "12500" }, { "time_seconds", "45" }
});

// Feature flags
var flags = await satori.GetAllFlagsAsync();
foreach (var flag in flags)
    if (flag.name == "new_ui" && flag.enabled) EnableNewUI();

// Single flag
var singleFlag = await satori.GetFlagAsync("new_ui", "false");

// A/B experiments
var experiments = await satori.GetExperimentsAsync();
var variant = await satori.GetExperimentVariantAsync("onboarding_flow");
LoadOnboardingVariant(variant.name, variant.config);

// Player identity / segmentation
await satori.UpdateIdentityAsync(
    defaultProperties: new Dictionary<string, string>
    {
        { "platform", Application.platform.ToString() }
    },
    customProperties: new Dictionary<string, string>
    {
        { "install_date", DateTime.UtcNow.ToString("O") }
    }
);

// Audience memberships
var audiences = await satori.GetAudienceMembershipsAsync();

// Live events
var liveEvents = await satori.GetLiveEventsAsync();
foreach (var ev in liveEvents.events)
    if (ev.IsActive) await satori.JoinLiveEventAsync(ev.id);

// Messages
var satoriMessages = await satori.GetMessagesAsync();
```

---

### Step 6 — AI Conversational & LLM Stack

> **Full guide**: [AI Getting Started](guides/ai-getting-started.md)

#### 6.0 AI Backend Setup

Create an `IVXAIConfig` asset (**Assets → Create → IntelliVerseX → AI → Configuration**):

| Field | Example | Notes |
|-------|---------|-------|
| **Provider** | `IntelliVerseX` / `OpenAI` / `AzureOpenAI` / `Anthropic` / `Custom` | `Custom` = any OpenAI-compatible endpoint (Ollama, vLLM, LiteLLM) |
| **API Base URL** | `https://api.intelli-verse-x.ai/api/ai` | Or `http://localhost:11434/v1` for local Ollama |
| **API Key** | `sk-...` | Leave empty for bearer-token auth; for production, inject at runtime via `config.SetApiKey(key)` |
| **Model Name** | `gpt-4o`, `llama3:8b`, empty | Empty = server default |
| **Mock Mode** | ✅ during dev | Returns canned responses, zero API calls, zero cost |
| **Max Retries** | `2` | Retries on 5xx with exponential backoff |

Initialize all managers after bootstrap completes, passing the auth token:

```csharp
IVXBootstrap.Instance.OnBootstrapComplete += () =>
{
    string token = IVXBootstrap.Instance.AuthToken;
    string userId = IVXBootstrap.Instance.UserId;

    IVXAINPCDialogManager.Instance.Initialize(aiConfig);
    IVXAINPCDialogManager.Instance.SetAuthToken(token);

    IVXAIAssistant.Instance.Initialize(aiConfig);
    IVXAIAssistant.Instance.SetAuthToken(token);

    IVXAIModerator.Instance.Initialize(aiConfig);
    IVXAIModerator.Instance.SetAuthToken(token);

    IVXAIContentGenerator.Instance.Initialize(aiConfig);
    IVXAIContentGenerator.Instance.SetAuthToken(token);

    IVXAIProfiler.Instance.Initialize(aiConfig, userId);
    IVXAIProfiler.Instance.SetAuthToken(token);
    IVXAIProfiler.Instance.StartAutoTracking();

    IVXAIVoiceServices.Instance.Initialize(aiConfig);
    IVXAIVoiceServices.Instance.SetAuthToken(token);
};
```

#### 6a. NPC Dialog System

```csharp
var npc = IVXAINPCDialogManager.Instance;

// Register an NPC
npc.RegisterNPC(new IVXAINPCProfile
{
    NpcId = "blacksmith",
    DisplayName = "Gorrak the Smith",
    PersonaPrompt = "Gruff but kind dwarf blacksmith. Expert in rare metals.",
    KnowledgeBaseIds = new[] { "weapons", "armor", "rare_materials" },
    VoiceId = "deep_male_01",
    MaxTurns = 20,
    AvailableActions = new[] { "open_shop", "give_quest", "repair_item" }
});

// Listen for NPC responses and actions
npc.OnNPCResponse += (sessionId, text) =>
    ShowDialogBubble(text);

npc.OnNPCAction += (sessionId, action) =>
    HandleNPCAction(action);  // e.g. open shop, give quest

// Start a dialog session (playerId is required)
string playerId = IVXBootstrap.Instance?.UserId ?? "local_player";
npc.StartDialog("blacksmith", playerId, "Player is level 42 warrior", session =>
{
    Debug.Log($"Dialog started: {session.SessionId}");

    // Send a message (keyed by sessionId, not npcId)
    npc.SendMessage(session.SessionId, "Can you forge a dragon-slaying sword?", response =>
    {
        Debug.Log($"NPC says: {response}");
    });
});
```

#### 6b. In-Game Assistant

```csharp
var assistant = IVXAIAssistant.Instance;

// Ask a question (optional game context)
assistant.Ask("How do I defeat the fire boss?", null, response =>
    ShowHelpPopup(response.Response));

// Get contextual hints (requires levelId + objectiveId)
assistant.GetHint("level_5", "defeat_boss", null, hint =>
    ShowHintBubble(hint.Hint));

// Tutorial generation (by featureId)
assistant.GetTutorial("crafting_system", tutorial =>
    StartTutorialSequence(tutorial.Steps));

// Knowledge base search (returns string[])
assistant.SearchKnowledgeBase("enchantment recipes", results =>
{
    foreach (var r in results) DisplaySearchResult(r);
});
```

#### 6c. Content Moderation

```csharp
var mod = IVXAIModerator.Instance;

// Classify a chat message
mod.ClassifyText(playerMessage, result =>
{
    // result has: Category, Severity, Confidence, SuggestedAction, Replacement
    if (result.SuggestedAction == IVXModerationActionType.Allow)
        BroadcastChat(playerMessage);
    else
        ShowWarning($"Blocked ({result.Category}, severity: {result.Severity})");
});

// Filter with custom rules
mod.AddCustomRule(new IVXModerationRule
{
    Pattern = "cheat|hack|exploit",
    Category = IVXContentCategory.Harassment,
    Action = IVXModerationActionType.Block,
    ReplacementText = "***"
});

// Filter a message (returns cleaned text)
mod.FilterMessage(playerMessage, filtered =>
    BroadcastChat(filtered));

// Batch scan
mod.ScanBatch(chatMessages, results =>
{
    foreach (var r in results)
        if (r.SuggestedAction != IVXModerationActionType.Allow)
            FlagMessage(r.OriginalText);
});
```

#### 6d. AI Content Generation

```csharp
var gen = IVXAIContentGenerator.Instance;

// Generate a quest
gen.GenerateQuest(new IVXQuestTemplate
{
    Genre = "dragon_slaying",
    Difficulty = "hard",
    RequiredElements = new[] { "boss_fight", "rare_loot" },
    EstimatedDurationMinutes = 30
}, null, quest =>
{
    Debug.Log($"Quest: {quest.Title} — {quest.Description}");
    StartQuest(quest);
});

// Generate dialogue (scenario + character array)
gen.GenerateDialogue("player_buying_potion", new[] { "merchant", "player" },
    dialogue => PlayDialogue(dialogue));

// Generate item descriptions
gen.GenerateItemDescription("Flamebrand", "weapon", "legendary",
    item => AddToInventory(item));

// Generate story content
gen.GenerateStory("A dark forest conceals an ancient temple", "fantasy", 500,
    story => DisplayStory(story));
```

#### 6e. Player Behavior Profiling

```csharp
var profiler = IVXAIProfiler.Instance;

// Track events
profiler.TrackEvent("purchase", new Dictionary<string, object>
{
    { "item", "gem_pack_100" }, { "price", 4.99 }
});

// Get profile (TotalSessionCount, not TotalSessions)
profiler.GetPlayerProfile(profile =>
    Debug.Log($"Sessions: {profile.TotalSessionCount}, Cohort: {profile.Cohort}"));

// Predict churn
profiler.PredictChurn((risk, factors) =>
{
    if (risk > 0.7f) SendRetentionOffer();
    Debug.Log($"Churn risk: {risk:P0}, factors: {string.Join(", ", factors)}");
});

// Get personalization hints (returns List<IVXPersonalizationHint>, not List<string>)
profiler.GetPersonalizationHints(hints =>
{
    foreach (var hint in hints)
        ApplyPersonalization(hint);
});

// Classify player (returns IVXPlayerCohort enum, not string)
profiler.ClassifyPlayer(cohort =>
    Debug.Log($"Player cohort: {cohort}"));

// Auto-tracking (session events, periodic flush)
profiler.StartAutoTracking();
```

#### 6f. Voice AI Services

```csharp
var voice = IVXAIVoiceServices.Instance;

// Text-to-Speech (voiceId is optional)
voice.SynthesizeSpeech("Welcome, brave adventurer!", null, audioBytes =>
    PlayAudio(audioBytes));

// Speech-to-Text (returns IVXTranscriptionResult with .Text, .Language, .Confidence)
voice.TranscribeAudio(micPcmData, 16000, result =>
    ProcessPlayerSpeech(result.Text));

// List available voices
voice.ListVoices(voices =>
    PopulateVoiceSelector(voices));

// Language detection
voice.DetectLanguage(audioPcmData, 16000, (lang, confidence) =>
    SetGameLanguage(lang));

// Streaming transcription
voice.StartStreamingTranscription(16000);
voice.FeedAudioChunk(pcmChunk); // feed chunks as they arrive
voice.OnTranscriptionResult += result => ShowTranscription(result.Text);
voice.StopStreamingTranscription();
```

#### 6g. AI Voice Personas & Host

```csharp
var ai = IVXAISessionManager.Instance;

// Set player context for personalized AI
ai.SetPlayerContext(new IVXAIPlayerContext
{
    PlayerId = "player_123",
    DisplayName = "DragonSlayer42",
    TotalGamesPlayed = 150,
    OverallAccuracy = 0.78f,
    BestScore = 15000
});

// Start voice persona session
ai.StartVoiceSession("persona_id", "general_chat",
    onSuccess: session => Debug.Log($"Voice session: {session}"),
    onError: err => Debug.LogError($"Voice error: {err}")
);

// Listen for captions and audio
ai.OnCaptionReceived += (text) => ShowCaption(text);
ai.OnCaptionComplete += (fullText) => FinalizeCaption(fullText);
ai.OnAudioReceived += (base64Audio) => PlayBase64Audio(base64Audio);

// Send text input
ai.SendText("Tell me about the dragon's weakness");

// AI Host commentary
ai.StartHostSession(new IVXAICreateHostSessionRequest
{
    HostPersonaId = "sports_commentator",
    Topic = "trivia_round"
}, onSuccess: session => Debug.Log($"Host session: {session}"),
   onError: err => Debug.LogError(err)
);

ai.OnHostMessageReceived += msg => ShowCommentary(msg);
ai.SendHostGameEvent("player_scored", "Player1 answered correctly!");

// Get available personas
ai.GetPersonas(
    onSuccess: personas =>
    {
        foreach (var p in personas) Debug.Log($"Persona: {p.Name}");
    },
    onError: err => Debug.LogError(err)
);
```

---

### Step 7 — Discord Social SDK (11 Subsystems)

```csharp
// Account Linking
IVXDiscordManager.Instance.LinkAccount(); // no callback — fire-and-forget

// Mobile PKCE flow (requires redirect scheme)
IVXDiscordManager.Instance.StartMobileOAuth2Flow("mygame://oauth2", (success) =>
    Debug.Log($"Mobile OAuth2: {success}"));

// Console device code flow
IVXDiscordManager.Instance.StartConsoleOAuth2Flow(
    onDeviceCode: code => ShowDeviceCodeUI(code),  // show code to user
    onComplete: success => Debug.Log($"Console OAuth2: {success}")
);

// Rich Presence (note: details first, then state)
var presence = IVXDiscordPresence.Instance;
presence.SetActivity("Fighting the Dragon Boss", "In Battle"); // (details, state)
presence.SetParty("party_123", 3, 4);
presence.StartTimer();  // elapsed time from now
presence.AddButton("Join Game", "https://yourgame.com/join");
presence.SetSupportedPlatforms(IVXActivityPlatforms.Desktop | IVXActivityPlatforms.Mobile);

// Friends & Relationships (IDs are ulong for Discord, string for game friends)
var friends = IVXDiscordFriends.Instance;
friends.Refresh();
friends.OnFriendsUpdated += list =>
{
    foreach (var f in list) Debug.Log($"{f.DisplayName} ({f.Source})");
};
friends.SendGameFriendRequest("player_username");     // by username
friends.SendGameFriendRequestById(123456789UL);       // by Discord user ID
friends.BlockUser(123456789UL);                       // ulong, not string

// Direct Messages (recipientId is ulong)
var dms = IVXDiscordMessages.Instance;
dms.SendDM(123456789UL, "GG! Want to rematch?",
    onSuccess: messageId => Debug.Log($"Sent: {messageId}"),
    onError: err => Debug.LogError(err));
dms.GetDMHistory(123456789UL, 20, messages =>
{
    foreach (var m in messages) DisplayMessage(m);
});
dms.GetDMSummaries(summaries => UpdateInbox(summaries));

// Lobbies (uses secret string, not name+maxPlayers)
var lobby = IVXDiscordLobby.Instance;
lobby.CreateOrJoinLobby("ranked_match_secret");
lobby.CreateOrJoinLobbyWithMetadata("ranked_secret", "map=arena", "role=dps",
    onComplete: lobbyId => Debug.Log($"Joined lobby: {lobbyId}"));
lobby.SendMessage("I'm ready!");
lobby.OnMessageReceived += (sender, msg) => ShowChat(sender, msg);
lobby.SetLobbyIdleTimeout(300);

// Voice Chat (no callback on JoinCall)
var discordVoice = IVXDiscordVoice.Instance;
discordVoice.JoinCall(lobby.CurrentLobbyId);
discordVoice.SetSelfMute(true);
discordVoice.SetVADThreshold(useCustom: true, thresholdDb: -25f);

// Game Invites
IVXDiscordInvites.Instance.SendInvite("discord_user_id_string", "Join my match!");
IVXDiscordInvites.Instance.OnInviteReceived += invite => ShowInviteUI(invite);

// Linked Channels
IVXDiscordLinkedChannels.Instance.SendToLinkedChannel("Boss defeated!");

// Moderation (userId is ulong)
var discordMod = IVXDiscordModeration.Instance;
discordMod.EnableAutoModeration(true);
discordMod.ReportUser(123456789UL, "Toxic behavior", success =>
    Debug.Log($"Report submitted: {success}"));

// Debug
IVXDiscordDebug.Instance.SetLogLevel(IVXDiscordLogLevel.Warning);
```

---

### Step 8 — Multiplayer & Game Modes

```csharp
// Game Mode Manager — SelectMode (not SetGameMode)
var modes = IVXGameModeManager.Instance;
modes.SelectMode(IVXGameMode.OnlineVersus, maxPlayers: 4);

// Or use a full config
modes.SetConfig(IVXMatchConfig.OnlineVersus(maxPlayers: 4));
modes.SetConfig(IVXMatchConfig.Ranked());
modes.SetConfig(IVXMatchConfig.Local(maxPlayers: 2));

// Add players
var localPlayer = modes.AddLocalPlayer("Player 1", inputDeviceIndex: 0);
var bot = modes.AddBot("CPU");
modes.SetPlayerReady(localPlayer.SlotIndex, true);
modes.StartMatch();

// Events
modes.OnModeChanged += mode => Debug.Log($"Mode: {mode}");
modes.OnAllPlayersReady += config => StartGame(config);

// Lobby system — uses request objects
var lobby = IVXLobbyManager.Instance;

// List rooms (event-driven, not callback)
lobby.OnRoomListUpdated += rooms => DisplayRoomList(rooms);
lobby.RefreshRoomList(new IVXRoomFilter { OnlyAvailable = true, Limit = 20 });

// Create room (request object, not bare args)
lobby.CreateRoom(new IVXCreateRoomRequest
{
    RoomName = "My Room",
    Config = IVXMatchConfig.OnlineVersus(maxPlayers: 4),
    Password = null
});
lobby.OnRoomCreated += response => Debug.Log($"Room: {response.RoomId}");

// Join room (request object)
lobby.JoinRoom(new IVXJoinRoomRequest { RoomId = "room_id" });
lobby.OnRoomJoined += response => Debug.Log($"Joined: {response.Success}");

// Matchmaking — StartSearch (not StartMatchmaking)
var mm = IVXMatchmakingManager.Instance;
mm.StartSearch(IVXMatchConfig.Ranked());
mm.OnMatchFound += match =>
{
    Debug.Log($"Match: {match.MatchId} vs {match.OpponentDisplayName}");
    LoadMatch(match);
};
mm.OnSearchProgress += elapsed => UpdateSearchUI(elapsed);

// Quick match / ranked shortcuts
mm.QuickMatch();
mm.RankedMatch();

// Local Multiplayer — uses IVXGameModeManager for player registration
modes.SelectMode(IVXGameMode.LocalMultiplayer, maxPlayers: 4);
modes.AddLocalPlayer("Player 1", 0);
modes.AddLocalPlayer("Player 2", 1);

var local = IVXLocalMultiplayerManager.Instance;
local.StartSession(hotSeat: true, turnTimeLimitSeconds: 30f);
local.OnTurnStarted += player => ShowTurnUI(player.DisplayName);
local.OnRoundCompleted += round => Debug.Log($"Round {round} complete");
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

    // For async Hiro/Satori calls:
    var result = await manager.SomeAsync("param");

    // For callback-based AI/Discord calls:
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
void OnApplicationPause(bool paused)
{
    if (paused)
        IVXAIProfiler.Instance?.FlushEvents();
}

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

const client = new IVXClient({
  apiBase: 'https://api.intelli-verse-x.ai',
  nakamaHost: 'your-server.com',
  nakamaPort: 7350,
  nakamaKey: 'your-key'
});

await client.initialize();
const userId = client.userId;

const assistant = new IVXAIAssistant(client);
await assistant.initialize({ apiKey: 'YOUR_AI_KEY' });
const answer = await assistant.ask('How do I craft a sword?');

const npc = new IVXAINPCDialogManager(client);
await npc.initialize({ apiKey: 'YOUR_AI_KEY' });
npc.registerNPC({ npcId: 'blacksmith', personaPrompt: 'Gruff dwarf blacksmith' });
const session = await npc.startDialog('blacksmith', userId);
const response = await npc.sendMessage(session.sessionId, 'Got any rare metals?');

const profiler = new IVXAIProfiler(client);
await profiler.initialize({ apiKey: 'YOUR_AI_KEY', playerId: userId });
profiler.trackEvent('purchase', { item: 'gem_pack', price: 4.99 });
```

#### Flutter / Dart

```dart
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

final client = IVXClient(
  apiBase: 'https://api.intelli-verse-x.ai',
  nakamaHost: 'your-server.com',
  nakamaPort: 7350,
);

await client.initialize();

final assistant = IVXAIAssistant(client);
await assistant.initialize(apiKey: 'YOUR_AI_KEY');
final answer = await assistant.ask('How do I defeat the boss?');

final npc = IVXAINPCDialogManager(client);
await npc.initialize(apiKey: 'YOUR_AI_KEY');
npc.registerNPC(IVXNPCProfile(npcId: 'guard', personaPrompt: 'Stern city guard'));
final session = await npc.startDialog('guard', client.userId);
final response = await npc.sendMessage(session.sessionId, 'Can I pass?');

final mod = IVXAIModerator(client);
await mod.initialize(apiKey: 'YOUR_AI_KEY');
final result = await mod.classifyText(playerMessage);
if (result.suggestedAction == ModerationAction.allow) broadcastChat(playerMessage);
```

#### Java / Android

```java
IVXClient client = IVXClient.builder()
    .apiBase("https://api.intelli-verse-x.ai")
    .nakamaHost("your-server.com")
    .nakamaPort(7350)
    .nakamaKey("your-key")
    .build();

client.initialize().thenAccept(v -> {
    String userId = client.getUserId();

    IVXAIAssistant assistant = IVXAIAssistant.getInstance();
    assistant.initialize("YOUR_AI_KEY");
    assistant.ask("How do I craft a sword?", null, response -> {
        Log.d("IVX", "Answer: " + response.getResponse());
    });

    IVXAINPCDialogManager npc = IVXAINPCDialogManager.getInstance();
    npc.initialize("YOUR_AI_KEY");
    npc.registerNPC("blacksmith", "Gruff dwarf blacksmith");
    npc.startDialog("blacksmith", userId, null, session -> {
        npc.sendMessage(session.getSessionId(), "Got any rare metals?", text -> {
            Log.d("IVX", "NPC: " + text);
        });
    });

    IVXAIProfiler profiler = IVXAIProfiler.getInstance();
    profiler.initialize("YOUR_AI_KEY", userId);
    profiler.trackEvent("purchase", Map.of("item", "gem_pack", "price", "4.99"));
});
```

#### Godot (GDScript)

```gdscript
var client = IVXClient.new()
client.api_base = "https://api.intelli-verse-x.ai"
client.nakama_host = "your-server.com"
await client.initialize()

var assistant = IVXAIAssistant.new()
await assistant.initialize("YOUR_AI_KEY")
var answer = await assistant.ask("How do I defeat the boss?")
print("Answer: " + answer.response)

var npc = IVXAINPCDialogManager.new()
await npc.initialize("YOUR_AI_KEY")
npc.register_npc("blacksmith", "Gruff dwarf blacksmith")
var session = await npc.start_dialog("blacksmith", client.user_id)
var response = await npc.send_message(session.session_id, "Got any swords?")
print("NPC: " + response)
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

*IntelliVerseX SDK v5.8.0 — 10 platforms, 70+ features, 1 prompt.*
