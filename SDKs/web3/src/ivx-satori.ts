// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/**
 * Satori Analytics — event tracking, feature flags, experiments, live-ops.
 * Wraps Heroic Labs Satori for server-driven analytics and A/B testing.
 */

export interface IVXSatoriConfig {
  satoriUrl: string;
  apiKey: string;
  identityToken?: string;
}

export interface IVXSatoriEvent {
  name: string;
  value?: string;
  metadata?: Record<string, string>;
  timestamp?: number;
}

export interface IVXSatoriFlag {
  name: string;
  value: string;
  conditionChanged: boolean;
}

export interface IVXSatoriExperiment {
  name: string;
  variant: string;
}

export interface IVXSatoriLiveEvent {
  id: string;
  name: string;
  description: string;
  value: string;
  activeStartTime: number;
  activeEndTime: number;
}

export class IVXSatori {
  private static _instance: IVXSatori | null = null;
  private _config: IVXSatoriConfig | null = null;
  private _initialized = false;
  private _identityId = '';

  static getInstance(): IVXSatori {
    if (!IVXSatori._instance) {
      IVXSatori._instance = new IVXSatori();
    }
    return IVXSatori._instance;
  }

  static resetInstance(): void {
    IVXSatori._instance = null;
  }

  private constructor() {}

  private stubWarn(method: string): void {
    console.warn(`[IVX-Web3] ${method}: stub – not yet implemented`);
  }

  get isInitialized(): boolean { return this._initialized; }

  /** Initialize the Satori analytics client. */
  initialize(config: IVXSatoriConfig): void {
    if (!config.satoriUrl) throw new Error('satoriUrl is required.');
    if (!config.apiKey) throw new Error('apiKey is required.');
    this._config = config;
    this._initialized = true;
  }

  /** Authenticate/identify the current player for analytics. */
  async authenticate(identityId: string, defaultProperties?: Record<string, string>, customProperties?: Record<string, string>): Promise<void> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.authenticate');
    this._identityId = identityId;
  }

  /** Update the identity properties. */
  async updateIdentity(defaultProperties?: Record<string, string>, customProperties?: Record<string, string>): Promise<void> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.updateIdentity');
  }

  /** Capture one or more analytics events. */
  async captureEvents(events: IVXSatoriEvent[]): Promise<void> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.captureEvents');
  }

  /** Get all feature flags for the current identity. */
  async getAllFlags(): Promise<IVXSatoriFlag[]> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.getAllFlags');
    return [];
  }

  /** Get a specific feature flag by name. */
  async getFlag(name: string): Promise<IVXSatoriFlag | null> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.getFlag');
    return null;
  }

  /** Get the variant assigned to the current identity for a given experiment. */
  async getExperimentVariant(experimentName: string): Promise<string> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.getExperimentVariant');
    return '';
  }

  /** Get all experiments and their assigned variants. */
  async getAllExperiments(): Promise<IVXSatoriExperiment[]> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.getAllExperiments');
    return [];
  }

  /** Get currently active live events. */
  async getLiveEvents(): Promise<IVXSatoriLiveEvent[]> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.getLiveEvents');
    return [];
  }

  /** Logout / clear the current identity session. */
  async logout(): Promise<void> {
    this.ensureInitialized();
    this.stubWarn('IVXSatori.logout');
    this._identityId = '';
  }

  private ensureInitialized(): void {
    if (!this._initialized || !this._config) {
      throw new Error('Satori not initialized. Call initialize() first.');
    }
  }
}
