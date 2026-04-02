-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Discord Social SDK — Debug & logging: route Discord SDK logs to custom sinks.
--- @module discord_debug

local M = {}

M.LOG_LEVEL = {
    NONE = 0,
    ERROR = 1,
    WARN = 2,
    INFO = 3,
    DEBUG = 4,
}

local MAX_HISTORY = 500

local _log_level = M.LOG_LEVEL.WARN
local _callbacks = {}
local _history = {}

local function _now_ms()
    if socket and socket.gettime then
        return math.floor(socket.gettime() * 1000)
    end
    return os.time() * 1000
end

--- Set the minimum log level for Discord SDK output.
--- @param level number One of M.LOG_LEVEL
function M.set_log_level(level)
    _log_level = level or M.LOG_LEVEL.NONE
end

--- @return number Current minimum log level
function M.get_log_level()
    return _log_level
end

--- Register a callback invoked for each log entry at or below the current level.
--- callback_fn receives: { level, message, timestamp, source }
function M.add_log_callback(callback_fn)
    if type(callback_fn) == "function" then
        table.insert(_callbacks, callback_fn)
    end
end

--- Remove a previously registered callback (same function reference).
function M.remove_log_callback(callback_fn)
    for i = #_callbacks, 1, -1 do
        if _callbacks[i] == callback_fn then
            table.remove(_callbacks, i)
        end
    end
end

--- @param limit number|nil Max entries from the end of the buffer (default 100)
--- @return table[] Array of { level, message, timestamp, source }
function M.get_log_history(limit)
    limit = limit or 100
    local n = #_history
    if n == 0 then
        return {}
    end
    local start_idx = math.max(1, n - limit + 1)
    local out = {}
    for i = start_idx, n do
        table.insert(out, _history[i])
    end
    return out
end

function M.clear_log_history()
    _history = {}
end

--- Internal: emit a log entry (called by the Discord SDK bridge layer).
--- @param level number
--- @param message string
--- @param source string|nil Default "discord"
function M._emit_log(level, message, source)
    source = source or "discord"
    if level > _log_level then
        return
    end
    local entry = {
        level = level,
        message = message,
        timestamp = _now_ms(),
        source = source,
    }
    table.insert(_history, entry)
    while #_history > MAX_HISTORY do
        table.remove(_history, 1)
    end
    for _, cb in ipairs(_callbacks) do
        local ok, err = pcall(cb, entry)
        if not ok then
            print("[discord_debug] log callback error: " .. tostring(err))
        end
    end
end

return M
