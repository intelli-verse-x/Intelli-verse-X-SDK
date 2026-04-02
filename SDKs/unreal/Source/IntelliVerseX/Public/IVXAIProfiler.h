// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAIProfiler.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXProfilerJsonDelegate, const FString&, Json);

/**
 * AI player profiling (Unity IVXAIProfiler). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIProfiler : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Profiler", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAIProfiler* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Profiler")
    void Initialize(UObject* Config, const FString& PlayerId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Profiler")
    void TrackEvent(const FString& EventName);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Profiler")
    void FlushEvents();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Profiler")
    void GetPlayerProfile(const FIVXProfilerJsonDelegate& OnComplete);

private:
    static TWeakObjectPtr<UIVXAIProfiler> Singleton;
};
