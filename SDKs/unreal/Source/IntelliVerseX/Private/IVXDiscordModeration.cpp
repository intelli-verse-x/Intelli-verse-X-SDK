// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXDiscordModeration.h"

TWeakObjectPtr<UIVXDiscordModeration> UIVXDiscordModeration::Singleton = nullptr;

UIVXDiscordModeration* UIVXDiscordModeration::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXDiscordModeration>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXDiscordModeration::EnableAutoModeration(bool bEnable) {}

void UIVXDiscordModeration::ProcessModerationMetadata(int64 MessageId, const TMap<FString, FString>& Metadata) {}

void UIVXDiscordModeration::StartVoiceModerationCapture(int64 LobbyId) {}

void UIVXDiscordModeration::StopVoiceModerationCapture() {}

void UIVXDiscordModeration::ReportUser(int64 UserId, const FString& Reason, const FIVXModerationBoolDelegate& OnComplete)
{
    OnComplete.ExecuteIfBound(false);
}
