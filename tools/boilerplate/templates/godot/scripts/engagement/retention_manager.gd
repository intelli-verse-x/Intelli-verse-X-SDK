class_name RetentionManager
extends Node

var _ivx: Node
var _hiro: IVXHiroSystems

func _ready() -> void:
	Analytics.track_event("session_start", {"game_id": "{{game_id}}", "engine": "godot"})
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _ivx == null or _ivx.nakama_client == null or _ivx.nakama_session == null:
		return
	_hiro = IVXHiroSystems.new()
	add_child(_hiro)
	_hiro.initialize(_ivx.nakama_client, _ivx.nakama_session)
	var state: Dictionary = await _hiro.retention_get()
	if bool(state.get("reward_available", state.get("can_claim", false))):
		_show_daily_popup()
	_schedule_return_ping()


func _show_daily_popup() -> void:
	var dlg := AcceptDialog.new()
	dlg.title = "{{game_name}}"
	dlg.dialog_text = "{{tagline}}\n\nDaily reward is ready!"
	get_tree().root.add_child(dlg)
	dlg.popup_centered()
	dlg.confirmed.connect(dlg.queue_free)


func _schedule_return_ping() -> void:
	# Hook your OS notification / FCM / local scheduler here (platform plugins).
	if OS.has_feature("mobile"):
		push_warning("RetentionManager: wire mobile local notifications for return reminders.")
	Analytics.track_event("return_notification_scheduled", {"game_id": "{{game_id}}"})
