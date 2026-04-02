// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXDiscordModeration.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXDiscordModeration& IVXDiscordModeration::getInstance() {
    static IVXDiscordModeration instance;
    return instance;
}

void IVXDiscordModeration::enableAutoModeration(bool) {
    throw std::runtime_error("Not implemented");
}

void IVXDiscordModeration::processModerationMetadata(const std::string&, const std::map<std::string, std::string>&) {
    throw std::runtime_error("Not implemented");
}

IVXModerationDecision IVXDiscordModeration::getModerationAction(const std::map<std::string, std::string>&) {
    throw std::runtime_error("Not implemented");
}

void IVXDiscordModeration::startVoiceModerationCapture(const std::string&) {
    throw std::runtime_error("Not implemented");
}

void IVXDiscordModeration::stopVoiceModerationCapture() {
    throw std::runtime_error("Not implemented");
}

void IVXDiscordModeration::reportUser(const std::string&, const std::string&, std::function<void(bool)> onComplete) {
    if (onComplete) onComplete(false);
}

} // namespace IntelliVerseX
