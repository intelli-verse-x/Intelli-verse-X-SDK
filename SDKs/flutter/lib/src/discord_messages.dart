// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/// Discord DM line (Unity [IVXDirectMessage]).
class IVXDirectMessage {
  final String messageId;
  final String authorId;
  final String content;
  final int timestamp;

  const IVXDirectMessage({
    required this.messageId,
    required this.authorId,
    required this.content,
    required this.timestamp,
  });
}

/// DM conversation summary (Unity [IVXDMSummary]).
class IVXDMSummary {
  final String userId;
  final String displayName;
  final String lastMessageId;
  final int lastMessageTimestamp;

  const IVXDMSummary({
    required this.userId,
    required this.displayName,
    required this.lastMessageId,
    required this.lastMessageTimestamp,
  });
}

/// Direct messages API — stub matching Unity [IVXDiscordMessages].
class IVXDiscordMessages {
  IVXDiscordMessages._();
  static final IVXDiscordMessages instance = IVXDiscordMessages._();

  bool get isShowingChat => false;

  Future<String> sendDM(String recipientId, String message) async {
    throw UnimplementedError('IVXDiscordMessages.sendDM');
  }

  Future<void> editDM(
    String recipientId,
    String messageId,
    String newContent,
  ) async {
    throw UnimplementedError('IVXDiscordMessages.editDM');
  }

  Future<List<IVXDirectMessage>> getDMHistory(
    String recipientId, {
    int limit = 50,
  }) async {
    throw UnimplementedError('IVXDiscordMessages.getDMHistory');
  }

  Future<List<IVXDMSummary>> getDMSummaries() async {
    throw UnimplementedError('IVXDiscordMessages.getDMSummaries');
  }

  void setShowingChat(bool showing) {
    throw UnimplementedError('IVXDiscordMessages.setShowingChat');
  }

  void openMessageInDiscord(String messageId) {
    throw UnimplementedError('IVXDiscordMessages.openMessageInDiscord');
  }

  void openDMSettingsInDiscord() {
    throw UnimplementedError('IVXDiscordMessages.openDMSettingsInDiscord');
  }
}
