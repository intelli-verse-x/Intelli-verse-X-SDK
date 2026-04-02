# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXAIClient
extends Node

## IntelliVerseX AI Client — voice sessions, text chat, host AI, and entitlement checks.
## Auto-load as a singleton or access via IVXManager.

signal voice_session_started(session_id: String)
signal message_received(session_id: String, message: Dictionary)
signal host_message_received(session_id: String, message: Dictionary)
signal entitlement_changed(user_id: String, entitled: bool)

var _api_base_url: String = ""
var _api_key: String = ""
var _http: HTTPRequest
var _pending_requests: Dictionary = {}
var _request_id: int = 0


func _ready() -> void:
	_http = HTTPRequest.new()
	_http.name = "HTTPRequest"
	add_child(_http)
	_http.request_completed.connect(_on_request_completed)


## Configure the AI client with a base URL and optional API key.
func initialize(api_base_url: String, api_key: String = "") -> void:
	_api_base_url = api_base_url.rstrip("/")
	_api_key = api_key
	print("[IVXAIClient] Initialized — %s" % _api_base_url)


## Start a voice session for a persona.
func start_voice_session(persona_id: String, user_id: String) -> Dictionary:
	var body := {"persona_id": persona_id, "user_id": user_id}
	var result := await _post("/v1/voice/sessions", body)
	if result.has("session_id"):
		voice_session_started.emit(result["session_id"])
	return result


## End an active voice session.
func end_voice_session(session_id: String) -> Dictionary:
	return await _post("/v1/voice/sessions/%s/end" % session_id, {})


## Send a text message within a session.
func send_text(session_id: String, text: String) -> Dictionary:
	var body := {"text": text}
	var result := await _post("/v1/voice/sessions/%s/text" % session_id, body)
	if result.size() > 0:
		message_received.emit(session_id, result)
	return result


## Poll for new messages in a session.
func poll_messages(session_id: String) -> Array:
	var result := await _get("/v1/voice/sessions/%s/messages" % session_id)
	if result.has("messages"):
		return result["messages"]
	return []


## Start an AI host session for a match.
func start_host_session(match_id: String, profile: Dictionary) -> Dictionary:
	var body := {"match_id": match_id, "profile": profile}
	return await _post("/v1/host/sessions", body)


## Send an event to the AI host.
func send_host_event(session_id: String, event_type: String, data: String) -> Dictionary:
	var body := {"event_type": event_type, "data": data}
	var result := await _post("/v1/host/sessions/%s/events" % session_id, body)
	if result.size() > 0:
		host_message_received.emit(session_id, result)
	return result


## Check whether a user has AI entitlement.
func check_entitlement(user_id: String) -> Dictionary:
	var result := await _get("/v1/entitlements/%s" % user_id)
	if result.has("entitled"):
		entitlement_changed.emit(user_id, result["entitled"])
	return result


## Retrieve the list of available AI personas.
func get_personas() -> Array:
	var result := await _get("/v1/personas")
	if result.has("personas"):
		return result["personas"]
	return []


# ── Private helpers ──────────────────────────────────────────────────────────

func _build_headers() -> PackedStringArray:
	var headers := PackedStringArray(["Content-Type: application/json"])
	if _api_key != "":
		headers.append("Authorization: Bearer %s" % _api_key)
	return headers


func _post(path: String, body: Dictionary) -> Dictionary:
	var url := _api_base_url + path
	var json_body := JSON.stringify(body)
	var rid := _next_request_id()
	_pending_requests[rid] = null

	var http := HTTPRequest.new()
	http.name = "Req_%d" % rid
	add_child(http)
	http.request_completed.connect(_on_oneshot_completed.bind(rid, http))
	var err := http.request(url, _build_headers(), HTTPClient.METHOD_POST, json_body)
	if err != OK:
		http.queue_free()
		push_warning("[IVXAIClient] POST request failed to start: %s" % url)
		return {"error": "request_failed", "code": err}

	while _pending_requests.get(rid) == null:
		await get_tree().process_frame
	var result: Dictionary = _pending_requests[rid]
	_pending_requests.erase(rid)
	return result


func _get(path: String) -> Dictionary:
	var url := _api_base_url + path
	var rid := _next_request_id()
	_pending_requests[rid] = null

	var http := HTTPRequest.new()
	http.name = "Req_%d" % rid
	add_child(http)
	http.request_completed.connect(_on_oneshot_completed.bind(rid, http))
	var err := http.request(url, _build_headers(), HTTPClient.METHOD_GET)
	if err != OK:
		http.queue_free()
		push_warning("[IVXAIClient] GET request failed to start: %s" % url)
		return {"error": "request_failed", "code": err}

	while _pending_requests.get(rid) == null:
		await get_tree().process_frame
	var result: Dictionary = _pending_requests[rid]
	_pending_requests.erase(rid)
	return result


func _on_oneshot_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray, rid: int, http_node: HTTPRequest) -> void:
	http_node.queue_free()
	if result != HTTPRequest.RESULT_SUCCESS:
		_pending_requests[rid] = {"error": "http_error", "result": result}
		return
	var json := JSON.new()
	if json.parse(body.get_string_from_utf8()) == OK:
		_pending_requests[rid] = json.data if json.data is Dictionary else {"data": json.data}
	else:
		_pending_requests[rid] = {"error": "parse_error", "response_code": response_code}


func _on_request_completed(_result: int, _response_code: int, _headers: PackedStringArray, _body: PackedByteArray) -> void:
	pass


func _next_request_id() -> int:
	_request_id += 1
	return _request_id
