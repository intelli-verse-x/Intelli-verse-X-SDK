// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export interface IVXAIVoice {
  voiceId: string;
  displayName: string;
  language: string;
  gender: string;
  previewUrl?: string;
  tags?: string[];
}

export interface IVXTranscriptionResult {
  text: string;
  language: string;
  confidence: number;
  isFinal: boolean;
}

/**
 * STT, TTS, voice listing, streaming transcription (Unity IVXAIVoiceServices).
 */
export class IVXAIVoiceServices {
  get isTranscribing(): boolean {
    return false;
  }

  get availableVoices(): readonly IVXAIVoice[] {
    return [];
  }

  initialize(_config: unknown): void {
    throw new Error('Not implemented');
  }

  async transcribeAudio(
    _pcmData: Uint8Array,
    _sampleRate?: number
  ): Promise<IVXTranscriptionResult | null> {
    throw new Error('Not implemented');
  }

  async synthesizeSpeech(
    _text: string,
    _voiceId?: string
  ): Promise<Uint8Array | null> {
    throw new Error('Not implemented');
  }

  async listVoices(): Promise<IVXAIVoice[]> {
    throw new Error('Not implemented');
  }

  async detectLanguage(
    _pcmData: Uint8Array,
    _sampleRate?: number
  ): Promise<{ language: string; confidence: number }> {
    throw new Error('Not implemented');
  }

  startStreamingTranscription(_sampleRate?: number): void {
    throw new Error('Not implemented');
  }

  stopStreamingTranscription(): void {
    throw new Error('Not implemented');
  }

  feedAudioChunk(_pcmChunk: Uint8Array): void {
    throw new Error('Not implemented');
  }
}
