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
  /** Max queued profiling events before oldest are dropped. */
  maxEventQueueSize?: number;
}

// ---------------------------------------------------------------------------
// Enums
// ---------------------------------------------------------------------------

/**
 * High-level player segmentation used for profiling and personalization.
 * Mirrors Unity's `IVXPlayerCohort`.
 */
export enum IVXPlayerCohort {
  /** Casual engagement pattern. */
  Casual = 'Casual',
  /** Socially driven players. */
  Social = 'Social',
  /** Competitive-focused players. */
  Competitive = 'Competitive',
  /** Exploration-oriented players. */
  Explorer = 'Explorer',
  /** Achievement collectors. */
  Achiever = 'Achiever',
  /** High-value spenders. */
  Whale = 'Whale',
  /** Players showing disengagement signals. */
  AtRisk = 'AtRisk',
  /** Recently onboarded players. */
  NewPlayer = 'NewPlayer',
  /** Long-term retained players. */
  Veteran = 'Veteran',
  /** Previously active, now inactive. */
  Lapsed = 'Lapsed',
}

// ---------------------------------------------------------------------------
// Data Models
// ---------------------------------------------------------------------------

/**
 * Cached player profiling snapshot for gameplay and UI personalization.
 * Mirrors Unity's `IVXPlayerProfile` with all backend fields.
 */
export interface IVXPlayerProfile {
  /** Stable player identifier. */
  playerId: string;
  /** Assigned cohort. */
  cohort: IVXPlayerCohort;
  /** Engagement score from 0–100. */
  engagementScore: number;
  /** Churn risk from 0–1. */
  churnRiskScore: number;
  /** Likelihood to monetize from 0–1. */
  monetizationPropensity: number;
  /** Total sessions observed. */
  totalSessionCount: number;
  /** Average session length in minutes. */
  avgSessionDurationMinutes: number;
  /** Favourite game modes. */
  preferredGameModes: string[];
  /** Favourite product features. */
  preferredFeatures: string[];
  /** Unix milliseconds of last activity. */
  lastActiveTimestamp: number;
  /** Custom scalar metrics keyed by name. */
  customMetrics: Record<string, number>;
}

/**
 * A single personalization hint returned by the profiling service.
 * Mirrors Unity's `IVXPersonalizationHint`.
 */
export interface IVXPersonalizationHint {
  /** Hint category, e.g. `recommend_mode`, `offer_discount`. */
  hintType: string;
  /** Feature or surface to act on. */
  targetFeature: string;
  /** Human-readable message for UI. */
  message: string;
  /** Relative priority from 0–1. */
  priority: number;
  /** Additional string parameters. */
  parameters?: Record<string, string>;
}

/** Churn prediction result with risk score and explanatory factors. */
export interface IVXChurnPrediction {
  /** Risk score from 0–1. */
  score: number;
  /** Human-readable contributing factors. */
  factors: string[];
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Fired when the cached profile is refreshed from the backend. */
export type OnProfileUpdatedCallback = (profile: IVXPlayerProfile) => void;

/** Fired when churn risk assessment completes. */
export type OnChurnRiskAssessedCallback = (score: number, factors: string[]) => void;

/** Fired when personalization hints are received. */
export type OnPersonalizationReadyCallback = (hints: IVXPersonalizationHint[]) => void;

// ---------------------------------------------------------------------------
// IVXAIProfiler
// ---------------------------------------------------------------------------

/**
 * Player profiling: event tracking, profile fetch, personalization, and churn prediction.
 *
 * Mirrors Unity's `IVXAIProfiler` MonoBehaviour — queues and sends analytics
 * events, fetches player profiles, and retrieves personalization signals.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAIProfiler {
  private _config: IVXAIConfig | null = null;

  /** Callback fired when the cached profile is refreshed. */
  onProfileUpdated: OnProfileUpdatedCallback | null = null;

  /** Callback fired when churn risk assessment completes. */
  onChurnRiskAssessed: OnChurnRiskAssessedCallback | null = null;

  /** Callback fired when personalization hints are received. */
  onPersonalizationReady: OnPersonalizationReadyCallback | null = null;

  /** True while automatic session/event tracking is enabled. */
  get isTracking(): boolean {
    return false;
  }

  /** Latest profile returned by the backend, or `null`. */
  get cachedProfile(): IVXPlayerProfile | null {
    return null;
  }

  /**
   * Binds configuration and player identity. Required before tracking or API calls.
   * @param config - AI configuration asset. `maxEventQueueSize` controls queue depth.
   * @param playerId - Current player identifier.
   */
  initialize(_config: IVXAIConfig, _playerId: string): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Sets the bearer token applied to profiling HTTP requests.
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Enqueues a profiling event for batched delivery.
   * @param eventName - Logical event name (e.g. `level_complete`, `purchase`).
   * @param data - Optional properties.
   */
  trackEvent(_eventName: string, _data?: Record<string, unknown>): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /** Sends all queued events immediately. */
  flushEvents(): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Fetches the latest player profile from the backend and updates {@link cachedProfile}.
   * Fires {@link onProfileUpdated} on success.
   * @returns Player profile, or `null` on failure.
   */
  async getPlayerProfile(): Promise<IVXPlayerProfile | null> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Requests personalization hints for the current player.
   * Fires {@link onPersonalizationReady} on success.
   * @returns Array of hints (may be empty).
   */
  async getPersonalizationHints(): Promise<IVXPersonalizationHint[]> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Resolves the player's cohort (typically via profile).
   * @returns Assigned cohort value.
   */
  async classifyPlayer(): Promise<IVXPlayerCohort> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /**
   * Requests churn risk and explanatory factors.
   * Fires {@link onChurnRiskAssessed} on success.
   * @returns Object with `score` (0–1) and `factors` array.
   */
  async predictChurn(): Promise<IVXChurnPrediction> {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /** Starts lightweight automatic tracking (session markers and periodic flush). */
  startAutoTracking(): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }

  /** Stops automatic tracking and flushes queued events. */
  stopAutoTracking(): void {
    console.warn('[IVX-JS] stub – not yet implemented');
    throw new Error('[IVX-JS] stub – not yet implemented');
  }
}
