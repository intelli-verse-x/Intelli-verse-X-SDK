# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXConfig
extends Resource

## Game ID (UUID) for this title on the IntelliVerseX platform. Obtain from https://intelli-verse-x.ai/developers or POST https://msapi.intelli-verse-x.io/api/games/game/info
@export_group("Game Identity")
@export var game_id: String = ""

@export_group("Nakama")
@export var nakama_host: String = "nakama-rest.intelli-verse-x.ai"
@export var nakama_port: int = 443
@export var nakama_server_key: String = "defaultkey"
@export var nakama_use_ssl: bool = true

@export_group("Identity")
@export var cognito_region: String = ""
@export var cognito_user_pool_id: String = ""
@export var cognito_client_id: String = ""

@export_group("Analytics")
@export var enable_analytics: bool = true

@export_group("Debug")
@export var enable_debug_logs: bool = false
@export var verbose_logging: bool = false

var nakama_scheme: String:
	get:
		return "https" if nakama_use_ssl else "http"

var nakama_url: String:
	get:
		return "%s://%s:%d" % [nakama_scheme, nakama_host, nakama_port]
