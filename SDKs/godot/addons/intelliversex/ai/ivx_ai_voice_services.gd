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
	push_warning("IVXAIVoiceServices.initialize: Not yet implemented — stub only")

func transcribe_audio(_pcm_data: PackedByteArray, _sample_rate: int = 16000) -> Dictionary:
	push_warning("IVXAIVoiceServices.transcribe_audio: Not yet implemented — stub only")
	return {}

func synthesize_speech(_text: String, _voice_id: String = "") -> PackedByteArray:
	push_warning("IVXAIVoiceServices.synthesize_speech: Not yet implemented — stub only")
	return PackedByteArray()

func list_voices() -> Array:
	push_warning("IVXAIVoiceServices.list_voices: Not yet implemented — stub only")
	return []

func detect_language(_pcm_data: PackedByteArray, _sample_rate: int = 16000) -> Dictionary:
	push_warning("IVXAIVoiceServices.detect_language: Not yet implemented — stub only")
	return {}

func start_streaming_transcription(_sample_rate: int = 16000) -> void:
	push_warning("IVXAIVoiceServices.start_streaming_transcription: Not yet implemented — stub only")

func stop_streaming_transcription() -> void:
	push_warning("IVXAIVoiceServices.stop_streaming_transcription: Not yet implemented — stub only")

func feed_audio_chunk(_pcm_chunk: PackedByteArray) -> void:
	push_warning("IVXAIVoiceServices.feed_audio_chunk: Not yet implemented — stub only")
