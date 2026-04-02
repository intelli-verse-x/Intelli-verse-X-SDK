// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXDiscordLinkedChannels.h"

TWeakObjectPtr<UIVXDiscordLinkedChannels> UIVXDiscordLinkedChannels::Singleton = nullptr;

UIVXDiscordLinkedChannels* UIVXDiscordLinkedChannels::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXDiscordLinkedChannels>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXDiscordLinkedChannels::LinkChannel(const FString& LobbyId, const FString& ChannelId,
                                            const FIVXLinkedChannelSingleDelegate& OnComplete)
{
    FIVXLinkedChannel Empty;
    OnComplete.ExecuteIfBound(false, Empty);
}

void UIVXDiscordLinkedChannels::UnlinkChannel(const FString& LobbyId, const FString& ChannelId,
                                              const FIVXLinkedChannelSingleDelegate& OnComplete)
{
    FIVXLinkedChannel Empty;
    OnComplete.ExecuteIfBound(false, Empty);
}

void UIVXDiscordLinkedChannels::GetLinkedChannels(const FString& LobbyId, const FIVXLinkedChannelArrayDelegate& OnComplete)
{
    TArray<FIVXLinkedChannel> Empty;
    OnComplete.ExecuteIfBound(false, Empty);
}
