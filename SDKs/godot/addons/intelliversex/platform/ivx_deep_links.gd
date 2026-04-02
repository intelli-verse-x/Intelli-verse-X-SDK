## Lightweight deep link parser and dispatcher.
##
## Parses URLs in the format [code]{scheme}://{host}/{route}?key=value[/code]
## and fires registered handlers for matching routes.
class_name IVXDeepLinks
extends Node

signal deep_link_received(route: String, params: Dictionary)

var _scheme: String = ""
var _host: String = ""
var _initialized: bool = false
var _handlers: Dictionary = {}


## Configure the expected scheme and host.
func initialize(scheme: String, host: String) -> void:
	_scheme = scheme
	_host = host
	_initialized = true


## Parse [param url] and dispatch to registered handlers.
## Returns a Dictionary with keys: matched, scheme, host, route, params, raw.
func handle_url(url: String) -> Dictionary:
	var result := _parse(url)
	if result["matched"]:
		_dispatch(result)
		deep_link_received.emit(result["route"], result["params"])
	return result


## Register a [param callback] for a specific [param route].
func register_handler(route: String, callback: Callable) -> void:
	if not _handlers.has(route):
		_handlers[route] = []
	var list: Array = _handlers[route]
	if not list.has(callback):
		list.append(callback)


## Remove a previously registered [param callback] from a [param route].
func remove_handler(route: String, callback: Callable) -> void:
	if _handlers.has(route):
		var list: Array = _handlers[route]
		list.erase(callback)


## Remove all handlers, or only those for a specific [param route].
func remove_all_handlers(route: String = "") -> void:
	if route != "":
		_handlers.erase(route)
	else:
		_handlers.clear()


## Whether [method initialize] has been called.
func is_initialized() -> bool:
	return _initialized


func _parse(url: String) -> Dictionary:
	var empty := {
		"matched": false,
		"scheme": "",
		"host": "",
		"route": "",
		"params": {},
		"raw": url,
	}

	var scheme_end := url.find("://")
	if scheme_end == -1:
		return empty

	var scheme := url.substr(0, scheme_end)
	var rest := url.substr(scheme_end + 3)

	var path_start := rest.find("/")
	var host := rest if path_start == -1 else rest.substr(0, path_start)

	if _initialized and (scheme != _scheme or host != _host):
		return empty

	var path_and_query := "" if path_start == -1 else rest.substr(path_start + 1)
	var query_start := path_and_query.find("?")
	var route := path_and_query if query_start == -1 else path_and_query.substr(0, query_start)
	var query_string := "" if query_start == -1 else path_and_query.substr(query_start + 1)

	var params := {}
	if query_string != "":
		for pair in query_string.split("&"):
			var eq_idx := pair.find("=")
			if eq_idx == -1:
				params[pair.uri_decode()] = ""
			else:
				params[pair.substr(0, eq_idx).uri_decode()] = pair.substr(eq_idx + 1).uri_decode()

	return {
		"matched": true,
		"scheme": scheme,
		"host": host,
		"route": route,
		"params": params,
		"raw": url,
	}


func _dispatch(result: Dictionary) -> void:
	var route: String = result["route"]
	if not _handlers.has(route):
		return
	var list: Array = _handlers[route]
	for callback: Callable in list:
		callback.call(result["params"], result)
