// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXDiscordLinkedChannels.generated.h"

/**
 * Linked Discord text channel for lobby bridging (Unity IVXDiscordLinkedChannels). Stub surface.
 */
USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXLinkedChannel
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|LinkedChannels")
    FString ChannelId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|LinkedChannels")
    FString GuildId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|LinkedChannels")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|LinkedChannels")
    FString LobbyId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|LinkedChannels")
    int64 LinkedAt = 0;
};

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXLinkedChannelSingleDelegate, bool, bSuccess, const FIVXLinkedChannel&, Channel);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXLinkedChannelArrayDelegate, bool, bSuccess, const TArray<FIVXLinkedChannel>&, Channels);

/**
 * Discord Social SDK — linked channels API.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXDiscordLinkedChannels : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|LinkedChannels",
              meta = (WorldContextObject = "WorldContextObject"))
    static UIVXDiscordLinkedChannels* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|LinkedChannels")
    void LinkChannel(const FString& LobbyId, const FString& ChannelId, const FIVXLinkedChannelSingleDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|LinkedChannels")
    void UnlinkChannel(const FString& LobbyId, const FString& ChannelId, const FIVXLinkedChannelSingleDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|LinkedChannels")
    void GetLinkedChannels(const FString& LobbyId, const FIVXLinkedChannelArrayDelegate& OnComplete);

private:
    static TWeakObjectPtr<UIVXDiscordLinkedChannels> Singleton;
};
