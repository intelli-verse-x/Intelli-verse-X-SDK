-- Copyright (c) 2026 Intelli-verse-X — MIT License

--- Discord Social Settings — notification preferences, privacy, DND mode.
--- Stub: API shape matches Unity IVXDiscordSettings.

local M = {}

local _defaults = {
    notifications_enabled = true,
    friend_requests_enabled = true,
    do_not_disturb = false,
    show_online_status = true,
    allow_direct_messages = true,
}

local _state = {}
for k, v in pairs(_defaults) do _state[k] = v end

function M.get_settings()
    local copy = {}
    for k, v in pairs(_state) do copy[k] = v end
    return copy
end

function M.update_settings(partial)
    for k, v in pairs(partial) do
        if _state[k] ~= nil then
            _state[k] = v
        end
    end
end

function M.enable_dnd() _state.do_not_disturb = true end
function M.disable_dnd() _state.do_not_disturb = false end

function M.reset_to_defaults()
    for k, v in pairs(_defaults) do _state[k] = v end
end

return M
