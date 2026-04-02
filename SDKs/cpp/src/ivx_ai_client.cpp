// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/ivx_ai_client.h"
#include "ivx_http_internal.h"
#include <iostream>

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

    std::string body = "{\"personaId\":\"" + json::escape(personaId)
                     + "\",\"userId\":\"" + json::escape(userId) + "\"}";

    http::post(_baseUrl + "/ai-voice/session", body, _apiKey,
        [this, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("startVoiceSession failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                else if (cb) cb(AISessionResponse{});
                return;
            }
            AISessionResponse r;
            r.sessionId = json::getString(resp.body, "sessionId");
            r.status    = json::getString(resp.body, "status");
            r.wsUrl     = json::getString(resp.body, "wsUrl");
            log("Voice session started: " + r.sessionId);
            if (cb) cb(r);
        });
}

void AIClient::endVoiceSession(const std::string& sessionId, VoidCallback cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }

    http::post(_baseUrl + "/ai-voice/session/" + sessionId + "/end", "{}", _apiKey,
        [this, sessionId, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("endVoiceSession failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                return;
            }
            log("Voice session ended: " + sessionId);
            if (cb) cb();
        });
}

void AIClient::sendText(const std::string& sessionId, const std::string& text,
                        VoidCallback cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }

    std::string body = "{\"text\":\"" + json::escape(text) + "\"}";

    http::post(_baseUrl + "/ai-voice/session/" + sessionId + "/text", body, _apiKey,
        [this, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("sendText failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                return;
            }
            if (cb) cb();
        });
}

// --- Host sessions ---

void AIClient::startHostSession(const std::string& matchId, const HostProfile& profile,
                                Callback<AISessionResponse> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }

    std::string body = "{\"matchId\":\"" + json::escape(matchId)
                     + "\",\"profile\":{\"displayName\":\"" + json::escape(profile.displayName) + "\"";
    if (!profile.voiceId.empty())
        body += ",\"voiceId\":\"" + json::escape(profile.voiceId) + "\"";
    if (!profile.language.empty())
        body += ",\"language\":\"" + json::escape(profile.language) + "\"";
    if (!profile.personaId.empty())
        body += ",\"personaId\":\"" + json::escape(profile.personaId) + "\"";
    body += "}}";

    http::post(_baseUrl + "/ai-host/session", body, _apiKey,
        [this, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("startHostSession failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                else if (cb) cb(AISessionResponse{});
                return;
            }
            AISessionResponse r;
            r.sessionId = json::getString(resp.body, "sessionId");
            r.status    = json::getString(resp.body, "status");
            r.wsUrl     = json::getString(resp.body, "wsUrl");
            log("Host session started: " + r.sessionId);
            if (cb) cb(r);
        });
}

void AIClient::sendHostEvent(const std::string& sessionId, const std::string& eventType,
                             const std::string& data) {
    if (!_init) return;

    std::string body = "{\"eventType\":\"" + json::escape(eventType)
                     + "\",\"data\":\"" + json::escape(data) + "\"}";

    http::post(_baseUrl + "/ai-host/session/" + sessionId + "/event", body, _apiKey,
        [this, eventType](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("sendHostEvent failed: " + resp.error);
                return;
            }
            log("Host event sent: " + eventType);
        });
}

// --- Entitlement ---

void AIClient::checkEntitlement(const std::string& userId,
                                Callback<AIEntitlement> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }

    http::get(_baseUrl + "/ai-voice/entitlement/" + userId, _apiKey,
        [this, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("checkEntitlement failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                else if (cb) cb(AIEntitlement{});
                return;
            }
            AIEntitlement e;
            e.entitled         = json::getBool(resp.body, "entitled");
            e.tier             = json::getString(resp.body, "tier");
            e.remainingCredits = json::getInt(resp.body, "remainingCredits");
            e.expiresAt        = json::getString(resp.body, "expiresAt");
            if (cb) cb(e);
        });
}

// --- Personas ---

void AIClient::getPersonas(Callback<std::vector<AIPersona>> cb, ErrorCb err) {
    if (!_init) {
        if (err) err({-1, "AIClient not initialized"});
        return;
    }

    http::get(_baseUrl + "/ai-voice/personas", _apiKey,
        [this, cb, err](const http::HttpResponse& resp) {
            if (!resp.success) {
                log("getPersonas failed: " + resp.error);
                if (err) err({static_cast<int>(resp.statusCode), resp.error});
                else if (cb) cb({});
                return;
            }
            std::vector<AIPersona> personas;
            auto elements = json::getArrayElements(resp.body);
            for (const auto& elem : elements) {
                AIPersona p;
                p.personaId = json::getString(elem, "personaId");
                if (p.personaId.empty())
                    p.personaId = json::getString(elem, "id");
                p.name        = json::getString(elem, "name");
                p.description = json::getString(elem, "description");
                p.voiceId     = json::getString(elem, "voiceId");
                p.avatarUrl   = json::getString(elem, "avatarUrl");
                personas.push_back(std::move(p));
            }
            log("getPersonas returned " + std::to_string(personas.size()) + " personas");
            if (cb) cb(personas);
        });
}

// --- Logging ---

void AIClient::log(const std::string& msg) {
    std::cout << "[IVX:AIClient] " << msg << std::endl;
}

} // namespace ivx
