-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Procedural content — stub matching Unity IVXAIContentGenerator.
--- @module ai_content_generator

local M = {}

function M.is_generating()
    return false
end

function M.initialize(_config)
    error("Not implemented")
end

function M.generate_quest(_template, _player_context)
    error("Not implemented")
end

function M.generate_story(_prompt, _genre, _max_words)
    error("Not implemented")
end

function M.generate_item_description(_name, _item_type, _rarity)
    error("Not implemented")
end

function M.generate_dialogue(_scenario, _characters)
    error("Not implemented")
end

function M.generate_from_template(_template, _variables)
    error("Not implemented")
end

function M.cancel_generation()
    error("Not implemented")
end

return M
