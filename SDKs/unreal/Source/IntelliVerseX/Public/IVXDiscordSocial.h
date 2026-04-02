// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXDiscordSocial.generated.h"

// --- Config ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXDiscordConfig
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|Discord")
    int64 ApplicationId = 0;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|Discord")
    FString DefaultLobbySecret;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|Discord")
    bool bEnableVoice = true;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|Discord")
    bool bEnableOverlay = false;
};

// --- Unified Friend ---

UENUM(BlueprintType)
enum class EIVXFriendSource : uint8
{
    Game     UMETA(DisplayName = "Game"),
    Discord  UMETA(DisplayName = "Discord"),
    Both     UMETA(DisplayName = "Both")
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXUnifiedFriend
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Friends")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Friends")
    FString DisplayName;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Friends")
    FString AvatarUrl;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Friends")
    EIVXFriendSource Source = EIVXFriendSource::Game;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Friends")
    bool bOnline = false;
};

// --- Game Invite ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXGameInvite
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Invites")
    FString InviteId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Invites")
    FString SenderId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Invites")
    FString SenderName;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Invites")
    FString Message;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Invites")
    FString LobbySecret;
};

// --- Voice Participant ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXVoiceParticipant
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Voice")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Voice")
    FString DisplayName;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Voice")
    bool bMuted = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Voice")
    bool bDeafened = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Voice")
    float Volume = 1.0f;
};

// --- Lobby Message ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXLobbyMessage
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Lobby")
    FString SenderId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Lobby")
    FString SenderName;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Lobby")
    FString Content;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Lobby")
    int64 Timestamp = 0;
};

// --- Delegates ---

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXDiscordSuccessDelegate, bool, bSuccess);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXUnifiedFriendsDelegate, bool, bSuccess, const TArray<FIVXUnifiedFriend>&, Friends);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXGameInviteDelegate, bool, bSuccess, const FIVXGameInvite&, Invite);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXVoiceParticipantsDelegate, bool, bSuccess, const TArray<FIVXVoiceParticipant>&, Participants);

// --- Multicast Events ---

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXOnDiscordReady, bool, bProvisional);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXOnDiscordError, const FString&, ErrorMessage);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXOnInviteReceived, const FIVXGameInvite&, Invite);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FIVXOnJoinRequest, const FString&, UserId, const FString&, Username);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXOnLobbyMessage, const FIVXLobbyMessage&, Message);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FIVXOnVoiceStateUpdate, const FString&, UserId, bool, bSpeaking);

/**
 * Wraps Discord Social SDK features: Rich Presence, unified friends,
 * lobby chat, voice calls, and game invites.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXDiscordSocial : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord", meta = (DisplayName = "Get IVX Discord Social", WorldContext = "WorldContextObject"))
    static UIVXDiscordSocial* GetInstance(UObject* WorldContextObject);

    // --- Manager ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord")
    void Initialize(const FIVXDiscordConfig& Config, const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord")
    void LinkAccount(const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord")
    void UnlinkAccount(const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Discord")
    bool IsInitialized() const;

    // --- Rich Presence ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Presence")
    void SetActivity(const FString& Details, const FString& State, int64 StartTimestamp = 0, int64 EndTimestamp = 0);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Presence")
    void SetParty(const FString& PartyId, int32 CurrentSize, int32 MaxSize, const FString& JoinSecret);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Presence")
    void ClearPresence();

    // --- Friends ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Friends")
    void GetUnifiedFriends(const FIVXUnifiedFriendsDelegate& OnComplete);

    // --- Lobby ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Lobby")
    void CreateOrJoinLobby(const FString& LobbySecret, const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Lobby")
    void LeaveLobby(const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Lobby")
    void SendLobbyMessage(const FString& Content);

    // --- Voice ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void JoinVoiceCall(const FString& LobbyId, const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void LeaveVoiceCall(const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void SetSelfMute(bool bMute);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void SetSelfDeafen(bool bDeafen);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void SetParticipantVolume(const FString& UserId, float Volume);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Voice")
    void GetVoiceParticipants(const FIVXVoiceParticipantsDelegate& OnComplete);

    // --- Invites ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Invites")
    void SendInvite(const FString& UserId, const FString& Message, const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Invites")
    void AcceptInvite(const FString& InviteId, const FIVXDiscordSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Invites")
    void DeclineInvite(const FString& InviteId);

    // --- Events ---

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnDiscordReady OnDiscordReady;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnDiscordError OnDiscordError;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnInviteReceived OnInviteReceived;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnJoinRequest OnJoinRequest;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnLobbyMessage OnLobbyMessageReceived;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Discord|Events")
    FIVXOnVoiceStateUpdate OnVoiceStateUpdate;

private:
    static TWeakObjectPtr<UIVXDiscordSocial> Singleton;

    FIVXDiscordConfig CurrentConfig;
    bool bInitialized = false;
    FString ActiveLobbyId;
    FString ActiveVoiceChannelId;
    bool bSelfMuted = false;
    bool bSelfDeafened = false;

    bool EnsureInitialized() const;
    void LogDebug(const FString& Message) const;
    void LogError(const FString& Message) const;
};
