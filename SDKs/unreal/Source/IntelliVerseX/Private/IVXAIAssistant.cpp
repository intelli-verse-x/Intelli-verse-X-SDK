// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIAssistant.h"

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

void UIVXAIAssistant::Initialize(UObject* Config) {}

void UIVXAIAssistant::Ask(const FString& Question, const FIVXAssistantTextDelegate& OnResponse)
{
    OnResponse.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::GetHint(const FString& LevelId, const FString& ObjectiveId, const FIVXAssistantTextDelegate& OnHint)
{
    OnHint.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::GetTutorial(const FString& FeatureId, const FIVXAssistantTextDelegate& OnTutorialJson)
{
    OnTutorialJson.ExecuteIfBound(TEXT(""));
}

void UIVXAIAssistant::SearchKnowledgeBase(const FString& Query, const FIVXAssistantTextDelegate& OnResultsJson)
{
    OnResultsJson.ExecuteIfBound(TEXT("[]"));
}
