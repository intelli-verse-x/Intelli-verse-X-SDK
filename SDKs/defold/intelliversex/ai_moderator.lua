-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- AI text moderation — stub matching Unity IVXAIModerator.
--- @module ai_moderator

local M = {}

function M.is_enabled()
    return false
end

function M.initialize(_config)
    error("Not implemented")
end

function M.classify_text(_text)
    error("Not implemented")
end

function M.filter_message(_text)
    error("Not implemented")
end

function M.scan_batch(_messages)
    error("Not implemented")
end

function M.add_custom_rule(_rule)
    error("Not implemented")
end

function M.remove_custom_rule(_pattern)
    error("Not implemented")
end

function M.set_custom_rules(_rules)
    error("Not implemented")
end

function M.clear_custom_rules()
    error("Not implemented")
end

function M.check_local_rules(_text)
    error("Not implemented")
end

function M.get_discord_moderation_metadata(_result)
    error("Not implemented")
end

return M
