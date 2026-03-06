extends GutTest

## IntelliVerseX SDK unit tests — requires GUT addon (https://github.com/bitwes/Gut).
## Run from the Godot editor via the GUT panel or CLI.

var manager: Node


func before_each():
	manager = load("res://addons/intelliversex/core/ivx_manager.gd").new()
	add_child(manager)


func after_each():
	if manager:
		manager.queue_free()
		manager = null


# ---------------------------------------------------------------------------
# Config creation & validation
# ---------------------------------------------------------------------------

func test_config_resource_exists():
	var cfg = IVXConfig.new()
	assert_not_null(cfg, "IVXConfig resource should instantiate")


func test_config_defaults():
	var cfg = IVXConfig.new()
	assert_eq(cfg.nakama_server_key, "defaultkey", "Default server key should be 'defaultkey'")
	assert_eq(cfg.nakama_host, "127.0.0.1", "Default host should be localhost")
	assert_eq(cfg.nakama_port, 7350, "Default port should be 7350")


func test_config_custom_values():
	var cfg = IVXConfig.new()
	cfg.nakama_host = "play.example.com"
	cfg.nakama_port = 443
	cfg.nakama_scheme = "https"
	cfg.enable_debug_logs = true

	assert_eq(cfg.nakama_host, "play.example.com")
	assert_eq(cfg.nakama_port, 443)
	assert_eq(cfg.nakama_scheme, "https")
	assert_true(cfg.enable_debug_logs)


# ---------------------------------------------------------------------------
# Manager initialisation
# ---------------------------------------------------------------------------

func test_not_initialized_by_default():
	assert_false(manager.is_initialized, "Manager should not be initialized before calling initialize()")


func test_sdk_version_set():
	assert_eq(manager.SDK_VERSION, "5.1.0", "SDK version constant should be 5.1.0")


func test_initialize_sets_flag():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	assert_true(manager.is_initialized, "is_initialized should be true after initialize()")


func test_initialize_emits_signal():
	var cfg = IVXConfig.new()
	watch_signals(manager)
	manager.initialize(cfg)
	assert_signal_emitted(manager, "initialized")


func test_initialize_creates_client():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	assert_not_null(manager.nakama_client, "nakama_client should be created after initialize()")


# ---------------------------------------------------------------------------
# Session state
# ---------------------------------------------------------------------------

func test_no_session_by_default():
	assert_null(manager.nakama_session, "Session should be null before auth")
	assert_false(manager.has_valid_session(), "has_valid_session() should be false before auth")


func test_user_id_empty_without_session():
	assert_eq(manager.user_id, "", "user_id should be empty string without session")


func test_username_empty_without_session():
	assert_eq(manager.username, "", "username should be empty string without session")


func test_restore_returns_false_with_no_data():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	var restored := manager.restore_session()
	assert_false(restored, "restore_session() should return false when no saved token exists")


func test_is_authenticating_default():
	assert_false(manager.is_authenticating, "is_authenticating should be false initially")


func test_clear_session_resets_state():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	manager.clear_session()
	assert_null(manager.nakama_session, "Session should be null after clear_session()")
	assert_null(manager.nakama_socket, "Socket should be null after clear_session()")


# ---------------------------------------------------------------------------
# Event signal connections
# ---------------------------------------------------------------------------

func test_auth_error_emitted_when_not_initialized():
	watch_signals(manager)
	manager.authenticate_device("test-device")
	assert_signal_emitted(manager, "error")


func test_error_signal_exists():
	assert_has_signal(manager, "error")


func test_auth_success_signal_exists():
	assert_has_signal(manager, "auth_success")


func test_auth_error_signal_exists():
	assert_has_signal(manager, "auth_error")


func test_profile_loaded_signal_exists():
	assert_has_signal(manager, "profile_loaded")


func test_wallet_updated_signal_exists():
	assert_has_signal(manager, "wallet_updated")


func test_initialized_signal_exists():
	assert_has_signal(manager, "initialized")


# ---------------------------------------------------------------------------
# Disconnect socket safety
# ---------------------------------------------------------------------------

func test_disconnect_socket_safe_when_null():
	manager.disconnect_socket()
	assert_null(manager.nakama_socket, "disconnect_socket() should not crash when socket is null")


# ---------------------------------------------------------------------------
# Logging safety
# ---------------------------------------------------------------------------

func test_log_safe_before_config():
	# _log should not crash when config is null
	manager._log("test message")
	pass_test("_log did not crash with null config")


func test_log_safe_with_debug_disabled():
	var cfg = IVXConfig.new()
	cfg.enable_debug_logs = false
	manager.initialize(cfg)
	manager._log("test message")
	pass_test("_log did not crash with debug disabled")


# ---------------------------------------------------------------------------
# Fetch / write guards (no session)
# ---------------------------------------------------------------------------

func test_fetch_profile_emits_error_without_session():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	watch_signals(manager)
	manager.fetch_profile()
	assert_signal_emitted(manager, "error")


func test_write_storage_emits_error_without_session():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	watch_signals(manager)
	manager.write_storage("test_col", "test_key", {"a": 1})
	assert_signal_emitted(manager, "error")


func test_submit_score_returns_false_without_session():
	var cfg = IVXConfig.new()
	manager.initialize(cfg)
	var result = await manager.submit_score("lb_test", 100)
	assert_false(result, "submit_score should return false without a valid session")
