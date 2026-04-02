# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXGameModes
extends Node

## IntelliVerseX Game Modes — lobby management, matchmaking, and player slots.

signal mode_changed(mode: GameMode)
signal player_added(slot: int, info: Dictionary)
signal player_removed(slot: int)
signal player_ready_changed(slot: int, ready: bool)
signal match_started
signal match_ended
signal match_found(match_info: Dictionary)
signal room_created(room_id: String)
signal room_joined(room_id: String)
signal room_left
signal room_list_updated(rooms: Array)
signal search_cancelled

enum GameMode {
	SOLO,
	LOCAL_MULTIPLAYER,
	ONLINE_VERSUS,
	ONLINE_COOP,
	RANKED,
	TURN_BASED,
}

var current_mode: GameMode = GameMode.SOLO
var max_players: int = 4
var players: Array[Dictionary] = []
var is_match_active: bool = false
var current_room_id: String = ""

var _is_searching: bool = false


## Select a game mode and configure the maximum number of players.
func select_mode(mode: GameMode, p_max_players: int = 4) -> void:
	current_mode = mode
	max_players = p_max_players
	players.clear()
	is_match_active = false
	print("[IVXGameModes] Mode set to %s (max %d players)" % [GameMode.keys()[mode], max_players])
	mode_changed.emit(mode)


## Add a player to the next available slot. Returns the slot info dictionary.
func add_player(display_name: String, is_local: bool = true) -> Dictionary:
	if players.size() >= max_players:
		push_warning("[IVXGameModes] Cannot add player — lobby full (%d/%d)" % [players.size(), max_players])
		return {}
	var slot_index := players.size()
	var info := {
		"slot": slot_index,
		"display_name": display_name,
		"is_local": is_local,
		"is_ready": false,
	}
	players.append(info)
	player_added.emit(slot_index, info)
	return info


## Remove a player by slot index.
func remove_player(slot_index: int) -> void:
	if slot_index < 0 or slot_index >= players.size():
		push_warning("[IVXGameModes] Invalid slot index: %d" % slot_index)
		return
	players.remove_at(slot_index)
	for i in range(slot_index, players.size()):
		players[i]["slot"] = i
	player_removed.emit(slot_index)


## Mark a player slot as ready or not.
func set_player_ready(slot: int, ready: bool) -> void:
	if slot < 0 or slot >= players.size():
		push_warning("[IVXGameModes] Invalid slot index: %d" % slot)
		return
	players[slot]["is_ready"] = ready
	player_ready_changed.emit(slot, ready)


## Start the match if all players are ready.
func start_match() -> void:
	if is_match_active:
		push_warning("[IVXGameModes] Match already active")
		return
	for p in players:
		if not p["is_ready"]:
			push_warning("[IVXGameModes] Not all players are ready")
			return
	is_match_active = true
	print("[IVXGameModes] Match started with %d players" % players.size())
	match_started.emit()


## End the current match.
func end_match() -> void:
	if not is_match_active:
		return
	is_match_active = false
	print("[IVXGameModes] Match ended")
	match_ended.emit()


## Reset all state back to defaults.
func reset() -> void:
	current_mode = GameMode.SOLO
	max_players = 4
	players.clear()
	is_match_active = false
	current_room_id = ""
	_is_searching = false


# ── Lobby ────────────────────────────────────────────────────────────────────

## Create a new room/lobby.
func create_room(room_name: String, room_config: Dictionary = {}) -> void:
	var room_id := "%s_%d" % [room_name.to_lower().replace(" ", "_"), Time.get_ticks_msec()]
	current_room_id = room_id
	print("[IVXGameModes] Room created: %s (config: %s)" % [room_id, str(room_config)])
	room_created.emit(room_id)


## Join an existing room by ID.
func join_room(room_id: String) -> void:
	current_room_id = room_id
	print("[IVXGameModes] Joined room: %s" % room_id)
	room_joined.emit(room_id)


## List available rooms. Returns an array of room info dictionaries via signal.
func list_rooms() -> Array:
	var rooms: Array = []
	room_list_updated.emit(rooms)
	return rooms


## Leave the current room.
func leave_room() -> void:
	if current_room_id == "":
		return
	print("[IVXGameModes] Left room: %s" % current_room_id)
	current_room_id = ""
	room_left.emit()


# ── Matchmaking ──────────────────────────────────────────────────────────────

## Start searching for a match with optional configuration.
func find_match(match_config: Dictionary = {}) -> void:
	if _is_searching:
		push_warning("[IVXGameModes] Already searching for a match")
		return
	_is_searching = true
	print("[IVXGameModes] Searching for match (config: %s)" % str(match_config))


## Cancel the current matchmaking search.
func cancel_search() -> void:
	if not _is_searching:
		return
	_is_searching = false
	print("[IVXGameModes] Search cancelled")
	search_cancelled.emit()
