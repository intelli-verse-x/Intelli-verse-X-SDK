// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXDiscordMessages.h"

namespace IntelliVerseX {

namespace {
void notImplemented(ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}
} // namespace

IVXDiscordMessages& IVXDiscordMessages::getInstance() {
    static IVXDiscordMessages instance;
    return instance;
}

bool IVXDiscordMessages::isShowingChat() const {
    return false;
}

void IVXDiscordMessages::sendDM(const std::string&, const std::string&, MessageIdCallback, ErrorCallback onError) {
    notImplemented(onError);
}

void IVXDiscordMessages::editDM(const std::string&, const std::string&, const std::string&,
                                SuccessCallback, ErrorCallback onError) {
    notImplemented(onError);
}

void IVXDiscordMessages::getDMHistory(const std::string&, int, DMHistoryCallback onComplete) {
    if (onComplete) onComplete({});
}

void IVXDiscordMessages::getDMSummaries(DMSummariesCallback onComplete) {
    if (onComplete) onComplete({});
}

void IVXDiscordMessages::setShowingChat(bool) {}
void IVXDiscordMessages::openMessageInDiscord(const std::string&) {}
void IVXDiscordMessages::openDMSettingsInDiscord() {}

} // namespace IntelliVerseX
