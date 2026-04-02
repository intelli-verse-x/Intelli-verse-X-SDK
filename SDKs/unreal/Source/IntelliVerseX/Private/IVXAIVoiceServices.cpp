// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIVoiceServices.h"
#include "IVXManager.h"

TWeakObjectPtr<UIVXAIVoiceServices> UIVXAIVoiceServices::Singleton = nullptr;

UIVXAIVoiceServices* UIVXAIVoiceServices::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXAIVoiceServices>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXAIVoiceServices::Shutdown()
{
    if (Singleton.IsValid())
    {
        Singleton->RemoveFromRoot();
        Singleton = nullptr;
    }
}

void UIVXAIVoiceServices::Initialize(UObject* Config)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIVoiceServices] Initialize: stub – not yet implemented"));
}

void UIVXAIVoiceServices::TranscribeAudio(const TArray<uint8>& PcmData, int32 SampleRate, const FIVXVoiceTextDelegate& OnText)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIVoiceServices] TranscribeAudio: stub – not yet implemented"));
    OnText.ExecuteIfBound(TEXT(""));
}

void UIVXAIVoiceServices::SynthesizeSpeech(const FString& Text, const FString& VoiceId, const FIVXVoiceBytesDelegate& OnAudio)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIVoiceServices] SynthesizeSpeech: stub – not yet implemented"));
    OnAudio.ExecuteIfBound(TArray<uint8>());
}

void UIVXAIVoiceServices::ListVoices(const FIVXVoiceTextDelegate& OnJson)
{
    UE_LOG(LogIVX, Warning, TEXT("[IVXAIVoiceServices] ListVoices: stub – not yet implemented"));
    OnJson.ExecuteIfBound(TEXT("[]"));
}
