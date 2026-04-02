# AI & LLM Stack

## 1. Overview

**Every game that onboards IntelliVerseX gets a complete AI-powered game layer** — voice personas, AI host commentary, NPC dialog, in-game assistant, moderation, procedural content, player profiling, and standalone speech services — behind a single configuration surface (`IVXAIConfig`) and shared HTTP/WebSocket infrastructure.

Namespace: **`IntelliVerseX.AI`**.

!!! note "Backend contract"
    The SDK works with any OpenAI-compatible REST endpoint. Set `IVXAIConfig.Provider` to `IntelliVerseX`, `OpenAI`, `AzureOpenAI`, `Anthropic`, or `Custom` (Ollama, vLLM, LiteLLM, etc.). Point `ApiBaseUrl` at your deployed environment and supply auth via `SetAuthToken()` or `ApiKey`. See [AI Getting Started](../guides/ai-getting-started.md) for backend setup options.

---

## 2. Architecture (text diagram)

```
                    ┌─────────────────────────────────────────┐
                    │            IVXAIConfig                     │
                    │  (base URL, audio, language, trials)    │
                    └────────────────────┬──────────────────────┘
                                         │
         ┌───────────────────────────────┼───────────────────────────────┐
         │                               │                               │
         ▼                               ▼                               ▼
┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
│ IVXAISessionMgr │           │ IVXAIApiClient  │           │ IVXAIWebSocket  │
│ Voice + Host    │◄─────────│ REST (voice/    │           │ Realtime STT/   │
│ sessions        │           │ host/content/   │           │ voice stream    │
└────────┬────────┘           │ npc/assistant)   │           └────────┬────────┘
         │                    └─────────────────┘                    │
         │                              │                            │
         ▼                              ▼                            ▼
┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
│ IVXAINPCDialog  │           │ IVXAIAssistant  │           │ IVXAIVoiceServ. │
│ Manager         │           │ hints / KB       │           │ STT / TTS / list │
└────────┬────────┘           └────────┬────────┘           └────────┬────────┘
         │                              │                            │
         ▼                              ▼                            ▼
┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
│ IVXAIContentGen │           │ IVXAIModerator  │           │ IVXAIProfiler   │
│ quests, stories │           │ classify/filter│           │ events, cohort  │
└─────────────────┘           └─────────────────┘           └─────────────────┘
```

Shared pieces: **`IVXAIApiClient`**, **`IVXAIAudioPlayer`**, **`IVXAIAudioRecorder`**, **`IVXAIEntitlementManager`**, DTOs under `Models/`.

---

## 3. AI Voice Sessions (existing)

`IVXAISessionManager` orchestrates **voice personas** (WebSocket + HTTP fallback) and **AI host** sessions:

- Voice: create session, stream captions/audio, entitlements, social proof, upsell/scarcity hints, session timer
- Host: commentary aligned with match/player context
- **`SetPlayerContext(IVXAIPlayerContext)`** serializes player profiling into voice session requests
- **`OnSpeechDetected`**, **`OnSpeechStopped`** — backend-reported speech activity

=== "C#"

    ```csharp
    using IntelliVerseX.AI;

    IVXAISessionManager.Instance.Initialize(userId, userName, authToken, language);
    IVXAISessionManager.Instance.SetPlayerContext(playerContext);
    IVXAISessionManager.Instance.OnSpeechDetected += () => { /* show mic active */ };
    ```

=== "Concept"

    Bind `IVXAIConfig` in the Inspector; call `Initialize` after auth. Use `IVXAIEntitlementManager` for trials and IAP gating.

See also: [AI Voice & Host](ai.md).

---

## 4. AI NPC Dialog System (`IVXAINPCDialogManager`)

| Concern | API / model |
|---------|-------------|
| Profiles | `RegisterNPC` / `UnregisterNPC` with `IVXAINPCProfile` |
| Sessions | `StartDialog`, `SendMessage`, `EndDialog`, `GetSession`, `GetSessionsForNPC` |
| Branching & tools | Server returns `IVXAINPCAction` on `OnNPCAction`; dialog state in `IVXAINPCDialogSession` |
| Memory | Conversation `History` on sessions + optional `playerContext` string on `StartDialog` |

Events: `OnNPCResponse`, `OnNPCAction`, `OnDialogStarted`, `OnDialogEnded`, `OnError`.

```csharp
npcMgr.Initialize(config);
npcMgr.RegisterNPC(profile);
npcMgr.StartDialog(npcId, playerId, playerContextJson, session => { });
npcMgr.SendMessage(sessionId, "Hello!", response => { });
```

---

## 5. AI In-Game Assistant (`IVXAIAssistant`)

Grounded Q&A and guided help using `IVXAIGameContext` (level, objective, phase, inventory, stats, custom JSON).

| Method | Purpose |
|--------|---------|
| `Ask` | General question + optional game context → `IVXAIAssistantResponse` |
| `GetHint` | Contextual hint → `IVXAIHintResponse` |
| `GetTutorial` | Tutorial flow → `IVXAITutorialResponse` |
| `SearchKnowledgeBase` | Document id / snippet results |
| `SetSystemPrompt` | Optional persona/instructions |
| `ClearHistory` | Reset assistant thread |

```csharp
assistant.Initialize(config);
assistant.Ask("How do I beat this boss?", new IVXAIGameContext { CurrentLevel = "3-2" }, r => Debug.Log(r.Response));
```

---

## 6. AI Chat Moderation (`IVXAIModerator`)

| Feature | Detail |
|---------|--------|
| Classification | `IVXContentCategory`, `IVXModerationSeverity`, `IVXModerationActionType` |
| Pipeline | `ClassifyText`, `FilterMessage`, `ScanBatch` |
| Custom rules | `IVXModerationRule` — `AddCustomRule`, `SetCustomRules`, `ClearCustomRules` |
| Discord | `GetDiscordModerationMetadata(IVXModerationResult)` → key/value map for `IVXDiscordModeration.ProcessModerationMetadata` |

```csharp
moderator.Initialize(config);
moderator.ClassifyText(chatLine, result => {
    if (result.SuggestedAction == IVXModerationActionType.Block) RejectMessage();
});
```

---

## 7. AI Content Generation (`IVXAIContentGenerator`)

| Type | Method |
|------|--------|
| Quests | `GenerateQuest(IVXQuestTemplate, playerContext, ...)` |
| Stories | `GenerateStory(prompt, genre, maxWords, ...)` |
| Items | `GenerateItemDescription(name, type, rarity, ...)` |
| Dialogue | `GenerateDialogue(scenario, characters, ...)` |
| Templates | `GenerateFromTemplate(template, variables, ...)` |

Events: `OnContentGenerated`, `OnStreamingChunk`, `OnError`. Use `CancelGeneration` to abort in-flight HTTP.

---

## 8. Player Behavior Profiling (`IVXAIProfiler`)

| Capability | API |
|------------|-----|
| Events | `TrackEvent`, `FlushEvents` |
| Profile | `GetPlayerProfile` → `IVXPlayerProfile` (cohort, engagement, churn risk, monetization, modes, features) |
| Personalization | `GetPersonalizationHints` |
| Cohort | `ClassifyPlayer` → `IVXPlayerCohort` |
| Churn | `PredictChurn` |
| Automation | `StartAutoTracking` / `StopAutoTracking` |

```csharp
IVXAIProfiler.Instance.Initialize(config, playerId);
IVXAIProfiler.Instance.TrackEvent("match_end", new Dictionary<string, object> { ["score"] = 1200 });
```

---

## 9. Voice AI Services (`IVXAIVoiceServices`)

Standalone **STT**, **TTS**, **voice listing**, **language detection**, and **streaming** transcription (WebSocket) — usable without a full voice persona session.

| Area | Entry points |
|------|--------------|
| STT | `TranscribeAudio(pcm, sampleRate, ...)` |
| TTS | `SynthesizeSpeech(text, voiceId, ...)` → PCM16; `OnSpeechSynthesized` |
| Voices | `ListVoices` → `AvailableVoices` / `IVXAIVoice` |
| Language | Detect language from audio (HTTP helper on service) |
| Streaming | Start/stop streaming STT APIs; `OnPartialTranscription`, `OnTranscriptionResult`, `IsTranscribing` |

Initialize with `IVXAIConfig` before use.

---

## 10. Configuration (`IVXAIConfig`)

| Group | Fields |
|-------|--------|
| API | `ApiBaseUrl`, `ApiKey` |
| Session | `PollingInterval`, `RequestTimeout`, `PreferWebSocket`, `DebugLogging` |
| Audio | `AudioSampleRate`, `AudioChannels`, `AudioBufferSize` |
| Language | `DefaultLanguage`, `SupportedLanguages` |
| Trial / monetization UI | `FreeSessionsPerDay`, `ShowUpsellDuringFreeSessions`, `UpsellSecondsBeforeEnd` |
| UI hints | `ShowSocialProof`, `ShowScarcityMessages`, `ShowSessionTimer` |

Helpers: `VoiceEndpoint`, `HostEndpoint`, `WebSocketUrl`, per-session URL builders, `Validate`, `IsLanguageSupported`.

---

## 11. API Reference — Key Classes

| Class | Role |
|-------|------|
| `IVXAIConfig` | Central AI configuration |
| `IVXAISessionManager` | Voice + host sessions, player context, speech events |
| `IVXAINPCDialogManager` | NPC profiles and dialog HTTP API |
| `IVXAIAssistant` | Assistant, hints, tutorials, KB search |
| `IVXAIModerator` | Classification, filtering, rules, Discord metadata bridge |
| `IVXAIContentGenerator` | Quest/story/item/dialogue/template generation |
| `IVXAIProfiler` | Analytics, cohort, churn, personalization |
| `IVXAIVoiceServices` | STT/TTS/voices/streaming |
| `IVXAIApiClient` | Shared authenticated HTTP |
| `IVXAIWebSocketClient` | Realtime streaming |
| `IVXAIAudioPlayer` / `IVXAIAudioRecorder` | PCM16 playback and capture |
| `IVXAIEntitlementManager` | AI SKU / trial gating |

---

## Related

- [AI Getting Started Guide](../guides/ai-getting-started.md) — Quickstart for AI features
- [AI Voice & Host — Full Reference](ai.md) — Complete voice session and host API
- [Discord Social SDK](discord.md) — Moderation metadata bridge from `IVXAIModerator`
