// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/** Single DM in a Discord conversation (mirrors Unity IVXDirectMessage). */
export interface IVXDirectMessage {
  messageId: string;
  authorId: string;
  authorName: string;
  content: string;
  timestamp: number;
  isDisclosure: boolean;
  hasAdditionalContent: boolean;
  additionalContentDescription?: string;
  moderationMetadata?: Record<string, string>;
}

/** DM conversation summary row (mirrors Unity IVXDMSummary). */
export interface IVXDMSummary {
  userId: string;
  displayName: string;
  lastMessageId: string;
  lastMessageTimestamp: number;
}

/**
 * Discord Social SDK — direct messages: send, edit, history, summaries, chat visibility, deep links.
 * Stub surface; integrate with the native Discord Social SDK.
 */
export class IVXDiscordMessages {
  readonly isShowingChat = false;

  async sendDM(
    recipientId: string,
    message: string
  ): Promise<{ messageId: string }> {
    throw new Error('Not implemented');
  }

  async editDM(
    recipientId: string,
    messageId: string,
    newContent: string
  ): Promise<void> {
    throw new Error('Not implemented');
  }

  async getDMHistory(
    recipientId: string,
    limit = 50
  ): Promise<IVXDirectMessage[]> {
    throw new Error('Not implemented');
  }

  async getDMSummaries(): Promise<IVXDMSummary[]> {
    throw new Error('Not implemented');
  }

  setShowingChat(_showing: boolean): void {
    throw new Error('Not implemented');
  }

  openMessageInDiscord(_messageId: string): void {
    throw new Error('Not implemented');
  }

  openDMSettingsInDiscord(): void {
    throw new Error('Not implemented');
  }
}
