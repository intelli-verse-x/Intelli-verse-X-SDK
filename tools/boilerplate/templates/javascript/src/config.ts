/**
 * Runtime config expanded from boilerplate template variables.
 * Replace placeholders before production builds; `serverPort` parses from digits in quotes.
 */
export const IVX_CONFIG = {
  gameId: '{{game_id}}',
  serverHost: '{{server_host}}',
  serverPort: parseInt('{{server_port}}', 10) || 7350,
  serverKey: '{{server_key}}',
} as const;

export function buildIvManagerConfig() {
  return {
    gameId: IVX_CONFIG.gameId,
    nakamaHost: IVX_CONFIG.serverHost,
    nakamaPort: IVX_CONFIG.serverPort,
    nakamaServerKey: IVX_CONFIG.serverKey,
    useSSL: IVX_CONFIG.serverPort === 443,
    enableAnalytics: true,
    enableDebugLogs: false,
  };
}
