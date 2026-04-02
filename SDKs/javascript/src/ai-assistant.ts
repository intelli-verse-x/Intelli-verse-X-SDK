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
  /** Max conversation history lines kept before oldest entries are trimmed. */
  maxConversationHistory?: number;
}

// ---------------------------------------------------------------------------
// Data Models
// ---------------------------------------------------------------------------

/**
 * Serializable snapshot of in-game state sent to the assistant API for grounded answers.
 * Mirrors Unity's `IVXAIGameContext`.
 */
export interface IVXAIGameContext {
  /** Current level or map identifier. */
  currentLevel?: string;
  /** What the player is trying to accomplish right now. */
  currentObjective?: string;
  /** Coarse game phase label, e.g. `tutorial` or `mid_game`. */
  gamePhase?: string;
  /** Inventory item ids or display names. */
  inventory?: string[];
  /** Numeric stats (health, score, etc.) keyed by name. */
  playerStats?: Record<string, number>;
  /** Additional JSON payload for custom integrations. */
  customContext?: string;
}

/**
 * Response from the general `assistant/ask` endpoint.
 * Mirrors Unity's `IVXAIAssistantResponse`.
 */
export interface IVXAIAssistantResponse {
  /** Full natural-language answer. */
  response: string;
  /** Optional citation or document ids supporting the answer. */
  sources?: string[];
  /** Model self-reported confidence in [0, 1] if provided. */
  confidence?: number;
  /** True when the reply was produced in streaming mode on the server. */
  isStreaming?: boolean;
}

/**
 * Response from the contextual hint endpoint.
 * Mirrors Unity's `IVXAIHintResponse`.
 */
export interface IVXAIHintResponse {
  /** Short hint text. */
  hint: string;
  /** Difficulty or spoiler level of the hint. */
  difficultyLevel?: string;
  /** Whether another hint can be requested. */
  nextHintAvailable?: boolean;
}

/** One step in a guided tutorial sequence. */
export interface IVXAITutorialStep {
  /** 1-based or 0-based step index from the server. */
  stepNumber: number;
  /** Short title for the step. */
  title: string;
  /** Body copy explaining the step. */
  description: string;
  /** Optional UI or input action id required to advance. */
  actionRequired?: string;
}

/**
 * Structured tutorial flow for a feature.
 * Mirrors Unity's `IVXAITutorialResponse`.
 */
export interface IVXAITutorialResponse {
  /** Feature or screen identifier. */
  featureId: string;
  /** Ordered tutorial steps. */
  steps: IVXAITutorialStep[];
  /** Estimated duration in seconds. */
  estimatedTimeSeconds?: number;
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Fired when a full assistant response is available. */
export type OnResponseReceivedCallback = (response: string) => void;

/** Fired for streaming chunks (or the full response for single-shot HTTP). */
export type OnStreamingChunkCallback = (chunk: string) => void;

/** Fired on network or parse errors. */
export type OnAssistantErrorCallback = (error: string) => void;

// ---------------------------------------------------------------------------
// IVXAIAssistant
// ---------------------------------------------------------------------------

/**
 * In-game AI assistant: hints, tutorials, Q&A, and knowledge search.
 *
 * Mirrors Unity's `IVXAIAssistant` MonoBehaviour — sends questions and game
 * context to the IVX assistant HTTP API and returns grounded answers.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAIAssistant {
  private _config: IVXAIConfig | null = null;

  /** Optional system prompt overriding default assistant behaviour (sent on each ask). */
  systemPrompt: string | undefined;

  /** Callback fired when a full assistant response is available. */
  onResponseReceived: OnResponseReceivedCallback | null = null;

  /** Callback fired when the server marks the reply as streaming; emits chunk text. */
  onStreamingChunk: OnStreamingChunkCallback | null = null;

  /** Callback fired on network or parse errors. */
  onError: OnAssistantErrorCallback | null = null;

  /** True while a request is in flight. */
  get isProcessing(): boolean {
    return false;
  }

  /** True after {@link initialize} succeeds. */
  get isInitialized(): boolean {
    return false;
  }

  /**
   * Binds AI configuration for assistant HTTP calls (base URL, keys, timeouts, history limits).
   * @param config - AI configuration. `maxConversationHistory` controls local history trimming.
   */
  initialize(_config: IVXAIConfig): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sets the bearer token applied to assistant HTTP requests (`Authorization` header).
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /** Clears locally tracked conversation lines used with {@link ask}. */
  clearHistory(): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sets the system prompt (same as assigning `systemPrompt`).
   * @param prompt - System prompt text.
   */
  setSystemPrompt(_prompt: string): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Asks a general question with optional game context.
   * Fires {@link onResponseReceived} and/or {@link onStreamingChunk} on success.
   * @param question - Natural-language question.
   * @param context - Optional snapshot of in-game state for grounded answers.
   * @returns Parsed assistant response, or `null` on failure.
   */
  async ask(
    _question: string,
    _context?: IVXAIGameContext,
  ): Promise<IVXAIAssistantResponse | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Requests a contextual hint for the current level/objective.
   * @param levelId - Current level identifier.
   * @param objectiveId - Current objective identifier.
   * @param context - Optional game context for richer hints.
   * @returns Hint response, or `null` on failure.
   */
  async getHint(
    _levelId: string,
    _objectiveId: string,
    _context?: IVXAIGameContext,
  ): Promise<IVXAIHintResponse | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Fetches a structured tutorial for a feature id.
   * @param featureId - Feature or screen identifier.
   * @returns Tutorial response with ordered steps, or `null` on failure.
   */
  async getTutorial(
    _featureId: string,
  ): Promise<IVXAITutorialResponse | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Performs a knowledge-base (RAG) search and returns result snippets or ids.
   * @param query - Search query.
   * @returns Array of result strings (may be empty).
   */
  async searchKnowledgeBase(_query: string): Promise<string[]> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }
}
