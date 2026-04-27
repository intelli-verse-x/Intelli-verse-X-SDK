# IVXMultiplayerKernel — Godot 4 adapter for the IntelliVerseX Multiplayer
# Kernel. Mirrors the IIVXMultiplayer / IIVXMatchSession contract from the
# Unity, JS, Unreal, Flutter, Java, and C++ adapters.
#
# Wraps the official `addons/com.heroiclabs.nakama` Godot client and speaks
# the wire protocol defined in
#   `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
#
# Usage:
#   var kernel = IVXMultiplayerKernel.new(nakama_client, nakama_session)
#   kernel.transport_state_changed.connect(_on_state)
#   var resp = await kernel.create_match({
#       "template_id": "sync-turn-v1",
#       "game_id": "demo",
#       "template_init": {"min_players": 2}
#   })
#   var session = await kernel.join_match(resp.match_id)
#   session.subscribe(0xC100, func(env): print("question: ", env.payload))
#   session.send(0xC101, {"answer_id": "a"})
#
# This file is intentionally Godot-idiomatic (signals, Resource-style structs)
# and avoids exposing raw Nakama types so games port cleanly to other engines.

class_name IVXMultiplayerKernel
extends RefCounted

# ---- enums ----

enum TransportState {
    DISCONNECTED  = 0,
    CONNECTING    = 1,
    CONNECTED     = 2,
    RECONNECTING  = 3,
    FAILED_FATAL  = 4,
}

enum EndReason {
    UNKNOWN              = 0,
    COMPLETED            = 1,
    CANCELLED            = 2,
    DURATION_EXCEEDED    = 3,
    KERNEL_INTERNAL      = 4,
    ALL_PLAYERS_LEFT     = 5,
    HOST_TERMINATED      = 6,
}

# ---- signals ----

signal transport_state_changed(state)
signal kernel_error(err_dict)

# ---- members ----

var _nakama_client: Variant         # NakamaClient
var _nakama_session: Variant        # NakamaSession
var _socket: Variant                # NakamaSocket
var _initialized: bool = false
var transport_state: int = TransportState.DISCONNECTED
# match_id -> IVXMatchSession
var _active_sessions: Dictionary = {}

# ---- ctor ----

func _init(nakama_client, nakama_session) -> void:
    _nakama_client = nakama_client
    _nakama_session = nakama_session

# ---- public API ----

func initialize() -> bool:
    if _initialized: return true
    if _nakama_client == null or _nakama_session == null:
        push_warning("[IVXMultiplayerKernel] initialize: missing client/session")
        return false
    _socket = _nakama_client.create_socket_from(_nakama_session)
    if _socket == null:
        _set_state(TransportState.FAILED_FATAL)
        return false
    # Wire signals from the Nakama Godot client. Names match the
    # @heroiclabs/nakama-godot 3.x addon.
    _socket.connected.connect(func(): _set_state(TransportState.CONNECTED))
    _socket.closed.connect(func(): _set_state(TransportState.DISCONNECTED))
    _socket.received_error.connect(func(e): kernel_error.emit({"code": e.code, "message": e.message}))
    _socket.received_match_state.connect(_on_match_state)
    _set_state(TransportState.CONNECTING)
    var connected = await _socket.connect_async(_nakama_session)
    if connected.is_exception():
        _set_state(TransportState.FAILED_FATAL)
        return false
    _initialized = true
    return true

func shutdown() -> void:
    if not _initialized: return
    for sess in _active_sessions.values():
        sess.dispose()
    _active_sessions.clear()
    if _socket != null:
        _socket.close()
        _socket = null
    _initialized = false
    _set_state(TransportState.DISCONNECTED)

# Returns Dictionary{match_id, template_id, region, expires_unix_ms} or
# Dictionary{error: str} on failure.
func create_match(req: Dictionary) -> Dictionary:
    if not _initialized:
        return {"error": "not_initialized"}
    var payload := JSON.stringify({
        "template_id":   req.get("template_id", ""),
        "game_id":       req.get("game_id", ""),
        "region":        req.get("region", ""),
        "template_init": req.get("template_init", {}),
    })
    var rpc = await _nakama_client.rpc_async(_nakama_session, "mp_create_match", payload)
    if rpc.is_exception():
        return {"error": rpc.get_exception().message}
    var parsed: Variant = JSON.parse_string(rpc.payload)
    if typeof(parsed) != TYPE_DICTIONARY:
        return {"error": "invalid_response"}
    return parsed

# Returns IVXMatchSession or null on failure.
func join_match(match_id: String) -> Variant:
    if not _initialized:
        return null
    var session := IVXMatchSession.new(self, _socket, match_id, _nakama_session.user_id)
    var joined = await _socket.join_match_async(match_id)
    if joined.is_exception():
        push_warning("[IVXMultiplayerKernel] join_match failed: %s" % joined.get_exception().message)
        return null
    session._template_id = joined.label   # kernel writes templateId in label
    session._set_state(TransportState.CONNECTED)
    _active_sessions[match_id] = session
    return session

# Convenience: create + join.
func create_and_join(req: Dictionary) -> Variant:
    var resp := await create_match(req)
    if resp.has("error"):
        return null
    return await join_match(resp.get("match_id", ""))

# ---- internal ----

func _on_match_state(match_state) -> void:
    var sess = _active_sessions.get(match_state.match_id, null)
    if sess == null: return
    var inbound: Dictionary = {}
    var raw := match_state.data
    if typeof(raw) == TYPE_PACKED_BYTE_ARRAY:
        raw = raw.get_string_from_utf8()
    var parsed = JSON.parse_string(raw)
    if typeof(parsed) != TYPE_DICTIONARY:
        return
    var hdr: Dictionary = parsed.get("h", {})
    var env := {
        "header": {
            "op_code":        match_state.op_code,
            "seq":            int(hdr.get("s", 0)),
            "match_time_ms":  int(hdr.get("t", 0)),
            "uuid":           str(hdr.get("u", "")),
            "sender_user_id": match_state.presence.user_id if match_state.presence != null else "",
        },
        "payload":      parsed.get("p", null),
        "recv_unix_ms": Time.get_unix_time_from_system() * 1000,
    }
    sess._dispatch(env)

func _set_state(s: int) -> void:
    transport_state = s
    transport_state_changed.emit(s)

# ===========================================================================
# IVXMatchSession — nested class. One per joined match.
# ===========================================================================

class IVXMatchSession extends RefCounted:
    signal transport_state_changed(state)

    var _kernel: IVXMultiplayerKernel
    var _socket: Variant
    var match_id: String = ""
    var local_user_id: String = ""
    var current_match_time_ms: int = 0
    var active_player_count: int = 0
    var _template_id: String = ""
    var _state: int = TransportState.DISCONNECTED
    # op_code -> Array[Callable]
    var _handlers: Dictionary = {}
    # Array of {"from": int, "to": int, "handler": Callable}
    var _range_handlers: Array = []
    var _local_seq: int = 0
    var _disposed: bool = false

    func _init(kernel: IVXMultiplayerKernel, socket: Variant, mid: String, uid: String) -> void:
        _kernel = kernel
        _socket = socket
        match_id = mid
        local_user_id = uid

    func template_id() -> String: return _template_id
    func state() -> int: return _state

    func subscribe(op_code: int, handler: Callable) -> Callable:
        var arr: Array = _handlers.get(op_code, [])
        arr.append(handler)
        _handlers[op_code] = arr
        # Return an unsubscribe callable for parity with the JS adapter.
        return func():
            var current: Array = _handlers.get(op_code, [])
            current.erase(handler)
            if current.is_empty(): _handlers.erase(op_code)
            else: _handlers[op_code] = current

    func subscribe_range(op_from: int, op_to: int, handler: Callable) -> Callable:
        var rec := {"from": op_from, "to": op_to, "handler": handler}
        _range_handlers.append(rec)
        return func(): _range_handlers.erase(rec)

    # `payload` is a Dictionary; the adapter encodes the kernel envelope.
    func send(op_code: int, payload: Variant) -> void:
        if _disposed or _socket == null: return
        _local_seq += 1
        var env := {
            "h": {
                "s": _local_seq,
                "t": current_match_time_ms,
                "u": _make_uuid_v4(),
            },
            "p": payload,
        }
        _socket.send_match_state_async(match_id, op_code, JSON.stringify(env))

    func leave() -> void:
        if _disposed or _socket == null: return
        await _socket.leave_match_async(match_id)
        dispose()

    func dispose() -> void:
        if _disposed: return
        _disposed = true
        _handlers.clear()
        _range_handlers.clear()
        _set_state(TransportState.DISCONNECTED)
        if _kernel != null:
            _kernel._active_sessions.erase(match_id)

    # ---- internals ----
    func _dispatch(env: Dictionary) -> void:
        if _disposed: return
        current_match_time_ms = env.header.match_time_ms
        var op: int = env.header.op_code
        var bound: Array = _handlers.get(op, [])
        for h in bound:
            if h is Callable: h.call(env)
        for r in _range_handlers:
            if op >= int(r.from) and op <= int(r.to):
                if r.handler is Callable: r.handler.call(env)

    func _set_state(s: int) -> void:
        _state = s
        transport_state_changed.emit(s)

    func _make_uuid_v4() -> String:
        # Pseudo-uuid v4 (xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx). Good enough
        # for envelope dedup; not for cryptographic use.
        const HEX := "0123456789abcdef"
        var b := PackedByteArray()
        b.resize(16)
        var rng := RandomNumberGenerator.new()
        rng.randomize()
        for i in 16: b[i] = rng.randi() & 0xFF
        b[6] = (b[6] & 0x0F) | 0x40   # version 4
        b[8] = (b[8] & 0x3F) | 0x80   # variant
        var out := ""
        var dashes := [4, 6, 8, 10]
        for i in 16:
            out += HEX[(b[i] >> 4) & 0xF] + HEX[b[i] & 0xF]
            if dashes.has(i + 1) and i + 1 < 16: out += "-"
        return out
