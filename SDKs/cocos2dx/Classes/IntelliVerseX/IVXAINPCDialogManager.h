// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXAINPCProfile {
    std::string npcId;
    int maxTurns = 0;
};

struct IVXAINPCDialogSession {
    std::string sessionId;
    std::string npcId;
    std::string playerId;
};

/// NPC dialog — stub matching Unity IVXAINPCDialogManager.
class IVXAINPCDialogManager {
public:
    static IVXAINPCDialogManager& getInstance();

    bool isInitialized() const;

    void initialize(void* config);
    void setAuthToken(const std::string& token);
    void registerNPC(const IVXAINPCProfile& profile);
    void unregisterNPC(const std::string& npcId);
    void startDialog(const std::string& npcId, const std::string& playerId, const std::string& playerContext,
                     std::function<void(const IVXAINPCDialogSession&)> onStarted = nullptr, ErrorCallback onError = nullptr);
    void sendMessage(const std::string& sessionId, const std::string& message,
                     std::function<void(const std::string&)> onResponse = nullptr, ErrorCallback onError = nullptr);
    void endDialog(const std::string& sessionId, SuccessCallback onComplete = nullptr, ErrorCallback onError = nullptr);
    const IVXAINPCDialogSession* getSession(const std::string& sessionId) const;
    std::vector<IVXAINPCDialogSession> getSessionsForNPC(const std::string& npcId) const;

private:
    IVXAINPCDialogManager() = default;
};

} // namespace IntelliVerseX
