import 'dart:convert';

import 'package:nakama/nakama.dart';

import 'ivx_config.dart';
import 'types.dart';

typedef IVXEventHandler = void Function(dynamic data);

/// Central coordinator for the IntelliVerseX Flutter SDK.
///
/// Provides singleton access to authentication, profile management,
/// wallet/economy, leaderboards, cloud storage, RPC, and real-time sockets.
class IVXManager {
  static IVXManager? _instance;

  Client? _client;
  Session? _session;
  IVXConfig _config = const IVXConfig();
  bool _initialized = false;
  final Map<IVXEvent, Set<IVXEventHandler>> _listeners = {};
  String? _cachedDeviceId;

  IVXManager._();

  static IVXManager get instance => _instance ??= IVXManager._();

  /// Reset the singleton (useful for testing).
  static void resetInstance() => _instance = null;

  Client? get client => _client;
  Session? get session => _session;
  bool get isInitialized => _initialized;
  String get userId => _session?.userId ?? '';
  String get username => _session?.username ?? '';

  bool get hasValidSession {
    if (_session == null) return false;
    final now = DateTime.now();
    return _session!.hasExpired(now) == false;
  }

  // ---------------------------------------------------------------------------
  // Initialization
  // ---------------------------------------------------------------------------

  void initialize(IVXConfig config) {
    config.validate();
    _config = config;

    _client = getNakamaClient(
      host: _config.nakamaHost,
      httpPort: _config.nakamaPort,
      serverKey: _config.nakamaServerKey,
      ssl: _config.useSSL,
    );

    _initialized = true;
    _log('SDK v$sdkVersion initialized - '
        '${_config.useSSL ? "https" : "http"}://${_config.nakamaHost}:${_config.nakamaPort}');
    _emit(IVXEvent.initialized);
  }

  // ---------------------------------------------------------------------------
  // Events
  // ---------------------------------------------------------------------------

  void on(IVXEvent event, IVXEventHandler handler) {
    _listeners.putIfAbsent(event, () => {});
    _listeners[event]!.add(handler);
  }

  void off(IVXEvent event, IVXEventHandler handler) {
    _listeners[event]?.remove(handler);
  }

  void _emit(IVXEvent event, [dynamic data]) {
    final handlers = _listeners[event];
    if (handlers == null) return;
    for (final handler in handlers) {
      handler(data);
    }
  }

  // ---------------------------------------------------------------------------
  // Authentication
  // ---------------------------------------------------------------------------

  Future<void> authenticateDevice([String? deviceId]) async {
    _ensureInitialized();
    final resolvedId = deviceId ?? _generateDeviceId();
    try {
      _session = await _client!.authenticateDevice(deviceId: resolvedId);
      _onAuthSuccess();
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.authError, error);
      throw error;
    }
  }

  Future<void> authenticateEmail(
    String email,
    String password, {
    bool create = false,
  }) async {
    _ensureInitialized();
    try {
      _session = await _client!.authenticateEmail(
        email: email,
        password: password,
        create: create,
      );
      _onAuthSuccess();
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.authError, error);
      throw error;
    }
  }

  Future<void> authenticateGoogle(String token) async {
    _ensureInitialized();
    try {
      _session = await _client!.authenticateGoogle(token: token);
      _onAuthSuccess();
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.authError, error);
      throw error;
    }
  }

  Future<void> authenticateApple(String token) async {
    _ensureInitialized();
    try {
      _session = await _client!.authenticateApple(token: token);
      _onAuthSuccess();
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.authError, error);
      throw error;
    }
  }

  Future<void> authenticateCustom(String customId) async {
    _ensureInitialized();
    try {
      _session = await _client!.authenticateCustom(id: customId);
      _onAuthSuccess();
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.authError, error);
      throw error;
    }
  }

  void clearSession() {
    _session = null;
    _log('Session cleared');
  }

  // ---------------------------------------------------------------------------
  // Profile
  // ---------------------------------------------------------------------------

  Future<IVXProfile> fetchProfile() async {
    _ensureSession();
    try {
      final account = await _client!.getAccount(_session!);
      final profile = IVXProfile(
        userId: account.user?.id ?? '',
        username: account.user?.username ?? '',
        displayName: account.user?.displayName ?? '',
        avatarUrl: account.user?.avatarUrl ?? '',
        langTag: account.user?.langTag ?? '',
        metadata: _safeDecodeJson(account.user?.metadata),
        wallet: _safeDecodeWallet(account.wallet),
      );
      _emit(IVXEvent.profileLoaded, profile);
      return profile;
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  Future<void> updateProfile({
    String? displayName,
    String? avatarUrl,
    String? langTag,
  }) async {
    _ensureSession();
    try {
      await _client!.updateAccount(
        _session!,
        displayName: displayName,
        avatarUrl: avatarUrl,
        langTag: langTag,
      );
      _log('Profile updated');
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  // ---------------------------------------------------------------------------
  // Wallet / Economy
  // ---------------------------------------------------------------------------

  Future<Map<String, dynamic>> fetchWallet() async {
    final result = await callRpc('hiro_economy_list');
    _emit(IVXEvent.walletUpdated, result);
    return result;
  }

  Future<Map<String, dynamic>> grantCurrency(
      String currencyId, int amount) async {
    return callRpc('hiro_economy_grant',
        jsonEncode({'currencies': {currencyId: amount}}));
  }

  // ---------------------------------------------------------------------------
  // Leaderboards
  // ---------------------------------------------------------------------------

  Future<void> submitScore(String leaderboardId, int score) async {
    _ensureSession();
    try {
      await _client!.writeLeaderboardRecord(
        _session!,
        leaderboardId: leaderboardId,
        score: score,
      );
      _log('Score submitted: $score to $leaderboardId');
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  Future<List<IVXLeaderboardRecord>> fetchLeaderboard(
    String leaderboardId, {
    int limit = 20,
  }) async {
    _ensureSession();
    try {
      final result = await _client!.listLeaderboardRecords(
        _session!,
        leaderboardId: leaderboardId,
        limit: limit,
      );
      final records = (result.records ?? [])
          .map((r) => IVXLeaderboardRecord(
                ownerId: r.ownerId ?? '',
                username: r.username?.value ?? '',
                score: int.tryParse(r.score ?? '0') ?? 0,
                rank: int.tryParse(r.rank ?? '0') ?? 0,
              ))
          .toList();
      _emit(IVXEvent.leaderboardFetched, records);
      return records;
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  // ---------------------------------------------------------------------------
  // Storage
  // ---------------------------------------------------------------------------

  Future<void> writeStorage(
      String collection, String key, Map<String, dynamic> value) async {
    _ensureSession();
    try {
      await _client!.writeStorageObjects(
        _session!,
        objects: [
          StorageObjectWrite(
            collection: collection,
            key: key,
            value: jsonEncode(value),
            permissionRead: 1,
            permissionWrite: 1,
          ),
        ],
      );
      _log('Storage write: $collection/$key');
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  Future<Map<String, dynamic>?> readStorage(
      String collection, String key) async {
    _ensureSession();
    try {
      final result = await _client!.readStorageObjects(
        _session!,
        objectIds: [
          StorageObjectId(
            collection: collection,
            key: key,
            userId: userId,
          ),
        ],
      );
      if (result.objects != null && result.objects!.isNotEmpty) {
        final data = _safeDecodeJson(result.objects!.first.value);
        _emit(IVXEvent.storageRead, data);
        return data;
      }
      _emit(IVXEvent.storageRead, null);
      return null;
    } catch (e) {
      final error = _toIVXError(e);
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  // ---------------------------------------------------------------------------
  // RPC
  // ---------------------------------------------------------------------------

  Future<Map<String, dynamic>> callRpc(String rpcId,
      [String payload = '{}']) async {
    _ensureSession();
    try {
      final result = await _client!.rpc(_session!, id: rpcId, payload: payload);
      _log('RPC $rpcId response received');
      final data = _safeDecodeJson(result.payload);
      _emit(IVXEvent.rpcResponse, {'rpcId': rpcId, 'data': data});
      return data;
    } catch (e) {
      final error = _toIVXError(e);
      _log('RPC $rpcId failed: ${error.message}');
      _emit(IVXEvent.error, error);
      throw error;
    }
  }

  // ---------------------------------------------------------------------------
  // Internal helpers
  // ---------------------------------------------------------------------------

  void _onAuthSuccess() {
    _log('Authenticated - UserId: ${_session!.userId}');
    _syncMetadata();
    _emit(IVXEvent.authSuccess, _session!.userId);
  }

  Future<void> _syncMetadata() async {
    if (!hasValidSession) return;
    try {
      await callRpc(
        'ivx_sync_metadata',
        jsonEncode({
          'metadata': {
            'sdk_version': sdkVersion,
            'platform': 'dart',
            'engine': 'flutter',
          }
        }),
      );
    } catch (_) {
      // Non-fatal: metadata sync failure should not break the app
    }
  }

  String _generateDeviceId() {
    if (_cachedDeviceId != null) return _cachedDeviceId!;
    final now = DateTime.now().microsecondsSinceEpoch.toRadixString(36);
    final rand1 = (DateTime.now().millisecond * 31337).toRadixString(36);
    final rand2 = now.hashCode.abs().toRadixString(36);
    _cachedDeviceId = 'dart-$now-$rand1-$rand2';
    return _cachedDeviceId!;
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

  Map<String, num> _safeDecodeWallet(dynamic value) {
    final raw = _safeDecodeJson(value);
    return raw.map((k, v) => MapEntry(k, (v is num) ? v : 0));
  }

  IVXError _toIVXError(dynamic e) {
    if (e is IVXError) return e;
    return IVXError(code: -1, message: e.toString());
  }

  void _ensureInitialized() {
    if (!_initialized || _client == null) {
      final err = const IVXError(
        code: -1,
        message: 'SDK not initialized. Call initialize() first.',
      );
      _emit(IVXEvent.error, err);
      throw err;
    }
  }

  void _ensureSession() {
    _ensureInitialized();
    if (!hasValidSession) {
      final err = const IVXError(
        code: -1,
        message: 'No valid session. Authenticate first.',
      );
      _emit(IVXEvent.error, err);
      throw err;
    }
  }

  void _log(String message) {
    if (_config.enableDebugLogs) {
      print('[IntelliVerseX] $message');
    }
  }
}
