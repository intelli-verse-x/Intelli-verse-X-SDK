-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Streaks = {}

function Streaks.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("hiro/streaks/get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Streaks.get failed"
end

function Streaks.update(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("hiro/streaks/update", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Streaks.update failed"
end

function Streaks.claim(player: Player, streak_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("hiro/streaks/claim", HttpClient.json_encode({ id = streak_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Streaks.claim failed"
end

return Streaks
