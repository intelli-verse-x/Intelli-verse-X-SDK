// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <cstddef>
#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

struct IVXAIVoice {
    std::string voiceId;
    std::string displayName;
};

struct IVXTranscriptionResult {
    std::string text;
    std::string language;
    float confidence = 0.f;
    bool isFinal = true;
};

/// Voice services — stub matching Unity IVXAIVoiceServices.
class IVXAIVoiceServices {
public:
    static IVXAIVoiceServices& getInstance();

    bool isTranscribing() const;

    void initialize(void* config);
    void transcribeAudio(const std::uint8_t* pcm, std::size_t len, int sampleRate,
                         std::function<void(const IVXTranscriptionResult&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void synthesizeSpeech(const std::string& text, const std::string& voiceId,
                          std::function<void(const std::vector<std::uint8_t>&)> onAudio = nullptr, ErrorCallback onError = nullptr);
    void listVoices(std::function<void(const std::vector<IVXAIVoice>&)> onComplete = nullptr, ErrorCallback onError = nullptr);
    void detectLanguage(const std::uint8_t* pcm, std::size_t len, int sampleRate,
                        std::function<void(const std::string&, float)> onResult = nullptr, ErrorCallback onError = nullptr);
    void startStreamingTranscription(int sampleRate = 16000);
    void stopStreamingTranscription();
    void feedAudioChunk(const std::uint8_t* pcm, std::size_t len);

private:
    IVXAIVoiceServices() = default;
};

} // namespace IntelliVerseX
