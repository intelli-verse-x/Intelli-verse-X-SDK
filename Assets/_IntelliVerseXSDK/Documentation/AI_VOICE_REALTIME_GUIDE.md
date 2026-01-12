# AI Voice Realtime Integration Guide

## Complete End-to-End Implementation for Unity Mobile Apps

This guide covers the complete integration of real-time AI Voice conversations between your Unity mobile app and the xAI backend service.

---

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Backend Setup](#backend-setup)
3. [Unity Client Setup](#unity-client-setup)
4. [Connection Flow](#connection-flow)
5. [Audio Recording](#audio-recording)
6. [Message Types](#message-types)
7. [Error Handling](#error-handling)
8. [Testing](#testing)
9. [Production Deployment](#production-deployment)

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Unity Mobile App                            │
├─────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────┐  ┌───────────────────┐  ┌─────────────────┐  │
│  │ AIVoiceRealtime  │  │ AudioRecording    │  │ AIVoiceConnection│ │
│  │    Manager       │  │    Manager        │  │      UI          │ │
│  └────────┬─────────┘  └────────┬──────────┘  └─────────────────┘  │
│           │                     │                                   │
│           │ WebSocket/HTTP      │ PCM16 Audio                       │
│           ▼                     ▼                                   │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │              AIVoiceWebSocket (Native WebSocket)             │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ wss://api.intelli-verse-x.ai:8766
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        NestJS Backend                               │
├─────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────┐  ┌───────────────────┐  ┌─────────────────┐  │
│  │ AIVoiceWsGateway │  │ AIVoiceService    │  │ AIVoiceSession  │  │
│  │   (Port 8766)    │  │                   │  │    Manager      │  │
│  └────────┬─────────┘  └────────┬──────────┘  └────────┬────────┘  │
│           │                     │                      │           │
│           └─────────────────────┴──────────────────────┘           │
│                                 │                                   │
│                                 │ xAI Realtime API                  │
│                                 ▼                                   │
│           ┌─────────────────────────────────────────┐              │
│           │  xAI Grok Realtime (wss://api.x.ai)     │              │
│           └─────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Backend Setup

### Environment Variables

```bash
# xAI Configuration
XAI_API_KEY=your_xai_api_key_here
XAI_REALTIME_URL=wss://api.x.ai/v1/realtime
XAI_MODEL=grok-2-public

# WebSocket Server
AI_VOICE_WS_PORT=8766
AI_VOICE_WS_ENABLED=true

# AI Host WebSocket (for quiz mode)
AI_HOST_WS_PORT=8765
AI_HOST_WS_ENABLED=true
```

### WebSocket Ports

| Service | Port | Purpose |
|---------|------|---------|
| AI Voice | 8766 | Fortune Teller, Relationship Coach, etc. |
| AI Host | 8765 | Quiz game hosting (Solo, Party, etc.) |

### Starting the Backend

```bash
cd Intelliverse-X-AI
npm install
npm run start:dev
```

The WebSocket servers will start automatically on the configured ports.

---

## 📱 Unity Client Setup

### 1. Add Required Components

Create a GameObject with the following components:

```csharp
// Add to a persistent GameObject
GameObject aiVoiceManager = new GameObject("AIVoiceManager");
DontDestroyOnLoad(aiVoiceManager);

// Add required components
aiVoiceManager.AddComponent<AIVoiceRealtimeManager>();
// WebSocket and AudioRecording are added automatically
```

### 2. Create Configuration Asset

1. In Unity: **Assets > Create > QuizVerse > AI Voice > Configuration**
2. Configure the asset:

```
API Base URL: https://api.intelli-verse-x.ai/api/ai
API Key: (optional - for authenticated requests)
Polling Interval: 0.5 seconds (for HTTP fallback)
Audio Sample Rate: 16000
Audio Channels: 1
```

### 3. Initialize the Manager

```csharp
using Trivia.AIVoice;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Initialize with user credentials
        AIVoiceRealtimeManager.Instance.Initialize(
            userId: "user_123",
            userName: "John",
            authToken: null, // Optional OAuth token
            language: "en"
        );
        
        // Subscribe to events
        SubscribeToEvents();
    }
    
    void SubscribeToEvents()
    {
        var manager = AIVoiceRealtimeManager.Instance;
        
        // Connection events
        manager.OnConnected += () => Debug.Log("Connected!");
        manager.OnConnectionLost += () => Debug.Log("Connection lost");
        manager.OnReconnecting += (attempt, max) => Debug.Log($"Reconnecting {attempt}/{max}");
        manager.OnReconnected += () => Debug.Log("Reconnected!");
        manager.OnConnectionFailed += (error) => Debug.LogError(error);
        
        // Voice events
        manager.OnVoiceAudioReceived += (audio) => { /* Audio played automatically */ };
        manager.OnCaptionReceived += (text) => UpdateCaption(text);
        manager.OnCaptionComplete += (text) => FinalizeCaption(text);
        manager.OnVoiceTurnComplete += () => Debug.Log("AI finished speaking");
        
        // Recording events
        manager.OnRecordingStarted += () => ShowRecordingIndicator();
        manager.OnRecordingStopped += () => HideRecordingIndicator();
        manager.OnSpeechDetected += () => Debug.Log("User speaking...");
        manager.OnAudioLevelChanged += (level) => UpdateAudioMeter(level);
    }
}
```

---

## 🔄 Connection Flow

### Starting a Session

```csharp
public void StartFortuneTellerSession()
{
    AIVoiceRealtimeManager.Instance.CheckAndStartSession(
        persona: AIPersonaType.FortuneTeller,
        topic: "Will I find love this year?",
        onSuccess: (response) => {
            Debug.Log($"Session started: {response.sessionId}");
            ShowSessionUI();
        },
        onNeedsPayment: (reason) => {
            ShowPaywall(reason);
        },
        onError: (error) => {
            ShowError(error);
        }
    );
}
```

### Session Lifecycle

```
1. CheckAndStartSession()
   └─> REST: POST /api/ai/ai-voice/sessions
   └─> Response: { sessionId, config, socialProof }
   
2. WebSocket Connect
   └─> ws://server:8766?sessionId=xxx&userId=yyy
   └─> Message: { type: "join_session" }
   └─> Response: { type: "session_joined" }
   
3. Real-time Communication
   └─> Send: { type: "user_text", text: "Hello" }
   └─> Receive: { type: "voice_audio", audio: "base64..." }
   └─> Receive: { type: "voice_caption", text: "I sense..." }
   
4. End Session
   └─> REST: DELETE /api/ai/ai-voice/sessions/:id
   └─> Response: { analytics }
```

---

## 🎤 Audio Recording

### Starting Voice Input

```csharp
public void OnPushToTalkPressed()
{
    AIVoiceRealtimeManager.Instance.StartRecording();
}

public void OnPushToTalkReleased()
{
    AIVoiceRealtimeManager.Instance.StopRecording();
}
```

### Voice Activity Detection (VAD)

The `AudioRecordingManager` includes automatic VAD:

```csharp
// Configure VAD thresholds
var recorder = GetComponent<AudioRecordingManager>();
recorder.SetSpeechThreshold(0.01f); // Adjust sensitivity
recorder.SetVADEnabled(true); // Enable/disable VAD

// Events
recorder.OnSpeechDetected += () => { /* User started speaking */ };
recorder.OnSpeechStopped += () => { /* User stopped - auto-commits audio */ };
```

### Microphone Calibration

```csharp
// Calibrate noise level for better detection
recorder.CalibrateNoiseLevel(3f, (recommendedThreshold) => {
    recorder.SetSpeechThreshold(recommendedThreshold);
});
```

---

## 📨 Message Types

### Client → Server

| Type | Description | Payload |
|------|-------------|---------|
| `join_session` | Join existing session | `{ sessionId, userId, userName }` |
| `rejoin_session` | Rejoin after reconnect | `{ sessionId, userId }` |
| `user_text` | Send text message | `{ text: "..." }` |
| `input_audio_buffer.append` | Send audio chunk | `{ audio: "base64..." }` |
| `input_audio_buffer.commit` | End audio input | `{}` |
| `trigger_speech` | Make AI speak | `{ prompt: "..." }` |
| `end_session` | End session | `{}` |
| `ping` | Heartbeat | `{}` |

### Server → Client

| Type | Description | Payload |
|------|-------------|---------|
| `connected` | Connection established | `{ serverTime }` |
| `session_joined` | Session joined | `{ sessionId, config }` |
| `voice_audio` | Audio from AI | `{ audio: "base64..." }` |
| `voice_caption` | Streaming caption | `{ text: "..." }` |
| `voice_caption_complete` | Final caption | `{ text: "..." }` |
| `voice_turn_complete` | AI finished | `{}` |
| `speech_detected` | User speaking | `{}` |
| `speech_stopped` | User stopped | `{}` |
| `social_proof` | Engagement data | `{ readingsToday, activeUsers }` |
| `session_ending` | Time warning | `{ upsellMessage }` |
| `connection_lost` | Disconnected | `{ message }` |
| `reconnecting` | Reconnecting | `{ attempt, maxAttempts }` |
| `reconnected` | Reconnected | `{}` |
| `connection_failed` | Failed | `{ error }` |
| `error` | Error | `{ error }` |
| `pong` | Heartbeat response | `{ timestamp }` |

---

## ⚠️ Error Handling

### Connection Errors

```csharp
AIVoiceRealtimeManager.Instance.OnConnectionFailed += (error) => {
    // Show user-friendly error
    if (error.Contains("timeout"))
    {
        ShowMessage("Connection timed out. Please check your internet.");
    }
    else if (error.Contains("XAI_API_KEY"))
    {
        ShowMessage("Service unavailable. Please try again later.");
    }
    else
    {
        ShowMessage($"Connection error: {error}");
    }
    
    // Offer retry
    ShowRetryButton();
};
```

### Automatic Reconnection

The system automatically reconnects with exponential backoff:

- Attempt 1: 1 second delay
- Attempt 2: 2 seconds delay
- Attempt 3: 4 seconds delay
- Attempt 4: 8 seconds delay
- Attempt 5: 16 seconds delay
- After 5 failures: `OnConnectionFailed` is fired

### Manual Reconnection

```csharp
public void OnRetryButtonClicked()
{
    AIVoiceRealtimeManager.Instance.ForceReconnect();
}
```

---

## 🧪 Testing

### Integration Test Component

Add `AIVoiceIntegrationTest` to a test scene:

```csharp
// Run all tests
GetComponent<AIVoiceIntegrationTest>().RunAllTests();

// Run specific test
GetComponent<AIVoiceIntegrationTest>().RunTest("Test_WebSocketConnection");

// Get results
var results = GetComponent<AIVoiceIntegrationTest>().GetResults();
Debug.Log($"Summary: {GetComponent<AIVoiceIntegrationTest>().GetSummary()}");
```

### Test Coverage

| Test | Description |
|------|-------------|
| `Test_Initialization` | Manager initialization |
| `Test_SessionCreation` | Session creation via REST |
| `Test_WebSocketConnection` | WebSocket connectivity |
| `Test_TextCommunication` | Text message send/receive |
| `Test_AudioRecording` | Microphone recording |
| `Test_MessageHandling` | Message parsing |
| `Test_ConnectionRecovery` | Reconnection logic |
| `Test_SessionEnd` | Session cleanup |
| `Test_ErrorHandling` | Error scenarios |
| `Test_EntitlementCheck` | IAP entitlements |

### Backend Health Check

```bash
curl http://your-server:3000/api/ai/ai-voice/health
```

---

## 🚀 Production Deployment

### Checklist

- [ ] Set `XAI_API_KEY` environment variable
- [ ] Configure firewall for WebSocket ports (8765, 8766)
- [ ] Enable SSL/TLS for WebSocket connections (wss://)
- [ ] Set up load balancing for WebSocket servers
- [ ] Configure IAP products in App Store / Google Play
- [ ] Test on real devices (iOS and Android)
- [ ] Monitor connection metrics and errors

### Unity Build Settings

**iOS:**
- Enable `NSMicrophoneUsageDescription` in Info.plist
- Enable `NSAppTransportSecurity` for WebSocket connections

**Android:**
- Add `RECORD_AUDIO` permission to AndroidManifest.xml
- Enable `INTERNET` permission

### Performance Tips

1. **Audio Chunking**: Send audio in 100ms chunks (1600 samples at 16kHz)
2. **Compression**: Disable WebSocket compression for lower latency
3. **Heartbeat**: 30-second interval keeps connections alive
4. **Timeout**: 60-second timeout for dead connections

---

## 📊 Monitoring

### Key Metrics

```csharp
// Get connection stats
var manager = AIVoiceRealtimeManager.Instance;
Debug.Log($"Connection State: {manager.ConnectionState}");
Debug.Log($"Latency: {manager.LatencyMs}ms");
Debug.Log($"Session Active: {manager.IsSessionActive}");
Debug.Log($"Remaining Time: {manager.RemainingSessionTime}s");
```

### Backend Analytics Endpoint

```bash
curl http://your-server:3000/api/ai/ai-voice/analytics
```

Response:
```json
{
  "totalSessions": 1234,
  "premiumSessions": 567,
  "conversionRate": "45.9%",
  "totalRevenue": "5678.90",
  "activeSessions": 23,
  "uniqueUsers": 890
}
```

---

## 🆘 Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection refused | Check WebSocket port is open |
| Audio not playing | Verify AudioSource is assigned |
| No microphone input | Check permissions on device |
| xAI errors | Verify API key is valid |
| Reconnection loop | Check network stability |
| High latency | Use WebSocket mode, not HTTP polling |

---

## 📝 Version History

- **2.0** - Added WebSocket support, reconnection, audio recording
- **1.0** - Initial HTTP polling implementation

---

*IntelliVerse-X SDK - AI Voice Realtime Integration*

