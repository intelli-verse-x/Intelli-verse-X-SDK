// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <functional>
#include <stdexcept>
#include <string>
#include <vector>

namespace ivx {

struct AIGameContext {
    std::string currentLevel;
    std::string currentObjective;
};

struct AIAssistantResponse {
    std::string response;
    std::vector<std::string> sources;
    float confidence = 0.f;
    bool isStreaming = false;
};

struct AIHintResponse {
    std::string hint;
    std::string difficultyLevel;
    bool nextHintAvailable = false;
};

struct AITutorialStep {
    int stepNumber = 0;
    std::string title;
    std::string description;
};

struct AITutorialResponse {
    std::string featureId;
    std::vector<AITutorialStep> steps;
    int estimatedTimeSeconds = 0;
};

/// In-game assistant — stub matching Unity IVXAIAssistant.
class IVXAIAssistant {
public:
    static IVXAIAssistant& instance() {
        static IVXAIAssistant inst;
        return inst;
    }

    bool isProcessing() const { return false; }
    bool isInitialized() const { return false; }
    std::string systemPrompt;

    void initialize(void*) { throw std::runtime_error("Not implemented"); }
    void setAuthToken(const std::string&) { throw std::runtime_error("Not implemented"); }
    void clearHistory() { throw std::runtime_error("Not implemented"); }
    void setSystemPrompt(const std::string& p) { systemPrompt = p; }

    void ask(const std::string&, const AIGameContext*, std::function<void(const AIAssistantResponse*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void getHint(const std::string&, const std::string&, const AIGameContext*,
                 std::function<void(const AIHintResponse*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void getTutorial(const std::string&, std::function<void(const AITutorialResponse*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void searchKnowledgeBase(const std::string&, std::function<void(std::vector<std::string>)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

private:
    IVXAIAssistant() = default;
};

} // namespace ivx
