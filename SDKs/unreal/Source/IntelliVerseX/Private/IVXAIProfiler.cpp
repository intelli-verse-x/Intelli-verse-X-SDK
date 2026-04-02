// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIProfiler.h"

TWeakObjectPtr<UIVXAIProfiler> UIVXAIProfiler::Singleton = nullptr;

UIVXAIProfiler* UIVXAIProfiler::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIProfiler>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIProfiler::Initialize(UObject* Config, const FString& PlayerId) {}

void UIVXAIProfiler::TrackEvent(const FString& EventName) {}

void UIVXAIProfiler::FlushEvents() {}

void UIVXAIProfiler::GetPlayerProfile(const FIVXProfilerJsonDelegate& OnComplete)
{
    OnComplete.ExecuteIfBound(TEXT("{}"));
}
