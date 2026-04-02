// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/// Discord Social SDK — Linked Channels: bridge in-game chat to Discord channels.
class IVXLinkedChannel {
  final String channelId;
  final String guildId;
  final String name;
  final String lobbyId;
  final int linkedAt;

  IVXLinkedChannel({
    required this.channelId,
    required this.guildId,
    required this.name,
    required this.lobbyId,
    required this.linkedAt,
  });
}

class IVXDiscordLinkedChannels {
  Future<IVXLinkedChannel> linkChannel(String lobbyId, String channelId) async {
    throw UnimplementedError('Requires Discord Social SDK native integration.');
  }

  Future<void> unlinkChannel(String lobbyId, String channelId) async {
    throw UnimplementedError('Requires Discord Social SDK native integration.');
  }

  Future<List<IVXLinkedChannel>> getLinkedChannels(String lobbyId) async {
    throw UnimplementedError('Requires Discord Social SDK native integration.');
  }
}
