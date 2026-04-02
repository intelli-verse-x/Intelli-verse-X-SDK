// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIModerator.h"
#include "IVXManager.h"

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

void UIVXAIModerator::Shutdown()
{
    if (Singleton.IsValid())
    {
        Singleton->RemoveFromRoot();
        Singleton = nullptr;
    }
}

void UIVXAIModerator::Initialize(UObject* Config)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIModerator] Initialize: stub – not yet implemented"));
}

void UIVXAIModerator::ClassifyText(const FString& Text, const FIVXAIModerationJsonDelegate& OnResult)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIModerator] ClassifyText: stub – not yet implemented"));
    OnResult.ExecuteIfBound(TEXT("{}"));
}

void UIVXAIModerator::FilterMessage(const FString& Text, const FIVXAIModerationJsonDelegate& OnFiltered)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIModerator] FilterMessage: stub – not yet implemented"));
    OnFiltered.ExecuteIfBound(Text);
}
