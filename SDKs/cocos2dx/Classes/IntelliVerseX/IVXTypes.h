#pragma once

#include <string>
#include <functional>
#include <map>
#include <vector>

namespace IntelliVerseX {

struct IVXProfile {
    std::string userId;
    std::string username;
    std::string displayName;
    std::string avatarUrl;
    std::string langTag;
    std::string metadata;
    std::string wallet;
};

struct IVXLeaderboardRecord {
    std::string ownerId;
    std::string username;
    int64_t score = 0;
    int64_t rank = 0;
};

struct IVXError {
    int code = 0;
    std::string message;
};

using ErrorCallback = std::function<void(const IVXError&)>;
using SuccessCallback = std::function<void()>;
using ProfileCallback = std::function<void(const IVXProfile&)>;
using WalletCallback = std::function<void(const std::string&)>;
using LeaderboardCallback = std::function<void(const std::vector<IVXLeaderboardRecord>&)>;
using RpcCallback = std::function<void(const std::string&)>;
using StorageCallback = std::function<void(const std::string&)>;

} // namespace IntelliVerseX
