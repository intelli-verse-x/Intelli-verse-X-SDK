// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/IVXAIAssistant.h"
#include "ivx_http_internal.h"
#include <chrono>
#include <iostream>
#include <random>
#include <sstream>

namespace ivx {

static std::string generateSessionId() {
    auto now = std::chrono::system_clock::now().time_since_epoch();
    auto ms  = std::chrono::duration_cast<std::chrono::milliseconds>(now).count();
    std::mt19937 rng(static_cast<unsigned>(ms));
    std::uniform_int_distribution<int> dist(1000, 9999);
    std::ostringstream oss;
    oss << "sess_" << ms << "_" << dist(rng);
    return oss.str();
}

IVXAIAssistant& IVXAIAssistant::instance() {
    static IVXAIAssistant inst;
    return inst;
}

bool IVXAIAssistant::isProcessing() const { return _processing; }
bool IVXAIAssistant::isInitialized() const { return _initialized; }

void IVXAIAssistant::initialize(void* /*config*/) {
    _sessionId = generateSessionId();
    _initialized = true;
    log("initialized (sessionId=" + _sessionId + ")");
}

void IVXAIAssistant::setAuthToken(const std::string& token) {
    _authToken = token;
    log("auth token set");
}

void IVXAIAssistant::clearHistory() {
    _sessionId = generateSessionId();
    log("history cleared (new sessionId=" + _sessionId + ")");
}

void IVXAIAssistant::setSystemPrompt(const std::string& prompt) {
    systemPrompt = prompt;
}

// ---------------------------------------------------------------------------
// ask  →  POST /chat/response
// ---------------------------------------------------------------------------

void IVXAIAssistant::ask(const std::string& question, const AIGameContext* ctx,
                         std::function<void(const AIAssistantResponse*)> onComplete,
                         ErrorCb onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        else if (onComplete) onComplete(nullptr);
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        else if (onComplete) onComplete(nullptr);
        return;
    }

    _processing = true;

    std::string prompt = question;
    if (!systemPrompt.empty())
        prompt = systemPrompt + "\n\n" + prompt;
    if (ctx)
        prompt += "\n[Context: level=" + ctx->currentLevel
                + ", objective=" + ctx->currentObjective + "]";

    std::string body = "{\"prompt\":\"" + json::escape(prompt)
                     + "\",\"sessionId\":\"" + json::escape(_sessionId) + "\"}";

    http::post(_apiUrl + "/chat/response", body, _authToken,
        [this, onComplete, onError](const http::HttpResponse& resp) {
            _processing = false;
            if (!resp.success) {
                log("ask failed: " + resp.error);
                if (onError) onError({static_cast<int>(resp.statusCode), resp.error});
                else if (onComplete) onComplete(nullptr);
                return;
            }
            AIAssistantResponse result;
            result.response = json::getString(resp.body, "response");
            if (result.response.empty())
                result.response = json::getString(resp.body, "result");
            result.sources    = json::getStringArray(resp.body, "sources");
            result.confidence = 1.0f;
            log("ask completed (" + std::to_string(result.response.size()) + " chars)");
            if (onComplete) onComplete(&result);
        });
}

// ---------------------------------------------------------------------------
// getHint  →  POST /prompts/get-custom-interrogation-response
// ---------------------------------------------------------------------------

void IVXAIAssistant::getHint(const std::string& levelId, const std::string& objectiveId,
                             const AIGameContext* ctx,
                             std::function<void(const AIHintResponse*)> onComplete,
                             ErrorCb onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        else if (onComplete) onComplete(nullptr);
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        else if (onComplete) onComplete(nullptr);
        return;
    }

    _processing = true;

    std::string prompt = "Give a hint for level " + levelId
                       + " with objective: " + objectiveId;
    if (ctx)
        prompt += ". Current progress: level=" + ctx->currentLevel
                + ", objective=" + ctx->currentObjective;

    std::string retFmt = "Return JSON: {\"hint\":\"<text>\","
                         "\"difficultyLevel\":\"easy|medium|hard\","
                         "\"nextHintAvailable\":true|false}";

    std::string body = "{\"prompt\":\"" + json::escape(prompt)
                     + "\",\"return_format\":\"" + json::escape(retFmt) + "\"}";

    http::post(_apiUrl + "/prompts/get-custom-interrogation-response", body, _authToken,
        [this, onComplete, onError](const http::HttpResponse& resp) {
            _processing = false;
            if (!resp.success) {
                log("getHint failed: " + resp.error);
                if (onError) onError({static_cast<int>(resp.statusCode), resp.error});
                else if (onComplete) onComplete(nullptr);
                return;
            }
            std::string data = resp.body;
            std::string inner = json::getString(data, "result");
            if (!inner.empty()) data = inner;

            AIHintResponse result;
            result.hint              = json::getString(data, "hint");
            result.difficultyLevel   = json::getString(data, "difficultyLevel");
            result.nextHintAvailable = json::getBool(data, "nextHintAvailable");
            log("getHint completed");
            if (onComplete) onComplete(&result);
        });
}

// ---------------------------------------------------------------------------
// getTutorial  →  POST /prompts/get-custom-interrogation-response
// ---------------------------------------------------------------------------

void IVXAIAssistant::getTutorial(const std::string& featureId,
                                 std::function<void(const AITutorialResponse*)> onComplete,
                                 ErrorCb onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        else if (onComplete) onComplete(nullptr);
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        else if (onComplete) onComplete(nullptr);
        return;
    }

    _processing = true;

    std::string prompt = "Create a step-by-step tutorial for feature: " + featureId;
    std::string retFmt = "Return JSON: {\"featureId\":\"<id>\","
                         "\"steps\":[{\"stepNumber\":1,\"title\":\"<text>\","
                         "\"description\":\"<text>\"}],"
                         "\"estimatedTimeSeconds\":<int>}";

    std::string body = "{\"prompt\":\"" + json::escape(prompt)
                     + "\",\"return_format\":\"" + json::escape(retFmt) + "\"}";

    http::post(_apiUrl + "/prompts/get-custom-interrogation-response", body, _authToken,
        [this, featureId, onComplete, onError](const http::HttpResponse& resp) {
            _processing = false;
            if (!resp.success) {
                log("getTutorial failed: " + resp.error);
                if (onError) onError({static_cast<int>(resp.statusCode), resp.error});
                else if (onComplete) onComplete(nullptr);
                return;
            }
            std::string data = resp.body;
            std::string inner = json::getString(data, "result");
            if (!inner.empty()) data = inner;

            AITutorialResponse result;
            result.featureId = json::getString(data, "featureId");
            if (result.featureId.empty()) result.featureId = featureId;
            result.estimatedTimeSeconds = json::getInt(data, "estimatedTimeSeconds");

            auto stepElements = json::getObjectArray(data, "steps");
            for (const auto& elem : stepElements) {
                AITutorialStep step;
                step.stepNumber  = json::getInt(elem, "stepNumber");
                step.title       = json::getString(elem, "title");
                step.description = json::getString(elem, "description");
                result.steps.push_back(std::move(step));
            }
            log("getTutorial completed (" + std::to_string(result.steps.size()) + " steps)");
            if (onComplete) onComplete(&result);
        });
}

// ---------------------------------------------------------------------------
// searchKnowledgeBase  →  POST /prompts/search-web
// ---------------------------------------------------------------------------

void IVXAIAssistant::searchKnowledgeBase(const std::string& query,
                                         std::function<void(std::vector<std::string>)> onResults,
                                         ErrorCb onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        else if (onResults) onResults({});
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        else if (onResults) onResults({});
        return;
    }

    _processing = true;

    std::string body = "{\"prompt\":\"" + json::escape(query) + "\"}";

    http::post(_apiUrl + "/prompts/search-web", body, _authToken,
        [this, onResults, onError](const http::HttpResponse& resp) {
            _processing = false;
            if (!resp.success) {
                log("searchKnowledgeBase failed: " + resp.error);
                if (onError) onError({static_cast<int>(resp.statusCode), resp.error});
                else if (onResults) onResults({});
                return;
            }
            std::vector<std::string> results;
            auto sources = json::getStringArray(resp.body, "sources");
            if (!sources.empty()) {
                results = std::move(sources);
            } else {
                std::string text = json::getString(resp.body, "result");
                if (text.empty()) text = json::getString(resp.body, "response");
                if (!text.empty()) results.push_back(std::move(text));
            }
            log("searchKnowledgeBase found " + std::to_string(results.size()) + " results");
            if (onResults) onResults(std::move(results));
        });
}

// ---------------------------------------------------------------------------

void IVXAIAssistant::log(const std::string& msg) {
    std::cout << "[IVX:AIAssistant] " << msg << std::endl;
}

} // namespace ivx
