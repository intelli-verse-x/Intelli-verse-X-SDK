export interface IVXSatoriEvent {
  name: string;
  properties?: Record<string, unknown>;
  metadata?: Record<string, string>;
}

export interface IVXLeaderboardRecord {
  ownerId: string;
  username: string;
  score: number;
  subscore?: number;
  numScore?: number;
  metadata?: string;
  createTime?: string;
  updateTime?: string;
  expiryTime?: string;
  rank?: number;
}

export class IVXManager {
  private static instance: IVXManager;
  public client: any = null;
  public session: any = null;
  public userId: string = '';
  public hasValidSession: boolean = false;
  private listeners: Record<string, Function[]> = {};

  public static getInstance(): IVXManager {
    if (!IVXManager.instance) {
      IVXManager.instance = new IVXManager();
    }
    return IVXManager.instance;
  }

  public initialize(config: any): void {}

  public on(event: string, callback: Function): void {
    if (!this.listeners[event]) this.listeners[event] = [];
    this.listeners[event].push(callback);
  }
  
  public off(event: string, callback: Function): void {
    if (this.listeners[event]) {
      this.listeners[event] = this.listeners[event].filter(cb => cb !== callback);
    }
  }

  public restoreSession(): boolean {
    return false;
  }

  public async authenticateDevice(): Promise<void> {}
  public async authenticateEmail(email: string, pass: string, create?: boolean): Promise<void> {}
  public async authenticateGoogle(token: string): Promise<void> {}
  public async authenticateApple(token: string): Promise<void> {}
  public clearSession(): void {}

  public async callRpc(id: string, payload?: string): Promise<any> { return null; }
  public async fetchLeaderboard(id: string, arg2?: any, arg3?: any, arg4?: any): Promise<any> { return []; }
  public async fetchWallet(): Promise<Record<string, number>> { return {}; }
}

export class IVXHiroSystems {
  private static instance: IVXHiroSystems;

  public retention: any = { claim: async () => {}, get: async () => {} };
  public streaks: any = { get: async () => {} };

  public static getInstance(): IVXHiroSystems {
    if (!IVXHiroSystems.instance) {
      IVXHiroSystems.instance = new IVXHiroSystems();
    }
    return IVXHiroSystems.instance;
  }

  public initialize(client: any, session: any): void {}
}

export class IVXSatori {
  private static instance: IVXSatori;
  public isInitialized: boolean = false;

  public static getInstance(): IVXSatori {
    if (!IVXSatori.instance) {
      IVXSatori.instance = new IVXSatori();
    }
    return IVXSatori.instance;
  }

  public initialize(client: any, session?: any): void {
    this.isInitialized = true;
  }
  public async event(event: IVXSatoriEvent): Promise<void> {}
  public async identify(userId: string): Promise<void> {}
  public async authenticate(userId: string, props1?: any, props2?: any): Promise<void> {}
  public async captureEvents(events: IVXSatoriEvent[]): Promise<void> {}
}
