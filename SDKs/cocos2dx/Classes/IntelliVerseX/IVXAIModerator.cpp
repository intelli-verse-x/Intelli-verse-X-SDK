// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIModerator.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAIModerator& IVXAIModerator::getInstance() {
    static IVXAIModerator instance;
    return instance;
}

bool IVXAIModerator::isEnabled() const {
    return false;
}

void IVXAIModerator::initialize(void*) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::classifyText(const std::string&, std::function<void(const IVXAIModerationResult&)>) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::filterMessage(const std::string&, std::function<void(const std::string&)>) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::scanBatch(const std::vector<std::string>&, std::function<void(std::vector<IVXAIModerationResult>)>) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::addCustomRule(const IVXAIModerationRule&) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::removeCustomRule(const std::string&) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::setCustomRules(const std::vector<IVXAIModerationRule>&) {
    throw std::runtime_error("Not implemented");
}

void IVXAIModerator::clearCustomRules() {
    throw std::runtime_error("Not implemented");
}

IVXAIModerationResult IVXAIModerator::checkLocalRules(const std::string&) {
    throw std::runtime_error("Not implemented");
}

std::map<std::string, std::string> IVXAIModerator::getDiscordModerationMetadata(const IVXAIModerationResult&) {
    throw std::runtime_error("Not implemented");
}

} // namespace IntelliVerseX
