// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAIAssistant.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXAssistantTextDelegate, const FString&, Text);

/**
 * In-game AI assistant (Unity IVXAIAssistant). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIAssistant : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAIAssistant* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI")
    static void Shutdown();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant")
    void Initialize(UObject* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant")
    void Ask(const FString& Question, const FIVXAssistantTextDelegate& OnResponse);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant")
    void GetHint(const FString& LevelId, const FString& ObjectiveId, const FIVXAssistantTextDelegate& OnHint);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant")
    void GetTutorial(const FString& FeatureId, const FIVXAssistantTextDelegate& OnTutorialJson);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Assistant")
    void SearchKnowledgeBase(const FString& Query, const FIVXAssistantTextDelegate& OnResultsJson);

private:
    static TWeakObjectPtr<UIVXAIAssistant> Singleton;
};
