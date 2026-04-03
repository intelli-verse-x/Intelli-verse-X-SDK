class_name ProgressionBar
extends Control

@onready var _bar: ProgressBar = get_node_or_null("ProgressBar") as ProgressBar
@onready var _label: Label = get_node_or_null("Label") as Label

func _ready() -> void:
	if _bar:
		_bar.value = 0
	if _label:
		_label.text = "Level 1"
