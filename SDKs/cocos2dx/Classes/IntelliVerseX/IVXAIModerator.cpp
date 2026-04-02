// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIModerator.h"
#include "cocos2d.h"
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
    cocos2d::log("[IVX-Cocos] IVXAIModerator::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAIModerator::classifyText(const std::string&, std::function<void(const IVXAIModerationResult&)>) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::filterMessage(const std::string&, std::function<void(const std::string&)>) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::scanBatch(const std::vector<std::string>&, std::function<void(std::vector<IVXAIModerationResult>)>) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::addCustomRule(const IVXAIModerationRule&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::removeCustomRule(const std::string&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::setCustomRules(const std::vector<IVXAIModerationRule>&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIModerator::clearCustomRules() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

IVXAIModerationResult IVXAIModerator::checkLocalRules(const std::string&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

std::map<std::string, std::string> IVXAIModerator::getDiscordModerationMetadata(const IVXAIModerationResult&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

} // namespace IntelliVerseX
