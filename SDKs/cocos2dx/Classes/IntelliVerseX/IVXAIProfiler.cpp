// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIProfiler.h"
#include "cocos2d.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAIProfiler& IVXAIProfiler::getInstance() {
    static IVXAIProfiler instance;
    return instance;
}

bool IVXAIProfiler::isTracking() const {
    return false;
}

const IVXPlayerProfile* IVXAIProfiler::cachedProfile() const {
    return nullptr;
}

void IVXAIProfiler::initialize(void*, const std::string&) {
    cocos2d::log("[IVX-Cocos] IVXAIProfiler::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAIProfiler::trackEvent(const std::string&, const std::map<std::string, double>&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIProfiler::flushEvents() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIProfiler::getPlayerProfile(std::function<void(const IVXPlayerProfile*)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIProfiler::getPersonalizationHints(std::function<void(const std::vector<IVXPersonalizationHint>&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIProfiler::classifyPlayer(std::function<void(IVXPlayerCohort)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIProfiler::predictChurn(std::function<void(float, const std::vector<std::string>&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIProfiler::startAutoTracking() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIProfiler::stopAutoTracking() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

} // namespace IntelliVerseX
