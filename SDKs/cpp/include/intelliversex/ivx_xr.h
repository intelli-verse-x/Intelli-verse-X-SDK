#pragma once
#include <functional>

namespace intelliversex {

enum class XRPlatform {
    None,
    MetaQuest,
    SteamVR,
    AppleVisionPro,
    PSVR2,
    GenericOpenXR,
    ARKit,
    ARCore
};

struct XRCapabilities {
    bool handTracking = false;
    bool eyeTracking = false;
    bool passthrough = false;
    int recommendedRefreshRate = 60;
};

class IVXXRHelper {
public:
    using DetectionCallback = std::function<void(XRPlatform, const XRCapabilities&)>;

    static XRPlatform detectPlatform();
    static XRCapabilities getCapabilities(XRPlatform platform);
    static bool isXRActive();
    static const char* platformName(XRPlatform platform);
};

} // namespace intelliversex
