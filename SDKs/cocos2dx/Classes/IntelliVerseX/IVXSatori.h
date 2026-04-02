// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <cstdint>
#include <functional>
#include <map>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXSatoriConfig {
    std::string satoriUrl;
    std::string apiKey;
    std::string identityToken;
};

struct IVXSatoriEvent {
    std::string name;
    std::string value;
    std::map<std::string, std::string> metadata;
    int64_t timestamp = 0;
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
    int64_t activeStartTime = 0;
    int64_t activeEndTime = 0;
};

using SatoriFlagsCallback = std::function<void(const std::vector<IVXSatoriFlag>&)>;
using SatoriExperimentsCallback = std::function<void(const std::vector<IVXSatoriExperiment>&)>;
using SatoriLiveEventsCallback = std::function<void(const std::vector<IVXSatoriLiveEvent>&)>;

/// Satori analytics — events, flags, experiments, live-ops (stub; integrate Heroic Labs Satori).
class IVXSatori {
public:
    static IVXSatori& getInstance();

    bool isInitialized() const { return _initialized; }

    void initialize(const IVXSatoriConfig& config);

    void authenticate(const std::string& identityId,
                      const std::map<std::string, std::string>& defaultProperties,
                      const std::map<std::string, std::string>& customProperties,
                      SuccessCallback onSuccess = nullptr, ErrorCallback onError = nullptr);

    void updateIdentity(const std::map<std::string, std::string>& defaultProperties,
                        const std::map<std::string, std::string>& customProperties,
                        SuccessCallback onSuccess = nullptr, ErrorCallback onError = nullptr);

    void captureEvents(const std::vector<IVXSatoriEvent>& events, SuccessCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);

    void getAllFlags(SatoriFlagsCallback onSuccess, ErrorCallback onError = nullptr);
    void getFlag(const std::string& name,
                 std::function<void(bool found, const IVXSatoriFlag& flag)> onSuccess,
                 ErrorCallback onError = nullptr);
    void getExperimentVariant(const std::string& experimentName, RpcCallback onSuccess,
                              ErrorCallback onError = nullptr);
    void getAllExperiments(SatoriExperimentsCallback onSuccess, ErrorCallback onError = nullptr);
    void getLiveEvents(SatoriLiveEventsCallback onSuccess, ErrorCallback onError = nullptr);

    void logout(SuccessCallback onSuccess = nullptr, ErrorCallback onError = nullptr);

private:
    IVXSatori() = default;
    ~IVXSatori() = default;
    IVXSatori(const IVXSatori&) = delete;
    IVXSatori& operator=(const IVXSatori&) = delete;

    void ensureInitialized() const;

    bool _initialized = false;
    IVXSatoriConfig _config;
    std::string _identityId;
};

} // namespace IntelliVerseX
