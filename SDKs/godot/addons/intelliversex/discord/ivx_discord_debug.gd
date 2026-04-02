# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXDiscordDebug
extends RefCounted

## Discord SDK debug logging — level filter, history ring buffer, and broadcast signal.

enum LogLevel {
	NONE,
	ERROR,
	WARN,
	INFO,
	DEBUG,
}

signal log_received(level: int, message: String, source: String)

var _log_level: LogLevel = LogLevel.WARN
var _log_history: Array = []


func set_log_level(level: LogLevel) -> void:
	_log_level = level


func get_log_level() -> LogLevel:
	return _log_level


func get_log_history(limit: int = 100) -> Array:
	var n: int = mini(limit, _log_history.size())
	if n <= 0:
		return []
	return _log_history.slice(_log_history.size() - n, _log_history.size())


func clear_log_history() -> void:
	_log_history.clear()
