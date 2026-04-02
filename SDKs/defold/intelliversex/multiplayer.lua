-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX Multiplayer — Lobby & Matchmaking via Nakama RPCs.
--- @module multiplayer

local json = require "nakama.util.json"

local M = {}

local _client = nil
local _session = nil
local _callbacks = {}

-- -------------------------------------------------------------------------
-- Internal helpers
-- -------------------------------------------------------------------------

local function _fire(event, ...)
    if _callbacks[event] then
        _callbacks[event](...)
    end
end

local function _rpc(rpc_id, payload, callback)
    if _client == nil or _session == nil then
        _fire("error", "IVXMultiplayer not initialized")
        return
    end

    local body = payload and json.encode(payload) or "{}"
    _client.rpc_func(_session, rpc_id, body, function(result)
        if result.error then
            _fire("error", result.error.message or ("RPC error: " .. rpc_id))
            return
        end

        local data = {}
        if result.payload then
            local ok, decoded = pcall(json.decode, result.payload)
            data = ok and decoded or {}
        end

        if callback then callback(data) end
    end)
end

-- -------------------------------------------------------------------------
-- Public API
-- -------------------------------------------------------------------------

--- Initialize the multiplayer module.
--- @param client  Nakama client
--- @param session Nakama session
function M.initialize(client, session)
    _client = client
    _session = session
end

--- Register a callback for an event: "error", "lobby_created", "lobby_joined",
--- "lobby_left", "lobbies_listed", "matchmaking_started", "matchmaking_cancelled"
--- @param event string
--- @param fn    function
function M.on(event, fn)
    _callbacks[event] = fn
end

-- -------------------------------------------------------------------------
-- Lobby
-- -------------------------------------------------------------------------

--- Create a lobby.
--- @param name        string
--- @param max_players number
--- @param is_public   boolean
--- @param callback    function Receives lobby table
function M.create_lobby(name, max_players, is_public, callback)
    _rpc("create_lobby", {
        name = name,
        max_players = max_players,
        is_public = is_public,
    }, function(data)
        _fire("lobby_created", data)
        if callback then callback(data) end
    end)
end

--- Join a lobby by ID.
--- @param lobby_id string
--- @param callback function Receives lobby table
function M.join_lobby(lobby_id, callback)
    _rpc("join_lobby", { lobby_id = lobby_id }, function(data)
        _fire("lobby_joined", data)
        if callback then callback(data) end
    end)
end

--- Leave a lobby.
--- @param lobby_id string
--- @param callback function Receives boolean success
function M.leave_lobby(lobby_id, callback)
    _rpc("leave_lobby", { lobby_id = lobby_id }, function()
        _fire("lobby_left", lobby_id)
        if callback then callback(true) end
    end)
end

--- List public lobbies.
--- @param callback function Receives array of lobby tables
function M.list_lobbies(callback)
    _rpc("list_lobbies", nil, function(data)
        local lobbies = data.lobbies or {}
        _fire("lobbies_listed", lobbies)
        if callback then callback(lobbies) end
    end)
end

-- -------------------------------------------------------------------------
-- Matchmaking
-- -------------------------------------------------------------------------

--- Start matchmaking.
--- @param min_players number
--- @param max_players number
--- @param rank_range  number|nil  Optional rank range
--- @param callback    function    Receives ticket table
function M.start_matchmaking(min_players, max_players, rank_range, callback)
    local payload = {
        min_players = min_players,
        max_players = max_players,
    }
    if rank_range and rank_range > 0 then
        payload.rank_range = rank_range
    end

    _rpc("start_matchmaking", payload, function(data)
        _fire("matchmaking_started", data)
        if callback then callback(data) end
    end)
end

--- Cancel matchmaking.
--- @param ticket_id string
--- @param callback  function Receives boolean success
function M.cancel_matchmaking(ticket_id, callback)
    _rpc("cancel_matchmaking", { ticket_id = ticket_id }, function()
        _fire("matchmaking_cancelled", ticket_id)
        if callback then callback(true) end
    end)
end

return M
