// IVXVoiceLiveKit — Unreal Engine 5 client for the IVX IIVXVoice abstraction
// against LiveKit's Native (C++) SDK.
//
// Wire contract: schemas/multiplayer/services/voice.proto.
//
// LiveKit ships a C++/Rust client that compiles into a UE plugin via
// ThirdParty/livekit-rtc-engine. The implementation routes session-token
// minting through the IVX kernel (IVXMultiplayer) and translates incoming
// audio tracks to UE's AudioMixer with positional Submix attenuation
// driven by the active SpatialFrame.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Object.h"
#include "Delegates/DelegateCombinations.h"
#include "IVXVoiceLiveKit.generated.h"

UENUM(BlueprintType)
enum class EIVXVoiceProvider : uint8
{
    Unspecified = 0 UMETA(DisplayName = "Unspecified"),
    LiveKit     = 1 UMETA(DisplayName = "LiveKit"),
    Agora       = 2 UMETA(DisplayName = "Agora"),
    Twilio      = 3 UMETA(DisplayName = "Twilio"),
    Dolby       = 4 UMETA(DisplayName = "Dolby"),
    None        = 5 UMETA(DisplayName = "None"),
};

UENUM(BlueprintType)
enum class EIVXVoiceMode : uint8
{
    Off       = 0,
    Broadcast = 1,
    Spatial   = 2,
    Ptt       = 3,
};

USTRUCT(BlueprintType)
struct FIVXVoiceSessionToken
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    EIVXVoiceProvider Provider = EIVXVoiceProvider::LiveKit;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    FString Token;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    FString RoomId;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    FString Identity;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    FString Url;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    int64 ExpiresAtMs = 0;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    bool bCanPublish = true;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    bool bCanSubscribe = true;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    bool bSpatial = true;

    UPROPERTY(BlueprintReadWrite, Category = "IVX")
    FString Region;
};

USTRUCT(BlueprintType)
struct FIVXSpeakerStateChanged
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IVX") FString UserId;
    UPROPERTY(BlueprintReadOnly, Category = "IVX") bool bGranted = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX") bool bMutedBySelf = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX") bool bMutedByKernel = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX") int32 FloorSecondsRemaining = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX") FString Reason;
};

USTRUCT(BlueprintType)
struct FIVXPoseFrameRef
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IVX") FString FrameId;
    UPROPERTY(BlueprintReadWrite, Category = "IVX") int64 TsMs = 0;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVoiceConnectionChanged, bool, bConnected);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVoiceSpeakerChanged, const FIVXSpeakerStateChanged&, Ev);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVoiceModeChanged, EIVXVoiceMode, Mode);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVoiceProviderFailover, EIVXVoiceProvider, NextProvider);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVoiceUnavailable, const FString&, Reason);

/**
 * UObject wrapper for the LiveKit Voice provider on Unreal. Spawn one per
 * match session; call Connect() with a kernel-minted session token.
 */
UCLASS(BlueprintType, Blueprintable, ClassGroup = "IVX|Voice")
class INTELLIVERSEX_API UIVXLiveKitVoiceProvider : public UObject
{
    GENERATED_BODY()

public:
    UIVXLiveKitVoiceProvider();

    UPROPERTY(BlueprintReadOnly, Category = "IVX|Voice") EIVXVoiceProvider Provider = EIVXVoiceProvider::LiveKit;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Voice") bool bIsConnected = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Voice") bool bIsLocallyMuted = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Voice") bool bHasFloor = false;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Voice") EIVXVoiceMode CurrentMode = EIVXVoiceMode::Off;

    UPROPERTY(BlueprintAssignable, Category = "IVX|Voice") FIVXVoiceConnectionChanged OnConnectionChanged;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Voice") FIVXVoiceSpeakerChanged OnSpeakerStateChanged;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Voice") FIVXVoiceModeChanged OnVoiceModeChanged;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Voice") FIVXVoiceProviderFailover OnProviderFailover;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Voice") FIVXVoiceUnavailable OnVoiceUnavailable;

    /** Connect to the LiveKit room described by the session token. */
    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void Connect(const FIVXVoiceSessionToken& Token);

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void Disconnect();

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void SetLocalMute(bool bMuted);

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void RequestSpeaker(const FString& TopicHint);

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void ReleaseSpeaker();

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void PublishSpatialPosition(const FIVXPoseFrameRef& FrameRef, float X, float Y, float Z, float YawDeg);

    UFUNCTION(BlueprintCallable, Category = "IVX|Voice")
    void SetVoiceMode(EIVXVoiceMode Mode);

    /** Internal: kernel feeds SpeakerStateChanged here. */
    void OnKernelSpeakerStateChanged(const FIVXSpeakerStateChanged& Ev);

    /** Internal: kernel signals provider failover. */
    void OnKernelProviderFailover(EIVXVoiceProvider Next);
};
