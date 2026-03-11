#pragma once

#include "ivx_config.h"
#include "ivx_types.h"
#include "nakama-cpp/Nakama.h"
#include <memory>

namespace ivx {

/// Central manager for the IntelliVerseX C/C++ SDK.
/// Wraps Nakama C++ client with auth, profile, wallet, leaderboards, storage, and RPC.
///
/// Thread-safety: The Manager singleton and all its public methods must be called
/// from the same thread (typically the main/game thread). Callback invocations happen
/// inside tick(), so they also execute on the calling thread. If you need cross-thread
/// access, protect calls with an external mutex.
class Manager {
public:
    static constexpr const char* VERSION = "5.1.0";

    static Manager& instance();

    /// Initialise the SDK with the given configuration.
    /// Validates the config (port range, non-empty host, etc.) and creates the
    /// underlying Nakama client. Throws std::invalid_argument on bad config and
    /// std::runtime_error if the Nakama client could not be created.
    void init(const Config& cfg);
    bool initialized() const { return _init; }

    void authDevice(const std::string& deviceId = "", SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void authEmail(const std::string& email, const std::string& password, bool create = false, SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void authGoogle(const std::string& token, SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void authApple(const std::string& token, SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void authCustom(const std::string& id, SuccessCb ok = nullptr, ErrorCb err = nullptr);

    bool restoreSession();
    void clearSession();
    bool hasSession() const;

    std::string userId() const;
    std::string username() const;

    void fetchProfile(ProfileCb ok = nullptr, ErrorCb err = nullptr);
    void updateProfile(const std::string& displayName, const std::string& avatarUrl = "", const std::string& langTag = "", SuccessCb ok = nullptr, ErrorCb err = nullptr);

    void fetchWallet(StringCb ok = nullptr, ErrorCb err = nullptr);
    void grantCurrency(const std::string& currencyId, int64_t amount, StringCb ok = nullptr, ErrorCb err = nullptr);

    void submitScore(const std::string& leaderboardId, int64_t score, SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void fetchLeaderboard(const std::string& leaderboardId, int limit = 20, LeaderboardCb ok = nullptr, ErrorCb err = nullptr);

    void writeStorage(const std::string& collection, const std::string& key, const std::string& json, SuccessCb ok = nullptr, ErrorCb err = nullptr);
    void readStorage(const std::string& collection, const std::string& key, StringCb ok = nullptr, ErrorCb err = nullptr);

    void rpc(const std::string& id, const std::string& payload = "{}", StringCb ok = nullptr, ErrorCb err = nullptr);

    /// Must be called each frame to process async callbacks.
    /// All queued Nakama callbacks are dispatched during this call, so ensure
    /// it runs frequently (e.g. once per game-loop iteration).
    void tick();

private:
    Manager() = default;
    Config _cfg;
    Nakama::NClientPtr _client;
    Nakama::NSessionPtr _session;
    bool _init = false;

    void onAuth(Nakama::NSessionPtr s);
    void save();
    void load();
    std::string deviceId();
    void syncMeta();
    void log(const std::string& msg);

    std::string sessionFilePath() const;
    std::string deviceFilePath() const;

    static std::string escapeJsonString(const std::string& input);
};

} // namespace ivx
