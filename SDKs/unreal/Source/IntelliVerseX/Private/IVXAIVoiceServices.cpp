// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXAIVoiceServices.h"

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

void UIVXAIVoiceServices::Initialize(UObject* Config) {}

void UIVXAIVoiceServices::TranscribeAudio(const TArray<uint8>& PcmData, int32 SampleRate, const FIVXVoiceTextDelegate& OnText)
{
    OnText.ExecuteIfBound(TEXT(""));
}

void UIVXAIVoiceServices::SynthesizeSpeech(const FString& Text, const FString& VoiceId, const FIVXVoiceBytesDelegate& OnAudio)
{
    OnAudio.ExecuteIfBound(TArray<uint8>());
}

void UIVXAIVoiceServices::ListVoices(const FIVXVoiceTextDelegate& OnJson)
{
    OnJson.ExecuteIfBound(TEXT("[]"));
}
