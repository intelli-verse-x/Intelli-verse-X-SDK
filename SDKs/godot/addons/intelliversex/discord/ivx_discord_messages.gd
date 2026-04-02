# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXDiscordMessages
extends Node

## Discord DMs API — stub matching Unity IVXDiscordMessages.

var is_showing_chat: bool:
	get:
		return false

func send_dm(_recipient_id: String, _message: String) -> Dictionary:
	push_error("IVXDiscordMessages.send_dm: Not implemented")
	return {"error": "Not implemented"}

func edit_dm(_recipient_id: String, _message_id: String, _new_content: String) -> void:
	push_error("IVXDiscordMessages.edit_dm: Not implemented")

func get_dm_history(_recipient_id: String, _limit: int = 50) -> Array:
	push_error("IVXDiscordMessages.get_dm_history: Not implemented")
	return []

func get_dm_summaries() -> Array:
	push_error("IVXDiscordMessages.get_dm_summaries: Not implemented")
	return []

func set_showing_chat(_showing: bool) -> void:
	push_error("IVXDiscordMessages.set_showing_chat: Not implemented")

func open_message_in_discord(_message_id: String) -> void:
	push_error("IVXDiscordMessages.open_message_in_discord: Not implemented")

func open_dm_settings_in_discord() -> void:
	push_error("IVXDiscordMessages.open_dm_settings_in_discord: Not implemented")
