-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX Hiro Systems for Defold
--- Spin wheel, streaks, offerwall, friend quests & battles via Nakama RPC.
--- @module ivx_hiro

local json = require "nakama.util.json"

local M = {}

local nakama_client = nil
local session = nil
local initialized = false
local callbacks = {}


--- Register a callback: "spin_wheel_result", "streak_updated", "streak_claimed",
--- "offerwall_updated", "friend_quest_updated", "friend_battle_result"
--- @param event string
--- @param fn function
function M.on(event, fn)
    callbacks[event] = fn
end


--- Initialize with a live Nakama client and session.
--- @param client table Nakama client instance
--- @param sess table Active session
function M.initialize(client, sess)
    nakama_client = client
    session = sess
    initialized = true
    print("[IVXHiroSystems] Initialized")
end


-- ── Spin Wheel ──────────────────────────────────────────────────────────────

--- Retrieve the current spin-wheel state.
--- @param callback function(result, error)
function M.spin_wheel_get(callback)
    _rpc("hiro/spinwheel/get", "{}", callback)
end


--- Execute a spin on the wheel.
--- @param callback function(result, error)
function M.spin_wheel_spin(callback)
    _rpc("hiro/spinwheel/spin", "{}", function(result, err)
        if result then _fire("spin_wheel_result", result) end
        if callback then callback(result, err) end
    end)
end


-- ── Streaks ─────────────────────────────────────────────────────────────────

--- Retrieve all active streaks.
--- @param callback function(result, error)
function M.streaks_get(callback)
    _rpc("hiro/streaks/get", "{}", callback)
end


--- Update progress on a streak.
--- @param streak_id string
--- @param callback function(result, error)
function M.streaks_update(streak_id, callback)
    local payload = json.encode({ id = streak_id })
    _rpc("hiro/streaks/update", payload, function(result, err)
        if result then _fire("streak_updated", result) end
        if callback then callback(result, err) end
    end)
end


--- Claim a milestone reward for a streak.
--- @param streak_id string
--- @param milestone string
--- @param callback function(result, error)
function M.streaks_claim(streak_id, milestone, callback)
    local payload = json.encode({ id = streak_id, milestone = milestone })
    _rpc("hiro/streaks/claim", payload, function(result, err)
        if result then _fire("streak_claimed", result) end
        if callback then callback(result, err) end
    end)
end


-- ── Offerwall ───────────────────────────────────────────────────────────────

--- Get all available offerwall entries.
--- @param callback function(offers, error)
function M.offerwall_get(callback)
    _rpc("hiro/offerwall/get", "{}", function(result, err)
        local offers = result and result.offers or {}
        if #offers > 0 then _fire("offerwall_updated", offers) end
        if callback then callback(offers, err) end
    end)
end


--- Mark an offer as completed.
--- @param offer_id string
--- @param callback function(result, error)
function M.offerwall_complete(offer_id, callback)
    local payload = json.encode({ id = offer_id })
    _rpc("hiro/offerwall/complete", payload, callback)
end


--- Claim all pending offerwall rewards.
--- @param callback function(result, error)
function M.offerwall_claim_pending(callback)
    _rpc("hiro/offerwall/claim_pending", "{}", callback)
end


-- ── Friend Quests ───────────────────────────────────────────────────────────

--- Get active friend quests.
--- @param callback function(result, error)
function M.friend_quests_get(callback)
    _rpc("hiro/friend_quests/get", "{}", callback)
end


--- Contribute progress to a friend quest.
--- @param quest_id string
--- @param amount number? Default 1
--- @param callback function(result, error)
function M.friend_quests_contribute(quest_id, amount, callback)
    local payload = json.encode({ id = quest_id, amount = amount or 1 })
    _rpc("hiro/friend_quests/contribute", payload, function(result, err)
        if result then _fire("friend_quest_updated", result) end
        if callback then callback(result, err) end
    end)
end


-- ── Friend Battles ──────────────────────────────────────────────────────────

--- Challenge a friend to a battle.
--- @param friend_id string
--- @param score number
--- @param callback function(result, error)
function M.friend_battles_challenge(friend_id, score, callback)
    local payload = json.encode({ friend_id = friend_id, score = score or 0 })
    _rpc("hiro/friend_battles/challenge", payload, function(result, err)
        if result then _fire("friend_battle_result", result) end
        if callback then callback(result, err) end
    end)
end


--- Get pending and active friend battles.
--- @param callback function(result, error)
function M.friend_battles_get(callback)
    _rpc("hiro/friend_battles/get", "{}", callback)
end


-- ── Private helpers ─────────────────────────────────────────────────────────

function _fire(event, ...)
    if callbacks[event] then
        callbacks[event](...)
    end
end

function _rpc(rpc_id, payload, callback)
    if not initialized then
        print("[IVXHiroSystems] Not initialized — call M.initialize() first")
        if callback then callback(nil, "not_initialized") end
        return
    end
    nakama_client.rpc_func(session, rpc_id, payload, function(result)
        if result.error then
            print("[IVXHiroSystems] RPC '" .. rpc_id .. "' error: " .. tostring(result.error))
            if callback then callback(nil, result.error) end
            return
        end
        local ok, data = pcall(json.decode, result.payload or "{}")
        if ok then
            if callback then callback(data, nil) end
        else
            if callback then callback(nil, "parse_error") end
        end
    end)
end


return M
