class_name IVXXRHelper
extends Node

## XR platform detection and capability queries for Godot 4.
## Supports Meta Quest, SteamVR, Apple Vision Pro, PSVR2, Pico, and AR Foundation.

enum XRPlatform {
	NONE,
	META_QUEST,
	STEAMVR,
	APPLE_VISION_PRO,
	PSVR2,
	PICO,
	GENERIC_OPENXR,
	AR_FOUNDATION
}

enum TrackingState { NOT_TRACKING, LIMITED, FULL }

signal xr_platform_detected(platform: XRPlatform)
signal tracking_state_changed(state: TrackingState)
signal xr_focus_changed(has_focus: bool)

var active_platform: XRPlatform = XRPlatform.NONE
var is_xr_active: bool = false
var tracking_state: TrackingState = TrackingState.NOT_TRACKING
var hand_tracking_available: bool = false
var eye_tracking_available: bool = false
var passthrough_available: bool = false

var _xr_interface: XRInterface = null

func _ready() -> void:
	detect_xr_platform()

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_FOCUS_IN:
		xr_focus_changed.emit(true)
		if is_xr_active:
			_refresh_capabilities()
	elif what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		xr_focus_changed.emit(false)

func detect_xr_platform() -> XRPlatform:
	active_platform = XRPlatform.NONE
	is_xr_active = false
	hand_tracking_available = false
	eye_tracking_available = false
	passthrough_available = false

	_xr_interface = XRServer.find_interface("OpenXR")
	if _xr_interface == null:
		_xr_interface = XRServer.find_interface("native")
	if _xr_interface == null:
		_xr_interface = XRServer.find_interface("WebXR")
	if _xr_interface == null:
		push_warning("[IVXXRHelper] No XR interface found")
		return active_platform

	if not _xr_interface.is_initialized():
		if not _xr_interface.initialize():
			push_warning("[IVXXRHelper] Failed to initialize XR interface: %s" % _xr_interface.get_name())
			return active_platform

	is_xr_active = true
	var iface_name := _xr_interface.get_name()

	if "Quest" in iface_name or "Meta" in iface_name or "Oculus" in iface_name:
		active_platform = XRPlatform.META_QUEST
	elif "SteamVR" in iface_name:
		active_platform = XRPlatform.STEAMVR
	elif "Apple" in iface_name or "Vision" in iface_name:
		active_platform = XRPlatform.APPLE_VISION_PRO
	elif "PSVR" in iface_name or "PlayStation" in iface_name:
		active_platform = XRPlatform.PSVR2
	elif "Pico" in iface_name:
		active_platform = XRPlatform.PICO
	elif "WebXR" in iface_name:
		active_platform = XRPlatform.GENERIC_OPENXR
	else:
		active_platform = XRPlatform.GENERIC_OPENXR

	tracking_state = TrackingState.FULL
	_refresh_capabilities()

	push_warning("[IVXXRHelper] Detected XR platform: %s" % XRPlatform.keys()[active_platform])
	xr_platform_detected.emit(active_platform)
	tracking_state_changed.emit(tracking_state)
	return active_platform

func get_recommended_settings() -> Dictionary:
	match active_platform:
		XRPlatform.META_QUEST:
			return {
				"ui_scale": 0.001,
				"use_world_space_ui": true,
				"prefer_hand_tracking": true,
				"target_fps": 72,
				"render_scale": 1.0,
			}
		XRPlatform.APPLE_VISION_PRO:
			return {
				"ui_scale": 0.001,
				"use_world_space_ui": true,
				"prefer_hand_tracking": true,
				"target_fps": 90,
				"render_scale": 1.2,
			}
		XRPlatform.STEAMVR:
			return {
				"ui_scale": 0.001,
				"use_world_space_ui": true,
				"prefer_hand_tracking": false,
				"target_fps": 90,
				"render_scale": 1.0,
			}
		XRPlatform.PSVR2:
			return {
				"ui_scale": 0.001,
				"use_world_space_ui": true,
				"prefer_hand_tracking": false,
				"target_fps": 90,
				"render_scale": 1.0,
			}
		XRPlatform.AR_FOUNDATION:
			return {
				"ui_scale": 1.0,
				"use_world_space_ui": false,
				"prefer_hand_tracking": false,
				"target_fps": 60,
				"render_scale": 1.0,
			}
		_:
			return {
				"ui_scale": 1.0,
				"use_world_space_ui": false,
				"prefer_hand_tracking": false,
				"target_fps": 60,
				"render_scale": 1.0,
			}

func _refresh_capabilities() -> void:
	match active_platform:
		XRPlatform.META_QUEST:
			hand_tracking_available = true
			eye_tracking_available = true
			passthrough_available = true
		XRPlatform.APPLE_VISION_PRO:
			hand_tracking_available = true
			eye_tracking_available = true
			passthrough_available = true
		XRPlatform.PSVR2:
			hand_tracking_available = false
			eye_tracking_available = true
			passthrough_available = true
		XRPlatform.STEAMVR, XRPlatform.GENERIC_OPENXR:
			hand_tracking_available = _xr_interface != null and _xr_interface.get_capabilities() & XRInterface.XR_HAND_TRACKING != 0 if _xr_interface else false
			eye_tracking_available = false
			passthrough_available = false
		XRPlatform.PICO:
			hand_tracking_available = true
			eye_tracking_available = true
			passthrough_available = true
		_:
			hand_tracking_available = false
			eye_tracking_available = false
			passthrough_available = false
