# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

## Stub used when the real Nakama addon is not installed.
## Allows the project to open and parse without errors.
## Install addons/com.heroiclabs.nakama from https://github.com/heroiclabs/nakama-godot for real backend.

class_name IVXNakamaStub

const STUB_MESSAGE := "Nakama addon not installed. Add addons/com.heroiclabs.nakama from https://github.com/heroiclabs/nakama-godot"

static func create_client(_server_key: String, _host: String, _port: int, _scheme: String) -> RefCounted:
	return StubClient.new()


static func restore_session(_token: String, _refresh: String = "") -> RefCounted:
	var s := StubSession.new()
	s._expired = true
	return s


static func create_socket_from(_client: RefCounted) -> RefCounted:
	return StubSocket.new()


static func write_storage_object_new(_collection: String, _key: String, _version: int, _version_read: int, _value: String, _permission_write: String) -> RefCounted:
	return RefCounted.new()


static func storage_object_id_new(_collection: String, _key: String, _user_id: String) -> RefCounted:
	return RefCounted.new()


class StubException:
	var message: String = "Nakama addon not installed. Add addons/com.heroiclabs.nakama from https://github.com/heroiclabs/nakama-godot"
	func is_exception() -> bool: return true
	func get_exception() -> RefCounted: return self


class StubSession:
	var token: String = ""
	var refresh_token: String = ""
	var user_id: String = ""
	var username: String = ""
	var _expired: bool = true
	func is_expired() -> bool: return _expired


class StubClient:
	func _stub_async_exception() -> RefCounted:
		return StubException.new()

	func authenticate_device_async(_id: String, _username, _create: bool):
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func authenticate_email_async(_email: String, _password: String, _username, _create: bool):
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func authenticate_google_async(_token: String, _username, _create: bool):
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func authenticate_apple_async(_token: String, _username, _create: bool):
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func authenticate_custom_async(_id: String, _username, _create: bool):
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func get_account_async(_session) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func update_account_async(_session, _username, _display_name: String, _avatar_url: String, _lang_tag: String) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func write_leaderboard_record_async(_session, _leaderboard_id: String, _score: int) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func list_leaderboard_records_async(_session, _leaderboard_id: String, _owner_ids, _cursor, _limit: int) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func write_storage_objects_async(_session, _objects: Array) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func read_storage_objects_async(_session, _object_ids: Array) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()

	func rpc_async(_session, _rpc_id: String, _payload: String) -> RefCounted:
		await Engine.get_main_loop().process_frame
		return _stub_async_exception()


class StubSocket:
	func close() -> void:
		pass
	func connect_async(_session):
		await Engine.get_main_loop().process_frame
		return StubException.new()
