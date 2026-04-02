#include "intelliversex/ivx_xr.h"
#include <iostream>

namespace intelliversex {

XRPlatform IVXXRHelper::detectPlatform() {
    std::cerr << "[IVX-CPP] IVXXRHelper::detectPlatform: stub — link your XR SDK (OpenXR, Oculus, etc.)" << std::endl;
    return XRPlatform::None;
}

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
        default:
            break;
    }
    return caps;
}

bool IVXXRHelper::isXRActive() { return false; }

const char* IVXXRHelper::platformName(XRPlatform platform) {
    switch (platform) {
        case XRPlatform::MetaQuest: return "Meta Quest";
        case XRPlatform::SteamVR: return "SteamVR";
        case XRPlatform::AppleVisionPro: return "Apple Vision Pro";
        case XRPlatform::PSVR2: return "PSVR2";
        case XRPlatform::GenericOpenXR: return "Generic OpenXR";
        case XRPlatform::ARKit: return "ARKit";
        case XRPlatform::ARCore: return "ARCore";
        default: return "None";
    }
}

} // namespace intelliversex
