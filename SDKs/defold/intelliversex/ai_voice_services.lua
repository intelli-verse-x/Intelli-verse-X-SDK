-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Voice STT/TTS — stub matching Unity IVXAIVoiceServices.
--- @module ai_voice_services

local M = {}

function M.is_transcribing()
    return false
end

function M.initialize(_config)
    print("[IVX] ai_voice_services.initialize: stub – not yet implemented")
    return nil
end

function M.transcribe_audio(_pcm_data, _sample_rate)
    print("[IVX] ai_voice_services.transcribe_audio: stub – not yet implemented")
    return nil
end

function M.synthesize_speech(_text, _voice_id)
    print("[IVX] ai_voice_services.synthesize_speech: stub – not yet implemented")
    return nil
end

function M.list_voices()
    print("[IVX] ai_voice_services.list_voices: stub – not yet implemented")
    return nil
end

function M.detect_language(_pcm_data, _sample_rate)
    print("[IVX] ai_voice_services.detect_language: stub – not yet implemented")
    return nil
end

function M.start_streaming_transcription(_sample_rate)
    print("[IVX] ai_voice_services.start_streaming_transcription: stub – not yet implemented")
    return nil
end

function M.stop_streaming_transcription()
    print("[IVX] ai_voice_services.stop_streaming_transcription: stub – not yet implemented")
    return nil
end

function M.feed_audio_chunk(_pcm_chunk)
    print("[IVX] ai_voice_services.feed_audio_chunk: stub – not yet implemented")
    return nil
end

return M
