// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct AIPersona {
    std::string id;
    std::string name;
    std::string description;
    std::string avatarUrl;
    std::vector<std::string> supportedLanguages;
};

struct AIMessage {
    std::string id;
    std::string sessionId;
    std::string role;
    std::string text;
    int64_t timestamp = 0;
    std::string metadata;
};

struct AISessionResponse {
    std::string sessionId;
    std::string personaId;
    std::string userId;
    std::string status;
    int64_t createdAt = 0;
};

struct AIEntitlement {
    std::string userId;
    bool entitled = false;
    int remainingCredits = 0;
    std::string plan;
};

struct HostProfile {
    std::string displayName;
    std::string avatarUrl;
    std::string metadata;
};

using AISessionCallback = std::function<void(const AISessionResponse&)>;
using AIEntitlementCallback = std::function<void(const AIEntitlement&)>;
using AIPersonasCallback = std::function<void(const std::vector<AIPersona>&)>;
using AIMessageCallback = std::function<void(const AIMessage&)>;

class IVXAIClient {
public:
    static IVXAIClient& getInstance();

    void initialize(const std::string& apiBaseUrl,
                    const std::string& apiKey,
                    bool enableDebugLogs = false);
    bool isInitialized() const { return _initialized; }

    // Voice Sessions
    void startVoiceSession(const std::string& personaId,
                           const std::string& userId,
                           AISessionCallback onSuccess = nullptr,
                           ErrorCallback onError = nullptr);
    void endVoiceSession(const std::string& sessionId,
                         SuccessCallback onSuccess = nullptr,
                         ErrorCallback onError = nullptr);
    void sendText(const std::string& sessionId,
                  const std::string& text,
                  AIMessageCallback onSuccess = nullptr,
                  ErrorCallback onError = nullptr);

    // AI Host
    void startHostSession(const std::string& matchId,
                          const HostProfile& profile,
                          AISessionCallback onSuccess = nullptr,
                          ErrorCallback onError = nullptr);
    void sendHostEvent(const std::string& sessionId,
                       const std::string& eventType,
                       const std::string& data,
                       SuccessCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);

    // Entitlements & Personas
    void checkEntitlement(const std::string& userId,
                          AIEntitlementCallback onSuccess = nullptr,
                          ErrorCallback onError = nullptr);
    void getPersonas(AIPersonasCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);

private:
    IVXAIClient() = default;
    ~IVXAIClient() = default;
    IVXAIClient(const IVXAIClient&) = delete;
    IVXAIClient& operator=(const IVXAIClient&) = delete;

    std::string _apiBaseUrl;
    std::string _apiKey;
    bool _initialized = false;
    bool _enableDebugLogs = false;

    void httpPost(const std::string& path,
                  const std::string& bodyJson,
                  std::function<void(const std::string&)> onSuccess,
                  ErrorCallback onError);
    void httpGet(const std::string& path,
                 std::function<void(const std::string&)> onSuccess,
                 ErrorCallback onError);
    void log(const std::string& message);
};

} // namespace IntelliVerseX
