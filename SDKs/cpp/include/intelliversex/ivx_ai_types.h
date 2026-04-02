// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <cstdint>
#include <functional>
#include <string>
#include <unordered_map>
#include <vector>

namespace ivx {

struct AISessionResponse {
    std::string sessionId;
    std::string status;
    std::string wsUrl;
};

struct AIMessage {
    std::string role;
    std::string content;
    std::string timestamp;
};

struct AIPersona {
    std::string personaId;
    std::string name;
    std::string description;
    std::string voiceId;
    std::string avatarUrl;
};

struct AIEntitlement {
    bool entitled = false;
    std::string tier;
    int32_t remainingCredits = 0;
    std::string expiresAt;
};

struct HostProfile {
    std::string personaId;
    std::string displayName;
    std::string voiceId;
    std::string language;
    std::unordered_map<std::string, std::string> extraParams;
};

template <typename T>
using Callback = std::function<void(const T&)>;

using VoidCallback = std::function<void()>;

} // namespace ivx
