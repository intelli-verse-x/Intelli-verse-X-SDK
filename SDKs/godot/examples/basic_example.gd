extends Node

## IntelliVerseX SDK — complete usage example.
##
## Attach this script to any Node in your scene. It demonstrates:
##   1. SDK initialisation
##   2. Device authentication (with session restore)
##   3. Fetching and updating the player profile
##   4. Wallet operations (fetch / grant currency)
##   5. Leaderboard (submit score / fetch records)
##   6. Cloud storage (write / read)
##
## Prerequisites:
##   - The Nakama Godot addon is installed.
##   - The IntelliVerseX addon is in res://addons/intelliversex/.
##   - An IVXConfig resource exists at res://intelliversex_config.tres
##     (or create one in code as shown below).

@export var server_host: String = "127.0.0.1"
@export var server_port: int = 7350
@export var server_key: String = "defaultkey"
@export var use_ssl: bool = false
@export var debug_logs: bool = true

var ivx: Node


func _ready():
	ivx = get_node("/root/IntelliVerseX")

	_connect_signals()
	_initialize_sdk()


# ---------------------------------------------------------------------------
# 1. Initialisation
# ---------------------------------------------------------------------------

func _initialize_sdk() -> void:
	var cfg := IVXConfig.new()
	cfg.nakama_host = server_host
	cfg.nakama_port = server_port
	cfg.nakama_server_key = server_key
	cfg.nakama_scheme = "https" if use_ssl else "http"
	cfg.enable_debug_logs = debug_logs

	ivx.initialize(cfg)


func _connect_signals() -> void:
	ivx.initialized.connect(_on_initialized)
	ivx.auth_success.connect(_on_auth_success)
	ivx.auth_error.connect(_on_auth_error)
	ivx.profile_loaded.connect(_on_profile_loaded)
	ivx.wallet_updated.connect(_on_wallet_updated)
	ivx.error.connect(_on_error)


# ---------------------------------------------------------------------------
# 2. Authentication
# ---------------------------------------------------------------------------

func _on_initialized() -> void:
	print("SDK ready — attempting session restore…")

	if ivx.restore_session():
		print("Session restored! User: %s" % ivx.user_id)
		_post_auth_flow()
	else:
		print("No saved session — authenticating with device ID…")
		ivx.authenticate_device()


func _on_auth_success(_session) -> void:
	print("Authenticated! User ID: %s  Username: %s" % [ivx.user_id, ivx.username])
	_post_auth_flow()


func _on_auth_error(message: String) -> void:
	printerr("Auth failed: %s" % message)


# ---------------------------------------------------------------------------
# 3. Post-auth workflow
# ---------------------------------------------------------------------------

func _post_auth_flow() -> void:
	await _demo_profile()
	await _demo_wallet()
	await _demo_leaderboard()
	await _demo_storage()
	print("\n--- All demos complete ---")


# ---------------------------------------------------------------------------
# 4. Profile
# ---------------------------------------------------------------------------

func _demo_profile() -> void:
	print("\n--- Profile ---")
	var profile := await ivx.fetch_profile()
	if profile.is_empty():
		print("  (could not fetch profile)")
		return

	print("  Display name : %s" % profile.get("display_name", ""))
	print("  Avatar URL   : %s" % profile.get("avatar_url", ""))
	print("  Language      : %s" % profile.get("lang_tag", ""))

	var ok := await ivx.update_profile("GodotPlayer", "", "en")
	if ok:
		print("  Profile updated successfully")


func _on_profile_loaded(profile: Dictionary) -> void:
	print("  [signal] profile_loaded — user_id=%s" % profile.get("user_id", ""))


# ---------------------------------------------------------------------------
# 5. Wallet
# ---------------------------------------------------------------------------

func _demo_wallet() -> void:
	print("\n--- Wallet ---")
	var wallet := await ivx.fetch_wallet()
	print("  Current wallet: %s" % str(wallet))

	var grant_result := await ivx.grant_currency("coins", 100)
	print("  Grant result  : %s" % str(grant_result))


func _on_wallet_updated(wallet: Dictionary) -> void:
	print("  [signal] wallet_updated — %s" % str(wallet))


# ---------------------------------------------------------------------------
# 6. Leaderboard
# ---------------------------------------------------------------------------

func _demo_leaderboard() -> void:
	print("\n--- Leaderboard ---")
	var leaderboard_id := "weekly_high_scores"

	var submitted := await ivx.submit_score(leaderboard_id, randi_range(500, 5000))
	print("  Score submitted: %s" % str(submitted))

	var records := await ivx.fetch_leaderboard(leaderboard_id, 10)
	print("  Top %d records:" % records.size())
	for r in records:
		print("    #%s  %s — %d pts" % [str(r.rank), r.username, r.score])


# ---------------------------------------------------------------------------
# 7. Cloud Storage
# ---------------------------------------------------------------------------

func _demo_storage() -> void:
	print("\n--- Storage ---")
	var save_data := {
		"level": 5,
		"xp": 2350,
		"inventory": ["sword", "shield", "potion"],
		"last_save": Time.get_datetime_string_from_system(),
	}

	var written := await ivx.write_storage("player_saves", "slot_1", save_data)
	print("  Write OK: %s" % str(written))

	var loaded := await ivx.read_storage("player_saves", "slot_1")
	if loaded.is_empty():
		print("  (nothing loaded)")
	else:
		print("  Loaded save — level=%s  xp=%s" % [str(loaded.get("level")), str(loaded.get("xp"))])
		print("  Inventory: %s" % str(loaded.get("inventory", [])))


# ---------------------------------------------------------------------------
# Error handler
# ---------------------------------------------------------------------------

func _on_error(message: String) -> void:
	printerr("[IntelliVerseX Error] %s" % message)


# ---------------------------------------------------------------------------
# Cleanup
# ---------------------------------------------------------------------------

func _exit_tree() -> void:
	if ivx:
		ivx.disconnect_socket()
