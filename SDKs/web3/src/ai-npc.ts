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
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  setAuthToken(_token: string | null): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  registerNPC(_profile: IVXAINPCProfile): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  unregisterNPC(_npcId: string): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async startDialog(
    _npcId: string,
    _playerId: string,
    _playerContext?: string
  ): Promise<IVXAINPCDialogSession | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async sendMessage(
    _sessionId: string,
    _message: string
  ): Promise<string | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async endDialog(_sessionId: string): Promise<void> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  getSession(_sessionId: string): IVXAINPCDialogSession | null {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  getSessionsForNPC(_npcId: string): IVXAINPCDialogSession[] {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }
}
