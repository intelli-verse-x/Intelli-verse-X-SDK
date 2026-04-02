# AI Module

The AI module provides real-time voice personas and AI host commentary for games, backed by the IntelliVerseX AI service. It handles session management, audio streaming, entitlement gating, and player-context-aware personalisation out of the box.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.AI` |
| **Assembly** | `IntelliVerseX.AI` |
| **Dependencies** | `IntelliVerseX.Core`, Newtonsoft.Json |

The module exposes two core capabilities:

**Voice Personas** -- Interactive, bidirectional voice sessions where the player speaks (or types) to an AI character. The SDK streams audio, captions, and monetisation signals in real time.

**Host Commentary** -- A server-driven AI host that reacts to game events with text and audio commentary. Designed for quiz shows, trivia games, and any genre that benefits from dynamic narration.

Both capabilities share a single entry point (`IVXAISessionManager`) and are gated through a built-in entitlement system that supports free trials, session packs, and subscriptions.

---

## Setup

### 1. Create the configuration asset

In the Unity Editor menu bar:

**Assets > Create > IntelliVerseX > AI > Configuration**

This creates an `IVXAIConfig` ScriptableObject. Fill in the required fields:

| Field | Required | Notes |
|-------|----------|-------|
| API Base URL | Yes | Provided by IntelliVerseX (default included) |
| API Key | No | Only needed if not using bearer-token auth |

!!! note
    Leave the API Key blank if your game already authenticates through Nakama or another OAuth provider. Pass the bearer token at runtime via `SetAuthToken()` instead.

### 2. Create the session manager GameObject

Add an empty `GameObject` to your bootstrap scene and attach the `IVXAISessionManager` component. Assign the `IVXAIConfig` asset to the **Configuration** slot in the inspector.

Optionally assign an `AudioSource` to the **Audio** slot. If omitted, the manager creates one automatically.

!!! warning
    `IVXAISessionManager` is a singleton marked `DontDestroyOnLoad`. Only one instance should exist in your scene hierarchy.

### 3. Initialise at runtime

```csharp
using IntelliVerseX.AI;

public class GameBootstrap : MonoBehaviour
{
    void Start()
    {
        IVXAISessionManager.Instance.Initialize(
            userId:   "player_123",
            userName: "Alex",
            authToken: myBearerToken,   // optional
            language:  "en"             // optional, defaults to config
        );
    }
}
```

After `Initialize()` returns, `IVXAISessionManager.Instance.IsInitialized` is `true` and sessions can be started.

### 4. Update the auth token (optional)

If your backend refreshes tokens (e.g. Nakama session expiry), call:

```csharp
IVXAISessionManager.Instance.SetAuthToken(newToken);
```

---

## AI Voice Personas

Voice persona sessions allow players to have a live conversation with an AI character. The SDK handles transport negotiation (WebSocket with HTTP polling fallback), audio playback, and microphone recording.

### Fetching available personas

```csharp
IVXAISessionManager.Instance.GetPersonas(
    onSuccess: personas =>
    {
        foreach (var p in personas)
            Debug.Log($"{p.DisplayName} ({p.Id}) -- premium: {p.IsPremium}");
    },
    onError: err => Debug.LogError(err)
);
```

Each `IVXAIPersona` contains:

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Identifier sent to the API (e.g. `"FortuneTeller"`) |
| `DisplayName` | `string` | Human-readable label |
| `Description` | `string` | Short summary of the persona |
| `IsPremium` | `bool` | Whether a paid entitlement is required |
| `Tier` | `string` | Revenue tier label |
| `DefaultDurationSeconds` | `int` | Session length for free users |
| `PremiumDurationSeconds` | `int` | Session length for subscribers |

### Starting a voice session

```csharp
IVXAISessionManager.Instance.StartVoiceSession(
    personaId: "FortuneTeller",
    topic: "career",
    onSuccess: resp =>
    {
        Debug.Log($"Session {resp.SessionId} started, duration: {resp.DurationSeconds}s");
    },
    onError: err => Debug.LogError(err)
);
```

`StartVoiceSession` automatically performs an entitlement check. If the user lacks access, `OnEntitlementRequired` fires instead of creating the session.

To skip the entitlement check (when your game manages access control separately):

```csharp
IVXAISessionManager.Instance.StartVoiceSessionDirect("FortuneTeller", "career");
```

### Sending text

```csharp
IVXAISessionManager.Instance.SendText("Tell me about my future.");
```

### Recording and sending audio

```csharp
// Start capturing from the microphone
IVXAISessionManager.Instance.StartRecording();

// ... player speaks ...

// Stop recording -- captured PCM data is sent automatically
IVXAISessionManager.Instance.StopRecording();
```

To send raw PCM16 data directly:

```csharp
IVXAISessionManager.Instance.SendAudio(pcmBytes);
IVXAISessionManager.Instance.CommitAudio(); // signal end of speech
```

### Triggering AI speech

Force the AI to say something specific (useful for scripted moments):

```csharp
IVXAISessionManager.Instance.TriggerSpeech("Welcome back, brave adventurer!");
```

### Handling captions

Subscribe to caption events to display subtitles:

```csharp
var mgr = IVXAISessionManager.Instance;

mgr.OnCaptionReceived += partial =>
{
    subtitleText.text = partial; // streaming partial text
};

mgr.OnCaptionComplete += final =>
{
    subtitleText.text = final;   // final complete caption
};
```

### Stopping audio and ending the session

```csharp
// Immediately silence the AI
IVXAISessionManager.Instance.StopAudio();

// End the session and receive analytics
IVXAISessionManager.Instance.EndVoiceSession(analytics =>
{
    if (analytics != null)
        Debug.Log($"Duration: {analytics.DurationSeconds}s, Cost: ${analytics.EstimatedCost:F4}");
});
```

---

## AI Host Commentary

Host sessions provide server-driven commentary that reacts to game events. Unlike voice sessions, host sessions are primarily one-directional: the game sends events and the AI responds with text, audio, or action payloads.

### Starting a host session

```csharp
var request = new IVXAICreateHostSessionRequest
{
    GameMode       = "classic",
    PlayerCount    = 4,
    PlayerNames    = new[] { "Alex", "Jordan", "Sam", "Riley" },
    Topic          = "Science",
    Difficulty     = "medium",
    TotalQuestions = 10,
    TextOnlyMode   = false,
    PlayerProfiles = new[]
    {
        new IVXAIHostPlayerProfile
        {
            Name             = "Alex",
            TotalGamesPlayed = 42,
            OverallAccuracy  = 0.78f,
            IsVeteran        = true
        },
        // ... additional players
    }
};

IVXAISessionManager.Instance.StartHostSession(
    request,
    onSuccess: resp => Debug.Log($"Host session: {resp.SessionId}"),
    onError:   err  => Debug.LogError(err)
);
```

### Sending game events

Notify the AI host of game state changes so it can generate contextual commentary:

```csharp
IVXAISessionManager.Instance.SendHostGameEvent(
    eventType: "question_start",
    state:     "question_3_of_10",
    data:      "{\"category\":\"Physics\",\"difficulty\":\"hard\"}"
);
```

Common event types:

| Event Type | When to Send |
|------------|-------------|
| `question_start` | A new question is displayed |
| `question_end` | Time runs out or all players answered |
| `answer_reveal` | Correct answer is shown |
| `round_end` | A round or set of questions ends |
| `match_end` | The game is over |
| `streak` | A player hits a notable streak |
| `comeback` | A trailing player overtakes the leader |

### Submitting player answers

```csharp
IVXAISessionManager.Instance.SubmitHostAnswer(
    playerId:    "player_123",
    answerIndex: 2
);
```

### Sending player chat to the host

```csharp
IVXAISessionManager.Instance.SendHostText(
    playerId: "player_123",
    text:     "That was a tricky question!"
);
```

### Triggering host speech

```csharp
IVXAISessionManager.Instance.TriggerHostSpeech("And now, the final round!");
```

### Receiving host messages

```csharp
IVXAISessionManager.Instance.OnHostMessageReceived += msg =>
{
    // msg.Text   -- commentary text
    // msg.Audio  -- base64 audio (if not text-only mode)
    // msg.Action -- optional action hint (e.g. "celebrate", "taunt")
    DisplayCommentary(msg.Text);

    if (!string.IsNullOrEmpty(msg.Audio))
        PlayAudioFromBase64(msg.Audio);
};
```

### Ending the host session

```csharp
IVXAISessionManager.Instance.EndHostSession(() =>
{
    Debug.Log("Host session ended");
});
```

---

## Entitlements

The SDK includes a built-in entitlement system that gates AI access behind free trials and paid subscriptions. The `IVXAIEntitlementManager` is created internally by `IVXAISessionManager` and exposed via the `Entitlement` property.

### Access flow

```
StartVoiceSession()
    |
    v
CheckAccess(personaId)
    |
    +-- CanAccessPersona == true  --> create session
    |
    +-- CanAccessPersona == false --> OnEntitlementRequired fires
```

!!! info
    `StartVoiceSession` runs the entitlement check automatically. You only need to interact with `IVXAIEntitlementManager` directly if you want to build custom paywall UI or pre-check access.

### Checking access manually

```csharp
var ent = IVXAISessionManager.Instance.Entitlement;

ent.CheckAccess("FortuneTeller",
    onResult: resp =>
    {
        if (resp.CanAccessPersona)
            Debug.Log("Access granted");
        else
            Debug.Log($"Blocked: {resp.Reason}, free remaining: {resp.FreeSessionsRemaining}");
    }
);
```

### Querying cached state

```csharp
var ent = IVXAISessionManager.Instance.Entitlement;

bool subscribed     = ent.HasSubscription;
int  freeRemaining  = ent.FreeSessionsRemaining;
bool canStartFree   = ent.CanStartFreeSession();
```

### Fetching IAP products

```csharp
IVXAISessionManager.Instance.Entitlement.GetProducts(
    onResult: products =>
    {
        foreach (var p in products)
            Debug.Log($"{p.DisplayName} - {p.GetFormattedPrice()} ({p.Type})");
    }
);
```

Each `IVXAIProductInfo` contains:

| Property | Type | Description |
|----------|------|-------------|
| `ProductId` | `string` | Store product identifier |
| `DisplayName` | `string` | Display name |
| `Description` | `string` | Product description |
| `Price` | `float` | Price value |
| `Currency` | `string` | Currency code |
| `Type` | `string` | `"consumable"`, `"non_consumable"`, or `"subscription"` |
| `SessionsIncluded` | `int` | Number of sessions in a pack |
| `DurationDays` | `int` | Subscription duration |
| `IsPopular` | `bool` | Backend-flagged popular product |
| `DiscountPercent` | `int` | Active discount percentage |

### Submitting a purchase receipt

After a successful IAP transaction, validate the receipt server-side:

```csharp
IVXAISessionManager.Instance.Entitlement.SubmitPurchase(
    productId:   "ai_monthly_sub",
    receiptData: platformReceipt,
    onResult: resp =>
    {
        if (resp.Success)
            Debug.Log("Purchase validated, entitlement refreshed");
        else
            Debug.LogError($"Validation failed: {resp.Error}");
    }
);
```

The SDK automatically detects the platform (`ios`, `android`, `other`) and refreshes the cached entitlement state on success.

### Entitlement events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnEntitlementChanged` | `Action<IVXAIEntitlementResponse>` | Fired after any entitlement state change |
| `OnPaymentRequired` | `Action<string>` | Fired when access is denied (carries reason string) |

---

## Player Context

`IVXAIPlayerContext` lets you send rich player data to the AI backend so that responses are personalised. The AI can reference a player's skill level, streaks, topic strengths, and personality traits.

### Building a player context

```csharp
var ctx = new IVXAIPlayerContext
{
    PlayerId         = "player_123",
    DisplayName      = "Alex",
    FirstName        = "Alex",
    TotalGamesPlayed = 42,
    OverallAccuracy  = 0.78f,
    LongestStreak    = 12,
    CurrentStreak    = 3,
    AverageAnswerTime = 6.2f,
    BestScore        = 9500,
    StrongTopics     = new[] { "Science", "History" },
    WeakTopics       = new[] { "Sports" },
    FavoriteTopics   = new[] { "Science" }
};

// Auto-calculate personality flags from stats
ctx.CalculatePersonalityHints();

// Result: IsVeteran=false, IsCompetitive=true, IsSpeedDemon=false, etc.
```

### Personality auto-detection

`CalculatePersonalityHints()` sets boolean flags based on thresholds:

| Flag | Condition |
|------|-----------|
| `IsNewPlayer` | `TotalGamesPlayed < 5` |
| `IsVeteran` | `TotalGamesPlayed >= 100` |
| `IsCompetitive` | `OverallAccuracy >= 0.75` and `TotalGamesPlayed >= 20` |
| `IsCasual` | Not competitive and `TotalGamesPlayed >= 5` |
| `IsSpeedDemon` | `AverageAnswerTime > 0` and `< 5 seconds` |
| `IsAccuracyFocused` | `OverallAccuracy >= 0.85` |

### Generating a summary string

```csharp
string summary = ctx.GetPersonalitySummary();
// "competitive, accuracy-focused, expert in Science/History"
```

### Custom data

Attach arbitrary key-value pairs for custom server-side prompting:

```csharp
ctx.CustomData = new Dictionary<string, string>
{
    { "team", "Blue Dragons" },
    { "mood", "confident" }
};
```

### Match context (for host sessions)

`IVXAIMatchContext` wraps match-level data alongside player contexts:

```csharp
var match = new IVXAIMatchContext
{
    MatchId              = "match_456",
    GameMode             = "classic",
    Topic                = "Science",
    Difficulty           = "hard",
    TotalQuestions       = 10,
    CurrentQuestionIndex = 4,
    QuestionsRemaining   = 5,
    CurrentLeader        = "Alex",
    IsCloseMatch         = true,
    HasUnderdog          = true,
    Players              = new[] { ctx }
};

string contextString = match.GenerateContextString();
```

The generated context string can be passed as `MatchContext` when creating a host session.

---

## Events

### IVXAISessionManager Events

#### Voice session events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnSessionStarted` | `Action<IVXAICreateVoiceSessionResponse>` | Voice session successfully created |
| `OnSessionEnded` | `Action<IVXAISessionAnalytics>` | Voice session ended (carries analytics, may be `null` on error) |
| `OnCaptionReceived` | `Action<string>` | Partial caption text streaming in |
| `OnCaptionComplete` | `Action<string>` | Full caption text finalised |
| `OnAudioReceived` | `Action<string>` | Base64-encoded audio chunk received |
| `OnTurnComplete` | `Action` | AI finished its current turn |
| `OnSocialProofReceived` | `Action<IVXAISocialProofData>` | Social proof data received (active users, ratings) |
| `OnUpsellPrompt` | `Action<string>` | Upsell message for free-tier users |
| `OnScarcityMessage` | `Action<string>` | Scarcity/urgency message received |
| `OnSessionTimeWarning` | `Action` | Session time is running low |
| `OnError` | `Action<string>` | Error during session |
| `OnEntitlementRequired` | `Action<IVXAIEntitlementResponse>` | Entitlement check failed -- payment required |

#### Host session events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnHostSessionStarted` | `Action<IVXAICreateHostSessionResponse>` | Host session created |
| `OnHostMessageReceived` | `Action<IVXAIMessage>` | Host message received (text, audio, action) |
| `OnHostSessionEnded` | `Action` | Host session ended |

### IVXAIEntitlementManager Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnEntitlementChanged` | `Action<IVXAIEntitlementResponse>` | Entitlement state updated |
| `OnPaymentRequired` | `Action<string>` | Access denied, carries reason string |

---

## Configuration Reference

All properties on the `IVXAIConfig` ScriptableObject:

### API Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiBaseUrl` | `string` | `https://api.intelli-verse-x.ai/api/ai` | Base URL for the IVX AI API |
| `ApiKey` | `string` | `""` | API key (optional when using bearer-token auth) |

### Session Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PollingInterval` | `float` | `0.5` | Seconds between HTTP polls when WebSocket is unavailable (0.1 -- 2.0) |
| `RequestTimeout` | `float` | `30` | HTTP request timeout in seconds (5 -- 60) |
| `PreferWebSocket` | `bool` | `true` | Use WebSocket transport for real-time voice; auto-falls back to HTTP polling |
| `DebugLogging` | `bool` | `false` | Enable verbose debug logging to the Unity console |

### Audio Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AudioSampleRate` | `int` | `16000` | Playback sample rate in Hz (the IVX AI service outputs 16 kHz) |
| `AudioChannels` | `int` | `1` | Audio channels (1 = mono) |
| `AudioBufferSize` | `int` | `4096` | Buffer size in bytes for audio streaming |

### Language

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultLanguage` | `string` | `"en"` | Default language code (ISO 639-1) |
| `SupportedLanguages` | `string[]` | 18 languages | List of supported language codes |

!!! tip
    Supported languages out of the box: `en`, `es`, `fr`, `de`, `it`, `pt`, `ja`, `ko`, `zh`, `ar`, `hi`, `ru`, `nl`, `pl`, `tr`, `vi`, `th`, `id`. Custom languages can be added server-side.

### Free Trial

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FreeSessionsPerDay` | `int` | `1` | Free sessions allowed per day before requiring a purchase |
| `ShowUpsellDuringFreeSessions` | `bool` | `true` | Display an upsell prompt during free sessions |
| `UpsellSecondsBeforeEnd` | `int` | `15` | Seconds before session end to trigger the upsell |

### UI Hints

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowSocialProof` | `bool` | `true` | Surface social proof data (active users, ratings) |
| `ShowScarcityMessages` | `bool` | `true` | Surface scarcity/urgency messages |
| `ShowSessionTimer` | `bool` | `true` | Show remaining session time in voice UI |

---

## Code Examples

### Complete voice chat integration

```csharp
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

public class VoiceChatUI : MonoBehaviour
{
    [SerializeField] private Text _captionText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Button _talkButton;
    [SerializeField] private Button _endButton;
    [SerializeField] private InputField _textInput;
    [SerializeField] private Button _sendButton;

    private IVXAISessionManager _ai;
    private bool _isRecording;

    void Start()
    {
        _ai = IVXAISessionManager.Instance;

        _ai.OnCaptionReceived     += OnCaption;
        _ai.OnCaptionComplete     += OnCaptionFinal;
        _ai.OnTurnComplete        += OnTurnDone;
        _ai.OnSessionEnded        += OnSessionEnd;
        _ai.OnSessionTimeWarning  += OnTimeWarning;
        _ai.OnUpsellPrompt        += OnUpsell;
        _ai.OnEntitlementRequired += OnPaywall;
        _ai.OnError               += OnError;

        _talkButton.onClick.AddListener(ToggleRecording);
        _endButton.onClick.AddListener(EndSession);
        _sendButton.onClick.AddListener(SendTypedMessage);
    }

    public void StartSession(string personaId)
    {
        _ai.StartVoiceSession(personaId, topic: null,
            onSuccess: resp =>
            {
                Debug.Log($"Session started: {resp.SessionId}");
            },
            onError: err => Debug.LogError(err)
        );
    }

    void ToggleRecording()
    {
        if (_isRecording)
        {
            _ai.StopRecording();
            _isRecording = false;
        }
        else
        {
            _ai.StartRecording();
            _isRecording = true;
        }
    }

    void SendTypedMessage()
    {
        if (!string.IsNullOrEmpty(_textInput.text))
        {
            _ai.SendText(_textInput.text);
            _textInput.text = "";
        }
    }

    void EndSession()
    {
        _ai.EndVoiceSession();
    }

    void Update()
    {
        if (_ai.IsVoiceSessionActive && _ai.Config.ShowSessionTimer)
            _timerText.text = $"{Mathf.CeilToInt(_ai.RemainingVoiceTime)}s";
    }

    void OnCaption(string partial)         => _captionText.text = partial;
    void OnCaptionFinal(string final)      => _captionText.text = final;
    void OnTurnDone()                      => Debug.Log("AI finished speaking");
    void OnTimeWarning()                   => _timerText.color = Color.red;
    void OnUpsell(string message)          => ShowUpsellDialog(message);
    void OnError(string error)             => Debug.LogError($"AI Error: {error}");

    void OnSessionEnd(IVXAISessionAnalytics analytics)
    {
        if (analytics != null)
            Debug.Log($"Session lasted {analytics.DurationSeconds}s");
        _captionText.text = "";
        _timerText.text = "";
    }

    void OnPaywall(IVXAIEntitlementResponse ent)
    {
        Debug.Log($"Payment required: {ent.Reason}");
        ShowPaywallUI(ent.FreeSessionsRemaining);
    }

    void ShowUpsellDialog(string message) { /* show upgrade prompt */ }
    void ShowPaywallUI(int freeRemaining) { /* show paywall */ }

    void OnDestroy()
    {
        if (_ai == null) return;
        _ai.OnCaptionReceived     -= OnCaption;
        _ai.OnCaptionComplete     -= OnCaptionFinal;
        _ai.OnTurnComplete        -= OnTurnDone;
        _ai.OnSessionEnded        -= OnSessionEnd;
        _ai.OnSessionTimeWarning  -= OnTimeWarning;
        _ai.OnUpsellPrompt        -= OnUpsell;
        _ai.OnEntitlementRequired -= OnPaywall;
        _ai.OnError               -= OnError;
    }
}
```

### Complete AI host integration

```csharp
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

public class AIHostController : MonoBehaviour
{
    [SerializeField] private Text _commentaryText;

    private IVXAISessionManager _ai;

    void Start()
    {
        _ai = IVXAISessionManager.Instance;

        _ai.OnHostSessionStarted  += OnHostStarted;
        _ai.OnHostMessageReceived += OnHostMessage;
        _ai.OnHostSessionEnded    += OnHostEnded;
    }

    public void BeginHostedMatch(string[] playerNames, IVXAIHostPlayerProfile[] profiles)
    {
        var request = new IVXAICreateHostSessionRequest
        {
            GameMode       = "classic",
            PlayerCount    = playerNames.Length,
            PlayerNames    = playerNames,
            PlayerProfiles = profiles,
            Topic          = "General Knowledge",
            Difficulty     = "medium",
            TotalQuestions = 10,
            TextOnlyMode   = false
        };

        _ai.StartHostSession(request,
            onSuccess: resp => Debug.Log($"Host ready: {resp.SessionId}"),
            onError:   err  => Debug.LogError(err)
        );
    }

    public void OnQuestionDisplayed(int questionIndex, int totalQuestions, string category)
    {
        _ai.SendHostGameEvent(
            eventType: "question_start",
            state:     $"question_{questionIndex + 1}_of_{totalQuestions}",
            data:      $"{{\"category\":\"{category}\"}}"
        );
    }

    public void OnPlayerAnswered(string playerId, int answerIndex)
    {
        _ai.SubmitHostAnswer(playerId, answerIndex);
    }

    public void OnRoundComplete()
    {
        _ai.SendHostGameEvent("round_end", "scores_updated");
    }

    public void OnMatchComplete()
    {
        _ai.SendHostGameEvent("match_end", "final_scores");
        _ai.EndHostSession();
    }

    void OnHostStarted(IVXAICreateHostSessionResponse resp)
    {
        _commentaryText.text = "Your AI host is ready!";
    }

    void OnHostMessage(IVXAIMessage msg)
    {
        if (!string.IsNullOrEmpty(msg.Text))
            _commentaryText.text = msg.Text;
    }

    void OnHostEnded()
    {
        _commentaryText.text = "";
    }

    void OnDestroy()
    {
        if (_ai == null) return;
        _ai.OnHostSessionStarted  -= OnHostStarted;
        _ai.OnHostMessageReceived -= OnHostMessage;
        _ai.OnHostSessionEnded    -= OnHostEnded;
    }
}
```

### Entitlement pre-check with custom paywall

```csharp
using IntelliVerseX.AI;

public class PersonaSelector
{
    public void OnPersonaSelected(string personaId)
    {
        var ent = IVXAISessionManager.Instance.Entitlement;

        ent.CheckAccess(personaId,
            onResult: resp =>
            {
                if (resp.CanAccessPersona)
                {
                    IVXAISessionManager.Instance.StartVoiceSessionDirect(personaId);
                    return;
                }

                if (resp.HasSubscription)
                {
                    ShowMessage("Your subscription does not include this persona.");
                }
                else if (resp.FreeSessionsRemaining > 0)
                {
                    ShowMessage($"You have {resp.FreeSessionsRemaining} free sessions left today.");
                    IVXAISessionManager.Instance.StartVoiceSessionDirect(personaId);
                }
                else
                {
                    ShowPaywall(personaId);
                }
            },
            onError: err => ShowMessage($"Could not verify access: {err}")
        );
    }

    void ShowPaywall(string personaId)
    {
        var ent = IVXAISessionManager.Instance.Entitlement;

        ent.GetProducts(products =>
        {
            foreach (var p in products)
                Debug.Log($"{p.DisplayName}: {p.GetFormattedPrice()}");

            // Display product list in your paywall UI
        });
    }

    void ShowMessage(string msg) { /* display to player */ }
}
```

### Building player context from game data

```csharp
using IntelliVerseX.AI;
using System.Collections.Generic;

public static class PlayerContextBuilder
{
    public static IVXAIPlayerContext Build(
        string playerId,
        string displayName,
        int gamesPlayed,
        float accuracy,
        int longestStreak,
        float avgAnswerTime,
        int bestScore,
        string[] strongTopics,
        string[] weakTopics)
    {
        var ctx = new IVXAIPlayerContext
        {
            PlayerId          = playerId,
            DisplayName       = displayName,
            TotalGamesPlayed  = gamesPlayed,
            OverallAccuracy   = accuracy,
            LongestStreak     = longestStreak,
            AverageAnswerTime = avgAnswerTime,
            BestScore         = bestScore,
            StrongTopics      = strongTopics,
            WeakTopics        = weakTopics,
            CustomData        = new Dictionary<string, string>
            {
                { "region", "NA-West" },
                { "preferredDifficulty", "hard" }
            }
        };

        ctx.CalculatePersonalityHints();
        return ctx;
    }
}
```

---

## Data Models Reference

### IVXAISessionAnalytics

Returned when a voice session ends:

| Property | Type | Description |
|----------|------|-------------|
| `SessionId` | `string` | Session identifier |
| `UserId` | `string` | Player who owned the session |
| `Persona` | `string` | Persona used |
| `DurationSeconds` | `int` | Actual session duration |
| `CreditsUsed` | `int` | Backend credits consumed |
| `EstimatedCost` | `float` | Estimated cost in USD |
| `IsPremium` | `bool` | Whether the session was premium |
| `WasFreeTrial` | `bool` | Whether it was a free trial session |
| `CompletedSuccessfully` | `bool` | Clean completion vs. error/timeout |

### IVXAIEntitlementResponse

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string` | Player identifier |
| `HasSubscription` | `bool` | Active subscription |
| `SubscriptionExpiryDate` | `string` | Expiry date string |
| `FreeTrialUsed` | `bool` | Whether the free trial has been consumed |
| `FreeSessionsRemaining` | `int` | Free sessions left today |
| `TotalSessionsCompleted` | `int` | Lifetime completed sessions |
| `CanAccessPersona` | `bool` | Whether the requested persona is accessible |
| `Reason` | `string` | Human-readable denial reason (when blocked) |

### IVXAIMessage

Unified message type received from both voice and host sessions:

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string` | Raw message type string |
| `Text` | `string` | Text content |
| `Audio` | `string` | Base64-encoded audio |
| `Action` | `string` | Action hint (host sessions) |
| `Data` | `string` | Arbitrary JSON payload |
| `Error` | `string` | Error message (if type is `error`) |
| `SocialProof` | `IVXAISocialProofData` | Social proof payload |
| `ScarcityMessage` | `string` | Scarcity/urgency text |
| `UpsellMessage` | `string` | Upsell text |

### IVXAISocialProofData

| Property | Type | Description |
|----------|------|-------------|
| `ReadingsToday` | `int` | Sessions started today across all users |
| `ActiveUsers` | `int` | Currently active users |
| `AverageRating` | `float` | Average user rating |
| `TotalSessionsAllTime` | `int` | All-time session count |
| `HappyUsers` | `int` | Users who rated positively |

---

## Related Documentation

- [Core Module](core.md) -- Shared utilities and configuration
- [Backend Module](backend.md) -- Nakama integration (auth tokens for AI)
- [Monetization Module](monetization.md) -- IAP and ad integration
- [Analytics Module](analytics.md) -- Event tracking
