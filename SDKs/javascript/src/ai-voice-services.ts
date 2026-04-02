// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/** AI configuration matching Unity's IVXAIConfig ScriptableObject. */
export interface IVXAIConfig {
  apiBaseUrl: string;
  apiKey?: string;
  provider?: 'intelliversex' | 'openai' | 'azure' | 'anthropic' | 'custom';
  modelName?: string;
  mockMode?: boolean;
  maxRetries?: number;
  retryBaseDelay?: number;
  rateLimitPerSecond?: number;
  requestTimeout?: number;
  debugLogging?: boolean;
  defaultLanguage?: string;
}

// ---------------------------------------------------------------------------
// Data Models
// ---------------------------------------------------------------------------

/**
 * Describes a TTS voice available from the voice service.
 * Mirrors Unity's `IVXAIVoice`.
 */
export interface IVXAIVoice {
  /** Backend voice identifier. */
  voiceId: string;
  /** Display name for UI. */
  displayName: string;
  /** ISO or BCP-47 language code. */
  language: string;
  /** Gender label from the provider. */
  gender: string;
  /** Optional preview audio URL. */
  previewUrl?: string;
  /** Arbitrary style or capability tags. */
  tags?: string[];
}

/**
 * Result of speech-to-text, one-shot or final streaming utterance.
 * Mirrors Unity's `IVXTranscriptionResult`.
 */
export interface IVXTranscriptionResult {
  /** Recognized text. */
  text: string;
  /** Detected or requested language code. */
  language: string;
  /** Confidence 0–1. */
  confidence: number;
  /** True when the utterance is finalized. */
  isFinal: boolean;
}

// ---------------------------------------------------------------------------
// Event callback signatures
// ---------------------------------------------------------------------------

/** Fired for a finalized or one-shot transcription result. */
export type OnTranscriptionResultCallback = (result: IVXTranscriptionResult) => void;

/** Fired with streaming partial hypothesis text. */
export type OnPartialTranscriptionCallback = (text: string) => void;

/** Fired with PCM16 audio bytes from TTS synthesis. */
export type OnSpeechSynthesizedCallback = (pcmData: Uint8Array) => void;

/** Fired on HTTP or WebSocket errors. */
export type OnVoiceErrorCallback = (error: string) => void;

// ---------------------------------------------------------------------------
// IVXAIVoiceServices
// ---------------------------------------------------------------------------

/**
 * Speech-to-text, text-to-speech, voice listing, and streaming transcription.
 *
 * Mirrors Unity's `IVXAIVoiceServices` MonoBehaviour — provides HTTP and
 * WebSocket helpers for STT, TTS, and voice catalog operations.
 *
 * **Reference implementation only.** All methods throw `Error` at runtime.
 * See `docs/guides/ai-getting-started.md` for integration guidance.
 */
export class IVXAIVoiceServices {
  private _config: IVXAIConfig | null = null;

  /** Callback fired for a finalized or one-shot transcription result. */
  onTranscriptionResult: OnTranscriptionResultCallback | null = null;

  /** Callback fired with streaming partial hypothesis text. */
  onPartialTranscription: OnPartialTranscriptionCallback | null = null;

  /** Callback fired with PCM16 audio bytes from TTS synthesis. */
  onSpeechSynthesized: OnSpeechSynthesizedCallback | null = null;

  /** Callback fired on HTTP or WebSocket errors. */
  onError: OnVoiceErrorCallback | null = null;

  /** True while streaming STT is active and the socket is connected. */
  get isTranscribing(): boolean {
    return false;
  }

  /** Voices returned by the last {@link listVoices} call. */
  get availableVoices(): readonly IVXAIVoice[] {
    return [];
  }

  /**
   * Binds AI configuration; required before other calls.
   * @param config - Configuration asset (base URL, keys, timeouts).
   */
  initialize(_config: IVXAIConfig): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sets the bearer token applied to voice HTTP requests.
   * @param token - Bearer token string, or `null` to clear.
   */
  setAuthToken(_token: string | null): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * One-shot speech-to-text over HTTP.
   * Fires {@link onTranscriptionResult} on success.
   * @param pcmData - PCM16 mono audio samples.
   * @param sampleRate - Sample rate in Hz (default 16000).
   * @returns Transcription result, or `null` on failure.
   */
  async transcribeAudio(
    _pcmData: Uint8Array,
    _sampleRate?: number,
  ): Promise<IVXTranscriptionResult | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * One-shot text-to-speech over HTTP; returns PCM16 bytes.
   * Fires {@link onSpeechSynthesized} on success.
   * @param text - Text to synthesize.
   * @param voiceId - Optional voice id override.
   * @returns PCM16 audio bytes, or `null` on failure.
   */
  async synthesizeSpeech(
    _text: string,
    _voiceId?: string,
  ): Promise<Uint8Array | null> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Fetches the catalog of voices and caches {@link availableVoices}.
   * @returns Array of available voice definitions.
   */
  async listVoices(): Promise<IVXAIVoice[]> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Detects spoken language from a short PCM16 sample.
   * @param pcmData - Audio bytes.
   * @param sampleRate - Sample rate in Hz (default 16000).
   * @returns Object with detected `language` code and `confidence` score.
   */
  async detectLanguage(
    _pcmData: Uint8Array,
    _sampleRate?: number,
  ): Promise<{ language: string; confidence: number }> {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Opens a WebSocket to the streaming STT endpoint and begins listening.
   * @param sampleRate - Sample rate advertised to the server (default 16000).
   */
  startStreamingTranscription(_sampleRate?: number): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /** Stops streaming STT and closes the socket. */
  stopStreamingTranscription(): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }

  /**
   * Sends a PCM16 chunk to the active streaming session (binary frame).
   * @param pcmChunk - Raw PCM bytes to feed into the stream.
   */
  feedAudioChunk(_pcmChunk: Uint8Array): void {
    throw new Error(
      'Not implemented — Unity reference implementation only. See docs/guides/ai-getting-started.md',
    );
  }
}
