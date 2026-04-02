// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

class IVXAINPCProfile {
  final String npcId;
  final int maxTurns;

  const IVXAINPCProfile({required this.npcId, this.maxTurns = 0});
}

class IVXAINPCDialogSession {
  final String sessionId;
  final String npcId;
  final String playerId;

  const IVXAINPCDialogSession({
    required this.sessionId,
    required this.npcId,
    required this.playerId,
  });
}

/// NPC dialog manager — stub matching Unity [IVXAINPCDialogManager].
class IVXAINPCDialogManager {
  IVXAINPCDialogManager._();
  static final IVXAINPCDialogManager instance = IVXAINPCDialogManager._();

  bool get isInitialized => false;

  void initialize(Object? config) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void setAuthToken(String? token) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void registerNPC(IVXAINPCProfile profile) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  void unregisterNPC(String npcId) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<IVXAINPCDialogSession?> startDialog(
    String npcId,
    String playerId, [
    String? playerContext,
  ]) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<String?> sendMessage(String sessionId, String message) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  Future<void> endDialog(String sessionId) async {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  IVXAINPCDialogSession? getSession(String sessionId) {
    throw UnimplementedError('Not yet implemented — stub only');
  }

  List<IVXAINPCDialogSession> getSessionsForNPC(String npcId) {
    throw UnimplementedError('Not yet implemented — stub only');
  }
}
