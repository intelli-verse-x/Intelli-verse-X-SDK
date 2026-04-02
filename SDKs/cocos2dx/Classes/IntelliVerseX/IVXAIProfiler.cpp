// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIProfiler.h"
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
    throw std::runtime_error("Not implemented");
}

void IVXAIProfiler::trackEvent(const std::string&, const std::map<std::string, double>&) {
    throw std::runtime_error("Not implemented");
}

void IVXAIProfiler::flushEvents() {
    throw std::runtime_error("Not implemented");
}

void IVXAIProfiler::getPlayerProfile(std::function<void(const IVXPlayerProfile*)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIProfiler::getPersonalizationHints(std::function<void(const std::vector<IVXPersonalizationHint>&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIProfiler::classifyPlayer(std::function<void(IVXPlayerCohort)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIProfiler::predictChurn(std::function<void(float, const std::vector<std::string>&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIProfiler::startAutoTracking() {
    throw std::runtime_error("Not implemented");
}

void IVXAIProfiler::stopAutoTracking() {
    throw std::runtime_error("Not implemented");
}

} // namespace IntelliVerseX
