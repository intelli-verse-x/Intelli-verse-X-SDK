// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/IVXDiscordLinkedChannels.h"

namespace ivx {

void IVXDiscordLinkedChannels::linkChannel(const std::string&, const std::string&,
                                           LinkedChannelCb, ErrorCb onError) {
    if (onError) onError({-1, "Not implemented — requires Discord Social SDK native integration."});
}

void IVXDiscordLinkedChannels::unlinkChannel(const std::string&, const std::string&,
                                             SuccessCb, ErrorCb onError) {
    if (onError) onError({-1, "Not implemented — requires Discord Social SDK native integration."});
}

void IVXDiscordLinkedChannels::getLinkedChannels(const std::string&, LinkedChannelListCb onComplete,
                                                 ErrorCb onError) {
    if (onError) {
        onError({-1, "Not implemented — requires Discord Social SDK native integration."});
        return;
    }
    if (onComplete) onComplete({});
}

} // namespace ivx
