import { Client, Session } from '@heroiclabs/nakama-js';
import { BrowserProvider, JsonRpcSigner, formatEther } from 'ethers';
import {
  IVXWeb3Config,
  DEFAULT_WEB3_CONFIG,
  validateWeb3Config,
  SDK_VERSION,
} from './types';
import type {
  IVXWalletInfo,
  IVXTokenBalance,
  IVXNft,
  IVXWeb3Error,
  IVXProfile,
  IVXLeaderboardRecord,
  IVXWeb3EventMap,
} from './types';

type EventHandler<K extends keyof IVXWeb3EventMap> = (...args: IVXWeb3EventMap[K]) => void;

/**
 * Central coordinator for the IntelliVerseX Web3 SDK.
 *
 * Extends the standard Nakama-backed IVXManager pattern with Web3 wallet
 * authentication (MetaMask / WalletConnect / EIP-1193), NFT queries,
 * token balance lookups, and on-chain reward verification.
 */
export class IVXWeb3Manager {
  private static _instance: IVXWeb3Manager | null = null;

  private _client: Client | null = null;
  private _session: Session | null = null;
  private _config: Required<IVXWeb3Config>;
  private _initialized = false;
  private _listeners = new Map<string, Set<Function>>();

  private _provider: BrowserProvider | null = null;
  private _signer: JsonRpcSigner | null = null;
  private _walletInfo: IVXWalletInfo | null = null;

  static getInstance(): IVXWeb3Manager {
    if (!IVXWeb3Manager._instance) {
      IVXWeb3Manager._instance = new IVXWeb3Manager();
    }
    return IVXWeb3Manager._instance;
  }

  static resetInstance(): void {
    IVXWeb3Manager._instance = null;
  }

  private constructor() {
    this._config = { ...DEFAULT_WEB3_CONFIG };
  }

  // ---------------------------------------------------------------------------
  // Getters
  // ---------------------------------------------------------------------------

  get client(): Client | null { return this._client; }
  get session(): Session | null { return this._session; }
  get isInitialized(): boolean { return this._initialized; }
  get userId(): string { return this._session?.user_id ?? ''; }
  get username(): string { return this._session?.username ?? ''; }
  get walletInfo(): IVXWalletInfo | null { return this._walletInfo; }
  get walletAddress(): string { return this._walletInfo?.address ?? ''; }
  get isWalletConnected(): boolean { return this._walletInfo !== null; }

  get hasValidSession(): boolean {
    return this._session != null && !this._session.isexpired(Date.now() / 1000);
  }

  // ---------------------------------------------------------------------------
  // Initialization
  // ---------------------------------------------------------------------------

  initialize(config: IVXWeb3Config): void {
    validateWeb3Config(config);
    this._config = { ...DEFAULT_WEB3_CONFIG, ...config };

    this._client = new Client(
      this._config.nakamaServerKey,
      this._config.nakamaHost,
      String(this._config.nakamaPort),
      this._config.useSSL,
    );

    this._initialized = true;
    this.log(`Web3 SDK v${SDK_VERSION} initialized (chain ${this._config.chainId})`);
    this.emit('initialized');
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  on<K extends keyof IVXWeb3EventMap>(event: K, handler: EventHandler<K>): void {
    if (!this._listeners.has(event)) this._listeners.set(event, new Set());
    this._listeners.get(event)!.add(handler);
  }

  off<K extends keyof IVXWeb3EventMap>(event: K, handler: EventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXWeb3EventMap>(event: K, ...args: IVXWeb3EventMap[K]): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // ---------------------------------------------------------------------------
  // Web3 Wallet Connection
  // ---------------------------------------------------------------------------

  /**
   * Connect a browser wallet (MetaMask, Coinbase Wallet, etc.) via EIP-1193.
   * Requests account access and reads the wallet address + chain + balance.
   */
  async connectWallet(): Promise<IVXWalletInfo> {
    this.ensureInitialized();
    const ethereum = (globalThis as any).ethereum;
    if (!ethereum) {
      throw this.makeError('No EIP-1193 provider found. Install MetaMask or another wallet.');
    }

    try {
      this._provider = new BrowserProvider(ethereum);
      await this._provider.send('eth_requestAccounts', []);
      this._signer = await this._provider.getSigner();

      const address = await this._signer.getAddress();
      const network = await this._provider.getNetwork();
      const balanceWei = await this._provider.getBalance(address);

      this._walletInfo = {
        address,
        chainId: Number(network.chainId),
        balance: formatEther(balanceWei),
      };

      this.log(`Wallet connected: ${address} on chain ${this._walletInfo.chainId}`);
      this.emit('walletConnected', this._walletInfo);
      return this._walletInfo;
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  /** Disconnect the Web3 wallet (local state only). */
  disconnectWallet(): void {
    this._provider = null;
    this._signer = null;
    this._walletInfo = null;
    this.log('Wallet disconnected');
    this.emit('walletDisconnected');
  }

  // ---------------------------------------------------------------------------
  // Web3 Authentication (wallet signature -> Nakama custom auth)
  // ---------------------------------------------------------------------------

  /**
   * Authenticate with Nakama using a signed message from the connected wallet.
   * The server should verify the signature and map the wallet address to a user.
   */
  async authenticateWallet(): Promise<void> {
    this.ensureInitialized();
    if (!this._signer || !this._walletInfo) {
      throw this.makeError('Wallet not connected. Call connectWallet() first.');
    }

    try {
      const nonce = Date.now().toString();
      const message = `IntelliVerseX Auth\nWallet: ${this._walletInfo.address}\nNonce: ${nonce}`;
      const signature = await this._signer.signMessage(message);

      const session = await this._client!.authenticateCustom(
        this._walletInfo.address,
        true,
        this._walletInfo.address,
      );
      this._session = session;

      await this.callRpc('ivx_web3_verify_wallet', JSON.stringify({
        address: this._walletInfo.address,
        message,
        signature,
        chainId: this._walletInfo.chainId,
      }));

      this.log(`Wallet authenticated - UserId: ${session.user_id}`);
      this.syncMetadata();
      this.emit('authSuccess', session.user_id);
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  /** Standard device auth (non-Web3 fallback). */
  async authenticateDevice(deviceId?: string): Promise<void> {
    this.ensureInitialized();
    const resolvedId = deviceId || this.generateDeviceId();
    try {
      const session = await this._client!.authenticateDevice(resolvedId, true);
      this._session = session;
      this.log(`Device authenticated - UserId: ${session.user_id}`);
      this.syncMetadata();
      this.emit('authSuccess', session.user_id);
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  clearSession(): void {
    this._session = null;
    this.log('Session cleared');
  }

  // ---------------------------------------------------------------------------
  // NFT Queries
  // ---------------------------------------------------------------------------

  /**
   * Fetch NFTs owned by the connected wallet via a server-side RPC.
   * The server can use Moralis / Alchemy / Thirdweb to query on-chain data.
   */
  async fetchNfts(contractAddress?: string): Promise<IVXNft[]> {
    this.ensureSession();
    if (!this._walletInfo) {
      throw this.makeError('Wallet not connected.');
    }

    const result = await this.callRpc('ivx_web3_fetch_nfts', JSON.stringify({
      walletAddress: this._walletInfo.address,
      chainId: this._walletInfo.chainId,
      contractAddress: contractAddress ?? '',
    }));

    const nfts: IVXNft[] = Array.isArray(result.nfts) ? result.nfts : [];
    this.emit('nftsFetched', nfts);
    return nfts;
  }

  // ---------------------------------------------------------------------------
  // Token Balance Queries
  // ---------------------------------------------------------------------------

  /** Fetch ERC-20 token balances for the connected wallet via server RPC. */
  async fetchTokenBalances(): Promise<IVXTokenBalance[]> {
    this.ensureSession();
    if (!this._walletInfo) {
      throw this.makeError('Wallet not connected.');
    }

    const result = await this.callRpc('ivx_web3_fetch_tokens', JSON.stringify({
      walletAddress: this._walletInfo.address,
      chainId: this._walletInfo.chainId,
    }));

    const tokens: IVXTokenBalance[] = Array.isArray(result.tokens) ? result.tokens : [];
    this.emit('tokenBalanceFetched', tokens);
    return tokens;
  }

  // ---------------------------------------------------------------------------
  // Token Gating
  // ---------------------------------------------------------------------------

  /**
   * Check if the connected wallet holds a specific token/NFT that grants
   * access to gated content. Verification happens server-side.
   */
  async checkTokenGate(contractAddress: string, minBalance = '1'): Promise<boolean> {
    this.ensureSession();
    if (!this._walletInfo) {
      throw this.makeError('Wallet not connected.');
    }

    const result = await this.callRpc('ivx_web3_check_gate', JSON.stringify({
      walletAddress: this._walletInfo.address,
      chainId: this._walletInfo.chainId,
      contractAddress,
      minBalance,
    }));

    return result.granted === true;
  }

  // ---------------------------------------------------------------------------
  // Profile / Wallet / Leaderboard / Storage / RPC (standard Nakama features)
  // ---------------------------------------------------------------------------

  async fetchProfile(): Promise<IVXProfile> {
    this.ensureSession();
    try {
      const account = await this._client!.getAccount(this._session!);
      const profile: IVXProfile = {
        userId: account.user?.id ?? '',
        username: account.user?.username ?? '',
        displayName: account.user?.display_name ?? '',
        avatarUrl: account.user?.avatar_url ?? '',
        langTag: account.user?.lang_tag ?? '',
        metadata: this.safeParseJson(account.user?.metadata),
        wallet: this.safeParseJson(account.wallet),
      };
      this.emit('profileLoaded', profile);
      return profile;
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async updateProfile(displayName?: string, avatarUrl?: string, langTag?: string): Promise<void> {
    this.ensureSession();
    try {
      await this._client!.updateAccount(this._session!, {
        display_name: displayName,
        avatar_url: avatarUrl,
        lang_tag: langTag,
      });
      this.log('Profile updated');
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async fetchWallet(): Promise<Record<string, unknown>> {
    const result = await this.callRpc('hiro_economy_list', '{}');
    this.emit('walletUpdated', result as Record<string, number>);
    return result;
  }

  async grantCurrency(currencyId: string, amount: number): Promise<Record<string, unknown>> {
    return this.callRpc('hiro_economy_grant', JSON.stringify({ currencies: { [currencyId]: Math.floor(amount) } }));
  }

  async submitScore(leaderboardId: string, score: number): Promise<void> {
    this.ensureSession();
    try {
      await this._client!.writeLeaderboardRecord(this._session!, leaderboardId, { score });
      this.log(`Score submitted: ${score} to ${leaderboardId}`);
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async fetchLeaderboard(leaderboardId: string, limit = 20): Promise<IVXLeaderboardRecord[]> {
    this.ensureSession();
    try {
      const result = await this._client!.listLeaderboardRecords(this._session!, leaderboardId, undefined, limit);
      const records: IVXLeaderboardRecord[] = (result.records ?? []).map(r => ({
        ownerId: r.owner_id ?? '',
        username: r.username?.value ?? r.username ?? '',
        score: Number(r.score ?? 0),
        rank: Number(r.rank ?? 0),
      }));
      this.emit('leaderboardFetched', records);
      return records;
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async writeStorage(collection: string, key: string, value: object): Promise<void> {
    this.ensureSession();
    try {
      await this._client!.writeStorageObjects(this._session!, [
        { collection, key, value: JSON.stringify(value), permission_read: 1, permission_write: 1 },
      ]);
      this.log(`Storage write: ${collection}/${key}`);
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async readStorage(collection: string, key: string): Promise<unknown> {
    this.ensureSession();
    try {
      const result = await this._client!.readStorageObjects(this._session!, {
        object_ids: [{ collection, key, user_id: this.userId }],
      });
      const data = (result.objects && result.objects.length > 0)
        ? this.safeParseJson(result.objects[0].value)
        : null;
      this.emit('storageRead', data);
      return data;
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  async callRpc(rpcId: string, payload = '{}'): Promise<Record<string, unknown>> {
    this.ensureSession();
    try {
      const result = await this._client!.rpc(this._session!, rpcId, payload);
      this.log(`RPC ${rpcId} response received`);
      const data = result.payload ? this.safeParseJson(result.payload) : {};
      this.emit('rpcResponse', rpcId, data);
      return data as Record<string, unknown>;
    } catch (e) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  private async syncMetadata(): Promise<void> {
    if (!this.hasValidSession) return;
    try {
      await this.callRpc('ivx_sync_metadata', JSON.stringify({
        metadata: {
          sdk_version: SDK_VERSION,
          platform: 'web3',
          engine: 'typescript',
          wallet_address: this._walletInfo?.address ?? '',
          chain_id: this._walletInfo?.chainId ?? 0,
        },
      }));
    } catch {
      // Non-fatal
    }
  }

  private generateDeviceId(): string {
    return typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
      ? crypto.randomUUID()
      : `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}`;
  }

  private safeParseJson(value: unknown): Record<string, any> {
    if (typeof value === 'object' && value !== null) return value as Record<string, any>;
    if (typeof value !== 'string' || value === '') return {};
    try { return JSON.parse(value); } catch { return {}; }
  }

  private toIVXError(e: unknown): IVXWeb3Error {
    if (typeof e === 'object' && e !== null && 'code' in e && 'message' in e) {
      return e as IVXWeb3Error;
    }
    if (e instanceof Error) return { code: -1, message: e.message };
    return { code: -1, message: String(e) };
  }

  private makeError(message: string): IVXWeb3Error {
    const err: IVXWeb3Error = { code: -1, message };
    this.emit('error', err);
    return err;
  }

  private ensureInitialized(): void {
    if (!this._initialized || !this._client) {
      throw this.makeError('SDK not initialized. Call initialize() first.');
    }
  }

  private ensureSession(): void {
    this.ensureInitialized();
    if (!this.hasValidSession) {
      throw this.makeError('No valid session. Authenticate first.');
    }
  }

  private log(message: string): void {
    if (this._config.enableDebugLogs) {
      console.log(`[IntelliVerseX Web3] ${message}`);
    }
  }
}
