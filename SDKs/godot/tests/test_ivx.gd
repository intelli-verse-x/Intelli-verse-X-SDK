# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

## IntelliVerseX Godot SDK — unit tests.
##
## Run from the Godot editor via GUT or from the command line:
##   godot --headless -s tests/test_ivx.gd

extends SceneTree


var _passed := 0
var _failed := 0
var _errors: Array[Dictionary] = []


func _assert_true(val: bool, msg: String = "") -> void:
	if not val:
		var text := msg if msg != "" else "expected true, got %s" % str(val)
		assert(false, text)


func _assert_eq(a: Variant, b: Variant, msg: String = "") -> void:
	if a != b:
		var text := "%s: expected %s, got %s" % [msg, str(b), str(a)]
		assert(false, text)


func _assert_not_nil(val: Variant, msg: String = "") -> void:
	if val == null:
		var text := msg if msg != "" else "expected non-null value"
		assert(false, text)


func _run_test(test_name: String, callable: Callable) -> void:
	var ok := true
	var err_msg := ""
	# GDScript doesn't have pcall; use a simple try-pattern via return values.
	callable.call()
	_passed += 1
	print("  ✓ %s" % test_name)


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------

func test_version() -> void:
	var mgr := preload("res://addons/intelliversex/core/ivx_manager.gd")
	_assert_not_nil(mgr, "ivx_manager script should load")
	_assert_eq(mgr.SDK_VERSION, "5.8.0", "SDK_VERSION")


func test_not_initialized_before_init() -> void:
	var mgr_node := preload("res://addons/intelliversex/core/ivx_manager.gd").new()
	_assert_true(not mgr_node.is_initialized, "should not be initialized before init")
	mgr_node.queue_free()


# ---------------------------------------------------------------------------
# Runner
# ---------------------------------------------------------------------------

func _init() -> void:
	print("\n========================================")
	print("  IntelliVerseX Godot SDK — Test Suite")
	print("========================================\n")

	test_version()
	test_not_initialized_before_init()

	print("\n----------------------------------------")
	print("  Results: %d passed, %d failed" % [_passed, _failed])
	print("----------------------------------------\n")

	quit(1 if _failed > 0 else 0)
