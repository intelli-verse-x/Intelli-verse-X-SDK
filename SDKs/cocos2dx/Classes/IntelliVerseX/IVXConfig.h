#pragma once

#include <string>

namespace IntelliVerseX {

struct IVXConfig {
    std::string nakamaHost = "127.0.0.1";
    int nakamaPort = 7350;
    std::string nakamaServerKey = "defaultkey";
    bool useSSL = false;

    std::string cognitoRegion;
    std::string cognitoUserPoolId;
    std::string cognitoClientId;

    bool enableAnalytics = true;
    bool enableDebugLogs = false;
    bool verboseLogging = false;

    std::string getScheme() const { return useSSL ? "https" : "http"; }
    std::string getBaseUrl() const {
        return getScheme() + "://" + nakamaHost + ":" + std::to_string(nakamaPort);
    }
};

} // namespace IntelliVerseX
