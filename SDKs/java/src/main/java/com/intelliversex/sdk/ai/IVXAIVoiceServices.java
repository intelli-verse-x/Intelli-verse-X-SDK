// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import java.util.List;
import java.util.concurrent.CompletableFuture;

/**
 * Speech-to-text, text-to-speech, voices, streaming STT (Unity {@code IVXAIVoiceServices}).
 */
public final class IVXAIVoiceServices {

    private static final IVXAIVoiceServices INSTANCE = new IVXAIVoiceServices();

    private IVXAIVoiceServices() {}

    public static IVXAIVoiceServices getInstance() {
        return INSTANCE;
    }

    public boolean isTranscribing() {
        return false;
    }

    public List<IVXAIVoice> getAvailableVoices() {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void initialize(Object config) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public CompletableFuture<IVXTranscriptionResult> transcribeAudio(byte[] pcmData, int sampleRate) {
        return failed();
    }

    public CompletableFuture<byte[]> synthesizeSpeech(String text, String voiceId) {
        return failed();
    }

    public CompletableFuture<List<IVXAIVoice>> listVoices() {
        return failed();
    }

    public CompletableFuture<IVXLanguageDetectResult> detectLanguage(byte[] pcmData, int sampleRate) {
        return failed();
    }

    public void startStreamingTranscription(int sampleRate) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void stopStreamingTranscription() {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void feedAudioChunk(byte[] pcmChunk) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    private static <T> CompletableFuture<T> failed() {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not yet implemented — stub only"));
        return f;
    }

    public static final class IVXAIVoice {
        public String voiceId;
        public String displayName;
        public String language;
        public String gender;
        public String previewUrl;
        public String[] tags;
    }

    public static final class IVXTranscriptionResult {
        public String text;
        public String language;
        public float confidence;
        public boolean isFinal;
    }

    public static final class IVXLanguageDetectResult {
        public String language;
        public float confidence;
    }
}
