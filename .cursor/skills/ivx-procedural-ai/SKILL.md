---
name: ivx-procedural-ai
description: >-
  AI-powered procedural content generation for IntelliVerseX SDK games including
  levels, NPCs, items, quests, and assets. Use when the user says "procedural generation",
  "generate levels", "AI level design", "generate NPCs", "random loot tables",
  "AI quest generation", "procedural content", "PCG", "generate items",
  "AI dungeon", "world generation", "texture generation", "AI assets",
  or needs help with any AI-powered content creation.
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

# IntelliVerseX Procedural AI

## Overview

The Procedural AI module extends the SDK's 7 AI subsystems with structured content generators for levels, NPCs, items, quests, and visual assets. All generators share the `IVXAIConfig` provider system and support both cloud-based LLM generation and deterministic seed-based fallbacks for offline play.

```
IVXPCGManager (orchestrator)
      │
      ├── IVXLevelGenerator      → Layout graphs, tile maps, difficulty curves
      ├── IVXNPCGenerator        → Personalities, dialog, behavior trees
      ├── IVXLootGenerator       → Item tables, rarity, stat rolls
      ├── IVXQuestGenerator      → Mission chains, objectives, narratives
      ├── IVXAssetGenerator      → Texture variations, sprite recolors
      └── IVXMusicGenerator      → Adaptive music, SFX variations
              │
              ▼
      IVXAIContentGenerator (structured output)
      IVXAIConfig (provider selection)
```

---

## 1. Configuration

### IVXPCGConfig ScriptableObject

Create via **Create > IntelliVerseX > Procedural Content Configuration**.

| Field | Description |
|-------|-------------|
| `EnablePCG` | Master toggle for procedural generation |
| `GenerationMode` | `CloudAI` (LLM-based), `Deterministic` (seed-based), `Hybrid` |
| `DefaultSeed` | Base seed for deterministic generation (0 = random) |
| `CacheGeneratedContent` | Cache results to avoid regenerating identical requests |
| `CacheTTLMinutes` | How long cached content remains valid (default 1440) |
| `MaxConcurrentGenerations` | Throttle parallel AI calls (default 3) |
| `FallbackToDeterministic` | Use seed-based generation if AI fails |
| `QualityLevel` | `Draft` (fast, cheaper), `Standard`, `Premium` (detailed, costly) |

---

## 2. Level Generator

### IVXLevelGenerator

Generate level layouts using graph-based room placement:

```csharp
using IntelliVerseX.PCG;

var request = new LevelGenerationRequest
{
    Theme = "dungeon",
    Difficulty = 0.6f,
    RoomCount = new IntRange(8, 15),
    BossRoom = true,
    TreasureRooms = 2,
    Seed = 42,
    Constraints = new LevelConstraints
    {
        MaxBacktracking = 0.2f,
        BranchingFactor = new FloatRange(1.5f, 3.0f),
        CriticalPathLength = new IntRange(5, 8),
    },
};

var level = await IVXLevelGenerator.Instance.GenerateAsync(request);

foreach (var room in level.Rooms)
{
    Debug.Log($"Room {room.Id}: {room.Type} at ({room.Position.x}, {room.Position.y})");
    Debug.Log($"  Connections: {string.Join(", ", room.Connections)}");
    Debug.Log($"  Enemies: {room.EnemyCount}, Loot: {room.LootTier}");
}
```

### Level Output Format

```csharp
public class GeneratedLevel
{
    public List<Room> Rooms;
    public List<Connection> Connections;
    public Room StartRoom;
    public Room BossRoom;
    public List<int> CriticalPath;
    public float EstimatedDuration;
    public float DifficultyScore;
    public int Seed;
}
```

### Tile Map Export

```csharp
var tilemap = level.ToTileMap(tileSet: "dungeon_tiles");
tilemap.ApplyTo(unityTilemap);
```

---

## 3. NPC Generator

### IVXNPCGenerator

Generate NPC personalities, backstories, and dialog:

```csharp
var npc = await IVXNPCGenerator.Instance.GenerateAsync(new NPCGenerationRequest
{
    Role = NPCRole.Merchant,
    Setting = "medieval fantasy village",
    PersonalityTraits = new[] { "greedy", "humorous", "knowledgeable" },
    ImportanceLevel = NPCImportance.Recurring,
    GenerateDialog = true,
    DialogTopics = new[] { "trading", "rumors", "quests" },
});

Debug.Log($"Name: {npc.Name}");
Debug.Log($"Backstory: {npc.Backstory}");
Debug.Log($"Greeting: {npc.Dialog.Greeting}");
Debug.Log($"Trade lines: {npc.Dialog.GetLines("trading").Count}");
```

### Behavior Tree Generation

```csharp
var behaviorTree = await IVXNPCGenerator.Instance.GenerateBehaviorAsync(new BehaviorRequest
{
    NPCType = "patrol_guard",
    Behaviors = new[] { "patrol", "investigate_noise", "chase_player", "return_to_post" },
    AggressionLevel = 0.7f,
});

Debug.Log(behaviorTree.ToJson());
```

---

## 4. Loot Generator

### IVXLootGenerator

Generate loot tables with rarity weights and stat distributions:

```csharp
var lootTable = await IVXLootGenerator.Instance.GenerateTableAsync(new LootTableRequest
{
    Context = "boss_dragon_king",
    PlayerLevel = 25,
    ItemCount = new IntRange(3, 8),
    GuaranteedRarities = new[] { Rarity.Rare },
    RarityWeights = new Dictionary<Rarity, float>
    {
        { Rarity.Common, 0.5f },
        { Rarity.Uncommon, 0.3f },
        { Rarity.Rare, 0.15f },
        { Rarity.Epic, 0.04f },
        { Rarity.Legendary, 0.01f },
    },
});

foreach (var item in lootTable.Items)
{
    Debug.Log($"{item.Rarity} {item.Name}: {item.Description}");
    foreach (var stat in item.Stats)
    {
        Debug.Log($"  {stat.Name}: +{stat.Value}");
    }
}
```

### Item Generation

```csharp
var weapon = await IVXLootGenerator.Instance.GenerateItemAsync(new ItemGenerationRequest
{
    Type = ItemType.Weapon,
    SubType = "sword",
    Rarity = Rarity.Epic,
    Level = 30,
    Theme = "fire",
    GenerateName = true,
    GenerateDescription = true,
    StatRanges = new Dictionary<string, FloatRange>
    {
        { "damage", new FloatRange(120, 180) },
        { "speed", new FloatRange(1.0f, 1.5f) },
        { "fire_damage", new FloatRange(20, 50) },
    },
});
```

---

## 5. Quest Generator

### IVXQuestGenerator

Generate quest chains with narrative coherence:

```csharp
var questChain = await IVXQuestGenerator.Instance.GenerateChainAsync(new QuestChainRequest
{
    Theme = "investigate the disappearances in the village",
    QuestCount = new IntRange(3, 5),
    Difficulty = QuestDifficulty.Medium,
    RewardTier = RewardTier.Moderate,
    NPCsInvolved = new[] { "elder_thomas", "guard_captain" },
    IncludeBranching = true,
    WorldContext = "medieval kingdom under threat from dark forces",
});

foreach (var quest in questChain.Quests)
{
    Debug.Log($"Quest: {quest.Title}");
    Debug.Log($"  Giver: {quest.GiverNPC}");
    Debug.Log($"  Objectives: {quest.Objectives.Count}");
    foreach (var obj in quest.Objectives)
    {
        Debug.Log($"    - {obj.Type}: {obj.Description}");
    }
    Debug.Log($"  Rewards: {string.Join(", ", quest.Rewards)}");
}
```

### Objective Types

| Type | Example | Generated Details |
|------|---------|------------------|
| `Kill` | "Defeat 5 shadow wolves" | Enemy type, count, location |
| `Collect` | "Gather 3 moonstone fragments" | Item, count, drop source |
| `Escort` | "Guide the merchant to the city" | NPC, destination, hazards |
| `Explore` | "Discover the hidden cave" | Location, discovery condition |
| `Dialog` | "Interrogate the innkeeper" | NPC, dialog tree, clues |
| `Puzzle` | "Solve the ancient mechanism" | Puzzle type, solution hints |
| `Defend` | "Protect the village for 3 waves" | Wave count, enemy types |

---

## 6. Asset Generator

### Texture Variations

```csharp
var variations = await IVXAssetGenerator.Instance.GenerateTextureVariationsAsync(
    new TextureVariationRequest
    {
        BaseTexture = baseTexturePath,
        VariationCount = 4,
        Style = "pixel art",
        Modifications = new[] { "recolor to ice theme", "add cracks", "weathered look" },
        OutputSize = new Vector2Int(64, 64),
    }
);

foreach (var texture in variations)
{
    Debug.Log($"Generated: {texture.Name} ({texture.Width}x{texture.Height})");
}
```

### Sprite Generation

```csharp
var sprite = await IVXAssetGenerator.Instance.GenerateSpriteAsync(new SpriteRequest
{
    Description = "medieval sword with fire enchantment, pixel art style",
    Size = new Vector2Int(32, 32),
    Style = "16-bit RPG",
    Provider = ImageProvider.DallE,
});
```

### Music Generation (Stub)

```csharp
var music = await IVXMusicGenerator.Instance.GenerateAsync(new MusicRequest
{
    Mood = "tense exploration",
    Genre = "orchestral",
    DurationSec = 120,
    Provider = MusicProvider.Suno,
});
```

Music generation requires external API access (Suno, Udio). The SDK provides the integration wrapper; you supply the API key.

---

## 7. Deterministic Fallbacks

When AI is unavailable, all generators fall back to seed-based algorithms:

```csharp
IVXPCGConfig.Instance.GenerationMode = PCGMode.Deterministic;

var level = await IVXLevelGenerator.Instance.GenerateAsync(request);
```

Deterministic mode uses:
- **Levels**: Graph grammar + cellular automata with configurable rulesets
- **NPCs**: Name tables + personality templates + markov chain dialog
- **Loot**: Weighted random with seed-based stat rolls
- **Quests**: Template-based with fill-in-the-blank objectives
- **Assets**: Color palette swaps and sprite sheet slicing (no AI images)

---

## 8. Cross-Platform API

| Engine | Class / Module | Generation API |
|--------|---------------|---------------|
| **Unity** | `IVXPCGManager` | Full API (all generators) |
| **Unreal** | `UIVXPCGSubsystem` | Level, NPC, Loot, Quest generators |
| **Godot** | `IVXPCG` autoload | Level, NPC, Loot, Quest generators |
| **JavaScript** | `IVXPCGManager` | Full API (Node.js + browser) |
| **Roblox** | `IVX.PCG` | NPC, Loot, Quest generators |
| **Java** | `IVXPCGManager` | NPC, Loot, Quest generators |
| **Flutter** | `IvxPCG` | NPC, Loot generators |
| **C++** | `IVXPCGManager` | Level, NPC, Loot generators |

---

## Platform-Specific PCG

### VR Platforms

| Generator | VR Guidance |
|-----------|------------|
| **Levels** | Generate room-scale compatible spaces. Min room size 3m x 3m. Include teleport anchor points. |
| **NPCs** | Position NPCs at comfortable conversation distance (1.5m). Generate spatial dialog cues. |
| **Assets** | Generate at higher resolution for VR close-up inspection. |

### Console Platforms

| Platform | PCG Notes |
|----------|----------|
| **All consoles** | Pre-generate and cache content during loading screens to avoid AI latency during gameplay. |
| **Switch** | Use deterministic mode preferentially due to bandwidth constraints. Cache aggressively. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Generation time** | LLM calls add latency. Show loading indicators and generate content ahead of player progress. |
| **Caching** | Cache generated content in `IndexedDB` to avoid regeneration on page reload. |
| **Fallbacks** | Default to deterministic mode for instant generation on slow connections. |

---

## Checklist

- [ ] `EnablePCG` toggled on in `IVXBootstrapConfig`
- [ ] `IVXPCGConfig` created with appropriate generation mode
- [ ] AI provider configured in `IVXAIConfig` (if using CloudAI mode)
- [ ] Level generator tested with target constraints
- [ ] NPC generator producing coherent personalities and dialog
- [ ] Loot tables balanced with appropriate rarity weights
- [ ] Quest generator producing completable quest chains
- [ ] Deterministic fallbacks tested for offline play
- [ ] Content caching configured to reduce redundant API calls
- [ ] VR room-scale constraints applied to level generation (if targeting VR)
- [ ] Console pre-generation strategy implemented (if targeting consoles)
- [ ] WebGL caching and fallback strategy configured (if targeting browser)
