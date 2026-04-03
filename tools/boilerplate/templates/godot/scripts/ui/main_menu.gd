class_name MainMenu
extends Control

var _tabs: TabContainer

func _ready() -> void:
	_tabs = find_child("MainTabs", true, false) as TabContainer
	var accent := Color.html("{{secondary_color}}")
	if _tabs:
		_tabs.set("theme_override_colors/font_selected_color", accent)
	var tag := find_child("Tagline", true, false) as Label
	if tag:
		tag.text = "{{tagline}}"
		tag.add_theme_color_override("font_color", Color.html("{{primary_color}}"))
	if _tabs:
		_tabs.tab_changed.connect(_on_tab_changed)


func _on_tab_changed(idx: int) -> void:
	var names := ["home", "store", "achievements", "daily", "leaderboard", "settings"]
	if idx >= 0 and idx < names.size():
		Analytics.track_event("main_menu_tab", {"tab": names[idx], "game_id": "{{game_id}}"})
