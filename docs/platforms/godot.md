# Godot Engine

> IntelliVerseX addon for Godot 4, using GDScript with full async/await support.

## Requirements

- Godot 4.2+
- [Nakama Godot addon](https://github.com/heroiclabs/nakama-godot) v3.5+

## Installation

1. Install the [Nakama Godot addon](https://heroiclabs.com/docs/nakama/client-libraries/godot/)
2. Copy `addons/intelliversex/` into your project's `addons/` folder
3. Enable both plugins in **Project > Project Settings > Plugins**

## Quick Start

```gdscript
extends Node

func _ready() -> void:
    var config := IVXConfig.new()
    config.nakama_host = "127.0.0.1"
    config.nakama_port = 7350
    config.nakama_server_key = "defaultkey"
    config.enable_debug_logs = true

    IntelliVerseX.initialize(config)
    IntelliVerseX.auth_success.connect(_on_auth)

    if not IntelliVerseX.restore_session():
        IntelliVerseX.authenticate_device()

func _on_auth(session) -> void:
    print("Logged in: ", IntelliVerseX.username)
    var profile = await IntelliVerseX.fetch_profile()
    print("Profile: ", profile)
```

## Key Classes

| Class | Description |
|-------|-------------|
| `IVXConfig` | Resource for server configuration |
| `IntelliVerseX` (autoload) | Central singleton manager |

## Nakama Client

Built on [nakama-godot](https://github.com/heroiclabs/nakama-godot) (737 stars, 88 forks).

## Source

[SDKs/godot/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/godot)
