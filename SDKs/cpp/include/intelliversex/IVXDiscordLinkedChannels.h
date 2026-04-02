// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace ivx {

struct IVXLinkedChannel {
    std::string channelId;
    std::string guildId;
    std::string name;
    std::string lobbyId;
    std::int64_t linkedAt = 0;
};

using LinkedChannelCb = std::function<void(const IVXLinkedChannel&)>;
using LinkedChannelListCb = std::function<void(const std::vector<IVXLinkedChannel>&)>;

/// Discord Social SDK — linked channels: bridge in-game chat to Discord text channels.
/// Stub surface; integrate with the native Discord Social SDK.
class IVXDiscordLinkedChannels {
public:
    static IVXDiscordLinkedChannels& instance() {
        static IVXDiscordLinkedChannels inst;
        return inst;
    }

    void linkChannel(const std::string& lobbyId, const std::string& channelId,
                     LinkedChannelCb onSuccess, ErrorCb onError = nullptr);

    void unlinkChannel(const std::string& lobbyId, const std::string& channelId,
                       SuccessCb onSuccess = nullptr, ErrorCb onError = nullptr);

    void getLinkedChannels(const std::string& lobbyId, LinkedChannelListCb onComplete,
                           ErrorCb onError = nullptr);

private:
    IVXDiscordLinkedChannels() = default;
};

} // namespace ivx
