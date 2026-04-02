# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXAIVoiceServices
extends Node

## Voice STT/TTS — stub matching Unity IVXAIVoiceServices.

var is_transcribing: bool:
	get:
		return false

var available_voices: Array:
	get:
		return []

func initialize(_config: Resource) -> void:
	push_error("IVXAIVoiceServices.initialize: Not implemented")

func transcribe_audio(_pcm_data: PackedByteArray, _sample_rate: int = 16000) -> Dictionary:
	push_error("IVXAIVoiceServices.transcribe_audio: Not implemented")
	return {}

func synthesize_speech(_text: String, _voice_id: String = "") -> PackedByteArray:
	push_error("IVXAIVoiceServices.synthesize_speech: Not implemented")
	return PackedByteArray()

func list_voices() -> Array:
	push_error("IVXAIVoiceServices.list_voices: Not implemented")
	return []

func detect_language(_pcm_data: PackedByteArray, _sample_rate: int = 16000) -> Dictionary:
	push_error("IVXAIVoiceServices.detect_language: Not implemented")
	return {}

func start_streaming_transcription(_sample_rate: int = 16000) -> void:
	push_error("IVXAIVoiceServices.start_streaming_transcription: Not implemented")

func stop_streaming_transcription() -> void:
	push_error("IVXAIVoiceServices.stop_streaming_transcription: Not implemented")

func feed_audio_chunk(_pcm_chunk: PackedByteArray) -> void:
	push_error("IVXAIVoiceServices.feed_audio_chunk: Not implemented")
