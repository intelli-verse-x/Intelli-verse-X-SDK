// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

enum class IVXDiscordLogLevel : std::uint8_t {
    NONE = 0,
    ERROR_LEVEL = 1,
    WARN = 2,
    INFO = 3,
    DEBUG_LEVEL = 4
};

struct IVXDiscordLogEntry {
    IVXDiscordLogLevel level = IVXDiscordLogLevel::NONE;
    std::string message;
    int64_t timestamp = 0;
    std::string source;
};

using IVXDiscordLogCallback = std::function<void(const IVXDiscordLogEntry&)>;

/// Discord Social SDK — debug logging: route Discord SDK logs to custom sinks.
class IVXDiscordDebug {
public:
    static IVXDiscordDebug& getInstance();

    void setLogLevel(IVXDiscordLogLevel level);
    IVXDiscordLogLevel getLogLevel() const;

    /// @return Opaque id for use with removeLogCallback.
    std::uint64_t addLogCallback(IVXDiscordLogCallback callback);
    bool removeLogCallback(std::uint64_t registrationId);

    std::vector<IVXDiscordLogEntry> getLogHistory(std::size_t limit = 100) const;
    void clearLogHistory();

    void emitLog(IVXDiscordLogLevel level, const std::string& message, const std::string& source = "discord");

private:
    IVXDiscordDebug() = default;

    static constexpr std::size_t MAX_HISTORY = 500;

    IVXDiscordLogLevel _logLevel = IVXDiscordLogLevel::WARN;
    std::vector<IVXDiscordLogEntry> _history;
    std::uint64_t _nextCallbackId = 1;

    struct RegisteredCallback {
        std::uint64_t id = 0;
        IVXDiscordLogCallback fn;
    };
    std::vector<RegisteredCallback> _callbacks;
};

} // namespace IntelliVerseX
