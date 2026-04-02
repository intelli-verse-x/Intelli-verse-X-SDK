// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <functional>
#include <stdexcept>
#include <string>
#include <unordered_map>
#include <vector>

namespace ivx {

struct QuestTemplate {
    std::string genre;
    std::string difficulty;
    std::vector<std::string> requiredElements;
    int estimatedDurationMinutes = 0;
};

struct GeneratedQuest {
    std::string title;
    std::string description;
};

struct GeneratedStory {
    std::string title;
    std::string body;
};

struct GeneratedItem {
    std::string name;
    std::string description;
};

struct GeneratedDialogue {
    std::string rawJson;
};

/// Procedural content — stub matching Unity IVXAIContentGenerator.
class IVXAIContentGenerator {
public:
    static IVXAIContentGenerator& instance() {
        static IVXAIContentGenerator inst;
        return inst;
    }

    bool isGenerating() const { return false; }

    void initialize(void*) { throw std::runtime_error("Not implemented"); }

    void generateQuest(const QuestTemplate*, const std::string&, std::function<void(const GeneratedQuest*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void generateStory(const std::string&, const std::string&, int, std::function<void(const GeneratedStory*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void generateItemDescription(const std::string&, const std::string&, const std::string&,
                                 std::function<void(const GeneratedItem*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void generateDialogue(const std::string&, const std::vector<std::string>&,
                          std::function<void(const GeneratedDialogue*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void generateFromTemplate(const std::string&, const std::unordered_map<std::string, std::string>&,
                              std::function<void(const std::string&)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void cancelGeneration() { throw std::runtime_error("Not implemented"); }

private:
    IVXAIContentGenerator() = default;
};

} // namespace ivx
