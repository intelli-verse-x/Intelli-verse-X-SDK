// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "NakamaClient.h"
#include "NakamaSession.h"
#include "IVXMultiplayer.generated.h"

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXLobbyPlayer
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString Username;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXLobby
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString LobbyId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString HostUserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    TArray<FIVXLobbyPlayer> Players;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int32 MaxPlayers = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    bool bIsPublic = true;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString MetadataJson;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXMatchmakingTicket
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString TicketId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString Status;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString MatchId;
};

// ---------------------------------------------------------------------------
// Delegates
// ---------------------------------------------------------------------------

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXLobbyDelegate, bool, bSuccess, const FIVXLobby&, Lobby);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXLobbyListDelegate, bool, bSuccess, const TArray<FIVXLobby>&, Lobbies);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXMultiplayerSuccessDelegate, bool, bSuccess);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXMatchmakingDelegate, bool, bSuccess, const FIVXMatchmakingTicket&, Ticket);

// ---------------------------------------------------------------------------
// UIVXMultiplayer
// ---------------------------------------------------------------------------

UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXMultiplayer : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer", meta = (DisplayName = "Get IVX Multiplayer", WorldContext = "WorldContextObject"))
    static UIVXMultiplayer* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void SetNakamaClient(UNakamaClient* Client, UNakamaSession* Session);

    // --- Lobby ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Lobby")
    void CreateLobby(const FString& Name, int32 MaxPlayers, bool bIsPublic, const FIVXLobbyDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Lobby")
    void JoinLobby(const FString& LobbyId, const FIVXLobbyDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Lobby")
    void LeaveLobby(const FString& LobbyId, const FIVXMultiplayerSuccessDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Lobby")
    void ListLobbies(const FIVXLobbyListDelegate& OnComplete);

    // --- Matchmaking ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Matchmaking")
    void StartMatchmaking(int32 MinPlayers, int32 MaxPlayers, int32 RankRange, const FIVXMatchmakingDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer|Matchmaking")
    void CancelMatchmaking(const FString& TicketId, const FIVXMultiplayerSuccessDelegate& OnComplete);

private:
    static TWeakObjectPtr<UIVXMultiplayer> Singleton;

    UPROPERTY()
    UNakamaClient* NakamaClient = nullptr;

    UPROPERTY()
    UNakamaSession* NakamaSession = nullptr;

    bool HasValidClient() const;
};
