extends Node

## Satori-oriented analytics facade (IVXSatori). Wire capture_events to SatoriClient when available.
var _satori: IVXSatori = IVXSatori.new()

func _ready() -> void:
	_satori.initialize({
		"game_id": "{{game_id}}",
		"host": "{{server_host}}",
		"port": int("{{server_port}}"),
		"tagline": "{{tagline}}",
	})
	track_event("app_launch", {
		"game_id": "{{game_id}}",
		"engine": "godot",
		"primary_color": "{{primary_color}}",
		"secondary_color": "{{secondary_color}}",
		"background_color": "{{background_color}}",
	})


func track_event(name: String, metadata: Dictionary = {}) -> void:
	var ev := IVXSatori.Event.new()
	ev.name = name
	ev.properties = metadata.duplicate()
	ev.properties["game_id"] = "{{game_id}}"
	_satori.capture_events([ev])
