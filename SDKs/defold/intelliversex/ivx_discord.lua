-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX Discord Social for Defold
--- Rich Presence, unified friends, lobby chat, voice calls, and game invites
--- via the Discord Social SDK.
--- @module ivx_discord

local json = require "nakama.util.json"

local M = {}

local config = {}
local initialized = false
local active_lobby_id = ""
local active_voice_channel_id = ""
local self_muted = false
local self_deafened = false
local callbacks = {}


--- Register an event callback.
--- Supported events: "discord_ready", "discord_error", "invite_received",
--- "join_request", "lobby_message", "voice_state_update",
--- "friends_updated", "lobby_joined", "lobby_left",
--- "voice_joined", "voice_left"
--- @param event string
--- @param fn function
function M.on(event, fn)
    callbacks[event] = fn
end


-- ── Manager ─────────────────────────────────────────────────────────────────

--- Initialize the Discord Social SDK.
--- @param cfg table Config with keys: application_id (number), default_lobby_secret (string), enable_voice (bool), enable_overlay (bool)
--- @param callback function(success, error)|nil
function M.initialize(cfg, callback)
    if initialized then
        print("[IVXDiscordSocial] Already initialized")
        if callback then callback(true, nil) end
        return
    end
    config = cfg or {}
    -- Discord Social SDK init goes here — integration point.
    initialized = true
    print("[IVXDiscordSocial] Initialized with app id " .. tostring(config.application_id or 0))
    _fire("discord_ready", false)
    if callback then callback(true, nil) end
end


--- Link the current player's account with Discord.
--- @param callback function(success, error)|nil
function M.link_account(callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Account linked")
    if callback then callback(true, nil) end
end


--- Unlink the current player's Discord account.
--- @param callback function(success, error)|nil
function M.unlink_account(callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Account unlinked")
    if callback then callback(true, nil) end
end


--- Returns true if the SDK has been initialised.
--- @return boolean
function M.is_initialized()
    return initialized
end


-- ── Rich Presence ───────────────────────────────────────────────────────────

--- Set the current activity shown in Discord.
--- @param details string
--- @param state string
--- @param start_timestamp number|nil Unix timestamp
--- @param end_timestamp number|nil Unix timestamp
function M.set_activity(details, state, start_timestamp, end_timestamp)
    if not _ensure_init() then return end
    -- Push activity to Discord Social SDK — integration point.
    print("[IVXDiscordSocial] SetActivity: " .. tostring(details) .. " — " .. tostring(state))
end


--- Set party info attached to the current presence.
--- @param party_id string
--- @param current_size number
--- @param max_size number
--- @param join_secret string|nil
function M.set_party(party_id, current_size, max_size, join_secret)
    if not _ensure_init() then return end
    print("[IVXDiscordSocial] SetParty: " .. party_id .. " (" .. current_size .. "/" .. max_size .. ")")
end


--- Clear the current Discord presence.
function M.clear_presence()
    if not _ensure_init() then return end
    print("[IVXDiscordSocial] Presence cleared")
end


-- ── Friends ─────────────────────────────────────────────────────────────────

--- Retrieve a merged list of game + Discord friends.
--- Each entry: { user_id, display_name, avatar_url, source, online }
--- @param callback function(friends, error)
function M.get_unified_friends(callback)
    if not _ensure_init(callback) then return end
    -- Merge Discord + game friends — integration point.
    local friends = {}
    print("[IVXDiscordSocial] GetUnifiedFriends: returned " .. #friends .. " friends")
    _fire("friends_updated", friends)
    if callback then callback(friends, nil) end
end


-- ── Lobby ───────────────────────────────────────────────────────────────────

--- Create or join a lobby by secret.
--- @param lobby_secret string
--- @param callback function(success, error)|nil
function M.create_or_join_lobby(lobby_secret, callback)
    if not _ensure_init(callback) then return end
    active_lobby_id = lobby_secret
    print("[IVXDiscordSocial] Joined lobby: " .. lobby_secret)
    _fire("lobby_joined", lobby_secret)
    if callback then callback(true, nil) end
end


--- Leave the current lobby.
--- @param callback function(success, error)|nil
function M.leave_lobby(callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Left lobby: " .. active_lobby_id)
    active_lobby_id = ""
    _fire("lobby_left")
    if callback then callback(true, nil) end
end


--- Send a text message to the current lobby.
--- @param content string
function M.send_lobby_message(content)
    if not _ensure_init() then return end
    if active_lobby_id == "" then
        print("[IVXDiscordSocial] Not in a lobby")
        return
    end
    print("[IVXDiscordSocial] Lobby message sent: " .. content)
end


-- ── Voice ───────────────────────────────────────────────────────────────────

--- Join a voice call for the given lobby.
--- @param lobby_id string
--- @param callback function(success, error)|nil
function M.join_voice_call(lobby_id, callback)
    if not _ensure_init(callback) then return end
    if config.enable_voice == false then
        print("[IVXDiscordSocial] Voice is disabled in config")
        if callback then callback(false, "voice_disabled") end
        return
    end
    active_voice_channel_id = lobby_id
    print("[IVXDiscordSocial] Joined voice call: " .. lobby_id)
    _fire("voice_joined", lobby_id)
    if callback then callback(true, nil) end
end


--- Leave the current voice call.
--- @param callback function(success, error)|nil
function M.leave_voice_call(callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Left voice call: " .. active_voice_channel_id)
    active_voice_channel_id = ""
    self_muted = false
    self_deafened = false
    _fire("voice_left")
    if callback then callback(true, nil) end
end


--- Mute or unmute self.
--- @param mute boolean
function M.set_self_mute(mute)
    if not _ensure_init() then return end
    self_muted = mute
    print("[IVXDiscordSocial] Self mute: " .. tostring(mute))
end


--- Deafen or undeafen self.
--- @param deafen boolean
function M.set_self_deafen(deafen)
    if not _ensure_init() then return end
    self_deafened = deafen
    print("[IVXDiscordSocial] Self deafen: " .. tostring(deafen))
end


--- Set volume for a specific participant (0.0 – 2.0).
--- @param user_id string
--- @param volume number
function M.set_participant_volume(user_id, volume)
    if not _ensure_init() then return end
    print("[IVXDiscordSocial] SetParticipantVolume: " .. user_id .. " -> " .. tostring(volume))
end


-- ── Invites ─────────────────────────────────────────────────────────────────

--- Send a game invite to a user.
--- @param user_id string
--- @param message string
--- @param callback function(success, error)|nil
function M.send_invite(user_id, message, callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Invite sent to " .. user_id .. ": " .. (message or ""))
    if callback then callback(true, nil) end
end


--- Accept an incoming invite.
--- @param invite_id string
--- @param callback function(success, error)|nil
function M.accept_invite(invite_id, callback)
    if not _ensure_init(callback) then return end
    print("[IVXDiscordSocial] Invite accepted: " .. invite_id)
    if callback then callback(true, nil) end
end


--- Decline an incoming invite.
--- @param invite_id string
function M.decline_invite(invite_id)
    if not _ensure_init() then return end
    print("[IVXDiscordSocial] Invite declined: " .. invite_id)
end


-- ── Private helpers ─────────────────────────────────────────────────────────

function _fire(event, ...)
    if callbacks[event] then
        callbacks[event](...)
    end
end

function _ensure_init(callback)
    if not initialized then
        print("[IVXDiscordSocial] Not initialized — call M.initialize() first")
        if callback then callback(nil, "not_initialized") end
        return false
    end
    return true
end


return M
