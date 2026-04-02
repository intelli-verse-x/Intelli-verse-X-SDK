// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXLinkedChannel {
    std::string channelId;
    std::string guildId;
    std::string name;
    std::string lobbyId;
    int64_t linkedAt = 0;
};

using LinkedChannelCallback = std::function<void(const IVXLinkedChannel&)>;
using LinkedChannelListCallback = std::function<void(const std::vector<IVXLinkedChannel>&)>;

/// Discord Social SDK — linked channels: bridge in-game chat to Discord text channels.
/// Stub surface; integrate with the native Discord Social SDK.
class IVXDiscordLinkedChannels {
public:
    static IVXDiscordLinkedChannels& getInstance();

    void linkChannel(const std::string& lobbyId, const std::string& channelId,
                     LinkedChannelCallback onSuccess = nullptr, ErrorCallback onError = nullptr);
    void unlinkChannel(const std::string& lobbyId, const std::string& channelId,
                       SuccessCallback onSuccess = nullptr, ErrorCallback onError = nullptr);
    void getLinkedChannels(const std::string& lobbyId, LinkedChannelListCallback onComplete = nullptr,
                           ErrorCallback onError = nullptr);

private:
    IVXDiscordLinkedChannels() = default;
};

} // namespace IntelliVerseX
