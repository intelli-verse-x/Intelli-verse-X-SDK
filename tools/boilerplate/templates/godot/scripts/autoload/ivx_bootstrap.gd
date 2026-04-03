extends Node

## Game bootstrap: applies IVXConfig to the IntelliVerseX autoload, initializes the SDK, restores or guest-auth.
signal ready

var sdk: Node

func _ready() -> void:
	sdk = get_node_or_null("/root/IntelliVerseX")
	if sdk == null:
		push_error("IVXBootstrap: IntelliVerseX autoload missing. Enable the IntelliVerseX addon.")
		ready.emit()
		return
	var cfg := IVXConfig.new()
	cfg.game_id = "{{game_id}}"
	cfg.nakama_host = "{{server_host}}"
	cfg.nakama_port = int("{{server_port}}")
	cfg.nakama_server_key = "{{server_key}}"
	cfg.nakama_use_ssl = cfg.nakama_port == 443
	cfg.enable_debug_logs = true
	sdk.initialized.connect(_on_sdk_initialized, CONNECT_ONE_SHOT)
	sdk.initialize(cfg)


func _on_sdk_initialized() -> void:
	if sdk.restore_session():
		ready.emit()
		return
	await sdk.authenticate_device()
	ready.emit()
