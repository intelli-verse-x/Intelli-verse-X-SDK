// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <string>

namespace IntelliVerseX {

struct IVXConfig {
    // Game ID (UUID) for this title on the IntelliVerseX platform.
    // Obtain from https://intelli-verse-x.ai/developers or POST https://msapi.intelli-verse-x.io/api/games/game/info
    std::string gameId;

    std::string nakamaHost = "nakama-rest.intelli-verse-x.ai";
    int nakamaPort = 443;
    std::string nakamaServerKey = "defaultkey";
    bool useSSL = true;

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
