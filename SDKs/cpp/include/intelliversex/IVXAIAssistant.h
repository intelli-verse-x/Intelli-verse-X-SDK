// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include <functional>
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

/// In-game AI assistant — delegates to the AI service via HTTP.
class IVXAIAssistant {
public:
    static IVXAIAssistant& instance();

    bool isProcessing() const;
    bool isInitialized() const;
    std::string systemPrompt;

    void initialize(void* config);
    void setAuthToken(const std::string& token);
    void clearHistory();
    void setSystemPrompt(const std::string& prompt);

    void ask(const std::string& question, const AIGameContext* ctx,
             std::function<void(const AIAssistantResponse*)> onComplete = nullptr,
             ErrorCb onError = nullptr);

    void getHint(const std::string& levelId, const std::string& objectiveId,
                 const AIGameContext* ctx,
                 std::function<void(const AIHintResponse*)> onComplete = nullptr,
                 ErrorCb onError = nullptr);

    void getTutorial(const std::string& featureId,
                     std::function<void(const AITutorialResponse*)> onComplete = nullptr,
                     ErrorCb onError = nullptr);

    void searchKnowledgeBase(const std::string& query,
                             std::function<void(std::vector<std::string>)> onResults = nullptr,
                             ErrorCb onError = nullptr);

private:
    IVXAIAssistant() = default;

    bool _initialized = false;
    bool _processing = false;
    std::string _authToken;
    std::string _apiUrl = "https://ai.intelli-verse-x.ai";
    std::string _sessionId;

    void log(const std::string& msg);
};

} // namespace ivx
