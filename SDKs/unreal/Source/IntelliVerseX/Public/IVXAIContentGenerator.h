// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAIContentGenerator.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXContentJsonDelegate, const FString&, Json);

/**
 * AI content generation (Unity IVXAIContentGenerator). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIContentGenerator : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Content", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAIContentGenerator* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Content")
    void Initialize(UObject* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Content")
    void GenerateStory(const FString& Prompt, const FIVXContentJsonDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Content")
    void CancelGeneration();

private:
    static TWeakObjectPtr<UIVXAIContentGenerator> Singleton;
};
