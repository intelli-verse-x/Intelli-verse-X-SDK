// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <string>
#include <stdexcept>

namespace ivx {

struct Config {
    /// Game ID (UUID) for this title on the IntelliVerseX platform.
    /// Obtain from https://intelli-verse-x.ai/developers or POST https://msapi.intelli-verse-x.io/api/games/game/info
    std::string game_id;

    std::string host = "nakama-rest.intelli-verse-x.ai";
    int port = 443;
    std::string serverKey = "defaultkey";
    bool useSSL = true;
    bool debugLogs = false;
    bool verboseLogs = false;

    /// Directory where session and device-id files are persisted.
    /// Defaults to "." (current working directory). On mobile or
    /// sandboxed platforms, set this to a writable app-data path
    /// before calling Manager::init().
    std::string storagePath = ".";

    /// Validates the configuration and throws std::invalid_argument
    /// if any value is out of range.
    void validate() const {
        if (host.empty())
            throw std::invalid_argument("Config::host must not be empty");
        if (port < 1 || port > 65535)
            throw std::invalid_argument("Config::port must be in range [1, 65535]");
        if (serverKey.empty())
            throw std::invalid_argument("Config::serverKey must not be empty");
        if (storagePath.empty())
            throw std::invalid_argument("Config::storagePath must not be empty");
    }
};

} // namespace ivx
