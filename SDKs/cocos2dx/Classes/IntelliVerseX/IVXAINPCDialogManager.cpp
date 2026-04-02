// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAINPCDialogManager.h"
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
    throw std::runtime_error("Not implemented");
}

void IVXAINPCDialogManager::setAuthToken(const std::string&) {
    throw std::runtime_error("Not implemented");
}

void IVXAINPCDialogManager::registerNPC(const IVXAINPCProfile&) {
    throw std::runtime_error("Not implemented");
}

void IVXAINPCDialogManager::unregisterNPC(const std::string&) {
    throw std::runtime_error("Not implemented");
}

void IVXAINPCDialogManager::startDialog(const std::string&, const std::string&, const std::string&,
                                        std::function<void(const IVXAINPCDialogSession&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAINPCDialogManager::sendMessage(const std::string&, const std::string&,
                                        std::function<void(const std::string&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAINPCDialogManager::endDialog(const std::string&, SuccessCallback, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

const IVXAINPCDialogSession* IVXAINPCDialogManager::getSession(const std::string&) const {
    return nullptr;
}

std::vector<IVXAINPCDialogSession> IVXAINPCDialogManager::getSessionsForNPC(const std::string&) const {
    return {};
}

} // namespace IntelliVerseX
