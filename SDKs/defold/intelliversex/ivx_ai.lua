-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX AI Client for Defold
--- Voice sessions, text chat, host AI, and entitlement checks.
--- @module ivx_ai

local json = require "nakama.util.json"

local M = {}

local config = {}
local callbacks = {}


--- Register a callback: "voice_session_started", "message_received",
--- "host_message_received", "entitlement_changed"
--- @param event string Event name
--- @param fn function Callback function
function M.on(event, fn)
    callbacks[event] = fn
end


--- Configure the AI client.
--- @param opts table { api_base_url = string, api_key = string? }
function M.initialize(opts)
    assert(opts and opts.api_base_url, "[IVXAIClient] api_base_url is required")
    config.api_base_url = opts.api_base_url:gsub("/$", "")
    config.api_key = opts.api_key or ""
    print("[IVXAIClient] Initialized — " .. config.api_base_url)
end


--- Start a voice session for a persona.
--- @param persona_id string
--- @param user_id string
--- @param callback function(result, error)
function M.start_voice_session(persona_id, user_id, callback)
    local body = json.encode({ persona_id = persona_id, user_id = user_id })
    _request("POST", "/v1/voice/sessions", body, function(result, err)
        if result and result.session_id then
            _fire("voice_session_started", result.session_id)
        end
        if callback then callback(result, err) end
    end)
end


--- End an active voice session.
--- @param session_id string
--- @param callback function(result, error)
function M.end_voice_session(session_id, callback)
    _request("POST", "/v1/voice/sessions/" .. session_id .. "/end", "{}", callback)
end


--- Send a text message within a session.
--- @param session_id string
--- @param text string
--- @param callback function(result, error)
function M.send_text(session_id, text, callback)
    local body = json.encode({ text = text })
    _request("POST", "/v1/voice/sessions/" .. session_id .. "/text", body, function(result, err)
        if result then
            _fire("message_received", session_id, result)
        end
        if callback then callback(result, err) end
    end)
end


--- Poll for new messages in a session.
--- @param session_id string
--- @param callback function(messages, error)
function M.poll_messages(session_id, callback)
    _request("GET", "/v1/voice/sessions/" .. session_id .. "/messages", nil, function(result, err)
        local messages = result and result.messages or {}
        if callback then callback(messages, err) end
    end)
end


--- Start an AI host session for a match.
--- @param match_id string
--- @param profile table
--- @param callback function(result, error)
function M.start_host_session(match_id, profile, callback)
    local body = json.encode({ match_id = match_id, profile = profile })
    _request("POST", "/v1/host/sessions", body, callback)
end


--- Send an event to the AI host.
--- @param session_id string
--- @param event_type string
--- @param data string
--- @param callback function(result, error)
function M.send_host_event(session_id, event_type, data, callback)
    local body = json.encode({ event_type = event_type, data = data })
    _request("POST", "/v1/host/sessions/" .. session_id .. "/events", body, function(result, err)
        if result then
            _fire("host_message_received", session_id, result)
        end
        if callback then callback(result, err) end
    end)
end


--- Check whether a user has AI entitlement.
--- @param user_id string
--- @param callback function(result, error)
function M.check_entitlement(user_id, callback)
    _request("GET", "/v1/entitlements/" .. user_id, nil, function(result, err)
        if result and result.entitled ~= nil then
            _fire("entitlement_changed", user_id, result.entitled)
        end
        if callback then callback(result, err) end
    end)
end


--- Retrieve the list of available AI personas.
--- @param callback function(personas, error)
function M.get_personas(callback)
    _request("GET", "/v1/personas", nil, function(result, err)
        local personas = result and result.personas or {}
        if callback then callback(personas, err) end
    end)
end


-- ── Private helpers ─────────────────────────────────────────────────────────

function _fire(event, ...)
    if callbacks[event] then
        callbacks[event](...)
    end
end

function _build_headers()
    local headers = { ["Content-Type"] = "application/json" }
    if config.api_key and config.api_key ~= "" then
        headers["Authorization"] = "Bearer " .. config.api_key
    end
    return headers
end

function _request(method, path, body, callback)
    if not config.api_base_url then
        print("[IVXAIClient] Not initialized — call M.initialize() first")
        if callback then callback(nil, "not_initialized") end
        return
    end
    local url = config.api_base_url .. path
    local headers = _build_headers()
    http.request(url, method, function(self, id, response)
        if response.status < 200 or response.status >= 300 then
            local err = "HTTP " .. tostring(response.status)
            print("[IVXAIClient] " .. err .. " — " .. url)
            if callback then callback(nil, err) end
            return
        end
        local ok, result = pcall(json.decode, response.response)
        if ok then
            if callback then callback(result, nil) end
        else
            if callback then callback(nil, "parse_error") end
        end
    end, headers, body or "", { timeout = 30 })
end


return M
