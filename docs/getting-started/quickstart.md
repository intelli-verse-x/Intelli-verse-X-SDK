# Quick Start

Get IntelliVerseX SDK running in your project in under 5 minutes — zero to all features.

---

## Prerequisites

- **Unity 2023.3+** or **Unity 6** (legacy 2021.3/2022.x may work but is not officially supported)
- TextMeshPro (included in Unity)
- Newtonsoft JSON (`com.unity.nuget.newtonsoft-json`)

**Optional (for full backend features):**

- Nakama SDK (`com.heroiclabs.nakama-unity`) — backend auth, leaderboards, economy, storage
- Discord Social SDK (`com.discord.social-sdk`) — Discord presence, friends, voice, DMs

!!! tip "No backend yet? No problem."
    **Core SDK** works offline — `IVXBootstrap` initializes gracefully without a server and demo UIs remain functional.
    **AI features** require a backend by default, but you can enable **Mock Mode** in `IVXAIConfig` to get canned placeholder responses with zero API calls during development. See [AI Getting Started](../guides/ai-getting-started.md) for details.

---

## Step 1 — Register Your Game

Before writing code, register your game on the IntelliVerseX platform to get a **Game ID** (UUID). This ID connects your game to leaderboards, wallets, analytics, and all backend services.

**Option A — Dashboard (recommended):**

1. Sign in at [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers)
2. Click **New Project** → enter your game title → copy the **Game ID**

**Option B — API:**

```bash
# Authenticate
TOKEN=$(curl -s -X POST 'https://api.intelli-verse-x.ai/api/admin/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"email":"you@example.com","password":"your-password"}' \
  | python3 -c "import sys,json; print(d:=json.load(sys.stdin),d.get('data',{}).get('accessToken',''))")

# Create game
curl -s -X POST 'https://msapi.intelli-verse-x.io/api/games/game/info' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"gameTitle": "My Awesome Game"}'

# Response → {"status":true,"data":{"gameId":"83e9cbd5-..."}}
```

!!! warning "Save your Game ID"
    You will paste this UUID into the **Game Config** in Step 3. Every API call, leaderboard, wallet, and analytics event is scoped to this ID.

---

## Step 2 — Install the SDK

```
Window > Package Manager > + > Add package from git URL:
https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git?path=Assets/Intelli-verse-X-SDK
```

---

## Step 3 — Create Configuration Assets

Create these three config assets via the Unity menu:

| Config | Menu Path | Purpose |
|--------|-----------|---------|
| **Bootstrap Config** | `Assets > Create > IntelliVerseX > Bootstrap Config` | Server settings, feature toggles |
| **AI Config** | `Assets > Create > IntelliVerseX > AI > Configuration` | AI API endpoint, API key, audio settings |
| **Discord Config** | `Assets > Create > IntelliVerseX > Discord Config` | Discord Application ID, OAuth2, presence settings |

!!! tip "Save configs in Resources/"
    Place all three config assets in `Assets/Resources/` so they can be loaded at runtime.

### Fill in the configs:

**Bootstrap Config:**

- **Game ID** — Paste the UUID from Step 1 (e.g. `83e9cbd5-3883-4fec-8344-0d2d3ca35be3`)
- **Server Host** — Your Nakama server address (leave as `127.0.0.1` for local development; see [Setting Up Nakama](#setting-up-nakama) below)
- **Server Port** — Default `7350`
- **Server Key** — Your Nakama server key (default `defaultkey` for local dev; change for production)
- **Auto Device Auth** — Check this for automatic authentication on startup
- **Enable Hiro / Satori / AI / Discord / Multiplayer** — Toggle each module

**AI Config:**

- **API Base URL** — Your IntelliVerseX AI API endpoint
- **API Key** — Your API key (optional if using Bearer tokens)

**Discord Config:**

- **Application ID** — From the [Discord Developer Portal](https://discord.com/developers/applications)
- **Client ID** — Same as Application ID for most games
- **Redirect URI** — Your registered OAuth2 redirect URI

---

## Step 4 — Add Bootstrap to Your Scene

1. Create an empty GameObject in your first scene
2. Name it `IVXBootstrap`
3. Add the `IVXBootstrap` component (from `IntelliVerseX.Bootstrap`)
4. Drag your **Bootstrap Config** asset into the `Config` field
5. Drag your **AI Config** into the Bootstrap Config's `AI Config` slot
6. Drag your **Discord Config** into the Bootstrap Config's `Discord Config` slot
7. Ensure **Auto Initialize** is checked (default)

**Or use prefabs:** Run `IntelliVerseX > Generate All Prefabs` from the menu bar, then drag `IVX_Bootstrap.prefab` into your scene.

---

## Step 5 — Press Play

That's it. Press Play and check the Console:

```
[IVXBootstrap] Starting SDK bootstrap...
[IVXBootstrap]   ✓ Platform ready
[IVXBootstrap] Connecting to Nakama at 127.0.0.1:7350...
[IVXBootstrap] Authenticated: Player_abc1 (user-id-123)
[IVXBootstrap]   ✓ Backend ready
[IVXBootstrap]   ✓ Hiro ready
[IVXBootstrap]   ✓ Satori ready
[IVXBootstrap]   ✓ Discord ready
[IVXBootstrap]   ✓ AI ready
[IVXBootstrap]   ✓ Multiplayer ready
[IVXBootstrap] Bootstrap complete. User: user-id-123
```

!!! success "All Systems Go"
    Every module is initialized. Start using any feature immediately.

!!! tip "No Nakama running?"
    If Nakama isn't available, you'll see: `[IVXBootstrap] Backend auth failed — continuing in offline mode`. The SDK still works — all modules use mock/local data. Add a backend when ready.

---

## Setting Up Nakama

The SDK uses [Nakama](https://heroiclabs.com/nakama/) as its game server backend. Here's how to get one running:

**Quick start with Docker (recommended for local dev):**

```bash
docker run -d --name nakama \
  -p 7349:7349 -p 7350:7350 -p 7351:7351 \
  heroiclabs/nakama
```

**Heroic Labs Cloud (recommended for production):**

Use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting with automatic scaling, monitoring, and Hiro/Satori integration.

**Full setup guide:** See the [Nakama documentation](https://heroiclabs.com/docs/nakama/getting-started/install/) for Docker Compose, binary installs, and cloud deployment options.

Once running, update your **Bootstrap Config** with the server address and key.

---

## Step 6 — Listen for Bootstrap Completion

In your game script, listen for the bootstrap to finish before using SDK features:

```csharp
using UnityEngine;
using IntelliVerseX.Bootstrap;

public class MyGame : MonoBehaviour
{
    void Start()
    {
        var bootstrap = IVXBootstrap.Instance;

        if (bootstrap.IsInitialized)
        {
            OnSDKReady();
            return;
        }

        bootstrap.OnBootstrapComplete += success =>
        {
            if (success) OnSDKReady();
            else Debug.LogWarning("SDK in offline mode — some features unavailable");
        };

        bootstrap.OnModuleFailed += (module, error) =>
            Debug.LogWarning($"[Optional] {module} failed: {error}");
    }

    void OnSDKReady()
    {
        Debug.Log($"SDK ready! User: {IVXBootstrap.Instance.UserId}");

        // Start using any feature:
        // IVXHiroCoordinator.Instance.Leaderboards.SubmitScore(...)
        // IVXAIAssistant.Instance.Ask(...)
        // IVXDiscordPresence.Instance.SetActivity(...)
    }
}
```

---

## Step 6 — Explore the Demo Hub

Drop the Demo Hub prefab into any scene to see all 16 features in action:

1. Run `IntelliVerseX > Generate All Prefabs`
2. Drag `Assets/_IntelliVerseXSDK/Prefabs/IVX_DemoHub.prefab` into your scene
3. Press Play — browse and launch any demo

| Demo | What You'll See |
|------|----------------|
| Identity & Auth | Device auth, guest login, session save/restore |
| Leaderboard | Score submission, rankings, player rank |
| Discord Social | Account linking, presence, friends, DMs, voice, lobbies |
| AI Voice Chat | Conversational AI with voice personas |
| AI NPC Dialog | Dynamic NPC conversations with branching |
| AI Assistant | In-game help, hints, tutorials |
| AI Moderation | Content classification and filtering |
| AI Content Gen | Quest, story, item generation |
| AI Profiler | Behavior tracking, cohort analysis, churn prediction |
| AI Voice Services | Standalone STT, TTS, language detection |
| Spin Wheel | Daily reward wheel |
| Daily Streak | Login streak rewards |
| Offerwall | Ad monetization offers |
| Game Modes | Solo, local MP, online MP |
| Lobby | Online lobby and matchmaking |
| AI Host | AI game commentary |

---

## Step 8 — Integrate Features Into Your Game

Use the **[Master Integration Prompt](../MASTER_INTEGRATION_PROMPT.md)** — a single document with executable code samples for every SDK feature. Copy it into any AI assistant alongside your game project to integrate everything at once.

### Quick Examples

=== "Leaderboard"

    ```csharp
    var hiro = IVXHiroCoordinator.Instance;
    await hiro.Leaderboards.SubmitScoreAsync("global", 12500);
    var records = await hiro.Leaderboards.GetRecordsAsync("global", limit: 10);
    ```

=== "AI NPC Dialog"

    ```csharp
    var npc = IVXAINPCDialogManager.Instance;
    npc.RegisterNPC(new IVXAINPCProfile
    {
        NpcId = "blacksmith",
        DisplayName = "Gorrak the Smith",
        PersonaPrompt = "Gruff but kind dwarf blacksmith."
    });
    string playerId = IVXBootstrap.Instance?.UserId ?? "local";
    npc.StartDialog("blacksmith", playerId, null, session =>
    {
        npc.SendMessage(session.SessionId, "Can you forge a sword?", response =>
            Debug.Log($"NPC: {response}"));
    });
    ```

=== "Discord Presence"

    ```csharp
    var presence = IVXDiscordPresence.Instance;
    presence.SetActivity("Fighting the Dragon Boss", "In Battle");
    presence.SetParty("party_123", 3, 4);
    presence.AddButton("Join Game", "https://yourgame.com/join");
    ```

=== "Content Moderation"

    ```csharp
    var mod = IVXAIModerator.Instance;
    mod.ClassifyText(playerMessage, result =>
    {
        if (result.SuggestedAction == IVXModerationActionType.Allow)
            BroadcastChat(playerMessage);
        else ShowWarning($"Blocked: {result.Category}");
    });
    ```

=== "Spin Wheel"

    ```csharp
    var hiro = IVXHiroCoordinator.Instance;
    var prize = await hiro.SpinWheel.SpinAsync("free");
    ```

---

## Lifecycle Management

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

## Cross-Platform Quick Start

The SDK supports 10 platforms. For non-Unity platforms, see the [Master Integration Prompt](../MASTER_INTEGRATION_PROMPT.md#step-12--cross-platform-installation) for install instructions.

=== "JavaScript / TypeScript"

    ```bash
    npm install @intelliversex/sdk
    ```
    ```typescript
    import { IVXClient, IVXAIAssistant, IVXDiscordMessages } from '@intelliversex/sdk';

    const client = new IVXClient({ apiBase: 'https://api.intelli-verse-x.ai' });
    await client.initialize();

    const assistant = new IVXAIAssistant(client);
    await assistant.initialize({ apiKey: 'YOUR_KEY' });
    const answer = await assistant.ask('How do I defeat the boss?');
    ```

=== "Flutter / Dart"

    ```bash
    flutter pub add intelliversex_sdk
    ```
    ```dart
    import 'package:intelliversex_sdk/intelliversex_sdk.dart';

    final client = IVXClient(apiBase: 'https://api.intelli-verse-x.ai');
    await client.initialize();

    final assistant = IVXAIAssistant(client);
    await assistant.initialize(apiKey: 'YOUR_KEY');
    final answer = await assistant.ask('How do I defeat the boss?');
    ```

=== "Java / Android"

    ```java
    IVXClient client = IVXClient.builder()
        .apiBase("https://api.intelli-verse-x.ai")
        .build();
    client.initialize();

    IVXAIAssistant assistant = IVXAIAssistant.getInstance();
    assistant.initialize("YOUR_KEY");
    assistant.ask("How do I defeat the boss?", answer -> {
        Log.d("IVX", "Answer: " + answer);
    });
    ```

=== "Unreal Engine (C++)"

    Copy `SDKs/unreal/` to your project's `Plugins/IntelliVerseX/` folder.

    ```cpp
    #include "IVXAIAssistant.h"

    UIVXAIAssistant* Assistant = UIVXAIAssistant::GetInstance();
    Assistant->Initialize(TEXT("YOUR_KEY"));
    Assistant->Ask(TEXT("How do I defeat the boss?"),
        FOnAnswerReceived::CreateLambda([](const FString& Answer) {
            UE_LOG(LogTemp, Log, TEXT("Answer: %s"), *Answer);
        }));
    ```

=== "Godot (GDScript)"

    Copy `SDKs/godot/addons/intelliversex/` to your project's `addons/` folder.

    ```gdscript
    var assistant = IVXAIAssistant.new()
    assistant.initialize("YOUR_KEY")
    var answer = await assistant.ask("How do I defeat the boss?")
    print("Answer: " + answer)
    ```

---

## Troubleshooting

### Bootstrap logs "No IVXBootstrapConfig assigned"

Drag the Bootstrap Config asset onto the `IVXBootstrap` component's `Config` field in the Inspector.

### "Nakama not installed — skipping backend auth"

Install the Nakama SDK: `Window > Package Manager > + > Add package from git URL: https://github.com/heroiclabs/nakama-unity/releases`

### Discord features show "stub mode"

Install the Discord Social SDK (`com.discord.social-sdk`) and add the scripting define `INTELLIVERSEX_HAS_DISCORD`.

### AI services return null

1. **During development**: Enable **Mock Mode** in `IVXAIConfig` to get placeholder responses without an API.
2. **With a live backend**: Ensure `IVXAIConfig` has a valid `API Base URL` and `API Key` (or call `SetAuthToken()`).
3. Subscribe to `OnError` events on each AI manager to see the actual HTTP error.
4. Enable `Debug Logging` in `IVXAIConfig` to see full request/response details in Console.

See the full [AI Getting Started Guide](../guides/ai-getting-started.md) for backend setup options.

### Demo Hub doesn't appear

Make sure TextMeshPro is installed. On first use, Unity may prompt you to import TMP Essential Resources — click **Import**.

---

## Next Steps

| Want to... | Go to... |
|-----------|---------|
| Integrate all features at once | [Master Integration Prompt](../MASTER_INTEGRATION_PROMPT.md) |
| Add AI to your game (NPC, Moderation, etc.) | [AI Getting Started](../guides/ai-getting-started.md) |
| AI architecture reference | [AI LLM Stack](../modules/ai-llm-stack.md) |
| Set up Discord | [Discord Social SDK](../modules/discord.md) |
| Explore Hiro live-ops | [Hiro Systems](../modules/hiro.md) |
| See the feature matrix | [Feature Coverage](../FEATURE_COVERAGE_MATRIX.md) |
| Try the demos | [Demo UIs](../modules/demos.md) |
