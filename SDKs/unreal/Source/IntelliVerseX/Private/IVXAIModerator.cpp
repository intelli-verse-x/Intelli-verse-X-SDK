// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIModerator.h"

TWeakObjectPtr<UIVXAIModerator> UIVXAIModerator::Singleton = nullptr;

UIVXAIModerator* UIVXAIModerator::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIModerator>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIModerator::Initialize(UObject* Config) {}

void UIVXAIModerator::ClassifyText(const FString& Text, const FIVXAIModerationJsonDelegate& OnResult)
{
    OnResult.ExecuteIfBound(TEXT("{}"));
}

void UIVXAIModerator::FilterMessage(const FString& Text, const FIVXAIModerationJsonDelegate& OnFiltered)
{
    OnFiltered.ExecuteIfBound(Text);
}
