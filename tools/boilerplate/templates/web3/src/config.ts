/**
 * Runtime config + Web3 chain / contract (template variables).
 */
export const IVX_CONFIG = {
  gameId: '{{game_id}}',
  serverHost: '{{server_host}}',
  serverPort: parseInt('{{server_port}}', 10) || 7350,
  serverKey: '{{server_key}}',
  chainId: Number.parseInt('{{chain_id}}', 10) || 1,
  contractAddress: '{{contract_address}}',
  primaryColor: '{{primary_color}}',
  secondaryColor: '{{secondary_color}}',
  backgroundColor: '{{background_color}}',
  companyName: '{{company_name}}',
  bundleId: '{{bundle_id}}',
  packageName: '{{package_name}}',
  tagline: '{{tagline}}',
  maxEnergy: parseInt('{{max_energy}}', 10) || 100,
  energyRefillMinutes: parseInt('{{energy_refill_minutes}}', 10) || 5,
  initialCoins: parseInt('{{initial_coins}}', 10) || 0,
  initialGems: parseInt('{{initial_gems}}', 10) || 0,
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
