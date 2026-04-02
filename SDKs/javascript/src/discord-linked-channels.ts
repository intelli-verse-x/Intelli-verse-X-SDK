// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/**
 * Discord Social SDK — Linked Channels: bridge in-game chat to Discord server text channels.
 * Stub surface; integrate with the native Discord Social SDK.
 */

export interface IVXLinkedChannel {
  channelId: string;
  guildId: string;
  name: string;
  lobbyId: string;
  linkedAt: number;
}

export class IVXDiscordLinkedChannels {
  /** Link a Discord text channel to a game lobby for message bridging. */
  async linkChannel(lobbyId: string, channelId: string): Promise<IVXLinkedChannel> {
    throw new Error('Not implemented — requires Discord Social SDK native integration.');
  }

  /** Unlink a previously linked channel from a lobby. */
  async unlinkChannel(lobbyId: string, channelId: string): Promise<void> {
    throw new Error('Not implemented — requires Discord Social SDK native integration.');
  }

  /** Get all linked channels for a given lobby. */
  async getLinkedChannels(lobbyId: string): Promise<IVXLinkedChannel[]> {
    throw new Error('Not implemented — requires Discord Social SDK native integration.');
  }
}
