// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXAIVoiceServices.h"
#include "cocos2d.h"
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
    cocos2d::log("[IVX-Cocos] IVXAIVoiceServices::initialize: stub — not yet implemented. AI features will return empty results.");
}

void IVXAIVoiceServices::transcribeAudio(const std::uint8_t*, std::size_t, int,
                                          std::function<void(const IVXTranscriptionResult&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIVoiceServices::synthesizeSpeech(const std::string&, const std::string&,
                                          std::function<void(const std::vector<std::uint8_t>&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIVoiceServices::listVoices(std::function<void(const std::vector<IVXAIVoice>&)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIVoiceServices::detectLanguage(const std::uint8_t*, std::size_t, int,
                                        std::function<void(const std::string&, float)>, ErrorCallback onError) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    if (onError) onError({-1, "Not yet implemented — stub only"});
}

void IVXAIVoiceServices::startStreamingTranscription(int) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIVoiceServices::stopStreamingTranscription() {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

void IVXAIVoiceServices::feedAudioChunk(const std::uint8_t*, std::size_t) {
    cocos2d::log("%s", "[IVX-Cocos] AI stub: not yet implemented — stub only");
    throw std::runtime_error("Not yet implemented — stub only");
}

} // namespace IntelliVerseX
