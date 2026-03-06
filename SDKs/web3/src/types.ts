export const SDK_VERSION = '5.1.0';

export interface IVXWeb3Config {
  nakamaHost?: string;
  nakamaPort?: number;
  nakamaServerKey?: string;
  useSSL?: boolean;
  enableAnalytics?: boolean;
  enableDebugLogs?: boolean;
  verboseLogging?: boolean;

  /** Default chain ID (e.g. 1 for Ethereum mainnet, 137 for Polygon). */
  chainId?: number;

  /** Optional Thirdweb client ID for Thirdweb SDK integration. */
  thirdwebClientId?: string;

  /** Optional Moralis API key for Moralis SDK integration. */
  moralisApiKey?: string;
}

export const DEFAULT_WEB3_CONFIG: Required<IVXWeb3Config> = {
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  useSSL: false,
  enableAnalytics: true,
  enableDebugLogs: false,
  verboseLogging: false,
  chainId: 1,
  thirdwebClientId: '',
  moralisApiKey: '',
};

export function validateWeb3Config(config: IVXWeb3Config): void {
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

export interface IVXWalletInfo {
  address: string;
  chainId: number;
  balance: string;
  ensName?: string;
}

export interface IVXTokenBalance {
  contractAddress: string;
  symbol: string;
  name: string;
  decimals: number;
  balance: string;
}

export interface IVXNft {
  contractAddress: string;
  tokenId: string;
  name: string;
  description: string;
  imageUrl: string;
  metadata: Record<string, unknown>;
}

export interface IVXWeb3Error {
  code: number;
  message: string;
}

export interface IVXProfile {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string;
  langTag: string;
  metadata: Record<string, unknown>;
  wallet: Record<string, number>;
}

export interface IVXLeaderboardRecord {
  ownerId: string;
  username: string;
  score: number;
  rank: number;
}

export interface IVXWeb3EventMap {
  initialized: [];
  walletConnected: [info: IVXWalletInfo];
  walletDisconnected: [];
  authSuccess: [userId: string];
  authError: [error: IVXWeb3Error];
  profileLoaded: [profile: IVXProfile];
  walletUpdated: [wallet: Record<string, number>];
  leaderboardFetched: [records: IVXLeaderboardRecord[]];
  nftsFetched: [nfts: IVXNft[]];
  tokenBalanceFetched: [tokens: IVXTokenBalance[]];
  storageRead: [data: unknown];
  rpcResponse: [rpcId: string, data: unknown];
  error: [error: IVXWeb3Error];
}
