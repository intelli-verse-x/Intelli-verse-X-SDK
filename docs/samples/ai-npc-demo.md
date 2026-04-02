# AI NPC & Assistant Demo

Two sample scripts demonstrating AI-powered NPC dialog with tool calls, and a general-purpose AI assistant with hints, tutorials, and knowledge search.

---

## Scene Overview

**Location:**

- `Assets/_IntelliVerseXSDK/Demos/IVXAINPCDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXAIAssistantDemo.cs`

This sample demonstrates:

- NPC registration with persona prompts and backstories
- Dialog sessions with AI-generated responses
- NPC tool/action calls (give item, open shop, start quest)
- AI Assistant Q&A with source citations and confidence scores
- Context-aware gameplay hints
- Step-by-step tutorials
- Knowledge base search

---

## Scene Hierarchy — NPC Dialog

```
Canvas (1920×1080)
├── Background
├── Root
│   ├── Title ("NPC Dialog System")
│   ├── NPC selector (Merchant | Guard | Sage)
│   ├── Status line
│   ├── Mid (split layout)
│   │   ├── ChatCol (conversation bubbles)
│   │   └── LogCol (action/tool call log)
│   ├── InputRow (message + Send button)
│   └── CtrlRow (Start Dialog | End Dialog)
```

---

## Key Components

### NPC Registration

```csharp
var npc = IVXAINPCDialogManager.Instance;
npc.Initialize(aiConfig);

npc.RegisterNPC(new IVXAINPCProfile
{
    NpcId = "npc_merchant",
    DisplayName = "Merchant",
    PersonaPrompt = "You are a friendly shopkeeper who loves haggling.",
    Backstory = "Runs a bustling market stall by the city gates.",
    MaxTurns = 0,
    AvailableActions = new[] { "give_item", "open_shop", "start_quest" }
});
```

### Starting & Sending Messages

```csharp
npc.StartDialog("npc_merchant", playerId, "Town square.", session =>
    Debug.Log($"Session: {session.SessionId}"));

npc.SendMessage(sessionId, "What do you have for sale?");
```

### Handling NPC Responses & Actions

```csharp
npc.OnNPCResponse += (sessionId, text) =>
    AddChatBubble("NPC", text);

npc.OnNPCAction += (sessionId, action) =>
    Debug.Log($"Tool call: {action.ActionName} payload={action.ActionPayload}");

npc.OnDialogStarted += (sessionId) =>
    Debug.Log("Dialog started");
npc.OnDialogEnded += (sessionId) =>
    Debug.Log("Dialog ended");
```

### Ending a Dialog

```csharp
npc.EndDialog(sessionId);
```

---

## Scene Hierarchy — AI Assistant

```
Canvas (1920×1080)
├── Background
├── Root
│   ├── Title ("AI Assistant")
│   ├── Loading indicator
│   ├── AskRow (input + Ask button)
│   ├── HintRow (level id + objective + Get Hint)
│   ├── TutRow (feature id + Get Tutorial)
│   ├── SearchRow (query + Search Knowledge)
│   ├── Responses (scrollable result blocks)
│   └── Clear History button
```

---

### AI Assistant — Ask

```csharp
var assistant = IVXAIAssistant.Instance;
assistant.Initialize(aiConfig);

assistant.Ask("How do I defeat the boss?", context: null, res =>
{
    Debug.Log(res.Response);
    Debug.Log($"Confidence: {res.Confidence:0.###}");
    Debug.Log($"Sources: {string.Join(", ", res.Sources)}");
});
```

### Gameplay Hints

```csharp
assistant.GetHint("level_3", "find_key", context: null, hint =>
{
    Debug.Log(hint.Hint);
    Debug.Log($"Difficulty: {hint.DifficultyLevel}");
    Debug.Log($"Next hint in: {hint.NextHintAvailable}");
});
```

### Step-by-Step Tutorials

```csharp
assistant.GetTutorial("inventory_ui", tut =>
{
    Debug.Log($"Feature: {tut.FeatureId} (~{tut.EstimatedTimeSeconds}s)");
    foreach (var step in tut.Steps)
        Debug.Log($"  {step.StepNumber}. {step.Title}: {step.Description}");
});
```

### Knowledge Base Search

```csharp
assistant.SearchKnowledgeBase("crafting recipes", results =>
{
    foreach (var r in results)
        Debug.Log(r);
});
```

---

## How to Use

### Running the NPC Demo

1. Add `IVXAINPCDemo` to any GameObject
2. Assign an `IVXAIConfig` asset in the Inspector
3. Press **Play**
4. Select an NPC chip (Merchant, Guard, or Sage)
5. Click **Start Dialog**, type messages, and click **Send**
6. Watch the action log for tool calls

### Running the Assistant Demo

1. Add `IVXAIAssistantDemo` to any GameObject
2. Assign an `IVXAIConfig` asset
3. Press **Play**
4. Type questions in the Ask field, or try hints/tutorials/search

### Testing Without a Backend

Both demos instantiate their managers automatically if not present. Without a configured `IVXAIConfig`, error messages appear in the UI — useful for verifying the UI flow.

---

## Customization

### Adding NPCs

Call `RegisterNPC()` with a new `IVXAINPCProfile` containing a unique `NpcId`, persona prompt, backstory, and available actions.

### Custom Tool Actions

Add action names to `AvailableActions` on the NPC profile. Listen to `OnNPCAction` and switch on `action.ActionName` to trigger game logic.

---

## See Also

- [AI Module](../modules/ai.md)
- [AI LLM Stack](../modules/ai-llm-stack.md)
- [AI Getting Started Guide](../guides/ai-getting-started.md)
