// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAIVoiceServices.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXVoiceBytesDelegate, const TArray<uint8>&, PcmBytes);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXVoiceTextDelegate, const FString&, Text);

/**
 * AI voice STT/TTS (Unity IVXAIVoiceServices). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIVoiceServices : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAIVoiceServices* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI")
    static void Shutdown();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void Initialize(UObject* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void TranscribeAudio(const TArray<uint8>& PcmData, int32 SampleRate, const FIVXVoiceTextDelegate& OnText);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void SynthesizeSpeech(const FString& Text, const FString& VoiceId, const FIVXVoiceBytesDelegate& OnAudio);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void ListVoices(const FIVXVoiceTextDelegate& OnJson);

private:
    static TWeakObjectPtr<UIVXAIVoiceServices> Singleton;
};
