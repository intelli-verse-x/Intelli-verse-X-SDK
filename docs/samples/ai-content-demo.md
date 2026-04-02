# AI Content & Moderation Demo

Three sample scripts demonstrating AI-powered content generation, chat moderation, and player profiling.

---

## Scene Overview

**Location:**

- `Assets/_IntelliVerseXSDK/Demos/IVXAIContentGenDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXAIModerationDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXAIProfilerDemo.cs`

This sample demonstrates:

- Quest generation (genre, difficulty, objectives, rewards)
- Story generation with word count control
- Item description generation (stats, flavor text)
- Dialogue generation (multi-character, emotions, actions)
- Text classification (category, severity, confidence)
- Message filtering and replacement
- Batch moderation scanning
- Custom moderation rules
- Discord moderation metadata export
- Player event tracking and profiling
- Cohort classification and churn prediction
- Personalization hints

---

## Content Generation — IVXAIContentGenDemo

### Scene Hierarchy

```
Canvas (1920×1080)
├── Background
├── Root
│   ├── Title ("AI Content Generation")
│   ├── Progress indicator
│   ├── TabBar (Quest | Story | Item | Dialogue)
│   ├── Body
│   │   ├── QuestTab (genre chips, difficulty chips, output)
│   │   ├── StoryTab (prompt input, genre, word slider, output)
│   │   ├── ItemTab (name, type, rarity inputs, output)
│   │   └── DialogueTab (scenario, characters, output)
│   └── Cancel generation button
```

### Quest Generation

```csharp
var gen = IVXAIContentGenerator.Instance;
gen.Initialize(aiConfig);

gen.GenerateQuest(new IVXQuestTemplate
{
    Genre = "fantasy",
    Difficulty = "medium",
    RequiredElements = new[] { "exploration", "combat" },
    EstimatedDurationMinutes = 15,
    CustomPrompt = "Suitable for a casual mobile RPG."
}, "player_id", quest =>
{
    Debug.Log($"{quest.Title}: {quest.Description}");
    foreach (var obj in quest.Objectives) Debug.Log($"  Objective: {obj}");
    foreach (var rew in quest.Rewards) Debug.Log($"  Reward: {rew}");
});
```

### Story Generation

```csharp
gen.GenerateStory("A hero opens a forbidden door.", "fantasy", maxWords: 500, story =>
{
    Debug.Log($"{story.Title} ({story.Genre}, {story.WordCount} words)");
    Debug.Log(story.Content);
});
```

### Item Description

```csharp
gen.GenerateItemDescription("Mystic Blade", "weapon", "rare", item =>
{
    Debug.Log($"{item.ItemName} ({item.ItemType}, {item.Rarity})");
    Debug.Log(item.FlavorText);
    foreach (var kv in item.Stats)
        Debug.Log($"  {kv.Key}: {kv.Value}");
});
```

### Dialogue Generation

```csharp
gen.GenerateDialogue("A tense negotiation at the gate.",
    new[] { "Guard", "Mercenary", "Merchant" }, dialogue =>
{
    foreach (var line in dialogue.Lines)
        Debug.Log($"{line.Character} ({line.Emotion}): {line.Text}");
});
```

---

## Moderation — IVXAIModerationDemo

### Scene Hierarchy

```
Canvas (1920×1080)
├── Root
│   ├── Title ("AI Moderation")
│   ├── Test message input (multiline)
│   ├── Button row (Classify | Filter | Scan Batch)
│   ├── Results panel (category, severity, confidence, action)
│   ├── Custom rules panel
│   │   ├── Action picker (Allow/Warn/Replace/Block/Flag)
│   │   ├── Pattern + replacement inputs
│   │   └── Rules list (scrollable)
│   └── Discord metadata integration
```

### Text Classification

```csharp
var mod = IVXAIModerator.Instance;
mod.Initialize(aiConfig);

mod.ClassifyText("suspicious message", result =>
{
    Debug.Log($"Category: {result.Category}");
    Debug.Log($"Severity: {result.Severity}");
    Debug.Log($"Confidence: {result.Confidence:0.###}");
    Debug.Log($"Action: {result.SuggestedAction}");
});
```

### Message Filtering

```csharp
mod.FilterMessage("bad word here", filtered =>
    Debug.Log($"Filtered: {filtered}"));
```

### Batch Scanning

```csharp
mod.ScanBatch(new List<string>
{
    "Hello, good luck!",
    "Buy cheap gold now!!!",
    "I will find you",
    "My email is test@example.com"
}, results =>
{
    for (int i = 0; i < results.Count; i++)
        Debug.Log($"[{i}] {results[i].Category} | {results[i].Severity}");
});
```

### Custom Rules

```csharp
mod.AddCustomRule(new IVXModerationRule
{
    Pattern = "spam_keyword",
    Category = IVXContentCategory.Custom,
    Action = IVXModerationActionType.Replace,
    ReplacementText = "[filtered]"
});
```

---

## Player Profiler — IVXAIProfilerDemo

### Key API Calls

```csharp
var profiler = IVXAIProfiler.Instance;

// Track a gameplay event
profiler.TrackEvent("level_complete", new Dictionary<string, object>
{
    { "type", "gameplay" }
});

// Fetch full player profile
profiler.GetPlayerProfile(profile => Debug.Log(profile));

// Classify into a cohort
profiler.ClassifyPlayer(cohort => Debug.Log($"Cohort: {cohort}"));

// Predict churn risk
profiler.PredictChurn((score, factors) =>
    Debug.Log($"Churn risk: {score:P0}, factors: {string.Join(", ", factors)}"));

// Get personalization hints
profiler.GetPersonalizationHints(hints =>
{
    foreach (var h in hints) Debug.Log($"Hint: {h}");
});
```

---

## How to Use

### Running the Content Gen Demo

1. Add `IVXAIContentGenDemo` to any GameObject
2. Assign an `IVXAIConfig` asset
3. Press **Play** and switch between Quest/Story/Item/Dialogue tabs

### Running the Moderation Demo

1. Add `IVXAIModerationDemo` to any GameObject
2. Assign an `IVXAIConfig` asset
3. Type test messages and click **Classify**, **Filter**, or **Scan Batch**
4. Add custom rules in the rules panel

### Running the Profiler Demo

1. Add `IVXAIProfilerDemo` to any GameObject
2. Ensure `IVXAIProfiler` is initialized
3. Track events and fetch profile/cohort/churn data

---

## Customization

### Extending Content Types

Call additional methods on `IVXAIContentGenerator` for custom content. Set `CustomPrompt` on templates to steer the AI output.

### Adding Moderation Rules at Runtime

Use `mod.AddCustomRule()` with regex patterns. Rules persist for the session and apply to all subsequent `ClassifyText` and `FilterMessage` calls.

---

## See Also

- [AI Module](../modules/ai.md)
- [AI LLM Stack](../modules/ai-llm-stack.md)
- [AI Getting Started Guide](../guides/ai-getting-started.md)
