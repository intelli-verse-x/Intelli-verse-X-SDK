class_name AuthFlow
extends Control

var _guest_btn: Button
var _email_btn: Button
var _social_btn: Button

var _ivx: Node

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	_guest_btn = find_child("GuestLogin", true, false) as Button
	_email_btn = find_child("EmailLogin", true, false) as Button
	_social_btn = find_child("SocialLogin", true, false) as Button
	var accent := Color.html("{{primary_color}}")
	var bg := Color.html("{{background_color}}")
	self_modulate = Color.WHITE
	var title := find_child("Title", true, false) as Label
	if title:
		title.add_theme_color_override("font_color", accent)
	var panel := find_child("Panel", true, false) as Control
	if panel:
		panel.self_modulate = bg
	if _guest_btn:
		_guest_btn.pressed.connect(_on_guest)
	if _email_btn:
		_email_btn.pressed.connect(_on_email)
	if _social_btn:
		_social_btn.pressed.connect(_on_social)
	if _ivx:
		_ivx.auth_success.connect(_on_auth_ok, CONNECT_ONE_SHOT)


func _on_guest() -> void:
	if _ivx:
		await _ivx.authenticate_device()


func _on_email() -> void:
	if _ivx:
		await _ivx.authenticate_email("player@example.com", "changeme", true)


func _on_social() -> void:
	if _ivx:
		await _ivx.authenticate_google("")


func _on_auth_ok(_session) -> void:
	get_tree().change_scene_to_file("res://scenes/main_menu.tscn")
