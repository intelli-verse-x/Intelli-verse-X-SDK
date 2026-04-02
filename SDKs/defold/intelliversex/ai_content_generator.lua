-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Procedural content — stub matching Unity IVXAIContentGenerator.
--- @module ai_content_generator

local M = {}

function M.is_generating()
    return false
end

function M.initialize(_config)
    print("[IVX] ai_content_generator.initialize: stub – not yet implemented")
    return nil
end

function M.generate_quest(_template, _player_context)
    print("[IVX] ai_content_generator.generate_quest: stub – not yet implemented")
    return nil
end

function M.generate_story(_prompt, _genre, _max_words)
    print("[IVX] ai_content_generator.generate_story: stub – not yet implemented")
    return nil
end

function M.generate_item_description(_name, _item_type, _rarity)
    print("[IVX] ai_content_generator.generate_item_description: stub – not yet implemented")
    return nil
end

function M.generate_dialogue(_scenario, _characters)
    print("[IVX] ai_content_generator.generate_dialogue: stub – not yet implemented")
    return nil
end

function M.generate_from_template(_template, _variables)
    print("[IVX] ai_content_generator.generate_from_template: stub – not yet implemented")
    return nil
end

function M.cancel_generation()
    print("[IVX] ai_content_generator.cancel_generation: stub – not yet implemented")
    return nil
end

return M
