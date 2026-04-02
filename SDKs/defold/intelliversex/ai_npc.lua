-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- NPC dialog — stub matching Unity IVXAINPCDialogManager.
--- @module ai_npc

local M = {}

function M.is_initialized()
    return false
end

function M.initialize(_config)
    print("[IVX] ai_npc.initialize: stub – not yet implemented")
    return nil
end

function M.set_auth_token(_token)
    print("[IVX] ai_npc.set_auth_token: stub – not yet implemented")
    return nil
end

function M.register_npc(_profile)
    print("[IVX] ai_npc.register_npc: stub – not yet implemented")
    return nil
end

function M.unregister_npc(_npc_id)
    print("[IVX] ai_npc.unregister_npc: stub – not yet implemented")
    return nil
end

function M.start_dialog(_npc_id, _player_id, _player_context)
    print("[IVX] ai_npc.start_dialog: stub – not yet implemented")
    return nil
end

function M.send_message(_session_id, _message)
    print("[IVX] ai_npc.send_message: stub – not yet implemented")
    return nil
end

function M.end_dialog(_session_id)
    print("[IVX] ai_npc.end_dialog: stub – not yet implemented")
    return nil
end

function M.get_session(_session_id)
    print("[IVX] ai_npc.get_session: stub – not yet implemented")
    return nil
end

function M.get_sessions_for_npc(_npc_id)
    print("[IVX] ai_npc.get_sessions_for_npc: stub – not yet implemented")
    return nil
end

return M
