// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXDiscordDebug.h"

#include <algorithm>
#include <chrono>

namespace IntelliVerseX {

IVXDiscordDebug& IVXDiscordDebug::getInstance() {
    static IVXDiscordDebug instance;
    return instance;
}

void IVXDiscordDebug::setLogLevel(IVXDiscordLogLevel level) {
    _logLevel = level;
}

IVXDiscordLogLevel IVXDiscordDebug::getLogLevel() const {
    return _logLevel;
}

std::uint64_t IVXDiscordDebug::addLogCallback(IVXDiscordLogCallback callback) {
    if (!callback) return 0;
    const std::uint64_t id = _nextCallbackId++;
    _callbacks.push_back({id, std::move(callback)});
    return id;
}

bool IVXDiscordDebug::removeLogCallback(std::uint64_t registrationId) {
    if (registrationId == 0) return false;
    const auto it = std::find_if(_callbacks.begin(), _callbacks.end(),
                                 [registrationId](const RegisteredCallback& r) { return r.id == registrationId; });
    if (it == _callbacks.end()) return false;
    _callbacks.erase(it);
    return true;
}

std::vector<IVXDiscordLogEntry> IVXDiscordDebug::getLogHistory(std::size_t limit) const {
    if (limit == 0 || _history.empty()) return {};
    if (limit >= _history.size()) return _history;
    return std::vector<IVXDiscordLogEntry>(_history.end() - static_cast<std::ptrdiff_t>(limit), _history.end());
}

void IVXDiscordDebug::clearLogHistory() {
    _history.clear();
}

void IVXDiscordDebug::emitLog(IVXDiscordLogLevel level, const std::string& message, const std::string& source) {
    const auto lv = static_cast<std::uint8_t>(level);
    const auto maxLv = static_cast<std::uint8_t>(_logLevel);
    if (lv > maxLv) return;

    IVXDiscordLogEntry entry;
    entry.level = level;
    entry.message = message;
    entry.source = source;
    entry.timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(
                          std::chrono::system_clock::now().time_since_epoch())
                          .count();

    _history.push_back(std::move(entry));
    if (_history.size() > MAX_HISTORY) {
        _history.erase(_history.begin(),
                       _history.begin() + static_cast<std::ptrdiff_t>(_history.size() - MAX_HISTORY));
    }

    const IVXDiscordLogEntry& ref = _history.back();
    for (const auto& reg : _callbacks) {
        if (reg.fn) reg.fn(ref);
    }
}

} // namespace IntelliVerseX
