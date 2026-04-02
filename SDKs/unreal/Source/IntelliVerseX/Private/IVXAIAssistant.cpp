// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIAssistant.h"
#include "IVXManager.h"

TWeakObjectPtr<UIVXAIAssistant> UIVXAIAssistant::Singleton = nullptr;

UIVXAIAssistant* UIVXAIAssistant::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIAssistant>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIAssistant::Shutdown()
{
    if (Singleton.IsValid())
    {
        Singleton->RemoveFromRoot();
        Singleton = nullptr;
    }
}

void UIVXAIAssistant::Initialize(UObject* Config)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIAssistant] Initialize: stub – not yet implemented"));
}

void UIVXAIAssistant::Ask(const FString& Question, const FIVXAssistantTextDelegate& OnResponse)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIAssistant] Ask: stub – not yet implemented"));
    OnResponse.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::GetHint(const FString& LevelId, const FString& ObjectiveId, const FIVXAssistantTextDelegate& OnHint)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIAssistant] GetHint: stub – not yet implemented"));
    OnHint.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::GetTutorial(const FString& FeatureId, const FIVXAssistantTextDelegate& OnTutorialJson)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIAssistant] GetTutorial: stub – not yet implemented"));
    OnTutorialJson.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::SearchKnowledgeBase(const FString& Query, const FIVXAssistantTextDelegate& OnResultsJson)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIAssistant] SearchKnowledgeBase: stub – not yet implemented"));
    OnResultsJson.ExecuteIfBound(TEXT("[]"));
}
