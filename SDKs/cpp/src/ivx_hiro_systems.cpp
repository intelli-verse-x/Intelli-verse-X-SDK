// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/ivx_hiro_systems.h"
#include <iostream>
#include <sstream>

namespace ivx {

HiroSystems& HiroSystems::instance() {
    static HiroSystems inst;
    return inst;
}

void HiroSystems::setNakamaClient(Nakama::NClientPtr client, Nakama::NSessionPtr session) {
    _client = std::move(client);
    _session = std::move(session);
    log("nakama client set");
}

bool HiroSystems::hasClient() const {
    return _client && _session;
}

// --- Spin Wheel ---

void HiroSystems::getSpinWheel(SpinWheelStateCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        SpinWheelState state;
        // Placeholder: parse rpc.payload JSON into state
        if (cb) cb(state);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/spin_wheel_get", "{}", successCb, errorCb);
}

void HiroSystems::spin(SpinRewardCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        SpinWheelReward reward;
        if (cb) cb(reward);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/spin_wheel_spin", "{}", successCb, errorCb);
}

// --- Streaks ---

void HiroSystems::getStreaks(StreakStateCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        StreakState state;
        if (cb) cb(state);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/streaks_get", "{}", successCb, errorCb);
}

void HiroSystems::updateStreak(StreakStateCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        StreakState state;
        if (cb) cb(state);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/streaks_update", "{}", successCb, errorCb);
}

void HiroSystems::claimStreakMilestone(int32_t day, SuccessCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    std::string payload = "{\"day\":" + std::to_string(day) + "}";

    auto successCb = [cb](const Nakama::NRpc&) {
        if (cb) cb();
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/streaks_claim", payload, successCb, errorCb);
}

// --- Offerwall ---

void HiroSystems::getOfferwall(OfferListCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        std::vector<Offer> offers;
        if (cb) cb(offers);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/offerwall_get", "{}", successCb, errorCb);
}

void HiroSystems::completeOffer(const std::string& offerId, SuccessCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    std::string payload = "{\"offer_id\":\"" + offerId + "\"}";

    auto successCb = [cb](const Nakama::NRpc&) {
        if (cb) cb();
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/offerwall_complete", payload, successCb, errorCb);
}

void HiroSystems::claimPendingOffers(OfferListCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        std::vector<Offer> claimed;
        if (cb) cb(claimed);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/offerwall_claim", "{}", successCb, errorCb);
}

// --- Friend Quests ---

void HiroSystems::getActiveFriendQuests(FriendQuestListCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        std::vector<FriendQuest> quests;
        if (cb) cb(quests);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/friend_quests_get", "{}", successCb, errorCb);
}

void HiroSystems::contributeToFriendQuest(const std::string& questId, int32_t amount,
                                           SuccessCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    std::string payload = "{\"quest_id\":\"" + questId + "\",\"amount\":" + std::to_string(amount) + "}";

    auto successCb = [cb](const Nakama::NRpc&) {
        if (cb) cb();
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/friend_quests_contribute", payload, successCb, errorCb);
}

// --- Friend Battles ---

void HiroSystems::challengeFriend(const std::string& opponentId, SuccessCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    std::string payload = "{\"opponent_id\":\"" + opponentId + "\"}";

    auto successCb = [cb](const Nakama::NRpc&) {
        if (cb) cb();
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/friend_battles_challenge", payload, successCb, errorCb);
}

void HiroSystems::getActiveFriendBattles(FriendBattleListCb cb, ErrorCb err) {
    if (!hasClient()) {
        if (err) err({-1, "Nakama client not set"});
        return;
    }

    auto successCb = [cb](const Nakama::NRpc& rpc) {
        std::vector<FriendBattle> battles;
        if (cb) cb(battles);
    };

    auto errorCb = [err](const Nakama::NError& e) {
        if (err) err({e.code, e.message});
    };

    _client->rpc(_session, "hiro/friend_battles_get", "{}", successCb, errorCb);
}

// --- Logging ---

void HiroSystems::log(const std::string& msg) {
    std::cout << "[IVX:HiroSystems] " << msg << std::endl;
}

} // namespace ivx
