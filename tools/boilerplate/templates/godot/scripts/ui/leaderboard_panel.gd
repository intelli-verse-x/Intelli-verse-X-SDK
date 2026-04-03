class_name LeaderboardPanel
extends Control

## IVXLeaderboards-style UI over IntelliVerseX.fetch_leaderboard.

const BOARD_GLOBAL := "global_leaderboard"

@onready var _tabs: TabBar = get_node_or_null("ScopeTabs") as TabBar
@onready var _list: VBoxContainer = get_node_or_null("RankList") as VBoxContainer

var _ivx: Node
var _scope := "global"

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _tabs:
		_tabs.tab_selected.connect(_on_scope_changed)
	await _load_rows()


func _on_scope_changed(idx: int) -> void:
	_scope = "friends" if idx == 1 else "global"
	await _load_rows()


func _load_rows() -> void:
	if _list == null or _ivx == null:
		return
	for c in _list.get_children():
		c.queue_free()
	var board_id := BOARD_GLOBAL if _scope == "global" else "friends_leaderboard"
	var rows: Array = await _ivx.fetch_leaderboard(board_id, 20)
	if rows.is_empty() and _scope == "friends":
		rows = await _ivx.fetch_leaderboard(BOARD_GLOBAL, 20)
	for r in rows:
		var row := HBoxContainer.new()
		var rank := Label.new()
		rank.text = str(r.get("rank", "?"))
		var name_l := Label.new()
		name_l.text = str(r.get("username", r.get("owner_id", "?")))
		var score := Label.new()
		score.text = str(r.get("score", 0))
		row.add_child(rank)
		row.add_child(name_l)
		row.add_child(score)
		_list.add_child(row)
