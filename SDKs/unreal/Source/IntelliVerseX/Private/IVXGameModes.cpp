// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXGameModes.h"
#include "IVXManager.h"
#include "Misc/Guid.h"

void UIVXGameModes::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);
    UE_LOG(LogIVX, Log, TEXT("[IVXGameModes] Subsystem initialized"));
}

void UIVXGameModes::Deinitialize()
{
    Players.Empty();
    bMatchInProgress = false;
    bSearching = false;
    Super::Deinitialize();
}

// --- Mode selection ---

void UIVXGameModes::SelectMode(EIVXGameMode Mode)
{
    if (bMatchInProgress)
    {
        LogError(TEXT("Cannot change mode while match is in progress"));
        OnGameModeError.Broadcast(TEXT("Cannot change mode while match is in progress"));
        return;
    }
    CurrentMode = Mode;
    LogDebug(FString::Printf(TEXT("Mode changed to %d"), static_cast<int32>(Mode)));
    OnModeChanged.Broadcast(Mode);
}

// --- Player management ---

void UIVXGameModes::AddPlayer(const FIVXPlayerSlot& Player)
{
    for (const FIVXPlayerSlot& Existing : Players)
    {
        if (Existing.PlayerId == Player.PlayerId)
        {
            LogError(FString::Printf(TEXT("Player %s already in lobby"), *Player.PlayerId));
            return;
        }
    }
    Players.Add(Player);
    LogDebug(FString::Printf(TEXT("Player added: %s"), *Player.DisplayName));
    OnPlayerAdded.Broadcast(Player);
}

void UIVXGameModes::RemovePlayer(const FString& PlayerId)
{
    int32 Removed = Players.RemoveAll([&PlayerId](const FIVXPlayerSlot& S) { return S.PlayerId == PlayerId; });
    if (Removed > 0)
    {
        OnPlayerRemoved.Broadcast(PlayerId);
    }
}

void UIVXGameModes::SetPlayerReady(const FString& PlayerId, bool bReady)
{
    for (FIVXPlayerSlot& Slot : Players)
    {
        if (Slot.PlayerId == PlayerId)
        {
            Slot.bReady = bReady;
            return;
        }
    }
}

bool UIVXGameModes::AllPlayersReady() const
{
    if (Players.Num() == 0) return false;
    for (const FIVXPlayerSlot& Slot : Players)
    {
        if (!Slot.bReady) return false;
    }
    return true;
}

// --- Match lifecycle ---

void UIVXGameModes::StartMatch(const FIVXMatchConfig& Config)
{
    if (bMatchInProgress)
    {
        OnGameModeError.Broadcast(TEXT("Match already in progress"));
        return;
    }

    CurrentMatchId = FGuid::NewGuid().ToString();
    bMatchInProgress = true;
    CurrentMode = Config.Mode;

    LogDebug(FString::Printf(TEXT("Match started: %s"), *CurrentMatchId));
    OnMatchStarted.Broadcast(CurrentMatchId);
}

void UIVXGameModes::EndMatch(const FIVXMatchResult& Result)
{
    if (!bMatchInProgress) return;
    bMatchInProgress = false;
    LogDebug(FString::Printf(TEXT("Match ended: %s, winner: %s"), *CurrentMatchId, *Result.WinnerId));
    OnMatchEnded.Broadcast(Result);
    CurrentMatchId.Empty();
}

// --- Lobby ---

void UIVXGameModes::CreateRoom(const FString& RoomName, const FIVXMatchConfig& Config)
{
    CurrentRoomId = FGuid::NewGuid().ToString();
    LogDebug(FString::Printf(TEXT("Room created: %s (%s)"), *RoomName, *CurrentRoomId));

    FIVXRoomInfo Info;
    Info.RoomId = CurrentRoomId;
    Info.RoomName = RoomName;
    Info.MaxPlayers = Config.MaxPlayers;
    Info.Mode = Config.Mode;
    Info.PlayerCount = 1;
    OnMatchFound.Broadcast(Info);
}

void UIVXGameModes::JoinRoom(const FString& RoomId)
{
    CurrentRoomId = RoomId;
    LogDebug(FString::Printf(TEXT("Joined room: %s"), *RoomId));
}

void UIVXGameModes::ListRooms()
{
    LogDebug(TEXT("Requesting room list"));
}

void UIVXGameModes::LeaveRoom()
{
    LogDebug(FString::Printf(TEXT("Left room: %s"), *CurrentRoomId));
    CurrentRoomId.Empty();
}

// --- Matchmaking ---

void UIVXGameModes::FindMatch(const FIVXMatchConfig& Config)
{
    if (bSearching)
    {
        OnGameModeError.Broadcast(TEXT("Already searching for a match"));
        return;
    }
    bSearching = true;
    LogDebug(TEXT("Matchmaking search started"));
}

void UIVXGameModes::CancelSearch()
{
    bSearching = false;
    LogDebug(TEXT("Matchmaking search cancelled"));
}

// --- Logging ---

void UIVXGameModes::LogDebug(const FString& Message) const
{
    UE_LOG(LogIVX, Log, TEXT("[IVXGameModes] %s"), *Message);
}

void UIVXGameModes::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("[IVXGameModes] %s"), *Message);
}
