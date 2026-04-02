// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include "IntelliVerseX/IVXManager.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct SpinWheelReward {
    std::string rewardId;
    std::string type;
    int amount = 0;
    std::string metadata;
};

struct StreakInfo {
    int currentStreak = 0;
    int longestStreak = 0;
    bool claimedToday = false;
    std::string lastClaimDate;
    std::vector<SpinWheelReward> rewards;
};

struct OfferwallItem {
    std::string offerId;
    std::string title;
    std::string description;
    std::string imageUrl;
    int cost = 0;
    std::string currency;
    bool available = true;
};

struct FriendInfo {
    std::string userId;
    std::string username;
    std::string displayName;
    std::string avatarUrl;
    int state = 0;
};

using SpinWheelCallback = std::function<void(const SpinWheelReward&)>;
using StreakCallback = std::function<void(const StreakInfo&)>;
using OfferwallCallback = std::function<void(const std::vector<OfferwallItem>&)>;
using FriendListCallback = std::function<void(const std::vector<FriendInfo>&)>;

class IVXHiroSystems {
public:
    static IVXHiroSystems& getInstance();

    // Spin Wheel
    void spinWheel(const std::string& wheelId = "default",
                   SpinWheelCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void getWheelConfig(const std::string& wheelId = "default",
                        RpcCallback onSuccess = nullptr,
                        ErrorCallback onError = nullptr);

    // Daily Streaks
    void getStreak(StreakCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void claimStreak(StreakCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);

    // Offerwall
    void getOffers(OfferwallCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void claimOffer(const std::string& offerId,
                    SuccessCallback onSuccess = nullptr,
                    ErrorCallback onError = nullptr);

    // Friends
    void listFriends(int state = 0,
                     FriendListCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);
    void addFriend(const std::string& userId,
                   SuccessCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void removeFriend(const std::string& userId,
                      SuccessCallback onSuccess = nullptr,
                      ErrorCallback onError = nullptr);
    void blockUser(const std::string& userId,
                   SuccessCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);

private:
    IVXHiroSystems() = default;
    ~IVXHiroSystems() = default;
    IVXHiroSystems(const IVXHiroSystems&) = delete;
    IVXHiroSystems& operator=(const IVXHiroSystems&) = delete;

    void callHiroRpc(const std::string& rpcId,
                     const std::string& payloadJson,
                     RpcCallback onSuccess,
                     ErrorCallback onError);
    void log(const std::string& message);
};

} // namespace IntelliVerseX
