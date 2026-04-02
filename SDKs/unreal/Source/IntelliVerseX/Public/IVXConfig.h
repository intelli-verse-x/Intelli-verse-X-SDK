// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "IVXConfig.generated.h"

UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXConfig : public UPrimaryDataAsset
{
    GENERATED_BODY()

public:
    /** Game ID (UUID) for this title on the IntelliVerseX platform. Obtain from https://intelli-verse-x.ai/developers or POST https://msapi.intelli-verse-x.io/api/games/game/info */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|Game Identity")
    FString GameId;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    FString NakamaHost = TEXT("nakama-rest.intelli-verse-x.ai");

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    int32 NakamaPort = 443;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    FString NakamaServerKey = TEXT("defaultkey");

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    bool bUseSSL = true;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Identity")
    FString CognitoRegion;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Identity")
    FString CognitoUserPoolId;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Identity")
    FString CognitoClientId;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Analytics")
    bool bEnableAnalytics = true;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Debug")
    bool bEnableDebugLogs = false;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Debug")
    bool bVerboseLogging = false;
};
