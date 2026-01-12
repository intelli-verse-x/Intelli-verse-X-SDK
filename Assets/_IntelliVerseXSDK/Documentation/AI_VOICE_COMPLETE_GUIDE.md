# AI Voice Integration - Complete Guide

> **IntelliVerse-X SDK** | Voice AI Conversations with Revenue Optimization
> 
> Version: 1.0 | Last Updated: January 2026

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Quick Start (5 minutes)](#quick-start)
3. [Architecture](#architecture)
4. [API Reference](#api-reference)
5. [Persona Guide](#persona-guide)
6. [IAP Configuration](#iap-configuration)
7. [UI Components](#ui-components)
8. [Revenue Optimization](#revenue-optimization)
9. [Example Implementations](#example-implementations)
10. [Troubleshooting](#troubleshooting)

---

## Overview

AI Voice enables real-time voice conversations with AI personas. Features include:

- 🎭 **10 AI Personas** - Fortune Teller, Teacher, Coach, Host, and more
- 💰 **Revenue Optimization** - Built-in IAP, subscriptions, social proof, scarcity
- 🎤 **Real-time Voice** - Powered by xAI Grok realtime API
- 📱 **Easy Integration** - Drop-in components, minimal coding

### Revenue Potential

| MAU | Conversion | ARPU | Monthly Revenue |
|-----|-----------|------|-----------------|
| 10K | 3% | $8 | $2,400 |
| 50K | 4% | $10 | $20,000 |
| 100K | 5% | $12 | $60,000 |

---

## Quick Start

### Step 1: Create Configuration Asset

```
Assets > Create > QuizVerse > AI Voice > Configuration
```

Configure your `AIVoiceConfig`:
- **API Base URL**: Your backend URL (e.g., `https://api.your-backend.com/api/ai`)
- **Polling Interval**: 0.5 seconds (recommended)
- **Free Sessions Per Day**: 1 (for trial hook)

### Step 2: Add Managers to Scene

1. Create empty GameObject called `AIVoiceSystem`
2. Add components:
   - `AIVoiceManager`
   - `AIVoiceIAPManager`
3. Assign your `AIVoiceConfig` to both

### Step 3: Initialize on App Start

```csharp
using Trivia.AIVoice;

public class GameBootstrap : MonoBehaviour
{
    void Start()
    {
        // Initialize with user info
        AIVoiceManager.Instance.Initialize(
            userId: "user_123",      // Your user ID
            userName: "John",        // Display name (AI will use this)
            authToken: null,         // OAuth token if using auth
            language: "en"           // Language code
        );
        
        // Fetch available products
        AIVoiceIAPManager.Instance.FetchProducts();
    }
}
```

### Step 4: Start a Session

```csharp
using Trivia.AIVoice;

public class FortuneButton : MonoBehaviour
{
    public void OnFortuneTellerClicked()
    {
        AIVoiceManager.Instance.CheckAndStartSession(
            persona: AIPersonaType.FortuneTeller,
            topic: "Will I find love this year?",
            onSuccess: (response) => {
                Debug.Log($"Session started: {response.sessionId}");
                // UI will update automatically via events
            },
            onNeedsPayment: (reason) => {
                // Show your paywall UI
                ShowPaywall(AIPersonaType.FortuneTeller);
            },
            onError: (error) => {
                Debug.LogError(error);
            }
        );
    }
}
```

That's it! The AI will start speaking automatically.

---

## Architecture

### File Structure

```
Assets/_QuizVerse/Scripts/AIVoice/
├── Core/
│   ├── AIVoiceEnums.cs        # All enums (PersonaType, Status, etc.)
│   ├── AIVoiceModels.cs       # Data models (requests, responses)
│   └── AIVoiceConfig.cs       # ScriptableObject configuration
├── Managers/
│   ├── AIVoiceManager.cs      # Main API client & session management
│   └── AIVoiceIAPManager.cs   # IAP product & purchase handling
├── UI/
│   ├── AIVoicePaywallUI.cs    # Paywall with revenue features
│   ├── AIVoiceSessionUI.cs    # Active session display
│   └── AIVoiceProductItemUI.cs # Individual product in paywall
└── Personas/
    ├── FortuneTellerController.cs   # Fortune Teller implementation
    ├── AITeacherController.cs       # AI Teacher implementation
    └── GenericAIPersonaController.cs # Works with any persona
```

### Data Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                         YOUR UNITY APP                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   ┌──────────────────┐      ┌──────────────────┐                   │
│   │ Your UI/Button   │─────▶│ CheckAndStart    │                   │
│   │ "Talk to AI"     │      │ Session()        │                   │
│   └──────────────────┘      └────────┬─────────┘                   │
│                                      │                              │
│                    ┌─────────────────┼─────────────────┐           │
│                    ▼                 ▼                 ▼           │
│   ┌──────────────────┐   ┌──────────────────┐   ┌──────────────┐  │
│   │ HasAccess?       │   │ StartSession()   │   │ ShowPaywall  │  │
│   │ (IAP/FreeTrial)  │   │ → API Call       │   │ (Revenue)    │  │
│   └──────────────────┘   └────────┬─────────┘   └──────────────┘  │
│                                   │                                 │
│                    ┌──────────────┴────────────────┐               │
│                    │     OnSessionStarted Event     │               │
│                    └──────────────┬─────────────────┘               │
│                                   │                                 │
│   ┌──────────────────┐           │                                 │
│   │ AIVoiceSessionUI │◀──────────┘                                 │
│   │ - Timer          │                                              │
│   │ - Captions       │                                              │
│   │ - Voice Activity │                                              │
│   │ - Text Input     │                                              │
│   └──────────────────┘                                              │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   │ HTTP REST API
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      INTELLIVERSE-X BACKEND                          │
├─────────────────────────────────────────────────────────────────────┤
│   ┌──────────────────┐      ┌──────────────────────────────────┐   │
│   │ AI Voice Module  │─────▶│ xAI Grok Realtime API            │   │
│   │ - Session Mgmt   │      │ - WebSocket Connection           │   │
│   │ - IAP Validation │      │ - Voice Generation               │   │
│   │ - Analytics      │      │ - Persona Instructions           │   │
│   └──────────────────┘      └──────────────────────────────────┘   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## API Reference

### AIVoiceManager

The main manager for AI Voice sessions.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `AIVoiceManager` | Singleton instance |
| `IsInitialized` | `bool` | Whether Initialize() was called |
| `IsSessionActive` | `bool` | Active session running |
| `CurrentSessionId` | `string` | Current session ID |
| `CurrentPersona` | `AIPersonaType` | Active persona type |
| `IsPremiumSession` | `bool` | Whether premium session |
| `RemainingSessionTime` | `float` | Seconds remaining |

#### Methods

```csharp
// Initialize the system (required first)
void Initialize(string userId, string userName, string authToken = null, string language = "en")

// Set auth token later (after OAuth)
void SetAuthToken(string token)

// Check if user can access a persona
void CheckEntitlement(AIPersonaType persona, Action<EntitlementResponse> callback)

// Check access and start session (RECOMMENDED)
void CheckAndStartSession(
    AIPersonaType persona,
    string topic = null,
    Action<CreateSessionResponse> onSuccess = null,
    Action<string> onNeedsPayment = null,
    Action<string> onError = null
)

// Start session directly (if you handle access yourself)
void StartSession(
    AIPersonaType persona,
    string topic = null,
    Action<CreateSessionResponse> onSuccess = null,
    Action<string> onError = null
)

// Send text message to AI
void SendText(string text)

// Send audio (PCM16 bytes)
void SendAudio(byte[] pcmData)

// Signal end of audio input
void CommitAudio()

// Make AI speak a prompt
void TriggerSpeech(string prompt)

// End current session
void EndSession(Action<SessionAnalytics> callback = null)

// Stop audio playback
void StopAudio()
```

#### Events

```csharp
// Session lifecycle
event Action<CreateSessionResponse> OnSessionStarted;
event Action<SessionAnalytics> OnSessionEnded;

// Voice output
event Action<string> OnVoiceAudioReceived;      // Base64 PCM16
event Action<string> OnCaptionReceived;         // Streaming caption
event Action<string> OnCaptionComplete;         // Full caption
event Action OnVoiceTurnComplete;               // AI done speaking

// Revenue features
event Action<SocialProofData> OnSocialProofReceived;
event Action<string> OnUpsellPrompt;
event Action<string> OnScarcityMessage;
event Action<int> OnSessionTimeWarning;         // Seconds remaining

// Errors
event Action<string> OnError;
```

### AIVoiceIAPManager

Handles IAP products and purchases.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `AIVoiceIAPManager` | Singleton instance |
| `Products` | `List<IAPProductInfo>` | Available products |
| `ProductsFetched` | `bool` | Whether products loaded |
| `CurrentEntitlement` | `EntitlementResponse` | User's access level |
| `IsPurchasing` | `bool` | Purchase in progress |

#### Methods

```csharp
// Load products from backend
void FetchProducts(Action<List<IAPProductInfo>> callback = null)

// Get products for a persona
List<IAPProductInfo> GetProductsForPersona(AIPersonaType persona)

// Get subscription products only
List<IAPProductInfo> GetSubscriptionProducts()

// Get session packs only
List<IAPProductInfo> GetSessionPackProducts()

// Get best value product
IAPProductInfo GetBestValueProduct()

// Get cheapest product (impulse buy)
IAPProductInfo GetCheapestProduct()

// Check user entitlement
void CheckEntitlement(string userId, AIPersonaType persona, Action<EntitlementResponse> callback)

// Quick access checks
bool HasAccess(AIPersonaType persona)
bool HasSubscription()
bool HasUsedFreeTrial()
int GetFreeSessionsRemaining()

// Purchase a product
void Purchase(string productId, Action<IAPProductInfo> onSuccess = null, Action<string> onError = null)

// Restore purchases (iOS)
void RestorePurchases(Action<bool> callback = null)
```

#### Events

```csharp
event Action<List<IAPProductInfo>> OnProductsLoaded;
event Action<IAPProductInfo> OnPurchaseSuccess;
event Action<string, string> OnPurchaseFailed;    // productId, error
event Action<EntitlementResponse> OnEntitlementUpdated;
```

---

## Persona Guide

### Available Personas

| Persona | Revenue Tier | Session Length | Best For |
|---------|-------------|----------------|----------|
| **FortuneTeller** | Tier 1 ($5-15 ARPU) | 60s | Mystical readings, life questions |
| **RelationshipCoach** | Tier 1 ($5-15 ARPU) | 180s | Love advice, relationship guidance |
| **AITeacher** | Tier 1 ($5-15 ARPU) | 60s | Quick lessons on any topic |
| **Matchmaker** | Premium ($8-30 ARPU) | 120s | Dating advice, compatibility |
| **PartyHost** | Tier 2 ($2-6 ARPU) | 120s | Party games, entertainment |
| **TriviaHost** | Tier 2 ($2-6 ARPU) | 60s | Quiz hosting, competition |
| **MotivationalCoach** | Tier 2 ($3-5 ARPU) | 90s | Daily inspiration, goals |
| **StoryTeller** | Tier 2 ($2-6 ARPU) | 120s | Kids stories, bedtime tales |
| **CareerCoach** | Premium ($5-15 ARPU) | 180s | Professional guidance |
| **HealthAdvisor** | Premium ($5-15 ARPU) | 120s | Wellness tips (non-medical) |

### Using the Generic Controller

For any persona, use `GenericAIPersonaController`:

```csharp
// In Inspector, set:
// - Persona Type: MotivationalCoach
// - Persona Display Name: "✨ Coach Spark"
// - Persona Description: "Daily motivation & goal setting"
// - Input Placeholder: "What goal are you working on?"
// - Suggested Prompts: ["Boost my confidence", "Help me focus", "Morning motivation"]
// - Quick Action Labels: ["💪 Confidence", "🎯 Focus", "☀️ Morning"]
```

---

## IAP Configuration

### Backend Products

Configure these products in your App Store Connect / Google Play Console:

```
Product ID                          | Type         | Price  | Sessions
------------------------------------|--------------|--------|----------
fortune_single_reading              | Consumable   | $0.99  | 1
fortune_5_pack                      | Consumable   | $3.99  | 5
fortune_unlimited_monthly           | Subscription | $9.99  | Unlimited
ai_teacher_single_lesson            | Consumable   | $0.99  | 1
ai_teacher_5_lessons                | Consumable   | $3.99  | 5
ai_teacher_unlimited_monthly        | Subscription | $9.99  | Unlimited
ai_voice_all_access_monthly         | Subscription | $14.99 | All Personas
ai_voice_all_access_yearly          | Subscription | $99.99 | All Personas
relationship_coach_session          | Consumable   | $1.99  | 1
relationship_coach_5_pack           | Consumable   | $6.99  | 5
matchmaker_session                  | Consumable   | $2.99  | 1
matchmaker_unlimited_monthly        | Subscription | $14.99 | Unlimited
```

### Unity IAP Integration

In `AIVoiceIAPManager.cs`, integrate with your IAP system:

```csharp
// In PurchaseCoroutine(), replace the demo code with:

IVXIAPManager.Instance.PurchaseProduct(product.productId, (success, receipt) =>
{
    purchaseComplete = true;
    purchaseSuccess = success;
    receiptData = receipt;
});
```

---

## UI Components

### AIVoicePaywallUI

Features:
- Social proof display ("10,000 readings today")
- Scarcity messaging ("Your fortune expires in 24 hours")
- Multiple product options with badges
- Free trial button
- Restore purchases button

**Inspector Setup:**
1. Create Canvas with paywall panel
2. Add `AIVoicePaywallUI` component
3. Assign references:
   - Panel references
   - Social proof texts
   - Product container & prefab
   - Buttons

### AIVoiceSessionUI

Features:
- Session timer with color warnings
- Voice activity indicator
- Live captions (streaming)
- Text input field
- Microphone toggle
- Mute button
- Upsell prompt area

**Inspector Setup:**
1. Create Canvas with session panel
2. Add `AIVoiceSessionUI` component
3. Assign references:
   - Timer elements
   - Caption display
   - Input controls
   - Control buttons

### Creating Product Item Prefab

1. Create prefab with:
   - Background Image
   - Name Text (TMP)
   - Price Text (TMP)
   - Description Text (TMP)
   - Badge Object (with Text)
   - Discount Object (with Text)
   - Purchase Button
2. Add `AIVoiceProductItemUI` component
3. Assign references

---

## Revenue Optimization

### Built-in Features

#### 1. Free Trial Hook
```csharp
// Config: FreeSessionsPerDay = 1
// Automatically tracks usage via PlayerPrefs
```

#### 2. Social Proof
```csharp
// Displayed automatically in paywall
// Backend returns: readingsToday, activeUsers, averageRating

// Manual update:
paywallUI.RefreshSocialProof(new SocialProofData {
    readingsToday = 10523,
    activeUsers = 342,
    averageRating = 4.8f
});
```

#### 3. Scarcity Messaging
```csharp
// Passed to paywall on show:
paywallUI.Show(AIPersonaType.FortuneTeller, socialProof, 
    "🔮 Your fortune is ready for 24 hours!");
```

#### 4. Personalization
```csharp
// User name is passed to AI via Initialize()
AIVoiceManager.Instance.Initialize("user_123", "Sarah");

// AI will say: "Hello Sarah, I sense great energy in you today..."
```

#### 5. Time Warnings & Upsells
```csharp
// Subscribe to warning event
AIVoiceManager.Instance.OnSessionTimeWarning += (secondsRemaining) => {
    if (!AIVoiceManager.Instance.IsPremiumSession) {
        ShowUpsellBanner($"⏱️ {secondsRemaining}s left! Upgrade for more time!");
    }
};
```

### Conversion Optimization

```csharp
// 1. Show cheapest product for impulse buys
var impulseProduct = AIVoiceIAPManager.Instance.GetCheapestProduct();

// 2. Highlight best value
var bestValue = AIVoiceIAPManager.Instance.GetBestValueProduct();

// 3. Use subscription for high-value personas
var subscriptions = AIVoiceIAPManager.Instance.GetSubscriptionProducts();
```

---

## Example Implementations

### Minimal Integration

```csharp
using UnityEngine;
using Trivia.AIVoice;

public class QuickFortuneTeller : MonoBehaviour
{
    [SerializeField] private AIVoicePaywallUI paywall;
    
    void Start()
    {
        AIVoiceManager.Instance.Initialize("user_" + SystemInfo.deviceUniqueIdentifier, "Player");
    }
    
    public void StartFortune()
    {
        AIVoiceManager.Instance.CheckAndStartSession(
            AIPersonaType.FortuneTeller,
            "Tell me about my future",
            onSuccess: (r) => Debug.Log("Fortune started!"),
            onNeedsPayment: (reason) => paywall.Show(AIPersonaType.FortuneTeller),
            onError: Debug.LogError
        );
    }
}
```

### Full Feature Integration

See:
- `FortuneTellerController.cs` - Complete fortune teller with topics, social proof
- `AITeacherController.cs` - Educational with quiz verification
- `GenericAIPersonaController.cs` - Works with any persona

---

## Troubleshooting

### Common Issues

#### "AIVoiceManager not initialized"
```csharp
// Call Initialize() before any other method
AIVoiceManager.Instance.Initialize(userId, userName);
```

#### "No products found"
```csharp
// Ensure FetchProducts was called
AIVoiceIAPManager.Instance.FetchProducts((products) => {
    if (products == null) {
        Debug.LogError("Failed to fetch products - check backend URL");
    }
});
```

#### Session starts but no audio
1. Check AudioSource is assigned or auto-created
2. Verify polling is working (check logs)
3. Ensure backend is returning audio data

#### "Payment needed" always fires
1. Check entitlement endpoint is correct
2. Verify user has free sessions remaining
3. Check PlayerPrefs for free session tracking

### Debug Mode

Enable debug logging:
```csharp
// In AIVoiceConfig asset: DebugLogging = true
// Or in Inspector: Show Debug Logs = true
```

### Backend Health Check

```csharp
StartCoroutine(CheckBackendHealth());

IEnumerator CheckBackendHealth()
{
    using (var request = UnityWebRequest.Get(config.HealthEndpoint))
    {
        yield return request.SendWebRequest();
        Debug.Log($"Backend health: {request.downloadHandler.text}");
    }
}
```

---

## REST API Endpoints Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/ai-voice/personas` | List available personas |
| GET | `/ai-voice/products` | Get IAP products |
| GET | `/ai-voice/entitlements/:userId` | Check user access |
| POST | `/ai-voice/purchase` | Process IAP purchase |
| POST | `/ai-voice/sessions` | Create new session |
| GET | `/ai-voice/sessions/:id` | Get session status |
| GET | `/ai-voice/sessions/:id/messages` | Poll for messages |
| POST | `/ai-voice/sessions/:id/text` | Send text to AI |
| POST | `/ai-voice/sessions/:id/audio` | Send audio to AI |
| DELETE | `/ai-voice/sessions/:id` | End session |
| GET | `/ai-voice/social-proof` | Get social proof data |
| GET | `/ai-voice/health` | Health check |

---

## Support

- 📧 Email: support@intelli-verse-x.ai
- 📖 Docs: https://docs.intelli-verse-x.ai
- 💬 Discord: https://discord.gg/intelliverse-x

---

*IntelliVerse-X SDK - Bringing AI to life in your games* 🎮✨

