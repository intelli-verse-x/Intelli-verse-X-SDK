// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <cstdint>
#include <functional>
#include <stdexcept>
#include <string>
#include <vector>

namespace ivx {

struct DirectMessage {
    std::string messageId;
    std::string authorId;
    std::string content;
    std::int64_t timestamp = 0;
};

struct DMSummary {
    std::string userId;
    std::string displayName;
    std::string lastMessageId;
    std::int64_t lastMessageTimestamp = 0;
};

/// Discord DM API — stub matching Unity IVXDiscordMessages.
class IVXDiscordMessages {
public:
    static IVXDiscordMessages& instance() {
        static IVXDiscordMessages inst;
        return inst;
    }

    bool isShowingChat() const { return false; }

    void sendDM(const std::string&, const std::string&,
                std::function<void(std::string)>, std::function<void(std::string)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void editDM(const std::string&, const std::string&, const std::string&,
                std::function<void()>, std::function<void(std::string)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void getDMHistory(const std::string&, int, std::function<void(std::vector<DirectMessage>)>) {
        throw std::runtime_error("Not implemented");
    }

    void getDMSummaries(std::function<void(std::vector<DMSummary>)>) {
        throw std::runtime_error("Not implemented");
    }

    void setShowingChat(bool) { throw std::runtime_error("Not implemented"); }
    void openMessageInDiscord(const std::string&) { throw std::runtime_error("Not implemented"); }
    void openDMSettingsInDiscord() { throw std::runtime_error("Not implemented"); }

private:
    IVXDiscordMessages() = default;
};

} // namespace ivx
