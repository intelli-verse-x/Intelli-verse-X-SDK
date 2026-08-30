// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export interface IVXConfig {
  nakamaHost?: string;
  nakamaPort?: number;
  nakamaServerKey?: string;
  useSSL?: boolean;
  enableAnalytics?: boolean;
  enableDebugLogs?: boolean;
  verboseLogging?: boolean;
}

export const DEFAULT_CONFIG: Required<IVXConfig> = {
  nakamaHost: 'nakama-rest.intelli-verse-x.ai',
  nakamaPort: 443,
  nakamaServerKey: 'defaultkey',
  useSSL: true,
  enableAnalytics: true,
  enableDebugLogs: false,
  verboseLogging: false,
};

export function validateConfig(config: IVXConfig): void {
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
