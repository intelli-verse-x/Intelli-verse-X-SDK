// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "NakamaClient.h"
#include "NakamaSession.h"
#include "IVXHiroSystems.generated.h"

// --- Spin Wheel ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSpinWheelReward
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    FString RewardId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    FString RewardType;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    int32 Amount = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    FString DisplayName;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSpinWheelState
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    TArray<FIVXSpinWheelReward> Rewards;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    int32 SpinsRemaining = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SpinWheel")
    int64 NextFreeSpinTimestamp = 0;
};

// --- Streaks ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXStreakMilestone
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    int32 Day = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    FString RewardJson;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    bool bClaimed = false;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXStreakState
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    int32 CurrentStreak = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    int32 LongestStreak = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Streaks")
    TArray<FIVXStreakMilestone> Milestones;
};

// --- Offerwall ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXOffer
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    FString OfferId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    FString Title;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    FString Description;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    FString RewardJson;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    bool bCompleted = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Offerwall")
    bool bClaimed = false;
};

// --- Friend Quests ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXFriendQuest
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    FString QuestId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    FString Title;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    int32 CurrentProgress = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    int32 TargetProgress = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    FString RewardJson;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendQuests")
    TArray<FString> Participants;
};

// --- Friend Battles ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXFriendBattle
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendBattles")
    FString BattleId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendBattles")
    FString ChallengerId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendBattles")
    FString OpponentId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendBattles")
    FString Status;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|FriendBattles")
    FString RewardJson;
};

// --- Delegates ---

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXSpinWheelDelegate, bool, bSuccess, const FIVXSpinWheelState&, State);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXSpinResultDelegate, bool, bSuccess, const FIVXSpinWheelReward&, Reward);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXStreakDelegate, bool, bSuccess, const FIVXStreakState&, State);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXOfferListDelegate, bool, bSuccess, const TArray<FIVXOffer>&, Offers);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXHiroSuccessDelegate, bool, bSuccess);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXFriendQuestDelegate, bool, bSuccess, const TArray<FIVXFriendQuest>&, Quests);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXFriendBattleDelegate, bool, bSuccess, const TArray<FIVXFriendBattle>&, Battles);

/**
 * Wraps Hiro (Nakama) systems: Spin Wheel, Streaks, Offerwall, Friend Quests, Friend Battles.
 * Calls Nakama RPCs and parses responses into Blueprint-friendly structs.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXHiroSystems : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro", meta = (DisplayName = "Get IVX Hiro Systems", WorldContext = "WorldContextObject"))
    static UIVXHiroSystems* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro")
    void SetNakamaClient(UNakamaClient* Client, UNakamaSession* Session);

    // --- Spin Wheel ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|SpinWheel")
    void GetSpinWheel(const FIVXSpinWheelDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|SpinWheel")
    void Spin(const FIVXSpinResultDelegate& OnComplete);

    // --- Streaks ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Streaks")
    void GetStreaks(const FIVXStreakDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Streaks")
    void UpdateStreak(const FIVXStreakDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Streaks")
    void ClaimStreakMilestone(int32 Day, const FIVXHiroSuccessDelegate& OnComplete);

    // --- Offerwall ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Offerwall")
    void GetOfferwall(const FIVXOfferListDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Offerwall")
    void CompleteOffer(const FString& OfferId, const FIVXHiroSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Offerwall")
    void ClaimPendingOffers(const FIVXOfferListDelegate& OnComplete);

    // --- Friend Quests ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|FriendQuests")
    void GetActiveFriendQuests(const FIVXFriendQuestDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|FriendQuests")
    void ContributeToFriendQuest(const FString& QuestId, int32 Amount, const FIVXHiroSuccessDelegate& OnComplete);

    // --- Friend Battles ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|FriendBattles")
    void ChallengeFriend(const FString& OpponentId, const FIVXHiroSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|FriendBattles")
    void GetActiveFriendBattles(const FIVXFriendBattleDelegate& OnComplete);

private:
    static TWeakObjectPtr<UIVXHiroSystems> Singleton;

    UPROPERTY()
    UNakamaClient* NakamaClient = nullptr;

    UPROPERTY()
    UNakamaSession* NakamaSession = nullptr;

    bool HasValidClient() const;
    void LogDebug(const FString& Message) const;
    void LogError(const FString& Message) const;
};
