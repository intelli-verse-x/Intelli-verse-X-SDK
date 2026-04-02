// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <functional>
#include <stdexcept>
#include <string>
#include <vector>

namespace ivx {

struct AINPCProfile {
    std::string npcId;
    int maxTurns = 0;
};

struct AINPCDialogSession {
    std::string sessionId;
    std::string npcId;
    std::string playerId;
};

/// NPC dialog — stub matching Unity IVXAINPCDialogManager.
class IVXAINPCDialogManager {
public:
    static IVXAINPCDialogManager& instance() {
        static IVXAINPCDialogManager inst;
        return inst;
    }

    bool isInitialized() const { return false; }

    void initialize(void*) { throw std::runtime_error("Not implemented"); }
    void setAuthToken(const std::string&) { throw std::runtime_error("Not implemented"); }
    void registerNPC(const AINPCProfile&) { throw std::runtime_error("Not implemented"); }
    void unregisterNPC(const std::string&) { throw std::runtime_error("Not implemented"); }

    void startDialog(const std::string&, const std::string&, const std::string&,
                     std::function<void(const AINPCDialogSession*)> onStarted = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void sendMessage(const std::string&, const std::string&, std::function<void(const std::string&)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void endDialog(const std::string&, std::function<void()> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    const AINPCDialogSession* getSession(const std::string&) { throw std::runtime_error("Not implemented"); }

    std::vector<AINPCDialogSession> getSessionsForNPC(const std::string&) {
        throw std::runtime_error("Not implemented");
    }

private:
    IVXAINPCDialogManager() = default;
};

} // namespace ivx
