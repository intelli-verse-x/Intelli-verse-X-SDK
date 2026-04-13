---
name: ivx-narrative-engine
description: >-
  Add branching dialog, story state machines, and narrative tools to IntelliVerseX
  SDK games. Use when the user says "dialog system", "branching dialog",
  "conversation tree", "story state", "narrative engine", "character relationships",
  "Ink import", "Yarn Spinner", "quest dialog", "cutscene", "visual novel",
  "dialog editor", "story flags", "NPC conversation", or needs help with
  any narrative or dialog system.
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

# IntelliVerseX Narrative Engine

## Overview

The Narrative Engine provides a complete dialog and story system with branching conversations, persistent story state, character relationship tracking, and AI-assisted dialog generation. It supports importing from industry-standard formats (Ink, Yarn Spinner) and includes a visual conversation editor for Unity.

```
IVXNarrativeManager (orchestrator)
      │
      ├── IVXDialogManager       → Branching conversation playback
      ├── IVXStoryState           → Flags, variables, conditions
      ├── IVXConversationTree     → Node graph data structure
      ├── IVXCharacterRelationship → Affinity tracking
      ├── IVXCutsceneSequencer    → Timed narrative events
      └── IVXNarrativeAI          → AI dialog generation
              │
              ▼
      IVXLocalizationManager (localized strings)
```

---

## 1. Configuration

### IVXNarrativeConfig ScriptableObject

Create via **Create > IntelliVerseX > Narrative Configuration**.

| Field | Description |
|-------|-------------|
| `EnableNarrative` | Master toggle |
| `DialogFormat` | `IVXNative`, `Ink`, `YarnSpinner` |
| `AutoAdvanceDelay` | Seconds before auto-advancing non-choice dialog (0 = manual) |
| `TypewriterSpeed` | Characters per second for typewriter effect (0 = instant) |
| `EnableVoiceover` | Integrate with IVXAIVoiceServices for TTS dialog |
| `EnableAIDialog` | Allow AI-generated dialog for dynamic NPCs |
| `PersistState` | Save story state to Nakama storage |
| `MaxRelationshipValue` | Cap for relationship affinity (default 100) |

---

## 2. Conversation Trees

### Defining Conversations

```csharp
using IntelliVerseX.Narrative;

var tree = new IVXConversationTree("merchant_greeting");

var root = tree.AddNode(new DialogNode
{
    Speaker = "merchant",
    Text = "Welcome, traveler! What brings you to my shop?",
});

var buyOption = tree.AddChoice(root, new ChoiceNode
{
    Text = "I'd like to buy something.",
    Condition = null,
});

var sellOption = tree.AddChoice(root, new ChoiceNode
{
    Text = "I have items to sell.",
    Condition = new StoryCondition("has_sellable_items", true),
});

var rumorOption = tree.AddChoice(root, new ChoiceNode
{
    Text = "Heard any interesting rumors?",
    Condition = new StoryCondition("relationship.merchant", CompareOp.GreaterThan, 20),
});

tree.AddNode(buyOption, new DialogNode
{
    Speaker = "merchant",
    Text = "Excellent! Take a look at my finest wares.",
    Action = new StoryAction("open_store"),
});

tree.AddNode(sellOption, new DialogNode
{
    Speaker = "merchant",
    Text = "Let me see what you've got...",
    Action = new StoryAction("open_sell_panel"),
});

tree.AddNode(rumorOption, new DialogNode
{
    Speaker = "merchant",
    Text = "Well, between you and me... I heard strange noises from the old mine.",
    Action = new StoryAction("unlock_quest", "investigate_mine"),
    RelationshipChange = new RelationshipDelta("merchant", +5),
});
```

### JSON Format

Conversations can also be defined in JSON:

```json
{
    "id": "merchant_greeting",
    "nodes": [
        {
            "id": "start",
            "speaker": "merchant",
            "text": "dialog.merchant.greeting",
            "choices": [
                { "text": "dialog.merchant.choice_buy", "next": "buy_response" },
                { "text": "dialog.merchant.choice_sell", "next": "sell_response",
                  "condition": { "flag": "has_sellable_items", "value": true } },
                { "text": "dialog.merchant.choice_rumors", "next": "rumors",
                  "condition": { "relationship": "merchant", "min": 20 } }
            ]
        }
    ]
}
```

Text values prefixed with `dialog.` are resolved through `IVXLocalizationManager`.

---

## 3. Playing Dialog

### IVXDialogManager

```csharp
IVXDialogManager.Instance.OnDialogLine += (line) =>
{
    dialogUI.ShowLine(line.Speaker, line.Text, line.Portrait);
};

IVXDialogManager.Instance.OnChoicesPresented += (choices) =>
{
    dialogUI.ShowChoices(choices.Select(c => c.Text).ToList());
};

IVXDialogManager.Instance.OnDialogEnd += () =>
{
    dialogUI.Hide();
};

await IVXDialogManager.Instance.StartConversationAsync("merchant_greeting");
```

### Selecting Choices

```csharp
dialogUI.OnChoiceSelected += (index) =>
{
    IVXDialogManager.Instance.SelectChoice(index);
};
```

---

## 4. Story State

### IVXStoryState

Persistent flags and variables that control narrative flow:

```csharp
IVXStoryState.Instance.SetFlag("met_merchant", true);
IVXStoryState.Instance.SetInt("quests_completed", 5);
IVXStoryState.Instance.SetString("player_title", "Dragon Slayer");

bool metMerchant = IVXStoryState.Instance.GetFlag("met_merchant");
int quests = IVXStoryState.Instance.GetInt("quests_completed");
```

### Conditions

```csharp
bool canAccess = IVXStoryState.Instance.Evaluate(new StoryCondition[]
{
    new("met_merchant", true),
    new("quests_completed", CompareOp.GreaterThanOrEqual, 3),
    new("relationship.elder", CompareOp.GreaterThan, 50),
});
```

### State Persistence

```csharp
await IVXStoryState.Instance.SaveAsync();
await IVXStoryState.Instance.LoadAsync();
```

With `PersistState` enabled, story state syncs to Nakama storage for cloud save and cross-device continuity.

---

## 5. Character Relationships

### IVXCharacterRelationship

Track player affinity with NPCs:

```csharp
IVXCharacterRelationship.Instance.Modify("merchant", +10);
IVXCharacterRelationship.Instance.Modify("guard_captain", -5);

int merchantAffinity = IVXCharacterRelationship.Instance.Get("merchant");
```

### Relationship Tiers

| Range | Tier | Effect |
|-------|------|--------|
| 0–19 | Stranger | Basic dialog only |
| 20–49 | Acquaintance | Rumors, minor quests available |
| 50–79 | Friend | Special prices, side quests |
| 80–100 | Trusted | Secret quests, unique items, lore |

### Relationship Events

```csharp
IVXCharacterRelationship.Instance.OnTierChanged += (character, oldTier, newTier) =>
{
    Debug.Log($"{character}: {oldTier} → {newTier}");
    ShowRelationshipNotification(character, newTier);
};
```

---

## 6. Ink and Yarn Spinner Import

### Ink Import

```csharp
var inkStory = await IVXNarrativeImporter.Instance.ImportInkAsync("Assets/Narratives/story.ink.json");

IVXDialogManager.Instance.StartInkStory(inkStory);
```

### Yarn Spinner Import

```csharp
var yarnProject = await IVXNarrativeImporter.Instance.ImportYarnAsync("Assets/Narratives/dialog.yarn");

IVXDialogManager.Instance.StartYarnNode(yarnProject, "MerchantGreeting");
```

### Format Comparison

| Feature | IVX Native | Ink | Yarn Spinner |
|---------|-----------|-----|-------------|
| Branching dialog | Yes | Yes | Yes |
| Variables/flags | Yes | Yes | Yes |
| Localization | Built-in | Manual | Built-in |
| Visual editor | Yes (Unity) | Inky (external) | VS Code extension |
| AI generation | Yes | Export to Ink | Export to Yarn |
| Relationship tracking | Built-in | Custom functions | Custom commands |
| Voice integration | Built-in | Manual | Manual |

---

## 7. AI-Assisted Dialog

### Dynamic Dialog Generation

For NPCs that need varied responses, use the AI subsystem:

```csharp
var response = await IVXNarrativeAI.Instance.GenerateDialogAsync(new DialogGenerationRequest
{
    NPC = "merchant",
    PlayerMessage = "Do you have any fire-resistant armor?",
    Personality = "shrewd, helpful, slightly greedy",
    Context = new DialogContext
    {
        Relationship = IVXCharacterRelationship.Instance.Get("merchant"),
        StoryFlags = IVXStoryState.Instance.GetAllFlags(),
        RecentDialog = lastThreeLines,
    },
});

dialogUI.ShowLine("merchant", response.Text);
```

### Conversation Memory

AI dialog maintains conversation history per-NPC so responses stay contextually consistent within a session.

---

## 8. Cutscene Sequencer

### IVXCutsceneSequencer

Timeline-based narrative events:

```csharp
var cutscene = new IVXCutscene("intro_sequence");

cutscene.At(0f, new CameraAction("pan_to_village"));
cutscene.At(2f, new DialogAction("elder", "Our village has been peaceful for centuries..."));
cutscene.At(6f, new DialogAction("elder", "Until the shadows came."));
cutscene.At(9f, new CameraAction("pan_to_dark_forest"));
cutscene.At(10f, new SoundAction("thunder_rumble"));
cutscene.At(12f, new FadeAction(FadeType.Out, duration: 1f));

await IVXCutsceneSequencer.Instance.PlayAsync(cutscene);
```

### Cutscene Actions

| Action | Description |
|--------|------------|
| `DialogAction` | Show dialog line with speaker |
| `CameraAction` | Move/rotate camera |
| `SoundAction` | Play sound effect or music |
| `FadeAction` | Fade in/out |
| `AnimationAction` | Trigger character animation |
| `WaitAction` | Pause for duration or input |
| `StoryAction` | Set flags/variables |
| `SpawnAction` | Instantiate objects |

---

## 9. Cross-Platform API

| Engine | Class / Module | Core API |
|--------|---------------|----------|
| **Unity** | `IVXNarrativeManager` | Full feature set + visual editor |
| **Unreal** | `UIVXNarrativeSubsystem` | Dialog, story state, relationships, Ink/Yarn |
| **Godot** | `IVXNarrative` autoload | Dialog, story state, relationships |
| **JavaScript** | `IVXNarrative` | Dialog, story state (JSON-based trees) |
| **Roblox** | `IVX.Narrative` | Dialog (via RemoteEvents), story state |
| **Java** | `IVXNarrative` | Dialog, story state |
| **Flutter** | `IvxNarrative` | Dialog, story state |
| **C++** | `IVXNarrative` | Dialog, story state, relationships |

---

## Platform-Specific Narrative

### VR Platforms

| Feature | VR Guidance |
|---------|------------|
| **Dialog UI** | World-space dialog bubbles near NPC. No 2D overlay. |
| **Choices** | Gaze-select or controller-point to choose dialog options. |
| **Cutscenes** | Avoid taking camera control from the player. Use guided attention (audio cues, lighting) instead. |
| **Voice** | Spatial audio for NPC voices. Players naturally turn toward speakers. |

### Console Platforms

| Feature | Console Guidance |
|---------|-----------------|
| **Dialog speed** | Support variable text speed and instant-reveal for accessibility. |
| **Controller** | D-pad for dialog choices. A/Cross to advance. B/Circle to skip. |
| **Subtitles** | Mandatory subtitle support with size and background options. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Story state** | Persist to `localStorage` or `IndexedDB`. Sync to Nakama on session end. |
| **Dialog assets** | Load conversation JSON on demand to reduce initial bundle size. |
| **Voice** | Use Web Speech API for client-side TTS as a lightweight alternative to AI voice. |

---

## Checklist

- [ ] `EnableNarrative` toggled on in `IVXBootstrapConfig`
- [ ] Dialog format chosen (IVX Native, Ink, or Yarn Spinner)
- [ ] Conversation trees defined for key NPCs
- [ ] Dialog UI connected to `IVXDialogManager` events
- [ ] Story state flags and variables defined
- [ ] Character relationships initialized with starting values
- [ ] Ink/Yarn files imported (if using external format)
- [ ] AI dialog tested for dynamic NPCs (if using AI)
- [ ] Cutscene sequences created for key narrative moments
- [ ] Dialog localized through `IVXLocalizationManager`
- [ ] VR world-space dialog UI implemented (if targeting VR)
- [ ] Console controller navigation for dialog tested (if targeting consoles)
- [ ] WebGL story state persistence configured (if targeting browser)
