// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXDiscordMessages.h"

TWeakObjectPtr<UIVXDiscordMessages> UIVXDiscordMessages::Singleton = nullptr;

UIVXDiscordMessages* UIVXDiscordMessages::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXDiscordMessages>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXDiscordMessages::SendDM(int64 RecipientId, const FString& Message, const FIVXDMMessageIdDelegate& OnSuccess,
                                 const FIVXDMStringDelegate& OnError)
{
    OnError.ExecuteIfBound(TEXT("Not implemented"));
}

void UIVXDiscordMessages::EditDM(int64 RecipientId, int64 MessageId, const FString& NewContent,
                                 const FIVXDiscordSuccessDelegate& OnSuccess, const FIVXDMStringDelegate& OnError)
{
    OnError.ExecuteIfBound(TEXT("Not implemented"));
}

void UIVXDiscordMessages::GetDMHistory(int64 RecipientId, int32 Limit, const FIVXDMStringDelegate& OnComplete)
{
    OnComplete.ExecuteIfBound(TEXT("[]"));
}

void UIVXDiscordMessages::GetDMSummaries(const FIVXDMStringDelegate& OnComplete)
{
    OnComplete.ExecuteIfBound(TEXT("[]"));
}

void UIVXDiscordMessages::SetShowingChat(bool bShowing)
{
    bShowingChat = bShowing;
}

void UIVXDiscordMessages::OpenMessageInDiscord(int64 MessageId) {}

void UIVXDiscordMessages::OpenDMSettingsInDiscord() {}
