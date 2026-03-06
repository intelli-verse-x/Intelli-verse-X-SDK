class_name IVXConfig
extends Resource

@export_group("Nakama")
@export var nakama_host: String = "127.0.0.1"
@export var nakama_port: int = 7350
@export var nakama_server_key: String = "defaultkey"
@export var nakama_use_ssl: bool = false

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
