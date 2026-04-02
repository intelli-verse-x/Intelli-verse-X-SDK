-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Satori Analytics — event tracking, feature flags, experiments, live-ops (singleton-style stub).
--- Aligns with JS IVXSatori; production builds integrate the Heroic Labs Satori client.
--- @module ivx_satori

local M = {}

local _initialized = false
local _config = nil
local _identity_id = ""

local function _ensure_initialized()
    if not _initialized or not _config then
        error("Satori not initialized. Call M.initialize() first.")
    end
end

--- @param config table { satori_url, api_key, identity_token? }
function M.initialize(config)
    if not config or not config.satori_url or config.satori_url == "" then
        error("satori_url is required.")
    end
    if not config.api_key or config.api_key == "" then
        error("api_key is required.")
    end
    _config = config
    _initialized = true
end

--- @param identity_id string
--- @param default_props table|nil
--- @param custom_props table|nil
function M.authenticate(identity_id, _default_props, _custom_props)
    _ensure_initialized()
    _identity_id = identity_id or ""
end

--- @param default_props table|nil
--- @param custom_props table|nil
function M.update_identity(_default_props, _custom_props)
    _ensure_initialized()
end

--- @param events table[] Array of { name, value?, metadata?, timestamp? }
function M.capture_events(_events)
    _ensure_initialized()
end

--- @return table[] Feature flags for the current identity
function M.get_all_flags()
    _ensure_initialized()
    return {}
end

--- @param name string
--- @return table|nil Flag table or nil
function M.get_flag(_name)
    _ensure_initialized()
    return nil
end

--- @param experiment_name string
--- @return string Assigned variant (empty if none)
function M.get_experiment_variant(_experiment_name)
    _ensure_initialized()
    return ""
end

--- @return table[] Experiments with variants
function M.get_all_experiments()
    _ensure_initialized()
    return {}
end

--- @return table[] Active live events
function M.get_live_events()
    _ensure_initialized()
    return {}
end

function M.logout()
    _ensure_initialized()
    _identity_id = ""
end

return M
