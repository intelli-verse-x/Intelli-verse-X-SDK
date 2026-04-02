// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

// ---------------------------------------------------------------------------
// Hiro Systems — Spin wheel, streaks, offerwall, friends (Web3 SDK)
// ---------------------------------------------------------------------------

import { Client, Session } from '@heroiclabs/nakama-js';

export interface IVXSpinWheelReward {
  rewardId: string;
  type: string;
  amount: number;
  metadata?: Record<string, unknown>;
}

export interface IVXStreakInfo {
  currentStreak: number;
  longestStreak: number;
  claimedToday: boolean;
  lastClaimDate: string;
  rewards: IVXSpinWheelReward[];
}

export interface IVXOfferwallItem {
  offerId: string;
  title: string;
  description: string;
  imageUrl: string;
  cost: number;
  currency: string;
  available: boolean;
}

export interface IVXFriendInfo {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl: string;
  state: number;
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

export interface IVXHiroEventMap {
  wheelSpun: [reward: IVXSpinWheelReward];
  streakClaimed: [info: IVXStreakInfo];
  offerClaimed: [offerId: string];
  friendAdded: [userId: string];
  friendRemoved: [userId: string];
  retentionUpdated: [state: IVXRetentionState];
  iapTriggerChecked: [result: IVXIapTriggerResult];
  adTimerChecked: [result: IVXSmartAdResult];
  error: [error: { code: number; message: string }];
}

type HiroEventHandler<K extends keyof IVXHiroEventMap> = (...args: IVXHiroEventMap[K]) => void;

/**
 * Hiro-powered game systems accessed via Nakama RPC.
 *
 * Wraps spin wheel, daily streaks, offerwall, and friend management
 * RPCs with typed interfaces for the Web3 SDK.
 */
export class IVXHiroSystems {
  private static _instance: IVXHiroSystems | null = null;

  private _client: Client | null = null;
  private _session: Session | null = null;
  private _enableDebugLogs = false;
  private _listeners = new Map<string, Set<Function>>();

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

  /**
   * Bind a Nakama client and session for RPC calls.
   * Typically called after IVXWeb3Manager authenticates.
   */
  configure(client: Client, session: Session, enableDebugLogs = false): void {
    this._client = client;
    this._session = session;
    this._enableDebugLogs = enableDebugLogs;
    this.log('Hiro systems configured');
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  /** Subscribe to a Hiro event. */
  on<K extends keyof IVXHiroEventMap>(event: K, handler: HiroEventHandler<K>): void {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event)!.add(handler);
  }

  /** Unsubscribe from a Hiro event. */
  off<K extends keyof IVXHiroEventMap>(event: K, handler: HiroEventHandler<K>): void {
    this._listeners.get(event)?.delete(handler);
  }

  private emit<K extends keyof IVXHiroEventMap>(event: K, ...args: IVXHiroEventMap[K]): void {
    this._listeners.get(event)?.forEach(fn => (fn as Function)(...args));
  }

  // ---------------------------------------------------------------------------
  // Spin Wheel
  // ---------------------------------------------------------------------------

  /** Spin a prize wheel and return the reward. */
  async spinWheel(wheelId = 'default'): Promise<IVXSpinWheelReward> {
    const result = await this.callRpc('hiro_spin_wheel', JSON.stringify({ wheel_id: wheelId }));
    const reward: IVXSpinWheelReward = {
      rewardId: result.reward_id ?? '',
      type: result.type ?? '',
      amount: Number(result.amount ?? 0),
      metadata: result.metadata as Record<string, unknown> | undefined,
    };
    this.log(`Wheel spun: ${reward.type} x${reward.amount}`);
    this.emit('wheelSpun', reward);
    return reward;
  }

  /** Fetch the wheel configuration (segments, probabilities). */
  async getWheelConfig(wheelId = 'default'): Promise<Record<string, unknown>> {
    return this.callRpc('hiro_spin_wheel_config', JSON.stringify({ wheel_id: wheelId }));
  }

  // ---------------------------------------------------------------------------
  // Daily Streaks
  // ---------------------------------------------------------------------------

  /** Get the current streak state for the authenticated user. */
  async getStreak(): Promise<IVXStreakInfo> {
    const result = await this.callRpc('hiro_streak_get', '{}');
    return this.parseStreakInfo(result);
  }

  /** Claim today's streak reward. */
  async claimStreak(): Promise<IVXStreakInfo> {
    const result = await this.callRpc('hiro_streak_claim', '{}');
    const info = this.parseStreakInfo(result);
    this.log(`Streak claimed — day ${info.currentStreak}`);
    this.emit('streakClaimed', info);
    return info;
  }

  // ---------------------------------------------------------------------------
  // Offerwall
  // ---------------------------------------------------------------------------

  /** List available offerwall items. */
  async getOffers(): Promise<IVXOfferwallItem[]> {
    const result = await this.callRpc('hiro_offerwall_list', '{}');
    const offers: IVXOfferwallItem[] = Array.isArray(result.offers)
      ? result.offers.map((o: Record<string, unknown>) => ({
          offerId: String(o.offer_id ?? ''),
          title: String(o.title ?? ''),
          description: String(o.description ?? ''),
          imageUrl: String(o.image_url ?? ''),
          cost: Number(o.cost ?? 0),
          currency: String(o.currency ?? ''),
          available: Boolean(o.available ?? true),
        }))
      : [];
    return offers;
  }

  /** Claim (purchase) an offerwall item. */
  async claimOffer(offerId: string): Promise<void> {
    await this.callRpc('hiro_offerwall_claim', JSON.stringify({ offer_id: offerId }));
    this.log(`Offer claimed: ${offerId}`);
    this.emit('offerClaimed', offerId);
  }

  // ---------------------------------------------------------------------------
  // Friends
  // ---------------------------------------------------------------------------

  /** List friends, optionally filtered by state (0=mutual, 1=invite sent, etc.). */
  async listFriends(state = 0): Promise<IVXFriendInfo[]> {
    const result = await this.callRpc('hiro_friends_list', JSON.stringify({ state }));
    const friends: IVXFriendInfo[] = Array.isArray(result.friends)
      ? result.friends.map((f: Record<string, unknown>) => ({
          userId: String(f.user_id ?? ''),
          username: String(f.username ?? ''),
          displayName: String(f.display_name ?? ''),
          avatarUrl: String(f.avatar_url ?? ''),
          state: Number(f.state ?? 0),
        }))
      : [];
    return friends;
  }

  /** Send a friend request. */
  async addFriend(userId: string): Promise<void> {
    await this.callRpc('hiro_friends_add', JSON.stringify({ user_id: userId }));
    this.log(`Friend added: ${userId}`);
    this.emit('friendAdded', userId);
  }

  /** Remove a friend. */
  async removeFriend(userId: string): Promise<void> {
    await this.callRpc('hiro_friends_remove', JSON.stringify({ user_id: userId }));
    this.log(`Friend removed: ${userId}`);
    this.emit('friendRemoved', userId);
  }

  /** Block a user. */
  async blockUser(userId: string): Promise<void> {
    await this.callRpc('hiro_friends_block', JSON.stringify({ user_id: userId }));
    this.log(`User blocked: ${userId}`);
  }

  // ---------------------------------------------------------------------------
  // Retention
  // ---------------------------------------------------------------------------

  /** Get the player's retention / daily-login state. */
  async getRetentionState(): Promise<IVXRetentionState> {
    const result = await this.callRpc('hiro_retention_get', '{}');
    return {
      day: Number(result.day ?? 0),
      lastLoginAt: Number(result.last_login_at ?? 0),
      rewards: Array.isArray(result.rewards)
        ? result.rewards.map((r: Record<string, unknown>) => ({
            day: Number(r.day ?? 0),
            claimed: Boolean(r.claimed ?? false),
          }))
        : [],
    };
  }

  /** Record today's login for retention tracking. */
  async updateRetention(): Promise<IVXRetentionState> {
    const result = await this.callRpc('hiro_retention_update', '{}');
    return {
      day: Number(result.day ?? 0),
      lastLoginAt: Number(result.last_login_at ?? 0),
      rewards: Array.isArray(result.rewards)
        ? result.rewards.map((r: Record<string, unknown>) => ({
            day: Number(r.day ?? 0),
            claimed: Boolean(r.claimed ?? false),
          }))
        : [],
    };
  }

  // ---------------------------------------------------------------------------
  // Friend Quests
  // ---------------------------------------------------------------------------

  /** Get all active friend quests. */
  async getActiveFriendQuests(): Promise<IVXFriendQuest[]> {
    const result = await this.callRpc('hiro_friend_quests_get_active', '{}');
    return Array.isArray(result.quests)
      ? result.quests.map((q: Record<string, unknown>) => ({
          questId: String(q.quest_id ?? ''),
          title: String(q.title ?? ''),
          description: String(q.description ?? ''),
          targetProgress: Number(q.target_progress ?? 0),
          currentProgress: Number(q.current_progress ?? 0),
          completed: Boolean(q.completed ?? false),
          expiresAt: Number(q.expires_at ?? 0),
        }))
      : [];
  }

  /** Contribute progress to a friend quest. */
  async contributeFriendQuest(questId: string, progress: number): Promise<void> {
    await this.callRpc('hiro_friend_quests_contribute', JSON.stringify({ quest_id: questId, progress }));
    this.log(`Friend quest progress: ${questId} +${progress}`);
  }

  // ---------------------------------------------------------------------------
  // Friend Battles
  // ---------------------------------------------------------------------------

  /** Challenge a friend with your score. */
  async challengeFriend(friendId: string, score: number): Promise<void> {
    await this.callRpc('hiro_friend_battles_challenge', JSON.stringify({ friend_id: friendId, score }));
    this.log(`Friend battle challenge sent to ${friendId}`);
  }

  /** Get all active friend battles. */
  async getActiveFriendBattles(): Promise<IVXFriendBattle[]> {
    const result = await this.callRpc('hiro_friend_battles_get_active', '{}');
    return Array.isArray(result.battles)
      ? result.battles.map((b: Record<string, unknown>) => ({
          battleId: String(b.battle_id ?? ''),
          friendId: String(b.friend_id ?? ''),
          friendName: String(b.friend_name ?? ''),
          challengerScore: Number(b.challenger_score ?? 0),
          friendScore: Number(b.friend_score ?? 0),
          status: String(b.status ?? 'pending') as 'pending' | 'active' | 'completed',
          expiresAt: Number(b.expires_at ?? 0),
        }))
      : [];
  }

  // ---------------------------------------------------------------------------
  // IAP Trigger
  // ---------------------------------------------------------------------------

  /** Check whether an IAP offer should be shown for the given event. */
  async checkIapTrigger(eventType: string): Promise<IVXIapTriggerResult> {
    const result = await this.callRpc('hiro_iap_trigger_check', JSON.stringify({ event_type: eventType }));
    return {
      shouldShow: Boolean(result.should_show ?? false),
      offerId: result.offer_id != null ? String(result.offer_id) : undefined,
      discount: result.discount != null ? Number(result.discount) : undefined,
      expiresAt: result.expires_at != null ? Number(result.expires_at) : undefined,
    };
  }

  // ---------------------------------------------------------------------------
  // Smart Ad Timer
  // ---------------------------------------------------------------------------

  /** Check if an ad can be shown for the given placement. */
  async canShowAd(placement: string): Promise<IVXSmartAdResult> {
    const result = await this.callRpc('hiro_smart_ad_can_show', JSON.stringify({ placement }));
    return {
      canShow: Boolean(result.can_show ?? false),
      nextAvailableAt: Number(result.next_available_at ?? 0),
      reason: result.reason != null ? String(result.reason) : undefined,
    };
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  private async callRpc(rpcId: string, payload = '{}'): Promise<Record<string, unknown>> {
    this.ensureConfigured();
    try {
      const result = await this._client!.rpc(this._session!, rpcId, payload);
      this.log(`RPC ${rpcId} response received`);
      return result.payload ? this.safeParseJson(result.payload) : {};
    } catch (e: unknown) {
      const error = this.toError(e);
      this.emit('error', error);
      throw error;
    }
  }

  private parseStreakInfo(data: Record<string, unknown>): IVXStreakInfo {
    return {
      currentStreak: Number(data.current_streak ?? 0),
      longestStreak: Number(data.longest_streak ?? 0),
      claimedToday: Boolean(data.claimed_today ?? false),
      lastClaimDate: String(data.last_claim_date ?? ''),
      rewards: Array.isArray(data.rewards)
        ? data.rewards.map((r: Record<string, unknown>) => ({
            rewardId: String(r.reward_id ?? ''),
            type: String(r.type ?? ''),
            amount: Number(r.amount ?? 0),
          }))
        : [],
    };
  }

  private ensureConfigured(): void {
    if (!this._client || !this._session) {
      const err = { code: -1, message: 'Hiro systems not configured. Call configure() first.' };
      this.emit('error', err);
      throw err;
    }
  }

  private safeParseJson(value: unknown): Record<string, unknown> {
    if (typeof value === 'object' && value !== null) return value as Record<string, unknown>;
    if (typeof value !== 'string' || value === '') return {};
    try { return JSON.parse(value); } catch { return {}; }
  }

  private toError(e: unknown): { code: number; message: string } {
    if (typeof e === 'object' && e !== null && 'code' in e && 'message' in e) {
      return e as { code: number; message: string };
    }
    if (e instanceof Error) return { code: -1, message: e.message };
    return { code: -1, message: String(e) };
  }

  private log(message: string): void {
    if (this._enableDebugLogs) {
      console.log(`[IntelliVerseX Web3:Hiro] ${message}`);
    }
  }
}
