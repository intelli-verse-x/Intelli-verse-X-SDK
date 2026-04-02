# AI Voice & Host API Reference

Complete API reference for the AI module covering voice personas, host commentary, entitlements, and player context.

---

## IVXAISessionManager

Central singleton for AI voice persona sessions and host commentary sessions.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXAISessionManager` | Singleton instance |
| `IsInitialized` | `bool` | Whether the manager has been initialized |
| `IsVoiceSessionActive` | `bool` | Whether a voice session is currently running |
| `IsHostSessionActive` | `bool` | Whether a host session is currently running |
| `RemainingVoiceTime` | `float` | Seconds remaining in the active voice session |
| `Config` | `IVXAIConfig` | Configuration asset reference |
| `Entitlement` | `IVXAIEntitlementManager` | Entitlement sub-manager |

### Methods

#### Initialize

```csharp
public void Initialize(string userId, string userName, string authToken = null, string language = null)
```

Initializes the AI session manager with player identity and optional auth.

**Parameters:**
- `userId` - Player identifier
- `userName` - Player display name
- `authToken` - Bearer token for API auth (optional if using API key)
- `language` - ISO 639-1 language code (optional, defaults to config value)

**Example:**
```csharp
IVXAISessionManager.Instance.Initialize("player_123", "Alex", myBearerToken, "en");
```

---

#### SetAuthToken

```csharp
public void SetAuthToken(string newToken)
```

Updates the bearer token after a backend refresh.

**Parameters:**
- `newToken` - New bearer token string

---

#### GetPersonas

```csharp
public void GetPersonas(Action<List<IVXAIPersona>> onSuccess, Action<string> onError = null)
```

Fetches the list of available AI personas from the backend.

**Parameters:**
- `onSuccess` - Callback with persona list
- `onError` - Error callback

**Example:**
```csharp
IVXAISessionManager.Instance.GetPersonas(
    personas => { foreach (var p in personas) Debug.Log(p.DisplayName); },
    err => Debug.LogError(err));
```

---

#### StartVoiceSession

```csharp
public void StartVoiceSession(string personaId, string topic = null, Action<IVXAICreateVoiceSessionResponse> onSuccess = null, Action<string> onError = null)
```

Starts a voice session with entitlement check. If access is denied, `OnEntitlementRequired` fires instead.

**Parameters:**
- `personaId` - Persona identifier (e.g. `"FortuneTeller"`)
- `topic` - Conversation topic (optional)
- `onSuccess` - Success callback with session response
- `onError` - Error callback

**Example:**
```csharp
IVXAISessionManager.Instance.StartVoiceSession("FortuneTeller", "career",
    resp => Debug.Log($"Session {resp.SessionId} started"),
    err => Debug.LogError(err));
```

---

#### StartVoiceSessionDirect

```csharp
public void StartVoiceSessionDirect(string personaId, string topic = null)
```

Starts a voice session without the entitlement check. Use when your game manages access control separately.

**Parameters:**
- `personaId` - Persona identifier
- `topic` - Conversation topic (optional)

---

#### SendText

```csharp
public void SendText(string text)
```

Sends a text message to the active voice session.

**Parameters:**
- `text` - Message text

**Example:**
```csharp
IVXAISessionManager.Instance.SendText("Tell me about my future.");
```

---

#### StartRecording

```csharp
public void StartRecording()
```

Begins capturing audio from the microphone for the active voice session.

---

#### StopRecording

```csharp
public void StopRecording()
```

Stops microphone recording and sends the captured audio automatically.

---

#### SendAudio

```csharp
public void SendAudio(byte[] pcmBytes)
```

Sends raw PCM16 audio data to the active voice session.

**Parameters:**
- `pcmBytes` - Raw PCM16 audio bytes

---

#### CommitAudio

```csharp
public void CommitAudio()
```

Signals the end of a speech segment after `SendAudio` calls.

---

#### TriggerSpeech

```csharp
public void TriggerSpeech(string text)
```

Forces the AI to speak specific text during a voice session.

**Parameters:**
- `text` - Text for the AI to speak

---

#### StopAudio

```csharp
public void StopAudio()
```

Immediately silences AI audio playback.

---

#### EndVoiceSession

```csharp
public void EndVoiceSession(Action<IVXAISessionAnalytics> onComplete = null)
```

Ends the active voice session and returns analytics.

**Parameters:**
- `onComplete` - Callback with session analytics (may be `null` on error)

**Example:**
```csharp
IVXAISessionManager.Instance.EndVoiceSession(analytics =>
{
    if (analytics != null)
        Debug.Log($"Duration: {analytics.DurationSeconds}s");
});
```

---

#### StartHostSession

```csharp
public void StartHostSession(IVXAICreateHostSessionRequest request, Action<IVXAICreateHostSessionResponse> onSuccess = null, Action<string> onError = null)
```

Creates an AI host commentary session for game events.

**Parameters:**
- `request` - Host session configuration
- `onSuccess` - Success callback with session response
- `onError` - Error callback

**Example:**
```csharp
var request = new IVXAICreateHostSessionRequest
{
    GameMode = "classic",
    PlayerCount = 4,
    PlayerNames = new[] { "Alex", "Jordan" },
    Topic = "Science",
    Difficulty = "medium",
    TotalQuestions = 10,
    TextOnlyMode = false
};
IVXAISessionManager.Instance.StartHostSession(request,
    resp => Debug.Log($"Host session: {resp.SessionId}"),
    err => Debug.LogError(err));
```

---

#### SendHostGameEvent

```csharp
public void SendHostGameEvent(string eventType, string state, string data = null)
```

Notifies the AI host of a game state change to trigger contextual commentary.

**Parameters:**
- `eventType` - Event type (`"question_start"`, `"round_end"`, `"match_end"`, etc.)
- `state` - State descriptor (e.g. `"question_3_of_10"`)
- `data` - Optional JSON payload with additional context

---

#### SubmitHostAnswer

```csharp
public void SubmitHostAnswer(string playerId, int answerIndex)
```

Submits a player's answer to the host session for commentary.

**Parameters:**
- `playerId` - Player identifier
- `answerIndex` - Index of the selected answer

---

#### SendHostText

```csharp
public void SendHostText(string playerId, string text)
```

Sends player chat text to the AI host.

**Parameters:**
- `playerId` - Player identifier
- `text` - Chat message text

---

#### TriggerHostSpeech

```csharp
public void TriggerHostSpeech(string text)
```

Forces the AI host to speak specific text.

**Parameters:**
- `text` - Text for the host to speak

---

#### EndHostSession

```csharp
public void EndHostSession(Action onComplete = null)
```

Ends the active host session.

**Parameters:**
- `onComplete` - Completion callback

---

### Events

#### Voice Session Events

| Event | Type | Description |
|-------|------|-------------|
| `OnSessionStarted` | `Action<IVXAICreateVoiceSessionResponse>` | Voice session created |
| `OnSessionEnded` | `Action<IVXAISessionAnalytics>` | Voice session ended with analytics |
| `OnCaptionReceived` | `Action<string>` | Partial caption text streaming |
| `OnCaptionComplete` | `Action<string>` | Final caption text |
| `OnAudioReceived` | `Action<string>` | Base64-encoded audio chunk |
| `OnTurnComplete` | `Action` | AI finished speaking |
| `OnSocialProofReceived` | `Action<IVXAISocialProofData>` | Social proof data received |
| `OnUpsellPrompt` | `Action<string>` | Upsell message for free-tier users |
| `OnScarcityMessage` | `Action<string>` | Scarcity/urgency message |
| `OnSessionTimeWarning` | `Action` | Session time running low |
| `OnError` | `Action<string>` | Error during session |
| `OnEntitlementRequired` | `Action<IVXAIEntitlementResponse>` | Entitlement check failed |

#### Host Session Events

| Event | Type | Description |
|-------|------|-------------|
| `OnHostSessionStarted` | `Action<IVXAICreateHostSessionResponse>` | Host session created |
| `OnHostMessageReceived` | `Action<IVXAIMessage>` | Host commentary received (text, audio, action) |
| `OnHostSessionEnded` | `Action` | Host session ended |

---

## IVXAIEntitlementManager

Manages access control, free trials, subscriptions, and IAP product queries. Created internally by `IVXAISessionManager` and accessible via the `Entitlement` property.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `HasSubscription` | `bool` | Whether the player has an active subscription |
| `FreeSessionsRemaining` | `int` | Free sessions left today |

### Methods

#### CheckAccess

```csharp
public void CheckAccess(string personaId, Action<IVXAIEntitlementResponse> onResult, Action<string> onError = null)
```

Checks whether the player can access a specific persona.

**Parameters:**
- `personaId` - Persona to check
- `onResult` - Result callback with entitlement response
- `onError` - Error callback

**Example:**
```csharp
IVXAISessionManager.Instance.Entitlement.CheckAccess("FortuneTeller",
    resp => Debug.Log($"Can access: {resp.CanAccessPersona}"));
```

---

#### CanStartFreeSession

```csharp
public bool CanStartFreeSession()
```

Returns whether a free session is available today.

**Returns:** `true` if free sessions remain

---

#### GetProducts

```csharp
public void GetProducts(Action<List<IVXAIProductInfo>> onResult)
```

Fetches available IAP products for AI access.

**Parameters:**
- `onResult` - Callback with product list

---

#### SubmitPurchase

```csharp
public void SubmitPurchase(string productId, string receiptData, Action<IVXAIPurchaseResponse> onResult)
```

Validates an IAP receipt server-side and refreshes entitlements.

**Parameters:**
- `productId` - Store product identifier
- `receiptData` - Platform-specific receipt data
- `onResult` - Result callback

---

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnEntitlementChanged` | `Action<IVXAIEntitlementResponse>` | Entitlement state updated |
| `OnPaymentRequired` | `Action<string>` | Access denied with reason string |

---

## IVXAIConfig

ScriptableObject holding AI module configuration.

### API Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiBaseUrl` | `string` | `https://api.intelli-verse-x.ai/api/ai` | Base URL for the IVX AI API |
| `ApiKey` | `string` | `""` | API key (optional with bearer-token auth) |

### Session Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PollingInterval` | `float` | `0.5` | Seconds between HTTP polls (0.1--2.0) |
| `RequestTimeout` | `float` | `30` | HTTP request timeout in seconds (5--60) |
| `PreferWebSocket` | `bool` | `true` | Prefer WebSocket transport |
| `DebugLogging` | `bool` | `false` | Enable verbose debug logging |

### Audio Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AudioSampleRate` | `int` | `16000` | Playback sample rate in Hz |
| `AudioChannels` | `int` | `1` | Audio channels (1 = mono) |
| `AudioBufferSize` | `int` | `4096` | Buffer size in bytes |

### Language

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultLanguage` | `string` | `"en"` | Default language code (ISO 639-1) |
| `SupportedLanguages` | `string[]` | 18 languages | Supported language codes |

### Free Trial

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FreeSessionsPerDay` | `int` | `1` | Free sessions per day |
| `ShowUpsellDuringFreeSessions` | `bool` | `true` | Show upsell during free sessions |
| `UpsellSecondsBeforeEnd` | `int` | `15` | Seconds before end to trigger upsell |

### UI Hints

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowSocialProof` | `bool` | `true` | Surface social proof data |
| `ShowScarcityMessages` | `bool` | `true` | Surface scarcity messages |
| `ShowSessionTimer` | `bool` | `true` | Show remaining session time |

---

## Data Models

### IVXAIPersona

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Persona identifier |
| `DisplayName` | `string` | Human-readable label |
| `Description` | `string` | Short summary |
| `IsPremium` | `bool` | Requires paid entitlement |
| `Tier` | `string` | Revenue tier label |
| `DefaultDurationSeconds` | `int` | Session length for free users |
| `PremiumDurationSeconds` | `int` | Session length for subscribers |

### IVXAISessionAnalytics

| Property | Type | Description |
|----------|------|-------------|
| `SessionId` | `string` | Session identifier |
| `UserId` | `string` | Player who owned the session |
| `Persona` | `string` | Persona used |
| `DurationSeconds` | `int` | Actual session duration |
| `CreditsUsed` | `int` | Backend credits consumed |
| `EstimatedCost` | `float` | Estimated cost in USD |
| `IsPremium` | `bool` | Whether the session was premium |
| `WasFreeTrial` | `bool` | Whether it was a free trial |
| `CompletedSuccessfully` | `bool` | Clean completion vs. error/timeout |

### IVXAIEntitlementResponse

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string` | Player identifier |
| `HasSubscription` | `bool` | Active subscription |
| `SubscriptionExpiryDate` | `string` | Expiry date string |
| `FreeTrialUsed` | `bool` | Whether free trial was consumed |
| `FreeSessionsRemaining` | `int` | Free sessions left today |
| `TotalSessionsCompleted` | `int` | Lifetime completed sessions |
| `CanAccessPersona` | `bool` | Whether the requested persona is accessible |
| `Reason` | `string` | Human-readable denial reason |

### IVXAIMessage

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

### IVXAIProductInfo

| Property | Type | Description |
|----------|------|-------------|
| `ProductId` | `string` | Store product identifier |
| `DisplayName` | `string` | Display name |
| `Description` | `string` | Product description |
| `Price` | `float` | Price value |
| `Currency` | `string` | Currency code |
| `Type` | `string` | `"consumable"`, `"non_consumable"`, or `"subscription"` |
| `SessionsIncluded` | `int` | Sessions in a pack |
| `DurationDays` | `int` | Subscription duration |
| `IsPopular` | `bool` | Backend-flagged popular product |
| `DiscountPercent` | `int` | Active discount percentage |

### IVXAIPlayerContext

| Property | Type | Description |
|----------|------|-------------|
| `PlayerId` | `string` | Player identifier |
| `DisplayName` | `string` | Display name |
| `FirstName` | `string` | First name |
| `TotalGamesPlayed` | `int` | Lifetime games played |
| `OverallAccuracy` | `float` | Accuracy ratio (0.0--1.0) |
| `LongestStreak` | `int` | Best streak ever |
| `CurrentStreak` | `int` | Active streak |
| `AverageAnswerTime` | `float` | Seconds per answer |
| `BestScore` | `int` | Personal best score |
| `StrongTopics` | `string[]` | Strong topic categories |
| `WeakTopics` | `string[]` | Weak topic categories |
| `FavoriteTopics` | `string[]` | Preferred topic categories |
| `CustomData` | `Dictionary<string, string>` | Arbitrary key-value pairs |

#### Methods

```csharp
public void CalculatePersonalityHints()
```

Auto-detects personality flags (`IsNewPlayer`, `IsVeteran`, `IsCompetitive`, `IsCasual`, `IsSpeedDemon`, `IsAccuracyFocused`) from stats.

```csharp
public string GetPersonalitySummary()
```

Returns a human-readable personality summary string.

### IVXAICreateHostSessionRequest

| Property | Type | Description |
|----------|------|-------------|
| `GameMode` | `string` | Game mode identifier |
| `PlayerCount` | `int` | Number of players |
| `PlayerNames` | `string[]` | Player display names |
| `Topic` | `string` | Session topic |
| `Difficulty` | `string` | Difficulty level |
| `TotalQuestions` | `int` | Total questions in the session |
| `TextOnlyMode` | `bool` | Disable audio responses |
| `PlayerProfiles` | `IVXAIHostPlayerProfile[]` | Player profile data |

### IVXAIMatchContext

| Property | Type | Description |
|----------|------|-------------|
| `MatchId` | `string` | Match identifier |
| `GameMode` | `string` | Game mode |
| `Topic` | `string` | Match topic |
| `Difficulty` | `string` | Difficulty level |
| `TotalQuestions` | `int` | Total questions |
| `CurrentQuestionIndex` | `int` | Current question index |
| `QuestionsRemaining` | `int` | Remaining questions |
| `CurrentLeader` | `string` | Current leader's name |
| `IsCloseMatch` | `bool` | Whether the match is close |
| `HasUnderdog` | `bool` | Whether there is an underdog |
| `Players` | `IVXAIPlayerContext[]` | Player contexts |

#### Methods

```csharp
public string GenerateContextString()
```

Generates a formatted context string for the AI host session.

---

## See Also

- [AI Module Guide](../modules/ai.md)
- [Core Module](../modules/core.md)
- [Monetization Module](../modules/monetization.md)
