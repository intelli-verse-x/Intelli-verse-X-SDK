// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export interface IVXConfig {
  /**
   * Game ID (UUID) for your title on the IntelliVerseX platform.
   * Copy it from the developer dashboard, or obtain it by calling
   * `POST https://msapi.intelli-verse-x.io/api/games/game/info` with your game credentials.
   */
  gameId?: string;
  nakamaHost?: string;
  nakamaPort?: number;
  nakamaServerKey?: string;
  useSSL?: boolean;
  enableAnalytics?: boolean;
  enableDebugLogs?: boolean;
  verboseLogging?: boolean;
}

export const DEFAULT_CONFIG: Required<IVXConfig> = {
  gameId: '',
  nakamaHost: 'nakama-rest.intelli-verse-x.ai',
  nakamaPort: 443,
  nakamaServerKey: 'defaultkey',
  useSSL: true,
  enableAnalytics: true,
  enableDebugLogs: false,
  verboseLogging: false,
};

export function validateConfig(config: IVXConfig): void {
  const gameId = config.gameId ?? '';
  if (gameId.trim() === '') {
    console.warn(
      '[IntelliVerseX] gameId is not set. Set gameId to your Game UUID from the IntelliVerseX dashboard, or obtain it via POST https://msapi.intelli-verse-x.io/api/games/game/info.',
    );
  }
  if (config.nakamaPort !== undefined && (config.nakamaPort < 1 || config.nakamaPort > 65535)) {
    throw new Error(`Invalid port: ${config.nakamaPort}. Must be 1-65535.`);
  }
  if (config.nakamaHost !== undefined && config.nakamaHost.trim() === '') {
    throw new Error('nakamaHost cannot be empty.');
  }
  if (config.nakamaServerKey !== undefined && config.nakamaServerKey.trim() === '') {
    throw new Error('nakamaServerKey cannot be empty.');
  }
}
