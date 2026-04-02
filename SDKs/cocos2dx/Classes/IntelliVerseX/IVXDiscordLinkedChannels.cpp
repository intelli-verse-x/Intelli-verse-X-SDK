// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXDiscordLinkedChannels.h"

namespace IntelliVerseX {

namespace {
void notImplemented(ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented — requires Discord Social SDK native integration."});
}
} // namespace

IVXDiscordLinkedChannels& IVXDiscordLinkedChannels::getInstance() {
    static IVXDiscordLinkedChannels instance;
    return instance;
}

void IVXDiscordLinkedChannels::linkChannel(const std::string&, const std::string&, LinkedChannelCallback,
                                           ErrorCallback onError) {
    notImplemented(onError);
}

void IVXDiscordLinkedChannels::unlinkChannel(const std::string&, const std::string&, SuccessCallback,
                                             ErrorCallback onError) {
    notImplemented(onError);
}

void IVXDiscordLinkedChannels::getLinkedChannels(const std::string&, LinkedChannelListCallback onComplete,
                                                 ErrorCallback onError) {
    if (onError) {
        notImplemented(onError);
        return;
    }
    if (onComplete) onComplete({});
}

} // namespace IntelliVerseX
