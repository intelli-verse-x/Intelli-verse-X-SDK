// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

import 'dart:convert';
import 'package:nakama/nakama.dart';

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

class IVXLobbyPlayer {
  final String userId;
  final String username;
  final Map<String, dynamic>? metadata;

  IVXLobbyPlayer({required this.userId, required this.username, this.metadata});

  factory IVXLobbyPlayer.fromJson(Map<String, dynamic> json) {
    return IVXLobbyPlayer(
      userId: json['userId'] as String? ?? json['user_id'] as String? ?? '',
      username: json['username'] as String? ?? '',
      metadata: json['metadata'] as Map<String, dynamic>?,
    );
  }
}

class IVXLobby {
  final String lobbyId;
  final String name;
  final String hostUserId;
  final List<IVXLobbyPlayer> players;
  final int maxPlayers;
  final bool isPublic;
  final Map<String, dynamic> metadata;

  IVXLobby({
    required this.lobbyId,
    required this.name,
    required this.hostUserId,
    required this.players,
    required this.maxPlayers,
    required this.isPublic,
    required this.metadata,
  });

  factory IVXLobby.fromJson(Map<String, dynamic> json) {
    final rawPlayers = json['players'] as List<dynamic>? ?? [];
    return IVXLobby(
      lobbyId: json['lobbyId'] as String? ?? json['lobby_id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      hostUserId: json['hostUserId'] as String? ?? json['host_user_id'] as String? ?? '',
      players: rawPlayers
          .map((p) => IVXLobbyPlayer.fromJson(p as Map<String, dynamic>))
          .toList(),
      maxPlayers: (json['maxPlayers'] ?? json['max_players'] ?? 0) as int,
      isPublic: (json['isPublic'] ?? json['is_public'] ?? false) as bool,
      metadata: json['metadata'] as Map<String, dynamic>? ?? {},
    );
  }
}

class IVXMatchmakingTicket {
  final String ticketId;
  final String status;
  final String matchId;

  IVXMatchmakingTicket({
    required this.ticketId,
    required this.status,
    required this.matchId,
  });

  factory IVXMatchmakingTicket.fromJson(Map<String, dynamic> json) {
    return IVXMatchmakingTicket(
      ticketId: json['ticketId'] as String? ?? json['ticket_id'] as String? ?? '',
      status: json['status'] as String? ?? '',
      matchId: json['matchId'] as String? ?? json['match_id'] as String? ?? '',
    );
  }
}

// ---------------------------------------------------------------------------
// IVXMultiplayer
// ---------------------------------------------------------------------------

class IVXMultiplayer {
  IVXMultiplayer._();
  static final IVXMultiplayer instance = IVXMultiplayer._();

  Client? _client;
  Session? _session;

  void initialize(Client client, Session session) {
    _client = client;
    _session = session;
  }

  Future<Map<String, dynamic>> _rpc(String rpcId, [Map<String, dynamic>? payload]) async {
    if (_client == null || _session == null) {
      throw StateError('[IVXMultiplayer] Not initialized — call initialize(client, session) first.');
    }
    final body = payload != null ? jsonEncode(payload) : '{}';
    final result = await _client!.rpc(session: _session!, id: rpcId, payload: body);
    if (result.payload == null || result.payload!.isEmpty) return {};
    return jsonDecode(result.payload!) as Map<String, dynamic>;
  }

  // -----------------------------------------------------------------------
  // Lobby
  // -----------------------------------------------------------------------

  Future<IVXLobby> createLobby(String name, int maxPlayers, bool isPublic) async {
    final data = await _rpc('create_lobby', {
      'name': name,
      'max_players': maxPlayers,
      'is_public': isPublic,
    });
    return IVXLobby.fromJson(data);
  }

  Future<IVXLobby> joinLobby(String lobbyId) async {
    final data = await _rpc('join_lobby', {'lobby_id': lobbyId});
    return IVXLobby.fromJson(data);
  }

  Future<void> leaveLobby(String lobbyId) async {
    await _rpc('leave_lobby', {'lobby_id': lobbyId});
  }

  Future<List<IVXLobby>> listLobbies() async {
    final data = await _rpc('list_lobbies');
    final raw = data['lobbies'] as List<dynamic>? ?? [];
    return raw.map((l) => IVXLobby.fromJson(l as Map<String, dynamic>)).toList();
  }

  // -----------------------------------------------------------------------
  // Matchmaking
  // -----------------------------------------------------------------------

  Future<IVXMatchmakingTicket> startMatchmaking(int minPlayers, int maxPlayers, [int? rankRange]) async {
    final payload = <String, dynamic>{
      'min_players': minPlayers,
      'max_players': maxPlayers,
    };
    if (rankRange != null) payload['rank_range'] = rankRange;
    final data = await _rpc('start_matchmaking', payload);
    return IVXMatchmakingTicket.fromJson(data);
  }

  Future<void> cancelMatchmaking(String ticketId) async {
    await _rpc('cancel_matchmaking', {'ticket_id': ticketId});
  }
}
