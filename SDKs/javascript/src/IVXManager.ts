import { Client, Session, Socket } from '@heroiclabs/nakama-js';
import { IVXConfig, DEFAULT_CONFIG, validateConfig } from './IVXConfig';
import { SDK_VERSION } from './types';
import type { IVXProfile, IVXLeaderboardRecord, IVXError, IVXEventMap } from './types';

const SESSION_TOKEN_KEY = 'ivx_session_token';
const REFRESH_TOKEN_KEY = 'ivx_refresh_token';
const DEVICE_ID_KEY = 'ivx_device_id';

type EventHandler<K extends keyof IVXEventMap> = (...args: IVXEventMap[K]) => void;

export class IVXManager {
  private static _instance: IVXManager | null = null;

  private _client: Client | null = null;
  private _session: Session | null = null;
  private _socket: Socket | null = null;
  private _config: Required<IVXConfig>;
  private _initialized = false;
  private _listeners = new Map<string, Set<Function>>();

  static getInstance(): IVXManager {
    if (!IVXManager._instance) {
      IVXManager._instance = new IVXManager();
    }
    return IVXManager._instance;
  }

  /** Reset the singleton (useful for testing). */
  static resetInstance(): void {
    IVXManager._instance = null;
  }

  private constructor() {
    this._config = { ...DEFAULT_CONFIG };
  }

  get client(): Client | null { return this._client; }
  get session(): Session | null { return this._session; }
  get isInitialized(): boolean { return this._initialized; }
  get userId(): string { return this._session?.user_id ?? ''; }
  get username(): string { return this._session?.username ?? ''; }

  get hasValidSession(): boolean {
    return this._session != null && !this._session.isexpired(Date.now() / 1000);
  }

  initialize(config: IVXConfig): void {
    validateConfig(config);
    this._config = { ...DEFAULT_CONFIG, ...config };

    this._client = new Client(
      this._config.nakamaServerKey,
      this._config.nakamaHost,
      String(this._config.nakamaPort),
      this._config.useSSL,
    );

    this._initialized = true;
    this.log(`SDK v${SDK_VERSION} initialized - ${this._config.useSSL ? 'https' : 'http'}://${this._config.nakamaHost}:${this._config.nakamaPort}`);
    this.emit('initialized');
  }

  // --- Events ---

  on<K extends keyof IVXEventMap>(event: K, handler: EventHandler<K>): void {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event)!.add(handler);
  }

  off<K extends keyof IVXEventMap>(event: K, handler: EventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXEventMap>(event: K, ...args: IVXEventMap[K]): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // --- Auth ---

  async authenticateDevice(deviceId?: string): Promise<void> {
    this.ensureInitialized();
    const resolvedId = deviceId || this.getPersistentDeviceId();
    try {
      const session = await this._client!.authenticateDevice(resolvedId, true);
      this.onAuthSuccess(session);
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  async authenticateEmail(email: string, password: string, create = false): Promise<void> {
    this.ensureInitialized();
    try {
      const session = await this._client!.authenticateEmail(email, password, create);
      this.onAuthSuccess(session);
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  async authenticateGoogle(token: string): Promise<void> {
    this.ensureInitialized();
    try {
      const session = await this._client!.authenticateGoogle(token, true);
      this.onAuthSuccess(session);
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  async authenticateApple(token: string): Promise<void> {
    this.ensureInitialized();
    try {
      const session = await this._client!.authenticateApple(token, true);
      this.onAuthSuccess(session);
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  async authenticateCustom(customId: string): Promise<void> {
    this.ensureInitialized();
    try {
      const session = await this._client!.authenticateCustom(customId, true);
      this.onAuthSuccess(session);
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('authError', error);
      throw error;
    }
  }

  restoreSession(): boolean {
    const token = this.loadString(SESSION_TOKEN_KEY);
    const refresh = this.loadString(REFRESH_TOKEN_KEY);
    if (!token) return false;

    try {
      const session = Session.restore(token, refresh);
      if (session.isexpired(Date.now() / 1000)) {
        this.log('Stored session expired');
        return false;
      }
      this._session = session;
      this.log(`Session restored for user: ${session.user_id}`);
      this.syncMetadata();
      return true;
    } catch {
      this.log('Failed to restore session');
      return false;
    }
  }

  clearSession(): void {
    this._session = null;
    if (this._socket) {
      try { this._socket.disconnect(false); } catch { /* ignore */ }
      this._socket = null;
    }
    this.saveString(SESSION_TOKEN_KEY, '');
    this.saveString(REFRESH_TOKEN_KEY, '');
    this.log('Session cleared');
  }

  // --- Profile ---

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
    } catch (e: unknown) {
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
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  // --- Wallet ---

  async fetchWallet(): Promise<Record<string, unknown>> {
    const result = await this.callRpc('hiro_economy_list', '{}');
    this.emit('walletUpdated', result as Record<string, number>);
    return result;
  }

  async grantCurrency(currencyId: string, amount: number): Promise<Record<string, unknown>> {
    return this.callRpc('hiro_economy_grant', JSON.stringify({ currencies: { [currencyId]: Math.floor(amount) } }));
  }

  // --- Leaderboard ---

  async submitScore(leaderboardId: string, score: number): Promise<void> {
    this.ensureSession();
    try {
      await this._client!.writeLeaderboardRecord(this._session!, leaderboardId, { score });
      this.log(`Score submitted: ${score} to ${leaderboardId}`);
    } catch (e: unknown) {
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
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  // --- Storage ---

  async writeStorage(collection: string, key: string, value: object): Promise<void> {
    this.ensureSession();
    try {
      await this._client!.writeStorageObjects(this._session!, [
        { collection, key, value: JSON.stringify(value), permission_read: 1, permission_write: 1 },
      ]);
      this.log(`Storage write: ${collection}/${key}`);
    } catch (e: unknown) {
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
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  // --- RPC ---

  async callRpc(rpcId: string, payload = '{}'): Promise<Record<string, unknown>> {
    this.ensureSession();
    try {
      const result = await this._client!.rpc(this._session!, rpcId, payload);
      this.log(`RPC ${rpcId} response received`);
      const data = result.payload ? this.safeParseJson(result.payload) : {};
      this.emit('rpcResponse', rpcId, data);
      return data as Record<string, unknown>;
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.log(`RPC ${rpcId} failed: ${error.message}`);
      this.emit('error', error);
      throw error;
    }
  }

  // --- Socket ---

  async connectSocket(): Promise<Socket> {
    this.ensureSession();
    try {
      this._socket = this._client!.createSocket(this._config.useSSL, false);
      await this._socket.connect(this._session!, true);
      this.log('Socket connected');
      return this._socket;
    } catch (e: unknown) {
      const error = this.toIVXError(e);
      this.emit('error', error);
      throw error;
    }
  }

  // --- Internal ---

  private onAuthSuccess(session: Session): void {
    this._session = session;
    this.saveString(SESSION_TOKEN_KEY, session.token);
    this.saveString(REFRESH_TOKEN_KEY, session.refresh_token);
    this.log(`Authenticated - UserId: ${session.user_id}`);
    this.syncMetadata();
    this.emit('authSuccess', session.user_id);
  }

  private async syncMetadata(): Promise<void> {
    if (!this.hasValidSession) return;
    try {
      await this.callRpc('ivx_sync_metadata', JSON.stringify({
        metadata: {
          sdk_version: SDK_VERSION,
          platform: typeof navigator !== 'undefined' ? navigator.platform : 'node',
          engine: 'javascript',
        },
      }));
    } catch {
      // Non-fatal: metadata sync failure should not break the app
    }
  }

  private getPersistentDeviceId(): string {
    let id = this.loadString(DEVICE_ID_KEY);
    if (!id) {
      id = typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
        ? crypto.randomUUID()
        : `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}-${Math.random().toString(36).slice(2)}`;
      this.saveString(DEVICE_ID_KEY, id);
    }
    return id;
  }

  private saveString(key: string, value: string): void {
    try {
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem(key, value);
      }
    } catch { /* Storage unavailable (SSR, node, etc.) */ }
  }

  private loadString(key: string): string {
    try {
      if (typeof localStorage !== 'undefined') {
        return localStorage.getItem(key) ?? '';
      }
    } catch { /* Storage unavailable */ }
    return '';
  }

  private safeParseJson(value: unknown): Record<string, any> {
    if (typeof value === 'object' && value !== null) return value as Record<string, any>;
    if (typeof value !== 'string' || value === '') return {};
    try {
      return JSON.parse(value);
    } catch {
      return {};
    }
  }

  private toIVXError(e: unknown): IVXError {
    if (typeof e === 'object' && e !== null && 'code' in e && 'message' in e) {
      return e as IVXError;
    }
    if (e instanceof Error) {
      return { code: -1, message: e.message };
    }
    return { code: -1, message: String(e) };
  }

  private ensureInitialized(): void {
    if (!this._initialized || !this._client) {
      const err: IVXError = { code: -1, message: 'SDK not initialized. Call initialize() first.' };
      this.emit('error', err);
      throw err;
    }
  }

  private ensureSession(): void {
    this.ensureInitialized();
    if (!this.hasValidSession) {
      const err: IVXError = { code: -1, message: 'No valid session. Authenticate first.' };
      this.emit('error', err);
      throw err;
    }
  }

  private log(message: string): void {
    if (this._config.enableDebugLogs) {
      console.log(`[IntelliVerseX] ${message}`);
    }
  }
}
