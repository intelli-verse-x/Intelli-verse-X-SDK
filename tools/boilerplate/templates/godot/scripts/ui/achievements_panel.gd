class_name AchievementsPanel
extends Control

@onready var _list: VBoxContainer = get_node_or_null("AchievementList") as VBoxContainer

var _ivx: Node

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _ivx:
		await _reload()


func _reload() -> void:
	if _list == null:
		return
	for c in _list.get_children():
		c.queue_free()
	var data: Dictionary = await _ivx.call_rpc("hiro_achievements_list", "{}")
	var rows: Array = data.get("achievements", data.get("list", []))
	for a in rows:
		_list.add_child(_make_row(a))


func _make_row(a: Dictionary) -> HBoxContainer:
	var row := HBoxContainer.new()
	var title := Label.new()
	title.text = str(a.get("title", a.get("id", "?")))
	var bar := ProgressBar.new()
	bar.max_value = 100.0
	bar.value = float(a.get("progress", 0))
	var claim := Button.new()
	claim.text = "Claim"
	claim.disabled = not bool(a.get("claimable", false))
	var id := str(a.get("id", ""))
	claim.pressed.connect(_claim.bind(id))
	row.add_child(title)
	row.add_child(bar)
	row.add_child(claim)
	return row


func _claim(ach_id: String) -> void:
	if _ivx == null:
		return
	await _ivx.call_rpc("hiro_achievements_claim", JSON.stringify({"id": ach_id}))
	Analytics.track_event("achievement_claimed", {"id": ach_id, "game_id": "{{game_id}}"})
	await _reload()
