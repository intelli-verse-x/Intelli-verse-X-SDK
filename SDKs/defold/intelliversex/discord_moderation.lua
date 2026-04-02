-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Discord moderation — stub matching Unity IVXDiscordModeration.
--- @module discord_moderation

local M = {}

M.auto_moderate_enabled = true

function M.enable_auto_moderation(_enable)
    error("Not implemented")
end

function M.process_moderation_metadata(_message_id, _metadata)
    error("Not implemented")
end

function M.get_moderation_action(_metadata)
    error("Not implemented")
end

function M.start_voice_moderation_capture(_lobby_id)
    error("Not implemented")
end

function M.stop_voice_moderation_capture()
    error("Not implemented")
end

function M.report_user(_user_id, _reason)
    error("Not implemented")
end

return M
