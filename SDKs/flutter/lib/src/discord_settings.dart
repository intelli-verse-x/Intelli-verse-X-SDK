// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/// Discord Social Settings — notification preferences, privacy, DND mode.
///
/// Stub: API shape matches Unity IVXDiscordSettings for zero-code-change upgrade.
class IVXDiscordSettings {
  bool notificationsEnabled = true;
  bool friendRequestsEnabled = true;
  bool doNotDisturb = false;
  bool showOnlineStatus = true;
  bool allowDirectMessages = true;

  void enableDoNotDisturb() => doNotDisturb = true;
  void disableDoNotDisturb() => doNotDisturb = false;

  void resetToDefaults() {
    notificationsEnabled = true;
    friendRequestsEnabled = true;
    doNotDisturb = false;
    showOnlineStatus = true;
    allowDirectMessages = true;
  }
}
