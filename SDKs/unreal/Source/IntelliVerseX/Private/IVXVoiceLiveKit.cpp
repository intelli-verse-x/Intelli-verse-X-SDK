// IVXVoiceLiveKit — Unreal implementation of the IVX LiveKit voice provider.
//
// NOTE: This wraps the LiveKit Native (rust+C++) SDK that ships as a UE
// ThirdParty plugin. To keep the IVX module compileable WITHOUT the SDK
// installed (engineless games / dedicated servers without voice) we
// stub the LiveKit calls behind `WITH_IVX_LIVEKIT`. Add the define to
// the build target to enable real audio:
//
//   PublicDefinitions.Add("WITH_IVX_LIVEKIT=1");
//   PublicDependencyModuleNames.Add("LiveKit"); // ThirdParty/LiveKit

#include "IVXVoiceLiveKit.h"
#include "Engine/Engine.h"

#if WITH_IVX_LIVEKIT
#include "LiveKitClient.h"
#endif

UIVXLiveKitVoiceProvider::UIVXLiveKitVoiceProvider() {}

void UIVXLiveKitVoiceProvider::Connect(const FIVXVoiceSessionToken& Token)
{
    if (Token.Token.IsEmpty() || Token.Url.IsEmpty())
    {
        OnVoiceUnavailable.Broadcast(TEXT("livekit_token_missing"));
        return;
    }

#if !WITH_IVX_LIVEKIT
    UE_LOG(LogTemp, Warning, TEXT("[IVXLiveKitVoiceProvider] WITH_IVX_LIVEKIT=0 — voice degrade to none."));
    OnVoiceUnavailable.Broadcast(TEXT("livekit_sdk_not_compiled"));
    return;
#else
    LiveKit::ConnectOptions Opts;
    Opts.bAutoSubscribe = Token.bCanSubscribe;
    LiveKit::Client::Connect(Token.Url, Token.Token, Opts,
        [this, bCanPublish = Token.bCanPublish](bool bConnected, FString Error)
        {
            bIsConnected = bConnected;
            OnConnectionChanged.Broadcast(bConnected);
            if (!bConnected)
            {
                OnVoiceUnavailable.Broadcast(FString::Printf(TEXT("livekit_connect_failed: %s"), *Error));
                return;
            }
            if (bCanPublish)
            {
                LiveKit::Client::PublishMicrophone();
            }
        });
#endif
}

void UIVXLiveKitVoiceProvider::Disconnect()
{
#if WITH_IVX_LIVEKIT
    LiveKit::Client::Disconnect();
#endif
    bIsConnected = false;
    OnConnectionChanged.Broadcast(false);
}

void UIVXLiveKitVoiceProvider::SetLocalMute(bool bMuted)
{
    bIsLocallyMuted = bMuted;
#if WITH_IVX_LIVEKIT
    LiveKit::Client::SetMicrophoneMuted(bMuted);
#endif
}

void UIVXLiveKitVoiceProvider::RequestSpeaker(const FString& /*TopicHint*/) {}
void UIVXLiveKitVoiceProvider::ReleaseSpeaker() {}

void UIVXLiveKitVoiceProvider::PublishSpatialPosition(const FIVXPoseFrameRef& FrameRef, float X, float Y, float Z, float YawDeg)
{
#if WITH_IVX_LIVEKIT
    const FString Payload = FString::Printf(
        TEXT("{\"frame\":\"%s\",\"x\":%.3f,\"y\":%.3f,\"z\":%.3f,\"yaw\":%.1f,\"ts\":%lld}"),
        *FrameRef.FrameId, X, Y, Z, YawDeg, FrameRef.TsMs);
    LiveKit::Client::PublishData(TCHAR_TO_UTF8(*Payload), /*bReliable*/ false);
#endif
}

void UIVXLiveKitVoiceProvider::SetVoiceMode(EIVXVoiceMode Mode)
{
    CurrentMode = Mode;
    OnVoiceModeChanged.Broadcast(Mode);
}

void UIVXLiveKitVoiceProvider::OnKernelSpeakerStateChanged(const FIVXSpeakerStateChanged& Ev)
{
    bHasFloor = Ev.bGranted;
    OnSpeakerStateChanged.Broadcast(Ev);
}

void UIVXLiveKitVoiceProvider::OnKernelProviderFailover(EIVXVoiceProvider Next)
{
    OnProviderFailover.Broadcast(Next);
    Disconnect();
}
