// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAINPCDialogManager.h"

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

void UIVXAINPCDialogManager::Initialize(UObject* Config) {}

void UIVXAINPCDialogManager::SetAuthToken(const FString& Token) {}

void UIVXAINPCDialogManager::StartDialog(const FString& NpcId, const FString& PlayerId, const FString& PlayerContext,
                                         const FIVXNPCSessionJsonDelegate& OnStarted)
{
    OnStarted.ExecuteIfBound(TEXT(""));
}

void UIVXAINPCDialogManager::SendMessage(const FString& SessionId, const FString& Message,
                                        const FIVXNPCSessionJsonDelegate& OnResponse)
{
    OnResponse.ExecuteIfBound(TEXT(""));
}

void UIVXAINPCDialogManager::EndDialog(const FString& SessionId) {}
