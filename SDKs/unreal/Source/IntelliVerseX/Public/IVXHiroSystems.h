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

// --- Retention ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXRetentionState
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int64 FirstSessionAt = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int64 LastSessionAt = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int32 TotalSessions = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int32 CurrentSessionDepth = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int32 DaysSinceLastSession = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    FString ChurnRisk;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    bool bOnboardingComplete = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    int32 OnboardingStep = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    bool bComebackBonusAvailable = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|Retention")
    FString ComebackBonusRewardJson;
};

// --- IAP trigger (monetization opt) ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXIapTriggerResult
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|IapTrigger")
    bool bShouldShow = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|IapTrigger")
    FString OfferId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|IapTrigger")
    float Discount = 0.f;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|IapTrigger")
    int64 ExpiresAt = 0;
};

// --- Smart ad timer ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSmartAdResult
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SmartAd")
    bool bCanShow = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SmartAd")
    int64 NextAvailableAt = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Hiro|SmartAd")
    FString Reason;
};

// --- Delegates ---

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXSpinWheelDelegate, bool, bSuccess, const FIVXSpinWheelState&, State);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXSpinResultDelegate, bool, bSuccess, const FIVXSpinWheelReward&, Reward);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXStreakDelegate, bool, bSuccess, const FIVXStreakState&, State);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXOfferListDelegate, bool, bSuccess, const TArray<FIVXOffer>&, Offers);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXHiroSuccessDelegate, bool, bSuccess);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXFriendQuestDelegate, bool, bSuccess, const TArray<FIVXFriendQuest>&, Quests);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXFriendBattleDelegate, bool, bSuccess, const TArray<FIVXFriendBattle>&, Battles);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXRetentionDelegate, bool, bSuccess, const FIVXRetentionState&, State);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXIapTriggerResultDelegate, bool, bSuccess, const FIVXIapTriggerResult&, Result);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXSmartAdResultDelegate, bool, bSuccess, const FIVXSmartAdResult&, Result);

/**
 * Wraps Hiro (Nakama) systems: Spin Wheel, Streaks, Offerwall, Friend Quests, Friend Battles,
 * Retention, IAP triggers, Smart ad timer.
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

    // --- Retention ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Retention")
    void GetRetention(const FIVXRetentionDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|Retention")
    void UpdateRetention(const FIVXRetentionDelegate& OnComplete);

    // --- IAP trigger ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|IapTrigger")
    void CheckIapTrigger(const FString& EventType, const FIVXIapTriggerResultDelegate& OnComplete);

    // --- Smart ad timer ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Hiro|SmartAd")
    void CanShowSmartAd(const FString& Placement, const FIVXSmartAdResultDelegate& OnComplete);

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
