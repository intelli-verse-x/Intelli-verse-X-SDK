// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXSatori.generated.h"

/** Satori client configuration (Heroic Labs Satori). */
USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSatoriConfig
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    FString SatoriUrl;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    FString ApiKey;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    FString IdentityToken;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSatoriEvent
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    FString Name;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    FString Value;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    TMap<FString, FString> Metadata;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Satori")
    int64 Timestamp = 0;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSatoriFlag
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Value;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    bool bConditionChanged = false;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSatoriExperiment
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Variant;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXSatoriLiveEvent
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Id;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Description;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    FString Value;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    int64 ActiveStartTime = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Satori")
    int64 ActiveEndTime = 0;
};

/**
 * Satori analytics — events, flags, experiments, live-ops. Stub surface; integrate Satori HTTP client.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXSatori : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXSatori* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Satori")
    bool IsInitialized() const { return bInitialized; }

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    void Initialize(const FIVXSatoriConfig& Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    void Authenticate(const FString& IdentityId, const TMap<FString, FString>& DefaultProperties,
                      const TMap<FString, FString>& CustomProperties);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    void UpdateIdentity(const TMap<FString, FString>& DefaultProperties, const TMap<FString, FString>& CustomProperties);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    void CaptureEvents(const TArray<FIVXSatoriEvent>& Events);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    TArray<FIVXSatoriFlag> GetAllFlags();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    bool GetFlag(const FString& Name, FIVXSatoriFlag& OutFlag);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    FString GetExperimentVariant(const FString& ExperimentName);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    TArray<FIVXSatoriExperiment> GetAllExperiments();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    TArray<FIVXSatoriLiveEvent> GetLiveEvents();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Satori")
    void Logout();

private:
    static TWeakObjectPtr<UIVXSatori> Singleton;

    bool bInitialized = false;

    UPROPERTY()
    FIVXSatoriConfig Config;

    FString IdentityId;
};
