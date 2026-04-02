// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export enum IVXModerationAction {
  Show = 'Show',
  Hide = 'Hide',
  Blur = 'Blur',
  Replace = 'Replace',
}

/** Parsed moderation outcome for a Discord message. */
export interface IVXModerationDecision {
  messageId: string;
  action: IVXModerationAction;
  reason: string;
  replacement: string;
  severity: string;
}

/**
 * Discord moderation metadata, voice capture for moderation pipelines, and user reporting.
 * Stub surface matching Unity IVXDiscordModeration.
 */
export class IVXDiscordModeration {
  autoModerateEnabled = true;

  enableAutoModeration(_enable: boolean): void {
    throw new Error('Not implemented');
  }

  processModerationMetadata(
    _messageId: string,
    _metadata: Record<string, string>
  ): void {
    throw new Error('Not implemented');
  }

  static getModerationAction(
    _metadata: Record<string, string> | null
  ): IVXModerationDecision {
    throw new Error('Not implemented');
  }

  startVoiceModerationCapture(_lobbyId: string): void {
    throw new Error('Not implemented');
  }

  stopVoiceModerationCapture(): void {
    throw new Error('Not implemented');
  }

  async reportUser(_userId: string, _reason: string): Promise<boolean> {
    throw new Error('Not implemented');
  }
}
