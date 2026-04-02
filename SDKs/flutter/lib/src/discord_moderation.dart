// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

enum IVXModerationAction { show, hide, blur, replace }

/// Moderation decision for a Discord message.
class IVXModerationDecision {
  final String messageId;
  final IVXModerationAction action;
  final String reason;
  final String replacement;
  final String severity;

  const IVXModerationDecision({
    required this.messageId,
    required this.action,
    required this.reason,
    required this.replacement,
    required this.severity,
  });
}

/// Discord moderation & reporting — stub matching Unity [IVXDiscordModeration].
class IVXDiscordModeration {
  IVXDiscordModeration._();
  static final IVXDiscordModeration instance = IVXDiscordModeration._();

  bool autoModerateEnabled = true;

  void enableAutoModeration(bool enable) {
    throw UnimplementedError('IVXDiscordModeration.enableAutoModeration');
  }

  void processModerationMetadata(
    String messageId,
    Map<String, String> metadata,
  ) {
    throw UnimplementedError('IVXDiscordModeration.processModerationMetadata');
  }

  static IVXModerationDecision getModerationAction(
    Map<String, String>? metadata,
  ) {
    throw UnimplementedError('IVXDiscordModeration.getModerationAction');
  }

  void startVoiceModerationCapture(String lobbyId) {
    throw UnimplementedError('IVXDiscordModeration.startVoiceModerationCapture');
  }

  void stopVoiceModerationCapture() {
    throw UnimplementedError('IVXDiscordModeration.stopVoiceModerationCapture');
  }

  Future<bool> reportUser(String userId, String reason) async {
    throw UnimplementedError('IVXDiscordModeration.reportUser');
  }
}
