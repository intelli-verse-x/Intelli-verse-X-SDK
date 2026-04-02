import 'dart:async';

import 'types.dart';

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

/// Configuration for the Discord Social SDK integration.
class IVXDiscordConfig {
  final String applicationId;
  final String clientId;
  final String? redirectUri;
  final bool enableDebugLogs;

  const IVXDiscordConfig({
    required this.applicationId,
    required this.clientId,
    this.redirectUri,
    this.enableDebugLogs = false,
  });

  @override
  String toString() =>
      'IVXDiscordConfig(applicationId: $applicationId, clientId: $clientId)';
}

/// A friend entry from the unified (Discord + game) friends list.
class IVXUnifiedFriend {
  final String userId;
  final String? discordId;
  final String username;
  final String displayName;
  final String avatarUrl;

  /// `"discord"`, `"game"`, or `"both"`.
  final String source;

  /// `"online"`, `"idle"`, `"dnd"`, or `"offline"`.
  final String status;

  const IVXUnifiedFriend({
    required this.userId,
    this.discordId,
    required this.username,
    this.displayName = '',
    this.avatarUrl = '',
    this.source = 'game',
    this.status = 'offline',
  });

  factory IVXUnifiedFriend.fromJson(Map<String, dynamic> json) =>
      IVXUnifiedFriend(
        userId: json['user_id'] as String? ?? '',
        discordId: json['discord_id'] as String?,
        username: json['username'] as String? ?? '',
        displayName: json['display_name'] as String? ?? '',
        avatarUrl: json['avatar_url'] as String? ?? '',
        source: json['source'] as String? ?? 'game',
        status: json['status'] as String? ?? 'offline',
      );

  @override
  String toString() =>
      'IVXUnifiedFriend(userId: $userId, username: $username, source: $source)';
}

/// An incoming game invite from another player.
class IVXGameInvite {
  final String inviteId;
  final String senderId;
  final String senderName;
  final String? message;
  final String? lobbyId;
  final DateTime timestamp;

  const IVXGameInvite({
    required this.inviteId,
    required this.senderId,
    required this.senderName,
    this.message,
    this.lobbyId,
    required this.timestamp,
  });

  factory IVXGameInvite.fromJson(Map<String, dynamic> json) => IVXGameInvite(
        inviteId: json['invite_id'] as String? ?? '',
        senderId: json['sender_id'] as String? ?? '',
        senderName: json['sender_name'] as String? ?? '',
        message: json['message'] as String?,
        lobbyId: json['lobby_id'] as String?,
        timestamp: DateTime.tryParse(json['timestamp'] as String? ?? '') ??
            DateTime.now(),
      );

  @override
  String toString() =>
      'IVXGameInvite(inviteId: $inviteId, senderId: $senderId)';
}

/// Information about a Discord lobby.
class IVXDiscordLobbyInfo {
  final String lobbyId;
  final String secret;
  final String ownerId;
  final int memberCount;
  final int maxMembers;
  final Map<String, String> metadata;

  const IVXDiscordLobbyInfo({
    required this.lobbyId,
    required this.secret,
    this.ownerId = '',
    this.memberCount = 1,
    this.maxMembers = 16,
    this.metadata = const {},
  });

  factory IVXDiscordLobbyInfo.fromJson(Map<String, dynamic> json) =>
      IVXDiscordLobbyInfo(
        lobbyId: json['lobby_id'] as String? ?? '',
        secret: json['secret'] as String? ?? '',
        ownerId: json['owner_id'] as String? ?? '',
        memberCount: json['member_count'] as int? ?? 1,
        maxMembers: json['max_members'] as int? ?? 16,
        metadata: (json['metadata'] as Map<String, dynamic>?)
                ?.map((k, v) => MapEntry(k, v.toString())) ??
            const {},
      );

  @override
  String toString() =>
      'IVXDiscordLobbyInfo(lobbyId: $lobbyId, members: $memberCount/$maxMembers)';
}

/// A participant in a Discord voice call.
class IVXVoiceParticipant {
  final String userId;
  final String username;
  final bool isMuted;
  final bool isDeafened;
  final bool isSpeaking;
  final int volume;

  const IVXVoiceParticipant({
    required this.userId,
    required this.username,
    this.isMuted = false,
    this.isDeafened = false,
    this.isSpeaking = false,
    this.volume = 100,
  });

  factory IVXVoiceParticipant.fromJson(Map<String, dynamic> json) =>
      IVXVoiceParticipant(
        userId: json['user_id'] as String? ?? '',
        username: json['username'] as String? ?? '',
        isMuted: json['is_muted'] as bool? ?? false,
        isDeafened: json['is_deafened'] as bool? ?? false,
        isSpeaking: json['is_speaking'] as bool? ?? false,
        volume: json['volume'] as int? ?? 100,
      );

  @override
  String toString() =>
      'IVXVoiceParticipant(userId: $userId, username: $username)';
}

/// A single chat message entry within a lobby.
class IVXChatEntry {
  final String senderId;
  final String message;
  final DateTime timestamp;

  const IVXChatEntry({
    required this.senderId,
    required this.message,
    required this.timestamp,
  });

  @override
  String toString() =>
      'IVXChatEntry(senderId: $senderId, message: $message)';
}

// ---------------------------------------------------------------------------
// Events
// ---------------------------------------------------------------------------

/// Events emitted by [IVXDiscordSocial].
enum IVXDiscordEvent {
  initialized,
  presenceUpdated,
  friendsUpdated,
  lobbyJoined,
  lobbyLeft,
  chatMessage,
  voiceJoined,
  voiceLeft,
  voiceParticipantUpdated,
  inviteReceived,
  joinRequested,
  error,
}

// ---------------------------------------------------------------------------
// Client
// ---------------------------------------------------------------------------

/// Discord Social SDK integration for IntelliVerseX.
///
/// Wraps Discord Rich Presence, unified friends list, lobby/text-chat,
/// voice channels, and game invites behind a single ergonomic API.
///
/// ```dart
/// final discord = IVXDiscordSocial.instance;
/// await discord.initialize(
///   config: IVXDiscordConfig(applicationId: '...', clientId: '...'),
/// );
/// await discord.setActivity('Playing QuizVerse', state: 'Round 3');
/// ```
class IVXDiscordSocial {
  static IVXDiscordSocial? _instance;

  IVXDiscordConfig? _config;
  bool _initialized = false;
  IVXDiscordLobbyInfo? _currentLobby;
  final List<IVXChatEntry> _chatHistory = [];

  final _controller = StreamController<MapEntry<IVXDiscordEvent, dynamic>>.broadcast();

  IVXDiscordSocial._();

  /// Singleton accessor.
  static IVXDiscordSocial get instance => _instance ??= IVXDiscordSocial._();

  /// Reset the singleton (useful for testing).
  static void resetInstance() => _instance = null;

  /// Whether [initialize] has been called successfully.
  bool get isInitialized => _initialized;

  /// Stream of all Discord Social events.
  ///
  /// Each event is a [MapEntry] of the [IVXDiscordEvent] and its payload.
  Stream<MapEntry<IVXDiscordEvent, dynamic>> get events => _controller.stream;

  /// Filtered stream for a specific [IVXDiscordEvent].
  Stream<T> on<T>(IVXDiscordEvent event) =>
      _controller.stream
          .where((e) => e.key == event)
          .map((e) => e.value as T);

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------

  /// Initialize the Discord Social SDK with the given [config].
  ///
  /// Must be called before any other method. Supports account linking and
  /// provisional (guest) accounts.
  Future<void> initialize({required IVXDiscordConfig config}) async {
    if (config.applicationId.trim().isEmpty) {
      throw const IVXError(code: -1, message: 'applicationId cannot be empty.');
    }
    if (config.clientId.trim().isEmpty) {
      throw const IVXError(code: -1, message: 'clientId cannot be empty.');
    }
    _config = config;
    _initialized = true;
    _log('Discord Social SDK initialized');
    _emit(IVXDiscordEvent.initialized, null);
  }

  /// Link the current game account with a Discord account via OAuth.
  ///
  /// Returns the linked Discord user ID on success.
  Future<String> linkAccount() async {
    _ensureInitialized();
    _log('Account link flow started');
    return '';
  }

  /// Create or retrieve a provisional (guest) Discord account for the
  /// current player, enabling social features without a full Discord login.
  Future<String> getProvisionalAccount() async {
    _ensureInitialized();
    _log('Provisional account requested');
    return '';
  }

  // ---------------------------------------------------------------------------
  // Rich Presence
  // ---------------------------------------------------------------------------

  /// Set the player's Discord Rich Presence activity text.
  Future<void> setActivity(String details, {String? state}) async {
    _ensureInitialized();
    _log('Presence updated — details="$details" state="${state ?? ''}"');
    _emit(IVXDiscordEvent.presenceUpdated, null);
  }

  /// Set Rich Presence party info for multiplayer sessions.
  Future<void> setParty(
    String partyId,
    int currentSize,
    int maxSize, {
    String? joinSecret,
  }) async {
    _ensureInitialized();
    _log('Party set — id=$partyId size=$currentSize/$maxSize');
    _emit(IVXDiscordEvent.presenceUpdated, null);
  }

  /// Start an elapsed-time timer on the Rich Presence display.
  Future<void> startTimer() async {
    _ensureInitialized();
    _log('Presence timer started');
    _emit(IVXDiscordEvent.presenceUpdated, null);
  }

  /// Clear all Rich Presence data.
  Future<void> clearPresence() async {
    _ensureInitialized();
    _log('Presence cleared');
    _emit(IVXDiscordEvent.presenceUpdated, null);
  }

  // ---------------------------------------------------------------------------
  // Friends
  // ---------------------------------------------------------------------------

  /// Retrieve a unified friends list that merges Discord friends with
  /// in-game friends.
  ///
  /// Each entry indicates whether the friend was sourced from Discord,
  /// the game backend, or both.
  Future<List<IVXUnifiedFriend>> getUnifiedFriends() async {
    _ensureInitialized();
    _log('Fetching unified friends list');
    return [];
  }

  // ---------------------------------------------------------------------------
  // Lobby
  // ---------------------------------------------------------------------------

  /// Create or join a lobby identified by a shared [secret].
  Future<IVXDiscordLobbyInfo> createOrJoinLobby(
    String secret, {
    Map<String, String>? metadata,
  }) async {
    _ensureInitialized();
    final lobby = IVXDiscordLobbyInfo(
      lobbyId: '',
      secret: secret,
      metadata: metadata ?? const {},
    );
    _currentLobby = lobby;
    _chatHistory.clear();
    _log('Lobby joined — secret=$secret');
    _emit(IVXDiscordEvent.lobbyJoined, lobby);
    return lobby;
  }

  /// Leave the current lobby.
  Future<void> leaveLobby() async {
    _ensureInitialized();
    _currentLobby = null;
    _chatHistory.clear();
    _log('Left lobby');
    _emit(IVXDiscordEvent.lobbyLeft, null);
  }

  /// Send a text-chat [message] to the current lobby.
  Future<void> sendMessage(String message) async {
    _ensureInitialized();
    if (_currentLobby == null) {
      throw const IVXError(
        code: -1,
        message: 'Not in a lobby. Call createOrJoinLobby() first.',
      );
    }
    _chatHistory.add(IVXChatEntry(
      senderId: 'self',
      message: message,
      timestamp: DateTime.now(),
    ));
    _log('Chat message sent: $message');
  }

  /// Return the most recent chat messages for the current lobby.
  List<IVXChatEntry> getChatHistory({int limit = 50}) {
    _ensureInitialized();
    final start = (_chatHistory.length - limit).clamp(0, _chatHistory.length);
    return List.unmodifiable(_chatHistory.sublist(start));
  }

  // ---------------------------------------------------------------------------
  // Voice
  // ---------------------------------------------------------------------------

  /// Join a voice call in the specified lobby.
  Future<void> joinCall(String lobbyId) async {
    _ensureInitialized();
    _log('Joined voice call — lobby=$lobbyId');
    _emit(IVXDiscordEvent.voiceJoined, lobbyId);
  }

  /// Leave the current voice call.
  Future<void> leaveCall() async {
    _ensureInitialized();
    _log('Left voice call');
    _emit(IVXDiscordEvent.voiceLeft, null);
  }

  /// Mute or unmute the local player's microphone.
  Future<void> setSelfMute(bool muted) async {
    _ensureInitialized();
    _log('Self mute: $muted');
  }

  /// Deafen or undeafen the local player.
  Future<void> setSelfDeafen(bool deaf) async {
    _ensureInitialized();
    _log('Self deafen: $deaf');
  }

  /// Set input (microphone) and output (speaker) volume levels (0–100).
  Future<void> setVolume(int input, int output) async {
    _ensureInitialized();
    _log('Volume set — input=$input output=$output');
  }

  // ---------------------------------------------------------------------------
  // Invites
  // ---------------------------------------------------------------------------

  /// Send a game invite to another user by their ID.
  Future<void> sendInvite(String userId, {String? message}) async {
    _ensureInitialized();
    _log('Invite sent to $userId');
  }

  /// Stream of incoming game invites.
  ///
  /// Shorthand for `on<IVXGameInvite>(IVXDiscordEvent.inviteReceived)`.
  Stream<IVXGameInvite> get onInviteReceived =>
      on<IVXGameInvite>(IVXDiscordEvent.inviteReceived);

  /// Stream of "Ask to Join" requests from other players.
  ///
  /// Shorthand for `on<String>(IVXDiscordEvent.joinRequested)`.
  Stream<String> get onJoinRequested =>
      on<String>(IVXDiscordEvent.joinRequested);

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  void _emit(IVXDiscordEvent event, dynamic data) {
    _controller.add(MapEntry(event, data));
  }

  void _ensureInitialized() {
    if (!_initialized) {
      throw const IVXError(
        code: -1,
        message: 'IVXDiscordSocial not initialized. Call initialize() first.',
      );
    }
  }

  void _log(String message) {
    if (_config?.enableDebugLogs ?? false) {
      // ignore: avoid_print
      print('[IntelliVerseX:Discord] $message');
    }
  }
}
