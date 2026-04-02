-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Voice STT/TTS — stub matching Unity IVXAIVoiceServices.
--- @module ai_voice_services

local M = {}

function M.is_transcribing()
    return false
end

function M.initialize(_config)
    error("Not implemented")
end

function M.transcribe_audio(_pcm_data, _sample_rate)
    error("Not implemented")
end

function M.synthesize_speech(_text, _voice_id)
    error("Not implemented")
end

function M.list_voices()
    error("Not implemented")
end

function M.detect_language(_pcm_data, _sample_rate)
    error("Not implemented")
end

function M.start_streaming_transcription(_sample_rate)
    error("Not implemented")
end

function M.stop_streaming_transcription()
    error("Not implemented")
end

function M.feed_audio_chunk(_pcm_chunk)
    error("Not implemented")
end

return M
