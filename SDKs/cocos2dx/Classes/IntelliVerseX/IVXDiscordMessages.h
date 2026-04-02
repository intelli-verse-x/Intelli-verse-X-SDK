// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXDirectMessage {
    std::string messageId;
    std::string authorId;
    std::string content;
    int64_t timestamp = 0;
};

struct IVXDMSummary {
    std::string userId;
    std::string displayName;
    std::string lastMessageId;
    int64_t lastMessageTimestamp = 0;
};

using DMHistoryCallback = std::function<void(const std::vector<IVXDirectMessage>&)>;
using DMSummariesCallback = std::function<void(const std::vector<IVXDMSummary>&)>;
using MessageIdCallback = std::function<void(const std::string&)>;

/// Discord DMs — stub matching Unity IVXDiscordMessages.
class IVXDiscordMessages {
public:
    static IVXDiscordMessages& getInstance();

    bool isShowingChat() const;

    void sendDM(const std::string& recipientId, const std::string& message,
                MessageIdCallback onSuccess = nullptr, ErrorCallback onError = nullptr);
    void editDM(const std::string& recipientId, const std::string& messageId, const std::string& newContent,
                SuccessCallback onSuccess = nullptr, ErrorCallback onError = nullptr);
    void getDMHistory(const std::string& recipientId, int limit, DMHistoryCallback onComplete);
    void getDMSummaries(DMSummariesCallback onComplete);
    void setShowingChat(bool showing);
    void openMessageInDiscord(const std::string& messageId);
    void openDMSettingsInDiscord();

private:
    IVXDiscordMessages() = default;
};

} // namespace IntelliVerseX
