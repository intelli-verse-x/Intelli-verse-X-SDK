// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include <cstdint>
#include <functional>
#include <string>
#include <unordered_map>
#include <vector>

namespace ivx {

struct IVXSatoriConfig {
    std::string satoriUrl;
    std::string apiKey;
    std::string identityToken;
};

struct IVXSatoriEvent {
    std::string name;
    std::string value;
    std::unordered_map<std::string, std::string> metadata;
    std::int64_t timestamp = 0;
};

struct IVXSatoriFlag {
    std::string name;
    std::string value;
    bool conditionChanged = false;
};

struct IVXSatoriExperiment {
    std::string name;
    std::string variant;
};

struct IVXSatoriLiveEvent {
    std::string id;
    std::string name;
    std::string description;
    std::string value;
    std::int64_t activeStartTime = 0;
    std::int64_t activeEndTime = 0;
};

using SatoriFlagsCb = std::function<void(const std::vector<IVXSatoriFlag>&)>;
using SatoriExperimentsCb = std::function<void(const std::vector<IVXSatoriExperiment>&)>;
using SatoriLiveEventsCb = std::function<void(const std::vector<IVXSatoriLiveEvent>&)>;

/// Satori analytics — events, flags, experiments, live-ops (stub; integrate Heroic Labs Satori).
class IVXSatori {
public:
    static IVXSatori& instance() {
        static IVXSatori inst;
        return inst;
    }

    bool isInitialized() const { return _initialized; }

    void initialize(const IVXSatoriConfig& config);

    void authenticate(const std::string& identityId,
                      const std::unordered_map<std::string, std::string>& defaultProperties,
                      const std::unordered_map<std::string, std::string>& customProperties,
                      SuccessCb onSuccess = nullptr, ErrorCb onError = nullptr);

    void updateIdentity(const std::unordered_map<std::string, std::string>& defaultProperties,
                        const std::unordered_map<std::string, std::string>& customProperties,
                        SuccessCb onSuccess = nullptr, ErrorCb onError = nullptr);

    void captureEvents(const std::vector<IVXSatoriEvent>& events, SuccessCb onSuccess = nullptr,
                       ErrorCb onError = nullptr);

    void getAllFlags(SatoriFlagsCb onSuccess, ErrorCb onError = nullptr);
    void getFlag(const std::string& name,
                 std::function<void(bool found, const IVXSatoriFlag& flag)> onSuccess,
                 ErrorCb onError = nullptr);
    void getExperimentVariant(const std::string& experimentName, StringCb onSuccess,
                              ErrorCb onError = nullptr);
    void getAllExperiments(SatoriExperimentsCb onSuccess, ErrorCb onError = nullptr);
    void getLiveEvents(SatoriLiveEventsCb onSuccess, ErrorCb onError = nullptr);

    void logout(SuccessCb onSuccess = nullptr, ErrorCb onError = nullptr);

private:
    IVXSatori() = default;

    void ensureInitialized() const;

    bool _initialized = false;
    IVXSatoriConfig _config;
    std::string _identityId;
};

} // namespace ivx
