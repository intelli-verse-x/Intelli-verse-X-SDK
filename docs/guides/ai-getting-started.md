# AI & LLM Stack — Getting Started

> **Time to first AI response: ~10 minutes** (including project setup).

---

## What You Get

Every game that integrates IntelliVerseX gets a **complete AI-powered game layer**:

| Feature | Class | What It Does |
|---------|-------|-------------|
| NPC Dialog | `IVXAINPCDialogManager` | Dynamic, personality-driven NPC conversations |
| In-Game Assistant | `IVXAIAssistant` | Contextual hints, tutorials, Q&A |
| Chat Moderation | `IVXAIModerator` | Classify/filter/block toxic chat in real time |
| Content Generation | `IVXAIContentGenerator` | Procedural quests, stories, items, dialogue |
| Player Profiling | `IVXAIProfiler` | Cohort, churn risk, personalization hints |
| Voice AI | `IVXAIVoiceServices` | Speech-to-text, text-to-speech, streaming STT |
| Voice Personas | `IVXAISessionManager` | Full voice conversation sessions with AI personas |
| AI Host | `IVXAISessionManager` | Live match commentary and hosting |

All features share a single config asset (`IVXAIConfig`) and work on **Android, iOS, WebGL, and Standalone**.

---

## Prerequisites

| Requirement | Details |
|-------------|---------|
| Unity | 2023.3+ or Unity 6 |
| IntelliVerseX SDK | v5.8.0+ installed via UPM |
| Newtonsoft JSON | Included with SDK |
| Backend | See [Backend Options](#2-choose-your-ai-backend) below |

---

## Step 1 — Create the AI Config Asset

1. In Unity: **Assets → Create → IntelliVerseX → AI → Configuration**
2. Name it `IVXAIConfig` and place it alongside your `IVXBootstrapConfig`

The Inspector shows these key fields:

| Field | What to Set | Notes |
|-------|------------|-------|
| **API Base URL** | Your AI endpoint | See Step 2 |
| **API Key** | Your API key | Leave empty for bearer-token auth |
| **Provider** | `IntelliVerseX`, `OpenAI`, `AzureOpenAI`, `Anthropic`, or `Custom` | Choose your backend |
| **Model Name** | e.g. `gpt-4o`, `claude-3-opus`, `llama3` | Empty = server default |
| **Mock Mode** | ✅ during development | Returns canned responses, zero API calls |
| **Debug Logging** | ✅ during development | Logs all requests/responses to Console |

!!! warning "API Key Security"
    Keys stored in `IVXAIConfig` are baked into builds and extractable.
    For production, leave the field empty and inject at runtime:

    ```csharp
    // After authenticating with your server, inject the key:
    aiConfig.SetApiKey(serverProvidedKey);
    ```

---

## Step 2 — Choose Your AI Backend

### Option A: IntelliVerseX Managed API (Recommended)

The default endpoint (`https://api.intelli-verse-x.ai/api/ai`) is our managed service.

1. Sign up at [intelli-verse-x.ai/developers](https://intelli-verse-x.ai/developers) (or contact sales)
2. Create a project → copy your **API Key**
3. Paste the key into `IVXAIConfig.ApiKey` (dev) or inject at runtime (prod)

**Pricing**: Free tier includes 1,000 requests/month. See the developer portal for plans.

### Option B: OpenAI / Azure OpenAI / Anthropic

Set `Provider` to `OpenAI`, `AzureOpenAI`, or `Anthropic` and update:

| Field | OpenAI | Azure OpenAI | Anthropic |
|-------|--------|--------------|-----------|
| **API Base URL** | `https://api.openai.com/v1` | `https://{resource}.openai.azure.com/openai/deployments/{deployment}` | `https://api.anthropic.com/v1` |
| **API Key** | Your OpenAI key | Your Azure key | Your Anthropic key |
| **Model Name** | `gpt-4o` | `gpt-4o` | `claude-3-opus-20240229` |

!!! note "API Compatibility"
    The SDK uses a standard REST contract (JSON request/response). The IntelliVerseX API, OpenAI, and most OpenAI-compatible servers use the same request shape for chat completions. For moderation, NPC, and content endpoints, you'll need the IntelliVerseX backend or a compatible proxy.

### Option C: Self-Hosted (Ollama, vLLM, LiteLLM)

For cost control, data sovereignty, or offline use:

1. Set `Provider` to `Custom`
2. Set `API Base URL` to your server (e.g. `http://localhost:11434/v1` for Ollama)
3. Set `Model Name` to the model you've pulled (e.g. `llama3:8b`)
4. Leave `API Key` empty or set to your server's auth token

```bash
# Example: Run Ollama locally
brew install ollama
ollama pull llama3:8b
ollama serve  # Serves on http://localhost:11434
```

!!! tip "LiteLLM as a Universal Proxy"
    [LiteLLM](https://github.com/BerriAI/litellm) can proxy requests to 100+ LLM providers with a single OpenAI-compatible endpoint. This is the easiest way to swap models without changing SDK config.

---

## Step 3 — Mock Mode (Zero Cost Development)

Enable **Mock Mode** in `IVXAIConfig` to develop your UI and game logic without any API calls:

```csharp
// All AI managers check config.MockMode before making HTTP calls.
// When enabled, they return sample/placeholder data instantly.

// You can also toggle at runtime:
aiConfig.SetApiKey("");  // Ensure no accidental real calls
```

Mock mode returns:
- **NPC Dialog**: `"This is a mock NPC response. Connect your AI backend for real conversations."`
- **Assistant**: Sample hints and tutorial steps
- **Moderator**: Always returns `Clean` classification
- **Content Generator**: Template-based placeholder content
- **Profiler**: Default `Casual` cohort with neutral scores
- **Voice**: Empty audio buffers (no crashes)

This lets you:
- Build and test your entire UI flow
- Write integration tests in CI without API keys
- Demo the game to stakeholders without burning credits

---

## Step 4 — Hello World: AI NPC in 10 Lines

```csharp
using IntelliVerseX.AI;
using UnityEngine;

public class MyNPCExample : MonoBehaviour
{
    [SerializeField] private IVXAIConfig _aiConfig;

    private void Start()
    {
        var npc = IVXAINPCDialogManager.Instance;
        npc.Initialize(_aiConfig);

        // Register an NPC personality
        npc.RegisterNPC(new IVXAINPCProfile
        {
            NpcId = "blacksmith",
            DisplayName = "Gorrak the Smith",
            PersonaPrompt = "Gruff but kind dwarf blacksmith. Expert in rare metals.",
            MaxTurns = 20
        });

        // Start a conversation
        npc.StartDialog("blacksmith", "player_123", null, session =>
        {
            Debug.Log($"Dialog started: {session.SessionId}");

            // Send a message
            npc.SendMessage(session.SessionId, "Can you repair my sword?", response =>
            {
                Debug.Log($"NPC says: {response.Text}");
            });
        });
    }
}
```

---

## Step 5 — Wire All AI Managers (Full Integration)

For games that want every AI feature, initialize all managers after `IVXBootstrap` completes:

```csharp
using IntelliVerseX.AI;
using UnityEngine;

public class AIIntegration : MonoBehaviour
{
    [SerializeField] private IVXAIConfig _aiConfig;

    private void OnEnable()
    {
        IVXBootstrap.Instance.OnBootstrapComplete += OnReady;
    }

    private void OnReady()
    {
        string userId = IVXBootstrap.Instance.UserId;
        string token = IVXBootstrap.Instance.AuthToken;

        // NPC Dialog
        IVXAINPCDialogManager.Instance.Initialize(_aiConfig);
        IVXAINPCDialogManager.Instance.SetAuthToken(token);

        // In-Game Assistant
        IVXAIAssistant.Instance.Initialize(_aiConfig);
        IVXAIAssistant.Instance.SetAuthToken(token);

        // Chat Moderation
        IVXAIModerator.Instance.Initialize(_aiConfig);
        IVXAIModerator.Instance.SetAuthToken(token);

        // Content Generation
        IVXAIContentGenerator.Instance.Initialize(_aiConfig);
        IVXAIContentGenerator.Instance.SetAuthToken(token);

        // Player Profiling
        IVXAIProfiler.Instance.Initialize(_aiConfig, userId);
        IVXAIProfiler.Instance.SetAuthToken(token);
        IVXAIProfiler.Instance.StartAutoTracking();

        // Voice Services (STT/TTS)
        IVXAIVoiceServices.Instance.Initialize(_aiConfig);
        IVXAIVoiceServices.Instance.SetAuthToken(token);

        Debug.Log("[AI] All AI managers initialized.");
    }
}
```

---

## Step 6 — Common Recipes

### Moderate Chat Messages Before Sending

```csharp
IVXAIModerator.Instance.FilterMessage(playerInput, filtered =>
{
    if (string.IsNullOrEmpty(filtered))
    {
        ShowWarning("Message blocked by moderation.");
        return;
    }
    BroadcastToChat(filtered);
});
```

### Generate a Quest Based on Player Progress

```csharp
IVXAIContentGenerator.Instance.GenerateQuest(
    new IVXQuestTemplate { Genre = "sci-fi", Difficulty = "hard" },
    $"Player level: {playerLevel}, Items: {string.Join(",", inventory)}",
    quest =>
    {
        if (quest != null)
            questLog.Add(quest);
    });
```

### Ask the AI Assistant for Help

```csharp
var ctx = new IVXAIGameContext
{
    CurrentLevel = "3-2",
    CurrentObjective = "Defeat the ice dragon",
    PlayerStats = new Dictionary<string, float> { ["health"] = 45f }
};

IVXAIAssistant.Instance.Ask("How do I beat this boss?", ctx, response =>
{
    ShowHintUI(response.Response);
});
```

### Track Player Behavior for Personalization

```csharp
IVXAIProfiler.Instance.TrackEvent("match_complete", new Dictionary<string, object>
{
    ["score"] = 1200,
    ["duration_seconds"] = 180,
    ["mode"] = "ranked"
});

IVXAIProfiler.Instance.GetPersonalizationHints(hints =>
{
    foreach (var hint in hints)
        Debug.Log($"Suggestion: {hint.Message} (priority: {hint.Priority})");
});
```

---

## Error Handling & Resilience

Every AI manager fires an `OnError` event. Always subscribe:

```csharp
IVXAINPCDialogManager.Instance.OnError += msg => Debug.LogWarning($"NPC error: {msg}");
IVXAIAssistant.Instance.OnError += msg => Debug.LogWarning($"Assistant error: {msg}");
IVXAIModerator.Instance.OnContentBlocked += (text, reason) => Debug.Log($"Blocked: {reason}");
IVXAIContentGenerator.Instance.OnError += msg => Debug.LogWarning($"Content error: {msg}");
IVXAIVoiceServices.Instance.OnError += msg => Debug.LogWarning($"Voice error: {msg}");
```

**What happens when the AI backend is down?**

- **Moderator**: Falls back to local custom rules (offline-capable)
- **Profiler**: Queues events locally, flushes when connectivity returns
- **Other managers**: Return `null` or invoke `OnError` — your game keeps running, the AI feature gracefully degrades
- **Config-level retries**: Set `MaxRetries` in `IVXAIConfig` for automatic retry with exponential backoff on 5xx errors

---

## Rate Limits & Costs

| Concern | How It's Handled |
|---------|-----------------|
| **Rate limiting** | Set `RateLimitPerSecond` in `IVXAIConfig` to cap outbound requests |
| **API credits** | Use `MockMode` during dev; `FreeSessionsPerDay` gates player access |
| **Cost tracking** | `IVXAISessionAnalytics.EstimatedCost` and `CreditsUsed` after voice sessions |
| **Budget alerts** | Configure in your AI provider's dashboard (OpenAI, Azure, etc.) |

---

## Cross-Platform Status

| Platform | AI Voice/Host | NPC Dialog | Assistant | Moderator | Content Gen | Profiler | Voice STT/TTS |
|----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Unity** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **JavaScript/TS** | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |
| **Java/Android** | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |
| **Flutter/Dart** | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |
| **Unreal (C++)** | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |
| **Godot (GDScript)** | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |
| Others | ✅ | Stub | Stub | Stub | Stub | Stub | Stub |

"Stub" means the class and types exist with matching signatures — implementations arrive when each platform reaches production. Unity is the reference implementation.

---

## Troubleshooting

### AI responses are null

1. Check `IVXAIConfig` has a valid **API Base URL**
2. Ensure `MockMode` is off (or on, depending on what you want)
3. Subscribe to `OnError` events — they'll tell you the HTTP status
4. Enable `DebugLogging` to see full request/response in Console

### 401 Unauthorized errors

- Ensure you called `SetAuthToken(token)` on each manager after auth
- Or set the `ApiKey` field in the config

### Voice features return empty audio

- Check `AudioSampleRate` matches your backend (default: 16000 Hz)
- Verify microphone permissions are granted (Android/iOS)
- Check `IVXAIAudioRecorder` is on an active GameObject

### Mock mode returns unexpected data

- Mock responses are hardcoded placeholders — they don't reflect your NPC profiles or game context
- Use mock mode only for UI development and CI testing

---

## Next Steps

- [AI Voice & Host — Full Reference](../modules/ai.md)
- [AI LLM Stack — Architecture](../modules/ai-llm-stack.md)
- [MASTER_INTEGRATION_PROMPT](../MASTER_INTEGRATION_PROMPT.md) — One-shot integration guide
- [Discord Moderation Bridge](../modules/discord.md) — Route moderation to Discord
