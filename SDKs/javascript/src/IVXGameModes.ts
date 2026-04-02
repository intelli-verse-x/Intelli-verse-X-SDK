// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// Game Modes — Mode selection, lobby, matchmaking
// ---------------------------------------------------------------------------

export const IVXGameMode = {
  SOLO: 'solo',
  LOCAL_MULTIPLAYER: 'local_multiplayer',
  ONLINE_VERSUS: 'online_versus',
  ONLINE_COOP: 'online_coop',
  RANKED: 'ranked',
  TURN_BASED: 'turn_based',
} as const;

export type IVXGameMode = (typeof IVXGameMode)[keyof typeof IVXGameMode];

export interface IVXPlayerSlot {
  index: number;
  name: string;
  isLocal: boolean;
  ready: boolean;
}

export interface IVXMatchConfig {
  mode: IVXGameMode;
  maxPlayers?: number;
  metadata?: Record<string, unknown>;
}

export interface IVXRoomConfig {
  maxPlayers?: number;
  isPrivate?: boolean;
  password?: string;
  metadata?: Record<string, unknown>;
}

export interface IVXRoomInfo {
  roomId: string;
  name: string;
  hostName: string;
  playerCount: number;
  maxPlayers: number;
  isPrivate: boolean;
  metadata?: Record<string, unknown>;
}

export interface IVXMatchResult {
  matchId: string;
  mode: IVXGameMode;
  players: IVXPlayerSlot[];
  startedAt: number;
  endedAt: number;
  metadata?: Record<string, unknown>;
}

export interface IVXRoomFilter {
  mode?: IVXGameMode;
  hasSlots?: boolean;
  query?: string;
}

export interface IVXGameModesEventMap {
  modeChanged: [mode: IVXGameMode];
  playerAdded: [slot: IVXPlayerSlot];
  playerRemoved: [slotIndex: number];
  playerReady: [slotIndex: number, ready: boolean];
  matchStarted: [matchId: string];
  matchEnded: [result: IVXMatchResult];
  matchFound: [matchId: string, players: IVXPlayerSlot[]];
  matchmakingCancelled: [];
  roomCreated: [room: IVXRoomInfo];
  roomJoined: [room: IVXRoomInfo];
  roomLeft: [];
  roomListUpdated: [rooms: IVXRoomInfo[]];
  error: [error: { code: number; message: string }];
}

type ModeEventHandler<K extends keyof IVXGameModesEventMap> =
  (...args: IVXGameModesEventMap[K]) => void;

export class IVXGameModes {
  private static _instance: IVXGameModes | null = null;

  private _currentMode: IVXGameMode = IVXGameMode.SOLO;
  private _maxPlayers = 1;
  private _players: IVXPlayerSlot[] = [];
  private _matchId: string | null = null;
  private _currentRoom: IVXRoomInfo | null = null;
  private _searching = false;
  private _listeners = new Map<string, Set<Function>>();

  /** Return the shared singleton instance. */
  static getInstance(): IVXGameModes {
    if (!IVXGameModes._instance) {
      IVXGameModes._instance = new IVXGameModes();
    }
    return IVXGameModes._instance;
  }

  /** Reset the singleton (useful for testing). */
  static resetInstance(): void {
    IVXGameModes._instance = null;
  }

  private constructor() {}

  // ---------------------------------------------------------------------------
  // Getters
  // ---------------------------------------------------------------------------

  get currentMode(): IVXGameMode { return this._currentMode; }
  get maxPlayers(): number { return this._maxPlayers; }
  get players(): ReadonlyArray<IVXPlayerSlot> { return this._players; }
  get matchId(): string | null { return this._matchId; }
  get currentRoom(): IVXRoomInfo | null { return this._currentRoom; }
  get isSearching(): boolean { return this._searching; }

  /** True when the minimum requirements to start a match are met. */
  get canStartMatch(): boolean {
    if (this._players.length === 0) return false;
    if (!this._players.every(p => p.ready)) return false;
    if (this._currentMode === IVXGameMode.SOLO) return this._players.length === 1;
    return this._players.length >= 2;
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  /** Subscribe to a game-mode event. */
  on<K extends keyof IVXGameModesEventMap>(event: K, handler: ModeEventHandler<K>): void {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event)!.add(handler);
  }

  /** Unsubscribe from a game-mode event. */
  off<K extends keyof IVXGameModesEventMap>(event: K, handler: ModeEventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXGameModesEventMap>(
    event: K,
    ...args: IVXGameModesEventMap[K]
  ): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // ---------------------------------------------------------------------------
  // Mode & Players
  // ---------------------------------------------------------------------------

  /**
   * Select the active game mode.
   * @param mode     One of the `IVXGameMode` constants.
   * @param maxPlayers  Override the default player cap for this mode.
   */
  selectMode(mode: IVXGameMode, maxPlayers?: number): void {
    this._currentMode = mode;
    this._maxPlayers = maxPlayers ?? IVXGameModes.defaultMaxPlayers(mode);
    this._players = [];
    this.emit('modeChanged', mode);
  }

  /**
   * Add a player to the lobby.
   * @returns The allocated `IVXPlayerSlot`.
   */
  addPlayer(name: string, isLocal = true): IVXPlayerSlot {
    if (this._players.length >= this._maxPlayers) {
      const err = { code: -1, message: `Lobby full (max ${this._maxPlayers}).` };
      this.emit('error', err);
      throw err;
    }
    const slot: IVXPlayerSlot = {
      index: this._players.length,
      name,
      isLocal,
      ready: false,
    };
    this._players.push(slot);
    this.emit('playerAdded', slot);
    return slot;
  }

  /** Remove a player by slot index. */
  removePlayer(slotIndex: number): void {
    if (slotIndex < 0 || slotIndex >= this._players.length) return;
    this._players.splice(slotIndex, 1);
    this._players.forEach((p, i) => { p.index = i; });
    this.emit('playerRemoved', slotIndex);
  }

  /** Toggle the ready state for a slot. */
  setPlayerReady(slotIndex: number, ready: boolean): void {
    const slot = this._players[slotIndex];
    if (!slot) return;
    slot.ready = ready;
    this.emit('playerReady', slotIndex, ready);
  }

  /** Start the match. Throws if `canStartMatch` is false. */
  startMatch(): string {
    if (!this.canStartMatch) {
      const err = { code: -1, message: 'Cannot start match — check canStartMatch.' };
      this.emit('error', err);
      throw err;
    }
    this._matchId = IVXGameModes.generateId();
    this.emit('matchStarted', this._matchId);
    return this._matchId;
  }

  /** End the current match and return a result summary. */
  endMatch(): IVXMatchResult {
    const result: IVXMatchResult = {
      matchId: this._matchId ?? '',
      mode: this._currentMode,
      players: [...this._players],
      startedAt: 0,
      endedAt: Date.now(),
    };
    this._matchId = null;
    this.emit('matchEnded', result);
    return result;
  }

  /** Reset all state back to defaults. */
  reset(): void {
    this._currentMode = IVXGameMode.SOLO;
    this._maxPlayers = 1;
    this._players = [];
    this._matchId = null;
    this._currentRoom = null;
    this._searching = false;
  }

  // ---------------------------------------------------------------------------
  // Lobby / Rooms
  // ---------------------------------------------------------------------------

  /** Create a new room and become its host. */
  async createRoom(name: string, config?: IVXRoomConfig): Promise<IVXRoomInfo> {
    const room: IVXRoomInfo = {
      roomId: IVXGameModes.generateId(),
      name,
      hostName: this._players[0]?.name ?? 'Host',
      playerCount: this._players.length,
      maxPlayers: config?.maxPlayers ?? this._maxPlayers,
      isPrivate: config?.isPrivate ?? false,
      metadata: config?.metadata,
    };
    this._currentRoom = room;
    this.emit('roomCreated', room);
    return room;
  }

  /** Join an existing room by id, with an optional password for private rooms. */
  async joinRoom(roomId: string, _password?: string): Promise<IVXRoomInfo> {
    const room: IVXRoomInfo = {
      roomId,
      name: '',
      hostName: '',
      playerCount: 0,
      maxPlayers: 0,
      isPrivate: false,
    };
    this._currentRoom = room;
    this.emit('roomJoined', room);
    return room;
  }

  /** Leave the current room. */
  async leaveRoom(): Promise<void> {
    this._currentRoom = null;
    this.emit('roomLeft');
  }

  /** List available rooms, optionally filtered. */
  async listRooms(_filter?: IVXRoomFilter): Promise<IVXRoomInfo[]> {
    const rooms: IVXRoomInfo[] = [];
    this.emit('roomListUpdated', rooms);
    return rooms;
  }

  // ---------------------------------------------------------------------------
  // Matchmaking
  // ---------------------------------------------------------------------------

  /** Begin searching for a match with the current mode settings. */
  async findMatch(config?: IVXMatchConfig): Promise<string> {
    if (config) this.selectMode(config.mode, config.maxPlayers);
    this._searching = true;

    const matchId = IVXGameModes.generateId();
    this._matchId = matchId;
    this._searching = false;
    this.emit('matchFound', matchId, [...this._players]);
    return matchId;
  }

  /** Cancel an in-progress matchmaking search. */
  cancelSearch(): void {
    this._searching = false;
    this.emit('matchmakingCancelled');
  }

  // ---------------------------------------------------------------------------
  // Internal helpers
  // ---------------------------------------------------------------------------

  private static defaultMaxPlayers(mode: IVXGameMode): number {
    switch (mode) {
      case IVXGameMode.SOLO: return 1;
      case IVXGameMode.LOCAL_MULTIPLAYER: return 4;
      case IVXGameMode.ONLINE_VERSUS: return 2;
      case IVXGameMode.ONLINE_COOP: return 4;
      case IVXGameMode.RANKED: return 2;
      case IVXGameMode.TURN_BASED: return 4;
      default: return 2;
    }
  }

  private static generateId(): string {
    return typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
      ? crypto.randomUUID()
      : `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}`;
  }
}
