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
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async transcribeAudio(
    _pcmData: Uint8Array,
    _sampleRate?: number
  ): Promise<IVXTranscriptionResult | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async synthesizeSpeech(
    _text: string,
    _voiceId?: string
  ): Promise<Uint8Array | null> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async listVoices(): Promise<IVXAIVoice[]> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  async detectLanguage(
    _pcmData: Uint8Array,
    _sampleRate?: number
  ): Promise<{ language: string; confidence: number }> {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  startStreamingTranscription(_sampleRate?: number): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  stopStreamingTranscription(): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }

  feedAudioChunk(_pcmChunk: Uint8Array): void {
    console.warn('[IVX-Web3] stub – not yet implemented');
    throw new Error('[IVX-Web3] stub – not yet implemented');
  }
}
