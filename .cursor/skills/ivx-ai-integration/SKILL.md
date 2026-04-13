---
name: ivx-ai-integration
description: >-
  Integrate AI features into IntelliVerseX SDK games including AI voice host, NPC dialog,
  content generation, moderation, and profiling. Use when the user says "add AI host",
  "set up AI NPC", "AI voice chat", "generate content with AI", "configure LLM provider",
  "AI moderation", "OpenAI setup", "Azure AI", "Anthropic", "custom LLM", "AI profiler",
  or needs help with any AI integration.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX AI Integration

## Overview

The SDK's AI module provides seven subsystems for adding intelligence to games — from voice-driven hosts to real-time content moderation. All subsystems share a common provider abstraction so you can swap between managed (IntelliVerseX cloud), commercial (OpenAI, Azure, Anthropic), or self-hosted (Ollama, vLLM, LiteLLM) backends.

---

## When to Use

Ask your AI agent any of these:

* "Add an AI voice host for my trivia game"
* "Set up NPC dialog with GPT-4o"
* "Generate trivia questions using AI"
* "Add content moderation to player chat"
* "Configure Ollama as a self-hosted LLM provider"
* "Set up AI player profiling"
* "Add AI hints to my puzzle game"

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

## 1. Configuration

### IVXAIConfig ScriptableObject

Create via **Create > IntelliVerseX > AI Configuration**.

| Field | Description |
|-------|-------------|
| `Provider` | `IntelliVerseX`, `OpenAI`, `AzureOpenAI`, `Anthropic`, `Custom` |
| `BaseUrl` | API endpoint (auto-filled for named providers, required for Custom) |
| `Model` | Model identifier (e.g. `gpt-4o`, `claude-sonnet-4-20250514`, `llama3`) |
| `MaxTokens` | Default max response tokens |
| `Temperature` | Default sampling temperature (0.0–2.0) |
| `EnableVoice` | Toggle voice streaming subsystem |
| `VoiceId` | TTS voice identifier |
| `EnableMockMode` | Return canned responses without API calls (for testing) |

### API Key Injection (Secure)

**Never** hardcode API keys in config assets or source code.

```csharp
// Inject at runtime — typically from a secure config service or environment
IVXAIConfig.Instance.SetApiKey(SecureConfigService.GetKey("AI_API_KEY"));
```

For the **IntelliVerseX** managed provider, the SDK uses your `GameId` for authentication — no separate API key needed.

---

## 2. Provider Options

| Provider | Models | Voice | Cost |
|----------|--------|-------|------|
| **IntelliVerseX (Managed)** | Managed fleet (GPT-4o, Claude, etc.) | Yes | Per-token, billed to your project |
| **OpenAI** | GPT-4o, GPT-4o-mini, o1, o3 | Yes (TTS API) | Your OpenAI billing |
| **Azure OpenAI** | Same as OpenAI, deployed in your Azure tenant | Yes | Your Azure billing |
| **Anthropic** | Claude Opus, Sonnet, Haiku | No (text only) | Your Anthropic billing |
| **Custom** | Any OpenAI-compatible endpoint (Ollama, vLLM, LiteLLM) | Varies | Self-hosted |

### Self-Hosted Example (Ollama)

```csharp
IVXAIConfig.Instance.Provider = AIProvider.Custom;
IVXAIConfig.Instance.BaseUrl = "http://localhost:11434/v1"; // Ollama
IVXAIConfig.Instance.Model = "llama3:8b";
```

---

## 3. The Seven AI Subsystems

### 3.1 IVXAISessionManager

Manages conversation sessions with context windows.

```csharp
using IntelliVerseX.AI;

var session = await IVXAISessionManager.Instance.CreateSessionAsync(
    persona: "quiz_host",
    systemPrompt: "You are a witty trivia host."
);

string response = await session.SendAsync("Ask me a science question.");
```

### 3.2 IVXAINPCDialogManager

Drives in-game NPC conversations with persona configs.

```csharp
var npcConfig = new NPCDialogConfig
{
    PersonaName = "Professor Oak",
    SystemPrompt = "You are a wise professor who gives hints about nature.",
    ContextWindowSize = 10,
    MaxResponseTokens = 150,
};

IVXAINPCDialogManager.Instance.RegisterNPC("prof_oak", npcConfig);

string reply = await IVXAINPCDialogManager.Instance.ChatAsync(
    "prof_oak", "What can you tell me about photosynthesis?"
);
```

Conversation history is maintained per-NPC and automatically trimmed to the context window.

### 3.3 IVXAIAssistant

General-purpose assistant for in-game help, hints, and tutorials.

```csharp
string hint = await IVXAIAssistant.Instance.GetHintAsync(
    context: "Player stuck on level 5, boss has fire weakness",
    style: HintStyle.Subtle
);
```

### 3.4 IVXAIModerator

Real-time content moderation for chat, usernames, and user-generated content.

```csharp
var result = await IVXAIModerator.Instance.ModerateAsync(chatMessage);

if (result.Flagged)
{
    Debug.Log($"Message blocked: {result.Categories}");
    ShowModerationWarning();
}
```

Categories: `Profanity`, `Harassment`, `HateSpeech`, `SexualContent`, `Violence`, `Spam`.

### 3.5 IVXAIContentGenerator

Generates structured game content from prompts.

```csharp
var questions = await IVXAIContentGenerator.Instance.GenerateAsync<List<TriviaQuestion>>(
    prompt: "Generate 5 medium-difficulty science trivia questions",
    outputSchema: TriviaQuestion.JsonSchema
);
```

Supports structured output (JSON mode) for typed deserialization.

### 3.6 IVXAIProfiler

Builds player behavior profiles for personalization.

```csharp
IVXAIProfiler.Instance.TrackEvent("answer_correct", new { category = "science" });
IVXAIProfiler.Instance.TrackEvent("session_end", new { duration = 300 });

var profile = await IVXAIProfiler.Instance.GetProfileAsync();
// profile.Strengths: ["science", "history"]
// profile.WeakAreas: ["geography"]
// profile.PlayStyle: "competitive"
```

### 3.7 IVXAIVoiceServices

Voice streaming for AI hosts and NPCs.

```csharp
using IntelliVerseX.AI;

var voiceSession = await IVXAIVoiceServices.Instance.StartVoiceSessionAsync(
    persona: "quiz_host",
    voiceId: "alloy"
);

// Stream text and play audio in real-time
voiceSession.OnAudioChunk += (audioData) =>
{
    IVXAIAudioPlayer.Instance.EnqueueChunk(audioData);
};

await voiceSession.SpeakAsync("Welcome to tonight's trivia challenge!");
```

#### Audio Pipeline

```
Text Input → WebSocket → TTS Server → PCM Audio Chunks → IVXAIAudioPlayer → AudioSource
```

The `IVXAIAudioPlayer` manages buffering, crossfade, and lip-sync events.

---

## 4. Mock Mode

Enable `EnableMockMode` in `IVXAIConfig` to return deterministic canned responses. This is essential for:

- Unit testing without API costs
- Offline development
- CI/CD pipelines
- Demo builds

```csharp
// Register mock responses
IVXAIMockProvider.Instance.RegisterResponse(
    pattern: "trivia",
    response: "{\"question\": \"What is H2O?\", \"answer\": \"Water\"}"
);
```

---

## 5. Best Practices

1. **Never hardcode API keys** — use `SetApiKey()` at runtime from a secure source.
2. **Set token limits** — prevent runaway costs with `MaxTokens`.
3. **Use structured output** — request JSON mode for reliable parsing.
4. **Cache NPC responses** — identical prompts with same context can be cached locally.
5. **Rate limit client calls** — debounce player input to avoid excessive API calls.
6. **Fall back gracefully** — if the AI service is down, show pre-authored content.
7. **Use mock mode in tests** — never call real APIs from automated tests.

---

## 6. Architecture

```
┌────────────┐    ┌──────────────────┐    ┌──────────────┐
│  Game Code │ -> │  IVXAISession    │ -> │  AI Provider │
│            │    │  Manager         │    │  (API call)  │
└────────────┘    └──────────────────┘    └──────────────┘
                         │
          ┌──────────────┼──────────────┐
          │              │              │
     NPC Dialog    Content Gen    Voice Services
          │              │              │
     Persona +      Structured     WebSocket +
     History        Output         Audio Player
```

All subsystems share the same provider and session infrastructure.

---

## Platform-Specific AI Considerations

### VR Platforms (Meta Quest / SteamVR / Vision Pro)

| Feature | VR Notes |
|---------|----------|
| **Voice Host** | Use **spatial audio** — attach `AudioSource` to a world-space object. `IVXAIAudioPlayer` manages chunk playback; position it in the scene for immersion. |
| **NPC Dialog** | NPC dialog in VR should use gaze-triggered interaction. Wire `IVXXRInputAdapter.OnGazeTarget` to start dialog sessions with nearby NPCs. |
| **On-device vs Cloud** | Quest standalone has limited bandwidth. Consider caching NPC responses and using smaller models (`gpt-4o-mini`) for lower latency. Use mock mode for offline VR demos. |
| **Vision Pro** | Apple's visionOS encourages privacy — prefer on-device ML for moderation and profiling when possible. Voice TTS works via AVSpeechSynthesizer natively. |
| **PSVR2** | Sony's TRC requires disclosing AI-generated content. Add disclosure UI when showing AI responses. |

### Console Platforms (PS5 / Xbox / Switch)

| Consideration | Guidance |
|--------------|----------|
| **Network restrictions** | Console TRC/XR may require handling AI service unavailability gracefully. Always implement fallback to pre-authored content. |
| **API keys** | Console builds must store API keys in platform-secure storage, not in PlayerPrefs. Use the console platform's keychain/secret store. |
| **Content rating** | AI-generated text must comply with the game's content rating (ESRB/PEGI). Use `IVXAIModerator` to filter all AI output before display. |
| **Voice on Switch** | Nintendo Switch has limited microphone support. Voice input features should be disabled or optional on Switch. |

### WebGL / Browser

| Feature | WebGL Support |
|---------|--------------|
| **Voice streaming** | WebSocket TTS streaming works in browsers. `IVXAIWebSocketClient` has a WebGL-specific coroutine path. |
| **Audio recording** | `IVXAIAudioRecorder` is **disabled** on WebGL — browsers have limited mic access. Use text-only AI interaction. |
| **TTS playback** | Use the browser's Web Audio API or `SpeechSynthesis` API for client-side TTS as a fallback. |
| **API calls** | All AI provider calls go through HTTPS — no special WebGL restrictions beyond standard CORS. |

---

## Checklist

- [ ] `EnableAI` toggled on in `IVXBootstrapConfig`
- [ ] `IVXAIConfig` created with correct provider and model
- [ ] API key injected at runtime (not hardcoded)
- [ ] AI session creation and chat response verified
- [ ] Voice streaming tested (if using voice)
- [ ] Mock mode tested for offline/CI scenarios
- [ ] Token limits and rate limits configured
- [ ] Spatial audio positioned for VR AI host (if targeting VR)
- [ ] AI content disclosure added for console certification
- [ ] WebGL audio recording limitations handled
