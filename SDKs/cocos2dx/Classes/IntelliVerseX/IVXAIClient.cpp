// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIClient.h"
#include "cocos2d.h"
#include "network/HttpClient.h"
#include "network/HttpRequest.h"
#include "network/HttpResponse.h"
#include "json/rapidjson.h"
#include "json/document.h"
#include "json/writer.h"
#include "json/stringbuffer.h"

namespace IntelliVerseX {

IVXAIClient& IVXAIClient::getInstance() {
    static IVXAIClient instance;
    return instance;
}

void IVXAIClient::initialize(const std::string& apiBaseUrl,
                              const std::string& apiKey,
                              bool enableDebugLogs) {
    _apiBaseUrl = apiBaseUrl;
    _apiKey = apiKey;
    _enableDebugLogs = enableDebugLogs;
    _initialized = true;
    log("AI client initialized — " + apiBaseUrl);
}

void IVXAIClient::startVoiceSession(const std::string& personaId,
                                     const std::string& userId,
                                     AISessionCallback onSuccess,
                                     ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("personaId"); w.String(personaId.c_str());
    w.Key("userId"); w.String(userId.c_str());
    w.EndObject();

    httpPost("/ai-voice/session", sb.GetString(),
        [this, onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            if (doc.HasParseError() || !doc.IsObject()) {
                return;
            }
            AISessionResponse session;
            session.sessionId = doc.HasMember("sessionId") ? doc["sessionId"].GetString() : "";
            session.personaId = doc.HasMember("personaId") ? doc["personaId"].GetString() : "";
            session.userId = doc.HasMember("userId") ? doc["userId"].GetString() : "";
            session.status = doc.HasMember("status") ? doc["status"].GetString() : "";
            session.createdAt = doc.HasMember("createdAt") ? doc["createdAt"].GetInt64() : 0;
            log("Voice session started: " + session.sessionId);
            if (onSuccess) onSuccess(session);
        }, onError);
}

void IVXAIClient::endVoiceSession(const std::string& sessionId,
                                   SuccessCallback onSuccess,
                                   ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    httpPost("/ai-voice/session/" + sessionId + "/end", "{}",
        [this, sessionId, onSuccess](const std::string&) {
            log("Voice session ended: " + sessionId);
            if (onSuccess) onSuccess();
        }, onError);
}

void IVXAIClient::sendText(const std::string& sessionId,
                            const std::string& text,
                            AIMessageCallback onSuccess,
                            ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("text"); w.String(text.c_str());
    w.EndObject();

    httpPost("/ai-voice/session/" + sessionId + "/text", sb.GetString(),
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            if (doc.HasParseError() || !doc.IsObject()) {
                return;
            }
            AIMessage msg;
            msg.id = doc.HasMember("id") ? doc["id"].GetString() : "";
            msg.sessionId = doc.HasMember("sessionId") ? doc["sessionId"].GetString() : "";
            msg.role = doc.HasMember("role") ? doc["role"].GetString() : "";
            msg.text = doc.HasMember("text") ? doc["text"].GetString() : "";
            msg.timestamp = doc.HasMember("timestamp") ? doc["timestamp"].GetInt64() : 0;
            if (onSuccess) onSuccess(msg);
        }, onError);
}

void IVXAIClient::startHostSession(const std::string& matchId,
                                    const HostProfile& profile,
                                    AISessionCallback onSuccess,
                                    ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("matchId"); w.String(matchId.c_str());
    w.Key("profile");
    w.StartObject();
    w.Key("displayName"); w.String(profile.displayName.c_str());
    if (!profile.avatarUrl.empty()) {
        w.Key("avatarUrl"); w.String(profile.avatarUrl.c_str());
    }
    if (!profile.metadata.empty()) {
        w.Key("metadata"); w.String(profile.metadata.c_str());
    }
    w.EndObject();
    w.EndObject();

    httpPost("/ai-host/session", sb.GetString(),
        [this, onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            if (doc.HasParseError() || !doc.IsObject()) {
                return;
            }
            AISessionResponse session;
            session.sessionId = doc.HasMember("sessionId") ? doc["sessionId"].GetString() : "";
            session.personaId = doc.HasMember("personaId") ? doc["personaId"].GetString() : "";
            session.userId = doc.HasMember("userId") ? doc["userId"].GetString() : "";
            session.status = doc.HasMember("status") ? doc["status"].GetString() : "";
            session.createdAt = doc.HasMember("createdAt") ? doc["createdAt"].GetInt64() : 0;
            log("Host session started: " + session.sessionId);
            if (onSuccess) onSuccess(session);
        }, onError);
}

void IVXAIClient::sendHostEvent(const std::string& sessionId,
                                 const std::string& eventType,
                                 const std::string& data,
                                 SuccessCallback onSuccess,
                                 ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    rapidjson::StringBuffer sb;
    rapidjson::Writer<rapidjson::StringBuffer> w(sb);
    w.StartObject();
    w.Key("eventType"); w.String(eventType.c_str());
    w.Key("data"); w.String(data.c_str());
    w.EndObject();

    httpPost("/ai-host/session/" + sessionId + "/event", sb.GetString(),
        [this, eventType, onSuccess](const std::string&) {
            log("Host event sent: " + eventType);
            if (onSuccess) onSuccess();
        }, onError);
}

void IVXAIClient::checkEntitlement(const std::string& userId,
                                    AIEntitlementCallback onSuccess,
                                    ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    httpGet("/ai-voice/entitlement/" + userId,
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            if (doc.HasParseError() || !doc.IsObject()) {
                return;
            }
            AIEntitlement ent;
            ent.userId = doc.HasMember("userId") ? doc["userId"].GetString() : "";
            ent.entitled = doc.HasMember("entitled") ? doc["entitled"].GetBool() : false;
            ent.remainingCredits = doc.HasMember("remainingCredits") ? doc["remainingCredits"].GetInt() : 0;
            ent.plan = doc.HasMember("plan") ? doc["plan"].GetString() : "";
            if (onSuccess) onSuccess(ent);
        }, onError);
}

void IVXAIClient::getPersonas(AIPersonasCallback onSuccess,
                               ErrorCallback onError) {
    if (!_initialized) {
        if (onError) onError({-1, "AI client not initialized"});
        return;
    }

    httpGet("/ai-voice/personas",
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            std::vector<AIPersona> personas;
            if (!doc.HasParseError() && doc.IsArray()) {
                for (rapidjson::SizeType i = 0; i < doc.Size(); ++i) {
                    const auto& obj = doc[i];
                    if (!obj.IsObject()) continue;
                    AIPersona p;
                    p.id = obj.HasMember("id") ? obj["id"].GetString() : "";
                    p.name = obj.HasMember("name") ? obj["name"].GetString() : "";
                    p.description = obj.HasMember("description") ? obj["description"].GetString() : "";
                    p.avatarUrl = obj.HasMember("avatarUrl") ? obj["avatarUrl"].GetString() : "";
                    if (obj.HasMember("supportedLanguages") && obj["supportedLanguages"].IsArray()) {
                        for (rapidjson::SizeType j = 0; j < obj["supportedLanguages"].Size(); ++j) {
                            p.supportedLanguages.push_back(obj["supportedLanguages"][j].GetString());
                        }
                    }
                    personas.push_back(p);
                }
            }
            if (onSuccess) onSuccess(personas);
        }, onError);
}

// ---------------------------------------------------------------------------
// HTTP helpers (cocos2d::network::HttpClient)
// ---------------------------------------------------------------------------

void IVXAIClient::httpPost(const std::string& path,
                            const std::string& bodyJson,
                            std::function<void(const std::string&)> onSuccess,
                            ErrorCallback onError) {
    auto request = new (std::nothrow) cocos2d::network::HttpRequest();
    if (!request) return;

    request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
    request->setUrl((_apiBaseUrl + path).c_str());
    request->setRequestData(bodyJson.c_str(), bodyJson.size());

    std::vector<std::string> headers;
    headers.push_back("Content-Type: application/json");
    headers.push_back("Authorization: Bearer " + _apiKey);
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

void IVXAIClient::httpGet(const std::string& path,
                           std::function<void(const std::string&)> onSuccess,
                           ErrorCallback onError) {
    auto request = new (std::nothrow) cocos2d::network::HttpRequest();
    if (!request) return;

    request->setRequestType(cocos2d::network::HttpRequest::Type::GET);
    request->setUrl((_apiBaseUrl + path).c_str());

    std::vector<std::string> headers;
    headers.push_back("Content-Type: application/json");
    headers.push_back("Authorization: Bearer " + _apiKey);
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

void IVXAIClient::log(const std::string& message) {
    if (_enableDebugLogs) {
        cocos2d::log("[IntelliVerseX:AI] %s", message.c_str());
    }
}

} // namespace IntelliVerseX
