#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "IVXConfig.generated.h"

UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXConfig : public UPrimaryDataAsset
{
    GENERATED_BODY()

public:
    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    FString NakamaHost = TEXT("127.0.0.1");

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    int32 NakamaPort = 7350;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    FString NakamaServerKey = TEXT("defaultkey");

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Nakama")
    bool bUseSSL = false;

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
