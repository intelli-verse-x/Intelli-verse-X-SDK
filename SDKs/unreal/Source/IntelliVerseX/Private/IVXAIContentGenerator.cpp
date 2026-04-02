// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIContentGenerator.h"

TWeakObjectPtr<UIVXAIContentGenerator> UIVXAIContentGenerator::Singleton = nullptr;

UIVXAIContentGenerator* UIVXAIContentGenerator::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIContentGenerator>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIContentGenerator::Initialize(UObject* Config) {}

void UIVXAIContentGenerator::GenerateStory(const FString& Prompt, const FIVXContentJsonDelegate& OnComplete)
{
    OnComplete.ExecuteIfBound(TEXT("{}"));
}

void UIVXAIContentGenerator::CancelGeneration() {}
