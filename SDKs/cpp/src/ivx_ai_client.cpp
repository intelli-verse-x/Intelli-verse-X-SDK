// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/ivx_ai_client.h"
#include <iostream>
#include <stdexcept>

namespace ivx {

AIClient& AIClient::instance() {
    static AIClient inst;
    return inst;
}

void AIClient::initialize(const std::string& apiBaseUrl, const std::string& apiKey) {
    _baseUrl = apiBaseUrl;
    while (!_baseUrl.empty() && _baseUrl.back() == '/') {
        _baseUrl.pop_back();
    }
    _apiKey = apiKey;
    _init = true;
    log("initialized (base=" + _baseUrl + ")");
}

// --- Voice sessions ---

void AIClient::startVoiceSession(const std::string& personaId, const std::string& userId,
                                 Callback<AISessionResponse> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    // Placeholder: real implementation would POST /v1/ai/voice/sessions
    log("startVoiceSession persona=" + personaId + " user=" + userId);
    if (cb) {
        AISessionResponse r;
        r.status = "pending";
        cb(r);
    }
}

void AIClient::endVoiceSession(const std::string& sessionId, VoidCallback cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    log("endVoiceSession session=" + sessionId);
    if (cb) cb();
}

void AIClient::sendText(const std::string& sessionId, const std::string& text,
                        VoidCallback cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    log("sendText session=" + sessionId + " len=" + std::to_string(text.size()));
    if (cb) cb();
}

// --- Host sessions ---

void AIClient::startHostSession(const std::string& matchId, const HostProfile& profile,
                                Callback<AISessionResponse> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    log("startHostSession match=" + matchId + " persona=" + profile.personaId);
    if (cb) {
        AISessionResponse r;
        r.status = "pending";
        cb(r);
    }
}

void AIClient::sendHostEvent(const std::string& sessionId, const std::string& eventType,
                             const std::string& data) {
    if (!_init) return;
    log("sendHostEvent session=" + sessionId + " type=" + eventType);
}

// --- Entitlement ---

void AIClient::checkEntitlement(const std::string& userId,
                                Callback<AIEntitlement> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    log("checkEntitlement user=" + userId);
    if (cb) {
        AIEntitlement e;
        cb(e);
    }
}

// --- Personas ---

void AIClient::getPersonas(Callback<std::vector<AIPersona>> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }
    log("getPersonas");
    if (cb) {
        std::vector<AIPersona> empty;
        cb(empty);
    }
}

// --- Logging ---

void AIClient::log(const std::string& msg) {
    std::cout << "[IVX:AIClient] " << msg << std::endl;
}

} // namespace ivx
