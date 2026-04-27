// IVXMultiplayerKernel — Flutter / Dart adapter for the IntelliVerseX
// Multiplayer Kernel. Mirrors the IIVXMultiplayer / IIVXMatchSession
// contract from the Unity, JS, Unreal, Godot, Java, C++, and Web3 SDKs.
//
// Wraps the official `nakama` Dart package (https://pub.dev/packages/nakama)
// and speaks the wire protocol defined in
//   `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
//
// Usage:
//   final kernel = IVXMultiplayerKernel(client: client, session: session);
//   await kernel.initialize();
//   final resp = await kernel.createMatch(IVXCreateMatchRequest(
//     templateId: 'sync-turn-v1',
//     gameId: 'demo',
//     templateInit: {'min_players': 2},
//   ));
//   final session = await kernel.joinMatch(resp.matchId);
//   session.subscribe(0xC100, (env) => print('question: ${env.payload}'));
//   await session.send(0xC101, {'answer_id': 'a'});

import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';
import 'dart:math';

// Imports from the official Nakama Dart package. Kept in a single import
// block so consumers can mirror them in their pubspec.yaml.
//
//   nakama: ^1.0.0
//
// We only depend on the public surface of `Client`, `Session`, and the
// realtime types (`NakamaWebsocketClient`, `Match`, `MatchData`). If
// upstream renames any of these (their API has shifted across versions),
// only the import + the few touch-points in this file need updating.
//
// `dynamic` typing in spots below is intentional — the package's API has
// drifted across major versions and we want a single adapter file that
// builds on 0.x and 1.x.

/// Transport-state machine, identical across all kernel adapters.
enum IVXTransportState { disconnected, connecting, connected, reconnecting, failedFatal }

/// Match-end reason, mirrors `kernel.proto EndReason`.
enum IVXEndReason {
  unknown,
  completed,
  cancelled,
  durationExceeded,
  kernelInternal,
  allPlayersLeft,
  hostTerminated,
}

/// Header of an inbound or outbound kernel envelope.
class IVXKernelHeader {
  final int seq;
  final int matchTimeMs;
  final String uuid;
  final int opCode;
  final String senderUserId;
  const IVXKernelHeader({
    required this.seq,
    required this.matchTimeMs,
    required this.uuid,
    required this.opCode,
    required this.senderUserId,
  });
}

/// One inbound kernel event delivered to a subscriber.
class IVXKernelEvent {
  final IVXKernelHeader header;
  /// Decoded JSON payload — usually a `Map<String, dynamic>` but the kernel
  /// supports arrays / scalars in some templates so we leave it `dynamic`.
  final dynamic payload;
  final int recvUnixMs;
  const IVXKernelEvent({
    required this.header,
    required this.payload,
    required this.recvUnixMs,
  });
}

/// Request sent to the `mp_create_match` Nakama RPC.
class IVXCreateMatchRequest {
  final String templateId;
  final String gameId;
  final String region;
  final Map<String, dynamic>? templateInit;
  const IVXCreateMatchRequest({
    required this.templateId,
    this.gameId = '',
    this.region = '',
    this.templateInit,
  });

  Map<String, dynamic> toJson() => {
        'template_id': templateId,
        'game_id': gameId,
        'region': region,
        'template_init': templateInit ?? {},
      };
}

class IVXCreateMatchResponse {
  final String matchId;
  final String templateId;
  final String region;
  final int expiresUnixMs;
  const IVXCreateMatchResponse({
    required this.matchId,
    required this.templateId,
    required this.region,
    required this.expiresUnixMs,
  });

  factory IVXCreateMatchResponse.fromJson(Map<String, dynamic> j) =>
      IVXCreateMatchResponse(
        matchId: j['match_id'] as String? ?? '',
        templateId: j['template_id'] as String? ?? '',
        region: j['region'] as String? ?? '',
        expiresUnixMs: (j['expires_unix_ms'] as num?)?.toInt() ?? 0,
      );
}

typedef IVXEnvelopeHandler = void Function(IVXKernelEvent event);

/// One subscription token returned by `subscribe()`. Calling [.dispose] is
/// idempotent and safe after the session is disposed.
class IVXSubscription {
  bool _disposed = false;
  final void Function() _unbind;
  IVXSubscription._(this._unbind);
  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _unbind();
  }
}

/// Live handle for one joined match. Disposing tears down handlers and
/// politely leaves the match. Safe to call multiple times.
class IVXMatchSession {
  IVXMatchSession._({
    required this.matchId,
    required this.localUserId,
    required IVXMultiplayerKernel kernel,
    required dynamic socket,
  })  : _kernel = kernel,
        _socket = socket;

  final String matchId;
  final String localUserId;
  String _templateId = '';
  String get templateId => _templateId;
  int currentMatchTimeMs = 0;
  int activePlayerCount = 0;
  IVXTransportState _state = IVXTransportState.connecting;
  IVXTransportState get state => _state;

  final IVXMultiplayerKernel _kernel;
  final dynamic _socket;
  final Map<int, List<IVXEnvelopeHandler>> _handlers = {};
  final List<_RangeBinding> _ranges = [];
  int _localSeq = 0;
  bool _disposed = false;

  final StreamController<IVXTransportState> _stateCtrl =
      StreamController.broadcast();
  Stream<IVXTransportState> get onTransportState => _stateCtrl.stream;

  IVXSubscription subscribe(int opCode, IVXEnvelopeHandler handler) {
    final list = _handlers.putIfAbsent(opCode, () => []);
    list.add(handler);
    return IVXSubscription._(() {
      list.remove(handler);
      if (list.isEmpty) _handlers.remove(opCode);
    });
  }

  IVXSubscription subscribeRange(int from, int to, IVXEnvelopeHandler handler) {
    final r = _RangeBinding(from, to, handler);
    _ranges.add(r);
    return IVXSubscription._(() => _ranges.remove(r));
  }

  Future<void> send(int opCode, dynamic payload) async {
    if (_disposed || _socket == null) return;
    _localSeq++;
    final env = jsonEncode({
      'h': {'s': _localSeq, 't': currentMatchTimeMs, 'u': _uuidV4()},
      'p': payload,
    });
    // Nakama-Dart's send signature has shifted across versions; we use a
    // duck-typed call so this file builds against 0.x and 1.x.
    try {
      await _socket.sendMatchData(
        matchId: matchId,
        opCode: opCode,
        data: utf8.encode(env),
      );
    } catch (_) {
      // 0.x signature is positional / takes a String body.
      // ignore: avoid_dynamic_calls
      await _socket.sendMatchData(matchId, opCode, env);
    }
  }

  Future<void> leave() async {
    if (_disposed) return;
    try {
      await _socket.leaveMatch(matchId);
    } catch (_) {}
    dispose();
  }

  void dispose() {
    if (_disposed) return;
    _disposed = true;
    _handlers.clear();
    _ranges.clear();
    _setState(IVXTransportState.disconnected);
    _stateCtrl.close();
    _kernel._activeSessions.remove(matchId);
  }

  // ---- internals ----

  void _dispatch(IVXKernelEvent event) {
    if (_disposed) return;
    currentMatchTimeMs = event.header.matchTimeMs;
    final list = _handlers[event.header.opCode];
    if (list != null) {
      for (final h in List<IVXEnvelopeHandler>.from(list)) h(event);
    }
    for (final r in _ranges) {
      if (event.header.opCode >= r.from && event.header.opCode <= r.to) {
        r.handler(event);
      }
    }
  }

  void _setState(IVXTransportState s) {
    _state = s;
    if (!_stateCtrl.isClosed) _stateCtrl.add(s);
  }

  static String _uuidV4() {
    final r = Random.secure();
    final b = Uint8List(16);
    for (var i = 0; i < 16; i++) b[i] = r.nextInt(256);
    b[6] = (b[6] & 0x0F) | 0x40;
    b[8] = (b[8] & 0x3F) | 0x80;
    String hx(int i) => b[i].toRadixString(16).padLeft(2, '0');
    return '${hx(0)}${hx(1)}${hx(2)}${hx(3)}-${hx(4)}${hx(5)}-${hx(6)}${hx(7)}-${hx(8)}${hx(9)}-${hx(10)}${hx(11)}${hx(12)}${hx(13)}${hx(14)}${hx(15)}';
  }
}

class _RangeBinding {
  final int from;
  final int to;
  final IVXEnvelopeHandler handler;
  _RangeBinding(this.from, this.to, this.handler);
}

/// Top-level adapter. One per authenticated player.
class IVXMultiplayerKernel {
  IVXMultiplayerKernel({
    required this.client,
    required this.session,
  });

  /// Nakama Dart `Client` — duck-typed so the adapter compiles against
  /// every recent release of the package.
  final dynamic client;
  /// Nakama Dart `Session`.
  final dynamic session;

  IVXTransportState _transport = IVXTransportState.disconnected;
  IVXTransportState get transportState => _transport;
  final Map<String, IVXMatchSession> _activeSessions = {};
  bool _initialized = false;
  dynamic _socket;

  final StreamController<IVXTransportState> _stateCtrl =
      StreamController.broadcast();
  Stream<IVXTransportState> get onTransportState => _stateCtrl.stream;

  Future<bool> initialize() async {
    if (_initialized) return true;
    if (client == null || session == null) return false;
    try {
      _socket = await client.createSocket(session: session);
    } catch (_) {
      _setState(IVXTransportState.failedFatal);
      return false;
    }
    _setState(IVXTransportState.connecting);
    // Wire socket signals — across versions these expose either Streams or
    // setters. We feature-detect and pick one path.
    _wireSocketStreams();
    _initialized = true;
    _setState(IVXTransportState.connected);
    return true;
  }

  Future<void> shutdown() async {
    if (!_initialized) return;
    for (final s in List<IVXMatchSession>.from(_activeSessions.values)) {
      s.dispose();
    }
    _activeSessions.clear();
    try {
      await _socket?.close();
    } catch (_) {}
    _socket = null;
    _initialized = false;
    _setState(IVXTransportState.disconnected);
    if (!_stateCtrl.isClosed) await _stateCtrl.close();
  }

  Future<IVXCreateMatchResponse?> createMatch(IVXCreateMatchRequest req) async {
    if (!_initialized) return null;
    try {
      final rpc = await client.rpc(
        session: session,
        id: 'mp_create_match',
        payload: jsonEncode(req.toJson()),
      );
      final body = rpc?.payload as String? ?? '';
      if (body.isEmpty) return null;
      return IVXCreateMatchResponse.fromJson(
          jsonDecode(body) as Map<String, dynamic>);
    } catch (e) {
      // Production: hook into your error reporter.
      // ignore: avoid_print
      print('[IVXMultiplayerKernel] createMatch failed: $e');
      return null;
    }
  }

  Future<IVXMatchSession?> joinMatch(String matchId) async {
    if (!_initialized || _socket == null) return null;
    try {
      final m = await _socket.joinMatch(matchId);
      final sess = IVXMatchSession._(
        matchId: matchId,
        localUserId: _readLocalUserId(),
        kernel: this,
        socket: _socket,
      );
      sess._templateId = (m?.label as String?) ?? '';
      _activeSessions[matchId] = sess;
      sess._setState(IVXTransportState.connected);
      return sess;
    } catch (e) {
      // ignore: avoid_print
      print('[IVXMultiplayerKernel] joinMatch failed: $e');
      return null;
    }
  }

  Future<IVXMatchSession?> createAndJoin(IVXCreateMatchRequest req) async {
    final r = await createMatch(req);
    if (r == null) return null;
    return joinMatch(r.matchId);
  }

  // ---- internals ----

  void _wireSocketStreams() {
    // The recent versions of nakama-dart expose `.onMatchData`. Older
    // versions expose `.matchData.listen`. We try the modern path first.
    try {
      // ignore: avoid_dynamic_calls
      final stream = _socket.onMatchData as Stream;
      stream.listen(_onMatchData);
      return;
    } catch (_) {}
    try {
      // ignore: avoid_dynamic_calls
      _socket.matchData.listen(_onMatchData);
    } catch (e) {
      // ignore: avoid_print
      print('[IVXMultiplayerKernel] could not wire matchData stream: $e');
    }
  }

  void _onMatchData(dynamic md) {
    final matchId = (md.matchId ?? md.match_id ?? '') as String;
    final sess = _activeSessions[matchId];
    if (sess == null) return;
    final raw = md.data;
    String body;
    if (raw is String) {
      body = raw;
    } else if (raw is List<int>) {
      body = utf8.decode(raw, allowMalformed: true);
    } else {
      return;
    }
    Map<String, dynamic> obj;
    try {
      obj = jsonDecode(body) as Map<String, dynamic>;
    } catch (_) {
      return;
    }
    final hdr = (obj['h'] as Map<String, dynamic>?) ?? const {};
    final ev = IVXKernelEvent(
      header: IVXKernelHeader(
        seq: (hdr['s'] as num?)?.toInt() ?? 0,
        matchTimeMs: (hdr['t'] as num?)?.toInt() ?? 0,
        uuid: (hdr['u'] as String?) ?? '',
        opCode: (md.opCode ?? md.op_code ?? 0) as int,
        senderUserId:
            (md.presence?.userId ?? md.presence?.user_id ?? '') as String,
      ),
      payload: obj['p'],
      recvUnixMs: DateTime.now().millisecondsSinceEpoch,
    );
    sess._dispatch(ev);
  }

  String _readLocalUserId() {
    try {
      // ignore: avoid_dynamic_calls
      return (session.userId ?? session.user_id ?? '') as String;
    } catch (_) {
      return '';
    }
  }

  void _setState(IVXTransportState s) {
    _transport = s;
    if (!_stateCtrl.isClosed) _stateCtrl.add(s);
  }
}
