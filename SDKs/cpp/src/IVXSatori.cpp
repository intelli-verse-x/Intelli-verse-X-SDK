// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/IVXSatori.h"

#include <stdexcept>

namespace ivx {

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
                             const std::unordered_map<std::string, std::string>&,
                             const std::unordered_map<std::string, std::string>&,
                             SuccessCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    _identityId = identityId;
    if (onSuccess) onSuccess();
}

void IVXSatori::updateIdentity(const std::unordered_map<std::string, std::string>&,
                               const std::unordered_map<std::string, std::string>&,
                               SuccessCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess();
}

void IVXSatori::captureEvents(const std::vector<IVXSatoriEvent>&, SuccessCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess();
}

void IVXSatori::getAllFlags(SatoriFlagsCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess({});
}

void IVXSatori::getFlag(const std::string&,
                        std::function<void(bool found, const IVXSatoriFlag& flag)> onSuccess,
                        ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess(false, {});
}

void IVXSatori::getExperimentVariant(const std::string&, StringCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess("");
}

void IVXSatori::getAllExperiments(SatoriExperimentsCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess({});
}

void IVXSatori::getLiveEvents(SatoriLiveEventsCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    if (onSuccess) onSuccess({});
}

void IVXSatori::logout(SuccessCb onSuccess, ErrorCb onError) {
    try {
        ensureInitialized();
    } catch (const std::exception& e) {
        if (onError) onError({-1, e.what()});
        return;
    }
    _identityId.clear();
    if (onSuccess) onSuccess();
}

} // namespace ivx
