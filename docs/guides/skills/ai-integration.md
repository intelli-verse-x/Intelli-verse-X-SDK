# Skill: AI Integration

**Skill ID:** `ivx-ai-integration`

Integrates 7 AI subsystems into your game -- voice host, NPC dialog, content generation, moderation, player profiling, assistant hints, and structured output. Supports OpenAI, Azure, Anthropic, and self-hosted LLMs.

---

## When to Use

Ask your AI agent any of these:

- "Add an AI voice host for my trivia game"
- "Set up NPC dialog with GPT-4o"
- "Generate trivia questions using AI"
- "Add content moderation to player chat"
- "Configure Ollama as a self-hosted LLM provider"
- "Set up AI player profiling"
- "Add AI hints to my puzzle game"

---

## What the Agent Does

```mermaid
flowchart TD
    A[You: "Add AI to my game"] --> B[Agent loads ivx-ai-integration skill]
    B --> C[Creates IVXAIConfig]
    C --> D[Configures provider + model]
    D --> E{Which subsystem?}
    E -->|Voice Host| F[IVXAIVoiceServices]
    E -->|NPC Dialog| G[IVXAINPCDialogManager]
    E -->|Content Gen| H[IVXAIContentGenerator]
    E -->|Moderation| I[IVXAIModerator]
    E -->|Profiling| J[IVXAIProfiler]
    E -->|Assistant| K[IVXAIAssistant]
    E -->|Sessions| L[IVXAISessionManager]
```

---

## Configuration

### IVXAIConfig ScriptableObject

Create via **Create > IntelliVerseX > AI Configuration**.

| Field | Description |
|-------|-------------|
| `Provider` | `IntelliVerseX`, `OpenAI`, `AzureOpenAI`, `Anthropic`, `Custom` |
| `BaseUrl` | API endpoint (auto-filled for named providers) |
| `Model` | e.g. `gpt-4o`, `claude-sonnet-4-20250514`, `llama3` |
| `MaxTokens` | Default max response tokens |
| `Temperature` | Sampling temperature (0.0 -- 2.0) |
| `EnableVoice` | Toggle voice streaming |
| `VoiceId` | TTS voice identifier |
| `EnableMockMode` | Canned responses without API calls |

### API Key Injection (Secure)

```csharp
IVXAIConfig.Instance.SetApiKey(SecureConfigService.GetKey("AI_API_KEY"));
```

For the managed IntelliVerseX provider, your GameId handles authentication automatically.

---

## The 7 Subsystems

### 1. Session Manager (`IVXAISessionManager`)

Manages conversation sessions with context windows.

```csharp
var session = await IVXAISessionManager.Instance.CreateSessionAsync(
    persona: "quiz_host",
    systemPrompt: "You are a witty trivia host."
);
string response = await session.SendAsync("Ask me a science question.");
```

### 2. NPC Dialog (`IVXAINPCDialogManager`)

Drives in-game NPC conversations with persona configs. History is maintained per-NPC.

```csharp
IVXAINPCDialogManager.Instance.RegisterNPC("prof_oak", new NPCDialogConfig {
    PersonaName = "Professor Oak",
    SystemPrompt = "You are a wise professor who gives hints about nature.",
    ContextWindowSize = 10,
    MaxResponseTokens = 150,
});
string reply = await IVXAINPCDialogManager.Instance.ChatAsync(
    "prof_oak", "What can you tell me about photosynthesis?"
);
```

### 3. Assistant (`IVXAIAssistant`)

General-purpose in-game help, hints, and tutorials.

```csharp
string hint = await IVXAIAssistant.Instance.GetHintAsync(
    context: "Player stuck on level 5, boss has fire weakness",
    style: HintStyle.Subtle
);
```

### 4. Moderator (`IVXAIModerator`)

Real-time content moderation for chat, usernames, and UGC.

```csharp
var result = await IVXAIModerator.Instance.ModerateAsync(chatMessage);
if (result.Flagged) ShowModerationWarning();
```

Categories: `Profanity`, `Harassment`, `HateSpeech`, `SexualContent`, `Violence`, `Spam`.

### 5. Content Generator (`IVXAIContentGenerator`)

Generates structured game content with JSON mode.

```csharp
var questions = await IVXAIContentGenerator.Instance.GenerateAsync<List<TriviaQuestion>>(
    prompt: "Generate 5 medium-difficulty science trivia questions",
    outputSchema: TriviaQuestion.JsonSchema
);
```

### 6. Profiler (`IVXAIProfiler`)

Builds player behavior profiles for personalization.

```csharp
IVXAIProfiler.Instance.TrackEvent("answer_correct", new { category = "science" });
var profile = await IVXAIProfiler.Instance.GetProfileAsync();
// profile.Strengths: ["science", "history"]
// profile.PlayStyle: "competitive"
```

### 7. Voice Services (`IVXAIVoiceServices`)

Real-time voice streaming for AI hosts and NPCs.

```csharp
var voiceSession = await IVXAIVoiceServices.Instance.StartVoiceSessionAsync(
    persona: "quiz_host", voiceId: "alloy"
);
voiceSession.OnAudioChunk += (audioData) =>
    IVXAIAudioPlayer.Instance.EnqueueChunk(audioData);
await voiceSession.SpeakAsync("Welcome to tonight's trivia challenge!");
```

Audio pipeline: `Text Input -> WebSocket -> TTS Server -> PCM Audio Chunks -> AudioSource`

---

## Provider Comparison

| Provider | Models | Voice | Cost Model |
|----------|--------|-------|-----------|
| IntelliVerseX (Managed) | GPT-4o, Claude, etc. | Yes | Per-token, billed to project |
| OpenAI | GPT-4o, o1, o3 | Yes (TTS API) | Your OpenAI billing |
| Azure OpenAI | Same as OpenAI | Yes | Your Azure billing |
| Anthropic | Claude Opus, Sonnet, Haiku | No (text only) | Your Anthropic billing |
| Custom | Ollama, vLLM, LiteLLM | Varies | Self-hosted |

### Self-Hosted Example (Ollama)

```csharp
IVXAIConfig.Instance.Provider = AIProvider.Custom;
IVXAIConfig.Instance.BaseUrl = "http://localhost:11434/v1";
IVXAIConfig.Instance.Model = "llama3:8b";
```

---

## Mock Mode

Enable `EnableMockMode` for testing without API costs:

```csharp
IVXAIMockProvider.Instance.RegisterResponse(
    pattern: "trivia",
    response: "{\"question\": \"What is H2O?\", \"answer\": \"Water\"}"
);
```

Use for: unit testing, offline dev, CI/CD pipelines, demo builds.

---

## Best Practices

1. **Never hardcode API keys** -- use `SetApiKey()` from a secure source
2. **Set token limits** -- prevent runaway costs with `MaxTokens`
3. **Use structured output** -- JSON mode for reliable parsing
4. **Cache NPC responses** -- identical prompts with same context can be cached locally
5. **Rate limit client calls** -- debounce player input
6. **Fall back gracefully** -- if AI is down, show pre-authored content
7. **Use mock mode in tests** -- never call real APIs from automated tests

---

## Architecture

```
Game Code -> IVXAISessionManager -> AI Provider (API call)
                  |
     ┌────────────┼────────────┐
     |            |            |
 NPC Dialog  Content Gen  Voice Services
     |            |            |
 Persona +    Structured   WebSocket +
 History      Output       Audio Player
```

---

## Completion Checklist

- [ ] `EnableAI` toggled on in `IVXBootstrapConfig`
- [ ] `IVXAIConfig` created with correct provider and model
- [ ] API key injected at runtime (not hardcoded)
- [ ] AI session creation and chat response verified
- [ ] Voice streaming tested (if using voice)
- [ ] Mock mode tested for offline/CI scenarios
- [ ] Token limits and rate limits configured
