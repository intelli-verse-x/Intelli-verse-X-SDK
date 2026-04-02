// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// Hiro Systems — Typed RPC wrappers for Hiro live-ops modules
// ---------------------------------------------------------------------------

import type { Client, Session } from '@heroiclabs/nakama-js';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface IVXSpinWheelResult {
  rewardId: string;
  rewardType: string;
  amount: number;
  nextSpinAt: number;
  metadata?: Record<string, unknown>;
}

export interface IVXSpinWheelState {
  availableSpins: number;
  nextSpinAt: number;
  rewards: IVXSpinWheelResult[];
}

export interface IVXStreakState {
  streakId: string;
  currentStreak: number;
  longestStreak: number;
  lastClaimAt: number;
  milestones: IVXStreakMilestone[];
}

export interface IVXStreakMilestone {
  day: number;
  rewardId: string;
  claimed: boolean;
}

export interface IVXOffer {
  offerId: string;
  title: string;
  description: string;
  rewardType: string;
  rewardAmount: number;
  completed: boolean;
  metadata?: Record<string, unknown>;
}

export interface IVXOfferWallState {
  offers: IVXOffer[];
  pendingRewards: number;
}

export interface IVXRetentionState {
  day: number;
  lastLoginAt: number;
  rewards: { day: number; claimed: boolean }[];
}

export interface IVXFriendQuest {
  questId: string;
  title: string;
  description: string;
  targetProgress: number;
  currentProgress: number;
  completed: boolean;
  expiresAt: number;
}

export interface IVXFriendBattle {
  battleId: string;
  friendId: string;
  friendName: string;
  challengerScore: number;
  friendScore: number;
  status: 'pending' | 'active' | 'completed';
  expiresAt: number;
}

export interface IVXIapTriggerResult {
  shouldShow: boolean;
  offerId?: string;
  discount?: number;
  expiresAt?: number;
}

export interface IVXSmartAdResult {
  canShow: boolean;
  nextAvailableAt: number;
  reason?: string;
}

// ---------------------------------------------------------------------------
// Sub-system interfaces (for the grouped API surface)
// ---------------------------------------------------------------------------

interface SpinWheelAPI {
  /** Get the current spin-wheel state. */
  get(): Promise<IVXSpinWheelState>;
  /** Perform a spin and return the reward. */
  spin(): Promise<IVXSpinWheelResult>;
}

interface StreaksAPI {
  /** Get all streak states for the current user. */
  get(): Promise<IVXStreakState[]>;
  /** Record a check-in / update for a specific streak. */
  update(streakId: string): Promise<IVXStreakState>;
  /** Claim a milestone reward within a streak. */
  claimMilestone(streakId: string, milestone: number): Promise<IVXStreakState>;
}

interface OfferWallAPI {
  /** Get the current offer-wall state. */
  get(): Promise<IVXOfferWallState>;
  /** Mark an offer as completed. */
  complete(offerId: string): Promise<IVXOffer>;
  /** Claim all pending rewards from completed offers. */
  claimPending(): Promise<{ claimed: number }>;
}

interface RetentionAPI {
  /** Get the player's retention / daily-login state. */
  get(): Promise<IVXRetentionState>;
  /** Record today's login for retention tracking. */
  update(): Promise<IVXRetentionState>;
}

interface FriendQuestsAPI {
  /** Get all active friend quests. */
  getActive(): Promise<IVXFriendQuest[]>;
  /** Contribute progress to a friend quest. */
  contribute(questId: string, progress: number): Promise<IVXFriendQuest>;
}

interface FriendBattlesAPI {
  /** Challenge a friend with your score. */
  challenge(friendId: string, score: number): Promise<IVXFriendBattle>;
  /** Get all active friend battles. */
  getActive(): Promise<IVXFriendBattle[]>;
}

interface IapTriggerAPI {
  /** Check whether an IAP offer should be shown for the given event. */
  check(eventType: string): Promise<IVXIapTriggerResult>;
}

interface SmartAdTimerAPI {
  /** Check if an ad can be shown for the given placement. */
  canShowAd(placement: string): Promise<IVXSmartAdResult>;
}

// ---------------------------------------------------------------------------
// Main class
// ---------------------------------------------------------------------------

export class IVXHiroSystems {
  private static _instance: IVXHiroSystems | null = null;

  private _client: Client | null = null;
  private _session: Session | null = null;
  private _initialized = false;

  /** Return the shared singleton instance. */
  static getInstance(): IVXHiroSystems {
    if (!IVXHiroSystems._instance) {
      IVXHiroSystems._instance = new IVXHiroSystems();
    }
    return IVXHiroSystems._instance;
  }

  /** Reset the singleton (useful for testing). */
  static resetInstance(): void {
    IVXHiroSystems._instance = null;
  }

  private constructor() {}

  get isInitialized(): boolean { return this._initialized; }

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------

  /**
   * Provide an authenticated Nakama client + session so RPCs can be executed.
   * Call again whenever the session is refreshed.
   */
  initialize(client: Client, session: Session): void {
    if (!client) throw new Error('Nakama client is required.');
    if (!session) throw new Error('Nakama session is required.');
    this._client = client;
    this._session = session;
    this._initialized = true;
  }

  // ---------------------------------------------------------------------------
  // Grouped API surface
  // ---------------------------------------------------------------------------

  /** Spin-wheel reward system. */
  readonly spinWheel: SpinWheelAPI = {
    get: () => this.rpc<IVXSpinWheelState>('hiro_spin_wheel_get'),
    spin: () => this.rpc<IVXSpinWheelResult>('hiro_spin_wheel_spin'),
  };

  /** Daily / login streak tracking. */
  readonly streaks: StreaksAPI = {
    get: () => this.rpc<IVXStreakState[]>('hiro_streaks_get'),
    update: (streakId: string) =>
      this.rpc<IVXStreakState>('hiro_streaks_update', { streak_id: streakId }),
    claimMilestone: (streakId: string, milestone: number) =>
      this.rpc<IVXStreakState>('hiro_streaks_claim', { streak_id: streakId, milestone }),
  };

  /** Offer-wall: task-based rewards. */
  readonly offerwall: OfferWallAPI = {
    get: () => this.rpc<IVXOfferWallState>('hiro_offerwall_get'),
    complete: (offerId: string) =>
      this.rpc<IVXOffer>('hiro_offerwall_complete', { offer_id: offerId }),
    claimPending: () => this.rpc<{ claimed: number }>('hiro_offerwall_claim_pending'),
  };

  /** Retention / daily-login calendar. */
  readonly retention: RetentionAPI = {
    get: () => this.rpc<IVXRetentionState>('hiro_retention_get'),
    update: () => this.rpc<IVXRetentionState>('hiro_retention_update'),
  };

  /** Cooperative friend quests. */
  readonly friendQuests: FriendQuestsAPI = {
    getActive: () => this.rpc<IVXFriendQuest[]>('hiro_friend_quests_get_active'),
    contribute: (questId: string, progress: number) =>
      this.rpc<IVXFriendQuest>('hiro_friend_quests_contribute', { quest_id: questId, progress }),
  };

  /** Asynchronous friend-vs-friend battles. */
  readonly friendBattles: FriendBattlesAPI = {
    challenge: (friendId: string, score: number) =>
      this.rpc<IVXFriendBattle>('hiro_friend_battles_challenge', { friend_id: friendId, score }),
    getActive: () => this.rpc<IVXFriendBattle[]>('hiro_friend_battles_get_active'),
  };

  /** Context-sensitive IAP trigger checks. */
  readonly iapTrigger: IapTriggerAPI = {
    check: (eventType: string) =>
      this.rpc<IVXIapTriggerResult>('hiro_iap_trigger_check', { event_type: eventType }),
  };

  /** Smart ad-frequency manager. */
  readonly smartAdTimer: SmartAdTimerAPI = {
    canShowAd: (placement: string) =>
      this.rpc<IVXSmartAdResult>('hiro_smart_ad_can_show', { placement }),
  };

  // ---------------------------------------------------------------------------
  // Internal RPC helper
  // ---------------------------------------------------------------------------

  private async rpc<T>(rpcId: string, payload?: Record<string, unknown>): Promise<T> {
    this.ensureInitialized();
    const body = payload ? JSON.stringify(payload) : '{}';
    const result = await this._client!.rpc(this._session!, rpcId, body as unknown as object);
    const data = result.payload;
    if (typeof data === 'string') return JSON.parse(data) as T;
    if (typeof data === 'object' && data !== null) return data as unknown as T;
    return {} as T;
  }

  private ensureInitialized(): void {
    if (!this._initialized || !this._client || !this._session) {
      throw new Error('HiroSystems not initialized. Call initialize(client, session) first.');
    }
  }
}
