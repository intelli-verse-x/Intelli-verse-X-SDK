// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

import 'dart:typed_data';

class IVXAIVoice {
  String voiceId = '';
  String displayName = '';
  String language = '';
  String gender = '';
  String? previewUrl;
  List<String>? tags;
}

class IVXTranscriptionResult {
  String text = '';
  String language = '';
  double confidence = 0;
  bool isFinal = false;
}

/// Voice STT/TTS — stub matching Unity [IVXAIVoiceServices].
class IVXAIVoiceServices {
  IVXAIVoiceServices._();
  static final IVXAIVoiceServices instance = IVXAIVoiceServices._();

  bool get isTranscribing => false;
  List<IVXAIVoice> get availableVoices => const [];

  void initialize(Object? config) {
    throw UnimplementedError('IVXAIVoiceServices.initialize');
  }

  Future<IVXTranscriptionResult?> transcribeAudio(
    Uint8List pcmData, {
    int sampleRate = 16000,
  }) async {
    throw UnimplementedError('IVXAIVoiceServices.transcribeAudio');
  }

  Future<Uint8List?> synthesizeSpeech(
    String text, [
    String? voiceId,
  ]) async {
    throw UnimplementedError('IVXAIVoiceServices.synthesizeSpeech');
  }

  Future<List<IVXAIVoice>> listVoices() async {
    throw UnimplementedError('IVXAIVoiceServices.listVoices');
  }

  Future<({String? language, double confidence})> detectLanguage(
    Uint8List pcmData, {
    int sampleRate = 16000,
  }) async {
    throw UnimplementedError('IVXAIVoiceServices.detectLanguage');
  }

  void startStreamingTranscription({int sampleRate = 16000}) {
    throw UnimplementedError('IVXAIVoiceServices.startStreamingTranscription');
  }

  void stopStreamingTranscription() {
    throw UnimplementedError('IVXAIVoiceServices.stopStreamingTranscription');
  }

  void feedAudioChunk(Uint8List pcmChunk) {
    throw UnimplementedError('IVXAIVoiceServices.feedAudioChunk');
  }
}
