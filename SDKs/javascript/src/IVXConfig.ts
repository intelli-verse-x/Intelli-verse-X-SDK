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
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  useSSL: false,
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
