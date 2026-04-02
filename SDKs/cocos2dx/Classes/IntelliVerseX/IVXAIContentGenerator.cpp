// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIContentGenerator.h"
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
    throw std::runtime_error("Not implemented");
}

void IVXAIContentGenerator::generateQuest(const IVXQuestTemplate*, const std::string&,
                                          std::function<void(const IVXGeneratedQuest*)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIContentGenerator::generateStory(const std::string&, const std::string&, int,
                                          std::function<void(const IVXGeneratedStory*)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIContentGenerator::generateItemDescription(const std::string&, const std::string&, const std::string&,
                                                   std::function<void(const IVXGeneratedItem*)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIContentGenerator::generateDialogue(const std::string&, const std::vector<std::string>&,
                                             std::function<void(const IVXGeneratedDialogue*)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIContentGenerator::generateFromTemplate(const std::string&, const std::map<std::string, std::string>&,
                                                 std::function<void(const std::string&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIContentGenerator::cancelGeneration() {}

} // namespace IntelliVerseX
