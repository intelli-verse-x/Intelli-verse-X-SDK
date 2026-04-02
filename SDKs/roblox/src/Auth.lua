-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- Authentication module: maps Roblox Player.UserId to Nakama custom auth.
--- Sessions are cached in DataStoreService for cross-session persistence.
--- @module Auth

local DataStoreService = game:GetService("DataStoreService")
local Config = require(script.Parent.Config)
local HttpClient = require(script.Parent.HttpClient)

local Auth = {}

local SESSION_STORE_NAME = "IVXSessions"
local _sessions: { [number]: { token: string, refresh_token: string, user_id: string } } = {}

local function _log(fmt: string, ...: any)
	if Config.get().debug then
		print(string.format("[IVX.Auth] " .. fmt, ...))
	end
end

local function _get_store()
	local ok, store = pcall(DataStoreService.GetDataStore, DataStoreService, SESSION_STORE_NAME)
	return if ok then store else nil
end

--- Authenticate a Roblox player via Nakama custom auth using their UserId.
--- Returns session data on success, nil + error on failure.
function Auth.authenticate_player(player: Player): ({ token: string, refresh_token: string, user_id: string }?, string?)
	local custom_id = "roblox:" .. tostring(player.UserId)

	local resp = HttpClient.post("/v2/account/authenticate/custom?create=true&username=", {
		id = custom_id,
	})

	if not resp.ok then
		return nil, "Auth failed: HTTP " .. tostring(resp.status)
	end

	local session_data = {
		token = resp.body.token or "",
		refresh_token = resp.body.refresh_token or "",
		user_id = resp.body.user_id or "",
	}

	_sessions[player.UserId] = session_data
	_log("Authenticated player %s (%d) -> Nakama user %s", player.Name, player.UserId, session_data.user_id)

	local store = _get_store()
	if store then
		pcall(store.SetAsync, store, tostring(player.UserId), {
			token = session_data.token,
			refresh_token = session_data.refresh_token,
		})
	end

	return session_data, nil
end

--- Authenticate with email/password (for accounts linked beyond Roblox).
function Auth.authenticate_email(email: string, password: string, create: boolean?): ({ token: string, refresh_token: string, user_id: string }?, string?)
	local create_flag = if create then "true" else "false"
	local resp = HttpClient.post("/v2/account/authenticate/email?create=" .. create_flag, {
		email = email,
		password = password,
	})

	if not resp.ok then
		return nil, "Email auth failed: HTTP " .. tostring(resp.status)
	end

	return {
		token = resp.body.token or "",
		refresh_token = resp.body.refresh_token or "",
		user_id = resp.body.user_id or "",
	}, nil
end

--- Try to restore a cached session for a player.
function Auth.restore_session(player: Player): ({ token: string, refresh_token: string, user_id: string }?, string?)
	if _sessions[player.UserId] then
		return _sessions[player.UserId], nil
	end

	local store = _get_store()
	if not store then
		return nil, "DataStore unavailable"
	end

	local ok, saved = pcall(store.GetAsync, store, tostring(player.UserId))
	if ok and saved and saved.token and saved.token ~= "" then
		local session_data = {
			token = saved.token,
			refresh_token = saved.refresh_token or "",
			user_id = "",
		}
		_sessions[player.UserId] = session_data
		_log("Restored session for player %d", player.UserId)
		return session_data, nil
	end

	return nil, "No saved session"
end

--- Get the active session token for a player.
function Auth.get_token(player: Player): string?
	local s = _sessions[player.UserId]
	return if s then s.token else nil
end

--- Get the Nakama user_id for a player.
function Auth.get_nakama_user_id(player: Player): string?
	local s = _sessions[player.UserId]
	return if s then s.user_id else nil
end

--- Clear session data for a player (on leave or logout).
function Auth.clear_session(player: Player)
	_sessions[player.UserId] = nil
	local store = _get_store()
	if store then
		pcall(store.RemoveAsync, store, tostring(player.UserId))
	end
	_log("Cleared session for player %d", player.UserId)
end

--- Check if a player has an active session.
function Auth.has_session(player: Player): boolean
	return _sessions[player.UserId] ~= nil
end

return Auth
