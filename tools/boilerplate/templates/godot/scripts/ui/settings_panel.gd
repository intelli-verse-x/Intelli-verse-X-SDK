class_name SettingsPanel
extends Control

@onready var _music: HSlider = get_node_or_null("MusicSlider") as HSlider
@onready var _sfx: HSlider = get_node_or_null("SfxSlider") as HSlider
@onready var _notify: CheckButton = get_node_or_null("NotifyToggle") as CheckButton
@onready var _logout: Button = get_node_or_null("LogoutButton") as Button

var _ivx: Node

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _music:
		_music.value_changed.connect(func(v: float) -> void: _set_bus_volume("Music", v))
	if _sfx:
		_sfx.value_changed.connect(func(v: float) -> void: _set_bus_volume("SFX", v))
	if _notify:
		_notify.toggled.connect(_on_notify_toggled)
	if _logout:
		_logout.pressed.connect(_on_logout)


func _on_notify_toggled(enabled: bool) -> void:
	Analytics.track_event("notifications_toggle", {"enabled": enabled, "game_id": "{{game_id}}"})


func _set_bus_volume(bus_name: String, v: float) -> void:
	var idx := AudioServer.get_bus_index(bus_name)
	if idx >= 0:
		AudioServer.set_bus_volume_db(idx, linear_to_db(v / 100.0))


func _on_logout() -> void:
	if _ivx:
		_ivx.clear_session()
	Analytics.track_event("logout", {"game_id": "{{game_id}}"})
	get_tree().change_scene_to_file("res://scenes/auth.tscn")
