// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIClient.h"
#include "IVXManager.h"
#include "HttpModule.h"
#include "Interfaces/IHttpResponse.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"

TWeakObjectPtr<UIVXAIClient> UIVXAIClient::Singleton = nullptr;

UIVXAIClient* UIVXAIClient::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIClient>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIClient::Initialize(const FString& ApiBaseUrl, const FString& InApiKey)
{
    BaseUrl = ApiBaseUrl.TrimEnd();
    if (BaseUrl.EndsWith(TEXT("/")))
    {
        BaseUrl.LeftChopInline(1);
    }
    ApiKey = InApiKey;
    bIsInitialized = true;
    LogDebug(TEXT("Initialized"));
}

// --- REST helpers ---

TSharedRef<IHttpRequest> UIVXAIClient::CreateRequest(const FString& Endpoint, const FString& Verb) const
{
    TSharedRef<IHttpRequest> Req = FHttpModule::Get().CreateRequest();
    Req->SetURL(BaseUrl + Endpoint);
    Req->SetVerb(Verb);
    Req->SetHeader(TEXT("Content-Type"), TEXT("application/json"));
    Req->SetHeader(TEXT("Authorization"), FString::Printf(TEXT("Bearer %s"), *ApiKey));
    return Req;
}

TSharedPtr<FJsonObject> UIVXAIClient::ParseResponse(FHttpResponsePtr Response, bool bSucceeded) const
{
    if (!bSucceeded || !Response.IsValid())
    {
        return nullptr;
    }
    TSharedPtr<FJsonObject> Json;
    TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(Response->GetContentAsString());
    FJsonSerializer::Deserialize(Reader, Json);
    return Json;
}

// --- Voice sessions ---

void UIVXAIClient::StartVoiceSession(const FString& PersonaId, const FString& UserId, const FIVXAISessionDelegate& OnComplete)
{
    if (!bIsInitialized)
    {
        LogError(TEXT("StartVoiceSession called before Initialize"));
        FIVXAISessionResponse Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    TSharedRef<IHttpRequest> Req = CreateRequest(TEXT("/v1/ai/voice/sessions"), TEXT("POST"));
    TSharedPtr<FJsonObject> Body = MakeShared<FJsonObject>();
    Body->SetStringField(TEXT("persona_id"), PersonaId);
    Body->SetStringField(TEXT("user_id"), UserId);
    FString Payload;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Payload);
    FJsonSerializer::Serialize(Body.ToSharedRef(), Writer);
    Req->SetContentAsString(Payload);

    FIVXAISessionDelegate Cb = OnComplete;
    Req->OnProcessRequestComplete().BindLambda(
        [this, Cb](FHttpRequestPtr, FHttpResponsePtr Response, bool bOk)
        {
            FIVXAISessionResponse Result;
            TSharedPtr<FJsonObject> Json = ParseResponse(Response, bOk);
            bool bSuccess = Json.IsValid() && Response->GetResponseCode() >= 200 && Response->GetResponseCode() < 300;
            if (bSuccess)
            {
                Result.SessionId = Json->GetStringField(TEXT("session_id"));
                Result.Status = Json->GetStringField(TEXT("status"));
                Result.WebSocketUrl = Json->GetStringField(TEXT("ws_url"));
            }
            Cb.ExecuteIfBound(bSuccess, Result);
        });
    Req->ProcessRequest();
}

void UIVXAIClient::EndVoiceSession(const FString& SessionId)
{
    if (!bIsInitialized) return;
    TSharedRef<IHttpRequest> Req = CreateRequest(FString::Printf(TEXT("/v1/ai/voice/sessions/%s"), *SessionId), TEXT("DELETE"));
    Req->ProcessRequest();
}

void UIVXAIClient::SendText(const FString& SessionId, const FString& Text)
{
    if (!bIsInitialized) return;

    TSharedRef<IHttpRequest> Req = CreateRequest(FString::Printf(TEXT("/v1/ai/voice/sessions/%s/messages"), *SessionId), TEXT("POST"));
    TSharedPtr<FJsonObject> Body = MakeShared<FJsonObject>();
    Body->SetStringField(TEXT("text"), Text);
    FString Payload;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Payload);
    FJsonSerializer::Serialize(Body.ToSharedRef(), Writer);
    Req->SetContentAsString(Payload);

    Req->OnProcessRequestComplete().BindLambda(
        [this, SessionId](FHttpRequestPtr, FHttpResponsePtr Response, bool bOk)
        {
            TSharedPtr<FJsonObject> Json = ParseResponse(Response, bOk);
            if (Json.IsValid())
            {
                FIVXAIMessage Msg;
                Msg.Role = Json->GetStringField(TEXT("role"));
                Msg.Content = Json->GetStringField(TEXT("content"));
                Msg.Timestamp = Json->GetStringField(TEXT("timestamp"));
                OnMessageReceived.Broadcast(SessionId, Msg);
            }
            else
            {
                OnAIError.Broadcast(SessionId, TEXT("Failed to send text message"));
            }
        });
    Req->ProcessRequest();
}

// --- Host sessions ---

void UIVXAIClient::StartHostSession(const FString& MatchId, const FIVXHostProfile& Profile, const FIVXAISessionDelegate& OnComplete)
{
    if (!bIsInitialized)
    {
        LogError(TEXT("StartHostSession called before Initialize"));
        FIVXAISessionResponse Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    TSharedRef<IHttpRequest> Req = CreateRequest(TEXT("/v1/ai/host/sessions"), TEXT("POST"));
    TSharedPtr<FJsonObject> Body = MakeShared<FJsonObject>();
    Body->SetStringField(TEXT("match_id"), MatchId);
    Body->SetStringField(TEXT("persona_id"), Profile.PersonaId);
    Body->SetStringField(TEXT("display_name"), Profile.DisplayName);
    Body->SetStringField(TEXT("voice_id"), Profile.VoiceId);
    Body->SetStringField(TEXT("language"), Profile.Language);

    TSharedPtr<FJsonObject> Extras = MakeShared<FJsonObject>();
    for (const auto& Pair : Profile.ExtraParams)
    {
        Extras->SetStringField(Pair.Key, Pair.Value);
    }
    Body->SetObjectField(TEXT("extra"), Extras);

    FString Payload;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Payload);
    FJsonSerializer::Serialize(Body.ToSharedRef(), Writer);
    Req->SetContentAsString(Payload);

    FIVXAISessionDelegate Cb = OnComplete;
    Req->OnProcessRequestComplete().BindLambda(
        [this, Cb](FHttpRequestPtr, FHttpResponsePtr Response, bool bOk)
        {
            FIVXAISessionResponse Result;
            TSharedPtr<FJsonObject> Json = ParseResponse(Response, bOk);
            bool bSuccess = Json.IsValid() && Response->GetResponseCode() >= 200 && Response->GetResponseCode() < 300;
            if (bSuccess)
            {
                Result.SessionId = Json->GetStringField(TEXT("session_id"));
                Result.Status = Json->GetStringField(TEXT("status"));
                Result.WebSocketUrl = Json->GetStringField(TEXT("ws_url"));
            }
            Cb.ExecuteIfBound(bSuccess, Result);
        });
    Req->ProcessRequest();
}

void UIVXAIClient::SendHostEvent(const FString& SessionId, const FString& EventType, const FString& Data)
{
    if (!bIsInitialized) return;

    TSharedRef<IHttpRequest> Req = CreateRequest(FString::Printf(TEXT("/v1/ai/host/sessions/%s/events"), *SessionId), TEXT("POST"));
    TSharedPtr<FJsonObject> Body = MakeShared<FJsonObject>();
    Body->SetStringField(TEXT("event_type"), EventType);
    Body->SetStringField(TEXT("data"), Data);
    FString Payload;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Payload);
    FJsonSerializer::Serialize(Body.ToSharedRef(), Writer);
    Req->SetContentAsString(Payload);
    Req->ProcessRequest();
}

// --- Entitlement ---

void UIVXAIClient::CheckEntitlement(const FString& UserId, const FIVXEntitlementDelegate& OnComplete)
{
    if (!bIsInitialized)
    {
        FIVXAIEntitlement Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    TSharedRef<IHttpRequest> Req = CreateRequest(FString::Printf(TEXT("/v1/ai/entitlements/%s"), *UserId), TEXT("GET"));

    FIVXEntitlementDelegate Cb = OnComplete;
    Req->OnProcessRequestComplete().BindLambda(
        [this, Cb](FHttpRequestPtr, FHttpResponsePtr Response, bool bOk)
        {
            FIVXAIEntitlement Result;
            TSharedPtr<FJsonObject> Json = ParseResponse(Response, bOk);
            bool bSuccess = Json.IsValid() && Response->GetResponseCode() >= 200 && Response->GetResponseCode() < 300;
            if (bSuccess)
            {
                Result.bEntitled = Json->GetBoolField(TEXT("entitled"));
                Result.Tier = Json->GetStringField(TEXT("tier"));
                Result.RemainingCredits = Json->GetIntegerField(TEXT("remaining_credits"));
                Result.ExpiresAt = Json->GetStringField(TEXT("expires_at"));
            }
            Cb.ExecuteIfBound(bSuccess, Result);
        });
    Req->ProcessRequest();
}

// --- Personas ---

void UIVXAIClient::GetPersonas(const FIVXPersonasDelegate& OnComplete)
{
    if (!bIsInitialized)
    {
        TArray<FIVXAIPersona> Empty;
        OnComplete.ExecuteIfBound(false, Empty);
        return;
    }

    TSharedRef<IHttpRequest> Req = CreateRequest(TEXT("/v1/ai/personas"), TEXT("GET"));

    FIVXPersonasDelegate Cb = OnComplete;
    Req->OnProcessRequestComplete().BindLambda(
        [this, Cb](FHttpRequestPtr, FHttpResponsePtr Response, bool bOk)
        {
            TArray<FIVXAIPersona> Results;
            TSharedPtr<FJsonObject> Json = ParseResponse(Response, bOk);
            bool bSuccess = Json.IsValid() && Response->GetResponseCode() >= 200 && Response->GetResponseCode() < 300;
            if (bSuccess)
            {
                const TArray<TSharedPtr<FJsonValue>>* Arr;
                if (Json->TryGetArrayField(TEXT("personas"), Arr))
                {
                    for (const auto& Val : *Arr)
                    {
                        const TSharedPtr<FJsonObject>& Obj = Val->AsObject();
                        if (!Obj.IsValid()) continue;
                        FIVXAIPersona P;
                        P.PersonaId = Obj->GetStringField(TEXT("persona_id"));
                        P.Name = Obj->GetStringField(TEXT("name"));
                        P.Description = Obj->GetStringField(TEXT("description"));
                        P.VoiceId = Obj->GetStringField(TEXT("voice_id"));
                        P.AvatarUrl = Obj->GetStringField(TEXT("avatar_url"));
                        Results.Add(P);
                    }
                }
            }
            Cb.ExecuteIfBound(bSuccess, Results);
        });
    Req->ProcessRequest();
}

// --- Logging ---

void UIVXAIClient::LogDebug(const FString& Message) const
{
    UE_LOG(LogIVX, Log, TEXT("[IVXAIClient] %s"), *Message);
}

void UIVXAIClient::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("[IVXAIClient] %s"), *Message);
}
