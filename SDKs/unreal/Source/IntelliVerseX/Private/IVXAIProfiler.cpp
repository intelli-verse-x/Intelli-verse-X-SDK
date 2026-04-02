// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIProfiler.h"
#include "IVXManager.h"

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

void UIVXAIProfiler::Shutdown()
{
    if (Singleton.IsValid())
    {
        Singleton->RemoveFromRoot();
        Singleton = nullptr;
    }
}

void UIVXAIProfiler::Initialize(UObject* Config, const FString& PlayerId)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIProfiler] Initialize: stub – not yet implemented"));
}

void UIVXAIProfiler::TrackEvent(const FString& EventName)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIProfiler] TrackEvent: stub – not yet implemented"));
}

void UIVXAIProfiler::FlushEvents()
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIProfiler] FlushEvents: stub – not yet implemented"));
}

void UIVXAIProfiler::GetPlayerProfile(const FIVXProfilerJsonDelegate& OnComplete)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIProfiler] GetPlayerProfile: stub – not yet implemented"));
    OnComplete.ExecuteIfBound(TEXT("{}"));
}
