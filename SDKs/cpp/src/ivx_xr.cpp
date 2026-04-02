#include "intelliversex/ivx_xr.h"
#include <cstring>
#include <iostream>

#ifdef IVX_HAS_OPENXR
#include <openxr/openxr.h>
#endif

namespace intelliversex {

#ifdef IVX_HAS_OPENXR

static XRPlatform detectFromSystemName(const char* systemName) {
    if (std::strstr(systemName, "Quest") || std::strstr(systemName, "Meta") || std::strstr(systemName, "Oculus"))
        return XRPlatform::MetaQuest;
    if (std::strstr(systemName, "SteamVR") || std::strstr(systemName, "Valve") || std::strstr(systemName, "Index"))
        return XRPlatform::SteamVR;
    if (std::strstr(systemName, "Apple") || std::strstr(systemName, "Vision"))
        return XRPlatform::AppleVisionPro;
    if (std::strstr(systemName, "PSVR") || std::strstr(systemName, "PlayStation"))
        return XRPlatform::PSVR2;
    return XRPlatform::GenericOpenXR;
}

XRPlatform IVXXRHelper::detectPlatform() {
    uint32_t extensionCount = 0;
    XrResult result = xrEnumerateInstanceExtensionProperties(nullptr, 0, &extensionCount, nullptr);
    if (XR_FAILED(result) || extensionCount == 0) {
        std::cerr << "[IVX-CPP] No OpenXR extensions found — runtime may not be available" << std::endl;
        return XRPlatform::None;
    }

    XrApplicationInfo appInfo{};
    std::strncpy(appInfo.applicationName, "IntelliVerseX", XR_MAX_APPLICATION_NAME_SIZE);
    appInfo.applicationVersion = 1;
    appInfo.apiVersion = XR_CURRENT_API_VERSION;

    XrInstanceCreateInfo createInfo{XR_TYPE_INSTANCE_CREATE_INFO};
    createInfo.applicationInfo = appInfo;
    createInfo.enabledExtensionCount = 0;
    createInfo.enabledExtensionNames = nullptr;

    XrInstance instance = XR_NULL_HANDLE;
    result = xrCreateInstance(&createInfo, &instance);
    if (XR_FAILED(result)) {
        std::cerr << "[IVX-CPP] xrCreateInstance failed (" << result << ")" << std::endl;
        return XRPlatform::None;
    }

    XrSystemGetInfo systemGetInfo{XR_TYPE_SYSTEM_GET_INFO};
    systemGetInfo.formFactor = XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY;

    XrSystemId systemId = XR_NULL_SYSTEM_ID;
    result = xrGetSystem(instance, &systemGetInfo, &systemId);
    if (XR_FAILED(result)) {
        std::cerr << "[IVX-CPP] No HMD system found" << std::endl;
        xrDestroyInstance(instance);
        return XRPlatform::None;
    }

    XrSystemProperties systemProps{XR_TYPE_SYSTEM_PROPERTIES};
    result = xrGetSystemProperties(instance, systemId, &systemProps);

    XRPlatform detected = XRPlatform::GenericOpenXR;
    if (XR_SUCCEEDED(result)) {
        detected = detectFromSystemName(systemProps.systemName);
        std::cerr << "[IVX-CPP] Detected XR system: " << systemProps.systemName << std::endl;
    }

    xrDestroyInstance(instance);
    return detected;
}

#else // !IVX_HAS_OPENXR

XRPlatform IVXXRHelper::detectPlatform() {
    std::cerr << "[IVX-CPP] IVXXRHelper::detectPlatform: stub — compile with IVX_HAS_OPENXR to enable real detection" << std::endl;
    return XRPlatform::None;
}

#endif // IVX_HAS_OPENXR

XRCapabilities IVXXRHelper::getCapabilities(XRPlatform platform) {
    XRCapabilities caps;
    switch (platform) {
        case XRPlatform::MetaQuest:
            caps.handTracking = true;
            caps.eyeTracking = true;
            caps.passthrough = true;
            caps.recommendedRefreshRate = 72;
            break;
        case XRPlatform::AppleVisionPro:
            caps.handTracking = true;
            caps.eyeTracking = true;
            caps.passthrough = true;
            caps.recommendedRefreshRate = 90;
            break;
        case XRPlatform::SteamVR:
            caps.recommendedRefreshRate = 90;
            break;
        case XRPlatform::PSVR2:
            caps.eyeTracking = true;
            caps.recommendedRefreshRate = 120;
            break;
        case XRPlatform::GenericOpenXR:
            caps.recommendedRefreshRate = 72;
            break;
        default:
            break;
    }
    return caps;
}

bool IVXXRHelper::isXRActive() {
    return detectPlatform() != XRPlatform::None;
}

const char* IVXXRHelper::platformName(XRPlatform platform) {
    switch (platform) {
        case XRPlatform::MetaQuest:      return "Meta Quest";
        case XRPlatform::SteamVR:        return "SteamVR";
        case XRPlatform::AppleVisionPro: return "Apple Vision Pro";
        case XRPlatform::PSVR2:          return "PSVR2";
        case XRPlatform::GenericOpenXR:  return "Generic OpenXR";
        case XRPlatform::ARKit:          return "ARKit";
        case XRPlatform::ARCore:         return "ARCore";
        default:                         return "None";
    }
}

} // namespace intelliversex
