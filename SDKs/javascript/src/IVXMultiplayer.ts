// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

import type { Client, Session } from '@heroiclabs/nakama-js';

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

export interface IVXLobbyPlayer {
  userId: string;
  username: string;
  metadata?: Record<string, unknown>;
}

export interface IVXLobby {
  lobbyId: string;
  name: string;
  hostUserId: string;
  players: IVXLobbyPlayer[];
  maxPlayers: number;
  isPublic: boolean;
  metadata: Record<string, unknown>;
}

export interface IVXMatchmakingTicket {
  ticketId: string;
  status: string;
  matchId: string;
}

// ---------------------------------------------------------------------------
// API surface
// ---------------------------------------------------------------------------

export interface IVXMultiplayerLobbyAPI {
  createLobby(name: string, maxPlayers: number, isPublic: boolean): Promise<IVXLobby>;
  joinLobby(lobbyId: string): Promise<IVXLobby>;
  leaveLobby(lobbyId: string): Promise<void>;
  listLobbies(): Promise<IVXLobby[]>;
}

export interface IVXMultiplayerMatchmakingAPI {
  startMatchmaking(minPlayers: number, maxPlayers: number, rankRange?: number): Promise<IVXMatchmakingTicket>;
  cancelMatchmaking(ticketId: string): Promise<void>;
}

// ---------------------------------------------------------------------------
// Implementation
// ---------------------------------------------------------------------------

export class IVXMultiplayer {
  private static _instance: IVXMultiplayer | null = null;
  private _client: Client | null = null;
  private _session: Session | null = null;

  private constructor() {}

  static getInstance(): IVXMultiplayer {
    if (!IVXMultiplayer._instance) {
      IVXMultiplayer._instance = new IVXMultiplayer();
    }
    return IVXMultiplayer._instance;
  }

  static resetInstance(): void {
    IVXMultiplayer._instance = null;
  }

  initialize(client: Client, session: Session): void {
    this._client = client;
    this._session = session;
  }

  private async rpc<T>(rpcId: string, payload?: Record<string, unknown>): Promise<T> {
    if (!this._client || !this._session) {
      throw new Error('[IVXMultiplayer] Not initialized — call initialize(client, session) first.');
    }
    const body = (payload ?? {}) as unknown as object;
    const res = await this._client.rpc(this._session, rpcId, body);
    const raw = res.payload;
    if (raw == null) return {} as T;
    if (typeof raw === 'object') return raw as T;
    if (typeof raw === 'string') return JSON.parse(raw) as T;
    return {} as T;
  }

  // -----------------------------------------------------------------------
  // Lobby
  // -----------------------------------------------------------------------

  get lobby(): IVXMultiplayerLobbyAPI {
    return {
      createLobby: (name, maxPlayers, isPublic) =>
        this.rpc<IVXLobby>('create_lobby', { name, max_players: maxPlayers, is_public: isPublic }),

      joinLobby: (lobbyId) =>
        this.rpc<IVXLobby>('join_lobby', { lobby_id: lobbyId }),

      leaveLobby: async (lobbyId) => {
        await this.rpc<void>('leave_lobby', { lobby_id: lobbyId });
      },

      listLobbies: async () => {
        const res = await this.rpc<{ lobbies: IVXLobby[] }>('list_lobbies');
        return res.lobbies ?? [];
      },
    };
  }

  // -----------------------------------------------------------------------
  // Matchmaking
  // -----------------------------------------------------------------------

  get matchmaking(): IVXMultiplayerMatchmakingAPI {
    return {
      startMatchmaking: (minPlayers, maxPlayers, rankRange) =>
        this.rpc<IVXMatchmakingTicket>('start_matchmaking', {
          min_players: minPlayers,
          max_players: maxPlayers,
          ...(rankRange !== undefined ? { rank_range: rankRange } : {}),
        }),

      cancelMatchmaking: async (ticketId) => {
        await this.rpc<void>('cancel_matchmaking', { ticket_id: ticketId });
      },
    };
  }
}
