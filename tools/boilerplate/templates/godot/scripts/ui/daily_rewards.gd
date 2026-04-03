class_name DailyRewardsPanel
extends Control

## IVXStreaks-style rewards: IVXHiroSystems streak RPCs (daily_login streak / milestones).
const DAY_COUNT := 7

@onready var _row: HBoxContainer = get_node_or_null("DaySlots") as HBoxContainer
@onready var _streak_lbl: Label = get_node_or_null("StreakLabel") as Label

var _ivx: Node
var _hiro: IVXHiroSystems

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _row:
		for d in range(DAY_COUNT):
			var b := Button.new()
			b.text = "D%d" % (d + 1)
			var day := d + 1
			b.pressed.connect(func() -> void: _claim_day(day))
			_row.add_child(b)
	if _ivx and _ivx.nakama_client and _ivx.nakama_session:
		_hiro = IVXHiroSystems.new()
		add_child(_hiro)
		_hiro.initialize(_ivx.nakama_client, _ivx.nakama_session)
		await _refresh()


func _refresh() -> void:
	if _hiro == null:
		return
	var state: Dictionary = await _hiro.streaks_get()
	var streak: int = int(state.get("current_streak", state.get("streak", 0)))
	if _streak_lbl:
		_streak_lbl.text = "Streak: %d" % streak
	if _row:
		for i in range(mini(_row.get_child_count(), DAY_COUNT)):
			var b := _row.get_child(i) as BaseButton
			if b:
				b.disabled = (i + 1) > streak


func _claim_day(day: int) -> void:
	if _hiro == null:
		return
	await _hiro.streaks_claim("daily_login", str(day))
	Analytics.track_event("daily_reward_claimed", {"day": day, "game_id": "{{game_id}}"})
	await _refresh()
