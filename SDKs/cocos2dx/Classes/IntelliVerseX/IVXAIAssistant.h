// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXAIGameContext {
    std::string currentLevel;
    std::string currentObjective;
};

struct IVXAIAssistantResponse {
    std::string response;
    std::vector<std::string> sources;
    float confidence = 0.f;
};

struct IVXAIHintResponse {
    std::string hint;
};

struct IVXAITutorialResponse {
    std::string featureId;
};

/// In-game AI assistant — delegates to the AI service via HTTP.
class IVXAIAssistant {
public:
    static IVXAIAssistant& getInstance();

    bool isProcessing() const;
    bool isInitialized() const;
    std::string systemPrompt;

    void initialize(void* config);
    void setAuthToken(const std::string& token);
    void clearHistory();
    void setSystemPrompt(const std::string& prompt);
    void ask(const std::string& question, const IVXAIGameContext* ctx,
             std::function<void(const IVXAIAssistantResponse&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void getHint(const std::string& levelId, const std::string& objectiveId, const IVXAIGameContext* ctx,
                 std::function<void(const IVXAIHintResponse&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void getTutorial(const std::string& featureId,
                     std::function<void(const IVXAITutorialResponse&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void searchKnowledgeBase(const std::string& query,
                             std::function<void(const std::vector<std::string>&)> onResults = nullptr,
                             ErrorCallback onError = nullptr);

private:
    IVXAIAssistant() = default;

    bool _initialized = false;
    bool _processing = false;
    std::string _authToken;
    std::string _apiUrl = "https://ai.intelli-verse-x.ai";
    std::string _sessionId;

    void httpPost(const std::string& path, const std::string& bodyJson,
                  std::function<void(const std::string&)> onSuccess, ErrorCallback onError);
    void log(const std::string& msg);
};

} // namespace IntelliVerseX
