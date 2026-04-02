-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- AI text moderation — stub matching Unity IVXAIModerator.
--- @module ai_moderator

local M = {}

function M.is_enabled()
    return false
end

function M.initialize(_config)
    print("[IVX] ai_moderator.initialize: stub – not yet implemented")
    return nil
end

function M.classify_text(_text)
    print("[IVX] ai_moderator.classify_text: stub – not yet implemented")
    return nil
end

function M.filter_message(_text)
    print("[IVX] ai_moderator.filter_message: stub – not yet implemented")
    return nil
end

function M.scan_batch(_messages)
    print("[IVX] ai_moderator.scan_batch: stub – not yet implemented")
    return nil
end

function M.add_custom_rule(_rule)
    print("[IVX] ai_moderator.add_custom_rule: stub – not yet implemented")
    return nil
end

function M.remove_custom_rule(_pattern)
    print("[IVX] ai_moderator.remove_custom_rule: stub – not yet implemented")
    return nil
end

function M.set_custom_rules(_rules)
    print("[IVX] ai_moderator.set_custom_rules: stub – not yet implemented")
    return nil
end

function M.clear_custom_rules()
    print("[IVX] ai_moderator.clear_custom_rules: stub – not yet implemented")
    return nil
end

function M.check_local_rules(_text)
    print("[IVX] ai_moderator.check_local_rules: stub – not yet implemented")
    return nil
end

function M.get_discord_moderation_metadata(_result)
    print("[IVX] ai_moderator.get_discord_moderation_metadata: stub – not yet implemented")
    return nil
end

return M
