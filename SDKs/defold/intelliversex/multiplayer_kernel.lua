--- IVXMultiplayerKernel — Defold / Lua bridge for the IntelliVerseX
-- Multiplayer Kernel.
--
-- Defold's runtime is Lua + a C++ extension layer. The "real" adapter lives
-- in `SDKs/cpp/src/intelliversex/ivx_multiplayer_kernel.cpp`; this Lua
-- module is a thin facade that calls into it via Defold's `extension`
-- bindings (see `SDKs/defold/intelliversex/intelliversex.cpp`, the same
-- pattern the existing Defold adapter uses for Hiro/Satori).
--
-- Wire protocol: `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
--
-- Public API (mirrors the JS / Unity / Unreal / Godot / Flutter / Java
-- contracts so games port across engines):
--
--   local mp = require("intelliversex.multiplayer_kernel")
--   mp.initialize(client, session)
--   mp.create_match({
--       template_id = "sync-turn-v1",
--       game_id     = "demo",
--       template_init = { min_players = 2 },
--   }, function(ok, resp)
--       if not ok then return end
--       mp.join_match(resp.match_id, function(ok, sess)
--           if not ok then return end
--           sess:subscribe(0xC100, function(env) print("question") end)
--           sess:send(0xC101, { answer_id = "a" })
--       end)
--   end)

local M = {}

-- Native binding (provided by the Defold extension layer). When running
-- inside the Defold editor without the native extension built, we fall back
-- to a no-op stub so requires don't crash during dev-loop iteration.
local native = (rawget(_G, "intelliversex_mp_kernel") or
                rawget(_G, "intelliversex") and _G.intelliversex.multiplayer_kernel) or {
    -- Stub native binding. Returns failure with `error_message` set so games
    -- can detect "extension not built yet" and surface a clear message.
    initialize     = function(_, _)        return false                           end,
    shutdown       = function()                                                   end,
    create_match   = function(_, cb)        cb(false, { error_message = "ext_unavailable" }) end,
    join_match     = function(_, cb)        cb(false, nil)                        end,
    create_and_join= function(_, cb)        cb(false, nil)                        end,
    rpc            = function(_, _, cb)     cb(false, { error_message = "ext_unavailable" }) end,
    state          = function()             return 0                              end,
    -- Per-session methods routed by match_id from Lua.
    session_subscribe       = function(_, _, _) return -1   end,
    session_subscribe_range = function(_, _, _, _) return -1 end,
    session_unsubscribe     = function(_, _)                end,
    session_send            = function(_, _, _)             end,
    session_leave           = function(_)                   end,
    session_dispose         = function(_)                   end,
}

-- Constants (mirrors all other adapters).

M.STATE = {
    DISCONNECTED  = 0,
    CONNECTING    = 1,
    CONNECTED     = 2,
    RECONNECTING  = 3,
    FAILED_FATAL  = 4,
}

M.END_REASON = {
    UNKNOWN              = 0,
    COMPLETED            = 1,
    CANCELLED            = 2,
    DURATION_EXCEEDED    = 3,
    KERNEL_INTERNAL      = 4,
    ALL_PLAYERS_LEFT     = 5,
    HOST_TERMINATED      = 6,
}

-- ---------------------------------------------------------------------------
-- MatchSession
-- ---------------------------------------------------------------------------

local Session = {}
Session.__index = Session

function Session.new(match_id, local_user_id, template_id)
    return setmetatable({
        match_id              = match_id,
        local_user_id         = local_user_id or "",
        template_id           = template_id or "",
        current_match_time_ms = 0,
        active_player_count   = 0,
        state                 = M.STATE.CONNECTED,
        _subs                 = {},  -- subscription_id -> bool (for dispose tracking)
        _disposed             = false,
    }, Session)
end

--- Subscribe to a single opcode. `handler(env)` will be called from the
-- native dispatch tick on the main game thread.
-- Returns a subscription_id usable with :unsubscribe.
function Session:subscribe(op_code, handler)
    if self._disposed then return -1 end
    local id = native.session_subscribe(self.match_id, op_code, handler)
    if id and id >= 0 then self._subs[id] = true end
    return id
end

function Session:subscribe_range(op_from, op_to, handler)
    if self._disposed then return -1 end
    local id = native.session_subscribe_range(self.match_id, op_from, op_to, handler)
    if id and id >= 0 then self._subs[id] = true end
    return id
end

function Session:unsubscribe(subscription_id)
    if self._disposed then return end
    native.session_unsubscribe(self.match_id, subscription_id)
    self._subs[subscription_id] = nil
end

--- Send a payload table. The native layer JSON-encodes it and stamps the
-- kernel header (seq, match_time_ms, uuid).
function Session:send(op_code, payload)
    if self._disposed then return end
    native.session_send(self.match_id, op_code, payload or {})
end

function Session:leave()
    if self._disposed then return end
    native.session_leave(self.match_id)
    self:dispose()
end

function Session:dispose()
    if self._disposed then return end
    self._disposed = true
    native.session_dispose(self.match_id)
    self._subs = {}
    self.state = M.STATE.DISCONNECTED
end

-- ---------------------------------------------------------------------------
-- Top-level kernel API
-- ---------------------------------------------------------------------------

local _initialized = false
local _sessions_by_id = {}    -- match_id -> Session

--- @param client a userdata returned by Nakama Defold extension
--- @param session a userdata returned by Nakama Defold extension
function M.initialize(client, session)
    if _initialized then return true end
    local ok = native.initialize(client, session)
    _initialized = ok and true or false
    return _initialized
end

function M.shutdown()
    if not _initialized then return end
    for _, s in pairs(_sessions_by_id) do s:dispose() end
    _sessions_by_id = {}
    native.shutdown()
    _initialized = false
end

function M.transport_state()
    return native.state() or M.STATE.DISCONNECTED
end

--- Create a match. cb(ok, resp).
-- resp = { match_id, template_id, region, expires_unix_ms } on success;
-- resp = { error_message } on failure.
function M.create_match(req, cb)
    assert(type(req) == "table", "req must be a table")
    assert(type(cb)  == "function", "cb must be a function")
    native.create_match({
        template_id   = req.template_id or "",
        game_id       = req.game_id or "",
        region        = req.region or "",
        template_init = req.template_init or {},
    }, function(ok, resp)
        cb(ok and true or false, resp or {})
    end)
end

local function rpc_dict(rpc_id, payload, cb)
    assert(type(rpc_id) == "string" and #rpc_id > 0, "rpc_id required")
    assert(type(cb) == "function", "cb required")
    native.rpc(rpc_id, payload or {}, function(ok, resp)
        cb(ok and true or false, resp or {})
    end)
end

function M.list_templates(cb)
    rpc_dict("mp_list_templates", {}, cb)
end

function M.read_match_result(match_id, cb)
    assert(type(match_id) == "string" and #match_id > 0, "match_id required")
    rpc_dict("mp_read_match_result", { match_id = match_id }, cb)
end

function M.list_agent_personas(cb)
    rpc_dict("mp_agent_list_personas", {}, cb)
end

function M.spawn_agent(req, cb)
    assert(type(req) == "table", "req required")
    assert(type(req.match_id) == "string" and #req.match_id > 0, "match_id required")
    assert(type(req.persona_id) == "string" and #req.persona_id > 0, "persona_id required")
    rpc_dict("mp_agent_spawn", req, cb)
end

function M.despawn_agent(req, cb)
    assert(type(req) == "table", "req required")
    assert(type(req.match_id) == "string" and #req.match_id > 0, "match_id required")
    assert(type(req.agent_id) == "string" and #req.agent_id > 0, "agent_id required")
    rpc_dict("mp_agent_despawn", req, cb)
end

function M.agent_speak(req, cb)
    assert(type(req) == "table", "req required")
    assert(type(req.match_id) == "string" and #req.match_id > 0, "match_id required")
    assert(type(req.agent_id) == "string" and #req.agent_id > 0, "agent_id required")
    assert(type(req.text) == "string" and #req.text > 0, "text required")
    rpc_dict("mp_agent_speak", req, cb)
end

--- Join an existing match. cb(ok, session).
function M.join_match(match_id, cb)
    assert(type(match_id) == "string" and #match_id > 0, "match_id required")
    assert(type(cb)       == "function", "cb required")
    native.join_match(match_id, function(ok, info)
        if not ok then cb(false, nil); return end
        local sess = Session.new(match_id, info and info.local_user_id, info and info.template_id)
        _sessions_by_id[match_id] = sess
        cb(true, sess)
    end)
end

--- Create + join in one call. cb(ok, session).
function M.create_and_join(req, cb)
    assert(type(req) == "table",    "req required")
    assert(type(cb)  == "function", "cb required")
    M.create_match(req, function(ok, resp)
        if not ok then cb(false, nil); return end
        M.join_match(resp.match_id, cb)
    end)
end

--- Internal — called by the native dispatcher when a session is removed.
function M.__internal_remove_session(match_id)
    _sessions_by_id[match_id] = nil
end

return M
