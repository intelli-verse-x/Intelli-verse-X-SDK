// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXHiroSystems.h"
#include "cocos2d.h"
#include "json/rapidjson.h"
#include "json/document.h"
#include "json/writer.h"
#include "json/stringbuffer.h"

namespace IntelliVerseX {

IVXHiroSystems& IVXHiroSystems::getInstance() {
    static IVXHiroSystems instance;
    return instance;
}

// ---------------------------------------------------------------------------
// Spin Wheel
// ---------------------------------------------------------------------------

void IVXHiroSystems::spinWheel(const std::string& wheelId,
                                SpinWheelCallback onSuccess,
                                ErrorCallback onError) {
    std::string payload = "{\"wheel_id\":\"" + wheelId + "\"}";
    callHiroRpc("hiro_spin_wheel", payload,
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            SpinWheelReward reward;
            if (!doc.HasParseError() && doc.IsObject()) {
                reward.rewardId = doc.HasMember("reward_id") ? doc["reward_id"].GetString() : "";
                reward.type = doc.HasMember("type") ? doc["type"].GetString() : "";
                reward.amount = doc.HasMember("amount") ? doc["amount"].GetInt() : 0;
                reward.metadata = doc.HasMember("metadata") ? doc["metadata"].GetString() : "";
            }
            if (onSuccess) onSuccess(reward);
        }, onError);
}

void IVXHiroSystems::getWheelConfig(const std::string& wheelId,
                                     RpcCallback onSuccess,
                                     ErrorCallback onError) {
    std::string payload = "{\"wheel_id\":\"" + wheelId + "\"}";
    callHiroRpc("hiro_spin_wheel_config", payload, onSuccess, onError);
}

// ---------------------------------------------------------------------------
// Daily Streaks
// ---------------------------------------------------------------------------

void IVXHiroSystems::getStreak(StreakCallback onSuccess,
                                ErrorCallback onError) {
    callHiroRpc("hiro_streak_get", "{}",
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            StreakInfo info;
            if (!doc.HasParseError() && doc.IsObject()) {
                info.currentStreak = doc.HasMember("current_streak") ? doc["current_streak"].GetInt() : 0;
                info.longestStreak = doc.HasMember("longest_streak") ? doc["longest_streak"].GetInt() : 0;
                info.claimedToday = doc.HasMember("claimed_today") ? doc["claimed_today"].GetBool() : false;
                info.lastClaimDate = doc.HasMember("last_claim_date") ? doc["last_claim_date"].GetString() : "";
            }
            if (onSuccess) onSuccess(info);
        }, onError);
}

void IVXHiroSystems::claimStreak(StreakCallback onSuccess,
                                  ErrorCallback onError) {
    callHiroRpc("hiro_streak_claim", "{}",
        [this, onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            StreakInfo info;
            if (!doc.HasParseError() && doc.IsObject()) {
                info.currentStreak = doc.HasMember("current_streak") ? doc["current_streak"].GetInt() : 0;
                info.longestStreak = doc.HasMember("longest_streak") ? doc["longest_streak"].GetInt() : 0;
                info.claimedToday = true;
                info.lastClaimDate = doc.HasMember("last_claim_date") ? doc["last_claim_date"].GetString() : "";
            }
            log("Streak claimed — day " + std::to_string(info.currentStreak));
            if (onSuccess) onSuccess(info);
        }, onError);
}

// ---------------------------------------------------------------------------
// Offerwall
// ---------------------------------------------------------------------------

void IVXHiroSystems::getOffers(OfferwallCallback onSuccess,
                                ErrorCallback onError) {
    callHiroRpc("hiro_offerwall_list", "{}",
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            std::vector<OfferwallItem> offers;
            if (!doc.HasParseError() && doc.IsObject() &&
                doc.HasMember("offers") && doc["offers"].IsArray()) {
                const auto& arr = doc["offers"];
                for (rapidjson::SizeType i = 0; i < arr.Size(); ++i) {
                    const auto& obj = arr[i];
                    if (!obj.IsObject()) continue;
                    OfferwallItem item;
                    item.offerId = obj.HasMember("offer_id") ? obj["offer_id"].GetString() : "";
                    item.title = obj.HasMember("title") ? obj["title"].GetString() : "";
                    item.description = obj.HasMember("description") ? obj["description"].GetString() : "";
                    item.imageUrl = obj.HasMember("image_url") ? obj["image_url"].GetString() : "";
                    item.cost = obj.HasMember("cost") ? obj["cost"].GetInt() : 0;
                    item.currency = obj.HasMember("currency") ? obj["currency"].GetString() : "";
                    item.available = obj.HasMember("available") ? obj["available"].GetBool() : true;
                    offers.push_back(item);
                }
            }
            if (onSuccess) onSuccess(offers);
        }, onError);
}

void IVXHiroSystems::claimOffer(const std::string& offerId,
                                 SuccessCallback onSuccess,
                                 ErrorCallback onError) {
    std::string payload = "{\"offer_id\":\"" + offerId + "\"}";
    callHiroRpc("hiro_offerwall_claim", payload,
        [this, offerId, onSuccess](const std::string&) {
            log("Offer claimed: " + offerId);
            if (onSuccess) onSuccess();
        }, onError);
}

// ---------------------------------------------------------------------------
// Friends
// ---------------------------------------------------------------------------

void IVXHiroSystems::listFriends(int state,
                                  FriendListCallback onSuccess,
                                  ErrorCallback onError) {
    std::string payload = "{\"state\":" + std::to_string(state) + "}";
    callHiroRpc("hiro_friends_list", payload,
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            std::vector<FriendInfo> friends;
            if (!doc.HasParseError() && doc.IsObject() &&
                doc.HasMember("friends") && doc["friends"].IsArray()) {
                const auto& arr = doc["friends"];
                for (rapidjson::SizeType i = 0; i < arr.Size(); ++i) {
                    const auto& obj = arr[i];
                    if (!obj.IsObject()) continue;
                    FriendInfo f;
                    f.userId = obj.HasMember("user_id") ? obj["user_id"].GetString() : "";
                    f.username = obj.HasMember("username") ? obj["username"].GetString() : "";
                    f.displayName = obj.HasMember("display_name") ? obj["display_name"].GetString() : "";
                    f.avatarUrl = obj.HasMember("avatar_url") ? obj["avatar_url"].GetString() : "";
                    f.state = obj.HasMember("state") ? obj["state"].GetInt() : 0;
                    friends.push_back(f);
                }
            }
            if (onSuccess) onSuccess(friends);
        }, onError);
}

void IVXHiroSystems::addFriend(const std::string& userId,
                                SuccessCallback onSuccess,
                                ErrorCallback onError) {
    std::string payload = "{\"user_id\":\"" + userId + "\"}";
    callHiroRpc("hiro_friends_add", payload,
        [this, userId, onSuccess](const std::string&) {
            log("Friend added: " + userId);
            if (onSuccess) onSuccess();
        }, onError);
}

void IVXHiroSystems::removeFriend(const std::string& userId,
                                   SuccessCallback onSuccess,
                                   ErrorCallback onError) {
    std::string payload = "{\"user_id\":\"" + userId + "\"}";
    callHiroRpc("hiro_friends_remove", payload,
        [this, userId, onSuccess](const std::string&) {
            log("Friend removed: " + userId);
            if (onSuccess) onSuccess();
        }, onError);
}

void IVXHiroSystems::blockUser(const std::string& userId,
                                SuccessCallback onSuccess,
                                ErrorCallback onError) {
    std::string payload = "{\"user_id\":\"" + userId + "\"}";
    callHiroRpc("hiro_friends_block", payload,
        [this, userId, onSuccess](const std::string&) {
            log("User blocked: " + userId);
            if (onSuccess) onSuccess();
        }, onError);
}

// ---------------------------------------------------------------------------
// Retention / IAP / Smart ads
// ---------------------------------------------------------------------------

void IVXHiroSystems::getRetention(RetentionCallback onSuccess, ErrorCallback onError) {
    callHiroRpc("hiro_retention_get", "{}",
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXRetentionState state;
            if (!doc.HasParseError() && doc.IsObject()) {
                state.day = doc.HasMember("day") ? doc["day"].GetInt() : 0;
                state.lastLoginAt = doc.HasMember("last_login_at") ? doc["last_login_at"].GetInt64() : 0;
                if (doc.HasMember("rewards") && doc["rewards"].IsArray()) {
                    const auto& arr = doc["rewards"];
                    for (rapidjson::SizeType i = 0; i < arr.Size(); ++i) {
                        const auto& obj = arr[i];
                        if (!obj.IsObject()) continue;
                        IVXRetentionRewardDay r;
                        r.day = obj.HasMember("day") ? obj["day"].GetInt() : 0;
                        r.claimed = obj.HasMember("claimed") ? obj["claimed"].GetBool() : false;
                        state.rewards.push_back(r);
                    }
                }
            }
            if (onSuccess) onSuccess(state);
        }, onError);
}

void IVXHiroSystems::updateRetention(RetentionCallback onSuccess, ErrorCallback onError) {
    callHiroRpc("hiro_retention_update", "{}",
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXRetentionState state;
            if (!doc.HasParseError() && doc.IsObject()) {
                state.day = doc.HasMember("day") ? doc["day"].GetInt() : 0;
                state.lastLoginAt = doc.HasMember("last_login_at") ? doc["last_login_at"].GetInt64() : 0;
                if (doc.HasMember("rewards") && doc["rewards"].IsArray()) {
                    const auto& arr = doc["rewards"];
                    for (rapidjson::SizeType i = 0; i < arr.Size(); ++i) {
                        const auto& obj = arr[i];
                        if (!obj.IsObject()) continue;
                        IVXRetentionRewardDay r;
                        r.day = obj.HasMember("day") ? obj["day"].GetInt() : 0;
                        r.claimed = obj.HasMember("claimed") ? obj["claimed"].GetBool() : false;
                        state.rewards.push_back(r);
                    }
                }
            }
            if (onSuccess) onSuccess(state);
        }, onError);
}

void IVXHiroSystems::checkIapTrigger(const std::string& eventType, IapTriggerCallback onSuccess,
                                    ErrorCallback onError) {
    std::string payload = "{\"event_type\":\"" + eventType + "\"}";
    callHiroRpc("hiro_iap_trigger_check", payload,
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXIapTriggerResult r;
            if (!doc.HasParseError() && doc.IsObject()) {
                r.shouldShow = doc.HasMember("should_show") ? doc["should_show"].GetBool() : false;
                r.offerId = doc.HasMember("offer_id") ? doc["offer_id"].GetString() : "";
                r.discount = doc.HasMember("discount") ? doc["discount"].GetDouble() : 0.0;
                r.expiresAt = doc.HasMember("expires_at") ? doc["expires_at"].GetInt64() : 0;
            }
            if (onSuccess) onSuccess(r);
        }, onError);
}

void IVXHiroSystems::canShowSmartAd(const std::string& placement, SmartAdCallback onSuccess,
                                    ErrorCallback onError) {
    std::string payload = "{\"placement\":\"" + placement + "\"}";
    callHiroRpc("hiro_smart_ad_can_show", payload,
        [onSuccess](const std::string& response) {
            rapidjson::Document doc;
            doc.Parse(response.c_str());
            IVXSmartAdResult r;
            if (!doc.HasParseError() && doc.IsObject()) {
                r.canShow = doc.HasMember("can_show") ? doc["can_show"].GetBool() : false;
                r.nextAvailableAt = doc.HasMember("next_available_at") ? doc["next_available_at"].GetInt64() : 0;
                r.reason = doc.HasMember("reason") ? doc["reason"].GetString() : "";
            }
            if (onSuccess) onSuccess(r);
        }, onError);
}

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------

void IVXHiroSystems::callHiroRpc(const std::string& rpcId,
                                  const std::string& payloadJson,
                                  RpcCallback onSuccess,
                                  ErrorCallback onError) {
    IVXManager::getInstance().callRpc(rpcId, payloadJson, onSuccess, onError);
}

void IVXHiroSystems::log(const std::string& message) {
    cocos2d::log("[IntelliVerseX:Hiro] %s", message.c_str());
}

} // namespace IntelliVerseX
