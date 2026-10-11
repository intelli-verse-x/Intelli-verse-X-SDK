// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <string>
#include <functional>
#include <vector>
#include <cstdint>
#include <utility>

namespace ivx {

struct Profile {
    std::string userId;
    std::string username;
    std::string displayName;
    std::string avatarUrl;
    std::string langTag;
    std::string metadata;
    std::string wallet;
};

struct LeaderboardRecord {
    std::string ownerId;
    std::string username;
    int64_t score = 0;
    int64_t rank = 0;
};

struct Error {
    int code = 0;
    std::string message;

    Error() = default;
    Error(int errorCode, std::string errorMessage)
        : code(errorCode), message(std::move(errorMessage)) {}

    template <typename ErrorCode>
    Error(ErrorCode errorCode, std::string errorMessage)
        : code(static_cast<int>(errorCode)), message(std::move(errorMessage)) {}
};

using ErrorCb = std::function<void(const Error&)>;
using SuccessCb = std::function<void()>;
using ProfileCb = std::function<void(const Profile&)>;
using StringCb = std::function<void(const std::string&)>;
using LeaderboardCb = std::function<void(const std::vector<LeaderboardRecord>&)>;

} // namespace ivx
