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
    error("Not implemented")
end

function M.set_auth_token(_token)
    error("Not implemented")
end

function M.clear_history()
    error("Not implemented")
end

function M.set_system_prompt(_prompt)
    error("Not implemented")
end

function M.ask(_question, _game_context)
    error("Not implemented")
end

function M.get_hint(_level_id, _objective_id, _game_context)
    error("Not implemented")
end

function M.get_tutorial(_feature_id)
    error("Not implemented")
end

function M.search_knowledge_base(_query)
    error("Not implemented")
end

return M
