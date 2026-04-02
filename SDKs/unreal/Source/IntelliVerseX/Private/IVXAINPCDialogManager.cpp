// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAINPCDialogManager.h"
#include "IVXManager.h"

TWeakObjectPtr<UIVXAINPCDialogManager> UIVXAINPCDialogManager::Singleton = nullptr;

UIVXAINPCDialogManager* UIVXAINPCDialogManager::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAINPCDialogManager>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAINPCDialogManager::Shutdown()
{
    if (Singleton.IsValid())
    {
        Singleton->RemoveFromRoot();
        Singleton = nullptr;
    }
}

void UIVXAINPCDialogManager::Initialize(UObject* Config)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAINPCDialogManager] Initialize: stub – not yet implemented"));
}

void UIVXAINPCDialogManager::SetAuthToken(const FString& Token)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAINPCDialogManager] SetAuthToken: stub – not yet implemented"));
}

void UIVXAINPCDialogManager::StartDialog(const FString& NpcId, const FString& PlayerId, const FString& PlayerContext,
                                         const FIVXNPCSessionJsonDelegate& OnStarted)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAINPCDialogManager] StartDialog: stub – not yet implemented"));
    OnStarted.ExecuteIfBound(TEXT(""));
}

void UIVXAINPCDialogManager::SendMessage(const FString& SessionId, const FString& Message,
                                        const FIVXNPCSessionJsonDelegate& OnResponse)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAINPCDialogManager] SendMessage: stub – not yet implemented"));
    OnResponse.ExecuteIfBound(TEXT(""));
}

void UIVXAINPCDialogManager::EndDialog(const FString& SessionId)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAINPCDialogManager] EndDialog: stub – not yet implemented"));
}
