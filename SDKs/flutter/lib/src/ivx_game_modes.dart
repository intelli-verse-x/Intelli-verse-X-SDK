import 'dart:async';

import 'types.dart';

// ---------------------------------------------------------------------------
// Enums
// ---------------------------------------------------------------------------

/// Available game modes.
enum IVXGameMode {
  /// Classic single-player quiz.
  solo,

  /// Local couch co-op (shared device).
  localMultiplayer,

  /// Online real-time multiplayer.
  onlineMultiplayer,

  /// Cooperative team play.
  coop,

  /// Tournament bracket mode.
  tournament,

  /// Practice / training.
  practice,
}

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

/// A slot occupied (or reserved) by a player in the current match.
class IVXPlayerSlot {
  final int index;
  final String name;
  final bool isLocal;
  bool isReady;

  IVXPlayerSlot({
    required this.index,
    required this.name,
    this.isLocal = true,
    this.isReady = false,
  });

  @override
  String toString() =>
      'IVXPlayerSlot(index: $index, name: $name, ready: $isReady)';
}

/// Configuration snapshot for a match about to start.
class IVXMatchConfig {
  final IVXGameMode mode;
  final int maxPlayers;
  final List<IVXPlayerSlot> players;

  const IVXMatchConfig({
    required this.mode,
    required this.maxPlayers,
    required this.players,
  });

  @override
  String toString() =>
      'IVXMatchConfig(mode: ${mode.name}, players: ${players.length}/$maxPlayers)';
}

/// Describes a joinable lobby room.
class IVXRoomInfo {
  final String roomId;
  final String name;
  final int playerCount;
  final int maxPlayers;
  final IVXGameMode mode;
  final Map<String, dynamic> metadata;

  const IVXRoomInfo({
    required this.roomId,
    required this.name,
    required this.playerCount,
    required this.maxPlayers,
    required this.mode,
    this.metadata = const {},
  });

  @override
  String toString() =>
      'IVXRoomInfo(roomId: $roomId, name: $name, $playerCount/$maxPlayers)';
}

/// Summary produced when a match concludes.
class IVXMatchResult {
  final String matchId;
  final IVXGameMode mode;
  final List<IVXPlayerSlot> players;
  final Map<String, dynamic> scores;
  final Duration duration;

  const IVXMatchResult({
    required this.matchId,
    required this.mode,
    required this.players,
    this.scores = const {},
    this.duration = Duration.zero,
  });

  @override
  String toString() =>
      'IVXMatchResult(matchId: $matchId, mode: ${mode.name})';
}

// ---------------------------------------------------------------------------
// Manager
// ---------------------------------------------------------------------------

/// Manages game-mode selection, player slots, lobby rooms, and matchmaking.
///
/// ```dart
/// final modes = IVXGameModeManager.instance;
/// modes.selectMode(IVXGameMode.onlineMultiplayer, maxPlayers: 4);
/// modes.addPlayer('Alice');
/// modes.addPlayer('Bob', isLocal: false);
/// if (modes.canStartMatch) modes.startMatch();
/// ```
class IVXGameModeManager {
  static IVXGameModeManager? _instance;

  IVXGameMode _currentMode = IVXGameMode.solo;
  int _maxPlayers = 4;
  final List<IVXPlayerSlot> _players = [];
  bool _matchActive = false;
  String? _currentRoomId;
  String? _activeMatchId;
  int _matchCounter = 0;

  final StreamController<IVXGameMode> _modeController =
      StreamController<IVXGameMode>.broadcast();
  final StreamController<List<IVXPlayerSlot>> _playerController =
      StreamController<List<IVXPlayerSlot>>.broadcast();
  final StreamController<bool> _matchStateController =
      StreamController<bool>.broadcast();
  final StreamController<IVXRoomInfo> _roomController =
      StreamController<IVXRoomInfo>.broadcast();

  IVXGameModeManager._();

  /// Singleton accessor.
  static IVXGameModeManager get instance =>
      _instance ??= IVXGameModeManager._();

  /// Reset the singleton (useful for testing).
  static void resetInstance() {
    _instance?._dispose();
    _instance = null;
  }

  // ---------------------------------------------------------------------------
  // Streams
  // ---------------------------------------------------------------------------

  /// Fires whenever the active game mode changes.
  Stream<IVXGameMode> get onModeChanged => _modeController.stream;

  /// Fires whenever the player list changes.
  Stream<List<IVXPlayerSlot>> get onPlayersChanged => _playerController.stream;

  /// Fires `true` when a match starts, `false` when it ends.
  Stream<bool> get onMatchStateChanged => _matchStateController.stream;

  /// Fires when a room is created or joined.
  Stream<IVXRoomInfo> get onRoomEvent => _roomController.stream;

  // ---------------------------------------------------------------------------
  // State accessors
  // ---------------------------------------------------------------------------

  /// The currently selected game mode.
  IVXGameMode get currentMode => _currentMode;

  /// Unmodifiable view of current player slots.
  List<IVXPlayerSlot> get players => List.unmodifiable(_players);

  /// Whether a match is in progress.
  bool get isMatchActive => _matchActive;

  /// The maximum number of players for the current configuration.
  int get maxPlayers => _maxPlayers;

  /// `true` when at least one player is present and all are ready.
  bool get canStartMatch =>
      !_matchActive &&
      _players.isNotEmpty &&
      _players.every((p) => p.isReady);

  // ---------------------------------------------------------------------------
  // Mode selection
  // ---------------------------------------------------------------------------

  /// Choose a [mode] and optionally override [maxPlayers].
  void selectMode(IVXGameMode mode, {int maxPlayers = 4}) {
    if (_matchActive) {
      throw const IVXError(
        code: -1,
        message: 'Cannot change mode while a match is active.',
      );
    }
    _currentMode = mode;
    _maxPlayers = maxPlayers;
    _log('Mode set to ${mode.name} (max $maxPlayers)');
    _modeController.add(mode);
  }

  // ---------------------------------------------------------------------------
  // Player management
  // ---------------------------------------------------------------------------

  /// Add a player by [name] and return the assigned slot.
  IVXPlayerSlot addPlayer(String name, {bool isLocal = true}) {
    if (_players.length >= _maxPlayers) {
      throw IVXError(
        code: -1,
        message: 'Lobby full ($maxPlayers max).',
      );
    }
    final slot = IVXPlayerSlot(
      index: _players.length,
      name: name,
      isLocal: isLocal,
    );
    _players.add(slot);
    _log('Player added: $name (slot ${slot.index})');
    _playerController.add(players);
    return slot;
  }

  /// Remove the player at [slotIndex].
  void removePlayer(int slotIndex) {
    if (slotIndex < 0 || slotIndex >= _players.length) return;
    final removed = _players.removeAt(slotIndex);
    // Re-index remaining slots.
    for (var i = 0; i < _players.length; i++) {
      _players[i] = IVXPlayerSlot(
        index: i,
        name: _players[i].name,
        isLocal: _players[i].isLocal,
        isReady: _players[i].isReady,
      );
    }
    _log('Player removed: ${removed.name}');
    _playerController.add(players);
  }

  /// Mark [slot] as ready (or not).
  void setPlayerReady(int slot, bool ready) {
    if (slot < 0 || slot >= _players.length) return;
    _players[slot].isReady = ready;
    _playerController.add(players);
  }

  // ---------------------------------------------------------------------------
  // Match lifecycle
  // ---------------------------------------------------------------------------

  /// Start a match with the current players and mode.
  ///
  /// Throws if [canStartMatch] is `false`.
  void startMatch() {
    if (!canStartMatch) {
      throw const IVXError(
        code: -1,
        message: 'Cannot start: ensure at least one ready player and no '
            'active match.',
      );
    }
    _matchCounter++;
    _activeMatchId = 'match_${_matchCounter}_'
        '${DateTime.now().millisecondsSinceEpoch}';
    _matchActive = true;
    _log('Match started: $_activeMatchId (${_currentMode.name})');
    _matchStateController.add(true);
  }

  /// End the current match and return a [IVXMatchResult] summary.
  IVXMatchResult endMatch() {
    if (!_matchActive) {
      throw const IVXError(code: -1, message: 'No active match to end.');
    }
    final result = IVXMatchResult(
      matchId: _activeMatchId ?? 'unknown',
      mode: _currentMode,
      players: List.of(_players),
    );
    _matchActive = false;
    _activeMatchId = null;
    _log('Match ended');
    _matchStateController.add(false);
    return result;
  }

  /// Reset all state — mode, players, match — to defaults.
  void reset() {
    _matchActive = false;
    _activeMatchId = null;
    _currentRoomId = null;
    _players.clear();
    _currentMode = IVXGameMode.solo;
    _maxPlayers = 4;
    _log('Game mode manager reset');
    _modeController.add(_currentMode);
    _playerController.add(players);
    _matchStateController.add(false);
  }

  // ---------------------------------------------------------------------------
  // Lobby
  // ---------------------------------------------------------------------------

  /// Create a new lobby room with a display [name].
  Future<IVXRoomInfo> createRoom(String name, {IVXGameMode? mode}) async {
    final roomMode = mode ?? _currentMode;
    final room = IVXRoomInfo(
      roomId: 'room_${DateTime.now().millisecondsSinceEpoch}',
      name: name,
      playerCount: 1,
      maxPlayers: _maxPlayers,
      mode: roomMode,
    );
    _currentRoomId = room.roomId;
    _log('Room created: ${room.roomId}');
    _roomController.add(room);
    return room;
  }

  /// Join an existing room by [roomId].
  Future<IVXRoomInfo> joinRoom(String roomId) async {
    _currentRoomId = roomId;
    final room = IVXRoomInfo(
      roomId: roomId,
      name: roomId,
      playerCount: 0,
      maxPlayers: _maxPlayers,
      mode: _currentMode,
    );
    _log('Joined room: $roomId');
    _roomController.add(room);
    return room;
  }

  /// List available rooms.
  ///
  /// In a real implementation this would query a matchmaker service; the stub
  /// returns an empty list so consumers can code against the API now.
  Future<List<IVXRoomInfo>> listRooms() async => const [];

  /// Leave the currently-joined room.
  Future<void> leaveRoom() async {
    if (_currentRoomId != null) {
      _log('Left room: $_currentRoomId');
      _currentRoomId = null;
    }
  }

  // ---------------------------------------------------------------------------
  // Matchmaking
  // ---------------------------------------------------------------------------

  /// Request the matchmaker to find an appropriate match.
  ///
  /// Returns a [IVXRoomInfo] when a match is found.
  Future<IVXRoomInfo> findMatch({IVXGameMode? mode}) async {
    final searchMode = mode ?? _currentMode;
    _log('Searching for ${searchMode.name} match…');
    return IVXRoomInfo(
      roomId: 'mm_${DateTime.now().millisecondsSinceEpoch}',
      name: 'Matchmade Room',
      playerCount: 1,
      maxPlayers: _maxPlayers,
      mode: searchMode,
    );
  }

  /// Cancel an in-progress matchmaking search.
  Future<void> cancelSearch() async {
    _log('Matchmaking search cancelled');
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  void _dispose() {
    _modeController.close();
    _playerController.close();
    _matchStateController.close();
    _roomController.close();
  }

  void _log(String message) {
    // ignore: avoid_print
    print('[IntelliVerseX:GameModes] $message');
  }
}
