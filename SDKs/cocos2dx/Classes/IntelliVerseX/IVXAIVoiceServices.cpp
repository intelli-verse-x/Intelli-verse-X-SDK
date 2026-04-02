// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIVoiceServices.h"
#include <stdexcept>

namespace IntelliVerseX {

IVXAIVoiceServices& IVXAIVoiceServices::getInstance() {
    static IVXAIVoiceServices instance;
    return instance;
}

bool IVXAIVoiceServices::isTranscribing() const {
    return false;
}

void IVXAIVoiceServices::initialize(void*) {
    throw std::runtime_error("Not implemented");
}

void IVXAIVoiceServices::transcribeAudio(const std::uint8_t*, std::size_t, int,
                                          std::function<void(const IVXTranscriptionResult&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIVoiceServices::synthesizeSpeech(const std::string&, const std::string&,
                                          std::function<void(const std::vector<std::uint8_t>&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIVoiceServices::listVoices(std::function<void(const std::vector<IVXAIVoice>&)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIVoiceServices::detectLanguage(const std::uint8_t*, std::size_t, int,
                                        std::function<void(const std::string&, float)>, ErrorCallback onError) {
    if (onError) onError({-1, "Not implemented"});
}

void IVXAIVoiceServices::startStreamingTranscription(int) {
    throw std::runtime_error("Not implemented");
}

void IVXAIVoiceServices::stopStreamingTranscription() {
    throw std::runtime_error("Not implemented");
}

void IVXAIVoiceServices::feedAudioChunk(const std::uint8_t*, std::size_t) {
    throw std::runtime_error("Not implemented");
}

} // namespace IntelliVerseX
