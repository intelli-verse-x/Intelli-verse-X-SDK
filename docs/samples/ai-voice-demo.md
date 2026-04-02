# AI Voice Chat Demo

Two sample scripts demonstrating the AI voice and text chat system — persona-based conversations with real-time captions, microphone recording, and standalone TTS/STT services.

---

## Scene Overview

**Location:** `Assets/_IntelliVerseXSDK/Demos/IVXAIVoiceChatDemo.cs` and `IVXAIVoiceServicesDemo.cs`

This sample demonstrates:

- AI persona selection (Fortune Teller, Teacher, Coach, etc.)
- Timed voice/text chat sessions with real-time captions
- Microphone recording, commit, and playback
- Text-to-Speech synthesis
- Speech-to-Text streaming transcription
- Language detection from audio
- Available voice listing

---

## Scene Hierarchy — Voice Chat

```
Canvas
├── Background
├── Root
│   ├── PersonaGrid
│   │   ├── Title ("Choose Your AI Persona")
│   │   └── GridHolder (2-column card grid)
│   │       ├── Fortune Teller
│   │       ├── AI Teacher
│   │       ├── Career Coach
│   │       ├── Story Teller
│   │       ├── Party Host
│   │       └── Health Advisor
│   └── ChatPanel
│       ├── Header (back, title, timer, end)
│       ├── ScrollArea (chat bubbles)
│       ├── CaptionBar (live captions)
│       └── InputBar (mic, text input, send)
```

---

## Key Components

### IVXAIVoiceChatDemo — Persona Chat

```csharp
// Starting a voice session with a persona
IVXAISessionManager.Instance.OnCaptionReceived += (text) =>
    captionLabel.text = text;
IVXAISessionManager.Instance.OnCaptionComplete += (text) =>
    AddBubble(text, isUser: false);

IVXAISessionManager.Instance.StartVoiceSessionDirect("Fortune Teller",
    onSuccess: resp =>
    {
        sessionDuration = resp.DurationSeconds;
        AddBubble("Hello! How can I help?", isUser: false);
    },
    onError: err => AddBubble($"[Error: {err}]", isUser: false));
```

### Sending Text Messages

```csharp
IVXAISessionManager.Instance.SendText("What does my future hold?");
```

### Microphone Recording

```csharp
// Start recording
IVXAISessionManager.Instance.StartRecording();

// Stop and send audio
IVXAISessionManager.Instance.StopRecording();
IVXAISessionManager.Instance.CommitAudio();
```

### Ending a Session

```csharp
IVXAISessionManager.Instance.EndVoiceSession();
```

### IVXAIVoiceServicesDemo — Standalone Services

```csharp
var svc = IVXAIVoiceServices.Instance;

// Text-to-Speech
svc.SynthesizeSpeech("Hello world", voiceId: null, audioBytes =>
    Debug.Log($"Synthesized {audioBytes.Length} bytes"));

// List available voices
svc.ListVoices(voices =>
{
    foreach (var v in voices) Debug.Log(v);
});

// Streaming Speech-to-Text
svc.StartStreamingTranscription();
// ... user speaks ...
svc.StopStreamingTranscription();
svc.TranscribeAudio(audioData, sampleRate: 16000, result =>
    Debug.Log($"Transcription: {result}"));

// Language detection
svc.DetectLanguage(audioData, sampleRate: 16000, (lang, confidence) =>
    Debug.Log($"Detected: {lang} ({confidence:P0})"));
```

---

## How to Use

### Running the Voice Chat Demo

1. Add `IVXAIVoiceChatDemo` to any Canvas
2. Optionally configure `IVXAISessionManager` in the scene for live backend
3. Press **Play** — the persona selection grid appears
4. Tap a persona card to start a chat session

### Testing Without a Backend

The demo operates in **mock mode** when `IVXAISessionManager` is not initialized — messages receive placeholder responses so you can test the UI flow.

### Testing Voice Services

1. Add `IVXAIVoiceServicesDemo` to any GameObject
2. Ensure `IVXAIVoiceServices` is initialized with an `IVXAIConfig`
3. Use the **Speak** button for TTS, **Start Streaming** for STT

### Testing the Session Timer

Each session runs on a configurable countdown (default 180 seconds). When the timer reaches zero, `EndVoiceSession()` is called automatically.

---

## Customization

### Adding Personas

Edit the `MOCK_PERSONAS` and `MOCK_ICONS` arrays in `IVXAIVoiceChatDemo.cs` to add new persona cards.

### Changing Session Duration

Set `_sessionDuration` before calling `StartVoiceSessionDirect`, or modify the backend response's `DurationSeconds`.

---

## See Also

- [AI Module](../modules/ai.md)
- [AI LLM Stack](../modules/ai-llm-stack.md)
- [AI Getting Started Guide](../guides/ai-getting-started.md)
