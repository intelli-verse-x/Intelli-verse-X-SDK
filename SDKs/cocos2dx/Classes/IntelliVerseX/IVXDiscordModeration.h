// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <map>
#include <string>

namespace IntelliVerseX {

enum class IVXModerationPresentationAction { Show, Hide, Blur, Replace };

struct IVXModerationDecision {
    std::string messageId;
    IVXModerationPresentationAction action = IVXModerationPresentationAction::Show;
    std::string reason;
    std::string replacement;
    std::string severity;
};

/// Discord moderation — stub matching Unity IVXDiscordModeration.
class IVXDiscordModeration {
public:
    static IVXDiscordModeration& getInstance();

    bool autoModerateEnabled = true;

    void enableAutoModeration(bool enable);
    void processModerationMetadata(const std::string& messageId, const std::map<std::string, std::string>& metadata);
    static IVXModerationDecision getModerationAction(const std::map<std::string, std::string>& metadata);
    void startVoiceModerationCapture(const std::string& lobbyId);
    void stopVoiceModerationCapture();
    void reportUser(const std::string& userId, const std::string& reason,
                    std::function<void(bool)> onComplete = nullptr);

private:
    IVXDiscordModeration() = default;
};

} // namespace IntelliVerseX
