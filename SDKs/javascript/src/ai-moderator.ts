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
// Enums
// ---------------------------------------------------------------------------

/** High-level classification bucket for user-generated text. */
export enum IVXContentCategory {
  /** No policy issues detected. */
  Clean = 'Clean',
  /** General toxic or abusive language. */
  Toxic = 'Toxic',
  /** Unwanted or repetitive promotional content. */
  Spam = 'Spam',
  /** Personally identifiable information. */
  PII = 'PII',
  /** Harassing or bullying language. */
  Harassment = 'Harassment',
  /** Hate speech or slurs. */
  HateSpeech = 'HateSpeech',
  /** Self-harm or suicide-related content. */
  SelfHarm = 'SelfHarm',
  /** Sexual content. */
  Sexual = 'Sexual',
  /** Graphic violence or threats. */
  Violence = 'Violence',
  /** Custom or vendor-specific category. */
  Custom = 'Custom',
}

/** Estimated severity of a policy violation. */
export enum IVXModerationSeverity {
  /** No violation. */
  None = 'None',
  /** Mild concern. */
  Low = 'Low',
  /** Moderate concern. */
  Medium = 'Medium',
  /** Strong concern. */
  High = 'High',
  /** Immediate safety or legal risk. */
  Critical = 'Critical',
}

/** Suggested moderation outcome. */
export enum IVXModerationActionType {
  /** Allow the message as-is. */
  Allow = 'Allow',
  /** Allow but show a warning to the user. */
  Warn = 'Warn',
  /** Substitute sanitized text. */
  Replace = 'Replace',
  /** Reject the message. */
  Block = 'Block',
  /** Send to human or async review. */
  Flag = 'Flag',
}

// ---------------------------------------------------------------------------
// Data Models
// ---------------------------------------------------------------------------

/**
 * Result of classifying or scanning a single piece of text.
 * Mirrors Unity's `IVXModerationResult`.
 */
export interface IVXModerationResult {
  /** Assigned content category. */
  category: IVXContentCategory;
  /** Severity estimate. */
  severity: IVXModerationSeverity;
  /** Model confidence in [0, 1]. */
  confidence: number;
  /** Recommended action for the client or human reviewer. */
  suggestedAction: IVXModerationActionType;
  /** Sanitized replacement when `suggestedAction` is `Replace`. */
  replacement: string;
  /** Original user text that was evaluated. */
  originalText: string;
}

/**
 * A client-defined rule evaluated locally before remote classification.
 * Mirrors Unity's `IVXModerationRule`.
 */
export interface IVXModerationRule {
  /** Regular expression pattern, or literal keyword if not valid regex. */
  pattern: string;
  /** Category to assign when this rule matches. */
  category: IVXContentCategory;
  /** Action to suggest when this rule matches. */
  action: IVXModerationActionType;
  /** Replacement text for `Replace` actions. */
  replacementText?: string;
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Fired when content should be reviewed or flagged (category is not Clean). */
export type OnContentFlaggedCallback = (result: IVXModerationResult) => void;

/** Fired when content is blocked; provides original text and reason. */
export type OnContentBlockedCallback = (originalText: string, reason: string) => void;

/** Fired when content is replaced; provides original and replacement text. */
export type OnContentReplacedCallback = (originalText: string, replacement: string) => void;

// ---------------------------------------------------------------------------
// IVXAIModerator
// ---------------------------------------------------------------------------

/**
 * Text moderation: local rules plus remote classify / filter / batch APIs.
 *
 * Mirrors Unity's `IVXAIModerator` MonoBehaviour — evaluates user-generated
 * text against custom rules locally and/or the remote moderation service.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAIModerator {
  private _config: IVXAIConfig | null = null;

  /** Callback fired when content should be reviewed or flagged. */
  onContentFlagged: OnContentFlaggedCallback | null = null;

  /** Callback fired when content is blocked; provides original text and reason. */
  onContentBlocked: OnContentBlockedCallback | null = null;

  /** Callback fired when content is replaced; provides original and replacement. */
  onContentReplaced: OnContentReplacedCallback | null = null;

  /** Whether remote moderation calls are allowed. */
  get isEnabled(): boolean {
    return false;
  }

  /** Active custom rules (read-only view). */
  get customRules(): readonly IVXModerationRule[] {
    return [];
  }

  /**
   * Binds configuration and marks the moderator ready for use.
   * @param config - API configuration; must not be null.
   */
  initialize(_config: IVXAIConfig): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Sets the bearer token applied to moderation HTTP requests.
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Classifies a single string. Local rules run first; the API is used when no local rule applies.
   * Fires {@link onContentFlagged}, {@link onContentBlocked}, or {@link onContentReplaced} as appropriate.
   * @param text - Text to classify.
   * @returns Classification result.
   */
  async classifyText(_text: string): Promise<IVXModerationResult> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Applies local replacement rules, then calls the remote filter endpoint for final sanitization.
   * @param text - Text to filter.
   * @returns Sanitized text string.
   */
  async filterMessage(_text: string): Promise<string> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Scores multiple messages in one HTTP request. Results align by index with the input.
   * @param messages - Array of messages to scan.
   * @returns Per-message moderation results.
   */
  async scanBatch(_messages: string[]): Promise<IVXModerationResult[]> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Adds a custom rule and refreshes local matchers.
   * @param rule - Rule definition.
   */
  addCustomRule(_rule: IVXModerationRule): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Removes the first rule whose pattern matches exactly.
   * @param pattern - Pattern string to remove.
   */
  removeCustomRule(_pattern: string): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Replaces the full custom rule list.
   * @param rules - New rule set.
   */
  setCustomRules(_rules: IVXModerationRule[]): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /** Clears all custom rules. */
  clearCustomRules(): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Fast synchronous scan using configured custom rules only (no HTTP).
   * @param text - Text to check.
   * @returns Moderation result based on local rules.
   */
  checkLocalRules(_text: string): IVXModerationResult {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Maps a moderation result into Discord-oriented metadata key/value pairs.
   * @param result - Moderation result to convert.
   * @returns Key/value map suitable for Discord moderation metadata.
   */
  getDiscordModerationMetadata(
    _result: IVXModerationResult,
  ): Record<string, string> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }
}
