# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXAINPCDialogManager
extends Node

## NPC dialog — stub matching Unity IVXAINPCDialogManager.

var is_initialized: bool:
	get:
		return false

func initialize(_config: Resource) -> void:
	push_error("IVXAINPCDialogManager.initialize: Not implemented")

func set_auth_token(_token: String) -> void:
	push_error("IVXAINPCDialogManager.set_auth_token: Not implemented")

func register_npc(_profile: Dictionary) -> void:
	push_error("IVXAINPCDialogManager.register_npc: Not implemented")

func unregister_npc(_npc_id: String) -> void:
	push_error("IVXAINPCDialogManager.unregister_npc: Not implemented")

func start_dialog(_npc_id: String, _player_id: String, _player_context: String = "") -> Dictionary:
	push_error("IVXAINPCDialogManager.start_dialog: Not implemented")
	return {}

func send_message(_session_id: String, _message: String) -> String:
	push_error("IVXAINPCDialogManager.send_message: Not implemented")
	return ""

func end_dialog(_session_id: String) -> void:
	push_error("IVXAINPCDialogManager.end_dialog: Not implemented")

func get_session(_session_id: String) -> Dictionary:
	push_error("IVXAINPCDialogManager.get_session: Not implemented")
	return {}

func get_sessions_for_npc(_npc_id: String) -> Array:
	push_error("IVXAINPCDialogManager.get_sessions_for_npc: Not implemented")
	return []
