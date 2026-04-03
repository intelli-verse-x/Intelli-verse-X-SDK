class_name EnergyBar
extends Control

const MAX_ENERGY := {{max_energy}}

@onready var _bar: ProgressBar = get_node_or_null("EnergyProgress") as ProgressBar
@onready var _countdown: Label = get_node_or_null("RefillTimerLabel") as Label

var _ivx: Node
var _timer: Timer

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _bar:
		_bar.max_value = float(MAX_ENERGY)
		_bar.value = float(MAX_ENERGY)
	_timer = Timer.new()
	_timer.wait_time = 1.0
	_timer.timeout.connect(_poll_energy)
	add_child(_timer)
	_timer.start()
	await _poll_energy()


func _poll_energy() -> void:
	if _ivx == null:
		return
	var d: Dictionary = await _ivx.call_rpc("hiro_energy_get", "{}")
	var cur: int = int(d.get("current", d.get("energy", MAX_ENERGY)))
	var mx: int = int(d.get("max", MAX_ENERGY))
	if _bar:
		_bar.max_value = float(mx)
		_bar.value = float(clampi(cur, 0, mx))
	var sec: int = int(d.get("seconds_to_refill", d.get("refill_in", 0)))
	if _countdown:
		_countdown.text = "%02d:%02d" % [sec / 60, sec % 60]
