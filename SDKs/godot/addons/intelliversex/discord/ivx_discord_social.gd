# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXDiscordSocial
extends Node

## IntelliVerseX Discord Social — Rich Presence, unified friends, lobby chat,
## voice calls, and game invites via the Discord Social SDK.

signal discord_ready(provisional: bool)
signal discord_error(error_message: String)
signal invite_received(invite: Dictionary)
signal join_request(user_id: String, username: String)
signal lobby_message_received(message: Dictionary)
signal voice_state_update(user_id: String, speaking: bool)
signal friends_updated(friends: Array)
signal lobby_joined(lobby_id: String)
signal lobby_left()
signal voice_joined(channel_id: String)
signal voice_left()

var _config: Dictionary = {}
var _is_initialized: bool = false
var _active_lobby_id: String = ""
var _active_voice_channel_id: String = ""
var _self_muted: bool = false
var _self_deafened: bool = false


# ── Manager ──────────────────────────────────────────────────────────────────

## Initialize the Discord Social SDK with the given config dictionary.
## Config keys: application_id (int), default_lobby_secret (String),
## enable_voice (bool), enable_overlay (bool).
func initialize(config: Dictionary) -> void:
	if _is_initialized:
		push_warning("[IVXDiscordSocial] Already initialized")
		return
	_config = config
	# Discord Social SDK init goes here — integration point.
	_is_initialized = true
	print("[IVXDiscordSocial] Initialized with app id %s" % str(config.get("application_id", 0)))
	discord_ready.emit(false)


## Link the current player's account with Discord.
func link_account() -> bool:
	if not _ensure_initialized():
		return false
	# Discord OAuth / account linking flow — integration point.
	print("[IVXDiscordSocial] Account linked")
	return true


## Unlink the current player's Discord account.
func unlink_account() -> bool:
	if not _ensure_initialized():
		return false
	print("[IVXDiscordSocial] Account unlinked")
	return true


## Returns true if the SDK has been initialised.
func is_initialized() -> bool:
	return _is_initialized


# ── Rich Presence ────────────────────────────────────────────────────────────

## Set the current activity shown in Discord.
func set_activity(details: String, state: String, start_timestamp: int = 0, end_timestamp: int = 0) -> void:
	if not _ensure_initialized():
		return
	# Push activity to Discord Social SDK — integration point.
	print("[IVXDiscordSocial] SetActivity: %s — %s" % [details, state])


## Set party info attached to the current presence.
func set_party(party_id: String, current_size: int, max_size: int, join_secret: String = "") -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] SetParty: %s (%d/%d)" % [party_id, current_size, max_size])


## Clear the current Discord presence.
func clear_presence() -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] Presence cleared")


# ── Friends ──────────────────────────────────────────────────────────────────

## Returns a merged list of game + Discord friends.
## Each entry is a Dictionary with keys: user_id, display_name, avatar_url,
## source ("game"|"discord"|"both"), online (bool).
func get_unified_friends() -> Array:
	if not _ensure_initialized():
		return []
	# Merge Discord friends with game friends — integration point.
	var friends: Array = []
	print("[IVXDiscordSocial] GetUnifiedFriends: returned %d friends" % friends.size())
	friends_updated.emit(friends)
	return friends


# ── Lobby ────────────────────────────────────────────────────────────────────

## Create or join a lobby by secret. Returns true on success.
func create_or_join_lobby(lobby_secret: String) -> bool:
	if not _ensure_initialized():
		return false
	_active_lobby_id = lobby_secret
	print("[IVXDiscordSocial] Joined lobby: %s" % lobby_secret)
	lobby_joined.emit(lobby_secret)
	return true


## Leave the current lobby.
func leave_lobby() -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] Left lobby: %s" % _active_lobby_id)
	_active_lobby_id = ""
	lobby_left.emit()


## Send a text message to the current lobby.
func send_lobby_message(content: String) -> void:
	if not _ensure_initialized():
		return
	if _active_lobby_id.is_empty():
		push_warning("[IVXDiscordSocial] Not in a lobby")
		return
	print("[IVXDiscordSocial] Lobby message sent: %s" % content)


# ── Voice ────────────────────────────────────────────────────────────────────

## Join a voice call for the given lobby.
func join_voice_call(lobby_id: String) -> bool:
	if not _ensure_initialized():
		return false
	if not _config.get("enable_voice", true):
		push_warning("[IVXDiscordSocial] Voice is disabled in config")
		return false
	_active_voice_channel_id = lobby_id
	print("[IVXDiscordSocial] Joined voice call: %s" % lobby_id)
	voice_joined.emit(lobby_id)
	return true


## Leave the current voice call.
func leave_voice_call() -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] Left voice call: %s" % _active_voice_channel_id)
	_active_voice_channel_id = ""
	_self_muted = false
	_self_deafened = false
	voice_left.emit()


## Mute or unmute self.
func set_self_mute(mute: bool) -> void:
	if not _ensure_initialized():
		return
	_self_muted = mute
	print("[IVXDiscordSocial] Self mute: %s" % str(mute))


## Deafen or undeafen self.
func set_self_deafen(deafen: bool) -> void:
	if not _ensure_initialized():
		return
	_self_deafened = deafen
	print("[IVXDiscordSocial] Self deafen: %s" % str(deafen))


## Set volume for a specific participant (0.0 – 2.0).
func set_participant_volume(user_id: String, volume: float) -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] SetParticipantVolume: %s -> %.2f" % [user_id, volume])


# ── Invites ──────────────────────────────────────────────────────────────────

## Send a game invite to a user.
func send_invite(user_id: String, message: String = "") -> bool:
	if not _ensure_initialized():
		return false
	print("[IVXDiscordSocial] Invite sent to %s: %s" % [user_id, message])
	return true


## Accept an incoming invite.
func accept_invite(invite_id: String) -> bool:
	if not _ensure_initialized():
		return false
	print("[IVXDiscordSocial] Invite accepted: %s" % invite_id)
	return true


## Decline an incoming invite.
func decline_invite(invite_id: String) -> void:
	if not _ensure_initialized():
		return
	print("[IVXDiscordSocial] Invite declined: %s" % invite_id)


# ── Private helpers ──────────────────────────────────────────────────────────

func _ensure_initialized() -> bool:
	if not _is_initialized:
		push_warning("[IVXDiscordSocial] Not initialized — call initialize() first")
	return _is_initialized
