// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXMultiplayerKernel.h"
#include "NakamaClient.h"
#include "NakamaSession.h"
#include "NakamaRealtimeClient.h"
#include "Dom/JsonObject.h"
#include "Dom/JsonValue.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"
#include "Misc/Guid.h"

#define LOCTEXT_NAMESPACE "IVXMultiplayerKernel"

namespace
{
    /** Minimum match-time advance per outbound; server is final authority. */
    constexpr int32 KERNEL_RPC_TIMEOUT_SEC = 10;

    FString MakeUuidV4()
    {
        return FGuid::NewGuid().ToString(EGuidFormats::DigitsWithHyphensLower);
    }

    /**
     * Build a kernel-shaped envelope JSON. The kernel server expects
     *   { "h": { "s":<seq>, "t":<match_time_ms>, "u":<uuid> }, "p": <payload> }
     * with payload either as raw JSON object or as a base64 string for proto.
     */
    FString BuildEnvelopeJson(int64 Seq, int64 MatchTimeMs, const FString& Uuid, const FString& PayloadJson)
    {
        FString Out;
        TSharedRef<TJsonWriter<TCHAR>> Writer = TJsonWriterFactory<TCHAR>::Create(&Out);
        Writer->WriteObjectStart();
        Writer->WriteObjectStart(TEXT("h"));
        Writer->WriteValue(TEXT("s"), Seq);
        Writer->WriteValue(TEXT("t"), MatchTimeMs);
        Writer->WriteValue(TEXT("u"), Uuid);
        Writer->WriteObjectEnd();
        Writer->WriteIdentifierPrefix(TEXT("p"));
        // Embed PayloadJson verbatim if it parses as object; otherwise wrap as
        // a string. We keep the verbatim path because most templates send
        // structured payloads and we don't want a double-encode.
        if (!PayloadJson.IsEmpty() && PayloadJson.TrimStartAndEnd().StartsWith(TEXT("{")))
        {
            Writer->WriteRawJSONValue(PayloadJson);
        }
        else
        {
            Writer->WriteValue(PayloadJson);
        }
        Writer->WriteObjectEnd();
        Writer->Close();
        return Out;
    }

    /**
     * Parse an inbound envelope from a Nakama match-data byte buffer.
     */
    bool TryParseEnvelope(const FString& InboundJson, int32 OpCode, const FString& Sender, FIVXKernelEnvelope& Out)
    {
        TSharedPtr<FJsonObject> Root;
        TSharedRef<TJsonReader<TCHAR>> Reader = TJsonReaderFactory<TCHAR>::Create(InboundJson);
        if (!FJsonSerializer::Deserialize(Reader, Root) || !Root.IsValid()) return false;

        TSharedPtr<FJsonObject> H;
        Out.Header.OpCode = OpCode;
        Out.Header.SenderUserId = Sender;
        if (Root->TryGetObjectField(TEXT("h"), H) && H.IsValid())
        {
            int64 Tmp;
            if (H->TryGetNumberField(TEXT("s"), Tmp)) Out.Header.Seq = Tmp;
            if (H->TryGetNumberField(TEXT("t"), Tmp)) Out.Header.MatchTimeMs = Tmp;
            FString S;
            if (H->TryGetStringField(TEXT("u"), S)) Out.Header.Uuid = S;
        }
        const TSharedPtr<FJsonValue> P = Root->TryGetField(TEXT("p"));
        if (P.IsValid())
        {
            // Re-serialize the payload sub-tree so game code receives the
            // exact JSON the kernel emitted (round-trippable).
            FString PayloadOut;
            TSharedRef<TJsonWriter<TCHAR>> W = TJsonWriterFactory<TCHAR>::Create(&PayloadOut);
            FJsonSerializer::Serialize(P.ToSharedRef(), TEXT(""), W);
            W->Close();
            Out.PayloadJson = PayloadOut;
        }
        Out.RecvUnixMs = FDateTime::UtcNow().ToUnixTimestamp() * 1000;
        return true;
    }
} // namespace

// ============================================================================
// UIVXMultiplayerKernel
// ============================================================================

void UIVXMultiplayerKernel::Initialize(UNakamaClient* InNakamaClient, UNakamaSession* InSession)
{
    if (bInitialized) return;
    NakamaClient = InNakamaClient;
    Session = InSession;
    if (!NakamaClient.IsValid() || !Session.IsValid())
    {
        UE_LOG(LogTemp, Warning, TEXT("[IVXMultiplayerKernel] Initialize called with null client/session"));
        return;
    }
    // Open one realtime socket per Initialize. Nakama UE plugin handles
    // reconnect with exponential backoff internally; we surface state
    // transitions via OnTransportStateChanged.
    RealtimeClient = NakamaClient->SetupRealtimeClient();
    if (!RealtimeClient.IsValid())
    {
        UE_LOG(LogTemp, Error, TEXT("[IVXMultiplayerKernel] failed to create realtime client"));
        TransportState = EIVXTransportState::FailedFatal;
        OnTransportStateChanged.ExecuteIfBound(TransportState);
        return;
    }
    // TODO(integration): wire RealtimeClient->ConnectionStateChanged to
    // TransitionState. The UE Nakama plugin doesn't expose that delegate
    // directly today; the bridge ticker in IntelliVerseXModule polls
    // GetIsConnected() and emits transitions. Documented in README.
    bInitialized = true;
    TransportState = EIVXTransportState::Connecting;
    OnTransportStateChanged.ExecuteIfBound(TransportState);
}

void UIVXMultiplayerKernel::Shutdown()
{
    if (!bInitialized) return;
    if (RealtimeClient.IsValid())
    {
        // Nakama UE plugin's Disconnect() is fire-and-forget.
        RealtimeClient->Disconnect();
    }
    for (auto& KV : ActiveSessions)
    {
        if (KV.Value.IsValid()) KV.Value->Dispose();
    }
    ActiveSessions.Empty();
    NakamaClient.Reset();
    Session.Reset();
    RealtimeClient.Reset();
    bInitialized = false;
    TransportState = EIVXTransportState::Disconnected;
    OnTransportStateChanged.ExecuteIfBound(TransportState);
}

void UIVXMultiplayerKernel::CreateMatch(const FIVXCreateMatchRequest& Request, const FIVXCreateMatchDelegate& OnDone)
{
    FIVXCreateMatchResponse Empty;
    if (!bInitialized || !NakamaClient.IsValid() || !Session.IsValid())
    {
        UE_LOG(LogTemp, Warning, TEXT("[IVXMultiplayerKernel] CreateMatch called before Initialize"));
        OnDone.ExecuteIfBound(false, Empty);
        return;
    }

    // Build payload conforming to schemas/multiplayer/kernel/match.proto
    // CreateMatchRequest.
    FString PayloadJson;
    {
        TSharedRef<TJsonWriter<TCHAR>> W = TJsonWriterFactory<TCHAR>::Create(&PayloadJson);
        W->WriteObjectStart();
        W->WriteValue(TEXT("template_id"), Request.TemplateId);
        W->WriteValue(TEXT("game_id"),     Request.GameId);
        W->WriteValue(TEXT("region"),      Request.Region);
        if (!Request.TemplateInitJson.IsEmpty())
        {
            W->WriteIdentifierPrefix(TEXT("template_init"));
            W->WriteRawJSONValue(Request.TemplateInitJson);
        }
        W->WriteObjectEnd();
        W->Close();
    }

    NakamaClient->RPC(Session.Get(), TEXT("mp_create_match"), PayloadJson,
        FOnRPC::CreateLambda([OnDone](const FNakamaRPC& Rpc)
        {
            FIVXCreateMatchResponse Resp;
            TSharedPtr<FJsonObject> Root;
            TSharedRef<TJsonReader<TCHAR>> Reader = TJsonReaderFactory<TCHAR>::Create(Rpc.Payload);
            if (FJsonSerializer::Deserialize(Reader, Root) && Root.IsValid())
            {
                Root->TryGetStringField(TEXT("match_id"),    Resp.MatchId);
                Root->TryGetStringField(TEXT("template_id"), Resp.TemplateId);
                Root->TryGetStringField(TEXT("region"),      Resp.Region);
                Root->TryGetNumberField(TEXT("expires_unix_ms"), Resp.ExpiresUnixMs);
            }
            OnDone.ExecuteIfBound(!Resp.MatchId.IsEmpty(), Resp);
        }),
        FOnRPCError::CreateLambda([OnDone](const FNakamaError& Err)
        {
            UE_LOG(LogTemp, Warning, TEXT("[IVXMultiplayerKernel] CreateMatch RPC error: %s"), *Err.Message);
            FIVXCreateMatchResponse Empty;
            OnDone.ExecuteIfBound(false, Empty);
        }));
}

void UIVXMultiplayerKernel::JoinMatch(const FString& MatchId, const FIVXJoinMatchDelegate& OnDone)
{
    if (!bInitialized || !RealtimeClient.IsValid())
    {
        UE_LOG(LogTemp, Warning, TEXT("[IVXMultiplayerKernel] JoinMatch called before Initialize"));
        OnDone.ExecuteIfBound(false, nullptr);
        return;
    }
    UIVXMatchSession* SessionObj = NewObject<UIVXMatchSession>(this);
    SessionObj->MatchId = MatchId;
    SessionObj->LocalUserId = Session.IsValid() ? Session->GetUserId() : FString();
    SessionObj->State = EIVXTransportState::Connecting;
    SessionObj->Internal_BindInfra(RealtimeClient.Get(), this);

    RealtimeClient->JoinMatch(MatchId, TMap<FString, FString>(),
        FOnMatchJoined::CreateLambda([this, MatchId, SessionObj, OnDone](const FNakamaMatch& Match)
        {
            SessionObj->TemplateId = Match.Label; // kernel writes templateId in label
            SessionObj->Internal_TransitionState(EIVXTransportState::Connected);
            ActiveSessions.Add(MatchId, SessionObj);
            OnDone.ExecuteIfBound(true, SessionObj);
        }),
        FOnError::CreateLambda([SessionObj, OnDone](const FNakamaError& Err)
        {
            UE_LOG(LogTemp, Warning, TEXT("[IVXMultiplayerKernel] JoinMatch error: %s"), *Err.Message);
            SessionObj->Internal_TransitionState(EIVXTransportState::FailedFatal);
            OnDone.ExecuteIfBound(false, nullptr);
        }));

    // Wire match-data callback. The UE Nakama plugin fans out OnMatchData on
    // the realtime client; we route it to the right session via MatchId.
    RealtimeClient->SetMatchDataCallback(
        FOnMatchData::CreateLambda([this](const FNakamaMatchData& Data)
        {
            const TWeakObjectPtr<UIVXMatchSession>* Found = ActiveSessions.Find(Data.MatchId);
            if (!Found || !Found->IsValid()) return;
            FIVXKernelEnvelope Env;
            const FString InboundJson(Data.Data); // Nakama gives raw bytes; we treat as UTF-8 JSON.
            if (TryParseEnvelope(InboundJson, Data.OpCode, Data.PresenceUserId, Env))
            {
                (*Found)->Internal_DispatchEnvelope(Env);
            }
        }));
}

void UIVXMultiplayerKernel::CreateAndJoin(const FIVXCreateMatchRequest& Request, const FIVXJoinMatchDelegate& OnDone)
{
    FIVXCreateMatchDelegate Bridge;
    Bridge.BindLambda([this, OnDone](bool bSuccess, const FIVXCreateMatchResponse& Resp)
    {
        if (!bSuccess)
        {
            OnDone.ExecuteIfBound(false, nullptr);
            return;
        }
        JoinMatch(Resp.MatchId, OnDone);
    });
    CreateMatch(Request, Bridge);
}

// ============================================================================
// UIVXMatchSession
// ============================================================================

void UIVXMatchSession::Internal_BindInfra(UNakamaRealtimeClient* InRtClient, UIVXMultiplayerKernel* InOwner)
{
    RtClient = InRtClient;
    Owner = InOwner;
}

void UIVXMatchSession::Internal_DispatchEnvelope(const FIVXKernelEnvelope& Env)
{
    if (bDisposed) return;
    CurrentMatchTimeMs = Env.Header.MatchTimeMs;

    // Exact-opcode handlers first.
    if (TArray<FIVXKernelEnvelopeDelegate>* Bound = Handlers.Find(Env.Header.OpCode))
    {
        for (const FIVXKernelEnvelopeDelegate& H : *Bound)
        {
            H.ExecuteIfBound(Env);
        }
    }
    // Range handlers second.
    for (const FRangeBinding& R : RangeHandlers)
    {
        if (Env.Header.OpCode >= R.From && Env.Header.OpCode <= R.To)
        {
            R.Handler.ExecuteIfBound(Env);
        }
    }
}

void UIVXMatchSession::Internal_TransitionState(EIVXTransportState NewState)
{
    State = NewState;
    OnTransportStateChanged.ExecuteIfBound(NewState);
}

void UIVXMatchSession::Subscribe(int32 OpCode, const FIVXKernelEnvelopeDelegate& Handler)
{
    Handlers.FindOrAdd(OpCode).Add(Handler);
}

void UIVXMatchSession::Unsubscribe(int32 OpCode, const FIVXKernelEnvelopeDelegate& Handler)
{
    if (TArray<FIVXKernelEnvelopeDelegate>* Bound = Handlers.Find(OpCode))
    {
        Bound->RemoveAll([&Handler](const FIVXKernelEnvelopeDelegate& H){
            return H.GetUObject() == Handler.GetUObject() && H.GetFunctionName() == Handler.GetFunctionName();
        });
        if (Bound->Num() == 0) Handlers.Remove(OpCode);
    }
}

void UIVXMatchSession::SubscribeRange(int32 OpCodeFrom, int32 OpCodeTo, const FIVXKernelEnvelopeDelegate& Handler)
{
    FRangeBinding R; R.From = OpCodeFrom; R.To = OpCodeTo; R.Handler = Handler;
    RangeHandlers.Add(R);
}

void UIVXMatchSession::Send(int32 OpCode, const FString& PayloadJson)
{
    if (bDisposed || !RtClient.IsValid()) return;
    LocalSeq++;
    const FString Uuid = MakeUuidV4();
    const FString EnvJson = BuildEnvelopeJson(LocalSeq, CurrentMatchTimeMs, Uuid, PayloadJson);
    RtClient->SendMatchData(MatchId, OpCode, EnvJson, /*Presences*/{});
}

void UIVXMatchSession::Leave()
{
    if (bDisposed || !RtClient.IsValid()) return;
    RtClient->LeaveMatch(MatchId,
        FOnMatchLeft::CreateLambda([](const FNakamaMatch&){}),
        FOnError::CreateLambda([](const FNakamaError&){}));
}

void UIVXMatchSession::Dispose()
{
    if (bDisposed) return;
    bDisposed = true;
    Handlers.Empty();
    RangeHandlers.Empty();
    State = EIVXTransportState::Disconnected;
    OnTransportStateChanged.ExecuteIfBound(State);
    if (UIVXMultiplayerKernel* OwnerStrong = Owner.Get())
    {
        OwnerStrong->ActiveSessions.Remove(MatchId);
    }
}

#undef LOCTEXT_NAMESPACE
