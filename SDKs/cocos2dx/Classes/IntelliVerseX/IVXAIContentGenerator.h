// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <map>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXQuestTemplate {
    std::string genre;
    std::string difficulty;
};

struct IVXGeneratedQuest {
    std::string title;
};

struct IVXGeneratedStory {
    std::string body;
};

struct IVXGeneratedItem {
    std::string description;
};

struct IVXGeneratedDialogue {
    std::string rawJson;
};

/// Content generation — stub matching Unity IVXAIContentGenerator.
class IVXAIContentGenerator {
public:
    static IVXAIContentGenerator& getInstance();

    bool isGenerating() const;

    void initialize(void* config);
    void generateQuest(const IVXQuestTemplate* tpl, const std::string& playerContext,
                       std::function<void(const IVXGeneratedQuest*)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void generateStory(const std::string& prompt, const std::string& genre, int maxWords,
                       std::function<void(const IVXGeneratedStory*)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void generateItemDescription(const std::string& name, const std::string& type, const std::string& rarity,
                                 std::function<void(const IVXGeneratedItem*)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void generateDialogue(const std::string& scenario, const std::vector<std::string>& characters,
                          std::function<void(const IVXGeneratedDialogue*)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void generateFromTemplate(const std::string& tpl, const std::map<std::string, std::string>& variables,
                              std::function<void(const std::string&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void cancelGeneration();

private:
    IVXAIContentGenerator() = default;
};

} // namespace IntelliVerseX
