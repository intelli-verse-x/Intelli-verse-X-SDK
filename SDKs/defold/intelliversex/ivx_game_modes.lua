-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX Game Modes for Defold
--- Lobby management, matchmaking, and player slots.
--- @module ivx_game_modes

local M = {}

-- ── Mode constants ──────────────────────────────────────────────────────────

M.SOLO              = 1
M.LOCAL_MULTIPLAYER = 2
M.ONLINE_VERSUS     = 3
M.ONLINE_COOP       = 4
M.RANKED            = 5
M.TURN_BASED        = 6

local MODE_NAMES = {
    [M.SOLO]              = "SOLO",
    [M.LOCAL_MULTIPLAYER] = "LOCAL_MULTIPLAYER",
    [M.ONLINE_VERSUS]     = "ONLINE_VERSUS",
    [M.ONLINE_COOP]       = "ONLINE_COOP",
    [M.RANKED]            = "RANKED",
    [M.TURN_BASED]        = "TURN_BASED",
}

-- ── State ───────────────────────────────────────────────────────────────────

local state = {
    current_mode    = M.SOLO,
    max_players     = 4,
    players         = {},
    match_active    = false,
    room_id         = nil,
    is_searching    = false,
}

local callbacks = {}


--- Register a callback: "mode_changed", "player_added", "player_removed",
--- "player_ready_changed", "match_started", "match_ended",
--- "match_found", "room_created", "room_joined", "room_left",
--- "room_list_updated", "search_cancelled"
--- @param event string
--- @param fn function
function M.on(event, fn)
    callbacks[event] = fn
end


--- Select a game mode.
--- @param mode number One of M.SOLO, M.LOCAL_MULTIPLAYER, etc.
--- @param max_players number? Default 4
function M.select_mode(mode, max_players)
    state.current_mode = mode
    state.max_players = max_players or 4
    state.players = {}
    state.match_active = false
    print("[IVXGameModes] Mode set to " .. (MODE_NAMES[mode] or "UNKNOWN") .. " (max " .. state.max_players .. " players)")
    _fire("mode_changed", mode)
end


--- Add a player to the next available slot.
--- @param display_name string
--- @param is_local boolean? Default true
--- @return table|nil Player info or nil if full
function M.add_player(display_name, is_local)
    if is_local == nil then is_local = true end
    if #state.players >= state.max_players then
        print("[IVXGameModes] Cannot add player — lobby full")
        return nil
    end
    local slot = #state.players + 1
    local info = {
        slot         = slot,
        display_name = display_name,
        is_local     = is_local,
        is_ready     = false,
    }
    table.insert(state.players, info)
    _fire("player_added", slot, info)
    return info
end


--- Remove a player by slot index.
--- @param slot number 1-based slot index
function M.remove_player(slot)
    if slot < 1 or slot > #state.players then
        print("[IVXGameModes] Invalid slot: " .. tostring(slot))
        return
    end
    table.remove(state.players, slot)
    for i = slot, #state.players do
        state.players[i].slot = i
    end
    _fire("player_removed", slot)
end


--- Set a player's ready state.
--- @param slot number 1-based slot index
--- @param ready boolean
function M.set_player_ready(slot, ready)
    if slot < 1 or slot > #state.players then
        print("[IVXGameModes] Invalid slot: " .. tostring(slot))
        return
    end
    state.players[slot].is_ready = ready
    _fire("player_ready_changed", slot, ready)
end


--- Start the match if all players are ready.
function M.start_match()
    if state.match_active then
        print("[IVXGameModes] Match already active")
        return
    end
    for _, p in ipairs(state.players) do
        if not p.is_ready then
            print("[IVXGameModes] Not all players are ready")
            return
        end
    end
    state.match_active = true
    print("[IVXGameModes] Match started with " .. #state.players .. " players")
    _fire("match_started")
end


--- End the current match.
function M.end_match()
    if not state.match_active then return end
    state.match_active = false
    print("[IVXGameModes] Match ended")
    _fire("match_ended")
end


--- Reset all state to defaults.
function M.reset()
    state.current_mode = M.SOLO
    state.max_players = 4
    state.players = {}
    state.match_active = false
    state.room_id = nil
    state.is_searching = false
end


-- ── Lobby ───────────────────────────────────────────────────────────────────

--- Create a new room/lobby.
--- @param name string
--- @param room_config table?
--- @param callback function?
function M.create_room(name, room_config, callback)
    local room_id = name:lower():gsub("%s", "_") .. "_" .. tostring(socket.gettime()):gsub("%.", "")
    state.room_id = room_id
    print("[IVXGameModes] Room created: " .. room_id)
    _fire("room_created", room_id)
    if callback then callback(room_id) end
end


--- Join an existing room.
--- @param room_id string
--- @param callback function?
function M.join_room(room_id, callback)
    state.room_id = room_id
    print("[IVXGameModes] Joined room: " .. room_id)
    _fire("room_joined", room_id)
    if callback then callback(room_id) end
end


--- List available rooms.
--- @param callback function(rooms)
function M.list_rooms(callback)
    local rooms = {}
    _fire("room_list_updated", rooms)
    if callback then callback(rooms) end
end


--- Leave the current room.
function M.leave_room()
    if not state.room_id then return end
    print("[IVXGameModes] Left room: " .. state.room_id)
    state.room_id = nil
    _fire("room_left")
end


-- ── Matchmaking ─────────────────────────────────────────────────────────────

--- Start searching for a match.
--- @param match_config table?
--- @param callback function?
function M.find_match(match_config, callback)
    if state.is_searching then
        print("[IVXGameModes] Already searching for a match")
        return
    end
    state.is_searching = true
    print("[IVXGameModes] Searching for match")
end


--- Cancel the current matchmaking search.
function M.cancel_search()
    if not state.is_searching then return end
    state.is_searching = false
    print("[IVXGameModes] Search cancelled")
    _fire("search_cancelled")
end


-- ── Getters ─────────────────────────────────────────────────────────────────

function M.get_current_mode() return state.current_mode end
function M.get_max_players() return state.max_players end
function M.get_players() return state.players end
function M.is_match_active() return state.match_active end
function M.get_room_id() return state.room_id end


-- ── Private ─────────────────────────────────────────────────────────────────

function _fire(event, ...)
    if callbacks[event] then
        callbacks[event](...)
    end
end


return M
