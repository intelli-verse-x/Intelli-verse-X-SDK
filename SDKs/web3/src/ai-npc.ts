// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/** Minimal NPC profile (stub). */
export interface IVXAINPCProfile {
  npcId: string;
  maxTurns?: number;
}

/** Active NPC dialog session (stub). */
export interface IVXAINPCDialogSession {
  sessionId: string;
  npcId: string;
  playerId: string;
}

/**
 * NPC dialog registration and HTTP session traffic (Unity IVXAINPCDialogManager).
 */
export class IVXAINPCDialogManager {
  get isInitialized(): boolean {
    return false;
  }

  initialize(_config: unknown): void {
    throw new Error('Not implemented');
  }

  setAuthToken(_token: string | null): void {
    throw new Error('Not implemented');
  }

  registerNPC(_profile: IVXAINPCProfile): void {
    throw new Error('Not implemented');
  }

  unregisterNPC(_npcId: string): void {
    throw new Error('Not implemented');
  }

  async startDialog(
    _npcId: string,
    _playerId: string,
    _playerContext?: string
  ): Promise<IVXAINPCDialogSession | null> {
    throw new Error('Not implemented');
  }

  async sendMessage(
    _sessionId: string,
    _message: string
  ): Promise<string | null> {
    throw new Error('Not implemented');
  }

  async endDialog(_sessionId: string): Promise<void> {
    throw new Error('Not implemented');
  }

  getSession(_sessionId: string): IVXAINPCDialogSession | null {
    throw new Error('Not implemented');
  }

  getSessionsForNPC(_npcId: string): IVXAINPCDialogSession[] {
    throw new Error('Not implemented');
  }
}
