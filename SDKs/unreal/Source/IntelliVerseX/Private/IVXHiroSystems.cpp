// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXHiroSystems.h"
#include "IVXManager.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"

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

// --- Logging ---

void UIVXHiroSystems::LogDebug(const FString& Message) const
{
    UE_LOG(LogIVX, Log, TEXT("[IVXHiroSystems] %s"), *Message);
}

void UIVXHiroSystems::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("[IVXHiroSystems] %s"), *Message);
}
