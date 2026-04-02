// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIAssistant.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAIAssistant& IVXAIAssistant::getInstance() {
    static IVXAIAssistant instance;
    return instance;
}

bool IVXAIAssistant::isProcessing() const {
    return false;
}
bool IVXAIAssistant::isInitialized() const {
    return false;
}

void IVXAIAssistant::initialize(void*) {
    throw std::runtime_error("Not implemented");
}

void IVXAIAssistant::setAuthToken(const std::string&) {
    throw std::runtime_error("Not implemented");
}

void IVXAIAssistant::clearHistory() {
    throw std::runtime_error("Not implemented");
}

void IVXAIAssistant::setSystemPrompt(const std::string& prompt) {
    systemPrompt = prompt;
}

void IVXAIAssistant::ask(const std::string&, const IVXAIGameContext*, std::function<void(const IVXAIAssistantResponse&)>,
                         ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIAssistant::getHint(const std::string&, const std::string&, const IVXAIGameContext*,
                             std::function<void(const IVXAIHintResponse&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIAssistant::getTutorial(const std::string&, std::function<void(const IVXAITutorialResponse&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIAssistant::searchKnowledgeBase(const std::string&, std::function<void(const std::vector<std::string>&)>,
                                         ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

} // namespace IntelliVerseX
