// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXSatori.h"

#include "cocos2d.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXSatori& IVXSatori::getInstance() {
    static IVXSatori instance;
    return instance;
}

void IVXSatori::ensureInitialized() const {
    if (!_initialized) {
        throw std::runtime_error("Satori not initialized. Call initialize() first.");
    }
}

void IVXSatori::initialize(const IVXSatoriConfig& config) {
    if (config.satoriUrl.empty()) throw std::runtime_error("satoriUrl is required.");
    if (config.apiKey.empty()) throw std::runtime_error("apiKey is required.");
    _config = config;
    _initialized = true;
}

void IVXSatori::authenticate(const std::string& identityId,
                             const std::map<std::string, std::string>&,
                             const std::map<std::string, std::string>&,
                             SuccessCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::authenticate: stub – not yet implemented");
    _identityId = identityId;
    if (onSuccess) onSuccess();
}

void IVXSatori::updateIdentity(const std::map<std::string, std::string>&,
                               const std::map<std::string, std::string>&,
                               SuccessCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::updateIdentity: stub – not yet implemented");
    if (onSuccess) onSuccess();
}

void IVXSatori::captureEvents(const std::vector<IVXSatoriEvent>&, SuccessCallback onSuccess,
                              ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::captureEvents: stub – not yet implemented");
    if (onSuccess) onSuccess();
}

void IVXSatori::getAllFlags(SatoriFlagsCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::getAllFlags: stub – not yet implemented");
    if (onSuccess) onSuccess({});
}

void IVXSatori::getFlag(const std::string&,
                        std::function<void(bool found, const IVXSatoriFlag& flag)> onSuccess,
                        ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::getFlag: stub – not yet implemented");
    if (onSuccess) onSuccess(false, {});
}

void IVXSatori::getExperimentVariant(const std::string&, RpcCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::getExperimentVariant: stub – not yet implemented");
    if (onSuccess) onSuccess("");
}

void IVXSatori::getAllExperiments(SatoriExperimentsCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::getAllExperiments: stub – not yet implemented");
    if (onSuccess) onSuccess({});
}

void IVXSatori::getLiveEvents(SatoriLiveEventsCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::getLiveEvents: stub – not yet implemented");
    if (onSuccess) onSuccess({});
}

void IVXSatori::logout(SuccessCallback onSuccess, ErrorCallback onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    cocos2d::log("%s", "[IVX-Cocos] IVXSatori::logout: stub – not yet implemented");
    _identityId.clear();
    if (onSuccess) onSuccess();
}

} // namespace IntelliVerseX
