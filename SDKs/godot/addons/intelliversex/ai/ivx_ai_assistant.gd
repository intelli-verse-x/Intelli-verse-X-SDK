# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXAIAssistant
extends Node

## In-game AI assistant — stub matching Unity IVXAIAssistant.

var system_prompt: String = ""
var is_processing: bool:
	get:
		return false
var is_initialized: bool:
	get:
		return false

func initialize(_config: Resource) -> void:
	push_error("IVXAIAssistant.initialize: Not implemented")

func set_auth_token(_token: String) -> void:
	push_error("IVXAIAssistant.set_auth_token: Not implemented")

func clear_history() -> void:
	push_error("IVXAIAssistant.clear_history: Not implemented")

func set_system_prompt(p: String) -> void:
	system_prompt = p

func ask(_question: String, _game_context: Dictionary = {}) -> Dictionary:
	push_error("IVXAIAssistant.ask: Not implemented")
	return {}

func get_hint(_level_id: String, _objective_id: String, _game_context: Dictionary = {}) -> Dictionary:
	push_error("IVXAIAssistant.get_hint: Not implemented")
	return {}

func get_tutorial(_feature_id: String) -> Dictionary:
	push_error("IVXAIAssistant.get_tutorial: Not implemented")
	return {}

func search_knowledge_base(_query: String) -> PackedStringArray:
	push_error("IVXAIAssistant.search_knowledge_base: Not implemented")
	return PackedStringArray()
