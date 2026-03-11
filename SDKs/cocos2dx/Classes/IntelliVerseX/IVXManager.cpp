#include "IntelliVerseX/IVXManager.h"
#include "cocos2d.h"
#include <cstdio>
#include <fstream>
#include <random>
#include <sstream>
#include <iomanip>
#include <algorithm>

namespace IntelliVerseX {

static const std::string SESSION_FILENAME = "ivx_session.dat";
static const std::string DEVICE_ID_FILENAME = "ivx_device.dat";

IVXManager& IVXManager::getInstance() {
    static IVXManager instance;
    return instance;
}

void IVXManager::initialize(const IVXConfig& config) {
    _config = config;

    Nakama::NClientParameters params;
    params.serverKey = config.nakamaServerKey;
    params.host = config.nakamaHost;
    params.port = config.nakamaPort;
    params.ssl = config.useSSL;

    _client = Nakama::createDefaultClient(params);

    if (!_client) {
        log("ERROR: Failed to create Nakama client");
        return;
    }

    _initialized = true;
    log("SDK initialized — " + config.getBaseUrl());
}

void IVXManager::authenticateDevice(const std::string& deviceId,
                                     SuccessCallback onSuccess,
                                     ErrorCallback onError) {
    if (!_initialized || !_client) {
        if (onError) onError({-1, "SDK not initialized"});
        return;
    }

    std::string resolvedId = deviceId.empty() ? getPersistentDeviceId() : deviceId;

    auto successCb = [this, onSuccess](Nakama::NSessionPtr session) {
        onAuthSuccess(session);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [this, onError](const Nakama::NError& error) {
        log("Auth failed: " + error.message);
        if (onError) onError({error.code, error.message});
    };

    _client->authenticateDevice(resolvedId, Nakama::opt::nullopt, true, {}, successCb, errorCb);
}

void IVXManager::authenticateEmail(const std::string& email,
                                    const std::string& password,
                                    bool create,
                                    SuccessCallback onSuccess,
                                    ErrorCallback onError) {
    if (!_initialized || !_client) {
        if (onError) onError({-1, "SDK not initialized"});
        return;
    }

    auto successCb = [this, onSuccess](Nakama::NSessionPtr session) {
        onAuthSuccess(session);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [this, onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->authenticateEmail(email, password, "", create, {}, successCb, errorCb);
}

void IVXManager::authenticateGoogle(const std::string& token,
                                     SuccessCallback onSuccess,
                                     ErrorCallback onError) {
    if (!_initialized || !_client) {
        if (onError) onError({-1, "SDK not initialized"});
        return;
    }

    auto successCb = [this, onSuccess](Nakama::NSessionPtr session) {
        onAuthSuccess(session);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [this, onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->authenticateGoogle(token, "", true, {}, successCb, errorCb);
}

void IVXManager::authenticateApple(const std::string& token,
                                    SuccessCallback onSuccess,
                                    ErrorCallback onError) {
    if (!_initialized || !_client) {
        if (onError) onError({-1, "SDK not initialized"});
        return;
    }

    auto successCb = [this, onSuccess](Nakama::NSessionPtr session) {
        onAuthSuccess(session);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [this, onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->authenticateApple(token, "", true, {}, successCb, errorCb);
}

void IVXManager::authenticateCustom(const std::string& customId,
                                     SuccessCallback onSuccess,
                                     ErrorCallback onError) {
    if (!_initialized || !_client) {
        if (onError) onError({-1, "SDK not initialized"});
        return;
    }

    auto successCb = [this, onSuccess](Nakama::NSessionPtr session) {
        onAuthSuccess(session);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [this, onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->authenticateCustom(customId, "", true, {}, successCb, errorCb);
}

bool IVXManager::restoreSession() {
    if (!_initialized || !_client) {
        log("Cannot restore session — SDK not initialized");
        return false;
    }

    loadSession();
    if (_session && !_session->isExpired()) {
        log("Session restored for user: " + _session->getUserId());
        syncMetadata();
        return true;
    }
    _session = nullptr;
    return false;
}

void IVXManager::clearSession() {
    _session = nullptr;
    std::string path = getWritablePath(SESSION_FILENAME);
    std::remove(path.c_str());
    log("Session cleared");
}

bool IVXManager::hasValidSession() const {
    return _session && !_session->isExpired();
}

std::string IVXManager::getUserId() const {
    return _session ? _session->getUserId() : "";
}

std::string IVXManager::getUsername() const {
    return _session ? _session->getUsername() : "";
}

void IVXManager::fetchProfile(ProfileCallback onSuccess, ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    auto successCb = [this, onSuccess](const Nakama::NAccount& account) {
        IVXProfile profile;
        profile.userId = account.user.id;
        profile.username = account.user.username;
        profile.displayName = account.user.displayName;
        profile.avatarUrl = account.user.avatarUrl;
        profile.langTag = account.user.langTag;
        profile.metadata = account.user.metadata;
        profile.wallet = account.wallet;
        if (onSuccess) onSuccess(profile);
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->getAccount(_session, successCb, errorCb);
}

void IVXManager::updateProfile(const std::string& displayName,
                                const std::string& avatarUrl,
                                const std::string& langTag,
                                SuccessCallback onSuccess,
                                ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    auto successCb = [this, onSuccess]() {
        log("Profile updated");
        if (onSuccess) onSuccess();
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->updateAccount(_session, Nakama::opt::nullopt, displayName, avatarUrl, langTag, Nakama::opt::nullopt, successCb, errorCb);
}

void IVXManager::fetchWallet(WalletCallback onSuccess, ErrorCallback onError) {
    callRpc("hiro_economy_list", "{}", [onSuccess](const std::string& result) {
        if (onSuccess) onSuccess(result);
    }, onError);
}

void IVXManager::grantCurrency(const std::string& currencyId,
                                int64_t amount,
                                WalletCallback onSuccess,
                                ErrorCallback onError) {
    if (!isAlphanumericOrUnderscore(currencyId)) {
        if (onError) onError({-1, "Invalid currency ID — only alphanumeric and underscore characters are allowed"});
        return;
    }

    std::string payload = "{\"currencies\":{\"" + currencyId + "\":" + std::to_string(amount) + "}}";
    callRpc("hiro_economy_grant", payload, [onSuccess](const std::string& result) {
        if (onSuccess) onSuccess(result);
    }, onError);
}

void IVXManager::submitScore(const std::string& leaderboardId,
                              int64_t score,
                              SuccessCallback onSuccess,
                              ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    auto successCb = [this, leaderboardId, score, onSuccess](const Nakama::NLeaderboardRecord&) {
        log("Score submitted: " + std::to_string(score) + " to " + leaderboardId);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->writeLeaderboardRecord(_session, leaderboardId, score, Nakama::opt::nullopt, Nakama::opt::nullopt, Nakama::opt::nullopt, successCb, errorCb);
}

void IVXManager::fetchLeaderboard(const std::string& leaderboardId,
                                   int limit,
                                   LeaderboardCallback onSuccess,
                                   ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    auto successCb = [onSuccess](Nakama::NLeaderboardRecordListPtr list) {
        std::vector<IVXLeaderboardRecord> records;
        if (list) {
            for (auto& r : list->records) {
                IVXLeaderboardRecord record;
                record.ownerId = r.ownerId;
                record.username = r.username.has_value() ? r.username.value() : "";
                record.score = r.score;
                record.rank = r.rank;
                records.push_back(record);
            }
        }
        if (onSuccess) onSuccess(records);
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->listLeaderboardRecords(_session, leaderboardId, {}, limit, Nakama::opt::nullopt, successCb, errorCb);
}

void IVXManager::writeStorage(const std::string& collection,
                               const std::string& key,
                               const std::string& valueJson,
                               SuccessCallback onSuccess,
                               ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    Nakama::NStorageObjectWrite writeObj;
    writeObj.collection = collection;
    writeObj.key = key;
    writeObj.value = valueJson;
    writeObj.permissionRead = 1;
    writeObj.permissionWrite = 1;

    auto successCb = [this, collection, key, onSuccess](const Nakama::NStorageObjectAcks&) {
        log("Storage write: " + collection + "/" + key);
        if (onSuccess) onSuccess();
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->writeStorageObjects(_session, {writeObj}, successCb, errorCb);
}

void IVXManager::readStorage(const std::string& collection,
                              const std::string& key,
                              StorageCallback onSuccess,
                              ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    Nakama::NReadStorageObjectId readId;
    readId.collection = collection;
    readId.key = key;
    readId.userId = getUserId();

    auto successCb = [onSuccess](const Nakama::NStorageObjects& objects) {
        if (!objects.empty()) {
            if (onSuccess) onSuccess(objects[0].value);
        } else {
            if (onSuccess) onSuccess("{}");
        }
    };

    auto errorCb = [onError](const Nakama::NError& error) {
        if (onError) onError({error.code, error.message});
    };

    _client->readStorageObjects(_session, {readId}, successCb, errorCb);
}

void IVXManager::callRpc(const std::string& rpcId,
                          const std::string& payloadJson,
                          RpcCallback onSuccess,
                          ErrorCallback onError) {
    if (!hasValidSession()) {
        if (onError) onError({-1, "No valid session"});
        return;
    }

    auto successCb = [this, rpcId, onSuccess](const Nakama::NRpc& rpc) {
        log("RPC " + rpcId + " response received");
        if (onSuccess) onSuccess(rpc.payload);
    };

    auto errorCb = [this, rpcId, onError](const Nakama::NError& error) {
        log("RPC " + rpcId + " failed: " + error.message);
        if (onError) onError({error.code, error.message});
    };

    _client->rpc(_session, rpcId, payloadJson, successCb, errorCb);
}

void IVXManager::tick() {
    if (_client) {
        _client->tick();
    }
}

void IVXManager::onAuthSuccess(Nakama::NSessionPtr session) {
    _session = session;
    saveSession();
    log("Authenticated — UserId: " + session->getUserId());
    syncMetadata();
}

void IVXManager::saveSession() {
    if (!_session) return;
    std::string path = getWritablePath(SESSION_FILENAME);
    std::ofstream out(path);
    if (out.is_open()) {
        out << _session->getAuthToken() << "\n" << _session->getRefreshToken();
        out.close();
    }
}

void IVXManager::loadSession() {
    std::string path = getWritablePath(SESSION_FILENAME);
    std::ifstream in(path);
    if (in.is_open()) {
        std::string token, refresh;
        std::getline(in, token);
        std::getline(in, refresh);
        in.close();
        if (!token.empty()) {
            _session = Nakama::restoreSession(token, refresh);
        }
    }
}

std::string IVXManager::getPersistentDeviceId() {
    std::string path = getWritablePath(DEVICE_ID_FILENAME);
    std::ifstream in(path);
    if (in.is_open()) {
        std::string id;
        std::getline(in, id);
        in.close();
        if (!id.empty()) return id;
    }

    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<uint32_t> dist(0, 0xFFFFFFFF);

    std::ostringstream oss;
    oss << std::hex << std::setfill('0');
    oss << std::setw(8) << dist(gen) << "-";
    oss << std::setw(4) << (dist(gen) & 0xFFFF) << "-4";
    oss << std::setw(3) << (dist(gen) & 0xFFF) << "-";
    oss << std::setw(4) << ((dist(gen) & 0x3FFF) | 0x8000) << "-";
    oss << std::setw(8) << dist(gen);
    oss << std::setw(4) << (dist(gen) & 0xFFFF);
    std::string id = oss.str();

    std::ofstream out(path);
    if (out.is_open()) {
        out << id;
        out.close();
    }
    return id;
}

std::string IVXManager::getWritablePath(const std::string& filename) {
    return cocos2d::FileUtils::getInstance()->getWritablePath() + filename;
}

void IVXManager::syncMetadata() {
    if (!hasValidSession()) return;
    std::string payload = "{\"metadata\":{\"sdk_version\":\"" + std::string(SDK_VERSION) +
                          "\",\"engine\":\"cocos2dx\",\"platform\":\"native\"}}";
    callRpc("ivx_sync_metadata", payload);
}

void IVXManager::log(const std::string& message) {
    if (_config.enableDebugLogs) {
        cocos2d::log("[IntelliVerseX] %s", message.c_str());
    }
}

bool IVXManager::isAlphanumericOrUnderscore(const std::string& s) {
    return !s.empty() && std::all_of(s.begin(), s.end(), [](char c) {
        return std::isalnum(static_cast<unsigned char>(c)) || c == '_';
    });
}

} // namespace IntelliVerseX
