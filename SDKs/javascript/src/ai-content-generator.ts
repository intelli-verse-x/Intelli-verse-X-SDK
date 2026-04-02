// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/** AI configuration matching Unity's IVXAIConfig ScriptableObject. */
export interface IVXAIConfig {
  apiBaseUrl: string;
  apiKey?: string;
  provider?: 'intelliversex' | 'openai' | 'azure' | 'anthropic' | 'custom';
  modelName?: string;
  mockMode?: boolean;
  maxRetries?: number;
  retryBaseDelay?: number;
  rateLimitPerSecond?: number;
  requestTimeout?: number;
  debugLogging?: boolean;
  defaultLanguage?: string;
}

// ---------------------------------------------------------------------------
// Data Models
// ---------------------------------------------------------------------------

/**
 * Describes constraints and hints for procedural quest generation.
 * Mirrors Unity's `IVXQuestTemplate`.
 */
export interface IVXQuestTemplate {
  /** Genre label (e.g. fantasy, sci-fi, horror). */
  genre?: string;
  /** Difficulty band: easy, medium, or hard. */
  difficulty?: string;
  /** Required gameplay or narrative elements (combat, puzzle, dialogue, etc.). */
  requiredElements?: string[];
  /** Approximate play-time in minutes. */
  estimatedDurationMinutes?: number;
  /** Additional free-form instructions for the generator. */
  customPrompt?: string;
}

/**
 * A generated quest definition returned from the content API.
 * Mirrors Unity's `IVXGeneratedQuest`.
 */
export interface IVXGeneratedQuest {
  /** Stable quest identifier from the backend. */
  questId?: string;
  /** Short player-facing title. */
  title?: string;
  /** Longer description or briefing text. */
  description?: string;
  /** Ordered objective strings. */
  objectives?: string[];
  /** Reward descriptions or IDs. */
  rewards?: string[];
  /** Difficulty label. */
  difficulty?: string;
  /** Estimated play-time in minutes. */
  estimatedDuration?: number;
  /** Optional hook line for UI or VO. */
  narrativeHook?: string;
}

/**
 * A generated short story or narrative block.
 * Mirrors Unity's `IVXGeneratedStory`.
 */
export interface IVXGeneratedStory {
  /** Story identifier from the backend. */
  storyId?: string;
  /** Title of the piece. */
  title?: string;
  /** Full story body. */
  body?: string;
  /** Genre label. */
  genre?: string;
  /** Approximate word count. */
  wordCount?: number;
}

/**
 * A generated item definition with stats and flavor text.
 * Mirrors Unity's `IVXGeneratedItem`.
 */
export interface IVXGeneratedItem {
  /** Display name of the item. */
  name?: string;
  /** Item category (weapon, consumable, etc.). */
  itemType?: string;
  /** Rarity tier label. */
  rarity?: string;
  /** Short flavor line. */
  flavorText?: string;
  /** Longer mechanical or lore description. */
  description?: string;
  /** Optional numeric stats keyed by name. */
  stats?: Record<string, number>;
}

/** A single line in a generated dialogue script. */
export interface IVXDialogueLine {
  /** Speaking character name or id. */
  speaker: string;
  /** Spoken text. */
  text: string;
  /** Emotional tone or expression hint. */
  emotion?: string;
  /** Stage direction or animation hint. */
  action?: string;
}

/**
 * A full multi-line dialogue generated for a scenario.
 * Mirrors Unity's `IVXGeneratedDialogue`.
 */
export interface IVXGeneratedDialogue {
  /** Scenario or beat identifier. */
  scenarioId?: string;
  /** Ordered dialogue lines. */
  lines?: IVXDialogueLine[];
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Fired when generation completes successfully; argument is raw JSON content. */
export type OnContentGeneratedCallback = (content: string) => void;

/** Fired when a request fails or JSON parsing fails. */
export type OnContentErrorCallback = (error: string) => void;

// ---------------------------------------------------------------------------
// IVXAIContentGenerator
// ---------------------------------------------------------------------------

/**
 * Procedural content generation: quests, stories, items, and dialogue.
 *
 * Mirrors Unity's `IVXAIContentGenerator` MonoBehaviour — sends generation
 * requests to the IVX content API and returns structured game content.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAIContentGenerator {
  private _config: IVXAIConfig | null = null;

  /** Callback fired when generation completes successfully; argument is raw JSON content. */
  onContentGenerated: OnContentGeneratedCallback | null = null;

  /** Callback fired when a request fails or JSON parsing fails. */
  onError: OnContentErrorCallback | null = null;

  /** True while an HTTP generation request is in flight. */
  get isGenerating(): boolean {
    return false;
  }

  /**
   * Binds API configuration. Required before calling generate methods.
   * @param config - AI configuration (base URL, keys, timeouts).
   */
  initialize(_config: IVXAIConfig): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sets the bearer token applied to content generation HTTP requests.
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Generates a quest from a template and optional player-specific context.
   * Fires {@link onContentGenerated} on success.
   * @param template - Quest constraints and hints (or `null` for defaults).
   * @param playerContext - Optional player/scene context string.
   * @returns Generated quest, or `null` on failure.
   */
  async generateQuest(
    _template: IVXQuestTemplate | null,
    _playerContext?: string,
  ): Promise<IVXGeneratedQuest | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Generates a short story or narrative passage.
   * @param prompt - Generation prompt.
   * @param genre - Genre label (default `"fantasy"`).
   * @param maxWords - Approximate word cap (default 500).
   * @returns Generated story, or `null` on failure.
   */
  async generateStory(
    _prompt: string,
    _genre?: string,
    _maxWords?: number,
  ): Promise<IVXGeneratedStory | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Generates item flavor text, description, and optional stats.
   * @param itemName - Item display name.
   * @param itemType - Category (weapon, consumable, etc.).
   * @param rarity - Rarity tier label.
   * @returns Generated item, or `null` on failure.
   */
  async generateItemDescription(
    _itemName: string,
    _itemType: string,
    _rarity: string,
  ): Promise<IVXGeneratedItem | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Generates a short dialogue script for the given scenario and cast.
   * @param scenario - Scene description or beat.
   * @param characters - Optional character names or ids.
   * @returns Generated dialogue, or `null` on failure.
   */
  async generateDialogue(
    _scenario: string,
    _characters?: string[],
  ): Promise<IVXGeneratedDialogue | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Fills a structured template using AI; completed text is passed as JSON or plain string.
   * @param template - Template string with placeholders.
   * @param variables - Key/value pairs to inject into the template.
   * @returns Generated text, or `null` on failure.
   */
  async generateFromTemplate(
    _template: string,
    _variables?: Record<string, string>,
  ): Promise<string | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /** Aborts the active generation request, if any. */
  cancelGeneration(): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }
}
