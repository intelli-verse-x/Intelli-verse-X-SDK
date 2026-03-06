@tool
extends EditorPlugin

const AUTOLOAD_NAME := "IntelliVerseX"
const AUTOLOAD_PATH := "res://addons/intelliversex/core/ivx_manager.gd"

func _enter_tree() -> void:
	add_autoload_singleton(AUTOLOAD_NAME, AUTOLOAD_PATH)
	print("[IntelliVerseX] Plugin enabled")

func _exit_tree() -> void:
	remove_autoload_singleton(AUTOLOAD_NAME)
	print("[IntelliVerseX] Plugin disabled")
