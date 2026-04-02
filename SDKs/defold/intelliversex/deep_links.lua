-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Lightweight deep link parser and dispatcher.
--- Parses URLs in the format {scheme}://{host}/{route}?key=value
--- and fires registered handlers for matching routes.
--- @module deep_links

local M = {}

local state = {
    scheme = "",
    host = "",
    initialized = false,
}

local handlers = {}

--- Configure the expected scheme and host.
--- @param scheme string
--- @param host string
function M.initialize(scheme, host)
    state.scheme = scheme
    state.host = host
    state.initialized = true
end

--- Whether initialize() has been called.
--- @return boolean
function M.is_initialized()
    return state.initialized
end

--- Parse url and dispatch to registered handlers.
--- @param url string
--- @return table {matched, scheme, host, route, params, raw}
function M.handle_url(url)
    local result = M._parse(url)
    if result.matched then
        M._dispatch(result)
    end
    return result
end

--- Register a callback for a specific route.
--- @param route string
--- @param callback function(params, result)
function M.register_handler(route, callback)
    if not handlers[route] then
        handlers[route] = {}
    end
    table.insert(handlers[route], callback)
end

--- Remove a previously registered callback from a route.
--- @param route string
--- @param callback function
function M.remove_handler(route, callback)
    local list = handlers[route]
    if not list then return end
    for i = #list, 1, -1 do
        if list[i] == callback then
            table.remove(list, i)
            return
        end
    end
end

--- Remove all handlers, or only those for a specific route.
--- @param route string|nil
function M.remove_all_handlers(route)
    if route then
        handlers[route] = nil
    else
        handlers = {}
    end
end

--- @private
function M._parse(url)
    local empty = {
        matched = false,
        scheme = "",
        host = "",
        route = "",
        params = {},
        raw = url,
    }

    local scheme_end = string.find(url, "://", 1, true)
    if not scheme_end then
        return empty
    end

    local scheme = string.sub(url, 1, scheme_end - 1)
    local rest = string.sub(url, scheme_end + 3)

    local path_start = string.find(rest, "/", 1, true)
    local host
    if path_start then
        host = string.sub(rest, 1, path_start - 1)
    else
        host = rest
    end

    if state.initialized and (scheme ~= state.scheme or host ~= state.host) then
        return empty
    end

    local path_and_query = ""
    if path_start then
        path_and_query = string.sub(rest, path_start + 1)
    end

    local query_start = string.find(path_and_query, "?", 1, true)
    local route, query_string
    if query_start then
        route = string.sub(path_and_query, 1, query_start - 1)
        query_string = string.sub(path_and_query, query_start + 1)
    else
        route = path_and_query
        query_string = ""
    end

    local params = {}
    if query_string ~= "" then
        for pair in string.gmatch(query_string, "[^&]+") do
            local eq = string.find(pair, "=", 1, true)
            if eq then
                local key = M._url_decode(string.sub(pair, 1, eq - 1))
                local value = M._url_decode(string.sub(pair, eq + 1))
                params[key] = value
            else
                params[M._url_decode(pair)] = ""
            end
        end
    end

    return {
        matched = true,
        scheme = scheme,
        host = host,
        route = route,
        params = params,
        raw = url,
    }
end

--- @private
function M._dispatch(result)
    local list = handlers[result.route]
    if not list then return end
    for _, callback in ipairs(list) do
        local ok, _ = pcall(callback, result.params, result)
        -- Handler errors are silently swallowed to avoid cascading failures.
    end
end

--- @private
function M._url_decode(s)
    s = string.gsub(s, "+", " ")
    s = string.gsub(s, "%%(%x%x)", function(hex)
        return string.char(tonumber(hex, 16))
    end)
    return s
end

return M
