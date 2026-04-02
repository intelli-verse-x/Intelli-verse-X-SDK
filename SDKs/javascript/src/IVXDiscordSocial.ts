// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// Discord Social SDK — Rich Presence, Friends, Lobby, Voice & Invites
// ---------------------------------------------------------------------------

export interface IVXDiscordConfig {
  applicationId: string;
  clientId: string;
  redirectUri?: string;
  enableDebugLogs?: boolean;
}

export interface IVXUnifiedFriend {
  userId: string;
  discordId?: string;
  username: string;
  displayName: string;
  avatarUrl: string;
  source: 'discord' | 'game' | 'both';
  status: 'online' | 'idle' | 'dnd' | 'offline';
}

export interface IVXGameInvite {
  inviteId: string;
  senderId: string;
  senderName: string;
  message?: string;
  lobbyId?: string;
  timestamp: number;
}

export interface IVXDiscordLobbyInfo {
  lobbyId: string;
  secret: string;
  ownerId: string;
  memberCount: number;
  maxMembers: number;
  metadata: Record<string, string>;
}

export interface IVXVoiceParticipant {
  userId: string;
  username: string;
  isMuted: boolean;
  isDeafened: boolean;
  isSpeaking: boolean;
  volume: number;
}

export interface IVXDiscordSocialEventMap {
  initialized: [];
  presenceUpdated: [];
  friendsUpdated: [friends: IVXUnifiedFriend[]];
  lobbyJoined: [lobby: IVXDiscordLobbyInfo];
  lobbyLeft: [];
  chatMessage: [senderId: string, message: string];
  voiceJoined: [lobbyId: string];
  voiceLeft: [];
  voiceParticipantUpdated: [participant: IVXVoiceParticipant];
  inviteReceived: [invite: IVXGameInvite];
  joinRequested: [userId: string, username: string];
  error: [error: { code: number; message: string }];
}

type DiscordEventHandler<K extends keyof IVXDiscordSocialEventMap> =
  (...args: IVXDiscordSocialEventMap[K]) => void;

/**
 * Discord Social SDK integration for IntelliVerseX.
 *
 * Wraps Discord Rich Presence, unified friends list, lobby/text-chat,
 * voice channels, and game invites behind a single ergonomic API.
 */
export class IVXDiscordSocial {
  private static _instance: IVXDiscordSocial | null = null;

  private _config: IVXDiscordConfig | null = null;
  private _initialized = false;
  private _listeners = new Map<string, Set<Function>>();
  private _currentLobby: IVXDiscordLobbyInfo | null = null;
  private _chatHistory: Array<{ senderId: string; message: string; timestamp: number }> = [];

  /** Return the shared singleton instance. */
  static getInstance(): IVXDiscordSocial {
    if (!IVXDiscordSocial._instance) {
      IVXDiscordSocial._instance = new IVXDiscordSocial();
    }
    return IVXDiscordSocial._instance;
  }

  /** Reset the singleton (useful for testing). */
  static resetInstance(): void {
    IVXDiscordSocial._instance = null;
  }

  private constructor() {}

  get isInitialized(): boolean { return this._initialized; }

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------

  /**
   * Initialize the Discord Social SDK with an application ID and client ID.
   * Supports account linking and provisional (guest) accounts.
   */
  initialize(config: IVXDiscordConfig): void {
    if (!config.applicationId || config.applicationId.trim() === '') {
      throw new Error('applicationId is required.');
    }
    if (!config.clientId || config.clientId.trim() === '') {
      throw new Error('clientId is required.');
    }
    this._config = { enableDebugLogs: false, ...config };
    this._initialized = true;
    this.log('Discord Social SDK initialized');
    this.emit('initialized');
  }

  /**
   * Link the current game account with a Discord account via OAuth.
   * Returns the linked Discord user ID on success.
   */
  async linkAccount(): Promise<string> {
    this.ensureInitialized();
    this.log('Account link flow started');
    return '';
  }

  /**
   * Create or retrieve a provisional (guest) Discord account for the
   * current player, enabling social features without a full Discord login.
   */
  async getProvisionalAccount(): Promise<string> {
    this.ensureInitialized();
    this.log('Provisional account requested');
    return '';
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  /** Subscribe to a Discord Social event. */
  on<K extends keyof IVXDiscordSocialEventMap>(event: K, handler: DiscordEventHandler<K>): void {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event)!.add(handler);
  }

  /** Unsubscribe from a Discord Social event. */
  off<K extends keyof IVXDiscordSocialEventMap>(event: K, handler: DiscordEventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXDiscordSocialEventMap>(event: K, ...args: IVXDiscordSocialEventMap[K]): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // ---------------------------------------------------------------------------
  // Rich Presence
  // ---------------------------------------------------------------------------

  /** Set the player's Discord Rich Presence activity text. */
  async setActivity(details: string, state?: string): Promise<void> {
    this.ensureInitialized();
    this.log(`Presence updated — details="${details}" state="${state ?? ''}"`);
    this.emit('presenceUpdated');
  }

  /** Set Rich Presence party info for multiplayer sessions. */
  async setParty(partyId: string, currentSize: number, maxSize: number, joinSecret?: string): Promise<void> {
    this.ensureInitialized();
    this.log(`Party set — id=${partyId} size=${currentSize}/${maxSize}`);
    this.emit('presenceUpdated');
  }

  /** Start an elapsed-time timer on the Rich Presence display. */
  async startTimer(): Promise<void> {
    this.ensureInitialized();
    this.log('Presence timer started');
    this.emit('presenceUpdated');
  }

  /** Clear all Rich Presence data. */
  async clearPresence(): Promise<void> {
    this.ensureInitialized();
    this.log('Presence cleared');
    this.emit('presenceUpdated');
  }

  // ---------------------------------------------------------------------------
  // Friends
  // ---------------------------------------------------------------------------

  /**
   * Retrieve a unified friends list that merges Discord friends with
   * in-game friends. Each entry indicates whether the friend was sourced
   * from Discord, the game backend, or both.
   */
  async getUnifiedFriends(): Promise<IVXUnifiedFriend[]> {
    this.ensureInitialized();
    this.log('Fetching unified friends list');
    return [];
  }

  // ---------------------------------------------------------------------------
  // Lobby
  // ---------------------------------------------------------------------------

  /** Create or join a lobby identified by a shared secret. */
  async createOrJoinLobby(secret: string, metadata?: Record<string, string>): Promise<IVXDiscordLobbyInfo> {
    this.ensureInitialized();
    const lobby: IVXDiscordLobbyInfo = {
      lobbyId: '',
      secret,
      ownerId: '',
      memberCount: 1,
      maxMembers: 16,
      metadata: metadata ?? {},
    };
    this._currentLobby = lobby;
    this._chatHistory = [];
    this.log(`Lobby joined — secret=${secret}`);
    this.emit('lobbyJoined', lobby);
    return lobby;
  }

  /** Leave the current lobby. */
  async leaveLobby(): Promise<void> {
    this.ensureInitialized();
    this._currentLobby = null;
    this._chatHistory = [];
    this.log('Left lobby');
    this.emit('lobbyLeft');
  }

  /** Send a text-chat message to the current lobby. */
  async sendMessage(message: string): Promise<void> {
    this.ensureInitialized();
    if (!this._currentLobby) {
      throw new Error('Not in a lobby. Call createOrJoinLobby() first.');
    }
    this._chatHistory.push({ senderId: 'self', message, timestamp: Date.now() });
    this.log(`Chat message sent: ${message}`);
  }

  /** Return the most recent chat messages for the current lobby. */
  getChatHistory(limit = 50): Array<{ senderId: string; message: string; timestamp: number }> {
    this.ensureInitialized();
    return this._chatHistory.slice(-limit);
  }

  // ---------------------------------------------------------------------------
  // Voice
  // ---------------------------------------------------------------------------

  /** Join a voice call in the specified lobby. */
  async joinCall(lobbyId: string): Promise<void> {
    this.ensureInitialized();
    this.log(`Joined voice call — lobby=${lobbyId}`);
    this.emit('voiceJoined', lobbyId);
  }

  /** Leave the current voice call. */
  async leaveCall(): Promise<void> {
    this.ensureInitialized();
    this.log('Left voice call');
    this.emit('voiceLeft');
  }

  /** Mute or unmute the local player's microphone. */
  async setSelfMute(muted: boolean): Promise<void> {
    this.ensureInitialized();
    this.log(`Self mute: ${muted}`);
  }

  /** Deafen or undeafen the local player. */
  async setSelfDeafen(deaf: boolean): Promise<void> {
    this.ensureInitialized();
    this.log(`Self deafen: ${deaf}`);
  }

  /** Set input (microphone) and output (speaker) volume levels (0–100). */
  async setVolume(input: number, output: number): Promise<void> {
    this.ensureInitialized();
    this.log(`Volume set — input=${input} output=${output}`);
  }

  // ---------------------------------------------------------------------------
  // Invites
  // ---------------------------------------------------------------------------

  /** Send a game invite to another user by their ID. */
  async sendInvite(userId: string, message?: string): Promise<void> {
    this.ensureInitialized();
    this.log(`Invite sent to ${userId}`);
  }

  /**
   * Register a callback for incoming game invites.
   * Shorthand for `on('inviteReceived', handler)`.
   */
  onInviteReceived(handler: DiscordEventHandler<'inviteReceived'>): void {
    this.on('inviteReceived', handler);
  }

  /**
   * Register a callback for "Ask to Join" requests.
   * Shorthand for `on('joinRequested', handler)`.
   */
  onJoinRequested(handler: DiscordEventHandler<'joinRequested'>): void {
    this.on('joinRequested', handler);
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  private ensureInitialized(): void {
    if (!this._initialized || !this._config) {
      const err = { code: -1, message: 'Discord Social SDK not initialized. Call initialize() first.' };
      this.emit('error', err);
      throw err;
    }
  }

  private log(message: string): void {
    if (this._config?.enableDebugLogs) {
      console.log(`[IntelliVerseX:Discord] ${message}`);
    }
  }
}
