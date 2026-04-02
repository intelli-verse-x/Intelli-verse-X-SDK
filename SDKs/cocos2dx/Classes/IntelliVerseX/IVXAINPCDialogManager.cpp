// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAINPCDialogManager.h"
#include "cocos2d.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAINPCDialogManager& IVXAINPCDialogManager::getInstance() {
    static IVXAINPCDialogManager instance;
    return instance;
}

bool IVXAINPCDialogManager::isInitialized() const {
    return false;
}

void IVXAINPCDialogManager::initialize(void*) {
    cocos2d::log("[IVX-Cocos] IVXAINPCDialogManager::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAINPCDialogManager::setAuthToken(const std::string&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAINPCDialogManager::registerNPC(const IVXAINPCProfile&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAINPCDialogManager::unregisterNPC(const std::string&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAINPCDialogManager::startDialog(const std::string&, const std::string&, const std::string&,
                                        std::function<void(const IVXAINPCDialogSession&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAINPCDialogManager::sendMessage(const std::string&, const std::string&,
                                        std::function<void(const std::string&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAINPCDialogManager::endDialog(const std::string&, SuccessCallback, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

const IVXAINPCDialogSession* IVXAINPCDialogManager::getSession(const std::string&) const {
    return nullptr;
}

std::vector<IVXAINPCDialogSession> IVXAINPCDialogManager::getSessionsForNPC(const std::string&) const {
    return {};
}

} // namespace IntelliVerseX
