class_name IVXManager
extends Node

signal auth_success(session)

var config: Node
var satori: Node
var hiro: Node

func _ready() -> void:
	config = load("res://addons/intelliversex/core/ivx_config.gd").new()
	add_child(config)
	satori = load("res://addons/intelliversex/core/ivx_satori.gd").new()
	add_child(satori)
	hiro = load("res://addons/intelliversex/core/ivx_hiro_systems.gd").new()
	add_child(hiro)

func clear_session() -> void:
	pass

func authenticate_device() -> void:
	await get_tree().create_timer(0.5).timeout
	auth_success.emit(null)

func authenticate_email(email: String, passw: String, create: bool) -> void:
	await get_tree().create_timer(0.5).timeout
	auth_success.emit(null)

func authenticate_google(token: String) -> void:
	await get_tree().create_timer(0.5).timeout
	auth_success.emit(null)
