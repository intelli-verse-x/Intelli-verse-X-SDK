-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Discord DMs API — stub matching Unity IVXDiscordMessages.
--- @module discord_messages

local M = {}

function M.is_showing_chat()
    return false
end

function M.send_dm(_recipient_id, _message)
    error("Not implemented")
end

function M.edit_dm(_recipient_id, _message_id, _new_content)
    error("Not implemented")
end

function M.get_dm_history(_recipient_id, _limit)
    error("Not implemented")
end

function M.get_dm_summaries()
    error("Not implemented")
end

function M.set_showing_chat(_showing)
    error("Not implemented")
end

function M.open_message_in_discord(_message_id)
    error("Not implemented")
end

function M.open_dm_settings_in_discord()
    error("Not implemented")
end

return M
