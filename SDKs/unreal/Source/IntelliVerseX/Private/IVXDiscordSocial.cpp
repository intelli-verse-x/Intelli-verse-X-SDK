// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXDiscordSocial.h"
#include "IVXManager.h"

TWeakObjectPtr<UIVXDiscordSocial> UIVXDiscordSocial::Singleton = nullptr;

UIVXDiscordSocial* UIVXDiscordSocial::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXDiscordSocial>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

// ---------------------------------------------------------------------------
// Manager
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::Initialize(const FIVXDiscordConfig& Config, const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (bInitialized)
    {
        LogDebug(TEXT("Already initialized"));
        OnComplete.ExecuteIfBound(true);
        return;
    }

    CurrentConfig = Config;

    // Discord Social SDK init goes here — stub for integration point.
    // On success the SDK would call back; we simulate that:
    bInitialized = true;
    LogDebug(FString::Printf(TEXT("Initialized with app id %lld"), Config.ApplicationId));
    OnDiscordReady.Broadcast(false);
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::LinkAccount(const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    // Discord account linking flow — integration point.
    LogDebug(TEXT("Account linked"));
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::UnlinkAccount(const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    LogDebug(TEXT("Account unlinked"));
    OnComplete.ExecuteIfBound(true);
}

bool UIVXDiscordSocial::IsInitialized() const
{
    return bInitialized;
}

// ---------------------------------------------------------------------------
// Rich Presence
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::SetActivity(const FString& Details, const FString& State,
                                     int64 StartTimestamp, int64 EndTimestamp)
{
    if (!EnsureInitialized()) return;

    // Build Discord activity and push via SDK.
    LogDebug(FString::Printf(TEXT("SetActivity: %s — %s"), *Details, *State));
}

void UIVXDiscordSocial::SetParty(const FString& PartyId, int32 CurrentSize,
                                  int32 MaxSize, const FString& JoinSecret)
{
    if (!EnsureInitialized()) return;

    LogDebug(FString::Printf(TEXT("SetParty: %s (%d/%d)"), *PartyId, CurrentSize, MaxSize));
}

void UIVXDiscordSocial::ClearPresence()
{
    if (!EnsureInitialized()) return;

    LogDebug(TEXT("Presence cleared"));
}

// ---------------------------------------------------------------------------
// Friends
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::GetUnifiedFriends(const FIVXUnifiedFriendsDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        TArray<FIVXUnifiedFriend> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    // Merge Discord friends with game friends here.
    TArray<FIVXUnifiedFriend> Friends;
    LogDebug(FString::Printf(TEXT("GetUnifiedFriends: returned %d friends"), Friends.Num()));
    OnComplete.ExecuteIfBound(true, Friends);
}

// ---------------------------------------------------------------------------
// Lobby
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::CreateOrJoinLobby(const FString& LobbySecret,
                                           const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    ActiveLobbyId = LobbySecret;
    LogDebug(FString::Printf(TEXT("Joined lobby: %s"), *LobbySecret));
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::LeaveLobby(const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    LogDebug(FString::Printf(TEXT("Left lobby: %s"), *ActiveLobbyId));
    ActiveLobbyId.Empty();
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::SendLobbyMessage(const FString& Content)
{
    if (!EnsureInitialized()) return;
    if (ActiveLobbyId.IsEmpty())
    {
        LogError(TEXT("SendLobbyMessage: not in a lobby"));
        return;
    }

    LogDebug(FString::Printf(TEXT("Lobby message sent: %s"), *Content));
}

// ---------------------------------------------------------------------------
// Voice
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::JoinVoiceCall(const FString& LobbyId,
                                       const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    if (!CurrentConfig.bEnableVoice)
    {
        LogError(TEXT("JoinVoiceCall: voice is disabled in config"));
        OnComplete.ExecuteIfBound(false);
        return;
    }

    ActiveVoiceChannelId = LobbyId;
    LogDebug(FString::Printf(TEXT("Joined voice call: %s"), *LobbyId));
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::LeaveVoiceCall(const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    LogDebug(FString::Printf(TEXT("Left voice call: %s"), *ActiveVoiceChannelId));
    ActiveVoiceChannelId.Empty();
    bSelfMuted = false;
    bSelfDeafened = false;
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::SetSelfMute(bool bMute)
{
    if (!EnsureInitialized()) return;
    bSelfMuted = bMute;
    LogDebug(FString::Printf(TEXT("Self mute: %s"), bMute ? TEXT("true") : TEXT("false")));
}

void UIVXDiscordSocial::SetSelfDeafen(bool bDeafen)
{
    if (!EnsureInitialized()) return;
    bSelfDeafened = bDeafen;
    LogDebug(FString::Printf(TEXT("Self deafen: %s"), bDeafen ? TEXT("true") : TEXT("false")));
}

void UIVXDiscordSocial::SetParticipantVolume(const FString& UserId, float Volume)
{
    if (!EnsureInitialized()) return;
    LogDebug(FString::Printf(TEXT("SetParticipantVolume: %s -> %.2f"), *UserId, Volume));
}

void UIVXDiscordSocial::GetVoiceParticipants(const FIVXVoiceParticipantsDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        TArray<FIVXVoiceParticipant> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    TArray<FIVXVoiceParticipant> Participants;
    OnComplete.ExecuteIfBound(true, Participants);
}

// ---------------------------------------------------------------------------
// Invites
// ---------------------------------------------------------------------------

void UIVXDiscordSocial::SendInvite(const FString& UserId, const FString& Message,
                                    const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    LogDebug(FString::Printf(TEXT("Invite sent to %s: %s"), *UserId, *Message));
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::AcceptInvite(const FString& InviteId,
                                      const FIVXDiscordSuccessDelegate& OnComplete)
{
    if (!EnsureInitialized())
    {
        OnComplete.ExecuteIfBound(false);
        return;
    }

    LogDebug(FString::Printf(TEXT("Invite accepted: %s"), *InviteId));
    OnComplete.ExecuteIfBound(true);
}

void UIVXDiscordSocial::DeclineInvite(const FString& InviteId)
{
    if (!EnsureInitialized()) return;
    LogDebug(FString::Printf(TEXT("Invite declined: %s"), *InviteId));
}

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------

bool UIVXDiscordSocial::EnsureInitialized() const
{
    if (!bInitialized)
    {
        LogError(TEXT("Discord Social SDK not initialized — call Initialize() first"));
    }
    return bInitialized;
}

void UIVXDiscordSocial::LogDebug(const FString& Message) const
{
    UE_LOG(LogIVX, Log, TEXT("[IVXDiscordSocial] %s"), *Message);
}

void UIVXDiscordSocial::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("[IVXDiscordSocial] %s"), *Message);
}
