export const SDK_VERSION = '5.1.0';

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
  metadata?: string;
}

export interface IVXError {
  code: number;
  message: string;
}

export interface IVXEventMap {
  initialized: [];
  authSuccess: [userId: string];
  authError: [error: IVXError];
  profileLoaded: [profile: IVXProfile];
  walletUpdated: [wallet: Record<string, number>];
  leaderboardFetched: [records: IVXLeaderboardRecord[]];
  storageRead: [data: unknown];
  rpcResponse: [rpcId: string, data: unknown];
  error: [error: IVXError];
}
