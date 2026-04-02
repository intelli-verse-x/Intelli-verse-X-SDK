// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <cstddef>
#include <cstdint>
#include <functional>
#include <stdexcept>
#include <string>
#include <vector>

namespace ivx {

struct AIVoiceInfo {
    std::string voiceId;
    std::string displayName;
    std::string language;
};

struct TranscriptionResult {
    std::string text;
    std::string language;
    float confidence = 0.f;
    bool isFinal = true;
};

/// Voice STT/TTS — stub matching Unity IVXAIVoiceServices.
class IVXAIVoiceServices {
public:
    static IVXAIVoiceServices& instance() {
        static IVXAIVoiceServices inst;
        return inst;
    }

    bool isTranscribing() const { return false; }

    void initialize(void*) { throw std::runtime_error("Not implemented"); }

    void transcribeAudio(const std::uint8_t*, std::size_t len, int sampleRate,
                         std::function<void(const TranscriptionResult*)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void synthesizeSpeech(const std::string&, const std::string& voiceId,
                          std::function<void(const std::vector<std::uint8_t>&)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void listVoices(std::function<void(std::vector<AIVoiceInfo>)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void detectLanguage(const std::uint8_t*, std::size_t, int sampleRate,
                        std::function<void(std::string, float)> = nullptr) {
        throw std::runtime_error("Not implemented");
    }

    void startStreamingTranscription(int sampleRate = 16000) { throw std::runtime_error("Not implemented"); }
    void stopStreamingTranscription() { throw std::runtime_error("Not implemented"); }
    void feedAudioChunk(const std::uint8_t*, std::size_t) { throw std::runtime_error("Not implemented"); }

private:
    IVXAIVoiceServices() = default;
};

} // namespace ivx
