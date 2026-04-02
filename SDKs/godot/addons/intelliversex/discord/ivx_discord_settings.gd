# Copyright (c) 2026 Intelli-verse-X — MIT License

## Discord Social Settings — notification preferences, privacy, DND mode.
## Stub: API shape matches Unity IVXDiscordSettings.
extends RefCounted

var notifications_enabled: bool = true
var friend_requests_enabled: bool = true
var do_not_disturb: bool = false
var show_online_status: bool = true
var allow_direct_messages: bool = true

func enable_do_not_disturb() -> void:
	do_not_disturb = true

func disable_do_not_disturb() -> void:
	do_not_disturb = false

func reset_to_defaults() -> void:
	notifications_enabled = true
	friend_requests_enabled = true
	do_not_disturb = false
	show_online_status = true
	allow_direct_messages = true
