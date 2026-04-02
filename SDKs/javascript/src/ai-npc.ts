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
// NPC Dialog Types
// ---------------------------------------------------------------------------

/** High-level state of an NPC dialog session. */
export enum IVXAINPCDialogState {
  Active = 'Active',
  WaitingForPlayer = 'WaitingForPlayer',
  WaitingForNPC = 'WaitingForNPC',
  Ended = 'Ended',
}

/**
 * Runtime profile describing an NPC participant in the dialog system.
 * Mirrors Unity's `IVXAINPCProfile` (persona, RAG, tools, model params).
 */
export interface IVXAINPCProfile {
  /** Stable identifier for this NPC (matches backend / routing). */
  npcId: string;
  /** Human-readable name shown in UI. */
  displayName?: string;
  /** System prompt defining personality and behaviour for the language model. */
  personaPrompt?: string;
  /** Document identifiers for retrieval-augmented generation (RAG). */
  knowledgeBaseIds?: string[];
  /** Voice identifier for text-to-speech integration. */
  voiceId?: string;
  /** Maximum dialog turns; use 0 for unlimited. */
  maxTurns?: number;
  /** Names of tools or game actions this NPC is allowed to invoke. */
  availableActions?: string[];
  /** Maximum tokens the model may generate per turn. */
  maxTokens?: number;
  /** Sampling temperature (0–2). */
  temperature?: number;
  /** Model name override (e.g. `gpt-4o`, `claude-3-opus`). */
  model?: string;
}

/**
 * A server- or client-side action associated with an NPC turn
 * (e.g. give_item, start_quest, open_shop).
 */
export interface IVXAINPCAction {
  /** Action key, e.g. `give_item`, `start_quest`, `open_shop`. */
  actionName: string;
  /** JSON-encoded parameters for the action. */
  actionPayload?: string;
  /** Whether the game client has executed this action. */
  executed?: boolean;
}

/** A single turn in the NPC dialog history. */
export interface IVXAINPCDialogMessage {
  /** Message role: typically `"player"` or `"npc"`. */
  role: string;
  /** Plain-text content of the message. */
  content: string;
  /** Unix-milliseconds timestamp. */
  timestamp?: number;
  /** Optional structured action (tool call); null if none. */
  action?: IVXAINPCAction | null;
}

/**
 * Server-backed NPC dialog session: identity, state, and transcript.
 * Mirrors Unity's `IVXAINPCDialogSession`.
 */
export interface IVXAINPCDialogSession {
  /** Backend session identifier. */
  sessionId: string;
  /** NPC this session belongs to. */
  npcId: string;
  /** Player / user identifier. */
  playerId: string;
  /** Current conversational state. */
  state?: IVXAINPCDialogState;
  /** Number of completed turns (server and client may both update). */
  turnCount?: number;
  /** Session start time (Unix milliseconds). */
  startTimestamp?: number;
  /** Ordered message history for this session. */
  history?: IVXAINPCDialogMessage[];
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Callback when an NPC produces a natural-language reply. */
export type OnNPCResponseCallback = (sessionId: string, response: string) => void;

/** Callback when an NPC triggers a game action (tool call). */
export type OnNPCActionCallback = (sessionId: string, action: IVXAINPCAction) => void;

/** Callback when a dialog session begins. */
export type OnDialogStartedCallback = (session: IVXAINPCDialogSession) => void;

/** Callback when a dialog session ends. */
export type OnDialogEndedCallback = (sessionId: string) => void;

/** Callback on network or parse errors. */
export type OnNPCErrorCallback = (error: string) => void;

// ---------------------------------------------------------------------------
// NPC Dialog Manager
// ---------------------------------------------------------------------------

/**
 * NPC dialog registration and HTTP session traffic.
 *
 * Mirrors Unity's `IVXAINPCDialogManager` — registers NPC profiles, creates
 * server-backed dialog sessions, and routes player messages.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAINPCDialogManager {
  private _config: IVXAIConfig | null = null;

  /** Callback fired when an NPC produces a natural-language reply. */
  onNPCResponse: OnNPCResponseCallback | null = null;

  /** Callback fired when an NPC triggers a game action (tool call). */
  onNPCAction: OnNPCActionCallback | null = null;

  /** Callback fired when a dialog session begins. */
  onDialogStarted: OnDialogStartedCallback | null = null;

  /** Callback fired when a dialog session ends. */
  onDialogEnded: OnDialogEndedCallback | null = null;

  /** Callback fired on network or parse errors. */
  onError: OnNPCErrorCallback | null = null;

  /** Whether the manager has been successfully initialized. */
  get isInitialized(): boolean {
    return false;
  }

  /**
   * Binds AI configuration for NPC dialog HTTP calls.
   * @param config - AI configuration (base URL, keys, timeouts).
   */
  initialize(_config: IVXAIConfig): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sets the bearer token applied to NPC dialog HTTP requests.
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Registers an NPC profile so dialog sessions can be started for it.
   * @param profile - Full NPC profile definition.
   */
  registerNPC(_profile: IVXAINPCProfile): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Removes a previously registered NPC.
   * @param npcId - Identifier of the NPC to unregister.
   */
  unregisterNPC(_npcId: string): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Creates a new dialog session between a player and a registered NPC.
   * @param npcId - NPC identifier (must be registered first).
   * @param playerId - Player / user identifier.
   * @param playerContext - Optional free-form context about the player or scene.
   * @returns The new session, or `null` on failure.
   */
  async startDialog(
    _npcId: string,
    _playerId: string,
    _playerContext?: string,
  ): Promise<IVXAINPCDialogSession | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sends a player message within an active dialog session.
   * @param sessionId - Active session identifier.
   * @param message - Player's message text.
   * @returns The NPC's natural-language reply, or `null` on failure.
   */
  async sendMessage(
    _sessionId: string,
    _message: string,
  ): Promise<string | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Ends an active dialog session.
   * @param sessionId - Session to terminate.
   */
  async endDialog(_sessionId: string): Promise<void> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Returns the cached session object for a given session id.
   * @param sessionId - Session identifier.
   * @returns Session data, or `null` if not found.
   */
  getSession(_sessionId: string): IVXAINPCDialogSession | null {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Returns all active sessions for a given NPC.
   * @param npcId - NPC identifier.
   * @returns Array of active sessions (may be empty).
   */
  getSessionsForNPC(_npcId: string): IVXAINPCDialogSession[] {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }
}
