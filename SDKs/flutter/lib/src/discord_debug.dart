// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/// Verbosity for Discord Social SDK debug output.
enum IVXDiscordLogLevel {
  none,
  error,
  warn,
  info,
  debug,
}

/// One log line from the Discord Social SDK or bridge.
class IVXDiscordLogEntry {
  final IVXDiscordLogLevel level;
  final String message;
  final int timestamp;
  final String source;

  const IVXDiscordLogEntry({
    required this.level,
    required this.message,
    required this.timestamp,
    required this.source,
  });
}

/// Invoked for each recorded log entry (when level is enabled).
typedef IVXDiscordLogCallback = void Function(IVXDiscordLogEntry entry);

/// Debug logging for Discord Social SDK — level, callbacks, and rolling history.
class IVXDiscordDebug {
  IVXDiscordDebug._();
  static final IVXDiscordDebug instance = IVXDiscordDebug._();

  static const int _maxHistory = 500;

  IVXDiscordLogLevel _logLevel = IVXDiscordLogLevel.info;
  final List<IVXDiscordLogCallback> _callbacks = [];
  final List<IVXDiscordLogEntry> _history = [];

  void setLogLevel(IVXDiscordLogLevel level) {
    _logLevel = level;
  }

  IVXDiscordLogLevel getLogLevel() => _logLevel;

  void addLogCallback(IVXDiscordLogCallback callback) {
    if (!_callbacks.contains(callback)) {
      _callbacks.add(callback);
    }
  }

  void removeLogCallback(IVXDiscordLogCallback callback) {
    _callbacks.remove(callback);
  }

  List<IVXDiscordLogEntry> getLogHistory() =>
      List<IVXDiscordLogEntry>.unmodifiable(_history);

  void clearLogHistory() {
    _history.clear();
  }

  /// Append a log line (e.g. from native Discord Social SDK). Respects [getLogLevel], updates [getLogHistory], notifies callbacks.
  void record(IVXDiscordLogEntry entry) {
    if (!_isLevelEnabled(entry.level)) {
      return;
    }
    _history.add(entry);
    while (_history.length > _maxHistory) {
      _history.removeAt(0);
    }
    for (final cb in List<IVXDiscordLogCallback>.from(_callbacks)) {
      cb(entry);
    }
  }

  bool _isLevelEnabled(IVXDiscordLogLevel level) {
    if (_logLevel == IVXDiscordLogLevel.none ||
        level == IVXDiscordLogLevel.none) {
      return false;
    }
    return level.index <= _logLevel.index;
  }
}
