# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXDiscordModeration
extends Node

## Discord moderation — stub matching Unity IVXDiscordModeration.

var auto_moderate_enabled: bool = true

func enable_auto_moderation(_enable: bool) -> void:
	push_error("IVXDiscordModeration.enable_auto_moderation: Not implemented")

func process_moderation_metadata(_message_id: String, _metadata: Dictionary) -> void:
	push_error("IVXDiscordModeration.process_moderation_metadata: Not implemented")

static func get_moderation_action(_metadata: Dictionary) -> Dictionary:
	push_error("IVXDiscordModeration.get_moderation_action: Not implemented")
	return {}

func start_voice_moderation_capture(_lobby_id: String) -> void:
	push_error("IVXDiscordModeration.start_voice_moderation_capture: Not implemented")

func stop_voice_moderation_capture() -> void:
	push_error("IVXDiscordModeration.stop_voice_moderation_capture: Not implemented")

func report_user(_user_id: String, _reason: String) -> bool:
	push_error("IVXDiscordModeration.report_user: Not implemented")
	return false
