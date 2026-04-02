// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <map>
#include <string>
#include <vector>

namespace IntelliVerseX {

enum class IVXPlayerCohort {
    Casual, Social, Competitive, Explorer, Achiever, Whale, AtRisk, NewPlayer, Veteran, Lapsed
};

struct IVXPlayerProfile {
    std::string playerId;
    IVXPlayerCohort cohort = IVXPlayerCohort::Casual;
    float engagementScore = 0.f;
};

struct IVXPersonalizationHint {
    std::string message;
};

/// Profiling — stub matching Unity IVXAIProfiler.
class IVXAIProfiler {
public:
    static IVXAIProfiler& getInstance();

    bool isTracking() const;
    const IVXPlayerProfile* cachedProfile() const;

    void initialize(void* config, const std::string& playerId);
    void trackEvent(const std::string& eventName, const std::map<std::string, double>& data);
    void flushEvents();
    void getPlayerProfile(std::function<void(const IVXPlayerProfile*)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void getPersonalizationHints(std::function<void(const std::vector<IVXPersonalizationHint>&)> onComplete = nullptr,
                                 ErrorCallback onError = nullptr);
    void classifyPlayer(std::function<void(IVXPlayerCohort)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void predictChurn(std::function<void(float, const std::vector<std::string>&)> onComplete = nullptr,
                      ErrorCallback onError = nullptr);
    void startAutoTracking();
    void stopAutoTracking();

private:
    IVXAIProfiler() = default;
};

} // namespace IntelliVerseX
