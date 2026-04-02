-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- IntelliVerseX SDK for Roblox
--- Lightweight SDK: AI/LLM Stack, Hiro Live-Ops, Cross-Game Identity.
--- @module IntelliVerseX

local Players = game:GetService("Players")
local Config = require(script.Config)
local Auth = require(script.Auth)
local HttpClient = require(script.HttpClient)
local Remotes = require(script.Remotes)

local IVX = {}

IVX.SDK_VERSION = "5.9.0"
IVX.Config = Config
IVX.Auth = Auth
IVX.Http = HttpClient
IVX.Remotes = Remotes

local _initialized = false
local _callbacks: { [string]: (...any) -> () } = {}
local _sync_metadata: (Player) -> ()

local function _log(fmt: string, ...: any)
	if Config.get().debug then
		print(string.format("[IntelliVerseX] " .. fmt, ...))
	end
end

local function _emit(event: string, ...: any)
	local cb = _callbacks[event]
	if cb then
		cb(...)
	end
end

--- Configure the SDK. Call once from a ServerScript before any other API.
--- @param opts table { game_id, host?, port?, server_key?, use_ssl?, ai_base_url?, ai_api_key?, debug? }
function IVX.configure(opts: { [string]: any })
	Config.set(opts)
	_initialized = true

	local cfg = Config.get()
	_log("SDK v%s initialized — %s:%d | AI: %s", IVX.SDK_VERSION, cfg.host, cfg.port, cfg.ai_base_url)
end

--- Register a callback for SDK events.
function IVX.on(event: string, fn: (...any) -> ())
	_callbacks[event] = fn
end

--- Check if the SDK has been configured.
function IVX.is_initialized(): boolean
	return _initialized
end

--- Authenticate a player and return their session. Convenience wrapper around Auth.
function IVX.authenticate(player: Player): (boolean, string?)
	if not _initialized then
		return false, "SDK not initialized"
	end

	local session, err = Auth.authenticate_player(player)
	if session then
		_sync_metadata(player)
		_emit("auth_success", player, session)
		return true, nil
	else
		_emit("auth_error", player, err)
		return false, err
	end
end

--- Call a Nakama RPC endpoint for a player.
function IVX.call_rpc(player: Player, rpc_id: string, payload: string?): (any?, string?)
	local token = Auth.get_token(player)
	if not token then
		return nil, "No session for player"
	end

	local resp = HttpClient.rpc_post(rpc_id, payload, token)
	if resp.ok then
		return resp.body, nil
	else
		return nil, "RPC failed: HTTP " .. tostring(resp.status)
	end
end

_sync_metadata = function(player: Player)
	local meta = HttpClient.json_encode({
		metadata = {
			sdk_version = IVX.SDK_VERSION,
			platform = "roblox",
			engine = "roblox",
			roblox_user_id = player.UserId,
		},
	})
	pcall(IVX.call_rpc, player, "ivx_sync_metadata", meta)
end

--- Auto-auth on player join and cleanup on leave.
function IVX.enable_auto_auth()
	Players.PlayerAdded:Connect(function(player)
		local ok, err = IVX.authenticate(player)
		if not ok then
			warn("[IntelliVerseX] Auto-auth failed for " .. player.Name .. ": " .. (err or "unknown"))
		end
	end)

	Players.PlayerRemoving:Connect(function(player)
		Auth.clear_session(player)
	end)

	for _, player in Players:GetPlayers() do
		task.spawn(function()
			IVX.authenticate(player)
		end)
	end
end

-- Lazy-loaded sub-modules
IVX.AI = require(script.AI)
IVX.Hiro = require(script.Hiro)
IVX.Identity = require(script.Identity)

return IVX
