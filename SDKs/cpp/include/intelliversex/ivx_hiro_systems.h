// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include "nakama-cpp/Nakama.h"
#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace ivx {

// --- Spin Wheel ---

struct SpinWheelReward {
    std::string rewardId;
    std::string rewardType;
    int32_t amount = 0;
    std::string displayName;
};

struct SpinWheelState {
    std::vector<SpinWheelReward> rewards;
    int32_t spinsRemaining = 0;
    int64_t nextFreeSpinTimestamp = 0;
};

// --- Streaks ---

struct StreakMilestone {
    int32_t day = 0;
    std::string rewardJson;
    bool claimed = false;
};

struct StreakState {
    int32_t currentStreak = 0;
    int32_t longestStreak = 0;
    std::vector<StreakMilestone> milestones;
};

// --- Offerwall ---

struct Offer {
    std::string offerId;
    std::string title;
    std::string description;
    std::string rewardJson;
    bool completed = false;
    bool claimed = false;
};

// --- Friend Quests ---

struct FriendQuest {
    std::string questId;
    std::string title;
    int32_t currentProgress = 0;
    int32_t targetProgress = 0;
    std::string rewardJson;
    std::vector<std::string> participants;
};

// --- Friend Battles ---

struct FriendBattle {
    std::string battleId;
    std::string challengerId;
    std::string opponentId;
    std::string status;
    std::string rewardJson;
};

// --- Retention / IAP trigger / Smart ad ---

struct RetentionRewardDay {
    int32_t day = 0;
    bool claimed = false;
};

struct RetentionState {
    int32_t day = 0;
    int64_t lastLoginAt = 0;
    std::vector<RetentionRewardDay> rewards;
};

struct IapTriggerResult {
    bool shouldShow = false;
    std::string offerId;
    double discount = 0;
    int64_t expiresAt = 0;
};

struct SmartAdResult {
    bool canShow = false;
    int64_t nextAvailableAt = 0;
    std::string reason;
};

// Callback typedefs
using SpinWheelStateCb  = std::function<void(const SpinWheelState&)>;
using SpinRewardCb      = std::function<void(const SpinWheelReward&)>;
using StreakStateCb      = std::function<void(const StreakState&)>;
using OfferListCb        = std::function<void(const std::vector<Offer>&)>;
using FriendQuestListCb  = std::function<void(const std::vector<FriendQuest>&)>;
using FriendBattleListCb = std::function<void(const std::vector<FriendBattle>&)>;
using RetentionStateCb   = std::function<void(const RetentionState&)>;
using IapTriggerResultCb = std::function<void(const IapTriggerResult&)>;
using SmartAdResultCb    = std::function<void(const SmartAdResult&)>;

/// Wraps Hiro/Nakama RPC systems: Spin Wheel, Streaks, Offerwall,
/// Friend Quests, and Friend Battles.
///
/// Thread-safety: same as Manager — single-thread only.
class HiroSystems {
public:
    static HiroSystems& instance();

    void setNakamaClient(Nakama::NClientPtr client, Nakama::NSessionPtr session);

    // Spin Wheel
    void getSpinWheel(SpinWheelStateCb cb, ErrorCb err = nullptr);
    void spin(SpinRewardCb cb, ErrorCb err = nullptr);

    // Streaks
    void getStreaks(StreakStateCb cb, ErrorCb err = nullptr);
    void updateStreak(StreakStateCb cb, ErrorCb err = nullptr);
    void claimStreakMilestone(int32_t day, SuccessCb cb = nullptr, ErrorCb err = nullptr);

    // Offerwall
    void getOfferwall(OfferListCb cb, ErrorCb err = nullptr);
    void completeOffer(const std::string& offerId, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void claimPendingOffers(OfferListCb cb, ErrorCb err = nullptr);

    // Friend Quests
    void getActiveFriendQuests(FriendQuestListCb cb, ErrorCb err = nullptr);
    void contributeToFriendQuest(const std::string& questId, int32_t amount,
                                 SuccessCb cb = nullptr, ErrorCb err = nullptr);

    // Friend Battles
    void challengeFriend(const std::string& opponentId, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void getActiveFriendBattles(FriendBattleListCb cb, ErrorCb err = nullptr);

    // Retention / monetization helpers
    void getRetention(RetentionStateCb cb, ErrorCb err = nullptr);
    void updateRetention(RetentionStateCb cb, ErrorCb err = nullptr);
    void checkIapTrigger(const std::string& eventType, IapTriggerResultCb cb, ErrorCb err = nullptr);
    void canShowSmartAd(const std::string& placement, SmartAdResultCb cb, ErrorCb err = nullptr);

private:
    HiroSystems() = default;
    Nakama::NClientPtr _client;
    Nakama::NSessionPtr _session;

    bool hasClient() const;
    void log(const std::string& msg);
};

} // namespace ivx
