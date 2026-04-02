// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <functional>
#include <stdexcept>
#include <string>
#include <unordered_map>

namespace ivx {

enum class ModerationPresentationAction { Show, Hide, Blur, Replace };

struct ModerationDecision {
    std::string messageId;
    ModerationPresentationAction action = ModerationPresentationAction::Show;
    std::string reason;
    std::string replacement;
    std::string severity;
};

/// Discord moderation — stub matching Unity IVXDiscordModeration.
class IVXDiscordModeration {
public:
    static IVXDiscordModeration& instance() {
        static IVXDiscordModeration inst;
        return inst;
    }

    bool autoModerateEnabled = true;

    void enableAutoModeration(bool) { throw std::runtime_error("Not implemented"); }

    void processModerationMetadata(const std::string&, const std::unordered_map<std::string, std::string>&) {
        throw std::runtime_error("Not implemented");
    }

    static ModerationDecision getModerationAction(const std::unordered_map<std::string, std::string>&) {
        throw std::runtime_error("Not implemented");
    }

    void startVoiceModerationCapture(const std::string&) { throw std::runtime_error("Not implemented"); }
    void stopVoiceModerationCapture() { throw std::runtime_error("Not implemented"); }

    void reportUser(const std::string&, const std::string&, std::function<void(bool)> onComplete = nullptr) {
        throw std::runtime_error("Not implemented");
    }

private:
    IVXDiscordModeration() = default;
};

} // namespace ivx
