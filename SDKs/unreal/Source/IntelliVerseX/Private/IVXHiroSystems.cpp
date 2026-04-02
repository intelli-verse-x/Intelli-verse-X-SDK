// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXHiroSystems.h"
#include "IVXManager.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"

namespace
{
void ParseRetentionJson(const TSharedPtr<FJsonObject>& Json, FIVXRetentionState& Out)
{
    if (!Json.IsValid())
    {
        return;
    }
    FString S;
    double N = 0.0;
    bool B = false;

    if (Json->TryGetStringField(TEXT("userId"), S) || Json->TryGetStringField(TEXT("user_id"), S))
    {
        Out.UserId = MoveTemp(S);
    }
    if (Json->TryGetNumberField(TEXT("firstSessionAt"), N) || Json->TryGetNumberField(TEXT("first_session_at"), N))
    {
        Out.FirstSessionAt = static_cast<int64>(N);
    }
    N = 0.0;
    if (Json->TryGetNumberField(TEXT("lastSessionAt"), N) || Json->TryGetNumberField(TEXT("last_session_at"), N))
    {
        Out.LastSessionAt = static_cast<int64>(N);
    }
    N = 0.0;
    if (Json->TryGetNumberField(TEXT("totalSessions"), N) || Json->TryGetNumberField(TEXT("total_sessions"), N))
    {
        Out.TotalSessions = static_cast<int32>(N);
    }
    N = 0.0;
    if (Json->TryGetNumberField(TEXT("currentSessionDepth"), N) || Json->TryGetNumberField(TEXT("current_session_depth"), N))
    {
        Out.CurrentSessionDepth = static_cast<int32>(N);
    }
    N = 0.0;
    if (Json->TryGetNumberField(TEXT("daysSinceLastSession"), N) || Json->TryGetNumberField(TEXT("days_since_last_session"), N))
    {
        Out.DaysSinceLastSession = static_cast<int32>(N);
    }
    if (Json->TryGetStringField(TEXT("churnRisk"), S) || Json->TryGetStringField(TEXT("churn_risk"), S))
    {
        Out.ChurnRisk = MoveTemp(S);
    }
    if (Json->TryGetBoolField(TEXT("onboardingComplete"), B) || Json->TryGetBoolField(TEXT("onboarding_complete"), B))
    {
        Out.bOnboardingComplete = B;
    }
    N = 0.0;
    if (Json->TryGetNumberField(TEXT("onboardingStep"), N) || Json->TryGetNumberField(TEXT("onboarding_step"), N))
    {
        Out.OnboardingStep = static_cast<int32>(N);
    }
    B = false;
    if (Json->TryGetBoolField(TEXT("comebackBonusAvailable"), B) || Json->TryGetBoolField(TEXT("comeback_bonus_available"), B))
    {
        Out.bComebackBonusAvailable = B;
    }
    TSharedPtr<FJsonObject> RewardObj;
    if (Json->TryGetObjectField(TEXT("comebackBonusReward"), RewardObj) && RewardObj.IsValid())
    {
        FString RewardStr;
        TSharedRef<TJsonWriter<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>> Writer =
            TJsonWriterFactory<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>::Create(&RewardStr);
        FJsonSerializer::Serialize(RewardObj.ToSharedRef(), Writer);
        Out.ComebackBonusRewardJson = RewardStr;
    }
    else if (Json->TryGetStringField(TEXT("comeback_bonus_reward_json"), S))
    {
        Out.ComebackBonusRewardJson = MoveTemp(S);
    }
}
} // namespace

TWeakObjectPtr<UIVXHiroSystems> UIVXHiroSystems::Singleton = nullptr;

UIVXHiroSystems* UIVXHiroSystems::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXHiroSystems>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXHiroSystems::SetNakamaClient(UNakamaClient* Client, UNakamaSession* Session)
{
    NakamaClient = Client;
    NakamaSession = Session;
    LogDebug(TEXT("Nakama client set"));
}

bool UIVXHiroSystems::HasValidClient() const
{
    return NakamaClient != nullptr && NakamaSession != nullptr;
}

// --- Spin Wheel ---

void UIVXHiroSystems::GetSpinWheel(const FIVXSpinWheelDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("GetSpinWheel: Nakama client not set"));
        FIVXSpinWheelState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXSpinWheelState State;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            State.SpinsRemaining = Json->GetIntegerField(TEXT("spins_remaining"));
            State.NextFreeSpinTimestamp = static_cast<int64>(Json->GetNumberField(TEXT("next_free_spin_ts")));
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("rewards"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXSpinWheelReward R;
                    R.RewardId = Obj->GetStringField(TEXT("reward_id"));
                    R.RewardType = Obj->GetStringField(TEXT("reward_type"));
                    R.Amount = Obj->GetIntegerField(TEXT("amount"));
                    R.DisplayName = Obj->GetStringField(TEXT("display_name"));
                    State.Rewards.Add(R);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, State);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetSpinWheel RPC failed: %s"), *Err.Message));
        FIVXSpinWheelState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/spin_wheel_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::Spin(const FIVXSpinResultDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("Spin: Nakama client not set"));
        FIVXSpinWheelReward Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXSpinWheelReward Reward;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            Reward.RewardId = Json->GetStringField(TEXT("reward_id"));
            Reward.RewardType = Json->GetStringField(TEXT("reward_type"));
            Reward.Amount = Json->GetIntegerField(TEXT("amount"));
            Reward.DisplayName = Json->GetStringField(TEXT("display_name"));
        }
        OnComplete.ExecuteIfBound(true, Reward);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("Spin RPC failed: %s"), *Err.Message));
        FIVXSpinWheelReward Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/spin_wheel_spin"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

// --- Streaks ---

void UIVXHiroSystems::GetStreaks(const FIVXStreakDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        FIVXStreakState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXStreakState State;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            State.CurrentStreak = Json->GetIntegerField(TEXT("current_streak"));
            State.LongestStreak = Json->GetIntegerField(TEXT("longest_streak"));
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("milestones"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXStreakMilestone M;
                    M.Day = Obj->GetIntegerField(TEXT("day"));
                    M.RewardJson = Obj->GetStringField(TEXT("reward"));
                    M.bClaimed = Obj->GetBoolField(TEXT("claimed"));
                    State.Milestones.Add(M);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, State);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetStreaks RPC failed: %s"), *Err.Message));
        FIVXStreakState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/streaks_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::UpdateStreak(const FIVXStreakDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        FIVXStreakState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXStreakState State;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            State.CurrentStreak = Json->GetIntegerField(TEXT("current_streak"));
            State.LongestStreak = Json->GetIntegerField(TEXT("longest_streak"));
        }
        OnComplete.ExecuteIfBound(true, State);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("UpdateStreak RPC failed: %s"), *Err.Message));
        FIVXStreakState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/streaks_update"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::ClaimStreakMilestone(int32 Day, const FIVXHiroSuccessDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    FString Payload = FString::Printf(TEXT("{\"day\":%d}"), Day);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
    {
        OnComplete.ExecuteIfBound(true);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("ClaimStreakMilestone RPC failed: %s"), *Err.Message));
        OnComplete.ExecuteIfBound(false);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/streaks_claim"), Payload, SuccessDelegate, ErrorDelegate);
}

// --- Offerwall ---

void UIVXHiroSystems::GetOfferwall(const FIVXOfferListDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        TArray<FIVXOffer> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        TArray<FIVXOffer> Offers;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("offers"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXOffer O;
                    O.OfferId = Obj->GetStringField(TEXT("offer_id"));
                    O.Title = Obj->GetStringField(TEXT("title"));
                    O.Description = Obj->GetStringField(TEXT("description"));
                    O.RewardJson = Obj->GetStringField(TEXT("reward"));
                    O.bCompleted = Obj->GetBoolField(TEXT("completed"));
                    O.bClaimed = Obj->GetBoolField(TEXT("claimed"));
                    Offers.Add(O);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, Offers);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetOfferwall RPC failed: %s"), *Err.Message));
        TArray<FIVXOffer> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/offerwall_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::CompleteOffer(const FString& OfferId, const FIVXHiroSuccessDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    FString Payload = FString::Printf(TEXT("{\"offer_id\":\"%s\"}"), *OfferId);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
    {
        OnComplete.ExecuteIfBound(true);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("CompleteOffer RPC failed: %s"), *Err.Message));
        OnComplete.ExecuteIfBound(false);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/offerwall_complete"), Payload, SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::ClaimPendingOffers(const FIVXOfferListDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        TArray<FIVXOffer> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        TArray<FIVXOffer> Claimed;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("claimed"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXOffer O;
                    O.OfferId = Obj->GetStringField(TEXT("offer_id"));
                    O.Title = Obj->GetStringField(TEXT("title"));
                    O.bClaimed = true;
                    Claimed.Add(O);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, Claimed);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("ClaimPendingOffers RPC failed: %s"), *Err.Message));
        TArray<FIVXOffer> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/offerwall_claim"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

// --- Friend Quests ---

void UIVXHiroSystems::GetActiveFriendQuests(const FIVXFriendQuestDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        TArray<FIVXFriendQuest> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        TArray<FIVXFriendQuest> Quests;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("quests"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXFriendQuest Q;
                    Q.QuestId = Obj->GetStringField(TEXT("quest_id"));
                    Q.Title = Obj->GetStringField(TEXT("title"));
                    Q.CurrentProgress = Obj->GetIntegerField(TEXT("current_progress"));
                    Q.TargetProgress = Obj->GetIntegerField(TEXT("target_progress"));
                    Q.RewardJson = Obj->GetStringField(TEXT("reward"));
                    const TArray<TSharedPtr<FJsonValue>>* Participants;
                    if (Obj->TryGetArrayField(TEXT("participants"), Participants))
                    {
                        for (const auto& P : *Participants)
                        {
                            Q.Participants.Add(P->AsString());
                        }
                    }
                    Quests.Add(Q);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, Quests);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetActiveFriendQuests RPC failed: %s"), *Err.Message));
        TArray<FIVXFriendQuest> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/friend_quests_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::ContributeToFriendQuest(const FString& QuestId, int32 Amount, const FIVXHiroSuccessDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    FString Payload = FString::Printf(TEXT("{\"quest_id\":\"%s\",\"amount\":%d}"), *QuestId, Amount);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
    {
        OnComplete.ExecuteIfBound(true);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("ContributeToFriendQuest RPC failed: %s"), *Err.Message));
        OnComplete.ExecuteIfBound(false);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/friend_quests_contribute"), Payload, SuccessDelegate, ErrorDelegate);
}

// --- Friend Battles ---

void UIVXHiroSystems::ChallengeFriend(const FString& OpponentId, const FIVXHiroSuccessDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    FString Payload = FString::Printf(TEXT("{\"opponent_id\":\"%s\"}"), *OpponentId);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
    {
        OnComplete.ExecuteIfBound(true);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("ChallengeFriend RPC failed: %s"), *Err.Message));
        OnComplete.ExecuteIfBound(false);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/friend_battles_challenge"), Payload, SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::GetActiveFriendBattles(const FIVXFriendBattleDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        TArray<FIVXFriendBattle> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        TArray<FIVXFriendBattle> Battles;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            const TArray<TSharedPtr<FJsonValue>>* Arr;
            if (Json->TryGetArrayField(TEXT("battles"), Arr))
            {
                for (const auto& Val : *Arr)
                {
                    const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                    if (!Obj.IsValid()) continue;
                    FIVXFriendBattle B;
                    B.BattleId = Obj->GetStringField(TEXT("battle_id"));
                    B.ChallengerId = Obj->GetStringField(TEXT("challenger_id"));
                    B.OpponentId = Obj->GetStringField(TEXT("opponent_id"));
                    B.Status = Obj->GetStringField(TEXT("status"));
                    B.RewardJson = Obj->GetStringField(TEXT("reward"));
                    Battles.Add(B);
                }
            }
        }
        OnComplete.ExecuteIfBound(true, Battles);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetActiveFriendBattles RPC failed: %s"), *Err.Message));
        TArray<FIVXFriendBattle> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro/friend_battles_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

// --- Retention ---

void UIVXHiroSystems::GetRetention(const FIVXRetentionDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("GetRetention: Nakama client not set"));
        FIVXRetentionState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXRetentionState State;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            ParseRetentionJson(Json, State);
        }
        OnComplete.ExecuteIfBound(true, State);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("GetRetention RPC failed: %s"), *Err.Message));
        FIVXRetentionState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro_retention_get"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

void UIVXHiroSystems::UpdateRetention(const FIVXRetentionDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("UpdateRetention: Nakama client not set"));
        FIVXRetentionState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXRetentionState State;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            ParseRetentionJson(Json, State);
        }
        OnComplete.ExecuteIfBound(true, State);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("UpdateRetention RPC failed: %s"), *Err.Message));
        FIVXRetentionState Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro_retention_update"), TEXT("{}"), SuccessDelegate, ErrorDelegate);
}

// --- IAP trigger ---

void UIVXHiroSystems::CheckIapTrigger(const FString& EventType, const FIVXIapTriggerResultDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("CheckIapTrigger: Nakama client not set"));
        FIVXIapTriggerResult Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    FString Escaped = EventType;
    Escaped.ReplaceInline(TEXT("\\"), TEXT("\\\\"));
    Escaped.ReplaceInline(TEXT("\""), TEXT("\\\""));
    const FString Payload = FString::Printf(TEXT("{\"event_type\":\"%s\"}"), *Escaped);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXIapTriggerResult Result;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            bool B = false;
            if (Json->TryGetBoolField(TEXT("should_show"), B) || Json->TryGetBoolField(TEXT("shouldShow"), B))
            {
                Result.bShouldShow = B;
            }
            FString S;
            if (Json->TryGetStringField(TEXT("offer_id"), S) || Json->TryGetStringField(TEXT("offerId"), S))
            {
                Result.OfferId = S;
            }
            double D = 0.0;
            if (Json->TryGetNumberField(TEXT("discount"), D))
            {
                Result.Discount = static_cast<float>(D);
            }
            if (Json->TryGetNumberField(TEXT("expires_at"), D) || Json->TryGetNumberField(TEXT("expiresAt"), D))
            {
                Result.ExpiresAt = static_cast<int64>(D);
            }
        }
        OnComplete.ExecuteIfBound(true, Result);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("CheckIapTrigger RPC failed: %s"), *Err.Message));
        FIVXIapTriggerResult Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro_iap_trigger_check"), Payload, SuccessDelegate, ErrorDelegate);
}

// --- Smart ad timer ---

void UIVXHiroSystems::CanShowSmartAd(const FString& Placement, const FIVXSmartAdResultDelegate& OnComplete)
{
    if (!HasValidClient())
    {
        LogError(TEXT("CanShowSmartAd: Nakama client not set"));
        FIVXSmartAdResult Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    FString Escaped = Placement;
    Escaped.ReplaceInline(TEXT("\\"), TEXT("\\\\"));
    Escaped.ReplaceInline(TEXT("\""), TEXT("\\\""));
    const FString Payload = FString::Printf(TEXT("{\"placement\":\"%s\"}"), *Escaped);

    auto SuccessDelegate = FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
    {
        FIVXSmartAdResult Result;
        TSharedPtr<FJsonObject> Json;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
        if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
        {
            bool B = false;
            if (Json->TryGetBoolField(TEXT("can_show"), B) || Json->TryGetBoolField(TEXT("canShow"), B))
            {
                Result.bCanShow = B;
            }
            double D = 0.0;
            if (Json->TryGetNumberField(TEXT("next_available_at"), D) || Json->TryGetNumberField(TEXT("nextAvailableAt"), D))
            {
                Result.NextAvailableAt = static_cast<int64>(D);
            }
            FString S;
            if (Json->TryGetStringField(TEXT("reason"), S))
            {
                Result.Reason = S;
            }
        }
        OnComplete.ExecuteIfBound(true, Result);
    });

    auto ErrorDelegate = FOnError::CreateLambda([this, OnComplete](const FNakamaError& Err)
    {
        LogError(FString::Printf(TEXT("CanShowSmartAd RPC failed: %s"), *Err.Message));
        FIVXSmartAdResult Empty;
        OnComplete.ExecuteIfBound(false, Empty);
    });

    NakamaClient->RPC(NakamaSession, TEXT("hiro_smart_ad_can_show"), Payload, SuccessDelegate, ErrorDelegate);
}

// --- Logging ---

void UIVXHiroSystems::LogDebug(const FString& Message) const
{
    UE_LOG(LogIVX, Log, TEXT("[IVXHiroSystems] %s"), *Message);
}

void UIVXHiroSystems::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("[IVXHiroSystems] %s"), *Message);
}
