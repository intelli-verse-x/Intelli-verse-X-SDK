// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "IVXGameModes.generated.h"

UENUM(BlueprintType)
enum class EIVXGameMode : uint8
{
    Solo              UMETA(DisplayName = "Solo"),
    LocalMultiplayer  UMETA(DisplayName = "Local Multiplayer"),
    OnlineVersus      UMETA(DisplayName = "Online Versus"),
    OnlineCoop        UMETA(DisplayName = "Online Co-op"),
    Ranked            UMETA(DisplayName = "Ranked"),
    TurnBased         UMETA(DisplayName = "Turn-Based")
};

// --- Struct types ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXPlayerSlot
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    FString PlayerId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    FString DisplayName;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    int32 SlotIndex = 0;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    bool bReady = false;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    bool bIsLocal = true;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXMatchConfig
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    EIVXGameMode Mode = EIVXGameMode::Solo;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    int32 MaxPlayers = 4;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    int32 RoundCount = 1;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    float TimeLimitSeconds = 0.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|GameModes")
    TMap<FString, FString> CustomProperties;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXRoomInfo
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    FString RoomId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    FString RoomName;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    FString HostId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    int32 PlayerCount = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    int32 MaxPlayers = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    EIVXGameMode Mode = EIVXGameMode::Solo;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    bool bInProgress = false;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXMatchResult
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    FString MatchId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    FString WinnerId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    TMap<FString, int32> PlayerScores;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|GameModes")
    float DurationSeconds = 0.0f;
};

// --- Delegates ---

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXModeChanged, EIVXGameMode, NewMode);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXPlayerAdded, const FIVXPlayerSlot&, Player);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXPlayerRemoved, const FString&, PlayerId);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXMatchFound, const FIVXRoomInfo&, Room);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXRoomListUpdated, const TArray<FIVXRoomInfo>&, Rooms);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXMatchStarted, const FString&, MatchId);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXMatchEnded, const FIVXMatchResult&, Result);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXGameModeError, const FString&, ErrorMessage);

/**
 * Manages game mode selection, lobby, and matchmaking for IntelliVerseX.
 * Operates as a GameInstanceSubsystem for automatic lifecycle management.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXGameModes : public UGameInstanceSubsystem
{
    GENERATED_BODY()

public:
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    // --- Mode selection ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void SelectMode(EIVXGameMode Mode);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|GameModes")
    EIVXGameMode GetCurrentMode() const { return CurrentMode; }

    // --- Player management ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void AddPlayer(const FIVXPlayerSlot& Player);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void RemovePlayer(const FString& PlayerId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void SetPlayerReady(const FString& PlayerId, bool bReady);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|GameModes")
    TArray<FIVXPlayerSlot> GetPlayers() const { return Players; }

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|GameModes")
    bool AllPlayersReady() const;

    // --- Match lifecycle ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void StartMatch(const FIVXMatchConfig& Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes")
    void EndMatch(const FIVXMatchResult& Result);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|GameModes")
    bool IsMatchInProgress() const { return bMatchInProgress; }

    // --- Lobby ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Lobby")
    void CreateRoom(const FString& RoomName, const FIVXMatchConfig& Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Lobby")
    void JoinRoom(const FString& RoomId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Lobby")
    void ListRooms();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Lobby")
    void LeaveRoom();

    // --- Matchmaking ---

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Matchmaking")
    void FindMatch(const FIVXMatchConfig& Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|GameModes|Matchmaking")
    void CancelSearch();

    // --- Events ---

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXModeChanged OnModeChanged;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXPlayerAdded OnPlayerAdded;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXPlayerRemoved OnPlayerRemoved;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXMatchFound OnMatchFound;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXRoomListUpdated OnRoomListUpdated;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXMatchStarted OnMatchStarted;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXMatchEnded OnMatchEnded;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|GameModes|Events")
    FOnIVXGameModeError OnGameModeError;

private:
    EIVXGameMode CurrentMode = EIVXGameMode::Solo;
    TArray<FIVXPlayerSlot> Players;
    bool bMatchInProgress = false;
    bool bSearching = false;
    FString CurrentMatchId;
    FString CurrentRoomId;

    void LogDebug(const FString& Message) const;
    void LogError(const FString& Message) const;
};
