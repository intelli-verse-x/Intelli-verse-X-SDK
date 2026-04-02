// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAIModerator.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXAIModerationJsonDelegate, const FString&, Json);

/**
 * AI text moderation (Unity IVXAIModerator). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIModerator : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Moderation", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAIModerator* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Moderation")
    void Initialize(UObject* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Moderation")
    void ClassifyText(const FString& Text, const FIVXAIModerationJsonDelegate& OnResult);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Moderation")
    void FilterMessage(const FString& Text, const FIVXAIModerationJsonDelegate& OnFiltered);

private:
    static TWeakObjectPtr<UIVXAIModerator> Singleton;
};
