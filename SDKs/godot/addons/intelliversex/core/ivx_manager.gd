# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

extends Node

## IntelliVerseX SDK Manager — central coordinator for Nakama integration.
## Auto-loaded as the "IntelliVerseX" singleton.
## Uses runtime-loaded Nakama (real addon or built-in stub) so the project parses without the Nakama addon installed.

signal initialized
signal auth_success(session)
signal auth_error(message: String)
signal profile_loaded(profile: Dictionary)
signal wallet_updated(wallet: Dictionary)
signal error(message: String)

const SDK_VERSION := "5.8.0"
const SESSION_TOKEN_KEY := "ivx_session_token"
const REFRESH_TOKEN_KEY := "ivx_refresh_token"
const DEVICE_ID_KEY := "ivx_device_id"

var config: IVXConfig
var nakama_client: Variant
var nakama_session: Variant
var nakama_socket: Variant

var is_initialized: bool = false
var is_authenticating: bool = false

var _nakama_refs: Dictionary = {}

var user_id: String:
	get:
		return nakama_session.user_id if nakama_session else ""

var username: String:
	get:
		return nakama_session.username if nakama_session else ""


func _load_nakama_refs() -> Dictionary:
	# Try real Nakama addon first. Official addon has Nakama.gd at root, NakamaClient.gd in client/.
	var paths := [
		["res://addons/com.heroiclabs.nakama/Nakama.gd", "res://addons/com.heroiclabs.nakama/client/NakamaClient.gd"],
		["res://addons/nakama/Nakama.gd", "res://addons/nakama/client/NakamaClient.gd"],
	]
	for path_pair in paths:
		var nakama_script: GDScript = load(path_pair[0]) as GDScript
		var client_script: GDScript = load(path_pair[1]) as GDScript
		if nakama_script and client_script:
			# Official Nakama addon uses instance methods (create_client/create_socket_from need a node in the tree).
			var nakama_node: Node = nakama_script.new()
			nakama_node.name = "Nakama"
			add_child(nakama_node)
			var base_dir: String = path_pair[0].get_base_dir()
			var write_obj_script: GDScript = load(base_dir.path_join("api").path_join("NakamaWriteStorageObject.gd")) as GDScript
			var storage_id_script: GDScript = load(base_dir.path_join("api").path_join("NakamaStorageObjectId.gd")) as GDScript
			return {
				"nakama": nakama_script,
				"nakama_client": client_script,
				"nakama_node": nakama_node,
				"NakamaWriteStorageObject": write_obj_script,
				"NakamaStorageObjectId": storage_id_script
			}
	# If user installed the wrong addon (e.g. Asset Library #433 "Nakama Client in GDScript"), hint them.
	if DirAccess.dir_exists_absolute("res://addons/nakama-client"):
		push_warning("[IntelliVerseX] Found addons/nakama-client (Snopek). IntelliVerseX needs the official Heroic Labs client. Remove nakama-client and install addons/com.heroiclabs.nakama from https://github.com/heroiclabs/nakama-godot — see README.")
	# Fallback: built-in stub (project opens without Nakama addon).
	var stub: GDScript = load("res://addons/intelliversex/nakama_stub.gd") as GDScript
	if stub:
		return { "nakama": stub, "nakama_client": stub }
	return {}


func initialize(sdk_config: IVXConfig) -> void:
	config = sdk_config

	if config.game_id.strip_edges().is_empty():
		push_warning("[IntelliVerseX] gameId is empty. Get yours from https://intelli-verse-x.ai/developers")

	if _nakama_refs.is_empty():
		_nakama_refs = _load_nakama_refs()
	if _nakama_refs.is_empty():
		error.emit("IntelliVerseX: Could not load Nakama (install addons/com.heroiclabs.nakama from https://github.com/heroiclabs/nakama-godot for full backend).")
		return

	var nakama_script: GDScript = _nakama_refs.nakama
	if _nakama_refs.has("nakama_node"):
		nakama_client = _nakama_refs.nakama_node.create_client(config.nakama_server_key, config.nakama_host, config.nakama_port, config.nakama_scheme)
	else:
		nakama_client = nakama_script.call_static("create_client", config.nakama_server_key, config.nakama_host, config.nakama_port, config.nakama_scheme)

	is_initialized = true
	_log("SDK initialized — %s" % config.nakama_url)
	initialized.emit()


func authenticate_device(device_id: String = "") -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var resolved_id := device_id if device_id != "" else _get_persistent_device_id()

	var session: Variant = await nakama_client.authenticate_device_async(resolved_id, null, true)

	if session and session.is_exception():
		is_authenticating = false
		auth_error.emit(_format_network_error(session.get_exception().message))
		return

	_on_auth_success(session)


func authenticate_email(email: String, password: String, create: bool = false) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: Variant = await nakama_client.authenticate_email_async(email, password, null, create)

	if session and session.is_exception():
		is_authenticating = false
		auth_error.emit(_format_network_error(session.get_exception().message))
		return

	_on_auth_success(session)


func authenticate_google(token: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: Variant = await nakama_client.authenticate_google_async(token, null, true)

	if session and session.is_exception():
		is_authenticating = false
		auth_error.emit(_format_network_error(session.get_exception().message))
		return

	_on_auth_success(session)


func authenticate_apple(token: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: Variant = await nakama_client.authenticate_apple_async(token, null, true)

	if session and session.is_exception():
		is_authenticating = false
		auth_error.emit(_format_network_error(session.get_exception().message))
		return

	_on_auth_success(session)


func authenticate_custom(custom_id: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: Variant = await nakama_client.authenticate_custom_async(custom_id, null, true)

	if session and session.is_exception():
		is_authenticating = false
		auth_error.emit(_format_network_error(session.get_exception().message))
		return

	_on_auth_success(session)


func restore_session() -> bool:
	if _nakama_refs.is_empty():
		_nakama_refs = _load_nakama_refs()
	var token := _load_string(SESSION_TOKEN_KEY)
	var refresh := _load_string(REFRESH_TOKEN_KEY)

	if token == "":
		return false

	var client_script: GDScript = _nakama_refs.get("nakama_client", null)
	if not client_script:
		return false

	# Official NakamaClient.restore_session takes auth_token only.
	nakama_session = client_script.call_static("restore_session", token)

	if not nakama_session or nakama_session.is_expired():
		_log("Stored session expired, re-authenticating")
		nakama_session = null
		return false

	_log("Session restored for user: %s" % nakama_session.user_id)
	_sync_metadata()
	return true


func disconnect_socket() -> void:
	if nakama_socket:
		nakama_socket.close()
		nakama_socket = null
		_log("Socket disconnected")


func clear_session() -> void:
	disconnect_socket()
	nakama_session = null
	_save_string(SESSION_TOKEN_KEY, "")
	_save_string(REFRESH_TOKEN_KEY, "")
	_log("Session cleared")


func has_valid_session() -> bool:
	return nakama_session != null and not nakama_session.is_expired()


# --- Profile ---

func fetch_profile() -> Dictionary:
	if not has_valid_session():
		error.emit("No valid session")
		return {}

	var account: Variant = await nakama_client.get_account_async(nakama_session)
	if account and account.is_exception():
		error.emit(_format_network_error(account.get_exception().message))
		return {}

	var profile := {
		"user_id": account.user.id,
		"username": account.user.username,
		"display_name": account.user.display_name,
		"avatar_url": account.user.avatar_url,
		"lang_tag": account.user.lang_tag,
		"metadata": account.user.metadata,
		"wallet": account.wallet,
	}
	profile_loaded.emit(profile)
	return profile


func update_profile(display_name: String = "", avatar_url: String = "", lang_tag: String = "") -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var result = await nakama_client.update_account_async(nakama_session, null, display_name, avatar_url, lang_tag)
	if result and result.is_exception():
		error.emit(_format_network_error(result.get_exception().message))
		return false

	_log("Profile updated")
	return true


# --- Wallet ---

func fetch_wallet() -> Dictionary:
	var wallet := await call_rpc("hiro_economy_list", "{}")
	if not wallet.is_empty():
		wallet_updated.emit(wallet)
	return wallet


func grant_currency(currency_id: String, amount: int) -> Dictionary:
	var payload := JSON.stringify({"currencies": {currency_id: amount}})
	return await call_rpc("hiro_economy_grant", payload)


# --- Leaderboard ---

func submit_score(leaderboard_id: String, score: int) -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var result = await nakama_client.write_leaderboard_record_async(nakama_session, leaderboard_id, score)
	if result and result.is_exception():
		error.emit(_format_network_error(result.get_exception().message))
		return false

	_log("Score submitted: %d to %s" % [score, leaderboard_id])
	return true


func fetch_leaderboard(leaderboard_id: String, limit: int = 20) -> Array:
	if not has_valid_session():
		error.emit("No valid session")
		return []

	var result = await nakama_client.list_leaderboard_records_async(nakama_session, leaderboard_id, null, null, limit)
	if result and result.is_exception():
		error.emit(_format_network_error(result.get_exception().message))
		return []

	var records := []
	for record in result.records:
		records.append({
			"owner_id": record.owner_id,
			"username": record.username if record.username else "",
			"score": record.score,
			"rank": record.rank,
		})
	return records


# --- Storage ---

func _write_storage_object_new(collection: String, key: String, value: String, permission_write: String) -> Variant:
	if _nakama_refs.has("NakamaWriteStorageObject"):
		var Klass: GDScript = _nakama_refs.NakamaWriteStorageObject
		return Klass.new(collection, key, 0, 0, value, "")
	var nakama_script: GDScript = _nakama_refs.get("nakama", null)
	if nakama_script:
		return nakama_script.call_static("write_storage_object_new", collection, key, 1, 1, value, permission_write)
	return null


func _storage_object_id_new(collection: String, key: String, uid: String) -> Variant:
	if _nakama_refs.has("NakamaStorageObjectId"):
		var Klass: GDScript = _nakama_refs.NakamaStorageObjectId
		return Klass.new(collection, key, uid, "")
	var nakama_script: GDScript = _nakama_refs.get("nakama", null)
	if nakama_script:
		return nakama_script.call_static("storage_object_id_new", collection, key, uid)
	return null


func write_storage(collection: String, key: String, value: Dictionary) -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var write_obj := _write_storage_object_new(collection, key, JSON.stringify(value), "")
	var ack = await nakama_client.write_storage_objects_async(nakama_session, [write_obj])
	if ack and ack.is_exception():
		error.emit(_format_network_error(ack.get_exception().message))
		return false

	return true


func read_storage(collection: String, key: String) -> Dictionary:
	if not has_valid_session():
		error.emit("No valid session")
		return {}

	var storage_id := _storage_object_id_new(collection, key, user_id)
	var result = await nakama_client.read_storage_objects_async(nakama_session, [storage_id])
	if result and result.is_exception():
		error.emit(_format_network_error(result.get_exception().message))
		return {}

	if result.objects.size() > 0:
		var parsed = JSON.parse_string(result.objects[0].value)
		return parsed if parsed != null else {}
	return {}


# --- RPC ---

func call_rpc(rpc_id: String, payload: String = "{}") -> Dictionary:
	if not has_valid_session():
		error.emit("No valid session")
		return {}

	var result = await nakama_client.rpc_async(nakama_session, rpc_id, payload)
	if result and result.is_exception():
		error.emit(_format_network_error(result.get_exception().message))
		return {}

	_log("RPC %s response received" % rpc_id)
	if result.payload:
		var parsed = JSON.parse_string(result.payload)
		return parsed if parsed != null else {}
	return {}


# --- Socket / Real-time ---

func connect_socket() -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var nakama_script: GDScript = _nakama_refs.get("nakama", null)
	if not nakama_script:
		return false
	if _nakama_refs.has("nakama_node"):
		nakama_socket = _nakama_refs.nakama_node.create_socket_from(nakama_client)
	else:
		nakama_socket = nakama_script.call_static("create_socket_from", nakama_client)
	var connected: Variant = await nakama_socket.connect_async(nakama_session)
	if connected and connected.is_exception():
		error.emit("Socket connection failed")
		return false

	_log("Socket connected")
	return true


# --- Internal ---

func _on_auth_success(session: Variant) -> void:
	nakama_session = session
	is_authenticating = false
	_save_string(SESSION_TOKEN_KEY, session.token)
	_save_string(REFRESH_TOKEN_KEY, session.refresh_token)
	_log("Authenticated — UserId: %s" % session.user_id)
	_sync_metadata()
	auth_success.emit(session)


func _sync_metadata() -> void:
	if not has_valid_session():
		return
	var meta := {
		"sdk_version": SDK_VERSION,
		"platform": OS.get_name(),
		"engine": "godot",
		"engine_version": Engine.get_version_info().string,
	}
	var result = await nakama_client.rpc_async(nakama_session, "ivx_sync_metadata", JSON.stringify({"metadata": meta}))
	if result and result.is_exception():
		_log("Metadata sync failed (non-fatal)")


func _get_persistent_device_id() -> String:
	var id := _load_string(DEVICE_ID_KEY)
	if id == "":
		id = _generate_uuid()
		_save_string(DEVICE_ID_KEY, id)
	return id


func _generate_uuid() -> String:
	var rng := RandomNumberGenerator.new()
	rng.randomize()
	var parts: PackedStringArray = []
	for i in range(16):
		parts.append("%02x" % rng.randi_range(0, 255))
	return "-".join([
		"".join(parts.slice(0, 4)),
		"".join(parts.slice(4, 6)),
		"".join(parts.slice(6, 8)),
		"".join(parts.slice(8, 10)),
		"".join(parts.slice(10, 16)),
	])


func _save_string(key: String, value: String) -> void:
	var config_file := ConfigFile.new()
	var path := "user://intelliversex.cfg"
	var err := config_file.load(path)
	if err != OK and err != ERR_FILE_NOT_FOUND:
		_log("Warning: config load returned error %d" % err)
	config_file.set_value("session", key, value)
	config_file.save(path)


func _load_string(key: String) -> String:
	var config_file := ConfigFile.new()
	var path := "user://intelliversex.cfg"
	if config_file.load(path) == OK:
		return config_file.get_value("session", key, "")
	return ""


func _format_network_error(raw_message: String) -> String:
	if raw_message == "HTTPRequest failed!" and config != null:
		return "Could not connect to the server at %s. Is the Nakama server running? Check host, port, and firewall." % config.nakama_url
	return raw_message


func _log(msg: String) -> void:
	if config != null and config.enable_debug_logs:
		print("[IntelliVerseX] %s" % msg)
