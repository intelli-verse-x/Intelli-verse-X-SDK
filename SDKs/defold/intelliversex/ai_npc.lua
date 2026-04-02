-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- NPC dialog — stub matching Unity IVXAINPCDialogManager.
--- @module ai_npc

local M = {}

function M.is_initialized()
    return false
end

function M.initialize(_config)
    error("Not implemented")
end

function M.set_auth_token(_token)
    error("Not implemented")
end

function M.register_npc(_profile)
    error("Not implemented")
end

function M.unregister_npc(_npc_id)
    error("Not implemented")
end

function M.start_dialog(_npc_id, _player_id, _player_context)
    error("Not implemented")
end

function M.send_message(_session_id, _message)
    error("Not implemented")
end

function M.end_dialog(_session_id)
    error("Not implemented")
end

function M.get_session(_session_id)
    error("Not implemented")
end

function M.get_sessions_for_npc(_npc_id)
    error("Not implemented")
end

return M
