// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIAssistant.h"
#include "cocos2d.h"
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
    cocos2d::log("[IVX-Cocos] IVXAIAssistant::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAIAssistant::setAuthToken(const std::string&) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIAssistant::clearHistory() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIAssistant::setSystemPrompt(const std::string& prompt) {
    systemPrompt = prompt;
}

void IVXAIAssistant::ask(const std::string&, const IVXAIGameContext*, std::function<void(const IVXAIAssistantResponse&)>,
                         ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIAssistant::getHint(const std::string&, const std::string&, const IVXAIGameContext*,
                             std::function<void(const IVXAIHintResponse&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIAssistant::getTutorial(const std::string&, std::function<void(const IVXAITutorialResponse&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIAssistant::searchKnowledgeBase(const std::string&, std::function<void(const std::vector<std::string>&)>,
                                         ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

} // namespace IntelliVerseX
