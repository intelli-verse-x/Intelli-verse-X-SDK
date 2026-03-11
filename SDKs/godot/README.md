# IntelliVerseX Godot Engine SDK

> Complete modular game development SDK for Godot — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- Godot 4.2+
- [Nakama Godot addon](https://github.com/heroiclabs/nakama-godot) v3.5+

## Installation

1. Install the [Nakama Godot addon](https://heroiclabs.com/docs/nakama/client-libraries/godot/) into your project's `addons/` folder
2. Copy `addons/intelliversex/` into your project's `addons/` folder
3. Enable both plugins in **Project > Project Settings > Plugins**

## Quick Start

```gdscript
extends Node

var config: IVXConfig

func _ready() -> void:
    config = IVXConfig.new()
    config.nakama_host = "127.0.0.1"
    config.nakama_port = 7350
    config.nakama_server_key = "defaultkey"
    config.enable_debug_logs = true

    IntelliVerseX.initialized.connect(_on_initialized)
    IntelliVerseX.auth_success.connect(_on_auth_success)
    IntelliVerseX.error.connect(_on_error)

    IntelliVerseX.initialize(config)

    if not IntelliVerseX.restore_session():
        IntelliVerseX.authenticate_device()


func _on_initialized() -> void:
    print("IntelliVerseX SDK ready!")


func _on_auth_success(session) -> void:
    print("Logged in as: ", IntelliVerseX.username)

    var profile = await IntelliVerseX.fetch_profile()
    print("Profile: ", profile)

    var wallet = await IntelliVerseX.fetch_wallet()
    print("Wallet: ", wallet)


func _on_error(message: String) -> void:
    push_error("IntelliVerseX error: " + message)
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| Profile Management | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Real-time Socket | Supported |
| Hiro Systems | Via RPC |
| Analytics | Planned |
| Monetization | Planned |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/godot/).

## Nakama Client Library

This SDK wraps the official [Nakama Godot Client](https://github.com/heroiclabs/nakama-godot) (737 stars, 88 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
