// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_ai_types.h"
#include "ivx_types.h"
#include <string>

namespace ivx {

/// REST-based AI client for voice/text sessions and host AI.
///
/// Thread-safety: same rules as Manager — call from one thread, callbacks
/// fire during tick().
class AIClient {
public:
    static AIClient& instance();

    void initialize(const std::string& apiBaseUrl, const std::string& apiKey);
    bool initialized() const { return _init; }

    void startVoiceSession(const std::string& personaId, const std::string& userId,
                           Callback<AISessionResponse> cb, ErrorCb err = nullptr);
    void endVoiceSession(const std::string& sessionId, VoidCallback cb = nullptr, ErrorCb err = nullptr);
    void sendText(const std::string& sessionId, const std::string& text,
                  VoidCallback cb = nullptr, ErrorCb err = nullptr);

    void startHostSession(const std::string& matchId, const HostProfile& profile,
                          Callback<AISessionResponse> cb, ErrorCb err = nullptr);
    void sendHostEvent(const std::string& sessionId, const std::string& eventType,
                       const std::string& data);

    void checkEntitlement(const std::string& userId,
                          Callback<AIEntitlement> cb, ErrorCb err = nullptr);
    void getPersonas(Callback<std::vector<AIPersona>> cb, ErrorCb err = nullptr);

private:
    AIClient() = default;
    bool _init = false;
    std::string _baseUrl;
    std::string _apiKey;

    void log(const std::string& msg);
};

} // namespace ivx
