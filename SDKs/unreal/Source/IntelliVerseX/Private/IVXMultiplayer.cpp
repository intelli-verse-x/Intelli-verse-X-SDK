// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXMultiplayer.h"
#include "IVXManager.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"

namespace
{

void ParseLobbyPlayer(const TSharedPtr<FJsonObject>& Json, FIVXLobbyPlayer& Out)
{
    if (!Json.IsValid()) return;
    FString S;
    if (Json->TryGetStringField(TEXT("userId"), S) || Json->TryGetStringField(TEXT("user_id"), S))
    {
        Out.UserId = MoveTemp(S);
    }
    if (Json->TryGetStringField(TEXT("username"), S))
    {
        Out.Username = MoveTemp(S);
    }
}

void ParseLobbyJson(const TSharedPtr<FJsonObject>& Json, FIVXLobby& Out)
{
    if (!Json.IsValid()) return;
    FString S;
    double N = 0.0;
    bool B = false;

    if (Json->TryGetStringField(TEXT("lobbyId"), S) || Json->TryGetStringField(TEXT("lobby_id"), S))
    {
        Out.LobbyId = MoveTemp(S);
    }
    if (Json->TryGetStringField(TEXT("name"), S))
    {
        Out.Name = MoveTemp(S);
    }
    if (Json->TryGetStringField(TEXT("hostUserId"), S) || Json->TryGetStringField(TEXT("host_user_id"), S))
    {
        Out.HostUserId = MoveTemp(S);
    }
    if (Json->TryGetNumberField(TEXT("maxPlayers"), N) || Json->TryGetNumberField(TEXT("max_players"), N))
    {
        Out.MaxPlayers = static_cast<int32>(N);
    }
    if (Json->TryGetBoolField(TEXT("isPublic"), B) || Json->TryGetBoolField(TEXT("is_public"), B))
    {
        Out.bIsPublic = B;
    }

    const TArray<TSharedPtr<FJsonValue>>* PlayersArr;
    if (Json->TryGetArrayField(TEXT("players"), PlayersArr))
    {
        for (const auto& Val : *PlayersArr)
        {
            if (Val.IsValid() && Val->Type == EJson::Object)
            {
                FIVXLobbyPlayer Player;
                ParseLobbyPlayer(Val->AsObject(), Player);
                Out.Players.Add(Player);
            }
        }
    }

    const TSharedPtr<FJsonObject>* MetaObj;
    if (Json->TryGetObjectField(TEXT("metadata"), MetaObj) && MetaObj && (*MetaObj).IsValid())
    {
        FString MetaStr;
        TSharedRef<TJsonWriter<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>> Writer =
            TJsonWriterFactory<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>::Create(&MetaStr);
        FJsonSerializer::Serialize((*MetaObj).ToSharedRef(), Writer);
        Out.MetadataJson = MetaStr;
    }
}

void ParseMatchmakingTicket(const TSharedPtr<FJsonObject>& Json, FIVXMatchmakingTicket& Out)
{
    if (!Json.IsValid()) return;
    FString S;
    if (Json->TryGetStringField(TEXT("ticketId"), S) || Json->TryGetStringField(TEXT("ticket_id"), S))
    {
        Out.TicketId = MoveTemp(S);
    }
    if (Json->TryGetStringField(TEXT("status"), S))
    {
        Out.Status = MoveTemp(S);
    }
    if (Json->TryGetStringField(TEXT("matchId"), S) || Json->TryGetStringField(TEXT("match_id"), S))
    {
        Out.MatchId = MoveTemp(S);
    }
}

} // anonymous namespace

// ---------------------------------------------------------------------------

TWeakObjectPtr<UIVXMultiplayer> UIVXMultiplayer::Singleton = nullptr;

UIVXMultiplayer* UIVXMultiplayer::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXMultiplayer>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXMultiplayer::SetNakamaClient(UNakamaClient* Client, UNakamaSession* Session)
{
    NakamaClient = Client;
    NakamaSession = Session;
}

bool UIVXMultiplayer::HasValidClient() const
{
    return NakamaClient != nullptr && NakamaSession != nullptr;
}

// ---------------------------------------------------------------------------
// Lobby
// ---------------------------------------------------------------------------

void UIVXMultiplayer::CreateLobby(const FString& Name, int32 MaxPlayers, bool bIsPublic, const FIVXLobbyDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false, FIVXLobby()); return; }

    FString Payload = FString::Printf(TEXT("{\"name\":\"%s\",\"max_players\":%d,\"is_public\":%s}"),
        *Name, MaxPlayers, bIsPublic ? TEXT("true") : TEXT("false"));

    NakamaClient->RPC(NakamaSession, TEXT("create_lobby"), Payload,
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
        {
            FIVXLobby Lobby;
            TSharedPtr<FJsonObject> Json;
            TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
            if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
            {
                ParseLobbyJson(Json, Lobby);
            }
            OnComplete.ExecuteIfBound(true, Lobby);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false, FIVXLobby());
        })
    );
}

void UIVXMultiplayer::JoinLobby(const FString& LobbyId, const FIVXLobbyDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false, FIVXLobby()); return; }

    FString Payload = FString::Printf(TEXT("{\"lobby_id\":\"%s\"}"), *LobbyId);

    NakamaClient->RPC(NakamaSession, TEXT("join_lobby"), Payload,
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
        {
            FIVXLobby Lobby;
            TSharedPtr<FJsonObject> Json;
            TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
            if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
            {
                ParseLobbyJson(Json, Lobby);
            }
            OnComplete.ExecuteIfBound(true, Lobby);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false, FIVXLobby());
        })
    );
}

void UIVXMultiplayer::LeaveLobby(const FString& LobbyId, const FIVXMultiplayerSuccessDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false); return; }

    FString Payload = FString::Printf(TEXT("{\"lobby_id\":\"%s\"}"), *LobbyId);

    NakamaClient->RPC(NakamaSession, TEXT("leave_lobby"), Payload,
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
        {
            OnComplete.ExecuteIfBound(true);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false);
        })
    );
}

void UIVXMultiplayer::ListLobbies(const FIVXLobbyListDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false, TArray<FIVXLobby>()); return; }

    NakamaClient->RPC(NakamaSession, TEXT("list_lobbies"), TEXT("{}"),
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
        {
            TArray<FIVXLobby> Lobbies;
            TSharedPtr<FJsonObject> Root;
            TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
            if (FJsonSerializer::Deserialize(Reader, Root) && Root.IsValid())
            {
                const TArray<TSharedPtr<FJsonValue>>* Arr;
                if (Root->TryGetArrayField(TEXT("lobbies"), Arr))
                {
                    for (const auto& Val : *Arr)
                    {
                        if (Val.IsValid() && Val->Type == EJson::Object)
                        {
                            FIVXLobby L;
                            ParseLobbyJson(Val->AsObject(), L);
                            Lobbies.Add(L);
                        }
                    }
                }
            }
            OnComplete.ExecuteIfBound(true, Lobbies);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false, TArray<FIVXLobby>());
        })
    );
}

// ---------------------------------------------------------------------------
// Matchmaking
// ---------------------------------------------------------------------------

void UIVXMultiplayer::StartMatchmaking(int32 MinPlayers, int32 MaxPlayers, int32 RankRange, const FIVXMatchmakingDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false, FIVXMatchmakingTicket()); return; }

    FString Payload;
    if (RankRange > 0)
    {
        Payload = FString::Printf(TEXT("{\"min_players\":%d,\"max_players\":%d,\"rank_range\":%d}"),
            MinPlayers, MaxPlayers, RankRange);
    }
    else
    {
        Payload = FString::Printf(TEXT("{\"min_players\":%d,\"max_players\":%d}"),
            MinPlayers, MaxPlayers);
    }

    NakamaClient->RPC(NakamaSession, TEXT("start_matchmaking"), Payload,
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC& Rpc)
        {
            FIVXMatchmakingTicket Ticket;
            TSharedPtr<FJsonObject> Json;
            TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Rpc.Payload);
            if (FJsonSerializer::Deserialize(Reader, Json) && Json.IsValid())
            {
                ParseMatchmakingTicket(Json, Ticket);
            }
            OnComplete.ExecuteIfBound(true, Ticket);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false, FIVXMatchmakingTicket());
        })
    );
}

void UIVXMultiplayer::CancelMatchmaking(const FString& TicketId, const FIVXMultiplayerSuccessDelegate& OnComplete)
{
    if (!HasValidClient()) { OnComplete.ExecuteIfBound(false); return; }

    FString Payload = FString::Printf(TEXT("{\"ticket_id\":\"%s\"}"), *TicketId);

    NakamaClient->RPC(NakamaSession, TEXT("cancel_matchmaking"), Payload,
        FOnRPC::CreateLambda([OnComplete](const FNakamaRPC&)
        {
            OnComplete.ExecuteIfBound(true);
        }),
        FOnError::CreateLambda([OnComplete](const FNakamaError&)
        {
            OnComplete.ExecuteIfBound(false);
        })
    );
}
