extends Node

## IntelliVerseX SDK Manager — central coordinator for Nakama integration.
## Auto-loaded as the "IntelliVerseX" singleton.

signal initialized
signal auth_success(session)
signal auth_error(message: String)
signal profile_loaded(profile: Dictionary)
signal wallet_updated(wallet: Dictionary)
signal error(message: String)

const SDK_VERSION := "5.1.0"
const SESSION_TOKEN_KEY := "ivx_session_token"
const REFRESH_TOKEN_KEY := "ivx_refresh_token"
const DEVICE_ID_KEY := "ivx_device_id"

var config: IVXConfig
var nakama_client: NakamaClient
var nakama_session: NakamaSession
var nakama_socket: NakamaSocket

var is_initialized: bool = false
var is_authenticating: bool = false

var user_id: String:
	get:
		return nakama_session.user_id if nakama_session else ""

var username: String:
	get:
		return nakama_session.username if nakama_session else ""


func initialize(sdk_config: IVXConfig) -> void:
	config = sdk_config

	nakama_client = Nakama.create_client(
		config.nakama_server_key,
		config.nakama_host,
		config.nakama_port,
		config.nakama_scheme
	)

	is_initialized = true
	_log("SDK initialized — %s" % config.nakama_url)
	initialized.emit()


func authenticate_device(device_id: String = "") -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var resolved_id := device_id if device_id != "" else _get_persistent_device_id()

	var session: NakamaSession = await nakama_client.authenticate_device_async(resolved_id, null, true)

	if session.is_exception():
		is_authenticating = false
		auth_error.emit(session.get_exception().message)
		return

	_on_auth_success(session)


func authenticate_email(email: String, password: String, create: bool = false) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: NakamaSession = await nakama_client.authenticate_email_async(email, password, null, create)

	if session.is_exception():
		is_authenticating = false
		auth_error.emit(session.get_exception().message)
		return

	_on_auth_success(session)


func authenticate_google(token: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: NakamaSession = await nakama_client.authenticate_google_async(token, null, true)

	if session.is_exception():
		is_authenticating = false
		auth_error.emit(session.get_exception().message)
		return

	_on_auth_success(session)


func authenticate_apple(token: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: NakamaSession = await nakama_client.authenticate_apple_async(token, null, true)

	if session.is_exception():
		is_authenticating = false
		auth_error.emit(session.get_exception().message)
		return

	_on_auth_success(session)


func authenticate_custom(custom_id: String) -> void:
	if not is_initialized:
		error.emit("SDK not initialized")
		return

	is_authenticating = true
	var session: NakamaSession = await nakama_client.authenticate_custom_async(custom_id, null, true)

	if session.is_exception():
		is_authenticating = false
		auth_error.emit(session.get_exception().message)
		return

	_on_auth_success(session)


func restore_session() -> bool:
	var token := _load_string(SESSION_TOKEN_KEY)
	var refresh := _load_string(REFRESH_TOKEN_KEY)

	if token == "":
		return false

	nakama_session = NakamaClient.restore_session(token, refresh)

	if nakama_session.is_expired():
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

	var account: NakamaAPI.ApiAccount = await nakama_client.get_account_async(nakama_session)
	if account.is_exception():
		error.emit(account.get_exception().message)
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
	if result.is_exception():
		error.emit(result.get_exception().message)
		return false

	_log("Profile updated")
	return true


# --- Wallet ---

func fetch_wallet() -> Dictionary:
	return await call_rpc("hiro_economy_list", "{}")


func grant_currency(currency_id: String, amount: int) -> Dictionary:
	var payload := JSON.stringify({"currencies": {currency_id: amount}})
	return await call_rpc("hiro_economy_grant", payload)


# --- Leaderboard ---

func submit_score(leaderboard_id: String, score: int) -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var result = await nakama_client.write_leaderboard_record_async(nakama_session, leaderboard_id, score)
	if result.is_exception():
		error.emit(result.get_exception().message)
		return false

	_log("Score submitted: %d to %s" % [score, leaderboard_id])
	return true


func fetch_leaderboard(leaderboard_id: String, limit: int = 20) -> Array:
	if not has_valid_session():
		error.emit("No valid session")
		return []

	var result = await nakama_client.list_leaderboard_records_async(nakama_session, leaderboard_id, null, null, limit)
	if result.is_exception():
		error.emit(result.get_exception().message)
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

func write_storage(collection: String, key: String, value: Dictionary) -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	var ack = await nakama_client.write_storage_objects_async(nakama_session, [
		NakamaWriteStorageObject.new(collection, key, 1, 1, JSON.stringify(value), "")
	])
	if ack.is_exception():
		error.emit(ack.get_exception().message)
		return false

	return true


func read_storage(collection: String, key: String) -> Dictionary:
	if not has_valid_session():
		error.emit("No valid session")
		return {}

	var result = await nakama_client.read_storage_objects_async(nakama_session, [
		NakamaStorageObjectId.new(collection, key, user_id)
	])
	if result.is_exception():
		error.emit(result.get_exception().message)
		return {}

	if result.objects.size() > 0:
		return JSON.parse_string(result.objects[0].value)
	return {}


# --- RPC ---

func call_rpc(rpc_id: String, payload: String = "{}") -> Dictionary:
	if not has_valid_session():
		error.emit("No valid session")
		return {}

	var result = await nakama_client.rpc_async(nakama_session, rpc_id, payload)
	if result.is_exception():
		error.emit(result.get_exception().message)
		return {}

	_log("RPC %s response received" % rpc_id)
	if result.payload:
		return JSON.parse_string(result.payload)
	return {}


# --- Socket / Real-time ---

func connect_socket() -> bool:
	if not has_valid_session():
		error.emit("No valid session")
		return false

	nakama_socket = Nakama.create_socket_from(nakama_client)
	var connected: NakamaAsyncResult = await nakama_socket.connect_async(nakama_session)
	if connected.is_exception():
		error.emit("Socket connection failed")
		return false

	_log("Socket connected")
	return true


# --- Internal ---

func _on_auth_success(session: NakamaSession) -> void:
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
	if result.is_exception():
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


func _log(msg: String) -> void:
	if config != null and config.enable_debug_logs:
		print("[IntelliVerseX] %s" % msg)
