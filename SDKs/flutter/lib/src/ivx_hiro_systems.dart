import 'dart:convert';

import 'package:nakama/nakama.dart';

import 'types.dart';

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

/// Result of a spin-wheel action.
class IVXSpinWheelResult {
  final String rewardId;
  final String rewardType;
  final int amount;
  final int spinsRemaining;
  final DateTime? nextFreeSpinAt;

  const IVXSpinWheelResult({
    required this.rewardId,
    required this.rewardType,
    required this.amount,
    this.spinsRemaining = 0,
    this.nextFreeSpinAt,
  });

  factory IVXSpinWheelResult.fromJson(Map<String, dynamic> json) =>
      IVXSpinWheelResult(
        rewardId: json['reward_id'] as String? ?? '',
        rewardType: json['reward_type'] as String? ?? '',
        amount: json['amount'] as int? ?? 0,
        spinsRemaining: json['spins_remaining'] as int? ?? 0,
        nextFreeSpinAt: json['next_free_spin_at'] != null
            ? DateTime.tryParse(json['next_free_spin_at'] as String)
            : null,
      );

  @override
  String toString() =>
      'IVXSpinWheelResult(reward: $rewardType x$amount, spinsLeft: $spinsRemaining)';
}

/// Current state of a daily-streak system.
class IVXStreakState {
  final int currentStreak;
  final int longestStreak;
  final bool claimedToday;
  final DateTime? lastClaimAt;
  final Map<String, dynamic> rewards;

  const IVXStreakState({
    required this.currentStreak,
    this.longestStreak = 0,
    this.claimedToday = false,
    this.lastClaimAt,
    this.rewards = const {},
  });

  factory IVXStreakState.fromJson(Map<String, dynamic> json) => IVXStreakState(
        currentStreak: json['current_streak'] as int? ?? 0,
        longestStreak: json['longest_streak'] as int? ?? 0,
        claimedToday: json['claimed_today'] as bool? ?? false,
        lastClaimAt: json['last_claim_at'] != null
            ? DateTime.tryParse(json['last_claim_at'] as String)
            : null,
        rewards: json['rewards'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXStreakState(current: $currentStreak, claimedToday: $claimedToday)';
}

/// An offer surfaced by the offerwall or IAP-trigger system.
class IVXOffer {
  final String offerId;
  final String title;
  final String description;
  final String rewardType;
  final int rewardAmount;
  final DateTime? expiresAt;
  final Map<String, dynamic> metadata;

  const IVXOffer({
    required this.offerId,
    required this.title,
    this.description = '',
    required this.rewardType,
    required this.rewardAmount,
    this.expiresAt,
    this.metadata = const {},
  });

  factory IVXOffer.fromJson(Map<String, dynamic> json) => IVXOffer(
        offerId: json['offer_id'] as String? ?? '',
        title: json['title'] as String? ?? '',
        description: json['description'] as String? ?? '',
        rewardType: json['reward_type'] as String? ?? '',
        rewardAmount: json['reward_amount'] as int? ?? 0,
        expiresAt: json['expires_at'] != null
            ? DateTime.tryParse(json['expires_at'] as String)
            : null,
        metadata: json['metadata'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() => 'IVXOffer(id: $offerId, title: $title)';
}

/// Retention metrics returned by the retention sub-system.
class IVXRetentionState {
  final int daysSinceInstall;
  final int sessionsThisWeek;
  final bool atRisk;
  final Map<String, dynamic> incentives;

  const IVXRetentionState({
    this.daysSinceInstall = 0,
    this.sessionsThisWeek = 0,
    this.atRisk = false,
    this.incentives = const {},
  });

  factory IVXRetentionState.fromJson(Map<String, dynamic> json) =>
      IVXRetentionState(
        daysSinceInstall: json['days_since_install'] as int? ?? 0,
        sessionsThisWeek: json['sessions_this_week'] as int? ?? 0,
        atRisk: json['at_risk'] as bool? ?? false,
        incentives: json['incentives'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXRetentionState(days: $daysSinceInstall, atRisk: $atRisk)';
}

/// Describes a friend-quest.
class IVXFriendQuest {
  final String questId;
  final String title;
  final String friendUserId;
  final double progress;
  final bool completed;
  final Map<String, dynamic> rewards;

  const IVXFriendQuest({
    required this.questId,
    required this.title,
    required this.friendUserId,
    this.progress = 0.0,
    this.completed = false,
    this.rewards = const {},
  });

  factory IVXFriendQuest.fromJson(Map<String, dynamic> json) => IVXFriendQuest(
        questId: json['quest_id'] as String? ?? '',
        title: json['title'] as String? ?? '',
        friendUserId: json['friend_user_id'] as String? ?? '',
        progress: (json['progress'] as num?)?.toDouble() ?? 0.0,
        completed: json['completed'] as bool? ?? false,
        rewards: json['rewards'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXFriendQuest(id: $questId, progress: $progress, done: $completed)';
}

/// A friend-battle challenge.
class IVXFriendBattle {
  final String battleId;
  final String opponentUserId;
  final String status;
  final Map<String, dynamic> result;

  const IVXFriendBattle({
    required this.battleId,
    required this.opponentUserId,
    this.status = 'pending',
    this.result = const {},
  });

  factory IVXFriendBattle.fromJson(Map<String, dynamic> json) =>
      IVXFriendBattle(
        battleId: json['battle_id'] as String? ?? '',
        opponentUserId: json['opponent_user_id'] as String? ?? '',
        status: json['status'] as String? ?? 'pending',
        result: json['result'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXFriendBattle(id: $battleId, opponent: $opponentUserId, status: $status)';
}

/// Recommendation from the smart-ad-timer system.
class IVXAdTimerRecommendation {
  final bool shouldShowAd;
  final String adType;
  final int cooldownSeconds;
  final Map<String, dynamic> metadata;

  const IVXAdTimerRecommendation({
    required this.shouldShowAd,
    this.adType = 'interstitial',
    this.cooldownSeconds = 0,
    this.metadata = const {},
  });

  factory IVXAdTimerRecommendation.fromJson(Map<String, dynamic> json) =>
      IVXAdTimerRecommendation(
        shouldShowAd: json['should_show_ad'] as bool? ?? false,
        adType: json['ad_type'] as String? ?? 'interstitial',
        cooldownSeconds: json['cooldown_seconds'] as int? ?? 0,
        metadata: json['metadata'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXAdTimerRecommendation(show: $shouldShowAd, type: $adType)';
}

/// IAP trigger recommendation.
class IVXIapTrigger {
  final String offerId;
  final String productId;
  final String placement;
  final bool shouldShow;
  final Map<String, dynamic> metadata;

  const IVXIapTrigger({
    required this.offerId,
    required this.productId,
    this.placement = '',
    this.shouldShow = false,
    this.metadata = const {},
  });

  factory IVXIapTrigger.fromJson(Map<String, dynamic> json) => IVXIapTrigger(
        offerId: json['offer_id'] as String? ?? '',
        productId: json['product_id'] as String? ?? '',
        placement: json['placement'] as String? ?? '',
        shouldShow: json['should_show'] as bool? ?? false,
        metadata: json['metadata'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXIapTrigger(offer: $offerId, product: $productId, show: $shouldShow)';
}

// ---------------------------------------------------------------------------
// Sub-system wrappers
// ---------------------------------------------------------------------------

/// Spin-wheel operations backed by Hiro RPCs.
class IVXSpinWheelSystem {
  final IVXHiroSystems _parent;
  IVXSpinWheelSystem._(this._parent);

  /// Fetch the current wheel configuration and remaining spins.
  Future<IVXSpinWheelResult> getState() async {
    final json = await _parent._rpc('hiro_spin_wheel_get');
    return IVXSpinWheelResult.fromJson(json);
  }

  /// Execute a spin and return the reward.
  Future<IVXSpinWheelResult> spin() async {
    final json = await _parent._rpc('hiro_spin_wheel_spin');
    return IVXSpinWheelResult.fromJson(json);
  }
}

/// Daily-streak operations.
class IVXStreakSystem {
  final IVXHiroSystems _parent;
  IVXStreakSystem._(this._parent);

  /// Fetch the player's streak state.
  Future<IVXStreakState> getState() async {
    final json = await _parent._rpc('hiro_streaks_get');
    return IVXStreakState.fromJson(json);
  }

  /// Claim today's streak reward.
  Future<IVXStreakState> claim() async {
    final json = await _parent._rpc('hiro_streaks_claim');
    return IVXStreakState.fromJson(json);
  }
}

/// Offerwall operations.
class IVXOfferwallSystem {
  final IVXHiroSystems _parent;
  IVXOfferwallSystem._(this._parent);

  /// List all active offers.
  Future<List<IVXOffer>> list() async {
    final json = await _parent._rpc('hiro_offerwall_list');
    final items = json['offers'] as List<dynamic>? ?? [];
    return items
        .cast<Map<String, dynamic>>()
        .map(IVXOffer.fromJson)
        .toList();
  }

  /// Claim an offer by [offerId].
  Future<IVXOffer> claim(String offerId) async {
    final json = await _parent._rpc(
      'hiro_offerwall_claim',
      {'offer_id': offerId},
    );
    return IVXOffer.fromJson(json);
  }
}

/// Retention-insight operations.
class IVXRetentionSystem {
  final IVXHiroSystems _parent;
  IVXRetentionSystem._(this._parent);

  /// Fetch the player's retention metrics.
  Future<IVXRetentionState> getState() async {
    final json = await _parent._rpc('hiro_retention_get');
    return IVXRetentionState.fromJson(json);
  }
}

/// Friend-quest operations.
class IVXFriendQuestSystem {
  final IVXHiroSystems _parent;
  IVXFriendQuestSystem._(this._parent);

  /// List active friend quests.
  Future<List<IVXFriendQuest>> list() async {
    final json = await _parent._rpc('hiro_friend_quests_list');
    final items = json['quests'] as List<dynamic>? ?? [];
    return items
        .cast<Map<String, dynamic>>()
        .map(IVXFriendQuest.fromJson)
        .toList();
  }

  /// Accept a quest by [questId].
  Future<IVXFriendQuest> accept(String questId) async {
    final json = await _parent._rpc(
      'hiro_friend_quests_accept',
      {'quest_id': questId},
    );
    return IVXFriendQuest.fromJson(json);
  }

  /// Update progress on [questId] by [delta].
  Future<IVXFriendQuest> updateProgress(String questId, double delta) async {
    final json = await _parent._rpc(
      'hiro_friend_quests_progress',
      {'quest_id': questId, 'delta': delta},
    );
    return IVXFriendQuest.fromJson(json);
  }
}

/// Friend-battle operations.
class IVXFriendBattleSystem {
  final IVXHiroSystems _parent;
  IVXFriendBattleSystem._(this._parent);

  /// List pending and active battles.
  Future<List<IVXFriendBattle>> list() async {
    final json = await _parent._rpc('hiro_friend_battles_list');
    final items = json['battles'] as List<dynamic>? ?? [];
    return items
        .cast<Map<String, dynamic>>()
        .map(IVXFriendBattle.fromJson)
        .toList();
  }

  /// Challenge a friend by [opponentUserId].
  Future<IVXFriendBattle> challenge(String opponentUserId) async {
    final json = await _parent._rpc(
      'hiro_friend_battles_challenge',
      {'opponent_user_id': opponentUserId},
    );
    return IVXFriendBattle.fromJson(json);
  }

  /// Accept a battle by [battleId].
  Future<IVXFriendBattle> accept(String battleId) async {
    final json = await _parent._rpc(
      'hiro_friend_battles_accept',
      {'battle_id': battleId},
    );
    return IVXFriendBattle.fromJson(json);
  }
}

/// IAP-trigger evaluation system.
class IVXIapTriggerSystem {
  final IVXHiroSystems _parent;
  IVXIapTriggerSystem._(this._parent);

  /// Evaluate whether an IAP trigger should fire at [placement].
  Future<IVXIapTrigger> evaluate(String placement) async {
    final json = await _parent._rpc(
      'hiro_iap_trigger_evaluate',
      {'placement': placement},
    );
    return IVXIapTrigger.fromJson(json);
  }

  /// Record that the player purchased [productId].
  Future<void> recordPurchase(String productId) async {
    await _parent._rpc(
      'hiro_iap_trigger_purchase',
      {'product_id': productId},
    );
  }
}

/// Smart-ad-timer system.
class IVXSmartAdTimerSystem {
  final IVXHiroSystems _parent;
  IVXSmartAdTimerSystem._(this._parent);

  /// Ask the server whether an ad should be shown now.
  Future<IVXAdTimerRecommendation> check({String adType = 'interstitial'}) async {
    final json = await _parent._rpc(
      'hiro_smart_ad_timer_check',
      {'ad_type': adType},
    );
    return IVXAdTimerRecommendation.fromJson(json);
  }

  /// Record that an ad impression occurred.
  Future<void> recordImpression(String adType) async {
    await _parent._rpc(
      'hiro_smart_ad_timer_impression',
      {'ad_type': adType},
    );
  }
}

// ---------------------------------------------------------------------------
// Main facade
// ---------------------------------------------------------------------------

/// Unified access to Hiro-powered engagement systems via Nakama RPCs.
///
/// Initialize with an authenticated Nakama [Client] and [Session], then
/// interact through the typed sub-objects.
///
/// ```dart
/// final hiro = IVXHiroSystems.instance;
/// hiro.initialize(nakamaClient, nakamaSession);
/// final wheel = await hiro.spinWheel.spin();
/// final streak = await hiro.streaks.getState();
/// ```
class IVXHiroSystems {
  static IVXHiroSystems? _instance;

  Client? _client;
  Session? _session;
  bool _initialized = false;

  late final IVXSpinWheelSystem spinWheel;
  late final IVXStreakSystem streaks;
  late final IVXOfferwallSystem offerwall;
  late final IVXRetentionSystem retention;
  late final IVXFriendQuestSystem friendQuests;
  late final IVXFriendBattleSystem friendBattles;
  late final IVXIapTriggerSystem iapTrigger;
  late final IVXSmartAdTimerSystem smartAdTimer;

  IVXHiroSystems._() {
    spinWheel = IVXSpinWheelSystem._(this);
    streaks = IVXStreakSystem._(this);
    offerwall = IVXOfferwallSystem._(this);
    retention = IVXRetentionSystem._(this);
    friendQuests = IVXFriendQuestSystem._(this);
    friendBattles = IVXFriendBattleSystem._(this);
    iapTrigger = IVXIapTriggerSystem._(this);
    smartAdTimer = IVXSmartAdTimerSystem._(this);
  }

  /// Singleton accessor.
  static IVXHiroSystems get instance => _instance ??= IVXHiroSystems._();

  /// Reset the singleton (useful for testing).
  static void resetInstance() => _instance = null;

  /// Whether [initialize] has been called with a valid client and session.
  bool get isInitialized => _initialized;

  // ---------------------------------------------------------------------------
  // Initialization
  // ---------------------------------------------------------------------------

  /// Wire up Hiro systems with an authenticated Nakama [client] and [session].
  void initialize(Client client, Session session) {
    _client = client;
    _session = session;
    _initialized = true;
    _log('Hiro systems initialized for user ${session.userId}');
  }

  /// Update the session (e.g. after token refresh) without full re-init.
  void updateSession(Session session) {
    _session = session;
    _log('Session updated');
  }

  // ---------------------------------------------------------------------------
  // Internal RPC helper
  // ---------------------------------------------------------------------------

  Future<Map<String, dynamic>> _rpc(
    String rpcId, [
    Map<String, dynamic>? payload,
  ]) async {
    _ensureInitialized();
    try {
      final result = await _client!.rpc(
        _session!,
        id: rpcId,
        payload: payload != null ? jsonEncode(payload) : '{}',
      );
      _log('RPC $rpcId OK');
      return _safeDecodeJson(result.payload);
    } catch (e) {
      _log('RPC $rpcId failed: $e');
      throw _toIVXError(e);
    }
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  void _ensureInitialized() {
    if (!_initialized || _client == null || _session == null) {
      throw const IVXError(
        code: -1,
        message:
            'IVXHiroSystems not initialized. Call initialize(client, session) first.',
      );
    }
  }

  Map<String, dynamic> _safeDecodeJson(dynamic value) {
    if (value is Map<String, dynamic>) return value;
    if (value is String && value.isNotEmpty) {
      try {
        final decoded = jsonDecode(value);
        if (decoded is Map<String, dynamic>) return decoded;
      } catch (_) {}
    }
    return {};
  }

  IVXError _toIVXError(dynamic e) {
    if (e is IVXError) return e;
    return IVXError(code: -1, message: e.toString());
  }

  void _log(String message) {
    // ignore: avoid_print
    print('[IntelliVerseX:Hiro] $message');
  }
}
