// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIAssistant.h"
#include "cocos2d.h"
#include "network/HttpClient.h"
#include "network/HttpRequest.h"
#include "network/HttpResponse.h"
#include "json/rapidjson.h"
#include "json/document.h"
#include "json/writer.h"
#include "json/stringbuffer.h"
#include <chrono>
#include <random>
#include <sstream>

namespace IntelliVerseX {

static std::string generateSessionId() {
    auto now = std::chrono::system_clock::now().time_since_epoch();
    auto ms  = std::chrono::duration_cast<std::chrono::milliseconds>(now).count();
    std::mt19937 rng(static_cast<unsigned>(ms));
    std::uniform_int_distribution<int> dist(1000, 9999);
    std::ostringstream oss;
    oss << "sess_" << ms << "_" << dist(rng);
    return oss.str();
}

IVXAIAssistant& IVXAIAssistant::getInstance() {
    static IVXAIAssistant instance;
    return instance;
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

void IVXAIAssistant::ask(const std::string& question, const IVXAIGameContext* ctx,
                         std::function<void(const IVXAIAssistantResponse&)> onComplete,
                         ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        return;
    }

    _processing = true;

    std::string prompt = question;
    if (!systemPrompt.empty())
        prompt = systemPrompt + "\n\n" + prompt;
    if (ctx)
        prompt += "\n[Context: level=" + ctx->currentLevel
                + ", objective=" + ctx->currentObjective + "]";

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("prompt");    w.String(prompt.c_str());
    w.Key("sessionId"); w.String(_sessionId.c_str());
    w.EndObject();

    httpPost("/chat/response", sb.GetString(),
        [this, onComplete](const std::string& response) {
            _processing = false;
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXAIAssistantResponse result;
            if (!doc.HasParseError() && doc.IsObject()) {
                if (doc.HasMember("response") && doc["response"].IsString())
                    result.response = doc["response"].GetString();
                else if (doc.HasMember("result") && doc["result"].IsString())
                    result.response = doc["result"].GetString();
                if (doc.HasMember("sources") && doc["sources"].IsArray()) {
                    for (rapidjson::SizeType i = 0; i < doc["sources"].Size(); ++i) {
                        if (doc["sources"][i].IsString())
                            result.sources.push_back(doc["sources"][i].GetString());
                    }
                }
                result.confidence = 1.0f;
            }
            log("ask completed (" + std::to_string(result.response.size()) + " chars)");
            if (onComplete) onComplete(result);
        },
        [this, onError](const IVXError& err) {
            _processing = false;
            log("ask failed: " + err.message);
            if (onError) onError(err);
        });
}

// ---------------------------------------------------------------------------
// getHint  →  POST /prompts/get-custom-interrogation-response
// ---------------------------------------------------------------------------

void IVXAIAssistant::getHint(const std::string& levelId, const std::string& objectiveId,
                             const IVXAIGameContext* ctx,
                             std::function<void(const IVXAIHintResponse&)> onComplete,
                             ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        return;
    }

    _processing = true;

    std::string prompt = "Give a hint for level " + levelId
                       + " with objective: " + objectiveId;
    if (ctx)
        prompt += ". Current progress: level=" + ctx->currentLevel
                + ", objective=" + ctx->currentObjective;

    std::string retFmt = "Return JSON: {\"hint\":\"<text>\"}";

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("prompt");        w.String(prompt.c_str());
    w.Key("return_format"); w.String(retFmt.c_str());
    w.EndObject();

    httpPost("/prompts/get-custom-interrogation-response", sb.GetString(),
        [this, onComplete](const std::string& response) {
            _processing = false;
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXAIHintResponse result;
            if (!doc.HasParseError() && doc.IsObject()) {
                if (doc.HasMember("result") && doc["result"].IsString()) {
                    rapidjson::Document inner;
                    inner.Parse(doc["result"].GetString());
                    if (!inner.HasParseError() && inner.IsObject()) {
                        if (inner.HasMember("hint") && inner["hint"].IsString())
                            result.hint = inner["hint"].GetString();
                    }
                } else if (doc.HasMember("hint") && doc["hint"].IsString()) {
                    result.hint = doc["hint"].GetString();
                }
            }
            log("getHint completed");
            if (onComplete) onComplete(result);
        },
        [this, onError](const IVXError& err) {
            _processing = false;
            log("getHint failed: " + err.message);
            if (onError) onError(err);
        });
}

// ---------------------------------------------------------------------------
// getTutorial  →  POST /prompts/get-custom-interrogation-response
// ---------------------------------------------------------------------------

void IVXAIAssistant::getTutorial(const std::string& featureId,
                                 std::function<void(const IVXAITutorialResponse&)> onComplete,
                                 ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        return;
    }

    _processing = true;

    std::string prompt = "Create a step-by-step tutorial for feature: " + featureId;
    std::string retFmt = "Return JSON: {\"featureId\":\"<id>\"}";

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("prompt");        w.String(prompt.c_str());
    w.Key("return_format"); w.String(retFmt.c_str());
    w.EndObject();

    httpPost("/prompts/get-custom-interrogation-response", sb.GetString(),
        [this, featureId, onComplete](const std::string& response) {
            _processing = false;
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXAITutorialResponse result;
            result.featureId = featureId;
            if (!doc.HasParseError() && doc.IsObject()) {
                if (doc.HasMember("result") && doc["result"].IsString()) {
                    rapidjson::Document inner;
                    inner.Parse(doc["result"].GetString());
                    if (!inner.HasParseError() && inner.IsObject()) {
                        if (inner.HasMember("featureId") && inner["featureId"].IsString())
                            result.featureId = inner["featureId"].GetString();
                    }
                }
            }
            log("getTutorial completed");
            if (onComplete) onComplete(result);
        },
        [this, onError](const IVXError& err) {
            _processing = false;
            log("getTutorial failed: " + err.message);
            if (onError) onError(err);
        });
}

// ---------------------------------------------------------------------------
// searchKnowledgeBase  →  POST /prompts/search-web
// ---------------------------------------------------------------------------

void IVXAIAssistant::searchKnowledgeBase(const std::string& query,
                                         std::function<void(const std::vector<std::string>&)> onResults,
                                         ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "IVXAIAssistant not initialized"});
        return;
    }
    if (_processing) {
        if (onError) onError({-2, "Request already in progress"});
        return;
    }

    _processing = true;

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("prompt"); w.String(query.c_str());
    w.EndObject();

    httpPost("/prompts/search-web", sb.GetString(),
        [this, onResults](const std::string& response) {
            _processing = false;
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            std::vector<std::string> results;
            if (!doc.HasParseError() && doc.IsObject()) {
                if (doc.HasMember("sources") && doc["sources"].IsArray()) {
                    for (rapidjson::SizeType i = 0; i < doc["sources"].Size(); ++i) {
                        if (doc["sources"][i].IsString())
                            results.push_back(doc["sources"][i].GetString());
                    }
                }
                if (results.empty()) {
                    std::string text;
                    if (doc.HasMember("result") && doc["result"].IsString())
                        text = doc["result"].GetString();
                    else if (doc.HasMember("response") && doc["response"].IsString())
                        text = doc["response"].GetString();
                    if (!text.empty())
                        results.push_back(std::move(text));
                }
            }
            log("searchKnowledgeBase found " + std::to_string(results.size()) + " results");
            if (onResults) onResults(results);
        },
        [this, onError](const IVXError& err) {
            _processing = false;
            log("searchKnowledgeBase failed: " + err.message);
            if (onError) onError(err);
        });
}

// ---------------------------------------------------------------------------
// HTTP helper (mirrors IVXAIClient pattern)
// ---------------------------------------------------------------------------

void IVXAIAssistant::httpPost(const std::string& path, const std::string& bodyJson,
                              std::function<void(const std::string&)> onSuccess,
                              ErrorCallback onError) {
    auto request = new (std::nothrow) cocos2d::network::HttpRequest();
    if (!request) return;

    request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
    request->setUrl((_apiUrl + path).c_str());
    request->setRequestData(bodyJson.c_str(), bodyJson.size());

    std::vector<std::string> headers;
    headers.push_back("Content-Type: application/json");
    if (!_authToken.empty())
        headers.push_back("Authorization: Bearer " + _authToken);
    request->setHeaders(headers);

    request->setResponseCallback([this, onSuccess, onError](
            cocos2d::network::HttpClient* /*client*/,
            cocos2d::network::HttpResponse* resp) {
        if (!resp || !resp->isSucceed()) {
            std::string errMsg = resp ? resp->getErrorBuffer() : "Request failed";
            long statusCode = resp ? resp->getResponseCode() : -1;
            log("HTTP error: " + errMsg);
            if (onError) onError({static_cast<int>(statusCode), errMsg});
            return;
        }
        auto* data = resp->getResponseData();
        std::string body(data->begin(), data->end());
        if (onSuccess) onSuccess(body);
    });

    cocos2d::network::HttpClient::getInstance()->send(request);
    request->release();
}

// ---------------------------------------------------------------------------

void IVXAIAssistant::log(const std::string& msg) {
    cocos2d::log("[IntelliVerseX:AIAssistant] %s", msg.c_str());
}

} // namespace IntelliVerseX
