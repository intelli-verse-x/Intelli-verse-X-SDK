// Copyright (c) 2026 Intelli-verse-X — MIT License.
//
// Implementation of UIVXVoiceTokenClient. Speaks the kernel's
// `mp_voice_token` RPC over the Nakama UE plugin and maps the JSON
// response onto FIVXVoiceSessionToken. Wire shape mirrors
// `nakama/data/modules/src/multiplayer-kernel/voice-providers/index.ts`.

#include "IVXVoiceTokenClient.h"

#include "NakamaClient.h"
#include "NakamaSession.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"

namespace
{
    constexpr const TCHAR* RPC_VOICE_TOKEN = TEXT("mp_voice_token");
    constexpr const TCHAR* LOG_PREFIX      = TEXT("[IVXVoiceTokenClient]");

    EIVXVoiceProvider ProviderFromInt(int32 V)
    {
        switch (V)
        {
            case 1: return EIVXVoiceProvider::LiveKit;
            case 2: return EIVXVoiceProvider::Agora;
            case 3: return EIVXVoiceProvider::Twilio;
            case 4: return EIVXVoiceProvider::Dolby;
            case 5: return EIVXVoiceProvider::None;
            default: return EIVXVoiceProvider::Unspecified;
        }
    }
}

FString UIVXVoiceTokenClient::BuildPayload(const FIVXMintVoiceTokenRequest& Request)
{
    TSharedPtr<FJsonObject> Obj = MakeShared<FJsonObject>();
    Obj->SetStringField(TEXT("match_id"),      Request.MatchId);
    Obj->SetBoolField  (TEXT("can_publish"),   Request.bCanPublish);
    Obj->SetBoolField  (TEXT("can_subscribe"), Request.bCanSubscribe);
    Obj->SetBoolField  (TEXT("spatial"),       Request.bSpatial);
    if (!Request.Region.IsEmpty())
    {
        Obj->SetStringField(TEXT("region"), Request.Region);
    }

    FString Out;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Out);
    FJsonSerializer::Serialize(Obj.ToSharedRef(), Writer);
    return Out;
}

bool UIVXVoiceTokenClient::ParseTokenJson(const FString& Json, FIVXVoiceSessionToken& Out, FString& OutErrorMessage)
{
    if (Json.IsEmpty())
    {
        OutErrorMessage = TEXT("empty payload");
        return false;
    }

    TSharedPtr<FJsonObject> Obj;
    TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Json);
    if (!FJsonSerializer::Deserialize(Reader, Obj) || !Obj.IsValid())
    {
        OutErrorMessage = FString::Printf(TEXT("could not parse JSON: %s"), *Json.Left(120));
        return false;
    }

    int32 ProviderInt = 0;
    if (Obj->TryGetNumberField(TEXT("provider"), ProviderInt))
    {
        Out.Provider = ProviderFromInt(ProviderInt);
    }
    else
    {
        // Default to LiveKit if the kernel didn't tag the provider.
        Out.Provider = EIVXVoiceProvider::LiveKit;
    }

    Obj->TryGetStringField(TEXT("token"),    Out.Token);
    Obj->TryGetStringField(TEXT("room_id"),  Out.RoomId);
    Obj->TryGetStringField(TEXT("identity"), Out.Identity);
    Obj->TryGetStringField(TEXT("url"),      Out.Url);

    int64 Expires = 0;
    if (Obj->TryGetNumberField(TEXT("expires_at_ms"), Expires))
    {
        Out.ExpiresAtMs = Expires;
    }

    bool BField = false;
    if (Obj->TryGetBoolField(TEXT("can_publish"),   BField)) Out.bCanPublish   = BField;
    if (Obj->TryGetBoolField(TEXT("can_subscribe"), BField)) Out.bCanSubscribe = BField;
    if (Obj->TryGetBoolField(TEXT("spatial"),       BField)) Out.bSpatial      = BField;
    Obj->TryGetStringField(TEXT("region"), Out.Region);

    if (Out.Token.IsEmpty() || Out.Url.IsEmpty())
    {
        OutErrorMessage = TEXT("kernel response missing token or url");
        return false;
    }
    return true;
}

void UIVXVoiceTokenClient::MintAsync(
    const FIVXMintVoiceTokenRequest& Request,
    const FIVXMintVoiceTokenSuccess& OnSuccess,
    const FIVXMintVoiceTokenFailure& OnFailure)
{
    if (Request.Client == nullptr)
    {
        OnFailure.ExecuteIfBound(TEXT("bad_args"), TEXT("Client (UNakamaClient*) is null"));
        return;
    }
    if (Request.Session == nullptr)
    {
        OnFailure.ExecuteIfBound(TEXT("bad_args"), TEXT("Session (UNakamaSession*) is null"));
        return;
    }
    if (Request.MatchId.IsEmpty())
    {
        OnFailure.ExecuteIfBound(TEXT("bad_args"), TEXT("MatchId is empty"));
        return;
    }
    // The Nakama UE plugin exposes `IsExpired()` on UNakamaSession in
    // recent versions; gate behind a defensive check that compiles even
    // when the API isn't available (treat as not-expired so the RPC
    // surfaces the real 401 if it actually is).
    #if defined(NAKAMA_UE_HAS_SESSION_ISEXPIRED)
    if (Request.Session->IsExpired())
    {
        OnFailure.ExecuteIfBound(TEXT("session_expired"), TEXT("Nakama session has expired; refresh before minting voice token"));
        return;
    }
    #endif

    const FString Payload = BuildPayload(Request);
    UE_LOG(LogTemp, Log, TEXT("%s POST mp_voice_token match=%s pub=%d sub=%d spatial=%d region=%s"),
        LOG_PREFIX, *Request.MatchId,
        Request.bCanPublish ? 1 : 0, Request.bCanSubscribe ? 1 : 0,
        Request.bSpatial ? 1 : 0, *Request.Region);

    Request.Client->RPC(Request.Session, RPC_VOICE_TOKEN, Payload,
        FOnRPC::CreateLambda([OnSuccess, OnFailure](const FNakamaRPC& Rpc)
        {
            if (Rpc.Payload.IsEmpty())
            {
                OnFailure.ExecuteIfBound(TEXT("voice_unconfigured"),
                    TEXT("kernel returned empty payload (LiveKit env vars missing or feature flag off?)"));
                return;
            }
            FIVXVoiceSessionToken Out;
            FString DecodeError;
            if (!ParseTokenJson(Rpc.Payload, Out, DecodeError))
            {
                OnFailure.ExecuteIfBound(TEXT("decode_failed"), DecodeError);
                return;
            }
            OnSuccess.ExecuteIfBound(Out);
        }),
        FOnError::CreateLambda([OnFailure](const FNakamaError& Err)
        {
            OnFailure.ExecuteIfBound(TEXT("rpc_failed"), Err.Message);
        })
    );
}
