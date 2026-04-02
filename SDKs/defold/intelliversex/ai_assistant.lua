-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- In-game AI assistant — stub matching Unity IVXAIAssistant.
--- @module ai_assistant

local M = {}

function M.is_processing()
    return false
end

function M.is_initialized()
    return false
end

function M.initialize(_config)
    print("[IVX] ai_assistant.initialize: stub – not yet implemented")
    return nil
end

function M.set_auth_token(_token)
    print("[IVX] ai_assistant.set_auth_token: stub – not yet implemented")
    return nil
end

function M.clear_history()
    print("[IVX] ai_assistant.clear_history: stub – not yet implemented")
    return nil
end

function M.set_system_prompt(_prompt)
    print("[IVX] ai_assistant.set_system_prompt: stub – not yet implemented")
    return nil
end

function M.ask(_question, _game_context)
    print("[IVX] ai_assistant.ask: stub – not yet implemented")
    return nil
end

function M.get_hint(_level_id, _objective_id, _game_context)
    print("[IVX] ai_assistant.get_hint: stub – not yet implemented")
    return nil
end

function M.get_tutorial(_feature_id)
    print("[IVX] ai_assistant.get_tutorial: stub – not yet implemented")
    return nil
end

function M.search_knowledge_base(_query)
    print("[IVX] ai_assistant.search_knowledge_base: stub – not yet implemented")
    return nil
end

return M
