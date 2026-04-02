// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// AI Client — Voice companion & AI host interactions (Web3 SDK)
// ---------------------------------------------------------------------------

export interface IVXAIConfig {
  apiBaseUrl: string;
  apiKey: string;
  enableDebugLogs?: boolean;
}

export interface IVXAIPersona {
  id: string;
  name: string;
  description: string;
  avatarUrl: string;
  supportedLanguages: string[];
}

export interface IVXAIMessage {
  id: string;
  sessionId: string;
  role: 'user' | 'assistant';
  text: string;
  timestamp: number;
  metadata?: Record<string, unknown>;
}

export interface IVXAISessionResponse {
  sessionId: string;
  personaId: string;
  userId: string;
  status: 'active' | 'ended';
  createdAt: number;
}

export interface IVXAIEntitlement {
  userId: string;
  entitled: boolean;
  remainingCredits: number;
  plan: string;
}

export interface IVXAIHostProfile {
  displayName: string;
  avatarUrl?: string;
  metadata?: Record<string, unknown>;
}

export interface IVXAIEventMap {
  initialized: [];
  sessionStarted: [session: IVXAISessionResponse];
  sessionEnded: [sessionId: string];
  messageReceived: [message: IVXAIMessage];
  error: [error: { code: number; message: string }];
}

type AIEventHandler<K extends keyof IVXAIEventMap> = (...args: IVXAIEventMap[K]) => void;

/**
 * AI client for IntelliVerseX Web3 SDK.
 *
 * Provides voice companion sessions, AI host sessions, entitlement checks,
 * and persona queries against the IntelliVerseX AI REST API.
 */
export class IVXAIClient {
  private static _instance: IVXAIClient | null = null;

  private _config: IVXAIConfig | null = null;
  private _initialized = false;
  private _listeners = new Map<string, Set<Function>>();

  /** Return the shared singleton instance. */
  static getInstance(): IVXAIClient {
    if (!IVXAIClient._instance) {
      IVXAIClient._instance = new IVXAIClient();
    }
    return IVXAIClient._instance;
  }

  /** Reset the singleton (useful for testing). */
  static resetInstance(): void {
    IVXAIClient._instance = null;
  }

  private constructor() {}

  get isInitialized(): boolean { return this._initialized; }

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------

  /**
   * Configure the AI client with an API base URL and key.
   * Must be called before any other method.
   */
  initialize(config: IVXAIConfig): void {
    if (!config.apiBaseUrl || config.apiBaseUrl.trim() === '') {
      throw new Error('apiBaseUrl is required.');
    }
    if (!config.apiKey || config.apiKey.trim() === '') {
      throw new Error('apiKey is required.');
    }
    this._config = { enableDebugLogs: false, ...config };
    this._initialized = true;
    this.log('AI client initialized');
    this.emit('initialized');
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  /** Subscribe to an AI client event. */
  on<K extends keyof IVXAIEventMap>(event: K, handler: AIEventHandler<K>): void {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event)!.add(handler);
  }

  /** Unsubscribe from an AI client event. */
  off<K extends keyof IVXAIEventMap>(event: K, handler: AIEventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXAIEventMap>(event: K, ...args: IVXAIEventMap[K]): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // ---------------------------------------------------------------------------
  // Voice Sessions
  // ---------------------------------------------------------------------------

  /** Start a new AI voice session for the given persona and user. */
  async startVoiceSession(
    personaId: string,
    userId: string,
    language?: string,
  ): Promise<IVXAISessionResponse> {
    this.ensureInitialized();
    const body: Record<string, unknown> = { personaId, userId };
    if (language) body.language = language;

    const session = await this.post<IVXAISessionResponse>('/ai-voice/session', body);
    this.log(`Voice session started: ${session.sessionId}`);
    this.emit('sessionStarted', session);
    return session;
  }

  /** End an active voice session. */
  async endVoiceSession(sessionId: string): Promise<void> {
    this.ensureInitialized();
    await this.post<void>(`/ai-voice/session/${sessionId}/end`, {});
    this.log(`Voice session ended: ${sessionId}`);
    this.emit('sessionEnded', sessionId);
  }

  /** Send a text message within a voice session. */
  async sendText(sessionId: string, text: string): Promise<IVXAIMessage> {
    this.ensureInitialized();
    return this.post<IVXAIMessage>(`/ai-voice/session/${sessionId}/text`, { text });
  }

  /** Poll for new messages since `lastTimestamp`. */
  async pollMessages(sessionId: string, lastTimestamp?: number): Promise<IVXAIMessage[]> {
    this.ensureInitialized();
    const params = new URLSearchParams();
    if (lastTimestamp !== undefined) params.set('since', String(lastTimestamp));
    const qs = params.toString();
    const path = `/ai-voice/session/${sessionId}/messages${qs ? `?${qs}` : ''}`;

    const messages = await this.get<IVXAIMessage[]>(path);
    for (const msg of messages) {
      this.emit('messageReceived', msg);
    }
    return messages;
  }

  // ---------------------------------------------------------------------------
  // AI Host
  // ---------------------------------------------------------------------------

  /** Start an AI-hosted session for a match. */
  async startHostSession(
    matchId: string,
    profile: IVXAIHostProfile,
  ): Promise<IVXAISessionResponse> {
    this.ensureInitialized();
    const session = await this.post<IVXAISessionResponse>('/ai-host/session', { matchId, profile });
    this.log(`Host session started: ${session.sessionId}`);
    this.emit('sessionStarted', session);
    return session;
  }

  /** Send a game event to an AI host session. */
  async sendHostEvent(
    sessionId: string,
    eventType: string,
    data: Record<string, unknown>,
  ): Promise<void> {
    this.ensureInitialized();
    await this.post<void>(`/ai-host/session/${sessionId}/event`, { eventType, data });
    this.log(`Host event sent: ${eventType}`);
  }

  // ---------------------------------------------------------------------------
  // Entitlements & Personas
  // ---------------------------------------------------------------------------

  /** Check whether a user is entitled to AI voice features. */
  async checkEntitlement(userId: string): Promise<IVXAIEntitlement> {
    this.ensureInitialized();
    return this.get<IVXAIEntitlement>(`/ai-voice/entitlement/${userId}`);
  }

  /** Retrieve the full list of available AI personas. */
  async getPersonas(): Promise<IVXAIPersona[]> {
    this.ensureInitialized();
    return this.get<IVXAIPersona[]>('/ai-voice/personas');
  }

  // ---------------------------------------------------------------------------
  // HTTP helpers
  // ---------------------------------------------------------------------------

  private async post<T>(path: string, body: unknown): Promise<T> {
    return this.request<T>('POST', path, body);
  }

  private async get<T>(path: string): Promise<T> {
    return this.request<T>('GET', path);
  }

  private async request<T>(method: string, path: string, body?: unknown): Promise<T> {
    const url = `${this._config!.apiBaseUrl}${path}`;
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this._config!.apiKey}`,
    };

    const init: RequestInit = { method, headers };
    if (body !== undefined) {
      init.body = JSON.stringify(body);
    }

    let res: Response;
    try {
      res = await fetch(url, init);
    } catch (e: unknown) {
      const error = { code: -1, message: e instanceof Error ? e.message : String(e) };
      this.emit('error', error);
      throw error;
    }

    if (!res.ok) {
      let message = `HTTP ${res.status}`;
      try {
        const json = await res.json();
        if (json?.message) message = json.message;
      } catch { /* use status text */ }
      const error = { code: res.status, message };
      this.emit('error', error);
      throw error;
    }

    const text = await res.text();
    if (!text) return undefined as unknown as T;
    return JSON.parse(text) as T;
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  private ensureInitialized(): void {
    if (!this._initialized || !this._config) {
      const err = { code: -1, message: 'AI client not initialized. Call initialize() first.' };
      this.emit('error', err);
      throw err;
    }
  }

  private log(message: string): void {
    if (this._config?.enableDebugLogs) {
      console.log(`[IntelliVerseX Web3:AI] ${message}`);
    }
  }
}
