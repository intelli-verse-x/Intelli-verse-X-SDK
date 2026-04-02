# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

## Lobby & Matchmaking via Nakama RPCs.
class_name IVXMultiplayer
extends Node

# ---------------------------------------------------------------------------
# Signals
# ---------------------------------------------------------------------------
signal lobby_created(lobby: Dictionary)
signal lobby_joined(lobby: Dictionary)
signal lobby_left(lobby_id: String)
signal lobbies_listed(lobbies: Array)
signal matchmaking_started(ticket: Dictionary)
signal matchmaking_cancelled(ticket_id: String)
signal multiplayer_error(message: String)

# ---------------------------------------------------------------------------
# Private state
# ---------------------------------------------------------------------------
var _nakama_client = null
var _session = null

# ---------------------------------------------------------------------------
# Initialisation
# ---------------------------------------------------------------------------

func initialize(nakama_client, session) -> void:
	_nakama_client = nakama_client
	_session = session

# ---------------------------------------------------------------------------
# RPC helper
# ---------------------------------------------------------------------------

func _rpc(rpc_id: String, payload: Dictionary = {}) -> Dictionary:
	if _nakama_client == null or _session == null:
		push_warning("[IVXMultiplayer] Not initialized — call initialize(client, session) first.")
		multiplayer_error.emit("Not initialized")
		return {}

	var body := JSON.stringify(payload) if payload.size() > 0 else "{}"
	var result = await _nakama_client.rpc_async(_session, rpc_id, body)

	if result == null:
		push_warning("[IVXMultiplayer] RPC '%s' returned null" % rpc_id)
		multiplayer_error.emit("RPC returned null: %s" % rpc_id)
		return {}

	if result.is_exception():
		var msg: String = result.get_exception().message if result.get_exception() else "Unknown error"
		push_warning("[IVXMultiplayer] RPC '%s' error: %s" % [rpc_id, msg])
		multiplayer_error.emit(msg)
		return {}

	if result.payload == null or result.payload.is_empty():
		return {}

	var parsed = JSON.parse_string(result.payload)
	if parsed == null:
		push_warning("[IVXMultiplayer] Failed to parse response for '%s'" % rpc_id)
		return {}

	return parsed as Dictionary

# ---------------------------------------------------------------------------
# Lobby
# ---------------------------------------------------------------------------

func create_lobby(lobby_name: String, max_players: int, is_public: bool) -> Dictionary:
	var data := await _rpc("create_lobby", {
		"name": lobby_name,
		"max_players": max_players,
		"is_public": is_public,
	})
	lobby_created.emit(data)
	return data

func join_lobby(lobby_id: String) -> Dictionary:
	var data := await _rpc("join_lobby", {"lobby_id": lobby_id})
	lobby_joined.emit(data)
	return data

func leave_lobby(lobby_id: String) -> void:
	await _rpc("leave_lobby", {"lobby_id": lobby_id})
	lobby_left.emit(lobby_id)

func list_lobbies() -> Array:
	var data := await _rpc("list_lobbies")
	var lobbies: Array = data.get("lobbies", [])
	lobbies_listed.emit(lobbies)
	return lobbies

# ---------------------------------------------------------------------------
# Matchmaking
# ---------------------------------------------------------------------------

func start_matchmaking(min_players: int, max_players: int, rank_range: int = -1) -> Dictionary:
	var payload := {
		"min_players": min_players,
		"max_players": max_players,
	}
	if rank_range >= 0:
		payload["rank_range"] = rank_range

	var data := await _rpc("start_matchmaking", payload)
	matchmaking_started.emit(data)
	return data

func cancel_matchmaking(ticket_id: String) -> void:
	await _rpc("cancel_matchmaking", {"ticket_id": ticket_id})
	matchmaking_cancelled.emit(ticket_id)
