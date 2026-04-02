class_name IVXXRHelper
extends Node

enum XRPlatform { NONE, META_QUEST, STEAMVR, PICO, GENERIC_OPENXR }

signal xr_platform_detected(platform: XRPlatform)

var active_platform: XRPlatform = XRPlatform.NONE
var is_xr_active: bool = false

func detect_xr_platform() -> XRPlatform:
    var xr_interface := XRServer.find_interface("OpenXR")
    if xr_interface == null:
        xr_interface = XRServer.find_interface("native")
    if xr_interface == null:
        active_platform = XRPlatform.NONE
        is_xr_active = false
        return active_platform

    if xr_interface.initialize():
        is_xr_active = true
        var name := xr_interface.get_name()
        if "Quest" in name or "Meta" in name or "Oculus" in name:
            active_platform = XRPlatform.META_QUEST
        elif "SteamVR" in name:
            active_platform = XRPlatform.STEAMVR
        elif "Pico" in name:
            active_platform = XRPlatform.PICO
        else:
            active_platform = XRPlatform.GENERIC_OPENXR
    else:
        active_platform = XRPlatform.NONE
        is_xr_active = false

    push_warning("[IVXXRHelper] Detected XR platform: %s" % XRPlatform.keys()[active_platform])
    xr_platform_detected.emit(active_platform)
    return active_platform
