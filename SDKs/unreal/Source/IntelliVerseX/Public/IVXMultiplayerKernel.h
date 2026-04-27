// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// IVXMultiplayerKernel — Unreal Engine 5 adapter implementing the same
// IIVXMultiplayer / IIVXMatchSession contract as the Unity, JS, Godot,
// Flutter, Java, C++, and Web3 SDKs.
//
// Wraps the official UE Nakama plugin (UNakamaClient + UNakamaSession +
// UNakamaRealtimeClient) and speaks the wire protocol defined in
// `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
//
// THIS FILE IS THE ADAPTER CONTRACT. The .cpp under Private/ implements it
// against the UE Nakama plugin. Game code only ever talks to this header.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Templates/SharedPointer.h"
#include "Containers/Map.h"
#include "Delegates/Delegate.h"
#include "IVXMultiplayerKernel.generated.h"

class UNakamaClient;
class UNakamaSession;
class UNakamaRealtimeClient;
class UIVXMatchSession;

// ---------------------------------------------------------------------------
// Wire-level structs (Blueprint-friendly, mirrors schemas/multiplayer/*.proto)
// ---------------------------------------------------------------------------

UENUM(BlueprintType)
enum class EIVXTransportState : uint8
{
    Disconnected = 0,
    Connecting   = 1,
    Connected    = 2,
    Reconnecting = 3,
    FailedFatal  = 4,
};

UENUM(BlueprintType)
enum class EIVXEndReason : uint8
{
    Unknown                 = 0,
    Completed               = 1,
    Cancelled               = 2,
    DurationExceeded        = 3,
    KernelInternal          = 4,
    AllPlayersLeft          = 5,
    HostTerminated          = 6,
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXKernelHeader
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int64 Seq = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int64 MatchTimeMs = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString Uuid;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int32 OpCode = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString SenderUserId;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXKernelEnvelope
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FIVXKernelHeader Header;

    /** JSON payload (the kernel sends wire JSON until proto-binary is enabled). */
    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString PayloadJson;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int64 RecvUnixMs = 0;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXCreateMatchRequest
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Multiplayer")
    FString TemplateId;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Multiplayer")
    FString GameId;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Multiplayer")
    FString Region;

    /** JSON-encoded template_init dict. Kernel parses on the server. */
    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Multiplayer")
    FString TemplateInitJson;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXCreateMatchResponse
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString MatchId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString TemplateId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString Region;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int64 ExpiresUnixMs = 0;
};

// ---------------------------------------------------------------------------
// Delegates
// ---------------------------------------------------------------------------

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXCreateMatchDelegate,
    bool, bSuccess, const FIVXCreateMatchResponse&, Response);

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXJoinMatchDelegate,
    bool, bSuccess, UIVXMatchSession*, Session);

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXTransportStateDelegate,
    EIVXTransportState, NewState);

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXKernelEnvelopeDelegate,
    const FIVXKernelEnvelope&, Envelope);

// ---------------------------------------------------------------------------
// UIVXMatchSession — live handle for one joined match
// ---------------------------------------------------------------------------

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXMatchSession : public UObject
{
    GENERATED_BODY()

public:
    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString MatchId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString TemplateId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    FString LocalUserId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int64 CurrentMatchTimeMs = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    int32 ActivePlayerCount = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    EIVXTransportState State = EIVXTransportState::Disconnected;

    /** Subscribe to a single opcode. The same delegate may be registered for
     *  multiple opcodes; call Unsubscribe(OpCode, Handler) to remove. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Subscribe(int32 OpCode, const FIVXKernelEnvelopeDelegate& Handler);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Unsubscribe(int32 OpCode, const FIVXKernelEnvelopeDelegate& Handler);

    /** Subscribe to all opcodes in [From..To]. Useful for game-defined ranges
     *  (0xC000–0xCFFF). */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void SubscribeRange(int32 OpCodeFrom, int32 OpCodeTo, const FIVXKernelEnvelopeDelegate& Handler);

    /** Send `PayloadJson` to the server. Header (seq, match_time_ms, uuid) is
     *  auto-stamped by the adapter. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Send(int32 OpCode, const FString& PayloadJson);

    /** Politely leave the match. Server fans out PLAYER_LEFT to remaining peers. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Leave();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Dispose();

    /** Lifecycle. Register before Join so Welcome isn't missed. */
    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Multiplayer")
    FIVXTransportStateDelegate OnTransportStateChanged;

    // Internal — set by the adapter on creation. Not Blueprint-exposed.
    void Internal_BindInfra(UNakamaRealtimeClient* InRtClient, UIVXMultiplayerKernel* InOwner);
    void Internal_DispatchEnvelope(const FIVXKernelEnvelope& Env);
    void Internal_TransitionState(EIVXTransportState NewState);

private:
    TWeakObjectPtr<UNakamaRealtimeClient> RtClient;
    TWeakObjectPtr<UIVXMultiplayerKernel> Owner;
    /** OpCode -> array of bound delegates. */
    TMap<int32, TArray<FIVXKernelEnvelopeDelegate>> Handlers;
    /** [From..To, Handler] — checked after exact-opcode handlers fire. */
    struct FRangeBinding { int32 From; int32 To; FIVXKernelEnvelopeDelegate Handler; };
    TArray<FRangeBinding> RangeHandlers;
    /** Outbound monotonic sequence for THIS client (server reorders by recv time). */
    int64 LocalSeq = 0;
    /** True after Dispose — drops any further inbound. */
    bool bDisposed = false;
};

// ---------------------------------------------------------------------------
// UIVXMultiplayerKernel — top-level adapter (one per player)
// ---------------------------------------------------------------------------

UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXMultiplayerKernel : public UObject
{
    GENERATED_BODY()

public:
    /** Initialise. Must be called AFTER Nakama auth.  Idempotent. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Initialize(UNakamaClient* InNakamaClient, UNakamaSession* InSession);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void Shutdown();

    /** Calls the `mp_create_match` Nakama RPC. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void CreateMatch(const FIVXCreateMatchRequest& Request, const FIVXCreateMatchDelegate& OnDone);

    /** Joins an existing match by id. Server re-issues Welcome on success. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void JoinMatch(const FString& MatchId, const FIVXJoinMatchDelegate& OnDone);

    /** Convenience: create + join in a single call. Most game code uses this. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Multiplayer")
    void CreateAndJoin(const FIVXCreateMatchRequest& Request, const FIVXJoinMatchDelegate& OnDone);

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Multiplayer")
    EIVXTransportState TransportState = EIVXTransportState::Disconnected;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Multiplayer")
    FIVXTransportStateDelegate OnTransportStateChanged;

private:
    TWeakObjectPtr<UNakamaClient> NakamaClient;
    TWeakObjectPtr<UNakamaSession> Session;
    TWeakObjectPtr<UNakamaRealtimeClient> RealtimeClient;
    /** MatchId -> live session (weak ref so GC reclaims after Dispose). */
    UPROPERTY()
    TMap<FString, TWeakObjectPtr<UIVXMatchSession>> ActiveSessions;
    bool bInitialized = false;
};
