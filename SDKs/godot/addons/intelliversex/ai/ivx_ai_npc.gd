# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXAINPCDialogManager
extends Node

## NPC dialog — stub matching Unity IVXAINPCDialogManager.

var is_initialized: bool:
	get:
		return false

func initialize(_config: Resource) -> void:
	push_warning("IVXAINPCDialogManager.initialize: Not yet implemented — stub only")

func set_auth_token(_token: String) -> void:
	push_warning("IVXAINPCDialogManager.set_auth_token: Not yet implemented — stub only")

func register_npc(_profile: Dictionary) -> void:
	push_warning("IVXAINPCDialogManager.register_npc: Not yet implemented — stub only")

func unregister_npc(_npc_id: String) -> void:
	push_warning("IVXAINPCDialogManager.unregister_npc: Not yet implemented — stub only")

func start_dialog(_npc_id: String, _player_id: String, _player_context: String = "") -> Dictionary:
	push_warning("IVXAINPCDialogManager.start_dialog: Not yet implemented — stub only")
	return {}

func send_message(_session_id: String, _message: String) -> String:
	push_warning("IVXAINPCDialogManager.send_message: Not yet implemented — stub only")
	return ""

func end_dialog(_session_id: String) -> void:
	push_warning("IVXAINPCDialogManager.end_dialog: Not yet implemented — stub only")

func get_session(_session_id: String) -> Dictionary:
	push_warning("IVXAINPCDialogManager.get_session: Not yet implemented — stub only")
	return {}

func get_sessions_for_npc(_npc_id: String) -> Array:
	push_warning("IVXAINPCDialogManager.get_sessions_for_npc: Not yet implemented — stub only")
	return []
