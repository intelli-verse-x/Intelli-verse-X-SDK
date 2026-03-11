const String sdkVersion = '5.1.0';

class IVXProfile {
  final String userId;
  final String username;
  final String displayName;
  final String avatarUrl;
  final String langTag;
  final Map<String, dynamic> metadata;
  final Map<String, num> wallet;

  const IVXProfile({
    required this.userId,
    required this.username,
    this.displayName = '',
    this.avatarUrl = '',
    this.langTag = '',
    this.metadata = const {},
    this.wallet = const {},
  });

  @override
  String toString() =>
      'IVXProfile(userId: $userId, username: $username, displayName: $displayName)';
}

class IVXLeaderboardRecord {
  final String ownerId;
  final String username;
  final int score;
  final int rank;

  const IVXLeaderboardRecord({
    required this.ownerId,
    required this.username,
    required this.score,
    required this.rank,
  });

  @override
  String toString() =>
      'IVXLeaderboardRecord(username: $username, score: $score, rank: $rank)';
}

class IVXError implements Exception {
  final int code;
  final String message;

  const IVXError({required this.code, required this.message});

  @override
  String toString() => 'IVXError(code: $code, message: $message)';
}

enum IVXEvent {
  initialized,
  authSuccess,
  authError,
  profileLoaded,
  walletUpdated,
  leaderboardFetched,
  storageRead,
  rpcResponse,
  error,
}
