#include "intelliversex/ivx_manager.h"
#include <fstream>
#include <cstdio>
#include <random>
#include <sstream>
#include <stdexcept>
#include <iomanip>

namespace ivx {

static const char* FILE_SESSION = "ivx_session.dat";
static const char* FILE_DEVICE  = "ivx_device.dat";

Manager& Manager::instance() {
    static Manager inst;
    return inst;
}

// --- helpers ---

std::string Manager::sessionFilePath() const {
    return _cfg.storagePath + "/" + FILE_SESSION;
}

std::string Manager::deviceFilePath() const {
    return _cfg.storagePath + "/" + FILE_DEVICE;
}

std::string Manager::escapeJsonString(const std::string& input) {
    std::ostringstream ss;
    for (char c : input) {
        switch (c) {
            case '"':  ss << "\\\""; break;
            case '\\': ss << "\\\\"; break;
            case '\b': ss << "\\b";  break;
            case '\f': ss << "\\f";  break;
            case '\n': ss << "\\n";  break;
            case '\r': ss << "\\r";  break;
            case '\t': ss << "\\t";  break;
            default:
                if (static_cast<unsigned char>(c) < 0x20) {
                    ss << "\\u" << std::hex << std::setw(4) << std::setfill('0')
                       << static_cast<int>(c);
                } else {
                    ss << c;
                }
        }
    }
    return ss.str();
}

// --- init ---

void Manager::init(const Config& cfg) {
    cfg.validate();

    _cfg = cfg;
    Nakama::NClientParameters p;
    p.serverKey = cfg.serverKey;
    p.host = cfg.host;
    p.port = cfg.port;
    p.ssl = cfg.useSSL;

    _client = Nakama::createDefaultClient(p);
    if (!_client) {
        _init = false;
        log("ERROR: Nakama::createDefaultClient returned null");
        throw std::runtime_error("Failed to create Nakama client");
    }

    _init = true;
    log("SDK initialized — " + std::string(cfg.useSSL ? "https" : "http") + "://" + cfg.host + ":" + std::to_string(cfg.port));
}

// --- auth ---

void Manager::authDevice(const std::string& id, SuccessCb ok, ErrorCb err) {
    if (!_init) { if (err) err({-1, "Not initialized"}); return; }
    std::string rid = id.empty() ? deviceId() : id;
    _client->authenticateDevice(rid, Nakama::opt::nullopt, true, {},
        [this, ok](Nakama::NSessionPtr s) { onAuth(s); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::authEmail(const std::string& email, const std::string& pw, bool create, SuccessCb ok, ErrorCb err) {
    if (!_init) { if (err) err({-1, "Not initialized"}); return; }
    _client->authenticateEmail(email, pw, "", create, {},
        [this, ok](Nakama::NSessionPtr s) { onAuth(s); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::authGoogle(const std::string& token, SuccessCb ok, ErrorCb err) {
    if (!_init) { if (err) err({-1, "Not initialized"}); return; }
    _client->authenticateGoogle(token, "", true, {},
        [this, ok](Nakama::NSessionPtr s) { onAuth(s); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::authApple(const std::string& token, SuccessCb ok, ErrorCb err) {
    if (!_init) { if (err) err({-1, "Not initialized"}); return; }
    _client->authenticateApple(token, "", true, {},
        [this, ok](Nakama::NSessionPtr s) { onAuth(s); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::authCustom(const std::string& id, SuccessCb ok, ErrorCb err) {
    if (!_init) { if (err) err({-1, "Not initialized"}); return; }
    _client->authenticateCustom(id, "", true, {},
        [this, ok](Nakama::NSessionPtr s) { onAuth(s); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

// --- session ---

bool Manager::restoreSession() {
    load();
    if (_session && !_session->isExpired()) {
        log("Session restored: " + _session->getUserId());
        syncMeta();
        return true;
    }
    _session = nullptr;
    return false;
}

void Manager::clearSession() {
    _session = nullptr;
    std::remove(sessionFilePath().c_str());
    log("Session cleared");
}

bool Manager::hasSession() const { return _session && !_session->isExpired(); }
std::string Manager::userId()   const { return _session ? _session->getUserId()  : ""; }
std::string Manager::username() const { return _session ? _session->getUsername() : ""; }

// --- profile ---

void Manager::fetchProfile(ProfileCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    _client->getAccount(_session,
        [ok](const Nakama::NAccount& a) {
            Profile p{a.user.id, a.user.username, a.user.displayName, a.user.avatarUrl, a.user.langTag, a.user.metadata, a.wallet};
            if (ok) ok(p);
        },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::updateProfile(const std::string& dn, const std::string& av, const std::string& lt, SuccessCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    _client->updateAccount(_session, Nakama::opt::nullopt, dn, av, lt, Nakama::opt::nullopt,
        [this, ok]() { log("Profile updated"); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

// --- wallet ---

void Manager::fetchWallet(StringCb ok, ErrorCb err) {
    rpc("hiro_economy_list", "{}", ok, err);
}

void Manager::grantCurrency(const std::string& cid, int64_t amt, StringCb ok, ErrorCb err) {
    std::string safeCid = escapeJsonString(cid);
    rpc("hiro_economy_grant",
        "{\"currencies\":{\"" + safeCid + "\":" + std::to_string(amt) + "}}",
        ok, err);
}

// --- leaderboard ---

void Manager::submitScore(const std::string& lid, int64_t score, SuccessCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    _client->writeLeaderboardRecord(_session, lid, score, Nakama::opt::nullopt, Nakama::opt::nullopt, Nakama::opt::nullopt,
        [this, ok, lid, score](const Nakama::NLeaderboardRecord&) { log("Score " + std::to_string(score) + " -> " + lid); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::fetchLeaderboard(const std::string& lid, int limit, LeaderboardCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    _client->listLeaderboardRecords(_session, lid, {}, limit, Nakama::opt::nullopt,
        [ok](Nakama::NLeaderboardRecordListPtr list) {
            std::vector<LeaderboardRecord> out;
            if (list) {
                for (auto& r : list->records) {
                    LeaderboardRecord rec;
                    rec.ownerId = r.ownerId;
                    rec.username = r.username.has_value() ? r.username.value() : "";
                    rec.score = r.score;
                    rec.rank = r.rank;
                    out.push_back(rec);
                }
            }
            if (ok) ok(out);
        },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

// --- storage ---

void Manager::writeStorage(const std::string& col, const std::string& key, const std::string& json, SuccessCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    Nakama::NStorageObjectWrite w;
    w.collection = col; w.key = key; w.value = json;
    w.permissionRead = 1; w.permissionWrite = 1;
    _client->writeStorageObjects(_session, {w},
        [this, ok, col, key](const Nakama::NStorageObjectAcks&) { log("Write " + col + "/" + key); if (ok) ok(); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

void Manager::readStorage(const std::string& col, const std::string& key, StringCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    Nakama::NReadStorageObjectId r;
    r.collection = col; r.key = key; r.userId = userId();
    _client->readStorageObjects(_session, {r},
        [ok](const Nakama::NStorageObjects& objs) { if (ok) ok(objs.empty() ? "{}" : objs[0].value); },
        [err](const Nakama::NError& e) { if (err) err({e.code, e.message}); });
}

// --- rpc ---

void Manager::rpc(const std::string& id, const std::string& payload, StringCb ok, ErrorCb err) {
    if (!hasSession()) { if (err) err({-1, "No session"}); return; }
    _client->rpc(_session, id, payload,
        [this, ok, id](const Nakama::NRpc& r) { log("RPC " + id + " OK"); if (ok) ok(r.payload); },
        [this, err, id](const Nakama::NError& e) { log("RPC " + id + " FAIL: " + e.message); if (err) err({e.code, e.message}); });
}

// --- tick ---

void Manager::tick() {
    if (_client) _client->tick();
}

// --- internal ---

void Manager::onAuth(Nakama::NSessionPtr s) {
    _session = s;
    save();
    log("Auth OK: " + s->getUserId());
    syncMeta();
}

void Manager::save() {
    if (!_session) return;
    std::ofstream f(sessionFilePath());
    if (f) {
        f << _session->getAuthToken() << "\n" << _session->getRefreshToken();
        f.flush();
    }
}

void Manager::load() {
    std::ifstream f(sessionFilePath());
    if (f) {
        std::string t, r;
        std::getline(f, t);
        std::getline(f, r);
        if (!t.empty()) _session = Nakama::restoreSession(t, r);
    }
}

std::string Manager::deviceId() {
    {
        std::ifstream f(deviceFilePath());
        if (f) {
            std::string id;
            std::getline(f, id);
            if (!id.empty()) return id;
        }
    }

    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<uint32_t> dist16(0, 0xFFFF);
    std::uniform_int_distribution<uint32_t> dist32(0, 0xFFFFFFFF);

    auto hex = [](uint32_t v, int width) {
        std::ostringstream ss;
        ss << std::hex << std::setw(width) << std::setfill('0') << v;
        return ss.str();
    };

    std::string id =
        hex(dist32(gen), 8) + "-" +
        hex(dist16(gen), 4) + "-4" +
        hex(dist16(gen) & 0x0FFF, 3) + "-" +
        hex((dist16(gen) & 0x3FFF) | 0x8000, 4) + "-" +
        hex(dist32(gen), 8) + hex(dist16(gen), 4);

    {
        std::ofstream o(deviceFilePath());
        if (o) { o << id; o.flush(); }
    }
    return id;
}

void Manager::syncMeta() {
    if (!hasSession()) return;
    rpc("ivx_sync_metadata",
        "{\"metadata\":{\"sdk_version\":\"" + std::string(VERSION)
        + "\",\"engine\":\"cpp\",\"platform\":\"native\"}}");
}

void Manager::log(const std::string& msg) {
    if (_cfg.debugLogs) std::printf("[IntelliVerseX] %s\n", msg.c_str());
}

} // namespace ivx
