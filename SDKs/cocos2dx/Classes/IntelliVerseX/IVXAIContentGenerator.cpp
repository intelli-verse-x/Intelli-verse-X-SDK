// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIContentGenerator.h"
#include "cocos2d.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAIContentGenerator& IVXAIContentGenerator::getInstance() {
    static IVXAIContentGenerator instance;
    return instance;
}

bool IVXAIContentGenerator::isGenerating() const {
    return false;
}

void IVXAIContentGenerator::initialize(void*) {
    cocos2d::log("[IVX-Cocos] IVXAIContentGenerator::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAIContentGenerator::generateQuest(const IVXQuestTemplate*, const std::string&,
                                          std::function<void(const IVXGeneratedQuest*)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIContentGenerator::generateStory(const std::string&, const std::string&, int,
                                          std::function<void(const IVXGeneratedStory*)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIContentGenerator::generateItemDescription(const std::string&, const std::string&, const std::string&,
                                                   std::function<void(const IVXGeneratedItem*)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIContentGenerator::generateDialogue(const std::string&, const std::vector<std::string>&,
                                             std::function<void(const IVXGeneratedDialogue*)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIContentGenerator::generateFromTemplate(const std::string&, const std::map<std::string, std::string>&,
                                                 std::function<void(const std::string&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIContentGenerator::cancelGeneration() {}

} // namespace IntelliVerseX
