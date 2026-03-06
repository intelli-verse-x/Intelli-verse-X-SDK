#pragma once

#include "IntelliVerseX/IVXConfig.h"
#include "IntelliVerseX/IVXTypes.h"
#include "nakama-cpp/Nakama.h"
#include <memory>
#include <string>

namespace IntelliVerseX {

class IVXManager {
public:
    static constexpr const char* SDK_VERSION = "5.1.0";

    static IVXManager& getInstance();

    void initialize(const IVXConfig& config);
    bool isInitialized() const { return _initialized; }

    // Auth
    void authenticateDevice(const std::string& deviceId = "",
                            SuccessCallback onSuccess = nullptr,
                            ErrorCallback onError = nullptr);
    void authenticateEmail(const std::string& email,
                           const std::string& password,
                           bool create = false,
                           SuccessCallback onSuccess = nullptr,
                           ErrorCallback onError = nullptr);
    void authenticateGoogle(const std::string& token,
                            SuccessCallback onSuccess = nullptr,
                            ErrorCallback onError = nullptr);
    void authenticateApple(const std::string& token,
                           SuccessCallback onSuccess = nullptr,
                           ErrorCallback onError = nullptr);
    void authenticateCustom(const std::string& customId,
                            SuccessCallback onSuccess = nullptr,
                            ErrorCallback onError = nullptr);

    bool restoreSession();
    void clearSession();
    bool hasValidSession() const;

    std::string getUserId() const;
    std::string getUsername() const;

    // Profile
    void fetchProfile(ProfileCallback onSuccess = nullptr,
                      ErrorCallback onError = nullptr);
    void updateProfile(const std::string& displayName,
                       const std::string& avatarUrl = "",
                       const std::string& langTag = "",
                       SuccessCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);

    // Wallet
    void fetchWallet(WalletCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);
    void grantCurrency(const std::string& currencyId,
                       int64_t amount,
                       WalletCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);

    // Leaderboard
    void submitScore(const std::string& leaderboardId,
                     int64_t score,
                     SuccessCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);
    void fetchLeaderboard(const std::string& leaderboardId,
                          int limit = 20,
                          LeaderboardCallback onSuccess = nullptr,
                          ErrorCallback onError = nullptr);

    // Storage
    void writeStorage(const std::string& collection,
                      const std::string& key,
                      const std::string& valueJson,
                      SuccessCallback onSuccess = nullptr,
                      ErrorCallback onError = nullptr);
    void readStorage(const std::string& collection,
                     const std::string& key,
                     StorageCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);

    // RPC
    void callRpc(const std::string& rpcId,
                 const std::string& payloadJson = "{}",
                 RpcCallback onSuccess = nullptr,
                 ErrorCallback onError = nullptr);

    void tick();

private:
    IVXManager() = default;
    ~IVXManager() = default;
    IVXManager(const IVXManager&) = delete;
    IVXManager& operator=(const IVXManager&) = delete;

    IVXConfig _config;
    Nakama::NClientPtr _client;
    Nakama::NSessionPtr _session;
    bool _initialized = false;

    void onAuthSuccess(Nakama::NSessionPtr session);
    void saveSession();
    void loadSession();
    std::string getPersistentDeviceId();
    std::string getWritablePath(const std::string& filename);
    void syncMetadata();
    void log(const std::string& message);

    static bool isAlphanumericOrUnderscore(const std::string& s);
};

} // namespace IntelliVerseX
